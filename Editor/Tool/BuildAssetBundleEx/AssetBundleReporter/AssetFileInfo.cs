using System.Collections.Generic;

namespace AssetBundleBrowser
{
    /// <summary>
    ///     <para> Asset file info </para>
    /// </summary>
    /// <remarks>
    ///     Include : 
    ///             name
    ///             guid
    ///             asset Type
    ///             asset properties list
    ///             a list of AssetBundle file names that are included
    /// </remarks>
    public class AssetFileInfo
    {
        /// <summary>
        ///     <para> Asset name(It might have the same name)</para>
        /// </summary>
        public string name;

        /// <summary>
        /// 唯一ID 
        /// 需要取得 PathID 才能确保唯一性
        /// </summary>
        public long guid;

        /// <summary>
        ///     <para> Asset Type, example : "MonoScript"、"AnimatorController" </para>
        /// </summary>
        public string type;

        /// <summary>
        ///     <para> Asset properties. example : KeyValuePair<"顶点数", mesh.vertexCount>、KeyValuePair<"子网格数", mesh.subMeshCount> </para>
        /// </summary>
        public List<KeyValuePair<string, object>> propertys;

        /// <summary>
        ///     <para>A list of AssetBundle file names that are included </para>
        ///     <para> 被包含所在的AssetBundle文件名称列表 </para>
        /// </summary>
        public HashSet<AssetBundleFileInfo> includedBundles = new HashSet<AssetBundleFileInfo>();

        /// <summary>
        ///     <para> Detailed links to Excel workbooks</para>
        ///     <para> 工作簿的详细链接 </para>
        /// </summary>
        public OfficeOpenXml.ExcelHyperLink detailHyperLink;

        public override string ToString()
        {
            return name;
        }
    }
}