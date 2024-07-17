namespace CommunicationModule
{
    /// <summary>
    /// 连接设备类型
    /// </summary>
    public enum MACHINETYPE
    {
        MaxWell,
        FolungWin,
        Halm,
        Berger,
        Beccni,
        TheWay
    }

    /// <summary>
    /// 转码格式
    /// </summary>
    public enum ENCODING
    {
        /// <summary>
        /// 不转码
        /// </summary>
        NONE,
        ASCII,
        UTF8,
        Unicode,
        Default,
    }

    public enum SYSCONVERT
    {
        十进制,
        十六进制
    }
}
