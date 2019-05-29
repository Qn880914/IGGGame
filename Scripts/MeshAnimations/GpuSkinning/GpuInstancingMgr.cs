using System.Collections.Generic;
using UnityEngine;

namespace IGG.MeshAnimation
{
    public class GpuInstancingMgr : IGG.Utility.Singleton<GpuInstancingMgr>
    {
        //key: skinId（兵种的配置id）
        Dictionary<int, InstancingBatchData> m_instancingBatchDataDic = new Dictionary<int, InstancingBatchData>();

        private int m_layer;
        public int Layer {
            get { return m_layer; }
            set { m_layer = value; }
        }
        private Camera m_camera;
        public Camera RenderCamera {
            get { return m_camera; }
            set { m_camera = value; }
        }
        private UnityEngine.Rendering.ShadowCastingMode m_shadowCastingMode;
        public UnityEngine.Rendering.ShadowCastingMode CustomShadowCastingMode {
            get { return m_shadowCastingMode; }
            set { m_shadowCastingMode = value; }
        }

        private int m_frameIndexProId;
        public int FrameIndexProId {
            get { return m_frameIndexProId; }
        }

        private int m_colorProIdR, m_colorProIdG, m_colorProIdB, m_colorProIdA;
        public int AddColorProIdR {
            get { return m_colorProIdR; }
        }
        public int AddColorProIdG
        {
            get { return m_colorProIdG; }
        }
        public int AddColorProIdB
        {
            get { return m_colorProIdB; }
        }
        public int AddColorProIdA
        {
            get { return m_colorProIdA; }
        }

        private int m_groupProIdR, m_groupProIdG, m_groupProIdB;
        public int GroupProIdR {
            get { return m_groupProIdR; }
        }
        public int GroupProIdG {
            get { return m_groupProIdG; }
        }
        public int GroupProIdB {
            get { return m_groupProIdB; }
        }

        //private int m_alphaValueProId;
        //public int AlphaValueProId {
        //    get { return m_alphaValueProId; }
        //}

        private int m_bProId;
        private int m_sProId;
        private int m_cProId;
        private int m_enableProId;

        public int ProIdB {
            get { return m_bProId; }
        }
        public int ProIdS {
            get { return m_sProId; }
        }
        public int ProIdC {
            get { return m_cProId; }
        }
        public int ProIdEnableBsc {
            get { return m_enableProId; }
        }

        private int m_openIceProId;
        public int OpenIceProId
        {
            get { return m_openIceProId; }
        }

        public const int InstancingSizePerBatch = 50;

        private float[] m_tempColorR = new float[InstancingSizePerBatch];
        private float[] m_tempColorG = new float[InstancingSizePerBatch];
        private float[] m_tempColorB = new float[InstancingSizePerBatch];
        private float[] m_tempColorA = new float[InstancingSizePerBatch];
        private float[] m_tempB = new float[InstancingSizePerBatch];
        private float[] m_tempS = new float[InstancingSizePerBatch];
        private float[] m_tempC = new float[InstancingSizePerBatch];
        private float[] m_enableBsc = new float[InstancingSizePerBatch];

        public GpuInstancingMgr()
        {
            m_frameIndexProId = Shader.PropertyToID("frameIndex");
            m_colorProIdR = Shader.PropertyToID("_AddColorR");
            m_colorProIdG = Shader.PropertyToID("_AddColorG");
            m_colorProIdB = Shader.PropertyToID("_AddColorB");
            m_colorProIdA = Shader.PropertyToID("_AddColorA");
            m_bProId = Shader.PropertyToID("_Brightness");
            m_sProId = Shader.PropertyToID("_Saturation");
            m_cProId = Shader.PropertyToID("_Contrast");
            m_enableProId = Shader.PropertyToID("_EnableBsc");
            m_groupProIdR = Shader.PropertyToID("_GroupColorR");
            m_groupProIdG = Shader.PropertyToID("_GroupColorG");
            m_groupProIdB = Shader.PropertyToID("_GroupColorB");
            //m_alphaValueProId = Shader.PropertyToID("_AlphaVal");
            m_openIceProId = Shader.PropertyToID("_OpenIce");
        }

        public bool ContainInstancingBatchData(int skinId)
        {
            return m_instancingBatchDataDic.ContainsKey(skinId);
        }

        public void AddInstancingBatchData(int skinId, InstancingBatchData data)
        {
            m_instancingBatchDataDic.Add(skinId, data);
        }

