#if UNITY_EDITOR
#define DEBUG
#endif

#region Namespace

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#endregion

//using IGG.CCMafia.Network;

namespace IGG
{
    public static class Util
    {
        public delegate bool ChildLoppCallBack(Transform t);

        /// <summary>
        /// 计算相机视锥体在高为seaLevel的XZ平面上的投射范围
        /// </summary>
        /// <returns></returns>
        public static Rect CalculateViewRange(Camera camera, float fovAdd, float seaLevel)
        {
            Vector3 p1, p2, p3, p4;
            CalculateFrustumCrossSeaLevel(camera, fovAdd, seaLevel, out p1, out p2, out p3, out p4);
            Rect viewRange = new Rect();
            viewRange.min = Vector2.one * 99999;
            viewRange.max = Vector2.one * -99999;
            viewRange = viewRange.Extern(new Vector2(p1.x, p1.z));
            viewRange = viewRange.Extern(new Vector2(p2.x, p2.z));
            viewRange = viewRange.Extern(new Vector2(p3.x, p3.z));
            viewRange = viewRange.Extern(new Vector2(p4.x, p4.z));

            return viewRange;
        }

        /// <summary>
        /// 计算相机的视锥体和平面的四个交点
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="fovAdd">相机fov扩大多少，不需要则设置为0</param>
        /// <param name="seaLevel">平面高度，默认平面是XZ平面</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public static void CalculateFrustumCrossSeaLevel(Camera camera, float fovAdd, float seaLevel, out Vector3 p1,
                                                         out Vector3 p2, out Vector3 p3, out Vector3 p4)
        {
            p1 = Vector3.zero;
            p2 = Vector3.zero;
            p3 = Vector3.zero;
            p4 = Vector3.zero;

            if (null == camera)
            {
                return;
            }

            float tanHalfFov = Mathf.Tan(Mathf.Deg2Rad * (camera.fieldOfView + fovAdd) * 0.5f);
            float halfHeight = tanHalfFov * camera.farClipPlane;
            float halfWidth = halfHeight * camera.pixelWidth / camera.pixelHeight;
            Vector3 cameraUpLeftPoint = new Vector3(-halfWidth, halfHeight, camera.farClipPlane);
            Vector3 cameraUpRightPoint = new Vector3(halfWidth, halfHeight, camera.farClipPlane);
            Vector3 cameraDownLeftPoint = new Vector3(-halfWidth, -halfHeight, camera.farClipPlane);
            Vector3 cameraDownRightPoint = new Vector3(halfWidth, -halfHeight, camera.farClipPlane);

            Plane seaPlane = new Plane(Vector3.up, seaLevel);
            //屏幕四个角发出的射线(这个方法太慢)
            //Ray r1 = ViewCamera.ScreenPointToRay(new Vector3(0, Screen.height));
            //Ray r2 = ViewCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height));
            //Ray r3 = ViewCamera.ScreenPointToRay(Vector3.zero);
            //Ray r4 = ViewCamera.ScreenPointToRay(new Vector3(Screen.width, 0));

            Vector3 worldUpLeftVector = camera.transform.TransformPoint(cameraUpLeftPoint) - camera.transform.position;
            Vector3 worldUpRightVector =
                camera.transform.TransformPoint(cameraUpRightPoint) - camera.transform.position;
            Vector3 worldDownLeftVector =
                camera.transform.TransformPoint(cameraDownLeftPoint) - camera.transform.position;
            Vector3 worldDownRightVector =
                camera.transform.TransformPoint(cameraDownRightPoint) - camera.transform.position;

            Ray r1 = new Ray(camera.transform.position, worldUpLeftVector);
            Ray r2 = new Ray(camera.transform.position, worldUpRightVector);
            Ray r3 = new Ray(camera.transform.position, worldDownLeftVector);
            Ray r4 = new Ray(camera.transform.position, worldDownRightVector);

            float d1, d2, d3, d4;
            if (seaPlane.Raycast(r1, out d1))
            {
                p1 = r1.direction * d1 + camera.transform.position;
            }

            if (seaPlane.Raycast(r2, out d2))
            {
                p2 = r2.direction * d2 + camera.transform.position;
            }

            if (seaPlane.Raycast(r3, out d3))
            {
                p3 = r3.direction * d3 + camera.transform.position;
            }

            if (seaPlane.Raycast(r4, out d4))
            {
                p4 = r4.direction * d4 + camera.transform.position;
            }
        }

