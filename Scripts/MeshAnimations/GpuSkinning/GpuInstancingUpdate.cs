using IGG.Game.Core;
using IGG.Utility;
using System.Collections.Generic;
using UnityEngine;
namespace IGG.Animation
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
