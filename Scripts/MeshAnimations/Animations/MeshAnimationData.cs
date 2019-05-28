/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:29
	file base:	MeshAnimationData
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using IGG.Animation.Model;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace IGG.Animation.Data
{
    public class MeshAnimationData
    {
        private List<Vector3[]> m_bonePositions;

        private List<Quaternion[]> m_boneRotations;
        private Vector3[][] m_frames;

        protected bool m_generateNormal;


        public MeshAnimationData(MeshAnimationSerializable animationSerializable, int pBoneCount, bool generateNormal)
        {
            GenerateNormal = generateNormal;
            FrameCount = animationSerializable.FrameCount;
            Name = animationSerializable.Name;

            BoneNames = animationSerializable.BoneNames;

            BoneCount = pBoneCount;

            BuildFrames(animationSerializable.FloatFrames,
                        animationSerializable.FrameSize);

            BuildBones(animationSerializable.FloatBonePositions,
                       animationSerializable.FloatBoneRotations,
                       pBoneCount,
                       animationSerializable.FrameCount);
        }

        public MeshAnimationData(string animName, Vector3[][] animData, bool generateNormal)
        {
            FrameCount = animData.Length;
            Name = animName;
            GenerateNormal = generateNormal;
            BoneNames = null;

            BoneCount = 0;

            BuildFramesByData(animData);
        }

        public int BoneCount { get; private set; }

        public int FrameCount { get; private set; }

        public string Name { get; private set; }

        public string[] BoneNames { get; private set; }

        public bool GenerateNormal
        {
            get { return m_generateNormal; }
            set { m_generateNormal = value; }
        }

        private void BuildFramesByData(Vector3[][] animData)
        {
            m_frames = animData;
        }

        /// <summary>
        /// Get the vertices for the requested frame.
        /// </summary>
        /// <returns>Frame vertices</returns>
        /// <param name="pFrame">Requested frame</param>
        public Vector3[] GetFrameVertices(int pFrame)
        {
            if (pFrame < m_frames.Length && pFrame >= 0)
            {
                return m_frames[pFrame];
            }

            return null;
        }

        public Quaternion[] GetFrameBoneRotations(int pFrame)
        {
            if (pFrame >= FrameCount && pFrame < 0)
            {
                UnityEngine.Debug.LogError("MeshAnimationData.GetFrameBoneRotations(): Invalid frame index (" + pFrame.ToString() + ")");
            }


            if (pFrame < m_frames.Length && pFrame >= 0)
            {
                Quaternion[] rot = new Quaternion[BoneCount];

                for (int i = 0; i < BoneCount; i++)
                {
                    rot[i] = m_boneRotations[i][pFrame];
                }

                return rot;
            }

            return null;
        }

        public Vector3[] GetFrameBonePositions(int pFrame)
        {
            if (pFrame < FrameCount && pFrame >= 0)
            {
                UnityEngine.Debug.LogError("MeshAnimationData.GetFrameBonePositions(): Invalid frame index (" + pFrame + ")");
            }


            if (pFrame < m_frames.Length && pFrame >= 0)
            {
                Vector3[] pos = new Vector3[BoneCount];

                for (int i = 0; i < BoneCount; i++)
                {
                    pos[i] = m_bonePositions[i][pFrame];
                }

                return pos;
            }

            return null;
        }

        private void BuildBones(float[] pFloatPositions, float[] pFloatRotations, int pBoneCount, int pFrameCount)
        {
            m_bonePositions = new List<Vector3[]>();
            m_boneRotations = new List<Quaternion[]>();

            if (pFloatPositions == null)
            {
                return;
            }

            for (int i = 0; i < pBoneCount; i++)
            {
                Vector3[] positions = new Vector3[pFrameCount];
                Quaternion[] rotations = new Quaternion[pFrameCount];

                for (int j = 0; j < pFrameCount; j++)
                {
                    int posIndex = j * 3 + i * pFrameCount * 3;
                    int rotIndex = j * 4 + i * pFrameCount * 4;

                    float px = pFloatPositions[posIndex];
                    float py = pFloatPositions[posIndex + 1];
                    float pz = pFloatPositions[posIndex + 2];

                    positions[j] = new Vector3(px, py, pz);

                    float rx = pFloatRotations[rotIndex];
                    float ry = pFloatRotations[rotIndex + 1];
                    float rz = pFloatRotations[rotIndex + 2];
                    float rw = pFloatRotations[rotIndex + 3];

                    rotations[j] = new Quaternion(rx, ry, rz, rw);
                }

                m_bonePositions.Add(positions);
                m_boneRotations.Add(rotations);
            }
        }

        /// <summary>
        /// Rebuild the original Vector3 vertices for each frame using deserialized
        /// float array vertices.
        /// </summary>
        private void BuildFrames(float[] pFloatFrames, int pFrameSize)
        {
            m_frames = new Vector3[FrameCount][];

            for (int i = 0; i < FrameCount; i++)
            {
                Vector3[] vertices = new Vector3[pFrameSize];

                for (int j = 0; j < pFrameSize; j++)
                {
                    int index = j * 3 + i * pFrameSize * 3;

                    float x = pFloatFrames[index];
                    float y = pFloatFrames[index + 1];
                    float z = pFloatFrames[index + 2];

                    vertices[j] = new Vector3(x, y, z);
                }

                m_frames[i] = vertices;
            }
        }

        public void Destory()
        {
            BoneNames = null;
            m_frames = null;
            m_bonePositions = null;
            m_boneRotations = null;
        }
    }
}