using IGG.Core.Resource;

namespace IGG.Utility
{
    public class LoaderDataPool
    {
        private static readonly ObjectPool<LoaderData> s_AssetLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_BundleAssetLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_BundleLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_CacheBundleLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_ResourceLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_SceneLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());
        private static readonly ObjectPool<LoaderData> s_StreamLoaderDataPool = new ObjectPool<LoaderData>(null, loaderData => loaderData.loader.Reset());

        public static LoaderData Get(LoaderType type)
        {
            LoaderData loaderData = null;
            switch (type)
            {
                case LoaderType.Asset:
                    loaderData = s_AssetLoaderDataPool.Get();
                    break;
                case LoaderType.BundleAsset:
                    loaderData = s_BundleAssetLoaderDataPool.Get();
                    break;
                case LoaderType.Bundle:
                    if (ConstantData.EnableAssetBundle)
                        loaderData = s_CacheBundleLoaderDataPool.Get();
                    else
                        loaderData = s_BundleLoaderDataPool.Get();
                    break;
                case LoaderType.Resource:
                    loaderData = s_ResourceLoaderDataPool.Get();
                    break;
                case LoaderType.Scene:
                    loaderData = s_SceneLoaderDataPool.Get();
                    break;
                case LoaderType.Stream:
                    loaderData = s_StreamLoaderDataPool.Get();
                    break;
            }

            if(null != loaderData && null == loaderData.loader)
            {
                loaderData.loader = LoaderPool.Get(type);
            }

            return loaderData;
        }

        public static void Release(LoaderData loaderData)
        {
            switch (loaderData.loader.type)
            {
                case LoaderType.Asset:
                    s_AssetLoaderDataPool.Release(loaderData);
                    break;
                case LoaderType.BundleAsset:
                    s_BundleAssetLoaderDataPool.Release(loaderData);
                    break;
                case LoaderType.Bundle:
                    if (ConstantData.EnableAssetBundle)
                        s_CacheBundleLoaderDataPool.Release(loaderData);
                    else
                        s_BundleLoaderDataPool.Release(loaderData);
                    break;
                case LoaderType.Resource:
                    s_ResourceLoaderDataPool.Release(loaderData);
                    break;
                case LoaderType.Scene:
                    s_SceneLoaderDataPool.Release(loaderData);
                    break;
                case LoaderType.Stream:
                    s_StreamLoaderDataPool.Release(loaderData);
                    break;
            }
        }
    }
}
