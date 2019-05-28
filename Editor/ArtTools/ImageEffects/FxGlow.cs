#region Namespace

using UnityEngine;
using UnityEngine.Serialization;

#endregion

[ExecuteInEditMode]
public class FxGlow : MonoBehaviour
{
    #region Properties

    private Material TempMaterial
    {
        get
        {
            if (m_curMaterial == null)
            {
                m_curMaterial = new Material(GlowShader);
                m_curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return m_curMaterial;
        }
    }

    #endregion

    public void OnDisable()
    {
        if (m_curMaterial)
            DestroyImmediate(m_curMaterial);
    }

    private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        TempMaterial.SetVector("_Parameter", new Vector4(GlowSize, GlowIntensity, GlowSaturation, 0));

        int rtW = 1024 >> DownSample;
        int rtH = 1024 >> DownSample;

        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, sourceTexture.format);
        RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, sourceTexture.format);
        RenderTexture rt3 = RenderTexture.GetTemporary(rtW >> 2, rtH >> 2, 0, sourceTexture.format);
        RenderTexture rt4 = RenderTexture.GetTemporary(rtW >> 2, rtH >> 2, 0, sourceTexture.format);

        Graphics.Blit(sourceTexture, rt, m_curMaterial, 0);
        m_curMaterial.SetVector("_Parameter", new Vector4(GlowSize, GlowIntensity, GlowSaturation, 0));

        Graphics.Blit(rt, rt2, m_curMaterial, 2);
        Graphics.Blit(rt2, rt, m_curMaterial, 1);
        Graphics.Blit(rt, rt3, m_curMaterial, 2);
        Graphics.Blit(rt3, rt4, m_curMaterial, 1);

        m_curMaterial.SetTexture("_GlowTex1", rt);
        m_curMaterial.SetTexture("_GlowTex2", rt4);
        Graphics.Blit(sourceTexture, destTexture, m_curMaterial, 3);

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(rt2);
        RenderTexture.ReleaseTemporary(rt3);
        RenderTexture.ReleaseTemporary(rt4);
    }

    #region Variables

    [FormerlySerializedAs("glowShader")] public Shader GlowShader;


    [Range(0.0f, 8.0f)] public float GlowIntensity = 1.4f;

    [Range(0.0f, 8.0f)] public float GlowSaturation = 6.0f;

    [Range(0, 4)] public int DownSample = 1;

    [Range(0.0f, 8.0f)] public float GlowSize = 2.0f;

    [SerializeField] private Material m_curMaterial;

    #endregion
}