namespace IGG.StateMachine
{
    public interface IStateMachine<KT, OT>
    {
        OT owner { get; set; }

        KT GetCurrentState();

        IState<KT, OT> GetState();

        void AddState(KT keyType, IState<KT, OT> state);

        void RemoveState(KT keyType);

        void SetState(KT keyType);

        void ModifyState(KT keyType);
    }
}
