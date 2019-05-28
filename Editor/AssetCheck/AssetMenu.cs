using IGG.Validation;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetMenu  {
    
    [MenuItem("Assets/规范检测/单个模型规范检测", true)]
	private static bool CanCheckSelectedObject()
	{
		string Path = "";
		if (CheckSelectionGameObject (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/规范检测/单个模型规范检测", false)]
	private static void CheckSelectedObject()
	{
		bool bAllOK = true;

		foreach (GameObject gameObj in Selection.gameObjects)
		{
			string errorMsg = ModelAssetValidation.PerformanceCheckModelPrefab(gameObj);
			bool bPassed = string.IsNullOrEmpty(errorMsg);

			if ( bPassed )
				Debug.Log( "Asset '" + gameObj.name + " passed.", gameObj );
			else
				Debug.LogError( errorMsg, gameObj );

			bAllOK = bAllOK && bPassed;
		}

		if (!bAllOK)
		{
			EditorUtility.DisplayDialog( "Performance Warning", "Some of the selected assets failed our Project Import and Performance settings.  Check the log window for details!", "Thanks!" );
			EditorApplication.ExecuteMenuItem( "Window/Console" );
		}
	}


	[MenuItem("Assets/规范检测/单个纹理规范检测", true)]
	private static bool CanCheckSelectedTexture()
	{
		string Path = "";
		if (CheckSelectionTexture (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/规范检测/单个纹理规范检测", false)]
	private static void CheckSelectedTexture()
	{
		var selectedTextures = Selection.objects.Where(x => x is Texture).Cast<Texture>();
		List<Texture>listTex = TextureAssetValidation.ValidateTextures( selectedTextures.ToArray());
		if ( listTex.Count > 0 )
		{
			EditorUtility.DisplayDialog( "Performance Warning", "Some of the selected assets exceed the performance guidelines set for the project.  Look in the Console Window for details.", "Thanks!" );
			EditorApplication.ExecuteMenuItem( "Window/Console" );
		}
		else
		{
			EditorUtility.DisplayDialog( "Passed", "All of the selected textures conform to their rules and are deemed suitable for shipping.", "Woo hoo!" );
		}
	}



	/*[MenuItem("Assets/单个shader规范检测", true)]
	private static bool CanCheckSelectedShader()
	{
		string Path = "";
		if (CheckSelectionShader (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/单个shader规范检测", false)]
	private static void CheckSelectedShader()
	{
		string Path = "";
		CheckSelectionShader (ref Path);
		Debug.Log(Path);
	}


	[MenuItem("Assets/单个脚本规范检测", true)]
	private static bool CanCheckSelectedScript()
	{
		string Path = "";
		if (CheckSelectionScript (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/单个脚本规范检测", false)]
	private static void CheckSelectedScript()
	{
		string Path = "";
		CheckSelectionScript (ref Path);
		Debug.Log(Path);
	}


	[MenuItem("Assets/单个材质规范检测", true)]
	private static bool CanCheckSelectedMaterial()
	{
		string Path = "";
		if (CheckSelectionMaterial (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/单个材质规范检测", false)]
	private static void CheckSelectedMaterial()
	{
		string Path = "";
		CheckSelectionMaterial (ref Path);
		Debug.Log(Path);
	}


	[MenuItem("Assets/单个动画规范检测", true)]
	private static bool CanCheckSelectedAnimation()
	{
		string Path = "";
		if (CheckSelectionAnimation (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/单个动画规范检测", false)]
	private static void CheckSelectedAnimation()
	{
		string Path = "";
		CheckSelectionAnimation (ref Path);
		Debug.Log(Path);
	}



	[MenuItem("Assets/单个声音规范检测", true)]
	private static bool CanCheckSelectedAudio()
	{
		string Path = "";
		if (CheckSelectionAudio (ref Path) == true) {
			return true;
		} else
			return false;
	}


	[MenuItem("Assets/单个声音规范检测", false)]
	private static void CheckSelectedAudio()
	{
		string Path = "";
		CheckSelectionAudio (ref Path);
		Debug.Log(Path);
	}*/


	[MenuItem("Assets/规范检测/本目录下模型规范检测", true)]
	private static bool CanCheckFileDirObject()
	{
		string Path = "";
		if (CheckSelectionFileDir (ref Path) == true) {
			List<string> listPath = new List<string>();
			var guid2 = AssetDatabase.FindAssets("t:gameObject",listPath.ToArray());
			if (guid2.Length > 0)
				return true;
		}
		return false;
	}


	[MenuItem("Assets/规范检测/本目录下模型规范检测", false)]
	private static void CheckFileDirObjectTest()
	{
		List<string> listPath = new List<string>();
		List<GameObject> listgo = new List<GameObject>();
		string Path = "";
		CheckSelectionFileDir (ref Path);
		listPath.Add(Path);
		var guid2 = AssetDatabase.FindAssets("t:gameObject",listPath.ToArray());
		if (guid2.Length == 0) {
			Debug.Log( "本目录不存在gameobject" );
			return;
		}

		foreach (var guid in guid2) {
			GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid),typeof(GameObject)) as GameObject;
			listgo.Add(go);
		}

		bool bAllOK = true;

		foreach (GameObject gameObj in listgo)
		{
			string errorMsg = ModelAssetValidation.PerformanceCheckModelPrefab(gameObj);
			bool bPassed = string.IsNullOrEmpty(errorMsg);

			if ( !bPassed )
                Debug.LogError(errorMsg, gameObj);

			bAllOK = bAllOK && bPassed;
		}

		if (!bAllOK)
		{
			EditorUtility.DisplayDialog( "Performance Warning", "Some of the selected assets failed our Project Import and Performance settings.  Check the log window for details!", "Thanks!" );
			EditorApplication.ExecuteMenuItem( "Window/Console" );
		}
	}


	[MenuItem("Assets/规范检测/本目录下纹理规范检测", true)]
	private static bool CanCheckFileDirTexture()
	{
		string Path = "";
		if (CheckSelectionFileDir (ref Path) == true) {
			List<string> listPath = new List<string>();
			var guid2 = AssetDatabase.FindAssets("t:texture2D",listPath.ToArray());
			if (guid2.Length > 0)
				return true;
		}
	    return false;
	}


	[MenuItem("Assets/规范检测/本目录下纹理规范检测", false)]
	private static void CheckFileDirTextureTest()
	{
		List<string> listPath = new List<string>();
		List<Texture> listText = new List<Texture>();
		string Path = "";
		CheckSelectionFileDir (ref Path);
		Debug.Log(Path);
		listPath.Add(Path);
		var guid2 = AssetDatabase.FindAssets("t:texture2D",listPath.ToArray());
		foreach (var guid in guid2) {
			Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
			Texture tex = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid),typeof(Texture)) as Texture;
			listText.Add(tex);
		}

		List<Texture>ltex = TextureAssetValidation.ValidateTextures( listText.ToArray());
		if ( ltex.Count > 0 )
		{
			EditorUtility.DisplayDialog( "Performance Warning", "Some of the selected assets exceed the performance guidelines set for the project.  Look in the Console Window for details.", "Thanks!" );
			EditorApplication.ExecuteMenuItem( "Window/Console" );
		}
		else
		{
			EditorUtility.DisplayDialog( "Passed", "All of the selected textures conform to their rules and are deemed suitable for shipping.", "Woo hoo!" );
		}
	}




	// 检测选择的是为文件夹
	public static bool  CheckSelectionFileDir(ref  string Path){
		if (Selection.assetGUIDs.Length == 1) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs [0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return true;
		}
		return false;
	}


	// 检测选择的是为单个对象/模型
	public static bool  CheckSelectionGameObject(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.gameObjects != null && Selection.gameObjects.Length == 1;
	}


	// 检测选择的是为单个纹理
	private static bool  CheckSelectionTexture(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);			
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is Texture ) && Selection.objects.Length == 1;
	}

	// 检测选择的是为单个材质
	private static bool  CheckSelectionMaterial(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is Material ) && Selection.objects.Length == 1;
	}


	// 检测选择的是为单个脚本
	private static bool  CheckSelectionScript(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is MonoScript ) && Selection.objects.Length == 1;
	}


	// 检测选择的是为单个shader
	private static bool  CheckSelectionShader(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is Shader ) && Selection.objects.Length == 1;
	}

	// 检测选择的是为单个Animation
	private static bool  CheckSelectionAnimation(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is AnimationClip ) && Selection.objects.Length == 1;
	}


	// 检测选择的是为单个audio
	private static bool  CheckSelectionAudio(ref  string Path){
		if (Selection.assetGUIDs.Length > 0) {
			Path = AssetDatabase.GUIDToAssetPath (Selection.assetGUIDs[0]);
			if (AssetDatabase.IsValidFolder (Path) == true)
				return false;
		}
		return Selection.objects.Any( x => x is AudioClip ) && Selection.objects.Length == 1;
	}

}
