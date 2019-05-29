using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainLightMapEditor : EditorWindow
{
    private GameObject m_target;
    private const string m_savePath = "Data/Prefabs/Map/Terrain/";
    [MenuItem("Tools/SaveTerrainTileTool")]
    static void BakeTerrainLightMap()
    {
        Rect rect = new Rect(0, 0, 300, 200);
        TerrainLightMapEditor window = (TerrainLightMapEditor)EditorWindow.GetWindowWithRect(typeof(TerrainLightMapEditor), rect, true, "TerrainLightMapEditor");
        window.Show();
    }

    private void OnGUI()
    {
        m_target = (GameObject)EditorGUILayout.ObjectField(m_target, typeof(GameObject), true);
        if (GUILayout.Button("执行", GUILayout.Width(100)))
        {
            if (null == m_target)
            {
                Debug.LogError("target is null.");
                return;
            }
            if (!Directory.Exists(Application.dataPath +"/"+ m_savePath))
            {
                Directory.CreateDirectory(Application.dataPath + "/" + m_savePath);
            }
            SaveTerrain();
        }
    }

    private void SaveTerrain()
    {
        FileUtil.DeleteFileOrDirectory(m_savePath);
        for (int i = 0; i < m_target.transform.childCount; i++)
        {
            Transform child = m_target.transform.GetChild(i);
            PrefabUtility.SaveAsPrefabAsset(child.gameObject, "Assets/" +m_savePath+child.name+".prefab");
        }

        TerrainController terrainCtr = m_target.GetComponent<TerrainController>();
        if (null == terrainCtr)
        {
            terrainCtr = m_target.AddComponent<TerrainController>();
        }
        terrainCtr.Init();
        AssetDatabase.Refresh();
        m_target.transform.DestroyChildren();
        
    }
}