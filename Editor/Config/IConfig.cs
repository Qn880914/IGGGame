namespace IGG.Game.Data.Config
{
    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.13
    /// Desc    配置文件类型
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IConfig<TKey>
    {
        TKey GetKey();
    }
}