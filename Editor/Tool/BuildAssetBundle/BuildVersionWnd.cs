using UnityEditor;
using UnityEngine;
using System.IO;
using IGG.EditorTools;

public class BuildVersionWnd : EditorWindow
{
    [@MenuItem("版本发布/编译 GSServer.dll")]
	public static void BuildServerDLL() 
    {
        EditorHelper.DoBat("GenBattleDLL.bat", null, "/Tools/GameDll");
    }

    [@MenuItem("版本发布/编译 GSServerJournal.dll")]
    public static void BuildServerJournalDLL()
    {
        EditorHelper.DoBat("GenBattleJournalDLL.bat", null, "/Tools/GameDll");
    }

    [@MenuItem("版本发布/编译 Game.dll")]
    public static void BuildGameScriptDLL()
    {
        EditorHelper.DoBat("GenGameDll.bat", null, "/Tools/GameDll");
    }

    [MenuItem("版本发布/拷贝AssetBundle到StreamingAssets目录")]
	public static void CopyAssets()
	{
		FullVersionResource.CopyAssets();
	}

	[MenuItem("版本发布/拷贝配置文件到StreamingAssets目录")]
    public static void CopyConfigToStreamingAssets()
    {
        if (Directory.Exists(Application.streamingAssetsPath + "/Config"))
        {
            IGG.FileUtil.DeleteFileDirectory(Application.streamingAssetsPath + "/Config");
        }
        else
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Config");
        }
        IGG.FileUtil.CopyDirectory(Application.dataPath + "/Config", Application.streamingAssetsPath + "/Config");
    }

	// 整包版本。
	public static void CreateFullVersion(string pathPlatform = "", string pathPackage = "", bool development = false, bool autoConnectProfiler = false, bool isBattleDebug = false, bool isApk = true)
    {
		FullVersionResource.CopyAssets();
		if (ConfigDataHelper.EnablePatch)
		{
			FullVersionResource.InitPatch();
		}
		FullVersionResource.Build(pathPlatform, pathPackage, development, autoConnectProfiler, isBattleDebug, isApk);
		UnityEngine.Debug.Log(PlayerSettings.productName + "游戏发布完成");
	}

	// public static void SwitchPlatform(BuildTarget BuildVersionTarget)
    // {
	// 	if (BuildVersionTarget == BuildTarget.StandaloneWindows){
	// 		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
	// 		PlayerSettings.bundleVersion = LogicConstantData.Version;
	// 	}else if (BuildVersionTarget == BuildTarget.Android){
	// 		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
	// 		PlayerSettings.Android.keystorePass = "123456";
	// 		PlayerSettings.Android.keyaliasPass = "123456";
    //         PlayerSettings.Android.keystoreName = EditorHelper.GetProjPath("Tools/Keystore/user.keystore");
    //         PlayerSettings.bundleVersion = LogicConstantData.Version;
	// 	}else if (BuildVersionTarget == BuildTarget.iOS){
	// 		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
	// 		PlayerSettings.bundleVersion = LogicConstantData.Version;
    //     }
	// 	else if (BuildVersionTarget == BuildTarget.WebGL)
    //     {
	// 		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebGL);
	// 		PlayerSettings.bundleVersion = LogicConstantData.Version;
    //     }
    //     AssetDatabase.Refresh();
    // }
}
