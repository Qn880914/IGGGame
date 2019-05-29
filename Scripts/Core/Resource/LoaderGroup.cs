using IGG.Core.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.Core.Resource
{
    public class LoaderGroup : MonoBehaviour
    {
        /// <summary>
        /// 加载列表
        /// </summary>
        private readonly List<LoaderInfo> m_ListLoaderInfos = new List<LoaderInfo>();

        /// <summary>
        ///  加载任务
        /// </summary>
        private LoaderTask m_LoaderTask;
        public LoaderTask loaderTask { get { return m_LoaderTask; } set { m_LoaderTask = value; } }

        /// <summary>
        /// 当前加载器
        /// </summary>
        private LoaderInfo m_LoaderInfo;

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool isFinish { get; private set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public LoadManager.LoadPriority priority { get; set; }

        /// <summary>
        /// 加载中
        /// </summary>
        public bool isLoading { get; private set; }

        public LoaderGroup() { }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="task">加载任务</param>
        public LoaderGroup(LoaderTask task)
        {
            m_LoaderTask = task;
        }

        public void Inti(LoaderTask loaderTask)
        {
            m_LoaderTask = loaderTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (null != m_LoaderInfo)
            {
                m_LoaderInfo.loader.Update();
                if (m_LoaderInfo.loader.isCompleted)
                {
                    LoadNext();
                }
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            isLoading = true;
            LoadNext();
        }

        /// <summary>
        /// 加载下一个
        /// </summary>
        private void LoadNext()
        {
            if (null != m_LoaderInfo)
            {
                m_LoaderTask.ReleaseLoader(m_LoaderInfo.loader);
                m_LoaderInfo = null;
            }

            if (m_ListLoaderInfos.Count == 0)
            {
                isFinish = true;
                return;
            }

            m_LoaderInfo = m_ListLoaderInfos[0];
            m_ListLoaderInfos.RemoveAt(0);

            switch (m_LoaderInfo.loader.state)
            {
                case LoaderState.None:
                    PushCallback();
                    m_LoaderInfo.loader.Start();
                    break;
                case LoaderState.Loading:
                    PushCallback();
                    break;
                case LoaderState.Complete:
                    LoadCompleted(m_LoaderInfo, m_LoaderInfo.loader.data);
                    LoadNext();
                    break;
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="path">路径</param>
        /// <param name="param">附加参数</param>
        /// <param name="completeCallback">回调</param>
        /// <param name="async">异步</param>
        /// <param name="insert">插队</param>
        public void Add(LoaderType type, string path, object param, LoadManager.LoaderGroupCompleteCallback completeCallback,
                        bool async, bool insert)
        {
            LoaderInfo loaderInfo = new LoaderInfo
            {
                loader = m_LoaderTask.GetLoader(type, path, param, async),
                completeCallback = completeCallback
            };

            if (insert)
            {
                m_ListLoaderInfos.Insert(0, loaderInfo);
            }
            else
            {
                m_ListLoaderInfos.Add(loaderInfo);
            }

            if (isLoading && isFinish)
            {
                isFinish = false;
                LoadNext();
            }
        }

        /// <summary>
        /// 添加回调
        /// </summary>
        private void PushCallback()
        {
            m_LoaderTask.PushCallback(m_LoaderInfo.loader, (data) => { LoadCompleted(m_LoaderInfo, data); });
        }

        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="info">加载器信息</param>
        /// <param name="data">加载结果</param>
        private void LoadCompleted(LoaderInfo info, object data)
        {
            if (info == null)
            {
                return;
            }

            info.completeCallback?.Invoke(this, data);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            isFinish = false;
            isLoading = false;

            m_LoaderInfo = null;
        }

        /// <summary>
        /// 加载信息
        /// </summary>
        private class LoaderInfo
        {
            /// <summary>
            /// 回调
            /// </summary>
            public LoadManager.LoaderGroupCompleteCallback completeCallback;

            /// <summary>
            /// 加载器
            /// </summary>
            public Loader loader;
        }
    }
}

