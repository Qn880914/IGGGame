#region Namespace

using IGG.Game;
using IGG.MeshAnimation.Data;
using IGG.Utility;
using System;
using System.Collections.Generic;

#endregion

namespace IGG.MeshAnimation
{
    public class MeshAnimationLoader : Singleton<MeshAnimationLoader>
    {
        private Dictionary<int, MeshAnimationGroup> m_cachedGroups = new Dictionary<int, MeshAnimationGroup>();

        private CallbackBucket<int, Action<MeshAnimationGroup>> m_callbacks =
            new CallbackBucket<int, Action<MeshAnimationGroup>>();

        public MeshAnimationGroup GetAnimationGroupFromCache(int pSoldierId)
        {
            if (m_cachedGroups.ContainsKey(pSoldierId) && m_cachedGroups[pSoldierId] != null)
            {
                return m_cachedGroups[pSoldierId];
            }

            return null;
        }

        public void Clear()
        {
            if (null != m_cachedGroups)
            {
                var itor = m_cachedGroups.GetEnumerator();
                while (itor.MoveNext())
                {
                    itor.Current.Value.Destroy();
                }

                m_cachedGroups.Clear();
                itor.Dispose();
            }
        }

        public void UnloadAnimationGroupFromCache(int pUnitId)
        {
            if (m_cachedGroups.ContainsKey(pUnitId))
            {
                m_cachedGroups[pUnitId].Destroy();
                m_cachedGroups[pUnitId] = null;
                m_cachedGroups.Remove(pUnitId);
            }
        }

        public void LoadAnimationGroupSkinData(int pUnitId, string pActorName, MeshSkinData skinData,
                                               Action<MeshAnimationGroup> pCallback)
        {
            MeshAnimationGroup group;
            if (m_cachedGroups.TryGetValue(pUnitId, out group))
            {
                if (null != group && null != pCallback)
                {
                    pCallback(group);
                }
            }
            else
            {
                if (pCallback != null)
                {
                    m_callbacks.Add(pUnitId, pCallback);
                }

                MeshAnimationGroupData groupData = new MeshAnimationGroupData(skinData);
                groupData.BuildAnim(skinData);

                group = groupData.GetAnimationGroup();
                if (null == group)
                {
                    return;
                }

                m_cachedGroups[pUnitId] = group;
                if (m_callbacks.Count(pUnitId) > 0)
                {
                    foreach (Action<MeshAnimationGroup> callback in m_callbacks.Get(pUnitId))
                    {
                        if (callback != null)
                        {
                            callback(group);
                        }
                    }

                    m_callbacks.Clear(pUnitId);
                }
            }
        }
    }
}