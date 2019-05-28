using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace IGG.EditorTools.AssetCheck
{
    /// <summary>
    /// 自动化检查入口
    /// 需要运行的检查方法挂上AutoCheckItem就行了
    /// author: gaofan
    /// </summary>
    public static class AssetAutoCheck
    {

        public static void Start(string path)
        {
            AssetCheckLogger.Stream = new StreamWriter(path);
            var checkItems = GetAllCheckItem();
            for (int i = 0; i < checkItems.Length; i++)
            {
                var item = checkItems[i];
                AssetCheckLogger.Log("=====================================");
                if (string.IsNullOrEmpty(item.Name))
                {
                    AssetCheckLogger.Log(item.Method.Name);
                }
                else
                {
                    AssetCheckLogger.Log(item.Name);
                }
                AssetCheckLogger.Log("=====================================");
                try
                {
                    item.Method.Invoke(null, null);
                }
                catch (Exception e)
                {
                    AssetCheckLogger.LogError("执行" + item.Name + "时发生错误:");
                    AssetCheckLogger.LogError(e.ToString());
                }
            }
            AssetCheckLogger.Stream.Flush();
            AssetCheckLogger.Stream.Close();
        }

        private static CheckItemVo[] GetAllCheckItem()
        {
            List<CheckItemVo> result = new List<CheckItemVo>();
            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            Type autoCheckItemType = typeof(AutoCheckItem);
            foreach (Assembly assembly in assemblys)
            {
                if (!assembly.GetName().Name.StartsWith("Assembly-CSharp"))
                {
                    continue;
                }

                Type[] types = assembly.GetExportedTypes();
                foreach (Type type in types)
                {
                    var allMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    for (int i = 0; i < allMethods.Length; i++)
                    {
                        var methodInfo = allMethods[i];
                        var attributes = methodInfo.GetCustomAttributes(autoCheckItemType, true);
                        if (attributes.Length < 1)
                        {
                            continue;
                        }
                        AutoCheckItem checkItem = attributes[0] as AutoCheckItem;
                        if (checkItem == null)
                        {
                            continue;
                        }

                        CheckItemVo itemVo = new CheckItemVo();
                        itemVo.Method = methodInfo;
                        itemVo.Name = checkItem.Name;

						result.Add(itemVo);
					}
                }
            }
            return result.ToArray();
        }

        class CheckItemVo
        {
            public string Name;
            public MethodInfo Method;
        }
    }

    public class AssetCheckLogger
    {
        public static StreamWriter Stream;

        private static int m_logCount = 0;

        public static void Log(string msg)
        {
            m_logCount++;
            msg = m_logCount + " " + msg;
            Debug.Log(msg);
            if (Stream != null)
            {
                Stream.WriteLine(msg);
            }
        }

        public static void LogWarning(string msg)
        {
            m_logCount++;
            msg = m_logCount + " [Warning]" + msg;
            Debug.LogWarning(msg);
            if (Stream != null)
            {
                Stream.WriteLine(msg);
            }
        }

        public static void LogError(string msg)
        {
            m_logCount++;
            msg = m_logCount + " [Error]" + msg;
            Debug.LogError(msg);
            if (Stream != null)
            {
                Stream.WriteLine(msg);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AutoCheckItem : Attribute
    {
        public string Name { private set; get; }

        /// <summary>
        /// 检查项的名字
        /// </summary>
        /// <param name="name"></param>
        public AutoCheckItem(string name)
        {
            Name = name;
        }
    }
}
