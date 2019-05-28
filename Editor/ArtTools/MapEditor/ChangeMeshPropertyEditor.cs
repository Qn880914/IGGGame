using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// 修改网格的顶点属性。
/// 例如uv\color\normal\tangent等，修改后的网格以asset方式存放在目录
/// 下。
/// </summary>
public class ChangeMeshPropertyEditor : EditorWindow
{
    private bool m_isCopyUV2;
    private bool m_isCopyNormal;

    private bool m_isChangeUV1;
    private bool m_isChangeUV2;
    private bool m_isChangeUV3;
    private bool m_isChangeUV4;
    private bool m_isChangeColor;
    private bool m_isChangeNormal;
    private bool m_isChangeTangent;

    private Vector2 m_uv1;
    private Vector2 m_uv2;
    private Vector2 m_uv3;
    private Vector2 m_uv4;
    private Color m_color;
    private Vector4 m_normal;
    private Vector4 m_tangent;

    private Mesh m_targetMesh;

    [MenuItem("Tools/ChangeMeshPropertyTool")]
    static void ShowWeldVertexToolWindow()
    {
        Rect rect = new Rect(0, 0, 300, 500);
        ChangeMeshPropertyEditor window = (ChangeMeshPropertyEditor)EditorWindow.GetWindowWithRect(typeof(ChangeMeshPropertyEditor), rect, true, "ChangeMeshPropertyTool");
        window.Show();
    }

    Mesh ChangeMesh(Mesh mesh)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.uv = mesh.uv;
        if (m_isCopyUV2)
        {
            newMesh.uv2 = mesh.uv2;
        }
        if (m_isCopyNormal)
        {
            newMesh.normals = mesh.normals;
        }

        if (m_isChangeUV1)
        {
            Vector2[] uvs = mesh.uv;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                uvs[i] = m_uv1;
            }
            newMesh.uv = uvs;
        }
        if (m_isChangeUV2)
        {
            Vector2[] uvs = mesh.uv2;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                uvs[i] = m_uv2;
            }
            newMesh.uv2 = uvs;
        }
        if (m_isChangeUV3)
        {
            Vector2[] uvs = mesh.uv3;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                uvs[i] = m_uv3;
            }
            newMesh.uv3 = uvs;
        }
        if (m_isChangeUV4)
        {
            Vector2[] uvs = mesh.uv4;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                uvs[i] = m_uv4;
            }
            newMesh.uv4 = uvs;
        }
        if (m_isChangeColor)
        {
            Color[] colors = mesh.colors;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                colors[i] = m_color;
            }
            newMesh.colors = colors;
        }
        if (m_isChangeNormal)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                normals[i] = m_normal;
            }
            newMesh.normals = normals;
        }
        if (m_isChangeTangent)
        {
            Vector4[] tangents = mesh.tangents;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                tangents[i] = m_tangent;
            }
            newMesh.tangents = tangents;
        }
        return newMesh;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        m_targetMesh = (Mesh)EditorGUILayout.ObjectField(m_targetMesh, typeof(Mesh), true);
        EditorGUILayout.Space();
        m_isCopyUV2 = EditorGUILayout.Toggle("is copy uv2", m_isCopyUV2);
        m_isCopyNormal = EditorGUILayout.Toggle("is copy normal", m_isCopyNormal);

        EditorGUILayout.Space();
        m_isChangeUV1 = EditorGUILayout.Toggle("uv1", m_isChangeUV1);
        if (m_isChangeUV1)
        {
            m_uv1 = EditorGUILayout.Vector2Field("uv1", m_uv1);
        }

        EditorGUILayout.Space();
        m_isChangeUV2 = EditorGUILayout.Toggle("uv2", m_isChangeUV2);
        if (m_isChangeUV2)
        {
            m_uv2 = EditorGUILayout.Vector2Field("uv2", m_uv2);
        }
        EditorGUILayout.Space();
        m_isChangeUV3 = EditorGUILayout.Toggle("uv3", m_isChangeUV3);
        if (m_isChangeUV3)
        {
            m_uv3 = EditorGUILayout.Vector2Field("uv3", m_uv3);
        }
        EditorGUILayout.Space();
        m_isChangeUV4 = EditorGUILayout.Toggle("uv4", m_isChangeUV4);
        if (m_isChangeUV4)
        {
            m_uv4 = EditorGUILayout.Vector2Field("uv4", m_uv4);
        }
        EditorGUILayout.Space();
        m_isChangeColor = EditorGUILayout.Toggle("color", m_isChangeColor);
        if (m_isChangeColor)
        {
            m_color = EditorGUILayout.ColorField("color", m_color);
        }
        EditorGUILayout.Space();
        m_isChangeNormal = EditorGUILayout.Toggle("normal", m_isChangeNormal);
        if (m_isChangeNormal)
        {
            m_normal = EditorGUILayout.Vector4Field("normal", m_normal);
        }
        EditorGUILayout.Space();
        m_isChangeTangent = EditorGUILayout.Toggle("tangent", m_isChangeTangent);
        if (m_isChangeTangent)
        {
            m_tangent = EditorGUILayout.Vector4Field("tangent", m_tangent);
        }
        EditorGUILayout.Space();
        if (null != m_targetMesh && GUILayout.Button("Save"))
        {
            string path = "Assets/Models/Environment/Models/Environment/";
            path = EditorUtility.SaveFilePanelInProject("保存网格", m_targetMesh.name+"_c", "asset", "保存网格", path);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            Mesh newMesh = ChangeMesh(m_targetMesh);
            AssetDatabase.CreateAsset(newMesh, path);
            AssetDatabase.Refresh();
            Debug.Log("保存"+m_targetMesh.name+"于"+path);
        }
    }
}
