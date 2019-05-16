using UnityEngine;

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> Resources 文件加载器(加载Resources文件夹下的资源，使用Resources.Load)</para>
    /// </summary>
    public class ResourceLoader : Loader
    {
        private ResourceRequest m_ResourceRequest;

        public ResourceLoader() : base(LoaderType.Resource)
        { }

        public override void Start()
        {
            base.Start();

            if (async)
                m_ResourceRequest = Resources.LoadAsync(path);
            else
            {
                Object data = Resources.Load(path);
                OnComplete(data);
            }
        }

        public override void Update()
        {
            if (state != LoaderState.Loading)
                return;

            if (null == m_ResourceRequest)
                OnLoadFail();
            else if (m_ResourceRequest.isDone)
            {
                OnComplete(m_ResourceRequest.asset);
                m_ResourceRequest = null;
            }
            else
                OnProgress(m_ResourceRequest.progress);
        }
    }
}
