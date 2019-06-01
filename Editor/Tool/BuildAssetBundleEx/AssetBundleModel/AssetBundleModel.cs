using AssetBundleBrowser.AssetBundleDataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundleBrowser.Model
{

    /// <summary>
    ///     <para> Static class holding model data for Asset Bundle Browser tool. Data in Model is read from DataSource, but is not pushed.  </para>
    ///  
    ///     <para> If not using a custom DataSource, then the data comes from the AssetDatabase.  If you wish to alter the data from code,</para> 
    ///     <para> you should just push changes to the AssetDatabase then tell the Model to Rebuild(). If needed, you can also loop over </para>
    ///     <para> Update() until it returns true to force all sub-items to refresh. </para>
    /// </summary>
    public static class AssetBundleModel
    {
        internal class AssetBundleMoveData
        {
            internal string assetName;

            internal string bundleName;

            internal string variantName;

            internal AssetBundleMoveData(string asset, string bundle, string variant)
            {
                assetName = asset;
                bundleName = bundle;
                variantName = variant;
            }

            internal void Apply()
            {
                if (!assetBundleData.IsReadOnly())
                {
                    assetBundleData.SetAssetBundleNameAndVariant(assetName, bundleName, variantName);
                }
            }
        }

        private const string kNewBundleBaseName = "newbundle";

        private const string kNewVariantBaseName = "newvariant";

        internal static Color lightGrey = Color.grey * 1.5f;

        const string kDefaultEmptyMessage = "Drag assets here or right-click to begin creating bundles.";

        const string kProblemEmptyMessage = "There was a problem parsing the list of bundles. See console.";

        private static AssetBundleFolderInfoConcrete s_RootLevelBundles = new AssetBundleFolderInfoConcrete("", null);

        private static List<AssetBundleMoveData> s_MoveDatas = new List<AssetBundleMoveData>();

        private static List<AssetBundleInfo> s_UpdateAssetBundleInfos = new List<AssetBundleInfo>();

        private static Dictionary<string, AssetInfo> s_GlobalAssetList = new Dictionary<string, AssetInfo>();

        private static Dictionary<string, HashSet<string>> s_DependencyTracker = new Dictionary<string, HashSet<string>>();

        private static bool s_InErrorState;

        private static string s_EmptyMessageString;

        private static Texture2D s_FolderIcon;
        internal static Texture2D folderIcon
        {
            get
            {
                if (s_FolderIcon == null)
                    FindBundleIcons();
                return s_FolderIcon;
            }
        }

        private static Texture2D s_BundleIcon;
        static internal Texture2D bundleIcon
        {
            get
            {
                if (s_BundleIcon == null)
                    FindBundleIcons();
                return s_BundleIcon;
            }
        }

        private static Texture2D s_sceneIcon;
        static internal Texture2D sceneIcon
        {
            get
            {
                if (s_sceneIcon == null)
                    FindBundleIcons();
                return s_sceneIcon;
            }
        }

        /// <summary>
        ///     <para> If using a custom source of asset bundles, you can implement your own ABDataSource and set it here as the active </para>
        ///     <para> DataSource.  This will allow you to use the Browser with data that you provide. </para>
        ///  
        ///     <para> If no custom DataSource is provided, then the Browser will create one that feeds off of and into the  </para>
        ///     <para> AssetDatabase. </para>
        /// </summary>
        private static AssetBundleData s_AssetBundleData;
        public static AssetBundleData assetBundleData
        {
            get
            {
                if (s_AssetBundleData == null)
                    s_AssetBundleData = new AssetDatabaseAssetBundleData ();

                return s_AssetBundleData;
            }

            set { s_AssetBundleData = value; }
        }

        /// <summary>
        ///     <para> Update will loop over bundles that need updating and update them. It will only update one bundle </para>
        ///     <para> per frame and will continue on the same bundle next frame until that bundle is marked as doneUpdating. </para>
        ///     <para> By default, this will cause a very slow collection of dependency data as it will only update one bundle per </para>
        /// </summary>
        public static bool Update()
        {
            bool shouldRepaint = false;
            ExecuteAssetMove(false);     //this should never do anything. just a safety check.

            //TODO - look into EditorApplication callback functions.
            
            int size = s_UpdateAssetBundleInfos.Count;
            if (size > 0)
            {
                s_UpdateAssetBundleInfos[size - 1].Update();
                s_UpdateAssetBundleInfos.RemoveAll(item => item.doneUpdating == true);
                if (s_UpdateAssetBundleInfos.Count == 0)
                {
                    shouldRepaint = true;
                    foreach(var bundle in s_RootLevelBundles.GetChildList())
                    {
                        bundle.RefreshDupeAssetWarning();
                    }
                }
            }
            return shouldRepaint;
        }

        internal static void ForceReloadData(TreeView tree)
        {
            s_InErrorState = false;
            Rebuild();
            tree.Reload();
            bool doneUpdating = s_UpdateAssetBundleInfos.Count == 0;

            EditorUtility.DisplayProgressBar("Updating Bundles", "", 0);
            int fullBundleCount = s_UpdateAssetBundleInfos.Count;
            while (!doneUpdating && !s_InErrorState)
            {
                int currCount = s_UpdateAssetBundleInfos.Count;
                EditorUtility.DisplayProgressBar("Updating Bundles", s_UpdateAssetBundleInfos[currCount-1].displayName, (float)(fullBundleCount- currCount) / (float)fullBundleCount);
                doneUpdating = Update();
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        ///     <para> Clears and rebuilds model data. </para>
        /// </summary>
        public static void Rebuild()
        {
            s_RootLevelBundles = new AssetBundleFolderInfoConcrete("", null);
            s_MoveDatas = new List<AssetBundleMoveData>();
            s_UpdateAssetBundleInfos = new List<AssetBundleInfo>();
            s_GlobalAssetList = new Dictionary<string, AssetInfo>();
            Refresh();
        }

        internal static void AddBundlesToUpdate(IEnumerable<AssetBundleInfo> bundles)
        {
            foreach(var bundle in bundles)
            {
                bundle.ForceNeedUpdate();
                s_UpdateAssetBundleInfos.Add(bundle);
            }
        }

        internal static void Refresh()
        {
            s_EmptyMessageString = kProblemEmptyMessage;
            if (s_InErrorState)
                return;

            var assetBundleNames = GetValidateAssetBundleNames();
            if(assetBundleNames != null)
            {
                s_EmptyMessageString = kDefaultEmptyMessage;
                foreach (var assetBundleName in assetBundleNames)
                {
                    AddAssetBundleByName(assetBundleName);
                }
                AddBundlesToUpdate(s_RootLevelBundles.GetChildList());
            }

            if(s_InErrorState)
            {
                s_RootLevelBundles = new AssetBundleFolderInfoConcrete("", null);
                s_EmptyMessageString = kProblemEmptyMessage;
            }
        }

        internal static string[] GetValidateAssetBundleNames()
        {
            var bundleList = assetBundleData.GetAllAssetBundleNames();
            bool valid = true;
            HashSet<string> bundleSet = new HashSet<string>();
            int index = 0;
            bool attemptedBundleReset = false;
            while(index < bundleList.Length)
            {
                var name = bundleList[index];
                if (!bundleSet.Add(name))
                {
                    LogError("Two bundles share the same name: " + name);
                    valid = false;
                }

                int lastDot = name.LastIndexOf('.');
                if (lastDot > -1)
                {
                    var bunName = name.Substring(0, lastDot);
                    var extraDot = bunName.LastIndexOf('.');
                    if(extraDot > -1)
                    {
                        if(attemptedBundleReset)
                        {
                            var message = "Bundle name '" + bunName + "' contains a period.";
                            message += "  Internally Unity keeps 'bundleName' and 'variantName' separate, but externally treat them as 'bundleName.variantName'.";
                            message += "  If a bundleName contains a period, the build will (probably) succeed, but this tool cannot tell which portion is bundle and which portion is variant.";
                            LogError(message);
                            valid = false;
                        }
                        else
                        {
                            if (!assetBundleData.IsReadOnly ())
                            {
                                assetBundleData.RemoveUnusedAssetBundleNames();
                            }
                            index = 0;
                            bundleSet.Clear();
                            bundleList = assetBundleData.GetAllAssetBundleNames();
                            attemptedBundleReset = true;
                            continue;
                        }
                    }


                    if (bundleList.Contains(bunName))
                    {
                        //there is a bundle.none and a bundle.variant coexisting.  Need to fix that or return an error.
                        if (attemptedBundleReset)
                        {
                            valid = false;
                            var message = "Bundle name '" + bunName + "' exists without a variant as well as with variant '" + name.Substring(lastDot+1) + "'.";
                            message += " That is an illegal state that will not build and must be cleaned up.";
                            LogError(message);
                        }
                        else
                        {
                            if (!assetBundleData.IsReadOnly ())
                            {
                                assetBundleData.RemoveUnusedAssetBundleNames();
                            }
                            index = 0;
                            bundleSet.Clear();
                            bundleList = assetBundleData.GetAllAssetBundleNames();
                            attemptedBundleReset = true;
                            continue;
                        }
                    }
                }

                index++;
            }

            if (valid)
                return bundleList;
            else
                return null;
        }

        internal static bool BundleListIsEmpty()
        {
            return (s_RootLevelBundles.GetChildList().Count() == 0);
        }

        internal static string GetEmptyMessage()
        {
            return s_EmptyMessageString;
        }

        internal static AssetBundleInfo CreateEmptyBundle(AssetBundleFolderInfo folder = null, string newName = null)
        {
            if ((folder as BundleVariantFolderInfo) != null)
                return CreateEmptyVariant(folder as BundleVariantFolderInfo);

            folder = (folder == null) ? s_RootLevelBundles : folder;
            string name = GetUniqueName(folder, newName);
            AssetBundleNameInfo nameData;
            nameData = new AssetBundleNameInfo(folder.m_Name.bundleName, name);
            return AddBundleToFolder(folder, nameData);
        }

        internal static AssetBundleInfo CreateEmptyVariant(BundleVariantFolderInfo folder)
        {
            string name = GetUniqueName(folder, kNewVariantBaseName);
            string variantName = folder.m_Name.bundleName + "." + name;
            AssetBundleNameInfo nameData = new AssetBundleNameInfo(variantName);
            return AddBundleToFolder(folder.parent, nameData);
        }

        internal static AssetBundleFolderInfo CreateEmptyBundleFolder(AssetBundleFolderInfoConcrete folder = null)
        {
            folder = (folder == null) ? s_RootLevelBundles : folder;
            string name = GetUniqueName(folder) + "/dummy";
            AssetBundleNameInfo nameData = new AssetBundleNameInfo(folder.m_Name.bundleName, name);
            return AddFoldersToBundle(s_RootLevelBundles, nameData);
        }

        private static AssetBundleInfo AddAssetBundleByName(string name)
        {
            if (name == null)
                return null;
            
            AssetBundleNameInfo assetBundleNameInfo = new AssetBundleNameInfo(name);

            AssetBundleFolderInfo assetBundleFolderInfo = AddFoldersToBundle(s_RootLevelBundles, assetBundleNameInfo);
            AssetBundleInfo assetBundleInfo = AddBundleToFolder(assetBundleFolderInfo, assetBundleNameInfo);

            return assetBundleInfo;
        }

        private static AssetBundleFolderInfoConcrete AddFoldersToBundle(AssetBundleFolderInfo root, AssetBundleNameInfo nameData)
        {
            AssetBundleInfo assetBundleInfo = root;
            var folder = assetBundleInfo as AssetBundleFolderInfoConcrete;
            var size = nameData.pathTokens.Count;
            for (var index = 0; index < size; index++)
            {
                if (folder != null)
                {
                    assetBundleInfo = folder.GetAssetBundleInfoByName(nameData.pathTokens[index]);
                    if (assetBundleInfo == null)
                    {
                        assetBundleInfo = new AssetBundleFolderInfoConcrete(nameData.pathTokens, index + 1, folder);
                        folder.AddChild(assetBundleInfo);
                    }

                    folder = assetBundleInfo as AssetBundleFolderInfoConcrete;
                    if (folder == null)
                    {
                        s_InErrorState = true;
                        LogFolderAndBundleNameConflict(assetBundleInfo.m_Name.fullNativeName);
                        break;
                    }
                }
            }
            return assetBundleInfo as AssetBundleFolderInfoConcrete;
        }

        private static void LogFolderAndBundleNameConflict(string name)
        {
            var message = "Bundle '";
            message += name;
            message += "' has a name conflict with a bundle-folder.";
            message += "Display of bundle data and building of bundles will not work.";
            message += "\nDetails: If you name a bundle 'x/y', then the result of your build will be a bundle named 'y' in a folder named 'x'.  You thus cannot also have a bundle named 'x' at the same level as the folder named 'x'.";
            LogError(message);
        }

        private static AssetBundleInfo AddBundleToFolder(AssetBundleFolderInfo root, AssetBundleNameInfo nameData)
        {
            AssetBundleInfo currInfo = root.GetAssetBundleInfoByName(nameData.shortName);
            if (!System.String.IsNullOrEmpty(nameData.variant))
            {
                if(currInfo == null)
                {
                    currInfo = new BundleVariantFolderInfo(nameData.bundleName, root);
                    root.AddChild(currInfo);
                }
                var folder = currInfo as BundleVariantFolderInfo;
                if (folder == null)
                {
                    var message = "Bundle named " + nameData.shortName;
                    message += " exists both as a standard bundle, and a bundle with variants.  ";
                    message += "This message is not supported for display or actual bundle building.  ";
                    message += "You must manually fix bundle naming in the inspector.";
                    
                    LogError(message);
                    return null;
                }
                
                
                currInfo = folder.GetAssetBundleInfoByName(nameData.variant);
                if (currInfo == null)
                {
                    currInfo = new BundleVariantDataInfo(nameData.fullNativeName, folder);
                    folder.AddChild(currInfo);
                }
                
            }
            else
            {
                if (currInfo == null)
                {
                    currInfo = new BundleDataInfo(nameData.fullNativeName, root);
                    root.AddChild(currInfo);
                }
                else
                {
                    var dataInfo = currInfo as BundleDataInfo;
                    if (dataInfo == null)
                    {
                        s_InErrorState = true;
                        LogFolderAndBundleNameConflict(nameData.fullNativeName);
                    }
                }
            }
            return currInfo;
        }

        private static string GetUniqueName(AssetBundleFolderInfo folder, string suggestedName = null)
        {
            suggestedName = (suggestedName == null) ? kNewBundleBaseName : suggestedName;
            string name = suggestedName;
            int index = 1;
            bool foundExisting = (folder.GetAssetBundleInfoByName(name) != null);
            while (foundExisting)
            {
                name = suggestedName + index;
                index++;
                foundExisting = (folder.GetAssetBundleInfoByName(name) != null);
            }
            return name;
        }

        internal static BundleTreeViewItem CreateBundleTreeView()
        {
            return s_RootLevelBundles.CreateTreeView(-1);
        }

        internal static AssetTreeViewItem CreateAssetListTreeView(IEnumerable<AssetBundleInfo> selectedBundles)
        {
            var root = new AssetTreeViewItem();
            if (selectedBundles != null)
            {
                foreach (var bundle in selectedBundles)
                {
                    bundle.AddAssetsToNode(root);
                }
            }
            return root;
        }

        internal static bool HandleBundleRename(BundleTreeViewItem item, string newName)
        {
            var originalName = new AssetBundleNameInfo(item.assetBundleInfo.m_Name.fullNativeName);

            var findDot = newName.LastIndexOf('.');
            var findSlash = newName.LastIndexOf('/');
            var findBSlash = newName.LastIndexOf('\\');
            if (findDot == 0 || findSlash == 0 || findBSlash == 0)
                return false; //can't start a bundle with a / or .

            bool result = item.assetBundleInfo.HandleRename(newName, 0);

            if (findDot > 0 || findSlash > 0 || findBSlash > 0)
            {
                item.assetBundleInfo.parent.HandleChildRename(newName, string.Empty);
            }

            ExecuteAssetMove();

            var node = FindBundle(originalName);
            if (node != null)
            {
                var message = "Failed to rename bundle named: ";
                message += originalName.fullNativeName;
                message += ".  Most likely this is due to the bundle being assigned to a folder in your Assets directory, AND that folder is either empty or only contains assets that are explicitly assigned elsewhere.";
                Debug.LogError(message);
            }

            return result;  
        }

        internal static void HandleBundleReparent(IEnumerable<AssetBundleInfo> bundles, AssetBundleFolderInfo parent)
        {
            parent = (parent == null) ? s_RootLevelBundles : parent;
            foreach (var bundle in bundles)
            {
                bundle.HandleReparent(parent.m_Name.bundleName, parent);
            }
            ExecuteAssetMove();
        }

        internal static void HandleBundleMerge(IEnumerable<AssetBundleInfo> bundles, BundleDataInfo target)
        {
            foreach (var bundle in bundles)
            {
                bundle.HandleDelete(true, target.m_Name.bundleName, target.m_Name.variant);
            }
            ExecuteAssetMove();
        }

        internal static void HandleBundleDelete(IEnumerable<AssetBundleInfo> bundles)
        {
            var nameList = new List<AssetBundleNameInfo>();
            foreach (var bundle in bundles)
            {
                nameList.Add(bundle.m_Name);
                bundle.HandleDelete(true);
            }
            ExecuteAssetMove();

            //check to see if any bundles are still there...
            foreach(var name in nameList)
            {
                var node = FindBundle(name);
                if(node != null)
                {
                    var message = "Failed to delete bundle named: ";
                    message += name.fullNativeName;
                    message += ".  Most likely this is due to the bundle being assigned to a folder in your Assets directory, AND that folder is either empty or only contains assets that are explicitly assigned elsewhere.";
                    Debug.LogError(message);
                }
            }
        }

        internal static AssetBundleInfo FindBundle(AssetBundleNameInfo name)
        {
            AssetBundleInfo currNode = s_RootLevelBundles;
            foreach (var token in name.pathTokens)
            {
                if(currNode is AssetBundleFolderInfo)
                {
                    currNode = (currNode as AssetBundleFolderInfo).GetAssetBundleInfoByName(token);
                    if (currNode == null)
                        return null;
                }
                else
                {
                    return null;
                }
            }

            if(currNode is AssetBundleFolderInfo)
            {
                currNode = (currNode as AssetBundleFolderInfo).GetAssetBundleInfoByName(name.shortName);
                if(currNode is BundleVariantFolderInfo)
                {
                    currNode = (currNode as BundleVariantFolderInfo).GetAssetBundleInfoByName(name.variant);
                }
                return currNode;
            }
            else
            {
                return null;
            }
        }

        internal static AssetBundleInfo HandleDedupeBundles(IEnumerable<AssetBundleInfo> bundles, bool onlyOverlappedAssets)
        {
            var newBundle = CreateEmptyBundle();
            HashSet<string> dupeAssets = new HashSet<string>();
            HashSet<string> fullAssetList = new HashSet<string>();

            //if they were just selected, then they may still be updating.
            bool doneUpdating = s_UpdateAssetBundleInfos.Count == 0;
            while (!doneUpdating)
                doneUpdating = Update();

            foreach (var bundle in bundles)
            {
                foreach (var asset in bundle.GetDependencies())
                {
                    if (onlyOverlappedAssets)
                    {
                        if (!fullAssetList.Add(asset.fullAssetName))
                            dupeAssets.Add(asset.fullAssetName);
                    }
                    else
                    {
                        if (asset.IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
                            dupeAssets.Add(asset.fullAssetName);
                    }
                }
            }

            if (dupeAssets.Count == 0)
                return null;
            
            MoveAssetToBundle(dupeAssets, newBundle.m_Name.bundleName, string.Empty);
            ExecuteAssetMove();
            return newBundle;
        }

        internal static AssetBundleInfo HandleConvertToVariant(BundleDataInfo bundle)
        {
            bundle.HandleDelete(true, bundle.m_Name.bundleName, kNewVariantBaseName);
            ExecuteAssetMove();
            var root = bundle.parent.GetAssetBundleInfoByName(bundle.m_Name.shortName) as BundleVariantFolderInfo;

            if (root != null)
                return root.GetAssetBundleInfoByName(kNewVariantBaseName);
            else
            {
                //we got here because the converted bundle was empty.
                var vfolder = new BundleVariantFolderInfo(bundle.m_Name.bundleName, bundle.parent);
                var vdata = new BundleVariantDataInfo(bundle.m_Name.bundleName + "." + kNewVariantBaseName, vfolder);
                bundle.parent.AddChild(vfolder);
                vfolder.AddChild(vdata);
                return vdata;
            }
        }

        internal static void MoveAssetToBundle(AssetInfo asset, string bundleName, string variant)
        {
            s_MoveDatas.Add(new AssetBundleMoveData(asset.fullAssetName, bundleName, variant));
        }

        internal static void MoveAssetToBundle(string assetName, string bundleName, string variant)
        {
            s_MoveDatas.Add(new AssetBundleMoveData(assetName, bundleName, variant));
        }

        internal static void MoveAssetToBundle(IEnumerable<AssetInfo> assets, string bundleName, string variant)
        {
            foreach (var asset in assets)
                MoveAssetToBundle(asset, bundleName, variant);
        }

        internal static void MoveAssetToBundle(IEnumerable<string> assetNames, string bundleName, string variant)
        {
            foreach (var assetName in assetNames)
                MoveAssetToBundle(assetName, bundleName, variant);
        }

        internal static void ExecuteAssetMove(bool forceAct=true)
        {
            var size = s_MoveDatas.Count;
            if(forceAct)
            {
                if (size > 0)
                {
                    bool autoRefresh = EditorPrefs.GetBool("kAutoRefresh");
                    EditorPrefs.SetBool("kAutoRefresh", false);
                    AssetDatabase.StartAssetEditing();
                    EditorUtility.DisplayProgressBar("Moving assets to bundles", "", 0);
                    for (int i = 0; i < size; i++)
                    {
                        EditorUtility.DisplayProgressBar("Moving assets to bundle " + s_MoveDatas[i].bundleName, System.IO.Path.GetFileNameWithoutExtension(s_MoveDatas[i].assetName), (float)i / (float)size);
                        s_MoveDatas[i].Apply();
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                    EditorPrefs.SetBool("kAutoRefresh", autoRefresh);
                    s_MoveDatas.Clear();
                }
                if (!assetBundleData.IsReadOnly ())
                {
                    assetBundleData.RemoveUnusedAssetBundleNames();
                }
                Refresh();
            }
        }
        
        //this version of CreateAsset is only used for dependent assets. 
        internal static AssetInfo CreateAsset(string name, AssetInfo parent)
        {
            if (ValidateAsset(name))
            {
                var bundleName = GetBundleName(name); 
                return CreateAsset(name, bundleName, parent);
            }
            return null;
        }

        internal static AssetInfo CreateAsset(string name, string bundleName)
        {
            if(ValidateAsset(name))
            {
                return CreateAsset(name, bundleName, null);
            }
            return null;
        }

        private static AssetInfo CreateAsset(string name, string bundleName, AssetInfo parent)
        {
            if(!string.IsNullOrEmpty(bundleName))
            {
                return new AssetInfo(name, bundleName);
            }
            else
            {
                if(!s_GlobalAssetList.TryGetValue(name, out AssetInfo info))
                {
                    info = new AssetInfo(name, string.Empty);
                    s_GlobalAssetList.Add(name, info);
                }
                info.AddParent(parent.displayName);
                return info;
            }
        }

        internal static bool ValidateAsset(string name)
        {
            if (!name.StartsWith("Assets/"))
                return false;
            string ext = System.IO.Path.GetExtension(name);
            if (string.Equals(ext, ".dll", System.StringComparison.Ordinal) ||
                string.Equals(ext, ".cs", StringComparison.Ordinal) ||
                string.Equals(ext, ".meta", StringComparison.Ordinal) ||
                string.Equals(ext, ".js", StringComparison.Ordinal) ||
                string.Equals(ext, ".boo", StringComparison.Ordinal))
                return false;

            return true;
        }

        internal static string GetBundleName(string asset)
        {
            return assetBundleData.GetAssetBundleName (asset);
        }

        internal static int RegisterAsset(AssetInfo asset, string bundle)
        {
            if(s_DependencyTracker.ContainsKey(asset.fullAssetName))
            {
                s_DependencyTracker[asset.fullAssetName].Add(bundle);
                int count = s_DependencyTracker[asset.fullAssetName].Count;
                if (count > 1)
                    asset.SetMessageFlag(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles, true);
                return count;
            }

            var bundles = new HashSet<string>();
            bundles.Add(bundle);
            s_DependencyTracker.Add(asset.fullAssetName, bundles);
            return 1;            
        }

        internal static void UnRegisterAsset(AssetInfo asset, string bundle)
        {
            if (s_DependencyTracker == null || asset == null)
                return;

            if (s_DependencyTracker.ContainsKey(asset.fullAssetName))
            {
                s_DependencyTracker[asset.fullAssetName].Remove(bundle);
                int count = s_DependencyTracker[asset.fullAssetName].Count;
                switch (count)
                {
                    case 0:
                        s_DependencyTracker.Remove(asset.fullAssetName);
                        break;
                    case 1:
                        asset.SetMessageFlag(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles, false);
                        break;
                    default:
                        break;
                }
            }
        }

        internal static IEnumerable<string> CheckDependencyTracker(AssetInfo asset)
        {
            if (s_DependencyTracker.ContainsKey(asset.fullAssetName))
            {
                return s_DependencyTracker[asset.fullAssetName];
            }
            return new HashSet<string>();
        }
        
        //TODO - switch local cache server on and utilize this method to stay up to date.
        //static List<string> m_importedAssets = new List<string>();
        //static List<string> m_deletedAssets = new List<string>();
        //static List<KeyValuePair<string, string>> m_movedAssets = new List<KeyValuePair<string, string>>();
        //class AssetBundleChangeListener : AssetPostprocessor
        //{
        //    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        //    {
        //        m_importedAssets.AddRange(importedAssets);
        //        m_deletedAssets.AddRange(deletedAssets);
        //        for (int i = 0; i < movedAssets.Length; i++)
        //            m_movedAssets.Add(new KeyValuePair<string, string>(movedFromAssetPaths[i], movedAssets[i]));
        //        //m_dirty = true;
        //    }
        //}

        static internal void LogError(string message)
        {
            Debug.LogError("AssetBundleBrowser: " + message);
        }

        static internal void LogWarning(string message)
        {
            Debug.LogWarning("AssetBundleBrowser: " + message);
        }

        static private void FindBundleIcons()
        {
            s_FolderIcon = EditorGUIUtility.FindTexture("Folder Icon");

            var packagePath = System.IO.Path.GetFullPath("Packages/com.unity.assetbundlebrowser");
            if (System.IO.Directory.Exists(packagePath))
            {
                s_BundleIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.assetbundlebrowser/Editor/Icons/ABundleBrowserIconY1756Basic.png", typeof(Texture2D));
                s_sceneIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.assetbundlebrowser/Editor/Icons/ABundleBrowserIconY1756Scene.png", typeof(Texture2D));
            }
        }
    }
}
