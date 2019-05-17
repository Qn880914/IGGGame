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

        public override void Start()
        {
            base.Start();

            BundleAssetLoadParam param = this.param as BundleAssetLoadParam;
            if (param == null || param.assetBundle == null || param.type == null || string.IsNullOrEmpty(path))
            {
                OnFailed();
                return;
            }

            if (async)
            {
                m_AssetBundleRequest = param.assetBundle.LoadAssetAsync(path, param.type);
            }
            else
            {
                Object asset = param.assetBundle.LoadAsset(path, param.type);
                OnComplete(asset);
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
                OnComplete(m_AssetBundleRequest.asset);
            }
            else
            {
                OnProgress(m_AssetBundleRequest.progress);
            }
        }

        public override void Reset()
        {
            base.Reset();

            m_AssetBundleRequest = null;
        }
    }
}
