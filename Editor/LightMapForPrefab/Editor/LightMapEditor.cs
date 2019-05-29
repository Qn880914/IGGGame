using GOE.Scene;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 描述：动态烘焙场景光照模型，并产生对应的Prefab文件
/// <para>创建时间：2016-06-15</para>
/// </summary>
public sealed class LightMapEditor
{
    private const string LightMapsDir = "Resources/Lightmaps/";
    private static List<RemapTexture2D> sceneLightmaps = new List<RemapTexture2D>();

    #region Menu

    [MenuItem("LightMap/Remove Prefab Lightmaps")]
    public static void RemoveLightData()
    {
        PrefabLightmapData[] pldArr = GameObject.FindObjectsOfType<PrefabLightmapData>();
        if (pldArr != null)
        {
            foreach (var data in pldArr)
            {
                var target = data.gameObject;
                GameObject.DestroyImmediate(data);
               
                GameObject targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(target);
                PrefabUtility.ReplacePrefab(target, targetPrefab);
            }
        }
    }

    [MenuItem("LightMap/Update Scene with Prefab Lightmaps")]
    public static void UpdateLightmaps()
    {
        UpdateSceneLightmap();
    }

    static string MODEL_PATH_SOLDIER = "Assets/Data/Prefabs/Actor/Soldier";


    [MenuItem("LightMap/Tools/AddModelPoint")]
    public static void AddModelPoint()
    {
        var guids2 = AssetDatabase.FindAssets("t:Prefab", new string[] { MODEL_PATH_SOLDIER });
        foreach (var guid in guids2)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            GameObject target = GameObject.Instantiate<GameObject>(obj);

            var point = target.transform.Find("ChestPoint");
            if (point != null)
                continue;


            target.transform.localPosition = Vector3.zero ;

            GameObject chestPoint = new GameObject();
            chestPoint.transform.parent = target.transform;
            chestPoint.transform.localPosition = new Vector3(0,5.05f,0.39f);
            chestPoint.name = "ChestPoint";

            GameObject headPoint = new GameObject();
            headPoint.transform.parent = target.transform;
            headPoint.transform.localPosition = new Vector3(0.19f, 7.04f, 0.39f);
            headPoint.name = "HeadPoint";


            //GameObject targetPrefab = PrefabUtility.GetPrefabParent(target) as GameObject;
            //Object targetPrefab = PrefabUtility.GetPrefabObject(target);
            PrefabUtility.ReplacePrefab(target, obj);
            GameObject.DestroyImmediate(target);

        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("add point finish");

    }



