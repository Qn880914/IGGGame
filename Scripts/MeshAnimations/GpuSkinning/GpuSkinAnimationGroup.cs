using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGG.MeshAnimation
{
    public class GpuSkinAnimationGroup : IAnimationGroup
    {
        private Dictionary<RoleAnimationType, AnimationData> m_animationsData;


        public GpuSkinAnimationGroup(GpuSkinData data)
        {
            Fps = data.Fps;
            FrameLength = 1f / Fps;

            BuildAnimationData(data);
        }

        public float Fps { get; private set; }

        public float FrameLength { get; private set; }

        public void Destroy()
        {
            if (null != m_animationsData)
            {
                m_animationsData.Clear();
                m_animationsData = null;
            }
        }

        private void BuildAnimationData(GpuSkinData pData)
        {
            m_animationsData = new Dictionary<RoleAnimationType, AnimationData>(new RoleAnimationTypeCompare());

            for (int i = 0; i < pData.Clips.Length; i++)
            {
                RoleAnimationType animType;

                if (AnimationType.AnimationTypeConvert.TryGetValue(pData.Clips[i].ClipName, out animType))
                {
                    GpuSkinAnimationClipData aniData = new GpuSkinAnimationClipData();
                    aniData.FrameCount = pData.Clips[i].FrameNum;
                    aniData.Name = pData.Clips[i].ClipName;
                    aniData.Type = animType;
                    aniData.StartFrameIndex = pData.Clips[i].StartFrameIndex;
                    m_animationsData.Add(animType, aniData);
                }
            }
        }

        public AnimationData GetAnimationData(RoleAnimationType type)
        {
            AnimationData data;
            m_animationsData.TryGetValue(type, out data);
            return data;
        }
    }
}
