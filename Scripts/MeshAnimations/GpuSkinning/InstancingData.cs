using System;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.MeshAnimation
{
    /// <summary>
    /// 每个需要渲染的实例对象的数据
    /// </summary>
    public class InstancingObjData : IDisposable
    {
        public GpuSkinAnimator Animator;

        protected Color m_additiveColor = new Color(0, 0, 0, 0);
        public Color AdditiveColor
        {
            get { return m_additiveColor; }
            set { m_additiveColor = value; }
        }

        protected Vector4 m_bsc = Vector4.zero;
        public Vector4 Bsc
        {
            get { return m_bsc; }
            set { m_bsc = value; }
        }

        protected Color m_groupColor = new Color(0.808f, 0.808f, 0.808f, 1);
        public Color GroupColor {
            get { return m_groupColor; }
            set { m_groupColor = value; }
        }

        //protected float m_alphaVal;
        //public float AlphaVal {
        //    get { return m_alphaVal; }
        //    set { m_alphaVal = value; }
        //}

        protected float m_openIce;
        public float OpenIce {
            get { return m_openIce; }
            set { m_openIce = value; }
        }

        public InstancingObjData()
        {
            Animator = null;
        }

        public InstancingObjData(GpuSkinAnimator animator, Color addColor, Vector4 bsc, Color groupColor)
        {
            Animator = animator;
            m_additiveColor = addColor;
            m_bsc = bsc;
            m_groupColor = groupColor;
            //m_alphaVal = alphaVal;
            m_openIce = 0;
        }
        public void Dispose()
        {
            if (null != Animator)
            {
                UnityEngine.Object.Destroy(Animator);
                Animator = null;
            }
        }
    }

    /// <summary>
    /// 一次DrawMeshInstance绘制的对象数据
    /// </summary>
    public class InstancingBatchData : IDisposable
    {
        //一组相同的GameObjects有Mesh[]、Property、Material
        public Mesh[] SkinMeshes;
        public MaterialPropertyBlock PropertyBlock;
        public Material SharedMaterial;
        private Texture m_animationTexture;
        private int m_blockWidth;
        private int m_blockHeight;
        private int m_textureWidth;
        private int m_textureHeight;
        private float m_alphaVal;

        //key: instanceId
        private Dictionary<int, InstancingObjData> m_instancingObjDic = new Dictionary<int, InstancingObjData>();

        //缓存批渲染的数据
        protected Matrix4x4[][] m_tempMatrixArray;
        public Matrix4x4[][] TempMatrixArray {
            get { return m_tempMatrixArray; }
        }

        protected float[][] m_tempFrameIndexArray;
        public float[][] TempFrameIndexArray {
            get { return m_tempFrameIndexArray; }
        }

        protected Color[][] m_tempAddColorArray;
        public Color[][] TempAddColorArray {
            get { return m_tempAddColorArray; }
        }

        protected Vector4[][] m_tempBscArray;
        public Vector4[][] TempBscArray {
            get { return m_tempBscArray; }
        }

        protected Color[][] m_tempGroupColorArray;
        public Color[][] TempGroupColorArray {
            get { return m_tempGroupColorArray; }
        }

        protected float[][] m_tempOpenIceArray;
        public float[][] TempOpenIceArray {
            get { return m_tempOpenIceArray; }
        }

        protected bool m_isDataChange;

        public InstancingBatchData(Mesh[] skinMeshes, MaterialPropertyBlock block, Material material)
        {
            m_isDataChange = false;
            SkinMeshes = skinMeshes;
            PropertyBlock = block;
            SharedMaterial = material;
            if (material.HasProperty("_AlphaVal"))
            {
                m_alphaVal = material.GetFloat("_AlphaVal");
            }
            //initialize SetFloatArray size
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.FrameIndexProId, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.AddColorProIdA, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.AddColorProIdR, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.AddColorProIdG, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.AddColorProIdB, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.ProIdB, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.ProIdS, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.ProIdC, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.ProIdEnableBsc, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.GroupProIdR, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.GroupProIdG, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.GroupProIdB, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            //PropertyBlock.SetFloatArray(GpuInstancingMgr.Inst.AlphaValueProId, new float[GpuInstancingMgr.InstancingSizePerBatch]);
            PropertyBlock.SetFloatArray(GpuInstancingMgr.instance.OpenIceProId, new float[GpuInstancingMgr.InstancingSizePerBatch]);
        }

        public int InstancingObjCount {
            get { return m_instancingObjDic.Count; }
        }

        public void SetMaterialProperty(Texture tex, int blockWidth, int blockHeight, int textureWidth, int textureHeight)
        {
            m_animationTexture = tex;
            m_blockHeight = blockHeight;
            m_blockWidth = blockWidth;
            m_textureWidth = textureWidth;
            m_textureHeight = textureHeight;
        }

        public void PrepareMaterial()
        {
            if (null != SharedMaterial && null != m_animationTexture)
            {
                SharedMaterial.SetTexture("BoneTexture", m_animationTexture);
                SharedMaterial.SetInt("BlockWidth", m_blockWidth);
                SharedMaterial.SetInt("BlockHeight", m_blockHeight);
                SharedMaterial.SetInt("BoneTextureWidth", m_textureWidth);
                SharedMaterial.SetInt("BoneTextureHeight", m_textureHeight);
                SharedMaterial.SetFloat("_AlphaVal", m_alphaVal);
            }
        }

        /// <summary>
        /// dispose是在m_instancingObjDic为空的时候调用
        /// </summary>
        public void Dispose()
        {
            m_isDataChange = false;
            SkinMeshes = null;
            PropertyBlock = null;
            SharedMaterial = null;
            m_tempOpenIceArray = null;
            m_tempMatrixArray = null;
            m_tempGroupColorArray = null;
            m_tempFrameIndexArray = null;
            m_tempBscArray = null;
            m_tempAddColorArray = null;
            //m_animationTexture = null;
            foreach (KeyValuePair<int, InstancingObjData> item in m_instancingObjDic)
            {
                item.Value.Dispose();
            }
            m_instancingObjDic.Clear();
        }

        public void UpdateRenderPropertyArray()
        {
            if (m_isDataChange)
            {
                GenerateRenderPropertyArray();
            }

            int index = 0;
            foreach (KeyValuePair<int, InstancingObjData> item in m_instancingObjDic)
            {
                GpuSkinAnimator animator = item.Value.Animator;
                int x = index / GpuInstancingMgr.InstancingSizePerBatch;
                int y = index % GpuInstancingMgr.InstancingSizePerBatch;

                m_tempMatrixArray[x][y] = animator.transform.localToWorldMatrix;

                m_tempFrameIndexArray[x][y] = animator.FrameIndex;

                m_tempAddColorArray[x][y] = item.Value.AdditiveColor;

                m_tempBscArray[x][y] = item.Value.Bsc;

                m_tempGroupColorArray[x][y] = item.Value.GroupColor;

                //alphaValueArray[x][y] = item.Value.AlphaVal;

                m_tempOpenIceArray[x][y] = item.Value.OpenIce;

                index++;
            }
        }

        /// <summary>
        /// 每个渲染对象的矩阵、当前动画帧、叠加颜色、BSC
        /// 
        /// 一个批次包装为一个数组，一个批次的大小为InstancingSizePerBatch
        /// </summary>
        protected void GenerateRenderPropertyArray()
        {
            if (m_instancingObjDic.Count <= 0)
            {
                m_tempAddColorArray = null;
                m_tempBscArray = null;
                m_tempFrameIndexArray = null;
                m_tempGroupColorArray = null;
                m_tempMatrixArray = null;
                m_tempOpenIceArray = null;
                return;
            }
            int size = Mathf.CeilToInt((float)m_instancingObjDic.Count / GpuInstancingMgr.InstancingSizePerBatch);

            m_tempMatrixArray = new Matrix4x4[size][];
            m_tempFrameIndexArray = new float[size][];
            m_tempAddColorArray = new Color[size][];
            m_tempBscArray = new Vector4[size][];
            m_tempGroupColorArray = new Color[size][];
            //alphaValueArray = new float[size][];
            m_tempOpenIceArray = new float[size][];

            int totalCount = m_instancingObjDic.Count;
            for (int i = 0; i < size; i++)
            {
                int left = totalCount - i * GpuInstancingMgr.InstancingSizePerBatch;
                int subCount = left >= GpuInstancingMgr.InstancingSizePerBatch ? GpuInstancingMgr.InstancingSizePerBatch : left;

                m_tempMatrixArray[i] = new Matrix4x4[subCount];
                m_tempFrameIndexArray[i] = new float[subCount];
                m_tempAddColorArray[i] = new Color[subCount];
                m_tempBscArray[i] = new Vector4[subCount];
                m_tempGroupColorArray[i] = new Color[subCount];
                //alphaValueArray[i] = new float[subCount];
                m_tempOpenIceArray[i] = new float[subCount];
            }

            m_isDataChange = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">instanceId</param>
        /// <returns></returns>
        public bool ContainInstancingObj(int key)
        {
            return m_instancingObjDic.ContainsKey(key);
        }

        public bool TryGetInstancingObj(int instanceId, out InstancingObjData data)
        {
            return m_instancingObjDic.TryGetValue(instanceId, out data);
        }

        public bool AddInstancingObj(GpuSkinAnimator animator, Color addColor, Vector4 bsc, Color groupColor)
        {
            if (null == animator)
            {
                return false;
            }
            int instanceId = animator.GetInstanceID();
            if (m_instancingObjDic.ContainsKey(instanceId))
            {
                return false;
            }
            m_instancingObjDic.Add(instanceId, new InstancingObjData(animator, addColor, bsc, groupColor));
            m_isDataChange = true;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">instanceId</param>
        /// <returns></returns>
        public bool RemoveInstancingObj(int key)
        {
            bool result = m_instancingObjDic.Remove(key);
            m_isDataChange = true;
            return result;
        }
    }
}
