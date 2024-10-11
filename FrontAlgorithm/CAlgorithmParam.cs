using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
            AlgorithmType = "FrontAlgorithm";
            DefectSpecies = new Dictionary<string, List<string>>()
            {
                ["铝层缺陷类"] = new List<string>() { "毛刺" },
                ["料区缺陷类"] = new List<string>() { "掉料" },
            };
        }

        ///// <summary>
        ///// 2024.7.5 李焕彬
        ///// 正在使用的PC参数结构体
        ///// </summary>
        //public SMaociAlgorParam MaociAlgorParamUse { get; set; }

        ///// <summary>
        ///// 2024.7.5 李焕彬
        ///// 正在使用的FPGA参数结构体
        ///// </summary>
        //public SMaociAlgorParamFpga MaociAlgorParamFpgaUse { get; set; }

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddFpgaParam(string name)
        {
            this.FpgaParams.Add(new CFpgaParam(name, token));
        }

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddPcParam(string name)
        {
            this.PcParams.Add(new CPcParam(name, token));
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        public override void UpdataMaociAlgorParamUse()
        {
            //if (PcParams.FirstOrDefault(o => o.Name == PcSelect) != null)
            //{
            //    MaociAlgorParamUse = new(
            //        (CPcParam)PcParams.FirstOrDefault(o => o.Name == PcSelect)
            //    );
            //}
            //if (FpgaParams.FirstOrDefault(o => o.Name == FpgaSelect) != null)
            //{
            //    MaociAlgorParamFpgaUse = new(
            //        (CFpgaParam)FpgaParams.FirstOrDefault(o => o.Name == FpgaSelect)
            //    );
            //}
        }

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
                CPcParam pcParam = (CPcParam)PcParams.FirstOrDefault(o => o.Name == PcSelect);
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
        /// 测试FPGA算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override EMDETECTRESULT DetectFpga(Cell cell)
        {
            var param = new SMaociAlgorParamFpga(
                (CFpgaParam)FpgaParams.FirstOrDefault(o => o.Name == FpgaSelect),
                cell.MmPerPixel * 1000
            );
            EMDETECTRESULT result = CAlgorithmDll.TestFpga(
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

            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkTop, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkBottom, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightTop, Brushes.Green));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightBot, Brushes.Green));

            return result;
        }

        /// <summary>
        /// 测试PC算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override EMDETECTRESULT DetectImage(Cell cell)
        {
            SMaociAlgorParam param =
                new(
                    (CPcParam)PcParams.FirstOrDefault(o => o.Name == PcSelect),
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

            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkTop, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeDarkBottom, Brushes.Blue));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightTop, Brushes.Green));
            cell.DrawEdges.Add(new CEdgeDraw(edgeLightBot, Brushes.Green));

            return result;
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public partial class CPcParam : CPcParamBase
    {
        public CPcParam()
            : base() { }

        public CPcParam(string name, Token token)
            : base(name, token) { }

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
    };

    /// <summary>
    /// FPGA算法参数
    /// </summary>
    public partial class CFpgaParam : CFpgaParamBase
    {
        public CFpgaParam()
            : base() { }

        public CFpgaParam(string name, Token token)
            : base(name, token) { }

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
        /// 料区厚度限制，掉料检测
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度限制(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThickLimit = 67.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度NG连续长度限制(um)")]
        [property: Description("料区NG连续长度说明")]
        private double darkThickContinueLen = 11.25;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThick = 189;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度限制，毛刺检测
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度限制(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThickLimit = 15.75;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度NG连续长度限制(um)")]
        [property: Description("铝层NG连续长度说明")]
        private double lightThickContinueLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThick = 13.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制上
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层在料区中心位置限制上(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitT = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制下
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层在料区中心位置限制下(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitB = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层位置偏移值
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层位置偏移值(um)")]
        [property: Description("料区中心位置限制说明")]
        private double lightPosOffest = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 超时时间ms
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("超时时间(ms)")]
        [property: Description("超时时间说明")]
        private uint timeOut = 3000;
    }
}
