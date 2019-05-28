using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GOE.Scene;
using System;
using IGG.Game;

/// <summary>
/// TODO: 代码添加PrefabLightDat,保存烘焙信息，创建预设，添加这个脚本，删除下面的tile
/// </summary>
[DisallowMultipleComponent]
public class TerrainController : MonoBehaviour
{

    [SerializeField]
    private TerrainTileData[] m_terrainTileArray;
    public TerrainTileData[] TerrainTiles {
        get { return m_terrainTileArray; }
    }

    [Serializable]
    public class TerrainTileData
    {
        public GameObject TerrainGo;
        public string TerrainName;
        public Rect Bound;

        public TerrainTileData(string name)
        {
            TerrainName = name;
            Bound = new Rect();
            TerrainGo = null;
        }

        public bool CalculateBound(GameObject go)
        {
            if (null == go)
            {
                return false;
            }
            MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
            Bounds bound = new Bounds();
            for (int i = 0; i < filters.Length; i++)
            {
                if (i == 0)
                {
                    bound = filters[i].sharedMesh.bounds;
                }
                else
                {
                    bound.Encapsulate(filters[i].sharedMesh.bounds);
                }
            }
            Vector2 min = new Vector2(bound.min.x, bound.min.z);
            Vector2 size = new Vector2(bound.size.x, bound.size.z);
            Bound = new Rect(min, size);
            return true;
        }
    }

    //相机范围内的地形同步加载
    void Start()
    {
        LoadTerrain();
    }

    private void Update()
    {
        UpdateTerrainVisible();
    }

    public void Init()
    {
        m_terrainTileArray = new TerrainTileData[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            m_terrainTileArray[i] = new TerrainTileData(child.gameObject.name);
            m_terrainTileArray[i].CalculateBound(child.gameObject);
        }
    }

    /// <summary>
    /// 如果地形不在相机视锥体范围内，则disable.
    /// </summary>
    private void UpdateTerrainVisible()
    {
        /*Rect cameraView = IGG.Util.CalculateViewRange(CameraController.Instance.MainCamera, 5, 0);
        for (int i = 0; i < m_terrainTileArray.Length; i++)
        {
            if (null == m_terrainTileArray[i].TerrainGo)
            {
                continue;
            }
            
            bool visible = cameraView.Overlaps(m_terrainTileArray[i].Bound);
            if (m_terrainTileArray[i].TerrainGo.activeSelf != visible)
            {
                m_terrainTileArray[i].TerrainGo.SetActive(visible);
            }
        }*/
    }

    public void LoadTerrain()
    {
        /*CameraController.Instance.MainCamera.transform.position = CommonDC.CameraPosition;
        Rect cameraView = IGG.Utility.CalculateViewRange(CameraController.Instance.MainCamera, 0, 0);
        for (int i = 0; i < m_terrainTileArray.Length; i++)
        {
            if (string.IsNullOrEmpty(m_terrainTileArray[i].TerrainName))
            {
                continue;
            }
            TerrainTileData data = m_terrainTileArray[i];
            bool async = !cameraView.Overlaps(data.Bound);
            ResourceManger.LoadMap("Terrain/"+m_terrainTileArray[i].TerrainName, transform, async, (go)=> {
                data.TerrainGo = go;
            }, true);
        }*/
    }
}
