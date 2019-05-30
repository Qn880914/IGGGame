using UnityEditor;
using System;

namespace AssetBundleBrowser
{
    /// <summary>
    ///     <para> Build Info struct used by ABDataSource to pass needed build data around. </para>
    /// </summary>
    public partial class BuildAssetBundleSettings
    {
        /// <summary>
        ///     <para> Directory to place build result </para>
        /// </summary>
        public string outputDirectory { get; set; }

        /// <summary>
        ///     <para> Standard asset bundle build options. </para>
        /// </summary>
        public BuildAssetBundleOptions options { get; set; }

        /// <summary>
        ///     <para> Target platform for build. </para>
        /// </summary>
        public BuildTarget buildTarget { get; set; }

        /// <summary>
        ///     <para> Callback for build event. </para>
        /// </summary>
        public Action<string> buildCallback { get; set; }
    }

    /// <summary>
    /// Interface class used by browser. It is expected to contain all information needed to display predicted bundle layout.
    ///  Any class deriving from this interface AND imlementing CreateDataSources() will be picked up by the browser automatically
    ///  and displayed in an in-tool dropdown.  By default, that dropdown is hidden if the browser detects no external data sources.
    ///  To turn it on, right click on tab header "AssetBUndles" and enable "Custom Sources"
    ///  
    /// Must implement CreateDataSources() to be picked up by the browser.
    ///   public static List<ABDataSource> CreateDataSources();
    /// 
    /// </summary>
    public partial interface AssetBundleData
    {
        //// all derived classes must implement the following interface in order to be picked up by the browser.
        //public static List<ABDataSource> CreateDataSources();

        /// <summary>
        /// Name of DataSource. Displayed in menu as "Name (ProvidorName)"
        /// </summary>
        string name { get; }

        /// <summary>
        /// Name of provider for DataSource. Displayed in menu as "Name (ProvidorName)"
        /// </summary>
        string providerName { get; }

        /// <summary>
        /// Array of paths in bundle.
        /// </summary>
        string[] GetAssetPathsFromAssetBundle (string assetBundleName);

        /// <summary>
        /// Name of bundle explicitly associated with asset at path.  
        /// </summary>
        string GetAssetBundleName(string assetPath);

        /// <summary>
        /// Name of bundle associated with asset at path.  
        ///  The difference between this and GetAssetBundleName() is for assets unassigned to a bundle, but
        ///  residing inside a folder that is assigned to a bundle.  Those assets will implicilty associate
        ///  with the bundle associated with the parent folder.
        /// </summary>
        string GetImplicitAssetBundleName(string assetPath);

        /// <summary>
        /// Array of asset bundle names in project
        /// </summary>
        string[] GetAllAssetBundleNames();

        /// <summary>
        /// If this data source is read only. 
        ///  If this returns true, much of the Browsers's interface will be diabled (drag&drop, etc.)
        /// </summary>
        bool IsReadOnly();

        /// <summary>
        /// Sets the asset bundle name (and variant) on a given asset
        /// </summary>
        void SetAssetBundleNameAndVariant (string assetPath, string bundleName, string variantName);

        /// <summary>
        /// Clears out any asset bundle names that do not have assets associated with them.
        /// </summary>
        void RemoveUnusedAssetBundleNames();

        /// <summary>
        /// Signals if this data source can have build target set by tool
        /// </summary>
        bool canSpecifyBuildTarget { get; }

        /// <summary>
        /// Signals if this data source can have output directory set by tool
        /// </summary>
        bool canSpecifyBuildOutputDirectory { get; }

        /// <summary>
        /// Signals if this data source can have build options set by tool
        /// </summary>
        bool canSpecifyBuildOptions { get; }

        /// <summary>
        /// Executes data source's implementation of asset bundle building.
        ///   Called by "build" button in build tab of tool.
        /// </summary>
        bool BuildAssetBundles (BuildAssetBundleSettings info);
    }
}
