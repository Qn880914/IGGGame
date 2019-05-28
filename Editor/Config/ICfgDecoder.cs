namespace IGG.Game.Data.Config
{
    /// Author  gaofan
    /// Date    2017.12.13
    /// Desc    配置文件解码器的接口
    public interface ICfgDecoder
    {
        bool Enable { get; }

        string GetName();

        string GetSaveName();

        bool Decode(ICfgReader table);

        void AllDecodeAfterProcess();

        object Data { get; }
    }
}