#region Namespace

using System;
using UnityEngine;

#endregion

namespace IGG.Game
{
    public class VersionData : ScriptableObject
    {
        public VersionItem[] Items;

        // ----------------------------------------------------------------------------------------------
        public static VersionData Inst { get; private set; }

        public void Init()
        {
            Inst = this;
        }

        // ----------------------------------------------------------------------------------------------
        [Serializable]
        public class VersionItem
        {
            public string Md5;
            public string Name;
            public long Size;
        }
    }
}