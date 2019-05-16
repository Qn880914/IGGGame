using IGG.Core.Manager;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> AssetBundle 加载器 </para>
    /// </summary>
    public class CacheBundleLoader : Loader
    {
        /// <summary>
        /// AssetBundle加载请求
        /// </summary>
        private UnityWebRequest m_UnityWebRequest;

        /// <summary>
        /// 构造
        /// </summary>
        public CacheBundleLoader() 
            : base(LoaderType.Bundle)
        { }

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            m_UnityWebRequest = null;
        }

        /// <summary>
        /// 开始加载
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (m_UnityWebRequest != null)
            {
                m_UnityWebRequest.Dispose();
                m_UnityWebRequest = null;
            }

            bool isCache = false;
            string path = LoadManager.instance.GetResourcePath(this.path, ref isCache, async);
            // IGG.Logging.Logger.Log(string.Format("-->CacheBundleLoader {0} - {1}:{2}", Path, path, IsAsync));
            if (async)
            {
                string pathDst = FileUtil.GetAssetBundleFullPath(path);
                m_UnityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(pathDst, FileUtil.DefaultHash);
            }
            else
            {
                if (isCache)
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(path);
                    OnLoaded(ab);
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
                            LoadManager.instance.AddToDownloadOrCache(path);
                        }
                    }

                    AssetBundle ab = AssetBundle.LoadFromFile(pathDst);
                    OnLoaded(ab);
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            if (state != LoaderState.Loading || null == m_UnityWebRequest)
                return;

            if (m_UnityWebRequest.isDone)
            {
                OnLoaded((m_UnityWebRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle);
            }
            else
            {
                OnProgress(m_UnityWebRequest.downloadProgress);
            }
        }

        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="ab"></param>
        private void OnLoaded(AssetBundle ab)
        {
            // IGG.Logging.Logger.Log(string.Format("NewBundlLoader {0} - {1} use {2}ms", Path, IsAsync, m_watch.ElapsedMilliseconds));
            OnComplete(ab);
        }
    }
}
