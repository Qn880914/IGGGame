using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Unity.AssetBundleBrowser.Editor.Tests")]

namespace AssetBundleBrowser
{
    public class AssetBundleBrowserMain : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        enum Mode
        {
            Browser,
            Builder,
            Inspect,
        }

        internal const float kButtonWidth = 150;
        internal const float kToolbarPadding = 15;
        internal const float kMenubarPadding = 32;

        [SerializeField] Mode m_Mode;

        [SerializeField] int m_DataSourceIndex;

        /// <summary>
        ///     <para> Configure AssetBundle Tab </para>
        /// </summary>
        [SerializeField] internal AssetBundleManageTab manageTab;

        /// <summary>
        ///     <para> Build AssetBundle Setting Tab </para>
        /// </summary>
        [SerializeField] internal AssetBundleBuildTab buildTab;

        /// <summary>
        ///     <para> Inspect AssetBundle Tab</para>
        /// </summary>
        [SerializeField] internal AssetBundleInspectTab inspectTab;

        [SerializeField] internal bool m_MultiDataSource;

        List<AssetBundleData> m_AssetBundleDatas;

        private Texture2D m_TextureRefresh;

        private static AssetBundleBrowserMain s_Instance;
        internal static AssetBundleBrowserMain instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetWindow<AssetBundleBrowserMain>();
                return s_Instance;
            }
        }

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
            if(manageTab == null)
                manageTab = new AssetBundleManageTab();
            manageTab.OnEnable(subPos, this);

            if(buildTab == null)
                buildTab = new AssetBundleBuildTab();
            buildTab.OnEnable(this);

            if (inspectTab == null)
                inspectTab = new AssetBundleInspectTab();
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
                AssetBundleModel.Model.assetBundleData = m_AssetBundleDatas[m_DataSourceIndex];
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
                case Mode.Builder:
                    break;
                case Mode.Inspect:
                    break;
                case Mode.Browser:
                default:
                    manageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            ModeToggle();

            switch(m_Mode)
            {
                case Mode.Builder:
                    buildTab.OnGUI();
                    break;
                case Mode.Inspect:
                    inspectTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Browser:
                default:
                    manageTab.OnGUI(GetSubWindowArea());
                    break;
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(kToolbarPadding);
            switch(m_Mode)
            {
                case Mode.Browser:
                    if (GUILayout.Button(m_TextureRefresh))
                        manageTab.ForceReloadData();
                    break;
                case Mode.Builder:
                    GUILayout.Space(m_TextureRefresh.width + kToolbarPadding);
                    break;
                case Mode.Inspect:
                    if (GUILayout.Button(m_TextureRefresh))
                        inspectTab.RefreshBundles();
                    break;
            }

            float toolbarWidth = position.width - kToolbarPadding * 4 - m_TextureRefresh.width;
            //string[] labels = new string[2] { "Configure", "Build"};
            string[] labels = { "Configure", "Build", "Inspect" };
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
                    var c = new GUIContent(string.Format("{0} ({1})", AssetBundleModel.Model.assetBundleData.name, AssetBundleModel.Model.assetBundleData.providerName), "Select Asset Bundle Set");
                    if (GUILayout.Button(c , EditorStyles.toolbarPopup) )
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
                                    AssetBundleModel.Model.assetBundleData = assetBundleData;
                                    manageTab.ForceReloadData();
                                }
                            );

                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    if (AssetBundleModel.Model.assetBundleData.IsReadOnly())
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
    }
}
