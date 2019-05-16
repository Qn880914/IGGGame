using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IGG.Core.Resource
{
    public class BundleAssetLoadParam
    {
        public AssetBundle assetBundle;
        public Type type;
    }

    public class BundleAssetLoader : Loader
    {
        private AssetBundleRequest m_AssetBundleRequest;

        public BundleAssetLoader()
            : base(LoaderType.BundleAsset)
        {
        }

        public override void Reset()
        {
            base.Reset();

            m_AssetBundleRequest = null;
        }

        public override void Start()
        {
            base.Start();

            BundleAssetLoadParam param = this.param as BundleAssetLoadParam;
            if (param == null || param.assetBundle == null || param.type == null || string.IsNullOrEmpty(path))
            {
                OnLoaded(null);
                return;
            }

            if (async)
            {
                m_AssetBundleRequest = param.assetBundle.LoadAssetAsync(path, param.type);
            }
            else
            {
                Object asset = param.assetBundle.LoadAsset(path, param.type);
                OnLoaded(asset);
            }
        }

        public override void Update()
        {
            if (state != LoaderState.Loading)
            {
                return;
            }

            if (m_AssetBundleRequest.isDone)
            {
                OnLoaded(m_AssetBundleRequest.asset);
            }
            else
            {
                OnProgress(m_AssetBundleRequest.progress);
            }
        }

        private void OnLoaded(Object asset)
        {
            //Logger.Log(string.Format("BundleAssetLoader {0} - {1} use {2}ms", m_path, m_async, m_watch.ElapsedMilliseconds));

            OnComplete(asset);
        }
    }
}
