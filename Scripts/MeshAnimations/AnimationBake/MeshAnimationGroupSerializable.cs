#region Namespace

using ProtoBuf;
using UnityEngine;

#endregion

namespace IGG.MeshAnimation.Model
{
    [ProtoContract]
    public class MeshAnimationGroupSerializable
    {
        [ProtoMember(5)] public int BoneCount;

        [ProtoMember(6)] public string[] BoneNames;

        [ProtoMember(1)] public float[] FloatUv;

        //[ProtoMember(3)]
        //public Dictionary<string, MeshAnimationSerializable> animations;

        [ProtoMember(3)] public float Fps;

        [ProtoMember(4)] public string ModelName;

        [ProtoMember(2)] public int[] Triangles;

        //public MeshAnimationGroupSerializable() {
        //    //this.animations = new Dictionary<string, MeshAnimationSerializable>();
        //}

        public void SetUv(Vector2[] pUv)
        {
            FloatUv = new float[pUv.Length * 2];
            for (int i = 0; i < pUv.Length; i++)
            {
                FloatUv[i * 2] = pUv[i].x;
                FloatUv[i * 2 + 1] = pUv[i].y;
            }
        }

        public void SetTriangles(int[] pTriangles)
        {
            Triangles = pTriangles.Clone() as int[];
        }

        public void SetFrameInterval(float pFps)
        {
            Fps = pFps;
        }

        //public void AddAnimation(string pName, List<Vector3[]> fFrames, MeshAnimationBoneGroup pBoneGroup) {
        //    if (this.animations == null) {
        //        this.animations = new Dictionary<string, MeshAnimationSerializable>();
        //    }
        //    if (this.boneNames == null) {
        //        IGG.Logging.Logger.Log("pBoneGroup.boneNames.Count: " + pBoneGroup.boneNames.Count);
        //        this.boneNames = pBoneGroup.boneNames.ToArray();
        //        IGG.Logging.Logger.Log("this.boneNames.Length: " + this.boneNames.Length);
        //    }
        //    MeshAnimationSerializable meshAnimationSerializable = new MeshAnimationSerializable();
        //    meshAnimationSerializable.AddFrames(pName, fFrames, pBoneGroup);
        //    this.animations[pName] = meshAnimationSerializable;
        //}
    }
}