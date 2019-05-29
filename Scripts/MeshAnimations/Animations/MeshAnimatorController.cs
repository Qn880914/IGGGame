/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:30
	file base:	meshanimatorcontroller
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

namespace IGG.MeshAnimation
{
    public class MeshAnimatorController : MonoBehaviour
    {
        //private static Stack<FrameRateBasedUpdateGroup<MeshAnimator>> animatorGroupStack = new Stack<FrameRateBasedUpdateGroup<MeshAnimator>>();

        private static FrameRateBasedUpdateGroup<IUpdatableIggBehaviour> animatorGroup =
            new FrameRateBasedUpdateGroup<IUpdatableIggBehaviour>(0.04f);

        private static GameObject singleton;

        /*public static void PushAnimatorsGroup()
		{
			//IGG.Logging.Logger.LogWarning("Pushing new animators group.");
			animatorGroupStack.Push(new FrameRateBasedUpdateGroup<MeshAnimator>());
		}

		public static void PopAnimatorsGroup()
		{
			//IGG.Logging.Logger.LogWarning("Popping animators group.");
			animatorGroupStack.Pop ();
		}*/

        public static void AddAnimator(IUpdatableIggBehaviour pAnimator)
        {
            if (singleton == null)
            {
                CreateUpdaterSingleton();
            }

            animatorGroup.AddMonoBehaviour(pAnimator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAnimator"></param>
        /// <returns>IsLastOneWithClear</returns>
        public static bool RemoveAnimator(IUpdatableIggBehaviour pAnimator)
        {
            animatorGroup.RemoveMonoBehaviour(pAnimator);

            if (animatorGroup.IsListEmpty())
            {
                MeshAnimationLoader.instance.Clear();
                Destroy(singleton);
                singleton = null;
                return true;
            }

            return false;
        }

        private static void CreateUpdaterSingleton()
        {
            singleton = new GameObject("_MeshAnimatorUpdater");
            DontDestroyOnLoad(singleton);
            singleton.transform.parent = SingletonHolder.PermanentGameObjectParent;
            singleton.AddComponent<MeshAnimatorController>();
        }

        private void Update()
        {
            /*var battle = BattleDC.Battle;
            if (battle == null)
            {
                animatorGroup.Update(Time.Default);
                m_realUpdated = false;
            }
            else
            {
                //如果战斗已经结束，那么继续由这里接管
                if (battle.CurState == BattleState.Finish)
                {
                    animatorGroup.Update(Time.Default);
                    if (m_isListenUpdate)
                    {
                        battle.EC.AntiRegisterHooks(GameEventType.BattleUpdateAfter, BattleUpdateHandler);
                        m_isListenUpdate = false;
                    }

                    return;
                }

                if (!m_isListenUpdate)
                {
                    battle.EC.RegisterHooks(GameEventType.BattleUpdateAfter, BattleUpdateHandler);
                    m_isListenUpdate = true;
                }

                if (!m_realUpdated)
                {
                    animatorGroup.Update(Time.Default);
                }
            }*/
        }

        private void BattleUpdateHandler(int eventSend, object param)
        {
            animatorGroup.Update(Time.Default);
        }

        private void OnDestroy()
        {
            /*if (m_isListenUpdate && BattleDC.Battle != null)
            {
                BattleDC.Battle.EC.AntiRegisterHooks(GameEventType.BattleUpdateAfter, BattleUpdateHandler);
                m_isListenUpdate = false;
            }*/
        }
    }
}