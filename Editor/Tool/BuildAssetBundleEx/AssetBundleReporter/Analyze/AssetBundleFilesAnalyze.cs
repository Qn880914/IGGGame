using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace AssetBundleBrowser
{
    /// <summary>
    /// AB 文件分析器
    /// </summary>
    public static class AssetBundleFilesAnalyze
    {
        #region 对外接口

        /// <summary>
        /// 自定义分析依赖
        /// </summary>
        public static System.Func<string, List<AssetBundleFileInfo>> analyzeCustomDepend { get; set; }

        /// <summary>
        /// 分析的时候，也导出资源
        /// </summary>
        public static bool analyzeExport { get; set; }

        /// <summary>
        /// 分析的时候，只分析场景，这需要播放运行才能分析场景
        /// </summary>
        public static bool analyzeOnlyScene { get; set; }

        #endregion

        #region 内部实现

        /// <summary>
        ///     <para> all AssetBundle File Info. </para>
        /// </summary>
        public static List<AssetBundleFileInfo> assetBundleFileInfos { get; set; }

        /// <summary>
        ///     <para> all Asset File Info. </para>
        ///     <para> guid map AssetFileInfo. </para>
        /// </summary>
        public static Dictionary<long, AssetFileInfo> assetFileInfos { get; set; }

        private static AssetBundleFilesAnalyzeScene s_AnalyzeScene;

        public static UnityAction analyzeCompletedCallback { get; set; }

        private static PropertyInfo s_InspectorMode;


        public static AssetBundleFileInfo GetAssetBundleFileInfo(string name)
        {
            if (assetBundleFileInfos == null)
                return null;

            return assetBundleFileInfos.Find(info => info.name == name);
        }

        public static AssetFileInfo GetAssetFileInfo(long guid)
        {
            if (assetFileInfos == null)
            {
                assetFileInfos = new Dictionary<long, AssetFileInfo>();
            }

            if (!assetFileInfos.TryGetValue(guid, out AssetFileInfo info))
            {
                info = new AssetFileInfo { guid = guid };
                assetFileInfos.Add(guid, info);
            }

            return info;
        }

        public static void Clear()
        {
            if (assetBundleFileInfos != null)
            {
                assetBundleFileInfos.Clear();
                assetBundleFileInfos = null;
            }
            if (assetFileInfos != null)
            {
                assetFileInfos.Clear();
                assetFileInfos = null;
            }
            s_AnalyzeScene = null;

#if UNITY_5 || UNITY_5_3_OR_NEWER
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
            System.GC.Collect();
        }

        public static bool Analyze(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogErrorFormat("{0} is not exists!", directoryPath);
                return false;
            }

            assetBundleFileInfos = analyzeCustomDepend?.Invoke(directoryPath);

            if (assetBundleFileInfos == null)
            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                assetBundleFileInfos = AnalyzeManifestDepend(directoryPath);
#endif
            }

            if (assetBundleFileInfos == null)
            {
                assetBundleFileInfos = AnalyzAllFiles(directoryPath);
            }

            if (assetBundleFileInfos == null)
            {
                return false;
            }

            s_AnalyzeScene = new AssetBundleFilesAnalyzeScene();
            AnalyzeBundleFiles(assetBundleFileInfos);
            s_AnalyzeScene.Analyze();

            if (!s_AnalyzeScene.IsAnalyzing())
            {
                analyzeCompletedCallback?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// 分析Unity5方式的依赖构成
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static List<AssetBundleFileInfo> AnalyzeManifestDepend(string directoryPath)
        {
            string manifestName = Path.GetFileName(directoryPath);
            string manifestPath = Path.Combine(directoryPath, manifestName);
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarningFormat("{0} is not exists! Use AnalyzAllFiles ...", manifestPath);
                return null;
            }
#if UNITY_5_3_OR_NEWER
            AssetBundle manifestAb = AssetBundle.LoadFromFile(manifestPath);
#else
            AssetBundle manifestAb = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(manifestPath));
#endif
            if (manifestAb == null)
            {
                Debug.LogErrorFormat("{0} ab load faild!", manifestPath);
                return null;
            }

            List<AssetBundleFileInfo> assetBundleFileInfos = new List<AssetBundleFileInfo>();
#if UNITY_5 || UNITY_5_3_OR_NEWER
            AssetBundleManifest assetBundleManifest = manifestAb.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            var assetBundles = assetBundleManifest.GetAllAssetBundles();
            foreach (var assetBundle in assetBundles)
            {
                string path = Path.Combine(directoryPath, assetBundle);
                AssetBundleFileInfo assetBundleFileInfo = new AssetBundleFileInfo
                {
                    name = assetBundle,
                    path = path,
                    rootPath = directoryPath,
                    size = new FileInfo(path).Length,
                    directDepends = assetBundleManifest.GetDirectDependencies(assetBundle),
                    allDepends = assetBundleManifest.GetAllDependencies(assetBundle)
                };
                assetBundleFileInfos.Add(assetBundleFileInfo);
            }
#endif
            manifestAb.Unload(true);
            return assetBundleFileInfos;
        }

        /// <summary>
        /// 直接递归所有文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static List<AssetBundleFileInfo> AnalyzAllFiles(string directoryPath)
        {
            List<AssetBundleFileInfo> infos = new List<AssetBundleFileInfo>();
            string bom = "Unity";
            char[] flag = new char[5];
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                using (StreamReader streamReader = new StreamReader(file))
                {
                    if (streamReader.Read(flag, 0, flag.Length) == flag.Length && new string(flag) == bom)
                    {
                        AssetBundleFileInfo info = new AssetBundleFileInfo
                        {
                            name = file.Substring(directoryPath.Length + 1),
                            path = file,
                            rootPath = directoryPath,
                            size = streamReader.BaseStream.Length,
                            directDepends = new string[] { },
                            allDepends = new string[] { }
                        };
                        infos.Add(info);
                    }
                }
            }

            return infos;
        }

        private static void AnalyzeBundleFiles(List<AssetBundleFileInfo> infos)
        {
            // 分析被依赖的关系
            foreach (var info in infos)
            {
                List<string> beDepends = new List<string>();
                foreach (var info2 in infos)
                {
                    if (info2.name == info.name)
                    {
                        continue;
                    }

                    if (info2.allDepends.Contains(info.name))
                    {
                        beDepends.Add(info2.name);
                    }
                }
                info.beDepends = beDepends.ToArray();
            }

            // 以下不能保证百分百找到所有的资源，最准确的方式是解密AssetBundle格式
            foreach (var info in infos)
            {
#if UNITY_5_3_OR_NEWER
                AssetBundle assetBundle = AssetBundle.LoadFromFile(info.path);
#else
                AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(info.path));
#endif

                if (assetBundle == null)
                {
                    continue;
                }

                try
                {
                    if (!assetBundle.isStreamedSceneAssetBundle)
                    {
                        if (!analyzeOnlyScene)
                        {
                            Object[] objs = assetBundle.LoadAllAssets<Object>();
                            foreach (var obj in objs)
                            {
                                AnalyzeObjectReference(info, obj);
                                AnalyzeObjectComponent(info, obj);
                            }
                            AnalyzeObjectsCompleted(info);
                        }
                    }
                    else
                    {
                        info.isScene = true;
                        s_AnalyzeScene.AddBundleSceneInfo(info, assetBundle.GetAllScenePaths());
                    }
                }
                finally
                {
                    assetBundle.Unload(true);
                }
            }
        }

        /// <summary>
        /// 分析对象的引用
        /// </summary>
        /// <param name="assetBundleFileInfo"></param>
        /// <param name="obj"></param>
        public static void AnalyzeObjectReference(AssetBundleFileInfo assetBundleFileInfo, Object obj)
        {
            if (obj == null || assetBundleFileInfo.objDict.ContainsKey(obj))
            {
                return;
            }

            var serializedObject = new SerializedObject(obj);
            assetBundleFileInfo.objDict.Add(obj, serializedObject);

            if (s_InspectorMode == null)
            {
                s_InspectorMode = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            s_InspectorMode.SetValue(serializedObject, InspectorMode.Debug, null);

            var it = serializedObject.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                {
                    AnalyzeObjectReference(assetBundleFileInfo, it.objectReferenceValue);
                }
            }

            // 只能用另一种方式获取的引用
            AnalyzeObjectReference2(assetBundleFileInfo, obj);
        }

        /// <summary>
        /// 动画控制器比较特殊，不能通过序列化得到
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        private static void AnalyzeObjectReference2(AssetBundleFileInfo info, Object o)
        {
            AnimatorController ac = o as AnimatorController;
            if (ac)
            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                foreach (var clip in ac.animationClips)
                {
                    AnalyzeObjectReference(info, clip);
                }
#else
                List<State> list = new List<State>();
                for (int i = 0; i < ac.layerCount; i++)
                {
                    AnimatorControllerLayer layer = ac.GetLayer(i);
                    list.AddRange(AnimatorStateMachine_StatesRecursive(layer.stateMachine));
                }
                foreach (State state in list)
                {
                    var clip = state.GetMotion() as AnimationClip;
                    if (clip)
                    {
                        AnalyzeObjectReference(info, clip);
                    }
                }
#endif
            }
        }

#if !(UNITY_5 || UNITY_5_3_OR_NEWER)
        private static List<State> AnimatorStateMachine_StatesRecursive(StateMachine stateMachine)
        {
            List<State> list = new List<State>();
            for (int i = 0; i < stateMachine.stateCount; i++)
            {
                list.Add(stateMachine.GetState(i));
            }
            for (int i = 0; i < stateMachine.stateMachineCount; i++)
            {
                list.AddRange(AnimatorStateMachine_StatesRecursive(stateMachine.GetStateMachine(i)));
            }
            return list;
        }
#endif

        /// <summary>
        /// 分析脚本的引用（这只在脚本在工程里时才有效）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        public static void AnalyzeObjectComponent(AssetBundleFileInfo info, Object o)
        {
            var go = o as GameObject;
            if (!go)
            {
                return;
            }

            var components = go.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (!component)
                {
                    continue;
                }

                AnalyzeObjectReference(info, component);
            }
        }

        public static void AnalyzeObjectsCompleted(AssetBundleFileInfo info)
        {
            foreach (var kv in info.objDict)
            {
                AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(kv.Key, kv.Value, info);
                kv.Value.Dispose();
            }
            info.objDict.Clear();
        }

        #endregion
    }
}