#region Namespace

using System;
using System.IO;
using System.Runtime.InteropServices;

#endregion

public class LzmaHelper
{
#if (UNITY_IOS || UNITY_WEBGL || UNITY_IPHONE) && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern void _releaseBuffer(IntPtr buffer);

	[DllImport("__Internal")]
	private static extern IntPtr Lzma_Compress( IntPtr buffer, int bufferLength, bool makeHeader, ref int v, IntPtr props);

	[DllImport("__Internal")]
	private static extern int Lzma_Uncompress( IntPtr buffer, int bufferLength, int uncompressedSize, IntPtr outbuffer,bool useHeader);

#else

#if (UNITY_ANDROID || UNITY_STANDALONE_LINUX) && (!UNITY_EDITOR || UNITY_EDITOR_LINUX)
	private const string g_libName = "lzma";
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    private const string g_libName = "liblzma";
#endif

    [DllImport(g_libName, EntryPoint = "_releaseBuffer")]
    private static extern void _releaseBuffer(IntPtr buffer);

    [DllImport(g_libName, EntryPoint = "Lzma_Compress")]
    private static extern IntPtr Lzma_Compress(IntPtr buffer, int bufferLength, bool makeHeader, ref int compressSize,
                                               IntPtr props);

    [DllImport(g_libName, EntryPoint = "Lzma_Uncompress")]
    private static extern int Lzma_Uncompress(IntPtr buffer, int bufferLength, int uncompressedSize, IntPtr outbuffer,
                                              bool useHeader);

#endif

    private static readonly byte[] g_outBuffer = new byte[1024];
    private static readonly int[] g_props = new int[7];
    private static bool g_defaultsSet;

    private static void SetProps(int level = 5, int dictSize = 16777216, int lc = 3, int lp = 0, int pb = 2,
                                 int fb = 32, int numThreads = 4)
    {
        g_defaultsSet = true;

        g_props[0] = level;
        g_props[1] = dictSize;
        g_props[2] = lc;
        g_props[3] = lp;
        g_props[4] = pb;
        g_props[5] = fb;
        g_props[6] = numThreads;
    }

    // 返回压缩后大小
    public static int Compress(byte[] inBuffer, ref byte[] outBuffer)
    {
        if (!g_defaultsSet)
        {
            SetProps();
        }

        GCHandle prps = GCHandle.Alloc(g_props, GCHandleType.Pinned);
        GCHandle cbuf = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);

        int compressSize = 0;
        IntPtr ptr = Lzma_Compress(cbuf.AddrOfPinnedObject(), inBuffer.Length, true, ref compressSize,
                                   prps.AddrOfPinnedObject());

        cbuf.Free();
        prps.Free();

        if (compressSize == 0 || ptr == IntPtr.Zero)
        {
            _releaseBuffer(ptr);
            return 0;
        }

        if (outBuffer.Length < compressSize)
        {
            Array.Resize(ref outBuffer, compressSize);
        }

        Marshal.Copy(ptr, outBuffer, 0, compressSize);
        _releaseBuffer(ptr);

        return compressSize;
    }

    // 返回解压后大小
    public static int Decompress(byte[] inBuffer, ref byte[] outBuffer)
    {
        int uncompressedSize = (int)BitConverter.ToUInt64(inBuffer, 5);

        if (outBuffer.Length < uncompressedSize)
        {
            Array.Resize(ref outBuffer, uncompressedSize);
        }

        GCHandle cbuf = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);
        GCHandle obuf = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);

        int ret = Lzma_Uncompress(cbuf.AddrOfPinnedObject(), inBuffer.Length, uncompressedSize,
                                  obuf.AddrOfPinnedObject(), true);

        cbuf.Free();
        obuf.Free();

        if (ret != 0)
        {
            return 0;
        }

        return uncompressedSize;
    }

    public static bool Compress(string inPath, string outPath)
    {
        byte[] inBuffer = File.ReadAllBytes(inPath);
        byte[] outBuffer = g_outBuffer;

        int size = Compress(inBuffer, ref outBuffer);
        if (size == 0)
        {
            return false;
        }

        Stream stream = File.OpenWrite(outPath);
        stream.Write(outBuffer, 0, size);
        stream.Close();

        return true;
    }

    public static bool Decompress(string inPath, string outPath)
    {
        byte[] inBuffer = File.ReadAllBytes(inPath);
        byte[] outBuffer = g_outBuffer;

        int size = Decompress(inBuffer, ref outBuffer);
        if (size == 0)
        {
            return false;
        }

        Stream outStream = File.OpenWrite(outPath);
        outStream.Write(outBuffer, 0, size);
        outStream.Close();

        return true;
    }
}