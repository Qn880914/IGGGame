#region Namespace

using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

#endregion

namespace IGG.Animation.Model
{
    [ProtoContract]
    public class MeshAnimationSerializable
    {
        [ProtoMember(7)] public string[] BoneNames;

        [ProtoMember(5)] public float[] FloatBonePositions;

        [ProtoMember(6)] public float[] FloatBoneRotations;

        [ProtoMember(1)] public float[] FloatFrames;

        [ProtoMember(2)] public int FrameCount;

        [ProtoMember(3)] public int FrameSize;

        [ProtoMember(4)] public string Name;

        public void AddFrames(string nameLocal, List<Vector3[]> pFrames, MeshAnimationBoneGroup pBoneGroup)
        {
            Name = nameLocal;
            FrameCount = pFrames.Count;
            if (pFrames.Count < 1)
            {
                return;
            }

            FrameSize = pFrames[0].Length;
            List<float> list = new List<float>();
            foreach (Vector3[] current in pFrames)
            {
                Vector3[] array = current;
                for (int i = 0; i < array.Length; i++)
                {
                    Vector3 vector = array[i];
                    list.Add(vector.x);
                    list.Add(vector.y);
                    list.Add(vector.z);
                }
            }

            FloatFrames = list.ToArray();
            list.Clear();
            BoneNames = pBoneGroup.BoneNames.ToArray();
            foreach (KeyValuePair<string, MeshAnimationBoneTransform> current2 in pBoneGroup.Bones)
            {
                foreach (Vector3 current3 in current2.Value.Positions)
                {
                    list.Add(current3.x);
                    list.Add(current3.y);
                    list.Add(current3.z);
                }
            }

            FloatBonePositions = list.ToArray();
            list.Clear();
            foreach (KeyValuePair<string, MeshAnimationBoneTransform> current4 in pBoneGroup.Bones)
            {
                foreach (Quaternion current5 in current4.Value.Rotations)
                {
                    list.Add(current5.x);
                    list.Add(current5.y);
                    list.Add(current5.z);
                    list.Add(current5.w);
                }
            }

            FloatBoneRotations = list.ToArray();
            list.Clear();
        }
    }
}