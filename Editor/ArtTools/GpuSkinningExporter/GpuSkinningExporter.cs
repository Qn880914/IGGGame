using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace IGG.MeshAnimation
{
    public class AnimationBakeInfo
    {
        public struct RecordClipData
        {
            public RecordFrameData[] Frames;
        }

        public struct RecordFrameData
        {
            //数组大小和Bones的数量一致
            public Matrix4x4[] BoneMatrix;
        }

        public AnimationClip Clip;
        public int CurFrame;
        public int TotalFrameNum;

        public RecordClipData ClipData;     //动画片段数据

        public AnimationBakeInfo(AnimationClip clip, int fps)
        {
            Clip = clip;
            CurFrame = 0;
            if (null != clip)
            {
                TotalFrameNum = Mathf.CeilToInt(clip.length * fps);
                ClipData = new RecordClipData();
                ClipData.Frames = new RecordFrameData[TotalFrameNum];
            }
        }
    }

    public class GpuSkinningExporter : EditorWindow
    {
        private GameObject m_fbx;   //采样目标FBX
        private List<GameObject> m_fbxBatchList = new List<GameObject>();
        private int m_fbxBatchIndex = 0;
        private int m_fps = 30;
        private int m_blockWidth = 3;

        private string m_exportFolder = "Assets/Data/Units/Troop/GpuSkinningData";
        private string m_troopDefaultFolder = "Assets/Models/Units/GpuSkinSolider";
        private List<AnimationClip> m_clipList = new List<AnimationClip>();
        private List<string> m_animationNameList = new List<string>();
        private List<AnimationBakeInfo> m_bakeList = new List<AnimationBakeInfo>();
        private AnimationBakeInfo m_workingInfo;
        private int m_workingIndex;
        private GameObject m_generatedObject;   //实例化出来用于采样的GameObject
        private Animator m_animator;

        private Transform[] m_bones;
        private Matrix4x4[] m_bindposes;        //M->J

        GpuSkinDataProxy m_skinData;
        void Reset()
        {
            m_clipList.Clear();
            m_animationNameList.Clear();
            m_bakeList.Clear();
            m_fbx = null;
            if (null != m_generatedObject)
            {
                GameObject.DestroyImmediate(m_generatedObject);
                m_generatedObject = null;
            }
            m_animator = null;
            m_workingInfo = null;
            m_bones = null;
            m_bindposes = null;
            if (null != m_skinData)
            {
                m_skinData = null;
            }
            m_workingIndex = 0;
        }

        private void Init()
        {
            if (null == m_generatedObject)
            {
                return;
            }
            SkinnedMeshRenderer[] smr = m_generatedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            m_skinData = GpuSkinDataProxy.CreateInstance<GpuSkinDataProxy>();
            m_skinData.SkinMeshes = new GpuSkinData.CustomSkinMesh[smr.Length];

            Vector4[][] boneIndex;
            Vector4[][] weights;
            MergeBone(smr, out m_bones, out m_bindposes, out boneIndex, out weights);


            for (int i = 0; i < smr.Length; i++)
            {
                Mesh mesh = new Mesh();

                m_skinData.SkinMeshes[i] = new GpuSkinData.CustomSkinMesh();
                m_skinData.SkinMeshes[i].Name = smr[i].sharedMesh.name;
                m_skinData.SkinMeshes[i].Vertices = smr[i].sharedMesh.vertices;
                m_skinData.SkinMeshes[i].Triangles = smr[i].sharedMesh.triangles;

                m_skinData.SkinMeshes[i].Indices = smr[i].sharedMesh.GetIndices(0); 
                m_skinData.SkinMeshes[i].Normals = smr[i].sharedMesh.normals;
                m_skinData.SkinMeshes[i].Uv = smr[i].sharedMesh.uv;
                m_skinData.SkinMeshes[i].BoneIndex = boneIndex[i];
                m_skinData.SkinMeshes[i].Weights = weights[i];
            }
        }

        /// <summary>
        /// 多个SkinnedMeshRenderer有多个骨骼，需要把这些骨骼合并到一起，同时需要修改骨骼索引
        /// </summary>
        private void MergeBone(SkinnedMeshRenderer[] smrs, out Transform[] bones, out Matrix4x4[] bindposes, out Vector4[][] boneIndex, out Vector4[][] weights)
        {
            List<Transform> boneTemp = new List<Transform>();
            List<Matrix4x4> bindposeTemp = new List<Matrix4x4>();
            boneIndex = new Vector4[smrs.Length][];
            weights = new Vector4[smrs.Length][];

            for (int i = 0; i < smrs.Length; i++)
            {
                Transform[] subBones = smrs[i].bones;
                Matrix4x4[] subBindposes = smrs[i].sharedMesh.bindposes;
                boneIndex[i] = new Vector4[smrs[i].sharedMesh.vertexCount];
                weights[i] = new Vector4[smrs[i].sharedMesh.vertexCount];
                for (int j = 0; j < smrs[i].sharedMesh.vertexCount; j++)
                {
                    BoneWeight bw = smrs[i].sharedMesh.boneWeights[j];
                    boneIndex[i][j] = new Vector4(bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3);
                    weights[i][j] = new Vector4(bw.weight0, bw.weight1, bw.weight2, bw.weight3);
                }

                //用来保存新的索引数据
                Vector4[] temp = new Vector4[boneIndex[i].Length];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = new Vector4(-1, -1, -1, -1);
                }

                for (int j = 0; j < subBones.Length; j++)
                {
                    int index = boneTemp.FindIndex(p => { return p == subBones[j]; });
                    if (index >= 0)
                    {
                        bindposeTemp[index] = subBindposes[j];
                        ModifyBoneIndex(boneIndex[i], temp, j, index);
                    }
                    else
                    {
                        boneTemp.Add(subBones[j]);
                        bindposeTemp.Add(subBindposes[j]);
                        index = boneTemp.Count - 1;
                        ModifyBoneIndex(boneIndex[i], temp, j, index);
                    }
                }

                //把暂存在temp的索引数据赋值给boneIndex[i]
                for (int j = 0; j < boneIndex[i].Length; j++)
                {
                    if (temp[j].x >= 0)
                    {
                        boneIndex[i][j].x = temp[j].x;
                    }
                    if (temp[j].y >= 0)
                    {
                        boneIndex[i][j].y = temp[j].y;
                    }
                    if (temp[j].z >= 0)
                    {
                        boneIndex[i][j].z = temp[j].z;
                    }
                    if (temp[j].w >= 0)
                    {
                        boneIndex[i][j].w = temp[j].w;
                    }
                }
            }
            bones = boneTemp.ToArray();
            bindposes = bindposeTemp.ToArray();
        }

        private void ModifyBoneIndex(Vector4[] boneIndex, Vector4[] temp, int src, int dst)
        {
            for (int i = 0; i < boneIndex.Length; i++)
            {
                if ((int)boneIndex[i].x == src)
                {
                    temp[i].x = dst;
                }
                if ((int)boneIndex[i].y == src)
                {
                    temp[i].y = dst;
                }
                if ((int)boneIndex[i].z == src)
                {
                    temp[i].z = dst;
                }
                if ((int)boneIndex[i].w == src)
                {
                    temp[i].w = dst;
                }
            }
        }

        private void OnEnable()
        {
            m_fbxBatchIndex = 0;
            m_fbxBatchList.Clear();
            Reset();
            EditorApplication.update += SampleAnimation;
            EditorApplication.update += SampleAllTroopAnimation;
        }

        private void OnDisable()
        {
            m_fbxBatchIndex = 0;
            m_fbxBatchList.Clear();
            Reset();
            EditorApplication.update -= SampleAnimation;
            EditorApplication.update -= SampleAllTroopAnimation;
        }

        [MenuItem("辅助工具/资源管理/GpuSkinningExporter")]
        static void MakeWindow()
        {
            GpuSkinningExporter window = (GpuSkinningExporter)EditorWindow.GetWindow(typeof(GpuSkinningExporter));
            window.ShowUtility();
        }

        private bool CheckAnimationNames()
        {
            for (int i = 0; i < m_animationNameList.Count; i++)
            {
                if (!CheckAnimationName(m_animationNameList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查动画名称是否有错.
        /// attack1\win\dead\hit\run\wait1\wait2\skill1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckAnimationName(string name)
        {
            name = name.ToLower();
            string[] strs = new string[] { "attack1", "win", "dead", "hit", "run", "wait1", "wait2", "skill1" };
            for (int i = 0; i < strs.Length; i++)
            {
                if (name.Equals(strs[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private void BrowseSaveDir()
        {
            string output = EditorUtility.OpenFolderPanel(
                "Save data path",
                m_exportFolder,
                ""
            );

            if (!output.StartsWith(Application.dataPath))
            {
                Debug.LogError(output+" is not under "+Application.dataPath);
                return;
            }
            m_exportFolder = output.Substring(output.IndexOf("Assets"));

            GUI.FocusControl("");
        }

        private void OnGUI()
        {
            GameObject target = EditorGUILayout.ObjectField(m_fbx, typeof(GameObject), true) as GameObject;
            if (target != m_fbx)
            {
                Reset();
                m_fbx = target;
                LoadSampleAnimationClip();
            }
            m_fps = EditorGUILayout.IntSlider("FPS", m_fps, 1, 60);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Animation Name:", GUILayout.Width(position.width * 0.3f));
                EditorGUILayout.LabelField("Animation File:", GUILayout.Width(position.width * 0.3f));
                EditorGUILayout.LabelField("Frames:", GUILayout.Width(position.width * 0.2f));
            }
            EditorGUILayout.EndHorizontal();


            for (int i = 0; i < m_clipList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_animationNameList[i] = EditorGUILayout.TextField(m_animationNameList[i], GUILayout.Width(position.width * 0.3f));
                EditorGUILayout.ObjectField(m_clipList[i], typeof(AnimationClip), true, GUILayout.Width(position.width * 0.3f));
                int frameCount = Mathf.CeilToInt(m_clipList[i].frameRate * m_clipList[i].length);
                EditorGUILayout.LabelField(frameCount.ToString(), GUILayout.Width(position.width * 0.15f));

                if (!CheckAnimationName(m_clipList[i].name))
                {
                    EditorGUILayout.LabelField("命名错误", GUILayout.Width(position.width * 0.2f));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Output Dir:");
            EditorGUILayout.BeginHorizontal();
            m_exportFolder = EditorGUILayout.TextField(m_exportFolder, GUILayout.Width(position.width*0.8f));
            if (GUILayout.Button("Browse"))
            {
                BrowseSaveDir();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export"))
            {
                if (null == m_fbx)
                {
                    return;
                }
                m_generatedObject = GameObject.Instantiate(m_fbx);
                Init();
                m_animator = m_generatedObject.GetComponent<Animator>();
                CreateAnimationController();
                GenerateBakeData();        
            }
            if (GUILayout.Button("ExportAll"))
            {
                if (EditorUtility.DisplayDialog("GPU Skin", "导出所有小兵的动画数据?", "是", "否"))
                {
                    DoExportAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DoExportAll()
        {
            m_fbxBatchList.Clear();
            string[] dirs = Directory.GetDirectories(m_troopDefaultFolder);
            for (int i = 0; i < dirs.Length; i++)
            {
                List<string> files = FileUtil.GetAllChildFiles(dirs[i], ".FBX");
                for (int j = 0; j < files.Count; j++)
                {
                    if (!files[j].Contains("@") || (files[j].Contains("@skin")))
                    {
                        GameObject fbx = AssetDatabase.LoadAssetAtPath(files[j], typeof(GameObject)) as GameObject;
                        if (null != fbx)
                        {
                            m_fbxBatchList.Add(fbx);
                        }
                    }
                }
            }
        }

        private void GenerateBakeData()
        {
            for (int i = 0; i < m_clipList.Count; i++)
            {
                m_bakeList.Add(new AnimationBakeInfo(m_clipList[i], m_fps));
            }
        }

        private void CreateAnimationController()
        {
            if (null == m_fbx)
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(m_fbx);
            path = path.Remove(path.LastIndexOf('/'));
            AnimatorController ctr = AnimatorController.CreateAnimatorControllerAtPath(path + "/" + m_fbx.name + ".controller");
            AnimatorControllerLayer layer = ctr.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;
            for (int i = 0; i < m_clipList.Count; i++)
            {
                AnimatorState state = sm.AddState(m_clipList[i].name.ToLower());
                state.motion = m_clipList[i];
            }
            if (null != m_animator)
            {
                m_animator.runtimeAnimatorController = ctr;
            }
        }

        private void LoadSampleAnimationClip()
        {
            m_clipList.Clear();
            m_animationNameList.Clear();
            string path = AssetDatabase.GetAssetPath(m_fbx);
            string dir = path.Remove(path.LastIndexOf('/'));
            List<string> files = FileUtil.GetAllChildFiles(dir, ".FBX");
            for (int i = 0; i < files.Count; i++)
            {
                string name = files[i].Remove(0, files[i].LastIndexOf('/') + 1);
                name = name.Substring(0, name.LastIndexOf('.'));
                if (!name.Contains("@") || (name.ToLower().Contains("@skin")))
                {
                    continue;
                }
                AnimationClip clip = AssetDatabase.LoadAssetAtPath(files[i], typeof(AnimationClip)) as AnimationClip;
                if (null != clip)
                {
                    m_clipList.Add(clip);
                    m_animationNameList.Add(clip.name.Capitalize());
                }
            }
        }

        private void SampleAllTroopAnimation()
        {
            if (m_fbxBatchList.Count <= 0)
            {
                return;
            }

            //采样下一个troop
            if (m_bakeList.Count <= 0)
            {
                //全部都完成了
                if (m_fbxBatchIndex >= m_fbxBatchList.Count)
                {
                    Debug.Log("导出完成");
                    m_fbxBatchIndex = 0;
                    m_fbxBatchList.Clear();
                    return;
                }
                m_fbx = m_fbxBatchList[m_fbxBatchIndex++];
                LoadSampleAnimationClip();
                if (!CheckAnimationNames())
                {
                    Debug.LogError(m_fbx.name+"存在命名不规范的动画文件，跳过导出。");
                    Reset();
                    return;
                }
                m_generatedObject = GameObject.Instantiate(m_fbx);
                Init();
                m_animator = m_generatedObject.GetComponent<Animator>();
                CreateAnimationController();
                GenerateBakeData();
            }

            if (null == m_workingInfo)
            {
                //采样完成
                if (m_workingIndex >= m_bakeList.Count)
                {
                    Save();
                    Reset();
                    return;
                }
                m_workingInfo = m_bakeList[m_workingIndex++];
                if (null != m_animator)
                {
                    m_animator.gameObject.SetActive(true);
                    m_animator.Update(0);
                    m_animator.Play(m_workingInfo.Clip.name.ToLower());
                    m_animator.Update(0);
                }
                return;
            }

            if (null != m_workingInfo)
            {
                if (m_workingInfo.CurFrame >= m_workingInfo.TotalFrameNum)
                {
                    m_workingInfo = null;
                    return;
                }

                if (m_workingInfo.CurFrame == 0)
                {
                    m_animator.Update(0);
                }
                else
                {
                    float time = 1.0f / m_workingInfo.TotalFrameNum * m_workingInfo.Clip.length;
                    if (null != m_animator)
                    {
                        m_animator.Update(time);
                    }
                }

                m_workingInfo.ClipData.Frames[m_workingInfo.CurFrame].BoneMatrix = CalculateSkinMatrix(m_bones, m_bindposes);

                m_workingInfo.CurFrame++;
            }
        }

        private void SampleAnimation()
        {
            //未开始采样
            if (m_bakeList.Count <= 0)
            {
                return;
            }
            if (null == m_workingInfo)
            {
                //采样完成
                if (m_workingIndex >= m_bakeList.Count)
                {
                    Save();
                    Reset();
                    return;
                }
                m_workingInfo = m_bakeList[m_workingIndex++];
                if (null != m_animator)
                {
                    m_animator.gameObject.SetActive(true);
                    m_animator.Update(0);
                    m_animator.Play(m_workingInfo.Clip.name.ToLower());
                    m_animator.Update(0);
                }
                return;
            }
            
            if (null != m_workingInfo)
            {
                if (m_workingInfo.CurFrame >= m_workingInfo.TotalFrameNum)
                {
                    m_workingInfo = null;
                    return;
                }

                if (m_workingInfo.CurFrame == 0)
                {
                    m_animator.Update(0);
                }
                else
                {
                    float time = 1.0f / m_workingInfo.TotalFrameNum * m_workingInfo.Clip.length;
                    if (null != m_animator)
                    {
                        m_animator.Update(time);
                    }
                }

                m_workingInfo.ClipData.Frames[m_workingInfo.CurFrame].BoneMatrix = CalculateSkinMatrix(m_bones, m_bindposes);


                m_workingInfo.CurFrame++;
            }
        }

        private void Save()
        {
            //save texture2d
            int blockWidth = m_blockWidth;
            int blockHeight = m_bones.Length;

            int totalFrame = 0;
            for (int i = 0; i < m_bakeList.Count; i++)
            {
                totalFrame += m_bakeList[i].TotalFrameNum;
            }

            int needPixelCount = totalFrame * blockWidth * blockHeight;
            int textureWidth, textureHeight;
            GetPowerOfTwo(needPixelCount, out textureWidth, out textureHeight);
            textureWidth = (int)Mathf.Pow(2, textureWidth);
            textureHeight = (int)Mathf.Pow(2, textureHeight);

            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAHalf, false);
            int index = 0;
            for (int i = 0; i < m_bakeList.Count; i++)
            {
                for (int j = 0; j < m_bakeList[i].TotalFrameNum; j++)
                {
                    Matrix4x4[] mats = m_bakeList[i].ClipData.Frames[j].BoneMatrix;

                    for (int k = 0; k < mats.Length; k++)
                    {
                        for (int m = 0; m < m_blockWidth && m < 4; m++)
                        {
                            Color c = mats[k].GetRow(m);
                            int x = index % textureWidth;
                            int y = index / textureWidth;
                            texture.SetPixel(x, y, c);
                            index++;
                        }
                    }
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            AssetDatabase.CreateAsset(texture, m_exportFolder + "/" + m_fbx.name + "_anim.asset");
            AssetDatabase.Refresh();
            //save asset
            m_skinData.Fps = m_fps;
            m_skinData.BlockWidth = blockWidth;
            m_skinData.BlockHeight = blockHeight;
            m_skinData.AnimationTextureWidth = textureWidth;
            m_skinData.AnimationTextureHeight = textureHeight;

            m_skinData.Clips = new GpuSkinData.CustomClipData[m_bakeList.Count];
            int frameIndex = 0;
            for (int i = 0; i < m_bakeList.Count; i++)
            {
                m_skinData.Clips[i].ClipName = m_bakeList[i].Clip.name.Capitalize();
                m_skinData.Clips[i].FrameNum = m_bakeList[i].TotalFrameNum;
                m_skinData.Clips[i].StartFrameIndex = frameIndex;
                frameIndex += m_bakeList[i].TotalFrameNum;
            }
            AssetDatabase.CreateAsset(m_skinData, m_exportFolder + "/" + m_fbx.name + "_gpuskin.asset");
            AssetDatabase.Refresh();
        }

        private void GetPowerOfTwo(int a, out int width, out int height)
        {
            int x = 1;
            int y = 0;
            while (x < a)
            {
                x <<= 1;
                y++;
            }
            if (y % 2 == 0)
            {
                width = y / 2;
                height = y / 2;
            }
            else
            {
                width = y / 2 + 1;
                height = y / 2;
            }
        }

        private Matrix4x4[] CalculateSkinMatrix(Transform[] bonePose,
            Matrix4x4[] bindPose)
        {
            if (bonePose.Length == 0)
                return null;

            Transform root = bonePose[0];
            while (root.parent != null)
            {
                root = root.parent;
            }
            Matrix4x4 rootMat = root.worldToLocalMatrix;

            Matrix4x4[] matrix = new Matrix4x4[bonePose.Length];
            for (int i = 0; i != bonePose.Length; ++i)
            {
                //bindPose: ModelSpace->JointSpace 
                //bonePose.localToWorldMatrix: JointSpace->WorldSpace
                //rootMat: WorldSpace->ModelSpace
                matrix[i] = rootMat * bonePose[i].localToWorldMatrix * bindPose[i];
            }
            return matrix;
        }
    }
}