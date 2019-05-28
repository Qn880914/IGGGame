/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:30
	file base:	MeshAnimatorEventController
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using IGG.Singleton;
using UnityEngine;
//using IGG.AI;

#endregion

//using IGG.GameContext;

namespace IGG.Animation
{
    public class MeshAnimatorEventController : MonoBehaviour
    {
        private static FrameRateBasedUpdateGroup<MeshAnimatorEvent> g_animatorGroup =
            new FrameRateBasedUpdateGroup<MeshAnimatorEvent>(0.04f);

        static MeshAnimatorEventController()
        {
            GameObject obj = new GameObject("_MeshAnimatorEventUpdater");
            DontDestroyOnLoad(obj);
            obj.transform.parent = SingletonHolder.PermanentGameObjectParent;
            obj.AddComponent<MeshAnimatorEventController>();
        }

        public static void AddAnimator(MeshAnimatorEvent pAnimator)
        {
            g_animatorGroup.AddMonoBehaviour(pAnimator);
        }

        public static void RemoveAnimator(MeshAnimatorEvent pAnimator)
        {
            g_animatorGroup.RemoveMonoBehaviour(pAnimator);
        }

        private void Update()
        {
            g_animatorGroup.Update(Time.Default);
        }
    }
}