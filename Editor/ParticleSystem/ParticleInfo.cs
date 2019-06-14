using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInfo
{
    public struct MinMaxCurve
    {
        private ParticleSystemCurveMode m_Mode;
        public ParticleSystemCurveMode mode { get { return m_Mode; } set { m_Mode = value; } }

        private float m_CurveMultiplier;
        public float curveMultiplier { get { return m_CurveMultiplier; } set { m_CurveMultiplier = value; } }
        public float curveScalar { get { return m_CurveMultiplier; } set { m_CurveMultiplier = value; } }

        private AnimationCurve m_CurveMin;
        public AnimationCurve curveMin { get { return m_CurveMin; } set { m_CurveMin = value; } }

        private AnimationCurve m_CurveMax;
        public AnimationCurve curveMax { get { return m_CurveMax; } set { m_CurveMax = value; } }

        private float m_ConstantMin;
        public float constantMin { get { return m_ConstantMin; } set { m_ConstantMin = value; } }

        private float m_ConstantMax;
        public float constantMax { get { return m_ConstantMax; } set { m_ConstantMax = value; } }
        public float constant { get { return m_ConstantMax; } set { m_ConstantMax = value; } }

        public MinMaxCurve(float constant)
        {
            this.m_Mode = ParticleSystemCurveMode.Constant;
            this.m_CurveMultiplier = 0f;
            this.m_CurveMin = null;
            this.m_CurveMax = null;
            this.m_ConstantMin = 0f;
            this.m_ConstantMax = constant;
        }

        public MinMaxCurve(float multiplier, AnimationCurve curve)
        {
            this.m_Mode = ParticleSystemCurveMode.Curve;
            this.m_CurveMultiplier = multiplier;
            this.m_CurveMin = null;
            this.m_CurveMax = curve;
            this.m_ConstantMin = 0f;
            this.m_ConstantMax = 0f;
        }

        public MinMaxCurve(float multiplier, AnimationCurve min, AnimationCurve max)
        {
            this.m_Mode = ParticleSystemCurveMode.TwoCurves;
            this.m_CurveMultiplier = multiplier;
            this.m_CurveMin = min;
            this.m_CurveMax = max;
            this.m_ConstantMin = 0f;
            this.m_ConstantMax = 0f;
        }

        public MinMaxCurve(float min, float max)
        {
            this.m_Mode = ParticleSystemCurveMode.TwoConstants;
            this.m_CurveMultiplier = 0f;
            this.m_CurveMin = null;
            this.m_CurveMax = null;
            this.m_ConstantMin = min;
            this.m_ConstantMax = max;
        }
        public float Evaluate(float time)
        {
            return this.Evaluate(time, 1f);
        }

        public float Evaluate(float time, float lerpFactor)
        {
            float result;
            switch (this.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    result = this.m_ConstantMax;
                    return result;
                case ParticleSystemCurveMode.TwoCurves:
                    result = Mathf.Lerp(this.m_CurveMin.Evaluate(time), this.m_CurveMax.Evaluate(time), lerpFactor) * this.m_CurveMultiplier;
                    return result;
                case ParticleSystemCurveMode.TwoConstants:
                    result = Mathf.Lerp(this.m_ConstantMin, this.m_ConstantMax, lerpFactor);
                    return result;
            }
            result = this.m_CurveMax.Evaluate(time) * this.m_CurveMultiplier;
            return result;
        }

        public static implicit operator MinMaxCurve(float constant)
        {
            return new MinMaxCurve(constant);
        }
    }

    public struct MainModule
    {
        public float duration { get; set; }

        public bool loop { get; set; }

        public bool prewarm { get; set; }

        public MinMaxCurve startDelay { get; set; }
        public float startDelayMultiplier { get; set; }

        public MinMaxCurve startLifetime { get; set; }
        public float startLifetimeMultiplier { get; set; }

        public MinMaxCurve startSpeed { get; set; }
        public float startSpeedMultiplier { get; set; }
    }
}
