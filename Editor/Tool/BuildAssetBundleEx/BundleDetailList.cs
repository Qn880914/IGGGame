using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AssetBundleBrowser.AssetBundleModel;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleBrowser
{
    internal class BundleDetailItem : TreeViewItem
    {
        internal BundleDetailItem(int id, int depth, string displayName, MessageType type) : base(id, depth, displayName)
        {
            messageLevel = type;
        }

        internal MessageType messageLevel { get; set; }
    }

    internal class TogglePathTreeViewItem : TreeViewItem
    {
        private static bool m_DisplayAlt;

        private readonly string m_DisplayNamePrefix;
        public string DisplayNamePrefix { get { return m_DisplayNamePrefix; } }

        private readonly string m_Path;
        public string Path { get { return m_Path; } }

        public TogglePathTreeViewItem(int id, int depth, string displayName, string path)
        {
            base.depth = depth;
            base.id = id;
            base.displayName = displayName;
            m_Path = path;
            m_DisplayNamePrefix = "";
        }

        public TogglePathTreeViewItem(int id, int depth, string displayNamePrefix, string displayName, string path)
        {
            base.depth = depth;
            base.id = id;
            base.displayName = displayName;
            m_Path = path;
            m_DisplayNamePrefix = displayNamePrefix;
        }

        public override string displayName
        {
            get
            {
                // TODO this is a bit unresponsive here in large projects, see if can be better elsewhere
                Event e = Event.current;
                if (e.alt && e.type == EventType.MouseDown)
                    m_DisplayAlt = !m_DisplayAlt;

                return m_DisplayNamePrefix + (m_DisplayAlt ? m_Path : base.displayName);
            }
            set
            {
                base.displayName = value;
            }
        }
    }

    internal class BundleDetailList : TreeView
    {
        private HashSet<AssetBundleModel.BundleDataInfo> m_Selecteditems;

        private Rect m_TotalRect;

        private const float kDoubleIndent = 32f;

        private const string kSizeHeader = "Size: ";

        private const string kDependencyHeader = "Dependent On:";

        private const string kDependencyEmpty = kDependencyHeader + " - None";

        private const string kMessageHeader = "Messages:";

        private const string kMessageEmpty = kMessageHeader + " - None";

        private const string kReferencedPrefix = "- ";

        internal BundleDetailList(TreeViewState state) : base(state)
        {
            m_Selecteditems = new HashSet<AssetBundleModel.BundleDataInfo>();
            showBorder = true;
        }

        internal void Update()
        {
            bool dirty = false;
            foreach (var bundle in m_Selecteditems)
            {
                dirty |= bundle.dirty;
            }
            if (dirty)
            {
                Reload();
                ExpandAll(2);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1)
            {
                children = new List<TreeViewItem>()
            };
            if (m_Selecteditems != null)
            {
                foreach (var bundle in m_Selecteditems)
                {
                    root.AddChild(AppendBundleToTree(bundle));
                }
            }
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if ((args.item as BundleDetailItem) != null)
            {
                EditorGUI.HelpBox(
                    new Rect(args.rowRect.x + kDoubleIndent, args.rowRect.y, args.rowRect.width - kDoubleIndent, args.rowRect.height),
                    args.item.displayName,
                    (args.item as BundleDetailItem).messageLevel);
            }
            else
            {
                Color old = GUI.color;
                if (args.item.depth == 1 &&
                    (args.item.displayName == kMessageEmpty || args.item.displayName == kDependencyEmpty))
                    GUI.color = AssetBundleModel.Model.lightGrey;
                base.RowGUI(args);
                GUI.color = old;
            }
        }

        public override void OnGUI(Rect rect)
        {
            m_TotalRect = rect;
            base.OnGUI(rect);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if ((item as BundleDetailItem) != null)
            {
                float height = DefaultStyles.backgroundEven.CalcHeight(new GUIContent(item.displayName), m_TotalRect.width);
                return height + 3f;
            }
            return base.GetCustomRowHeight(row, item);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            List<string> pathList = new List<string>();

            for (int i = 0; i < selectedIds.Count; ++i)
            {
                TreeViewItem item = this.FindItem(selectedIds[i], rootItem);
                if (item != null)
                {
                    AddDependentAssetsRecursive(item, pathList);
                }
            }

            AssetBundleBrowserMain.instance.manageTab.SetAssetListSelection(pathList);
        }

        void AddDependentAssetsRecursive(TreeViewItem item, List<string> pathList)
        {
            if (item is TogglePathTreeViewItem pathItem)
            {
                if (string.IsNullOrEmpty(pathItem.DisplayNamePrefix) == false && pathList.Contains(pathItem.Path) == false)
                {
                    pathList.Add(pathItem.Path);
                }
            }

            if (item.hasChildren)
            {
                for (int i = 0; i < item.children.Count; ++i)
                    AddDependentAssetsRecursive(item.children[i], pathList);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            TreeViewItem item = this.FindItem(id, rootItem);
            if (item != null)
            {
                if (item is TogglePathTreeViewItem pathItem)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(pathItem.Path);
                    if (o != null)
                    {
                        Selection.activeObject = o;
                        EditorGUIUtility.PingObject(o);
                    }
                }
            }
        }

        internal static TreeViewItem AppendBundleToTree(AssetBundleModel.BundleDataInfo bundle)
        {
            var itemName = bundle.m_Name.fullNativeName;
            var bunRoot = new TreeViewItem(itemName.GetHashCode(), 0, itemName);

            var str = itemName + kSizeHeader;
            var sz = new TreeViewItem(str.GetHashCode(), 1, kSizeHeader + bundle.TotalSize());

            str = itemName + kDependencyHeader;
            var dependency = new TreeViewItem(str.GetHashCode(), 1, kDependencyEmpty);
            var depList = bundle.GetBundleDependencies();
            if (depList.Count > 0)
            {
                dependency.displayName = kDependencyHeader;
                foreach (var dep in bundle.GetBundleDependencies())
                {
                    str = itemName + dep.m_BundleName;
                    TreeViewItem newItem = new TreeViewItem(str.GetHashCode(), 2, dep.m_BundleName)
                    {
                        icon = Model.GetBundleIcon()
                    };
                    dependency.AddChild(newItem);

                    Dictionary<string, TogglePathTreeViewItem> toAssetItems = new Dictionary<string, TogglePathTreeViewItem>();

                    for (int i = 0; i < dep.m_FromAssets.Count; ++i)
                    {
                        if (!toAssetItems.TryGetValue(dep.m_ToAssets[i].fullAssetName, out TogglePathTreeViewItem item))
                        {
                            str = itemName + dep.m_BundleName + dep.m_ToAssets[i].displayName;
                            item = new TogglePathTreeViewItem(str.GetHashCode(), 3, "/" + dep.m_ToAssets[i].displayName, "/" + dep.m_ToAssets[i].fullAssetName)
                            {
                                icon = AssetDatabase.GetCachedIcon(dep.m_ToAssets[i].fullAssetName) as Texture2D
                            };
                            newItem.AddChild(item);
                            toAssetItems.Add(dep.m_ToAssets[i].fullAssetName, item);
                        }

                        str = str + dep.m_FromAssets[i].displayName;
                        TreeViewItem refItem = new TogglePathTreeViewItem(str.GetHashCode(), 4, kReferencedPrefix,
                            dep.m_FromAssets[i].displayName, dep.m_FromAssets[i].fullAssetName)
                        {
                            icon = AssetDatabase.GetCachedIcon(dep.m_FromAssets[i].fullAssetName) as Texture2D
                        };
                        item.AddChild(refItem);
                    }
                }
            }

            str = itemName + kMessageHeader;
            var msg = new TreeViewItem(str.GetHashCode(), 1, kMessageEmpty);
            if (bundle.HasMessages())
            {
                msg.displayName = kMessageHeader;
                var currMessages = bundle.GetMessages();

                foreach (var currMsg in currMessages)
                {
                    str = itemName + currMsg.message;
                    msg.AddChild(new BundleDetailItem(str.GetHashCode(), 2, currMsg.message, currMsg.severity));
                }
            }

            bunRoot.AddChild(sz);
            bunRoot.AddChild(dependency);
            bunRoot.AddChild(msg);

            return bunRoot;
        }

        internal void SetItems(IEnumerable<AssetBundleModel.BundleInfo> items)
        {
            m_Selecteditems.Clear();
            foreach (var item in items)
            {
                CollectBundles(item);
            }
            SetSelection(new List<int>());
            Reload();
            ExpandAll(2);
        }

        internal void CollectBundles(AssetBundleModel.BundleInfo bundle)
        {
            if (bundle is AssetBundleModel.BundleDataInfo bunData)
                m_Selecteditems.Add(bunData);
            else
            {
                var bunFolder = bundle as AssetBundleModel.BundleFolderInfo;
                foreach (var bun in bunFolder.GetChildList())
                {
                    CollectBundles(bun);
                }
            }
        }

        internal void ExpandAll(int maximumDepth)
        {
            List<int> expanded = new List<int>(GetExpanded());
            FindItems(rootItem, maximumDepth, expanded);
            SetExpanded(expanded);
        }

        internal void FindItems(TreeViewItem item, int maximumDepth, List<int> expanded)
        {
            if (item.depth >= maximumDepth || !item.hasChildren)
                return;

            expanded.Add(item.id);
            for (int i = 0; i < item.children.Count; ++i)
            {
                FindItems(item.children[i], maximumDepth, expanded);
            }
        }
    }
}