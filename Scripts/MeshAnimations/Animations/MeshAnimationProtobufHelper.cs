/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:29
	file base:	MeshAnimationProtobufHelper
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System.IO;

#endregion

namespace IGG.MeshAnimation.Serializer
{
    /// <summary>
    /// Helper class to serialize/deserialize into and from protobuf byte array.
    /// 
    /// MeshAnimationSerializer comes from DLL built from a separate Solution found
    /// in SVN/branches/MeshAnimationSerializer.
    /// 
    /// MeshAnimationGroupModel project describes MeshAnimationGroupSerializable and
    /// MeshAnimationSerializable for MeshAnimationSerializer.
    /// 
    /// MeshAnimationSerializer project adds MeshAnimationGroupSerializable and
    /// MeshAnimationSerializable to Protobuf's TypeModel to build the serializer
    /// DLL.  This step is required for iOS AOT compile
    /// </summary>
    public class MeshAnimationProtobufHelper
    {
        //private static MeshAnimationSerializer serializer = new MeshAnimationSerializer ();

        public static T DeserializeProtoObject<T>(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                //return (T)serializer.Deserialize (stream, null, typeof(T));
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        }

        public static void SerializeObject<T>(string filePath, T serializedObject)
        {
            using (FileStream f = new FileStream(filePath, FileMode.Create))
            {
                //serializer.Serialize(f, serializedObject);
                ProtoBuf.Serializer.Serialize(f, serializedObject);
            }
        }
    }
}