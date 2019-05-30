using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IGG.EditorTools;
using IGG.Game.Data.Config;

/// <summary>
/// Author  gaofan
/// Date    2017.12.14
/// Desc    配置文件代码创建器
/// </summary>
public class CfgCodeCreator
{
    private readonly Dictionary<string, string> m_readValueFunMap;
    private readonly Dictionary<char, FunType> m_funMap;

    public  const string IntArrayCode = "IntArray[]";
    public const string UintArrayCode = "UintArray[]";
    public const string StringArrayCode = "StringArray[]";
    public const string FloatArrayCode = "FloatArray[]";

    public CfgCodeCreator()
    {
        m_readValueFunMap = new Dictionary<string, string>(4);
        m_readValueFunMap["int"] = "GetI32(\"{0}\", out excel.{1})";
        m_readValueFunMap["uint"] = "GetU32(\"{0}\", out excel.{1})";
        m_readValueFunMap["string"] = "GetString(\"{0}\", out excel.{1})";
        m_readValueFunMap["float"] = "GetFloat(\"{0}\", out excel.{1})";

        string getArrStr = "GetArr(\"{0}\", StrHelper.ArrSplitLv1, out excel.{1}, ";
        m_readValueFunMap["int[]"] = getArrStr + "ParseI32)";
        m_readValueFunMap["uint[]"] = getArrStr + "ParseU32)";
        m_readValueFunMap["string[]"] = getArrStr + "ParseStr)";
        m_readValueFunMap["float[]"] = getArrStr + "Parsef)";

        m_readValueFunMap[IntArrayCode] = getArrStr + "ParseArr<IntArray, int>)";
        m_readValueFunMap[UintArrayCode] = getArrStr + "ParseArr<UintArray, uint>)";
        m_readValueFunMap[StringArrayCode] = getArrStr + "ParseArr<StringArray, string>)";
        m_readValueFunMap[FloatArrayCode] = getArrStr + "ParseArr<FloatArray, float>)";

        m_funMap = new Dictionary<char, FunType>(2);
        m_funMap['#'] = FunType.Key;
        m_funMap['$'] = FunType.Lang;
    }

    public bool Do(string xmlName, string className = null, bool formatFieldName = true,
        bool formatClassName = true)
    {
        const bool readTemplateCache = false;
        if (className == null)
        {
            className = xmlName;
        }

        if (formatClassName)
        {
            className = FormatName(className);
            if (className.EndsWith("config"))
            {
                className = className.Replace("config", "Config");
            }
        }

        CfgFieldInfo[] fieldInfos = ReadFieldInfo(xmlName, formatFieldName);
        if (fieldInfos == null)
        {
            return false;
        }

        List<ClassInfo> saveList = new List<ClassInfo>(3);
        DateTime dt = DateTime.Now;
        
        Dictionary<string, string> valueDic = new Dictionary<string, string>();
        Dictionary<string, string> gValueDic = new Dictionary<string, string>();
        gValueDic["Author"] = "Robot_CfgCodeCreator";
        gValueDic["CreateDate"] = string.Format("{0}.{1}.{2}", dt.Year, dt.Month, dt.Day);
        gValueDic["XmlName"] = xmlName;

        if (className.EndsWith("Config"))
        {
            string sortName = className.Substring(0, "Config".Length);
            gValueDic["ConfigClsName"] = className;
            gValueDic["CfgDataClsName"] = sortName + "CfgData";
            gValueDic["DaoClsName"] = className + "Dao";
            gValueDic["cfgDecClsName"] = className + "Decoder";
        }
        else
        {
            gValueDic["ConfigClsName"] = className + "Config";
            gValueDic["CfgDataClsName"] = className + "CfgData";
            gValueDic["DaoClsName"] = className + "Dao";
            gValueDic["cfgDecClsName"] = className + "Decoder";
        }
        gValueDic["ConfigDefineFileName"] = gValueDic["ConfigClsName"];

        TemplateEngine.TemplateBasePath = "Assets/Editor/Config/Template/";

        //创建序列化文件
        CreateClsFile(gValueDic["CfgDataClsName"], "/CfgFile", "CfgDataClsTemplate.txt",
            null, gValueDic, null, null, readTemplateCache, saveList, className, false);

        /*
        CreateClsFile(gValueDic["DaoClsName"], "", "CfgDaoClsTemplate.txt",
            fieldInfos, gValueDic, valueDic, new Func<CfgFieldInfo[], Dictionary<string, string>,
            Dictionary<string, string>, string, bool>[] { FillConfig, FillDao }, readTemplateCache, saveList, className);

        CreateClsFile(gValueDic["cfgDecClsName"], "/Decoder", "CfgDecoderTemplate.txt",
            fieldInfos, gValueDic, valueDic, new Func<CfgFieldInfo[], Dictionary<string, string>, 
            Dictionary<string, string>, string, bool>[] { FillDecoder }, readTemplateCache, saveList, className);
        */

        //创建定义文件
        CreateClsFile(gValueDic["ConfigClsName"], "", "CfgDefineTemplate.txt",
            fieldInfos, gValueDic, valueDic, new Func<CfgFieldInfo[], Dictionary<string, string>,
                Dictionary<string, string>, string, bool>[] {FillConfig, FillDao, FillDecoder}, readTemplateCache,
            saveList, className, true);

        //创建自定义扩展文件
        CreateClsFile(gValueDic["ConfigClsName"] +"_Extend", "", "CfgExtendTemplate.txt",
            null, gValueDic, null, null, readTemplateCache, saveList, className, false);

        TemplateEngine.TemplateBasePath = null;

        if (saveList.Count > 0)
        {
            SaveFile(saveList.ToArray());
        }
        return true;
    }

