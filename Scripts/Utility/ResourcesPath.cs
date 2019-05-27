#region Namespace

using System.Text;

#endregion

// <summary>
// 资源路径管理
// </summary>
// <author>zhulin</author>

// 集中路径模式
public enum ResourcesPathMode
{
    Editor,
    AssetBundle,
}

public struct ResourcesType
{
    public const string UiWnd = "UIWnd";
    public const string UiWndItem = "UIWndItem";
    public const string UiAtlas = "UIAtlas";
    public const string UiPublicWndAtlas = "UIPublicWndAtlas";
    public const string SceneItem = "SceneItem";

    // hero
    public const string ActorHero = "ActorHero";
    public const string ActorShowHero = "ActorShowHero";
    public const string ActorShowHeroLow = "ActorShowHeroLow";

    public const string HeroAnim = "HeroAnim";

    // soldier
    public const string ActorSoldierPrefab = "ActorSoldier";
    public const string ActorSoldierMaterial = "ActorSoldierMaterial";
    public const string ActorSoldierGpuSkinMat = "ActorSoldierGpuSkinMaterial";
    //public const string ActorSoldierMesh = "ActorSoldierMesh";
    //public const string ActorSoldierAnim = "ActorSoldierAnim";
    public const string ActorSoldierAnimation = "ActorSoldierAnimation";    //旧的MeshAnimation的数据格式
    public const string ActorSoldierGpuSkinMesh = "ActorSoldierGpuSkinMesh";
    public const string ActorSoldierGpuSkinAnim = "ActorSoldierGpuSkinAnim";

    public const string ActorNpc = "ActorNPC";
    public const string Building = "Building";
    public const string BuildingModel = "BuildingModel";
    public const string BuildingBody = "BuildingBody";
    public const string BuildingSite = "BuildingSite";
    public const string CityTree = "CityTree";
    public const string BuildingTexture = "BuildingTexture";
    public const string PngTexture = "PngTexture";

    public const string Effect = "Effect";
    public const string Skill = "Skill";
    public const string Audio = "Audio";
    public const string Shader = "Shader";
    public const string Map = "Map";
    public const string Scene = "Scene";
    public const string MapData = "MapData";
    public const string Language = "Language";
    public const string luaData = "luaData";
    public const string ComMaterial = "CommonMaterial";

    public const string Depend = "Depend";

    public const string Config = "Config";

    public const string Other = "Other";
}

public class ResourcesPath
{
    /// <summary>
    /// 兵种材质路径
    /// </summary>
    public static readonly string TroopMaterialPath = "Data/Units/Troop/Material/";
    public static readonly string GpuSkinTroopMaterialPath = "Data/Units/Troop/GpuSkinningMat/";
    /// <summary>
    /// 兵种动画文件存放路径
    /// </summary>
    public static readonly string TroopMeshAniPath = "Data/Units/Troop/Animation/";
    public static readonly string GpuSkinTroopAniPath = "Data/Units/Troop/GpuSkinningData/";

    private static readonly string[] g_scenePath = {"Scene/", "scene/", ".unity"};
    private static readonly string[] g_luaDataPath = {"Scripts/lua/", "lua/", ".lua"};

    private static readonly string[] g_uiWndPath = {"Data/wnd/panel/", "wnd/panel/", ".prefab"};
    private static readonly string[] g_uiItemsPath = {"Data/wnd/items/", "wnd/items/", ".prefab"};
    private static readonly string[] g_uiPublicAtlasPath = {"Data/Atlas/WndPublic/", "atlas/wndpublic/", ".png"};
    private static readonly string[] g_uiAtlasPath = {"Data/Atlas/Items/", "atlas/item/", ".png"};

    private static readonly string[] g_uiFairyPath = {"Data/ui/", "ui/", ".bytes"};
    private static readonly string[] g_audioPath = {"Data/Audio/", "Audio/", ".ogg"};
    private static readonly string[] g_mapDataPath = {"Data/MapData/", "mapdata/", ".bytes"};
    private static readonly string[] g_languagePath = {"Data/Language/", "language/", ".xml"};

