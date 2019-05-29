using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class UIToolWnd : EditorWindow
{
    private string m_WndName = "";

    //[@MenuItem("辅助工具/UI制作/制作UI窗体")]
    static void Apply()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "Assets/Editor/ArtTools/UITool/UiEditor.unity")
        {
            EditorSceneManager.SaveScene(activeScene);
            EditorSceneManager.OpenScene("Assets/Editor/ArtTools/UITool/UiEditor.unity");
        }
        EditorWindow.GetWindow(typeof(UIToolWnd));
    }

    void OnGUI()
    {
        this.titleContent = new GUIContent("UI Wnd Make");
        this.minSize = new Vector2(480, 500);

        GUI.Label(new Rect(0, 50, 100, 20), "窗口名称:");
        m_WndName = GUI.TextField(new Rect(100, 50, 200, 20), m_WndName, 18);

        if (GUI.Button(new Rect(10, 80, 100, 30), "生成窗口"))
        {
            CreateWnd(m_WndName, "WndBase");
        }
    }

    private void CreateWnd(string WndName, string BaseWnd)
    {
        if (string.IsNullOrEmpty(WndName) == true)
            return;
        string ClassWnd = "";
        if (WndName.EndsWith("Wnd") == true)
        {
            ClassWnd = WndName;
        }
        else ClassWnd = WndName + "Wnd";

        MakeWnd_HCode(ClassWnd);
        MakeWndCode(ClassWnd, BaseWnd);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "脚本生成完毕", "确定");

        GameObject Prefab = AssetDatabase.LoadAssetAtPath("Assets/Editor/ArtTools/UITool/XXXWnd.prefab", typeof(GameObject)) as GameObject;
        GameObject go = GameObject.Instantiate(Prefab);
        if (null != go)
        {
            go.transform.SetParent(GameObject.Find("UI/Wnd").transform, false);
            go.name = ClassWnd;
            //
            Debug.Log("窗口模本生成完毕");
            //TextAsset code = AssetDatabase.LoadAssetAtPath("Assets/Scripts/FrameWork/Client/UI/Wnd_H/" + ClassWnd + "_h" + ".cs", typeof(TextAsset)) as TextAsset;
            //var assembly = System.Reflection.Assembly.Load(code.bytes);
            //var xxxWnd_h = assembly.GetType(ClassWnd + "_h");
            //Type xxxWnd_h = typeof(WndBase_h).Assembly.GetType(ClassWnd + "_h");
            //go.AddComponent(xxxWnd_h);
        }
    }

    public void DidReloadScripts()
    {


    }

    private void MakeWndCode(string WndName, string BaseWndName)
    {
        string filename = Application.dataPath + "/Scripts/FrameWork/Client/UI/Wnd/" + WndName + ".cs";
        filename.Replace("/", "\\");
        FileStream stream = new FileStream(filename, FileMode.Create);
        StreamWriter file = new StreamWriter(stream);
        file.WriteLine("using UnityEngine;");
        file.WriteLine("using System.Collections;");
        file.WriteLine("using UnityEngine.UI;");
        // 注释
        file.WriteLine("");
        file.WriteLine("// " + WndName + " 窗口" + " by zhulin");
        // 类名开始
        file.WriteLine("public class " + WndName + " : " + BaseWndName + " {");

        // 获取 MyHead
        file.WriteLine("");
        file.WriteLine("    public " + WndName + "_h MyHead {");
        file.WriteLine("        get  {return (base.BaseHead() as " + WndName + "_h);}");
        file.WriteLine("    }");
        // InitWnd 函数开始
        file.WriteLine("");
        file.WriteLine("    // 窗口初始华");
        file.WriteLine("    protected override void InitWnd() {");
        file.WriteLine("    \tRegisterHooks();");
        file.WriteLine("    }");
        // 事件绑定函数
        file.WriteLine("");
        file.WriteLine("    // 窗口内事件绑定");
        file.WriteLine("    protected override void BindEvents() {");
        file.WriteLine("    }");
        // 显示窗口时播放动画接口
        file.WriteLine("");
        file.WriteLine("    // 显示窗口时播放动画");
        file.WriteLine("    public override void PlayShowWndAni(bool IsShow) {");
        file.WriteLine("    }");
        // 注册消息函数
        file.WriteLine("");
        file.WriteLine("    // 注册消息函数");
        file.WriteLine("    private void RegisterHooks() {");
        file.WriteLine("    }");
        // 反注册消息函数
        file.WriteLine("");
        file.WriteLine("    // 反注册消息函数");
        file.WriteLine("    private void AntiRegisterHooks() {");
        file.WriteLine("    }");
        // 消费函数
        file.WriteLine("");
        file.WriteLine("    // 反注册消息函数");
        file.WriteLine("    public virtual void OnDestroy() {");
        file.WriteLine("    \tAntiRegisterHooks();");
        file.WriteLine("    }");

        // 类完成
        file.WriteLine("}");
        file.Close();
        stream.Close();
        Debug.Log(filename + "脚本生成完成");
    }


    private void MakeWnd_HCode(string WndName)
    {
        string Wnd_h = WndName + "_h";

        string filename = Application.dataPath + "/Scripts/FrameWork/Client/UI/Wnd_H/" + Wnd_h + ".cs";
        filename.Replace("/", "\\");
        FileStream stream = new FileStream(filename, FileMode.Create);
        StreamWriter file = new StreamWriter(stream);
        file.WriteLine("using UnityEngine;");
        file.WriteLine("using System.Collections;");
        file.WriteLine("using UnityEngine.UI;");
        // 注释
        file.WriteLine("");
        file.WriteLine("// " + WndName + " 窗口结点配置" + " by zhulin");
        // 类名开始
        file.WriteLine("public class " + Wnd_h + " : WndBase_h {");
        file.WriteLine("");
        // 类完成
        file.WriteLine("}");
        file.Close();
        stream.Close();
        Debug.Log(filename + "脚本生成完成");
    }


}
