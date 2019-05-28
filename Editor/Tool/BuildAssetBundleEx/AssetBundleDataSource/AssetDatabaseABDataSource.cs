using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBrowser.AssetBundleDataSource
{
    internal class AssetDatabaseABDataSource : ABDataSource
    {
        public static List<ABDataSource> CreateDataSources()
        {
            var op = new AssetDatabaseABDataSource();
            var retList = new List<ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string name { get { return "Default"; } }

        public string providerName { get { return "Built-in"; } }

        public bool canSpecifyBuildTarget { get { return true; } }

        public bool canSpecifyBuildOutputDirectory { get { return true; } }

        public bool canSpecifyBuildOptions { get { return true; } }

        public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
        }

        public string GetAssetBundleName(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (null == importer)
            {
                return string.Empty;
            }
            var bundleName = importer.assetBundleName;
            if (importer.assetBundleVariant.Length > 0)
            {
                bundleName = bundleName + "." + importer.assetBundleVariant;
            }

            return bundleName;
        }

        public string GetImplicitAssetBundleName(string assetPath)
        {
            return AssetDatabase.GetImplicitAssetBundleName(assetPath);
        }

        public string[] GetAllAssetBundleNames()
        {
            return AssetDatabase.GetAllAssetBundleNames();
        }

        public bool IsReadOnly()
        {
            return false;
        }

        public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if(null != importer)
            {
                importer.SetAssetBundleNameAndVariant(bundleName, variantName);
            }
        }

        public void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public bool BuildAssetBundles(BuildAssetBundleSettings settings)
        {
            if (null == settings)
            {
                Debug.Log("Error in build");
                return false;
            }

            var buildManifest = BuildPipeline.BuildAssetBundles(settings.outputDirectory, settings.options, settings.buildTarget);
            if (null == buildManifest)
            {
                Debug.Log("Error in build");
                return false;
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                if (null != settings.buildCallback)
                {
                    settings.buildCallback(assetBundleName);
                }
            }
            return true;
        }
    }
}
