namespace IGG.Game.Data.Config
{
    /// <summary>
    /// Author  gaofan
    /// Date    2018.1.2
    /// Desc    配置文件读取器
    /// /// </summary>
    public interface ICfgReader
    {
        /// <summary>
        /// 是否加载成功
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool Load(string path);

        uint RowCount { get; }
        uint ColCount { get; }
        string[] Heads { get; }

        IRowReader GetRow(uint index);
        string GetValue(uint rowIndex,uint colIndex);
        string GetValue(uint rowIndex, string colName);
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2018.1.2
    /// Desc    配置文件行读取器
    /// </summary>
    public interface IRowReader
    {
        uint Index { get; }
        uint Count { get; }
        string Get(uint colIndex);
        string Get(string colName);
    }
}