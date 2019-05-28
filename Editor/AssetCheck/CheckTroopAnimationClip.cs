using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 检查小兵动画长度
/// </summary>
public class CheckTroopAnimationClip {

    [MenuItem("Assets/规范检测/小兵动画检测", true)]
    static bool CanCheck()
    {
        string path = "";
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            return true;
        }
        return false;
    }

    [MenuItem("Assets/规范检测/小兵动画检测", false)]
    static void DoCheck()
    {
        string path = "";
        
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            string resultPath = Application.dataPath + "/../TroopAnimationClip.txt";
            StreamWriter writer = new StreamWriter(resultPath);
            List<string> filesPath = IGG.FileUtil.GetAllChildFiles(path, ".FBX");
            List<AnimationClip> wait2ClipList = new List<AnimationClip>();
            List<AnimationClip> otherClipList = new List<AnimationClip>();
            for (int i = 0; i < filesPath.Count; i++)
            {
                EditorUtility.DisplayProgressBar("检测小兵动画", filesPath[i], (float)i / filesPath.Count);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath(filesPath[i], typeof(AnimationClip)) as AnimationClip;
                if (null != clip)
                {
                    if (clip.name.ToLower().Contains("wait2"))
                    {
                        if ((int)(clip.frameRate * clip.length) > 60)
                        {
                            wait2ClipList.Add(clip);
                        }
                    }
                    else
                    {
                        if ((int)(clip.frameRate * clip.length) > 30)
                        {
                            otherClipList.Add(clip);
                        }
                    }
                }
            }
            //
            wait2ClipList.Sort((a, b) =>
            {
                return (b.frameRate * b.length).CompareTo(a.frameRate * a.length);
            });
            otherClipList.Sort((a, b) =>
            {
                return (b.frameRate * b.length).CompareTo(a.frameRate * a.length);
            });
            writer.WriteLine("===================wait2动作超过60帧===================");
            for (int i = 0; i < wait2ClipList.Count; i++)
            {
                int frame = (int)(wait2ClipList[i].frameRate * wait2ClipList[i].length);
                string clipPath = AssetDatabase.GetAssetPath(wait2ClipList[i]);
                writer.WriteLine(frame + "  " + clipPath);
            }
            writer.WriteLine(" ");
            writer.WriteLine("===================其他动作超过30帧===================");
            for (int i = 0; i < otherClipList.Count; i++)
            {
                int frame = (int)(otherClipList[i].frameRate * otherClipList[i].length);
                string clipPath = AssetDatabase.GetAssetPath(otherClipList[i]);
                writer.WriteLine(frame + "  " + clipPath);
            }
            writer.WriteLine("===================检测结束===================");
            EditorUtility.ClearProgressBar();
            writer.Flush();
            writer.Close();
            Debug.Log("检测结果保存在"+resultPath);
        }
    }
}
