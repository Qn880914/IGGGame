#region import namespace

using UnityEditor;
using UnityEngine;

#endregion

/// <summary>
/// 已经弃用
/// </summary>
public class MapPlane : ScriptableWizard
{
    public enum AnchorPoint
    {
        TopLeft,
        TopHalf,
        TopRight,
        RightHalf,
        BottomRight,
        BottomHalf,
        BottomLeft,
        LeftHalf,
        Center
    }

    public enum OrientationType
    {
        Horizontal,
        Vertical
    }

    public AnchorPoint Anchor = AnchorPoint.TopLeft;
    public OrientationType Orientation = OrientationType.Horizontal;
    public bool TwoSided = false;

    // 地图格子数
    public int WGridNum = 70;
    // 地图格子数
    public int HGridNum = 30;

    // 地图格子单位
    public float WGridUnit = 1.0f;
    // 地图格子单位
    public float HGridunit = 1.0f;


    //[MenuItem("辅助工具/地图系统/创建地图面片")]
    private static void CreateWizard()
    {
        DisplayWizard("创建地图Plane", typeof(MapPlane));
    }


    private void OnWizardCreate()
    {
        float mapWidth = WGridNum*WGridUnit;
        float mapLength = HGridNum*HGridunit;


        GameObject prefab = AssetDatabase.LoadAssetAtPath(
            "Assets/Editor/ArtTools/UITool/GridMap.prefab", 
            typeof(GameObject)) as GameObject;

        GameObject mgo = Instantiate(prefab);
        mgo.name = "Map";
        GameObject plane = mgo.transform.Find("MapPlane").gameObject;

        float dis = WGridNum > HGridNum ? WGridNum : HGridNum;
        dis = dis*0.65f;


        if (Camera.main != null)
        {
            mgo.transform.position = Camera.main.transform.position + Camera.main.transform.forward*dis;
            if (Anchor == AnchorPoint.BottomLeft)
            {
                Vector3 pos = mgo.transform.position;
                pos = new Vector3(pos.x - WGridNum*0.5f, pos.y, pos.z - HGridNum*0.5f);
                mgo.transform.position = pos;
            }
        }
        else
        { 
            mgo.transform.position = Vector3.zero;
        }

        Vector2 anchorOffset;
        string anchorId;
        switch (Anchor)
        {
            case AnchorPoint.TopLeft:
                anchorOffset = new Vector2(-mapWidth/2.0f, mapLength/2.0f);
                anchorId = "TL";
                break;
            case AnchorPoint.TopHalf:
                anchorOffset = new Vector2(0.0f, mapLength/2.0f);
                anchorId = "TH";
                break;
            case AnchorPoint.TopRight:
                anchorOffset = new Vector2(mapWidth/2.0f, mapLength/2.0f);
                anchorId = "TR";
                break;
            case AnchorPoint.RightHalf:
                anchorOffset = new Vector2(mapWidth/2.0f, 0.0f);
                anchorId = "RH";
                break;
            case AnchorPoint.BottomRight:
                anchorOffset = new Vector2(mapWidth/2.0f, -mapLength/2.0f);
                anchorId = "BR";
                break;
            case AnchorPoint.BottomHalf:
                anchorOffset = new Vector2(0.0f, -mapLength/2.0f);
                anchorId = "BH";
                break;
            case AnchorPoint.BottomLeft:
                anchorOffset = new Vector2(-mapWidth/2.0f, -mapLength/2.0f);
                anchorId = "BL";
                break;
            case AnchorPoint.LeftHalf:
                anchorOffset = new Vector2(-mapWidth/2.0f, 0.0f);
                anchorId = "LH";
                break;
            case AnchorPoint.Center:
            default:
                anchorOffset = Vector2.zero;
                anchorId = "C";
                break;
        }

        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        MeshFilter meshFilterTerrian = plane.transform.Find("Terrian").GetComponent<MeshFilter>();


        string planeAssetName = plane.name + "W" + mapWidth + "H" + mapLength + anchorId + ".asset";
        Mesh m = (Mesh) AssetDatabase.LoadAssetAtPath("Assets/Editor/ArtTools/UITool/" + planeAssetName, typeof(Mesh));

        if (m == null)
        {
            m = new Mesh();
            m.name = plane.name;

            int hCount2 = WGridNum + 1;
            int vCount2 = HGridNum + 1;
            float temp = WGridNum*HGridNum*6;
            int numTriangles = (int) temp;
            if (TwoSided)
            {
                numTriangles *= 2;
            }
            int numVertices = hCount2*vCount2;

            Vector3[] vertices = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles = new int[numTriangles];
            Vector4[] tangents = new Vector4[numVertices];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            int index = 0;
            float uvFactorX = 1.0f/WGridNum;
            float uvFactorY = 1.0f/HGridNum;
            for (int y = 0; y < vCount2; y++)
            {
                for (int x = 0; x < hCount2; x++)
                {
                    if (Orientation == OrientationType.Horizontal)
                    {
                        vertices[index] = new Vector3(x*WGridUnit - mapWidth/2f - anchorOffset.x, 0.0f,
                            y*HGridunit - mapLength/2f - anchorOffset.y);
                    }
                    else
                    {
                        vertices[index] = new Vector3(x*WGridUnit - mapWidth/2f - anchorOffset.x,
                            y*HGridunit - mapLength/2f - anchorOffset.y, 0.0f);
                    }
                    tangents[index] = tangent;
                    uvs[index++] = new Vector2(x*uvFactorX, y*uvFactorY);
                }
            }

            // 三角面
            index = 0;
            for (int y = 0; y < HGridNum; y++)
            {
                for (int x = 0; x < WGridNum; x++)
                {
                    triangles[index] = y*hCount2 + x;
                    triangles[index + 1] = (y + 1)*hCount2 + x;
                    triangles[index + 2] = y*hCount2 + x + 1;

                    triangles[index + 3] = (y + 1)*hCount2 + x;
                    triangles[index + 4] = (y + 1)*hCount2 + x + 1;
                    triangles[index + 5] = y*hCount2 + x + 1;
                    index += 6;
                }
                if (TwoSided)
                {
                    // Same tri vertices with order reversed, so normals point in the opposite direction
                    for (int x = 0; x < WGridNum; x++)
                    {
                        triangles[index] = y*hCount2 + x;
                        triangles[index + 1] = y*hCount2 + x + 1;
                        triangles[index + 2] = (y + 1)*hCount2 + x;

                        triangles[index + 3] = (y + 1)*hCount2 + x;
                        triangles[index + 4] = y*hCount2 + x + 1;
                        triangles[index + 5] = (y + 1)*hCount2 + x + 1;
                        index += 6;
                    }
                }
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.tangents = tangents;
            m.RecalculateNormals();

            AssetDatabase.CreateAsset(m, "Assets/Editor/ArtTools/UITool/" + planeAssetName);
            AssetDatabase.SaveAssets();
        }

        meshFilter.sharedMesh = m;
        meshFilterTerrian.sharedMesh = m;
        m.RecalculateBounds();
        plane.AddComponent<BoxCollider>();

        MeshRenderer ren = plane.GetComponent<MeshRenderer>();
        ren.sharedMaterial.SetTextureScale("_MainTex", new Vector2(WGridNum, HGridNum));
        ren.sharedMaterial.SetVector("_Bound", new Vector4(0, HGridNum, WGridNum, HGridNum));

        Selection.activeObject = plane;
    }
}