    [MenuItem("LightMap/Tools/CheckBuildingChildrenName")]
    public static void CheckBuildingChildrenName()
    {
        var guids2 = AssetDatabase.FindAssets("t:Prefab", new string[] { PRFAB_BUILDING_PATH });
        foreach (var guid in guids2)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));


            GameObject target = GameObject.Instantiate<GameObject>(obj);

            if (target.name.StartsWith("COL3"))
                continue;

            target.name = target.name.Remove(target.name.IndexOf("(Clone)"));

            Recursive(target.gameObject, target.name, 0);

            PrefabUtility.ReplacePrefab(target, obj);
            GameObject.DestroyImmediate(target);

        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CheckBuildingChildrenName finish");

    }

    private static void Recursive(GameObject parentGameObject,string compareName,int idx)
    {
        

        foreach (Transform child in parentGameObject.transform)
        {
            if (child.name.Equals(compareName))
            {
                child.name = child.name + "_" + idx;
                idx++;
            }
               

            Recursive(child.gameObject,compareName,idx);
        }
    }



    [MenuItem("LightMap/Tools/UndoBuildingChildrenName")]
    public static void UndoBuildingChildrenName()
    {


        RecursiveUndo(Selection.activeGameObject);

        

        Debug.Log("UndoBuildingChildrenName finish");

    }

    private static void RecursiveUndo(GameObject parentGameObject)
    {


        foreach (Transform child in parentGameObject.transform)
        {
            for (int i=0; i < 9; i++)
            {
                if (child.name.EndsWith("_" + i))
                {
                    child.name = child.name.Remove(child.name.IndexOf("_" + i));
                    break;

                }
            }

           


            RecursiveUndo(child.gameObject);
        }
    }







    static string PRFAB_BUILDING_PATH = "Assets/Data/Prefabs/BuildingModel";
    static Vector3 MAIN_BUILDING_POS = new Vector3(0,8,0);
    static Vector3 USUAL_BUILDING_POS = new Vector3(0, 0.35f, 0);
    static Vector3 USUAL_BUILDING_SCALE = new Vector3(0.75f, 0.75f,0.75f);
    


    [MenuItem("LightMap/Tools/AddBuildingPoint")]
    public static void AddBuildingPoint()
    {
        var guids2 = AssetDatabase.FindAssets("t:Prefab", new string[] { PRFAB_BUILDING_PATH });
        foreach (var guid in guids2)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

            
            GameObject target = GameObject.Instantiate<GameObject>(obj);

            if (target.name.StartsWith("COL3"))
                continue;

            target.name = target.name.Remove(target.name.IndexOf("(Clone)"));

            GameObject parentPoint = new GameObject();
            parentPoint.transform.localPosition = Vector3.zero;
            parentPoint.layer = LayerMask.NameToLayer(BUILDING_LAYER_NAME);
            target.transform.parent = parentPoint.transform;
            parentPoint.name = target.name;
            target.name = target.name + "_1";
            
            
            PrefabUtility.ReplacePrefab(parentPoint, obj);
            GameObject.DestroyImmediate(parentPoint);

        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("add building point finish");

    }






    //public static void UpdateLightmaps()
    //{
    //    PrefabLightmapData pld = GameObject.FindObjectOfType<PrefabLightmapData>();
    //    if (pld == null) return;

    //    LightmapSettings.lightmaps = null;
    //    PrefabLightmapData.ApplyLightmaps(pld.mRendererInfos, pld.mLightmapFars, pld.mLightmapNears);

    //    Debug.Log("Prefab Lightmap updated");
    //}

    [MenuItem("LightMap/Bake Prefab Lightmaps")]
    public static void GenLightmap()
    {
        genBakeLightmapAndPrefab();

        //        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("----------------Update to Prefab Lightmap Finish -------------------------");


    }



    private static void RemoveLeaf()
    {
        //var leaves = GameObject.FindObjectsOfType<IGG.Game.LeafNode>();
        //foreach (var leaf in leaves)
        //{
        //    GameObject.DestroyImmediate(leaf);
        //}


    }


    [MenuItem("LightMap/Map/MapCheck")]
    public static void DoMapCheck()
    {

        RemoveLeaf();
        //MapMeshMgr.ModifyMeshCollider(false);

    }

    static string BUILDING_LAYER_NAME = "BigMapBuilding";

    static string[] EXCLUDE_TRANS_NAMES = new string[] { "con_castle_1" };

    private static void RecursiveChangeLayer(GameObject parentGameObject,string layer)
    {
        parentGameObject.layer = LayerMask.NameToLayer(layer);


        foreach (Transform child in parentGameObject.transform)
        {

            RecursiveChangeLayer(child.gameObject, layer);
        }
    }
    #endregion


    static string sceneName = null;
    static string scenePath = null;
    static string scenePlatFormPath = null;

    static string afterFix = "_All";

    static void CopyFolder(string srcPath, string tarPath)
    {
        if (!Directory.Exists(srcPath))
        {
            Directory.CreateDirectory(srcPath);
        }
        if (!Directory.Exists(tarPath))
        {
            Directory.CreateDirectory(tarPath);
        }
        CopyFile(srcPath, tarPath);
        string[] directionName = Directory.GetDirectories(srcPath);
        foreach (string dirPath in directionName)
        {
            string directionPathTemp = tarPath + "\\" + dirPath.Substring(srcPath.Length + 1);
            CopyFolder(dirPath, directionPathTemp);
        }
    }
    static void CopyFile(string srcPath, string tarPath)
    {
        string[] filesList = Directory.GetFiles(srcPath);
        foreach (string f in filesList)
        {
            string fTarPath = tarPath + "\\" + f.Substring(srcPath.Length + 1);
            if (File.Exists(fTarPath))
            {
                File.Copy(f, fTarPath, true);
            }
            else
            {
                File.Copy(f, fTarPath);
            }
        }
    }

    /// <summary>
    /// 生成lightmap和prefab资源
    /// </summary>
    /// 
    private static void genBakeLightmapAndPrefab()
    {
        if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }
        Debug.ClearDeveloperConsole();

        PrefabLightmapData[] pldArr = GameObject.FindObjectsOfType<PrefabLightmapData>();
        if (pldArr == null || pldArr.Length <= 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到必要的脚本PrefabLightmapData，请检查场景", "OK");
            return;
        }

