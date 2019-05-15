using IGG.Utility;
using System;
using System.Collections.Generic;

namespace IGG.Event
{
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<Type, Delegate> m_dicDelegates = new Dictionary<Type, Delegate>();

        private static readonly object lock_helper = new object();

        public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            System.Delegate tempDel;
            if (m_dicDelegates.TryGetValue(typeof(T), out tempDel))
                tempDel = System.Delegate.Combine(tempDel, del);
            else
                tempDel = del;

            m_dicDelegates[typeof(T)] = tempDel;
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            System.Delegate currentDel;
            if (m_dicDelegates.TryGetValue(typeof(T), out currentDel))
            {
                currentDel = System.Delegate.Remove(currentDel, del);
                if (null == currentDel)
                    m_dicDelegates.Remove(typeof(T));
                else
                    m_dicDelegates[typeof(T)] = currentDel;
            }
        }

        public void RemoveAllListener<T>() where T : GameEvent
        {
            m_dicDelegates.Remove(typeof(T));
        }

        public void DispatchEvent<T>(T evt) where T : GameEvent
        {
            System.Delegate del;
            if (m_dicDelegates.TryGetValue(typeof(T), out del))
            {
                evt.LogEvent();
                del.DynamicInvoke(evt);
            }
        }

        public void ClearListener()
        {
            m_dicDelegates.Clear();
        }
    }
}
