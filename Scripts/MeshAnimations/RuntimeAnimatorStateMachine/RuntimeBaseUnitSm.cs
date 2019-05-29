#region Namespace

using UnityEngine;
using Random = System.Random;

#endregion

/**
Author: Aoicocoon
**/
namespace IGG.MeshAnimation
{
    public class
        RuntimeBaseUnitSm : MonoBehaviour
    {
        private Random m_animRnd;
        private int m_AnimSwitchProp = 5;

        public IAnimator Animator { get; set; }

        private void Awake()
        {
            m_animRnd = new Random(GetInstanceID());
        }

        public virtual void SetBoolValue(string boolKey, bool boolValue)
        {
        }

        public void PlayRange(RoleAnimationType animName, bool repeast)
        {
            switch (animName)
            {
                case RoleAnimationType.Attack:
                    Attack();
                    break;
                case RoleAnimationType.Dead:
                    Dead();
                    break;
                case RoleAnimationType.Hit:
                    Hit();
                    break;
                case RoleAnimationType.Repel:
                    Repel();
                    break;
                case RoleAnimationType.Walk:
                    Walk();
                    break;
                case RoleAnimationType.Run:
                    Run();
                    break;
                case RoleAnimationType.Skill1:
                    Skill1();
                    break;
                case RoleAnimationType.Wait:
                    Wait();
                    break;
                case RoleAnimationType.Skill2:
                    Skill2();
                    break;
                case RoleAnimationType.Win:
                    Win();
                    break;
            }
        }

        protected bool IsPassRndSwitch()
        {
            return m_animRnd.Next(1, 100) > m_AnimSwitchProp;
        }

        protected virtual void Attack()
        {
            //if (IsPassRndSwitch()) {
            //    Attack1();
            //} else {
            //    Attack2();
            //}
            Attack1();
        }

        protected virtual void Dead()
        {
            Animator.PlayRange(RoleAnimationType.Dead, false, 0, -1);
        }

        protected virtual void Hit()
        {
            Animator.PlayRange(RoleAnimationType.Hit, false, 0, -1);
        }

        protected virtual void Repel()
        {
            Animator.PlayRange(RoleAnimationType.Repel, false, 0, -1);
        }

        protected virtual void Walk()
        {
            //TODO:因为原来资源的动作槽有限，所以把走路动作放在hit里
            Animator.PlayRange(RoleAnimationType.Hit, true, 0, -1);
        }

        protected virtual void Run()
        {
            Animator.PlayRange(RoleAnimationType.Run, true, 0, -1);
        }

        protected virtual void Skill1()
        {
            Animator.PlayRange(RoleAnimationType.Skill1, false, 0, -1);
        }

        protected virtual void Skill2()
        {
            Animator.PlayRange(RoleAnimationType.Skill2, false, 0, -1);
        }

        protected virtual void Win()
        {
            Animator.PlayRange(RoleAnimationType.Win, true, 0, -1);
        }

        protected virtual void Wait()
        {
            if (IsPassRndSwitch())
            {
                Wait1();
            }
            else
            {
                Wait2();
            }
        }

        protected virtual void Wait1()
        {
            Animator.PlayRange(RoleAnimationType.Wait1, false, 0, -1);
        }

        protected virtual void Wait2()
        {
            AnimationData anim = Animator.AnimationGroup.GetAnimationData(RoleAnimationType.Wait2);
            if (null == anim)
            {
                int startFrame = 0;
                anim = Animator.AnimationGroup.GetAnimationData(RoleAnimationType.Wait1);
                if (null != anim)
                {
                    startFrame = UnityEngine.Random.Range(0, anim.FrameCount >> 1);
                }

                Animator.PlayRange(RoleAnimationType.Wait1, false, startFrame, -1);
            }
            else
            {
                Animator.PlayRange(RoleAnimationType.Wait2, false, 0, -1);
            }
        }

        protected virtual void Attack1()
        {
            Animator.PlayRange(RoleAnimationType.Attack1, false, 0, -1);
        }

        protected virtual void Attack2()
        {
            AnimationData anim = Animator.AnimationGroup.GetAnimationData(RoleAnimationType.Attack2);
            if (null == anim)
            {
                Attack1();
            }
            else
            {
                Animator.PlayRange(RoleAnimationType.Attack2, false, 0, -1);
            }
        }

        public virtual bool IsSammeWithPrevAnim(RoleAnimationType prevAnim, RoleAnimationType anim)
        {
            //忽略进攻
            if (anim == RoleAnimationType.Attack)
            {
                return false;
            }

            return prevAnim == anim;
        }


        public virtual RoleAnimationType TranslateAnim(RoleAnimationType anim)
        {
            if (anim == RoleAnimationType.Wait)
            {
                if (IsPassRndSwitch())
                {
                    return RoleAnimationType.Wait1;
                }
                else
                {
                    return RoleAnimationType.Wait2;
                }
            }
            else if (anim == RoleAnimationType.Attack)
            {
                //if (IsPassRndSwitch()) {
                //    return RoleAnimationType.Attack1;
                //} else {
                //    return RoleAnimationType.Attack2;
                //}
                return RoleAnimationType.Attack1;
            }

            return anim;
        }

        public virtual void OnChangeState(RoleAnimationType oldState)
        {
            switch (oldState)
            {
                case RoleAnimationType.Run:
                    Run();
                    break;

                case RoleAnimationType.Walk:
                    Walk();
                    break;

                case RoleAnimationType.Attack:
                    Wait1();
                    break;

                case RoleAnimationType.Dead:
                    break;

                default:
                    Wait();
                    break;
            }
        }

        public void SetAnimationSpeedScale(RoleAnimationType animationName, float factor)
        {
            if (animationName == RoleAnimationType.Attack)
            {
                AnimationData anim = Animator.AnimationGroup.GetAnimationData(RoleAnimationType.Attack1);
                if (null != anim)
                {
                    anim.SpeedScale = factor;
                }

                //anim = Animator.AnimationGroup.GetAnimationData(RoleAnimationType.Attack2);
                //if(null != anim) {
                //    anim.SpeedScale = Factor;
                //}
            }
            else
            {
                AnimationData anim = Animator.AnimationGroup.GetAnimationData(animationName);
                if (null != anim)
                {
                    anim.SpeedScale = factor;
                }
            }
        }
    }
}