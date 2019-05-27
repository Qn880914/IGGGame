#region Namespace

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#endregion

// <summary>
// 文件相关通用接口
// </summary>
// <author>zhulin</author>

namespace IGG
{
    public class FileUtil
    {
        // 拷贝文件夹
        public delegate bool CopyFilter(string file);

        public static Hash128 DefaultHash = new Hash128(0, 0, 0, 0);
        public static string DefaultHashString = DefaultHash.ToString();

        // <summary>
        // 确认文件是否存在
        // </summary>
        public static bool CheckFileExist(string path)
        {
            return File.Exists(path);
        }

        // <summary>
        // 创建文件夹
        // </summary>
        public static void CreateDirectory(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        ///     <para> 清空文件夹 </para>
        /// </summary>
        /// <param name="path">Path.</param>
        public static void ClearDirectory(string path)
        {
            CreateDirectory(path);
            List<string> lfile = new List<string>();

            DirectoryInfo rootDirInfo = new DirectoryInfo(path);
            foreach (FileInfo file in rootDirInfo.GetFiles())
            {
                lfile.Add(file.FullName);
            }

            foreach (string fileName in lfile)
            {
                File.Delete(fileName);
            }

            DirectoryInfo rootDic = new DirectoryInfo(path);
            foreach (DirectoryInfo fileDic in rootDic.GetDirectories())
            {
                DeleteFileDirectory(fileDic.FullName);
            }
        }

        public static void CopyDirectory(string sourcePath, string destinationPath, string suffix = "",
            CopyFilter onFilter = null)
        {
            if (onFilter != null && onFilter(sourcePath))
            {
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (string file in Directory.GetFileSystemEntries(sourcePath))
            {
                if (File.Exists(file))
                {
                    FileInfo info = new FileInfo(file);
                    if (string.IsNullOrEmpty(suffix) || file.ToLower().EndsWith(suffix.ToLower()))
                    {
                        string destName = Path.Combine(destinationPath, info.Name);
                        if (!(onFilter != null && onFilter(file)))
                        {
                            File.Copy(file, destName);
                        }
                    }
                }

                if (Directory.Exists(file))
                {
                    DirectoryInfo info = new DirectoryInfo(file);
                    string destName = Path.Combine(destinationPath, info.Name);
                    CopyDirectory(file, destName, suffix, onFilter);
                }
            }
        }

        // <summary>
        // 删除文件夹
        // </summary>
        public static void DeleteFileDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo rootDirInfo = new DirectoryInfo(path);
                rootDirInfo.Delete(true);
            }
        }

        // 删除文件
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        // 拷贝文件
        public static void CopyFile(string pathFrom, string pathTo)
        {
            if (!File.Exists(pathFrom))
            {
                return;
            }

            DeleteFile(pathTo);

            CreateDirectoryFromFile(pathTo);
            File.Copy(pathFrom, pathTo);
        }

        // 根据文件名创建文件所在的目录
        public static void CreateDirectoryFromFile(string path)
        {
            path = path.Replace("\\", "/");
            int index = path.LastIndexOf("/", StringComparison.Ordinal);
            string dir = path.Substring(0, index);

            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("目录创建失败，" + e.Message);
            }
        }

        // 获取指定文件下的所有文件
        public static List<string> GetAllChildFiles(string path, string suffix = null, List<string> files = null)
        {
            if (files == null)
            {
                files = new List<string>();
            }

            if (!Directory.Exists(path))
            {
                return files;
            }

            AddFiles(path, suffix, files);

            string[] temps = Directory.GetDirectories(path);
            for (int i = 0; i < temps.Length; ++i)
            {
                string dir = temps[i];
                GetAllChildFiles(dir, suffix, files);
            }

            return files;
        }

        private static void AddFiles(string path, string suffix, List<string> files)
        {
            string[] temps = Directory.GetFiles(path);
            for (int i = 0; i < temps.Length; ++i)
            {
                string file = temps[i];
                if (string.IsNullOrEmpty(suffix) || file.ToLower().EndsWith(suffix.ToLower()))
                {
                    files.Add(file);
                }
            }
        }

        public static bool IsFileInPackage(string fullpath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (fullpath.Contains(Application.streamingAssetsPath))
            {
                fullpath = fullpath.Replace(Application.streamingAssetsPath + "/", "");
                return AndroidHelper.FileHelper.CallStatic<bool>("IsAssetExist", fullpath);;
            }
#endif

            return File.Exists(fullpath);
        }

        // 拷贝文件(Android)
        public static bool CopyAndroidAssetFile(string pathSrc, string pathDst)
        {
            bool ret = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            pathSrc = pathSrc.Replace(Application.streamingAssetsPath + "/", "");
            ret = AndroidHelper.FileHelper.CallStatic<bool>("CopyFileTo", pathSrc, pathDst);
#endif

            return ret;
        }

        // 保存字节流到文件
        public static bool SaveBytesToFile(byte[] bytes, string path)
        {
            CreateDirectoryFromFile(path);

            try
            {
                Stream stream = File.Open(path, FileMode.Create);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                return false;
            }
        }

        // 保存文件到文件
        public static bool SaveTextToFile(string text, string path)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return SaveBytesToFile(bytes, path);
        }

        // 以字节流方式读取整个文件
        public static byte[] ReadByteFromFile(string path)
        {
            byte[] bytes = null;

            bool useFileReader = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            // 如果是读apk包里的资源,使用Android帮助库加载(目前还没有用到,如果需要,要实现一下java代码,暂时保留)
            if (path.Contains(Application.streamingAssetsPath))
            {
                useFileReader = false;

                path = path.Replace(Application.streamingAssetsPath + "/", "");
                bytes = AndroidHelper.FileHelper.CallStatic<byte[]>("LoadFile", path);
            }
#endif
            if (useFileReader && File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }

            return bytes;
        }

        // 以文本方式读取整个文件
        public static string ReadTextFromFile(string path)
        {
            string text = "";

            byte[] bytes = ReadByteFromFile(path);
            if (bytes != null)
            {
                text = Encoding.UTF8.GetString(bytes);
            }

            return text;
        }

        public static string GetMd5(string source)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            return CalcMd5StringFromHash(data);
        }

        private static string CalcMd5StringFromHash(byte[] bytes)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                str.Append(bytes[i].ToString("x2"));
            }

            return str.ToString();
        }

        public static string CalcFileMd5(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            FileStream stream = File.OpenRead(path);

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(stream);
            stream.Close();

            return CalcMd5StringFromHash(result);
        }

        public static string GetCacheAssetBundlePath(string filename)
        {
            return string.Format("{0}/{1}/{2}/__data", ConstantData.unpackPath, filename, DefaultHashString);
        }

        public static string GetAssetBundleFullPath(string path)
        {
#if UNITY_IOS || UNITY_EDITOR_OSX
            return string.Format("file:///{0}", path);
#else
            return path;
#endif
        }
    }
}