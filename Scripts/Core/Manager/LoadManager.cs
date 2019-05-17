using IGG.Core.Resource;
using IGG.Game;
using IGG.Utility;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IGG.Core.Manager
{
    public class LoadManager : Singleton<LoadManager>, IManager
    {
        public void OnUpdate(float deltaTime)
        { }

        public delegate void LoaderGroupCompleteCallback(LoaderGroup group, object data);

        public delegate void CompleteCallback(object data);

        public delegate void ProgressCallback(float progress);

        // 加载优先级
        public enum LoadPriority
        {
            Normal,
            High,

            Quantity,
        }

        private readonly AssetBundleCache m_cache;
        private readonly DownloadOrCache m_downloadOrCache;
        private readonly Dictionary<string, string> m_origns = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_patchs = new Dictionary<string, string>();

        private readonly List<string> m_searchPaths = new List<string>();

        // ------------------------------------------------------------------------------------------
        // 加载任务
        private readonly LoaderTask m_task;

        // ------------------------------------------------------------------------------------------
        private bool m_hasWarm;

        private AssetBundleManifest m_manifest;
        private AssetBundleMapping m_mapping;

        // ------------------------------------------------------------------------------------------
        private DownloadOrCache m_unpacker;

        private string m_urlPatch;

        public LoadManager()
        {
            if (ConstantData.EnableCache)
            {
                Caching.compressionEnabled = false;

                string path = ConstantData.UnpackPath;
                FileUtil.CreateFileDirectory(path);

                UnityEngine.Cache cache = Caching.GetCacheByPath(path);
                if (!cache.valid)
                {
                    cache = Caching.AddCache(path);
                }

                Caching.currentCacheForWriting = cache;
            }
            else
            {
                if (ConstantData.EnablePatch)
                {
                    AddSearchPath(ConstantData.PatchPath);
                }

                if (ConstantData.EnableCustomCompress)
                {
                    AddSearchPath(ConstantData.UnpackPath);
                }
            }

            m_task = new LoaderTask();
            m_cache = new AssetBundleCache(m_task);
            m_downloadOrCache = new DownloadOrCache();

            Clear();

            if (ConstantData.EnableAssetBundle)
            {
                LoadVersion();
            }
        }

        public bool Enabled { get; set; }

        public void Initialize(MonoBehaviour mb) { }

        public void Update()
        {
            m_task.Update();
            m_cache.Update();
            m_downloadOrCache.Update();
            UpdateUnpacker();
        }

        public void Clear(bool force = false)
        {
            m_task.Clear();
            m_cache.Clear(!force, false, force);

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void ClearAfterPatch()
        {
            m_manifest = null;
            m_mapping = null;

            UnloadAssetBundle(ConstantData.AssetbundleManifest, true);
            UnloadAssetBundle(ConstantData.AssetbundleMapping, true);

            Clear(true);
        }

        private void AddSearchPath(string path)
        {
            m_searchPaths.Add(path);
        }

        private string SearchPath(string subpath, bool noStreamAssetPath = false, bool needSuffix = false,
            bool isAssetBundle = true)
        {
            if (needSuffix)
            {
                subpath = string.Format("{0}{1}", subpath, ConstantData.AssetBundleExt);
            }

            // 优先从查找目录找
            for (int i = 0; i < m_searchPaths.Count; ++i)
            {
                string fullpath = string.Format("{0}/{1}", m_searchPaths[i], subpath);
                if (File.Exists(fullpath))
                {
                    return fullpath;
                }
            }

            // 不查找StreamAsset目录
            if (noStreamAssetPath)
            {
                return "";
            }

            if (isAssetBundle)
            {
                return string.Format("{0}/{1}", ConstantData.StreamingAssetsPath, subpath);
            }
            else
            {
                return string.Format("{0}/{1}", Application.streamingAssetsPath, subpath);
            }
        }

        public string GetResourcePath(string name)
        {
            bool isCache = false;
            return GetResourcePath(name, ref isCache);
        }

        public string GetResourcePath(string name, ref bool isCache, bool async = false)
        {
            if (ConstantData.EnableCache)
            {
                string md5;
                if (m_patchs.TryGetValue(name, out md5))
                {
                    if (async)
                    {
                        // 异步加载,返回远程路径
                        return string.Format("{0}/{1}{2}", m_urlPatch, md5, ConstantData.AssetBundleExt);
                    }
                    else
                    {
                        // 同步加载,返回缓存路径
                        isCache = true;
                        string path = FileUtil.GetCacheAssetBundlePath(md5);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }

                if (m_origns.TryGetValue(name, out md5))
                {
                    return string.Format("{0}/{1}{2}", ConstantData.StreamingAssetsPath, md5,
                        ConstantData.AssetBundleExt);
                }

                UnityEngine.Debug.LogErrorFormat("Get MD5 failed: {0}", name);
                return "";
            }
            else
            {
                string path = name;
                if (ConstantData.EnableMd5Name)
                {
                    string md5;
                    if (m_patchs.TryGetValue(name, out md5))
                    {
                        path = SearchPath(md5, true, true);
                        if (!string.IsNullOrEmpty(path))
                        {
                            return path;
                        }
                    }

                    if (!m_origns.TryGetValue(name, out md5))
                    {
                        UnityEngine.Debug.LogErrorFormat("Get MD5 failed: {0}", name);
                        return "";
                    }

                    path = md5;
                }

                return SearchPath(path, false, true);
            }
        }

        private void InitVersionData(bool start = true)
        {
            Clear();

            m_origns.Clear();
            for (int i = 0; i < VersionData.Inst.Items.Length; ++i)
            {
                VersionData.VersionItem item = VersionData.Inst.Items[i];
                m_origns.Add(item.Name, item.Md5);
            }

            if (start)
            {
                LoadManifest();
            }
        }

        public void SetPatchData(JSONClass json, bool clear = false)
        {
            if (clear)
            {
                ClearAfterPatch();
            }

            m_patchs.Clear();
            if (json != null)
            {
                m_urlPatch = json["url"];
                ConfigData.Inst.InitFromJson(json);

                JSONClass list = json["list"] as JSONClass;
                if (list != null)
                {
                    foreach (KeyValuePair<string, JSONNode> item in list)
                    {
                        if ((PatchFileType)item.Value["type"].AsInt == PatchFileType.AssetBundle)
                        {
                            m_patchs.Add(item.Key, item.Value["md5"]);
                        }
                    }
                }
            }

            LoadManifest();
        }

        private void LoadVersion()
        {
            UnityEngine.Debug.Log("LoadVersion");
            if (ConstantData.EnableMd5Name)
            {
                string pathPatch = string.Format("{0}/version", Application.persistentDataPath);
                bool hasPatch = false;

                if (ConstantData.EnablePatch)
                {
                    hasPatch = File.Exists(pathPatch);
                }

                InitVersionData(!hasPatch);

                if (hasPatch)
                {
                    LoadStream(pathPatch, (data) =>
                    {
                        byte[] bytes = data as byte[];
                        if (bytes == null)
                        {
                            UnityEngine.Debug.LogError("Load patch version Failed!");
                            SetPatchData(null);
                            return;
                        }

                        string text = Encoding.UTF8.GetString(bytes);
                        JSONClass json = JSONNode.Parse(text) as JSONClass;
                        if (json == null)
                        {
                            UnityEngine.Debug.LogError("Load patch version Failed!");
                            SetPatchData(null);
                            return;
                        }

                        if (!string.Equals(json["version"], ConstantData.MainVersion))
                        {
                            SetPatchData(null);
                            return;
                        }

                        SetPatchData(json);
                    }, false, false, true);
                }
            }
            else
            {
                LoadManifest();
            }
        }

        // 加载资源清单
        private void LoadManifest()
        {
            UnityEngine.Debug.Log("LoadManifest");

            // 加载AssetBundle依赖文件
            LoadAssetBundle(null, ConstantData.AssetbundleManifest, (group, data) =>
            {
                AssetBundleInfo ab = data as AssetBundleInfo;
                if (ab != null)
                {
                    m_manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }, false, false, false);

            // 加载资源到AssetBundle映射表
            string name = ConstantData.AssetbundleMapping;
            LoadAssetFromBundle(null, name, name, typeof(AssetBundleMapping), (group, data) =>
            {
                m_mapping = data as AssetBundleMapping;
                if (m_mapping != null)
                {
                    m_mapping.Init();
                }
            }, false);

            UnityEngine.Debug.Log("LoadManifest End");
        }

        public bool HasBundle(string path)
        {
            path = path.ToLower();
            return m_patchs.ContainsKey(path) || m_origns.ContainsKey(path);
        }

        public void WarmUpShader()
        {
            if (!ConstantData.EnableAssetBundle)
            {
                return;
            }

            if (m_hasWarm)
            {
                return;
            }

            m_hasWarm = true;

            LoadBundle("shader", (data) =>
            {
                AssetBundle ab = data as AssetBundle;
                if (ab == null)
                {
                    return;
                }

                ShaderVariantCollection variant = ab.LoadAsset<ShaderVariantCollection>("warmshader");
                if (variant != null)
                {
                    variant.WarmUp();
                }
            });
        }

        private void RefreshShader(AssetBundle ab)
        {
            if (ab.isStreamedSceneAssetBundle)
            {
                return;
            }

            Material[] materials = ab.LoadAllAssets<Material>();
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                string shaderName = mat.shader.name;
                Shader newShader = Shader.Find(shaderName);
                if (newShader != null)
                {
                    mat.shader = newShader;
                }
            }
        }

        // ------------------------------------------------------------------------------------------
        // 编辑器专用加载
        // 加载Assets目录下的文件(编辑器专用,带后缀)
        public void LoadFile(string path, CompleteCallback completeCallback,
            bool async = true, bool inData = true, LoadPriority priority = LoadPriority.Normal)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            Logger.LogError("LoadFile为编辑器专用方法!");
            return;
#endif

            string fullpath = string.Format("{0}/{1}", inData ? ConstantData.DataFullPath : Application.dataPath, path);
            if (!CheckFileExist(fullpath, completeCallback))
            {
                return;
            }

            m_task.AddLoadTask(null, LoaderType.Stream, fullpath, false, (group, data) =>
            {
                completeCallback?.Invoke(data);
            }, async, priority);
        }

        // 加载资源(Assets目录下,带后缀)
        public void LoadAssetFile(string path, CompleteCallback completeCallback, Type type = null,
            bool async = true, bool inData = true, LoadPriority priority = LoadPriority.Normal)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            Logger.LogError("LoadAssetFile为编辑器专用方法!");
            return;
#endif

            string fullpath = string.Format("{0}/{1}", inData ? ConstantData.DataFullPath : Application.dataPath, path);
            if (!CheckFileExist(fullpath, completeCallback))
            {
                return;
            }

            fullpath = inData
                ? string.Format("Assets/{0}/{1}", ConstantData.DataPath, path)
                : string.Format("Assets/{0}", path);
            m_task.AddLoadTask(null, LoaderType.Asset, fullpath, type, (group, data) =>
            {
                completeCallback?.Invoke(data);
            }, async, priority);
        }

        // 全平台加载
        // 加载流文件(remote==false时先从persistentData读,没有找到则从streamingAssets读,带后缀)
        public void LoadStream(string path, CompleteCallback completeCallback,
            bool async = true, bool remote = false, bool isFullPath = false,
            LoadPriority priority = LoadPriority.Normal)
        {
            string fullpath = path;
            if (!remote)
            {
                if (!isFullPath)
                {
                    fullpath = SearchPath(path, false, false, false);
                }
            }
            else
            {
                // 从服务器加载,一定是异步的
                async = true;
            }

            m_task.AddLoadTask(null, LoaderType.Stream, fullpath, remote, (group, data) =>
            {
                completeCallback?.Invoke(data);
            }, async, priority);
        }

        // 加载资源(Resource目录下,不带后缀)
        public void LoadResource(string path, CompleteCallback completeCallback,
            bool async = true, LoadPriority priority = LoadPriority.Normal)
        {
            m_task.AddLoadTask(null, LoaderType.Resource, path, null, (group, data) =>
            {
                completeCallback?.Invoke(data);
            }, async, priority);
        }

        // 加载关卡
        public void LoadScene(string name, CompleteCallback completeCallback, bool async = true, bool additive = false)
        {
            LoaderGroup group = m_task.PopGroup(LoadPriority.High);
            if (!additive)
            {
                m_task.AddLoadTask(group, LoaderType.Scene, "Empty", null, null, async);
            }

            if (ConstantData.EnableAssetBundle)
            {
                string path = string.Format("Scene/{0}", name).ToLower();

                LoadAssetBundle(group, path, (group1, data1) =>
                {
                    m_task.AddLoadTask(group1, LoaderType.Scene, name, additive, (group2, data2) =>
                    {
                        completeCallback?.Invoke(data2);
                    }, async, group.priority, true);
                }, async);
            }
            else
            {
                m_task.AddLoadTask(group, LoaderType.Scene, name, additive, (group1, data1) =>
                {
                    completeCallback?.Invoke(data1);
                }, async);
            }
        }

        public void UnloadScene(string name, bool additive = false)
        {
            if (additive)
            {
                SceneManager.UnloadSceneAsync(name);
            }

            if (ConstantData.EnableAssetBundle)
            {
                string path = string.Format("Scene/{0}", name).ToLower();
                UnloadBundle(path);
            }
        }

        // 加载AssetBundle(先从persistentData读,没有找到则从streamingAssets读)
        public void LoadBundle(string path, CompleteCallback completeCallback,
            bool async = true, bool persistent = false, bool manifest = true,
            LoadPriority priority = LoadPriority.Normal)
        {
            path = path.ToLower();
            LoadAssetBundle(null, path, (group, data) =>
            {
                AssetBundleInfo assetBundleInfo = data as AssetBundleInfo;

                completeCallback?.Invoke(null != assetBundleInfo ? assetBundleInfo.assetBundle : null);
            }, async, persistent, manifest, priority);
        }

        // 加载AssetBundle(先从persistentData读, 没有找到则从streamingAssets读)
        public AssetBundle LoadBundle(string path, bool persistent = false, bool manifest = true)
        {
            AssetBundle ab = null;
            LoadBundle(path, (data) => { ab = data as AssetBundle; }, false, persistent, manifest);

            return ab;
        }

        // 卸载AssetBundle
        public void UnloadBundle(string path, bool immediate = false, bool onlyRemove = false)
        {
            path = path.ToLower();
            UnloadAssetBundle(path, immediate, onlyRemove);
        }

        // 从AssetBundle中加载资源
        public void LoadAssetFromBundle(LoaderGroup group, string path, string name, Type type,
            LoaderGroupCompleteCallback callback,
            bool async = true, bool persistent = false,
            LoadPriority priority = LoadPriority.Normal)
        {
            path = path.ToLower();
            LoadAssetBundle(group, path, (group1, data) =>
            {
                AssetBundleInfo cache = data as AssetBundleInfo;
                LoadBundleAsset(group1, cache, name, type, callback, async);
            }, async, persistent, true, priority);
        }

        // 加载资源
        public void LoadAsset(string path, Type type, CompleteCallback onLoaded,
            bool async = true, bool persistent = false, bool inData = true,
            LoadPriority priority = LoadPriority.Normal)
        {
            //Logger.Log(string.Format("LoadAsset: {0} - {1}", path, async));
            if (ConstantData.EnableAssetBundle)
            {
                string abName = m_mapping.GetAssetBundleNameFromAssetPath(path);
                if (string.IsNullOrEmpty(abName))
                {
                    //建筑的disappear有些有，有些没有，不算bug
                    if (!path.EndsWith("_disappear.prefab"))
                    {
                        UnityEngine.Debug.LogErrorFormat("找不到资源所对应的ab文件:{0}", path);
                    }
                    if (onLoaded != null)
                    {
                        onLoaded(null);
                    }
                }
                else
                {
                    string assetName = Path.GetFileName(path);
                    LoadAssetFromBundle(null, abName, assetName, type, (group, data) =>
                    {
                        if (onLoaded != null)
                        {
                            onLoaded(data);
                        }
                    }, async, persistent, priority);
                }
            }
            else
            {
                LoadAssetFile(path, onLoaded, type, async, inData, priority);
            }
        }

        public object LoadAsset(string path, Type type, bool persistent = false, bool inData = true)
        {
            object asset = null;
            LoadAsset(path, type, (data) => { asset = data; }, false, persistent, inData);

            return asset;
        }

        public void UnloadAsset(string path)
        {
            if (ConstantData.EnableAssetBundle)
            {
                string abName = m_mapping.GetAssetBundleNameFromAssetPath(path);

                if (!string.IsNullOrEmpty(abName))
                {
                    UnloadBundle(abName);
                }
            }
        }

        // ------------------------------------------------------------------------------------------
        // 从AssetBundle里加载资源
        private void LoadBundleAsset(LoaderGroup group, AssetBundleInfo info, string name, Type type,
            LoaderGroupCompleteCallback callback,
            bool async = true, LoadPriority priority = LoadPriority.Normal)
        {
            if (info == null || string.IsNullOrEmpty(name))
            {
                callback?.Invoke(group, null);

                return;
            }

            if (!async)
            {
                // 同步,直接加载
                //Logger.Log(string.Format("-->LoadBundleAsset: {0}", name));

                System.Object asset = info.LoadAsset(name, type);
                callback?.Invoke(group, asset);
            }
            else
            {
                // 异步,创建一个加载器后加载
                BundleAssetLoadParam param = new BundleAssetLoadParam
                {
                    assetBundle = info.assetBundle,
                    type = type
                };

                m_task.AddLoadTask(group, LoaderType.BundleAsset, name, param, (group1, data1) =>
                {
                    // 加载回调
                    callback?.Invoke(group1, data1);
                }, true, priority, true);
            }
        }

        // 加载AssetBundle(先从persistentData读,没有找到则从streamingAssets读,带后缀)
        private void LoadAssetBundle(LoaderGroup group, string path, LoaderGroupCompleteCallback callback,
            bool async = true, bool persistent = false, bool manifest = true,
            LoadPriority priority = LoadPriority.Normal)
        {
            path = path.ToLower();

            if (async && group == null)
            {
                group = m_task.PopGroup(priority);
            }

            if (manifest)
            {
                if (!HasBundle(path))
                {
                    // Manifest里没有这个AssetBundle,说明是一个错误的路径
                    UnityEngine.Debug.LogErrorFormat("ab不存在:{0}", path);
                    if (null != callback)
                    {
                        if (!async)
                        {
                            callback(group, null);
                        }
                        else
                        {
                            m_task.AddAsyncCallback(callback, group, null);
                        }
                    }

                    return;
                }

                // 加载依赖
                LoadDependencies(group, path, async, persistent);
            }

            // 检查是否有缓存
            if (m_cache.CheckAssetBundleInfo(group, path, callback, persistent, async))
            {
                return;
            }

            // 添加加载任务
            m_task.AddLoadTask(group, LoaderType.Bundle, path, null, (group1, data) =>
            {
                AssetBundle ab = data as AssetBundle;
                AssetBundleInfo info = null;

                if (ab != null)
                {
                    info = m_cache.SetAssetBundle(path, ab);
#if UNITY_EDITOR
                    RefreshShader(ab);
#endif
                }

                // 加载回调
                callback?.Invoke(group1, info);
            }, async, priority);
        }

        // 卸载AssetBundle
        private void UnloadAssetBundle(string path, bool immediate = false, bool onlyRemove = false)
        {
            path = path.ToLower();

            if (m_cache.RemoveAssetBundleInfo(path, immediate, onlyRemove))
            {
                UnloadDependencies(path, immediate);
            }
        }

        // 依赖
        // 加载依赖
        private void LoadDependencies(LoaderGroup group, string name, bool async, bool persistent)
        {
            if (m_manifest == null)
            {
                return;
            }

            string[] dependencies =
                m_manifest.GetDirectDependencies(string.Format("{0}{1}", name, ConstantData.AssetBundleExt));
            for (int i = 0; i < dependencies.Length; ++i)
            {
                LoadAssetBundle(group, dependencies[i].Replace(ConstantData.AssetBundleExt, ""), null, async,
                    persistent);
            }
        }

        // 卸载依赖
        private void UnloadDependencies(string name, bool immediate = false)
        {
            if (m_manifest == null)
            {
                return;
            }

            string[] dependencies =
                m_manifest.GetDirectDependencies(string.Format("{0}{1}", name, ConstantData.AssetBundleExt));
            for (int i = 0; i < dependencies.Length; ++i)
            {
                UnloadAssetBundle(dependencies[i].Replace(ConstantData.AssetBundleExt, ""), immediate);
            }
        }

        // ------------------------------------------------------------------------------------------
        // 文件/目录是否存在
        private bool CheckFileExist(string path, CompleteCallback completeCallback, bool isFile = true)
        {
            bool exist = isFile ? File.Exists(path) : Directory.Exists(path);
            if (!exist)
            {
                // 不存在
                completeCallback?.Invoke(null);

                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------------------------------
        // 前台加载
        public void BeginFrontLoad()
        {
            //m_task.BeginFrontLoad();
        }

        public void StartFrontLoad(ProgressCallback onProgress)
        {
            //m_task.StartFrontLoad(onProgress);
        }

        // ------------------------------------------------------------------------------------------
        // 缓存
        public void AddToDownloadOrCache(string path)
        {
            m_downloadOrCache.Add(path);
        }

        // ------------------------------------------------------------------------------------------
        // 解压
        // 更新解压
        private void UpdateUnpacker()
        {
            if (m_unpacker != null)
            {
                m_unpacker.Update();
            }
        }

        // 解压所有的资源
        public void UnpackStreamingAssets(Action onStart, Action<int, int, int> onProgress, Action onCompleted)
        {
            List<string> files = new List<string>();
            for (int i = 0; i < VersionData.Inst.Items.Length; ++i)
            {
                VersionData.VersionItem item = VersionData.Inst.Items[i];

                string path = FileUtil.GetCacheAssetBundlePath(item.Md5);
                if (!FileUtil.CheckFileExist(path))
                {
                    files.Add(GetResourcePath(item.Name));
                }
            }

            if (files.Count == 0)
            {
                if (onCompleted != null)
                {
                    onCompleted();
                }

                return;
            }

            if (onStart != null)
            {
                onStart();
            }

            int thread = Mathf.Clamp(SystemInfo.processorCount - 1, 1, 20);

            m_unpacker = new DownloadOrCache(thread, files, onProgress, () =>
            {
                m_unpacker = null;
                if (onCompleted != null)
                {
                    onCompleted();
                }
            });
        }



        // 加载Lua
        // public byte[] LoadLua(string name)
        // {
        //     byte[] bytes = null;
        //
        //     string resType = ResourcesType.luaData;
        //     if (ConstantData.EnableAssetBundle)
        //     {
        //         AssetBundle ab = LoadBundle("lua");
        //         if (ab != null)
        //         {
        //             TextAsset asset = ab.LoadAsset<TextAsset>(name);
        //             if (asset != null)
        //             {
        //                 bytes = asset.bytes;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         string path = ResourcesPath.GetEditorFullPath(resType, name);
        //         LoadFile(path, (data) => { bytes = data as byte[]; }, false, false);
        //     }
        //
        //     return bytes;
        // }

        // ------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return m_cache.ToString();
        }
    }
}
