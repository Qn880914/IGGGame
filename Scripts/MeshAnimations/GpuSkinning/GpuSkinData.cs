using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.MeshAnimation
{
    
    public class GpuSkinData : ScriptableObject
    {
        public int Fps;
        public int BlockWidth;
        public int BlockHeight;
        public int AnimationTextureWidth;
        public int AnimationTextureHeight;

        public CustomSkinMesh[] SkinMeshes;
        public CustomClipData[] Clips;  //所有的动画片段数据

        public void Dispose()
        {
            Clips = null;
            for (int i = 0; i < SkinMeshes.Length; i++)
            {
                SkinMeshes[i].Vertices = null;
                SkinMeshes[i].Triangles = null;
                SkinMeshes[i].Indices = null;
                SkinMeshes[i].Normals = null;
                SkinMeshes[i].Uv = null;
                SkinMeshes[i].Weights = null;
                SkinMeshes[i].BoneIndex = null;
            }
            SkinMeshes = null;
        }

        [Serializable]
        public struct CustomSkinMesh
        {
            public string Name;
            public Vector3[] Vertices;
            public int[] Triangles;
            public int[] Indices;
            public Vector3[] Normals;
            public Vector2[] Uv;
            public Vector4[] Weights;
            public Vector4[] BoneIndex;
        }

        [Serializable]
        public struct CustomClipData
        {
            public string ClipName;
            public int StartFrameIndex;
            public int FrameNum;
        }
    }


}
