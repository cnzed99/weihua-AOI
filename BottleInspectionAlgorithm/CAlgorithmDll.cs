using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlgorithmDll;
using WH.RecipeCellRootBase;
using HalconDotNet;

namespace BottleInspectionAlgorithm
{
    /// <summary>
    /// 2025.01.09 易群生
    /// PC算法参数
    /// </summary>
    public struct BottleInspectionAlgorithmParam
    {
        /// <summary>
        /// 2025.01.09 易群生
        /// 标准日期的来源：1 固定（由UI界面输入）；2 其他来源(从客户服务器读取)
        /// </summary>
        //public int StandardDateSource = 1;

        /// <summary>
        /// 2025.04.05 易群生
        /// 强制设定测试结果："ok"-强制设定测试结果全部为ok，"ng"-强制设定测试结果全部为ng,""-实际测试结果
        /// </summary>
        public String InspectionResult = "";

        /// <summary>
        /// 2025.03.15 易群生
        /// 柱形物体的半径，单位是mm
        /// </summary>
        public double CylinderRadiusMM = 11;

        /// <summary>
        /// 2025.03.15 易群生
        /// 图像的像素实际尺寸，单位是mm
        /// </summary>
        public double PixelSizeMM = 0.058;

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签位置最上面的行位置
        /// </summary>
        public int LabelMinRow = 00;

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签位置最下面的行位置
        /// </summary>
        public int LabelMaxRow = 1000;


        /// <summary>
        /// 2025.03.20 易群生
        /// 瓶子直径
        /// </summary>
        public int BottleDiameter = 400;

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签亮度上限
        /// </summary>
        public int LabelBrightnessMax = 80;

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签亮度下限
        /// </summary>
        public int LabelBrightnessMin = 40;

        /// <summary>
        /// 标准日期
        /// </summary>
        public String StandardDate = "20240802";

        /// <summary>
        /// 2025.03.20 易群生
        /// 三期位置最上面的行位置
        /// </summary>
        public int DateMinRow = 00;

        /// <summary>
        /// 2025.03.20 易群生
        /// 三期位置最下面的行位置
        /// </summary>
        public int DateMaxRow = 1000;


        /// <summary>
        /// /// 2025.03.10 易群生
        /// 0 代表0度方向，90代表顺时针旋转90度方向，180代表顺时针旋转180方向，270代表顺时针旋转270度方向
        /// </summary>
        public int DateAngleIndex = 0;

        /// <summary>
        /// 2025.03.20 易群生
        /// 日期亮度差
        /// </summary>
        public int DateDeltaBrightness = 20;

        /// <summary>
        /// 2025.03.20 易群生
        /// 瓶盖位置最上面的行位置
        /// </summary>
        public int CapMinRow = 0;

        /// <summary>
        /// 2025.03.20 易群生
        /// 瓶盖位置最下面的行位置
        /// </summary>
        public int CapMaxRow = 1000;

        /// <summary>
        /// 2025.03.20 易群生
        /// 蓝盖亮度上限
        /// </summary>
        public int BlueCapBrightnessMax = 100;

        /// <summary>
        /// 2025.03.20 易群生
        /// 蓝盖亮度下限
        /// </summary>
        public int BlueCapBrightnessMin = 40;

        /// <summary>
        /// 2025.03.20 易群生
        /// 蓝盖厚度
        /// </summary>
        public int BlueCapThickness = 40;

        /// <summary>
        /// 2025.03.20 易群生
        /// 蓝盖直径
        /// </summary>
        public int BlueCapDiameter = 300;

        /// <summary>
        /// 2025.01.09 易群生
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public BottleInspectionAlgorithmParam(CPcParam param)
        {
            this.InspectionResult = param.InspectionResult;
            this.LabelMinRow = param.LabelMinRow;
            this.LabelMaxRow = param.LabelMaxRow;
            this.LabelBrightnessMax = param.LabelBrightnessMax;
            this.LabelBrightnessMin = param.LabelBrightnessMin;

            this.BottleDiameter = param.BottleDiameter; 

            this.StandardDate = param.StandardDate;

            this.DateMinRow = param.DateMinRow;
            this.DateMaxRow = param.DateMaxRow;
            this.DateAngleIndex = param.DateAngleIndex;

            this.DateDeltaBrightness = param.DateDeltaBrightness;

            this.CylinderRadiusMM = param.CylinderRadiusMM;
            this.PixelSizeMM = param.PixelSizeMM;

            this.CapMinRow = param.CapMinRow;
            this.CapMaxRow = param.CapMaxRow;

            this.BlueCapBrightnessMax = param.BlueCapBrightnessMax;
            this.BlueCapBrightnessMin = param.BlueCapBrightnessMin;
            this.BlueCapThickness = param.BlueCapThickness;
            this.BlueCapDiameter = param.BlueCapDiameter;
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public BottleInspectionAlgorithmParam(CPcParam param, double umPerPixel)
        {
            this.InspectionResult = param.InspectionResult;
            this.LabelMinRow = param.LabelMinRow;
            this.LabelMaxRow = param.LabelMaxRow;
            this.LabelBrightnessMax = param.LabelBrightnessMax;
            this.LabelBrightnessMin = param.LabelBrightnessMin;

            this.BottleDiameter = param.BottleDiameter;
            this.StandardDate = param.StandardDate;

            this.DateMinRow = param.DateMinRow;
            this.DateMaxRow = param.DateMaxRow;
            this.DateAngleIndex = param.DateAngleIndex;
            this.DateDeltaBrightness = param.DateDeltaBrightness;

            this.CylinderRadiusMM = param.CylinderRadiusMM;
            this.PixelSizeMM = param.PixelSizeMM;

            this.CapMinRow = param.CapMinRow;
            this.CapMaxRow = param.CapMaxRow;

            this.BlueCapBrightnessMax = param.BlueCapBrightnessMax;
            this.BlueCapBrightnessMin = param.BlueCapBrightnessMin;
            this.BlueCapThickness = param.BlueCapThickness;
            this.BlueCapDiameter = param.BlueCapDiameter;
        }
    };


    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域信息
    /// </summary>
    public struct SRegionInfo : IRegionInfo
    {
        ///// <summary>
        ///// 2024.7.4 李焕彬
        ///// um垂直宽度
        ///// </summary>
        //public double WidthBound = 0;

        ///// <summary>
        ///// 2024.7.4 李焕彬
        ///// um垂直高度
        ///// </summary>
        //public double HeightBound = 0;

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

        public SRegionInfo() { }

        public double GetValue(CFeacture feacture, SRegion region)
        {
            switch (feacture.Id)
            {

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
