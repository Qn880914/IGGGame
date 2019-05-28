using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace IGG.AssetImportSystem
{
    public class TextureImportDataEditor : EditorWindow
    {

        [MenuItem("辅助工具/资源管理/贴图格式设置")]
        static void DoTextureFormatSetting()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(TextureImportDataEditor));
            window.position = new Rect(100, 100, 1000, 600);  // 窗口的坐标
            window.minSize = new Vector2(1250, 600);
        }

        private string m_path;
        private string m_filter;
        private bool m_isRecursive;
        private string[] m_textureType = new string[] {
        "Default", "NormalMap", "Lightmap"
    };
        private int m_textureTypeSelected;
        private bool m_mipmap;
        private bool m_readWriteEnable;

        private string[] m_androidFormats = new string[] {
        "RGB ETC 4Bits", "RGB ETC2 4Bits", "RGB1A ETC2 4Bits", "RGBA ETC2 8Bits",
        "RGB 16", "RGB 24", "RGBA 16", "RGBA 32"
    };
        private int m_androidFormatSelected;

        private string[] m_iosFormats = new string[] {
        "RGB PVRTC 2Bits", "RGB PVRTC 4Bits", "RGBA PVRTC 2Bits", "RGBA PVRTC 4Bits",
        "RGB 16", "RGB 24", "RGBA 16", "RGBA 32",
        "RGB ASTC 4x4", "RGB ASTC 5x5", "RGB ASTC 6x6", "RGB ASTC 8x8","RGB ASTC 10x10", "RGB ASTC 12x12",
        "RGBA ASTC 4x4", "RGBA ASTC 5x5", "RGBA ASTC 6x6", "RGBA ASTC 8x8","RGBA ASTC 10x10", "RGBA ASTC 12x12",
    };
        private int m_iosFormatSelected;

        private int m_index;
        private string[] m_alphaSrc = new string[] {
            "FromInput", "None", "FromGrayScale"
        };
        private int m_alphaSrcSelected;
        private int m_maxSize;

        private Vector2 m_scrollPosition;
        private int m_selectedIndex;
        private int SelectedIndex {
            get { return m_selectedIndex; }
            set
            {
                GUI.FocusControl(null);
                m_selectedIndex = value;
                TextureImportDataToUI(TextureImportDataManager.Instance.GetRule(m_selectedIndex));
            }
        }

        private bool m_isShowResData;

        private TextureImportData NewData()
        {
            TextureImportData data = new TextureImportData();
            data.Index = TextureImportDataManager.Instance.GetNextIndex();
            return data;
        }

        private void TextureImportDataToUI(TextureImportData data)
        {
            if (null == data)
            {
                return;
            }

            m_path = data.AssetPath;
            m_isRecursive = data.IsRecursive;
            m_filter = data.FileFilter;
            m_index = data.Index;
            m_maxSize = data.MaxSize;
            switch (data.AlphaSource)
            {
                case TextureImporterAlphaSource.FromInput:
                    m_alphaSrcSelected = 0;
                    break;
                case TextureImporterAlphaSource.None:
                    m_alphaSrcSelected = 1;
                    break;
                case TextureImporterAlphaSource.FromGrayScale:
                    m_alphaSrcSelected = 2;
                    break;
            }
            switch (data.TextureType)
            {
                case TextureImporterType.Default:
                    m_textureTypeSelected = 0;
                    break;
                case TextureImporterType.NormalMap:
                    m_textureTypeSelected = 1;
                    break;
                case TextureImporterType.Lightmap:
                    m_textureTypeSelected = 2;
                    break;
            }
            m_mipmap = data.Mipmap;
            m_readWriteEnable = data.ReadWriteEnable;
            switch (data.AndroidFormat)
            {
                case TextureImporterFormat.ETC_RGB4:
                    m_androidFormatSelected = 0;
                    break;
                case TextureImporterFormat.ETC2_RGB4:
                    m_androidFormatSelected = 1;
                    break;
                case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA:
                    m_androidFormatSelected = 2;
                    break;
                case TextureImporterFormat.ETC2_RGBA8:
                    m_androidFormatSelected = 3;
                    break;
                case TextureImporterFormat.RGB16:
                    m_androidFormatSelected = 4;
                    break;
                case TextureImporterFormat.RGB24:
                    m_androidFormatSelected = 5;
                    break;
                case TextureImporterFormat.RGBA16:
                    m_androidFormatSelected = 6;
                    break;
                case TextureImporterFormat.RGBA32:
                    m_androidFormatSelected = 7;
                    break;
            }
            switch (data.IosFormat)
            {
                case TextureImporterFormat.PVRTC_RGB2:
                    m_iosFormatSelected = 0;
                    break;
                case TextureImporterFormat.PVRTC_RGB4:
                    m_iosFormatSelected = 1;
                    break;
                case TextureImporterFormat.PVRTC_RGBA2:
                    m_iosFormatSelected = 2;
                    break;
                case TextureImporterFormat.PVRTC_RGBA4:
                    m_iosFormatSelected = 3;
                    break;
                case TextureImporterFormat.RGB16:
                    m_iosFormatSelected = 4;
                    break;
                case TextureImporterFormat.RGB24:
                    m_iosFormatSelected = 5;
                    break;
                case TextureImporterFormat.RGBA16:
                    m_iosFormatSelected = 6;
                    break;
                case TextureImporterFormat.RGBA32:
                    m_iosFormatSelected = 7;
                    break;
                case TextureImporterFormat.ASTC_RGB_4x4:
                    m_iosFormatSelected = 8;
                    break;
                case TextureImporterFormat.ASTC_RGB_5x5:
                    m_iosFormatSelected = 9;
                    break;
                case TextureImporterFormat.ASTC_RGB_6x6:
                    m_iosFormatSelected = 10;
                    break;
                case TextureImporterFormat.ASTC_RGB_8x8:
                    m_iosFormatSelected = 11;
                    break;
                case TextureImporterFormat.ASTC_RGB_10x10:
                    m_iosFormatSelected = 12;
                    break;
                case TextureImporterFormat.ASTC_RGB_12x12:
                    m_iosFormatSelected = 13;
                    break;
                case TextureImporterFormat.ASTC_RGBA_4x4:
                    m_iosFormatSelected = 14;
                    break;
                case TextureImporterFormat.ASTC_RGBA_5x5:
                    m_iosFormatSelected = 15;
                    break;
                case TextureImporterFormat.ASTC_RGBA_6x6:
                    m_iosFormatSelected = 16;
                    break;
                case TextureImporterFormat.ASTC_RGBA_8x8:
                    m_iosFormatSelected = 17;
                    break;
                case TextureImporterFormat.ASTC_RGBA_10x10:
                    m_iosFormatSelected = 18;
                    break;
                case TextureImporterFormat.ASTC_RGBA_12x12:
                    m_iosFormatSelected = 19;
                    break;
            }
        }

        private void SaveCurSelectData()
        {
            TextureImportData data = TextureImportDataManager.Instance.GetRule(SelectedIndex);
            if (null == data)
            {
                return;
            }
            
            data.AssetPath = m_path;
            data.IsRecursive = m_isRecursive;
            data.FileFilter = m_filter;
            data.Index = m_index;
            data.MaxSize = m_maxSize;
            switch (m_alphaSrcSelected)
            {
                case 0:
                    data.AlphaSource = TextureImporterAlphaSource.FromInput;
                    break;
                case 1:
                    data.AlphaSource = TextureImporterAlphaSource.None;
                    break;
                case 2:
                    data.AlphaSource = TextureImporterAlphaSource.FromGrayScale;
                    break;
            }
            switch (m_textureTypeSelected)
            {
                case 0:
                    data.TextureType = TextureImporterType.Default;
                    break;
                case 1:
                    data.TextureType = TextureImporterType.NormalMap;
                    break;
                case 2:
                    data.TextureType = TextureImporterType.Lightmap;
                    break;
            }
            data.Mipmap = m_mipmap;
            data.ReadWriteEnable = m_readWriteEnable;
            switch (m_androidFormatSelected)
            {
                case 0:
                    data.AndroidFormat = TextureImporterFormat.ETC_RGB4;
                    break;
                case 1:
                    data.AndroidFormat = TextureImporterFormat.ETC2_RGB4;
                    break;
                case 2:
                    data.AndroidFormat = TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA;
                    break;
                case 3:
                    data.AndroidFormat = TextureImporterFormat.ETC2_RGBA8;
                    break;
                case 4:
                    data.AndroidFormat = TextureImporterFormat.RGB16;
                    break;
                case 5:
                    data.AndroidFormat = TextureImporterFormat.RGB24;
                    break;
                case 6:
                    data.AndroidFormat = TextureImporterFormat.RGBA16;
                    break;
                case 7:
                    data.AndroidFormat = TextureImporterFormat.RGBA32;
                    break;
            }
            switch (m_iosFormatSelected)
            {
                case 0:
                    data.IosFormat = TextureImporterFormat.PVRTC_RGB2;
                    break;
                case 1:
                    data.IosFormat = TextureImporterFormat.PVRTC_RGB4;
                    break;
                case 2:
                    data.IosFormat = TextureImporterFormat.PVRTC_RGBA2;
                    break;
                case 3:
                    data.IosFormat = TextureImporterFormat.PVRTC_RGBA4;
                    break;
                case 4:
                    data.IosFormat = TextureImporterFormat.RGB16;
                    break;
                case 5:
                    data.IosFormat = TextureImporterFormat.RGB24;
                    break;
                case 6:
                    data.IosFormat = TextureImporterFormat.RGBA16;
                    break;
                case 7:
                    data.IosFormat = TextureImporterFormat.RGBA32;
                    break;
                case 8:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_4x4;
                    break;
                case 9:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_5x5;
                    break;
                case 10:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_6x6;
                    break;
                case 11:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_8x8;
                    break;
                case 12:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_10x10;
                    break;
                case 13:
                    data.IosFormat = TextureImporterFormat.ASTC_RGB_12x12;
                    break;
                case 14:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_4x4;
                    break;
                case 15:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_5x5;
                    break;
                case 16:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_6x6;
                    break;
                case 17:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_8x8;
                    break;
                case 18:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_10x10;
                    break;
                case 19:
                    data.IosFormat = TextureImporterFormat.ASTC_RGBA_12x12;
                    break;
            }

            TextureImportDataManager.Save();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path:", GUILayout.MinWidth(50), GUILayout.MaxWidth(60));
            m_path = EditorGUILayout.TextField(m_path, GUILayout.MinWidth(400));
            EditorGUIUtility.labelWidth = 70;
            m_isRecursive = EditorGUILayout.Toggle("Recursive", m_isRecursive, GUILayout.MinWidth(80));
            EditorGUILayout.LabelField("NameFilter:", GUILayout.MinWidth(80), GUILayout.MaxWidth(90));
            m_filter = EditorGUILayout.TextField(m_filter, GUILayout.MinWidth(150));

            if (GUILayout.Button("Save", GUILayout.MinWidth(100)))
            {
                SaveCurSelectData();
            }
            if (GUILayout.Button("Delete", GUILayout.MinWidth(100)))
            {
                TextureImportDataManager.Instance.Delete(SelectedIndex);
            }
            if (GUILayout.Button("New Data", GUILayout.MinWidth(100)))
            {
                TextureImportData data = NewData();
                TextureImportDataManager.Instance.DataList.Add(data);
                TextureImportDataManager.Save();
                SelectedIndex = data.Index;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            m_textureTypeSelected = EditorGUILayout.Popup("TextureType", m_textureTypeSelected, m_textureType);
            GUILayout.Space(100);
            m_alphaSrcSelected = EditorGUILayout.Popup("AlphaSource", m_alphaSrcSelected, m_alphaSrc);
            EditorGUIUtility.labelWidth = 50;
            GUILayout.Space(100);
            m_androidFormatSelected = EditorGUILayout.Popup("Android", m_androidFormatSelected, m_androidFormats);
            GUILayout.Space(50);
            m_iosFormatSelected = EditorGUILayout.Popup("Ios", m_iosFormatSelected, m_iosFormats);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_mipmap = EditorGUILayout.ToggleLeft("Mipmap", m_mipmap, GUILayout.MinWidth(100));
            m_readWriteEnable = EditorGUILayout.ToggleLeft("ReadWriteEnable", m_readWriteEnable, GUILayout.MinWidth(150));
            EditorGUIUtility.labelWidth = 60;
            m_index = EditorGUILayout.IntField("Priority", m_index, GUILayout.MinWidth(100));
            GUILayout.Space(50);
            m_maxSize = EditorGUILayout.IntField("MaxSize", m_maxSize, GUILayout.MinWidth(100));
            GUILayout.EndHorizontal();

            int height = (TextureImportDataManager.Instance.DataList.Count+1) * 20;
            TextureImportData rule = TextureImportDataManager.Instance.GetRule(m_selectedIndex);
            string[] guids = null;
            if (null != rule)
            {
                guids = AssetDatabase.FindAssets("t:Texture", new string[] { rule.AssetPath });
                height += (guids.Length + 1) * 20;
            }

            m_scrollPosition = GUI.BeginScrollView(new Rect(0, 60, position.width, position.height - 60), m_scrollPosition, new Rect(0, 0, 1000, height));
            GUILayout.BeginHorizontal();
            GUILayout.Label("AssetPath", EditorStyles.label, GUILayout.MinWidth(250));
            GUILayout.Label("FileFilter", EditorStyles.label, GUILayout.MinWidth(120));
            GUILayout.Label("Priority", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("AlphaSource", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("TextureType", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("Mipmap", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("R/W", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("MaxSize", EditorStyles.label, GUILayout.MinWidth(85));
            GUILayout.Label("Android", EditorStyles.label, GUILayout.MinWidth(125));
            GUILayout.Label("Ios", EditorStyles.label, GUILayout.MinWidth(90));
            GUILayout.Label("Recursive", EditorStyles.label, GUILayout.MinWidth(55));
            GUILayout.Label("Apply", EditorStyles.label, GUILayout.MinWidth(55));
            GUILayout.EndHorizontal();

            GUIStyle style = GUI.skin.textField;
            //style.font = new Font()
            for (int i = 0; i < TextureImportDataManager.Instance.DataList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                TextureImportData data = TextureImportDataManager.Instance.DataList[i];

                if (data.Index == SelectedIndex)
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = new Color(0.8f, 0.8f, 0.8f, 1);
                }
                
                if (GUILayout.Button(data.AssetPath, style, GUILayout.MinWidth(250)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.FileFilter, style, GUILayout.MinWidth(120)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.Index.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.AlphaSource.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.TextureType.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.Mipmap.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.ReadWriteEnable.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.MaxSize.ToString(), style, GUILayout.MinWidth(80)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.AndroidFormat.ToString(), style, GUILayout.MinWidth(120)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.IosFormat.ToString(), style, GUILayout.MinWidth(120)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button(data.IsRecursive.ToString(), style, GUILayout.MinWidth(50)))
                {
                    SelectedIndex = data.Index;
                }
                if (GUILayout.Button("Apply", GUILayout.MinWidth(50)))
                {
                    TextureImportDataTool.ReimportTextures(data);
                }
                GUILayout.EndHorizontal();
            }

            if (null != guids)
            {
                int count = 0;
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }
                    if (!m_isRecursive)
                    {
                        string dir = path.Remove(path.LastIndexOf('/'));
                        if (!dir.Equals(m_path))
                        {
                            continue;
                        }
                    }
                    string name = path.Substring(path.LastIndexOf('/') + 1);
                    if (!Regex.IsMatch(name, m_filter))
                    {
                        continue;
                    }
                    TextureImporter ai = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (null != ai)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(path, GUILayout.MinWidth(250));
                        GUILayout.Label("", GUILayout.MinWidth(120));
                        GUILayout.Label((++count).ToString(), GUILayout.MinWidth(85));
                        GUILayout.Label(ai.alphaSource.ToString(), GUILayout.MinWidth(85));
                        GUILayout.Label(ai.textureType.ToString(), GUILayout.MinWidth(85));
                        GUILayout.Label(ai.mipmapEnabled.ToString(), GUILayout.MinWidth(85));
                        GUILayout.Label(ai.isReadable.ToString(), GUILayout.MinWidth(85));
                        TextureImporterPlatformSettings settingAndroid = ai.GetPlatformTextureSettings("Android");
                        GUILayout.Label(settingAndroid.maxTextureSize.ToString(), GUILayout.MinWidth(85));
                        GUILayout.Label(settingAndroid.format.ToString(), GUILayout.MinWidth(125));
                        TextureImporterPlatformSettings settingIos = ai.GetPlatformTextureSettings("iPhone");
                        GUILayout.Label(settingIos.format.ToString(), GUILayout.MinWidth(125));
                        GUILayout.Label("", GUILayout.MinWidth(55));
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUI.EndScrollView();
        }

    }
}