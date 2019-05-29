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
            Type assetType = param as Type;
            if (assetType == null)
            {
                assetType = typeof(Object);
            }

            if (async)
            {
                m_Data = AssetDatabase.LoadAssetAtPath(path, assetType);
            }
            else
            {
                Object asset = AssetDatabase.LoadAssetAtPath(path, assetType);
                OnComplete(asset);
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