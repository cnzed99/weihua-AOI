using System.ComponentModel;

namespace GearTestAlgorihm.Halcon
{
    /// <summary>
    /// 齿轮数 Halcon 比例/门槛
    /// </summary>
    public class GearToothCountParams
    {
        [Category("齿轮数")]
        [DisplayName("轮毂半径下限·短边")]
        public double HubRadiusMinFrac { get; set; } = 0.205;

        [Category("齿轮数")]
        [DisplayName("轮毂半径上限·短边")]
        public double HubRadiusMaxFrac { get; set; } = 0.265;

        [Category("齿轮数")]
        [DisplayName("法兰守卫半径·短边")]
        public double FlangeGuardFrac { get; set; } = 0.30;

        [Category("齿轮数")]
        [DisplayName("模糊轮次法兰守卫·短边")]
        public double FlangeGuardBlurFrac { get; set; } = 0.34;

        [Category("齿轮数")]
        [DisplayName("轮廓最短")]
        public double MinContourLen { get; set; } = 200;

        [Category("齿轮数")]
        [DisplayName("松轮廓最短")]
        public double MinContourLenLoose { get; set; } = 120;

        [Category("齿轮数")]
        [DisplayName("圆度下限")]
        public double MinCircularity { get; set; } = 0.62;

        [Category("齿轮数")]
        [DisplayName("松圆度下限")]
        public double MinCircularityLoose { get; set; } = 0.45;

        [Category("齿轮数")]
        [DisplayName("弧覆盖下限")]
        public double MinArcCoverage { get; set; } = 0.28;

        [Category("齿轮数")]
        [DisplayName("填孔圆心最大偏移·短边")]
        public double MaxFillShiftFrac { get; set; } = 0.20;

        [Category("齿轮数")]
        [DisplayName("极坐标宽度")]
        public int PolarWidth { get; set; } = 1440;

        [Category("齿轮数")]
        [DisplayName("极坐标内径·短边")]
        public double PolarRadiusStartFrac { get; set; } = 0.20;

        [Category("齿轮数")]
        [DisplayName("极坐标外径·短边")]
        public double PolarRadiusEndFrac { get; set; } = 0.36;

        [Category("齿轮数")]
        [DisplayName("齿根内垫 px")]
        public double RootInnerPad { get; set; } = 4;

        [Category("齿轮数")]
        [DisplayName("齿根外垫·短边")]
        public double RootOuterPadFrac { get; set; } = 0.050;

        [Category("齿轮数")]
        [DisplayName("占用率跌落下限")]
        public double MinimumOccupancyDrop { get; set; } = 0.030;

        [Category("齿轮数")]
        [DisplayName("齿数下限")]
        public int MinimumToothCount { get; set; } = 8;

        [Category("齿轮数")]
        [DisplayName("齿数上限")]
        public int MaximumToothCount { get; set; } = 30;

        [Category("齿轮数")]
        [DisplayName("径向带数")]
        public int NumberRadialBands { get; set; } = 7;

        [Category("齿轮数")]
        [DisplayName("带半高")]
        public int BandHalfHeight { get; set; } = 8;

        [Category("齿轮数")]
        [DisplayName("法兰占用率")]
        public double FlangeOccupancy { get; set; } = 0.80;

        [Category("齿轮数")]
        [DisplayName("暗槽占用率")]
        public double DarkGrooveOcc { get; set; } = 0.15;

        [Category("齿轮数")]
        [DisplayName("频率比下限")]
        public double MinimumScoreRatio { get; set; } = 1.15;

        [Category("齿轮数")]
        [DisplayName("最少票数")]
        public int MinimumVoteCount { get; set; } = 3;
    }

    /// <summary>
    /// 齿轮数结果。失败 Count=-1。
    /// </summary>
    public class GearToothCountResult
    {
        public float Count { get; set; } = -1f;

        public string Warn { get; set; }

        public static GearToothCountResult Fail(string warn)
        {
            return new GearToothCountResult { Count = -1f, Warn = warn };
        }
    }
}
