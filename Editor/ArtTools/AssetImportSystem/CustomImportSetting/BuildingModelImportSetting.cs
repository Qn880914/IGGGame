using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IGG.AssetImportSystem
{
    public class BuildingModelImportSetting : ProjectImportSettings.ModelFolderRule
    {
        public BuildingModelImportSetting(ProjectImportSettings.ModelFolderRule rule) : base(rule)
        {
        }

        public override void DoCustomRule(AssetImporter import)
        {
            
            if (!import.assetPath.StartsWith("Assets/Models/Environment/Models/Environment/COL3Building/Models"))
            {
                return;
            }
            ModelImporter modelImporter = (ModelImporter)import;
            bool isFindRule = false;
            bool isMerge = false;
            for (int i = 0; i < BuildingAssets.Instance.ConfigList.Count; i++)
            {
                BuildingAssets.BuildingConfig cfg = BuildingAssets.Instance.ConfigList[i];
                
                int index = modelImporter.assetPath.LastIndexOf("/");
                string modelName = modelImporter.assetPath.Substring(index+1);
                
                if (modelName.StartsWith(cfg.ModelName) &&
                    modelName.EndsWith(".FBX"))
                {
                    modelImporter.importAnimation = true;
                    isMerge = cfg.IsMergeMesh;
                    if (cfg.ModelLevelList != null)
                    {
                        for (int j = 0; j < cfg.ModelLevelList.Count; j++)
                        {
                            if (!string.IsNullOrEmpty( cfg.ModelLevelList[j].ModelLevelName) &&
                                modelImporter.assetPath.EndsWith(cfg.ModelLevelList[j].ModelLevelName + ".FBX"))
                            {
                                ModelImporterClipAnimation[] anims = new ModelImporterClipAnimation[cfg.ModelLevelList[j].ClipDefine.Count];
                                for (int m = 0; m < cfg.ModelLevelList[j].ClipDefine.Count; m++)
                                {
                                    anims[m] = SetClipAnimation(cfg.ModelLevelList[j].ClipDefine[m].ClipName, 
                                        cfg.ModelLevelList[j].ClipDefine[m].StartFrame, 
                                        cfg.ModelLevelList[j].ClipDefine[m].EndFrame, 
                                        cfg.ModelLevelList[j].ClipDefine[m].IsLoop);
                                }
                                modelImporter.clipAnimations = anims;
                                isFindRule = true;
                            }
                        }
                    }
                }
            }
            //
            if (isFindRule)
            {
                modelImporter.SaveAndReimport();
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(modelImporter.assetPath);
                SplitAnimationClipAndMergeMesh.DoSplitAnimationClipAndMergeMesh(go, isMerge);
            }
        }

        ModelImporterClipAnimation SetClipAnimation(string name, uint first, uint last, bool isLoop)
        {
            ModelImporterClipAnimation tempClip = new ModelImporterClipAnimation();
            tempClip.name = name;
            tempClip.firstFrame = first;
            tempClip.lastFrame = last;
            tempClip.loop = isLoop;
            if (isLoop)
            {
                tempClip.wrapMode = WrapMode.Loop;
            }
            else
            { 
                tempClip.wrapMode = WrapMode.Once;
            }

            return tempClip;
        }
    }
}