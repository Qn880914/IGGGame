using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using IGG.EditorTools;
using IGG.Game;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

public class FullVersionResource
{

	//public static bool _bDebugVersion = false;
	private const string VERSION_PATH = "Assets/Data/version.asset";

	public static void CopyAssets(bool clear = false)
	{
		if (clear)
		{
			DeleteAssets();
		}

		List<VersionData.VersionItem> list = new List<VersionData.VersionItem>();
		List<string> files = IGG.FileUtil.GetAllChildFiles(BuildSettings.outputPath, "ab");

		if (!ConstantData.enableCache && ConstantData.enableCustomCompress)
		{
			// 自定义压缩
			string inPath = string.Format("{0}{1}", BuildSettings.outputPath, ConstantData.mainVersion);
			string outPath = "";
			if (ConstantData.enableMd5Name)
			{
				string md5 = IGG.FileUtil.CalcFileMd5(inPath);
				outPath = string.Format("{0}/{1}{2}", ConstantData.streamingAssetsPath, md5, ConstantData.assetBundleExt);
			}
			else
			{
				outPath = string.Format("{0}/data", ConstantData.streamingAssetsPath);
			}

			ThreadParam param = new ThreadParam();
			param.pathSrc = BuildSettings.outputPath;
			param.pathDst = ConstantData.streamingAssetsPath;
			param.list = list;
			param.files = files;
			param.index = 0;
			param.lockd = new object();

			PackFile(inPath, outPath, "data", param);

			int threadCount = SystemInfo.processorCount;

			List<Thread> threads = new List<Thread>();
			for (int i = 0; i < threadCount; ++i)
			{
				Thread thread = new Thread(new ParameterizedThreadStart(OnThreadCompress));
				thread.Start(param);

				threads.Add(thread);
			}

			while (true)
			{
				EditorUtility.DisplayProgressBar("压缩中...", string.Format("{0}/{1}", param.index, param.files.Count), Mathf.InverseLerp(0, param.files.Count, param.index));

				bool hasAlive = false;
				foreach (Thread thread in threads)
				{
					if (thread.IsAlive)
					{
						hasAlive = true;
						break;
					}
				}

				if (!hasAlive)
				{
					break;
				}

				Thread.Sleep(10);
			}
		}
		else
		{
			// 直接拷贝
			if (ConstantData.enableMd5Name)
			{
				string pathSrc = BuildSettings.outputPath;
				string pathDst = ConstantData.streamingAssetsPath;

				{
					string file = string.Format("{0}/{1}", pathSrc, ConstantData.mainVersion);
					CopyAsset(file, pathDst, list, "data");
				}

				int index = 0;
				foreach (string file in files)
				{
					string name = file.Replace("\\", "/").Replace(pathSrc, "");
					CopyAsset(file, pathDst, list, name);

					++index;
					EditorUtility.DisplayProgressBar("拷贝中...", string.Format("{0}/{1}", index, files.Count), Mathf.InverseLerp(0, files.Count, index));
				}
			}
			else
			{
				// 把所有的ab文件拷贝进StreamAssets的ab目录下
				IGG.FileUtil.CopyDirectory(BuildSettings.outputPath, Application.streamingAssetsPath + "/ab/", ConstantData.assetBundleExt);

				// 拷贝manifest进StreamAssets,并命名为data
				string pathSrc = string.Format("{0}/{1}", BuildSettings.outputPath, ConstantData.mainVersion);
				string pathDst = string.Format("{0}/ab/data", Application.streamingAssetsPath);
				IGG.FileUtil.CopyFile(pathSrc, pathDst);
			}
		}

		if (ConstantData.enableMd5Name)
		{
			ConfigDataHelper.SaveData<VersionDataProxy>(VERSION_PATH, (data) =>
			{
				data.Items = list.ToArray();
			});
		}

		ClearObsolete(list);

		EditorUtility.ClearProgressBar();
		AssetDatabase.Refresh();
	}

