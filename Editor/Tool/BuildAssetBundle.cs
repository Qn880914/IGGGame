﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle
{
    public static string assetBundlePath { get { return IGG.EditorTools.EditorHelper.assetBundleDir; } }

    // 获取资源存储路径
    public static string GetAssetResourceSavePath(string type, ResourcesPathMode mode)
    {
        StringBuilder stringBuilder = new StringBuilder(string.Empty);
        if (mode == ResourcesPathMode.AssetBundle)
        {
            stringBuilder.Append(assetBundlePath);
        }

        string path = ResourcesPath.GetRelativePath(type, mode);
        stringBuilder.Append(path);
        return stringBuilder.ToString();
    }

    public static void Build()
    {
        ReimportAll(ConstantData.clearAssetBundle, ConstantData.resetAssetBundle, ConstantData.enableAssetBundleRedundance);

        AssetDatabase.RemoveUnusedAssetBundleNames();

        BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
        if (!ConstantData.EnableCache && ConstantData.EnableCustomCompress)
        {
            // 使用解压,AssetBundle不压缩,使用外部压缩
            buildOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
        }

        BuildPipeline.BuildAssetBundles(assetBundlePath, buildOptions, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }

    static Dictionary<string, HashSet<string>> ms_mapping = new Dictionary<string, HashSet<string>>();
    static void InitMapping()
    {
        ms_mapping.Clear();
    }

    static void AddMapping(string assetPath, string abName)
    {
        HashSet<string> files = null;
        if (!ms_mapping.TryGetValue(abName, out files))
        {
            files = new HashSet<string>();
            ms_mapping.Add(abName, files);
        }

        if (!files.Contains(assetPath))
        {
            files.Add(assetPath);
        }
    }

    static void SaveMapping()
    {
        string path = "Assets/Data/ab_mapping.asset";

        bool exist = true;
        AssetBundleMapping data = AssetDatabase.LoadAssetAtPath<AssetBundleMapping>(path);
        if (data == null)
        {
            exist = false;
            data = new AssetBundleMapping();
        }

        data.assetBundleInfos = new AssetBundleMapping.AssetBundleInfo[ms_mapping.Count];

        int i = 0;
        var iter = ms_mapping.GetEnumerator();
        while (iter.MoveNext())
        {
            AssetBundleMapping.AssetBundleInfo info = new AssetBundleMapping.AssetBundleInfo();
            info.assetBundleName = iter.Current.Key.ToLower();
            info.paths = new string[iter.Current.Value.Count];

            int j = 0;
            foreach (string file in iter.Current.Value)
            {
                info.paths[j] = file.Replace("\\", "/").Replace("Assets/Data/", "").Replace("Assets/", "").ToLower();
                ++j;
            }

            data.assetBundleInfos[i] = info;
            ++i;
        }

        iter.Dispose();

        if (exist)
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(data, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ms_mapping.Clear();

        Reimport(path, "ab_mapping");
    }

    // 设置指定资源的ab名
    static void Reimport(string assetPath, string abName)
    {
        var importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            Debug.LogErrorFormat("importer failed: {0}", assetPath);
            return;
        }
        abName = abName.ToLower().Replace("（", "").Replace("）", "");
        if (abName.StartsWith("data"))
        {
            abName = abName.Substring(5);
        }

        AddMapping(assetPath, abName);

        if (importer.assetBundleName != abName || importer.assetBundleVariant != "ab")
        {
            importer.assetBundleName = abName;
            importer.assetBundleVariant = "ab";

            importer.SaveAndReimport();
        }
    }

    // 设置文件夹下所有指定后缀的资源的ab名
    static void ReimportPath(string path, string abName, string ext)
    {
        List<string> files = IGG.FileUtil.GetAllChildFiles(path, ext);
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];
            Reimport(filepath, abName);
        }
    }

    // 设置文件夹下指定后缀资源的ab名,每个资源独立打成一个ab
    static void ReimportPathSingle(string inPath, string outPath, string ext)
    {
        List<string> files = IGG.FileUtil.GetAllChildFiles(inPath, ext);
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];

            string name = filepath.Replace("\\", "/").Replace(inPath, "").Replace(string.Format(".{0}", ext), "");
            name = string.Format("{0}{1}", outPath, name);
            Reimport(filepath, name);
        }
    }

    // 遍历文件夹,一个文件夹打成一个,ab的名称用最后一层文件夹名
    static void ReimportPathUsePathName(string inPath, string outPath, string ext)
    {
        List<string> files = IGG.FileUtil.GetAllChildFiles(inPath, ext);
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];

            filepath = filepath.Replace(inPath, outPath).Replace("\\", "/");
            filepath = filepath.Substring(0, filepath.LastIndexOf('/'));

            Reimport(files[i], filepath);
        }
    }

    static void ReimportPathWithResourceType(string typename)
    {
        string inPath = ResourcesPath.GetAssetResourceRunPath(typename, ResourcesPathMode.Editor);
        inPath = inPath.Substring(0, inPath.Length - 1);

        string outPath = ResourcesPath.GetRelativePath(typename, ResourcesPathMode.AssetBundle);
        outPath = outPath.Substring(0, outPath.Length - 1);

        string ext = ResourcesPath.GetFileExt(typename);
        ext = ext.Substring(1);

        ReimportPath(inPath, outPath, ext);
    }

    // 遍历文件夹,一个文件夹打成一个,ab的名称用最后一层文件夹名
    static void ReimportPathSingleWithResourceType(string typename)
    {
        string inPath = ResourcesPath.GetAssetResourceRunPath(typename, ResourcesPathMode.Editor);
        inPath = inPath.Substring(0, inPath.Length - 1);

        string outPath = ResourcesPath.GetRelativePath(typename, ResourcesPathMode.AssetBundle);
        outPath = outPath.Substring(0, outPath.Length - 1);

        string ext = ResourcesPath.GetFileExt(typename);
        ext = ext.Substring(1);

        ReimportPathSingle(inPath, outPath, ext);
    }

    // 设置文件夹下指定后缀资源的ab名,每个资源独立打成一个ab
    static void ReimportPathUsePathNameWidthResourceType(string typename)
    {
        string inPath = ResourcesPath.GetAssetResourceRunPath(typename, ResourcesPathMode.Editor);
        inPath = inPath.Substring(0, inPath.Length - 1);

        string outPath = ResourcesPath.GetRelativePath(typename, ResourcesPathMode.AssetBundle);
        outPath = outPath.Substring(0, outPath.Length - 1);

        string ext = ResourcesPath.GetFileExt(typename);
        ext = ext.Substring(1);

        ReimportPathUsePathName(inPath, outPath, ext);
    }

    // 场景
    static void ReimportScene()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes.Length > 0)
        {
            for (int i = 0; i < scenes.Length; ++i)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (string.IsNullOrEmpty(scene.path))
                {
                    continue;
                }

                string name = scene.path.Substring(scene.path.LastIndexOf("/") + 1);
                name = name.Substring(0, name.IndexOf(".")).ToLower();
                if (string.Equals(name, "lauch") || string.Equals(name, "empty"))
                {
                    // 启动场景随包发布,不需要打成ab
                    continue;
                }

                Reimport(scene.path, string.Format("scene/{0}", name));
            }
        }
    }

    static void CompressTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool needSave = false;
        TextureImporterPlatformSettings settings = null;

        settings = importer.GetPlatformTextureSettings("iPhone");
        if (!settings.overridden || settings.format != TextureImporterFormat.ASTC_RGBA_8x8 || settings.textureCompression == TextureImporterCompression.Uncompressed)
        {
            settings.overridden = true;
            settings.format = TextureImporterFormat.ASTC_RGBA_8x8;
            settings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(settings);

            needSave = true;
        }

        settings = importer.GetPlatformTextureSettings("Android");
        if (!settings.overridden || settings.format != TextureImporterFormat.ETC2_RGBA8 || settings.textureCompression == TextureImporterCompression.Uncompressed)
        {
            settings.overridden = true;
            settings.format = TextureImporterFormat.ETC2_RGBA8;
            settings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(settings);

            needSave = true;
        }

        if (needSave)
        {
            importer.SaveAndReimport();
        }
    }

    // FairyGUI
    public static void ReimportUI()
    {
        string path = ResourcesPath.GetAssetResourceRunPath(ResourcesType.UiFairy, ResourcesPathMode.Editor);
        path = path.Substring(0, path.Length - 1);

        List<string> files = IGG.FileUtil.GetAllChildFiles(path, "bytes");
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];
            string filename = Path.GetFileNameWithoutExtension(filepath);

            filename = filename.Replace("_fui", "");
            Reimport(filepath, "ui/" + filename);
        }

        files = IGG.FileUtil.GetAllChildFiles(path, "png");
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];
            string filename = Path.GetFileNameWithoutExtension(filepath);

            int index = filename.IndexOf("_atlas");
            if (index != -1)
            {
                filename = filename.Substring(0, index);
            }
            Reimport(filepath, "ui/" + filename + "_atlas");

            CompressTexture(filepath);
        }
    }

    // Lua
    static void ReimportLua()
    {
        string path = ResourcesPath.GetAssetResourceRunPath(ResourcesType.luaData, ResourcesPathMode.Editor);
        path = path.Substring(0, path.Length - 1);

        string tempPath = "Assets/Temp/lua";

        // 先删除旧的临时文件夹
        IGG.FileUtil.DeleteFileDirectory(tempPath);

        // 把所有的lua拷贝到Assets/Temp/lua文件夹下,整个文件夹打成一个lua.ab
        List<string> files = IGG.FileUtil.GetAllChildFiles(path, "lua");
        for (int i = 0; i < files.Count; ++i)
        {
            string pathSrc = files[i];
            string pathDst = pathSrc.Replace(path, tempPath).Replace(".lua", ".bytes");
            IGG.FileUtil.CopyFile(pathSrc, pathDst);
        }
        AssetDatabase.Refresh();

        ReimportPath(tempPath, "lua", "bytes");
    }

    // 打包冗余资源
    static void ReimportRedundance()
    {
        // 收集冗余资源
        Dictionary<string, HashSet<string>> assets = CollectionAssetBundle.CollectionRedundance();

        Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
        {
            var iter = assets.GetEnumerator();
            while (iter.MoveNext())
            {
                string name = iter.Current.Key;
                if (name.EndsWith(".shader"))
                {
                    continue;
                }

                string path = name.Substring(0, name.LastIndexOf("/"));
                List<string> files = null;
                if (!paths.TryGetValue(path, out files))
                {
                    files = new List<string>();
                    paths.Add(path, files);
                }

                files.Add(name);
            }
        }

        {
            var iter = paths.GetEnumerator();
            while (iter.MoveNext())
            {
                foreach (string file in iter.Current.Value)
                {
                    string abName = iter.Current.Key.Replace("Assets/", "");
                    if (file.EndsWith(".mat") && !file.EndsWith("_Material.mat"))
                    {
                        abName += "_mat";
                    }
                    else if (file.EndsWith("FBX"))
                    {
                        abName += "_model";
                    }

                    // 动作只能打到角色对应的ab包,测试中发现,有些特效也依赖动作,暂时先不处理动作相关的
                    if (abName.ToLower().StartsWith("data/units/hero"))
                    {
                        continue;
                    }

                    Reimport(file, abName);
                }
            }
        }
    }

    // 构建所有索引
    public static void ReimportAll(bool clearAssetBundle = true, bool reset = false, bool redundance = true)
    {
        if (clearAssetBundle)
        {
            // 清除现有ab目录
            IGG.FileUtil.ClearFileDirectory(assetBundlePath);
        }
        else
        {
            IGG.FileUtil.CreateFileDirectory(assetBundlePath);
        }

        if (reset)
        {
            // 重置所有的ab
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in names)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.RemoveUnusedAssetBundleNames();

        ReimportScene();
        ReimportUI();
        ReimportLua();

        ReimportPathSingle("Assets/Data/wnd", "wnd", "prefab");
        ReimportPathUsePathName("Assets/Data/atlas", "atlas", "png");
        ReimportPathUsePathName("Assets/Data/atlas", "atlas", "jpg");

        ReimportPath("Assets/Shader", "shader", "shader");
        ReimportPath("Assets/Shader", "shader", "shadervariants");
        ReimportPath("Assets/Shader", "shader", "cginc");
        ReimportPath("Assets/Data/Prefabs/Other", "prefab/other", "prefab");

        ReimportPathWithResourceType(ResourcesType.Config);
        ReimportPathSingleWithResourceType(ResourcesType.Audio);
        ReimportPathSingleWithResourceType(ResourcesType.Map);
        ReimportPathSingleWithResourceType(ResourcesType.MapData);
        ReimportPathWithResourceType(ResourcesType.Language);

        ReimportPathSingleWithResourceType(ResourcesType.ActorHero);
        ReimportPathSingleWithResourceType(ResourcesType.ActorShowHero);
        ReimportPathSingleWithResourceType(ResourcesType.ActorShowHeroLow);
        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierPrefab);
        ReimportPathSingleWithResourceType(ResourcesType.ActorNpc);

        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierGpuSkinMesh);
        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierGpuSkinAnim);
        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierAnimation);

        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierMaterial);
        ReimportPathSingleWithResourceType(ResourcesType.ActorSoldierGpuSkinMat);

        ReimportPathSingleWithResourceType(ResourcesType.SceneItem);
        ReimportPathSingleWithResourceType(ResourcesType.Building);
        ReimportPathSingleWithResourceType(ResourcesType.BuildingBody);
        ReimportPathSingleWithResourceType(ResourcesType.BuildingModel);
        ReimportPathSingleWithResourceType(ResourcesType.BuildingSite);
        ReimportPathSingleWithResourceType(ResourcesType.CityTree);

        ReimportPathSingleWithResourceType(ResourcesType.Skill);
        ReimportPathSingleWithResourceType(ResourcesType.Effect);

        ReimportPathSingleWithResourceType(ResourcesType.ComMaterial);
        ReimportPathSingleWithResourceType(ResourcesType.PngTexture);

        ReimportPathUsePathNameWidthResourceType(ResourcesType.HeroAnim);

        Reimport("Assets/Data/server_pm.txt", "server_pm");

        if (redundance)
        {
            ReimportRedundance();
        }

        SaveMapping();
    }
}