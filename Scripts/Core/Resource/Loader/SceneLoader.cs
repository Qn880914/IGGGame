using UnityEngine;
using UnityEngine.SceneManagement;

namespace IGG.Core.Resource
{
    public class SceneLoader : Loader
    {
        private AsyncOperation m_AsyncOperation;

        public SceneLoader()
            : base(LoaderType.Scene)
        {
        }

        public override void Start()
        {
            base.Start();

            LoadSceneMode mode = LoadSceneMode.Single;
            if (param != null)
            {
                bool additive = (bool)param;
                if (additive)
                {
                    mode = LoadSceneMode.Additive;
                }
            }

            if (async)
            {
                m_AsyncOperation = SceneManager.LoadSceneAsync(path, mode);
            }
            else
            {
                SceneManager.LoadScene(path, mode);
                OnComplete(true);
            }
        }

        public override void Update()
        {
            if (state != LoaderState.Loading)
                return;

            if (m_AsyncOperation == null)
            {
                OnComplete(false);
            }
            else if (m_AsyncOperation.isDone)
            {
                OnComplete(true);
                m_AsyncOperation = null;
            }
            else
            {
                OnProgress(m_AsyncOperation.progress);
            }
        }
    }
}
