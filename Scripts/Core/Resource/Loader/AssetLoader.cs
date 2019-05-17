#region Namespace

using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

namespace IGG.Core.Resource
{
    /// <summary>
    ///     <para> normal resources loader </para>
    ///   Note :  only use for Editor 
    /// </summary>
    public class AssetLoader : Loader
    {
        private Object m_Data;

        public AssetLoader() 
            : base(LoaderType.Asset)
        {
        }

        public override void Start()
        {
            base.Start();

#if UNITY_EDITOR
            Type type = param as Type;
            if (type == null)
            {
                type = typeof(Object);
            }

            if (async)
            {
                m_Data = AssetDatabase.LoadAssetAtPath(path, type);
            }
            else
            {
                Object data = AssetDatabase.LoadAssetAtPath(path, type);
                OnComplete(data);
            }
#else
			if(!async)
			{
				OnFailed();
			}
#endif
        }

        public override void Update()
        {
            if (state != LoaderState.Loading)
                return;

            OnComplete(m_Data);
            m_Data = null;
        }
    }
}