        public void AddInstancingBatchData(int skinId, GpuSkinData data, Material material, Texture tex)
        {
            Mesh[] skinMeshes = new Mesh[data.SkinMeshes.Length];
            for (int i = 0; i < data.SkinMeshes.Length; i++)
            {
                Mesh mesh = new Mesh();
                mesh.name = "GpuSkin_" + data.SkinMeshes[i].Name;
                mesh.vertices = data.SkinMeshes[i].Vertices;
                mesh.triangles = data.SkinMeshes[i].Triangles;
                mesh.SetIndices(data.SkinMeshes[i].Indices, MeshTopology.Triangles, 0);
                mesh.normals = data.SkinMeshes[i].Normals;
                mesh.uv = data.SkinMeshes[i].Uv;
                List<Vector4> uvs2 = new List<Vector4>();
                uvs2.AddRange(data.SkinMeshes[i].BoneIndex);
                mesh.SetUVs(1, uvs2);
                List<Vector4> uvs3 = new List<Vector4>();
                uvs3.AddRange(data.SkinMeshes[i].Weights);
                mesh.SetUVs(2, uvs3);
                skinMeshes[i] = mesh;
            }
            InstancingBatchData instancingData = new InstancingBatchData(skinMeshes, new MaterialPropertyBlock(), material);
            instancingData.SetMaterialProperty(tex, data.BlockWidth, data.BlockHeight, data.AnimationTextureWidth, data.AnimationTextureHeight);
            m_instancingBatchDataDic.Add(skinId, instancingData);
        }

        private bool RemoveInstancingBatchData(int skinId)
        {
            return m_instancingBatchDataDic.Remove(skinId);
        }

        public bool TryGetInstancingBatchData(int skinId, out InstancingBatchData data)
        {
            return m_instancingBatchDataDic.TryGetValue(skinId, out data);
        }

        /// <summary>
        /// 实际上GpuSkinAnimator也充当GpuSkinRenderer的角色
        /// </summary>
        /// <returns></returns>
        public bool AddGpuInstancingObjData(int skinId, GpuSkinAnimator animator, Color addColor, Vector4 bsc, Color groupColor)
        {
            InstancingBatchData batchData;
            if (TryGetInstancingBatchData(skinId, out batchData))
            {
                return batchData.AddInstancingObj(animator, addColor, bsc, groupColor);
            }
            else
            {
                return false;
            }
        }

        public bool RemoveGpuInstancingObjData(int skinId, int instanceId)
        {
            InstancingBatchData batchData;
            if (TryGetInstancingBatchData(skinId, out batchData))
            {
                bool result = batchData.RemoveInstancingObj(instanceId);
                //如果这个批处理的count为0，则移除这个批处理。
                if (batchData.InstancingObjCount <= 0)
                {
                    RemoveInstancingBatchData(skinId);
                }
                return result;
            }
            return false;
        }

        public bool TryGetGpuInstancingObjData(int skinId, int instanceId, out InstancingObjData data)
        {
            data = new InstancingObjData();
            InstancingBatchData batchData;
            if (TryGetInstancingBatchData(skinId, out batchData))
            {
                return batchData.TryGetInstancingObj(instanceId, out data);
            }
            return false;
        }

        public bool SetInstancingObjAdditiveColor(int skinId, int instanceId, Color addColor)
        {
            InstancingObjData data;
            if (TryGetGpuInstancingObjData(skinId, instanceId, out data))
            {
                data.AdditiveColor = addColor;
                return true;
            }
            return false;
        }

        public bool SetInstancingObjBsc(int skinId, int instanceId, Vector4 bsc)
        {
            InstancingObjData data;
            if (TryGetGpuInstancingObjData(skinId, instanceId, out data))
            {
                data.Bsc = bsc;
                return true;
            }
            return false;
        }

        public bool SetInstancingObjGroupColor(int skinId, int instanceId, Color groupColor)
        {
            InstancingObjData data;
            if (TryGetGpuInstancingObjData(skinId, instanceId, out data))
            {
                data.GroupColor = groupColor;
                return true;
            }
            return false;
        }

        //public bool SetInstancingObjAlphaVal(int skinId, int instanceId, float alphaVal)
        //{
        //    InstancingObjData data;
        //    if (TryGetGpuInstancingObjData(skinId, instanceId, out data))
        //    {
        //        data.AlphaVal = alphaVal;
        //        return true;
        //    }
        //    return false;
        //}

        public bool SetInstancingObjOpenIce(int skinId, int instanceId, bool openIce)
        {
            InstancingObjData data;
            if (TryGetGpuInstancingObjData(skinId, instanceId, out data))
            {
                data.OpenIce = openIce ? 1 : 0;
                return true;
            }
            return false;
        }

        public void Update()
        {
            Render();
        }

