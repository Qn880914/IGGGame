using IGG.Core.Manager;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> AssetBundle loader </para>
    /// </summary>
    public class CacheBundleLoader : Loader
    {
        /// <summary>
        ///     <para> assetbundle load request </para>
        /// </summary>
        private UnityWebRequest m_UnityWebRequest;

        public CacheBundleLoader() 
            : base(LoaderType.Bundle)
        { }

        public override void Start()
        {
            base.Start();

            if (m_UnityWebRequest != null)
            {
                m_UnityWebRequest.Dispose();
                m_UnityWebRequest = null;
            }

            bool isCache = false;
            string path = ResourceManager.instance.GetResourcePath(this.path, ref isCache, async);
            if (async)
            {
                string pathDst = FileUtil.GetAssetBundleFullPath(path);
                m_UnityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(pathDst, FileUtil.DefaultHash);
            }
            else
            {
                if (isCache)
                {
                    AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
                    OnComplete(assetbundle);
                }
                else
                {
                    // FGUI有bug.造成LoginPanel适配有问题.临时处理,先去掉UI的缓存
                    string pathDst = path;
                    if (!path.StartsWith("ui/"))
                    {
                        string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                        pathDst = FileUtil.GetCacheAssetBundlePath(filename);
                        if (!File.Exists(pathDst))
                        {
                            // 不存在,读取源文本
                            pathDst = path;

                            // 加到后台解压
                            path = FileUtil.GetAssetBundleFullPath(path);
                            ResourceManager.instance.AddToDownloadOrCache(path);
                        }
                    }

                    AssetBundle assetbundle = AssetBundle.LoadFromFile(pathDst);
                    OnComplete(assetbundle);
                }
            }
        }

        public override void Update()
        {
            if (state != LoaderState.Loading || null == m_UnityWebRequest)
                return;

            if (m_UnityWebRequest.isDone)
            {
                OnComplete((m_UnityWebRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle);
            }
            else
            {
                OnProgress(m_UnityWebRequest.downloadProgress);
            }
        }

        public override void Reset()
        {
            base.Reset();

            m_UnityWebRequest = null;
        }
    }
}
