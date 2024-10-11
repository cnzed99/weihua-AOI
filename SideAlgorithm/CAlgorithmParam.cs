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
            AlgorithmType = "SideAlgorithm";
            DefectSpecies = new Dictionary<string, List<string>>()
            {
                ["侧面毛刺类"] = new List<string>() { "毛刺" },
            };
        }

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
        public override void UpdataMaociAlgorParamUse() { }

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
            cellDetection1.Type = "侧面毛刺类";
            cellDetection1.RecipeDefectName = "毛刺";
            cellDetection1.Category = Category.区域;
            cellDetection1.regionOut = maociRegion;
            cell.AlgorithmOut.Add(cellDetection1);

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
            cellDetection1.Type = "侧面毛刺类";
            cellDetection1.RecipeDefectName = "毛刺";
            cellDetection1.Category = Category.区域;
            cellDetection1.regionOut = maociRegion;
            cell.AlgorithmOut.Add(cellDetection1);

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
    }
}
