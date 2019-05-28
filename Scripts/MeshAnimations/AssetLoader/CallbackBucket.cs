#region Namespace

using System.Collections.Generic;

#endregion

public class CallbackBucket<TKey, TValue>
{
    private Dictionary<TKey, List<TValue>> m_callbacks;

    public CallbackBucket()
    {
        m_callbacks = new Dictionary<TKey, List<TValue>>();
    }

    public bool ContainsKey(TKey pKey)
    {
        return m_callbacks.ContainsKey(pKey);
    }

    public void Add(TKey pKey, TValue pValue)
    {
        if (!m_callbacks.ContainsKey(pKey) || m_callbacks[pKey] == null)
        {
            m_callbacks[pKey] = new List<TValue>();
        }

        m_callbacks[pKey].Add(pValue);
    }

    public int Count(TKey pKey)
    {
        if (!m_callbacks.ContainsKey(pKey) || m_callbacks[pKey] == null)
        {
            return 0;
        }

        return m_callbacks[pKey].Count;
    }

    public void Clear(TKey pKey)
    {
        if (m_callbacks.ContainsKey(pKey) && m_callbacks[pKey] != null)
        {
            m_callbacks[pKey].Clear();
        }
    }

    public List<TValue> Get(TKey pKey)
    {
        if (m_callbacks.ContainsKey(pKey) && m_callbacks[pKey] != null)
        {
            return m_callbacks[pKey];
        }

        return null;
    }
}