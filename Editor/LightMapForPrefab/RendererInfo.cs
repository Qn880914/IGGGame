using UnityEngine;

namespace GOE.Scene
{
    /// <summary>
    /// 渲染信息
    /// </summary>
    [System.Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public int LightmapIndex;
        public Vector4 LightmapOffsetScale;    //偏移和缩放
    }

    /// <summary>
    /// 重渲贴图
    /// </summary>
    [System.Serializable]
    public struct RemapTexture2D
    {
        public int OriginalLightmapIndex;
        public Texture2D OrginalLightmap;

        public Texture2D LightmapFar;
        public Texture2D LightmapNear;
    }
}