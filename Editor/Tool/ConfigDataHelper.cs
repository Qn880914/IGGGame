using System.Collections;
using System.Collections.Generic;
using IGG.Game;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 配置文件帮助
/// </summary>
public class ConfigDataHelper
{
	// --------------------------------------------------------------------------------------------
	public static T GetData<T>(string path) where T : ScriptableObject
	{
		return AssetDatabase.LoadAssetAtPath<T>(path);
	}

	public delegate void SetDataHandler<T>(T data) where T : ScriptableObject;
	public static void SaveData<T>(string path, SetDataHandler<T> handler) where T : ScriptableObject
	{
		bool needCreate = false;

		T data = AssetDatabase.LoadAssetAtPath<T>(path);
		if (data == null)
		{
			data = ScriptableObject.CreateInstance<T>();
			needCreate = true;
		}

		if (handler != null)
		{
			handler(data);
		}

		if (needCreate)
		{
			AssetDatabase.CreateAsset(data, path);
		}
		else
		{
			EditorUtility.SetDirty(data);
			AssetDatabase.SaveAssets();
		}
		AssetDatabase.Refresh();
	}

	// --------------------------------------------------------------------------------------------
	// ConfigData
	// --------------------------------------------------------------------------------------------
	static string ms_path = "Assets/Data/config.asset";

	public static ConfigDataProxy GetConfigData()
	{
		return GetData<ConfigDataProxy>(ms_path);
	}

	static void SaveConfigData(SetDataHandler<ConfigDataProxy> handler)
	{
		SaveData(ms_path, handler);
	}

	// 应用名
	public static string GameName
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.gameName;
		}
	}

	// 版本发布类型
	public static ReleaseType Release
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.release;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.release = value;
			});
		}
	}

	// 编译ID
	public static uint BuildId
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.buildID;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.buildID = value;
			});
		}
	}

	// Svn版本-工程
	public static uint RevisionProject
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.revisionProject;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.revisionProject = value;
			});
		}
	}

	// Svn版本-Game
	public static uint RevisionGame
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.revisionGame;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.revisionGame = value;
			});
		}
	}

	// Svn版本-表格
	public static uint RevisionConfig
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.revisionConfig;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.revisionConfig = value;
			});
		}
	}

	public static bool EnablePatch
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.EnablePatch;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.EnablePatch = value;
			});
		}
	}

	public static bool ChinaMainland
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.ChinaMainland;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.ChinaMainland = value;
			});
		}
	}

	public static bool Censorship
	{
		get
		{
			ConfigDataProxy data = GetConfigData();
			return data.Censorship;
		}

		set
		{
			SaveConfigData((data) =>
			{
				data.Censorship = value;
			});
		}
	}
}