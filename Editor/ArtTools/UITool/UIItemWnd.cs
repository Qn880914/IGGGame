using UnityEditor;
using UnityEngine;
using System.IO;

public class UIItemWnd : EditorWindow
{

    private string m_ItemName = "";

	//[@MenuItem("辅助工具/UI制作/制作UI Item")]
    static void Apply()
    {
		if (EditorApplication.currentScene != "Assets/Editor/ArtTools/UITool/UiEditor.unity")
        {
            EditorApplication.SaveScene(EditorApplication.currentScene);
            EditorApplication.OpenScene("Assets/Editor/ArtTools/UITool/UiEditor.unity");
        }
        EditorWindow.GetWindow(typeof(UIItemWnd));
    }


    void OnGUI()
    {
        this.title = "UI Wnd Make";
        this.minSize = new Vector2(480, 500);

        GUI.Label(new Rect(0, 50, 100, 20), "Item名称:");
        m_ItemName = GUI.TextField(new Rect(100, 50, 200, 20), m_ItemName, 18);

        if (GUI.Button(new Rect(10, 80, 100, 30), "生成Item"))
        {
            CreateItem(m_ItemName, "WndItem");
        }
    }



    private void CreateItem(string ItemName, string BaseItem)
    {
        if (string.IsNullOrEmpty(ItemName) == true)
            return;
        string ClassItem = "";
        if (ItemName.EndsWith("Item") == true)
        {
            ClassItem = ItemName;
        }
        else ClassItem = ItemName + "Item";

        MakeItem_HCode(ClassItem);
        MakeItemCode(ClassItem, BaseItem);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "脚本生成完毕", "确定");

		GameObject Prefab = AssetDatabase.LoadAssetAtPath("Assets/Editor/ArtTools/UITool/XXXItem.prefab", typeof(GameObject)) as GameObject;
        GameObject go = GameObject.Instantiate(Prefab);
        if (null != go)
        {
            go.transform.SetParent(GameObject.Find("UI/Wnd").transform, false);
            go.name = ClassItem;
            //
            Debug.Log("Item模板生成完毕");
        }
    }

    public void DidReloadScripts()
    {


    }

    private void MakeItemCode(string ItemName, string BaseItemName)
    {
        string filename = Application.dataPath + "/Scripts/FrameWork/Client/UI/Items/" + ItemName + ".cs";
        filename.Replace("/", "\\");
        FileStream stream = new FileStream(filename, FileMode.Create);
        StreamWriter file = new StreamWriter(stream);
        file.WriteLine("using UnityEngine;");
        file.WriteLine("using System.Collections;");
        file.WriteLine("using UnityEngine.UI;");
        // 注释
        file.WriteLine("");
        file.WriteLine("// " + ItemName + " item" + " by zhulin");
        // 类名开始
        file.WriteLine("public class " + ItemName + " : " + BaseItemName + " {");

        // 获取 MyHead
        file.WriteLine("");
        file.WriteLine("    public " + ItemName + "_h MyHead {");
        file.WriteLine("        get  {return (base.BaseHead() as " + ItemName + "_h);}");
        file.WriteLine("    }");
        // 类完成
        file.WriteLine("}");
        file.Close();
        stream.Close();
        Debug.Log(filename + "脚本生成完成");
    }


    private void MakeItem_HCode(string ItemName)
    {
        string ItemName_h = ItemName + "_h";

        string filename = Application.dataPath + "/Scripts/FrameWork/Client/UI/Items_H/" + ItemName_h + ".cs";
        filename.Replace("/", "\\");
        FileStream stream = new FileStream(filename, FileMode.Create);
        StreamWriter file = new StreamWriter(stream);
        file.WriteLine("using UnityEngine;");
        file.WriteLine("using System.Collections;");
        file.WriteLine("using UnityEngine.UI;");
        // 注释
        file.WriteLine("");
        file.WriteLine("// " + ItemName + " 窗口结点配置" + " by zhulin");
        // 类名开始
        file.WriteLine("public class " + ItemName_h + " : WndItem_h {");
        file.WriteLine("");
        // 类完成
        file.WriteLine("}");
        file.Close();
        stream.Close();
        Debug.Log(filename + "脚本生成完成");
    }
}