	static void ClearObsolete(List<VersionData.VersionItem> list)
	{
		Dictionary<string, string> items = new Dictionary<string, string>();
		for (int i = 0; i < list.Count; ++i)
		{
			items.Add(list[i].Md5, list[i].Name);
		}

		List<string> files = IGG.FileUtil.GetAllChildFiles(ConstantData.streamingAssetsPath, ConstantData.assetBundleExt);
		for (int i = 0; i < files.Count; ++i)
		{
			string filename = Path.GetFileNameWithoutExtension(files[i]);
			if (!items.ContainsKey(filename))
			{
				IGG.FileUtil.DeleteFile(files[i]);
			}
		}
	}

	static void DeleteAssets()
	{
		IGG.FileUtil.DeleteFile(Application.streamingAssetsPath + "/" + "ab.zip");
		IGG.FileUtil.DeleteFileDirectory(ConstantData.streamingAssetsPath);

		AssetDatabase.Refresh();
	}

	static void PackFile(string fileSrc, string fileDst, string name, ThreadParam param)
	{
		if (!IGG.FileUtil.CheckFileExist(fileDst))
		{
			Debug.LogFormat("-->PackFile: {0} -> {1}", fileSrc, fileDst);

			IGG.FileUtil.CreateDirectoryFromFile(fileDst);
			IGG.FileUtil.DeleteFile(fileDst);

			LzmaHelper.Compress(fileSrc, fileDst);
		}

		if (ConstantData.enableMd5Name)
		{
			string md5 = IGG.FileUtil.CalcFileMd5(fileSrc);

			FileInfo fi = new FileInfo(fileDst);

			VersionData.VersionItem item = new VersionData.VersionItem();
			item.Name = name.Replace(ConstantData.assetBundleExt, "");
			item.Md5 = md5;
			item.Size = fi.Length;

			lock(param.lockd)
			{
				param.list.Add(item);
			}
		}
	}

	static void CopyAsset(string file, string pathDst, List<VersionData.VersionItem> list, string name)
	{
		string md5 = IGG.FileUtil.CalcFileMd5(file);
		string pathDstFull = string.Format("{0}/{1}.ab", pathDst, md5);
		if (IGG.FileUtil.CheckFileExist(pathDstFull)) {}
		{
			IGG.FileUtil.CopyFile(file, pathDstFull);
		}

		FileInfo fi = new FileInfo(file);

		VersionData.VersionItem item = new VersionData.VersionItem();
		item.Name = name.Replace(ConstantData.assetBundleExt, "");
		item.Md5 = md5;
		item.Size = fi.Length;

		list.Add(item);
	}

	class ThreadParam
	{
		public string pathSrc;
		public string pathDst;

		public List<VersionData.VersionItem> list;

		public List<string> files;
		public int index;

		public object lockd;
	}

	static void OnThreadCompress(object arg)
	{
		ThreadParam param = arg as ThreadParam;
		while (true)
		{
			string file = "";

			lock(param.lockd)
			{
				if (param.index >= param.files.Count)
				{
					// 完成
					break;
				}

				file = param.files[param.index];
				++param.index;
			}

			string name = file.Replace("\\", "/").Replace(param.pathSrc, "");
			string fileDst = "";
			if (ConstantData.enableMd5Name)
			{
				string md5 = IGG.FileUtil.CalcFileMd5(file);
				fileDst = string.Format("{0}/{1}{2}", param.pathDst, md5, ConstantData.assetBundleExt);
			}
			else
			{
				fileDst = file.Replace(param.pathSrc, param.pathDst);
			}

			PackFile(file, fileDst, name, param);
		}
	}

