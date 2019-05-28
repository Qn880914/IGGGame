#region Namespace

using System;
using UnityEngine;

#endregion
namespace IGG.Animation
{
    [RequireComponent(typeof(RuntimeBaseUnitSm))]
    [DisallowMultipleComponent]
    public class GpuSkinAnimator : MonoBehaviour, IUpdatableIggBehaviour, IAnimator
    {
        public Action<AnimationData> AnimationFinished
        {
            get;
            set;
        }

        public Action<AnimationData> AnimationLooped
        {
            get;
            set;
        }

        public Action<AnimationData> AnimationStarted
        {
            get;
            set;
        }

        public Action<AnimationData> AnimationWillStart
        {
            get;
            set;
        }

        // this should always be 1 when not playing a walk cycle
        protected float animationSpecificSpeedFactor = 1f;

        protected RuntimeBaseUnitSm animationStateMachine;

        protected Transform cachedTransform;

        protected int endFrame;

        protected float frameInterval;

        /// <summary>
        /// 每帧的时间
        /// 小等于0，表示使用DeltaTime
        /// 大于0，每示每帧固定用这个时间算
        /// </summary>
        public double FrameTime
        {
            get;
            set;
        }

        // last frame displayed was supposed to be at this time. next animation frame should be calculated from this value
        protected float lastUsedAnimationTime;

        /// <summary>
        /// 上一次的帧ID
        /// </summary>
        private int m_lastFrame = -1;

        /// <summary>
        // 逻辑时间, 就是真正过去的时间
        /// </summary>
        private double m_logicTime;

        protected float nextUpdate;

        protected float normalizedRatio;

        protected float normalizedTime;

        protected int startFrame;

        private bool wasUpdateSincePlay = false;

        private int m_frameIndexId;

        public IAnimationGroup AnimationGroup { get; set; }

        public AnimationData CurrentAnimation { get; set; }

        public bool IsRepeatPlay { get; set; }

        public bool IsPlaying { get; /*protected*/ set; }

        public int CurrentFrame { get; set; }

        public int FrameIndex { get; private set; }

        // This value is multiplied to the regular frame interval.
        // 1 is regular speed. > 1 is slower, < 1 faster
        public float SpeedFactor { get; set; }

        public float NormalizedTime
        {
            get { return normalizedTime; }
        }

        public bool IsEnabled
        {
            get { return gameObject.activeInHierarchy && enabled; }
        }

        /// <summary>
        /// Run the animation sequence if animation is currently running.
        /// Restart the animation at the end of the sequence if repeat is on,
        /// otherwise stop the animation.
        /// </summary>
        public void UpdateMonoBehaviour(ITime pTime)
        {
            if (CurrentAnimation == null ||
                !IsPlaying && wasUpdateSincePlay ||
                SpeedFactor <= 0)
            {
                return;
            }

            if (!wasUpdateSincePlay)
            {
                // considering that it was played in the previous frame.
                nextUpdate += pTime.FixedTime - pTime.FixedDeltaTime;
                wasUpdateSincePlay = true;
            }

            if (m_lastFrame != CurrentFrame)
            {
                //((MeshAnimationGroup)AnimationGroup).UpdateMeshWithAttach(CurrentFrame, CurrentAnimation.Type, m_hostMeshFilter);
                FrameIndex = ((GpuSkinAnimationClipData)CurrentAnimation).StartFrameIndex + CurrentFrame;
                m_lastFrame = CurrentFrame;
            }

            double logicFrameTime = -1;
            if (FrameTime > 0)
            {
                //走战斗中的帧刷新
                logicFrameTime = FrameTime * SpeedFactor * animationSpecificSpeedFactor;
                CurrentFrame = (int)Math.Round(m_logicTime / logicFrameTime);
                m_logicTime += FrameTime;
            }
            else
            {
                float currentTime = pTime.FixedTime;
                if (currentTime > nextUpdate)
                {
                    float modifiedSingleFrameTime = frameInterval * SpeedFactor * animationSpecificSpeedFactor;

                    int frameIncrement = (int)((currentTime - lastUsedAnimationTime) / modifiedSingleFrameTime);

                    if (frameIncrement > 0)
                    {
                        CurrentFrame += frameIncrement;
                        SetNormalizedTime();

                        float overshoot = currentTime - nextUpdate;
                        // calculate exactly when the next update should be
                        nextUpdate = lastUsedAnimationTime + (frameIncrement + 1) * modifiedSingleFrameTime;
                        lastUsedAnimationTime = currentTime - overshoot;
                    }
                }
            }

            if (CurrentFrame > endFrame)
            {
                if (IsRepeatPlay)
                {
                    if (AnimationLooped != null)
                    {
                        AnimationLooped(CurrentAnimation);
                    }

                    CurrentFrame = endFrame == 0 || CurrentFrame > endFrame ? 0 : CurrentFrame % endFrame;
                    if (logicFrameTime > 0)
                    {
                        m_logicTime = m_logicTime % logicFrameTime;
                    }
                }
                else
                {
                    CurrentFrame = endFrame;
                    RoleAnimationType endType = CurrentAnimation.Type;
                    Stop();
                    OnPlayOver(endType);
                }
            }
        }

