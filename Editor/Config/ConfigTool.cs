using IGG.EditorTools;
using IGG.EditorTools.UI;
using IGG.Game.Data.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author  gaofan
/// Date    2017.12.14
/// Desc    配置文件代码创建器
/// </summary>
public class Cfg2AssetsTool : ScriptableObject
{
    [MenuItem("辅助工具/配置相关/代码生成")]
    public static void ShowCfgCodeCreateWnd()
    {
        string cfgPath = EditorHelper.GetProjPath("Assets/Config/");
        string[] allCfg = Directory.GetFiles(cfgPath, "*.csv", SearchOption.AllDirectories);
        for (int i = 0; i < allCfg.Length; i++)
        {
            string s = allCfg[i];
            allCfg[i] = Path.GetFileName(s);
        }
        CfgCodeCreateWnd wnd = EditorWindow.GetWindow<CfgCodeCreateWnd>("配置代码生成工具");
        wnd.minSize = new Vector2(200, 250);
        wnd.SetCfg(allCfg);
    }

    [MenuItem("辅助工具/配置相关/配置序列化")]
    public static void ShowCfg2AssetsWnd()
    {
        Cfg2AssetsWnd wnd = EditorWindow.GetWindow<Cfg2AssetsWnd>("配置序列化工具");
        wnd.minSize = new Vector2(200, 250);
    }

    
    
