/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:36
	file base:	AnimatedUnitCompressor
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class AnimatedUnitCompressor : EditorWindow {
    Vector2 scrollPos = Vector2.zero;
    public List<GameObject> UnitPrefabs;
    public static string BonePrefix;
    private SerializedObject _serializedObject;
    private int _prefabsToTouch;
    private int _remainingPrefabsToTouch;

    [MenuItem("IGG/Animated Unit Compressor")]
    public static void ShowWindow() {
        GetWindow(typeof(AnimatedUnitCompressor));
    }

    public void OnEnable() {
        _serializedObject = new SerializedObject(this);
    }

    public void OnGUI()
    {
        _serializedObject.Update();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        GUILayout.Label("Specify the string prefix for all the bones. ", EditorStyles.helpBox);
        //EditorGUILayout.PropertyField(_serializedObject.FindProperty("BonePrefix"), true);
        BonePrefix = EditorGUILayout.TextField("Rigged_");

        GUILayout.Label("Drag and drop all the prefabs you wish to have compressed. " +
            "This will cause all unnecessary bone GameObjects to be lost.", EditorStyles.helpBox);
        EditorGUILayout.PropertyField(_serializedObject.FindProperty("UnitPrefabs"), true);

        if (GUILayout.Button("Start"))
        {
            _remainingPrefabsToTouch = 0;
            _prefabsToTouch = UnitPrefabs.Count;
            CompressModelBones();
        }

        EditorGUILayout.EndScrollView();
        _serializedObject.ApplyModifiedProperties();
    }

	void CompressModelBones () {
	    foreach (var unitPrefab in UnitPrefabs)
	    {
            _remainingPrefabsToTouch++;

	        if (unitPrefab == null) continue;

            UpdateProgressBar(unitPrefab.name);

            var prefabInstance = Instantiate(unitPrefab, new Vector3(), new Quaternion()) as GameObject;
	        if (prefabInstance == null) continue;

	        var exposedBones = new List<string>();
	        FindBonesRequiredToExpose(exposedBones, prefabInstance.transform);

            OptimizeAnimatorHierarchyPrefab(prefabInstance, exposedBones);
            OptimizeAnimatorHierarchyMesh(prefabInstance, exposedBones);
	        //UpdatePrefabWithNewModel(prefabInstance);

            // Save the prefab to disk and remove from scene:
            PrefabUtility.ReplacePrefab(prefabInstance, unitPrefab, ReplacePrefabOptions.ReplaceNameBased);
            DestroyImmediate(prefabInstance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
	    }
	}

    void UpdateProgressBar(string updateText) {
        if (_remainingPrefabsToTouch < _prefabsToTouch)
        {
            EditorUtility.DisplayProgressBar("Compressing Model Bones", updateText, (float)_remainingPrefabsToTouch / _prefabsToTouch);
        }
        else
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static void OptimizeAnimatorHierarchyPrefab(GameObject prefabInstance, List<string> exposedBones)
    {
        var animatorGO = prefabInstance.GetComponentInChildren<Animator>().gameObject;
        List<string> customBones = new List<string>();

        customBones.AddRange(exposedBones);

        var gameObjectParentPairDict = new Dictionary<GameObject, string>();
        FindEmptyGameObjects(prefabInstance, gameObjectParentPairDict);
        foreach (var customObject in gameObjectParentPairDict)
        {
            customBones.Add(customObject.Value);
            customObject.Key.transform.parent = null;
        }
        
        // Optimise the bones:
        AnimatorUtility.OptimizeTransformHierarchy(
            animatorGO, customBones.ToArray());

        foreach (var customObject in gameObjectParentPairDict)
        {
            //customObject.Key.transform.parent = prefabInstance.transform.FindInChildren(customObject.Value);
            customObject.Key.transform.parent = prefabInstance.transform.Find(customObject.Value);
        }
    }

    static void FindEmptyGameObjects(GameObject gameObject, Dictionary<GameObject, string> gameObjectParentPairDict) {
        List<string> objectToCache = new List<string>() {
            "boneShield", "cameraBone", "VFXpoint_top",
            "VFXpoint_mid", "VFXpoint_bottom", "VFXpoint_ground"
        };

        foreach (var obj in objectToCache)
        {
            var objGO = gameObject.transform.Find(obj);
            if(objGO != null)
                gameObjectParentPairDict.Add(objGO.gameObject, objGO.parent.name);
        }
    }

    static void OptimizeAnimatorHierarchyMesh(GameObject prefabInstance, List<string> exposedBones) {
        var mesh = GetSourceMesh(prefabInstance);
        var meshImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mesh)) as ModelImporter;
        if (meshImporter == null) return;

        meshImporter.optimizeGameObjects = true;
        meshImporter.extraExposedTransformPaths = exposedBones.ToArray();

        AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(mesh));
        AssetDatabase.Refresh();
    }

    static Mesh GetSourceMesh(GameObject prefab) {
        var meshRenderer = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).First();
        if (meshRenderer == null) return null;
        return meshRenderer.sharedMesh;
    }

    static void FindBonesRequiredToExpose(List<string> exposedBones, Transform transform)
    {
        // TODO: Do a sanity check for any components attached to rig GOs

        //var isRigging = transform.name.StartsWith(BonePrefix);

        //var isChildOfRigging =
        //        transform.parent != null && transform.parent.name.StartsWith(BonePrefix);

        //if (!isRigging && isChildOfRigging)
        //    exposedBones.Add(transform.parent.name);

        var isRigging = transform.name.StartsWith(BonePrefix);

        var isParentBone = transform.parent != null;
        if (isRigging && isParentBone) {
            exposedBones.Add(transform.name);
        }
           


        for (int x = 0; x < transform.childCount; x++)
        {
            //if (isRigging || transform.parent == null)
                FindBonesRequiredToExpose(exposedBones, transform.GetChild(x));
        }
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
}