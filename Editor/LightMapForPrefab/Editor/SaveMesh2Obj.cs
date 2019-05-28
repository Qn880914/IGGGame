using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
using System.IO;

public class SaveMesh2Obj  {

	class LightmapStuct
	{
		public LightmapStuct(int idx, Vector4 coord)
		{
			lightmapIdx = idx;
			lightmapScaleOffset = coord;

		}

		public int lightmapIdx;
		public Vector4 lightmapScaleOffset;
	}













    static Dictionary<string, LightmapStuct> LightDatas = new Dictionary<string, LightmapStuct>();


	[MenuItem("LightMap/Mesh/Save Combined Mesh")]
	public static void SaveMesh()
	{
		var targets = Selection.gameObjects;
		LightDatas.Clear ();

		foreach(var target in targets)
		{
			var filters = target.GetComponentsInChildren<MeshFilter> ();
			int i = 0;
			foreach(var filter in filters)
			{
				
				//if (filter.name.StartsWith ("-mesh")) 
				//{

					using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.obj", Application.dataPath+"/Mesh/", target.name)))  
					{  
						streamWriter.Write(MeshToString(filter, new Vector3(-1f, 1f, 1f)));  
						streamWriter.Close();  
					}  
					AssetDatabase.Refresh(); 
					i++;


				//}

			}



			var renders = target.GetComponentsInChildren<MeshRenderer> ();
			foreach(var ren in renders)
			{
				string key = target.name + "/" + ren.name;
				if (LightDatas.ContainsKey (key)) 
				{
					Debug.LogError ("repeat render : "+key);
					continue;
				}
				LightDatas.Add (key,new LightmapStuct(ren.lightmapIndex,ren.lightmapScaleOffset));
			}



		}


		Debug.Log ("------------------------------ Exoport finish--------------------------------");

	}

	[MenuItem("LightMap/Mesh/Load LightMap data")]
	private static void LoadMesh()
	{

		var targets = Selection.gameObjects;

		foreach(var target in targets)
		{
			var renders = target.GetComponentsInChildren<MeshRenderer> ();
			foreach(var ren in renders)
			{
				string key = target.name + "/" + ren.name;
				if (LightDatas.ContainsKey (key)) {
					ren.lightmapIndex = LightDatas [key].lightmapIdx;
					ren.lightmapScaleOffset = LightDatas [key].lightmapScaleOffset;
				} else
					Debug.LogError ("no render name found in dic: "+ key);
			}


		}


//		Mesh mesh   = AssetDatabase.LoadAssetAtPath<Mesh>(string.Format("{0}{1}.obj", projectPath, this.meshGO.name));  
//		mf.mesh     = mesh;  
//
//		PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", projectPath, this.meshGO.name), this.meshGO);  
//		AssetDatabase.Refresh(); 
//

	}





	private static string MeshToString(MeshFilter mf, Vector3 scale)  
	{  
		Mesh          mesh            = mf.sharedMesh;  
		Material[]    sharedMaterials = mf.GetComponent<MeshRenderer>().sharedMaterials;  
		Vector2       textureOffset   = mf.GetComponent<MeshRenderer>().sharedMaterial.GetTextureOffset("_MainTex");  
		Vector2       textureScale    = mf.GetComponent<MeshRenderer>().sharedMaterial.GetTextureScale ("_MainTex");  

		StringBuilder stringBuilder   = new StringBuilder().Append("mtllib design.mtl")  
			.Append("\n")  
			.Append("g ")  
			.Append(mf.name)  
			.Append("\n");  

		Vector3[] vertices = mesh.vertices;  
		for (int i = 0; i < vertices.Length; i++)  
		{  
			Vector3 vector = vertices[i];  
			stringBuilder.Append(string.Format("v {0} {1} {2}\n", vector.x * scale.x, vector.y * scale.y, vector.z * scale.z));  
		}  

		stringBuilder.Append("\n");  




		Dictionary<int, int> dictionary = new Dictionary<int, int>();  

		if (mesh.subMeshCount > 1)  
		{  
			int[] triangles = mesh.GetTriangles(1);  

			for (int j = 0; j < triangles.Length; j += 3)  
			{  
				if (!dictionary.ContainsKey(triangles[j]))  
				{  
					dictionary.Add(triangles[j], 1);  
				}  

				if (!dictionary.ContainsKey(triangles[j + 1]))  
				{  
					dictionary.Add(triangles[j + 1], 1);  
				}  

				if (!dictionary.ContainsKey(triangles[j + 2]))  
				{  
					dictionary.Add(triangles[j + 2], 1);  
				}  
			}  
		}  

		for (int num = 0; num != mesh.uv.Length; num++)  
		{  
			Vector2 vector2 = Vector2.Scale(mesh.uv[num], textureScale) + textureOffset;  

			if (dictionary.ContainsKey(num))  
			{  
				stringBuilder.Append(string.Format("vt {0} {1}\n", mesh.uv[num].x, mesh.uv[num].y));  
			}  
			else  
			{  
				stringBuilder.Append(string.Format("vt {0} {1}\n", vector2.x, vector2.y));  
			}  
		}  

		for (int k = 0; k < mesh.subMeshCount; k++)  
		{  
			stringBuilder.Append("\n");  

			if (k == 0)  
			{  
				stringBuilder.Append("usemtl ").Append("Material_design").Append("\n");  
			}  

			if (k == 1)  
			{  
				stringBuilder.Append("usemtl ").Append("Material_logo").Append("\n");  
			}  

			int[] triangles2 = mesh.GetTriangles(k);  

			for (int l = 0; l < triangles2.Length; l += 3)  
			{  
				stringBuilder.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles2[l] + 1, triangles2[l + 2] + 1, triangles2[l + 1] + 1));  
			}  
		}  

		return stringBuilder.ToString();  
	}  







}
