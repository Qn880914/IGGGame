using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleMapping : ScriptableObject
{
    public AssetBundleInfo[] assetBundleInfos;

    private Dictionary<string, string> m_PathMapAssetBundleName;

    public void Init()
    {
        m_PathMapAssetBundleName = new Dictionary<string, string>();

        foreach(var assetBundleInfo in assetBundleInfos)
        {
            for (int i = 0; i < assetBundleInfo.paths.Length; ++i)
                m_PathMapAssetBundleName[assetBundleInfo.paths[i]] = assetBundleInfo.assetBundleName;
        }
    }

    public string GetAssetBundleNameFromAssetPath(string path)
    {
        path = path.ToLower();
        if (path.StartsWith("data/"))
            path = path.Substring(path.IndexOf('/') + 1);

        string name;
        m_PathMapAssetBundleName.TryGetValue(path, out name);

        return name;
    }

    [Serializable]
    public class AssetBundleInfo
    {
        public string[] paths;
        public string assetBundleName;
    }
}