using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace IGG.AssetImportSystem
{
    public class CustomImportRuleFactory
    {
        public const string TroopPath = "Assets/Models/Units/Solider";
        public const string GpuSkinTroopPath = "Assets/Models/Units/GpuSkinSolider";
        public const string HeroPath = "Assets/Models/Units/Hero";
        public const string BuildingPath = "Assets/Models/Environment/Models";

        public static ProjectImportSettings.ModelFolderRule Create(ModelImporter modelImporter, ProjectImportSettings.ModelFolderRule rule)
        {
            if (modelImporter.assetPath.StartsWith(GpuSkinTroopPath))
            {
                return new GpuSkinTroopModelImportSetting(rule);
            }
            else if (modelImporter.assetPath.StartsWith(TroopPath))
            {
                return new TroopModelImportSetting(rule);
            }
            else if (modelImporter.assetPath.StartsWith(HeroPath))
            {
                return new HeroModelImportSetting(rule);
            }
            else if (modelImporter.assetPath.StartsWith(BuildingPath))
            {
                return new BuildingModelImportSetting(rule);
            }
            return rule;
        }
    }
}
