using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;

namespace AssetBundleBrowser
{
    [System.Serializable]
    internal class AssetBundleWindowConfigureTab 
    {
        [SerializeField] private TreeViewState m_BundleTreeState;

        [SerializeField] private TreeViewState m_AssetListState;

        [SerializeField] private MultiColumnHeaderState m_AssetListMCHState;

        [SerializeField] private TreeViewState m_BundleDetailState;

        [SerializeField] private float m_HorizontalSplitterPercent;

        [SerializeField] private float m_VerticalSplitterPercentRight;

        [SerializeField] private float m_VerticalSplitterPercentLeft;

        private SearchField m_SearchField;

        private const float kSplitterWidth = 3f;

        private static float s_UpdateDelay = 0f;

        private Rect m_Position;

        private AssetBundleWindowConfigureTabAssetBundleTreeView m_BundleTreeView;

        private AssetBundleWindowConfigureTabAssetListTreeView m_AssetTreeView;

        private AssetBundleWindowConfigureTabMessageList m_MessageList;

        private AssetBundleWindowConfigureTabAssetBundleDetailTreeView m_DetailTreeView;

        private bool m_ResizingHorizontalSplitter = false;
        private bool m_ResizingVerticalSplitterRight = false;
        private bool m_ResizingVerticalSplitterLeft = false;
        private Rect m_HorizontalSplitterRect, m_VerticalSplitterRectRight, m_VerticalSplitterRectLeft;

        private EditorWindow m_Parent = null;

        internal AssetBundleWindowConfigureTab()
        {
            m_HorizontalSplitterPercent = 0.4f;
            m_VerticalSplitterPercentRight = 0.7f;
            m_VerticalSplitterPercentLeft = 0.85f;
        }

        internal void OnEnable(Rect pos, EditorWindow parent)
        {
            m_Parent = parent;
            m_Position = pos;
            m_HorizontalSplitterRect = new Rect(
                (int)(m_Position.x + m_Position.width * m_HorizontalSplitterPercent),
                m_Position.y,
                kSplitterWidth,
                m_Position.height);
            m_VerticalSplitterRectRight = new Rect(
                m_HorizontalSplitterRect.x,
                (int)(m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight),
                (m_Position.width - m_HorizontalSplitterRect.width) - kSplitterWidth,
                kSplitterWidth);
            m_VerticalSplitterRectLeft = new Rect(
                m_Position.x,
                (int)(m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft),
                (m_HorizontalSplitterRect.width) - kSplitterWidth,
                kSplitterWidth);

            m_SearchField = new SearchField();
        }

        internal void Update()
        {
            var t = Time.realtimeSinceStartup;
            if (t - s_UpdateDelay > 0.1f ||
                s_UpdateDelay > t) //something went strangely wrong if this second check is true.
            {
                s_UpdateDelay = t - 0.001f;

                if(Model.AssetBundleModel.Update())
                    m_Parent.Repaint();

                if (m_DetailTreeView != null)
                    m_DetailTreeView.Update();

                if (m_AssetTreeView != null)
                    m_AssetTreeView.Update();

            }
        }

        internal void ForceReloadData()
        {
            UpdateSelectedBundles(new List<Model.AssetBundleInfo>());
            SetSelectedItems(new List<Model.AssetInfo>());
            m_BundleTreeView.SetSelection(new int[0]);
            Model.AssetBundleModel.ForceReloadData(m_BundleTreeView);
            m_Parent.Repaint();
        }

        internal void OnGUI(Rect pos)
        {
            m_Position = pos;

            if(m_BundleTreeView == null)
            {
                if (m_AssetListState == null)
                    m_AssetListState = new TreeViewState();

                var headerState = AssetBundleWindowConfigureTabAssetListTreeView.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_AssetListMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_AssetListMCHState, headerState);
                m_AssetListMCHState = headerState;

                m_AssetTreeView = new AssetBundleWindowConfigureTabAssetListTreeView(m_AssetListState, m_AssetListMCHState, this);
                m_AssetTreeView.Reload();
                m_MessageList = new AssetBundleWindowConfigureTabMessageList();

                if (m_BundleDetailState == null)
                    m_BundleDetailState = new TreeViewState();

                m_DetailTreeView = new AssetBundleWindowConfigureTabAssetBundleDetailTreeView(m_BundleDetailState);
                m_DetailTreeView.Reload();

                if (m_BundleTreeState == null)
                    m_BundleTreeState = new TreeViewState();

                m_BundleTreeView = new AssetBundleWindowConfigureTabAssetBundleTreeView(m_BundleTreeState, this);
                m_BundleTreeView.Refresh();
                m_Parent.Repaint();
            }
            
