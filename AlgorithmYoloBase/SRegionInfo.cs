using WH.RecipeCellRootBase;
using WH.RunCell;

namespace AlgorithmYoloBase
{
    /// 平台区域特征
    public struct SRegionInfo : IRegionInfo
    {
        public double WidthBound = 0;
        public double HeightBound = 0;
        public double LongLen = 0;
        public double ShorLen = 0;
        public double Phi = 0;
        public double Score = 0;
        public double Area = 0;
        public double ContLen = 0;
        public double ColorDiffValue = 0;
        public float PositionX = 0;
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
            if (regions != null && regions.Count > 0)
            {
                regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
                regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
                regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
                regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
            }
            List<System.Windows.Point> pts = new List<System.Windows.Point>();
            if (regions != null)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    if (regions[i].points != null)
                    {
                        pts.AddRange(regions[i].points);
                    }
                }
            }
            return new SRegion(regionInfo, pts);
        }
    }
}
