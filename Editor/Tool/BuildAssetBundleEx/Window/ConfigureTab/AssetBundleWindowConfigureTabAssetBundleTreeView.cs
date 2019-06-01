using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundleBrowser
{
    internal class AssetBundleWindowConfigureTabAssetBundleTreeView : TreeView
    { 
        private AssetBundleWindowConfigureTab m_Controller;

        private bool m_ContextOnItem ;

        private List<UnityEngine.Object> m_EmptyObjectList = new List<UnityEngine.Object>();

        private string[] dragToNewSpacePaths;

        private Model.AssetBundleFolderInfo dragToNewSpaceRoot;

        internal AssetBundleWindowConfigureTabAssetBundleTreeView(TreeViewState state, AssetBundleWindowConfigureTab ctrl) : base(state)
        {
            Model.AssetBundleModel.Rebuild();
            m_Controller = ctrl;
            showBorder = true;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item != null && item.displayName.Length > 0;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            var bundleItem = item as Model.BundleTreeViewItem;
            return bundleItem.assetBundleInfo.DoesItemMatchSearch(search);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var bundleItem = (args.item as Model.BundleTreeViewItem);
            if (args.item.icon == null)
                extraSpaceBeforeIconAndLabel = 16f;
            else
                extraSpaceBeforeIconAndLabel = 0f;

            Color old = GUI.color;
            if ((bundleItem.assetBundleInfo as Model.BundleVariantFolderInfo) != null)
                GUI.color = Model.AssetBundleModel.lightGrey; //new Color(0.3f, 0.5f, 0.85f);
            base.RowGUI(args);
            GUI.color = old;

            var message = bundleItem.BundleMessage();
            if(message.severity != MessageType.None)
            {
                var size = args.rowRect.height;
                var right = args.rowRect.xMax;
                Rect messageRect = new Rect(right - size, args.rowRect.yMin, size, size);
                GUI.Label(messageRect, new GUIContent(message.icon, message.message ));
            }
        }

        protected override void RenameEnded(RenameEndedArgs args)
        { 
            base.RenameEnded(args);
            if (args.newName.Length > 0 && args.newName != args.originalName)
            {
                args.newName = args.newName.ToLower();
                args.acceptedRename = true;

                Model.BundleTreeViewItem renamedItem = FindItem(args.itemID, rootItem) as Model.BundleTreeViewItem;
                args.acceptedRename = Model.AssetBundleModel.HandleBundleRename(renamedItem, args.newName);
                ReloadAndSelect(renamedItem.assetBundleInfo.nameHashCode, false);
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            Model.AssetBundleModel.Refresh();
            var root = Model.AssetBundleModel.CreateBundleTreeView();
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {

            var selectedBundles = new List<Model.AssetBundleInfo>();
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    if (FindItem(id, rootItem) is Model.BundleTreeViewItem item && item.assetBundleInfo != null)
                    {
                        item.assetBundleInfo.RefreshAssetList();
                        selectedBundles.Add(item.assetBundleInfo);
                    }
                }
            }

            m_Controller.UpdateSelectedBundles(selectedBundles);
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }

            List<Model.BundleTreeViewItem> selectedNodes = new List<Model.BundleTreeViewItem>();
            GenericMenu menu = new GenericMenu();

            if (!Model.AssetBundleModel.assetBundleData.IsReadOnly ()) {
                menu.AddItem(new GUIContent("Add new bundle"), false, CreateNewBundle, selectedNodes); 
                menu.AddItem(new GUIContent("Add new folder"), false, CreateFolder, selectedNodes);
            }

            menu.AddItem(new GUIContent("Reload all data"), false, ForceReloadData, selectedNodes);
            menu.ShowAsContext();
        }

        protected override void ContextClickedItem(int id)
        {
            if (Model.AssetBundleModel.assetBundleData.IsReadOnly ()) {
                return;
            }

            m_ContextOnItem = true;
            List<Model.BundleTreeViewItem> selectedNodes = new List<Model.BundleTreeViewItem>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as Model.BundleTreeViewItem);
            }
            
            GenericMenu menu = new GenericMenu();
            
            if(selectedNodes.Count == 1)
            {
                if ((selectedNodes[0].assetBundleInfo as Model.AssetBundleFolderInfoConcrete) != null)
                {
                    menu.AddItem(new GUIContent("Add Child/New Bundle"), false, CreateNewBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Child/New Folder"), false, CreateFolder, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                }
                else if( (selectedNodes[0].assetBundleInfo as Model.BundleVariantFolderInfo) != null)
                {
                    menu.AddItem(new GUIContent("Add Child/New Variant"), false, CreateNewVariant, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                }
                else
                {
                    if (!(selectedNodes[0].assetBundleInfo is Model.BundleVariantDataInfo variant))
                    {
                        menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                        menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                        menu.AddItem(new GUIContent("Convert to variant"), false, ConvertToVariant, selectedNodes);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add Sibling/New Variant"), false, CreateNewSiblingVariant, selectedNodes);
                    }
                }

                if(selectedNodes[0].assetBundleInfo.IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
                    menu.AddItem(new GUIContent("Move duplicates to new bundle"), false, DedupeAllBundles, selectedNodes);

                menu.AddItem(new GUIContent("Rename"), false, RenameBundle, selectedNodes);
                menu.AddItem(new GUIContent("Delete " + selectedNodes[0].displayName), false, DeleteBundles, selectedNodes);
                
            }
            else if (selectedNodes.Count > 1)
            { 
                menu.AddItem(new GUIContent("Move duplicates shared by selected"), false, DedupeOverlappedBundles, selectedNodes);
                menu.AddItem(new GUIContent("Move duplicates existing in any selected"), false, DedupeAllBundles, selectedNodes);
                menu.AddItem(new GUIContent("Delete " + selectedNodes.Count + " selected bundles"), false, DeleteBundles, selectedNodes);
            }
            menu.ShowAsContext();
        }

        void ForceReloadData(object context)
        {
            Model.AssetBundleModel.ForceReloadData(this);
        }

        void CreateNewSiblingFolder(object context)
        {
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                Model.AssetBundleFolderInfoConcrete folder = null;
                folder = selectedNodes[0].assetBundleInfo.parent as Model.AssetBundleFolderInfoConcrete;
                CreateFolderUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }

        void CreateFolder(object context)
        {
            Model.AssetBundleFolderInfoConcrete folder = null;
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                folder = selectedNodes[0].assetBundleInfo as Model.AssetBundleFolderInfoConcrete;
            }
            CreateFolderUnderParent(folder);
        }

        void CreateFolderUnderParent(Model.AssetBundleFolderInfoConcrete folder)
        {
            var newBundle = Model.AssetBundleModel.CreateEmptyBundleFolder(folder);
            ReloadAndSelect(newBundle.nameHashCode, true);
        }

        void RenameBundle(object context)
        {
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                BeginRename(FindItem(selectedNodes[0].assetBundleInfo.nameHashCode, rootItem));
            }
        }

        void CreateNewSiblingBundle(object context)
        {
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                Model.AssetBundleFolderInfoConcrete folder = null;
                folder = selectedNodes[0].assetBundleInfo.parent as Model.AssetBundleFolderInfoConcrete;
                CreateBundleUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }

        void CreateNewBundle(object context)
        {
            Model.AssetBundleFolderInfoConcrete folder = null;
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                folder = selectedNodes[0].assetBundleInfo as Model.AssetBundleFolderInfoConcrete;
            }
            CreateBundleUnderParent(folder);
        }

        void CreateBundleUnderParent(Model.AssetBundleFolderInfo folder)
        {
            var newBundle = Model.AssetBundleModel.CreateEmptyBundle(folder);
            ReloadAndSelect(newBundle.nameHashCode, true);
        }


        void CreateNewSiblingVariant(object context)
        {
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count > 0)
            {
                Model.BundleVariantFolderInfo folder = null;
                folder = selectedNodes[0].assetBundleInfo.parent as Model.BundleVariantFolderInfo;
                CreateVariantUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }

        void CreateNewVariant(object context)
        {
            Model.BundleVariantFolderInfo folder = null;
            if (context is List<Model.BundleTreeViewItem> selectedNodes && selectedNodes.Count == 1)
            {
                folder = selectedNodes[0].assetBundleInfo as Model.BundleVariantFolderInfo;
                CreateVariantUnderParent(folder);
            }
        }

        void CreateVariantUnderParent(Model.BundleVariantFolderInfo folder)
        {
            if (folder != null)
            {
                var newBundle = Model.AssetBundleModel.CreateEmptyVariant(folder);
                ReloadAndSelect(newBundle.nameHashCode, true);
            }
        }

        void ConvertToVariant(object context)
        {
            var selectedNodes = context as List<Model.BundleTreeViewItem>;
            if (selectedNodes.Count == 1)
            {
                var bundle = selectedNodes[0].assetBundleInfo as Model.BundleDataInfo;
                var newBundle = Model.AssetBundleModel.HandleConvertToVariant(bundle);
                int hash = 0;
                if (newBundle != null)
                    hash = newBundle.nameHashCode;
                ReloadAndSelect(hash, true);
            }
        }

        void DedupeOverlappedBundles(object context)
        {
            DedupeBundles(context, true);
        }

        void DedupeAllBundles(object context)
        {
            DedupeBundles(context, false);
        }

        void DedupeBundles(object context, bool onlyOverlappedAssets)
        {
            var selectedNodes = context as List<Model.BundleTreeViewItem>;
            var newBundle = Model.AssetBundleModel.HandleDedupeBundles(selectedNodes.Select(item => item.assetBundleInfo), onlyOverlappedAssets);
            if(newBundle != null)
            {
                var selection = new List<int>
                {
                    newBundle.nameHashCode
                };
                ReloadAndSelect(selection);
            }
            else
            {
                if (onlyOverlappedAssets)
                    Debug.LogWarning("There were no duplicated assets that existed across all selected bundles.");
                else
                    Debug.LogWarning("No duplicate assets found after refreshing bundle contents.");
            }
        }

        void DeleteBundles(object b)
        {
            var selectedNodes = b as List<Model.BundleTreeViewItem>;
            Model.AssetBundleModel.HandleBundleDelete(selectedNodes.Select(item => item.assetBundleInfo));
            ReloadAndSelect(new List<int>());
        }

        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<Model.BundleTreeViewItem> selectedNodes = new List<Model.BundleTreeViewItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as Model.BundleTreeViewItem);
                }
                DeleteBundles(selectedNodes);
            }
        }

        class DragAndDropData
        {
            internal bool hasBundleFolder;
            internal bool hasScene;
            internal bool hasNonScene;
            internal bool hasVariantChild;
            internal List<Model.AssetBundleInfo> draggedNodes;
            internal Model.BundleTreeViewItem targetNode;
            internal DragAndDropArgs args;
            internal string[] paths;

            internal DragAndDropData(DragAndDropArgs a)
            {
                args = a;
                draggedNodes = DragAndDrop.GetGenericData("AssetBundleModel.BundleInfo") as List<Model.AssetBundleInfo>;
                targetNode = args.parentItem as Model.BundleTreeViewItem;
                paths = DragAndDrop.paths;

                if (draggedNodes != null)
                {
                    foreach (var bundle in draggedNodes)
                    {
                        if ((bundle as Model.AssetBundleFolderInfo) != null)
                        {
                            hasBundleFolder = true;
                        }
                        else
                        {
                            if (bundle is Model.BundleDataInfo dataBundle)
                            {
                                if (dataBundle.isSceneBundle)
                                    hasScene = true;
                                else
                                    hasNonScene = true;

                                hasVariantChild |= (dataBundle as Model.BundleVariantDataInfo) != null;
                            }
                        }
                    }
                }
                else if (DragAndDrop.paths != null)
                {
                    foreach (var assetPath in DragAndDrop.paths)
                    {
                        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                            hasScene = true;
                        else
                            hasNonScene = true;
                    }
                }
            }

        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;
            DragAndDropData data = new DragAndDropData(args);
            
            if (Model.AssetBundleModel.assetBundleData.IsReadOnly ()) {
                return DragAndDropVisualMode.Rejected;
            }

            if ( (data.hasScene && data.hasNonScene) ||
                (data.hasVariantChild) )
                return DragAndDropVisualMode.Rejected;
            
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                    visualMode = HandleDragDropUpon(data);
                    break;
                case DragAndDropPosition.BetweenItems:
                    visualMode = HandleDragDropBetween(data);
                    break;
                case DragAndDropPosition.OutsideItems:
                    if (data.draggedNodes != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            Model.AssetBundleModel.HandleBundleReparent(data.draggedNodes, null);
                            Reload();
                        }
                    }
                    else if(data.paths != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            DragPathsToNewSpace(data.paths, null);
                        }
                    }
                    break;
            }
            return visualMode;
        }

        private DragAndDropVisualMode HandleDragDropUpon(DragAndDropData data)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.Copy;//Move;
            if (data.targetNode.assetBundleInfo is Model.BundleDataInfo targetDataBundle)
            {
                if (targetDataBundle.isSceneBundle)
                {
                    if (data.hasNonScene)
                        return DragAndDropVisualMode.Rejected;
                }
                else
                {
                    if (data.hasBundleFolder)
                    {
                        return DragAndDropVisualMode.Rejected;
                    }
                    else if (data.hasScene && !targetDataBundle.IsEmpty())
                    {
                        return DragAndDropVisualMode.Rejected;
                    }

                }

                if (data.args.performDrop)
                {
                    if (data.draggedNodes != null)
                    {
                        Model.AssetBundleModel.HandleBundleMerge(data.draggedNodes, targetDataBundle);
                        ReloadAndSelect(targetDataBundle.nameHashCode, false);
                    }
                    else if (data.paths != null)
                    {
                        Model.AssetBundleModel.MoveAssetToBundle(data.paths, targetDataBundle.m_Name.bundleName, targetDataBundle.m_Name.variant);
                        Model.AssetBundleModel.ExecuteAssetMove();
                        ReloadAndSelect(targetDataBundle.nameHashCode, false);
                    }
                }

            }
            else
            {
                if (data.targetNode.assetBundleInfo is Model.AssetBundleFolderInfo folder)
                {
                    if (data.args.performDrop)
                    {
                        if (data.draggedNodes != null)
                        {
                            Model.AssetBundleModel.HandleBundleReparent(data.draggedNodes, folder);
                            Reload();
                        }
                        else if (data.paths != null)
                        {
                            DragPathsToNewSpace(data.paths, folder);
                        }
                    }
                }
                else
                    visualMode = DragAndDropVisualMode.Rejected; //must be a variantfolder

            }
            return visualMode;
        }

        private DragAndDropVisualMode HandleDragDropBetween(DragAndDropData data)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.Copy;//Move;

            var parent = (data.args.parentItem as Model.BundleTreeViewItem);

            if (parent != null)
            {
                if (parent.assetBundleInfo is Model.BundleVariantFolderInfo variantFolder)
                    return DragAndDropVisualMode.Rejected;

                if (data.args.performDrop)
                {
                    if (parent.assetBundleInfo is Model.AssetBundleFolderInfoConcrete folder)
                    {
                        if (data.draggedNodes != null)
                        {
                            Model.AssetBundleModel.HandleBundleReparent(data.draggedNodes, folder);
                            Reload();
                        }
                        else if (data.paths != null)
                        {
                            DragPathsToNewSpace(data.paths, folder);
                        }
                    }
                }
            }

            return visualMode;
        }

        private void DragPathsAsOneBundle()
        {
            var newBundle = Model.AssetBundleModel.CreateEmptyBundle(dragToNewSpaceRoot);
            Model.AssetBundleModel.MoveAssetToBundle(dragToNewSpacePaths, newBundle.m_Name.bundleName, newBundle.m_Name.variant);
            Model.AssetBundleModel.ExecuteAssetMove();
            ReloadAndSelect(newBundle.nameHashCode, true);
        }

        private void DragPathsAsManyBundles()
        {
            List<int> hashCodes = new List<int>();
            foreach (var assetPath in dragToNewSpacePaths)
            {
                var newBundle = Model.AssetBundleModel.CreateEmptyBundle(dragToNewSpaceRoot, System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower());
                Model.AssetBundleModel.MoveAssetToBundle(assetPath, newBundle.m_Name.bundleName, newBundle.m_Name.variant);
                hashCodes.Add(newBundle.nameHashCode);
            }
            Model.AssetBundleModel.ExecuteAssetMove();
            ReloadAndSelect(hashCodes);
        }

        private void DragPathsToNewSpace(string[] paths, Model.AssetBundleFolderInfo root)
        {
            dragToNewSpacePaths = paths;
            dragToNewSpaceRoot = root;
            if (paths.Length > 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create 1 Bundle"), false, DragPathsAsOneBundle);
                var message = "Create ";
                message += paths.Length;
                message += " Bundles";
                menu.AddItem(new GUIContent(message), false, DragPathsAsManyBundles);
                menu.ShowAsContext();
            }
            else
                DragPathsAsManyBundles();
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (args.draggedItemIDs == null)
                return;

            DragAndDrop.PrepareStartDrag();

            var selectedBundles = new List<Model.AssetBundleInfo>();
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItem(id, rootItem) as Model.BundleTreeViewItem;
                selectedBundles.Add(item.assetBundleInfo);
            }
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = m_EmptyObjectList.ToArray();
            DragAndDrop.SetGenericData("AssetBundleModel.BundleInfo", selectedBundles);
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//Move;
            DragAndDrop.StartDrag("AssetBundleTree");
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        internal void Refresh()
        {
            var selection = GetSelection();
            Reload();
            SelectionChanged(selection);
        }

        private void ReloadAndSelect(int hashCode, bool rename)
        {
            var selection = new List<int>
            {
                hashCode
            };
            ReloadAndSelect(selection);
            if(rename)
            {
                BeginRename(FindItem(hashCode, rootItem), 0.25f);
            }
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
        }
    }
}
