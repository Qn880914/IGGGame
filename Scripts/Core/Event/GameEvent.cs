namespace IGG.Event
{
    public delegate void EventDelegate<T>(T e) where T : GameEvent;

    public class GameEvent
    {
        public bool log { get; set; }

        public GameEvent()
        {
            log = false;
        }

        public void Fire()
        { }

        public virtual void LogEvent()
        {
            if(log)
            {
                UnityEngine.Debug.Log("Event Dispathced : " + this.GetType().Name);
            }
        }
    }
}