//        Lightmapping.Bake();
        sceneLightmaps.Clear();

        string path = Path.Combine(Application.dataPath, LightMapsDir);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Scene curScene = EditorSceneManager.GetActiveScene();
        sceneName = Path.GetFileNameWithoutExtension(curScene.name);

        scenePath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + "/";

        string sceneOriginPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName;
        scenePlatFormPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + afterFix ;

        CopyFolder(sceneOriginPath, scenePlatFormPath);
        AssetDatabase.Refresh();


        scenePlatFormPath += "/";
        string resourcesPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + afterFix +  "_lightmap/" + sceneName;

        foreach (PrefabLightmapData pld in pldArr)
        {
            GameObject gObj = pld.gameObject;
            List<RendererInfo> renderers = new List<RendererInfo>();
            List<Texture2D> lightmapFars = new List<Texture2D>();
            List<Texture2D> lightmapNears = new List<Texture2D>();

            // scenePath copy至scenePlatFormPath中，之后在resourcesPath下生产asset
            genLightmapInfo(scenePlatFormPath, resourcesPath, gObj, renderers, lightmapFars, lightmapNears);

            pld.mRendererInfos = renderers.ToArray();
            pld.mLightmapFars = lightmapFars.ToArray();
            pld.mLightmapNears = lightmapNears.ToArray();

    //        GameObject targetPrefab = PrefabUtility.GetPrefabParent(gObj) as GameObject;

    //        if (targetPrefab != null)
    //        {
    //            //自定义存放的路径
    //            PrefabUtility.ReplacePrefab(gObj, targetPrefab);
    //        }
    //        else
    //        {
    //            //默认路径
				//string prefabPath = Path.GetDirectoryName(curScene.path) + "/"+ sceneName  + "/" + gObj.name + ".prefab";
    //            PrefabUtility.CreatePrefab(prefabPath, gObj, ReplacePrefabOptions.ConnectToPrefab);
    //        }

            //改变当前场景中的光照贴图信息
            PrefabLightmapData.ApplyLightmaps(pld.mRendererInfos, pld.mLightmapFars, pld.mLightmapNears);
        }
    }

    private static void genLightmapInfo(string scenePath, string resourcePath, GameObject root,
                                        List<RendererInfo> renderers, List<Texture2D> lightmapFars,
                                        List<Texture2D> lightmapNears)
    {
        MeshRenderer[] subRenderers = root.GetComponentsInChildren<MeshRenderer>();

        LightmapData[] srcLightData = LightmapSettings.lightmaps;

        foreach (MeshRenderer meshRenderer in subRenderers)
        {
            if (meshRenderer.lightmapIndex == -1) continue;

            RendererInfo renderInfo = new RendererInfo();
            renderInfo.renderer = meshRenderer;
            renderInfo.LightmapIndex = meshRenderer.lightmapIndex;
            renderInfo.LightmapOffsetScale = meshRenderer.lightmapScaleOffset;

            Texture2D lightmapFar = srcLightData[meshRenderer.lightmapIndex].lightmapColor;
            Texture2D lightmapNear = srcLightData[meshRenderer.lightmapIndex].lightmapDir;

            int sceneCacheIndex = addLightmap(scenePath, resourcePath, renderInfo.LightmapIndex, lightmapFar,
                lightmapNear);

            renderInfo.LightmapIndex = lightmapFars.IndexOf(sceneLightmaps[sceneCacheIndex].LightmapFar);
            if (renderInfo.LightmapIndex == -1)
            {
                renderInfo.LightmapIndex = lightmapFars.Count;
                lightmapFars.Add(sceneLightmaps[sceneCacheIndex].LightmapFar);
                lightmapNears.Add(sceneLightmaps[sceneCacheIndex].LightmapNear);
            }

            renderers.Add(renderInfo);
        }
    }


    private static int addLightmap(string scenePath, string resourcePath, int originalLightmapIndex,
        Texture2D lightmapFar,
        Texture2D lightmapNear)
    {

        for (int i = 0; i < sceneLightmaps.Count; i++)
        {
            if (sceneLightmaps[i].OriginalLightmapIndex == originalLightmapIndex)
            {
                return i;
            }
        }


        RemapTexture2D remapTex = new RemapTexture2D();
        remapTex.OriginalLightmapIndex = originalLightmapIndex;
        remapTex.OrginalLightmap = lightmapFar;

        string fileName = scenePath + "Lightmap-" + originalLightmapIndex;
        remapTex.LightmapFar = getLightmapAsset(fileName + "_comp_light.exr", resourcePath + "_light",
            originalLightmapIndex,false);

        if (lightmapNear != null)
            remapTex.LightmapNear = getLightmapAsset(fileName + "_comp_dir.exr", resourcePath + "_dir",
                originalLightmapIndex,true);

        sceneLightmaps.Add(remapTex);

        return sceneLightmaps.Count - 1;
    }


    private static Texture2D getLightmapAsset(string fileName, string resourecPath, int originalLightmapIndex, bool ifDir)
    {
        string assetPath = resourecPath + "_" + originalLightmapIndex;
        if (!ifDir)
            assetPath += ".exr";
        else
            assetPath += ".png";

        string dir = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        if (File.Exists(assetPath))
        {
            File.Copy(fileName, assetPath, true);
        }
        else
        {

            File.Copy(fileName, assetPath);

        }

        //File.Copy(fileName, assetPath, true);
        AssetDatabase.Refresh();
        Texture2D newLightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        return newLightmap;
    }

    //according to the current platform, update all the lightmap that assets refrence
    private static void UpdateSceneLightmap()
    {

        PrefabLightmapData[] pldArr = GameObject.FindObjectsOfType<PrefabLightmapData>();
        if (pldArr == null || pldArr.Length <= 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到必要的脚本PrefabLightmapData，请检查场景", "OK");
            return;
        }

        //        Lightmapping.Bake();
        sceneLightmaps.Clear();

        string path = Path.Combine(Application.dataPath, LightMapsDir);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Scene curScene = EditorSceneManager.GetActiveScene();
        sceneName = Path.GetFileNameWithoutExtension(curScene.name);

        string resourcesPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + afterFix + "_lightmap/" ;

        List<string> farNames = new List<string>();
        List<string> nearNames = new List<string>();
        foreach (PrefabLightmapData pld in pldArr)
        {
            GameObject gObj = pld.gameObject;
            List<RendererInfo> renderers = new List<RendererInfo>();
            List<Texture2D> lightmapFars = new List<Texture2D>();
            List<Texture2D> lightmapNears = new List<Texture2D>();

            RemapTexture2D remapTex = new RemapTexture2D();

            var originFars = pld.mLightmapFars;
            var originNears = pld.mLightmapNears;

            if (originFars != null && originFars.Length > 0)
            {

                foreach (var tex in originFars)
                {
                    if (tex == null)
                        continue;

                    string fullpath = AssetDatabase.GetAssetPath(tex);
                    int lastVal = fullpath.LastIndexOf("_ANDROID");
                    if (lastVal > 0)
                    {
                        fullpath = fullpath.Replace("_ANDROID", afterFix);
                    }
                    else if (fullpath.LastIndexOf("_IOS") > 0)
                    {
                        fullpath = fullpath.Replace("_IOS", afterFix);
                    }
                    else
                    {
                        fullpath = fullpath.Replace("_WIN", afterFix);
                    }


                    string fileName = fullpath;

                    Debug.Log(fileName);
                    if (farNames.Contains(fileName))
                        continue;
                    else
                        farNames.Add(fileName);

                    remapTex.LightmapFar = UpdateLightmapAsset(fileName );
                    lightmapFars.Add(remapTex.LightmapFar);

                }

            }

            if (originNears != null && originNears.Length > 0)
            {

                foreach (var tex in originNears)
                {

                    if (tex == null)
                        continue;

                    string fullpath = AssetDatabase.GetAssetPath(tex);
                    int lastVal = fullpath.LastIndexOf("_ANDROID");
                    if (lastVal > 0)
                    {
                        fullpath = fullpath.Replace("_ANDROID", afterFix);
                    }
                    else if (fullpath.LastIndexOf("_IOS") > 0)
                    {
                        fullpath = fullpath.Replace("_IOS", afterFix);
                    }
                    else
                    {
                        fullpath = fullpath.Replace("_WIN", afterFix);
                    }

                    string fileName = fullpath;

                    if (nearNames.Contains(fileName))
                        continue;
                    else
                        nearNames.Add(fileName);

                    remapTex.LightmapNear = UpdateLightmapAsset(fileName);
                    lightmapNears.Add(remapTex.LightmapNear);
                }
            }
            sceneLightmaps.Add(remapTex);

            //pld.mRendererInfos = renderers.ToArray();
            pld.mLightmapFars = lightmapFars.ToArray();
            pld.mLightmapNears = lightmapNears.ToArray();

            GameObject targetPrefab = PrefabUtility.GetPrefabParent(gObj) as GameObject;

            if (targetPrefab != null)
            {
                //自定义存放的路径
                PrefabUtility.ReplacePrefab(gObj, targetPrefab);
            }
            else
            {
                //默认路径
                //                string prefabPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + ".prefab";
                string prefabPath = Path.GetDirectoryName(curScene.path) + "/" + sceneName + "/" + gObj.name + ".prefab";
                PrefabUtility.CreatePrefab(prefabPath, gObj, ReplacePrefabOptions.ConnectToPrefab);
            }

            //改变当前场景中的光照贴图信息
            PrefabLightmapData.ApplyLightmaps(pld.mRendererInfos, pld.mLightmapFars, pld.mLightmapNears);
        }

        Debug.Log("*******************update finish*****************************");
    }

    private static Texture2D UpdateLightmapAsset(string fileName)
    {
        string assetPath = fileName;
        Texture2D newLightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        return newLightmap;
    }
}