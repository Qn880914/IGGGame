using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/************************************************************************/
/* Modified by : Aoicocoon  2017/12/25 
 * 1.增加烘培后的矩阵变换
 * 2.修改动作文件为纹理
/************************************************************************/

static class MeshAnimationExporter
{
	private static T[] ResizeArrayWithCopyExistingElements<T>(T[] pArray, int pNewSize)
	{
		T[] result = new T[pNewSize];
		for (int i = 0; i < pArray.Length && i <pNewSize; i++)
		{
			result[i] = pArray[i];
		}
		return result;
	}

	public class ExportParameters
	{
		public float framerate = 30f;
		public AnimationClip[] animationClips = new AnimationClip[0];
		public string[] animationNames = new string[0];
		public Transform[] boneTransforms = new Transform[0];
        public Quaternion quaternionOffset = Quaternion.identity;
		public string[] boneNames = new string[0];
		public string outputFolderPath = "";
		public string outputFilePath = "";
        public bool isLoadAllInMainScene = false;
	}

	public static bool SetAnimations(string[] pAnimationNames, AnimationClip[] pAnimationClips, ref ExportParameters pExportParameters)
	{
		if (pAnimationNames == null || pAnimationClips == null || pAnimationNames.Length != pAnimationClips.Length)
		{
			return false;
		}
		pExportParameters.animationNames = pAnimationNames;
		pExportParameters.animationClips = pAnimationClips;
		return true;
	}

	// multi word names are supposed to map like WarBird => war_bird
	private static string convertStringToFileFormat(string pInput)
	{
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		foreach (char c in pInput)
		{
			if (System.Char.IsUpper(c) && result.Length > 0)
			{
				result.Append('_');
			}
			result.Append(System.Char.ToLower(c));
		}
		return result.ToString();
	}

	private static readonly string MESH_ANIMATIONS_ABSOLUTE_FOLDER_PATH = Application.dataPath + "/" + ResourcesPath.TroopMeshAniPath;

	private static void PredictOutputPath(GameObject pObjectRoot, string prefabPath, ref ExportParameters pSettings)
	{
		string objectFileName = convertStringToFileFormat(pObjectRoot.name);
		string subFolderNameBossOrSoldier = Directory.GetParent(Application.dataPath + prefabPath).Name;

        //string folderPath = Path.Combine(MESH_ANIMATIONS_ABSOLUTE_FOLDER_PATH, subFolderNameBossOrSoldier);
        //modified by @horatio
        string folderPath = MESH_ANIMATIONS_ABSOLUTE_FOLDER_PATH;

        // is there some way to get the prefix number so that we can generate the filename without an existing version?
        string[] possibleMatch = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
			.Where(filepath => Path.GetFileNameWithoutExtension(filepath.ToLower())
			       .Contains(objectFileName) && !filepath.ToLower().Contains(".meta")).ToArray();

		if (possibleMatch.Length == 1)
		{
			pSettings.outputFilePath = possibleMatch[0];
			pSettings.outputFolderPath = Path.GetDirectoryName(pSettings.outputFilePath);
		}
		else
		{
			pSettings.outputFilePath = "";
			pSettings.outputFolderPath = folderPath;
		}
	}

	private static void PredictEmitterAnchors(GameObject pObjectRoot, ref ExportParameters pSettings)
	{		
		Transform[] transforms = pObjectRoot.GetComponentsInChildren<Transform>(); 
		// the transforms in the object tree not in the prefab are the emitter anchors
		var emitterAnchorsQuery = transforms.Where(transform => PrefabUtility.GetPrefabParent(transform.gameObject) == null);  
		Transform[] emitterAnchors = emitterAnchorsQuery.ToArray();
		string[] emitterAnchorNames = emitterAnchorsQuery.Select(transform => transform.gameObject.name).ToArray();
		int anchorCount = emitterAnchors.Length;
		
		ResizeDataLists(ref pSettings.boneTransforms, ref pSettings.boneNames, pDesiredCapacity: anchorCount);
		
		// fill data
		pSettings.boneTransforms = emitterAnchors;
		pSettings.boneNames = emitterAnchorNames;
	}

	public static void PredictSettings(GameObject pObjectRoot, ref ExportParameters pSettings)
	{
		string prefabPath = pObjectRoot == null? "" : AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(pObjectRoot));
		if (prefabPath.Length == 0)
		{
			Debug.LogWarning("Please use this tool with an instance of the prefab");
			return;
			// they drag/dropped the prefab itself. that's not supposed to be how you use this.
		}
		if (PrefabUtility.GetPrefabParent(pObjectRoot.transform.parent) != null)
		{
			Debug.LogWarning("Export settings prediction only works when given the root of the prefab instance");
			return;
		}

