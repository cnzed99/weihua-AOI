namespace CrankInfo
{
    /// <summary>
    /// 曲轴工件 ID。仅 ProductID；张号由取图线程本地计数
    /// </summary>
    public struct CrankID
    {
        public int ProductID { get; set; }
    }

    /// <summary>
    /// 曲轴组结果。OK=1 / NG=2
    /// </summary>
    public enum CrankResult
    {
        OK = 1,
        NG = 2
    }
}
