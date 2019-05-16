using IGG.Core.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.Core.Resource
{
    public class LoaderGroup : MonoBehaviour
    {
        /// <summary>
        /// 加载列表
        /// </summary>
        private readonly List<LoaderInfo> m_loaders = new List<LoaderInfo>();

        /// <summary>
        ///  加载任务
        /// </summary>
        private readonly LoaderTask m_task;

        /// <summary>
        /// 当前加载器
        /// </summary>
        private LoaderInfo m_loader;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="task">加载任务</param>
        public LoaderGroup(LoaderTask task)
        {
            m_task = task;
        }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsFinish { get; private set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public LoadManager.LoadPriority Priority { get; set; }

        /// <summary>
        /// 加载中
        /// </summary>
        public bool InLoading { get; private set; }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            IsFinish = false;
            InLoading = false;

            m_loader = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (m_loader != null)
            {
                m_loader.Loader.Update();
                if (m_loader.Loader.isCompleted)
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
            InLoading = true;
            LoadNext();
        }

        /// <summary>
        /// 加载下一个
        /// </summary>
        private void LoadNext()
        {
            if (m_loader != null)
            {
                m_task.PushLoader(m_loader.Loader);
                m_loader = null;
            }

            if (m_loaders.Count == 0)
            {
                IsFinish = true;
                return;
            }

            m_loader = m_loaders[0];
            m_loaders.RemoveAt(0);

            switch (m_loader.Loader.state)
            {
                case LoaderState.None:
                    PushCallback();
                    m_loader.Loader.Start();
                    break;
                case LoaderState.Loading:
                    PushCallback();
                    break;
                case LoaderState.Complete:
                    LoadCompleted(m_loader, m_loader.Loader.data);
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
        /// <param name="onLoaded">回调</param>
        /// <param name="async">异步</param>
        /// <param name="insert">插队</param>
        public void Add(LoaderType type, string path, object param, LoadManager.GroupLoadedCallback onLoaded,
                        bool async, bool insert)
        {
            LoaderInfo loader = new LoaderInfo
            {
                Loader = m_task.PopLoader(type, path, param, async),
                Callback = onLoaded
            };

            if (insert)
            {
                m_loaders.Insert(0, loader);
            }
            else
            {
                m_loaders.Add(loader);
            }

            if (InLoading && IsFinish)
            {
                IsFinish = false;
                LoadNext();
            }
        }

        /// <summary>
        /// 添加回调
        /// </summary>
        private void PushCallback()
        {
            m_task.PushCallback(m_loader.Loader, (data) => { LoadCompleted(m_loader, data); });
        }

        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="info">加载器信息</param>
        /// <param name="data">加载结果</param>
        private void LoadCompleted(LoaderInfo info, object data)
        {
            if (info == null || info.Callback == null)
            {
                return;
            }

            info.Callback(this, data);
        }

        /// <summary>
        /// 加载信息
        /// </summary>
        private class LoaderInfo
        {
            /// <summary>
            /// 回调
            /// </summary>
            public LoadManager.GroupLoadedCallback Callback;

            /// <summary>
            /// 加载器
            /// </summary>
            public Loader Loader;
        }
    }
}

