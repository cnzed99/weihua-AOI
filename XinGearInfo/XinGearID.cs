namespace XinGearInfo
{
    /// <summary>
    /// 工件 ID。仅 ProductID；张号由取图线程本地计数。
    /// </summary>
    public struct XinGearID
    {
        public int ProductID { get; set; }
    }

    /// <summary>
    /// 组结果。OK=1 / NG=2。
    /// </summary>
    public enum XinGearResult
    {
        OK = 1,
        NG = 2
    }
}
