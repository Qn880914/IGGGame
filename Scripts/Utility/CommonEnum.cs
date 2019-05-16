namespace IGG.Game
{
    // 发布版本类型
    public enum ReleaseType
    {
        TRUNK = 1,      // 主干
        DEV = 2,        // 分支开发
        INNER = 3,      // 分支内部测试
        RELEASE = 4,    // 分支正式
    }

    // 补丁文件类型
    public enum PatchFileType
    {
        AssetBundle = 1,    // AssetBundle
        Wwise = 2,          // Wwise
    }

    // 建筑类型
    public enum EBuildingType
    {
        kConstructionNone = 0,

        kGoldStore = 1001, // 金库
        kBarn = 1002, // 粮仓
        kWorkShop = 1003, // 工坊
        kTreasureVault = 1004, // 藏宝库
        kClockTower = 1005, // 钟楼
        kVillageA = 1011, // 村落A
        kVillageB = 1012, // 村落B
        kVillageC = 1013,
        kVillageD = 1014,
        kVillageE = 1015,
        kVillageF = 1016,

        kFarmLand = 1021, // 农田
        kPasture = 1022, // 牧场

        kBarbican = 1023, // 沙漠碉楼
        kSettlement = 1024, // 沙漠聚落
        kCitiesTown = 1025, // 沙漠城镇
        kCapital = 1026, // 沙漠都城

        kLumberyard = 1031, // 精灵小窝
        kLumberMill = 1032, // 精灵花房
        kTreeHouse = 1033, // 精灵树屋
        kElfSettlement = 1034, // 精灵聚落
        kPalace = 1035, // 精灵宫殿
        kSaintArea = 1036, // 精灵圣域

        kMarket = 1041, // 市集
        kMarketA = 1042,
        kMarketB = 1043,
        kMarketC = 1044,
        kMarketD = 1045,
        kMarketE = 1046,
        kMarketAA = 1051,
        kMarketBB = 1052,
        kMarketCC = 1053,
        kMarketDD = 1054,
        kMarketEE = 1055,
        kMarketFF = 1056,

        kCaravanStart = 1101, // 商队建筑开始
        kOasisMarket = 1102, // 绿洲集市
        kBlackMarket = 1103, // 哥布尔黑市
        kCaravanEnd = 1104, // 商队建筑结束

        kMysterBusinessman = 1105, // 神秘商人

        kStone = 1501, // 石材采集厂

        kFirstCommandCenter = 2001, // 第一指挥部
        kSecondCommandCenter = 2002, // 第二指挥部
        kThirdCommandCenter = 2003, // 第三指挥部
        kDefenseCenter = 2004, // 防御中心
        kHeroHall = 2005, // 英雄大厅
        kTechnicalResearch = 2006, // 科技研究所
        kArmyResearch = 2007, // 兵种研究院
        kMagicResearch = 2008, // 法术研究所
        kHornorHall = 2010, // 荣誉大厅
        kWarcollege = 2011, // 战争学院

        kResearchStationStart = 2101, // 研究所起始
        kResearchElf = 2102, // 精灵研究所
        kResearchOrc = 2103, // 兽族研究所
        kResearchGhost = 2104, // 亡灵研究所
        kResearchStationEnd = 2105, // 研究所终止

        kMasterCastle = 3001, // 王城
        kWatchTower = 3002, // 瞭望塔
        kGuild = 3003, // 联盟大使馆
        kChurch = 3005, // 教堂
        kVictoryStatue = 3006, // 胜利女神像
        kFactory = 3109, // 工厂
        kBuildingFarmland = 3301, // 农田

        kGreateWonder = 4001, // 大奇观
        kSmallWonder = 4002, // 小奇观

        kLegendArchtecture1 = 5001, // 传奇建筑
        kLegendArchtecture2 = 5002,
        kLegendArchtecture3 = 5003,
        kLegendArchtecture4 = 5004,
        kLegendArchtecture5 = 5005,
        kLegendArchtecture6 = 5006,
        kLegendArchtecture7 = 5007,
        kLegendArchtecture8 = 5008,
        kLegendArchtecture9 = 5009,
        kLegendArchtecture10 = 5010,

        kFishingVillage = 5101, //渔村

        kSpaceGate = 5206, // 时空之门
        kResourceWar = 5207, //资源战

        kTavernExpeditionTem = 3101, // 酒馆探险队
        kFarmWorkshop = 3102, // 农作坊
        kSmelt = 3103, // 冶炼厂
        kLumberycamp = 3104, // 伐木场

        kWeaponShop = 3105, // 武器店
        kArmorShop = 3106, // 防具店
        kRestaurant = 3107, // 餐馆
        kHermetist = 3108, // 炼金术士屋


        k3201 = 3201,
        k3202 = 3202,
        k3203 = 3203,
        k3204 = 3204,
        k3205 = 3205,
        k3206 = 3206,
        k3207 = 3207,
        k3208 = 3208,
        k3209 = 3209,
        k3210 = 3210,

        kMysteriousCave = 5201, //神秘洞窟
        kGoblinLair = 5202, //哥布林巢穴
        kPyramid = 5203, //金字塔
        kTrollNest = 5204, //巨魔巢穴
        kWildnessShrine = 5205, //蛮荒神殿
        kDeathMarsh = 5209, // 死亡沼泽
        kArena = 5210, // 竞技场
        kWarStatue = 5211, // 战争魔像
        kFairyDreamFactory = 5102, // 精灵梦工厂
        kQuarry = 6001, //石矿
        kIronOre = 6002, //铁矿
        kGoldMine = 6003, //金矿
        kCrystalMine = 6004, //水晶矿
        kMysteryMine = 6005, //秘银矿
        kWheatField = 6011, //小麦田
        kPumpkinField = 6012, //南瓜田
        kCottonField = 6013, //棉花田
        kHerbalField = 6014, //草药田
        kChineseFir = 6021, //杉木
        kPineWood = 6022, //松木
        kMagicMushroomForest = 6023, //魔幻菇林
        kElfTree = 6024, //精灵树
    }
}