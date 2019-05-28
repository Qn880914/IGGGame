#region Namespace

using System;
using UnityEngine;

#endregion

namespace IGG.Game
{
    public class MeshSkinData : ScriptableObject
    {
        public int arrayLength;
        public bool GenerateNormal;
        public bool isLoadAllInMainScene = false;
        public int maxVertexCount;

        public SubMeshData[] meshes;

        public int[] vertexCounts;

        [Serializable]
        public struct SubMeshData
        {
            public ClipData[] clips;
            public float framerate;
            public Vector2[] uvs;
            public int[] triangles;
        }

        [Serializable]
        public struct ClipData
        {
            public FrameData[] frames;
        }

        [Serializable]
        public struct FrameData
        {
            public Vector3[] vertexs;
        }
    }
}