        private void Render()
        {
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.BeginSample("Gpu Instancing Render");
#endif

            foreach (KeyValuePair<int, InstancingBatchData> item in m_instancingBatchDataDic)
            {
                item.Value.UpdateRenderPropertyArray();

                Matrix4x4[][] matrixArray = item.Value.TempMatrixArray;
                float[][] frameIndexArray = item.Value.TempFrameIndexArray;
                Color[][] addColorArray = item.Value.TempAddColorArray;
                Vector4[][] bscArray = item.Value.TempBscArray;
                Color[][] groupColorArray = item.Value.TempGroupColorArray;
                //float[][] alphaValueArray;
                float[][] openIceArray = item.Value.TempOpenIceArray;

#if UNITY_EDITOR
                item.Value.PrepareMaterial();
#endif

                for (int i = 0; i < matrixArray.Length; i++)
                {
                    item.Value.PropertyBlock.SetFloatArray(m_frameIndexProId, frameIndexArray[i]);
                    //item.Value.PropertyBlock.SetFloatArray(m_alphaValueProId, alphaValueArray[i]);
                    item.Value.PropertyBlock.SetFloatArray(m_openIceProId, openIceArray[i]);

                    //if (m_tempColorA.Length != addColorArray[i].Length)
                    //{
                    //    m_tempColorR = new float[addColorArray[i].Length];
                    //    m_tempColorG = new float[addColorArray[i].Length];
                    //    m_tempColorB = new float[addColorArray[i].Length];
                    //    m_tempColorA = new float[addColorArray[i].Length];
                    //}
                    
                    for (int j = 0; j < addColorArray[i].Length; j++)
                    {
                        m_tempColorR[j] = addColorArray[i][j].r;
                        m_tempColorG[j] = addColorArray[i][j].g;
                        m_tempColorB[j] = addColorArray[i][j].b;
                        m_tempColorA[j] = addColorArray[i][j].a;
                    }

                    item.Value.PropertyBlock.SetFloatArray(m_colorProIdR, m_tempColorR);
                    item.Value.PropertyBlock.SetFloatArray(m_colorProIdG, m_tempColorG);
                    item.Value.PropertyBlock.SetFloatArray(m_colorProIdB, m_tempColorB);
                    item.Value.PropertyBlock.SetFloatArray(m_colorProIdA, m_tempColorA);

                    //
                    for (int j = 0; j < groupColorArray[i].Length; j++)
                    {
                        m_tempColorR[j] = groupColorArray[i][j].r;
                        m_tempColorG[j] = groupColorArray[i][j].g;
                        m_tempColorB[j] = groupColorArray[i][j].b;
                    }

                    item.Value.PropertyBlock.SetFloatArray(m_groupProIdR, m_tempColorR);
                    item.Value.PropertyBlock.SetFloatArray(m_groupProIdG, m_tempColorG);
                    item.Value.PropertyBlock.SetFloatArray(m_groupProIdB, m_tempColorB);

                    //
                    //if (m_tempB.Length != bscArray[i].Length)
                    //{
                    //    m_tempB = new float[bscArray[i].Length];
                    //    m_tempS = new float[bscArray[i].Length];
                    //    m_tempC = new float[bscArray[i].Length];
                    //    m_enableBsc = new float[bscArray[i].Length];
                    //}

                    for (int j = 0; j < bscArray[i].Length; j++)
                    {
                        m_tempB[j] = bscArray[i][j].x;
                        m_tempS[j] = bscArray[i][j].y;
                        m_tempC[j] = bscArray[i][j].z;
                        m_enableBsc[j] = bscArray[i][j].w;
                    }

                    item.Value.PropertyBlock.SetFloatArray(m_bProId, m_tempB);
                    item.Value.PropertyBlock.SetFloatArray(m_sProId, m_tempS);
                    item.Value.PropertyBlock.SetFloatArray(m_cProId, m_tempC);
                    item.Value.PropertyBlock.SetFloatArray(m_enableProId, m_enableBsc);

                    for (int j = 0; j < item.Value.SkinMeshes.Length; j++)
                    {
                        Camera camera = RenderCamera == null ? Camera.main : RenderCamera;
                        if (null != camera)
                        {
                            Graphics.DrawMeshInstanced(item.Value.SkinMeshes[j], 
                                0, 
                                item.Value.SharedMaterial,
                                matrixArray[i], 
                                matrixArray[i].Length, 
                                item.Value.PropertyBlock, 
                                CustomShadowCastingMode, 
                                false, 
                                m_layer, 
                                camera);

                            //for (int k = 0; k < matrixArray[i].Length; k++)
                            //{
                            //    Graphics.DrawMesh(item.Value.SkinMeshes[j], matrixArray[i][k], item.Value.SharedMaterial, m_layer, camera, 
                            //        0, item.Value.PropertyBlock);
                            //}
                        }
                    }
                }
            }
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, InstancingBatchData> item in m_instancingBatchDataDic)
            {
                if (null != item.Value)
                {
                    item.Value.Dispose();
                }
            }
            m_instancingBatchDataDic.Clear();
        }

    }
}
