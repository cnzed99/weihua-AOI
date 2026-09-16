using System.ComponentModel;

namespace GearTestAlgorihm.Halcon
{
    /// <summary>
    /// 倒角偏 Halcon 比例/阈值
    /// </summary>
    public class ChamferOffsetParams
    {
        [Category("倒角偏")]
        [DisplayName("导出毫米")]
        public bool ExportDistAsMm { get; set; } = false;

        [Category("倒角偏")]
        [DisplayName("内圆暗阈值上限")]
        public double InnerDarkMax { get; set; } = 120;

        [Category("倒角偏")]
        [DisplayName("内圆面积下限×画面")]
        public double InnerAreaMinFrac { get; set; } = 0.015;

        [Category("倒角偏")]
        [DisplayName("内圆面积上限×画面")]
        public double InnerAreaMaxFrac { get; set; } = 0.15;

        [Category("倒角偏")]
        [DisplayName("内圆半径上限×短边")]
        public double InnerRMaxFrac { get; set; } = 0.28;

        [Category("倒角偏")]
        [DisplayName("环带内径×R0")]
        public double RingInnerMul { get; set; } = 1.43;

        [Category("倒角偏")]
        [DisplayName("环带外径×R0")]
        public double RingOuterMul { get; set; } = 2.70;

        [Category("倒角偏")]
        [DisplayName("小圆半径下限×R0")]
        public double HoleRMinMul { get; set; } = 0.17;

        [Category("倒角偏")]
        [DisplayName("小圆半径上限×R0")]
        public double HoleRMaxMul { get; set; } = 0.30;

        [Category("倒角偏")]
        [DisplayName("孔心距下限×R0")]
        public double HoleDistMinMul { get; set; } = 2.05;

        [Category("倒角偏")]
        [DisplayName("孔心距上限×R0")]
        public double HoleDistMaxMul { get; set; } = 2.75;

        [Category("倒角偏")]
        [DisplayName("沟环 opening")]
        public double ChamferOpenR { get; set; } = 1.0;

        [Category("倒角偏")]
        [DisplayName("沟环 closing")]
        public double ChamferCloseR { get; set; } = 1.5;
    }

    /// <summary>
    /// 倒角偏三个 Dist；失败为 -1。
    /// </summary>
    public class ChamferOffsetResult
    {
        public float Dist1 { get; set; } = -1f;

        public float Dist2 { get; set; } = -1f;

        public float Dist3 { get; set; } = -1f;

        public int HoleNum { get; set; }

        public double Radius0 { get; set; }

        public string Warn { get; set; }

        public static ChamferOffsetResult Fail(string warn)
        {
            return new ChamferOffsetResult { Warn = warn };
        }
    }
}
