using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGG.Animation
{
    public enum AnimationCallBackType
    {
        WillStart,
        Start,
        Finish,
        Loop
    }

    public interface IAnimator
    {
        Action<AnimationData> AnimationFinished
        {
            get;
            set;
        }

        Action<AnimationData> AnimationLooped
        {
            get;
            set;
        }

        Action<AnimationData> AnimationStarted
        {
            get;
            set;
        }

        Action<AnimationData> AnimationWillStart
        {
            get;
            set;
        }

        /// <summary>
        /// 每帧的时间
        /// 小等于0，表示使用DeltaTime
        /// 大于0，每示每帧固定用这个时间算
        /// </summary>
        double FrameTime
        {
            get;
            set;
        }

        IAnimationGroup AnimationGroup { get; set; }

        AnimationData CurrentAnimation { get; set; }

        bool IsRepeatPlay { get; set; }

        bool IsPlaying { get; /*protected*/ set; }

        int CurrentFrame { get; set; }

        // This value is multiplied to the regular frame interval.
        // 1 is regular speed. > 1 is slower, < 1 faster
        float SpeedFactor { get; set; }

        float NormalizedTime
        {
            get;
        }

        /// <summary>
        /// 设置当前动画是否循环
        /// </summary>
        /// <param name="AnimName"></param>
        /// <param name="Loopable"></param>
        void SetAnimationLoopable(string AnimName, bool Loopable);

        void SetAnimationSpeedFactor(RoleAnimationType pAnimationName, float pFactor, bool pUseMachine);

        void SetBoolValue(string boolKey, bool boolValue);

        /// <summary>
        /// 兼容旧的API
        /// </summary>
        /// <param name="pAnimName"></param>
        /// <param name="pRepeat"></param>
        void Play(string pAnimName, bool pRepeat);

        /// <summary>
        /// Play specified mesh animation with repeat setting.
        /// <param name="pAnimName">Name of mesh animation</param>
        /// <param name="pRepeat">Set playback repeat</param>
        /// </summary>
        void Play(RoleAnimationType pAnimName, bool pRepeat);

        void PlayRandomSequence(string[] pAnimNames);


        /// <summary>
        /// Plays the animation range.  Currently this is specified in frame number.  May be better
        /// to use normalized time instead.
        /// </summary>
        /// <param name="pAnimName">Animation name</param>
        /// <param name="pRepeat">Play on repeat or not</param>
        /// <param name="pStart">Range start frame</param>
        /// <param name="pEnd">Range end frame</param>
        void PlayRange(RoleAnimationType pAnimName, bool pRepeat, int pStart, int pEnd);

        /// <summary>
        ///  Stop the current animation and fire AnimationFinished callback.
        /// </summary>
        void Stop();

        void OnPlayOver(RoleAnimationType lastAniName);

        bool HasAnimation(RoleAnimationType type);

        void AddAnimationCallBack(Action<AnimationData> callback, AnimationCallBackType type);

        void RemoveAnimationCallBack(Action<AnimationData> callback, AnimationCallBackType type);
    }
}