        protected void Awake()
        {
            cachedTransform = transform;
            animationStateMachine = gameObject.GetComponent<RuntimeBaseUnitSm>();
            animationStateMachine.Animator = this;

            SpeedFactor = 1;
            FrameTime = -1;
            m_frameIndexId = Shader.PropertyToID("frameIndex");
        }

        private void OnDestroy()
        {
            animationStateMachine = null;
        }

        /// <summary>
        /// 设置当前动画是否循环
        /// </summary>
        /// <param name="AnimName"></param>
        /// <param name="Loopable"></param>
        public void SetAnimationLoopable(string AnimName, bool Loopable)
        {
            if (null == CurrentAnimation)
            {
                return;
            }

            if (CurrentAnimation.Name == AnimName)
            {
                IsRepeatPlay = Loopable;
            }
        }

        /// <summary>
        /// Set the mesh animation group for this animator.
        /// </summary>
        /// <param name="pGroup">Mesh animation group</param>
        public void SetAnimationGroup(IAnimationGroup group)
        {
            AnimationGroup = group;
        }

        public void SetAnimationSpeedFactor(RoleAnimationType pAnimationName, float pFactor, bool pUseMachine)
        {
            if (pUseMachine && null != animationStateMachine)
            {
                animationStateMachine.SetAnimationSpeedScale(pAnimationName, pFactor);
            }
            else
            {
                AnimationData anim = AnimationGroup.GetAnimationData(pAnimationName);
                if (null != anim)
                {
                    anim.SpeedScale = pFactor;
                }
            }
        }

        public void SetBoolValue(string boolKey, bool boolValue)
        {
            if (null != animationStateMachine)
            {
                animationStateMachine.SetBoolValue(boolKey, boolValue);
            }
        }

        /// <summary>
        /// 兼容旧的API
        /// </summary>
        /// <param name="pAnimName"></param>
        /// <param name="pRepeat"></param>
        public void Play(string pAnimName, bool pRepeat)
        {
            RoleAnimationType animType;
            AnimationType.AnimationTypeConvert.TryGetValue(pAnimName, out animType);
            Play(animType, pRepeat);
        }

        /// <summary>
        /// Play specified mesh animation with repeat setting.
        /// <param name="pAnimName">Name of mesh animation</param>
        /// <param name="pRepeat">Set playback repeat</param>
        /// </summary>
        public void Play(RoleAnimationType pAnimName, bool pRepeat)
        {
            if (null != animationStateMachine && null != CurrentAnimation)
            {
                bool bRet = animationStateMachine.IsSammeWithPrevAnim(CurrentAnimation.Type,
                                                                      pAnimName);

                if (bRet)
                {
                    return;
                }
            }

            //        IGGDebug.Assert(CurrentAnimation != null,
            //"MeshAnimator.Play(): Animation not found (" + pAnimName + ")", null);

            int start = 0;

            if (null != animationStateMachine)
            {
                animationStateMachine.PlayRange(pAnimName, pRepeat);
            }
            else
            {
                PlayRange(pAnimName, pRepeat, start, -1);
            }
        }

        public void PlayRandomSequence(string[] pAnimNames)
        {
            if (IsPlaying && CurrentAnimation != null)
            {
                Stop();
            }
        }


