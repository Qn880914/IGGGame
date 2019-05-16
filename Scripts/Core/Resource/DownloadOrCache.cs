#region Namespace

using System;
using System.Collections.Generic;
using UnityEngine.Networking;

#endregion

namespace IGG.Core.Resource
{
    public class DownloadOrCache
    {
        private readonly List<string> m_files = new List<string>();
        private readonly List<UnityWebRequestAsyncOperation> m_requests = new List<UnityWebRequestAsyncOperation>();

        private readonly int m_thread;
        private Action m_onCompleted;
        private Action<int, int, int> m_onProgress;
        private float m_progress;
        private int m_total;

        public DownloadOrCache(int thread = 1, List<string> files = null, Action<int, int, int> onProgress = null,
            Action onCompleted = null)
        {
            m_onProgress = onProgress;
            m_onCompleted = onCompleted;

            m_thread = thread;

            if (files != null)
            {
                m_files.AddRange(files);
            }

            m_total = m_files.Count;
        }

        public bool IsFinish { get; private set; }

        public void Add(string path)
        {
            if (m_files.Contains(path))
            {
                return;
            }

            ++m_total;
            m_files.Add(path);

            IsFinish = false;
        }

        public void Update()
        {
            if (IsFinish)
            {
                return;
            }

            m_progress = 0;

            int index = 0;
            while (index < m_requests.Count)
            {
                UnityWebRequestAsyncOperation request = m_requests[index];
                // Logging.Logger.Log(string.Format("--> {0} {1}", request.progress, m_progress));

                if (request.isDone)
                {
                    if (request.webRequest != null)
                    {
                        request.webRequest.Dispose();
                    }

                    m_requests.RemoveAt(index);
                }
                else
                {
                    m_progress += request.progress;
                    ++index;
                }
            }

            OnProgress(m_progress);

            if (m_files.Count > 0)
            {
                while (m_requests.Count < m_thread)
                {
                    string path = m_files[0];
                    m_files.RemoveAt(0);

                    UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path, FileUtil.DefaultHash, 0);
                    m_requests.Add(request.SendWebRequest());

                    if (m_files.Count == 0)
                    {
                        break;
                    }
                }
            }
            else if (m_requests.Count == 0)
            {
                // 完成
                OnCompleted();
            }
        }

        private void OnProgress(float rate)
        {
            if (m_onProgress != null)
            {
                int finish = m_total - (m_files.Count + m_requests.Count);
                int progress = (int) ((finish + rate) * 100f / m_total);

                // Logging.Logger.Log(string.Format("-->OnProgress {0} {1}, {2} {3} {4}", rate, progress, m_files.Count, m_requests.Count, m_total));
                m_onProgress(progress, finish, m_total);
            }
        }

        private void OnCompleted()
        {
            OnProgress(0f);

            if (m_onCompleted != null)
            {
                m_onCompleted();
            }

            m_onProgress = null;
            m_onCompleted = null;

            IsFinish = true;
        }
    }
}