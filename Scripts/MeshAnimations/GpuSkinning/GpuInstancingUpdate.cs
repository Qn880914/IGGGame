using IGG.Game.Core;

namespace IGG.MeshAnimation
{
    [ExternalComponent]
    public class GpuInstancingUpdate : IGG.Utility.Singleton<GpuInstancingUpdate>
    {
        public void Awake()
        {
            GpuInstancingMgr.instance.Clear();
        }
        
        public void LateUpdate()
        {
            GpuInstancingMgr.instance.Update();
        }

        public void OnDestroy()
        {
            GpuInstancingMgr.instance.Clear();
        }
    }
}
