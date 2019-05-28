/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:28
	file base:	MeshAnimation
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System.Collections.Generic;
using IGG.Animation.Data;
using UnityEngine;

#endregion

namespace IGG.Animation
{
    public class MeshAnimation
    {
        private Dictionary<string, Vector3[]> m_bonePositions;

        private Dictionary<string, Quaternion[]> m_boneRotations;
        protected MeshAnimationData m_data;
        private Mesh[] m_frames;
        protected bool m_hasInitFrame;
        protected bool m_isGenerateNormal;
        protected int[] m_triangles;

        protected Vector2[] m_uv;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAnimation"/> class.
        /// </summary>
        /// <param name="pUv">Mesh UV</param>
        /// <param name="pTriangles">Mesh Triangles</param>
        /// <param name="pData">Mesh animation data</param>
        public MeshAnimation(Vector2[] pUv, int[] pTriangles, MeshAnimationData pData)
        {
            FrameCount = pData.FrameCount;
            Name = pData.Name;
            m_isGenerateNormal = pData.GenerateNormal;
            BoneNames = pData.BoneNames;

            m_uv = pUv;
            m_triangles = pTriangles;
            m_data = pData;

            m_hasInitFrame = false;
        }

        public int FrameCount { get; private set; }

        public string Name { get; private set; }

        public string[] BoneNames { get; private set; }

        public void Destory()
        {
            if (m_frames != null)
            {
                for (int i = 0; i < m_frames.Length; i++)
                {
                    Object.DestroyImmediate(m_frames[i], true);
                }

                m_frames = null;
            }

            m_bonePositions = null;
            m_boneRotations = null;
            m_uv = null;
            m_data = null;
            m_triangles = null;
        }

        private void BuildBonePositions(MeshAnimationData pData)
        {
            m_bonePositions = new Dictionary<string, Vector3[]>();
            m_boneRotations = new Dictionary<string, Quaternion[]>();

            if (pData.BoneCount <= 0)
            {
                return;
            }

            for (int i = 0; i < FrameCount; i++)
            {
                Vector3[] positions = pData.GetFrameBonePositions(i);
                Quaternion[] rotations = pData.GetFrameBoneRotations(i);

                for (int j = 0; j < pData.BoneCount; j++)
                {
                    string key = pData.BoneNames[j];

                    if (!m_bonePositions.ContainsKey(key))
                    {
                        m_bonePositions[key] = new Vector3[FrameCount];
                    }

                    m_bonePositions[key][i] = positions[j];

                    if (!m_boneRotations.ContainsKey(key))
                    {
                        m_boneRotations[key] = new Quaternion[FrameCount];
                    }

                    m_boneRotations[key][i] = rotations[j];
                }
            }
        }

        /// <summary>
        /// Construct the mesh frames from mesh animation data.
        /// </summary>
        public void BuildFrames()
        {
            m_frames = new Mesh[FrameCount];

            for (int i = 0; i < FrameCount; i++)
            {
                Mesh mesh = new Mesh();
                mesh.name = "SkinMesh";
                mesh.vertices = m_data.GetFrameVertices(i);
                mesh.uv = m_uv;
                mesh.triangles = m_triangles;
                if (m_isGenerateNormal)
                {
                    mesh.RecalculateNormals();
                }

                m_frames[i] = mesh;
            }

            m_uv = null;
            m_triangles = null;
            m_data = null;
            m_hasInitFrame = true;
        }

        /// <summary>
        /// Get the mesh for the requested frame.
        /// </summary>
        /// <returns>Frame mesh</returns>
        /// <param name="pFrame">Frame index</param>
        public Mesh GetFrame(int pFrame)
        {
            if (!m_hasInitFrame)
            {
                BuildFrames();
            }

            if (pFrame >= 0 && pFrame < m_frames.Length)
            {
                return m_frames[pFrame];
            }

            return null;
        }

        public Vector3 GetBonePosition(int pFrame, string pBoneName)
        {
            // Look up if we have a position for this bone name
            Vector3[] frames;
            if (m_bonePositions.TryGetValue(pBoneName, out frames))
            {
                if (pFrame >= 0 && pFrame < frames.Length)
                {
                    return frames[pFrame];
                }
            }

            return Vector3.zero;
        }

        public Quaternion GetBoneRotation(int pFrame, string pBoneName)
        {
            if (m_boneRotations.ContainsKey(pBoneName) && pFrame >= 0 && pFrame < m_boneRotations[pBoneName].Length)
            {
                return m_boneRotations[pBoneName][pFrame];
            }

            return Quaternion.identity;
        }
    }
}