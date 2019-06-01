using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
//using System;


namespace AssetBundleBrowser
{
    internal class AssetBundleWindowConfigureTabAssetListTreeView : TreeView
    {
        private List<Model.AssetBundleInfo> m_BundleInfos = new List<Model.AssetBundleInfo>();

        private AssetBundleWindowConfigureTab m_Controller;

        private List<UnityEngine.Object> m_EmptyObjectList = new List<UnityEngine.Object>();

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Bundle", "Bundle name. 'auto' means asset was pulled in due to dependency");
            retVal[1].minWidth = 50;
            retVal[1].width = 100;
            retVal[1].maxWidth = 300;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Size", "Size on disk");
            retVal[2].minWidth = 30;
            retVal[2].width = 75;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("!", "Errors, Warnings, or Info");
            retVal[3].minWidth = 16;
            retVal[3].width = 16;
            retVal[3].maxWidth = 16;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = false;

            return retVal;
        }

        enum MyColumns
        {
            Asset,
            Bundle,
            Size,
            Message
        }

        internal enum SortOption
        {
            Asset,
            Bundle,
            Size,
            Message
        }

        readonly SortOption[] m_SortOptions =
        {
            SortOption.Asset,
            SortOption.Bundle,
            SortOption.Size,
            SortOption.Message
        };

        internal AssetBundleWindowConfigureTabAssetListTreeView(TreeViewState state, MultiColumnHeaderState mchs, AssetBundleWindowConfigureTab ctrl ) : base(state, new MultiColumnHeader(mchs))
        {
            m_Controller = ctrl;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        internal void Update()
        {
            bool dirty = false;
            foreach (var bundle in m_BundleInfos)
            {
                dirty |= bundle.dirty;
            }
            if (dirty)
                Reload();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        internal void SetSelectedBundles(IEnumerable<Model.AssetBundleInfo> bundles)
        {
            m_Controller.SetSelectedItems(null);
            m_BundleInfos = bundles.ToList();
            SetSelection(new List<int>());
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = Model.AssetBundleModel.CreateAssetListTreeView(m_BundleInfos);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as Model.AssetTreeViewItem, args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, Model.AssetTreeViewItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if(column != 3)
               GUI.color = item.itemColor;

            switch (column)
            {
                case 0:
                    {
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if(item.icon != null)
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height), 
                            item.displayName, 
                            args.selected, 
                            args.focused);
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.assetInfo.bundleName, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.assetInfo.GetSizeString(), args.selected, args.focused);
                    break;
                case 3:
                    var icon = item.MessageIcon();
                    if (icon != null)
                    {
                        var iconRect = new Rect(cellRect.x, cellRect.y, cellRect.height, cellRect.height);
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                    break;
            }
            GUI.color = oldColor;
        }

        protected override void DoubleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is Model.AssetTreeViewItem assetItem)
            {
                Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.assetInfo.fullAssetName);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        public void SetSelection( List<string> paths )
        {
            List<int> selected = new List<int>();
            AddIfInPaths( paths, selected, rootItem );
            SetSelection( selected );
        }

