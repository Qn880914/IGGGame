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
    ///     <para> 普通资源加载器(编辑器专用) </para>
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
				OnLoadCompleted(null);
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