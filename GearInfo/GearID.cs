namespace GearInfo
{
    /// <summary>
    /// 盘齿工件 ID。仅 ProductID；张号由取图线程本地计数（方案4）。
    /// </summary>
    public struct GearID
    {
        public int ProductID { get; set; }
    }

    /// <summary>
    /// 盘齿组结果。OK=1 / NG=2；NG_Severe=3 占位，一期不启用。
    /// </summary>
    public enum GearResult
    {
        OK = 1,
        NG = 2,
        NG_Severe = 3
    }
}
