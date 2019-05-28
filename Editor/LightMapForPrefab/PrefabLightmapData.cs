using System.Collections.Generic;
using UnityEngine;

namespace GOE.Scene
{
    /// <summary>
    /// 描述：光照模型预设
    /// </summary>
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class PrefabLightmapData : MonoBehaviour
    {
        [SerializeField]
        public RendererInfo[] mRendererInfos;

        [SerializeField]
        public Texture2D[] mLightmapFars;

        [SerializeField]
        public Texture2D[] mLightmapNears;

        private bool m_isInit = false;

        void Start()
        {
            if (m_isInit)
            {
                return;
            }
            if (mRendererInfos == null || mLightmapFars.Length <= 0)
            {
                return;
            }
            ApplyLightmaps(mRendererInfos, mLightmapFars, mLightmapNears);
            


            //设置原来烘焙时的光照模式，这个不设置正确，默认模式就会只显示光照贴图
            LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;

            Shader.EnableKeyword("LIGHTMAP_ON");
            m_isInit = true;
        }

        private static Texture2D[] CullNullTextures(Texture2D[] maps )
        {

            List<Texture2D> tempList = new List<Texture2D>();

            int length = maps.Length;
            for (int i=0; i< length; i++)
            {
                if (maps[i] != null)
                    tempList.Add(maps[i]);

            }

            return tempList.ToArray();


        }

        /// <summary>
        /// 应用光照模型，根据指定的渲染信息
        /// </summary>
        /// <param name="rendererInfos"></param>
        /// <param name="lightmapFars"></param>
        /// <param name="lightmapNears"></param>
        public static void ApplyLightmaps(RendererInfo[] rendererInfos, Texture2D[] lightmapFars,
                                          Texture2D[] lightmapNears)
        {
            if (rendererInfos == null || rendererInfos.Length <= 0)
            {
                Debug.LogWarning("<<PrefabLightmapData , ApplyLightmaps>>  renderer info is null ! :");
                return;
            }


            lightmapFars = CullNullTextures(lightmapFars);
            lightmapNears = CullNullTextures(lightmapNears);

            if (lightmapFars.Length == 0)
            {
                Debug.Log(rendererInfos[0].renderer.name + ":lightmap no fars ");
                return;


            }

            LightmapData[] settingLightMaps = LightmapSettings.lightmaps;
            int[] lightmapOffsetIndex = new int[lightmapFars.Length];
            List<LightmapData> combinedLightmaps = new List<LightmapData>();

            bool existAlready = false;
            for (int i = 0; i < lightmapFars.Length; i++)
            {
                existAlready = false;
                for (int j = 0; j < settingLightMaps.Length; j++)
                {
                    if (lightmapFars[i] == settingLightMaps[j].lightmapColor)
                    {
                        lightmapOffsetIndex[i] = j;
                        existAlready = true;
                        break;
                    }
                }

                //如果不存在，则创建新的光照数据
                if (!existAlready)
                {
                    lightmapOffsetIndex[i] = settingLightMaps.Length + combinedLightmaps.Count;

                    LightmapData newLightData = new LightmapData();
                    newLightData.lightmapColor = lightmapFars[i];
                    if (lightmapNears != null && lightmapNears.Length > i)
                        newLightData.lightmapDir = lightmapNears[i];
                    combinedLightmaps.Add(newLightData);
                }
            }

            //组合数据
            LightmapData[] finalCombinedLightData = new LightmapData[combinedLightmaps.Count + settingLightMaps.Length];
            settingLightMaps.CopyTo(finalCombinedLightData, 0);
            combinedLightmaps.CopyTo(finalCombinedLightData, settingLightMaps.Length);
            combinedLightmaps.Clear();

            applyRendererInfo(rendererInfos, lightmapOffsetIndex);

            //重新绑定
            LightmapSettings.lightmaps = finalCombinedLightData;

        }


        /// <summary>
        /// 应用渲染信息
        /// </summary>
        /// <param name="rendererInfos"></param>
        /// <param name="offsetIndexs"></param>
        private static void applyRendererInfo(RendererInfo[] rendererInfos, int[] offsetIndexs)
        {
            for (int i = 0; i < rendererInfos.Length; i++)
            {
                RendererInfo info = rendererInfos[i];
                //Debug.Log("origin idx:" + info.LightmapIndex + ",offset index length:" + offsetIndexs.Length);
                info.renderer.lightmapIndex = offsetIndexs[info.LightmapIndex];
                info.renderer.lightmapScaleOffset = info.LightmapOffsetScale;

            }
        }
    }

}