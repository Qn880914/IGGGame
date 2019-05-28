using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace IGG.EditorTools
{
    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.13
    /// Desc    简单的模板引擎, 主要用在编辑器上
    /// </summary>
    public class TemplateEngine
    {
        private static readonly Dictionary<string, string> g_templateCacheMap = new Dictionary<string, string>();

        /// <summary>
        /// 模板文件的基础路径
        /// eg: Assets/Ediotr/Config/Decoder/Template/
        /// 注意： 前面不要“/”，后央要“/”
        /// </summary>
        public static string TemplateBasePath;

        /// <summary>
        /// 处理模板
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="valueDic"></param>
        /// <param name="valueDic2"></param>
        /// <param name="readTemplateCache"></param>
        /// <returns></returns>
        public static string Do(string templatePath, Dictionary<string, string> valueDic,
            Dictionary<string, string> valueDic2, bool readTemplateCache = true)
        {
            if (!string.IsNullOrEmpty(TemplateBasePath))
            {
                templatePath = TemplateBasePath + templatePath;
            }

            string templateStr = null;
            if (readTemplateCache)
            {
                g_templateCacheMap.TryGetValue(templatePath, out templateStr);
            }

            if (templateStr == null)
            {
                string path = EditorHelper.GetProjPath(templatePath);
                try
                {
                    templateStr = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Debug.LogError("读取文件失败: " + e);
                    return null;
                }

                g_templateCacheMap[templatePath] = templateStr;
            }

            if (valueDic != null)
            {
                foreach (KeyValuePair<string, string> pair in valueDic)
                {
                    templateStr = templateStr.Replace("${" + pair.Key + "}", pair.Value);
                }
            }

            if (valueDic2 != null)
            {
                foreach (KeyValuePair<string, string> pair in valueDic2)
                {
                    templateStr = templateStr.Replace("${" + pair.Key + "}", pair.Value);
                }
            }
            return templateStr;
        }

        /// <summary>
        ///  处理列表类型模板
        /// </summary>
        /// <typeparam name="TItemType"></typeparam>
        /// <param name="itemTemplateStr"></param>
        /// <param name="list"></param>
        /// <param name="fieldOrPropNames"></param>
        /// <param name="afterProcessItem">后处理函数</param>
        /// <returns></returns>
        public static string DoList<TItemType>(string itemTemplateStr, TItemType[] list, string[] fieldOrPropNames, Func<string, TItemType,string> afterProcessItem = null)
        {
            Type itemType = typeof(TItemType);
            int len = list.Length;

            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                TItemType item = list[i];
                string template = itemTemplateStr;
                foreach (string name in fieldOrPropNames)
                {
                    string tName = "${" + name + "}";
                    FieldInfo fi = itemType.GetField(name);
                    if (fi != null)
                    {
                        template = template.Replace(tName, fi.GetValue(item).ToString());
                        continue;
                    }

                    PropertyInfo pi = itemType.GetProperty(name);
                    if (pi == null)
                    {
                        Debug.LogWarning(string.Format("no {0} fields or attributes in the {1}", name, itemType));
                        continue;
                    }

                    template = template.Replace(tName, pi.GetValue(item,null).ToString());
                }

                if (afterProcessItem != null)
                {
                    template = afterProcessItem(template, item);
                }
                sb.Append(template);
            }
            return sb.ToString();
        }

        public static bool CreateCodeFile(string path, string templateName, Dictionary<string, string> valueDic)
        {
            string clsStr = Do(templateName + ".txt", valueDic, null, false);
            if (clsStr == null)
            {
                Debug.LogError("模板转化失败, templateName:" + templateName);
                return false;
            }

            try
            {
                path = EditorHelper.GetProjPath("Assets/Scripts/" + path);
                string dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Debug.LogError("dir == null");
                    return false;
                }
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, clsStr);
                Debug.Log(path + "创建成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("创建代码文件失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        /// <summary>
        /// 按头字母大写来格式化名字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            StringWriter sw = new StringWriter();
            bool isFrist = true;
            for (int index = 0; index < value.Length; index++)
            {
                char c = value[index];
                if (c == '_')
                {
                    isFrist = true;
                    continue;
                }

                if (isFrist)
                {
                    if (c >= 'a' && c <= 'z')
                    {
                        c = Char.ToUpper(c);
                    }
                    isFrist = false;
                }
                sw.Write(c);
            }
            sw.Close();
            return sw.ToString();
        }
    }
}