using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticleInfo
{
    public MainModule main { get; set; }

    public EmissionModule emission { get; set; }

    public SubEmittersModule subEmitters { get; set; }

    public TextureSheetAnimationModule textureSheetAnimation { get; set; }

    public VelocityOverLifetimeModule velocityOverLifetime { get; set; }

    public LimitVelocityLifetimeModule limitVelocityLifetime { get; set; }

    public InHeritVelocityModule inHeritVelocity { get; set; }

    public ForceOverLifetimeModule forceOverLifetimeModule { get; set; }

    public ColorOverLifetimeModule colorOverLifetime { get; set; }

    public ColorBySpeedModule colorBySpeed { get; set; }

    public SizeOverLifetimeModule sizeOverLifetimeModule { get; set; }

    public SizeBySpeedModule sizeBySpeed { get; set; }

    public RotationOverLifetimeModule rotationOverLifetime { get; set; }

    public RotationBySpeedModule rotationBySpeed { get; set; }

    public ExternalForcesModule externalForces { get; set; }

    public NoiseModule noise { get; set; }

    public CollisionModule collision { get; set; }

    public TriggerModule trigger { get; set; }

    public LightsModule lights { get; set; }

    public TrailModule trail { get; set; }

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

        public ParticleSystem.Burst [] burst { get; set; }

        public int burstCount { get; set; }
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
    }

    public struct SubEmittersModule
    {
        public bool enabled { get; set; }

        public int subEmittersCount { get; set; }

        public SubEmittersModuleParticleSystem[] subParticleSystem { get; set; }

        public struct SubEmittersModuleParticleSystem
        {
            public ParticleSystem particleSystem { get; set; }

            public ParticleSystemSubEmitterType type { get; set; }

            public ParticleSystemSubEmitterProperties properties { get; set; }

            public float emitProbability { get; set; }
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

        public Vector2 speedRange { get; set; }
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
        
        public ParticleSystem.MinMaxCurve orbitalx { get; set; }
        public float orbitalXMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitaly { get; set; }
        public float orbitalYMultiplier { get; set; }

        public ParticleSystem.MinMaxCurve orbitalz { get; set; }
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
    }

    public struct LimitVelocityLifetimeModule
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
    }

    public struct InHeritVelocityModule
    {
        public bool enabled { get; set; }

        public ParticleSystemInheritVelocityMode mode { get; set; }

        public ParticleSystem.MinMaxCurve curve { get; set; }

        public float curveMultiplier { get; set; }
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
    }

    public struct ColorOverLifetimeModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxGradient color { get; set; }
    }

    public struct ColorBySpeedModule
    {
        public bool enabled { get; set; }

        public ParticleSystem.MinMaxGradient color { get; set; }

        public Vector2 range { get; set; }
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
    }

    public struct ExternalForcesModule
    {
        public bool enabled { get; set; }

        public float multiplier { get; set; }

        public ParticleSystem.MinMaxCurve multiplierCurve { get; set; }

        public ParticleSystemGameObjectFilter influenceFilter { get; set; }

        public LayerMask influenceMask { get; set; }

        public int influenceCount { get; set; }
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
    }

    public struct CustomDataModule
    { }

    public void CopyToParticleSystem(ParticleSystem particleStstem)
    {
        System.Type mainType = particleStstem.main.GetType();
        PropertyInfo property = mainType.GetProperty("duration");
        property.SetValue(particleStstem.main, main.duration);
    }
}
