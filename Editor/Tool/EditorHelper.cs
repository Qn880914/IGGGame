using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace IGG.EditorTools
{
    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.13
    /// Desc    编辑器工具可能会用到的各种工具
    /// </summary>
    public static class EditorHelper
    {
        /// <summary>
        /// 执行批处理文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="param"></param>
        /// <param name="openFolder"></param>
        public static void DoBat(string path, string param = null, string openFolder = null)
        {
            try
            {
                if (string.IsNullOrEmpty(param))
                {
                    Process.Start(GetProjPath(path));
                }
                else
                {
                    Process.Start(GetProjPath(path),param);
                }

                if (openFolder != null)
                {
                    OpenFileOrFolder(GetProjPath(openFolder));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 打开文件或文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFileOrFolder(string path)
        {
            Process.Start("explorer.exe", path.Replace("/", "\\"));
        }

        /// <summary>
        /// 得到项目绝对路径
        /// eg:
        /// GetProjPath("") //out: "E:/project/igg/col3/UnityProjectWithDll"
        /// GetProjPath("Assets") //out: "E:/project/igg/col3/UnityProjectWithDll/Assets"
        /// </summary>
        /// <returns></returns>
        public static string GetProjPath(string relativePath = "")
        {
            if (relativePath == null)
            {
                relativePath = "";
            }

            relativePath = relativePath.Trim();
            if (!string.IsNullOrEmpty(relativePath))
            {
                if (relativePath.Contains("\\"))
                {
                    relativePath = relativePath.Replace("\\", "/");
                }

                if (!relativePath.StartsWith("/"))
                {
                    relativePath = "/" + relativePath;
                }
            }

            string projFolder = Application.dataPath;
            return projFolder.Substring(0, projFolder.Length - 7) + relativePath;
        }

        /// <summary>
        /// 得到实现了某个接口的全部类型
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static Type[] GetAllTypeByInterface(Type interfaceType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                return a.GetTypes().Where(t =>
                {
                    return t.GetInterfaces().Contains(interfaceType);
                });
            }).ToArray();
        }

		public static string PlatformName
		{
			get
			{
				string platformName = "";
				switch (EditorUserBuildSettings.activeBuildTarget)
				{
					case BuildTarget.Android:
						platformName = "Android";
						break;
					case BuildTarget.iOS:
						platformName = "iOS";
						break;
					default:
						platformName = "Windows";
						break;
				}

				return platformName;
			}
		}

        // ab及平台工程输出目录
        public static string OutputDir
        {
            get
            {
                return "output";
            }
        }

        // 安装包输出目录
        public static string PackageDir
        {
            get
            {
                return string.Format("../{0}/package", PlayerSettings.productName);
            }
        }

		public static string assetBundleDir
		{
			get
			{
				return string.Format("{0}/ab/{1}/", OutputDir, ConstantData.MainVersion);
			}
		}

		public static string PatchDir
		{
			get
			{
				return string.Format("{0}/patch/{1}/", OutputDir, ConstantData.MainVersion);
			}
		}
	}
}
