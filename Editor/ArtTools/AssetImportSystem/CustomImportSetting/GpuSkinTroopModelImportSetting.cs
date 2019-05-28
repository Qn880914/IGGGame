using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace IGG.AssetImportSystem
{
    [System.Serializable]
    public class GpuSkinTroopModelImportSetting : ProjectImportSettings.ModelFolderRule
    {
        public GpuSkinTroopModelImportSetting(ProjectImportSettings.ModelFolderRule rule) : base(rule)
        {
        }

        /// <summary>
        /// 根据兵种的名称，在"Prefabs/Actor/Soldier/"路径下生成兵种预设
        /// </summary>
        /// <param name="import"></param>
        public override void DoCustomRule(AssetImporter import)
        {
            string name = import.assetPath.Remove(0, import.assetPath.LastIndexOf('/') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));
            if (name.Contains("@") && !(name.ToLower()).Contains("@skin"))
            {
                return;
            }
            ModelImporter mImport = import as ModelImporter;
            if (null != mImport)
            {
                mImport.importAnimation = false;
            }
            if (name.Contains("@"))
            {
                name = name.Remove(name.IndexOf('@'));
            }
            string subPath = "/Data/Prefabs/Actor/Soldier/" + name + ".prefab";
            string path = Application.dataPath + subPath;
            {
                string prefabPath = "";
                if (name.ToLower().Contains("cavalry"))
                {
                    prefabPath = "Assets/Data/Prefabs/Actor/Soldier/template_cavalry.prefab";
                }
                else
                {
                    prefabPath = "Assets/Data/Prefabs/Actor/Soldier/template.prefab";
                }
                GameObject obj = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
                PrefabUtility.CreatePrefab("Assets" + subPath, obj);

                Material mat = new Material(Shader.Find("Zero/GPUSkin/VertexLit"));
                mat.enableInstancing = true;

                string assetDir = import.assetPath.Remove(import.assetPath.LastIndexOf('/'));
                List<string> tgaFiles = FileUtil.GetAllChildFiles(assetDir, ".tga");
                List<string> pngFiles = FileUtil.GetAllChildFiles(assetDir, ".png");
                List<string> jpgFiles = FileUtil.GetAllChildFiles(assetDir, ".jpg");
                tgaFiles.AddRange(pngFiles);
                tgaFiles.AddRange(jpgFiles);
                foreach (string fileName in tgaFiles)
                {
                    if (fileName.ToLower().Contains(name.ToLower()))
                    {
                        Texture tex = AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture)) as Texture;
                        if (null != tex)
                        {
                            mat.mainTexture = tex;
                            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
                            if (null != textureImporter)
                            {
                                int maxSize;
                                TextureImporterFormat format;
                                bool hasAlpha = false;
                                if (textureImporter.GetPlatformTextureSettings("Android", out maxSize, out format))
                                {
                                    hasAlpha = IsTextureContainAlpha(format);
                                }
                                else if (textureImporter.GetPlatformTextureSettings("iPhone", out maxSize, out format))
                                {
                                    hasAlpha = IsTextureContainAlpha(format);
                                }
                                if (mat.HasProperty("_AlphaVal"))
                                {
                                    mat.SetFloat("_AlphaVal", hasAlpha ? 0.5f : 1);
                                }
                            }
                            break;
                        }
                    }
                }
                string iceTexPath = "Assets/Effect/Effect_Ay/Textures/e_ice_map.png";
                Texture iceTex = AssetDatabase.LoadAssetAtPath(iceTexPath, typeof(Texture)) as Texture;
                if (mat.HasProperty("_IceTex"))
                {
                    mat.SetTexture("_IceTex", iceTex);
                }

                string saveMatPath = "Assets/" + ResourcesPath.GpuSkinTroopMaterialPath + "m_" + name + ".mat";
                AssetDatabase.CreateAsset(mat, saveMatPath);
                AssetDatabase.Refresh();
            }

        }

        public bool IsTextureContainAlpha(TextureImporterFormat format)
        {
            string formatName = format.ToString().ToLower();
            if (formatName.Contains("rgba") || formatName.Contains("argb"))
            {
                return true;
            }
            return false;
        }
    }
}
