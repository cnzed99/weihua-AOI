namespace CrankInfo
{
    /// <summary>
    /// 【曲轴方案2-注释】曲轴工件 ID。仅 ProductID；张号由取图线程本地计数（方案4 / 停点 E）。
    /// </summary>
    public struct CrankID
    {
        public int ProductID { get; set; }
    }

    /// <summary>
    /// 【曲轴方案2-注释】曲轴组结果。OK=1 / NG=2。一期不用 NG_Severe / 缺陷位图。
    /// </summary>
    public enum CrankResult
    {
        OK = 1,
        NG = 2
    }
}
