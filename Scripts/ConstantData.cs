#region Namespace

using IGG.Game;
using UnityEngine;

#endregion

public class ConstantData
{
    // AssetBundle缓存时间
    public static float AssetBundleCacheTime = 10;

    // 开启AssetBundle
#if UNITY_EDITOR
    public static bool EnableAssetBundle = false;
#else
    public static bool EnableAssetBundle = true;
#endif

    // 打AssetBundle前,清除旧的AssetBundle(全部重新打)
    public static bool ClearAssetBundleBeforeBuild = false;

    // 打AssetBundle前,重置AssetBundle Name
    public static bool ResetAssetBundleBeforeBuild = false;

    // 开启冗余资源检测
    public static bool EnableAssetBundleRedundance = true;

    // 开启缓存
    public static bool EnableCache = true;

    // 使用自定义压缩
    public static bool EnableCustomCompress = false;

    // 开启资源解压
    public static bool EnableUnpack = false;

    // 开启检测更新
    public static bool EnablePatch
    {
        get
        {
            return ConfigData.Inst.EnablePatch;
        }
    }

    // AssetBundle使用md5名
    public static bool EnableMd5Name = true;

    // AssetBundle描述文件名
    public const string AssetbundleManifest = "data";

    // AssetBundle资源映射文件名
    public const string AssetbundleMapping = "ab_mapping";

    // AssetBundle后缀
    public const string AssetBundleExt = ".ab";

    private static bool m_openGpuSkin = true;
    public static bool UseGpuSkin {
        get { return SystemInfo.supportsInstancing && m_openGpuSkin; }
    }

    // 主版本,每个里程碑累加
    public static string MainVersion = "0.16.0";
    
    private static string g_version;
    private static string g_fullVersion;

    public static void ResetFullVersion()
    {
        g_version = "";
        g_fullVersion = "";
    }

    // 版本号 [主版本号].[BuildID]
    public static string Version
    {
        get
        {
            if (string.IsNullOrEmpty(g_version))
            {
                g_version = string.Format("{0}.{1}", ConstantData.MainVersion, ConfigData.Inst.BuildId);
            }
            return g_version;
        }
    }

    // 完整版本号
    // [trunk|dev|release]_[MainVersion].[BuildId]_r[RevGame]_r[RevConfig]_r[RevProject]
    public static string FullVersion
    {
        get
        {
            if (string.IsNullOrEmpty(g_fullVersion))
            {
                g_fullVersion = string.Format("{0}_{1}.{2}_r{3}_r{4}_r{5}",
                    ReleaseTypeName, MainVersion, ConfigData.Inst.BuildId, ConfigData.Inst.RevisionGame, ConfigData.Inst.RevisionConfig, ConfigData.Inst.RevisionProject);
            }
            return g_fullVersion;
        }
    }

    private static string ReleaseTypeName
    {
        get
        {
            switch (Release)
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

    public static ReleaseType Release
    {
        get
        {
            return ConfigData.Inst.Release;
        }
    }

    public static bool ChinaMainland
    {
        get
        {
            return ConfigData.Inst.ChinaMainland;
        }
    }

    public static string AssetBundleSavePath
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return Application.streamingAssetsPath + "/ab/" + ConstantData.MainVersion + "/";
#else
            return Application.persistentDataPath + "/ab/";
#endif
        }
    }

    /// <summary>
    /// 默认分辨率
    /// </summary>
    /// 
    public static float DefaultResolutionWidth = 1920;

    public static float DefaultResolutionHeight = 1080;

    public static string BattleDataPath
    {
        get { return Application.persistentDataPath + "/Battle/"; }
    }

    public static string BattleReplayPath
    {
        get { return Application.persistentDataPath + "/Replay/" + ConstantData.MainVersion + "/"; }
    }

    public static string BattleReplayExt
    {
        get { return ".Replay"; }
    }

    public static string ServerListServerPath
    {
        get { return "http://config.nos-eastchina1.126.net/"; }
    }

    public static string ServerListSavePath
    {
        get { return Application.persistentDataPath + "/"; }
    }

    public static string ServerListFile
    {
        get { return "serverlist.txt"; }
    }

    private static string g_updateUrl;

    public static string UpdateUrl
    {
        get
        {
            if (string.IsNullOrEmpty(g_updateUrl))
            {
                string url = "http://static-bc.igg.com/";

#if UNITY_IOS
                string platformName = "ios";
#elif UNITY_ANDROID
                string platformName = "android";
#else
                string platformName = "android";
#endif

                g_updateUrl = string.Format("{0}{1}/{2}/{3}/", url, ReleaseTypeName, platformName, ConstantData.MainVersion);
            }

            return g_updateUrl;
        }
    }

    // 服务条款
    public const string UrlAgreement = "https://www.igg.com/about/agreement.php";

    // 应用商店
    public static string UrlAppStore
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
    public static bool EnterBackgroundForReconnect = false; // 是否退到后台
    public static float EnterBackgroundTime = 0f; // 退到后台的时间
    public static float MaxReconnectTimeFromBackground = 30 * 60; // 退到后台时间小于30分钟,断线重连,否则重新登录

    public const string DataPath = "Data"; // 资源路径(Assets下的相对路径)
    public const string AssetBundlePath = "ab"; // AssetBundle相对路径

    private static string g_dataPath; // 资源绝对路径
    private static string g_streamingAssetsPath; // 资源的ab包绝对路径
    private static string g_unpackPath; // 解压绝对路径
    private static string g_patchPath; // 补丁绝对路径
    private static string g_wwisePatchPath; // Wwise补丁绝对路径
    private static string g_tempPath; // 临时文件绝对路径

    // 资源绝对路径
    public static string DataFullPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_dataPath))
            {
                g_dataPath = string.Format("{0}/{1}", Application.dataPath, DataPath);
            }

            return g_dataPath;
        }
    }

    // 资源的ab包绝对路径
    public static string StreamingAssetsPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_streamingAssetsPath))
            {
                g_streamingAssetsPath = string.Format("{0}/{1}", Application.streamingAssetsPath, AssetBundlePath);
            }

            return g_streamingAssetsPath;
        }
    }

    // 解压绝对路径
    public static string UnpackPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_unpackPath))
            {
                g_unpackPath = string.Format("{0}/{1}", Application.persistentDataPath, AssetBundlePath);
            }

            return g_unpackPath;
        }
    }

    // 更新资源绝对路径
    public static string PatchPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_patchPath))
            {
                g_patchPath = string.Format("{0}/patch", Application.persistentDataPath);
            }

            return g_patchPath;
        }
    }

    public static string WwisePatchPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_wwisePatchPath))
            {
                g_wwisePatchPath = string.Format("{0}/wwise", Application.persistentDataPath);
            }

            return g_wwisePatchPath;
        }
    }

    // 临时文件绝对路径
    public static string TempPath
    {
        get
        {
            if (string.IsNullOrEmpty(g_tempPath))
            {
                g_tempPath = string.Format("{0}/temp", Application.persistentDataPath);
            }

            return g_tempPath;
        }
    }
}