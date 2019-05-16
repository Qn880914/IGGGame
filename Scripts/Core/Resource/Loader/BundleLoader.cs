using IGG.Core.Manager;
using System;
using UnityEngine;

namespace IGG.Core.Resource
{
    public class BundleLoader : Loader
    {
        private AssetBundleCreateRequest m_AssetBundleCreateRequest;

        private bool m_NeedUnpack;
        private int m_StageCount;
        private int m_StageCurrent;
        private LzmaCompressRequest m_UnpackRequest;

        public BundleLoader()
            : base(LoaderType.Bundle)
        {
            m_StageCurrent = 0;
            m_StageCount = 1;
        }

        public override void Reset()
        {
            base.Reset();

            m_AssetBundleCreateRequest = null;
            m_UnpackRequest = null;

            m_StageCurrent = 0;
            m_StageCount = 1;
        }

        public override void Start()
        {
            base.Start();

            string path = LoadManager.instance.GetResourcePath(this.path);
            if (string.IsNullOrEmpty(path))
            {
                OnLoaded(null);
                return;
            }

            m_NeedUnpack = ConstantData.EnableCustomCompress && path.Contains(ConstantData.StreamingAssetsPath);

            if (async)
            {
                if (m_NeedUnpack)
                {
                    m_StageCount = 2;

                    byte[] bytes = FileUtil.ReadByteFromFile(path);
                    m_UnpackRequest = LzmaCompressRequest.CreateDecompress(bytes);
                }
                else
                {
                    m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
                }
            }
            else
            {
                AssetBundle ab = null;
                try
                {
                    if (m_NeedUnpack)
                    {
                        byte[] bytes = FileUtil.ReadByteFromFile(path);
                        bytes = Unpack(bytes);
                        ab = AssetBundle.LoadFromMemory(bytes);
                    }
                    else
                    {
                        ab = AssetBundle.LoadFromFile(path);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                finally
                {
                    OnLoaded(ab);
                }
            }
        }

        private byte[] Unpack(byte[] bytes)
        {
            byte[] result = new byte[1];
            int size = LzmaHelper.Uncompress(bytes, ref result);
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
                        OnLoaded(m_AssetBundleCreateRequest.assetBundle);
                    }
                    else
                    {
                        DoProgress(m_AssetBundleCreateRequest.progress);
                    }
                }
                else if (m_UnpackRequest != null)
                {
                    if (m_UnpackRequest.IsDone)
                    {
                        ++m_StageCurrent;
                        m_AssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(m_UnpackRequest.Bytes);

                        m_UnpackRequest.Dispose();
                        m_UnpackRequest = null;
                    }
                    else
                    {
                        DoProgress(m_UnpackRequest.Progress);
                    }
                }
            }
        }

        private void DoProgress(float rate)
        {
            OnProgress((m_StageCurrent + rate) / m_StageCount);
        }

        private void OnLoaded(AssetBundle ab)
        {
            //Logger.Log(string.Format("BundlLoader {0} - {1} use {2}ms", Path, IsAsync, m_watch.ElapsedMilliseconds));
            OnComplete(ab);
        }
    }
}
