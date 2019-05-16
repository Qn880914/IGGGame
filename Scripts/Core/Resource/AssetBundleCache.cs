#region Namespace

using IGG.Core.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace IGG.Core.Resource
{
    /// <summary>
    /// Author  xuzhihui
    /// Date    2018.06.13
    /// Desc    AssetBundle缓存信息
    /// </summary>
    public class AssetBundleInfo
    {
        /// <summary>
        /// AssetBundle
        /// </summary>
        private AssetBundle m_bundle;

        /// <summary>
        /// AssetBundle名
        /// </summary>
        private string m_name;

        /// <summary>
        /// 常驻
        /// </summary>
        private bool m_persistent;

        /// <summary>
        /// 引用计数
        /// </summary>
        private int m_reference;

        /// <summary>
        /// 释放时间
        /// </summary>
        private float m_unloadTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">AssetBundle名</param>
        /// <param name="ab">AssetBundle对象</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="reference">初始引用计数</param>
        public AssetBundleInfo(string name, AssetBundle ab, bool persistent, int reference = 0)
        {
            m_name = name;
            m_persistent = persistent;
            m_reference = reference;

            Bundle = ab;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">AssetBundle名</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="reference">初始引用计数</param>
        public AssetBundleInfo(string name, bool persistent, int reference = 0)
        {
            m_name = name;
            m_persistent = persistent;
            m_reference = reference;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// AssetBundle对象
        /// </summary>
        public AssetBundle Bundle
        {
            get { return m_bundle; }
            set
            {
                if (value == null || m_bundle == value)
                {
                    return;
                }

                m_bundle = value;
            }
        }

        /// <summary>
        /// 是否常驻
        /// </summary>
        public bool Persistent
        {
            get { return m_persistent; }
            set { m_persistent = value; }
        }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferencedCount
        {
            get { return m_reference; }

            set
            {
                m_reference = value;
                if (IsCanRemove)
                {
                    if (ConstantData.AssetBundleCacheTime > 0)
                    {
                        m_unloadTime = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        Unload();
                    }
                }
                else
                {
                    m_unloadTime = 0;
                }
            }
        }

        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool IsCanRemove
        {
            get
            {
                if (!(!Persistent && ReferencedCount == 0))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 缓存时间到
        /// </summary>
        public bool IsTimeOut
        {
            get { return Time.realtimeSinceStartup - m_unloadTime >= ConstantData.AssetBundleCacheTime; }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public Object LoadAsset(string name, Type type)
        {
            if (Bundle == null)
            {
                return null;
            }

            return Bundle.LoadAsset(name, type);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="name">资源名</param>
        /// <returns>资源对象</returns>
        public T LoadAsset<T>(string name)where T : Object
        {
            if (Bundle == null)
            {
                return null;
            }

            return Bundle.LoadAsset<T>(name);
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public void Unload()
        {
            if (Bundle != null)
            {
                //Logger.Log(string.Format("-->Unload AssetBundle: {0}", m_name));
                Bundle.Unload(true);
                Bundle = null;
            }
        }
    }

    /// <summary>
    /// Author  xuzhihui
    /// Date    2018.12.29
    /// Desc    AssetBundle缓存
    /// </summary>
    public class AssetBundleCache
    {
        /// <summary>
        /// 缓存队列
        /// </summary>
        private readonly Dictionary<string, AssetBundleInfo> m_caches = new Dictionary<string, AssetBundleInfo>();

        /// <summary>
        /// 加载任务
        /// </summary>
        private readonly LoaderTask m_task;

        /// <summary>
        /// 上一次清除时间
        /// </summary>
        private float m_lastClear;

        public AssetBundleCache(LoaderTask task)
        {
            m_task = task;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (ConstantData.AssetBundleCacheTime > 0 &&
                Time.realtimeSinceStartup - m_lastClear >= ConstantData.AssetBundleCacheTime)
            {
                m_lastClear = Time.realtimeSinceStartup;
                Clear();
            }
        }

        /// <summary>
        /// 清除AssetBundle缓存
        /// </summary>
        /// <param name="onlyRefZero">只清除引用计算为0的</param>
        /// <param name="onlyTimeout">只清除缓存时间已到的</param>
        /// <param name="includePersistent">是否包含常驻资源</param>
        public void Clear(bool onlyRefZero = true, bool onlyTimeout = true, bool includePersistent = false)
        {
            string[] keys = new string[m_caches.Count];
            m_caches.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; ++i)
            {
                string key = keys[i];
                AssetBundleInfo item = m_caches[key];
                if (onlyRefZero)
                {
                    // 只清除引用计数为0的
                    if (item.IsCanRemove && (!onlyTimeout || item.IsTimeOut))
                    {
                        UnloadAssetBundleInfo(key);
                    }
                }
                else if (includePersistent || item.IsCanRemove)
                {
                    UnloadAssetBundleInfo(key);
                }
            }
        }

        /// <summary>
        /// 添加AssetBundle缓存
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="persistent">常驻</param>
        /// <returns></returns>
        private AssetBundleInfo AddAssetBundleInfo(string name, bool persistent)
        {
            AssetBundleInfo info;
            if (!m_caches.TryGetValue(name, out info))
            {
                info = new AssetBundleInfo(name, persistent);
                m_caches.Add(name, info);
            }

            ++info.ReferencedCount;
            return info;
        }

        /// <summary>
        /// 移除AssetBundle缓存
        /// </summary>
        /// <param name="name">AssetBundle名称</param>
        /// <param name="immediate">是否立即卸载</param>
        /// <param name="onlyRemove">仅移除,不卸载</param>
        /// <returns>是否移除</returns>
        public bool RemoveAssetBundleInfo(string name, bool immediate = false, bool onlyRemove = false)
        {
            AssetBundleInfo item;
            if (!m_caches.TryGetValue(name, out item))
            {
                return false;
            }

            if (item.ReferencedCount <= 0)
            {
                UnityEngine.Debug.LogWarningFormat("-->RemoveAssetBundleInfo {0} {1}", name, item.ReferencedCount);
                return true;
            }

            --item.ReferencedCount;

            if ((ConstantData.AssetBundleCacheTime < 0.001f || immediate) && item.IsCanRemove)
            {
                UnloadAssetBundleInfo(name, onlyRemove);
            }

            return true;
        }

        /// <summary>
        /// 从缓存中获取AssetBundle
        /// </summary>
        /// <param name="name">AssetBundle名称</param>
        /// <param name="persistent">是否常驻</param>
        /// <returns>缓存对象</returns>
        private AssetBundleInfo GetAssetBundleInfo(string name, bool persistent)
        {
            if (!m_caches.ContainsKey(name))
            {
                return null;
            }

            AssetBundleInfo item = m_caches[name];
            ++item.ReferencedCount;
            if (persistent)
            {
                item.Persistent = true;
            }

            return item;
        }

        /// <summary>
        /// 获取AssetBundleInfo
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private AssetBundleInfo GetAssetBundleInfo(string name)
        {
            if (!m_caches.ContainsKey(name))
            {
                return null;
            }

            return m_caches[name];
        }

        /// <summary>
        /// 设置AssetBundle
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ab"></param>
        /// <returns></returns>
        public AssetBundleInfo SetAssetBundle(string name, AssetBundle ab)
        {
            AssetBundleInfo info;
            if (!m_caches.TryGetValue(name, out info))
            {
                info = AddAssetBundleInfo(name, false);
            }

            info.Bundle = ab;
            return info;
        }

        /// <summary>
        /// 卸载AssetBundle缓存
        /// </summary>
        /// <param name="key">AssetBundle名称</param>
        /// <param name="onlyRemove">仅删除,不卸载</param>
        private void UnloadAssetBundleInfo(string key, bool onlyRemove = false)
        {
            AssetBundleInfo item = m_caches[key];
            if (item != null && !onlyRemove)
            {
                item.Unload();
            }

            m_caches.Remove(key);
        }

        /// <summary>
        /// 检查是否已在缓存
        /// </summary>
        /// <param name="group">加载组</param>
        /// <param name="name">AssetBundle名称</param>
        /// <param name="onLoaded">完成回调</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="async">是否异步</param>
        /// <returns>已缓存(是-引用计数+1,并返回true, 否-返回false)</returns>
        public bool CheckAssetBundleInfo(LoaderGroup group, string name, LoadManager.GroupLoadedCallback onLoaded,
            bool persistent, bool async)
        {
            AssetBundleInfo info = GetAssetBundleInfo(name, persistent);
            if (info == null)
            {
                AddAssetBundleInfo(name, persistent);
                return false;
            }

            if (info.Bundle == null)
            {
                return false;
            }

            if (onLoaded != null)
            {
                if (!async)
                {
                    onLoaded(group, info);
                }
                else
                {
                    m_task.AddAsyncCallback(onLoaded, group, info);
                }
            }

            return true;
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-->Dump Bundle Cache. Count = {0}\n", m_caches.Count);
            var iter = m_caches.GetEnumerator();
            while (iter.MoveNext())
            {
                AssetBundleInfo item = iter.Current.Value;
                sb.AppendFormat("{0} count:{1} persistent:{2}\n", iter.Current.Key, item.ReferencedCount,
                    item.Persistent);
            }

            iter.Dispose();

            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}