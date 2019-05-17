using IGG.Core.Resource;

namespace IGG.Utility
{
    public class LoaderGroupPool
    {
        private static readonly ObjectPool<LoaderGroup> s_LoaderGroupPool = new ObjectPool<LoaderGroup>(null, loaderGroup => loaderGroup.Reset());

        public static LoaderGroup Get(LoaderTask loaderTask)
        {
            LoaderGroup loaderGroup = s_LoaderGroupPool.Get();
            loaderGroup.loaderTask = loaderTask;
            return loaderGroup;
        }

        public static void Release(LoaderGroup loaderGroup)
        {
            s_LoaderGroupPool.Release(loaderGroup);
        }
    }
}

