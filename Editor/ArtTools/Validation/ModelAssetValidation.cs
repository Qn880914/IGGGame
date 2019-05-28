/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:36
	file base:	ModelAssetValidation
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using UnityEditor;
using IGG.AssetImportSystem;
using System.Collections.Generic;
using System.Linq;

	using Debug = UnityEngine.Debug;

/// <summary>
/// Give us some functionality for test a model against pre-configured performance metrics (ProjectImportSettings).
/// </summary>
public class ModelAssetValidation : AssetPostprocessor
{
    private const int NUM_VERTICES_WHERE_IT_SHOULD_BATCH_REGARDLESS_OF_SETTINGS = 50;
    /// <summary>
    /// Performances the check model prefab.
    /// </summary>
    /// <returns>The check model prefab.</returns>
    /// <param name="modelRoot">Model root.</param>
    public static string PerformanceCheckModelPrefab(GameObject modelRoot)
    {
        //const int NUM_VERTICES_WHERE_IT_SHOULD_BATCH_REGARDLESS_OF_SETTINGS = 50;
        string errorStr = null;
        foreach (var meshFilter in modelRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = meshFilter.sharedMesh;
            errorStr = CheckMesh(mesh, false, modelRoot);
            if (errorStr != null)
                return errorStr;
        }

        foreach (var meshFilter in modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var mesh = meshFilter.sharedMesh;
            errorStr = CheckMesh(mesh, true, modelRoot);
            if (errorStr != null)
                return errorStr;
        }

        return null;
    }

    private static string CheckMesh(Mesh mesh, bool bSkin, GameObject modelRoot)
    {
        //var mesh = meshFilter.sharedMesh;
        bool bShouldBatch = false;

        if (EditorUtility.IsPersistent(mesh))
        {
            string assetPath = AssetDatabase.GetAssetPath(mesh);
            var modelImporter = ModelImporter.GetAtPath(assetPath);

            if (!modelImporter)
                return string.Format("Mesh in '{0}' did not contain a ModelImporter.  It cannot be performance tested.", assetPath);

            // Find the best rule, if none, we just assume we passed...
            var bestRule = ProjectImportSettings.FindBestRule(modelImporter, ProjectImportSettings.GetAllModelFolderRules());
            if (bestRule == null)
                return null;

            CheckModelByBestRule(modelImporter, bestRule, modelRoot, assetPath);

            if (mesh.vertexCount > bestRule.maxVertices)
                return string.Format("Mesh in '{0}' violated its folder rule setting '{1}' for maxVertices ({2} > {3})", assetPath, bestRule.folderPath, mesh.vertexCount, bestRule.maxVertices);

            if (mesh.bindposes.Length != 0 && mesh.bindposes.Length > bestRule.maxBones)
            {

                return string.Format("Mesh in '{0}' violated its folder rule setting '{1}' for bones ({2} > {3})", assetPath, bestRule.folderPath, mesh.bindposes.Length, bestRule.maxBones);
            }

            if (mesh.boneWeights.Length != 0)
            {
                if (!CheckBoneWeight(mesh.boneWeights, bestRule.maxBoneWeights))
                    return string.Format("Mesh in '{0}' violated its folder rule setting '{1}' for boneWeight too much", assetPath, bestRule.folderPath);
            }

            bShouldBatch = bestRule.shouldBatch;
        }

        bool bShouldBatchRegardless = (mesh.vertexCount <= NUM_VERTICES_WHERE_IT_SHOULD_BATCH_REGARDLESS_OF_SETTINGS);
        if (bShouldBatch || bShouldBatchRegardless)
        {
            if (!WillGameObjectBatch(modelRoot))
            {
                if (bShouldBatch)
                    return string.Format("Mesh in '{0}' was required to batch by rule but it does not batch.", mesh.name);
                else
                    return string.Format("Mesh in '{0}' should batch due to its very small vertex count, but will not!", mesh.name);
            }
        }

        return null;
    }

    private static bool CheckBoneWeight(BoneWeight[] boneWeights, int maxBoneWeight)
    {

        for (int i = 0; i < boneWeights.Length; ++i)
        {

            if (maxBoneWeight < 3 && boneWeights[i].weight2 > 0.001f)
            {
                return false;
            }
            if (maxBoneWeight < 4 && boneWeights[i].weight3 > 0.001f)
            {
                return false;
            }

        }

        return true;
    }

    private static bool CheckModelByBestRule(AssetImporter assetImporter, ProjectImportSettings.ModelFolderRule rule, GameObject model, string assetPath)
    {

        var modelImporter = assetImporter as ModelImporter;
        bool bMatch = true;
        if (modelImporter.isReadable != rule.m_IsReadable)
        {
            Debug.LogWarning(" Asset '" + assetPath + "'Readable option is not matched with the modelRule", model);
            bMatch = false;
        }

        if (modelImporter.optimizeMesh != rule.optimizeMeshForGPU)
        {
            Debug.LogWarning(" Asset '" + assetPath + "'optimizeMesh option is not matched with the modelRule", model);
            bMatch = false;
        }

        if (modelImporter.importNormals != rule.normalImportMode)
        {
            Debug.LogWarning(" Asset '" + assetPath + "'importNormals option is not matched with the modelRule", model);
            bMatch = false;
        }


        //skin model
        if (modelImporter.animationType != ModelImporterAnimationType.None)
        {

            if (modelImporter.optimizeGameObjects != rule.m_OptimizeGameObjects)
            {
                Debug.LogWarning(" Asset '" + assetPath + "'optimizeGameObject option is not matched with the modelRule", model);
                bMatch = false;
            }

        }

        return bMatch;
    }