    // 配置文件
    private static readonly string[] g_configPath = {"Data/config/", "config/", ".asset"};
    private static readonly string[] g_shaderPath = {"Shader/Core/", "shader/", ".shader"};

    private static readonly string[] g_effectPath = {"Data/Prefabs/Effect/", "effect/", ".prefab"};
    private static readonly string[] g_buildingPath = {"Data/Prefabs/Building/", "building/", ".prefab"};
    private static readonly string[] g_buildingBody = {"Data/Prefabs/BuildingBody/", "buildingbody/", ".prefab"};
    private static readonly string[] g_buildingModel = {"Data/Prefabs/BuildingModel/", "buildingmodel/", ".prefab"};
    private static readonly string[] g_buildingSite = {"Data/Prefabs/BuildingSite/", "buildingsite/", ".prefab"};

    private static readonly string[] g_buildingTexture =
        {"Models/Environment/Models/Environment/COL3Building/Textures/", "BuildingTexture/", ".tga"};

    private static readonly string[] g_cityTree = {"Data/Prefabs/TreeBlock/", "tree/", ".prefab"};
    private static readonly string[] g_pngTexture = {"Textures/CommonPng/", "CommonTextures/Png/", ".png"};

    private static readonly string[] g_skillPath = {"Data/Prefabs/Skill/", "skill/", ".prefab"};
    private static readonly string[] g_mapPath = {"Data/Prefabs/Map/", "map/", ".prefab"};
    private static readonly string[] g_sceneItemPath = {"Data/Prefabs/sceneItem/", "SceneItem/", ".prefab"};

    private static readonly string[] g_otherPath = {"Data/Prefabs/Other/", "Other/", ".prefab"};

    // 小兵
    private static readonly string[] g_actorSoldierMaterialPath = {TroopMaterialPath, "soldier/material/", ".mat"};
    private static readonly string[] g_actorSoldierGpuSkinMatPath = { GpuSkinTroopMaterialPath, "soldier/gpuskinmaterial/", ".mat" };
    private static readonly string[] g_actorSoldierMeshPath = { GpuSkinTroopAniPath, "soldier/gpuskin/", ".asset"};
    private static readonly string[] g_actorSoldierAnimPath = { GpuSkinTroopAniPath, "soldier/gpuskin/", ".asset"};
    private static readonly string[] g_actorSoldierAnimationPath = {TroopMeshAniPath, "soldier/animation/", ".asset"};

    private static readonly string[] g_actorSoldierPath = {"Data/Prefabs/Actor/Soldier/", "soldier/prefab/", ".prefab"};

    private static readonly string[] g_actorNpcPath = {"Data/Prefabs/Actor/NPC/", "npc/prefab/", ".prefab"};

    // 英雄
    private static readonly string[] g_actorHeroPath = {"Data/Prefabs/Actor/Hero/", "hero/prefab/", ".prefab"};

    private static readonly string[] g_actorShowHeroPath =
        {"Data/Prefabs/Actor/HeroShow/", "showhero/prefab/", ".prefab"};

    private static readonly string[] g_actorShowHeroLowPath = { "Data/Prefabs/Actor/HeroShowLow/", "showhero/lowprefab/", ".prefab" };

    private static readonly string[] g_heroAnimPath = {"Data/Units/Hero/", "hero/anim/", ".anim"};

    // 通用材质
    private static readonly string[] g_commonMaterialPath = {"Data/Materials/Common/", "common/material/", ".mat"};

    //LightMapPath
    private static readonly string[] g_dependPath = {"depend", "depend/", ".prefab"};

    // 获取资源运行时路径
    public static string GetAssetResourceRunPath(string Type, ResourcesPathMode Mode)
    {
        StringBuilder Builder = new StringBuilder(string.Empty);
        Builder.Append(Mode == ResourcesPathMode.AssetBundle ? GetABRunDir() : "Assets/");
        string str = GetRelativePath(Type, Mode);
        Builder.Append(str);
        return Builder.ToString();
    }

