
using OpenCvSharp;
using System.Drawing;
using WH.RecipeCellRootBase;

namespace PullZipperAlgorihm
{
    /// <summary>
    /// 2024.10.31 鲍赞宝
    /// AI检测结果结构体
    /// </summary>
    public struct SResultInfo
    {
        public SResultInfo()
        {
        }
        /// <summary>
        /// 检测标签
        /// </summary>
        public string LabelStr=string.Empty;
        /// <summary>
        /// 检测结果分数
        /// </summary>
        public double ResultScore = 0.0;
        /// <summary>
        /// 检测结果区域点位
        /// </summary>
        public List<Point2f> ResultPoints =new List<Point2f>();


    }


    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域信息
    /// </summary>
    public struct SRegionInfo : IRegionInfo
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直宽度
        /// </summary>
        public double WidthBound = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直高度
        /// </summary>
        public double HeightBound = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um长边长度
        /// </summary>
        public double LongLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um短边长度
        /// </summary>
        public double ShorLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 角度
        /// </summary>
        public double Phi = 0;

        /// <summary>
        /// 20241104 TCG
        /// 分数
        /// </summary>
        public double Score = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um?面积
        /// </summary>
        public double Area = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um周长
        /// </summary>
        public double ContLen = 0;

        /// <summary>
        /// 20260424 鲍赞宝
        /// 色差值
        /// </summary>
        public double ColorDiffValue = 0;
        /// <summary>
        /// 缺陷坐标X
        /// </summary>
        public float PositionX = 0;
        /// <summary>
        /// 缺陷坐标Y
        /// </summary>
        public float PositionY = 0;

        public SRegionInfo() { }

        public double GetValue(CFeacture feacture, SRegion region)
        {
            switch (feacture.Id)
            {
                case "Width":
                    return WidthBound;
                case "Height":
                    return HeightBound;
                case "Area":
                    return Area;
                case "LongLength":
                    return LongLen;
                case "ShortLength":
                    return ShorLen;
                case "Angle":
                    return Phi;
                case "Score":
                    return Score;
                case "ColorDiffValue":
                    return ColorDiffValue;
                case "PositionX":
                    return PositionX;
                case "PositionY":
                    return PositionY;
                default:
                    return 0;
            }
        }

        public SRegion Union(List<SRegion> regions)
        {
            SRegionInfo regionInfo = new SRegionInfo();
            //regionInfo.WidthBound = regions
            //    .Select(o => ((SRegionInfo)o.regionInfo).WidthBound)
            //    .Sum();
            //regionInfo.HeightBound = regions
            //    .Select(o => ((SRegionInfo)o.regionInfo).HeightBound)
            //    .Sum();
            
            regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
            regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
            regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
            //regionInfo.ContLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ContLen).Sum();
            regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
            List<System.Windows.Point> pts = new List<System.Windows.Point>();
            for (int i = 0; i < regions.Count; i++)
            {
                pts.AddRange(regions[i].points);
            }
            return new SRegion(regionInfo, pts);
        }
       
    };


}
