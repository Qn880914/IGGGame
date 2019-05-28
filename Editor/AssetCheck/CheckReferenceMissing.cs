using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IGG.Validation;
using UnityEditor.SceneManagement;
using IGG.EditorTools.AssetCheck;

public class CheckReferenceMissing {

    [AutoCheckItem("引用丢失检测")]
    private static void AutoCheckProjectReferenceMissing()
    {
        string[] prefabPath;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
        string[] guids2 = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" });
        prefabPath = new string[guids.Length + guids2.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            prefabPath[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        }
        for (int i = 0; i < guids2.Length; i++)
        {
            prefabPath[guids.Length + i] = AssetDatabase.GUIDToAssetPath(guids2[i]);
        }
        for (int i = 0; i < prefabPath.Length; i++)
        {
            if (!prefabPath[i].EndsWith(".prefab") && !prefabPath[i].EndsWith(".unity"))
            {
                continue;
            }
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(prefabPath[i]);
            EditorUtility.DisplayProgressBar("检测预设-场景空引用", obj.name, (float)(i + 1) / prefabPath.Length);
            if (obj.GetType() == typeof(SceneAsset))
            {
                EditorSceneManager.OpenScene(prefabPath[i]);
                GameObject[] gos = Object.FindObjectsOfType<GameObject>();
                for (int j = 0; j < gos.Length; j++)
                {
                    FindMissingReference(obj.name, gos[j]);
                }
            }
            else
            {
                GameObject go = obj as GameObject;
                FindMissingReference("", go);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/规范检测/引用丢失检测", true)]
    private static bool CanCheckReferenceMissingFunc()
    {
        string path = "";
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            return true;
        }
        else
        {
            //选择的物体只要有一个是预设则返回true
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (AssetDatabase.GetAssetPath(Selection.objects[i]).EndsWith(".prefab") ||
                    AssetDatabase.GetAssetPath(Selection.objects[i]).EndsWith(".unity"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [MenuItem("Assets/规范检测/引用丢失检测(预设+场景)", false)]
    private static void CheckReferenceMissingFunc()
    {
        string path = "";
        string[] prefabPath;
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
            string[] guids2 = AssetDatabase.FindAssets("t:Scene", new string[] { path });
            prefabPath = new string[guids.Length + guids2.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                prefabPath[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }
            for (int i = 0; i < guids2.Length; i++)
            {
                prefabPath[guids.Length + i] = AssetDatabase.GUIDToAssetPath(guids2[i]);
            }
        }
        else
        {
            prefabPath = new string[Selection.objects.Length];
            for (int i = 0; i < prefabPath.Length; i++)
            {
                prefabPath[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
            }
        }

        for (int i = 0; i < prefabPath.Length; i++)
        {
            if (!prefabPath[i].EndsWith(".prefab") && !prefabPath[i].EndsWith(".unity"))
            {
                continue;
            }
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(prefabPath[i]);
            EditorUtility.DisplayProgressBar("检测预设-场景空引用", obj.name, (float)(i + 1) / prefabPath.Length);
            if (obj.GetType() == typeof(SceneAsset))
            {
                EditorSceneManager.OpenScene(prefabPath[i]);
                GameObject[] gos = Object.FindObjectsOfType<GameObject>();
                for (int j = 0; j < gos.Length; j++)
                {
                    FindMissingReference(obj.name, gos[j]);
                }
            }
            else
            {
                GameObject go = obj as GameObject;
                FindMissingReference("", go);
            }
        }
        Debug.Log("检测结束");
        EditorUtility.ClearProgressBar();
    }

    private static void FindMissingReference(string sceneName, GameObject go)
    {
        Component[] coms;
        if (string.IsNullOrEmpty(sceneName))
        {
            coms = go.GetComponentsInChildren<Component>();
        }
        //如果检测目标是场景，则只需要检测go上的所有组件，不需要检测go的children，因为FindObjectsOfType已经包含了children
        else
        {
            coms = go.GetComponents<Component>();
            sceneName += "场景/";
        }
        for (int j = 0; j < coms.Length; j++)
        {
            if (null == coms[j])
            {
                AssetCheckLogger.Log(sceneName + go.name + "丢失了组件");
                continue;
            }
            SerializedObject so = new SerializedObject(coms[j]);
            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                    {
                        AssetCheckLogger.Log(sceneName + FullObjectPath(coms[j]) + "/" + coms[j].GetType() + "." + sp.propertyPath + "丢失了引用");
                        //Debug.LogError(sceneName + FullObjectPath(coms[j]) + "/" + coms[j].GetType() + "." + sp.propertyPath + "丢失了引用", go);
                    }
                }
            }
        }
    }

    private static string FullObjectPath(Component go)
    {
        return go.transform.parent == null ? go.name : FullObjectPath(go.transform.parent) + "/" + go.name;
    }
}