    private static string GetABRunDir()
    {
        return ConstantData.AssetBundleSavePath;
    }

    private static string[] GetResTypePath(string resType)
    {
        switch (resType)
        {
            case ResourcesType.UiWnd:
                return g_uiWndPath;
            case ResourcesType.UiWndItem:
                return g_uiItemsPath;
            case ResourcesType.UiAtlas:
                return g_uiAtlasPath;
            case ResourcesType.ActorHero:
                return g_actorHeroPath;
            case ResourcesType.HeroAnim:
                return g_heroAnimPath;
            case ResourcesType.ActorShowHero:
                return g_actorShowHeroPath;
            case ResourcesType.ActorShowHeroLow:
                return g_actorShowHeroLowPath;
            case ResourcesType.ActorNpc:
                return g_actorNpcPath;
            case ResourcesType.ActorSoldierPrefab:
                return g_actorSoldierPath;
            case ResourcesType.ActorSoldierGpuSkinMat:
                return g_actorSoldierGpuSkinMatPath;
            case ResourcesType.ActorSoldierMaterial:
                return g_actorSoldierMaterialPath;
            case ResourcesType.ActorSoldierGpuSkinMesh:
                return g_actorSoldierMeshPath;
            case ResourcesType.ActorSoldierGpuSkinAnim:
                return g_actorSoldierAnimPath;
            case ResourcesType.ActorSoldierAnimation:
                return g_actorSoldierAnimationPath;
            case ResourcesType.Building:
                return g_buildingPath;
            case ResourcesType.BuildingBody:
                return g_buildingBody;
            case ResourcesType.BuildingModel:
                return g_buildingModel;
            case ResourcesType.BuildingSite:
                return g_buildingSite;
            case ResourcesType.CityTree:
                return g_cityTree;
            case ResourcesType.BuildingTexture:
                return g_buildingTexture;
            case ResourcesType.PngTexture:
                return g_pngTexture;
            case ResourcesType.Effect:
                return g_effectPath;
            case ResourcesType.Skill:
                return g_skillPath;
            case ResourcesType.SceneItem:
                return g_sceneItemPath;
            case ResourcesType.Audio:
                return g_audioPath;
            case ResourcesType.Scene:
                return g_scenePath;
            case ResourcesType.UiPublicWndAtlas:
                return g_uiPublicAtlasPath;
            case ResourcesType.Shader:
                return g_shaderPath;
            case ResourcesType.Map:
                return g_mapPath;
            case ResourcesType.luaData:
                return g_luaDataPath;
            case ResourcesType.MapData:
                return g_mapDataPath;
            case ResourcesType.Language:
                return g_languagePath;
            case ResourcesType.ComMaterial:
                return g_commonMaterialPath;
            case ResourcesType.Depend:
                return g_dependPath;
            case ResourcesType.UiFairy:
                return g_uiFairyPath;
            case ResourcesType.Config:
                return g_configPath;
            case ResourcesType.Other:
                return g_otherPath;
            default:
                return null;
        }
    }

    public static string GetRelativePath(string Type, ResourcesPathMode Mode)
    {
        int index = (int) Mode;
        string[] sArray = GetResTypePath(Type);
        if (sArray == null || sArray.Length == 0)
        {
            return string.Empty;
        }

        return sArray[index];
    }


    public static string GetFileExt(string Type)
    {
        string[] sArray = GetResTypePath(Type);
        if (sArray == null || sArray.Length < 3)
        {
            return string.Empty;
        }

        return sArray[2];
    }

    public static string GetEditorFullPath(string resType, string name)
    {
        string path = GetRelativePath(resType, ResourcesPathMode.Editor);
        string ext = GetFileExt(resType);
        string fullpath = string.Format("{0}{1}{2}", path, name, ext);

        return fullpath;
    }
}