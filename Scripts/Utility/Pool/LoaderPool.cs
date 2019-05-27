using IGG.Core.Resource;

namespace IGG.Utility
{
    public class LoaderPool
    {
        private static readonly ObjectPool<AssetLoader> s_AssetLoaderPool = new ObjectPool<AssetLoader>(null, loader => loader.Reset());
        private static readonly ObjectPool<BundleAssetLoader> s_BundleAssetLoaderPool = new ObjectPool<BundleAssetLoader>(null, loader=>loader.Reset());
        private static readonly ObjectPool<BundleLoader> s_BundleLoaderPool = new ObjectPool<BundleLoader>(null, loader=>loader.Reset());
        private static readonly ObjectPool<CacheBundleLoader> s_CacheBundleLoaderPool = new ObjectPool<CacheBundleLoader>(null, loader=>loader.Reset());
        private static readonly ObjectPool<ResourceLoader> s_ResourceLoaderPool = new ObjectPool<ResourceLoader>(null, loader=>loader.Reset());
        private static readonly ObjectPool<SceneLoader> s_SceneLoaderPool = new ObjectPool<SceneLoader>(null, loader=>loader.Reset());
        private static readonly ObjectPool<StreamLoader> s_StreamLoaderPool = new ObjectPool<StreamLoader>(null, loader=>loader.Reset());


        public static Loader Get(LoaderType type)
        {
            Loader loader = null;
            switch (type)
            {
                case LoaderType.Asset:
                    loader = s_AssetLoaderPool.Get();
                    break;
                case LoaderType.BundleAsset:
                    loader = s_BundleAssetLoaderPool.Get();
                    break;
                case LoaderType.Bundle:
                    if (ConstantData.enableAssetBundle)
                        loader = s_CacheBundleLoaderPool.Get();
                    else
                        loader = s_BundleLoaderPool.Get();
                    break;
                case LoaderType.Resource:
                    loader = s_ResourceLoaderPool.Get();
                    break;
                case LoaderType.Scene:
                    loader = s_SceneLoaderPool.Get();
                    break;
                case LoaderType.Stream:
                    loader = s_StreamLoaderPool.Get();
                    break;
            }

            return loader;
        }

        public static void Release(Loader loader)
        {
            switch(loader.type)
            {
                case LoaderType.Asset:
                    s_AssetLoaderPool.Release(loader as AssetLoader);
                    break;
                case LoaderType.BundleAsset:
                    s_BundleAssetLoaderPool.Release(loader as BundleAssetLoader);
                    break;
                case LoaderType.Bundle:
                    if (ConstantData.enableAssetBundle)
                        s_CacheBundleLoaderPool.Release(loader as CacheBundleLoader);
                    else
                        s_BundleLoaderPool.Release(loader as BundleLoader);
                    break;
                case LoaderType.Resource:
                    s_ResourceLoaderPool.Release(loader as ResourceLoader);
                    break;
                case LoaderType.Scene:
                    s_SceneLoaderPool.Release(loader as SceneLoader);
                    break;
                case LoaderType.Stream:
                    s_StreamLoaderPool.Release(loader as StreamLoader);
                    break;
            }
        }
    }
}