		PredictOutputPath(pObjectRoot, prefabPath, ref pSettings);
		PredictEmitterAnchors(pObjectRoot, ref pSettings);
	}

	public static void ReadAnimationSettingsFromFbx(GameObject pFbxInstance, ref ExportParameters pSettings)
	{
		GameObject go = GameObject.Instantiate(pFbxInstance) as GameObject;
		Animation animation = go.GetComponentInChildren<Animation>();
		if (!animation)
		{
            UnityEngine.Debug.LogError("Target game object has no Animation component!" + "ReadAnimationSettingsFromFbx");
			return;
		}
		int clipCount = animation.GetClipCount();
		
		int index = 0;
		var animationClips = new AnimationClip[clipCount];
		var animationNames = new string[clipCount];

        var cli = animation.GetClip("idle");
        foreach (AnimationState state in animation)
		{
			animationClips[index] = state.clip;
			animationNames[index] = state.clip.name.Capitalize();
			index++;
		}
		GameObject.DestroyImmediate(go);

		pSettings.animationClips = animationClips;
		pSettings.animationNames = animationNames;
	}

	public static ExportParameters GenerateDefaultSettings(GameObject pFbxInstance, string defaultOutputFolder)
	{
		ExportParameters settings = new ExportParameters();
		ReadAnimationSettingsFromFbx(pFbxInstance, ref settings);
		PredictSettings(pFbxInstance, ref settings);

		//默认路径
		string defultPath = defaultOutputFolder + ((null != pFbxInstance) ? (pFbxInstance.name + "/") : "");

	    defultPath = defaultOutputFolder;

		settings.outputFilePath = defultPath;
		settings.outputFolderPath = Path.GetDirectoryName(defultPath);
		return settings;
	}

	public static void Export(GameObject pFbxInstance, ExportParameters pSettings, System.Action<float> pProgressBarUpdater= null)
	{
		if (AreParametersValid(pFbxInstance, pSettings))
		{
			//List<AnimationClip> exportClips = GetClips ();
			//List<Transform> exportBoneTransforms = GetBoneTransforms ();
			//List<string> exportBoneNames = GetBoneNames ();
			AnimationClip[] exportClips = pSettings.animationClips;
			Transform[] exportBoneTransforms = pSettings.boneTransforms;
			string[] exportBoneNames = pSettings.boneNames;
			string[] animationNames = pSettings.animationNames;
			
			GameObject model = pFbxInstance;
			Transform baseTransform = model.transform;
			SkinnedMeshRenderer renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
			Mesh mesh = renderer.sharedMesh;
			Animation animation = model.GetComponentInChildren<Animation>();

            // Set up the basic MeshAnimationGroupSerializable properties
            //MeshAnimationGroupSerializable group = new MeshAnimationGroupSerializable();
			//group.SetUV(mesh.uv);
			//group.SetTriangles(mesh.triangles);
			//group.SetFrameInterval(pSettings.framerate);
			//group.modelName = pFbxInstance.name;
			//group.boneCount = exportBoneTransforms.Length;
			
			float frameInterval = 1.0f / pSettings.framerate;

            //Mesh modifiedMesh = GameObject.Instantiate(mesh);
            //List<Vector4> modifiedTangent = new List<Vector4>();
            //for (int mi = 0; mi < mesh.vertexCount; mi++) {
            //    Vector4 tangent = new Vector4(mi, 0, 0);
            //    modifiedTangent.Add(tangent);
            //}

            //modifiedMesh.SetTangents(modifiedTangent);

            //AssetDatabase.CreateAsset(modifiedMesh, "Assets/GpuInstance/ooxx.asset");

            //清空并重建目标目录
            if (!System.IO.Directory.Exists(pSettings.outputFilePath)) {
                System.IO.Directory.CreateDirectory(pSettings.outputFilePath);
            } else {
                System.IO.Directory.Delete(pSettings.outputFilePath, true);
                System.IO.Directory.CreateDirectory(pSettings.outputFilePath);
            }

            for (int i = 0; i < exportClips.Length; i++)
			{
				if (pProgressBarUpdater != null)
				{
					pProgressBarUpdater((float)i / (float)(exportClips.Length + 1));
				}
				MeshAnimationBoneGroup boneGroup = new MeshAnimationBoneGroup(exportBoneNames.ToList(), exportBoneNames.Length);
				
				// Set the animation clip to export
				AnimationClip clip = exportClips[i];

                if(null == clip) {
                    continue;
                }

				animation.AddClip(clip, clip.name);
				animation.clip = clip;
				AnimationState state = animation[clip.name];
				state.enabled = true;
				state.weight = 1;
				
				float clipLength = clip.length;
				
				//Mesh frameMesh;
				//List<Vector3[]> frameVertices = new List<Vector3[]>();
				
				// Get the list of times for each frame
				List<float> frameTimes = GetFrameTimes(clipLength, frameInterval);

                ////顶点纹理 r->x,g->y,b->z,a->s
                Texture2D vertexTex = new Texture2D(mesh.vertexCount, frameTimes.Count, TextureFormat.RGBAHalf, false);
                
                int frame = 0;

                foreach (float time in frameTimes)
				{
					state.time = time;
					
					animation.Play();
					animation.Sample();
					
                    // Grab the position and rotation for each bone at the current frame
                    for (int k = 0; k < exportBoneTransforms.Length; k++)
					{
						string name = exportBoneNames[k];
						
						Vector3 pos = baseTransform.InverseTransformPoint(exportBoneTransforms[k].position);
						Quaternion rot = exportBoneTransforms[k].rotation * Quaternion.Inverse(baseTransform.rotation);
						
						boneGroup.Bones[name].Positions.Add(pos);
						boneGroup.Bones[name].Rotations.Add(rot);
                    }

                    Mesh bakeMesh = null;

                    //if (pSettings.quaternionOffset != Quaternion.identity) {
                    //    Matrix4x4 matrix = new Matrix4x4();
                    //    matrix.SetTRS(Vector2.zero, pSettings.quaternionOffset, Vector3.one);
                    //    bakeMesh = BakeFrameAfterMatrixTransform(renderer, matrix);
                    //} else {
                    //    bakeMesh = new Mesh();
                    //    renderer.BakeMesh(bakeMesh);
                    //}
                    Matrix4x4 matrix = new Matrix4x4();
                    //matrix.SetTRS(Vector2.zero, pSettings.quaternionOffset, Vector3.one);
                    matrix = renderer.transform.localToWorldMatrix;
                    bakeMesh = BakeFrameAfterMatrixTransform(renderer, matrix);

                    for (int k = 0; k < bakeMesh.vertexCount; k++) {
                        Vector3 vertex = bakeMesh.vertices[k];
                        vertexTex.SetPixel(k, frame, new Color(vertex.x, vertex.y, vertex.z));
                    }

                    bakeMesh.Clear();
                    Object.DestroyImmediate(bakeMesh);

                    frame++;

                    animation.Stop();
				}
				//group.AddAnimation(animationNames[i], frameVertices, boneGroup);

                vertexTex.Apply(false);

                string dataPath = pSettings.outputFilePath + "/" + pSettings.animationNames[i] + ".asset";

                AssetDatabase.CreateAsset(vertexTex, dataPath);
            }

            ////配置纹理 fps uv count, uv, tri count tri
            int countData = mesh.uv.Length / 2 + mesh.triangles.Length / 3 + 1;
            int texSize = 64; //不要问我为什么是64
            int texH = (int)Mathf.Ceil(countData / (float)texSize);
            int texW = texSize;

            Texture2D cfgTexture = new Texture2D(texW, texH, TextureFormat.RGBAHalf, false);
            Color[] colors = new Color[texW * texH];

            int cfgDataIdx = 0;
            colors[cfgDataIdx++] = new Color(pSettings.framerate, mesh.uv.Length, mesh.triangles.Length);
            for (int i = 0; i < mesh.uv.Length / 2; i++)
            {
                int uvIdx = i * 2;
                colors[cfgDataIdx++] = new Color(mesh.uv[uvIdx].x, mesh.uv[uvIdx].y, mesh.uv[uvIdx + 1].x, mesh.uv[uvIdx + 1].y);
            }
            for (int i = 0; i < mesh.triangles.Length / 3; i++)
            {
                int triIdx = i * 3;
                colors[cfgDataIdx++] = new Color(mesh.triangles[triIdx], mesh.triangles[triIdx + 1], mesh.triangles[triIdx + 2]);
            }

            cfgTexture.SetPixels(colors);
            cfgTexture.Apply();

            //Test(cfgTexture);

            AssetDatabase.CreateAsset(cfgTexture, pSettings.outputFilePath + "/" + pFbxInstance.name + ".asset");

            if (pProgressBarUpdater != null)
			{
				pProgressBarUpdater((float)exportClips.Length / (float)(exportClips.Length + 1));
			}

			//MeshAnimationProtobufHelper.SerializeObject<MeshAnimationGroupSerializable>(pSettings.outputFilePath + "/" + pFbxInstance.name + ".bytes", group);

			EditorUtility.DisplayDialog("Tip", "Mesh Animation Export Complete" + pFbxInstance.name, "OK");
			AssetDatabase.Refresh();
		}
	}

    public static void ExportCombinedTexture(GameObject pFbxInstance, ExportParameters pSettings, System.Action<float> pProgressBarUpdater = null)
    {
        if (!AreParametersValid(pFbxInstance, pSettings))
        {
            return;
        }

        //List<AnimationClip> exportClips = GetClips ();
        //List<Transform> exportBoneTransforms = GetBoneTransforms ();
        //List<string> exportBoneNames = GetBoneNames ();
        AnimationClip[] exportClips = pSettings.animationClips;
        Transform[] exportBoneTransforms = pSettings.boneTransforms;
        string[] exportBoneNames = pSettings.boneNames;
        string[] animationNames = pSettings.animationNames;

        GameObject model = pFbxInstance;
        Transform baseTransform = model.transform;
        Animation animation = model.GetComponentInChildren<Animation>();

        ///origin code
        //SkinnedMeshRenderer renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        //Mesh mesh = renderer.sharedMesh;

        SkinnedMeshRenderer[]  renderArray = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        int arrayLength = renderArray.Length;
        Mesh[] meshArray = new Mesh[arrayLength];
        for (int i=0; i< arrayLength;i++)
        {
            meshArray[i] = renderArray[i].sharedMesh;
        }

        float frameInterval = 1.0f / pSettings.framerate;

        //清空并重建目标目录
        if (!System.IO.Directory.Exists(pSettings.outputFilePath))
        {
            System.IO.Directory.CreateDirectory(pSettings.outputFilePath);
        }

        string filePath = pSettings.outputFilePath + model.name+".asset";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        ///modify by niexin
        ///Attack1","Win","Dead","Hit","Run","Skill1","Wait1","Wait2" 8 default animations
        ///first line marked as attached model info, first color to number,following by vertex counts for each sub mesh,which is the width of texture
        ///某行首位标记attach数量信息，如0则无，那2行则为该物体动画数据，否则往上推，后续像素r分别写入顶点数，即动画纹理宽度
        ///8个动画的顺序依次标记首位8个像素，r通道填入是否存在的信息，g,b分别对应初始行，结束行位置，

        int totalframeCount = 0;
           
        totalframeCount = 1 + arrayLength;

        int aniFrameCount = 0;

        //pre cacl the length of all frames
        for (int i = 0; i < exportClips.Length; i++)
        {
            AnimationClip clip = exportClips[i];

            if (null == clip)
            {
                continue;
            }

            float clipLength = clip.length;

            //Mesh frameMesh;
            //List<Vector3[]> frameVertices = new List<Vector3[]>();

            // Get the list of times for each frame
            List<float> frameTimes = GetFrameTimes(clipLength, frameInterval);
            aniFrameCount += frameTimes.Count;
        }

        totalframeCount += aniFrameCount * arrayLength;

        int[] vertexCount = new int[arrayLength];
        int maxVertexCount = 0;
        for (int i= 0; i< arrayLength;i++ )
        {
            int count = meshArray[i].vertexCount;
            vertexCount[i] = count;
            if (count > maxVertexCount)
                maxVertexCount = count;
        }

        Vector3[][] defaultAnimationInfos = new Vector3[arrayLength][];
        for (int i=0; i< arrayLength;i++)
        {
            defaultAnimationInfos[i] = new Vector3[8];
            for (int j=0; j< 8;j++)
            {
                defaultAnimationInfos[i][j] = new Vector3(0,0,0);
            }
        }

        string[] defaultAnimationArray = new string[] { "Attack1", "Win", "Dead", "Hit", "Run", "Skill1", "Wait1", "Wait2" };
        List<string> defaultAnimationList = new List<string>();
        for (int i = 0; i< defaultAnimationArray.Length;i++)
        {
            defaultAnimationList.Add(defaultAnimationArray[i].ToUpper());
        }

		MeshSkinDataProxy data = MeshSkinDataProxy.CreateInstance<MeshSkinDataProxy>();
        data.isLoadAllInMainScene = pSettings.isLoadAllInMainScene;
        data.arrayLength = arrayLength;
		data.maxVertexCount = maxVertexCount;
		data.vertexCounts = vertexCount;
		data.meshes = new MeshSkinDataProxy.SubMeshData[arrayLength];

		Texture2D combinedTex = new Texture2D(maxVertexCount, totalframeCount, TextureFormat.RGBAHalf, false);

        combinedTex.SetPixel(0, 0, new Color(arrayLength, maxVertexCount, 0));

        int countData = 1 + arrayLength;
           
        for (int i=0; i< arrayLength;i++)
        {
            combinedTex.SetPixel(i+1, 0, new Color(vertexCount[i], 0, 0));
            countData += meshArray[i].uv.Length / 2 + meshArray[i].triangles.Length / 3 ;
        }
   
        //////配置纹理 fps uv count, uv, tri count tri
 
        int texSize = 64; //不要问我为什么是64
        int texH = (int)Mathf.Ceil(countData / (float)texSize);
        int texW = texSize;

        Texture2D cfgTexture = new Texture2D(texW, texH, TextureFormat.RGBAHalf, false);
        Color[] colors = new Color[texW * texH];

        int cfgDataIdx = 0;
        colors[cfgDataIdx++] = new Color(arrayLength,0,0);

        for (int i=0;i<arrayLength;i++)
        {
            colors[cfgDataIdx++] = new Color(pSettings.framerate, meshArray[i].uv.Length, meshArray[i].triangles.Length);
        }

        int frame = 1 + arrayLength;

        for (int submeshCount = 0; submeshCount < arrayLength; submeshCount++)
        {
			data.meshes[submeshCount].clips = new MeshSkinDataProxy.ClipData[defaultAnimationArray.Length];
			data.meshes[submeshCount].framerate = pSettings.framerate;

			for (int i = 0; i < exportClips.Length; i++)
            {
				if (pProgressBarUpdater != null)
                {
                    pProgressBarUpdater((float)i / (float)(exportClips.Length + 1));
                }
                MeshAnimationBoneGroup boneGroup = new MeshAnimationBoneGroup(exportBoneNames.ToList(), exportBoneNames.Length);

                // Set the animation clip to export
                AnimationClip clip = exportClips[i];

                if (null == clip)
                {
                    continue;
                }

                int infoIdx = -1;

                string clipName = clip.name;
                if (clipName.Equals("walk"))
                    clipName = "Hit";
                else if (clipName.Equals("cut"))
                    clipName = "Skill1";
                else if (clipName.Equals("victory"))
                    clipName = "Dead";

                string upperName = clipName.ToUpper();
                if (defaultAnimationList.Contains(upperName))
                {
                    infoIdx = defaultAnimationList.IndexOf(upperName);
                    defaultAnimationInfos[submeshCount][infoIdx].x = 1;
                }

                animation.AddClip(clip, clip.name);
                animation.clip = clip;
                AnimationState state = animation[clip.name];
                state.enabled = true;
                state.weight = 1;

                float clipLength = clip.length;

                //Mesh frameMesh;
                //List<Vector3[]> frameVertices = new List<Vector3[]>();

                // Get the list of times for each frame
                List<float> frameTimes = GetFrameTimes(clipLength, frameInterval);

                ////顶点纹理 r->x,g->y,b->z,a->s

                //sample each sub mesh

                //start frame position
                try
                {
                    defaultAnimationInfos[submeshCount][infoIdx].y = frame;
                }
                catch 
                {
                    Debug.LogError(pFbxInstance.name+","+defaultAnimationInfos.Length+","+defaultAnimationInfos.LongLength +","+ submeshCount+","+infoIdx);
					continue;
				}

				data.meshes[submeshCount].clips[infoIdx].frames = new MeshSkinDataProxy.FrameData[frameTimes.Count];

				int index = 0;
				foreach (float time in frameTimes)
                {
                    state.time = time;

                    animation.Play();
                    animation.Sample();

                    // Grab the position and rotation for each bone at the current frame
                    for (int k = 0; k < exportBoneTransforms.Length; k++)
                    {
                        string name = exportBoneNames[k];

                        Vector3 pos = baseTransform.InverseTransformPoint(exportBoneTransforms[k].position);
                        Quaternion rot = exportBoneTransforms[k].rotation * Quaternion.Inverse(baseTransform.rotation);

                        boneGroup.Bones[name].Positions.Add(pos);
                        boneGroup.Bones[name].Rotations.Add(rot);
                    }

                    Mesh bakeMesh = null;

                    //if (pSettings.quaternionOffset != Quaternion.identity)
                    //{
                    //    Matrix4x4 matrix = new Matrix4x4();
                    //    matrix.SetTRS(Vector2.zero, pSettings.quaternionOffset, Vector3.one);
                    //    bakeMesh = BakeFrameAfterMatrixTransform(renderArray[submeshCount], matrix);
                    //}
                    //else
                    //{
                    //    bakeMesh = new Mesh();
                    //    renderArray[submeshCount].BakeMesh(bakeMesh);
                    //}
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix = renderArray[submeshCount].transform.localToWorldMatrix;
                    bakeMesh = BakeFrameAfterMatrixTransform(renderArray[submeshCount], matrix);

                    data.meshes[submeshCount].clips[infoIdx].frames[index++].vertexs = bakeMesh.vertices;

					for (int k = 0; k < bakeMesh.vertexCount; k++)
                    {
                        Vector3 vertex = bakeMesh.vertices[k];
                        combinedTex.SetPixel(k, frame, new Color(vertex.x, vertex.y, vertex.z));
                    }

					bakeMesh.Clear();
                    Object.DestroyImmediate(bakeMesh);

                    frame++;

                    animation.Stop();
                }
                //end frame position,exclude
                defaultAnimationInfos[submeshCount][infoIdx].z = frame;
            }

			data.meshes[submeshCount].uvs = meshArray[submeshCount].uv;
			data.meshes[submeshCount].triangles = meshArray[submeshCount].triangles;

			for (int i = 0; i < meshArray[submeshCount].uv.Length / 2; i++)
            {
                int uvIdx = i * 2;
                colors[cfgDataIdx++] = new Color(meshArray[submeshCount].uv[uvIdx].x, meshArray[submeshCount].uv[uvIdx].y, meshArray[submeshCount].uv[uvIdx + 1].x, meshArray[submeshCount].uv[uvIdx + 1].y);
            }
            for (int i = 0; i < meshArray[submeshCount].triangles.Length / 3; i++)
            {
                int triIdx = i * 3;
                colors[cfgDataIdx++] = new Color(meshArray[submeshCount].triangles[triIdx], meshArray[submeshCount].triangles[triIdx + 1], meshArray[submeshCount].triangles[triIdx + 2]);
            }
        }
        //group.AddAnimation(animationNames[i], frameVertices, boneGroup);

        for (int j=0; j<arrayLength;j++)
        {
            for (int i = 0; i < 8; i++)
            {
                combinedTex.SetPixel(i, j+1, new Color(defaultAnimationInfos[j][i].x, defaultAnimationInfos[j][i].y, defaultAnimationInfos[j][i].z));
            }
        }

        combinedTex.Apply(false);

		string dataPath = string.Format("{0}{1}.asset", pSettings.outputFilePath, pFbxInstance.name);
		AssetDatabase.CreateAsset(data, dataPath);

        if (pProgressBarUpdater != null)
        {
            pProgressBarUpdater((float)exportClips.Length / (float)(exportClips.Length + 1));
        }

        AssetDatabase.Refresh();
    }

    //测试解析
    private static void Test(Texture2D tex) {

        Color[] data = tex.GetPixels();
        Color cfgLength = data[0];

        int Fps = (int)cfgLength[0];
        float uvLength = cfgLength[1];
        float triLength = cfgLength[2];

        int cfgDataIdx = 1;
        Vector2[] UV = new Vector2[(int)uvLength];
        for (int i = 0; i < UV.Length / 2; i++) {
            cfgDataIdx++;
            int uvIdx = i * 2;
            Color singleUv = data[1 + i];
            UV[uvIdx].Set(singleUv.r, singleUv.g);
            UV[uvIdx + 1].Set(singleUv.b, singleUv.a);
        }

        int[] Triangles = new int[(int)triLength];
        for (int i = 0; i < Triangles.Length / 3; i++) {
            int triIdx = i * 3;
            Color singleTri = data[cfgDataIdx++];
            Triangles[triIdx++] = (int)singleTri.r;
            Triangles[triIdx++] = (int)singleTri.g;
            Triangles[triIdx++] = (int)singleTri.b;
        }

        Debug.Log("Done");
    }

    private static bool AreParametersValid(GameObject pFbx, ExportParameters pSettings)
	{
		if (string.IsNullOrEmpty(pSettings.outputFilePath.Trim()))
		{
			EditorUtility.DisplayDialog("Missing Output File", "Please set a output file.", "OK");
			return false;
		}
		
		if (pFbx == null)
		{
			EditorUtility.DisplayDialog("Missing Base FBX", "Please specify a base FBX.", "OK");
			return false;
		}
		
		bool clipsNotSet = true;
		
		for (int i = 0; i < pSettings.animationClips.Length; i++)
		{
			if (pSettings.animationClips[i] != null && string.IsNullOrEmpty(pSettings.animationNames[i].Trim()))
			{
				EditorUtility.DisplayDialog("Missing Animation Name", "Please specify a name for all animation files.", "OK");
				return false;
			}
			
			if (pSettings.animationClips[i] != null)
			{
				clipsNotSet = false;
			}
		}
		
		if (clipsNotSet)
		{
			EditorUtility.DisplayDialog("Missing Animation", "Please specify at least one animation file.", "OK");
			return false;
		}
		
		for (int i = 0; i < pSettings.boneTransforms.Length; i++)
		{
			if (pSettings.boneTransforms[i] != null && string.IsNullOrEmpty(pSettings.boneNames[i].Trim()))
			{
				EditorUtility.DisplayDialog("Missing Bone Name", "Please specify a name for all bone transforms.", "OK");
				return false;
			}
		}
		
		return true;
	}
	/// <summary>
	/// Calculate the time periods when a frame snapshot should be taken
	/// </summary>
	/// <returns>The frame times.</returns>
	/// <param name="pLength">P length.</param>
	/// <param name="pInterval">P interval.</param>
	public static List<float> GetFrameTimes(float pLength, float pInterval)
	{
		List<float> times = new List<float>();
		
		float time = 0;
		
		do
		{
			times.Add(time);
			time += pInterval;
		} while (time < pLength - pInterval);
		
		times.Add(pLength);
		
		return times;
	}

	/// <summary>
	/// Take a snapshot of the frame as a mesh and transform the vertices to from local
	/// to world.
	/// </summary>
	/// <returns>Mesh snapshot</returns>
	/// <param name="pRenderer">SkinnedMeshRenderer used to render the current frame</param>
	public static Mesh BakeFrame(SkinnedMeshRenderer pRenderer)
	{
		Mesh result = new Mesh();
		pRenderer.BakeMesh(result);
		result.vertices = TransformVertices(pRenderer.transform.localToWorldMatrix, result.vertices);
		return result;
	}

    public static Mesh BakeFrameAfterMatrixTransform(SkinnedMeshRenderer pRenderer, Matrix4x4 matrix) {
        Mesh result = new Mesh();
        pRenderer.BakeMesh(result);
        result.vertices = TransformVertices(matrix, result.vertices);
        return result;
    }
	
	/// <summary>
	/// Convert a set of vertices using the given transform matrix.
	/// </summary>
	/// <returns>Transformed vertices</returns>
	/// <param name="pLocalToWorld">Transform Matrix</param>
	/// <param name="pVertices">Vertices to transform</param>
	public static Vector3[] TransformVertices(Matrix4x4 pLocalToWorld, Vector3[] pVertices)
	{
		Vector3[] result = new Vector3[pVertices.Length];
		
		for (int i = 0; i < pVertices.Length; i++)
		{
			result[i] = pLocalToWorld.MultiplyPoint3x4( pVertices[i]);
        }
		
		return result;
	}

	public static void ResizeDataLists<DataType>(ref DataType[] pDataArray, ref string[] pNames, int pDesiredCapacity)
	{
		pDesiredCapacity = Mathf.Clamp(pDesiredCapacity, 0, 100);
		int previousNumber = pNames.Length;
		pNames = ResizeArrayWithCopyExistingElements(pNames, pDesiredCapacity);
		pDataArray = ResizeArrayWithCopyExistingElements(pDataArray, pDesiredCapacity);

		for (int i = previousNumber; i < pNames.Length; i++)
		{
			pNames[i] = "";
		}
	}
}

