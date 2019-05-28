/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:29
	file base:	MeshAnimationGroupData
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System.Collections.Generic;
using IGG.Game;
using UnityEngine;

#endregion

namespace IGG.Animation.Data
{
    public class MeshAnimationGroupData
    {
        private static string[] g_defaultAnimationArray =
            {"Attack1", "Win", "Dead", "Hit", "Run", "Skill1", "Wait1", "Wait2"};

        //public MeshAnimationGroupData (object pGroupSerializable)
        //{
        //          Animations = new Dictionary<string, MeshAnimationData> ();

        //          Texture2D cfgData = pGroupSerializable as Texture2D;

        //          Color[] data = cfgData.GetPixels();
        //          Color cfgLength = data[0];

        //          Fps = (int)cfgLength[0];
        //          float uvLength = cfgLength[1];
        //          float triLength = cfgLength[2];

        //          int cfgDataIdx = 1;
        //          UV = new Vector2[(int)uvLength];
        //          for (int i = 0; i < UV.Length / 2; i++) {
        //              cfgDataIdx++;
        //              int uvIdx = i * 2;
        //              Color singleUv = data[1 + i];
        //              UV[uvIdx].Set(singleUv.r, singleUv.g);
        //              UV[uvIdx + 1].Set(singleUv.b, singleUv.a);
        //          }

        //          Triangles = new int[(int)triLength];
        //          for (int i = 0; i < Triangles.Length / 3; i++) {
        //              int triIdx = i * 3;
        //              Color singleTri = data[cfgDataIdx++];
        //              Triangles[triIdx++] = (int)singleTri.r;
        //              Triangles[triIdx++] = (int)singleTri.g;
        //              Triangles[triIdx++] = (int)singleTri.b;
        //          }

        //          //MeshAnimationGroupSerializable serialzableData = pGroupSerializable as MeshAnimationGroupSerializable;

        //          //Fps = serialzableData.fps;

        //          //BuildUV (serialzableData.floatUV);

        //          //Triangles = serialzableData.triangles;


        //          //ModelName = pGroupSerializable.modelName;

        //          //BoneCount = pGroupSerializable.boneCount;

        //          //BoneNames = pGroupSerializable.boneNames;

        //          //foreach (string key in pGroupSerializable.animations.Keys)
        //          //{
        //          //	MeshAnimationSerializable animation = pGroupSerializable.animations [key];
        //          //	Animations.Add (animation.name, new MeshAnimationData (animation, BoneCount));
        //          //}
        //      }

        public MeshAnimationGroupData(MeshSkinData skinData)
        {
            AnimationsWithAttach = new Dictionary<int, Dictionary<string, MeshAnimationData>>();

            int totalCount = skinData.arrayLength;
            AttachCount = totalCount;

            Uv = new List<Vector2[]>(totalCount);
            Triangles = new List<int[]>(totalCount);

            for (int i = 0; i < totalCount; ++i)
            {
                Fps = (int) skinData.meshes[i].framerate;

                Uv.Add(skinData.meshes[i].uvs);
                Triangles.Add(skinData.meshes[i].triangles);
            }
        }

        public Dictionary<string, MeshAnimationData> Animations { get; private set; }

        public Dictionary<int, Dictionary<string, MeshAnimationData>> AnimationsWithAttach { get; private set; }

        public List<Vector2[]> Uv { get; private set; }

        public List<int[]> Triangles { get; private set; }

        public float Fps { get; private set; }

        public int AttachCount { get; private set; }

        //public string ModelName { get; private set; }

        public int BoneCount { get; private set; }

        public string[] BoneNames { get; private set; }

        public void BuildAnim(MeshSkinData skinData)
        {
            int count = AttachCount;
            for (int i = 0; i < count; i++)
            {
                //UnityEngine.Profiling.Profiler.BeginSample("Load Cfg");
                BuildAnimByCombinedTex(skinData, i);
                //UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public void BuildAnimByCombinedTex(MeshSkinData skinData, int attachId)
        {
            int totalNum = skinData.arrayLength; //attach num
            //int maxVertex = skinData.maxVertexCount;

            if (attachId >= totalNum)
            {
                UnityEngine.Debug.LogError("over the attach size");
                return;
            }

            //读取8个动作数据
            for (int i = 0; i < 8; i++)
            {
                string animationName = g_defaultAnimationArray[i];
                if (!animationName.StartsWith("Wait") && !skinData.isLoadAllInMainScene)
                {
                    continue;
                }

                MeshSkinData.ClipData clip = skinData.meshes[attachId].clips[i];

                int frameCount = clip.frames.Length;
                if (frameCount == 0)
                {
                    continue;
                }

                Vector3[][] animData = new Vector3[frameCount][];

                //每个动作顶点
                for (int curFrame = 0; curFrame < frameCount; curFrame++)
                {
                    animData[curFrame] = clip.frames[curFrame].vertexs;
                }

                Dictionary<string, MeshAnimationData> animationDataDict;
                bool hasDict = AnimationsWithAttach.TryGetValue(attachId, out animationDataDict);
                if (!hasDict)
                {
                    animationDataDict = new Dictionary<string, MeshAnimationData>();
                    AnimationsWithAttach.Add(attachId, animationDataDict);
                }

                animationDataDict.Add(animationName,
                                      new MeshAnimationData(animationName, animData, skinData.GenerateNormal));
            }
        }

        /// <summary>
        /// Return the data as an instance of mesh animation group.
        /// </summary>
        /// <returns>Mesh animation group of this data instance</returns>
        public MeshAnimationGroup GetAnimationGroup()
        {
            return new MeshAnimationGroup(this);
        }

        public void Destory()
        {
            Triangles = null;
            Uv = null;

            if (null != AnimationsWithAttach)
            {
                var itor = AnimationsWithAttach.GetEnumerator();
                while (itor.MoveNext())
                {
                    Dictionary<string, MeshAnimationData> dataDict = itor.Current.Value;
                    if (null != dataDict)
                    {
                        var dataItor = dataDict.GetEnumerator();
                        while (dataItor.MoveNext())
                        {
                            dataItor.Current.Value.Destory();
                        }

                        dataItor.Dispose();
                        dataDict.Clear();
                    }
                }

                itor.Dispose();
                AnimationsWithAttach.Clear();
                AnimationsWithAttach = null;
            }

            Animations = null;
        }
    }
}