using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ParticleInfo
{
    public MainModule main { get; set; }

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

        public MinMaxCurve(ParticleSystem.MinMaxCurve minMaxCurVe)
        {
            this.m_Mode = (ParticleSystemCurveMode)minMaxCurVe.mode;
            this.m_CurveMultiplier = minMaxCurVe.curveMultiplier;
            this.m_CurveMin = minMaxCurVe.curveMin;
            this.m_CurveMax = minMaxCurVe.curveMax;
            this.m_ConstantMin = minMaxCurVe.constantMin;
            this.m_ConstantMax = minMaxCurVe.constantMax;
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

        public ParticleSystem.MinMaxCurve startDelay { get; set; }
        public float startDelayMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startLifetime { get; set; }
        public float startLifetimeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startSpeed { get; set; }
        public float startSpeedMultiplier { get; set; }

        public bool startSize3D { get; set; }
        public ParticleSystem.MinMaxCurve startSize { get; set; }
        public float startSizeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startSizeX { get; set; }
        public float startSizeXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startSizeY { get; set; }
        public float startSizeYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startSizeZ { get; set; }
        public float startSizeZMultiplier { get; set; }

        public bool startRotation3D { get; set; }
        public ParticleSystem.MinMaxCurve startRotation { get; set; }
        public float startRotationMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startRotationX { get; set; }
        public float startRotationXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startRotationY { get; set; }
        public float startRotationYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startRotationZ { get; set; }
        public float startRotationZMultiplier { get; set; }

        public float flipRotation { get; set; }

        public ParticleSystem.MinMaxGradient startColor { get; set; }

        public ParticleSystem.MinMaxCurve gravityModifier { get; set; }
        public float gravityModifierMultiplier { get; set; }

        public ParticleSystemSimulationSpace simulationSpace { get; set; }

        public Transform customSimulationSpace { get; set; }

        public float simulationSpeed { get; set; }

        public bool useUnscaledTime { get; set; }

        public ParticleSystemScalingMode scalingMode { get; set; }

        public bool playOnAwake { get; set; }

        public int maxParticles { get; set; }

        public ParticleSystemEmitterVelocityMode emitterVelocityMode { get; set; }

        public ParticleSystemStopAction stopAction { get; set; }

        public ParticleSystemRingBufferMode ringBufferMode { get; set; }

        public Vector2 ringBufferLoopRange { get; set; }

        public ParticleSystemCullingMode cullingMode { get; set; }

        public MainModule(ParticleSystem.MainModule mode)
        {
            duration = mode.duration;

            loop = mode.loop;
            prewarm = mode.prewarm;

            startDelay = mode.startDelay;
            startDelayMultiplier = mode.startDelayMultiplier;

            startLifetime = mode.startLifetime;
            startLifetimeMultiplier = mode.startLifetimeMultiplier;

            startSpeed = mode.startSpeed;
            startSpeedMultiplier = mode.startSpeedMultiplier;

            startSize3D = mode.startSize3D;
            startSize = mode.startSize;
            startSizeMultiplier = mode.startSizeMultiplier;

            startSizeX = mode.startSizeX;
            startSizeXMultiplier = mode.startSizeXMultiplier;

            startSizeY = mode.startSizeY;
            startSizeYMultiplier = mode.startSizeYMultiplier;

            startSizeZ = mode.startSizeZ;
            startSizeZMultiplier = mode.startSizeZMultiplier;

            startRotation3D = mode.startRotation3D;
            startRotation = mode.startRotation;
            startRotationMultiplier = mode.startRotationMultiplier;

            startRotationX = mode.startRotationX;
            startRotationXMultiplier = mode.startRotationXMultiplier;

            startRotationY = mode.startRotationY;
            startRotationYMultiplier = mode.startRotationYMultiplier;

            startRotationZ = mode.startRotationZ;
            startRotationZMultiplier = mode.startRotationZMultiplier;

            flipRotation = mode.flipRotation;

            startColor = mode.startColor;

            gravityModifier = mode.gravityModifier;
            gravityModifierMultiplier = mode.gravityModifierMultiplier;

            simulationSpace = mode.simulationSpace;

            customSimulationSpace = mode.customSimulationSpace;

            simulationSpeed = mode.simulationSpeed;

            useUnscaledTime = mode.useUnscaledTime;

            scalingMode = mode.scalingMode;

            playOnAwake = mode.playOnAwake;

            maxParticles = mode.maxParticles;

            emitterVelocityMode = mode.emitterVelocityMode;

            stopAction = mode.stopAction;

            ringBufferMode = mode.ringBufferMode;

            ringBufferLoopRange = mode.ringBufferLoopRange;

            cullingMode = mode.cullingMode;
        }
    }

    public void CopyToParticleSystem(ParticleSystem particleStstem)
    {
        System.Type mainType = particleStstem.main.GetType();
        PropertyInfo property = mainType.GetProperty("duration");
        property.SetValue(particleStstem.main, main.duration);
    }
}
