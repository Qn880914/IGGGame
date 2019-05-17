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

            LoadSceneMode loadSceneMode = LoadSceneMode.Single;
            if (param != null)
            {
                bool additive = (bool)param;
                if (additive)
                {
                    loadSceneMode = LoadSceneMode.Additive;
                }
            }

            if (async)
            {
                m_AsyncOperation = SceneManager.LoadSceneAsync(path, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(path, loadSceneMode);
                OnComplete(true);
            }
        }

        public override void Update()
        {
            if (state != LoaderState.Loading)
                return;

            if (m_AsyncOperation == null)
            {
                OnFailed();
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