            HandleHorizontalResize();
            HandleVerticalResize();


            if (Model.AssetBundleModel.BundleListIsEmpty())
            {
                m_BundleTreeView.OnGUI(m_Position);
                var style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.wordWrap = true;
                GUI.Label(
                    new Rect(m_Position.x + 1f, m_Position.y + 1f, m_Position.width - 2f, m_Position.height - 2f), 
                    new GUIContent(Model.AssetBundleModel.GetEmptyMessage()),
                    style);
            }
            else
            {
                //Left half
                var bundleTreeRect = new Rect(
                    m_Position.x,
                    m_Position.y,
                    m_HorizontalSplitterRect.x,
                    m_VerticalSplitterRectLeft.y - m_Position.y);
                
                m_BundleTreeView.OnGUI(bundleTreeRect);
                m_DetailTreeView.OnGUI(new Rect(
                    bundleTreeRect.x,
                    bundleTreeRect.y + bundleTreeRect.height + kSplitterWidth,
                    bundleTreeRect.width,
                    m_Position.height - bundleTreeRect.height - kSplitterWidth*2));
                
                //Right half.
                float panelLeft = m_HorizontalSplitterRect.x + kSplitterWidth;
                float panelWidth = m_VerticalSplitterRectRight.width - kSplitterWidth * 2;
                float searchHeight = 20f;
                float panelTop = m_Position.y + searchHeight;
                float panelHeight = m_VerticalSplitterRectRight.y - panelTop;
                OnGUISearchBar(new Rect(panelLeft, m_Position.y, panelWidth, searchHeight));
                m_AssetTreeView.OnGUI(new Rect(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight));
                m_MessageList.OnGUI(new Rect(
                    panelLeft,
                    panelTop + panelHeight + kSplitterWidth,
                    panelWidth,
                    (m_Position.height - panelHeight) - kSplitterWidth * 2));

                if (m_ResizingHorizontalSplitter || m_ResizingVerticalSplitterRight || m_ResizingVerticalSplitterLeft)
                    m_Parent.Repaint();
            }
        }

        void OnGUISearchBar(Rect rect)
        {
            m_BundleTreeView.searchString = m_SearchField.OnGUI(rect, m_BundleTreeView.searchString);
            m_AssetTreeView.searchString = m_BundleTreeView.searchString;
        }

        public bool hasSearch
        {
            get { return m_BundleTreeView.hasSearch;  }
        }

        private void HandleHorizontalResize()
        {
            m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            m_HorizontalSplitterRect.height = m_Position.height;

            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && m_HorizontalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitter = true;

            if (m_ResizingHorizontalSplitter)
            {
                m_HorizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / m_Position.width, 0.1f, 0.9f);
                m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            }

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitter = false;
        }

        private void HandleVerticalResize()
        {
            m_VerticalSplitterRectRight.x = m_HorizontalSplitterRect.x;
            m_VerticalSplitterRectRight.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight);
            m_VerticalSplitterRectRight.width = m_Position.width - m_HorizontalSplitterRect.x;
            m_VerticalSplitterRectLeft.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft);
            m_VerticalSplitterRectLeft.width = m_VerticalSplitterRectRight.width;


            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRectRight, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VerticalSplitterRectRight.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitterRight = true;

            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRectLeft, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VerticalSplitterRectLeft.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitterLeft = true;


            if (m_ResizingVerticalSplitterRight)
            {
                m_VerticalSplitterPercentRight = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalSplitterRect.height, 0.2f, 0.98f);
                m_VerticalSplitterRectRight.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight);
            }
            else if (m_ResizingVerticalSplitterLeft)
            {
                m_VerticalSplitterPercentLeft = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalSplitterRect.height, 0.25f, 0.98f);
                m_VerticalSplitterRectLeft.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft);
            }


            if (Event.current.type == EventType.MouseUp)
            {
                m_ResizingVerticalSplitterRight = false;
                m_ResizingVerticalSplitterLeft = false;
            }
        }

        internal void UpdateSelectedBundles(IEnumerable<Model.AssetBundleInfo> bundles)
        {
            Model.AssetBundleModel.AddBundlesToUpdate(bundles);
            m_AssetTreeView.SetSelectedBundles(bundles);
            m_DetailTreeView.SetItems(bundles);
            m_MessageList.SetItems(null);
        }

        internal void SetSelectedItems(IEnumerable<Model.AssetInfo> items)
        {
            m_MessageList.SetItems(items);
        }
        
        internal void SetAssetListSelection( List<string> assets )
        {
            m_AssetTreeView.SetSelection( assets );
        }
    }
}