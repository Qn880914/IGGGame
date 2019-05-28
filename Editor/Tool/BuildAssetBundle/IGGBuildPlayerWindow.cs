using UnityEditor;
using UnityEngine;

namespace IGG.EditorTools
{
    /*public class BuildApkWnd : EditorWindow
    {
        [MenuItem("版本发布/出版本")]
        static void ShowBuildApkWnd()
        {
            EditorUtility.ClearProgressBar();
            BuildApkWnd wnd = EditorWindow.GetWindow<BuildApkWnd>("打包工具");
            wnd.minSize = new Vector2(200, 250);
        }

        void OnGUI()
        {
            if (m_toggleArr == null)
            {
                InitGUI();
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

            m_createBtn.Rect.y = position.height - m_createBtn.Rect.height - 10;
            m_createBtn.Draw();
            m_selAllBtn.Rect.y = m_createBtn.Rect.y;
            m_selAllBtn.Draw();
        }

        public const string ProxyCodePathForamt = "FrameWork/ComProxy/Module/${moduleName}ModuleProxy.cs";

        private void InitGUI()
        {
            const int itemW = 250;
            const int itemH = 17;

            List<Toggle> list = new List<Toggle>();
            Func<string, Action, Toggle> addItem =
                (label, act) =>
                {
                    Toggle item = new Toggle();
                    item.Label = label;
                    item.Data = act;
                    item.Rect = new Rect(20, list.Count*itemH, itemW, itemH);
                    list.Add(item);
                    return item;
                };

            addItem("1.xlua生成", CSObjectWrapEditor.Generator.GenAll);
            addItem("2.序列化配置", Cfg2AssetsTool.EncodeAllCfg);
            addItem("3.打AB包", BuildAssetBundle.Build);
            m_debugToggle = addItem("4.标记为Debug版本",null);
            // m_apkToggle = addItem("5.编译安装包", () =>
            // {
            //     BuildVersionWnd.SwitchPlatform(EditorUserBuildSettings.activeBuildTarget);
            //     BuildVersionWnd.CreateFullVersion("", "", m_debugToggle.Select, m_debugToggle.Select);
            // });

            m_debugToggle.OnChange = OnChangeBuildDebug;
            // m_apkToggle.OnChange = OnChangeBuildApk;

            m_toggleArr = list.ToArray();

            m_createBtn.Rect.Set(10, 0, 120, 30);
            m_createBtn.Label = "打包";
            m_createBtn.OnClick = OnClick;

            m_selAllBtn.Rect = m_createBtn.Rect;
            m_selAllBtn.Rect.x = m_createBtn.Rect.xMax + 10;
            m_selAllBtn.Label = "选择全部";
            m_selAllBtn.OnClick = OnSelAllBtnClick;
        }

        private void OnChangeBuildDebug(Toggle toggle)
        {
            if (toggle.Select)
            {
                m_apkToggle.Select = true;
            }
        }

        private void OnChangeBuildApk(Toggle toggle)
        {
            if (!toggle.Select)
            {
                m_debugToggle.Select = false;
            }
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
            / *
            m_genXlua = addItem("1.XLUA生成", 0);
            m_packageAb = addItem("2.序列化配置", 1);
            m_buildConfig = addItem("3.打AB包", 2);
            m_debugToggle = addItem("4.标记为Debug版本", 3);
            m_apkToggle = addItem("5.编译APK包", 4);
            * /

            float allCount = 0;
            foreach (Toggle toggle in m_toggleArr)
            {
                if (toggle.Select)
                {
                    allCount++;
                }
            }

            float index = 0;
            bool cannotNext = false;
            for (int i = 0; i < m_toggleArr.Length; i++)
            {
                if (cannotNext)
                {
                    EditorUtility.DisplayDialog("提示", "已取消打包", "确认");
                    break;
                }

                Toggle toggle = m_toggleArr[i];
                if (!toggle.Select)
                {
                    continue;
                }
                cannotNext = EditorUtility.DisplayCancelableProgressBar("打包进度", toggle.Label, index/allCount);
                var callback = toggle.Data as Action;
                if (callback != null)
                {
                    Debug.Log("开始执行：" + toggle.Label);
                    try
                    {
                        callback();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(toggle.Label + "错误: " + e);
                    }
                    Debug.Log("执行完毕：" + toggle.Label);
                }
                index++;
               
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "打包完成", "确定");
        }

        private Toggle[] m_toggleArr;
        private readonly ScrollView m_scrollView = new ScrollView();
        private readonly Button m_createBtn = new Button();
        private readonly Button m_selAllBtn = new Button();
        private Toggle m_debugToggle;
        private Toggle m_apkToggle;
    }*/

    public class IGGBuildPlayerWindow : EditorWindow
    {
        private class Styles
        {
            public GUIContent configBuild;

            public GUIContent assetBundleBuild;

            public GUIContent debugBuild;

            public GUIContent build;

            public GUIStyle toggle = "Toggle";

            public Vector2 toggleSize;

            public Styles()
            {
                this.configBuild = EditorGUIUtility.TrTextContent("Serialize Config");
                this.assetBundleBuild = EditorGUIUtility.TrTextContent("Build AssetBundle");
                this.debugBuild = EditorGUIUtility.TrTextContent("Development Build");
                this.build = EditorGUIUtility.TrTextContent("Build");
            }
        }

        private static IGGBuildPlayerWindow.Styles styles = null;

        [MenuItem("版本发布/出版本")]
        static void ShowWindow()
        {
            EditorUtility.ClearProgressBar();
            IGGBuildPlayerWindow wnd = EditorWindow.GetWindow<IGGBuildPlayerWindow>("打包工具");
            wnd.minSize = new Vector2(200, 250);
        }

        private void OnGUI()
        {
            if(null == IGGBuildPlayerWindow.styles)
            {
                IGGBuildPlayerWindow.styles = new Styles();
                IGGBuildPlayerWindow.styles.toggleSize = IGGBuildPlayerWindow.styles.toggle.CalcSize(new GUIContent("X"));
            }

            GUILayout.Space(5f);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            this.ShowBuildSetting();
            GUILayout.EndVertical();

        }

        private void ShowBuildSetting()
        {
            EditorGUIUtility.labelWidth = Mathf.Min(180f, (base.position.width - 265f) * 0.47f);

            IGGEditorBuildSettings.configBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.configBuild, IGGEditorBuildSettings.configBuild, new GUILayoutOption[0]);
            IGGEditorBuildSettings.assetBundleBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.assetBundleBuild, IGGEditorBuildSettings.assetBundleBuild, new GUILayoutOption[0]);
            IGGEditorBuildSettings.debugBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.debugBuild, IGGEditorBuildSettings.debugBuild, new GUILayoutOption[0]);
        }
    }
}