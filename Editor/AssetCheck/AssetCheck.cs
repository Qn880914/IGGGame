using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public class AssetCheck
{
    // 检测AssetBundle交叉依赖
    [@MenuItem("辅助工具/检测/检测AssetBundle交叉依赖")]
    static void CheckAssetBundleCross()
    {
        StringBuilder builder = new StringBuilder();

        string[] names = AssetDatabase.GetAllAssetBundleNames();
        foreach (string name in names)
        {
            string[] dependencies = AssetDatabase.GetAssetBundleDependencies(name, true);
            foreach (string dependency in dependencies)
            {
                string[] dependencies2 = AssetDatabase.GetAssetBundleDependencies(dependency, true);
                foreach (string dependency2 in dependencies2)
                {
                    if (string.Equals(name, dependency2))
                    {
                        builder.AppendFormat("{0} <-> {1}\n", name, dependency);
                    }
                }
            }
        }

        string txt = builder.ToString();
        if (!string.IsNullOrEmpty(txt))
        {
            Debug.LogFormat("以下ab出现交叉依赖:\n{0}", txt);
        }
        else
        {
            Debug.Log("没有出现交叉依赖");
        }
    }

    // 检测材质是否使用Standard的Shader
    [@MenuItem("辅助工具/检测/检测材质是否使用Standard.shader")]
    static void CheckMaterial()
    {
        string[] assets = AssetDatabase.FindAssets("t:Material", new string[] { "Assets" });
        foreach (string asset in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader && mat.shader.name == "Standard")
            {
                mat.shader = Shader.Find("Standard");
            }
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [@MenuItem("辅助工具/检测/检测是否使用Standard-Material")]
    static void CheckPrefab()
    {
        Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Data/Materials/Default_Material.mat");

        string[] assets = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
        foreach (string asset in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                bool need = false;
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; ++i)
                {
                    Material mat = mats[i];
                    if (mat != null && mat.name == "Default-Material")
                    {
                        need = true;
                        mats[i] = defaultMaterial;
                    }
                }

                if (need)
                {
                    renderer.sharedMaterials = mats;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
