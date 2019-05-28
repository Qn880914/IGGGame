using UnityEditor;
using UnityEngine;

namespace IGG.EditorTools
{
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
            IGGBuildPlayerWindow wnd = EditorWindow.GetWindow<IGGBuildPlayerWindow>("Build Setting");
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
            EditorGUIUtility.labelWidth = Mathf.Max(180f, (base.position.width - 265f) * 0.47f);

            IGGEditorBuildSettings.configBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.configBuild, IGGEditorBuildSettings.configBuild, new GUILayoutOption[0]);
            IGGEditorBuildSettings.assetBundleBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.assetBundleBuild, IGGEditorBuildSettings.assetBundleBuild, new GUILayoutOption[0]);
            IGGEditorBuildSettings.debugBuild = EditorGUILayout.Toggle(IGGBuildPlayerWindow.styles.debugBuild, IGGEditorBuildSettings.debugBuild, new GUILayoutOption[0]);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(IGGBuildPlayerWindow.styles.build, new GUILayoutOption[] { GUILayout.Width(110f)}))
            {
                IGGBuildPlayerWindow.CallBuildMethods();
            }
        }

        private static void CallBuildMethods()
        {
            if(IGGEditorBuildSettings.configBuild)
                Cfg2AssetsTool.EncodeAllCfg();

            if (IGGEditorBuildSettings.assetBundleBuild)
                IGGBuildPipeline.BuildAssetbundle();
        }
    }
}