        void AddIfInPaths( List<string> paths, List<int> selected, TreeViewItem me )
        {
            if (me is Model.AssetTreeViewItem assetItem && assetItem.assetInfo != null)
            {
                if (paths.Contains(assetItem.assetInfo.fullAssetName))
                {
                    if (selected.Contains(me.id) == false)
                        selected.Add(me.id);
                }
            }

            if (me.hasChildren )
            {
                foreach( TreeViewItem item in me.children )
                {
                    AddIfInPaths( paths, selected, item );
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null)
                return;

            List<Object> selectedObjects = new List<Object>();
            List<Model.AssetInfo> selectedAssets = new List<Model.AssetInfo>();
            foreach (var id in selectedIds)
            {
                if (FindItem(id, rootItem) is Model.AssetTreeViewItem assetItem)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.assetInfo.fullAssetName);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    selectedAssets.Add(assetItem.assetInfo);
                }
            }
            m_Controller.SetSelectedItems(selectedAssets);
            Selection.objects = selectedObjects.ToArray();
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            args.draggedItemIDs = GetSelection();
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = m_EmptyObjectList.ToArray();
            List<Model.AssetTreeViewItem> items = 
                new List<Model.AssetTreeViewItem>(args.draggedItemIDs.Select(id => FindItem(id, rootItem) as Model.AssetTreeViewItem));
            DragAndDrop.paths = items.Select(a => a.assetInfo.fullAssetName).ToArray();
            DragAndDrop.SetGenericData("AssetListTreeSource", this);
            DragAndDrop.StartDrag("AssetListTree");
        }
        
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if(IsValidDragDrop())
            {
                if (args.performDrop)
                {
                    Model.AssetBundleModel.MoveAssetToBundle(DragAndDrop.paths, m_BundleInfos[0].m_Name.bundleName, m_BundleInfos[0].m_Name.variant);
                    Model.AssetBundleModel.ExecuteAssetMove();
                    foreach (var bundle in m_BundleInfos)
                    {
                        bundle.RefreshAssetList();
                    }
                    m_Controller.UpdateSelectedBundles(m_BundleInfos);
                }
                return DragAndDropVisualMode.Copy;//Move;
            }

            return DragAndDropVisualMode.Rejected;
        }

        protected bool IsValidDragDrop()
        {
            //can't do drag & drop if data source is read only
            if (Model.AssetBundleModel.assetBundleData.IsReadOnly ())
                return false;

            //can't drag onto none or >1 bundles
            if (m_BundleInfos.Count == 0 || m_BundleInfos.Count > 1)
                return false;
            
            //can't drag nothing
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
                return false;

            //can't drag into a folder
            if (m_BundleInfos[0] is Model.AssetBundleFolderInfo folder)
                return false;

            if (!(m_BundleInfos[0] is Model.BundleDataInfo data))
                return false; // this should never happen.

            if (DragAndDrop.GetGenericData("AssetListTreeSource") is AssetBundleWindowConfigureTabAssetListTreeView thing)
                return false;

            if (data.IsEmpty())
                return true;


            if (data.isSceneBundle)
            {
                foreach (var assetPath in DragAndDrop.paths)
                {
                    if ((AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(SceneAsset)) &&
                        (!AssetDatabase.IsValidFolder(assetPath)))
                        return false;
                }
            }
            else
            {
                foreach (var assetPath in DragAndDrop.paths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                        return false;
                }
            }

            return true;
        }

        protected override void ContextClickedItem(int id)
        {
            if (Model.AssetBundleModel.assetBundleData.IsReadOnly ()) {
                return;
            }

            List<Model.AssetTreeViewItem> selectedNodes = new List<Model.AssetTreeViewItem>();
            foreach(var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as Model.AssetTreeViewItem);
            }

            if(selectedNodes.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove asset(s) from bundle."), false, RemoveAssets, selectedNodes);
                menu.ShowAsContext();
            }
        }

        void RemoveAssets(object obj)
        {
            var selectedNodes = obj as List<Model.AssetTreeViewItem>;
            var assets = new List<Model.AssetInfo>();
            //var bundles = new List<AssetBundleModel.BundleInfo>();
            foreach (var node in selectedNodes)
            {
                if (!System.String.IsNullOrEmpty(node.assetInfo.bundleName))
                    assets.Add(node.assetInfo);
            }
            Model.AssetBundleModel.MoveAssetToBundle(assets, string.Empty, string.Empty);
            Model.AssetBundleModel.ExecuteAssetMove();
            foreach (var bundle in m_BundleInfos)
            {
                bundle.RefreshAssetList();
            }
            m_Controller.UpdateSelectedBundles(m_BundleInfos);
            //ReloadAndSelect(new List<int>());
        }

        protected override void KeyEvent()
        {
            if (m_BundleInfos.Count > 0 && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<Model.AssetTreeViewItem> selectedNodes = new List<Model.AssetTreeViewItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as Model.AssetTreeViewItem);
                }

                RemoveAssets(selectedNodes);
            }
        }

        void OnSortingChanged(MultiColumnHeader header)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }

        void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<Model.AssetTreeViewItem> assetList = new List<Model.AssetTreeViewItem>();
            foreach(var item in rootItem.children)
            {
                assetList.Add(item as Model.AssetTreeViewItem);
            }
            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<Model.AssetTreeViewItem> InitialOrder(IEnumerable<Model.AssetTreeViewItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Asset:
                    return myTypes.Order(l => l.displayName, ascending);
                case SortOption.Size:
                    return myTypes.Order(l => l.assetInfo.fileSize, ascending);
                case SortOption.Message:
                    return myTypes.Order(l => l.HighestMessageLevel(), ascending);
                case SortOption.Bundle:
                default:
                    return myTypes.Order(l => l.assetInfo.bundleName, ascending);
            }
            
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes);
            SelectionChanged(hashCodes);
        }
    }

    static class MyExtensionMethods
    {
        internal static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        internal static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}
