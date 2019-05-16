#region Namespace

using SimpleJSON;
using UnityEngine;

#endregion

namespace IGG.Game
{
    public class ConfigData : ScriptableObject
    {
        // ----------------------------------------------------------------------------------------------
        // 应用名
        public string GameName = "Brave Conquest";

        // 版本类型(测试包/正式包)
        public ReleaseType Release = ReleaseType.TRUNK;

        // 打包版本号,目前是Jenkins的BuildID,不使用Jenkins时为0
        public uint BuildId = 0;

        // Svn版本-表格
        public uint RevisionConfig;

        // Svn版本-Game
        public uint RevisionGame;

        // Svn版本-工程
        public uint RevisionProject;

        // ----------------------------------------------------------------------------------------------
        // 开启补丁
        public bool EnablePatch;
        // 是否在中国大陆运营
        public bool ChinaMainland;
        // 送审版本
        public bool Censorship;

        // ----------------------------------------------------------------------------------------------
        public static ConfigData Inst { get; private set; }

        public void Init()
        {
            Inst = this;
        }

        public void InitFromJson(JSONClass json)
        {
            if (json == null)
            {
                return;
            }

            BuildId = json["build"].AsUInt;
            RevisionConfig = json["config"].AsUInt;
            RevisionGame = json["game"].AsUInt;
            RevisionProject = json["project"].AsUInt;

            ConstantData.ResetFullVersion();
        }
    }
}