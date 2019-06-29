using IGG.Core.Manager;
using System.Collections.Generic;

namespace IGG.Core.Resource
{
    /// <summary>
    /// 加载信息
    /// </summary>
    public class LoaderData
    {
        /// <summary>
        /// 回调函数列表
        /// </summary>
        public List<LoadCompleteCallback> completeCallbacks;

        /// <summary>
        /// 加载器
        /// </summary>
        public Loader loader;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int referenceCount;
    }
}
