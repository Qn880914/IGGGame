#region Namespace

using System;

#endregion

public class LzmaCompressRequest
{
    private byte[] m_result;

    public bool IsDone { get; private set; }

    public string Error { get; private set; }

    public float Progress { get; private set; }

    public byte[] Bytes
    {
        get { return m_result; }
    }

    public void Dispose()
    {
    }

    private void OnDone(LoomBase param)
    {
        IsDone = true;
    }

    public void Compress(byte[] data)
    {
        Loom.RunAsync(new LoomBase(), delegate(LoomBase param)
        {
            try
            {
                m_result = new byte[1];
                int size = LzmaHelper.Compress(data, ref m_result);
                if (size == 0)
                {
                    Error = "Compress Failed";
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
            finally
            {
                Loom.QueueOnMainThread(param, OnDone);
            }
        });
    }

    public void Decompress(byte[] data)
    {
        Loom.RunAsync(new LoomBase(), delegate(LoomBase param)
        {
            try
            {
                m_result = new byte[1];
                int size = LzmaHelper.Uncompress(data, ref m_result);
                if (size == 0)
                {
                    Error = "Compress Failed";
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
            finally
            {
                Loom.QueueOnMainThread(param, OnDone);
            }
        });
    }

    public static LzmaCompressRequest CreateCompress(byte[] data)
    {
        LzmaCompressRequest request = new LzmaCompressRequest();
        request.Compress(data);

        return request;
    }

    public static LzmaCompressRequest CreateDecompress(byte[] data)
    {
        LzmaCompressRequest request = new LzmaCompressRequest();
        request.Decompress(data);

        return request;
    }
}