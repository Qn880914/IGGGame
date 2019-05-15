namespace IGG.Utility
{
    public class Singleton<T> : Disposable where T : Singleton<T>
    {
        private static T s_Instance;

        private static object lock_helper = new object();

        public static T instance
        {
            get
            {
                if(null == s_Instance)
                {
                    lock (lock_helper)
                    {
                        if (null == s_Instance)
                        {
                            s_Instance = System.Activator.CreateInstance<T>();
                        }
                    }
                }

                return s_Instance;
            }
        }
    }
}

