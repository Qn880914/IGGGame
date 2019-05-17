using UnityEngine;

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> Resources file loader </para>
    ///     Note : use for load file that under the Resources folder
    /// </summary>
    public class ResourceLoader : Loader
    {
        private ResourceRequest m_ResourceRequest;

        public ResourceLoader()
            : base(LoaderType.Resource)
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
                OnFailed();
            else if (m_ResourceRequest.isDone)
            {
                OnComplete(m_ResourceRequest.asset);
                m_ResourceRequest = null;
            }
            else
                OnProgress(m_ResourceRequest.progress);
        }

        public override void Reset()
        {
            m_ResourceRequest = null;
            base.Reset();
        }
    }
}