	public static void InitPatch()
	{
		IGG.FileUtil.DeleteFileDirectory(BuildSettings.patchDir);

		// version_orign
		{
			string pathDst = string.Format("{0}/version_orign", BuildSettings.patchDir);

			VersionData data = ConfigDataHelper.GetData<VersionData>(VERSION_PATH);
			if (data != null)
			{
				JSONClass json = new JSONClass();
				for (int i = 0; i < data.Items.Length; ++i)
				{
					VersionData.VersionItem item = data.Items[i];

					JSONClass jsonItem = new JSONClass();
					jsonItem.Add("size", item.Size.ToString());
					jsonItem.Add("md5", item.Md5);

					json.Add(item.Name, jsonItem);
				}

				IGG.FileUtil.SaveTextToFile(json.ToString(""), pathDst);
			}
		}

		// audio_orign
		{
			string pathAudio = string.Format("{0}/../WwiseProject/GeneratedSoundBanks/{1}", Application.dataPath, EditorHelper.platformName);
			pathAudio = pathAudio.Replace("\\", "/");

			JSONClass jsonAudio = new JSONClass();

			List<string> files = IGG.FileUtil.GetAllChildFiles(pathAudio);
			foreach (string file in files)
			{
				string md5 = IGG.FileUtil.CalcFileMd5(file);
				string name = file.Replace("\\", "/").Replace(pathAudio, "");
				if (name.StartsWith("/"))
				{
					name = name.Substring(1);
				}

				jsonAudio.Add(name, md5);
			}

			string path = string.Format("{0}/audio_orign", BuildSettings.patchDir);
			IGG.FileUtil.SaveTextToFile(jsonAudio.ToString(""), path);
		}

		// version
		SaveVersionFile(new JSONClass());
	}

	private static void SaveVersionFile(JSONClass list)
	{
		JSONClass json = new JSONClass();
		json.Add("version", ConstantData.mainVersion);
		json.Add("build", ConfigDataHelper.BuildId.ToString());
		json.Add("game", ConfigDataHelper.RevisionGame.ToString());
		json.Add("config", ConfigDataHelper.RevisionConfig.ToString());
		json.Add("project", ConfigDataHelper.RevisionProject.ToString());

		JSONClass jsonList = new JSONClass();
		json.Add("list", list);

		string path = string.Format("{0}/version", BuildSettings.patchDir);
		IGG.FileUtil.SaveTextToFile(json.ToString(""), path);
	}

	public static void CopyPatch()
	{
		JSONClass jsonList = new JSONClass();

		// ab
		{
			string pathOrign = string.Format("{0}/version_orign", BuildSettings.patchDir);
			if (!File.Exists(pathOrign))
			{
				return;
			}

			JSONClass jsonOrign = JSONNode.Parse(IGG.FileUtil.ReadTextFromFile(pathOrign)) as JSONClass;
			VersionData data = ConfigDataHelper.GetData<VersionData>(VERSION_PATH);
			if (data != null)
			{
				for (int i = 0; i < data.Items.Length; ++i)
				{
					VersionData.VersionItem item = data.Items[i];
					if (jsonOrign[item.Name] != null && string.Equals(item.Md5, jsonOrign[item.Name]["md5"].Value))
					{
						continue;
					}

					string pathSrc = string.Format("{0}/{1}{2}", ConstantData.streamingAssetsPath, item.Md5, ConstantData.assetBundleExt);
					string pathDst = string.Format("{0}/ab/{1}{2}", BuildSettings.patchDir, item.Md5, ConstantData.assetBundleExt);
					IGG.FileUtil.CopyFile(pathSrc, pathDst);

					JSONClass itemJson = new JSONClass();
					itemJson.Add("md5", item.Md5);
					itemJson.Add("size", item.Size.ToString());
					itemJson.Add("type", new JSONData((int) PatchFileType.AssetBundle));

					jsonList.Add(item.Name, itemJson);
				}
			}
		}

		// audio
		{
			string pathOrign = string.Format("{0}/audio_orign", BuildSettings.patchDir);
			JSONClass jsonOrign = JSONNode.Parse(IGG.FileUtil.ReadTextFromFile(pathOrign)) as JSONClass;

			string pathAudio = string.Format("{0}/../WwiseProject/GeneratedSoundBanks/{1}", Application.dataPath, EditorHelper.platformName);
			pathAudio = pathAudio.Replace("\\", "/");

			List<string> files = IGG.FileUtil.GetAllChildFiles(pathAudio);
			foreach (string file in files)
			{
				string md5 = IGG.FileUtil.CalcFileMd5(file);
				long size = new FileInfo(file).Length;

				string filename = Path.GetFileName(file);
				if (jsonOrign[filename] == null || !string.Equals(md5, jsonOrign[filename].Value))
				{
					string pathDst = string.Format("{0}/wwise/{1}", BuildSettings.patchDir, filename);
					IGG.FileUtil.CopyFile(file, pathDst);

					JSONClass item = new JSONClass();
					item.Add("md5", md5);
					item.Add("size", size.ToString());
					item.Add("type", new JSONData((int) PatchFileType.Wwise));

					jsonList.Add(filename, item);
				}
			}
		}

		SaveVersionFile(jsonList);
	}

