using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace FrontAlgorithm
{
    /// <summary>
    /// 2024.9.11 李焕彬
    /// 算法参数派生类
    /// </summary>
    public class CAlgorithmParam : CAlgorithmParamBase
    {
        public CAlgorithmParam()
            : base()
        {
            //AlgorithmType = "FrontAlgorithm";
            DefectSpecies = new()
            {
                new("铝层缺陷类", new() { new("毛刺", Category.区域) }),
                new("料区缺陷类", new() { new("掉料", Category.区域) }),
                new(
                    "异常类",
                    new()
                    {
                        new("铝层异常", Category.值),
                        new("料区异常", Category.值),
                        new("超时", Category.值),
                        new("失焦", Category.值)
                    }
                ),
            };
            DefectFeatures = new();
            DefectFeatures.Add(new("PeakHeight", "顶点高度", "PeakHeight", "um"));
            DefectFeatures.Add(new("BotHeight", "低点高度", "BotHeight", "um"));
            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("ContLength", "周长", "ContLength", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
        }

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParam(name, token));
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        public override void UpdataAlgorParamUse() { }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 获取清晰度计算函数
        /// </summary>
        /// <returns>清晰度计算函数</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Func<CImage, float> GetDistinctFunc()
        {
            return (CImage image) =>
            {
                CParam pcParam = (CParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect);
                if (pcParam != null)
                {
                    return CAlgorithmDll.CalcDistinct(
                        image.ImageWidth,
                        image.ImageHeight,
                        image.StrideWidth,
                        image.ImageData,
                        0,
                        (int)pcParam.DarkThresh
                    );
                }
                else
                {
                    return 0;
                }
            };
        }

        /// <summary>
        /// 2024.9.12 李焕彬
        /// 检测信息，存放算法检测返回结果
        /// 内含非托管内存，可重复使用，PC算法和FPGA算法使用同一个
        /// </summary>
        SDetectInfo detectInfo = new SDetectInfo();

        /// <summary>
        /// 测试算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            var paramClass = (CParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect);
            EMDETECTRESULT result = EMDETECTRESULT.EMDR_OK;
            if (paramClass.UsePreAlg)
            {
                SMaociAlgorPreParam param = new(paramClass, cell.MmPerPixel * 1000);
                result = CAlgorithmDll.TestFpga(
                    cell.Image.ImageWidth,
                    cell.Image.ImageHeight,
                    cell.Image.StrideWidth,
                    cell.Image.ImageData,
                    ref param,
                    ref detectInfo
                );
            }
            else
            {
                SMaociAlgorParam param = new(paramClass, cell.MmPerPixel * 1000);
                result = CAlgorithmDll.Test(
                    cell.Image.ImageWidth,
                    cell.Image.ImageHeight,
                    cell.Image.StrideWidth,
                    cell.Image.ImageData,
                    ref param,
                    ref detectInfo
                );
            }
            detectInfo.DeComposeEdge(
                cell.MmPerPixel * 1000,
                out var edgeDarkTop,
                out var edgeDarkBottom,
                out var edgeLightTop,
                out var edgeLightBot,
                out var maociRegion,
                out var thickRegion
            );

            CellDetection cellDetection1 = new CellDetection();
            cellDetection1.Type = "铝层缺陷类";
            cellDetection1.RecipeDefectName = "毛刺";
            cellDetection1.Category = Category.区域;
            cellDetection1.regionOut = maociRegion;
            cell.AlgorithmOut.Add(cellDetection1);

            CellDetection cellDetection2 = new CellDetection();
            cellDetection2.Type = "料区缺陷类";
            cellDetection2.RecipeDefectName = "掉料";
            cellDetection2.Category = Category.区域;
            cellDetection2.regionOut = thickRegion;
            cell.AlgorithmOut.Add(cellDetection2);

            CellDetection cellDetection3 = new CellDetection();
            cellDetection3.Type = "异常类";
            cellDetection3.RecipeDefectName = "铝层异常";
            cellDetection3.Category = Category.值;
            cellDetection3.Value =
                result == EMDETECTRESULT.EMDR_NG_LIGHTEDGE ? new() { 1.0f } : null;
            cell.AlgorithmOut.Add(cellDetection3);

            CellDetection cellDetection4 = new CellDetection();
            cellDetection4.Type = "异常类";
            cellDetection4.RecipeDefectName = "料区异常";
            cellDetection4.Category = Category.值;
            cellDetection4.Value =
                result == EMDETECTRESULT.EMDR_NG_DARKEDGE ? new() { 1.0f } : null;
            cell.AlgorithmOut.Add(cellDetection4);

            CellDetection cellDetection5 = new CellDetection();
            cellDetection5.Type = "异常类";
            cellDetection5.RecipeDefectName = "超时";
            cellDetection5.Category = Category.值;
            cellDetection5.Value = result == EMDETECTRESULT.EMDR_TIMEOUT ? new() { 1.0f } : null;
            cell.AlgorithmOut.Add(cellDetection5);

            CellDetection cellDetection6 = new CellDetection();
            cellDetection6.Type = "异常类";
            cellDetection6.RecipeDefectName = "失焦";
            cellDetection6.Category = Category.值;
            cellDetection6.Value = result == EMDETECTRESULT.EMDR_LOSEFOCUS ? new() { 1.0f } : null;
            cell.AlgorithmOut.Add(cellDetection6);

            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkTop, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkBottom, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightTop, Brushes.Green));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightBot, Brushes.Green));
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public partial class CParam : CParamBase
    {
        public CParam()
            : base() { }

        public CParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 使用初筛算法
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("使用初筛算法")]
        [property: Description("使用初筛算法")]
        private bool usePreAlg = false;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层自适应阈值邻域大小")]
        [property: Description("铝层自适应阈值说明")]
        private uint adaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层自适应阈值增加值")]
        [property: Description("铝层自适应阈值说明")]
        private int adaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层过滤矩阵邻域大小")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层过滤矩阵邻域点数量限制")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值说明")]
        private uint darkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值说明")]
        private uint lightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层厚度(um)")]
        [property: Description("铝层厚度(um)")]
        private double lightThick = 13;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 超时时间ms
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("超时时间(ms)")]
        [property: Description("超时时间说明")]
        private uint timeOut = 3000;

        /// <summary>
        /// 2024.8.19 李焕彬
        /// 最小清晰度
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("最小清晰度")]
        [property: Description("最小清晰度说明")]
        private float minDistinct = 25.0f;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("铝层自适应阈值邻域大小")]
        [property: Description("铝层自适应阈值说明")]
        private uint adaptiveSizePre = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("铝层自适应阈值增加值")]
        [property: Description("铝层自适应阈值说明")]
        private int adaptiveAddGrayPre = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("铝层过滤矩阵邻域大小")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbSizePre = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("铝层过滤矩阵邻域点数量限制")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbLightPointPre = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值说明")]
        private uint darkThreshPre = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值说明")]
        private uint lightThreshPre = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度限制，掉料检测
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("料区厚度限制(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThickLimitPre = 67.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("料区厚度NG连续长度限制(um)")]
        [property: Description("料区NG连续长度说明")]
        private double darkThickContinueLenPre = 11.25;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("料区厚度(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThickPre = 189;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度限制，毛刺检测
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层厚度限制(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThickLimitPre = 15.75;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层厚度NG连续长度限制(um)")]
        [property: Description("铝层NG连续长度说明")]
        private double lightThickContinueLenPre = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层厚度(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThickPre = 13.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制上
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层在料区中心位置限制上(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitTPre = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制下
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层在料区中心位置限制下(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitBPre = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层位置偏移值
        /// </summary>
        [ObservableProperty]
        [property: Category("4.PreTest Judge")]
        [property: DisplayName("铝层位置偏移值(um)")]
        [property: Description("料区中心位置限制说明")]
        private double lightPosOffestPre = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 超时时间ms
        /// </summary>
        [ObservableProperty]
        [property: Category("3.PreTest")]
        [property: DisplayName("超时时间(ms)")]
        [property: Description("超时时间说明")]
        private uint timeOutPre = 3000;
    };

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
        /// um直角高度
        /// </summary>
        public double PeakHeight = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um低点高度
        /// </summary>
        public double BotHeight = 0;

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
        /// 2024.7.4 李焕彬
        /// um周长
        /// </summary>
        public double ContLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um²面积
        /// </summary>
        public double Area = 0;

        public SRegionInfo() { }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 获取对应缺陷特征值
        /// </summary>
        /// <param name="character">缺陷特征</param>
        /// <returns>缺陷特征值</returns>
        public double GetValue(CFeacture feacture, SRegion region)
        {
            switch (feacture.Id)
            {
                case "PeakHeight":
                    return PeakHeight;
                case "BotHeight":
                    return BotHeight;
                case "Area":
                    return Area;
                case "LongLength":
                    return LongLen;
                case "ShortLength":
                    return ShorLen;
                case "Angle":
                    return Phi;
                case "ContLength":
                    return ContLen;
                case "Width":
                    return WidthBound;
                case "Height":
                    return HeightBound;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 合并区域
        /// </summary>
        /// <param name="regions">区域集</param>
        /// <returns>合并后区域</returns>
        public SRegion Union(List<SRegion> regions)
        {
            SRegionInfo regionInfo = new SRegionInfo();
            regionInfo.WidthBound = regions
                .Select(o => ((SRegionInfo)o.regionInfo).WidthBound)
                .Sum();
            regionInfo.HeightBound = regions
                .Select(o => ((SRegionInfo)o.regionInfo).HeightBound)
                .Sum();
            regionInfo.PeakHeight = regions
                .Select(o => ((SRegionInfo)o.regionInfo).PeakHeight)
                .Max();
            regionInfo.BotHeight = regions.Select(o => ((SRegionInfo)o.regionInfo).BotHeight).Min();
            regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
            regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
            regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
            regionInfo.ContLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ContLen).Sum();
            regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
            List<Point> pts = new List<Point>();
            for (int i = 0; i < regions.Count; i++)
            {
                pts.AddRange(regions[i].points);
            }
            return new SRegion(regionInfo, pts);
        }
    };
}
