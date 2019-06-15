using System.Reflection;
using UnityEngine;

public class Test_ParticleSystem : MonoBehaviour
{
    private Object[] m_ObjEffect;

    [SerializeField]
    private ParticleSystem m_ParticleSystem;

    [SerializeField]
    private ParticleSystem m_ParticlesSystemCopy;

    private ParticleInfo.MainModule m_MainModule;

    // Start is called before the first frame update
    void Start()
    {
        m_MainModule = new ParticleInfo.MainModule(m_ParticlesSystemCopy.main);
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKey(KeyCode.G))
        {
            m_ObjEffect = Resources.LoadAll("Effect/");
        }
        else if(Input.GetKey(KeyCode.D))
        {
            / *for(int i = 0; i < m_ObjEffect.Length; ++ i)
            {
                GameObject.Destroy(m_ObjEffect[i]);
            }* /

            m_ObjEffect = null;
        }
        else if(Input.GetKey(KeyCode.C))
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }*/

        if(Input.GetKey(KeyCode.C))
        {
            for(int i = 0; i < 1; ++ i)
            {
                ParticleSystem.MainModule module = m_ParticleSystem.main;

                module.duration = m_MainModule.duration;

                module.loop = m_MainModule.loop;
                module.prewarm = m_MainModule.prewarm;

                module.startDelay = m_MainModule.startDelay;
                module.startDelayMultiplier = m_MainModule.startDelayMultiplier;

                module.startLifetime = m_MainModule.startLifetime;
                module.startLifetimeMultiplier = m_MainModule.startLifetimeMultiplier;

                module.startSpeed = m_MainModule.startSpeed;
                module.startSpeedMultiplier = m_MainModule.startSpeedMultiplier;

                module.startSize3D = m_MainModule.startSize3D;
                module.startSize = m_MainModule.startSize;
                module.startSizeMultiplier = m_MainModule.startSizeMultiplier;

                module.startSizeX = m_MainModule.startSizeX;
                module.startSizeXMultiplier = m_MainModule.startSizeXMultiplier;

                module.startSizeY = m_MainModule.startSizeY;
                module.startSizeYMultiplier = m_MainModule.startSizeYMultiplier;

                module.startSizeZ = m_MainModule.startSizeZ;
                module.startSizeZMultiplier = m_MainModule.startSizeZMultiplier;

                module.startRotation3D = m_MainModule.startRotation3D;
                module.startRotation = m_MainModule.startRotation;
                module.startRotationMultiplier = m_MainModule.startRotationMultiplier;

                module.startRotationX = m_MainModule.startRotationX;
                module.startRotationXMultiplier = m_MainModule.startRotationXMultiplier;

                module.startRotationY = m_MainModule.startRotationY;
                module.startRotationYMultiplier = m_MainModule.startRotationYMultiplier;

                module.startRotationZ = m_MainModule.startRotationZ;
                module.startRotationZMultiplier = m_MainModule.startRotationZMultiplier;

                module.flipRotation = m_MainModule.flipRotation;

                module.startColor = m_MainModule.startColor;

                module.gravityModifier = m_MainModule.gravityModifier;
                module.gravityModifierMultiplier = m_MainModule.gravityModifierMultiplier;

                module.simulationSpace = m_MainModule.simulationSpace;

                module.customSimulationSpace = m_MainModule.customSimulationSpace;

                module.simulationSpeed = m_MainModule.simulationSpeed;

                module.useUnscaledTime = m_MainModule.useUnscaledTime;

                module.scalingMode = m_MainModule.scalingMode;

                module.playOnAwake = m_MainModule.playOnAwake;

                module.maxParticles = m_MainModule.maxParticles;

                module.emitterVelocityMode = m_MainModule.emitterVelocityMode;

                module.stopAction = m_MainModule.stopAction;

                module.ringBufferMode = m_MainModule.ringBufferMode;

                module.ringBufferLoopRange = m_MainModule.ringBufferLoopRange;

                module.cullingMode = m_MainModule.cullingMode;
                /*System.Type type = m_ParticleSystem.GetType();
                PropertyInfo proterty = type.GetProperty("main");
                proterty.SetValue(m_ParticleSystem, m_MainModule);*/


                /*System.Type type = m_ParticleSystem.main.GetType();
                PropertyInfo property = type.GetProperty("duration");
                property.SetValue(m_ParticleSystem.main, m_MainModule.duration);
                property = type.GetProperty("loop");
                property.SetValue(m_ParticleSystem.main, m_MainModule.loop);
                property = type.GetProperty("prewarm");
                property.SetValue(m_ParticleSystem.main, m_MainModule.prewarm);
                property = type.GetProperty("startDelay");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startDelay);
                property = type.GetProperty("startDelayMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startDelayMultiplier);
                property = type.GetProperty("startLifetime");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startLifetime);
                property = type.GetProperty("startLifetimeMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startLifetimeMultiplier);
                property = type.GetProperty("startSpeed");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSpeed);
                property = type.GetProperty("startSpeedMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSpeedMultiplier);
                property = type.GetProperty("startSize3D");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSize3D);
                property = type.GetProperty("startSize");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSize);
                property = type.GetProperty("startSizeMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeMultiplier);
                property = type.GetProperty("startSizeX");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeX);
                property = type.GetProperty("startSizeXMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeXMultiplier);
                property = type.GetProperty("startSizeY");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeY);
                property = type.GetProperty("startSizeYMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeYMultiplier);
                property = type.GetProperty("startSizeZ");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeZ);
                property = type.GetProperty("startSizeZMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startSizeZMultiplier);
                property = type.GetProperty("startRotation3D");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotation3D);
                property = type.GetProperty("startRotation");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotation);
                property = type.GetProperty("startRotationMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationMultiplier);
                property = type.GetProperty("startRotationX");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationX);
                property = type.GetProperty("startRotationXMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationXMultiplier);
                property = type.GetProperty("startRotationY");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationY);
                property = type.GetProperty("startRotationYMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationYMultiplier);
                property = type.GetProperty("startRotationZ");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationZ);
                property = type.GetProperty("startRotationZMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startRotationZMultiplier);
                property = type.GetProperty("flipRotation");
                property.SetValue(m_ParticleSystem.main, m_MainModule.flipRotation);
                property = type.GetProperty("startColor");
                property.SetValue(m_ParticleSystem.main, m_MainModule.startColor);
                property = type.GetProperty("gravityModifier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.gravityModifier);
                property = type.GetProperty("gravityModifierMultiplier");
                property.SetValue(m_ParticleSystem.main, m_MainModule.gravityModifierMultiplier);
                property = type.GetProperty("simulationSpace");
                property.SetValue(m_ParticleSystem.main, m_MainModule.simulationSpace);
                property = type.GetProperty("customSimulationSpace");
                property.SetValue(m_ParticleSystem.main, m_MainModule.customSimulationSpace);
                property = type.GetProperty("simulationSpeed");
                property.SetValue(m_ParticleSystem.main, m_MainModule.simulationSpeed);
                property = type.GetProperty("useUnscaledTime");
                property.SetValue(m_ParticleSystem.main, m_MainModule.useUnscaledTime);
                property = type.GetProperty("scalingMode");
                property.SetValue(m_ParticleSystem.main, m_MainModule.scalingMode);
                property = type.GetProperty("playOnAwake");
                property.SetValue(m_ParticleSystem.main, m_MainModule.playOnAwake);
                property = type.GetProperty("maxParticles");
                property.SetValue(m_ParticleSystem.main, m_MainModule.maxParticles);
                property = type.GetProperty("emitterVelocityMode");
                property.SetValue(m_ParticleSystem.main, m_MainModule.emitterVelocityMode);
                property = type.GetProperty("stopAction");
                property.SetValue(m_ParticleSystem.main, m_MainModule.stopAction);
                property = type.GetProperty("ringBufferMode");
                property.SetValue(m_ParticleSystem.main, m_MainModule.ringBufferMode);
                property = type.GetProperty("ringBufferLoopRange");
                property.SetValue(m_ParticleSystem.main, m_MainModule.ringBufferLoopRange);
                property = type.GetProperty("cullingMode");
                property.SetValue(m_ParticleSystem.main, m_MainModule.cullingMode);*/
            }
        }
    }
}
