using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using GeneralMLOBBAlgorithm;
using HalconDotNet;
using HandyControl.Controls;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace BottleAlgorithm
{
    public class CBottleAlgorithmParam : GeneralMLOBBAlgorithmParam
    {
        /// <summary>
        /// 2025.01.09 鲍赞宝
        /// halcon Ocr句柄
        /// </summary>
        private static HTuple m_OCRHandle;

        /// <summary>
        /// 2025.01.09初始化算法
        /// </summary>
        // private bool m_bFlagInitialAlgorithmParam;
        /// <summary>
        /// OCR 检测区域
        /// </summary>
        private HObject m_DateCropRegion;

        /// <summary>
        /// 2025.03.18 鲍赞宝
        /// 字符串测试结果：0-待测，1-无字符，2-有字符
        /// </summary>
        private int m_DateStringTestResult = 0;

        /// <summary>
        //2025.03.18 鲍赞宝
        //检测蓝盖有无
        //0是待处理，1是瓶盖缺失，2是有蓝盖
        /// </summary>

        private int m_BlueCapTestResult = 0;

        /// <summary>
        //2025.03.18 鲍赞宝
        //检测铝盖有无
        //0是待处理，1是无铝盖，2是有铝盖
        /// </summary>
        private int m_CapTestResult = 0;

        /// <summary>
        /// 2025.03.23 鲍赞宝
        /// AI测试结果，0是ok，大于0是ng类型
        /// </summary>
        //  private int m_AITestResult = 0;

        /// <summary>
        //2025.03.18 鲍赞宝
        //检测标签有无,
        //0是待处理，1是无标 ，2是有标签，
        /// </summary>
        private int m_LabelTestResult = 0;

        public CBottleAlgorithmParam()
            : base()
        {
            string modelDirPath = ".\\AlgorithmPlug\\BottleAlgorithm\\Models";
            //ReadNames(modelDirPath);
            DefectSpecies = new List<CDefectSpecies>();
            string[] ModelNamesAll = text_Model_Names.Concat(LabelDetect_names).ToArray();
            if (ModelNamesAll?.Length > 0)
            {
                List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();

                for (int i = 0; i < ModelNamesAll.Length; i++)
                {
                    //if (Detect_names[i] == "三期")
                    //{
                    //    CDefectRecipe defectRecipe1 = new CDefectRecipe(Detect_names[i], Category.值);
                    //    cDefectRecipes.Add(defectRecipe1);
                    //}
                    //else
                    //{
                    //    CDefectRecipe defectRecipe = new CDefectRecipe(Detect_names[i], Category.区域);
                    //    cDefectRecipes.Add(defectRecipe);
                    //}

                    CDefectRecipe defectRecipe = new CDefectRecipe(ModelNamesAll[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }

                CDefectSpecies defectSpecies = new CDefectSpecies("西林瓶表面缺陷类", cDefectRecipes);
                DefectSpecies.Add(defectSpecies);
            }

            DefectSpecies.Add(
                new("西林瓶有无类", new() { new("瓶盖", Category.区域), new("标签", Category.区域), })
            );

            // DefectSpecies.Add(new("异常类", new() { new("没有产品", Category.值) }));

            DefectFeatures = new();

            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));

            //  m_bFlagInitialAlgorithmParam = true;
            //初始化Halcon OCR
            // InitialHalconOcrLib();
        }

        /// <summary>
        /// 2025.4.22 鲍赞宝
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CBottleParam(name, token));
        }

        public override void DetectImage(Cell cell)
        {
            CBottleParam param =
                AlgorParams.FirstOrDefault(o => o.Name == ParamSelect) as CBottleParam;
            if (param != null)
            {
                if (param.InspectionResult.ToLower() == "ok")
                {
                    // AddTestResult(cell, "", null);
                    return;
                }
                m_BlueCapTestResult = 0;
                m_DateStringTestResult = 0;
                m_CapTestResult = 0;
                m_LabelTestResult = 0;

                HOperatorSet.GenEmptyObj(out HObject CameraImage);
                CameraImage.Dispose();
                if (cell.Image.PixelFormat == System.Windows.Media.PixelFormats.Bgr32)
                {
                    //离线加载电脑图片
                    HOperatorSet.GenImageInterleaved(
                        out CameraImage,
                        cell.Image.ImageData,
                        "bgrx",
                        cell.Image.ImageWidth,
                        cell.Image.ImageHeight,
                        0,
                        "byte",
                        0,
                        0,
                        0,
                        0,
                        -1,
                        0
                    );
                }
                else
                {
                    //相机采集图片
                    HOperatorSet.GenImageInterleaved(
                        out CameraImage,
                        cell.Image.ImageData,
                        "rgb",
                        cell.Image.ImageWidth,
                        cell.Image.ImageHeight,
                        0,
                        "byte",
                        0,
                        0,
                        0,
                        0,
                        -1,
                        0
                    );
                }

                HTuple bFlagCap = null;
                //HTuple tempRow1 = null;
                //HTuple tempRow2 = null;
                //HTuple tempColumn1 = null;
                //HTuple tempColumn2 = null;

                HTuple hv_Width = new HTuple();
                HTuple hv_Height = new HTuple();

                HObject ho_ImageR = null,
                    ho_ImageG = null,
                    ho_ImageB = null;

                HOperatorSet.GenEmptyObj(out ho_ImageR);
                HOperatorSet.GenEmptyObj(out ho_ImageG);
                HOperatorSet.GenEmptyObj(out ho_ImageB);

                hv_Width.Dispose();
                hv_Height.Dispose();
                HOperatorSet.GetImageSize(CameraImage, out hv_Width, out hv_Height);
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();
                HOperatorSet.Decompose3(CameraImage, out ho_ImageR, out ho_ImageG, out ho_ImageB);

                HTuple CapMinRow = param.CapMinRow;
                HTuple CapMinCol = 0;
                HTuple CapMaxRow = param.CapMaxRow;
                HTuple CapMaxCol = cell.Image.ImageWidth - 1;

                #region 铝盖定位 注释

                //try
                //{
                //    //铝盖定位
                //    LocationCap(CameraImage, param.CapBrightnessMin, param.CapBrightnessMax, CapMinRow,
                //       CapMinCol, CapMaxRow, CapMaxCol, param.CapRadius, param.CapThickness, out bFlagCap);
                //    m_CapTestResult = bFlagCap.I;

                //    bFlagCap?.Dispose();

                //}
                //catch (Exception ex)
                //{
                //    OperateLog.Info("BottleTest_LocationCap:" + ex.Message.ToString());
                //    bFlagCap?.Dispose();
                //}

                #endregion 铝盖定位 注释
                base.DetectImage(cell);

                #region 蓝盖有无

                //int BlueCapLeftUPX = 0;
                //int BlueCapLeftUPY = 0;
                //int BlueCapRightDownX = 0;
                //int BlueCapRightDownY = 0;
                // double BlueCapMax = 0;

                HTuple bFlagBlueCap = null;
                try
                {
                    HObject ho_CapRegion = null;
                    HOperatorSet.GenEmptyObj(out ho_CapRegion);
                    HTuple hv_Max = new HTuple();
                    ho_CapRegion.Dispose();
                    hv_Max.Dispose();

                    //蓝盖缺失检测
                    InspectionBlueCap(
                        ho_ImageB,
                        ho_ImageR,
                        out ho_CapRegion,
                        param.BlueCapBrightnessMin,
                        param.BlueCapBrightnessMax,
                        CapMinRow,
                        CapMinCol,
                        CapMaxRow,
                        CapMaxCol,
                        param.BlueCapThickness,
                        out hv_Max
                    );

                    //m_BlueCapTestResult = bFlagBlueCap.I;
                    //BlueCapLeftUPX = col1.I+5;
                    //BlueCapLeftUPY = row1.I+5;
                    //BlueCapRightDownX = col2.I-5;
                    //BlueCapRightDownY = row2.I-5;
                    HTuple hv_Row1 = new HTuple(),
                        hv_Column1 = new HTuple();
                    HTuple hv_Row2 = new HTuple(),
                        hv_Column2 = new HTuple();
                    //HObject ho_Rectangle = null;
                    //HOperatorSet.GenEmptyObj(out ho_Rectangle);

                    hv_Row1.Dispose();
                    hv_Column1.Dispose();
                    hv_Row2.Dispose();
                    hv_Column2.Dispose();
                    HOperatorSet.SmallestRectangle1(
                        ho_CapRegion,
                        out hv_Row1,
                        out hv_Column1,
                        out hv_Row2,
                        out hv_Column2
                    );
                    //ho_Rectangle.Dispose();
                    //HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2,
                    //    hv_Column2);

                    //  BlueCapMax = hv_Max.D;

                    #region 盖子区域显示

                    //瓶盖的搜索范围显示
                    System.Windows.Point p1 = new System.Windows.Point(CapMinCol, CapMinRow);
                    System.Windows.Point p2 = new System.Windows.Point(CapMaxCol, CapMinRow);
                    System.Windows.Point p3 = new System.Windows.Point(CapMaxCol, CapMaxRow);
                    System.Windows.Point p4 = new System.Windows.Point(CapMinCol, CapMaxRow);
                    List<System.Windows.Point> capPoints = new List<System.Windows.Point>();
                    capPoints.Add(p1);
                    capPoints.Add(p2);
                    capPoints.Add(p3);
                    capPoints.Add(p4);
                    capPoints.Add(p1);
                    cell.DrawEdges.Add(new CEdgeDraw(capPoints, Brushes.Yellow));

                    //if ((m_BlueCapTestResult != 2) || (m_CapTestResult != 2))
                    //{
                    System.Windows.Point recp1 = new System.Windows.Point(hv_Column1.I, hv_Row1.I);
                    System.Windows.Point recp2 = new System.Windows.Point(hv_Column2.I, hv_Row1.I);
                    System.Windows.Point recp3 = new System.Windows.Point(hv_Column2.I, hv_Row2.I);
                    System.Windows.Point recp4 = new System.Windows.Point(hv_Column1.I, hv_Row2.I);
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();

                    rec1Points.Add(recp1);
                    rec1Points.Add(recp2);
                    rec1Points.Add(recp3);
                    rec1Points.Add(recp4);

                    AddTestResult(cell, DefectSpecies[1].RecipeDefects[0].Name, rec1Points);
                    System.Windows.Point pos = new System.Windows.Point(
                        (hv_Column2.D + hv_Column1.D) / 2.0 - 100,
                        (hv_Row2.D + hv_Row1.D) / 2.0
                    );
                    cell.DrawEdges.Add(new CEdgeDraw($"平均灰度:{hv_Max.D}", pos, Brushes.Red, 15));
                    //tempNgFlag = true;
                    //  }
                    //else
                    //{
                    //    //蓝盖的 找到的区域显示
                    //    System.Windows.Point capp1 = new System.Windows.Point(BlueCapLeftUPX, BlueCapLeftUPY);
                    //    System.Windows.Point capp2 = new System.Windows.Point(BlueCapRightDownX, BlueCapLeftUPY);
                    //    System.Windows.Point capp3 = new System.Windows.Point(BlueCapRightDownX, BlueCapRightDownY);
                    //    System.Windows.Point capp4 = new System.Windows.Point(BlueCapLeftUPX, BlueCapRightDownY);
                    //    List<System.Windows.Point> capPoints2 = new List<System.Windows.Point>();
                    //    capPoints2.Add(capp1);
                    //    capPoints2.Add(capp2);
                    //    capPoints2.Add(capp3);
                    //    capPoints2.Add(capp4);
                    //    capPoints2.Add(capp1);
                    //    cell.DrawEdges.Add(new CEdgeDraw(capPoints2, Brushes.Cyan));
                    //}

                    #endregion 盖子区域显示
                }
                catch (Exception ex)
                {
                    bFlagBlueCap?.Dispose();
                    OperateLog.Info("BottleTest_InspectionBlueCap:" + ex.Message.ToString());
                }

                #endregion 蓝盖有无

                #region 标签有无

                HTuple bFlagLabel = null;
                HTuple LabelMinRow = param.LabelMinRow;
                HTuple LabelMinCol = 0;
                HTuple LabelMaxRow = param.LabelMaxRow;
                HTuple LabelMaxCol = cell.Image.ImageWidth - 1;

                //int labelLeftUPX = 0;
                //int labelLeftUPY = 0;
                //int labelRightDownX = 0;
                //int labelRightDownY = 0;

                try
                {
                    HObject ho_LabelRegion = null;
                    HOperatorSet.GenEmptyObj(out ho_LabelRegion);
                    ho_LabelRegion.Dispose();

                    HTuple hv_Max1 = new HTuple();
                    hv_Max1.Dispose();

                    InspectionLabel(
                        ho_ImageR,
                        out ho_LabelRegion,
                        param.LabelBrightnessMin,
                        param.LabelBrightnessMax,
                        LabelMinRow,
                        LabelMinCol,
                        LabelMaxRow,
                        LabelMaxCol,
                        param.LabelThickness,
                        out hv_Max1
                    );

                    HTuple hv_Row1 = new HTuple(),
                        hv_Column1 = new HTuple();
                    HTuple hv_Row2 = new HTuple(),
                        hv_Column2 = new HTuple();
                    //HObject ho_Rectangle2 = null;
                    //HOperatorSet.GenEmptyObj(out ho_Rectangle2);

                    hv_Row1.Dispose();
                    hv_Column1.Dispose();
                    hv_Row2.Dispose();
                    hv_Column2.Dispose();
                    HOperatorSet.SmallestRectangle1(
                        ho_LabelRegion,
                        out hv_Row1,
                        out hv_Column1,
                        out hv_Row2,
                        out hv_Column2
                    );
                    //ho_Rectangle2.Dispose();
                    //HOperatorSet.GenRectangle1(out ho_Rectangle2, hv_Row1, hv_Column1, hv_Row2,
                    //    hv_Column2);

                    #region 显示结果

                    // bool tempNgFlag = false;
                    //搜索区域显示
                    System.Windows.Point lebelp1 = new System.Windows.Point(
                        LabelMinCol,
                        LabelMinRow
                    );
                    System.Windows.Point lebelp2 = new System.Windows.Point(
                        LabelMaxCol,
                        LabelMinRow
                    );
                    System.Windows.Point lebelp3 = new System.Windows.Point(
                        LabelMaxCol,
                        LabelMaxRow
                    );
                    System.Windows.Point lebelp4 = new System.Windows.Point(
                        LabelMinCol,
                        LabelMaxRow
                    );

                    List<System.Windows.Point> labelPoints = new List<System.Windows.Point>();
                    labelPoints.Add(lebelp1);
                    labelPoints.Add(lebelp2);
                    labelPoints.Add(lebelp3);
                    labelPoints.Add(lebelp4);
                    labelPoints.Add(lebelp1);
                    cell.DrawEdges.Add(new CEdgeDraw(labelPoints, Brushes.Orange));

                    //标签缺失
                    //if (m_LabelTestResult != 2)
                    //{
                    System.Windows.Point reclebelp1 = new System.Windows.Point(
                        hv_Column1.I,
                        hv_Row1.I
                    );
                    System.Windows.Point reclebelp2 = new System.Windows.Point(
                        hv_Column2.I,
                        hv_Row1.I
                    );
                    System.Windows.Point reclebelp3 = new System.Windows.Point(
                        hv_Column2.I,
                        hv_Row2.I
                    );
                    System.Windows.Point reclebelp4 = new System.Windows.Point(
                        hv_Column1.I,
                        hv_Row2.I
                    );
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();

                    rec1Points.Add(reclebelp1);
                    rec1Points.Add(reclebelp2);
                    rec1Points.Add(reclebelp3);
                    rec1Points.Add(reclebelp4);

                    AddTestResult(cell, DefectSpecies[1].RecipeDefects[1].Name, rec1Points);
                    System.Windows.Point pos = new System.Windows.Point(
                        (hv_Column2.D + hv_Column1.D) / 2.0 - 100,
                        (hv_Row2.D + hv_Row1.D) / 2.0
                    );
                    cell.DrawEdges.Add(new CEdgeDraw($"平均灰度:{hv_Max1.D}", pos, Brushes.Red, 15));
                    //tempNgFlag = true;
                    //  }
                    //else
                    //{
                    //    //标签的 找到的区域显示
                    //    System.Windows.Point Labelp1 = new System.Windows.Point(labelLeftUPX, labelLeftUPY);
                    //    System.Windows.Point Labelp2 = new System.Windows.Point(labelRightDownX, labelLeftUPY);
                    //    System.Windows.Point Labelp3 = new System.Windows.Point(labelRightDownX, labelRightDownY);
                    //    System.Windows.Point Labelp4 = new System.Windows.Point(labelLeftUPX, labelRightDownY);
                    //    List<System.Windows.Point> labelPoints2 = new List<System.Windows.Point>();
                    //    labelPoints2.Add(Labelp1);
                    //    labelPoints2.Add(Labelp2);
                    //    labelPoints2.Add(Labelp3);
                    //    labelPoints2.Add(Labelp4);
                    //    labelPoints2.Add(Labelp1);
                    //    cell.DrawEdges.Add(new CEdgeDraw(labelPoints2, Brushes.Cyan));
                    //}

                    #endregion 显示结果
                }
                catch (Exception ex)
                {
                    bFlagLabel?.Dispose();
                    OperateLog.Info("BottleTest_InspectionLabel:" + ex.Message.ToString());
                }

                #endregion 标签有无

                

                #region DET 三期有无

                // base.DetectImage(cell);
                //Mat img = GetMatImage(cell, param);
                //List<BaseResult> sResultInfos = ImageInfer(img, param.Score, param.Nms);
                //// if (sResultInfos.Count == 0) { return; }

                //foreach (var ds in DefectSpecies)
                //{
                //    foreach (var de in ds.RecipeDefects)
                //    {
                //        CellDetection cellDetection1 = new CellDetection();
                //        cellDetection1.Type = ds.Name;
                //        cellDetection1.Category = de.Category;
                //        cellDetection1.RecipeDefectName = de.Name;
                //        cellDetection1.Value = new List<float>();
                //        switch (_ModelType)
                //        {
                //            case ModelType.YOLOv8Det:
                //                DetResult detrets = sResultInfos as DetResult;

                //                detrets.for_each(info =>
                //                {
                //                    int index = int.Parse(info.lable);
                //                    if (text_Model_Names[index] == de.Name)
                //                    {
                //                        SRegion sRegion = GetDetectRegion(info);

                //                        cellDetection1.regionOut.Add(sRegion);
                //                        //infos.Add(info);
                //                    }
                //                });
                //                break;

                //            case ModelType.YOLOv8Obb:
                //                ObbResult obbrets = sResultInfos as ObbResult;
                //                obbrets.for_each(info =>
                //                {
                //                    int index = int.Parse(info.lable);
                //                    if (text_Model_Names[index] == de.Name)
                //                    {
                //                        SRegion sRegion = GetDetectRegion(info);

                //                        cellDetection1.regionOut.Add(sRegion);
                //                        //infos.Add(info);
                //                    }
                //                });
                //                //infos.ForEach(info => sResultInfos.Remove(info));
                //                break;

                //            default:
                //                break;
                //        }

                //        cell.AlgorithmOut.Add(cellDetection1);
                //    }
                //}

                #endregion DET 三期有无

                #region DET 标签缺陷

                // base.DetectImage(cell);
                //BaseResult sResultInfos = ImageInfer(img, param.Score, param.Nms);
                //// if (sResultInfos.Count == 0) { return; }

                //foreach (var ds in DefectSpecies)
                //{
                //    foreach (var de in ds.RecipeDefects)
                //    {
                //        CellDetection cellDetection1 = new CellDetection();
                //        cellDetection1.Type = ds.Name;
                //        cellDetection1.Category = de.Category;
                //        cellDetection1.RecipeDefectName = de.Name;
                //        cellDetection1.Value = new List<float>();
                //        switch (_ModelType)
                //        {
                //            case ModelType.YOLOv8Det:
                //                DetResult detrets = sResultInfos as DetResult;

                //                detrets.for_each(info =>
                //                {
                //                    int index = int.Parse(info.lable);
                //                    if (text_Model_Names[index] == de.Name)
                //                    {
                //                        SRegion sRegion = GetDetectRegion(info);

                //                        cellDetection1.regionOut.Add(sRegion);
                //                        //infos.Add(info);
                //                    }
                //                });
                //                break;

                //            case ModelType.YOLOv8Obb:
                //                ObbResult obbrets = sResultInfos as ObbResult;
                //                obbrets.for_each(info =>
                //                {
                //                    int index = int.Parse(info.lable);
                //                    if (text_Model_Names[index] == de.Name)
                //                    {
                //                        SRegion sRegion = GetDetectRegion(info);

                //                        cellDetection1.regionOut.Add(sRegion);
                //                        //infos.Add(info);
                //                    }
                //                });
                //                //infos.ForEach(info => sResultInfos.Remove(info));
                //                break;

                //            default:
                //                break;
                //        }

                //        cell.AlgorithmOut.Add(cellDetection1);
                //    }
                //}

                #endregion DET 标签缺陷

                #region OBB

                // base.DetectImage(cell);
                //Mat img = GetMatImage(cell, param);
                //List<ObbData> sResultInfos = ImageInfer(img, param.Score, param.Nms);
                //// if (sResultInfos.Count == 0) { return; }

                //foreach (var ds in DefectSpecies)
                //{
                //    foreach (var de in ds.RecipeDefects)
                //    {
                //        CellDetection cellDetection1 = new CellDetection();
                //        cellDetection1.Type = ds.Name;
                //        cellDetection1.Category = de.Category;
                //        cellDetection1.RecipeDefectName = de.Name;
                //        cellDetection1.Value = new List<float>();
                //        List<ObbData> infos = new List<ObbData>();
                //        //if (sResultInfos.Count == 0)
                //        //{
                //        //    if (de.Category == Category.值)
                //        //    {
                //        //        cellDetection1.Value.Add(2.0f); //没识别到三期
                //        //    }

                //        //}
                //        //else
                //        //{
                //        sResultInfos.ForEach(info =>
                //        {
                //            int index = int.Parse(info.lable);
                //            if (Detect_names[index] == de.Name)
                //            {
                //                if (de.Name == "三期") //识别到三期
                //                {
                //                    //cellDetection1.Value.Add(1.0f);
                //                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                //                    info.box.Points().ForEach(p => rec1Points.Add(new System.Windows.Point(p.X, p.Y)));
                //                    rec1Points.Add(rec1Points[0]);
                //                    cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.LightPink));

                //                }
                //                SRegion sRegion = GetDetectRegion(info);
                //                cellDetection1.regionOut.Add(sRegion);
                //                infos.Add(info);

                //            }
                //        });
                //        // }

                //        cell.AlgorithmOut.Add(cellDetection1);
                //        infos.ForEach(info => sResultInfos.Remove(info));
                //    }

                //}

                #endregion OBB
            }
        }

        #region 铝盖定位算法 注释

        //public void LocationCap(HObject ho_IntoImage, HTuple hv_IntoMinThreshold, HTuple hv_IntoMaxThreshold,
        // HTuple hv_MinRow, HTuple hv_MinCol, HTuple hv_MaxRow, HTuple hv_MaxCol, HTuple hv_CapRadius,
        // HTuple hv_CapThickness, out HTuple hv_OutFlagCap)
        //{
        //    // Stack for temporary objects
        //    HObject[] OTemp = new HObject[20];

        //    // Local iconic variables

        //    HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_Rectangle;
        //    HObject ho_ImageReduced, ho_Region, ho_RegionOpening, ho_SelectedRegions;

        //    // Local control variables

        //    HTuple hv_OutRow1 = new HTuple(), hv_OutColumn1 = new HTuple();
        //    HTuple hv_OutRow2 = new HTuple(), hv_OutColumn2 = new HTuple();
        //    HTuple hv_tempLength = new HTuple();
        //    // Initialize local and output iconic variables
        //    HOperatorSet.GenEmptyObj(out ho_ImageR);
        //    HOperatorSet.GenEmptyObj(out ho_ImageG);
        //    HOperatorSet.GenEmptyObj(out ho_ImageB);
        //    HOperatorSet.GenEmptyObj(out ho_Rectangle);
        //    HOperatorSet.GenEmptyObj(out ho_ImageReduced);
        //    HOperatorSet.GenEmptyObj(out ho_Region);
        //    HOperatorSet.GenEmptyObj(out ho_RegionOpening);
        //    HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
        //    hv_OutFlagCap = new HTuple();
        //    //get_image_size (IntoImage, Width, Height)
        //    ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
        //    HOperatorSet.Decompose3(ho_IntoImage, out ho_ImageR, out ho_ImageG, out ho_ImageB
        //        );

        //    ho_Rectangle.Dispose();
        //    HOperatorSet.GenRectangle1(out ho_Rectangle, hv_MinRow, hv_MinCol, hv_MaxRow,
        //        hv_MaxCol);
        //    ho_ImageReduced.Dispose();
        //    HOperatorSet.ReduceDomain(ho_ImageB, ho_Rectangle, out ho_ImageReduced);

        //    ho_Region.Dispose();
        //    HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, hv_IntoMinThreshold, 255);

        //    ho_RegionOpening.Dispose();
        //    HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, 1, hv_CapThickness);
        //    {
        //        HObject ExpTmpOutVar_0;
        //        HOperatorSet.OpeningRectangle1(ho_RegionOpening, out ExpTmpOutVar_0, hv_CapRadius,
        //            1);
        //        ho_RegionOpening.Dispose();
        //        ho_RegionOpening = ExpTmpOutVar_0;
        //    }

        //    ho_SelectedRegions.Dispose();
        //    HOperatorSet.SelectShapeStd(ho_RegionOpening, out ho_SelectedRegions, "max_area",
        //        70);
        //    hv_OutRow1.Dispose(); hv_OutColumn1.Dispose(); hv_OutRow2.Dispose(); hv_OutColumn2.Dispose();
        //    HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_OutRow1, out hv_OutColumn1,
        //        out hv_OutRow2, out hv_OutColumn2);

        //    hv_tempLength.Dispose();
        //    using (HDevDisposeHelper dh = new HDevDisposeHelper())
        //    {
        //        hv_tempLength = hv_OutColumn2 - hv_OutColumn1;
        //    }
        //    //1 是铝盖缺失 2是有铝盖
        //    if ((int)(new HTuple(hv_tempLength.TupleGreater(hv_CapRadius))) != 0)
        //    {
        //        hv_OutFlagCap.Dispose();
        //        hv_OutFlagCap = 2;
        //    }
        //    else
        //    {
        //        hv_OutFlagCap.Dispose();
        //        hv_OutFlagCap = 1;
        //    }

        //    ho_ImageR.Dispose();
        //    ho_ImageG.Dispose();
        //    ho_ImageB.Dispose();
        //    ho_Rectangle.Dispose();
        //    ho_ImageReduced.Dispose();
        //    ho_Region.Dispose();
        //    ho_RegionOpening.Dispose();
        //    ho_SelectedRegions.Dispose();

        //    hv_OutRow1.Dispose();
        //    hv_OutColumn1.Dispose();
        //    hv_OutRow2.Dispose();
        //    hv_OutColumn2.Dispose();
        //    hv_tempLength.Dispose();

        //    return;
        //}

        #endregion 铝盖定位算法 注释

        #region 蓝盖缺失算法

        public void InspectionBlueCap(
            HObject ho_ImageB,
            HObject ho_ImageR,
            out HObject ho_CapRegion,
            HTuple hv_IntoMinThreshold,
            HTuple hv_IntoMaxThreshold,
            HTuple hv_MinRow,
            HTuple hv_MinColumn,
            HTuple hv_MaxRow,
            HTuple hv_MaxColumn,
            HTuple hv_CapThickness,
            out HTuple hv_Max
        )
        {
            // Local iconic variables

            HObject ho_ImageSub,
                ho_Rectangle,
                ho_ImageReduced;
            HObject ho_Region,
                ho_RegionOpening2;

            // Local control variables

            HTuple hv_Min = new HTuple(),
                hv_Range = new HTuple();
            // Initialize local and output iconic variables
            HOperatorSet.GenEmptyObj(out ho_CapRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageSub);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening2);
            hv_Max = new HTuple();
            ho_CapRegion.Dispose();
            HOperatorSet.GenEmptyObj(out ho_CapRegion);
            ho_ImageSub.Dispose();
            HOperatorSet.SubImage(ho_ImageB, ho_ImageR, out ho_ImageSub, 1, 50);
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(
                out ho_Rectangle,
                hv_MinRow,
                hv_MinColumn,
                hv_MaxRow,
                hv_MaxColumn
            );
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageSub, ho_Rectangle, out ho_ImageReduced);
            ho_Region.Dispose();
            HOperatorSet.Threshold(
                ho_ImageReduced,
                out ho_Region,
                hv_IntoMinThreshold,
                hv_IntoMaxThreshold
            );
            ho_RegionOpening2.Dispose();
            HOperatorSet.OpeningRectangle1(
                ho_Region,
                out ho_RegionOpening2,
                hv_CapThickness,
                hv_CapThickness
            );
            ho_CapRegion.Dispose();
            HOperatorSet.SelectShapeStd(ho_RegionOpening2, out ho_CapRegion, "max_area", 70);
            hv_Min.Dispose();
            hv_Max.Dispose();
            hv_Range.Dispose();
            HOperatorSet.MinMaxGray(
                ho_CapRegion,
                ho_ImageReduced,
                50,
                out hv_Min,
                out hv_Max,
                out hv_Range
            );
            ho_ImageSub.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionOpening2.Dispose();

            hv_Min.Dispose();
            hv_Range.Dispose();

            return;
        }

        #endregion 蓝盖缺失算法

        #region 标签缺失算法

        public void InspectionLabel(
            HObject ho_IntoImage,
            out HObject ho_LabelRegion,
            HTuple hv_IntoMinThreshold,
            HTuple hv_IntoMaxThreshold,
            HTuple hv_MinRow,
            HTuple hv_MinColumn,
            HTuple hv_MaxRow,
            HTuple hv_MaxColumn,
            HTuple hv_LabelOpeningCircle,
            out HTuple hv_Max
        )
        {
            // Local iconic variables

            HObject ho_Rectangle,
                ho_ImageReduced,
                ho_Region;

            // Local control variables

            HTuple hv_Min = new HTuple(),
                hv_Range = new HTuple();
            // Initialize local and output iconic variables
            HOperatorSet.GenEmptyObj(out ho_LabelRegion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            hv_Max = new HTuple();
            ho_LabelRegion.Dispose();
            HOperatorSet.GenEmptyRegion(out ho_LabelRegion);
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(
                out ho_Rectangle,
                hv_MinRow,
                hv_MinColumn,
                hv_MaxRow,
                hv_MaxColumn
            );
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_IntoImage, ho_Rectangle, out ho_ImageReduced);

            ho_Region.Dispose();
            HOperatorSet.Threshold(
                ho_ImageReduced,
                out ho_Region,
                hv_IntoMinThreshold,
                hv_IntoMaxThreshold
            );

            //opening_rectangle1 (Region, RegionOpening, LabelOpeningCircle, LabelOpeningCircle)
            ho_LabelRegion.Dispose();
            HOperatorSet.OpeningCircle(ho_Region, out ho_LabelRegion, hv_LabelOpeningCircle);
            hv_Min.Dispose();
            hv_Max.Dispose();
            hv_Range.Dispose();
            HOperatorSet.MinMaxGray(
                ho_LabelRegion,
                ho_ImageReduced,
                50,
                out hv_Min,
                out hv_Max,
                out hv_Range
            );
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();

            hv_Min.Dispose();
            hv_Range.Dispose();

            return;
        }

        #endregion 标签缺失算法

        #region halcon OCR算法

        /// <summary>
        /// 2025.01.09 鲍赞宝
        /// 初始化halcon Ocr句柄
        /// </summary>
        /// <returns></returns>
        private bool InitialHalconOcrLib()
        {
            try
            {
                string tempOCRLibPath = ".\\AlgorithmPlug\\BottleAlgorithm\\Universal_Rej.occ";
                if (File.Exists(tempOCRLibPath))
                {
                    //halcon OCR库
                    HOperatorSet.ReadOcrClassCnn(tempOCRLibPath, out m_OCRHandle);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OcrImageInspection(
            HObject ho_IntoImage,
            HObject ho_IntoCropRegion,
            HTuple hv_OCRHandle,
            HTuple hv_IntoCharacterW,
            HTuple hv_IntoCharacterH,
            HTuple hv_DeltaBrightness,
            HTuple hv_ImageChannel,
            out HTuple hv_OutRow1,
            out HTuple hv_OutColumn1,
            out HTuple hv_OutRow2,
            out HTuple hv_OutColumn2,
            out HTuple hv_TextClass
        )
        {
            // Local iconic variables

            HObject ho_Image = null,
                ho_ImageG = null,
                ho_ImageB = null;
            HObject ho_ImageR = null,
                ho_ImageReduced,
                ho_Region,
                ho_RegionClosing;
            HObject ho_ConnectedRegions,
                ho_SelectedRegions,
                ho_SortedRegions;
            HObject ho_RegionUnion;

            // Local control variables

            HTuple hv_Confidence = new HTuple(),
                hv_TextCount = new HTuple();
            HTuple hv_Index = new HTuple();
            // Initialize local and output iconic variables
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SortedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            hv_OutRow1 = new HTuple();
            hv_OutColumn1 = new HTuple();
            hv_OutRow2 = new HTuple();
            hv_OutColumn2 = new HTuple();
            hv_TextClass = new HTuple();
            switch (hv_ImageChannel.I)
            {
                case 1:
                    ho_Image.Dispose();
                    ho_ImageG.Dispose();
                    ho_ImageB.Dispose();
                    HOperatorSet.Decompose3(
                        ho_IntoImage,
                        out ho_Image,
                        out ho_ImageG,
                        out ho_ImageB
                    );
                    break;

                case 2:
                    ho_ImageR.Dispose();
                    ho_Image.Dispose();
                    ho_ImageB.Dispose();
                    HOperatorSet.Decompose3(
                        ho_IntoImage,
                        out ho_ImageR,
                        out ho_Image,
                        out ho_ImageB
                    );
                    break;

                case 3:
                    ho_ImageR.Dispose();
                    ho_ImageG.Dispose();
                    ho_Image.Dispose();
                    HOperatorSet.Decompose3(
                        ho_IntoImage,
                        out ho_ImageR,
                        out ho_ImageG,
                        out ho_Image
                    );
                    break;
            }

            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_IntoCropRegion, out ho_ImageReduced);
            ho_Region.Dispose();
            HOperatorSet.VarThreshold(
                ho_ImageReduced,
                out ho_Region,
                hv_IntoCharacterW,
                hv_IntoCharacterH,
                0.2,
                hv_DeltaBrightness,
                "dark"
            );
            ho_RegionClosing.Dispose();
            HOperatorSet.ClosingCircle(ho_Region, out ho_RegionClosing, 1.5);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(
                ho_ConnectedRegions,
                out ho_SelectedRegions,
                "area",
                "and",
                10,
                99999
            );
            ho_SortedRegions.Dispose();
            HOperatorSet.SortRegion(
                ho_SelectedRegions,
                out ho_SortedRegions,
                "character",
                "true",
                "row"
            );
            hv_TextClass.Dispose();
            hv_Confidence.Dispose();
            HOperatorSet.DoOcrMultiClassCnn(
                ho_SortedRegions,
                ho_ImageReduced,
                hv_OCRHandle,
                out hv_TextClass,
                out hv_Confidence
            );

            hv_TextCount.Dispose();
            HOperatorSet.TupleLength(hv_TextClass, out hv_TextCount);
            HTuple end_val24 = hv_TextCount - 1;
            HTuple step_val24 = 1;
            for (
                hv_Index = 0;
                hv_Index.Continue(end_val24, step_val24);
                hv_Index = hv_Index.TupleAdd(step_val24)
            )
            {
                if ((int)(new HTuple(((hv_TextClass.TupleSelect(hv_Index))).TupleEqual("o"))) != 0)
                {
                    if (hv_TextClass == null)
                        hv_TextClass = new HTuple();
                    hv_TextClass[hv_Index] = "0";
                    continue;
                }

                if ((int)(new HTuple(((hv_TextClass.TupleSelect(hv_Index))).TupleEqual("O"))) != 0)
                {
                    if (hv_TextClass == null)
                        hv_TextClass = new HTuple();
                    hv_TextClass[hv_Index] = "0";
                    continue;
                }

                if ((int)(new HTuple(((hv_TextClass.TupleSelect(hv_Index))).TupleEqual("l"))) != 0)
                {
                    if (hv_TextClass == null)
                        hv_TextClass = new HTuple();
                    hv_TextClass[hv_Index] = "1";
                    continue;
                }
            }

            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_SortedRegions, out ho_RegionUnion);
            hv_OutRow1.Dispose();
            hv_OutColumn1.Dispose();
            hv_OutRow2.Dispose();
            hv_OutColumn2.Dispose();
            HOperatorSet.SmallestRectangle1(
                ho_RegionUnion,
                out hv_OutRow1,
                out hv_OutColumn1,
                out hv_OutRow2,
                out hv_OutColumn2
            );

            ho_Image.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageR.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionClosing.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SortedRegions.Dispose();
            ho_RegionUnion.Dispose();

            hv_Confidence.Dispose();
            hv_TextCount.Dispose();
            hv_Index.Dispose();

            return;
        }

        /// <summary>
        /// 检测字符的有无
        /// </summary>
        /// <param name="IntoImage"></param>
        /// <param name="IntoStandardDate"></param>
        /// <param name="param"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        //private bool HalconOCRInspection(HObject IntoImage, CBottleParam param, ref Cell cell)
        //{
        //    HTuple tempW1 = null, tempH1 = null;

        //    HTuple hv_OutRow1 = null;
        //    HTuple hv_OutColumn1 = null;
        //    HTuple hv_OutRow2 = null;
        //    HTuple hv_OutColumn2 = null;
        //    HTuple hv_Characters = null;

        //    bool tempHaveTextFlag = false;
        //    try
        //    {
        //        HOperatorSet.GetImageSize(IntoImage, out tempW1, out tempH1);
        //        int m_DateCropX = 0; // 切图的起始X坐标
        //        int m_DateCropY = param.DateMinRow; // 切图的起始Y坐标
        //        int m_DateCropWidth = tempW1; // 切图的宽度
        //        int m_DateCropHeight = param.DateMaxRow - param.DateMinRow + 1; // 切图的高度

        //        if (m_bFlagInitialAlgorithmParam)
        //        {
        //            HOperatorSet.GenEmptyObj(out m_DateCropRegion);
        //            m_DateCropRegion.Dispose();

        //            switch (param.DateAngleIndex)
        //            {
        //                case DATAROTATION.ROTA0:
        //                    HOperatorSet.GenRectangle1(out m_DateCropRegion, m_DateCropY, m_DateCropX, m_DateCropY + m_DateCropHeight - 1, m_DateCropX + m_DateCropWidth - 1);
        //                    break;

        //                case DATAROTATION.ROTA90:
        //                    HOperatorSet.GenRectangle1(out m_DateCropRegion, tempW1 - (m_DateCropX + m_DateCropWidth), m_DateCropY, tempW1 - m_DateCropX - 1, m_DateCropY + m_DateCropHeight - 1);
        //                    break;

        //                case DATAROTATION.ROTA180:
        //                    HOperatorSet.GenRectangle1(out m_DateCropRegion, tempH1 - (m_DateCropY + m_DateCropHeight), tempW1 - (m_DateCropX + m_DateCropWidth), tempH1 - m_DateCropY - 1, tempW1 - m_DateCropX - 1);
        //                    break;

        //                case DATAROTATION.ROTA270:
        //                    HOperatorSet.GenRectangle1(out m_DateCropRegion, m_DateCropX, tempH1 - (m_DateCropY + m_DateCropHeight), m_DateCropX + m_DateCropWidth - 1, tempH1 - m_DateCropY - 1);
        //                    break;
        //                default:
        //                    break;
        //            }

        //            m_bFlagInitialAlgorithmParam = false;
        //        }

        //        HObject tempDateImage = null;

        //        HOperatorSet.GenEmptyObj(out tempDateImage);
        //        tempDateImage.Dispose();

        //        switch (param.DateAngleIndex)
        //        {
        //            case DATAROTATION.ROTA0:
        //                OcrImageInspection(IntoImage, m_DateCropRegion, m_OCRHandle, 15, 10, 20, 2, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
        //                break;

        //            case DATAROTATION.ROTA90:
        //                HOperatorSet.RotateImage(IntoImage, out tempDateImage, 270, "constant");
        //                OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, 20, 2, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
        //                break;

        //            case DATAROTATION.ROTA180:
        //                HOperatorSet.RotateImage(IntoImage, out tempDateImage, 180, "constant");
        //                OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, 20, 2, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
        //                break;

        //            case DATAROTATION.ROTA270:
        //                HOperatorSet.RotateImage(IntoImage, out tempDateImage, 90, "constant");
        //                OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, 20, 2, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);

        //                break;
        //            default:
        //                break;
        //        }

        //        string tempResult = hv_Characters.ToString();

        //        if (hv_OutRow1.Length > 0)
        //        {
        //            if (tempResult.Length > 2)
        //            {
        //                //挑选出数字日期
        //                string temppattern = @"[^a-zA-Z0-9]";
        //                tempResult = tempResult.Substring(1, tempResult.Length - 2);
        //                tempResult = Regex.Replace(tempResult, temppattern, "");

        //                // cell.OcrResultString = tempResult;

        //                if (tempResult.Length > 0)
        //                {
        //                    tempHaveTextFlag = true;
        //                }

        //            }
        //            else
        //            {
        //                // cell.OcrResultString = "";
        //            }
        //        }
        //        else
        //        {
        //            //  cell.OcrResultString = "";
        //        }
        //        cell.DrawEdges.Clear();

        //        List<System.Windows.Point> tempPointList = new List<System.Windows.Point>();
        //        if (tempHaveTextFlag)
        //        {
        //            System.Windows.Point tempPoint = new System.Windows.Point();

        //            switch (param.DateAngleIndex)
        //            {
        //                case DATAROTATION.ROTA0:
        //                    tempPoint.X = hv_OutColumn1.D + m_DateCropX;
        //                    tempPoint.Y = hv_OutRow1.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    tempPoint.X = hv_OutColumn2.D + m_DateCropX;
        //                    tempPoint.Y = hv_OutRow2.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    break;
        //                case DATAROTATION.ROTA90:
        //                    tempPoint.X = tempW1 - hv_OutRow2.D - 1 + m_DateCropX;
        //                    tempPoint.Y = hv_OutColumn1.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    tempPoint.X = tempW1 - hv_OutRow1.D - 1 + m_DateCropX;
        //                    tempPoint.Y = hv_OutColumn2.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);
        //                    break;

        //                case DATAROTATION.ROTA180:
        //                    tempPoint.X = tempW1 - hv_OutColumn2.D + m_DateCropX;
        //                    tempPoint.Y = tempH1 - hv_OutRow2.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    tempPoint.X = tempW1 - hv_OutColumn1.D + m_DateCropX;
        //                    tempPoint.Y = tempH1 - hv_OutRow1.D + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    break;

        //                case DATAROTATION.ROTA270:
        //                    tempPoint.X = hv_OutRow1.D + m_DateCropX;
        //                    tempPoint.Y = tempH1 - hv_OutColumn2.D - 1 + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    tempPoint.X = hv_OutRow2.D + m_DateCropX;
        //                    tempPoint.Y = tempH1 - hv_OutColumn1.D - 1 + m_DateCropY;
        //                    tempPointList.Add(tempPoint);

        //                    break;
        //                default:
        //                    break;
        //            }

        //            cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
        //        }
        //        else
        //        {
        //            System.Windows.Point tempPoint = new System.Windows.Point();
        //            tempPoint.X = 0;
        //            tempPoint.Y = 0;
        //            tempPointList.Add(tempPoint);

        //            tempPoint.X = 0;
        //            tempPoint.Y = 0;
        //            tempPointList.Add(tempPoint);

        //            cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
        //        }

        //        tempW1?.Dispose();
        //        tempH1?.Dispose();
        //        hv_OutRow1?.Dispose();
        //        hv_OutColumn1?.Dispose();
        //        hv_OutRow2?.Dispose();
        //        hv_OutColumn2?.Dispose();
        //        hv_Characters?.Dispose();

        //    }
        //    catch (Exception ex)
        //    {
        //        tempW1?.Dispose();
        //        tempH1?.Dispose();
        //        hv_OutRow1?.Dispose();
        //        hv_OutColumn1?.Dispose();
        //        hv_OutRow2?.Dispose();
        //        hv_OutColumn2?.Dispose();
        //        hv_Characters?.Dispose();

        //        OperateLog.Info("BottleTest_ocr_1:" + ex.Message.ToString());
        //    }
        //    return tempHaveTextFlag;

        //}

        #endregion halcon OCR算法

        #region 添加检测结果

        /// <summary>
        /// 添加测试结果
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="IntoDefectName"></param>
        private void AddTestResult(
            Cell cell,
            string IntoDefectName,
            List<System.Windows.Point> rec1Points
        )
        {
            foreach (var ds in DefectSpecies)
            {
                foreach (var de in ds.RecipeDefects)
                {
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = ds.Name;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();
                    if(de.Name == IntoDefectName)
                    {
                        SRegion detectRegion = GetDetectRegion(rec1Points);
                        cellDetection1.regionOut.Add(detectRegion);
                        cell.AlgorithmOut.Add(cellDetection1);
                    }
                    
                    //infos.ForEach(info => sResultInfos.Remove(info));
                }
            }
            //var de = cell.AlgorithmOut.Find(n => n.RecipeDefectName == IntoDefectName);
            
        }

        public SRegion GetDetectRegion(List<System.Windows.Point> rec1Points)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.ShorLen = rec1Points[2].Y - rec1Points[1].Y;
            sRegioninfo.LongLen = rec1Points[1].X - rec1Points[0].X;

            sRegioninfo.WidthBound = rec1Points[1].X - rec1Points[0].X;
            sRegioninfo.HeightBound = rec1Points[2].Y - rec1Points[1].Y;
            sRegioninfo.Area = sRegioninfo.WidthBound * sRegioninfo.HeightBound;
            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            detectRegion.rect = new System.Windows.Rect(
                new System.Windows.Point(rec1Points[0].X, rec1Points[0].Y),
                new System.Windows.Point(rec1Points[2].X, rec1Points[2].Y)
            );
            return detectRegion;
        }

        #endregion 添加检测结果
    }

    public partial class CBottleParam : CParam
    {
        public CBottleParam()
            : base() { }

        public CBottleParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2025.04.05 鲍赞宝
        /// 强制设定测试结果："ok"-强制设定测试结果全部为ok，"ng"-强制设定测试结果全部为ng,""-实际测试结果
        /// </summary>
        [ObservableProperty]
        [property: Category("测试结果设定")]
        [property: DisplayName("测试结果设定")]
        [property: Description("测试结果设定")]
        private String inspectionResult = "";

        /// <summary>
        /// 2025.03.15 鲍赞宝
        /// 柱形物体的半径，单位是mm
        /// </summary>
        [ObservableProperty]
        [property: Category("Algorithm")]
        [property: DisplayName("西林瓶的半径mm")]
        [property: Description("西林瓶的半径mm")]
        private double cylinderRadiusMM = 11;

        /// <summary>
        /// 2025.03.15 鲍赞宝
        /// 图像的像素实际尺寸，单位是mm
        /// </summary>
        [ObservableProperty]
        [property: Category("Algorithm")]
        [property: DisplayName("图像的像素尺寸mm")]
        [property: Description("图像的像素尺寸mm")]
        private double pixelSizeMM = 0.058;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 标签位置最上面的行位置
        /// </summary>
        [ObservableProperty]
        [property: Category("标签检测")]
        [property: DisplayName("01.标签位置上限")]
        [property: Description("标签位置上限")]
        private int labelMinRow = 0;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 标签位置最下面的行位置
        /// </summary>
        [ObservableProperty]
        [property: Category("标签检测")]
        [property: DisplayName("02.标签位置下限")]
        [property: Description("标签位置下限")]
        private int labelMaxRow = 380;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 标签位置亮度下限
        /// </summary>
        [ObservableProperty]
        [property: Category("标签检测")]
        [property: DisplayName("03.标签亮度下限")]
        [property: Description("标签亮度下限")]
        private int labelBrightnessMin = 60;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 标签位置亮度上限
        /// </summary>
        [ObservableProperty]
        [property: Category("标签检测")]
        [property: DisplayName("04.标签亮度上限")]
        [property: Description("标签亮度上限")]
        private int labelBrightnessMax = 255;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 瓶子直径
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("标签检测")]
        //[property: DisplayName("05.瓶子直径")]
        //[property: Description("瓶子直径")]
        //private int bottleRadius = 100;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 过滤
        /// </summary>
        [ObservableProperty]
        [property: Category("标签检测")]
        [property: DisplayName("05.开运算(pix)")]
        [property: Description("开运算(pix)")]
        private float labelThickness = 3.5f;

        ///// <summary>
        ///// 2025.01.09 鲍赞宝
        ///// 日期值
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("日期值")]
        //[property: Description("日期值")]
        //private string standardDate = "20240802";

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 标签位置最上面的行位置
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("三期位置上限")]
        //[property: Description("三期位置上限")]
        //private int dateMinRow = 0;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 标签位置最下面的行位置
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("三期位置下限")]
        //[property: Description("三期位置下限")]
        //private int dateMaxRow = 1000;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 日期亮度差
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("日期亮度差")]
        //[property: Description("日期亮度差")]
        //private int dateDeltaBrightness = 20;

        ///// <summary>
        ///// 2025.03.10 鲍赞宝
        ///// 日期方向
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("日期方向")]
        //[property: Description("日期方向")]
        //private DATAROTATION dateAngleIndex = DATAROTATION.ROTA0;

        ///// <summary>
        ///// 2025.04.22鲍赞宝
        ///// 日期方向
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("三期检测")]
        //[property: DisplayName("算法选择")]
        //[property: Description("算法选择")]
        //private OCRSELECT libIndex = OCRSELECT.HOCR;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 瓶盖位置最下面的行位置
        /// </summary>
        [ObservableProperty]
        [property: Category("瓶盖检测参数")]
        [property: DisplayName("01.瓶盖位置上限")]
        [property: Description("瓶盖位置上限")]
        private int capMinRow = 0;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 瓶盖位置最下面的行位置
        /// </summary>
        [ObservableProperty]
        [property: Category("瓶盖检测参数")]
        [property: DisplayName("02.瓶盖位置下限")]
        [property: Description("瓶盖位置下限")]
        private int capMaxRow = 100;

        [ObservableProperty]
        [property: Category("瓶盖检测参数")]
        [property: DisplayName("03.蓝盖亮度下限")]
        [property: Description("蓝盖亮度下限")]
        private int blueCapBrightnessMin = 80;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 蓝盖亮度
        /// </summary>
        [ObservableProperty]
        [property: Category("瓶盖检测参数")]
        [property: DisplayName("04.蓝盖亮度上限")]
        [property: Description("蓝盖亮度上限")]
        private int blueCapBrightnessMax = 255;

        /// <summary>
        /// 2025.03.20 鲍赞宝
        /// 蓝盖厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("瓶盖检测参数")]
        [property: DisplayName("05.开运算边长（pix)")]
        [property: Description("开运算边长（pix)")]
        private int blueCapThickness = 5;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 蓝盖直径
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("瓶盖检测参数")]
        //[property: DisplayName("06.蓝盖直径")]
        //[property: Description("蓝盖直径")]
        //private int blueCapRadius = 300;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 铝盖亮度
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("瓶盖检测参数")]
        //[property: DisplayName("07.铝盖亮度下限")]
        //[property: Description("铝盖亮度下限")]
        //private int capBrightnessMin = 100;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 铝盖亮度
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("瓶盖检测参数")]
        //[property: DisplayName("08.铝盖亮度上限")]
        //[property: Description("铝盖亮度上限")]
        //private int capBrightnessMax = 255;

        ///// <summary>
        ///// 2025.03.20 鲍赞宝
        ///// 铝盖厚度
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("瓶盖检测参数")]
        //[property: DisplayName("09.铝盖过滤")]
        //[property: Description("铝盖过滤")]
        //private int capThickness = 5;

        ///// <summary>
        ///// 2025.04.23 鲍赞宝
        ///// 铝盖直径
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("瓶盖检测参数")]
        //[property: DisplayName("10.铝盖直径")]
        //[property: Description("铝盖直径")]
        //private int capRadius = 300;
    }

    //public enum OCRSELECT
    //{
    //    HOCR = 0,//Halcon OCR

    //    POCR = 1, //PaddleOCR
    //}

    //public enum DATAROTATION
    //{
    //    ROTA0 = 0,
    //    ROTA90 = 90,
    //    ROTA180 = 180,
    //    ROTA270 = 270,
    //}
}