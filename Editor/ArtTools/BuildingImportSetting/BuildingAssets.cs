using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IGG.AssetImportSystem
{
    [CreateAssetMenu]
    public class BuildingAssets : ScriptableObject
    {
        [Serializable]
        public class AnimationDefine
        {
            public string ClipName;
            public uint StartFrame;
            public uint EndFrame;
            public bool IsLoop;
        }

        [Serializable]
        public class BuildingLevelConfig
        {
            public string ModelLevelName;
            public List<AnimationDefine> ClipDefine = new List<AnimationDefine>();
        }

        [Serializable]
        public class BuildingConfig
        {
            public string ModelName;
            public bool IsMergeMesh;
            public List<BuildingLevelConfig> ModelLevelList = new List<BuildingLevelConfig>();
        }

        [SerializeField]
        public List<BuildingConfig> ConfigList = new List<BuildingConfig>();

        private static BuildingAssets g_instance;
        public static BuildingAssets Instance
        {
            get {
                if (null == g_instance)
                {
                    ReadBuildingImportSetting();
                }
                return g_instance;
            }
        }

        public static void ReadBuildingImportSetting()
        {
            g_instance = AssetDatabase.LoadAssetAtPath("Assets/BuildingImportSetting.asset", typeof(BuildingAssets)) as BuildingAssets;
            //UnityEngine.Object[] allObjs = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget("Assets/BuildingImportSetting.asset");
            //foreach (var obj in allObjs)
            //{
            //    BuildingAssets serializedInstance = obj as BuildingAssets;
            //    if (serializedInstance != null)
            //        g_instance = serializedInstance;
            //}
            if (null == g_instance)
            {
                g_instance = ScriptableObject.CreateInstance<BuildingAssets>();
                AssetDatabase.CreateAsset(g_instance, "Assets/BuildingImportSetting.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            g_instance.ConfigList.Sort((a, b) => {
                return a.ModelName.CompareTo(b.ModelName);
            });
        }
    }
}