using IGG.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.StateMachine
{
    public class StateMachine<KT, OT> : IStateMachine<KT, OT>
    {
        private OT m_Owner;

        private KT m_NowKey;

        private IState<KT, OT> m_NowState;

        private List<Pair<KT, IState<KT, OT>>> m_PairList = new List<Pair<KT, IState<KT, OT>>>();

        public OT owner { get { return m_Owner; } set { m_Owner = value; } }

        private void Assert(bool condition)
        {
            if (!condition)
            {
                throw new System.Exception();
            }
        }

        public KT GetCurrentState()
        {
            return m_NowKey;
        }

        public IState<KT, OT> GetState()
        {
            return m_NowState;
        }

        public void AddState(KT keyType, IState<KT, OT> state)
        {
            bool isSearched = false;
            foreach(var pair in m_PairList)
            {
                if(pair.first.Equals(keyType))
                {
                    pair.second = state;
                    isSearched = true;
                    break;
                }
            }

            if(!isSearched)
            {
                Pair<KT, IState<KT, OT>> pair = new Pair<KT, IState<KT, OT>>(keyType, state);
                m_PairList.Add(pair);
            }

            state.SetCurrentStateMachine(this);
        }

        public void  RemoveState(KT keyType)
        {
            Pair<KT, IState<KT, OT>> remove = null;
            foreach (var pair in m_PairList)
            {
                if (pair.first.Equals(keyType))
                {
                    remove = pair;
                    break;
                }
            }

            Assert(null != remove);

            m_PairList.Remove(remove);
        }

        public void SetState(KT keyType)
        {
            bool isSearched = false;

            foreach (var pair in m_PairList)
            {
                if (pair.first.Equals(keyType))
                {
                    if (null != m_NowState)
                    {
                        m_NowState.OnExit();
                    }

                    m_NowKey = keyType;
                    m_NowState = pair.second;
                    isSearched = true;
                    break;
                }
            }

            Assert(isSearched);

            if (null != m_NowState)
            {
                m_NowState.OnEnter();
            }
        }

        public void ModifyState(KT keyType)
        {
            bool isResearched = false;
            foreach (var pair in m_PairList)
            {
                if (pair.first.Equals(keyType))
                {
                    m_NowKey = keyType;
                    m_NowState = pair.second;
                    isResearched = true;
                    break;
                }
            }

            Assert(isResearched);
        }

        public class StateMachineMono<KT> : StateMachine<KT, MonoBehaviour> { }
    }
}
