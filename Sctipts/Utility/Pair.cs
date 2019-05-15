namespace IGG.Utility
{
    public class Pair<KT, OT>
    {
        private KT m_First;

        private OT m_Second;

        public KT first { get { return m_First; } set { m_First = value; } }

        public OT second { get { return m_Second; } set { m_Second = value; } }

        public Pair() { }

        public Pair(KT first, OT second)
        {
            m_First = first;
            m_Second = second;
        }
    }
}