    private bool CreateClsFile(string className, string path, string templateName, CfgFieldInfo[] fieldInfos,
        Dictionary<string, string> gValueDic, Dictionary<string, string> valueDic,
        Func<CfgFieldInfo[], Dictionary<string, string>, Dictionary<string, string>, string, bool>[] fillHandlers,
        bool readTemplateCache, List<ClassInfo> saveList, string fileName,bool canRewrite)
    {
        ClassInfo ci = new ClassInfo();
        ci.Name = className;
        ci.Dir = EditorHelper.GetProjPath("Assets/Scripts/Game/Data/Config" + path);

        if (canRewrite == false && File.Exists(ci.SavePath))
        {
            UnityEngine.Debug.Log(string.Format("{0}不需要更新，已经跳过处理", ci.Name));
            return false;
        }

        if (fillHandlers != null && fillHandlers.Length > 0)
        {
            for (int i = 0; i < fillHandlers.Length; i++)
            {
                var handler = fillHandlers[i];
                if (handler != null)
                {
                    if (!handler(fieldInfos, gValueDic, valueDic, fileName))
                    {
                        UnityEngine.Debug.LogError("填充数据错误:" + handler.Method.Name);
                        return false;
                    }
                }
            }
        }

        ci.Body = TemplateEngine.Do(templateName, gValueDic, valueDic, readTemplateCache);
        if (valueDic != null)
        {
            valueDic.Clear();
        }

        if (ci.Body == null)
        {
            UnityEngine.Debug.LogError("构建" + ci.Name + "失败!");
            return false;
        }

        saveList.Add(ci);
        return true;
    }

    /// <summary>
    /// 填充dao文件
    /// 主要处理了languageCode
    /// </summary>
    /// <param name="fieldInfos"></param>
    /// <param name="gValueDic"></param>
    /// <param name="valueDic"></param>
    /// <param name="readTemplateCache"></param>
    /// <param name="configName"></param>
    /// <returns></returns>
    private bool FillDao(CfgFieldInfo[] fieldInfos, Dictionary<string, string> gValueDic,
        Dictionary<string, string> valueDic, string configName)
    {
        List<CfgFieldInfo> langFields = new List<CfgFieldInfo>(fieldInfos.Length);
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            CfgFieldInfo info = fieldInfos[i];
            if (info.FunType == FunType.Lang)
            {
                langFields.Add(info);
            }
        }

        //创建CfgDao文件
        if (fieldInfos.Length < 1)
        {
            UnityEngine.Debug.LogError("xml没有字段信息" +  "CfgCodeCreator.FillDao");
            return false;
        }