    /// <summary>
    /// Do a quick heuristic to check if a given model will batch
    /// </summary>
    private static bool WillGameObjectBatch(GameObject modelRoot)
    {
        const int UNITY_VERTEX_BUFFER_SIZE = 300;
        const int TOO_MANY_VERTICES = 3000;

        // Try to figure out the asset path...
        string assetPath = AssetDatabase.GetAssetPath(modelRoot);
        if (string.IsNullOrEmpty(assetPath))
            assetPath = modelRoot.name;

        bool bWillBatch = true;
        foreach (var meshRenderer in modelRoot.GetComponentsInChildren<MeshRenderer>(true))
        {
            // According to http://docs.unity3d.com/ScriptReference/Renderer-lightmapIndex.html, this means no Lightmap
            const int NO_LIGHTMAP = 255;

            if (meshRenderer.lightmapIndex != NO_LIGHTMAP)
            {
                Debug.LogWarning(string.Format("MeshRenderer '{1}' on Asset '{0}' is light-mapped and therefore will not batch.  Lightmap Index: {2} Offset: {3}", modelRoot, meshRenderer.name, meshRenderer.lightmapIndex, meshRenderer.lightmapScaleOffset.ToString()), modelRoot);
                return false;
            }

            if (meshRenderer.sharedMaterials.Length > 1)
            {
                Debug.LogWarning("Asset '" + assetPath + "' has multiple materials.  We can't gracefully detect performance issues for such cases.", modelRoot);
                return false;
            }

            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogWarning("Asset '" + assetPath + "' has a MeshRenderer with no MeshFilter.  This is unexpected.", modelRoot);
                continue;
            }

            if (!meshFilter.sharedMesh)
            {
                Debug.LogWarning("Asset '" + assetPath + "' has no Mesh defined (probably built procedurally).  We cannot detect performance accurately on this asset.", modelRoot);
                continue;
            }

            if (meshFilter.sharedMesh.subMeshCount > 1)
            {
                Debug.LogWarning("Asset '" + assetPath + "' has " + meshFilter.sharedMesh.subMeshCount + " SubMeshes.  You will receive a draw call for each SubMesh.", modelRoot);
            }

            int vertexCount = meshFilter.sharedMesh.vertexCount;
            int normalCount = meshFilter.sharedMesh.normals.Length;
            int uv2Count = meshFilter.sharedMesh.uv2.Length;

            if (vertexCount > UNITY_VERTEX_BUFFER_SIZE)
            {
                // What suggestion will we provide?  Let's see...
                string suggestion = null;
                if (vertexCount > TOO_MANY_VERTICES)
                {
                    suggestion = "Ask an artist to reduce the model complexity.";
                }
                else if (vertexCount < UNITY_VERTEX_BUFFER_SIZE + 100)
                {
                    if (uv2Count > 0)
                        suggestion = "Ask an artist to remove the second set of UV coordinates.";
                    else if (normalCount > 0)
                        suggestion = "You may be able to reduce the vertex count by simply not importing normals.";
                }

                if (!string.IsNullOrEmpty(suggestion))
                    Debug.LogWarning("Asset '" + assetPath + "' will not batch due to too many vertices data (" + vertexCount + " > 300)\n Suggested Fix: " + suggestion);

                bWillBatch = false;
            }
        }

        return bWillBatch;
    }

    /**
     * Give us a menu item to test any selected model in the project
     */
    [MenuItem("IGG/Performance/Selected/Test Models", true)]
    private static bool CanPerformanceTestSelectedObjects()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    /**
     * Give us a menu item to test any selected model in the project
     */
    [MenuItem("IGG/Performance/Selected/Test Models", false)]
    private static void PerformanceTestSelectedObjects()
    {
        bool bAllOK = true;

        foreach (GameObject gameObj in Selection.gameObjects)
        {
            string errorMsg = PerformanceCheckModelPrefab(gameObj);
            bool bPassed = string.IsNullOrEmpty(errorMsg);

            if (bPassed)
                Debug.Log("Asset '" + gameObj.name + " passed.", gameObj);
            else
                Debug.LogError(errorMsg, gameObj);

            bAllOK = bAllOK && bPassed;
        }

        if (!bAllOK)
        {
            EditorUtility.DisplayDialog("Performance Warning", "Some of the selected assets failed our Project Import and Performance settings.  Check the log window for details!", "Thanks!");
            EditorApplication.ExecuteMenuItem("Window/Console");
        }
    }

    static Mesh GetSourceMesh(GameObject prefab)
    {
        var meshFilter = prefab.GetComponentsInChildren<MeshFilter>(true).First();
        if (meshFilter == null) return null;
        return meshFilter.mesh;
    }

}
