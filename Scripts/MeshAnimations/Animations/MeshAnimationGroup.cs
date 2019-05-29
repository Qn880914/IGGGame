/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:29
	file base:	MeshAnimationGroup
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System.Collections.Generic;
using IGG.MeshAnimation.Data;
using UnityEngine;

#endregion

namespace IGG.MeshAnimation
{
    public class MeshAnimationGroup : IAnimationGroup
    {
        private Dictionary<RoleAnimationType, AnimationData> m_animationsData;

        private Dictionary<int, Dictionary<RoleAnimationType, MeshAnimation>> m_animationsWithAttach;

        private MeshAnimationGroupData m_groupData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAnimationGroup"/> class.
        /// </summary>
        /// <param name="pData">Mesh animation group data</param>
        public MeshAnimationGroup(MeshAnimationGroupData pData)
        {
            Fps = pData.Fps;
            FrameLength = 1f / Fps;
            //ModelName = pData.ModelName;
            BoneNames = pData.BoneNames;

            m_groupData = pData;
            BuildAnimationsWithAttach(pData);
        }

        public float Fps { get; private set; }

        public float FrameLength { get; private set; }

        public int AttachCount { get; private set; }

        //public string ModelName { get; private set; }

        public string[] BoneNames { get; private set; }

        public void Destroy()
        {
            if (null != m_animationsWithAttach)
            {
                var itor = m_animationsWithAttach.GetEnumerator();
                while (itor.MoveNext())
                {
                    Dictionary<RoleAnimationType, MeshAnimation> animDict = itor.Current.Value;
                    if (null != animDict)
                    {
                        var animItor = animDict.GetEnumerator();
                        while (animItor.MoveNext())
                        {
                            animItor.Current.Value.Destory();
                        }

                        animItor.Dispose();
                        animDict.Clear();
                    }
                }

                itor.Dispose();
                m_animationsWithAttach.Clear();
                m_animationsWithAttach = null;
            }

            m_animationsData = null;

            if (null != m_groupData)
            {
                m_groupData.Destory();
                m_groupData = null;
            }
        }

        private void BuildAnimationsWithAttach(MeshAnimationGroupData pData)
        {
            m_animationsWithAttach = new Dictionary<int, Dictionary<RoleAnimationType, MeshAnimation>>();
            m_animationsData = new Dictionary<RoleAnimationType, AnimationData>(new RoleAnimationTypeCompare());
            AttachCount = pData.AttachCount;

            for (int i = 0; i < pData.AttachCount; i++)
            {
                Dictionary<string, MeshAnimationData> animDataDict;
                bool hasData = pData.AnimationsWithAttach.TryGetValue(i, out animDataDict);
                if (hasData)
                {
                    var itor = animDataDict.GetEnumerator();
                    Dictionary<RoleAnimationType, MeshAnimation> animDict =
                        new Dictionary<RoleAnimationType, MeshAnimation>(new RoleAnimationTypeCompare());
                    m_animationsWithAttach.Add(i, animDict);
                    while (itor.MoveNext())
                    {
                        Vector2[] pUv = pData.Uv[i];
                        int[] pTriangles = pData.Triangles[i];

                        MeshAnimation animation = new MeshAnimation(pUv, pTriangles, itor.Current.Value);
                        string animName = itor.Current.Key;

                        RoleAnimationType animType;
                        AnimationType.AnimationTypeConvert.TryGetValue(animName, out animType);

                        animDict.Add(animType, animation);

                        //只获取firstattach的数据
                        if (i == 0)
                        {
                            AnimationData data = new AnimationData();
                            data.Name = animName;
                            data.FrameCount = itor.Current.Value.FrameCount;

                            data.Type = animType;
                            m_animationsData.Add(animType, data);
                        }
                    }

                    itor.Dispose();
                }
            }
        }

        public AnimationData GetAnimationData(RoleAnimationType type)
        {
            AnimationData data;
            m_animationsData.TryGetValue(type, out data);
            return data;
        }

        public void UpdateMeshWithAttach(int pFrame, RoleAnimationType pAnimName, List<MeshFilter> pMeshFilterList)
        {
            int count = pMeshFilterList.Count;
            for (int i = 0; i < count; i++)
            {
                MeshFilter mf = pMeshFilterList[i];

                Dictionary<RoleAnimationType, MeshAnimation> dict = m_animationsWithAttach[i];

                MeshAnimation anim;

                bool ret = dict.TryGetValue(pAnimName, out anim);

                if (ret)
                {
                    mf.mesh = anim.GetFrame(pFrame);
                }
                else
                {
                    UnityEngine.Debug.LogError(string.Format("Not Found Animation: {0}" + pAnimName));
                }
            }
        }
    }

    public class AnimationData
    {
        public int FrameCount;
        public string Name;
        public float SpeedScale = 1.0f;
        public RoleAnimationType Type;
    }
}