        /// <summary>
        /// 检测2个碰撞体是否发生碰撞
        /// </summary>  
        public static bool CheckCollider(Rect rc1, Rect rc2)
        {
            double disx = rc1.x + rc1.width * 0.5f - (rc2.x + rc2.width * 0.5f);
            disx = disx < 0 ? -disx : disx;


            double disy = rc1.y + rc1.height * 0.5f - (rc2.y + rc2.height * 0.5f);
            disy = disy < 0 ? -disy : disy;


            double limtX = rc1.width * 0.5f + rc2.width * 0.5f;
            double limtY = rc1.height * 0.5f + rc2.height * 0.5f;


            if (disx < limtX && disy < limtY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a deep search for the provided name.
        /// </summary>
        /// <param name="trm">The transform to search through</param>
        /// <param name="name">The name of the transform object to search for.</param>
        /// <returns></returns>
        public static Transform FindChildRecursive(this Transform trm, string name)
        {
            Transform child = null;

            // Loop through top level
            foreach (Transform t in trm)
            {
                if (t.name == name)
                {
                    child = t;
                    return child;
                }
                else if (t.childCount > 0)
                {
                    child = t.FindChildRecursive(name);
                    if (child)
                    {
                        return child;
                    }
                }
            }

            return child;
        }

        public static bool Intersect(float startX, float startY, float endX, float endY,
                                     float otherStartX, float otherStartY, float otherEndX, float otherEndY)
        {
            if (Mathf.Max(startX, endX) < Mathf.Min(otherStartX, otherEndX))
            {
                return false;
            }

            if (Mathf.Max(startY, endY) < Mathf.Min(otherStartY, otherEndY))
            {
                return false;
            }

            if (Mathf.Max(otherStartX, otherEndX) < Mathf.Min(startX, endX))
            {
                return false;
            }

            if (Mathf.Max(otherStartY, otherEndY) < Mathf.Min(startY, endY))
            {
                return false;
            }

            if (Mult(otherStartX, otherStartY, endX, endY, startX, startY) *
                Mult(endX, endY, otherEndX, otherEndY, startX, startY) < 0)
            {
                return false;
            }

            if (Mult(startX, startY, otherEndX, otherEndY, otherStartX, otherStartY) *
                Mult(otherEndX, otherEndY, endX, endY, otherStartX, otherStartY) < 0)
            {
                return false;
            }

            return true;
        }

        public static double Mult(float aX, float aY, float bX, float bY,
                                  float cX, float cY)
        {
            return (aX - cX) * (bY - cY) - (bX - cX) * (aY - cY);
        }


        public static bool CheckCrose(float startX, float startY, float endX, float endY,
                                      float otherStartX, float otherStartY, float otherEndX, float otherEndY)
        {
            float x1 = otherStartX - endX;
            float y1 = otherStartY - endY;

            float x2 = otherEndX - endX;
            float y2 = otherEndY - endY;

            float x3 = startX - endX;
            float y3 = startY - endY;

            return CrossMul(x1, y1, x3, y3) * CrossMul(x2, y2, x3, y3) <= 0;
        }

        public static float CrossMul(float x1, float y1, float x2, float y2)
        {
            return x1 * y2 - y1 * x2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trm">The transform to search through</param>
        /// <param name="callBack">call every child, return false to stop loop its children</param>
        public static void LoopChildWitchCallBackRecursive(Transform trm, ChildLoppCallBack callBack)
        {
            // Loop through top level
            foreach (Transform t in trm)
            {
                bool continueLoop = true;
                if (null != callBack)
                {
                    continueLoop = callBack.Invoke(t);
                }

                if (t.childCount > 0 && continueLoop)
                {
                    LoopChildWitchCallBackRecursive(t, callBack);
                }
            }
        }
    }

    public static class FilePaths
    {
        public const string MetaFile = ".meta";

        // Replacement for Path.Combine(string,string,string,...).  
        // Because the overloads are not implemented in Unity/Mono BCL
        public static string Combine(string pPath, params string[] pSegments)
        {
            foreach (var segment in pSegments)
            {
                pPath = Path.Combine(pPath, segment);
            }

            return pPath;
        }

        public static void DeletePaths(string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return;
            }

            foreach (string path in paths)
            {
                DeletePath(path);
            }
        }

        public static void DeletePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Is dir?
            if (Directory.Exists(path) &&
                (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DeleteDirectory(path);
            }
            else if (File.Exists(path))
            {
                DeleteFile(path);
            }
        }

        private static void DeleteDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string file in files)
            {
                DeleteFile(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

        public static void CreateFile(string path, string content, Encoding encoding)
        {
            if (encoding == null)
            {
                return;
            }

            using (FileStream fs = File.Create(path))
            {
                byte[] info = encoding.GetBytes(content);
                fs.Write(info, 0, info.Length);
            }
        }

        public static string[] GetFilePaths(string path, bool includeMetaFile)
        {
            string[] paths = null;

            string lastDirOrFileName = GetLastDirectoryOrFileName(path);
            if (lastDirOrFileName.Contains("*"))
            {
                string dirName = Path.GetDirectoryName(path);

                if (Directory.Exists(dirName))
                {
                    // Is file or dir?
                    if (lastDirOrFileName.Contains("."))
                    {
                        paths = Directory.GetFiles(dirName, lastDirOrFileName);
                    }
                    else
                    {
                        paths = Directory.GetDirectories(dirName, lastDirOrFileName);
                    }
                }
            }
            else
            {
                if (Directory.Exists(path) || File.Exists(path))
                {
                    paths = new string[1]
                    {
                        path,
                    };
                }
            }

            if (includeMetaFile)
            {
                return GetFilePathIncludeMetaFile(paths);
            }

            return paths;
        }

        private static string GetLastDirectoryOrFileName(string path)
        {
            string[] dirOrFileNames = path.Split('/');
            if (dirOrFileNames.Length > 0)
            {
                return dirOrFileNames[dirOrFileNames.Length - 1];
            }

            return string.Empty;
        }

        private static string[] GetFilePathIncludeMetaFile(string[] paths)
        {
            if (paths == null)
            {
                return null;
            }

            List<string> newPaths = new List<string>();
            foreach (string path in paths)
            {
                newPaths.Add(path);
                string metaFilePath = Path.ChangeExtension(path, MetaFile);
                // Don't need to add meta file path if the original path already exists. (Meta file will be added to newPaths automatically)
                if (Path.GetExtension(path) != MetaFile && null != path &&
                    !path.Contains(metaFilePath))
                {
                    newPaths.Add(metaFilePath);
                }
            }

            return newPaths.ToArray();
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, true);
                }
            }
        }
    }
    /* 
	 * Assert and Fail procedures.
	 * 
	 */



    public static class Time
    {
        private static ITime g_internalTime = new InternalDefaultTime();

        public static ITime Default
        {
            get { return g_internalTime; }
        }

        public static int GetCurrentTimeStampSec()
        {
            return Mathf.FloorToInt(UnityEngine.Time.realtimeSinceStartup);
        }

        public static float GetTimeAtStartOfCurrentFrame()
        {
            return UnityEngine.Time.time;
        }

        // default implementation of ITime. Just return the values from unit.
        private class InternalDefaultTime : ITime
        {
            public int CurrentFrame
            {
                get { return UnityEngine.Time.frameCount; }
            }

            public float DeltaTime
            {
                get { return UnityEngine.Time.deltaTime; }
            }

            public float Time
            {
                get { return UnityEngine.Time.time; }
            }

            public float FixedDeltaTime
            {
                get { return UnityEngine.Time.fixedDeltaTime; }
            }

            public float FixedTime
            {
                get { return UnityEngine.Time.fixedTime; }
            }
        }
    } //public static class Time

    public interface ITime
    {
        int CurrentFrame { get; }
        float DeltaTime { get; }
        float Time { get; }
        float FixedTime { get; }
        float FixedDeltaTime { get; }
    }
}