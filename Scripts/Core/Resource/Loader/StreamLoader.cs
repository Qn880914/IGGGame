using System;
using UnityEngine.Networking;

namespace IGG.Core.Resource
{
    public class StreamLoader : Loader
    {
        private UnityWebRequest m_UnityWebRequest;

        public StreamLoader()
            : base(LoaderType.Stream)
        {
        }

        public override void Start()
        {
            base.Start();

            if (async)
            {
                string fullPath = this.path;

                bool hasHead = (bool)param;
                if (!hasHead)
                {
                    bool addFileHead = true;

#if UNITY_ANDROID && !UNITY_EDITOR
                    // 如果是读取apk里的资源,不需要加file:///,其它情况都要加
                    if (path.Contains (Application.streamingAssetsPath)) {
                        addFileHead = false;
                    }
#endif
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (addFileHead)
                    {
                        fullPath = string.Format("file:///{0}", path);
                    }
                }

                m_UnityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(fullPath);
            }
            else
            {
                object data = null;
                try
                {
                    data = FileUtil.ReadByteFromFile(path);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                finally
                {
                    OnComplete(data);
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            if (state == LoaderState.Loading)
            {
                if (m_UnityWebRequest == null)
                {
                    OnFailed();
                }
                else if (!string.IsNullOrEmpty(m_UnityWebRequest.error))
                {
                    UnityEngine.Debug.LogError(m_UnityWebRequest.error);
                    OnFailed();
                }
                else if (m_UnityWebRequest.isDone)
                {
                    OnComplete(m_UnityWebRequest.downloadedBytes);
                }
                else
                {
                    OnProgress(m_UnityWebRequest.downloadProgress);
                }
            }
        }

        public override void Reset()
        {
            base.Reset();

            if (m_UnityWebRequest != null)
            {
                m_UnityWebRequest.Dispose();
                m_UnityWebRequest = null;
            }
        }
    }
}