public class MeshAnimationExportToolEditorWindow : EditorWindow
{
	//private MeshAnimationExporter exporter;
	MeshAnimationExporter.ExportParameters exportSettings;
	private GameObject fbx;
	private int guiNumberOfClips;
	private int guiNumberOfBones;
	private Vector2 mainScrollPosition = Vector2.zero;
	private string outputFolderPath ;
	private string outputFilePath = "";
	private GUILayoutOption labelWidth;
	private float windowWidth;

	[MenuItem ("辅助工具/资源管理/Export Mesh Animation")]
	static void CreateWizard()
	{
		MeshAnimationExportToolEditorWindow window = (MeshAnimationExportToolEditorWindow)EditorWindow.GetWindow(typeof(MeshAnimationExportToolEditorWindow));
		window.ResetSettings();
		window.ShowUtility();

	}

	MeshAnimationExportToolEditorWindow()
	{
		ResetSettings();
	}

	private void ResetSettings()
	{
		exportSettings = new MeshAnimationExporter.ExportParameters();
	}


	private  string GetProjectPath()
	{
		return Application.dataPath.Substring (0, Application.dataPath.LastIndexOf ("/"));
	}
	/// <summary>
	/// Get the relative project file path from an absolute path
	/// </summary>
	/// <returns>The absolute file path</returns>
	/// <param name="pAbsolutePath">Absolute file path</param>
	public  string GetAssetPath (string pAbsolutePath)
	{
		string projectPath = GetProjectPath();
		if(pAbsolutePath.StartsWith(projectPath))
		{
			string relativePath = pAbsolutePath.Substring(projectPath.Length, pAbsolutePath.Length - projectPath.Length);

			if(relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
			{
				relativePath = relativePath.Substring(1, relativePath.Length - 1);
			}

			return relativePath;
		}

		return null;
	}

    protected static string DEFAULT_OUTPUT_FOLDER = "Assets/"+ResourcesPath.TroopMeshAniPath;
    public static Vector3 eulerAngle = new Vector3(-90, 0, 0);

    void OnGUI()
	{
        if(outputFilePath == null)
            outputFolderPath = Application.streamingAssetsPath;

        if (exportSettings == null)
		{
			exportSettings = new MeshAnimationExporter.ExportParameters();
		}
		windowWidth = position.width;	

		mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);

        //烘培定义角度转向
        eulerAngle = EditorGUILayout.Vector3Field("Bake model after apply Rotation Offset XYZ(除了特殊需求，一般按默认值导出)", eulerAngle);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Default Folder:" + exportSettings.outputFolderPath);

        EditorGUILayout.Space();

        #region Output File Selection
        EditorGUILayout.LabelField("Output File:");
		EditorGUILayout.BeginHorizontal();
		{
			string projectRelativeFilePath = GetAssetPath(outputFilePath);

            EditorGUILayout.SelectableLabel(projectRelativeFilePath,
			                                 EditorStyles.textField, 
			                                 GUILayout.Height(EditorGUIUtility.singleLineHeight));

			GUI.SetNextControlName("BrowseButton");
			if (GUILayout.Button("Browse"))
			{
				BrowseSaveFile();
			}
		}
		EditorGUILayout.EndHorizontal();
		#endregion

		EditorGUILayout.Space();

		#region Base FBX File Setting
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Base FBX:", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			GameObject newFbx = EditorGUILayout.ObjectField(fbx, typeof(GameObject), true) as GameObject;

			if (newFbx != null && newFbx != fbx)
			{
				// error if they drag the prefab itself, since it won't have any transform data
				if (PrefabUtility.GetPrefabParent(newFbx) != null)
				{
					fbx = newFbx;
					exportSettings = MeshAnimationExporter.GenerateDefaultSettings(newFbx, DEFAULT_OUTPUT_FOLDER);
					outputFolderPath = exportSettings.outputFolderPath;
					outputFilePath = exportSettings.outputFilePath;
					guiNumberOfClips = exportSettings.animationClips.Length;
					guiNumberOfBones = exportSettings.boneTransforms.Length;

				}
			}
		}

