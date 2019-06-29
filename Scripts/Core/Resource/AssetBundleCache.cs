#region Namespace

using IGG.StateMachine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> assetbundle cache info </para>
    /// </summary>
    public class AssetBundleInfo
    {
        /// <summary>
        /// AssetBundle
        /// </summary>
        private AssetBundle m_AssetBundle;
        public AssetBundle assetBundle
        {
            get { return m_AssetBundle; }
            set
            {
                if (value == null || m_AssetBundle == value)
                {
                    return;
                }

                m_AssetBundle = value;
            }
        }

        /// <summary>
        /// AssetBundle名
        /// </summary>
        private string m_Name;
        public string name { get { return m_Name; } set { m_Name = value; } }

        /// <summary>
        /// 常驻
        /// </summary>
        private bool m_Persistent;
        public bool persistent { get { return m_Persistent; } set { m_Persistent = value; } }

        /// <summary>
        /// 引用计数
        /// </summary>
        private int m_ReferenceCount;
        public int referencedCount
        {
            get { return m_ReferenceCount; }
            set
            {
                m_ReferenceCount = value;
                if (canRemove)
                {
                    if (ConstantData.assetBundleCacheTime > 0)
                    {
                        m_UnloadTime = UnityEngine.Time.realtimeSinceStartup;
                    }
                    else
                    {
                        Unload();
                    }
                }
                else
                {
                    m_UnloadTime = 0;
                }
            }
        }

        /// <summary>
        /// 释放时间
        /// </summary>
        private float m_UnloadTime;

        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool canRemove
        {
            get
            {
                return (!persistent && referencedCount == 0);
            }
        }

        /// <summary>
        /// 缓存时间到
        /// </summary>
        public bool timeOut
        {
            get { return UnityEngine.Time.realtimeSinceStartup - m_UnloadTime >= ConstantData.assetBundleCacheTime; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">AssetBundle名</param>
        /// <param name="assetbundle">AssetBundle对象</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="referenceCount">初始引用计数</param>
        public AssetBundleInfo(string name, AssetBundle assetbundle, bool persistent, int referenceCount = 0)
        {
            m_Name = name;
            m_Persistent = persistent;
            m_ReferenceCount = referenceCount;
            m_AssetBundle = assetbundle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">AssetBundle名</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="referenceCount">初始引用计数</param>
        public AssetBundleInfo(string name, bool persistent, int referenceCount = 0)
        {
            m_Name = name;
            m_Persistent = persistent;
            m_ReferenceCount = referenceCount;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public Object LoadAsset(string name, Type type)
        {
            if (null == m_AssetBundle)
            {
                return null;
            }

            return m_AssetBundle.LoadAsset(name, type);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="name">资源名</param>
        /// <returns>资源对象</returns>
        public T LoadAsset<T>(string name) where T : Object
        {
            if (null == m_AssetBundle)
            {
                return null;
            }

            return m_AssetBundle.LoadAsset<T>(name);
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public void Unload()
        {
            if (null != m_AssetBundle)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
        }
    }

    /// <summary>
    ///     <para> assebundle cache </para>
    /// </summary>
    public class AssetBundleCache : IUpdate
    {
        /// <summary>
        /// 缓存队列
        /// </summary>
        private readonly Dictionary<string, AssetBundleInfo> m_dicAssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

        /// <summary>
        /// 加载任务
        /// </summary>
        private readonly LoaderTask m_LoaderTask;

        /// <summary>
        /// 上一次清除时间
        /// </summary>
        private float m_LastClearTime;

        public AssetBundleCache(LoaderTask task)
        {
            m_LoaderTask = task;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            if (ConstantData.assetBundleCacheTime > 0 &&
                UnityEngine.Time.realtimeSinceStartup - m_LastClearTime >= ConstantData.assetBundleCacheTime)
            {
                m_LastClearTime = UnityEngine.Time.realtimeSinceStartup;
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
            string[] keys = new string[m_dicAssetBundleInfos.Count];
            m_dicAssetBundleInfos.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; ++i)
            {
                string key = keys[i];
                bool needUnload = false;
                AssetBundleInfo assetBundleInfo = m_dicAssetBundleInfos[key];
                if (onlyRefZero)
                {
                    // 只清除引用计数为0的
                    needUnload |= (assetBundleInfo.canRemove && (!onlyTimeout || assetBundleInfo.timeOut));
                }
                else 
                    needUnload |= (includePersistent || assetBundleInfo.canRemove);

                if (needUnload)
                    UnloadAssetBundleInfo(key);
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
            if (!m_dicAssetBundleInfos.TryGetValue(name, out AssetBundleInfo assetBundleInfo))
            {
                assetBundleInfo = new AssetBundleInfo(name, persistent);
                m_dicAssetBundleInfos.Add(name, assetBundleInfo);
            }

            ++assetBundleInfo.referencedCount;
            return assetBundleInfo;
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
            if (!m_dicAssetBundleInfos.TryGetValue(name, out AssetBundleInfo assetBundleInfo))
            {
                return false;
            }

            if (assetBundleInfo.referencedCount <= 0)
            {
                UnityEngine.Debug.LogWarningFormat("-->RemoveAssetBundleInfo {0} {1}", name, assetBundleInfo.referencedCount);
                return true;
            }

            --assetBundleInfo.referencedCount;

            if ((ConstantData.assetBundleCacheTime < 0.001f || immediate) && assetBundleInfo.canRemove)
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
            if (m_dicAssetBundleInfos.TryGetValue(name, out AssetBundleInfo assetBundleInfo))
            {
                ++assetBundleInfo.referencedCount;
                if (persistent)
                {
                    assetBundleInfo.persistent = true;
                }
            }

            return assetBundleInfo;
        }

        /// <summary>
        /// 获取AssetBundleInfo
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private AssetBundleInfo GetAssetBundleInfo(string name)
        {
            m_dicAssetBundleInfos.TryGetValue(name, out AssetBundleInfo assetBundleInfo);
            return assetBundleInfo;
        }

        /// <summary>
        /// 设置AssetBundle
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public AssetBundleInfo SetAssetBundle(string name, AssetBundle assetBundle)
        {
            if (!m_dicAssetBundleInfos.TryGetValue(name, out AssetBundleInfo assetBundleInfo))
            {
                assetBundleInfo = AddAssetBundleInfo(name, false);
            }

            assetBundleInfo.assetBundle = assetBundle;
            return assetBundleInfo;
        }

        /// <summary>
        /// 卸载AssetBundle缓存
        /// </summary>
        /// <param name="key">AssetBundle名称</param>
        /// <param name="onlyRemove">仅删除,不卸载</param>
        private void UnloadAssetBundleInfo(string key, bool onlyRemove = false)
        {
            AssetBundleInfo assetBundleInfo = m_dicAssetBundleInfos[key];
            if (null != assetBundleInfo && !onlyRemove)
            {
                assetBundleInfo.Unload();
            }

            m_dicAssetBundleInfos.Remove(key);
        }

        /// <summary>
        /// 检查是否已在缓存
        /// </summary>
        /// <param name="group">加载组</param>
        /// <param name="name">AssetBundle名称</param>
        /// <param name="callback">完成回调</param>
        /// <param name="persistent">是否常驻</param>
        /// <param name="async">是否异步</param>
        /// <returns>已缓存(是-引用计数+1,并返回true, 否-返回false)</returns>
        public bool CheckAssetBundleInfo(LoaderGroup group, string name, LoaderGroupCompleteCallback callback,
            bool persistent, bool async)
        {
            AssetBundleInfo assetBundleInfo = GetAssetBundleInfo(name, persistent);
            if (null == assetBundleInfo)
            {
                AddAssetBundleInfo(name, persistent);
                return false;
            }

            if (null == assetBundleInfo.assetBundle)
            {
                return false;
            }

            if (null != callback)
            {
                if (!async)
                {
                    callback(group, assetBundleInfo);
                }
                else
                {
                    m_LoaderTask.AddAsyncCallback(callback, group, assetBundleInfo);
                }
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("-->Dump Bundle Cache. Count = {0}\n", m_dicAssetBundleInfos.Count);
            foreach(var assetBundleInfoPair in m_dicAssetBundleInfos)
            {
                stringBuilder.AppendFormat("{0} count:{1} persistent:{2}\n", assetBundleInfoPair.Key,
                    assetBundleInfoPair.Value.referencedCount,
                    assetBundleInfoPair.Value.persistent);
            }

            return stringBuilder.ToString();
        }
    }
}