namespace GearInfo
{
    /// <summary>
    /// 【盘齿方案8-注释】盘齿型号外形尺寸（mm）。一期只读展示，不作为检测阈值。
    /// </summary>
    public class CGearProductModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double LengthMm { get; set; }

        public double WidthMm { get; set; }

        public double HeightMm { get; set; }
    }
}
