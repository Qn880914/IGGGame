#region Namespace

using System;
using System.IO;
using System.Runtime.InteropServices;

#endregion

public class ZipHelper
{
#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_IOS) && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int zipGetTotalFiles(string zipArchive, IntPtr fileBuffer, int fileBufferLength);

	[DllImport("__Internal")]
	private static extern int zipCD(int levelOfCompression, string zipArchive, string inFilePath, string fileName, string comment);

	[DllImport("__Internal")]
	private static extern int zipEX(string zipArchive, string outPath, IntPtr progress, IntPtr fileBuffer, int fileBufferLength, IntPtr proc);

#else

#if (UNITY_ANDROID || UNITY_STANDALONE_LINUX) && (!UNITY_EDITOR || UNITY_EDITOR_LINUX)
	private const string g_libName = "zipw";
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WP8_1 || UNITY_WSA
    private const string g_libName = "libzipw";
#endif

    [DllImport(g_libName, EntryPoint = "zipGetTotalFiles")]
    private static extern int zipGetTotalFiles(string zipArchive, IntPtr fileBuffer, int fileBufferLength);

    [DllImport(g_libName, EntryPoint = "zipCD"
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        , CharSet = CharSet.Auto
#endif
    )]
    private static extern int zipCD(int levelOfCompression, string zipArchive, string inFilePath, string fileName,
                                    string comment);

    [DllImport(g_libName, EntryPoint = "zipEX")]
    private static extern int zipEX(string zipArchive, string outPath, IntPtr progress, IntPtr fileBuffer,
                                    int fileBufferLength, IntPtr proc);

#endif

    public static int GetTotalFiles(string inPath)
    {
        return zipGetTotalFiles(inPath, IntPtr.Zero, 0);
    }

    public static bool Unpack(string inPath, string outPath, int[] progress)
    {
        if (!outPath.EndsWith("/"))
        {
            outPath += "/";
        }

        GCHandle ibuf = GCHandle.Alloc(progress, GCHandleType.Pinned);
        int res = zipEX(inPath, outPath, ibuf.AddrOfPinnedObject(), IntPtr.Zero, 0, IntPtr.Zero);

        ibuf.Free();
        return res == 1;
    }

    public static bool PackFile(string inPath, string outPath, bool append = false, string fileName = "")
    {
        if (!File.Exists(inPath))
        {
            return false;
        }

        if (!append)
        {
            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = Path.GetFileName(inPath);
        }

        int levelOfCompression = 6;

        int res = zipCD(levelOfCompression, outPath, inPath, fileName, "");
        return res == 1;
    }

    public static void Pack(string inPath, string outPath)
    {
        inPath = inPath.Replace("\\", "/");

        if (Directory.Exists(inPath))
        {
            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }

            foreach (string file in Directory.GetFiles(inPath, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta") || file.Equals(".DS_Store"))
                {
                    continue;
                }

                string filePath = file.Replace("\\", "/");

                string fileName = filePath.Replace(inPath, "");
                if (fileName.StartsWith("/"))
                {
                    fileName = fileName.Substring(1);
                }

                PackFile(filePath, outPath, true, fileName);
            }
        }
    }
}