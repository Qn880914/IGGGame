using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
namespace IGG.AssetImportSystem
{
    public class TextureImportDataManager
    {
        private static TextureImportDataManager g_instance;
        public static TextureImportDataManager Instance
        {
            get
            {
                if (null == g_instance)
                {
                    g_instance = new TextureImportDataManager();
                    if (!System.IO.File.Exists(m_path))
                    {
                        System.IO.File.Create(m_path);
                        Save();
                    }
                    Read();
                }
                return g_instance;
            }
        }
        [SerializeField]
        public List<TextureImportData> DataList = new List<TextureImportData>();
        private const string m_path = "Assets/TextureImportSetting.txt";

        public static void Read()
        {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(m_path), g_instance);
        }

        public static void Save()
        {
            Instance.SortDataList();
            System.IO.File.WriteAllText(m_path, JsonUtility.ToJson(Instance, true));
        }

        public void SortDataList()
        {
            DataList.Sort(
                delegate (TextureImportData a, TextureImportData b) 
                {
                    return a.Index.CompareTo(b.Index);
                }
                );
        }

        public int GetNextIndex()
        {
            int next = -1;
            for (int i = 0; i < DataList.Count; i++)
            {
                if (DataList[i].Index >= next)
                {
                    next = DataList[i].Index + 1;
                }
            }
            return next;
        }

        public TextureImportData GetRule(int index)
        {
            for (int i = 0; i < DataList.Count; i++)
            {
                if (DataList[i].Index == index)
                {
                    return DataList[i];
                }
            }
            return null;
        }

        public TextureImportData GetRule(string path, string name)
        {
            TextureImportData rule = null;
            for (int i = 0; i < DataList.Count; i++)
            {
                if (path.StartsWith(DataList[i].AssetPath))
                {
                    if (rule == null)
                    {
                        if (DataList[i].IsMatch(name))
                        {
                            rule = DataList[i];
                        }
                    }
                    else if(rule.Index < DataList[i].Index)
                    {
                        if (DataList[i].IsMatch(name))
                        {
                            rule = DataList[i];
                        }
                    }
                }
            }
            return rule;
        }

        public void Delete(int index)
        {
            for (int i = DataList.Count - 1; i >= 0; i--)
            {
                if (DataList[i].Index == index)
                {
                    DataList.RemoveAt(i);
                }
            }
            Save();
        }
    }

    [System.Serializable]
    public class TextureImportData
    {
        public string AssetPath = "";
        public string FileFilter = "";
        public bool IsRecursive = true;
        public int Index = -1;
        public int TotalCount = 0;
        public int TotalMemUse = 0;

        public TextureImporterAlphaSource AlphaSource = TextureImporterAlphaSource.FromInput;
        public TextureImporterType TextureType = TextureImporterType.Default;
        public bool Mipmap = false;
        public bool ReadWriteEnable = false;
        public int MaxSize = -1;
        public TextureImporterFormat AndroidFormat = TextureImporterFormat.ETC2_RGB4;
        public TextureImporterFormat IosFormat = TextureImporterFormat.PVRTC_RGB4;

        public bool IsMatch(string name)
        { 
            return Regex.IsMatch(name, FileFilter);
        }

        public TextureImportData()
        {
            AssetPath = "";
            IsRecursive = true;
            FileFilter = "";
            Index = -1;
            TotalCount = 0;
            TotalMemUse = 0;
            AlphaSource = TextureImporterAlphaSource.FromInput;
            TextureType = TextureImporterType.Default;
            Mipmap = false;
            ReadWriteEnable = false;
            MaxSize = -1;
            AndroidFormat = TextureImporterFormat.ETC2_RGB4;
            IosFormat = TextureImporterFormat.PVRTC_RGB4;
        }
    }
}