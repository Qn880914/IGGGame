namespace IGG.StateMachine
{
    public abstract class State<KT, OT> : IState<KT, OT>
    {
        private IStateMachine<KT, OT> m_StateMachine;

        public IStateMachine<KT, OT> GetCurrentStateMachine() { return m_StateMachine; }

        public void SetCurrentStateMachine(IStateMachine<KT, OT> stateMachine) { m_StateMachine = stateMachine; }

        public abstract void OnEnter();

        public abstract void OnUpdate(float deltaTime);

        public abstract void OnExit();
    }
}