        /// <summary>
        /// Plays the animation range.  Currently this is specified in frame number.  May be better
        /// to use normalized time instead.
        /// </summary>
        /// <param name="pAnimName">Animation name</param>
        /// <param name="pRepeat">Play on repeat or not</param>
        /// <param name="pStart">Range start frame</param>
        /// <param name="pEnd">Range end frame</param>
        public void PlayRange(RoleAnimationType pAnimName, bool pRepeat, int pStart, int pEnd)
        {
            if (IsPlaying && CurrentAnimation != null)
            {
                Stop();
            }

            CurrentAnimation = AnimationGroup.GetAnimationData(pAnimName);
            IsRepeatPlay = pRepeat;

            if (CurrentAnimation != null)
            {
                if (AnimationWillStart != null)
                {
                    AnimationWillStart(CurrentAnimation);
                }

                startFrame = Mathf.Clamp(pStart, 0, CurrentAnimation.FrameCount - 1);

                if (pEnd == -1)
                {
                    endFrame = CurrentAnimation.FrameCount - 1;
                }
                else
                {
                    endFrame = Mathf.Clamp(pEnd, 0, CurrentAnimation.FrameCount - 1);
                }

                animationSpecificSpeedFactor = 1.0f / CurrentAnimation.SpeedScale;
                if (float.IsInfinity(animationSpecificSpeedFactor))
                {
                    animationSpecificSpeedFactor = 1;
                    UnityEngine.Debug.LogError("animationSpecificSpeedFactor is infinity" +  "MeshAnimator.PlayRange");
                }

                frameInterval = 1.0f / ((GpuSkinAnimationGroup)AnimationGroup).Fps;
                nextUpdate = /*Time.time + */frameInterval * SpeedFactor * animationSpecificSpeedFactor;
                CurrentFrame = startFrame;
                m_lastFrame = -1;
                m_logicTime = 0;
                wasUpdateSincePlay = false;

                if (CurrentAnimation.FrameCount == 0)
                {
                    normalizedRatio = 0;
                }
                else
                {
                    normalizedRatio = 1.0f / (float)CurrentAnimation.FrameCount;
                }

                SetNormalizedTime();

                IsPlaying = true;

                if (AnimationStarted != null)
                {
                    AnimationStarted(CurrentAnimation);
                }
            }
        }

        /// <summary>
        ///  Stop the current animation and fire AnimationFinished callback.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;

            AnimationData lastAnimation = CurrentAnimation;
            CurrentAnimation = null;
            if (AnimationFinished != null)
            {
                AnimationFinished(lastAnimation);
            }
        }

        public void OnPlayOver(RoleAnimationType lastAniName)
        {
            if (null != animationStateMachine)
            {
                animationStateMachine.OnChangeState(lastAniName);
            }
        }

        /// <summary>
        /// Calculate the current normalized time
        /// </summary>
        protected void SetNormalizedTime()
        {
            normalizedTime = (float)CurrentFrame * normalizedRatio;
            normalizedTime = Mathf.Clamp(normalizedTime, 0, 1);
        }

        public bool HasAnimation(RoleAnimationType type)
        {
            return AnimationGroup.GetAnimationData(type) != null;
        }

        public void AddAnimationCallBack(Action<AnimationData> callback, AnimationCallBackType type)
        {
            switch (type)
            {
                case AnimationCallBackType.WillStart:
                    AnimationWillStart -= callback;
                    AnimationWillStart += callback;

                    break;
                case AnimationCallBackType.Start:
                    AnimationStarted -= callback;
                    AnimationStarted += callback;
                    break;
                case AnimationCallBackType.Finish:
                    AnimationFinished -= callback;
                    AnimationFinished += callback;
                    break;
                case AnimationCallBackType.Loop:
                    AnimationLooped -= callback;
                    AnimationLooped += callback;
                    break;
            }
        }

        public void RemoveAnimationCallBack(Action<AnimationData> callback, AnimationCallBackType type)
        {
            switch (type)
            {
                case AnimationCallBackType.WillStart:
                    AnimationWillStart -= callback;

                    break;
                case AnimationCallBackType.Start:
                    AnimationStarted -= callback;
                    break;
                case AnimationCallBackType.Finish:
                    AnimationFinished -= callback;
                    break;
                case AnimationCallBackType.Loop:
                    AnimationLooped -= callback;
                    break;
            }
        }
    }
}
