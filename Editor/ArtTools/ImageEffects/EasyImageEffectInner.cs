#region Namespace

using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.ImageEffects;
/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   15:18
	file base:	EasyImageEffect
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#endregion

[ExecuteInEditMode]
public class EasyImageEffectInner : PostEffectsBase
{
    public enum BlurType
    {
        Standard = 0,
        Sgx = 1,
    }

    public enum Resolution
    {
        Low = 0,
        High = 1,
    }

    [FormerlySerializedAs("bloomOpen")] public bool BloomOpen = true;

    [FormerlySerializedAs("blueChannel")]
    public AnimationCurve BlueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [FormerlySerializedAs("blurType")] public BlurType Blur = BlurType.Standard;

    [Range(1, 2)] [FormerlySerializedAs("blurIterations")]
    public int BlurIterations = 1;

    [Range(0.25f, 5.5f)] [FormerlySerializedAs("blurSize")]
    public float BlurSize = 1.0f;

    [FormerlySerializedAs("colorCorrectOpen")]
    public bool ColorCorrectOpen = true;

    [FormerlySerializedAs("fastBloomShader")]
    public Shader FastBloomShader = null;

    [FormerlySerializedAs("greenChannel")]
    public AnimationCurve GreenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [Range(0.0f, 2.5f)] [FormerlySerializedAs("intensity")]
    public float Intensity = 0.75f;

    private Material m_fastBloomMaterial;

    //private Material ccMaterial;
    private Texture2D m_rgbChannelTex;
    private bool m_updateTexturesOnStartup = true;

    //Color Correction
    [FormerlySerializedAs("redChannel")]
    public AnimationCurve RedChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    private Resolution m_resolution = Resolution.Low;

    [FormerlySerializedAs("saturation")] public float Saturation = 1.0f;

    [Tooltip("Bloom Effect threshold")] [Range(0.0f, 1.5f)] [FormerlySerializedAs("threshold")]
    public float Threshold = 0.25f;
    //public bool updateTextures = true;
    //public Shader simpleColorCorrectionCurvesShader = null;

    [FormerlySerializedAs("vignetIntensity")]
    public float VignetIntensity = 0.375f;

    [FormerlySerializedAs("vignetOpen")] public bool VignetOpen = true;

    private new void Start()
    {
        base.Start();
        m_updateTexturesOnStartup = true;
    }

    protected new bool CheckResources()
    {
        bool isSupport = CheckSupport();

        m_fastBloomMaterial = CheckShaderAndCreateMaterial(FastBloomShader, m_fastBloomMaterial);

        if (!m_rgbChannelTex)
        {
            m_rgbChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
        }

        m_rgbChannelTex.hideFlags = HideFlags.DontSave;
        m_rgbChannelTex.wrapMode = TextureWrapMode.Clamp;

        //if (!isSupported)
        // ReportAutoDisable();
        return isSupport;
    }

    public void UpdateParameters()
    {
        CheckResources(); // textures might not be created if we're tweaking UI while disabled

        if (RedChannel != null && GreenChannel != null && BlueChannel != null)
        {
            for (float i = 0.0f; i <= 1.0f; i += 1.0f / 255.0f)
            {
                float rCh = Mathf.Clamp(RedChannel.Evaluate(i), 0.0f, 1.0f);
                float gCh = Mathf.Clamp(GreenChannel.Evaluate(i), 0.0f, 1.0f);
                float bCh = Mathf.Clamp(BlueChannel.Evaluate(i), 0.0f, 1.0f);

                m_rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255.0f), 0, new Color(rCh, rCh, rCh));
                m_rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255.0f), 1, new Color(gCh, gCh, gCh));
                m_rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255.0f), 2, new Color(bCh, bCh, bCh));

                //float zC = Mathf.Clamp(zCurve.Evaluate(i), 0.0f, 1.0f);

                //zCurveTex.SetPixel((int)Mathf.Floor(i * 255.0f), 0, new Color(zC, zC, zC));

                //rCh = Mathf.Clamp(depthRedChannel.Evaluate(i), 0.0f, 1.0f);
                //gCh = Mathf.Clamp(depthGreenChannel.Evaluate(i), 0.0f, 1.0f);
                //bCh = Mathf.Clamp(depthBlueChannel.Evaluate(i), 0.0f, 1.0f);

                //rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 0, new Color(rCh, rCh, rCh));
                //rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 1, new Color(gCh, gCh, gCh));
                //rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 2, new Color(bCh, bCh, bCh));
            }

            m_rgbChannelTex.Apply();
            //rgbDepthChannelTex.Apply();
            //zCurveTex.Apply();
        }
    }

    private void UpdateTextures()
    {
        UpdateParameters();
    }

    private void OnDisable()
    {
        if (m_fastBloomMaterial)
        {
            DestroyImmediate(m_fastBloomMaterial);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (m_updateTexturesOnStartup)
        {
            UpdateParameters();
            m_updateTexturesOnStartup = false;
        }

        float fVntensity = Intensity;
        float fVignetIntensity = VignetIntensity;
        if (!BloomOpen)
        {
            fVntensity = 0.0f;
        }

        if (!VignetOpen)
        {
            fVignetIntensity = 0.0f;
        }


        int divider = m_resolution == Resolution.Low ? 4 : 2;
        float widthMod = m_resolution == Resolution.Low ? 0.5f : 1.0f;

        m_fastBloomMaterial.SetVector("_Parameter", new Vector4(BlurSize * widthMod, 0.0f, Threshold, fVntensity));
        source.filterMode = FilterMode.Bilinear;

        var rtW = source.width / divider;
        var rtH = source.height / divider;

        // downsample
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt, m_fastBloomMaterial, 1);

        var passOffs = Blur == BlurType.Standard ? 0 : 2;

        for (int i = 0; i < BlurIterations; i++)
        {
            m_fastBloomMaterial.SetVector("_Parameter",
                                          new Vector4(BlurSize * widthMod + i * 1.0f, 0.0f, Threshold, fVntensity));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, m_fastBloomMaterial, 2 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, m_fastBloomMaterial, 3 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }

        m_fastBloomMaterial.SetTexture("_Bloom", rt);

        //color correction
        m_fastBloomMaterial.SetTexture("_RgbTex", m_rgbChannelTex);
        m_fastBloomMaterial.SetFloat("_Saturation", Saturation);
        m_fastBloomMaterial.SetInt("_SaturationOpen", ColorCorrectOpen ? 1 : 0);

        //vignetting
        m_fastBloomMaterial.SetFloat("_VigIntensity", fVignetIntensity);

        //Graphics.Blit(source, destination, fastBloomMaterial, 0);
        Graphics.Blit(source, destination, m_fastBloomMaterial, 6);

        if (BloomOpen)
        {
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}