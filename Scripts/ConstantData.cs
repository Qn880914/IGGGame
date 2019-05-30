#region Namespace

using IGG.Game;
using UnityEngine;

#endregion

public class ConstantData
{
    // AssetBundle缓存时间
    public static float assetBundleCacheTime = 10;

    // 开启AssetBundle
#if UNITY_EDITOR
    public static bool enableAssetBundle = false;
#else
    public static bool enableAssetBundle = true;
#endif




    // 开启缓存
    public static bool enableCache = true;

    // 使用自定义压缩
    public static bool enableCustomCompress = false;

    // 开启资源解压
    public static bool enableUnpack = false;

    // 开启检测更新
    public static bool enablePatch
    {
        get
        {
            return ConfigData.Inst.EnablePatch;
        }
    }

    // AssetBundle使用md5名
    public static bool enableMd5Name = true;

    // AssetBundle描述文件名
    public const string assetbundleManifest = "data";

    // AssetBundle资源映射文件名
    public const string assetbundleMapping = "ab_mapping";

    // AssetBundle后缀
    public const string assetBundleExt = ".ab";

    private static bool s_UseGpuSkin = true;
    public static bool useGpuSkin { get { return SystemInfo.supportsInstancing && s_UseGpuSkin; } }

    // 主版本,每个里程碑累加
    public readonly static string mainVersion = "0.16.0";
    
    private static string s_Version;
    private static string s_FullVersion;

    public static void ResetFullVersion()
    {
        s_Version = "";
        s_FullVersion = "";
    }

    // 版本号 [主版本号].[BuildID]
    public static string version
    {
        get
        {
            if (string.IsNullOrEmpty(s_Version))
            {
                s_Version = string.Format("{0}.{1}", ConstantData.mainVersion, ConfigData.Inst.buildID);
            }
            return s_Version;
        }
    }

    // 完整版本号
    // [trunk|dev|release]_[MainVersion].[BuildId]_r[RevGame]_r[RevConfig]_r[RevProject]
    public static string fullVersion
    {
        get
        {
            if (string.IsNullOrEmpty(s_FullVersion))
            {
                s_FullVersion = string.Format("{0}_{1}.{2}_r{3}_r{4}_r{5}",
                    releaseTypeName, mainVersion, ConfigData.Inst.buildID, ConfigData.Inst.revisionGame, ConfigData.Inst.revisionConfig, ConfigData.Inst.revisionProject);
            }
            return s_FullVersion;
        }
    }

    private static string releaseTypeName
    {
        get
        {
            switch (release)
            {
                case ReleaseType.DEV:
                    return "dev";
                case ReleaseType.INNER:
                    return "inner";
                case ReleaseType.RELEASE:
                    return "release";
                default:
                    return "trunk";

            }
        }
    }

    public static ReleaseType release
    {
        get
        {
            return ConfigData.Inst.release;
        }
    }

    public static bool chinaMainland
    {
        get
        {
            return ConfigData.Inst.ChinaMainland;
        }
    }

    public static string assetBundleSavePath
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return Application.streamingAssetsPath + "/ab/" + ConstantData.mainVersion + "/";
#else
            return Application.persistentDataPath + "/ab/";
#endif
        }
    }

    /// <summary>
    /// 默认分辨率
    /// </summary>
    public const int kDefaultResolutionWidth = 1920;

    public const int kDefaultResolutionHeight = 1080;

    public static string battleDataPath
    {
        get { return Application.persistentDataPath + "/Battle/"; }
    }

    public static string battleReplayPath
    {
        get { return Application.persistentDataPath + "/Replay/" + ConstantData.mainVersion + "/"; }
    }

    public static string battleReplayExt
    {
        get { return ".Replay"; }
    }

    public static string serverListServerPath
    {
        get { return "http://config.nos-eastchina1.126.net/"; }
    }

    public static string serverListSavePath
    {
        get { return Application.persistentDataPath + "/"; }
    }

    public static string serverListFile
    {
        get { return "serverlist.txt"; }
    }

    private static string s_UpdateUrl;
    public static string updateUrl
    {
        get
        {
            if (string.IsNullOrEmpty(s_UpdateUrl))
            {
                string url = "http://static-bc.igg.com/";

#if UNITY_IOS
                string platformName = "ios";
#elif UNITY_ANDROID
                string platformName = "android";
#else
                string platformName = "android";
#endif

                s_UpdateUrl = string.Format("{0}{1}/{2}/{3}/", url, releaseTypeName, platformName, ConstantData.mainVersion);
            }

            return s_UpdateUrl;
        }
    }

    // 服务条款
    public const string kUrlAgreement = "https://www.igg.com/about/agreement.php";

    // 应用商店
    public static string urlAppStore
    {
        get
        {
#if UNITY_IOS
            // AppStore
            return "https://www.igg.com";
#else
            // GooglePlay
            return "https://www.igg.com";
#endif
        }
    }

    // 退到后台
    public static bool enterBackgroundForReconnect = false; // 是否退到后台
    public static float enterBackgroundTime = 0f; // 退到后台的时间
    public static float maxReconnectTimeFromBackground = 30 * 60; // 退到后台时间小于30分钟,断线重连,否则重新登录

    public const string kDataPath = "Data"; // 资源路径(Assets下的相对路径)
    public const string kAssetBundlePath = "ab"; // AssetBundle相对路径

    /// <summary>
    ///     <para> 资源绝对路径 </para>
    /// </summary>
    private static string s_DataPath;
    public static string dataFullPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_DataPath))
            {
                s_DataPath = string.Format("{0}/{1}", Application.dataPath, kDataPath);
            }

            return s_DataPath;
        }
    }

    /// <summary>
    ///     <para> 资源的ab包绝对路径 </para>
    /// </summary>
    private static string s_StreamingAssetsPath;
    public static string streamingAssetsPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_StreamingAssetsPath))
            {
                s_StreamingAssetsPath = string.Format("{0}/{1}", Application.streamingAssetsPath, kAssetBundlePath);
            }

            return s_StreamingAssetsPath;
        }
    }


    /// <summary>
    ///     <para> 解压绝对路径 </para>
    /// </summary>
    private static string s_UnpackPath;
    public static string unpackPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_UnpackPath))
            {
                s_UnpackPath = string.Format("{0}/{1}", Application.persistentDataPath, kAssetBundlePath);
            }

            return s_UnpackPath;
        }
    }

    /// <summary>
    ///     <para> 补丁绝对路径 </para>
    /// </summary>
    private static string s_PatchPath;
    public static string patchPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_PatchPath))
            {
                s_PatchPath = string.Format("{0}/patch", Application.persistentDataPath);
            }

            return s_PatchPath;
        }
    }

    /// <summary>
    ///     <para> Wwise补丁绝对路径 </para>
    /// </summary>
    private static string s_WwisePatchPath;
    public static string wwisePatchPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_WwisePatchPath))
            {
                s_WwisePatchPath = string.Format("{0}/wwise", Application.persistentDataPath);
            }

            return s_WwisePatchPath;
        }
    }

    /// <summary>
    ///     <para> 临时文件绝对路径 </para>
    /// </summary>
    private static string s_TempPath;
    public static string tempPath
    {
        get
        {
            if (string.IsNullOrEmpty(s_TempPath))
            {
                s_TempPath = string.Format("{0}/temp", Application.persistentDataPath);
            }

            return s_TempPath;
        }
    }
}