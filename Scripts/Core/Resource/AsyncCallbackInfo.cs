using IGG.Core.Manager;

namespace IGG.Core.Resource
{
    /// <summary>
    /// 异步加载回调信息
    /// </summary>
    public class AsyncCallbackInfo
    {
        /// <summary>
        /// 资源对象
        /// </summary>
        public object Data;

        /// <summary>
        /// 加载组
        /// </summary>
        public LoaderGroup Group;

        /// <summary>
        /// 回调函数
        /// </summary>
        public LoaderGroupCompleteCallback completeCallback;
    }
}

