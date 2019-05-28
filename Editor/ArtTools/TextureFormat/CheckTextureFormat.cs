using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using IGG.EditorTools.AssetCheck;
using UnityEditor;

/// <summary>
/// 
/// Author mingzhang02
/// Date 20181022
/// Desc 贴图格式检测脚本
/// 
/// </summary>
public class CheckTextureFormat {

    class TextureData
    {
        public long Size;
        public string Message;
        public Object Tex;
        public TextureData(long size, string msg, Object tex)
        {
            Size = size;
            Message = msg;
            Tex = tex;
        }
    }

    [AutoCheckItem("贴图格式检测")]
    private static void AutoCheckTextureFormat()
    {
        int totalCount;
        long totalMem;
        List<TextureData> outputs = DoCheckTextureFormat("Assets", out totalCount, out totalMem);

        AssetCheckLogger.Log("TotalCount:"+totalCount+"\t"+"TotalMem:"+ConvertToString(totalMem)+"(editor下检测的内存大概是实际使用的两倍，具体大小取决于真机。另外这里统计了所有贴图，包含没有被打进包里的。)");
        for (int i = 0; i < outputs.Count; i++)
        {
            AssetCheckLogger.Log(outputs[i].Message);
        }
        outputs.Clear();
    }
    [MenuItem("Assets/规范检测/贴图格式检测", true)]
    private static bool CanCheckTextureFormatManual()
    {
        string path = "";
        if (AssetMenu.CheckSelectionFileDir(ref path))
        {
            return true;
        }
        return false;
    }
    [MenuItem("Assets/规范检测/贴图格式检测", false)]
    private static void CheckTextureFormatManual()
    {
        string dir = "";

        if (!AssetMenu.CheckSelectionFileDir(ref dir))
        {
            return;
        }
        int totalCount;
        long totalMem;
        List<TextureData> outputs = DoCheckTextureFormat(dir, out totalCount, out totalMem);

        for (int i = 0; i < outputs.Count; i++)
        {
            Debug.Log(outputs[i].Message, outputs[i].Tex);
        }
        outputs.Clear();
    }

    private static string ConvertToString(long size)
    {
        string output = "";
        float fSize = 0;
        if (size >= 1024 * 1024 * 1024)
        {
            fSize = size / (1024.0f * 1024.0f * 1024.0f);
            output = fSize.ToString("0.0");
            output += "GB";
        }
        else if (size >= 1024 * 1024)
        {
            fSize = size / (1024.0f * 1024.0f);
            output = fSize.ToString("0.00");
            output += "MB";
        }
        else
        {
            fSize = size / (1024.0f);
            output = fSize.ToString("0.00");
            output += "KB";
        }
        return output;
    }

    private static List<TextureData> DoCheckTextureFormat(string dir, out int totalCount, out long totalMem)
    {
        List<TextureData> outputs = new List<TextureData>();
        string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { dir });
        totalCount = guids.Length;
        totalMem = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string message = "";
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }
            EditorUtility.DisplayProgressBar("检测贴图格式", path, (float)(i + 1) / guids.Length);

            TextureImporter ai = AssetImporter.GetAtPath(path) as TextureImporter;
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
            
            long size = 0;
            if (null != obj && null != ai)
            {
                size = Profiler.GetRuntimeMemorySizeLong(obj);
                Texture tex = obj as Texture;
                if (tex != null)
                {
                    message += "size:" + ConvertToString(size) + "\t";
                    message += tex.width + "*" + tex.height + "\t";
                    message += "mipmap:" + ai.mipmapEnabled + "\t";
                    message += "r/w:" + ai.isReadable + "\t";
                    //message += "texture type:" + ai.textureType + "\t";
                    //message += "alpha source:" + ai.alphaSource + "\t";
                    TextureImporterPlatformSettings settingAndroid = ai.GetPlatformTextureSettings("Android");
                    message += "android:" + settingAndroid.format + "\t";
                    TextureImporterPlatformSettings settingIos = ai.GetPlatformTextureSettings("Android");
                    message += "ios:" + settingIos.format + "\t";
                }
            }
            else
            {
                message += "load failed.\t";
            }
            message += path;
            totalMem += size;
            outputs.Add(new TextureData(size, message, obj));
        }
        outputs.Sort(
            delegate (TextureData a, TextureData b)
            {
                return b.Size.CompareTo(a.Size);
            }
            );
        EditorUtility.ClearProgressBar();
        return outputs;
    }
}
