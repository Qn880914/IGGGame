using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IGG.AssetImportSystem
{
    public class TextureImportDataTool
    {
        public static void ReimportTextures(TextureImportData data)
        {
            if (data == null)
            {
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { data.AssetPath });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                
                if (!data.IsRecursive)
                {
                    string dir = path.Remove(path.LastIndexOf('/'));
                    if (!dir.Equals(data.AssetPath))
                    {
                        continue;
                    }
                }
                
                string name = path.Substring(path.LastIndexOf('/') + 1);
                if (data.IsMatch(name))
                {
                    AssetImporter ai = AssetImporter.GetAtPath(path);
                    if (null != ai)
                    {
                        ApplyRulesToTexture(ai, data);
                    }
                }
            }
        }
        public static void TextureImport(AssetImporter importer)
        {
            if (null == importer)
            {
                return;
            }
            string dir = importer.assetPath.Remove(importer.assetPath.LastIndexOf('/'));
            string name = importer.assetPath.Substring(importer.assetPath.LastIndexOf('/') + 1);
            TextureImportData rule = TextureImportDataManager.Instance.GetRule(dir, name);
            if (null != rule)
            {
                Debug.Log("apply rule:"+rule.AssetPath+","+rule.Index);
                ApplyRulesToTexture(importer, rule);
            }
        }

        public static void ApplyRulesToTexture(string path, TextureImportData data)
        {
            ApplyRulesToTexture(AssetImporter.GetAtPath(path), data);
        }

        public static void ApplyRulesToTexture(AssetImporter importer, TextureImportData data)
        {
            if (null == importer)
            {
                return;
            }
            TextureImporter tImporter = importer as TextureImporter;
            if (null == tImporter)
            {
                return;
            }
            if (tImporter.textureType != data.TextureType)
            {
                tImporter.textureType = data.TextureType;
            }
            tImporter.isReadable = data.ReadWriteEnable;
            tImporter.mipmapEnabled = data.Mipmap;

            if (data.MaxSize > 0)
            {
                tImporter.maxTextureSize = data.MaxSize;
            }

            TextureImporterPlatformSettings settingAndroid = tImporter.GetPlatformTextureSettings("Android");
            settingAndroid.overridden = true;
            settingAndroid.format = data.AndroidFormat;
            settingAndroid.maxTextureSize = tImporter.maxTextureSize;
            tImporter.SetPlatformTextureSettings(settingAndroid);

            TextureImporterPlatformSettings settingIos = tImporter.GetPlatformTextureSettings("iPhone");
            settingIos.overridden = true;
            settingIos.format = data.IosFormat;
            settingIos.maxTextureSize = tImporter.maxTextureSize;
            tImporter.SetPlatformTextureSettings(settingIos);

            tImporter.SaveAndReimport();
        }
    }
}