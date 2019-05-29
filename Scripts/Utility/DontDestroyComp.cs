using UnityEngine;

namespace IGG.FrameWork.Utils
{
    /// <summary>
    /// Desc    这个组件使所挂的GO在切换场景时不删除
    ///         它会设置为DontDestroyOnLoad后移除自身
    /// </summary>
    public class DontDestroyComp:MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
            Destroy(this);
        }
    }
}