		EditorGUILayout.EndHorizontal();
		#endregion

		EditorGUILayout.Space();

		#region Framerate Setting
		exportSettings.framerate = EditorGUILayout.FloatField("Capture Framerate:", exportSettings.framerate);
		exportSettings.framerate = Mathf.Max(exportSettings.framerate, 1.0f);
        #endregion

        EditorGUILayout.Space();
        exportSettings.isLoadAllInMainScene = EditorGUILayout.Toggle("是否在主场景中加载全部动画", exportSettings.isLoadAllInMainScene);

		EditorGUILayout.Space();

		#region Clip Count Setting
		guiNumberOfClips = EditorGUILayout.IntField("Number of Clips:", guiNumberOfClips);
		ApplyChanges();
		#endregion

		EditorGUILayout.Space();

		labelWidth = GUILayout.Width(windowWidth * 0.3f);

		#region Animation Clip Setting
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Animation Name:", labelWidth);
			EditorGUILayout.LabelField("Animation File:", labelWidth);
			EditorGUILayout.LabelField("Frames:", GUILayout.Width(windowWidth * 0.2f));
		}
		EditorGUILayout.EndHorizontal();

		DrawAnimationArrayGui();
		#endregion

		EditorGUILayout.Space();

		#region Bone Count Setting
		guiNumberOfBones = EditorGUILayout.IntField("Number of Bones:", guiNumberOfBones);
		ApplyChanges();
		#endregion

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Bone Name:", labelWidth);
			EditorGUILayout.LabelField("Bone Transform:", labelWidth);
		}
		EditorGUILayout.EndHorizontal();

		DrawBoneArrayGui();

        #region Clear and Save Buttons
        EditorGUILayout.BeginHorizontal();
		{
			GUI.SetNextControlName("ClearButton");
			if (GUILayout.Button("Clear"))
			{
				ResetSettings();
				GUI.FocusControl("ClearButton");
			}

			if (GUILayout.Button("Export"))
			{
                //Save 
                if(eulerAngle != Vector3.zero) {
                    exportSettings.quaternionOffset = Quaternion.Euler(eulerAngle);
                }
                
                //MeshAnimationExporter.Export(fbx, exportSettings);
                MeshAnimationExporter.ExportCombinedTexture(fbx, exportSettings);
            }
		}

		EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Export All"))
            {
                if (EditorUtility.DisplayDialog("", "确定导出所有小兵动画？", "是", "否"))
                {
                    string folder = "Assets/Models/Units/Solider";
                    string[] dirs = Directory.GetDirectories(folder);
                    int progress = 0;
                    foreach (string dir in dirs)
                    {
                        string name = Path.GetFileName(dir);
                        string path = string.Format("{0}/{1}/{1}.fbx", folder, name);
                        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (fbx != null)
                        {
                            EditorUtility.DisplayProgressBar(folder, name, (progress++)/(float)dirs.Length);
                            exportSettings = MeshAnimationExporter.GenerateDefaultSettings(fbx, DEFAULT_OUTPUT_FOLDER);
                            outputFolderPath = exportSettings.outputFolderPath;
                            outputFilePath = exportSettings.outputFilePath;
                            guiNumberOfClips = exportSettings.animationClips.Length;
                            guiNumberOfBones = exportSettings.boneTransforms.Length;

                            //Save 
                            if (eulerAngle != Vector3.zero)
                            {
                                exportSettings.quaternionOffset = Quaternion.Euler(eulerAngle);
                            }

                            MeshAnimationExporter.ExportCombinedTexture(fbx, exportSettings);
                        }
                        else
                        {
                            Debug.LogErrorFormat("没有找到小兵模型:{0}", path);
                        }
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.EndScrollView();
	}

	private void DrawBoneArrayGui()
	{

		if (exportSettings.boneNames != null && exportSettings.boneNames.Length > 0)
		{
			for (int i = 0; i < exportSettings.boneNames.Length; i++)
			{
				EditorGUILayout.BeginHorizontal();
				exportSettings.boneNames[i] = EditorGUILayout.TextField(exportSettings.boneNames[i], labelWidth);
				
				Transform bone = EditorGUILayout.ObjectField(exportSettings.boneTransforms[i], typeof(Transform), true, labelWidth) as Transform;
				
				if (bone != exportSettings.boneTransforms[i] && bone != null)
				{
					exportSettings.boneNames[i] = bone.name;
				}
				
				exportSettings.boneTransforms[i] = bone;
				
				EditorGUILayout.EndHorizontal();
			}
		}
	}

	private void DrawAnimationArrayGui()
	{
		float interval = 1.0f / exportSettings.framerate;

		if (exportSettings.animationNames != null && exportSettings.animationNames.Length > 0)
		{
			for (int i = 0; i < exportSettings.animationNames.Length; i++)
			{
				EditorGUILayout.BeginHorizontal();
				exportSettings.animationNames[i] = EditorGUILayout.TextField(exportSettings.animationNames[i], labelWidth);
				
				AnimationClip clip = EditorGUILayout.ObjectField(exportSettings.animationClips[i], typeof(AnimationClip), true, labelWidth) as AnimationClip;
				
				if (clip != exportSettings.animationClips[i] && clip != null)
				{
					exportSettings.animationNames[i] = clip.name;
				}
				
				exportSettings.animationClips[i] = clip;
				
				float frameCount = 0;
				
				if (clip != null)
				{
					frameCount = clip.length / interval;
				}
				
				EditorGUILayout.LabelField(frameCount.ToString(), GUILayout.Width(windowWidth * 0.15f));

                string result = "命名错误";
                if (CheckAnimationName(clip.name))
                {
                    result = "";
                }
                EditorGUILayout.LabelField(result, GUILayout.Width(windowWidth * 0.2f));
				
                EditorGUILayout.EndHorizontal();
			}
		}
	}

    /// <summary>
    /// 检查动画名称是否有错.
    /// attack1\win\dead\hit\run\wait1\wait2\skill1
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool CheckAnimationName(string name)
    {
        string[] strs = new string[] { "attack1", "win", "dead", "hit", "run", "wait1", "wait2", "skill1"};
        for (int i = 0; i < strs.Length; i++)
        {
            if (name.Equals(strs[i]))
            {
                return true;
            }
        }
        return false;
    }

	private void ApplyChanges()
	{		
		if (Event.current.isKey)
		{			
			switch (Event.current.keyCode)
			{				
				case KeyCode.Return:				
				case KeyCode.KeypadEnter:				
					Event.current.Use();
					MeshAnimationExporter.ResizeDataLists(ref exportSettings.boneTransforms, ref exportSettings.boneNames, guiNumberOfBones);
					MeshAnimationExporter.ResizeDataLists(ref exportSettings.animationClips, ref exportSettings.animationNames, guiNumberOfClips);
					break;				
			}			
		}
	}

	private void BrowseSaveFile()
	{
		string output = EditorUtility.SaveFilePanel(
			"Save binary outpout",
			outputFolderPath,
			"",
			"bytes"
		);

		if (!string.IsNullOrEmpty(output.Trim()))
		{
			exportSettings.outputFilePath = outputFilePath = output;
			exportSettings.outputFolderPath = outputFolderPath = Path.GetDirectoryName(output);
		}

		GUI.FocusControl("");
	}







}