	public static void Build(string pathProject, string pathPackage, bool development, bool autoConnectProfiler, bool isBattleDebug = false, bool isApk = true)
	{
		List<string> scenes = new List<string>();
		if (isBattleDebug)
		{
			scenes.Add("Assets/Scene/LauchBattleDebug.unity");
		}
		else
		{
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (string.IsNullOrEmpty(scene.path))
				{
					continue;
				}

				string name = scene.path.Substring(scene.path.LastIndexOf("/") + 1);
				name = name.Substring(0, name.IndexOf(".")).ToLower();
				if (string.Equals(name, "lauch") || string.Equals(name, "empty"))
				{
					scenes.Add(scene.path);
				}
			}
		}

		BuildOptions options = BuildOptions.None;
		//if (_bDebugVersion)
		//{
		//	options = BuildOptions.AllowDebugging |
		//		BuildOptions.AutoRunPlayer |
		//		BuildOptions.Development |
		//		BuildOptions.ConnectWithProfiler |
		//		BuildOptions.ShowBuiltPlayer;
		//}

		if (development)
		{
			options |= BuildOptions.Development;
			if (autoConnectProfiler)
			{
				options |= BuildOptions.AllowDebugging | BuildOptions.AutoRunPlayer | BuildOptions.ConnectWithProfiler | BuildOptions.ShowBuiltPlayer;
			}
		}

		string output = "";
		switch (EditorUserBuildSettings.activeBuildTarget)
		{
			case BuildTarget.StandaloneWindows:
				if (string.IsNullOrEmpty(pathProject))
				{
					output = string.Format("{0}/{1}/{1}.exe", BuildSettings.outputDir, PlayerSettings.productName);
				}
				else
				{
					output = string.Format("{0}/{1}.exe", pathProject, PlayerSettings.productName);
				}
				break;
			case BuildTarget.Android:
				if (isApk)
				{
					EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

					if (string.IsNullOrEmpty(pathPackage))
					{
						output = string.Format("{0}/{1}_{2}_{3:yyyyMMdd_HHmmss}.apk", EditorHelper.packageDir, PlayerSettings.productName, ConstantData.mainVersion, DateTime.Now);
					}
					else
					{
						output = pathPackage;
					}
				}
				else
				{

					EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

					options |= BuildOptions.AcceptExternalModificationsToPlayer;
					output = string.Format("{0}/{1}", BuildSettings.outputDir, PlayerSettings.productName);
					SaveAndroidConfig(output);
				}
				break;
			case BuildTarget.iOS:
				options |= BuildOptions.AcceptExternalModificationsToPlayer;
				if (string.IsNullOrEmpty(pathProject))
				{
					output = BuildSettings.outputDir;
				}
				else
				{
					output = pathProject;
				}
				break;
		}

		BuildPipeline.BuildPlayer(scenes.ToArray(), output, EditorUserBuildSettings.activeBuildTarget, options);

