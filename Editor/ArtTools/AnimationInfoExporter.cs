using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class AnimationInfoExporter : EditorWindow
{
    struct UnitAnimInfo {
        public string ActorName;
        public string ActName;
        public uint FrameCount;

        public UnitAnimInfo(string actorName, string actName, uint frameCount) {
            ActorName = actorName;
            ActName = actName;
            FrameCount = frameCount;
        }
    }


    static List<UnitAnimInfo> GetTroopAnimInfo()
    {
        const string TroopModelPath = "Models/Units/Solider/";
        List<UnitAnimInfo> list = new List<UnitAnimInfo>();

        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + TroopModelPath);
        DirectoryInfo[] subDirs = dir.GetDirectories();

        for (int i = 0; i < subDirs.Length; i++)
        {
            FileInfo[] files = subDirs[i].GetFiles();
            for (int j = 0; j < files.Length; j++)
            {
                //非模型跳过
                if (!files[j].Name.EndsWith(".FBX"))
                    continue;
                //非包含动画的模型也跳过
                if (!files[j].Name.Contains("@") || files[j].Name.Contains("@skin"))
                    continue;

                string assetPath = "Assets/" + TroopModelPath + subDirs[i].Name + "/" + files[j].Name;
                GameObject fbx = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                if (null != fbx)
                {
                    Animation fbxAnim = fbx.GetComponent<Animation>();
                    if (null != fbxAnim && null != fbxAnim.clip)
                    {
                        UnitAnimInfo info = new UnitAnimInfo();
                        info.ActorName = subDirs[i].Name;
                        info.ActName = files[j].Name.Remove(files[j].Name.LastIndexOf('.'));
                        info.ActName = info.ActName.Remove(0, info.ActName.LastIndexOf('@') + 1);
                        info.FrameCount = (uint)(fbxAnim.clip.frameRate * fbxAnim.clip.length);
                        list.Add(info);
                    }
                }
            }
            EditorUtility.DisplayProgressBar("导出小兵动画帧数", subDirs[i].Name, (float)(i + 1) / subDirs.Length);
        }
        EditorUtility.ClearProgressBar();
        return list;
    }

    static List<UnitAnimInfo> GetHeroAnimInfo()
    {
        string HeroAnimPath = ResourcesPath.GetRelativePath(ResourcesType.HeroAnim, ResourcesPathMode.Editor);
        List<UnitAnimInfo> list = new List<UnitAnimInfo>();

        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + HeroAnimPath);
        DirectoryInfo[] subDirs = dir.GetDirectories();

        for (int i = 0; i < subDirs.Length; i++)
        {
            FileInfo[] files = subDirs[i].GetFiles();
            for (int j = 0; j < files.Length; j++)
            {
                if (!files[j].Name.EndsWith(".anim"))
                    continue;

                string assetPath = "Assets/" + HeroAnimPath + subDirs[i].Name + "/" + files[j].Name;
                AnimationClip anim = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip))as AnimationClip;
                if (null != anim)
                {
                    UnitAnimInfo info = new UnitAnimInfo();
                    info.ActorName = subDirs[i].Name;
                    info.ActName = files[j].Name.Remove(files[j].Name.LastIndexOf('.'));
                    info.FrameCount = (uint)(anim.frameRate * anim.length);
                    list.Add(info);
                }
            }
            EditorUtility.DisplayProgressBar("导出英雄动画帧数", subDirs[i].Name, (float)(i + 1) / subDirs.Length);
        }
        EditorUtility.ClearProgressBar();
        return list;
    }

    static string UnitAnimInfosToString(List<UnitAnimInfo> info)
    {
        string value = "";
        for (int i = 0; i < info.Count; i++)
        {
            value += info[i].ActorName + "," + info[i].ActName + "," + info[i].FrameCount + "\r\n";
        }
        return value;
    }

    [@MenuItem("辅助工具/配置相关/导出anim_info表")]
    static void ExportAnimInfo()
    {
        try
        {
            string fullPath = Application.dataPath + "/Config/anim_info.csv";
            string value = "";
            value += "角色资源名,动作名,动画长度" + "\r\n";
            value += "string,string,uint" + "\r\n";
            value += "#actor_name,#act_name,frame_count" + "\r\n";

            List<UnitAnimInfo> list = GetHeroAnimInfo();
            value += UnitAnimInfosToString(list);
            list.Clear();
            list = GetTroopAnimInfo();
            value += UnitAnimInfosToString(list);

            FileInfo newFile = new FileInfo(fullPath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(fullPath);
            }

            Encoding encoding = Encoding.GetEncoding("gb2312");
            StreamWriter csv = new StreamWriter(@fullPath, false, encoding);

            csv.Write(value);
            csv.Close();

            Debug.Log("导出anim_info.csv成功.");
        }
        catch (Exception err)
        {
            Debug.LogError(err.Message);
        }
    }
}
