/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:29
	file base:	meshanimator
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace IGG.MeshAnimation
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(RuntimeBaseUnitSm))]
    [DisallowMultipleComponent]
    public class MeshAnimator : MonoBehaviour, IUpdatableIggBehaviour, IAnimator
    {
        // this should always be 1 when not playing a walk cycle
        protected float m_animationSpecificSpeedFactor = 1f;

        protected RuntimeBaseUnitSm m_animationStateMachine;

        protected Transform m_cachedTransform;

        protected int m_endFrame;

        protected float m_frameInterval;

        protected List<MeshFilter> m_hostMeshFilter;

        /// <summary>
        /// 上一次的帧ID
        /// </summary>
        private int m_lastFrame = -1;

        // last frame displayed was supposed to be at this time. next animation frame should be calculated from this value
        protected float m_lastUsedAnimationTime;

        // 逻辑时间, 就是真正过去的时间
        private double m_logicTime;

        protected MeshRenderer m_meshRenderer;

        protected float m_nextUpdate;

        protected float m_normalizedRatio;

        protected float m_normalizedTime;

        // Note:  We don't want a Dictionary<> here because it creates Garbage when iterating over elements
        protected List<string> m_particleBoneNames;
        protected List<Transform> m_particleBoneTransforms;

        protected int m_startFrame;

        private bool m_wasUpdateSincePlay;

        public Action<AnimationData> AnimationFinished { get; set; }

        public Action<AnimationData> AnimationLooped { get; set; }

        public Action<AnimationData> AnimationStarted { get; set; }

        public Action<AnimationData> AnimationWillStart { get; set; }

        /// <summary>
        /// 每帧的时间
        /// 小等于0，表示使用DeltaTime
        /// 大于0，每示每帧固定用这个时间算
        /// </summary>
        public double FrameTime { get; set; }

        public IAnimationGroup AnimationGroup { get; set; }

        public AnimationData CurrentAnimation { get; set; }

        public bool IsRepeatPlay { get; set; }

        public bool IsPlaying { get; /*protected*/ set; }

        public int CurrentFrame { get; set; }

        // This value is multiplied to the regular frame interval.
        // 1 is regular speed. > 1 is slower, < 1 faster
        public float SpeedFactor { get; set; }

        public float NormalizedTime
        {
            get { return m_normalizedTime; }
        }

        /// <summary>
        /// 设置当前动画是否循环
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="loopable"></param>
        public void SetAnimationLoopable(string animName, bool loopable)
        {
            if (null == CurrentAnimation)
            {
                return;
            }

            if (CurrentAnimation.Name == animName)
            {
                IsRepeatPlay = loopable;
            }
        }

        public void SetAnimationSpeedFactor(RoleAnimationType pAnimationName, float pFactor, bool pUseMachine)
        {
            if (pUseMachine && null != m_animationStateMachine)
            {
                m_animationStateMachine.SetAnimationSpeedScale(pAnimationName, pFactor);
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
            if (null != m_animationStateMachine)
            {
                m_animationStateMachine.SetBoolValue(boolKey, boolValue);
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
            if (null != m_animationStateMachine && null != CurrentAnimation)
            {
                bool bRet = m_animationStateMachine.IsSammeWithPrevAnim(CurrentAnimation.Type,
                                                                        pAnimName);

                if (bRet)
                {
                    return;
                }
            }

            //        IGGDebug.Assert(CurrentAnimation != null,
            //"MeshAnimator.Play(): Animation not found (" + pAnimName + ")", null);

            int start = 0;

            if (null != m_animationStateMachine)
            {
                m_animationStateMachine.PlayRange(pAnimName, pRepeat);
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

                m_startFrame = Mathf.Clamp(pStart, 0, CurrentAnimation.FrameCount - 1);

                if (pEnd == -1)
                {
                    m_endFrame = CurrentAnimation.FrameCount - 1;
                }
                else
                {
                    m_endFrame = Mathf.Clamp(pEnd, 0, CurrentAnimation.FrameCount - 1);
                }

                m_animationSpecificSpeedFactor = 1.0f / CurrentAnimation.SpeedScale;
                if (float.IsInfinity(m_animationSpecificSpeedFactor))
                {
                    m_animationSpecificSpeedFactor = 1;
                    UnityEngine.Debug.LogError("animationSpecificSpeedFactor is infinity" + "MeshAnimator.PlayRange");
                }

                m_frameInterval = 1.0f / ((MeshAnimationGroup) AnimationGroup).Fps;
                m_nextUpdate = /*Time.time + */m_frameInterval * SpeedFactor * m_animationSpecificSpeedFactor;
                CurrentFrame = m_startFrame;
                m_lastFrame = -1;
                m_logicTime = 0;
                m_wasUpdateSincePlay = false;

                if (CurrentAnimation.FrameCount == 0)
                {
                    m_normalizedRatio = 0;
                }
                else
                {
                    m_normalizedRatio = 1.0f / CurrentAnimation.FrameCount;
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
            if (null != m_animationStateMachine)
            {
                m_animationStateMachine.OnChangeState(lastAniName);
            }
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
                !IsPlaying && m_wasUpdateSincePlay ||
                SpeedFactor <= 0)
            {
                return;
            }

            if (!m_wasUpdateSincePlay)
            {
                // considering that it was played in the previous frame.
                m_nextUpdate += pTime.FixedTime - pTime.FixedDeltaTime;
                m_wasUpdateSincePlay = true;
            }

            if (m_lastFrame != CurrentFrame)
            {
                ((MeshAnimationGroup) AnimationGroup).UpdateMeshWithAttach(
                    CurrentFrame, CurrentAnimation.Type, m_hostMeshFilter);
                m_lastFrame = CurrentFrame;
            }

            double logicFrameTime = -1;
            if (FrameTime > 0)
            {
                //走战斗中的帧刷新
                logicFrameTime = FrameTime * SpeedFactor * m_animationSpecificSpeedFactor;
                CurrentFrame = (int) Math.Round(m_logicTime / logicFrameTime);
                m_logicTime += FrameTime;
            }
            else
            {
                float currentTime = pTime.FixedTime;
                if (currentTime > m_nextUpdate)
                {
                    float modifiedSingleFrameTime = m_frameInterval * SpeedFactor * m_animationSpecificSpeedFactor;

                    // figure out how many frames to increment, to account for skipped animation frames if framerate is too slow
                    //int frameIncrement = Mathf.FloorToInt((currentTime - lastUsedAnimationTime) / modifiedSingleFrameTime);
                    // Dante: code below gets rid of the division, and the FloorToInt operation from the line above.
                    // Check http://msdn.microsoft.com/en-us/library/ms973852.aspx for more info.
                    //
                    // Carson: Mathf.FloorToInt is slow, but divide with (int) is faster than looping.  
                    //			While loop guarantees a branch misprediction penalty.
                    //			See PerformanceTests.SubLoopSlowerThanDivCast()

                    int frameIncrement = (int) ((currentTime - m_lastUsedAnimationTime) / modifiedSingleFrameTime);

                    if (frameIncrement > 0)
                    {
                        CurrentFrame += frameIncrement;
                        SetNormalizedTime();

                        float overshoot = currentTime - m_nextUpdate;
                        // calculate exactly when the next update should be
                        m_nextUpdate = m_lastUsedAnimationTime + (frameIncrement + 1) * modifiedSingleFrameTime;
                        m_lastUsedAnimationTime = currentTime - overshoot;
                    }
                }
            }

            if (CurrentFrame > m_endFrame)
            {
                if (IsRepeatPlay)
                {
                    if (AnimationLooped != null)
                    {
                        AnimationLooped(CurrentAnimation);
                    }

                    CurrentFrame = m_endFrame == 0 || CurrentFrame > m_endFrame ? 0 : CurrentFrame % m_endFrame;
                    if (logicFrameTime > 0)
                    {
                        m_logicTime = m_logicTime % logicFrameTime;
                    }
                }
                else
                {
                    CurrentFrame = m_endFrame;
                    RoleAnimationType endType = CurrentAnimation.Type;
                    Stop();
                    OnPlayOver(endType);
                }
            }
        }

        protected void Awake()
        {
            m_cachedTransform = transform;
            m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
            m_animationStateMachine = gameObject.GetComponent<RuntimeBaseUnitSm>();
            m_animationStateMachine.Animator = this;

            m_particleBoneNames = new List<string>();
            m_particleBoneTransforms = new List<Transform>();

            SpeedFactor = 1;
            FrameTime = -1;
        }

        private void OnDestroy()
        {
            m_animationStateMachine = null;
        }

        /// <summary>
        /// Set the mesh animation group for this animator.
        /// </summary>
        /// <param name="group">Mesh animation group</param>
        public void SetAnimationGroup(IAnimationGroup group)
        {
            MeshAnimationGroup pGroup = (MeshAnimationGroup) group;
            m_hostMeshFilter = new List<MeshFilter>();
            MeshFilter mainMf = gameObject.AddComponent<MeshFilter>();
            m_hostMeshFilter.Add(mainMf);

            MeshRenderer mainMr = gameObject.GetComponent<MeshRenderer>();

            int attachCount = pGroup.AttachCount - 1;
            Transform parent = gameObject.transform;

            while (attachCount-- > 0)
            {
                GameObject child = new GameObject("Attach_" + attachCount);
                MeshFilter mf = child.AddComponent<MeshFilter>();
                MeshRenderer mr = child.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mainMr.sharedMaterial;

                m_hostMeshFilter.Add(mf);

                child.transform.SetParent(parent, false);
                parent = child.transform;
            }


            AnimationGroup = group;

            // Create the empty bone objects
            if (pGroup.BoneNames != null)
            {
                foreach (string curName in pGroup.BoneNames)
                {
                    GameObject go = new GameObject(curName);
                    go.transform.parent = m_cachedTransform;

                    m_particleBoneNames.Add(curName);
                    m_particleBoneTransforms.Add(go.transform);
                }
            }
        }

        public bool ContainsBone(string pBoneName)
        {
            return m_particleBoneNames.Contains(pBoneName);
        }

        public Transform GetParticleBone(string pBoneName)
        {
            int index = m_particleBoneNames.IndexOf(pBoneName);
            if (index >= 0)
            {
                return m_particleBoneTransforms[index];
            }

            return null;
        }

        /// <summary>
        /// Calculate the current normalized time
        /// </summary>
        protected void SetNormalizedTime()
        {
            m_normalizedTime = CurrentFrame * m_normalizedRatio;
            m_normalizedTime = Mathf.Clamp(m_normalizedTime, 0, 1);
        }
    }
}