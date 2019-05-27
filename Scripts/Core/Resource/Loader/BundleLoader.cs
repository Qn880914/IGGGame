using IGG.Core.Manager;
using System;
using UnityEngine;

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> assetbundle loader </para>
    /// </summary>
    public class BundleLoader : Loader
    {
        private AssetBundleCreateRequest m_AssetBundleCreateRequest;

        private bool m_NeedDecompress;
        private int m_StageCount;
        private int m_StageCurrent;
        private LzmaCompressRequest m_LzmaCompressRequest;

        public BundleLoader()
            : base(LoaderType.Bundle)
        {
            m_StageCurrent = 0;
            m_StageCount = 1;
        }

        public override void Start()
        {
            base.Start();

            string path = LoadManager.instance.GetResourcePath(this.path);
            if (string.IsNullOrEmpty(path))
            {
                OnFailed();
                return;
            }

            m_NeedDecompress = ConstantData.enableCustomCompress && path.Contains(ConstantData.StreamingAssetsPath);

            if (async)
            {
                if (m_NeedDecompress)
                {
                    m_StageCount = 2;

                    byte[] bytes = FileUtil.ReadByteFromFile(path);
                    m_LzmaCompressRequest = LzmaCompressRequest.CreateDecompress(bytes);
                }
                else
                {
                    m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
                }
            }
            else
            {
                AssetBundle assetbundle = null;
                try
                {
                    if (m_NeedDecompress)
                    {
                        byte[] bytes = FileUtil.ReadByteFromFile(path);
                        bytes = Decompress(bytes);
                        assetbundle = AssetBundle.LoadFromMemory(bytes);
                    }
                    else
                    {
                        assetbundle = AssetBundle.LoadFromFile(path);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                finally
                {
                    OnComplete(assetbundle);
                }
            }
        }

        private byte[] Decompress(byte[] bytes)
        {
            byte[] result = new byte[1];
            int size = LzmaHelper.Decompress(bytes, ref result);
            if (size == 0)
            {
                UnityEngine.Debug.LogError("Uncompress Failed");
                return null;
            }

            return result;
        }

        public override void Update()
        {
            if (state == LoaderState.Loading)
            {
                if (m_AssetBundleCreateRequest != null)
                {
                    if (m_AssetBundleCreateRequest.isDone)
                    {
                        ++m_StageCurrent;
                        OnComplete(m_AssetBundleCreateRequest.assetBundle);
                    }
                    else
                    {
                        OnProgress(m_AssetBundleCreateRequest.progress);
                    }
                }
                else if (m_LzmaCompressRequest != null)
                {
                    if (m_LzmaCompressRequest.isDone)
                    {
                        ++m_StageCurrent;
                        m_AssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(m_LzmaCompressRequest.bytes);

                        m_LzmaCompressRequest.Dispose();
                        m_LzmaCompressRequest = null;
                    }
                    else
                    {
                        OnProgress(m_LzmaCompressRequest.progress);
                    }
                }
            }
        }

        protected override void OnProgress(float progress)
        {
            base.OnProgress((m_StageCurrent + progress) / m_StageCount);
        }

        public override void Reset()
        {
            base.Reset();

            m_AssetBundleCreateRequest = null;
            m_LzmaCompressRequest = null;

            m_StageCurrent = 0;
            m_StageCount = 1;
        }
    }
}