        //处理语言代码
        if (langFields.Count > 0)
        {
            string languageCode = "\r\n\r\n" + TemplateEngine.Do("CfgSwitchLangTemplate.txt", gValueDic, valueDic, false);
            string tryGetCodeStr = "";
            for (int i = 0; i < langFields.Count; i++)
            {
                CfgFieldInfo fieldInfo = langFields[i];
                string tryGetCodeItem = "                strDao.TryGetString(\"{0}.{1}.\" + cfg.GetKey(), language, ref cfg.{2});\r\n";
                tryGetCodeItem = string.Format(tryGetCodeItem, configName, fieldInfo.Name, fieldInfo.Name);
                tryGetCodeStr += tryGetCodeItem;
            }
            tryGetCodeStr = tryGetCodeStr.Remove(tryGetCodeStr.Length - 2);
            valueDic["languageCode"] = languageCode.Replace("${TrGetCode}", tryGetCodeStr);
        }
        else
        {
            valueDic["languageCode"] = "";
        }
        return true;
    }

    /// <summary>
    /// 填充config的数据，主要填充以下字段
    /// FieldList，KeyType，KeyFieldName
    /// 
    /// </summary>
    /// <param name="fieldInfos"></param>
    /// <param name="gValueDic"></param>
    /// <param name="valueDic"></param>
    /// <param name="configName"></param>
    /// <returns></returns>
    private bool FillConfig(CfgFieldInfo[] fieldInfos, Dictionary<string, string> gValueDic,
        Dictionary<string, string> valueDic, string configName)
    {
        List<CfgFieldInfo> keyFields = new List<CfgFieldInfo>(fieldInfos.Length);
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            CfgFieldInfo info = fieldInfos[i];
            if (info.FunType == FunType.Key)
            {
                keyFields.Add(info);
            }

            //处理注释中的换行
            if (!string.IsNullOrEmpty(info.Desc) &&
                (info.Desc.Contains("\r") || info.Desc.Contains("\n")))
            {
                info.Desc = info.Desc.Replace("\r\n", "${newLine}");
                info.Desc = info.Desc.Replace("\n\r", "${newLine}");
                info.Desc = info.Desc.Replace("\n", "${newLine}");
                info.Desc = info.Desc.Replace("\r", "${newLine}");
                info.Desc = info.Desc.Replace("${newLine}", "\r\n        /// ");
            }
        }

        //没有指定主键，默认第一个
        if (keyFields.Count == 0)
        {
            keyFields.Add(fieldInfos[0]);
        }

        //创建CfgDao文件
        if (fieldInfos.Length < 1)
        {
            UnityEngine.Debug.LogError("xml没有字段信息" + "CfgCodeCreator.Do");
            return false;
        }

        if (keyFields.Count == 1)
        {
            //只有一个key的情况
            CfgFieldInfo keyField = keyFields[0];
            valueDic["KeyType"] = keyField.Type;
            valueDic["KeyFieldName"] = keyField.Name;
        }
        else
        {
            valueDic["KeyType"] = "string";
            string keyCode = "";
            for (int i = 0; i < keyFields.Count; i++)
            {
                CfgFieldInfo keyField = keyFields[i];
                if (i == 0)
                {
                    keyCode += keyField.Name + ".ToString()";
                }
                else
                {
                    keyCode += " + \"_\" + " + keyField.Name + ".ToString()";
                }
            }
            valueDic["KeyFieldName"] = keyCode;
        }

        //处理字段声名
        valueDic["FieldList"] = TemplateEngine.DoList("        public ${Type} ${Name};\r\n\r\n",
            fieldInfos, new[] { "Type", "Name" }, (s, info) =>
            {
                if (!string.IsNullOrEmpty(info.Desc))
                {
                    string desc = "        /// <summary>\r\n        /// " + info.Desc + "\r\n        /// </summary>\r\n";
                    s = desc + s;
                }
                return s;
            });
        return true;
    }

    /// <summary>
    /// 填充decoder的内容
    /// </summary>
    /// <param name="fieldInfos"></param>
    /// <param name="gValueDic"></param>
    /// <param name="valueDic"></param>
    /// <param name="configName"></param>
    /// <returns></returns>
    private bool FillDecoder(CfgFieldInfo[] fieldInfos, Dictionary<string, string> gValueDic,
        Dictionary<string, string> valueDic, string configName)
    {
        //创建CfgDecoder文件
        StringBuilder sb = new StringBuilder(fieldInfos.Length);
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            CfgFieldInfo typeInfo = fieldInfos[i];
            if (!m_readValueFunMap.TryGetValue(typeInfo.Type, out string funCode))
            {
                m_readValueFunMap.TryGetValue("string", out funCode);
            }
            if (string.IsNullOrEmpty(funCode))
            {
                funCode = "";
            }
            sb.Append("            ");
            sb.Append(string.Format(funCode, typeInfo.KeyName, typeInfo.Name));
            sb.Append(i == (fieldInfos.Length - 1) ? ";" : ";\r\n");

        }
        valueDic["ReadValueCode"] = sb.ToString();
        return true;
    }

    private void SaveFile(ClassInfo[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            ClassInfo info = list[i];

            try
            {
                if (!Directory.Exists(info.Dir))
                {
                    Directory.CreateDirectory(info.Dir);
                }
                File.WriteAllText(info.SavePath, info.Body);
                UnityEngine.Debug.Log("保存成功:" + info.SavePath + "CfgCodeCreator.SaveFile");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("保存" + info.Name + "文件时出现了错误: " + e +  "CfgCodeCreator.SaveFile");
            }
        }
    }

    /// <summary>
    /// 从xml文件中读出配置信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="formatFieldName"></param>
    /// <returns></returns>
    private CfgFieldInfo[] ReadFieldInfo(string name, bool formatFieldName)
    {
        string path = EditorHelper.GetProjPath("Assets/Config/" + name + ".csv");
        Excel excel = new Excel(-1);
        if (!excel.Load(path))
        {
            return null;
        }

        if (excel.RowCount < 3)
        {
            UnityEngine.Debug.LogError("表定义错误，缺少必要的表头" + "CfgCodeCreator.ReadType");
            return null;
        }

        uint len = excel.ColCount;
        List<CfgFieldInfo> infos = new List<CfgFieldInfo>((int)len);
        for (uint i = 0; i < len; i++)
        {
            IRowReader desc = excel.GetRow(0);
            IRowReader type = excel.GetRow(1);
            IRowReader field = excel.GetRow(2);

            CfgFieldInfo info = new CfgFieldInfo();
            info.Type = type.Get(i);
            switch (info.Type)
            {
                case "sint":
                    info.Type = "int";
                    break;
                case "sint[]":
                    info.Type = "int[]";
                    break;

                case "sint[][]":
                case "int[][]":
                    info.Type = IntArrayCode;
                    break;
                case "uint[][]":
                    info.Type = UintArrayCode;
                    break;
                case "string[][]":
                    info.Type = StringArrayCode;
                    break;
                case "float[][]":
                    info.Type = FloatArrayCode;
                    break;
            }
            info.KeyName = field.Get(i);
            if (string.IsNullOrEmpty(info.KeyName))
            {
                UnityEngine.Debug.LogWarning("表定义错误,字段名为空,程序自动忽略" +  "CfgCodeCreator.ReadType");
                continue;
            }

            if (m_funMap.TryGetValue(info.KeyName[0], out info.FunType))
            {
                info.Name = info.KeyName.Substring(1);
                if (info.Name == "")
                {
                    UnityEngine.Debug.LogWarning("表定义错误,字段名只有功能符,程序自动忽略"+ "CfgCodeCreator.ReadType");
                    continue;
                }
            }
            else
            {
                info.Name = info.KeyName;
            }
            if (formatFieldName)
            {
                info.Name = FormatName(info.Name);
            }
            info.Desc = desc.Get(i);
            infos.Add(info);
        }
        return infos.ToArray();
    }

    /// <summary>
    /// 按头字母大写来格式化名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string FormatName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "";
        }

        StringWriter sw = new StringWriter();
        bool isFrist = true;
        for (int index = 0; index < name.Length; index++)
        {
            char c = name[index];
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
        string newName = sw.ToString();
        if (newName.EndsWith("config"))
        {
            newName = newName.Replace("config", "Config");
        }
        return newName;
    }

    public class ClassInfo
    {
        public string Name;
        public string Dir;
        public string Body;

        public string SavePath
        {
            get { return Dir + "/" + Name + ".cs"; }
        }
    }

    public class CfgFieldInfo
    {
        public string KeyName;
        public string Name;
        public string Desc;
        public string Type;
        public FunType FunType;
    }

    public enum FunType
    {
        /// <summary>
        /// 没有功能
        /// </summary>
        None,

        /// <summary>
        /// 这个字段作为主键
        /// </summary>
        Key,

        /// <summary>
        /// 这个字段需要转换语言
        /// </summary>
        Lang
    }
}