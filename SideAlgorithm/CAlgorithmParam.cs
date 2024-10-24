using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace SideAlgorithm
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
            //AlgorithmType = "SideAlgorithm";
            DefectSpecies = new()
            {
                new("侧面毛刺类", new() { new("毛刺", Category.区域) }),
                new(
                    "异常类",
                    new() { new("边缘异常", Category.值), new("超时", Category.值), new("失焦", Category.值) }
                ),
            };
            DefectFeatures = new();
            DefectFeatures.Add(new("PeakHeight", "顶点高度", "PeakHeight", "um"));
            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
        }

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CPcParam(name, token));
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
                CPcParam pcParam = (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect);
                if (pcParam != null)
                {
                    return CAlgorithmDll.CalcDistinct(
                        image.ImageWidth,
                        image.ImageHeight,
                        image.StrideWidth,
                        image.ImageData,
                        2,
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
        /// 测试PC算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            SMaociAlgorParam param =
                new(
                    (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect),
                    cell.MmPerPixel * 1000
                );
            EMDETECTRESULT result = CAlgorithmDll.Test(
                cell.Image.ImageWidth,
                cell.Image.ImageHeight,
                cell.Image.StrideWidth,
                cell.Image.ImageData,
                ref param,
                ref detectInfo
            );
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
            cellDetection1.Type = "侧面毛刺类";
            cellDetection1.RecipeDefectName = "毛刺";
            cellDetection1.Category = Category.区域;
            cellDetection1.regionOut = maociRegion;
            cell.AlgorithmOut.Add(cellDetection1);

            CellDetection cellDetection4 = new CellDetection();
            cellDetection4.Type = "异常类";
            cellDetection4.RecipeDefectName = "边缘异常";
            cellDetection4.Category = Category.值;
            cellDetection4.Value = result == EMDETECTRESULT.EMDR_NG_EDGE ? new() { 1.0f } : null;
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
    public partial class CPcParam : CParamBase
    {
        public CPcParam()
            : base() { }

        public CPcParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("过滤矩阵邻域大小")]
        [property: Description("过滤矩阵说明")]
        private uint neighbSize = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 极片阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("极片阈值")]
        [property: Description("极片阈值说明")]
        private uint darkThresh = 200;

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
        private float minDistinct = 4.0f;
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
