namespace IGG.Core.Resource
{
    /// <summary>
    /// 加载信息
    /// </summary>
    public class LoaderInfo
    {
        /// <summary>
        /// 回调
        /// </summary>
        public LoaderGroupCompleteCallback completeCallback { get; set; }

        /// <summary>
        /// 加载器
        /// </summary>
        public Loader loader { get; set; }
    }
}
