using UnityEngine;

namespace IGG.Singleton
{
	public class SingletonHolder : MonoBehaviour
	{
		public static Transform PermanentGameObjectParent { get; private set; }
		
		private void Awake()
		{
		    UnityEngine.Debug.LogWarning((PermanentGameObjectParent == null) + "SingletonHolder" +
		                      "2 version of the SingletonHolder script are running but this script should be unique. A program should only have 1 SingletonHolder.");
            PermanentGameObjectParent = gameObject.transform;
			DontDestroyOnLoad(gameObject);
		}
	}
}

