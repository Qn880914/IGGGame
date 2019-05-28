//using System;
using UnityEngine;
using UnityEditor;
using IGG.AssetImportSystem;

/// <summary>
/// Author: mingzhang02
/// Date: 20180807
/// Desc: 按照ProjectImportSetting的设置重新导入资源。
/// 目前处理的资源有：模型、贴图
/// </summary>
public class ReimportAssetMenu
{
    [MenuItem("Assets/检查使用了TrailRenderer组件的预设", false)]
    private static void CheckUseTrailRendererComPrefab()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == obj)
            {
                continue;
            }
            EditorUtility.DisplayProgressBar("检测预设-场景空引用", obj.name, (float)(i + 1) / guids.Length);
            TrailRenderer trailCom = obj.GetComponentInChildren<TrailRenderer>(true);
            if (null != trailCom)
            {
                Debug.Log(assetPath + ","+obj.name);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/按规则重新导入资源", false)]
    private static void ReimportAssetByRule()
    {
        string path = "";
        string[] prefabPath;
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            string[] guids = AssetDatabase.FindAssets("t:texture2D", new string[] { path });
            string[] guids2 = AssetDatabase.FindAssets("t:Model", new string[] { path });
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
            if (!prefabPath[i].EndsWith(".FBX") && !prefabPath[i].EndsWith(".tga") &&
                !prefabPath[i].EndsWith(".png") && !prefabPath[i].EndsWith(".jpg"))
            {
                continue;
            }
            EditorUtility.DisplayProgressBar("按照设置重新导入", prefabPath[i], (float)(i + 1) / prefabPath.Length);
            AssetImporter ai = AssetImporter.GetAtPath(prefabPath[i]);
            if (null != ai)
            {
                ProjectImportSettings.ApplyRulesToObject(ai);
                ai.SaveAndReimport();
            }
        }
        EditorUtility.ClearProgressBar();
    }
}

