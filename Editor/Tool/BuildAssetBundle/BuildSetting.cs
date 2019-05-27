public class BuildSettings
{
    /// <summary>
    ///    <para> assetbundle 输出目录 </para>
    /// </summary>
    /// <value>The output dir.</value>
    public static string outputDir {get{return "output";}}

    /// <summary>
    ///     <para> assetbundle 输出路径 </para>
    /// </summary>
    /// <value>The output path.</value>
    public static string outputPath
    {
        get
        {
            return string.Format("{0}/ab/{1}/", outputDir, ConstantData.mainVersion);
        }
    }

    public static string patchDir
    {
        get
        {
            return string.Format("{0}/patch/{1}/", outputDir, ConstantData.mainVersion);
        }
    }

    /// <summary>
    ///     <para> 是否清除旧的AssetBundle (全部重新打包)</para>
    /// </summary>
    public static bool clearAssetBundle = false;

    /// <summary>
    ///     <para> 是否重置 AssetBundle Name </para>
    /// </summary>
    public static bool resetAssetBundleName = false;

    /// <summary>
    ///     <para> 开启冗余资源检测 </para>
    /// </summary>
    public static bool enableAssetBundleRedundance = true;
}
