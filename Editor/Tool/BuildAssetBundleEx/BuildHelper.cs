using System.Collections.Generic;
using UnityEditor;

public class BuildHelper
{
    private static Dictionary<string, HashSet<string>> s_AssetBundleNameMapFilePaths = new Dictionary<string, HashSet<string>>();

    /// <summary>
    ///     <para> 构建所有索引 </para>
    /// </summary>
    /// <param name="clearAssetBundle"> </param>
    /// <param name="reset"></param>
    /// <param name="checkRedundance"></param>
    public static void ReimportAll(bool checkRedundance = true)
    {
        ReimportScene();
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

        if (checkRedundance)
        {
            ReimportRedundance();
        }

        SaveMapping();
    }

    /// <summary>
    ///     <para> reimport build settings scenes. </para>
    ///     
    ///     <para> Note : lauch scene release with package and don't need build into assetbundle. </para>
    /// </summary>
    private static void ReimportScene()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach(var scene in scenes)
        {
            if (string.IsNullOrEmpty(scene.path))
                continue;

            string name = scene.path.Substring(scene.path.LastIndexOf("/") + 1);
            name = name.Substring(0, name.IndexOf(".")).ToLower();

            if (string.Equals(name, "lauch") || string.Equals(name, "empty"))
                continue;

            Reimport(scene.path, string.Format("scene/{0}", name));
        }
    }

    /// <summary>
    ///     <para> reimport all .lua file with assetbundlename of "lua" </para>
    /// </summary>
    private static void ReimportLua()
    {
        string path = ResourcesPath.GetAssetResourceRunPath(ResourcesType.luaData, ResourcesPathMode.Editor);
        path = path.Substring(0, path.Length - 1);

        string tempPath = "Assets/Temp/Lua";
        FileUtil.DeleteFileOrDirectory(tempPath);

        List<string> files = FileHelper.GetAllChildFiles(path, "lua");
        for(int i = 0; i < files.Count; ++ i)
        {
            string pathSrc = files[i];
            string pathDst = pathSrc.Replace(path, tempPath).Replace(".lua", ".bytes");
            FileUtil.CopyFileOrDirectory(pathSrc, pathDst);
        }

        AssetDatabase.Refresh();

        ReimportPath(tempPath, "lua", "bytes");
    }

    /// <summary>
    ///     <para> reimport all files under the path separately </para>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    private static void ReimportPathSingle(string path, string prefix, string suffix)
    {
        List<string> files = FileHelper.GetAllChildFiles(path, suffix);
        for(int i = 0; i < files.Count; ++ i)
        {
            string filePath = files[i];

            string assetBundleName = filePath.Replace("\\", "/").Replace(path, "").Replace(string.Format(".{0}", suffix), "");
            assetBundleName = string.Format("{0}{1}", prefix, assetBundleName);
            Reimport(filePath, assetBundleName);
        }
    }

    /// <summary>
    ///     <para> Go through folders, making each folder into one, and assetbundlename with the last folder name </para>
    /// </summary>
    /// <param name="inPath"></param>
    /// <param name="outPath"></param>
    /// <param name="ext"></param>
    static void ReimportPathUsePathName(string inPath, string prefix, string suffix)
    {
        List<string> files = FileHelper.GetAllChildFiles(inPath, suffix);
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];

            filepath = filepath.Replace(inPath, prefix).Replace("\\", "/");
            filepath = filepath.Substring(0, filepath.LastIndexOf('/'));

            Reimport(files[i], filepath);
        }
    }

    static void ReimportPathWithResourceType(string typename)
    {
        string path = ResourcesPath.GetAssetResourceRunPath(typename, ResourcesPathMode.Editor);
        path = path.Substring(0, path.Length - 1);

        string assetBundleName = ResourcesPath.GetRelativePath(typename, ResourcesPathMode.AssetBundle);
        assetBundleName = assetBundleName.Substring(0, assetBundleName.Length - 1);

        string suffix = ResourcesPath.GetFileExt(typename);
        suffix = suffix.Substring(1);

        ReimportPath(path, assetBundleName, suffix);
    }

    /// <summary>
    ///     <para> Set the AssetBundle Name of specified resources </para>
    /// </summary>
    /// <param name="assetPath"> path of asset </param>
    /// <param name="assetBundleName"></param>
    private static void Reimport(string assetPath, string assetBundleName)
    {
        var importer = AssetImporter.GetAtPath(assetPath);
        if(importer == null)
        {
            UnityEngine.Debug.LogErrorFormat("importer failed : {0}", assetPath);
            return;
        }

        assetBundleName = assetBundleName.ToLower().Replace("(", "").Replace(")", "");

        AddMapping(assetPath, assetBundleName);

        if(importer.assetBundleName != assetBundleName || importer.assetBundleVariant != "ab")
        {
            importer.assetBundleName = assetBundleName;
            importer.assetBundleVariant = "ab";

            importer.SaveAndReimport();
        }
    }

    /// <summary>
    ///     <para> 设置文件夹下所有指定后缀的资源的ab名 </para>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetBundleName"></param>
    /// <param name="ext"></param>
    static void ReimportPath(string path, string assetBundleName, string ext)
    {
        List<string> files = FileHelper.GetAllChildFiles(path, ext);
        for (int i = 0; i < files.Count; ++i)
        {
            string filepath = files[i];
            Reimport(filepath, assetBundleName);
        }
    }

    private static void AddMapping(string assetPath, string assetBundleName)
    {
        HashSet<string> files = null;
        if (!s_AssetBundleNameMapFilePaths.TryGetValue(assetBundleName, out files))
        {
            files = new HashSet<string>();
            s_AssetBundleNameMapFilePaths.Add(assetBundleName, files);
        }

        if (!files.Contains(assetPath))
        {
            files.Add(assetPath);
        }
    }

}
