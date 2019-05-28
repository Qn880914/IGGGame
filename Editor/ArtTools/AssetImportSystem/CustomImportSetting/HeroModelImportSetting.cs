using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

namespace IGG.AssetImportSystem
{
    [System.Serializable]
    public class HeroModelImportSetting : ProjectImportSettings.ModelFolderRule
    {
        public HeroModelImportSetting(ProjectImportSettings.ModelFolderRule rule) : base(rule)
        {

        }

        /// <summary>
        /// 根据兵种的名称，在"Data/Prefabs/Actor/Hero/"路径下生成英雄预设
        /// 复制出各个英雄的动画到目录:Data/Animations/Units/Hero
        /// 材质球使用Toon/Basic着色器
        /// </summary>
        /// <param name="import"></param>
        public override void DoCustomRule(AssetImporter import)
        {
            string name = import.assetPath.Remove(0, import.assetPath.LastIndexOf('/') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));
            name = name.ToLower();
            if (name.Contains("@") && !name.Contains("@skin"))
            {
                AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(import.assetPath, typeof(AnimationClip));
                if (null != clip)
                {
                    AnimationClip newClip = GameObject.Instantiate(clip);
                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(newClip);
                    settings.loopTime = name.Contains("run") || name.Contains("wait") || name.Contains("win");
                    AnimationUtility.SetAnimationClipSettings(newClip, settings);
                    name = name.Remove(name.IndexOf('@'));
                    string dir = Application.dataPath + "/Data/Units/Hero/anim_" + name;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    AssetDatabase.CreateAsset(newClip, "Assets/Data/Units/Hero/anim_"+name+"/" + clip.name + ".anim");
                    AssetDatabase.Refresh();
                }
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
            string subPath = "/Data/Prefabs/Actor/Hero/" + name + ".prefab";
            string path = Application.dataPath + subPath;
            {
                RuntimeAnimatorController animatorCtr = AssetDatabase.LoadAssetAtPath("Assets/Data/Animator/Units/BaseHeroAnimatorController.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                GameObject prefab = AssetDatabase.LoadAssetAtPath(import.assetPath, typeof(GameObject)) as GameObject;
                GameObject target = null;
                if (null != prefab)
                {
                    target = GameObject.Instantiate(prefab);
                }
                else
                {
                    Debug.LogError("load asset at path failed. " + import.assetPath);
                }
                if (null != prefab && null != target && null != animatorCtr)
                {
                    Animator animator = target.GetComponent<Animator>();
                    if (null != animator)
                    {
                        animator.runtimeAnimatorController = animatorCtr;
                    }
                    GameObject shadowGo = new GameObject("Shadow");
                    GameObject headGo = new GameObject("head_point");
                    GameObject chestGo = new GameObject("chest_point");
                    shadowGo.transform.SetParent(target.transform);
                    headGo.transform.SetParent(target.transform);
                    chestGo.transform.SetParent(target.transform);
                    shadowGo.transform.localPosition = Vector3.zero;
                    headGo.transform.localPosition = new Vector3(0, 4, 0);
                    chestGo.transform.localPosition = new Vector3(0, 2.5f, 0);

                    PrefabUtility.CreatePrefab("Assets" + subPath, target);
                    Renderer renderer = prefab.GetComponentInChildren<Renderer>();
                    if (null != renderer && renderer.sharedMaterial != null)
                    {
                        renderer.sharedMaterial.shader = Shader.Find("Toon/Basic");
                        if (renderer.sharedMaterial.HasProperty("_ToonShade")) {
                            Texture tex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Effects/ToonShading/Textures/ToonLit.psd", typeof(Texture)) as Texture;
                            renderer.sharedMaterial.SetTexture("_ToonShade", tex);
                        }
                    }
                    GameObject.DestroyImmediate(target);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