		switch (EditorUserBuildSettings.activeBuildTarget)
		{
			case BuildTarget.StandaloneWindows:
				// 压缩成zip包
				string pathSrc = output.Substring(0, output.LastIndexOf('/'));
				string pathDst = pathPackage;
				if (string.IsNullOrEmpty(pathDst))
				{
					pathDst = string.Format("{0}/{1}_{2}_{3:yyyyMMdd_HHmmss}.zip", EditorHelper.packageDir, PlayerSettings.productName, ConstantData.mainVersion, DateTime.Now);
				}

				ZipHelper.Pack(pathSrc, pathDst);
				break;
		}
	}

	static void SaveAndroidConfig(string output)
	{
		// 版本号
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.AppendFormat("VERSION_NAME={0}\n", PlayerSettings.bundleVersion);
		sb.AppendFormat("VERSION_CODE={0}\n", PlayerSettings.Android.bundleVersionCode);
		sb.AppendFormat("BUNDLE_IDENTIFIER={0}\n", PlayerSettings.applicationIdentifier);

		string path = string.Format("{0}/{1}/version.properties", output, PlayerSettings.productName);
		IGG.FileUtil.SaveTextToFile(sb.ToString(), path);
	}

	private static void WritePCVersionSvnCmd(string AppName, string StrVersion, string VersionFlag)
	{

		string VersionFileName = AppName + "_" + StrVersion + "_" + VersionFlag + ".rar";
		IGG.FileUtil.CreateDirectory("C:/Shell"); 
		FileStream stream = new FileStream("C:/Shell/SvnPCVersion.bat", FileMode.Create);
		StreamWriter file = new StreamWriter(stream);
		file.WriteLine("@echo off");
		string str = "svn update \"D:\\Version\"";
		file.WriteLine(str);

		str = "copy /y c:\\zero\\PC\\" + VersionFileName;
		str += " D:\\Version\\" + StrVersion + "\\client\\windows\\";
		file.WriteLine(str);

		str = "svn add \"D:\\Version\\" + StrVersion + "\\client\\windows\\" + VersionFileName + "\"";
		file.WriteLine(str);

		str = "svn commit -m  \"Add pc version\" \"D:\\Version\\" + StrVersion + "\\client\\windows\\" + VersionFileName + "\"";
		file.WriteLine(str);

		file.Close();
		stream.Close();
		UnityEngine.Debug.Log("WritePCVersionSvnCmd");
	}

	private static void WriteAndroidShellCmd(string AppPath, string strVersionUpdate)
	{

		IGG.FileUtil.CreateDirectory("C:/Shell");
		FileStream stream = new FileStream("C:/Shell/Upload.sh", FileMode.Create);
		StreamWriter file = new StreamWriter(stream);
		file.WriteLine("#!/bin/sh");
		string str = "curl -F \"file=@" + AppPath + "\"" + " \\";
		file.WriteLine(str);
		str = " -F \"installType=2\"" + " \\";
		file.WriteLine(str);
		str = " -F \"password=igg\"" + " \\";
		file.WriteLine(str);
		str = " -F \"updateDescription=" + strVersionUpdate + "\"" + " \\";
		file.WriteLine(str);
		str = " -F \"uKey=e872c79cb97ae2f178899adbd6baede7\"" + " \\";
		file.WriteLine(str);
		str = " -F \"_api_key=0a75302daafd33c5f75f3fce2635284a\"";
		str += " https://www.pgyer.com/apiv1/app/upload";
		file.WriteLine(str);

		file.Close();
		stream.Close();
		UnityEngine.Debug.Log("ShellCmd Finish");
	}

	private static void WriteAndroidVersionSvnCmd(string AppName, string StrVersion, string VersionFlag)
	{

		string VersionFileName = AppName + "_" + StrVersion + "_" + VersionFlag + ".apk";
		IGG.FileUtil.CreateDirectory("C:/Shell");
		FileStream stream = new FileStream("C:/Shell/SvnAndroidVersion.bat", FileMode.Create);
		StreamWriter file = new StreamWriter(stream);
		file.WriteLine("@echo off");
		string str = "svn update \"D:\\Version\"";
		file.WriteLine(str);

		str = "copy /y c:\\zero\\Android\\" + VersionFileName;
		str += " D:\\Version\\" + StrVersion + "\\client\\android\\";
		file.WriteLine(str);

		str = "svn add \"D:\\Version\\" + StrVersion + "\\client\\android\\" + VersionFileName + "\"";
		file.WriteLine(str);

		str = "svn commit -m  \"Add android version\" \"D:\\Version\\" + StrVersion + "\\client\\android\\" + VersionFileName + "\"";
		file.WriteLine(str);

		file.Close();
		stream.Close();
		UnityEngine.Debug.Log("WriteAndroidVersionSvnCmd");
	}
}