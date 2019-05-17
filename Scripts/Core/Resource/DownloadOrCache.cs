#region Namespace

using System;
using System.Collections.Generic;
using UnityEngine.Networking;

#endregion

namespace IGG.Core.Resource
{
    public class DownloadOrCache
    {
        private readonly List<string> m_ListFiles = new List<string>();
        private readonly List<UnityWebRequestAsyncOperation> m_ListWebRequestAsyncOpe = new List<UnityWebRequestAsyncOperation>();

        private readonly int m_Thread;
        private Action m_ActionComplete;
        private Action<int, int, int> m_ActionProgress;
        private float m_Progress;
        private int m_Total;

        public bool isFinish { get; private set; }

        public DownloadOrCache(int thread = 1, List<string> files = null, Action<int, int, int> actionProgress = null,
            Action actionComplete = null)
        {
            m_ActionProgress = actionProgress;
            m_ActionComplete = actionComplete;

            m_Thread = thread;

            if (files != null)
            {
                m_ListFiles.AddRange(files);
            }

            m_Total = m_ListFiles.Count;
        }

        public void Add(string path)
        {
            if (m_ListFiles.Contains(path))
            {
                return;
            }

            ++m_Total;
            m_ListFiles.Add(path);

            isFinish = false;
        }

        public void Update()
        {
            if (isFinish)
            {
                return;
            }

            m_Progress = 0;

            int index = 0;
            while (index < m_ListWebRequestAsyncOpe.Count)
            {
                UnityWebRequestAsyncOperation request = m_ListWebRequestAsyncOpe[index];

                if (request.isDone)
                {
                    if (request.webRequest != null)
                    {
                        request.webRequest.Dispose();
                    }

                    m_ListWebRequestAsyncOpe.RemoveAt(index);
                }
                else
                {
                    m_Progress += request.progress;
                    ++index;
                }
            }

            OnProgress(m_Progress);

            if (m_ListFiles.Count > 0)
            {
                while (m_ListWebRequestAsyncOpe.Count < m_Thread)
                {
                    string path = m_ListFiles[0];
                    m_ListFiles.RemoveAt(0);

                    UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path, FileUtil.DefaultHash, 0);
                    m_ListWebRequestAsyncOpe.Add(request.SendWebRequest());

                    if (m_ListFiles.Count == 0)
                    {
                        break;
                    }
                }
            }
            else if (m_ListWebRequestAsyncOpe.Count == 0)
            {
                // 完成
                OnCompleted();
            }
        }

        private void OnProgress(float rate)
        {
            if (null != m_ActionProgress)
            {
                int finish = m_Total - (m_ListFiles.Count + m_ListWebRequestAsyncOpe.Count);
                int progress = (int) ((finish + rate) * 100f / m_Total);

                m_ActionProgress(progress, finish, m_Total);
            }
        }

        private void OnCompleted()
        {
            OnProgress(0f);

            m_ActionComplete?.Invoke();

            m_ActionProgress = null;
            m_ActionComplete = null;

            isFinish = true;
        }
    }
}