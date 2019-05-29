using IGG.Game;
using SimpleJSON;
using UnityEngine;

public static class CommandHelper
{
    static string GetValue(string key, string defaultValue)
    {
        int index = 0;
        string[] commandlines = System.Environment.GetCommandLineArgs();

        while (index < commandlines.Length)
        {
            if (commandlines[index].Equals("-executeMethod"))
            {
                break;
            }

            ++index;
        }

        string ret = defaultValue;

        index += 2;
        if (index < commandlines.Length)
        {
            JSONClass data = JSON.Parse(commandlines[index]) as SimpleJSON.JSONClass;
            if (data != null && !data[key].Equals(null))
            {
                ret = data[key];
            }
        }

        return ret;
    }

    static int GetValue(string key, int defaultValue)
    {
        return int.Parse(GetValue(key, defaultValue.ToString()));
    }

    static uint GetValue(string key, uint defaultValue)
    {
        return uint.Parse(GetValue(key, defaultValue.ToString()));
    }

    static bool GetValue(string key, bool defaultValue)
    {
        return (GetValue(key, defaultValue ? "1" : "0") == "1");
    }

    // 导出类型
    public static ExportProject.ExportType ExportType { get { return (ExportProject.ExportType) (GetValue("export_type", (int) ExportProject.ExportType.PROJECT)); } }

    // 项目名称
    //public static string ProductName { get { return GetValue("product_name", "zero"); } }
    // 平台工程位置
    public static string PlatformPath { get { return GetValue("platform_path", ""); } }
    // 安装包位置
    public static string PackagePath { get { return GetValue("package_path", ""); } }
    // Bundle Id
    public static string BundleId { get { return GetValue("id", ""); } }
    // 用户宏
    public static string Symbols { get { return GetValue("symbols", ""); } }
    // 图标
    //public static string IconName { get { return GetValue("icon", ""); } }
    // IL2CPP
    public static int Arch { get { return int.Parse(GetValue("arch", "3")); } }
    // 开发者模式
    public static bool IsDevelopment { get { return (GetValue("development", false)); } }
    // 输出路径
    public static string OutputPath { get { return GetValue("output", "output"); } }

    // 是否战斗测试
    public static bool IsBattleDebug { get { return (GetValue("battle_debug", false)); } }

    // 是否打包出apk(android)
    public static bool IsApk { get { return (GetValue("apk", false)); } }
    // 是否分包
    public static bool IsSplitResource { get { return (GetValue("split", false)); } }

    public static string BundleVersion { get { return GetValue("bundle_version", ConstantData.mainVersion); } }
    public static int BundleCode { get { return GetValue("bundle_code", (int) ConfigDataHelper.RevisionGame); } }

    public static void InitFromCommandLine()
    {
        ConfigDataHelper.Release = (ReleaseType) GetValue("release", (int) ConfigDataHelper.Release);
        ConfigDataHelper.BuildId = GetValue("build_id", ConfigDataHelper.BuildId);

        ConfigDataHelper.RevisionProject = GetValue("revision_project", ConfigDataHelper.RevisionProject);
        ConfigDataHelper.RevisionGame = GetValue("revision_game", ConfigDataHelper.RevisionGame);
        ConfigDataHelper.RevisionConfig = GetValue("revision_config", ConfigDataHelper.RevisionConfig);

        ConfigDataHelper.EnablePatch = GetValue("patch", ConfigDataHelper.EnablePatch);
        ConfigDataHelper.Censorship = GetValue("censorship", ConfigDataHelper.Censorship);
        ConfigDataHelper.ChinaMainland = GetValue("china_mainland", ConfigDataHelper.ChinaMainland);
    }
}