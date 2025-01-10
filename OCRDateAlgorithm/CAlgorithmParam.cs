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
using HalconDotNet;
using Microsoft.Web.Administration;

namespace OCRDateAlgorithm
{
    /// <summary>
    /// 2025.01.09 易群生
    /// 算法参数派生类
    /// </summary>
    public class CAlgorithmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// 2025.01.09 易群生
        /// halcon Ocr句柄
        /// </summary>
        private HTuple OCRHandle;

        /// <summary>
        /// 2025.01.09 易群生
        /// halcon文本识别句柄
        /// </summary>
        private HTuple TextModel;

        /// <summary>
        /// 2025.01.09 易群生
        /// 检测区域
        /// </summary>
        private HObject InspectionRegion;

        /// <summary>
        /// 初始化算法
        /// </summary>
        private bool bFlagInitialAlgorithmParam;

        public CAlgorithmParam()
            : base()
        {
            //AlgorithmType = "OCRDateAlgorithm";
            DefectSpecies = new()
            {
                new("印刷异常类", new() { new("漏印", Category.区域),new("缺印", Category.区域)}),
                new(
                    "异常类",
                    new() { new("没有产品", Category.值)}
                ),
            };
            DefectFeatures = new();

            InitialOcrLib();
            bFlagInitialAlgorithmParam = true;
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 初始化Ocr句柄
        /// </summary>
        /// <returns></returns>
        private bool InitialOcrLib()
        {
            try
            {
                HOperatorSet.ReadOcrClassMlp(AppDomain.CurrentDomain.BaseDirectory+ "AlgorithmPlug\\OCRDateAlgorithm\\Industrial_Rej.omc",out OCRHandle);
                HOperatorSet.CreateTextModelReader("auto", OCRHandle,out TextModel);
                HOperatorSet.SetTextModelParam(TextModel, "polarity", "light_on_dark");
                HOperatorSet.SetTextModelParam(TextModel, "text_line_structure", "");
                HOperatorSet.SetTextModelParam(TextModel, "text_line_separators", "/");
                HOperatorSet.SetTextModelParam(TextModel, "return_separators", "false");  
            
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 字符识别函数
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="ho_MaskRegion"></param>
        /// <param name="ho_TextLinesAll"></param>
        /// <param name="hv_BackThreshold"></param>
        /// <param name="hv_TextModel"></param>
        /// <param name="hv_OutRow1"></param>
        /// <param name="hv_OutColumn1"></param>
        /// <param name="hv_OutRow2"></param>
        /// <param name="hv_OutColumn2"></param>
        /// <param name="hv_TextLineCharacters"></param>
        public void OcrImageInspection(HObject ho_IntoImage, HObject ho_MaskRegion, out HObject ho_TextLinesAll,
             HTuple hv_BackThreshold, HTuple hv_TextModel, out HTuple hv_OutRow1, out HTuple hv_OutColumn1,
             out HTuple hv_OutRow2, out HTuple hv_OutColumn2, out HTuple hv_TextLineCharacters)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImageReduced, ho_Region, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_Rectangle, ho_ImageReduced1;
            HObject ho_ImageEmphasize, ho_TextLines, ho_TextLinesAllRegionUnion;

            // Local control variables 

            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Phi = new HTuple(), hv_Length1 = new HTuple();
            HTuple hv_Length2 = new HTuple(), hv_TextResult = new HTuple();
            HTuple hv_Orepstr = new HTuple(), hv_SingleCharacters = new HTuple();
            HTuple hv_Index1 = new HTuple(), hv_indices = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_TextLinesAll);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_TextLines);
            HOperatorSet.GenEmptyObj(out ho_TextLinesAllRegionUnion);
            hv_OutRow1 = new HTuple();
            hv_OutColumn1 = new HTuple();
            hv_OutRow2 = new HTuple();
            hv_OutColumn2 = new HTuple();
            hv_TextLineCharacters = new HTuple();
            try
            {
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_IntoImage, ho_MaskRegion, out ho_ImageReduced);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, 0, hv_BackThreshold);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                    70);
                hv_Row.Dispose(); hv_Column.Dispose(); hv_Phi.Dispose(); hv_Length1.Dispose(); hv_Length2.Dispose();
                HOperatorSet.SmallestRectangle2(ho_SelectedRegions, out hv_Row, out hv_Column,
                    out hv_Phi, out hv_Length1, out hv_Length2);
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Row, hv_Column, hv_Phi, hv_Length1,
                    hv_Length2);
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageReduced, ho_Rectangle, out ho_ImageReduced1
                    );
                ho_ImageEmphasize.Dispose();
                HOperatorSet.Emphasize(ho_ImageReduced1, out ho_ImageEmphasize, 14, 14, 3);
                hv_TextResult.Dispose();
                HOperatorSet.FindText(ho_ImageEmphasize, hv_TextModel, out hv_TextResult);
                //
                ho_TextLines.Dispose();
                HOperatorSet.GetTextObject(out ho_TextLines, hv_TextResult, "all_lines");
                ho_TextLinesAll.Dispose();
                HOperatorSet.GenEmptyObj(out ho_TextLinesAll);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_TextLinesAll, ho_TextLines, out ExpTmpOutVar_0);
                    ho_TextLinesAll.Dispose();
                    ho_TextLinesAll = ExpTmpOutVar_0;
                }

                hv_Orepstr.Dispose();
                hv_Orepstr = new HTuple();
                hv_Orepstr[0] = "o";
                hv_Orepstr[1] = "O";

                hv_SingleCharacters.Dispose();
                HOperatorSet.GetTextResult(hv_TextResult, "class", out hv_SingleCharacters);
                for (hv_Index1 = 0; (int)hv_Index1 <= (int)((new HTuple(hv_SingleCharacters.TupleLength()
                    )) - 1); hv_Index1 = (int)hv_Index1 + 1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_indices.Dispose();
                        HOperatorSet.TupleFind(hv_Orepstr, hv_SingleCharacters.TupleSelect(hv_Index1),
                            out hv_indices);
                    }
                    if ((int)(new HTuple(hv_indices.TupleGreaterEqual(0))) != 0)
                    {
                        if (hv_SingleCharacters == null)
                            hv_SingleCharacters = new HTuple();
                        hv_SingleCharacters[hv_Index1] = "0";
                    }
                    if ((int)(new HTuple(((hv_SingleCharacters.TupleSelect(hv_Index1))).TupleEqual(
                        "l"))) != 0)
                    {
                        if (hv_SingleCharacters == null)
                            hv_SingleCharacters = new HTuple();
                        hv_SingleCharacters[hv_Index1] = "1";
                    }
                }
                hv_TextLineCharacters.Dispose();
                HOperatorSet.TupleSum(hv_SingleCharacters, out hv_TextLineCharacters);

                ho_TextLinesAllRegionUnion.Dispose();
                HOperatorSet.Union1(ho_TextLinesAll, out ho_TextLinesAllRegionUnion);
                hv_OutRow1.Dispose(); hv_OutColumn1.Dispose(); hv_OutRow2.Dispose(); hv_OutColumn2.Dispose();
                HOperatorSet.SmallestRectangle1(ho_TextLinesAllRegionUnion, out hv_OutRow1,
                    out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2);

                ho_ImageReduced.Dispose();
                ho_Region.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageReduced1.Dispose();
                ho_ImageEmphasize.Dispose();
                ho_TextLines.Dispose();
                ho_TextLinesAllRegionUnion.Dispose();

                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Phi.Dispose();
                hv_Length1.Dispose();
                hv_Length2.Dispose();
                hv_TextResult.Dispose();
                hv_Orepstr.Dispose();
                hv_SingleCharacters.Dispose();
                hv_Index1.Dispose();
                hv_indices.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageReduced.Dispose();
                ho_Region.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageReduced1.Dispose();
                ho_ImageEmphasize.Dispose();
                ho_TextLines.Dispose();
                ho_TextLinesAllRegionUnion.Dispose();

                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Phi.Dispose();
                hv_Length1.Dispose();
                hv_Length2.Dispose();
                hv_TextResult.Dispose();
                hv_Orepstr.Dispose();
                hv_SingleCharacters.Dispose();
                hv_Index1.Dispose();
                hv_indices.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CPcParam(name, token));
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 更新参数结构体
        /// </summary>
        public override void UpdataAlgorParamUse() { }

        /// <summary>
        /// 2025.01.09 易群生
        /// 获取清晰度计算函数
        /// </summary>
        /// <returns>清晰度计算函数</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Func<CImage, float> GetDistinctFunc()
        {
            return (CImage image) =>
            {
                return 0;
            };
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 测试PC算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            HObject tempImage=null;
            HObject ho_TextLinesAll = null;
            HTuple hv_OutRow1 = null;
            HTuple hv_OutColumn1 = null;
            HTuple hv_OutRow2 = null;
            HTuple hv_OutColumn2 = null;
            HTuple hv_TextLineCharacters=null;

            string tempResult="";
            List<Point> tempPointList = new List<Point>();
            try
            {
                SOCRDateAlgorParam param =
                    new(
                        (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect),
                        cell.MmPerPixel * 1000
                    );
                
                HOperatorSet.GenImage1(out tempImage, "byte", cell.Image.ImageWidth, cell.Image.ImageHeight, cell.Image.ImageData);

                if (bFlagInitialAlgorithmParam)
                {
                    HOperatorSet.GenRectangle1(out InspectionRegion, 501, 324, 654, 917);
                    bFlagInitialAlgorithmParam = false;
                }
                
                OcrImageInspection(tempImage, InspectionRegion,out ho_TextLinesAll, param.DarkThresh, TextModel,
                    out hv_OutRow1,out hv_OutColumn1,out hv_OutRow2,out hv_OutColumn2,out hv_TextLineCharacters);

                tempResult = hv_TextLineCharacters.ToString();

                if (hv_OutRow1.Length > 0)
                {
                    if (tempResult.Length > 2)
                    {
                        tempResult = tempResult.Substring(1, tempResult.Length - 2);
                        cell.OcrResultString = tempResult;
                    }
                    else
                    {
                        cell.OcrResultString = "";
                    }
                }
                else 
                {
                    cell.OcrResultString = "";
                }
                cell.DrawEdges.Clear();
                
                if (hv_OutRow1.Length > 0)
                {
                    Point tempPoint = new Point();
                    tempPoint.X = hv_OutColumn1.D;
                    tempPoint.Y = hv_OutRow1.D;
                    tempPointList.Add(tempPoint);

                    tempPoint.X = hv_OutColumn2.D;
                    tempPoint.Y = hv_OutRow2.D;
                    tempPointList.Add(tempPoint);

                    cell.DrawEdges.Add(new CEdgeDraw(tempPointList, Brushes.Blue));
                }
                else
                {
                    Point tempPoint = new Point();
                    tempPoint.X = 0;
                    tempPoint.Y = 0;
                    tempPointList.Add(tempPoint);

                    tempPoint.X = 0;
                    tempPoint.Y = 0;
                    tempPointList.Add(tempPoint);

                    cell.DrawEdges.Add(new CEdgeDraw(tempPointList, Brushes.Blue));
                }

                
                if (tempResult == param.StandardDate)
                {
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = "印刷异常类";
                    cellDetection1.RecipeDefectName = "漏印";
                    cellDetection1.Category = Category.区域;
                    cellDetection1.regionOut = new List<SRegion>();
                    cell.AlgorithmOut.Add(cellDetection1);

                    CellDetection cellDetection2 = new CellDetection();
                    cellDetection2.Type = "印刷异常类";
                    cellDetection2.RecipeDefectName = "缺印";
                    cellDetection2.Category = Category.区域;
                    cellDetection2.regionOut = new List<SRegion>();
                    cell.AlgorithmOut.Add(cellDetection2);

                    CellDetection cellDetection4 = new CellDetection();
                    cellDetection4.Type = "异常类";
                    cellDetection4.RecipeDefectName = "没有产品";
                    cellDetection4.Category = Category.值;
                    cellDetection4.Value = null;
                    cell.AlgorithmOut.Add(cellDetection4);
                }
                else
                {
                    SRegionInfo sRegioninfo = new SRegionInfo();
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                    rec1Points.Add(new System.Windows.Point(0,0));
                    rec1Points.Add(new System.Windows.Point(100, 100));
                    SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);

                    if (hv_OutRow1.Length < 1)
                    {
                        CellDetection cellDetection1 = new CellDetection();
                        cellDetection1.Type = "印刷异常类";
                        cellDetection1.RecipeDefectName = "漏印";
                        cellDetection1.Category = Category.区域;
                        cellDetection1.regionOut = new List<SRegion>();
                        cellDetection1.regionOut.Add(detectRegion);
                        cell.AlgorithmOut.Add(cellDetection1);

                        CellDetection cellDetection2 = new CellDetection();
                        cellDetection2.Type = "印刷异常类";
                        cellDetection2.RecipeDefectName = "缺印";
                        cellDetection2.Category = Category.区域;
                        cellDetection2.regionOut = new List<SRegion>();
                        cell.AlgorithmOut.Add(cellDetection2);

                        CellDetection cellDetection4 = new CellDetection();
                        cellDetection4.Type = "异常类";
                        cellDetection4.RecipeDefectName = "没有产品";
                        cellDetection4.Category = Category.值;
                        cellDetection4.Value = null;
                        cell.AlgorithmOut.Add(cellDetection4);

                    }
                    else
                    {
                        CellDetection cellDetection1 = new CellDetection();
                        cellDetection1.Type = "印刷异常类";
                        cellDetection1.RecipeDefectName = "漏印";
                        cellDetection1.Category = Category.区域;
                        cellDetection1.regionOut = new List<SRegion>();
                        cell.AlgorithmOut.Add(cellDetection1);

                        CellDetection cellDetection2 = new CellDetection();
                        cellDetection2.Type = "印刷异常类";
                        cellDetection2.RecipeDefectName = "缺印";
                        cellDetection2.Category = Category.区域;
                        cellDetection2.regionOut = new List<SRegion>();
                        cellDetection2.regionOut.Add(detectRegion);
                        cell.AlgorithmOut.Add(cellDetection2);

                        CellDetection cellDetection4 = new CellDetection();
                        cellDetection4.Type = "异常类";
                        cellDetection4.RecipeDefectName = "没有产品";
                        cellDetection4.Category = Category.值;
                        cellDetection4.Value = null;
                        cell.AlgorithmOut.Add(cellDetection4);
                    }
                }
            }
            finally
            {
                tempImage.Dispose();
                ho_TextLinesAll.Dispose();
                hv_OutRow1.Dispose();
                hv_OutColumn1.Dispose();
                hv_OutRow2.Dispose();
                hv_OutColumn2.Dispose();
                hv_TextLineCharacters.Dispose();

            }
        }
    }

    /// <summary>
    /// 2025.01.09 易群生
    /// PC算法参数
    /// </summary>
    public partial class CPcParam : CParamBase
    {
        public CPcParam()
            : base() { }

        public CPcParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2025.01.09 易群生
        /// 日期值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("日期值")]
        [property: Description("日期值说明")]
        private String standardDate = "112412112024/12/232026/12/22B";

        /// <summary>
        /// 2025.01.09 易群生
        /// 日期黑色背景阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("日期黑色背景阈值")]
        [property: Description("日期黑色背景阈值说明")]
        private int darkThresh = 50;

    };

    /// <summary>
    /// 2025.01.09 易群生
    /// 区域信息
    /// </summary>
    public struct SRegionInfo : IRegionInfo
    {

        public SRegionInfo() { }

        /// <summary>
        /// 2025.01.09 易群生
        /// 获取对应缺陷特征值
        /// </summary>
        /// <param name="character">缺陷特征</param>
        /// <returns>缺陷特征值</returns>
        public double GetValue(CFeacture feacture, SRegion region)
        {
            switch (feacture.Id)
            {
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 合并区域
        /// </summary>
        /// <param name="regions">区域集</param>
        /// <returns>合并后区域</returns>
        public SRegion Union(List<SRegion> regions)
        {
            SRegionInfo regionInfo = new SRegionInfo();

            List<Point> pts = new List<Point>();
            for (int i = 0; i < regions.Count; i++)
            {
                pts.AddRange(regions[i].points);
            }
            return new SRegion(regionInfo, pts);
        }
    };
}
