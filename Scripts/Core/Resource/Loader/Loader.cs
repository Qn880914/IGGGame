using System.Diagnostics;

namespace IGG.Core.Resource
{
    public class Loader
    {
        public delegate void CompleteCallback(Loader loader, object data);

        public delegate void ProgressCallback(Loader loader, float progress);

        protected readonly Stopwatch m_StopWatch = new Stopwatch();

        private CompleteCallback m_CompleteCallback;

        private ProgressCallback m_ProgressCallback;

        public LoaderType type { get; private set; }

        public LoaderState state { get; private set; }

        public string path { get; private set; }

        protected object param { get; private set; }

        public bool isCompleted { get { return state == LoaderState.Complete; } }

        public object data { get; private set; }

        public bool async { get; set; }
        
        protected Loader(LoaderType type)
        {
            this.type = type;
        }

        public void Init(string path, object param, ProgressCallback progressCallback, CompleteCallback completeCallback, bool async = true)
        {
            this.path = path;
            this.param = param;
            this.m_ProgressCallback = progressCallback;
            this.m_CompleteCallback = completeCallback;
            this.async = async;
        }

        public virtual void Start()
        {
            m_StopWatch.Reset();
            m_StopWatch.Start();

            state = LoaderState.Loading;
            OnProgress(0);
        }

        public virtual void Update()
        { }

        protected void OnProgress(float progress)
        {
            m_ProgressCallback?.Invoke(this, progress);
        }

        protected void OnComplete(object data)
        {
            state = LoaderState.Complete;
            this.data = data;

            m_CompleteCallback?.Invoke(this, data);

            OnProgress(1);
        }

        protected void OnLoadFail()
        {
            UnityEngine.Debug.LogErrorFormat("Loader Type:  {0}    path  : {1}", type, path);
            OnComplete(null);
        }

        public virtual void Reset()
        {
            state = LoaderState.None;

            path = "";
            param = null;
            async = false;
            data = null;

            m_ProgressCallback = null;
            m_CompleteCallback = null;
        }
    }
}
