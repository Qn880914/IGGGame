using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Unity.AssetBundleBrowser.Editor.Tests")]

namespace AssetBundleBrowser
{
    public class AssetBundleBrowserMain : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        private static AssetBundleBrowserMain s_Instance = null;
        internal static AssetBundleBrowserMain instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetWindow<AssetBundleBrowserMain>();
                return s_Instance;
            }
        }

        internal const float kButtonWidth = 150;
        internal const float kToolbarPadding = 15;
        internal const float kMenubarPadding = 32;

        enum Mode
        {
            Browser,
            Builder,
            Inspect,
        }

        [SerializeField] Mode m_Mode;

        [SerializeField] int m_DataSourceIndex;

        [SerializeField] internal AssetBundleManageTab m_ManageTab;

        [SerializeField] internal AssetBundleBuildTab m_BuildTab;

        [SerializeField] internal AssetBundleInspectTab m_InspectTab;

        [SerializeField] internal bool m_MultiDataSource = false;

        List<AssetBundleDataSource.ABDataSource> m_DataSourceList = null;

        private Texture2D m_TextureRefresh;

        [MenuItem("Window/AssetBundle Browser", priority = 2050)]
        static void ShowWindow()
        {
            s_Instance = null;
            instance.titleContent = new GUIContent("AssetBundles");
            instance.Show();
        }

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            if(menu != null)
               menu.AddItem(new GUIContent("Custom Sources"), m_MultiDataSource, FlipDataSource);
        }

        internal void FlipDataSource()
        {
            m_MultiDataSource = !m_MultiDataSource;
        }

        private void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            if(m_ManageTab == null)
                m_ManageTab = new AssetBundleManageTab();
            m_ManageTab.OnEnable(subPos, this);

            if(m_BuildTab == null)
                m_BuildTab = new AssetBundleBuildTab();
            m_BuildTab.OnEnable(this);

            if (m_InspectTab == null)
                m_InspectTab = new AssetBundleInspectTab();
            m_InspectTab.OnEnable(subPos);

            m_TextureRefresh = EditorGUIUtility.FindTexture("Refresh");

            InitDataSources();
        } 

        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
            m_MultiDataSource = false;
            m_DataSourceList = new List<AssetBundleDataSource.ABDataSource>();
            foreach (var info in AssetBundleDataSource.ABDataSourceProviderUtility.CustomABDataSourceTypes)
            {
                m_DataSourceList.AddRange(info.GetMethod("CreateDataSources").Invoke(null, null) as List<AssetBundleDataSource.ABDataSource>);
            }
             
            if (m_DataSourceList.Count > 1)
            {
                m_MultiDataSource = true;
                if (m_DataSourceIndex >= m_DataSourceList.Count)
                    m_DataSourceIndex = 0;
                AssetBundleModel.Model.DataSource = m_DataSourceList[m_DataSourceIndex];
            }
        }
        private void OnDisable()
        {
            if (m_BuildTab != null)
                m_BuildTab.OnDisable();
            if (m_InspectTab != null)
                m_InspectTab.OnDisable();
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
                case Mode.Builder:
                    break;
                case Mode.Inspect:
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            ModeToggle();

            switch(m_Mode)
            {
                case Mode.Builder:
                    m_BuildTab.OnGUI();
                    break;
                case Mode.Inspect:
                    m_InspectTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.OnGUI(GetSubWindowArea());
                    break;
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(kToolbarPadding);
            bool clicked = false;
            switch(m_Mode)
            {
                case Mode.Browser:
                    clicked = GUILayout.Button(m_TextureRefresh);
                    if (clicked)
                        m_ManageTab.ForceReloadData();
                    break;
                case Mode.Builder:
                    GUILayout.Space(m_TextureRefresh.width + kToolbarPadding);
                    break;
                case Mode.Inspect:
                    clicked = GUILayout.Button(m_TextureRefresh);
                    if (clicked)
                        m_InspectTab.RefreshBundles();
                    break;
            }

            float toolbarWidth = position.width - kToolbarPadding * 4 - m_TextureRefresh.width;
            //string[] labels = new string[2] { "Configure", "Build"};
            string[] labels = new string[3] { "Configure", "Build", "Inspect" };
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth) );
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(m_MultiDataSource)
            {
                //GUILayout.BeginArea(r);
                GUILayout.BeginHorizontal();

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("Bundle Data Source:");
                    GUILayout.FlexibleSpace();
                    var c = new GUIContent(string.Format("{0} ({1})", AssetBundleModel.Model.DataSource.name, AssetBundleModel.Model.DataSource.providerName), "Select Asset Bundle Set");
                    if (GUILayout.Button(c , EditorStyles.toolbarPopup) )
                    {
                        GenericMenu menu = new GenericMenu();

                        for (int index = 0; index < m_DataSourceList.Count; index++)
                        {
                            var ds = m_DataSourceList[index];
                            if (ds == null)
                                continue;

                            if (index > 0)
                                menu.AddSeparator("");
                             
                            var counter = index;
                            menu.AddItem(new GUIContent(string.Format("{0} ({1})", ds.name, ds.providerName)), false,
                                () =>
                                {
                                    m_DataSourceIndex = counter;
                                    var thisDataSource = ds;
                                    AssetBundleModel.Model.DataSource = thisDataSource;
                                    m_ManageTab.ForceReloadData();
                                }
                            );

                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    if (AssetBundleModel.Model.DataSource.IsReadOnly())
                    {
                        GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbar);
                        tbLabel.alignment = TextAnchor.MiddleRight;

                        GUILayout.Label("Read Only", tbLabel);
                    }
                }

                GUILayout.EndHorizontal();
                //GUILayout.EndArea();
            }
        }


    }
}
