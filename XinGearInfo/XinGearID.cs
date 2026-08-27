namespace XinGearInfo
{
    /// <summary>
    /// 【新兴盘齿方案2-注释】工件 ID。仅 ProductID；张号由取图线程本地计数（方案4 / 停点 E）。
    /// </summary>
    public struct XinGearID
    {
        public int ProductID { get; set; }
    }

    /// <summary>
    /// 【新兴盘齿方案2-注释】组结果。OK=1 / NG=2。一期不用 NG_Severe / 缺陷位图。
    /// </summary>
    public enum XinGearResult
    {
        OK = 1,
        NG = 2
    }
}
