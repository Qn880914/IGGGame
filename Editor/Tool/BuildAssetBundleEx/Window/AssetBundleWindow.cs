using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Unity.AssetBundleBrowser.Editor.Tests")]
namespace AssetBundleBrowser
{
    public class AssetBundleWindow : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        enum Mode
        {
            Configure,
            BuildSetting,
            Inspect,
        }

        private static class Styles
        {
            public static readonly GUIContent[] modeToggles = new GUIContent[]
            {
                EditorGUIUtility.TrTextContent("Configure"),
                EditorGUIUtility.TrTextContent("Build"),
                EditorGUIUtility.TrTextContent("Inspect")
            };

            public static readonly GUIContent refreshIcon = new GUIContent(EditorGUIUtility.FindTexture("Refresh"));

            public static readonly GUIStyle buttongStyle = "LargeButton";
        }

        internal const float kButtonWidth = 150;
        internal const float kToolbarPadding = 15;
        internal const float kMenubarPadding = 32;

        private float m_ToolbarPadding = -1f;
        private float toolbarPadding
        {
            get
            {
                if (this.m_ToolbarPadding == -1f)
                {
                    this.m_ToolbarPadding = EditorStyles.toolbarButton.CalcSize(EditorGUIUtility.IconContent("HelpIcon")).x * 2f + 6f;
                }
                return this.m_ToolbarPadding;
            }
        }

        [SerializeField] AssetBundleWindow.Mode m_Mode = AssetBundleWindow.Mode.Configure;

        [SerializeField] int m_DataSourceIndex;

        /// <summary>
        ///     <para> Configure AssetBundle Tab </para>
        /// </summary>
        [SerializeField] internal AssetBundleWindowConfigureTab manageTab;

        /// <summary>
        ///     <para> Build AssetBundle Setting Tab </para>
        /// </summary>
        [SerializeField] internal AssetBundleWindowBuildTab buildTab;

        /// <summary>
        ///     <para> Inspect AssetBundle Tab</para>
        /// </summary>
        [SerializeField] internal AssetBundleWindowInspectTab inspectTab;

        [SerializeField] internal bool m_MultiDataSource;

        List<AssetBundleData> m_AssetBundleDatas;

        private Texture2D m_TextureRefresh;

        private static AssetBundleWindow s_Instance;
        internal static AssetBundleWindow instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetWindow<AssetBundleWindow>();
                return s_Instance;
            }
        }

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            if (menu != null)
                menu.AddItem(new GUIContent("Custom Sources"), m_MultiDataSource, FlipDataSource);
        }

        internal void FlipDataSource()
        {
            m_MultiDataSource = !m_MultiDataSource;
        }

        private void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            manageTab = new AssetBundleWindowConfigureTab();
            manageTab.OnEnable(subPos, this);
            buildTab = new AssetBundleWindowBuildTab();
            buildTab.OnEnable(this);
            inspectTab = new AssetBundleWindowInspectTab();
            inspectTab.OnEnable(subPos);

            m_TextureRefresh = EditorGUIUtility.FindTexture("Refresh");

            InitDataSources();
        }

        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
            m_MultiDataSource = false;
            m_AssetBundleDatas = new List<AssetBundleData>();
            foreach (var info in AssetBundleDataProvider.customAssetBundleDataTypes)
            {
                m_AssetBundleDatas.AddRange(info.GetMethod("CreateDataSources").Invoke(null, null) as List<AssetBundleData>);
            }

            if (m_AssetBundleDatas.Count > 1)
            {
                m_MultiDataSource = true;
                if (m_DataSourceIndex >= m_AssetBundleDatas.Count)
                    m_DataSourceIndex = 0;
                Model.AssetBundleModel.assetBundleData = m_AssetBundleDatas[m_DataSourceIndex];
            }
        }
        private void OnDisable()
        {
            if (buildTab != null)
                buildTab.OnDisable();

            if (inspectTab != null)
                inspectTab.OnDisable();
        }

        public void OnBeforeSerialize()
        {
        }
        public void OnAfterDeserialize()
        {
        }

        private Rect GetSubWindowArea()
        {
            float padding = kMenubarPadding;
            if (m_MultiDataSource)
                padding += kMenubarPadding * 0.5f;
            Rect subRect = new Rect(0, padding, position.width, position.height - padding);

            return subRect;
        }

        private void Update()
        {
            switch (m_Mode)
            {
                case Mode.BuildSetting:
                    break;
                case Mode.Inspect:
                    break;
                case Mode.Configure:
                default:
                    manageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(kToolbarPadding);
            DrawRefreshGUI();
            this.ModeToggle();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            switch (m_Mode)
            {
                case Mode.BuildSetting:
                    GUILayout.Space(m_TextureRefresh.width + kToolbarPadding);
                    buildTab.OnGUI();
                    break;
                case Mode.Inspect:
                    inspectTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Configure:
                default:
                    manageTab.OnGUI(GetSubWindowArea());
                    break;
            }

            if (m_MultiDataSource)
            {
                //GUILayout.BeginArea(r);
                GUILayout.BeginHorizontal();

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("Bundle Data Source:");
                    GUILayout.FlexibleSpace();
                    var c = new GUIContent(string.Format("{0} ({1})", Model.AssetBundleModel.assetBundleData.name, Model.AssetBundleModel.assetBundleData.providerName), "Select Asset Bundle Set");
                    if (GUILayout.Button(c, EditorStyles.toolbarPopup))
                    {
                        GenericMenu menu = new GenericMenu();

                        for (int index = 0; index < m_AssetBundleDatas.Count; index++)
                        {
                            var assetBundleData = m_AssetBundleDatas[index];
                            if (assetBundleData == null)
                                continue;

                            if (index > 0)
                                menu.AddSeparator("");

                            var counter = index;
                            menu.AddItem(new GUIContent(string.Format("{0} ({1})", assetBundleData.name, assetBundleData.providerName)), false,
                                () =>
                                {
                                    m_DataSourceIndex = counter;
                                    Model.AssetBundleModel.assetBundleData = assetBundleData;
                                    manageTab.ForceReloadData();
                                }
                            );

                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    if (Model.AssetBundleModel.assetBundleData.IsReadOnly())
                    {
                        GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbar)
                        {
                            alignment = TextAnchor.MiddleRight
                        };

                        GUILayout.Label("Read Only", tbLabel);
                    }
                }

                GUILayout.EndHorizontal();
                //GUILayout.EndArea();
            }
        }

        void ModeToggle()
        {
            float toolbarWidth = base.position.width - toolbarPadding * 2;
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, AssetBundleWindow.Styles.modeToggles, AssetBundleWindow.Styles.buttongStyle, new GUILayoutOption[] 
            {
                GUILayout.Width(toolbarWidth)
            } );
        }

        private void DrawRefreshGUI()
        {
            switch (m_Mode)
            {
                case Mode.Inspect:
                    if (GUILayout.Button(Styles.refreshIcon))
                        inspectTab.RefreshBundles();
                    break;
                case Mode.Configure:
                    if (GUILayout.Button(Styles.refreshIcon))
                        manageTab.ForceReloadData();
                    break;
            }
        }

        [MenuItem("Window/AssetBundle Browser", priority = 2050)]
        static void CreateBuildAssetBundleWindow()
        {
            s_Instance = null;
            instance.titleContent = new GUIContent("Build AssetBundle");
            instance.minSize = new Vector2(360f, 390f);
            instance.Show();
        }
    }
}
