using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CombineMesh : EditorWindow
{
    [MenuItem("LightMap/Mesh/合并Mesh")]
    static void AddWindow()
    {
        //创建窗口
        CombineMesh window = (CombineMesh)EditorWindow.GetWindow(typeof(CombineMesh), false, "合并Mesh");
        window.Show();

    }
    void OnGUI()
    {
        GUIStyle text_style = new GUIStyle();
        text_style.fontSize = 15;
        text_style.alignment = TextAnchor.MiddleCenter;

        if (GUILayout.Button("合并选中物体的Mesh", GUILayout.Height(30)))
        {
            Mesh combineMesh = new Mesh();
            List<MeshFilter> meshlist = new List<MeshFilter>();
            foreach (GameObject m in Selection.gameObjects)
            {
                meshlist.Add(m.GetComponent<MeshFilter>());
            }
            CombineInstance[] combine = new CombineInstance[meshlist.Count];
            for (int i = 0; i < meshlist.Count; i++)
            {
                combine[i].mesh = meshlist[i].sharedMesh;
                combine[i].transform = meshlist[i].transform.localToWorldMatrix;
            }
            GameObject root = new GameObject("root");
            root.AddComponent<MeshRenderer>().sharedMaterial = meshlist[0].GetComponent<MeshRenderer>().sharedMaterial;
            root.AddComponent<MeshFilter>().sharedMesh = new Mesh();
            root.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        }
    }


    void OnInspectorUpdate()
    {
        this.Repaint();
    }
}