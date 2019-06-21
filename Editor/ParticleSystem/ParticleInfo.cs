using IGG.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticleInfo
{
    public ParticleInfo(ParticleSystem particleSystem)
    {
        m_MainModule = new MainModule(particleSystem.main);
        m_EmissionModule.Init(particleSystem.emission);
        m_ShapeModule.Init(particleSystem.shape);
        m_SubEmittersModule.Init(particleSystem.subEmitters);
        m_TextureSheetAnimationModule.Init(particleSystem.textureSheetAnimation);
        m_VelocityOverLifetimeModule.Init(particleSystem.velocityOverLifetime);
        m_LimitVelocityOverLifetimeModule.Init(particleSystem.limitVelocityOverLifetime);
        m_InHeritVelocityModule.Init(particleSystem.inheritVelocity);
        m_ForceOverLifetimeModule.Init(particleSystem.forceOverLifetime);
        m_ColorOverLifetimeModule.Init(particleSystem.colorOverLifetime);
        m_ColorBySpeedModule.Init(particleSystem.colorBySpeed);
        m_SizeOverLifetimeModule.Init(particleSystem.sizeOverLifetime);
        m_SizeBySpeedModule.Init(particleSystem.sizeBySpeed);
        m_RotationOverLifetimeModule.Init(particleSystem.rotationOverLifetime);
        m_RotationBySpeedModule.Init(particleSystem.rotationBySpeed);
        m_ExternalForcesModule.Init(particleSystem.externalForces);
        m_NoiseModule.Init(particleSystem.noise);
        m_CollisionModule.Init(particleSystem.collision);
        m_TriggerModule.Init(particleSystem.trigger);
        m_LightsModule.Init(particleSystem.lights);
        m_TrailModule.Init(particleSystem.trails);
        m_ParticleSystemRender = new ParticleSystemRender(particleSystem.gameObject.GetComponent<UnityEngine.ParticleSystemRenderer>());
    }

    public void CopyToParticleSystem(ParticleSystem particleSystem)
    {
        /*System.Type mainType = particleStstem.main.GetType();
        PropertyInfo property = mainType.GetProperty("duration");
        property.SetValue(particleStstem.main, main.duration);*/

        ParticleSystem.MainModule mainModule = particleSystem.main;
        m_MainModule.Copy(ref mainModule);

        ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
        m_EmissionModule.Copy(ref emissionModule);

        ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
        m_ShapeModule.Copy(ref shapeModule);

        ParticleSystem.SubEmittersModule subEmittersModule = particleSystem.subEmitters;
        m_SubEmittersModule.Copy(ref subEmittersModule);

        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = particleSystem.textureSheetAnimation;
        m_TextureSheetAnimationModule.Copy(ref textureSheetAnimationModule);

        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = particleSystem.velocityOverLifetime;
        m_VelocityOverLifetimeModule.Copy(ref velocityOverLifetimeModule);

        ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityLifetimeModule = particleSystem.limitVelocityOverLifetime;
        m_LimitVelocityOverLifetimeModule.Copy(ref limitVelocityLifetimeModule);

        ParticleSystem.InheritVelocityModule inheriteVelocityModule = particleSystem.inheritVelocity;
        m_InHeritVelocityModule.Copy(ref inheriteVelocityModule);

        ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = particleSystem.forceOverLifetime;
        m_ForceOverLifetimeModule.Copy(ref forceOverLifetimeModule);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = particleSystem.colorOverLifetime;
        m_ColorOverLifetimeModule.Copy(ref colorOverLifetimeModule);

        ParticleSystem.ColorBySpeedModule colorBySpeedModule = particleSystem.colorBySpeed;
        m_ColorBySpeedModule.Copy(ref colorBySpeedModule);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = particleSystem.sizeOverLifetime;
        m_SizeOverLifetimeModule.Copy(ref sizeOverLifetimeModule);

        ParticleSystem.SizeBySpeedModule sizeBySpeedModule = particleSystem.sizeBySpeed;
        m_SizeBySpeedModule.Copy(ref sizeBySpeedModule);

        ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = particleSystem.rotationOverLifetime;
        m_RotationOverLifetimeModule.Copy(ref rotationOverLifetimeModule);

        ParticleSystem.RotationBySpeedModule rotationBySpeedModule = particleSystem.rotationBySpeed;
        m_RotationBySpeedModule.Copy(ref rotationBySpeedModule);

        ParticleSystem.ExternalForcesModule externalForcesModule = particleSystem.externalForces;
        m_ExternalForcesModule.Copy(ref externalForcesModule);

        ParticleSystem.NoiseModule noiseModule = particleSystem.noise;
        m_NoiseModule.Copy(ref noiseModule);

        ParticleSystem.CollisionModule collisionModule = particleSystem.collision;
        m_CollisionModule.Copy(ref collisionModule);

        ParticleSystem.TriggerModule triggerModule = particleSystem.trigger;
        m_TriggerModule.Copy(ref triggerModule);

        ParticleSystem.LightsModule lightsModule = particleSystem.lights;
        m_LightsModule.Copy(ref lightsModule);

        ParticleSystem.TrailModule trialModule = particleSystem.trails;
        m_TrailModule.Copy(ref trialModule);

        m_ParticleSystemRender.Copy(particleSystem.gameObject.GetComponent<UnityEngine.ParticleSystemRenderer>());
    }

    private MainModule m_MainModule;
    public MainModule main { get { return m_MainModule; } set { m_MainModule = value; } }

    private EmissionModule m_EmissionModule;
    public EmissionModule emission { get { return m_EmissionModule; } set { m_EmissionModule = value; } }

    private ShapeModule m_ShapeModule;
    public ShapeModule shape { get { return m_ShapeModule; } set { m_ShapeModule = value; } }

    private SubEmittersModule m_SubEmittersModule;
    public SubEmittersModule subEmitters { get { return m_SubEmittersModule; } set { m_SubEmittersModule = value; } }

    private TextureSheetAnimationModule m_TextureSheetAnimationModule;
    public TextureSheetAnimationModule textureSheetAnimation { get { return m_TextureSheetAnimationModule; } set { m_TextureSheetAnimationModule = value; } }

    private VelocityOverLifetimeModule m_VelocityOverLifetimeModule;
    public VelocityOverLifetimeModule velocityOverLifetime { get { return m_VelocityOverLifetimeModule; } set { m_VelocityOverLifetimeModule = value; } }

    private LimitVelocityOverLifetimeModule m_LimitVelocityOverLifetimeModule;
    public LimitVelocityOverLifetimeModule limitVelocityOverLifetime { get { return m_LimitVelocityOverLifetimeModule; } set { m_LimitVelocityOverLifetimeModule = value; } }

    private InHeritVelocityModule m_InHeritVelocityModule;
    public InHeritVelocityModule inheritVelocity { get { return m_InHeritVelocityModule; } set { m_InHeritVelocityModule = value; } }

    private ForceOverLifetimeModule m_ForceOverLifetimeModule;
    public ForceOverLifetimeModule forceOverLifetime { get { return m_ForceOverLifetimeModule; } set { m_ForceOverLifetimeModule = value; } }

    private ColorOverLifetimeModule m_ColorOverLifetimeModule;
    public ColorOverLifetimeModule colorOverLifetime { get { return m_ColorOverLifetimeModule; } set { m_ColorOverLifetimeModule = value; } }

    private ColorBySpeedModule m_ColorBySpeedModule;
    public ColorBySpeedModule colorBySpeed { get { return m_ColorBySpeedModule; } set { m_ColorBySpeedModule = value; } }

    private SizeOverLifetimeModule m_SizeOverLifetimeModule;
    public SizeOverLifetimeModule sizeOverLifetime { get { return m_SizeOverLifetimeModule; } set { m_SizeOverLifetimeModule = value; } }

    private SizeBySpeedModule m_SizeBySpeedModule;
    public SizeBySpeedModule sizeBySpeed { get { return m_SizeBySpeedModule; } set { m_SizeBySpeedModule = value; } }

    private RotationOverLifetimeModule m_RotationOverLifetimeModule;
    public RotationOverLifetimeModule rotationOverLifetime { get { return m_RotationOverLifetimeModule; } set { m_RotationOverLifetimeModule = value; } }

    private RotationBySpeedModule m_RotationBySpeedModule;
    public RotationBySpeedModule rotationBySpeed { get { return m_RotationBySpeedModule; } set { m_RotationBySpeedModule = value; } }

    private ExternalForcesModule m_ExternalForcesModule;
    public ExternalForcesModule externalForces { get { return m_ExternalForcesModule; } set { m_ExternalForcesModule = value; } }

    private NoiseModule m_NoiseModule;
    public NoiseModule noise { get { return m_NoiseModule; } set { m_NoiseModule = value; } }

    private CollisionModule m_CollisionModule;
    public CollisionModule collision { get { return m_CollisionModule; } set { m_CollisionModule = value; } }

    private TriggerModule m_TriggerModule;
    public TriggerModule trigger { get { return m_TriggerModule; } set { m_TriggerModule = value; } }

    private LightsModule m_LightsModule;
    public LightsModule lights { get { return m_LightsModule; } set { m_LightsModule = value; } }

    private TrailModule m_TrailModule;
    public TrailModule trail { get { return m_TrailModule; } set { m_TrailModule = value; } }

    private ParticleSystemRender m_ParticleSystemRender;
    public ParticleSystemRender render { get { return m_ParticleSystemRender; } set { m_ParticleSystemRender = value; } }

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

        public void Copy(ref ParticleSystem.MainModule mode)
        {
            mode.duration = duration;

            mode.loop = loop;
            mode.prewarm = prewarm;

            mode.startDelay = startDelay;
            mode.startDelayMultiplier = startDelayMultiplier;

            mode.startLifetime = startLifetime;
            mode.startLifetimeMultiplier = startLifetimeMultiplier;

            mode.startSpeed = startSpeed;
            mode.startSpeedMultiplier = startSpeedMultiplier;

            mode.startSize3D = startSize3D;
            mode.startSize = startSize;
            mode.startSizeMultiplier = startSizeMultiplier;

            mode.startSizeX = startSizeX;
            mode.startSizeXMultiplier = startSizeXMultiplier;

            mode.startSizeY = startSizeY;
            mode.startSizeYMultiplier = startSizeYMultiplier;

            mode.startSizeZ = startSizeZ;
            mode.startSizeZMultiplier = startSizeZMultiplier;

            mode.startRotation3D = startRotation3D;
            mode.startRotation = startRotation;
            mode.startRotationMultiplier = startRotationMultiplier;

            mode.startRotationX = startRotationX;
            mode.startRotationXMultiplier = startRotationXMultiplier;

            mode.startRotationY = startRotationY;
            mode.startRotationYMultiplier = startRotationYMultiplier;

            mode.startRotationZ = startRotationZ;
            mode.startRotationZMultiplier = startRotationZMultiplier;

            mode.flipRotation = flipRotation;

            mode.startColor = startColor;

            mode.gravityModifier = gravityModifier;
            mode.gravityModifierMultiplier = gravityModifierMultiplier;

            mode.simulationSpace = simulationSpace;

            mode.customSimulationSpace = customSimulationSpace;

            mode.simulationSpeed = simulationSpeed;

            mode.useUnscaledTime = useUnscaledTime;

            mode.scalingMode = scalingMode;

            mode.playOnAwake = playOnAwake;

            mode.maxParticles = maxParticles;

            mode.emitterVelocityMode = emitterVelocityMode;

            mode.stopAction = stopAction;

            mode.ringBufferMode = ringBufferMode;

            mode.ringBufferLoopRange = ringBufferLoopRange;

            mode.cullingMode = cullingMode;
        }
    }

    public struct EmissionModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve rateOverTime { get; set; }
        public float rateOverTimeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve rateOverDistance { get; set; }
        public float rateOverDistanceMultiplier { get; set; }

        public ParticleSystem.Burst[] bursts { get; set; }

        public int burstCount { get; set; }

        public void Init(ParticleSystem.EmissionModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            rateOverTime = mode.rateOverTime;
            rateOverTimeMultiplier = mode.rateOverTimeMultiplier;
            rateOverDistance = mode.rateOverDistance;
            rateOverDistanceMultiplier = mode.rateOverDistanceMultiplier;

            burstCount = mode.burstCount;
            bursts = new ParticleSystem.Burst[burstCount];
            for (int i = 0; i < burstCount; ++i)
            {
                bursts[i] = mode.GetBurst(i);
            }
        }

        public void Copy(ref ParticleSystem.EmissionModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
                return;

            mode.rateOverTime = rateOverTime;
            mode.rateOverTimeMultiplier = rateOverTimeMultiplier;
            mode.rateOverDistance = rateOverDistance;
            mode.rateOverDistanceMultiplier = rateOverDistanceMultiplier;

            mode.burstCount = burstCount;
            mode.SetBursts(bursts);
        }
    }

    public struct ShapeModule
    {
        public bool enabled { get; set; }

        public ParticleSystemShapeType shapeType { get; set; }

        public float randomDirectionAmount { get; set; }

        public float sphericalDirectionAmount { get; set; }

        public float randomPositionAmount { get; set; }

        public bool alignToDirection { get; set; }

        public float radius { get; set; }

        public ParticleSystemShapeMultiModeValue radiusMode { get; set; }

        public float radiusSpread { get; set; }

        public ParticleSystem.MinMaxCurve radiusSpeed { get; set; }

        public float radiusSpeedMultiplier { get; set; }

        public float radiusThickness { get; set; }

        public float angle { get; set; }

        public float length { get; set; }

        public Vector3 boxThickness { get; set; }

        public ParticleSystemMeshShapeType meshShapeType { get; set; }

        public Mesh mesh { get; set; }

        public MeshRenderer meshRenderer { get; set; }

        public SkinnedMeshRenderer skinnedMeshRenderer { get; set; }

        public Sprite sprite { get; set; }

        public SpriteRenderer spriteRenderer { get; set; }

        public bool useMeshMaterialIndex { get; set; }

        public int meshMaterialIndex { get; set; }

        public bool useMeshColors { get; set; }

        public float normalOffset { get; set; }

        public ParticleSystemShapeMultiModeValue meshSpawnMode { get; set; }

        public float meshSpawnSpread { get; set; }

        public ParticleSystem.MinMaxCurve meshSpawnSpeed { get; set; }
        public float meshSpawnSpeedMultiplier { get; set; }

        public float arc { get; set; }

        public ParticleSystemShapeMultiModeValue arcMode { get; set; }

        public float arcSpread { get; set; }

        public ParticleSystem.MinMaxCurve arcSpeed { get; set; }
        public float arcSpeedMultiplier { get; set; }

        public float donutRadius { get; set; }

        public Vector3 position { get; set; }

        public Vector3 rotation { get; set; }

        public Vector3 scale { get; set; }

        public Texture2D texture { get; set; }

        public ParticleSystemShapeTextureChannel textureClipChannel { get; set; }

        public float textureClipThreshold { get; set; }

        public bool textureColorAffectsParticles { get; set; }

        public bool textureAlphaAffectsParticles { get; set; }

        public bool textureBilinearFiltering { get; set; }

        public int textureUVChannel { get; set; }

        public void Init(ParticleSystem.ShapeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            shapeType = mode.shapeType;
            randomDirectionAmount = mode.randomDirectionAmount;
            sphericalDirectionAmount = mode.sphericalDirectionAmount;
            randomPositionAmount = mode.randomPositionAmount;
            alignToDirection = mode.alignToDirection;

            switch (shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                case ParticleSystemShapeType.Hemisphere:
                    {
                        radius = mode.radius;
                        radiusThickness = mode.radiusThickness;
                        arc = mode.arc;
                        radiusMode = mode.radiusMode;
                        radiusSpread = mode.radiusSpread;
                        if (radiusMode == ParticleSystemShapeMultiModeValue.Loop ||
                            radiusMode == ParticleSystemShapeMultiModeValue.PingPong)
                        {
                            radiusSpeed = mode.radiusSpeed;
                            radiusSpeedMultiplier = mode.radiusSpeedMultiplier;
                        }
                        texture = mode.texture;
                        textureClipChannel = mode.textureClipChannel;
                        textureClipThreshold = mode.textureClipThreshold;
                        textureColorAffectsParticles = mode.textureAlphaAffectsParticles;
                        textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                        textureBilinearFiltering = mode.textureBilinearFiltering;
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeVolume:
                    {
                        angle = mode.angle;
                        radius = mode.radius;
                        radiusThickness = mode.radiusThickness;
                        arc = mode.arc;
                        radiusMode = mode.radiusMode;
                        radiusSpread = mode.radiusSpread;
                        if (radiusMode == ParticleSystemShapeMultiModeValue.Loop ||
                            radiusMode == ParticleSystemShapeMultiModeValue.PingPong)
                        {
                            radiusSpeed = mode.radiusSpeed;
                            radiusSpeedMultiplier = mode.radiusSpeedMultiplier;
                        }
                        length = mode.length;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Donut:
                    {
                        radius = mode.radius;
                        donutRadius = mode.donutRadius;
                        radiusThickness = mode.radiusThickness;
                        arc = mode.arc;
                        arcMode = mode.arcMode;
                        arcSpread = mode.arcSpread;
                        arcSpeed = mode.arcSpeed;
                        arcSpeedMultiplier = mode.arcSpeedMultiplier;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Box:
                case ParticleSystemShapeType.BoxShell:
                case ParticleSystemShapeType.BoxEdge:
                    {
                        boxThickness = mode.boxThickness;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                    }
                    break;
                case ParticleSystemShapeType.Mesh:
                    {
                        meshShapeType = mode.meshShapeType;
                        meshSpawnMode = mode.meshSpawnMode;
                        meshSpawnSpread = mode.meshSpawnSpread;
                        meshSpawnSpeed = mode.meshSpawnSpeed;
                        meshSpawnSpeedMultiplier = mode.meshSpawnSpeedMultiplier;
                        mesh = mode.mesh;
                        useMeshMaterialIndex = mode.useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            meshMaterialIndex = mode.meshMaterialIndex;
                        }
                        useMeshColors = mode.useMeshColors;
                        normalOffset = mode.normalOffset;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                            textureUVChannel = mode.textureUVChannel;
                        }

                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.MeshRenderer:
                    {
                        meshShapeType = mode.meshShapeType;
                        meshSpawnMode = mode.meshSpawnMode;
                        meshSpawnSpread = mode.meshSpawnSpread;
                        meshSpawnSpeed = mode.meshSpawnSpeed;
                        meshSpawnSpeedMultiplier = mode.meshSpawnSpeedMultiplier;
                        meshRenderer = mode.meshRenderer;
                        useMeshMaterialIndex = mode.useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            meshMaterialIndex = mode.meshMaterialIndex;
                        }
                        useMeshColors = mode.useMeshColors;
                        normalOffset = mode.normalOffset;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                            textureUVChannel = mode.textureUVChannel;
                        }

                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.SkinnedMeshRenderer:
                    {
                        meshShapeType = mode.meshShapeType;
                        meshSpawnMode = mode.meshSpawnMode;
                        meshSpawnSpread = mode.meshSpawnSpread;
                        meshSpawnSpeed = mode.meshSpawnSpeed;
                        meshSpawnSpeedMultiplier = mode.meshSpawnSpeedMultiplier;
                        skinnedMeshRenderer = mode.skinnedMeshRenderer;
                        useMeshMaterialIndex = mode.useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            meshMaterialIndex = mode.meshMaterialIndex;
                        }
                        useMeshColors = mode.useMeshColors;
                        normalOffset = mode.normalOffset;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                            textureUVChannel = mode.textureUVChannel;
                        }

                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Sprite:
                    {
                        meshShapeType = mode.meshShapeType;
                        sprite = mode.sprite;
                        normalOffset = mode.normalOffset;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.SpriteRenderer:
                    {
                        meshShapeType = mode.meshShapeType;
                        spriteRenderer = mode.spriteRenderer;
                        normalOffset = mode.normalOffset;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Circle:
                    {
                        radius = mode.radius;
                        radiusThickness = mode.radiusThickness;
                        arc = mode.arc;
                        arcMode = mode.arcMode;
                        arcSpread = mode.arcSpread;
                        arcSpeed = mode.arcSpeed;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    {
                        radius = mode.radius;
                        radiusMode = mode.radiusMode;
                        radiusSpread = mode.radiusSpread;
                        radiusSpeed = mode.radiusSpeed;
                        radiusSpeedMultiplier = mode.radiusSpeedMultiplier;
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
                case ParticleSystemShapeType.Rectangle:
                    {
                        texture = mode.texture;
                        if (null != texture)
                        {
                            textureClipChannel = mode.textureClipChannel;
                            textureClipThreshold = mode.textureClipThreshold;
                            textureColorAffectsParticles = mode.textureColorAffectsParticles;
                            textureAlphaAffectsParticles = mode.textureAlphaAffectsParticles;
                            textureBilinearFiltering = mode.textureBilinearFiltering;
                        }
                        position = mode.position;
                        rotation = mode.rotation;
                        scale = mode.scale;
                    }
                    break;
            }
        }

        public void Copy(ref ParticleSystem.ShapeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }
            mode.shapeType = shapeType;
            mode.randomDirectionAmount = randomDirectionAmount;
            mode.sphericalDirectionAmount = sphericalDirectionAmount;
            mode.randomPositionAmount = randomPositionAmount;
            mode.alignToDirection = alignToDirection;

            switch (shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                case ParticleSystemShapeType.Hemisphere:
                    {
                        mode.radius = radius;
                        mode.radiusThickness = radiusThickness;
                        mode.arc = arc;
                        mode.radiusMode = radiusMode;
                        mode.radiusSpread = radiusSpread;
                        if (radiusMode == ParticleSystemShapeMultiModeValue.Loop ||
                            radiusMode == ParticleSystemShapeMultiModeValue.PingPong)
                        {
                            mode.radiusSpeed = radiusSpeed;
                            mode.radiusSpeedMultiplier = radiusSpeedMultiplier;
                        }
                        mode.texture = texture;
                        mode.textureClipChannel = textureClipChannel;
                        mode.textureClipThreshold = textureClipThreshold;
                        mode.textureColorAffectsParticles = textureAlphaAffectsParticles;
                        mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                        mode.textureBilinearFiltering = textureBilinearFiltering;
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeVolume:
                    {
                        mode.angle = angle;
                        mode.radius = radius;
                        mode.radiusThickness = radiusThickness;
                        mode.arc = arc;
                        mode.radiusMode = radiusMode;
                        mode.radiusSpread = radiusSpread;
                        if (radiusMode == ParticleSystemShapeMultiModeValue.Loop ||
                            radiusMode == ParticleSystemShapeMultiModeValue.PingPong)
                        {
                            mode.radiusSpeed = radiusSpeed;
                            mode.radiusSpeedMultiplier = radiusSpeedMultiplier;
                        }
                        mode.length = length;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Donut:
                    {
                        mode.radius = radius;
                        mode.donutRadius = donutRadius;
                        mode.radiusThickness = radiusThickness;
                        mode.arc = arc;
                        mode.arcMode = arcMode;
                        mode.arcSpread = arcSpread;
                        mode.arcSpeed = arcSpeed;
                        mode.arcSpeedMultiplier = arcSpeedMultiplier;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Box:
                case ParticleSystemShapeType.BoxShell:
                case ParticleSystemShapeType.BoxEdge:
                    {
                        mode.boxThickness = boxThickness;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                    }
                    break;
                case ParticleSystemShapeType.Mesh:
                    {
                        mode.meshShapeType = meshShapeType;
                        mode.meshSpawnMode = meshSpawnMode;
                        mode.meshSpawnSpread = meshSpawnSpread;
                        mode.meshSpawnSpeed = meshSpawnSpeed;
                        mode.meshSpawnSpeedMultiplier = meshSpawnSpeedMultiplier;
                        mode.mesh = mesh;
                        mode.useMeshMaterialIndex = useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            mode.meshMaterialIndex = meshMaterialIndex;
                        }
                        mode.useMeshColors = useMeshColors;
                        mode.normalOffset = normalOffset;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                            mode.textureUVChannel = textureUVChannel;
                        }

                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.MeshRenderer:
                    {
                        mode.meshShapeType = meshShapeType;
                        mode.meshSpawnMode = meshSpawnMode;
                        mode.meshSpawnSpread = meshSpawnSpread;
                        mode.meshSpawnSpeed = meshSpawnSpeed;
                        mode.meshSpawnSpeedMultiplier = meshSpawnSpeedMultiplier;
                        mode.meshRenderer = meshRenderer;
                        mode.useMeshMaterialIndex = useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            mode.meshMaterialIndex = meshMaterialIndex;
                        }
                        mode.useMeshColors = useMeshColors;
                        mode.normalOffset = normalOffset;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                            mode.textureUVChannel = textureUVChannel;
                        }

                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.SkinnedMeshRenderer:
                    {
                        mode.meshShapeType = meshShapeType;
                        mode.meshSpawnMode = meshSpawnMode;
                        mode.meshSpawnSpread = meshSpawnSpread;
                        mode.meshSpawnSpeed = meshSpawnSpeed;
                        mode.meshSpawnSpeedMultiplier = meshSpawnSpeedMultiplier;
                        mode.skinnedMeshRenderer = skinnedMeshRenderer;
                        mode.useMeshMaterialIndex = useMeshMaterialIndex;
                        if (useMeshMaterialIndex)
                        {
                            mode.meshMaterialIndex = meshMaterialIndex;
                        }
                        mode.useMeshColors = useMeshColors;
                        mode.normalOffset = normalOffset;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                            mode.textureUVChannel = textureUVChannel;
                        }

                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Sprite:
                    {
                        mode.meshShapeType = meshShapeType;
                        mode.sprite = sprite;
                        mode.normalOffset = normalOffset;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.SpriteRenderer:
                    {
                        mode.meshShapeType = meshShapeType;
                        mode.spriteRenderer = spriteRenderer;
                        mode.normalOffset = normalOffset;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Circle:
                    {
                        mode.radius = radius;
                        mode.radiusThickness = radiusThickness;
                        mode.arc = arc;
                        mode.arcMode = arcMode;
                        mode.arcSpread = arcSpread;
                        mode.arcSpeed = arcSpeed;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    {
                        mode.radius = radius;
                        mode.radiusMode = radiusMode;
                        mode.radiusSpread = radiusSpread;
                        mode.radiusSpeed = radiusSpeed;
                        mode.radiusSpeedMultiplier = radiusSpeedMultiplier;
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
                case ParticleSystemShapeType.Rectangle:
                    {
                        mode.texture = texture;
                        if (null != texture)
                        {
                            mode.textureClipChannel = textureClipChannel;
                            mode.textureClipThreshold = textureClipThreshold;
                            mode.textureColorAffectsParticles = textureColorAffectsParticles;
                            mode.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
                            mode.textureBilinearFiltering = textureBilinearFiltering;
                        }
                        mode.position = position;
                        mode.rotation = rotation;
                        mode.scale = scale;
                    }
                    break;
            }
        }
    }

    public struct SubEmittersModule
    {
        public bool enabled { get; set; }

        public int subEmittersCount { get; set; }

        public SubEmittersModuleParticleSystem[] subParticleSystem { get; set; }

        public struct SubEmittersModuleParticleSystem
        {
            public ParticleInfo particleInfo { get; set; }

            public ParticleSystemSubEmitterType type { get; set; }

            public ParticleSystemSubEmitterProperties properties { get; set; }

            public float emitProbability { get; set; }
        }

        public void Init(ParticleSystem.SubEmittersModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            subEmittersCount = mode.subEmittersCount;
            subParticleSystem = new SubEmittersModuleParticleSystem[subEmittersCount];
            for (int i = 0; i < subEmittersCount; ++i)
            {
                subParticleSystem[i].particleInfo = new ParticleInfo(mode.GetSubEmitterSystem(i));
                subParticleSystem[i].type = mode.GetSubEmitterType(i);
                subParticleSystem[i].properties = mode.GetSubEmitterProperties(i);
                subParticleSystem[i].emitProbability = mode.GetSubEmitterEmitProbability(i);
            }
        }

        public void Copy(ref ParticleSystem.SubEmittersModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            for (int i = 0; i < subEmittersCount; ++i)
            {
                ParticleSystem particleSystem = ParticleSystemPool.Get();
                subParticleSystem[i].particleInfo.CopyToParticleSystem(particleSystem);
                mode.AddSubEmitter(particleSystem, subParticleSystem[i].type,
                    subParticleSystem[i].properties, subParticleSystem[i].emitProbability);
            }
        }
    }

    public struct TextureSheetAnimationModule
    {
        public bool enabled { get; set; }

        public ParticleSystemAnimationMode mode { get; set; }

        public ParticleSystemAnimationTimeMode timeMode { get; set; }

        public float fps { get; set; }

        public int numTilesX { get; set; }

        public int numTilesY { get; set; }

        public ParticleSystemAnimationType animation { get; set; }

        public ParticleSystemAnimationRowMode rowMode { get; set; }

        public ParticleSystem.MinMaxCurve frameOverTime { get; set; }
        public float frameOverTimeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve startFrame { get; set; }
        public float startFrameMultiplier { get; set; }

        public int cycleCount { get; set; }

        public int rowIndex { get; set; }

        public UVChannelFlags uvChannelMask { get; set; }

        public int spriteCount { get; set; }

        public Sprite[] sprites { get; set; }

        public Vector2 speedRange { get; set; }

        public void Init(ParticleSystem.TextureSheetAnimationModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }
            this.mode = mode.mode;
            if (this.mode == ParticleSystemAnimationMode.Grid)
            {
                numTilesX = mode.numTilesX;
                numTilesY = mode.numTilesY;
                animation = mode.animation;
                startFrame = mode.startFrame;
                startFrameMultiplier = mode.startFrameMultiplier;
                uvChannelMask = mode.uvChannelMask;

                if (animation == ParticleSystemAnimationType.SingleRow)
                {
                    rowMode = mode.rowMode;
                    rowIndex = mode.rowIndex;
                }
            }
            else
            {
                spriteCount = mode.spriteCount;
                sprites = new Sprite[spriteCount];
                for (int i = 0; i < spriteCount; ++i)
                {
                    sprites[i] = mode.GetSprite(i);
                }
            }

            timeMode = mode.timeMode;
            switch (timeMode)
            {
                case ParticleSystemAnimationTimeMode.Lifetime:
                    {
                        frameOverTime = mode.frameOverTime;
                        frameOverTimeMultiplier = mode.frameOverTimeMultiplier;
                        cycleCount = mode.cycleCount;
                    }
                    break;
                case ParticleSystemAnimationTimeMode.Speed:
                    {
                        speedRange = mode.speedRange;
                        cycleCount = mode.cycleCount;
                    }
                    break;
                case ParticleSystemAnimationTimeMode.FPS:
                    {
                        fps = mode.fps;
                    }
                    break;
            }
        }

        public void Copy(ref ParticleSystem.TextureSheetAnimationModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }
            mode.mode = this.mode;
            if (this.mode == ParticleSystemAnimationMode.Grid)
            {
                mode.numTilesX = numTilesX;
                mode.numTilesY = numTilesY;
                mode.animation = animation;
                mode.startFrame = startFrame;
                mode.startFrameMultiplier = startFrameMultiplier;
                mode.uvChannelMask = uvChannelMask;

                if (animation == ParticleSystemAnimationType.SingleRow)
                {
                    mode.rowMode = rowMode;
                    mode.rowIndex = rowIndex;
                }
            }
            else
            {
                for (int i = 0; i < spriteCount; ++i)
                {
                    mode.AddSprite(sprites[i]);
                }
            }

            mode.timeMode = timeMode;
            switch (timeMode)
            {
                case ParticleSystemAnimationTimeMode.Lifetime:
                    {
                        mode.frameOverTime = frameOverTime;
                        mode.frameOverTimeMultiplier = frameOverTimeMultiplier;
                        mode.cycleCount = cycleCount;
                    }
                    break;
                case ParticleSystemAnimationTimeMode.Speed:
                    {
                        mode.speedRange = speedRange;
                        mode.cycleCount = cycleCount;
                    }
                    break;
                case ParticleSystemAnimationTimeMode.FPS:
                    {
                        mode.fps = fps;
                    }
                    break;
            }
        }
    }

    public struct VelocityOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }
        public float xMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }
        public float yMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }
        public float zMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalX { get; set; }
        public float orbitalXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalY { get; set; }
        public float orbitalYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalZ { get; set; }
        public float orbitalZMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalOffsetX { get; set; }
        public float orbitalOffsetXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalOffsetY { get; set; }
        public float orbitalOffsetYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalOffsetZ { get; set; }
        public float orbitalOffsetZMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve radial { get; set; }
        public float radialMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve speedModifier { get; set; }
        public float speedModifierMultiplier { get; set; }

        public ParticleSystemSimulationSpace space { get; set; }

        public void Init(ParticleSystem.VelocityOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            x = mode.x;
            xMultiplier = mode.xMultiplier;
            y = mode.y;
            yMultiplier = mode.yMultiplier;
            z = mode.z;
            zMultiplier = mode.zMultiplier;
            space = mode.space;
            orbitalX = mode.orbitalX;
            orbitalXMultiplier = mode.orbitalXMultiplier;
            orbitalY = mode.orbitalY;
            orbitalYMultiplier = mode.orbitalYMultiplier;
            orbitalZ = mode.orbitalZ;
            orbitalZMultiplier = mode.orbitalZMultiplier;
            orbitalOffsetX = mode.orbitalOffsetX;
            orbitalOffsetXMultiplier = mode.orbitalOffsetXMultiplier;
            orbitalOffsetY = mode.orbitalOffsetY;
            orbitalOffsetYMultiplier = mode.orbitalOffsetYMultiplier;
            orbitalOffsetZ = mode.orbitalOffsetZ;
            orbitalOffsetZMultiplier = mode.orbitalOffsetZMultiplier;
            radial = mode.radial;
            radialMultiplier = mode.radialMultiplier;
            speedModifier = mode.speedModifier;
            speedModifierMultiplier = mode.speedModifierMultiplier;
        }

        public void Copy(ref ParticleSystem.VelocityOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.x = x;
            mode.xMultiplier = xMultiplier;
            mode.y = y;
            mode.yMultiplier = yMultiplier;
            mode.z = z;
            mode.zMultiplier = zMultiplier;
            mode.space = space;
            mode.orbitalX = orbitalX;
            mode.orbitalXMultiplier = orbitalXMultiplier;
            mode.orbitalY = orbitalY;
            mode.orbitalYMultiplier = orbitalYMultiplier;
            mode.orbitalZ = orbitalZ;
            mode.orbitalZMultiplier = orbitalZMultiplier;
            mode.orbitalOffsetX = orbitalOffsetX;
            mode.orbitalOffsetXMultiplier = orbitalOffsetXMultiplier;
            mode.orbitalOffsetY = orbitalOffsetY;
            mode.orbitalOffsetYMultiplier = orbitalOffsetYMultiplier;
            mode.orbitalOffsetZ = orbitalOffsetZ;
            mode.orbitalOffsetZMultiplier = orbitalOffsetZMultiplier;
            mode.radial = radial;
            mode.radialMultiplier = radialMultiplier;
            mode.speedModifier = speedModifier;
            mode.speedModifierMultiplier = speedModifierMultiplier;
        }
    }

    public struct LimitVelocityOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve limitX { get; set; }
        public float limitXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve limitY { get; set; }
        public float limitYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve limitZ { get; set; }
        public float limitZMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve limit { get; set; }
        public float limitMultiplier { get; set; }

        public float dampen { get; set; }

        public bool separateAxes { get; set; }

        public ParticleSystemSimulationSpace space { get; set; }

        public ParticleSystem.MinMaxCurve drag { get; set; }
        public float dragMultiplier { get; set; }

        public bool multiplyDragByParticleSize { get; set; }

        public bool multiplyDragByParticleVelocity { get; set; }

        public void Init(ParticleSystem.LimitVelocityOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                limitX = mode.limitX;
                limitXMultiplier = mode.limitXMultiplier;
                limitY = mode.limitY;
                limitYMultiplier = mode.limitYMultiplier;
                limitZ = mode.limitZ;
                limitZMultiplier = mode.limitZMultiplier;
            }
            limit = mode.limit;
            limitMultiplier = mode.limitMultiplier;
            dampen = mode.dampen;
            drag = mode.drag;
            dragMultiplier = mode.dragMultiplier;
            multiplyDragByParticleSize = mode.multiplyDragByParticleSize;
            multiplyDragByParticleVelocity = mode.multiplyDragByParticleVelocity;
        }

        public void Copy(ref ParticleSystem.LimitVelocityOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.limitX = limitX;
                mode.limitXMultiplier = limitXMultiplier;
                mode.limitY = limitY;
                mode.limitYMultiplier = limitYMultiplier;
                mode.limitZ = limitZ;
                mode.limitZMultiplier = limitZMultiplier;
            }
            mode.limit = limit;
            mode.limitMultiplier = limitMultiplier;
            mode.dampen = dampen;
            mode.drag = drag;
            mode.dragMultiplier = dragMultiplier;
            mode.multiplyDragByParticleSize = multiplyDragByParticleSize;
            mode.multiplyDragByParticleVelocity = multiplyDragByParticleVelocity;
        }
    }

    public struct InHeritVelocityModule
    {
        public bool enabled { get; set; }

        public ParticleSystemInheritVelocityMode mode { get; set; }

        public ParticleSystem.MinMaxCurve curve { get; set; }

        public float curveMultiplier { get; set; }

        public void Init(ParticleSystem.InheritVelocityModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            this.mode = mode.mode;
            curve = mode.curve;
            curveMultiplier = mode.curveMultiplier;
        }

        public void Copy(ref ParticleSystem.InheritVelocityModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.mode = this.mode;
            mode.curve = curve;
            mode.curveMultiplier = curveMultiplier;
        }
    }

    public struct ForceOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }

        public float xMultiplier { get; set; }

        public float yMultiplier { get; set; }

        public float zMultiplier { get; set; }

        public ParticleSystemSimulationSpace space { get; set; }

        public bool randomized { get; set; }

        public void Init(ParticleSystem.ForceOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            x = mode.x;
            xMultiplier = mode.xMultiplier;
            y = mode.y;
            yMultiplier = mode.yMultiplier;
            z = mode.z;
            zMultiplier = mode.zMultiplier;
            space = mode.space;
        }

        public void Copy(ref ParticleSystem.ForceOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.x = x;
            mode.xMultiplier = xMultiplier;
            mode.y = y;
            mode.yMultiplier = yMultiplier;
            mode.z = z;
            mode.zMultiplier = zMultiplier;
            mode.space = space;
        }
    }

    public struct ColorOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxGradient color { get; set; }

        public void Init(ParticleSystem.ColorOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            color = mode.color;
        }

        public void Copy(ref ParticleSystem.ColorOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.color = color;
        }
    }

    public struct ColorBySpeedModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxGradient color { get; set; }

        public Vector2 range { get; set; }

        public void Init(ParticleSystem.ColorBySpeedModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            color = mode.color;
            range = mode.range;
        }

        public void Copy(ref ParticleSystem.ColorBySpeedModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.color = color;
            mode.range = range;
        }
    }

    public struct SizeOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve size { get; set; }

        public float sizeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }

        public float xMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }

        public float yMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }

        public float zMultiplier { get; set; }

        public bool separateAxes { get; set; }

        public void Init(ParticleSystem.SizeOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                x = mode.x;
                xMultiplier = mode.xMultiplier;
                y = mode.y;
                yMultiplier = mode.yMultiplier;
                z = mode.z;
                zMultiplier = mode.zMultiplier;
            }
            else
            {
                size = mode.size;
                sizeMultiplier = mode.sizeMultiplier;
            }
        }

        public void Copy(ref ParticleSystem.SizeOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.x = x;
                mode.xMultiplier = xMultiplier;
                mode.y = y;
                mode.yMultiplier = yMultiplier;
                mode.z = z;
                mode.zMultiplier = zMultiplier;
            }
            else
            {
                mode.size = size;
                mode.sizeMultiplier = sizeMultiplier;
            }
        }
    }

    public struct SizeBySpeedModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve size { get; set; }

        public float sizeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }

        public float xMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }

        public float yMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }

        public float zMultiplier { get; set; }

        public bool separateAxes { get; set; }

        public Vector2 range { get; set; }

        public void Init(ParticleSystem.SizeBySpeedModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            range = mode.range;
            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                x = mode.x;
                xMultiplier = mode.xMultiplier;
                y = mode.y;
                yMultiplier = mode.yMultiplier;
                z = mode.z;
                zMultiplier = mode.zMultiplier;
            }
            else
            {
                size = mode.size;
                sizeMultiplier = mode.sizeMultiplier;
            }
        }

        public void Copy(ref ParticleSystem.SizeBySpeedModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.range = range;
            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.x = x;
                mode.xMultiplier = xMultiplier;
                mode.y = y;
                mode.yMultiplier = yMultiplier;
                mode.z = z;
                mode.zMultiplier = zMultiplier;
            }
            else
            {
                mode.size = size;
                mode.sizeMultiplier = sizeMultiplier;
            }
        }
    }

    public struct RotationOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }

        public float xMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }

        public float yMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }

        public float zMultiplier { get; set; }

        public bool separateAxes { get; set; }

        public void Init(ParticleSystem.RotationOverLifetimeModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                x = mode.x;
                xMultiplier = mode.xMultiplier;
                y = mode.y;
                yMultiplier = mode.yMultiplier;
                z = mode.z;
                zMultiplier = mode.zMultiplier;
            }
            else
            {
                // TODO 
                // Angular Velocity
            }
        }

        public void Copy(ref ParticleSystem.RotationOverLifetimeModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.x = x;
                mode.xMultiplier = xMultiplier;
                mode.y = y;
                mode.yMultiplier = yMultiplier;
                mode.z = z;
                mode.zMultiplier = zMultiplier;
            }
            else
            {
                // TODO 
                // Angular Velocity
            }
        }
    }

    public struct RotationBySpeedModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxCurve x { get; set; }

        public float xMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve y { get; set; }

        public float yMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve z { get; set; }

        public float zMultiplier { get; set; }

        public bool separateAxes { get; set; }

        public Vector2 range { get; set; }

        public void Init(ParticleSystem.RotationBySpeedModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            range = mode.range;
            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                x = mode.x;
                xMultiplier = mode.xMultiplier;
                y = mode.y;
                yMultiplier = mode.yMultiplier;
                z = mode.z;
                zMultiplier = mode.zMultiplier;
            }
            else
            {
                // TODO
                // Angular Velocity
            }
        }

        public void Copy(ref ParticleSystem.RotationBySpeedModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.range = range;
            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.x = x;
                mode.xMultiplier = xMultiplier;
                mode.y = y;
                mode.yMultiplier = yMultiplier;
                mode.z = z;
                mode.zMultiplier = zMultiplier;
            }
            else
            {
                // TODO
                // Angular Velocity
            }
        }
    }

    public struct ExternalForcesModule
    {
        public bool enabled { get; set; }

        public float multiplier { get; set; }

        public ParticleSystem.MinMaxCurve multiplierCurve { get; set; }

        public ParticleSystemGameObjectFilter influenceFilter { get; set; }

        public LayerMask influenceMask { get; set; }

        public int influenceCount { get; set; }

        public ParticleSystemForceField[] forceFields { get; set; }

        public void Init(ParticleSystem.ExternalForcesModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            multiplier = mode.multiplier;
            multiplierCurve = mode.multiplierCurve;
            influenceFilter = mode.influenceFilter;
            influenceMask = mode.influenceMask;
            if (influenceFilter == ParticleSystemGameObjectFilter.LayerMaskAndList ||
                influenceFilter == ParticleSystemGameObjectFilter.List)
            {
                influenceCount = mode.influenceCount;
                forceFields = new ParticleSystemForceField[influenceCount];
                for (int i = 0; i < influenceCount; ++i)
                {
                    forceFields[i] = mode.GetInfluence(i);
                }
            }
        }

        public void Copy(ref ParticleSystem.ExternalForcesModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.multiplier = multiplier;
            mode.multiplierCurve = multiplierCurve;
            mode.influenceFilter = influenceFilter;
            mode.influenceMask = influenceMask;
            if (influenceFilter == ParticleSystemGameObjectFilter.LayerMaskAndList ||
                influenceFilter == ParticleSystemGameObjectFilter.List)
            {
                for (int i = 0; i < influenceCount; ++i)
                {
                    mode.AddInfluence(forceFields[i]);
                }
            }
        }
    }

    public struct NoiseModule
    {
        public bool enabled { get; set; }

        public bool separateAxes { get; set; }

        public ParticleSystem.MinMaxCurve strength { get; set; }

        public float strengthMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve strengthX { get; set; }

        public float strengthXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve strengthY { get; set; }

        public float strengthYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve strengthZ { get; set; }

        public float strengthZMultiplier { get; set; }

        public float frequency { get; set; }

        public bool damping { get; set; }

        public int octaveCount { get; set; }

        public float octaveMultiplier { get; set; }

        public float octaveScale { get; set; }

        public ParticleSystemNoiseQuality quality { get; set; }

        public ParticleSystem.MinMaxCurve scrollSpeed { get; set; }

        public float scrollSpeedMultiplier { get; set; }

        public bool remapEnabled { get; set; }

        public ParticleSystem.MinMaxCurve remap { get; set; }

        public float remapMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve remapX { get; set; }

        public float remapXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve remapY { get; set; }

        public float remapYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve remapZ { get; set; }

        public float remapZMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve positionAmount { get; set; }

        public ParticleSystem.MinMaxCurve rotationAmount { get; set; }

        public ParticleSystem.MinMaxCurve sizeAmount { get; set; }

        public void Init(ParticleSystem.NoiseModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            separateAxes = mode.separateAxes;
            if (separateAxes)
            {
                strengthX = mode.strengthX;
                strengthXMultiplier = mode.strengthXMultiplier;
                strengthY = mode.strengthY;
                strengthYMultiplier = mode.strengthYMultiplier;
                strengthZ = mode.strengthZMultiplier;
            }
            else
            {
                strength = mode.strength;
                strengthMultiplier = mode.strengthMultiplier;
            }
            remapEnabled = mode.remapEnabled;
            if (remapEnabled)
            {
                remapX = mode.remapX;
                remapXMultiplier = mode.remapXMultiplier;
                remapY = mode.remapY;
                remapYMultiplier = mode.remapYMultiplier;
                remapZ = mode.remapZ;
                remapZMultiplier = mode.remapZMultiplier;
            }
            else
            {
                remap = mode.remap;
                remapMultiplier = mode.remapMultiplier;
            }

            frequency = mode.frequency;
            scrollSpeed = mode.scrollSpeed;
            scrollSpeedMultiplier = mode.scrollSpeedMultiplier;
            damping = mode.damping;
            octaveCount = mode.octaveCount;
            octaveMultiplier = mode.octaveMultiplier;
            octaveScale = mode.octaveScale;
            quality = mode.quality;
            positionAmount = mode.positionAmount;
            rotationAmount = mode.rotationAmount;
            sizeAmount = mode.sizeAmount;
        }

        public void Copy(ref ParticleSystem.NoiseModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.separateAxes = separateAxes;
            if (separateAxes)
            {
                mode.strengthX = strengthX;
                mode.strengthXMultiplier = strengthXMultiplier;
                mode.strengthY = strengthY;
                mode.strengthYMultiplier = strengthYMultiplier;
                mode.strengthZ = strengthZMultiplier;
            }
            else
            {
                mode.strength = strength;
                mode.strengthMultiplier = strengthMultiplier;
            }
            mode.remapEnabled = remapEnabled;
            if (remapEnabled)
            {
                mode.remapX = remapX;
                mode.remapXMultiplier = remapXMultiplier;
                mode.remapY = remapY;
                mode.remapYMultiplier = remapYMultiplier;
                mode.remapZ = remapZ;
                mode.remapZMultiplier = remapZMultiplier;
            }
            else
            {
                mode.remap = remap;
                mode.remapMultiplier = remapMultiplier;
            }

            mode.frequency = frequency;
            mode.scrollSpeed = scrollSpeed;
            mode.scrollSpeedMultiplier = scrollSpeedMultiplier;
            mode.damping = damping;
            mode.octaveCount = octaveCount;
            mode.octaveMultiplier = octaveMultiplier;
            mode.octaveScale = octaveScale;
            mode.quality = quality;
            mode.positionAmount = positionAmount;
            mode.rotationAmount = rotationAmount;
            mode.sizeAmount = sizeAmount;
        }
    }

    public struct CollisionModule
    {
        public bool enabled { get; set; }

        public ParticleSystemCollisionType type { get; set; }

        public ParticleSystemCollisionMode mode { get; set; }

        public ParticleSystem.MinMaxCurve dampen { get; set; }

        public float dampenMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve bounce { get; set; }

        public float bounceMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve lifetimeLoss { get; set; }

        public float lifetimeLossMultiplier { get; set; }

        public float minKillSpeed { get; set; }

        public float maxKillSpeed { get; set; }

        public LayerMask collidesWith { get; set; }

        public bool enableDynamicColliders { get; set; }

        public int maxCollisionShapes { get; set; }

        public ParticleSystemCollisionQuality quality { get; set; }

        public float voxelSize { get; set; }

        public float radiusScale { get; set; }

        public bool sendCollisionMessages { get; set; }

        public float colliderForce { get; set; }

        public bool multiplyColliderForceByCollisionAngle { get; set; }

        public bool multiplyColliderForceByParticleSpeed { get; set; }

        public bool multiplyColliderForceByParticleSize { get; set; }

        public int maxPlaneCount { get; set; }

        public Transform[] transformPlanes { get; set; }

        public void Init(ParticleSystem.CollisionModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            type = mode.type;
            if (type == ParticleSystemCollisionType.Planes)
            {
                maxPlaneCount = mode.maxPlaneCount;
                transformPlanes = new Transform[maxPlaneCount];
                for (int i = 0; i < maxPlaneCount; ++i)
                {
                    transformPlanes[i] = mode.GetPlane(i);
                }
                // TODO
                // Visualization
                // scale Plane
            }
            else
            {
                this.mode = mode.mode;
                quality = mode.quality;
                collidesWith = mode.collidesWith;
                maxCollisionShapes = mode.maxCollisionShapes;
                enableDynamicColliders = mode.enableDynamicColliders;
                colliderForce = mode.colliderForce;
                multiplyColliderForceByCollisionAngle = mode.multiplyColliderForceByCollisionAngle;
                multiplyColliderForceByParticleSize = mode.multiplyColliderForceByParticleSize;
                multiplyColliderForceByParticleSpeed = mode.multiplyColliderForceByParticleSpeed;

            }
            dampen = mode.dampen;
            dampenMultiplier = mode.dampenMultiplier;
            bounce = mode.bounce;
            bounceMultiplier = mode.bounceMultiplier;
            lifetimeLoss = mode.lifetimeLoss;
            lifetimeLossMultiplier = mode.lifetimeLossMultiplier;
            minKillSpeed = mode.minKillSpeed;
            maxKillSpeed = mode.maxKillSpeed;
            radiusScale = mode.radiusScale;
            sendCollisionMessages = mode.sendCollisionMessages;
            // TODO
            // Visualize Bounds
        }

        public void Copy(ref ParticleSystem.CollisionModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.type = type;
            if (type == ParticleSystemCollisionType.Planes)
            {
                for (int i = 0; i < maxPlaneCount; ++i)
                {
                    mode.SetPlane(i, transformPlanes[i]);
                }
                // TODO
                // Visualization
                // scale Plane
            }
            else
            {
                mode.mode = this.mode;
                mode.quality = quality;
                mode.collidesWith = collidesWith;
                mode.maxCollisionShapes = maxCollisionShapes;
                mode.enableDynamicColliders = enableDynamicColliders;
                mode.colliderForce = colliderForce;
                mode.multiplyColliderForceByCollisionAngle = multiplyColliderForceByCollisionAngle;
                mode.multiplyColliderForceByParticleSize = multiplyColliderForceByParticleSize;
                mode.multiplyColliderForceByParticleSpeed = multiplyColliderForceByParticleSpeed;

            }
            mode.dampen = dampen;
            mode.dampenMultiplier = dampenMultiplier;
            mode.bounce = bounce;
            mode.bounceMultiplier = bounceMultiplier;
            mode.lifetimeLoss = lifetimeLoss;
            mode.lifetimeLossMultiplier = lifetimeLossMultiplier;
            mode.minKillSpeed = minKillSpeed;
            mode.maxKillSpeed = maxKillSpeed;
            mode.radiusScale = radiusScale;
            mode.sendCollisionMessages = sendCollisionMessages;
            // TODO
            // Visualize Bounds
        }
    }

    public struct TriggerModule
    {
        public bool enabled { get; set; }

        public ParticleSystemOverlapAction inside { get; set; }

        public ParticleSystemOverlapAction outside { get; set; }

        public ParticleSystemOverlapAction enter { get; set; }

        public ParticleSystemOverlapAction exit { get; set; }

        public float radiusScale { get; set; }

        public int maxColliderCount { get; set; }

        public Component[] colliders { get; set; }

        public void Init(ParticleSystem.TriggerModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            maxColliderCount = mode.maxColliderCount;
            colliders = new Component[maxColliderCount];
            for (int i = 0; i < maxColliderCount; ++i)
            {
                colliders[i] = mode.GetCollider(i);
            }

            inside = mode.inside;
            outside = mode.outside;
            enter = mode.enter;
            exit = mode.exit;
            radiusScale = mode.radiusScale;
            // TODO 
            // Visualize Bounds
        }

        public void Copy(ref ParticleSystem.TriggerModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            for (int i = 0; i < maxColliderCount; ++i)
            {
                mode.SetCollider(i, colliders[i]);
            }

            mode.inside = inside;
            mode.outside = outside;
            mode.enter = enter;
            mode.exit = exit;
            mode.radiusScale = radiusScale;
            // TODO 
            // Visualize Bounds}
        }
    }

    public struct LightsModule
    {
        public bool enabled { get; set; }

        public float ratio { get; set; }

        public bool useRandomDistribution { get; set; }

        public Light light { get; set; }

        public bool useParticleColor { get; set; }

        public bool sizeAffectsRange { get; set; }

        public bool alphaAffectsIntensity { get; set; }

        public ParticleSystem.MinMaxCurve range { get; set; }

        public float rangeMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve intensity { get; set; }

        public float intensityMultiplier { get; set; }

        public int maxLights { get; set; }

        public void Init(ParticleSystem.LightsModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            light = mode.light;
            ratio = mode.ratio;
            useRandomDistribution = mode.useRandomDistribution;
            useParticleColor = mode.useParticleColor;
            sizeAffectsRange = mode.sizeAffectsRange;
            alphaAffectsIntensity = mode.alphaAffectsIntensity;
            range = mode.range;
            rangeMultiplier = mode.rangeMultiplier;
            intensity = mode.intensity;
            intensityMultiplier = mode.intensityMultiplier;
            maxLights = mode.maxLights;
        }

        public void Copy(ref ParticleSystem.LightsModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.light = light;
            mode.ratio = ratio;
            mode.useRandomDistribution = useRandomDistribution;
            mode.useParticleColor = useParticleColor;
            mode.sizeAffectsRange = sizeAffectsRange;
            mode.alphaAffectsIntensity = alphaAffectsIntensity;
            mode.range = range;
            mode.rangeMultiplier = rangeMultiplier;
            mode.intensity = intensity;
            mode.intensityMultiplier = intensityMultiplier;
            mode.maxLights = maxLights;
        }
    }

    public struct TrailModule
    {
        public bool enabled { get; set; }

        public ParticleSystemTrailMode mode { get; set; }

        public float ratio { get; set; }

        public ParticleSystem.MinMaxCurve lifetime { get; set; }

        public float lifetimeMultiplier { get; set; }

        public float minVertexDistance { get; set; }

        public ParticleSystemTrailTextureMode textureMode { get; set; }

        public bool worldSpace { get; set; }

        public bool dieWithParticles { get; set; }

        public bool sizeAffectsWidth { get; set; }

        public bool sizeAffectsLifetime { get; set; }

        public bool inheritParticleColor { get; set; }

        public ParticleSystem.MinMaxGradient colorOverLifetime { get; set; }

        public ParticleSystem.MinMaxCurve widthOverTrail { get; set; }

        public float widthOverTrailMultiplier { get; set; }

        public ParticleSystem.MinMaxGradient colorOverTrail { get; set; }

        public bool generateLightingData { get; set; }

        public int ribbonCount { get; set; }

        public float shadowBias { get; set; }

        public bool splitSubEmitterRibbons { get; set; }

        public bool attachRibbonsToTransform { get; set; }

        public void Init(ParticleSystem.TrailModule mode)
        {
            enabled = mode.enabled;
            if (!enabled)
            {
                return;
            }

            this.mode = mode.mode;
            if (this.mode == ParticleSystemTrailMode.PerParticle)
            {
                ratio = mode.ratio;
                lifetime = mode.lifetime;
                lifetimeMultiplier = mode.lifetimeMultiplier;
                minVertexDistance = mode.minVertexDistance;
                worldSpace = mode.worldSpace;
                dieWithParticles = mode.dieWithParticles;
                textureMode = mode.textureMode;
                sizeAffectsLifetime = mode.sizeAffectsLifetime;
            }
            else
            {
                ribbonCount = mode.ribbonCount;
                splitSubEmitterRibbons = mode.splitSubEmitterRibbons;
                attachRibbonsToTransform = mode.attachRibbonsToTransform;
                textureMode = mode.textureMode;
            }
            sizeAffectsWidth = mode.sizeAffectsWidth;
            inheritParticleColor = mode.inheritParticleColor;
            colorOverLifetime = mode.colorOverLifetime;
            widthOverTrail = mode.widthOverTrail;
            widthOverTrailMultiplier = mode.widthOverTrailMultiplier;
            colorOverTrail = mode.colorOverTrail;
            generateLightingData = mode.generateLightingData;
            shadowBias = mode.shadowBias;
        }

        public void Copy(ref ParticleSystem.TrailModule mode)
        {
            mode.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            mode.mode = this.mode;
            if (this.mode == ParticleSystemTrailMode.PerParticle)
            {
                mode.ratio = ratio;
                mode.lifetime = lifetime;
                mode.lifetimeMultiplier = lifetimeMultiplier;
                mode.minVertexDistance = minVertexDistance;
                mode.worldSpace = worldSpace;
                mode.dieWithParticles = dieWithParticles;
                mode.textureMode = textureMode;
                mode.sizeAffectsLifetime = sizeAffectsLifetime;
            }
            else
            {
                mode.ribbonCount = ribbonCount;
                mode.splitSubEmitterRibbons = splitSubEmitterRibbons;
                mode.attachRibbonsToTransform = attachRibbonsToTransform;
                mode.textureMode = textureMode;
            }
            mode.sizeAffectsWidth = sizeAffectsWidth;
            mode.inheritParticleColor = inheritParticleColor;
            mode.colorOverLifetime = colorOverLifetime;
            mode.widthOverTrail = widthOverTrail;
            mode.widthOverTrailMultiplier = widthOverTrailMultiplier;
            mode.colorOverTrail = colorOverTrail;
            mode.generateLightingData = generateLightingData;
            mode.shadowBias = shadowBias;
        }
    }

    public struct CustomDataModule
    { }

    public class ParticleSystemRender// : Renderer
    {
        public ParticleSystemRenderMode renderMode { get; set; }

        public float cameraVelocityScale { get; set; }

        public float velocityScale { get; set; }

        public float lengthScale { get; set; }

        public Mesh mesh { get; set; }

        public float normalDirection { get; set; }

        public Material material { get; set; }

        public Material trailMaterial { get; set; }

        public ParticleSystemSortMode sortMode { get; set; }

        public float sortingFudge { get; set; }

        public float minParticleSize { get; set; }

        public float maxParticleSize { get; set; }

        public ParticleSystemRenderSpace alignment { get; set; }

        public Vector3 flip { get; set; }

        public bool allowRoll { get; set; }

        public Vector3 pivot { get; set; }

        // TODO  
        // Visualize Pivot

        public SpriteMaskInteraction maskInteraction { get; set; }

        // TODO 
        // Apply Active Color Space

        public int activeVertexStreamsCount { get; set; }

        public ShadowCastingMode shadowCastingMode { get; set; }

        public bool receiveShadows { get; set; }

        public List<ParticleSystemVertexStream> streams { get; set; }

        public float shadowBias { get; set; }

        //public ShadowCastingMode castShows { get; set; }

        //public bool receiveShows { get; set; }

        public MotionVectorGenerationMode motionVectorGenerationMode { get; set; }

        public int sortingLayerID { get; set; }

        public int sortingOrder { get; set; }

        public LightProbeUsage lightProbeUsage;

        public GameObject lightProbeProxyVolumeOverride { get; set; }

        public ReflectionProbeUsage reflectionProbeUsage { get; set; }

        public Transform probeAnchor { get; set; }

        public ParticleSystemRender(UnityEngine.ParticleSystemRenderer render)
        {
            /*enabled = render.enabled;
            if(!enabled)
            {
                return;
            }*/

            renderMode = render.renderMode;
            switch (renderMode)
            {
                case ParticleSystemRenderMode.Billboard:
                    break;
                case ParticleSystemRenderMode.Stretch:
                    {
                        cameraVelocityScale = render.cameraVelocityScale;
                        velocityScale = render.velocityScale;
                        lengthScale = render.lengthScale;
                    }
                    break;
                case ParticleSystemRenderMode.Mesh:
                    {
                        mesh = render.mesh;
                    }
                    break;
            }
            normalDirection = render.normalDirection;
            material = render.material;
            trailMaterial = render.trailMaterial;
            sortMode = render.sortMode;
            sortingFudge = render.sortingFudge;
            minParticleSize = render.minParticleSize;
            maxParticleSize = render.maxParticleSize;
            alignment = render.alignment;
            flip = render.flip;
            allowRoll = render.allowRoll;
            pivot = render.pivot;
            maskInteraction = render.maskInteraction;

            activeVertexStreamsCount = render.activeVertexStreamsCount;
            streams = new List<ParticleSystemVertexStream>();
            render.GetActiveVertexStreams(streams);
            shadowCastingMode = render.shadowCastingMode;
            receiveShadows = render.receiveShadows;
            shadowBias = render.shadowBias;
            motionVectorGenerationMode = render.motionVectorGenerationMode;
            sortingLayerID = render.sortingLayerID;
            sortingOrder = render.sortingOrder;
            lightProbeUsage = render.lightProbeUsage;
            lightProbeProxyVolumeOverride = render.lightProbeProxyVolumeOverride;
            reflectionProbeUsage = render.reflectionProbeUsage;
            probeAnchor = render.probeAnchor;
        }

        public void Copy(UnityEngine.ParticleSystemRenderer render)
        {
            /*render.enabled = enabled;
            if (!enabled)
            {
                return;
            }*/

            render.renderMode = renderMode;
            switch (renderMode)
            {
                case ParticleSystemRenderMode.Billboard:
                    break;
                case ParticleSystemRenderMode.Stretch:
                    {
                        render.cameraVelocityScale = cameraVelocityScale;
                        render.velocityScale = velocityScale;
                        render.lengthScale = lengthScale;
                    }
                    break;
                case ParticleSystemRenderMode.Mesh:
                    {
                        render.mesh = mesh;
                    }
                    break;
            }
            render.normalDirection = normalDirection;
            render.material = material;
            render.trailMaterial = trailMaterial;
            render.sortMode = sortMode;
            render.sortingFudge = sortingFudge;
            render.minParticleSize = minParticleSize;
            render.maxParticleSize = maxParticleSize;
            render.alignment = alignment;
            render.flip = flip;
            render.allowRoll = allowRoll;
            render.pivot = pivot;
            render.maskInteraction = maskInteraction;

            render.SetActiveVertexStreams(streams);

            render.shadowCastingMode = shadowCastingMode;
            render.receiveShadows = receiveShadows;
            render.shadowBias = shadowBias;
            render.motionVectorGenerationMode = motionVectorGenerationMode;
            render.sortingLayerID = sortingLayerID;
            render.sortingOrder = sortingOrder;
            render.lightProbeUsage = lightProbeUsage;
            render.lightProbeProxyVolumeOverride = lightProbeProxyVolumeOverride;
            render.reflectionProbeUsage = reflectionProbeUsage;
            render.probeAnchor = probeAnchor;
        }
    }
}
