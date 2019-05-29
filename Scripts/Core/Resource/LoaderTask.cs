using IGG.Core.Manager;
using IGG.Utility;
using System.Collections.Generic;

namespace IGG.Core.Resource
{
    public class LoaderTask
    {
        /// <summary>
        /// 加载队列数
        /// </summary>
        private const int kMaxGroupCount = 10;

        /// <summary>
        /// 回调信息列表
        /// </summary>
        private readonly List<AsyncCallbackInfo> m_AsyncCallbackInfos = new List<AsyncCallbackInfo>();

        /// <summary>
        /// 进行中的加载组
        /// </summary>
        private readonly List<LoaderGroup> m_ListLoaderGroups = new List<LoaderGroup>();

        /// <summary>
        /// 进行中的加载器列表
        /// </summary>
        private readonly Dictionary<string, LoaderData> m_DicLoaderDatas = new Dictionary<string, LoaderData>();

        /// <summary>
        /// 等待中的加载组
        /// </summary>
        private readonly Dictionary<LoadManager.LoadPriority, Queue<LoaderGroup>> m_DicLoaderGroupWaits =
            new Dictionary<LoadManager.LoadPriority, Queue<LoaderGroup>>();

        public LoaderTask()
        {
            for (int i = 0; i < (int)LoadManager.LoadPriority.Quantity; ++i)
            {
                m_DicLoaderGroupWaits.Add((LoadManager.LoadPriority)i, new Queue<LoaderGroup>());
            }
        }

        public void Clear()
        {
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            UpdateGroup();
            UpdateAsyncCallback();
        }

        /// <summary>
        /// 归还加载器
        /// </summary>
        /// <param name="loader">加载器</param>
        public void ReleaseLoader(Loader loader)
        {
            if (!m_DicLoaderDatas.TryGetValue(loader.path, out LoaderData loaderData))
            {
                return;
            }

            if (--loaderData.referenceCount > 0)
            {
                return;
            }

            m_DicLoaderDatas.Remove(loader.path);

            LoaderDataPool.Release(loaderData);
        }


        /// <summary>
        /// 获得加载器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="path">路径</param>
        /// <param name="param">附加参数</param>
        /// <param name="async">异步</param>
        /// <returns>加载器</returns>
        public Loader GetLoader(LoaderType type, string path, object param, bool async)
        {
            if (!m_DicLoaderDatas.TryGetValue(path, out LoaderData loaderData))
            {
                loaderData = LoaderDataPool.Get(type);

                if (loaderData == null)
                {
                    loaderData = new LoaderData
                    {
                        referenceCount = 0,
                        loader = LoaderPool.Get(type),
                        completeCallbacks = new List<LoadManager.CompleteCallback>()
                    };
                }

                loaderData.loader.Init(path, param, null, OnLoadCompleted, async);
                m_DicLoaderDatas.Add(path, loaderData);
            }

            if (!async)
            {
                loaderData.loader.async = false;
            }

            ++loaderData.referenceCount;
            return loaderData.loader;
        }

        /// <summary>
        /// 增加回调函数
        /// </summary>
        /// <param name="loader">加载器</param>
        /// <param name="completeCallback">回调</param>
        public void PushCallback(Loader loader, LoadManager.CompleteCallback completeCallback)
        {
            if (null == completeCallback)
            {
                return;
            }

            if (m_DicLoaderDatas.TryGetValue(loader.path, out LoaderData loaderData))
            {
                loaderData.completeCallbacks.Add(completeCallback);
            }
        }

        /// <summary>
        /// 加载完成回调
        /// </summary>
        /// <param name="loader">加载器</param>
        /// <param name="data">结果</param>
        private void OnLoadCompleted(Loader loader, object data)
        {
            if (!m_DicLoaderDatas.TryGetValue(loader.path, out LoaderData loaderData))
            {
                return;
            }

            for (int i = 0; i < loaderData.completeCallbacks.Count; ++i)
            {
                loaderData.completeCallbacks[i](data);
            }

            loaderData.completeCallbacks.Clear();
        }

        /// <summary>
        /// 获得加载组
        /// </summary>
        /// <returns></returns>
        public LoaderGroup PopGroup(LoadManager.LoadPriority priority = LoadManager.LoadPriority.Normal)
        {
            LoaderGroup group = LoaderGroupPool.Get(this);
            group.priority = priority;
            m_DicLoaderGroupWaits[priority].Enqueue(group);
            return group;
        }

        /// <summary>
        /// 开始下一组
        /// </summary>
        /// <returns></returns>
        private bool StartNextGroup()
        {
            LoaderGroup loaderGroup = null;
            for (int i = (int)LoadManager.LoadPriority.Quantity - 1; i >= 0; --i)
            {
                Queue<LoaderGroup> groups = m_DicLoaderGroupWaits[(LoadManager.LoadPriority)i];
                if (groups.Count == 0)
                {
                    continue;
                }

                loaderGroup = groups.Dequeue();
                break;
            }

            if (loaderGroup == null)
            {
                return false;
            }

            m_ListLoaderGroups.Add(loaderGroup);
            loaderGroup.Start();

            return true;
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void UpdateGroup()
        {
            int index = 0;
            while (index < m_ListLoaderGroups.Count)
            {
                LoaderGroup group = m_ListLoaderGroups[index];
                group.Update();

                if (group.isFinish)
                {
                    LoaderGroupPool.Release(group);
                    m_ListLoaderGroups.RemoveAt(index);
                }
                else
                {
                    ++index;
                }
            }

            while (m_ListLoaderGroups.Count < kMaxGroupCount)
            {
                if (!StartNextGroup())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 添加加载任务
        /// </summary>
        /// <param name="group">加载组</param>
        /// <param name="type">类型</param>
        /// <param name="path">路径</param>
        /// <param name="param">附加参数</param>
        /// <param name="completeCallback">回调</param>
        /// <param name="async">异步</param>
        /// <param name="priority">优先级</param>
        /// <param name="insert">插入</param>
        /// <returns></returns>
        public void AddLoadTask(LoaderGroup group, LoaderType type, string path, object param,
                                LoadManager.LoaderGroupCompleteCallback completeCallback, bool async,
                                LoadManager.LoadPriority priority = LoadManager.LoadPriority.Normal, bool insert = false)
        {
            if (!async)
            {
                Loader loader = GetLoader(type, path, param, false);
                PushCallback(loader, (data) =>
                {
                    completeCallback?.Invoke(group, data);
                });
                loader.Start();

                ReleaseLoader(loader);

                return;
            }

            if (group == null)
            {
                group = PopGroup(priority);
            }

            group.Add(type, path, param, completeCallback, true, insert);
        }

        /// <summary>
        /// 增加异步回调
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="group">加载组</param>
        /// <param name="data">资源对象</param>
        public void AddAsyncCallback(LoadManager.LoaderGroupCompleteCallback callback, LoaderGroup group, object data)
        {
            if (null == callback)
            {
                return;
            }

            AsyncCallbackInfo info = new AsyncCallbackInfo
            {
                completeCallback = callback,
                Group = group,
                Data = data
            };

            m_AsyncCallbackInfos.Add(info);
        }

        /// <summary>
        /// 更新异步回调
        /// </summary>
        private void UpdateAsyncCallback()
        {
            for (int i = 0; i < m_AsyncCallbackInfos.Count; ++i)
            {
                AsyncCallbackInfo info = m_AsyncCallbackInfos[i];
                info.completeCallback(info.Group, info.Data);
            }

            m_AsyncCallbackInfos.Clear();
        }
    }
}