    /// <summary>
    /// 得到全部配置的decoder
    /// </summary>
    /// <returns></returns>
    public static Type[] GetAllCfgDecoder()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> list = new List<Type>();
        foreach (Assembly assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (Type type in types)
            {
                var interfaces = type.GetInterfaces();
                if(interfaces.Contains(typeof(ICfgDecoder)) && type.IsAbstract == false)
                {
                    list.Add(type);
                }
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 编码所有的配置文件
    /// </summary>
    public static void EncodeAllCfg()
    {
        EncodeCfgs(GetAllCfgDecoder());
    }

    /// <summary>
    /// 给所列表中的配置文件编码
    /// </summary>
    /// <param name="decoders"></param>
    /// <returns></returns>
    public static void EncodeCfgs(Type[] decoders)
    {
        foreach (Type decoderType in decoders)
        {
            ICfgDecoder decoder = Activator.CreateInstance(decoderType) as ICfgDecoder;
            if (decoder == null)
            {
                Debug.LogError("decoder == null, type = " + decoderType);
                continue;
            }

            if (!decoder.Enable)
            {
                continue;
            }

            ICfgReader reader = ReadCfg(decoder.GetName());
            if (reader == null)
            {
                continue;
            }

            if (!decoder.Decode(reader))
            {
                continue;
            }

            Save(decoder);
        }
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="cfgName"></param>
    /// <returns></returns>
    private static ICfgReader ReadCfg(string cfgName)
    {
        string path = EditorHelper.GetProjPath("Assets/Config/" + cfgName + ".csv");
        Excel excel = new Excel(2);
        if (!excel.Load(path))
        {
            return null;
        }
        return excel;
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="decoder"></param>
    /// <returns></returns>
    private static bool Save(ICfgDecoder decoder)
    {
        string path = "Assets/Data/config/" + decoder.GetSaveName();
        ScriptableObject data = decoder.Data as ScriptableObject;
        if (data == null)
        {
            Debug.LogError("Save error：" + path);
            return false;
        }

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("Save success: " + path);
        return true;
    }


    [MenuItem("辅助工具/战斗相关/兵团位置编辑器")]
    public static void OpenTroopEditor()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scene/TroopEditor.unity");
        EditorApplication.ExecuteMenuItem("Edit/Play");
        Selection.activeTransform = GameObject.Find("Root/Editor").transform;
    }
}

/// <summary>
/// Author  gaofan
/// Date    2017.12.15
/// Desc    配置文件代码序列化工具窗口
/// </summary>
public class Cfg2AssetsWnd : EditorWindow
{
    void OnGUI()
    {
        if (m_toggleArr == null)
        {
            CreateList();
            return;
        }

        m_scrollView.Position.Set(0, 0, position.width, position.height - m_createBtn.Rect.height - 20);
        m_scrollView.ViewRect.width = position.width - 20;
        m_scrollView.Begin();

        for (int i = 0; i < m_toggleArr.Length; i++)
        {
            Toggle item = m_toggleArr[i];
            item.Draw();
        }

        m_scrollView.End();
        //m_createBtn.Rect.x = (position.width - m_createBtn.Rect.width) / 2;
        m_createBtn.Rect.y = position.height - m_createBtn.Rect.height - 10;
        m_createBtn.Draw();

        m_selAllBtn.Rect.y = m_createBtn.Rect.y;
        m_selAllBtn.Draw();
    }

    private void CreateList()
    {
        const int itemW = 200;
        const int itemH = 17;

        Type[] types = Cfg2AssetsTool.GetAllCfgDecoder();
        List<Toggle> list = new List<Toggle>(types.Length);
        foreach (var decoderType in types)
        {
            Toggle toggle = new Toggle();
            toggle.Data = decoderType;
            toggle.Label = decoderType.Name;
            toggle.Rect = new Rect(20, list.Count * itemH, itemW, itemH);
            list.Add(toggle);
        }
        
        m_toggleArr = list.ToArray();
        m_scrollView.ViewRect.height = m_toggleArr.Length * itemH;
        m_createBtn.Rect.Set(10, 0, 120, 30);
        m_createBtn.Label = "生成配置资源";
        m_createBtn.OnClick = OnClick;

        m_selAllBtn.Rect = m_createBtn.Rect;
        m_selAllBtn.Rect.x = m_createBtn.Rect.xMax + 10;
        m_selAllBtn.Label = "选择全部";
        m_selAllBtn.OnClick = OnSelAllBtnClick;
    }

    private void OnSelAllBtnClick(Button button)
    {
        foreach (Toggle toggle in m_toggleArr)
        {
            toggle.Select = !m_selAllBtn.Select;
        }

        m_selAllBtn.Select = !m_selAllBtn.Select;
        m_selAllBtn.Label = m_selAllBtn.Select ? "取消全部" : "选择全部";
    }

    private void OnClick(Button button)
    {
        List<Type> cfgs = new List<Type>(m_toggleArr.Length);
        foreach (Toggle toggle in m_toggleArr)
        {
            if (!toggle.Select)
            {
                continue;
            }

            Type decoderType = toggle.Data as Type;
            if (decoderType == null)
            {
                Debug.LogError("decoderType == null, " + toggle.Data);
                continue;;
            }

            cfgs.Add(decoderType);
        }

        Cfg2AssetsTool.EncodeCfgs(cfgs.ToArray());
        EditorUtility.DisplayDialog("提示", "处理完毕", "确认");
        AssetDatabase.Refresh();
    }

    public bool Save(ICfgDecoder decoder)
    {
        string path = "Assets/Resources/Config/" + decoder.GetSaveName();
        ScriptableObject data = decoder.Data as ScriptableObject;
        if (data == null)
        {
            Debug.LogError("Save error：" + path);
            return false;
        }

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("Save success: " + path);
        return true;
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="cfgName"></param>
    /// <returns></returns>
    private ICfgReader ReadCfg(string cfgName)
    {
        string path = EditorHelper.GetProjPath("Assets/Config/" + cfgName + ".csv");
        Excel excel = new Excel(2);
        if (!excel.Load(path))
        {
            return null;
        }
        return excel;
    }

    private Toggle[] m_toggleArr;
    private readonly ScrollView m_scrollView = new ScrollView();
    private readonly Button m_createBtn = new Button();
    private readonly Button m_selAllBtn = new Button();
}

/// <summary>
/// Author  gaofan
/// Date    2017.12.14
/// Desc    配置文件工具的界面
/// </summary>
public class CfgCodeCreateWnd : EditorWindow
{
    public void SetCfg(string[] cfgNames)
    {
        //分出有文件和没文件的区别
        List<string> existList = new List<string>();
        List<string> newList= new List<string>();
        string basePath = EditorHelper.GetProjPath("Assets/Scripts/Game/Data/Config");
        for (int i = 0; i < cfgNames.Length; i++)
        {
            string cfgName = cfgNames[i];
            string prePath = basePath + "/" + CfgCodeCreator.FormatName(Path.GetFileNameWithoutExtension(cfgName));
            if (File.Exists(prePath + "Dao.cs") || 
                File.Exists(prePath + "Config.cs"))
            {
                existList.Add(cfgName);
            }
            else
            {
                newList.Add(cfgName);
            }
        }

        const int itemW = 200;
        const int itemH = 17;

        List<Toggle> toggleList = new List<Toggle>(cfgNames.Length);
        for (int i = 0; i < newList.Count; i++)
        {
            Toggle item = new Toggle();
            item.Label = newList[i];
            item.Data = newList[i];
            item.Rect = new Rect(20, toggleList.Count * itemH, itemW, itemH);
            toggleList.Add(item);
        }
        for (int i = 0; i < existList.Count; i++)
        {
            Toggle item = new Toggle();
            item.Label = "[有]" + existList[i];
            item.Data = existList[i];
            item.Rect = new Rect(20, toggleList.Count * itemH, itemW, itemH);
            toggleList.Add(item);
        }
        m_toggleArr = toggleList.ToArray();
        m_scrollView.ViewRect.height = cfgNames.Length*itemH;

        m_createBtn.Rect.Set(10,0,120,30);
        m_createBtn.Label = "创建配置代码";
        m_createBtn.OnClick = OnClick;
    }

    private void OnClick(Button button)
    {
        bool success = true;
        foreach (Toggle toggle in m_toggleArr)
        {
            if (toggle.Select)
            {
                string xmlName = Path.GetFileNameWithoutExtension(toggle.Data as string);
                if (!m_codeCreator.Do(xmlName))
                {
                    EditorUtility.DisplayDialog("错误", "生成" + toggle.Label + "代码时出现错误,请查看LOG", "确认");
                    success = false;
                }
            }
        }

        if (success)
        {
            EditorUtility.DisplayDialog("成功", "代码创建成功", "确认");
            AssetDatabase.Refresh();
        }
    }

    void OnGUI()
    {
        if (m_toggleArr == null)
        {
            return;
        }

        m_scrollView.Position.Set(0, 0, position.width, position.height - m_createBtn.Rect.height - 20);
        m_scrollView.ViewRect.width = position.width - 20;
        m_scrollView.Begin();

        for (int i = 0; i < m_toggleArr.Length; i++)
        {
            Toggle item = m_toggleArr[i];
            item.Draw();
        }

        m_scrollView.End();

        m_createBtn.Rect.x = (position.width - m_createBtn.Rect.width)/2;
        m_createBtn.Rect.y = position.height - m_createBtn.Rect.height - 10;
        m_createBtn.Draw();
    }

    private readonly ScrollView m_scrollView = new ScrollView();
    private Toggle[] m_toggleArr;
    private readonly Button m_createBtn = new Button();
    
    private readonly CfgCodeCreator m_codeCreator = new CfgCodeCreator();
}