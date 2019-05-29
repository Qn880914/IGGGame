using IGG.Utility;
using System;
using System.Collections.Generic;

namespace IGG.Event
{
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<Type, Delegate> m_DicDelegates = new Dictionary<Type, Delegate>();

        private static readonly object lock_helper = new object();

        public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (m_DicDelegates.TryGetValue(typeof(T), out Delegate tempDel))
                tempDel = System.Delegate.Combine(tempDel, del);
            else
                tempDel = del;

            m_DicDelegates[typeof(T)] = tempDel;
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (m_DicDelegates.TryGetValue(typeof(T), out Delegate currentDel))
            {
                currentDel = System.Delegate.Remove(currentDel, del);
                if (null == currentDel)
                    m_DicDelegates.Remove(typeof(T));
                else
                    m_DicDelegates[typeof(T)] = currentDel;
            }
        }

        public void RemoveAllListener<T>() where T : GameEvent
        {
            m_DicDelegates.Remove(typeof(T));
        }

        public void DispatchEvent<T>(T evt) where T : GameEvent
        {
            if (m_DicDelegates.TryGetValue(typeof(T), out Delegate del))
            {
                evt.LogEvent();
                del.DynamicInvoke(evt);
            }
        }

        public void ClearListener()
        {
            m_DicDelegates.Clear();
        }
    }
}
