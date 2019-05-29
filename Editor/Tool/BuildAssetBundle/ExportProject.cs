using System.Collections.Generic;
using IGG.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExportProject
{
	// 导出类型
	public enum ExportType
	{
		ASSET_BUNDLE = 1 << 0, // AssetBundle
		PROJECT = 1 << 1, // 工程
		UWA = 1 << 2, // 开启UWA
	}

	// 设置architeure和backend
	static void BuildSetScriptingBackend(BuildTarget target, int flag)
	{
		BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

		ScriptingImplementation backend = ScriptingImplementation.IL2CPP;
		int architecture = -1;

		switch (flag)
		{
			case 1: // IL2CPP ArmV7
				backend = ScriptingImplementation.IL2CPP;
				architecture = 0;
				break;

			case 2: // IL2CPP Arm64
				backend = ScriptingImplementation.IL2CPP;
				architecture = 1;
				break;

			case 3: // IL2CPP Universal
				backend = ScriptingImplementation.IL2CPP;
				architecture = 2;
				break;

			default: // other Use Mono2.x
				backend = ScriptingImplementation.Mono2x;
				break;
		}

		PlayerSettings.SetScriptingBackend(group, backend);
		switch (target)
		{
			case BuildTarget.iOS:
				if (architecture != -1)
				{
					PlayerSettings.SetArchitecture(group, architecture);
				}
				break;

			case BuildTarget.Android:
				if (architecture != -1)
				{
					// ARMv7 ARM64 x86
					PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
				}
				else
				{
					// ARMv7 x86
					PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
				}
				break;
		}
	}

	// 打包前设置
	static void BuildSetting(BuildTarget target)
	{
		BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

		string id = CommandHelper.BundleId;
		if (!string.IsNullOrEmpty(id))
		{
			PlayerSettings.applicationIdentifier = id;
		}

		string symbols = CommandHelper.Symbols;
		if (!string.IsNullOrEmpty(symbols))
		{
			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
		}

		Debug.LogFormat("--> {0}", symbols);
	}

	static void InitUWA()
	{
		PlayerSettings.Android.forceSDCardPermission = true;

		string LaunchSceneName = "Lauch";

		UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		if (!string.Equals(scene.name, LaunchSceneName))
		{
			string pathScene = string.Format("Assets/Scene/{0}.unity", LaunchSceneName);
			scene = EditorSceneManager.OpenScene(pathScene);
		}

		string UWAName = "UWA_Android";

		GameObject go = GameObject.Find(UWAName);
		if (go != null)
		{
            Object.DestroyImmediate(go);
			go = null;
		}

		go = new GameObject(UWAName);
		string pathPrefab = string.Format("Assets/UWA/Prefabs/{0}.prefab", UWAName);
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathPrefab);
		PrefabUtility.ConnectGameObjectToPrefab(go, prefab);

		go = GameObject.Find(UWAName);
		go.AddComponent<IGG.FrameWork.Utils.DontDestroyComp>();

		EditorSceneManager.SaveScene(scene);
	}

	static void CopyConfig()
	{
		string pathSrc = "Assets/Config";
		string pathDst = "Assets/StreamingAssets/Config";
		List<string> files = new List<string>();

		IGG.FileUtil.GetAllChildFiles(pathSrc, "csv", files);
		IGG.FileUtil.GetAllChildFiles(pathSrc, "txt", files);

		foreach (string file in files)
		{
			string fileDst = file.Replace("\\", "/").Replace(pathSrc, pathDst);
			IGG.FileUtil.CopyFile(file, fileDst);
		}

		AssetDatabase.Refresh();
	}

	// 打包
	static void BuildProject(BuildTarget target)
	{
		// 设置编译参数
		BuildSetScriptingBackend(target, CommandHelper.Arch);

		// Splash Screen
		{
			PlayerSettings.SplashScreen.show = false;
			//PlayerSettings.SplashScreen.show = true;
			//PlayerSettings.SplashScreen.showUnityLogo = false;
			//PlayerSettings.SplashScreen.backgroundColor = Color.black;

			//List<PlayerSettings.SplashScreenLogo> logos = new List<PlayerSettings.SplashScreenLogo>();
			//logos.Add(PlayerSettings.SplashScreenLogo.Create(2, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/logo.png")));
			//PlayerSettings.SplashScreen.logos = logos.ToArray();
		}

		//PlayerSettings.bundleVersion = ConstantData.MainVersion;
		PlayerSettings.bundleVersion = CommandHelper.BundleVersion;

		switch (target)
		{
			case BuildTarget.iOS:
				PlayerSettings.productName = ConfigDataHelper.GameName;

				// 禁用自动签名
				PlayerSettings.iOS.appleEnableAutomaticSigning = false;
				PlayerSettings.iOS.buildNumber = ConfigDataHelper.RevisionGame.ToString();
				break;

			case BuildTarget.Android:
				PlayerSettings.productName = "zero";
				PlayerSettings.Android.useAPKExpansionFiles = CommandHelper.IsSplitResource;

				// 使用ETC
				//EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
				//PlayerSettings.Android.bundleVersionCode = (int) ConfigDataHelper.RevisionGame;
				PlayerSettings.Android.bundleVersionCode = CommandHelper.BundleCode;

				PlayerSettings.Android.keystorePass = "123456";
				PlayerSettings.Android.keyaliasPass = "123456";
				PlayerSettings.Android.keystoreName = EditorHelper.GetProjPath("Tools/Keystore/user.keystore");
				break;

			default:
				// 其它,认为是Standalone
				break;
		}

		if (CommandHelper.IsBattleDebug)
		{
			CopyConfig();
		}

		BuildVersionWnd.CreateFullVersion(CommandHelper.PlatformPath, CommandHelper.PackagePath, CommandHelper.IsDevelopment, false, CommandHelper.IsBattleDebug, CommandHelper.IsApk);
	}

	// 保存当前平台类型
	static void SavePlatform()
	{
		BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
		BuildSetting(target);

		SimpleJSON.JSONClass json = new SimpleJSON.JSONClass();
		json.Add("platform", new SimpleJSON.JSONData((int) target));
		json.Add("version", new SimpleJSON.JSONData(ConstantData.mainVersion));

		IGG.FileUtil.SaveTextToFile(json.ToString(""), CommandHelper.OutputPath);
	}

	// 重新导出AssetBundle信息
	static void ReimportAssetBundle()
	{
		//CommandHelper.InitFromCommandLine();
		//Cfg2AssetsTool.EncodeAllCfg();
		IGGBuildPipeline.ReimportAll(true);
	}

	// 导出工程
	static void Export()
	{
		CommandHelper.InitFromCommandLine();

		BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
		BuildSetting(target);

		ExportType et = CommandHelper.ExportType;
		Debug.LogFormat("Export Type: {0}", et);

		if ((et & ExportType.ASSET_BUNDLE) != 0)
		{
			// 序列化配置
			Cfg2AssetsTool.EncodeAllCfg();
            // 导出AssetBundle
            IGGBuildPipeline.BuildAssetbundle();
		}

		if ((et & ExportType.UWA) != 0)
		{
			InitUWA();
		}

		if ((et & ExportType.PROJECT) != 0)
		{
#if HOTFIX_ENABLE
			// xlua生成
			CSObjectWrapEditor.Generator.GenAll();
#endif

			// 导出工程
			BuildProject(target);

#if HOTFIX_ENABLE
			CSObjectWrapEditor.Generator.ClearAll();
#endif
		}
	}

	static void Patch()
	{
		CommandHelper.InitFromCommandLine();

		// 序列化配置
		Cfg2AssetsTool.EncodeAllCfg();

        // 导出AssetBundle
        IGGBuildPipeline.BuildAssetbundle();

		// 拷贝
		FullVersionResource.CopyAssets(false);

		// 拷贝到补丁仓库
		FullVersionResource.CopyPatch();
	}

	static void CheckAssets()
	{
		string filename = CommandHelper.OutputPath;
		IGG.FileUtil.CreateDirectoryFromFile(filename);

		IGG.EditorTools.AssetCheck.AssetAutoCheck.Start(filename);
	}

	static void GenSolution()
	{
		string path = "Assets/Editor/Build/ExportProject.cs";
		MonoScript item = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
		if (item == null)
		{
			return;
		}

		AssetDatabase.OpenAsset(item);
	}

	//[MenuItem("Me/Test")]
	//static void MeTest()
	//{
	//	//BuildAssetBundle.ReimportAll(true, true, false);
	//	//BuildAssetBundle.ReimportRedundance();
	//	//InitUWA();

	//	// FullVersionResource.InitPatch();
	//	// FullVersionResource.CopyPatch();
	//}
}