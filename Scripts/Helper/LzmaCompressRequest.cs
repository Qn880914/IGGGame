#region Namespace

using System;

#endregion

public class LzmaCompressRequest
{
    private byte[] m_Bytes;
    public byte[] bytes { get { return m_Bytes; } }

    public bool isDone { get; private set; }

    public string error { get; private set; }

    public float progress { get; private set; }

    public void Dispose()
    {
    }

    private void OnDone(LoomBase param)
    {
        isDone = true;
    }

    public void Compress(byte[] data)
    {
        Loom.RunAsync(new LoomBase(), delegate (LoomBase param)
        {
            try
            {
                m_Bytes = new byte[1];
                int size = LzmaHelper.Compress(data, ref m_Bytes);
                if (size == 0)
                {
                    error = "Compress Failed";
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            finally
            {
                Loom.QueueOnMainThread(param, OnDone);
            }
        });
    }

    public void Decompress(byte[] data)
    {
        Loom.RunAsync(new LoomBase(), delegate (LoomBase param)
        {
            try
            {
                m_Bytes = new byte[1];
                int size = LzmaHelper.Decompress(data, ref m_Bytes);
                if (size == 0)
                {
                    error = "Compress Failed";
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            finally
            {
                Loom.QueueOnMainThread(param, OnDone);
            }
        });
    }

    public static LzmaCompressRequest CreateCompress(byte[] bytes)
    {
        LzmaCompressRequest request = new LzmaCompressRequest();
        request.Compress(bytes);

        return request;
    }

    public static LzmaCompressRequest CreateDecompress(byte[] bytes)
    {
        LzmaCompressRequest request = new LzmaCompressRequest();
        request.Decompress(bytes);

        return request;
    }
}