using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IGG.Utility
{
    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionGet;
        private readonly UnityAction<T> m_ActionRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        private int countInactive { get { return m_Stack.Count; } }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionRelease)
        {
            m_ActionGet = actionOnGet;
            m_ActionRelease = actionRelease;
        }

        public T Get()
        {
            T element;
            if (0 == m_Stack.Count)
            {
                element = new T();
                ++countAll;
            }
            else
            {
                element = m_Stack.Pop();
            }

            m_ActionGet?.Invoke(element);

            return element;
        }

        public void Release(T element)
        {
            if(m_Stack.Count > 0 && object.ReferenceEquals(m_Stack.Peek(), element))
            {
                Debug.LogError("Internal error, Trying to destroy object that is already released to pool.");
            }

            m_ActionRelease?.Invoke(element);

            this.m_Stack.Push(element);
        }
    }
}
