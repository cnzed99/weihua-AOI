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
using System.Drawing.Imaging;
using OpenCvSharp;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ValueConverters;
using System;
using System.Diagnostics;
using WH.Entity.LogRecord;
using System.IO;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using System.Runtime.Serialization;
//using System.Runtime.Serialization;


namespace BottleInspectionAlgorithm
{
    /// <summary>
    /// 2025.03.16 易群生
    /// 图像信息
    /// </summary>
    struct CameraImageInfoStruct
    {
        public HObject CameraImage;
        public int CameraIndex;
    }
    /// <summary>
    /// 2025.01.09 易群生
    /// 算法参数派生类
    /// </summary>

    public class CAlgorithmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// 2025.01.09初始化算法
        /// </summary>
        private bool m_bFlagInitialAlgorithmParam;

        /// <summary>
        /// 2025.01.09 易群生
        /// halcon Ocr句柄
        /// </summary>
        private static HTuple m_OCRHandle;

        /// <summary>
        /// 2025.01.21 OCR识别引擎
        /// </summary>
        //private static PaddleOCREngine m_PaddleOCREngine;

        /// <summary>
        /// 2025.01.21  OCR库的选择：0-halcon OCR库，1-百度paddle OCR库
        /// </summary>
        private static int m_LibIndex;

        /// <summary>
        /// 2025.01.09 易群生
        /// halcon OCR检测区域
        /// </summary>
        private HObject m_DateCropRegion;

        /// <summary>
        /// 2025.01.21 易群生
        /// 百度OCR检测区域
        /// </summary>
        private System.Drawing.Rectangle m_DateCropRect;

        /// <summary>
        /// 2025.01.21 易群生
        /// 三期切图的起始X坐标
        /// </summary>
        private int m_DateCropX;

        /// <summary>
        /// 2025.01.21 易群生
        /// 三期切图的起始Y坐标
        /// </summary>
        private int m_DateCropY;

        /// <summary>
        /// 2025.01.21 易群生
        /// 三期切图的宽度
        /// </summary>
        private int m_DateCropWidth;

        /// <summary>
        /// 2025.01.21 易群生
        /// 三期切图的高度
        /// </summary>
        private int m_DateCropHeight;


        /// <summary>
        /// 2025.03.15 易群生
        /// 初始化同步事件
        /// </summary>
        private static bool m_bFlagCountdownEvent = true;

        /// <summary>
        /// 2025.03.15 易群生
        /// 图像的像素实际尺寸，单位是m
        /// </summary>
        private double m_PixelSize;

        /// <summary>
        /// 2025.03.18 易群生
        /// 等待4个相机图像展开的锁
        /// </summary>
        private static readonly object m_WaitImage_globalLock = new object();

        /// <summary>
        /// 2025.03.17 易群生
        /// 4个相机同时拍照的图片，用以进行柱形图像展开
        /// </summary>
        //private static List<CameraImageInfoStruct> CameraImageStructList = new List<CameraImageInfoStruct>();

        /// <summary>
        ///  2025.03.17 易群生
        /// 初始化OCR库，只初始化一次
        /// </summary>
        private volatile static bool m_OcrLibInitialFlag = true;

        /// <summary>
        /// 2025.03.17 易群生
        /// 相机拍照的时间序号
        /// </summary>
        private int PhotoID;

        /// <summary>
        /// 2025.03.18 易群生
        /// 三期日期
        /// </summary>
        private volatile static string DateString = "";

        /// <summary>
        /// 2025.03.18 易群生
        /// 合并三期字符串的锁
        /// </summary>
        private static readonly object m_concatenate_globalLock = new object();

        /// <summary>
        /// 2025.03.18 易群生
        /// 合并字符串的数量
        /// </summary>
        private volatile static int m_DateStringCount = 0;

        /// <summary>
        /// 2025.03.18 易群生
        /// 字符串测试结果：0-待测，1-ok，2-Ng
        /// </summary>
        private volatile static int m_DateStringTestResult = 0;

        /// <summary>
        //2025.03.18 易群生
        //检测瓶盖有无,只有一个相机要检测，瓶盖位置大于0的相机才会检测
        //1是瓶盖ok，0是瓶盖缺失，2是待处理
        /// </summary>

        private volatile static int BlueCapTestResult = 2;
        private volatile static int CapTestResult = 2;


        /// <summary>
        /// 2025.03.23 易群生
        /// AI测试结果，0是ok，大于0是ng类型
        /// </summary>
        private volatile static int AITestResult = 0;

        /// <summary>
        /// 2025.03.23 易群生
        /// AI测试结果，0是ok，1是ng,2是待测
        /// </summary>
        private volatile static int[] AITestResultAry = {0,0,0,0};


        /// <summary>
        //2025.03.18 易群生
        //检测标签有无,4个相机都要检测
        //1是标签存在，0是标签缺失
        /// </summary>
        private volatile static int LabelTestResult = 0;

        /// <summary>
        /// 相机编号
        /// </summary>
        private int CameraIndex = 0;

        /// <summary>
        /// 等待其他3个相机的测试结果
        /// </summary>
        static CountdownEvent[] WaitTestResultCountdownEvent;

        /// <summary>
        /// 等待其他3个相机结果合并完成
        /// </summary>
        static CountdownEvent[] WaitCombineResultCountdownEvent;

        /// <summary>
        /// 等待其他状态初始化完成
        /// </summary>
        static CountdownEvent[] WaitTestInitialCountdownEvent;

        private YOLO yolo = new YOLO();

        private string infer_type = "obb";

         private string engine_type_str = "OpenVINO";
        //private string engine_type_str = "TensorRT";

        string Model_Path = "D:\\yqs\\xilinping.onnx";
        string name_Path = "D:\\yqs\\classes.txt";

        string[] Detect_names;



        public CAlgorithmParam()
            : base()
        {
            //AlgorithmType = "OCRDateAlgorithm";
            DefectSpecies = new()
            {
                new("西林瓶异常类", new() { new("日期漏印", Category.区域),new("日期缺印", Category.区域),new("瓶盖缺失", Category.区域),
                    new("标签缺失", Category.区域),new("标签破损", Category.区域),new("标签褶皱", Category.区域),new("标签重贴", Category.区域)}),
                new(
                    "异常类",
                    new() { new("没有产品", Category.值)}
                ),
            };
            DefectFeatures = new();

            if (m_bFlagCountdownEvent)
            {
                m_bFlagCountdownEvent = false;
                
                WaitTestResultCountdownEvent = new CountdownEvent[4];
                WaitCombineResultCountdownEvent = new CountdownEvent[4];
                WaitTestInitialCountdownEvent = new CountdownEvent[4];

                for (int i = 0; i < 4; i++)
                {
                    WaitTestResultCountdownEvent[i] = new CountdownEvent(3);
                    WaitCombineResultCountdownEvent[i] = new CountdownEvent(3);
                    WaitTestInitialCountdownEvent[i] = new CountdownEvent(3);
                }
            }

            PhotoID = 0;


            m_bFlagInitialAlgorithmParam = true;

            if (m_OcrLibInitialFlag)
            {
                m_OCRHandle = null;
                //m_PaddleOCREngine = null;

                m_LibIndex = 0;

                switch (m_LibIndex)
                {
                    case 0:
                        InitialHalconOcrLib();
                        break;
                    case 1:
                        InitialPaddleOcrLib();
                        break;
                    default:
                        break;
                }

                m_OcrLibInitialFlag = false;
            }

            if (File.Exists(name_Path))
            {
                Detect_names = File.ReadAllLines(name_Path);
            }

        }

        /// <summary>
        /// AI检测标签破损、标签褶皱和标签重贴
        /// </summary>
        /// <param name="cell"></param>
        public int AIDetectImage(Cell cell)
        {
            int tempResult = 0;
            List<ObbData> sResultInfos = ImageInfer(cell);

            foreach (var ds in DefectSpecies)
            {
                foreach (var de in ds.RecipeDefects)
                {
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = ds.Name;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();
                    List<ObbData> infos = new List<ObbData>();
                    sResultInfos.ForEach(info => {
                        int index = int.Parse(info.lable);
                        if (Detect_names[index] == de.Name)
                        {

                            SRegion sRegion = GetDetectRegion(info);

                            cellDetection1.regionOut.Add(sRegion);
                            infos.Add(info);

                            tempResult = index+1;
                        }
                    });
                    cell.AlgorithmOut.Add(cellDetection1);
                    infos.ForEach(info => sResultInfos.Remove(info));
                }
            }

            return tempResult;
            //for (int i = 0; i < sResultInfos.Count; i++)
            //{
            //    CellDetection cellDetection2 = new CellDetection();
            //    List<float> detectRegion = new List<float>();
            //    detectRegion.Add((float)(sResultInfos[i].ResultScore*100));
            //    cellDetection2.Type = "表面缺陷类";
            //    cellDetection2.RecipeDefectName = "分数";
            //    cellDetection2.Category = Category.值;
            //    cellDetection2.Value = detectRegion;
            //    cell.AlgorithmOut.Add(cellDetection2);
            //}
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {

            string model_type_str = "YOLOv8Obb";

            ModelType model_type = MyEnum.GetModelType<ModelType>(model_type_str);
            EngineType engine_type = MyEnum.GetEngineType<EngineType>(engine_type_str);

            if ((model_type == ModelType.YOLOv8Det) || (model_type == ModelType.YOLOWorld))
            {
                infer_type = "det";
            }
            else if (
                (model_type == ModelType.YOLOv9Seg)
                || (model_type == ModelType.YOLOv8Seg)
                || (model_type == ModelType.YOLOv5Seg)
            )
            {
                infer_type = "seg";
            }
            else if ((model_type == ModelType.YOLOv8Pose))
            {
                infer_type = "pose";
            }
            else if ((model_type == ModelType.YOLOv8Obb))
            {
                infer_type = "obb";
            }
            else if ((model_type == ModelType.YOLOv8Cls))
            {
                infer_type = "cls";
            }

            string extension = Path.GetExtension(Model_Path);
            if (EngineType.TensorRT == engine_type)
            {
                //if ((extension != ".engine") && (extension == ".onnx"))
                //{
                //    OnnxToEngine from = new OnnxToEngine(Model_Path);
                //    from.Show();
                //    string directory = Path.GetDirectoryName(Model_Path);
                //    string file = Path.GetFileNameWithoutExtension(Model_Path);
                //    Model_Path = Path.Combine(directory, file) + ".engine";

                //    return;
                //}
                //else if (extension == ".engine") { }
                //else
                //{
                //   // show_worn_msg_box("Please select the correct model format.");
                //    return;
                //}
            }
            else
            {
                if (
                    (
                        extension == ".onnx"
                        && (
                            EngineType.ONNX == engine_type
                            || EngineType.OpenVINO == engine_type
                            || EngineType.OpenCV == engine_type
                        )
                    ) || (extension == ".xml" && EngineType.OpenVINO == engine_type)
                ) { }
                else
                {
                    // show_worn_msg_box("Please select the correct model format.");
                    return;
                }
            }

            yolo.Dispose();

            CPcParam param = AlgorParams[0] as CPcParam;
            
            if (param != null)
            {
                string CurrentDevice = param.CurrentDevice;
                int Categ_num = param.Categ_num;
                float Score = param.Score;
                float Nms = param.Nms;
                int Input_size = param.Input_size;
                yolo = YOLO.GetYolo(
                    model_type,
                    Model_Path,
                    engine_type,
                    CurrentDevice,
                    Categ_num,
                    Score,
                    Nms,
                    Input_size
                );
            }
        }

        List<ObbData> ImageInfer(Cell cell)
        {
            List<ObbData> sResultInfos = new List<ObbData>();

            Mat img = new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                cell.Image.ImageData
            );
            // img.SaveImage("C:\\Users\\Administrator.B\\Desktop\\新建文件夹\\1.jpg");
            //  Mat img = Cv2.ImRead("D:\\本地代码仓库\\yolo8 demo\\datasets\\Hamsausage0929\\images\\train\\0.jpg");
            //await Task.Run(() =>
            //{

            BaseResult result;
            result = yolo.predict(img);
            ObbResult obbResult = result as ObbResult;
            if (obbResult != null)
            {
                for (int i = 0; i < obbResult.count; i++)
                {
                    //SResultInfo reinfo = new SResultInfo();
                    //Point2f[] array = obbResult.datas[i].box.Points();
                    //reinfo.ResultPoints = array.ToList();
                    //reinfo.LabelStr = obbResult.datas[i].lable;
                    //reinfo.ResultScore = obbResult.datas[i].score;

                    sResultInfos.Add(obbResult.datas[i]);
                    //for (int j = 0; j < 4; j++)
                    //{
                    //    Cv2.Line(image, (Point)array[j], (Point)array[(j + 1) % 4], new Scalar(255.0, 100.0, 200.0), 2);
                    //}

                    // Cv2.PutText(image, obbResult.datas[i].lable + "-" + obbResult.datas[i].score.ToString("0.00"), (Point)array[0], HersheyFonts.HersheySimplex, 0.8, new Scalar(0.0, 0.0, 0.0), 2);
                }
            }
            img.Dispose();
            //});
            return sResultInfos;
        }


        /// <summary>
        /// 2025.01.09 易群生
        /// 初始化halcon Ocr句柄
        /// </summary>
        /// <returns></returns>
        private bool InitialHalconOcrLib()
        {
            try
            {
                string tempOCRLibPath = AppDomain.CurrentDomain.BaseDirectory + "AlgorithmPlug\\BottleInspectionAlgorithm";

                //halcon OCR库
                HOperatorSet.ReadOcrClassCnn(tempOCRLibPath + "\\Universal_Rej.occ", out m_OCRHandle);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 初始化百度Ocr句柄
        /// </summary>
        /// <returns></returns>
        private bool InitialPaddleOcrLib()
        {
            return  true;
            //try
            //{
            //    string tempOCRLibPath = AppDomain.CurrentDomain.BaseDirectory + "AlgorithmPlug\\BottleInspectionAlgorithm";

            //    百度OCR库
            //    OCRModelConfig config = new OCRModelConfig();
            //    string modelPathroot = tempOCRLibPath + @"\inference";

            //    config.det_infer = modelPathroot + @"\en_PP-OCRv3_det_slim_infer";
            //    config.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
            //    config.rec_infer = modelPathroot + @"\ch_PP-OCRv4_rec_server_infer";
            //    config.keys = modelPathroot + @"\ppocr_keys.txt";

            //    OCR参数
            //    OCRParameter oCRParameter = new OCRParameter();
            //    oCRParameter.cpu_math_library_num_threads = 10;//预测并发线程数
            //    oCRParameter.enable_mkldnn = true;//web部署该值建议设置为0,否则出错，内存如果使用很大，建议该值也设置为0.
            //    oCRParameter.cls = false; //是否执行文字方向分类；默认false
            //    oCRParameter.det = true;//是否开启方向检测，用于检测识别180旋转
            //    oCRParameter.use_angle_cls = false;//是否开启方向检测，用于检测识别180旋转
            //    oCRParameter.det_db_score_mode = true;//是否使用多段线，即文字区域是用多段线还是用矩形，

            //    oCRParameter.rec_img_h = 24;
            //    oCRParameter.rec_img_w = 40;

            //    初始化OCR引擎
            //    m_PaddleOCREngine = new PaddleOCREngine(config, oCRParameter);

            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

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

        private SRegion GetDetectRegion(ObbData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.box.Size.Width;
            sRegioninfo.ShorLen = info.box.Size.Height;
            sRegioninfo.Phi = info.box.Angle;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.score;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
            info.box.Points().ForEach(p => rec1Points.Add(new System.Windows.Point(p.X, p.Y)));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            var rect = info.box.BoundingRect();
            detectRegion.rect = new System.Windows.Rect(new System.Windows.Point(rect.TopLeft.X, rect.TopLeft.Y), new System.Windows.Size(rect.Width, rect.Height));
            return detectRegion;
        }

        /// <summary>
        /// 2025.03.17 易群生
        /// halcon OCR识别
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="ho_IntoOcrInspectionRegion"></param>
        /// <param name="hv_IntoOcrHandle"></param>
        /// <param name="hv_IntoCharacterW"></param>
        /// <param name="hv_IntoCharacterH"></param>
        /// <param name="hv_OutRow1"></param>
        /// <param name="hv_OutColumn1"></param>
        /// <param name="hv_OutRow2"></param>
        /// <param name="hv_OutColumn2"></param>
        /// <param name="hv_TextClass"></param>
        private void OcrImageInspection(HObject ho_IntoImage, HObject ho_IntoOcrInspectionRegion,
            HTuple hv_IntoOcrHandle, HTuple hv_IntoCharacterW, HTuple hv_IntoCharacterH,
            out HTuple hv_OutRow1, out HTuple hv_OutColumn1, out HTuple hv_OutRow2, out HTuple hv_OutColumn2,
            out HTuple hv_TextClass)
        {




            // Local iconic variables 

            HObject ho_ImageReduced, ho_Region, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_Characters, ho_RegionUnion;

            // Local control variables 

            HTuple hv_Confidence = new HTuple(), hv_TextClassCount = new HTuple();
            HTuple hv_Index = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Characters);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            hv_OutRow1 = new HTuple();
            hv_OutColumn1 = new HTuple();
            hv_OutRow2 = new HTuple();
            hv_OutColumn2 = new HTuple();
            hv_TextClass = new HTuple();
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_IntoImage, ho_IntoOcrInspectionRegion, out ho_ImageReduced
                );

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Region.Dispose();
                HOperatorSet.VarThreshold(ho_ImageReduced, out ho_Region, hv_IntoCharacterW * 3,
                    hv_IntoCharacterH * 3, 0.2, 10, "dark");
            }


            //Segment characters.
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, (new HTuple("width")).TupleConcat(
                    "height"), "and", hv_IntoCharacterW.TupleConcat(hv_IntoCharacterH), (new HTuple(1500)).TupleConcat(
                    1000));
            }
            ho_Characters.Dispose();
            HOperatorSet.SortRegion(ho_SelectedRegions, out ho_Characters, "character", "true",
                "row");
            //Classify.
            hv_TextClass.Dispose(); hv_Confidence.Dispose();
            HOperatorSet.DoOcrMultiClassCnn(ho_Characters, ho_ImageReduced, hv_IntoOcrHandle,
                out hv_TextClass, out hv_Confidence);

            hv_TextClassCount.Dispose();
            HOperatorSet.TupleLength(hv_TextClass, out hv_TextClassCount);
            HTuple end_val13 = hv_TextClassCount - 1;
            HTuple step_val13 = 1;
            for (hv_Index = 0; hv_Index.Continue(end_val13, step_val13); hv_Index = hv_Index.TupleAdd(step_val13))
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
                //字母l
                if ((int)(new HTuple(((hv_TextClass.TupleSelect(hv_Index))).TupleEqual("l"))) != 0)
                {
                    if (hv_TextClass == null)
                        hv_TextClass = new HTuple();
                    hv_TextClass[hv_Index] = "1";
                    continue;
                }
            }

            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_Characters, out ho_RegionUnion);
            hv_OutRow1.Dispose(); hv_OutColumn1.Dispose(); hv_OutRow2.Dispose(); hv_OutColumn2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_RegionUnion, out hv_OutRow1, out hv_OutColumn1,
                out hv_OutRow2, out hv_OutColumn2);
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_Characters.Dispose();
            ho_RegionUnion.Dispose();

            hv_Confidence.Dispose();
            hv_TextClassCount.Dispose();
            hv_Index.Dispose();

            return;
        }


        /// <summary>
        /// 2025.03.19 易群生
        /// 标签缺失检测算法
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="hv_IntoThreshold"></param>
        /// <param name="hv_MinRow"></param>
        /// <param name="hv_MaxRow"></param>
        /// <param name="hv_Radius"></param>
        /// <param name="hv_OutFlagCap"></param>
        private void InspectionLabel(HObject ho_IntoImage, HTuple hv_IntoThreshold, HTuple hv_MinRow,
      HTuple hv_MaxRow, HTuple hv_Radius, out HTuple hv_OutFlagCap)
        {




            // Local iconic variables 

            HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_Rectangle;
            HObject ho_ImageReduced, ho_Region, ho_RegionOpening, ho_ConnectedRegions;
            HObject ho_SelectedRegions;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_tempLength = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            hv_OutFlagCap = new HTuple();
            hv_Width.Dispose(); hv_Height.Dispose();
            HOperatorSet.GetImageSize(ho_IntoImage, out hv_Width, out hv_Height);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_IntoImage, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_MinRow, 100, hv_MaxRow, hv_Width - 100);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageB, ho_Rectangle, out ho_ImageReduced);

            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, hv_IntoThreshold, 255);

            ho_RegionOpening.Dispose();
            HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, hv_Radius, 1);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions);

            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                70);
            hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_Row1, out hv_Column1,
                out hv_Row2, out hv_Column2);

            hv_tempLength.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_tempLength = hv_Column2 - hv_Column1;
            }
            if ((int)(new HTuple(hv_tempLength.TupleGreater(hv_Radius))) != 0)
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 1;
            }
            else
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 0;
            }

            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionOpening.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();

            hv_Width.Dispose();
            hv_Height.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();
            hv_tempLength.Dispose();

            return;
        }

        /// <summary>
        /// 2025.03.19 易群生
        /// 铝盖缺失检测算法
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="hv_IntoThreshold"></param>
        /// <param name="hv_MinRow"></param>
        /// <param name="hv_MaxRow"></param>
        /// <param name="hv_OutFlagCap"></param>
        private void InspectionCap(HObject ho_IntoImage, HTuple hv_IntoThreshold, HTuple hv_MinRow,
            HTuple hv_MaxRow, HTuple hv_CapRadius, HTuple hv_CapThickness, out HTuple hv_OutFlagCap)
        {




            // Local iconic variables 

            HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_Rectangle;
            HObject ho_ImageReduced, ho_Region, ho_RegionOpening, ho_ConnectedRegions;
            HObject ho_SelectedRegions;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_tempLength = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            hv_OutFlagCap = new HTuple();
            hv_Width.Dispose(); hv_Height.Dispose();
            HOperatorSet.GetImageSize(ho_IntoImage, out hv_Width, out hv_Height);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_IntoImage, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_MinRow, 100, hv_MaxRow, hv_Width - 100);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageB, ho_Rectangle, out ho_ImageReduced);

            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, hv_IntoThreshold, 255);

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, 1, hv_CapThickness * 0.5);
            }
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions);

            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                70);
            hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_Row1, out hv_Column1,
                out hv_Row2, out hv_Column2);

            hv_tempLength.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_tempLength = hv_Column2 - hv_Column1;
            }
            if ((int)(new HTuple(hv_tempLength.TupleGreater(hv_CapRadius))) != 0)
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 1;
            }
            else
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 0;
            }

            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionOpening.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();

            hv_Width.Dispose();
            hv_Height.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();
            hv_tempLength.Dispose();

            return;
        }


        /// <summary>
        /// 2025.03.19 易群生
        /// 检测蓝盖有无
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="hv_IntoThreshold"></param>
        /// <param name="hv_MinRow"></param>
        /// <param name="hv_MaxRow"></param>
        /// <param name="hv_CapRadius"></param>
        /// <param name="hv_CapThickness"></param>
        /// <param name="hv_OutFlagCap"></param>
        private void InspectionBlueCap(HObject ho_IntoImage, HTuple hv_IntoThreshold, HTuple hv_MinRow,
            HTuple hv_MaxRow, HTuple hv_CapRadius, HTuple hv_CapThickness, out HTuple hv_OutFlagCap)
        {




            // Local iconic variables 

            HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_ImageSub;
            HObject ho_Rectangle, ho_ImageReduced, ho_Region, ho_RegionOpening;
            HObject ho_ConnectedRegions, ho_SelectedRegions;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_tempLength = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageSub);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            hv_OutFlagCap = new HTuple();
            hv_Width.Dispose(); hv_Height.Dispose();
            HOperatorSet.GetImageSize(ho_IntoImage, out hv_Width, out hv_Height);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_IntoImage, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );
            ho_ImageSub.Dispose();
            HOperatorSet.SubImage(ho_ImageB, ho_ImageR, out ho_ImageSub, 1, 0);

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_MinRow, 100, hv_MaxRow, hv_Width - 100);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageSub, ho_Rectangle, out ho_ImageReduced);

            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, hv_IntoThreshold, 255);

            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, 1, hv_CapThickness * 0.5);
            }
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions);

            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                70);
            hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_Row1, out hv_Column1,
                out hv_Row2, out hv_Column2);

            hv_tempLength.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_tempLength = hv_Column2 - hv_Column1;
            }
            if ((int)(new HTuple(hv_tempLength.TupleGreater(hv_CapRadius))) != 0)
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 1;
            }
            else
            {
                hv_OutFlagCap.Dispose();
                hv_OutFlagCap = 0;
            }

            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageSub.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionOpening.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();

            hv_Width.Dispose();
            hv_Height.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();
            hv_tempLength.Dispose();

            return;
        }


        /// <summary>
        /// 添加测试结果
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="IntoDefectName"></param>
        private void AddTestResult(Cell cell, string IntoDefectName, List<System.Windows.Point> rec1Points)
        {
            int tempDefectSpeciesCount = DefectSpecies.Count;
            int tempRecipeDefectsCount = 0;

            SRegionInfo sRegioninfo = new SRegionInfo();

            for (int i = 0; i < tempDefectSpeciesCount; i++)
            {
                tempRecipeDefectsCount = DefectSpecies[i].RecipeDefects.Count;
                for (int j = 0; j < tempRecipeDefectsCount; j++)
                {
                    CellDetection tempCellDetection = new CellDetection();
                    tempCellDetection.Type = DefectSpecies[i].Name;
                    tempCellDetection.RecipeDefectName = DefectSpecies[i].RecipeDefects[j].Name;
                    tempCellDetection.Category = DefectSpecies[i].RecipeDefects[j].Category;
                    tempCellDetection.regionOut = new List<SRegion>();

                    if ((DefectSpecies[i].RecipeDefects[j].Name == IntoDefectName) && (rec1Points != null))
                    {
                        SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
                        tempCellDetection.regionOut.Add(detectRegion);
                    }

                    cell.AlgorithmOut.Add(tempCellDetection);
                }
            }
        }


        /// <summary>
        /// 2025.01.09 易群生
        /// 缺陷检测流程
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            BottleInspectionAlgorithmParam param =
            new(
                (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect),
                cell.MmPerPixel * 1000
            );

            if (param.InspectionResult == "ok")
            {
                AddTestResult(cell, "", null);
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            CameraImageInfoStruct tempCameraImageInfo;
            tempCameraImageInfo.CameraIndex = 1;
            tempCameraImageInfo.CameraImage = null;

            PhotoID++;
            cell.ID = PhotoID.ToString();

            //2025.03.16 易群生
            //等待4个相机图像

            if (cell.CamName == "工位1")
            {
                tempCameraImageInfo.CameraIndex = 1;
            }
            else
            if (cell.CamName == "工位2")
            {
                tempCameraImageInfo.CameraIndex = 2;
            }
            else
            if (cell.CamName == "工位3")
            {
                tempCameraImageInfo.CameraIndex = 3;
            }
            else
            {
                tempCameraImageInfo.CameraIndex = 4;
            }

            CameraIndex = tempCameraImageInfo.CameraIndex;

            if (AITestResult == 0)
            {
                //int tempAITestResult = AIDetectImage(cell);
                int tempAITestResult = 0;

                if (tempAITestResult != 0)
                {
                    AITestResult = tempAITestResult;

                    AITestResultAry[CameraIndex-1] = tempAITestResult;
                }
            }

            HOperatorSet.GenEmptyObj(out tempCameraImageInfo.CameraImage);
            tempCameraImageInfo.CameraImage.Dispose();

            if (cell.Image.PixelFormat == System.Windows.Media.PixelFormats.Bgr32)
            {
                //2025.03.16 易群生
                //离线加载电脑图片
                HOperatorSet.GenImageInterleaved(out tempCameraImageInfo.CameraImage, cell.Image.ImageData, "bgrx", cell.Image.ImageWidth, cell.Image.ImageHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            }
            else
            {
                //2025.03.16 易群生
                //相机采集图片
                HOperatorSet.GenImageInterleaved(out tempCameraImageInfo.CameraImage, cell.Image.ImageData, "rgb", cell.Image.ImageWidth, cell.Image.ImageHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            }

            //HOperatorSet.WriteImage(tempCameraImageInfo.CameraImage,"bmp",0,"D:\\"+ tempCameraImageInfo.CameraIndex.ToString() + ".bmp");

            if ((param.CapMinRow > 0) && (param.CapMaxRow > 0))
            {
                HTuple bFlagBlueCap = null;
                try
                {
                    InspectionBlueCap(tempCameraImageInfo.CameraImage, param.BlueCapBrightnessMin, param.CapMinRow, param.CapMaxRow, param.BlueCapDiameter * 0.5, param.BlueCapThickness, out bFlagBlueCap);


                    if (bFlagBlueCap == 1)
                    {
                        BlueCapTestResult = 1;
                    }
                    else
                    {
                        BlueCapTestResult = 0;
                    }

                    if (bFlagBlueCap != null)
                    {
                        bFlagBlueCap.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    if (bFlagBlueCap != null)
                    {
                        bFlagBlueCap.Dispose();
                    }

                    OperateLog.Info("BottleTest_InspectionBlueCap:" + ex.Message.ToString());
                }

                //HTuple bFlagCap = null;
                //try
                //{
                //    InspectionCap(tempCameraImageInfo.CameraImage, param.CapBrightness, param.CapMinRow, param.CapMaxRow, param.CapDiameter*0.5, param.CapThickness, out bFlagCap);

                //    if (bFlagCap == 1)
                //    {
                //        CapTestResult = 1;
                //    }
                //    else
                //    {
                //        CapTestResult = 0;
                //    }

                //    if (bFlagCap != null)
                //    {
                //        bFlagCap.Dispose();
                //    }
                //}
                //catch (Exception ex)
                //{
                //    if (bFlagCap != null)
                //    {
                //        bFlagCap.Dispose();
                //    }

                //    OperateLog.Info("BottleTest_InspectionCap:" + ex.Message.ToString());
                //}
            }

            /// 2025.03.19 易群生
            /// 标签缺失检测
            HTuple bFlagLabel = null;
            try
            {
                InspectionLabel(tempCameraImageInfo.CameraImage, param.LabelBrightnessMin, param.LabelMinRow, param.LabelMaxRow - 100,param.BottleDiameter*0.5, out bFlagLabel);
                lock (m_WaitImage_globalLock)
                {
                    if (bFlagLabel.I == 1)
                    {
                        LabelTestResult = 1;
                    }
                }

                if (bFlagLabel != null)
                {
                    bFlagLabel.Dispose();
                }

            }
            catch (Exception ex)
            {
                if (bFlagLabel != null)
                {
                    bFlagLabel.Dispose();
                }

                OperateLog.Info("BottleTest_InspectionLabel:" + ex.Message.ToString());
            }

            //OCRResult ocrResult = new OCRResult();
            System.Drawing.Bitmap tempDateBitmap = null;
            System.Drawing.Bitmap tempDateCroppedBitmap = null;

            HObject tempDateImage = null;

            string tempResult = "";
            List<System.Windows.Point> tempPointList = new List<System.Windows.Point>();
            try
            {
                cell.OcrResultString = "";
                Boolean tempHaveTextFlag = false;

                switch (m_LibIndex)
                {
                    case 0:
                        //2025.03.18 易群生
                        //halcon Ocr

                        HTuple tempW1 = null, tempH1 = null;

                        HTuple hv_OutRow1 = null;
                        HTuple hv_OutColumn1 = null;
                        HTuple hv_OutRow2 = null;
                        HTuple hv_OutColumn2 = null;
                        HTuple hv_Characters = null;

                        try
                        {
                            HOperatorSet.GetImageSize(tempCameraImageInfo.CameraImage, out tempW1, out tempH1);

                            if (m_bFlagInitialAlgorithmParam)
                            {
                                m_DateCropX = 0; // 切图的起始X坐标
                                m_DateCropY = param.DateMinRow; // 切图的起始Y坐标
                                m_DateCropWidth = tempW1; // 切图的宽度
                                m_DateCropHeight = param.DateMaxRow - param.DateMinRow + 1; // 切图的高度

                                HOperatorSet.GenEmptyObj(out m_DateCropRegion);
                                m_DateCropRegion.Dispose();

                                switch (param.DateAngleIndex)
                                {
                                    case 0:
                                        HOperatorSet.GenRectangle1(out m_DateCropRegion, m_DateCropY, m_DateCropX, m_DateCropY + m_DateCropHeight - 1, m_DateCropX + m_DateCropWidth - 1);
                                        break;

                                    case 90:
                                        HOperatorSet.GenRectangle1(out m_DateCropRegion, tempW1 - (m_DateCropX + m_DateCropWidth), m_DateCropY, tempW1 - m_DateCropX - 1, m_DateCropY + m_DateCropHeight - 1);
                                        break;

                                    case 180:
                                        HOperatorSet.GenRectangle1(out m_DateCropRegion, tempH1 - (m_DateCropY + m_DateCropHeight), tempW1 - (m_DateCropX + m_DateCropWidth), tempH1 - m_DateCropY - 1, tempW1 - m_DateCropX - 1);
                                        break;

                                    case 270:
                                        HOperatorSet.GenRectangle1(out m_DateCropRegion, m_DateCropX, tempH1 - (m_DateCropY + m_DateCropHeight), m_DateCropX + m_DateCropWidth - 1, tempH1 - m_DateCropY - 1);
                                        break;
                                    default:
                                        break;
                                }

                                m_bFlagInitialAlgorithmParam = false;
                            }

                            HOperatorSet.GenEmptyObj(out tempDateImage);
                            tempDateImage.Dispose();


                            switch (param.DateAngleIndex)
                            {
                                case 0:
                                    OcrImageInspection(tempCameraImageInfo.CameraImage, m_DateCropRegion, m_OCRHandle, 15, 10, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
                                    break;

                                case 90:
                                    HOperatorSet.RotateImage(tempCameraImageInfo.CameraImage, out tempDateImage, 270, "constant");
                                    OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
                                    break;

                                case 180:
                                    HOperatorSet.RotateImage(tempCameraImageInfo.CameraImage, out tempDateImage, 180, "constant");
                                    OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);
                                    break;

                                case 270:
                                    HOperatorSet.RotateImage(tempCameraImageInfo.CameraImage, out tempDateImage, 90, "constant");
                                    OcrImageInspection(tempDateImage, m_DateCropRegion, m_OCRHandle, 15, 10, out hv_OutRow1, out hv_OutColumn1, out hv_OutRow2, out hv_OutColumn2, out hv_Characters);


                                    break;
                                default:
                                    break;
                            }

                            tempResult = hv_Characters.ToString();

                            if (hv_OutRow1.Length > 0)
                            {
                                if (tempResult.Length > 2)
                                {
                                    tempResult = tempResult.Substring(1, tempResult.Length - 2);
                                    tempResult = Regex.Replace(tempResult, @"[^\d]", "");
                                    cell.OcrResultString = tempResult;

                                    if (tempResult.Length > 0)
                                    {
                                        tempHaveTextFlag = true;
                                    }

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

                            if (tempHaveTextFlag)
                            {
                                System.Windows.Point tempPoint = new System.Windows.Point();

                                switch (param.DateAngleIndex)
                                {
                                    case 0:
                                        tempPoint.X = hv_OutColumn1.D + m_DateCropX;
                                        tempPoint.Y = hv_OutRow1.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = hv_OutColumn2.D + m_DateCropX;
                                        tempPoint.Y = hv_OutRow2.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        break;
                                    case 90:
                                        tempPoint.X = tempW1 - hv_OutRow2.D - 1 + m_DateCropX;
                                        tempPoint.Y = hv_OutColumn1.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = tempW1 - hv_OutRow1.D - 1 + m_DateCropX;
                                        tempPoint.Y = hv_OutColumn2.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);
                                        break;

                                    case 180:
                                        tempPoint.X = tempW1 - hv_OutColumn2.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutRow2.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = tempW1 - hv_OutColumn1.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutRow1.D + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        break;

                                    case 270:
                                        tempPoint.X = hv_OutRow1.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutColumn2.D - 1 + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = hv_OutRow2.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutColumn1.D - 1 + m_DateCropY;
                                        tempPointList.Add(tempPoint);

                                        break;
                                    default:
                                        break;
                                }

                                cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
                            }
                            else
                            {
                                System.Windows.Point tempPoint = new System.Windows.Point();
                                tempPoint.X = 0;
                                tempPoint.Y = 0;
                                tempPointList.Add(tempPoint);

                                tempPoint.X = 0;
                                tempPoint.Y = 0;
                                tempPointList.Add(tempPoint);

                                cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
                            }

                            if (tempW1 != null)
                            {
                                tempW1.Dispose();
                            }

                            if (tempH1 != null)
                            {
                                tempH1.Dispose();
                            }

                            if (hv_OutRow1 != null)
                            {
                                hv_OutRow1.Dispose();
                            }

                            if (hv_OutColumn1 != null)
                            {
                                hv_OutColumn1.Dispose();
                            }

                            if (hv_OutRow2 != null)
                            {
                                hv_OutRow2.Dispose();
                            }

                            if (hv_OutColumn2 != null)
                            {
                                hv_OutColumn2.Dispose();
                            }

                            if (hv_Characters != null)
                            {
                                hv_Characters.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (tempW1 != null)
                            {
                                tempW1.Dispose();
                            }

                            if (tempH1 != null)
                            {
                                tempH1.Dispose();
                            }

                            if (hv_OutRow1 != null)
                            {
                                hv_OutRow1.Dispose();
                            }

                            if (hv_OutColumn1 != null)
                            {
                                hv_OutColumn1.Dispose();
                            }

                            if (hv_OutRow2 != null)
                            {
                                hv_OutRow2.Dispose();
                            }

                            if (hv_OutColumn2 != null)
                            {
                                hv_OutColumn2.Dispose();
                            }

                            if (hv_Characters != null)
                            {
                                hv_Characters.Dispose();
                            }

                            OperateLog.Info("BottleTest_ocr_1:" + ex.Message.ToString());
                        }

                        break;

                    //case 1:
                    //    //2025.03.18 易群生
                    //    //百度 Ocr

                    //    HObject tempInterleaveImage;
                    //    HOperatorSet.GenEmptyObj(out tempInterleaveImage);
                    //    tempInterleaveImage.Dispose();

                    //    HTuple tempPointer = null, tempType = null, tempW = null, tempH = null;

                    //    try
                    //    {
                    //        HOperatorSet.InterleaveChannels(tempCameraImageInfo.CameraImage, out tempInterleaveImage, "argb", "match", 255);
                    //        HOperatorSet.GetImagePointer1(tempInterleaveImage, out tempPointer, out tempType, out tempW, out tempH);
                    //        IntPtr tempPtr = tempPointer;
                    //        tempDateBitmap = new System.Drawing.Bitmap(tempW / 4, tempH, tempW,
                    //            System.Drawing.Imaging.PixelFormat.Format32bppRgb, tempPtr);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        if (tempInterleaveImage != null)
                    //        {
                    //            tempInterleaveImage.Dispose();
                    //        }

                    //        if (tempPointer != null)
                    //        {
                    //            tempPointer.Dispose();
                    //        }

                    //        if (tempType != null)
                    //        {
                    //            tempType.Dispose();

                    //        }

                    //        if (tempW != null)
                    //        {
                    //            tempW.Dispose();
                    //        }

                    //        if (tempH != null)
                    //        {
                    //            tempH.Dispose();
                    //        }

                    //        OperateLog.Info("BottleTest_ocr_2:" + ex.Message.ToString());
                    //    }

                    //    if (tempInterleaveImage != null)
                    //    {
                    //        tempInterleaveImage.Dispose();
                    //    }

                    //    if (tempPointer != null)
                    //    {
                    //        tempPointer.Dispose();
                    //    }

                    //    if (tempType != null)
                    //    {
                    //        tempType.Dispose();

                    //    }

                    //    if (tempW != null)
                    //    {
                    //        tempW.Dispose();
                    //    }

                    //    if (tempH != null)
                    //    {
                    //        tempH.Dispose();
                    //    }

                        //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(cell.Image.ImageWidth, cell.Image.ImageHeight, cell.Image.ImageWidth*4, 
                        //System.Drawing.Imaging.PixelFormat.Format32bppRgb, cell.Image.ImageData);
                        //tempDateBitmap.Save("D:\\output_" + tempCameraImageInfo.CameraIndex.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //    if (m_bFlagInitialAlgorithmParam)
                    //    {
                    //        m_DateCropX = 0; // 切图的起始X坐标
                    //        m_DateCropY = param.DateMinRow - param.LabelMinRow; // 切图的起始Y坐标
                    //        m_DateCropWidth = tempW / 4; // 切图的宽度
                    //        m_DateCropHeight = param.DateMaxRow - param.DateMinRow + 1; // 切图的高度

                    //        m_DateCropRect = new System.Drawing.Rectangle(m_DateCropX, m_DateCropY, m_DateCropWidth, m_DateCropHeight);
                    //        m_bFlagInitialAlgorithmParam = false;
                    //    }

                    //    tempDateCroppedBitmap = tempDateBitmap.Clone(m_DateCropRect, tempDateBitmap.PixelFormat);

                    //    switch (param.DateAngleIndex)
                    //    {
                    //        case 90:
                    //            tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    //            break;

                    //        case 180:
                    //            tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    //            break;

                    //        case 270:
                    //            tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    //            break;
                    //        default:
                    //            break;
                    //    }

                        //tempDateCroppedBitmap.Save("D:\\croppedImage_" + tempCameraImageInfo.CameraIndex.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //    //ocrResult = m_PaddleOCREngine.DetectText(tempDateCroppedBitmap);

                    //    //if (ocrResult.TextBlocks.Count > 0)
                    //    {
                    //        //挑选出数字日期
                    //        //tempResult = Regex.Replace(ocrResult.Text, @"[^\d]", "");
                    //        cell.OcrResultString = tempResult;

                    //        if (tempResult != "")
                    //        {
                    //            tempHaveTextFlag = true;

                    //            System.Windows.Point tempLTPoint = new System.Windows.Point();
                    //            System.Windows.Point tempRBPoint = new System.Windows.Point();


                    //            double tempMinX0, tempMinY0, tempMaxX0, tempMaxY0;
                    //            tempMinX0 = Math.Min(ocrResult.TextBlocks[0].BoxPoints[0].X, ocrResult.TextBlocks[0].BoxPoints[3].X);
                    //            tempMinY0 = Math.Min(ocrResult.TextBlocks[0].BoxPoints[0].Y, ocrResult.TextBlocks[0].BoxPoints[1].Y);

                    //            tempMaxX0 = Math.Max(ocrResult.TextBlocks[0].BoxPoints[1].X, ocrResult.TextBlocks[0].BoxPoints[2].X);
                    //            tempMaxY0 = Math.Max(ocrResult.TextBlocks[0].BoxPoints[2].Y, ocrResult.TextBlocks[0].BoxPoints[3].Y);

                    //            double tempMinX, tempMinY, tempMaxX, tempMaxY;
                    //            for (int j = 1; j < ocrResult.TextBlocks.Count; j++)
                    //            {
                    //                tempMinX = Math.Min(ocrResult.TextBlocks[j].BoxPoints[0].X, ocrResult.TextBlocks[j].BoxPoints[3].X);
                    //                tempMinY = Math.Min(ocrResult.TextBlocks[j].BoxPoints[0].Y, ocrResult.TextBlocks[j].BoxPoints[1].Y);

                    //                if (tempMinX0 > tempMinX)
                    //                {
                    //                    tempMinX0 = tempMinX;
                    //                }

                    //                if (tempMinY0 > tempMinY)
                    //                {
                    //                    tempMinY0 = tempMinY;
                    //                }

                    //                tempMaxX = Math.Max(ocrResult.TextBlocks[j].BoxPoints[1].X, ocrResult.TextBlocks[j].BoxPoints[2].X);
                    //                tempMaxY = Math.Max(ocrResult.TextBlocks[j].BoxPoints[2].Y, ocrResult.TextBlocks[j].BoxPoints[3].Y);

                    //                if (tempMaxX0 < tempMaxX)
                    //                {
                    //                    tempMaxX0 = tempMaxX;
                    //                }

                    //                if (tempMaxY0 < tempMaxY)
                    //                {
                    //                    tempMaxY0 = tempMaxY;
                    //                }
                    //            }

                    //            switch (param.DateAngleIndex)
                    //            {
                    //                case 0:
                    //                    tempLTPoint.X = tempMinX0 + m_DateCropX;
                    //                    tempLTPoint.Y = tempMinY0 + m_DateCropY;
                    //                    tempRBPoint.X = tempMaxX0 + m_DateCropX;
                    //                    tempRBPoint.Y = tempMaxY0 + m_DateCropY;
                    //                    break;
                    //                case 90:
                    //                    tempLTPoint.X = tempMinY0 + m_DateCropX;
                    //                    tempLTPoint.Y = m_DateCropWidth - tempMaxX0 + m_DateCropY;
                    //                    tempRBPoint.X = tempMaxY0 + m_DateCropX;
                    //                    tempRBPoint.Y = m_DateCropWidth - tempMinX0 + m_DateCropY;
                    //                    break;

                    //                case 180:
                    //                    tempLTPoint.X = m_DateCropWidth - tempMaxX0 + m_DateCropX;
                    //                    tempLTPoint.Y = m_DateCropHeight - tempMaxY0 + m_DateCropY;
                    //                    tempRBPoint.X = m_DateCropWidth - tempMinX0 + m_DateCropX;
                    //                    tempRBPoint.Y = m_DateCropHeight - tempMinY0 + m_DateCropY;

                    //                    break;

                    //                case 270:
                    //                    tempLTPoint.X = m_DateCropHeight - tempMaxY0 + m_DateCropX;
                    //                    tempLTPoint.Y = tempMinX0 + m_DateCropY;
                    //                    tempRBPoint.X = m_DateCropHeight - tempMinY0 + m_DateCropX;
                    //                    tempRBPoint.Y = tempMaxX0 + m_DateCropY;

                    //                    break;
                    //                default:
                    //                    break;
                    //            }

                    //            tempPointList.Add(tempLTPoint);
                    //            tempPointList.Add(tempRBPoint);
                    //            cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
                    //        }
                    //    }

                    //    break;


                    default:
                        break;
                }

                //2025.03.18 易群生
                //没有识别到三期
                if (!tempHaveTextFlag)
                {
                    cell.OcrResultString = "";

                    System.Windows.Point tempPoint = new System.Windows.Point();
                    tempPoint.X = 0;
                    tempPoint.Y = 0;
                    tempPointList.Add(tempPoint);

                    tempPoint.X = 0;
                    tempPoint.Y = 0;
                    tempPointList.Add(tempPoint);

                    cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
                }

                //2025.03.18 易群生
                //4个相机的三期结果合并
                lock (m_concatenate_globalLock)
                {
                    tempResult = cell.OcrResultString;
                    cell.OcrResultString = "";
                    if (tempHaveTextFlag)
                    {
                        int tempCount = tempResult.Length;
                        for (int i = 0; i < tempCount; i++)
                        {
                            if (param.StandardDate.Contains(tempResult[i]))
                            {
                                DateString = DateString + tempResult[i];

                                cell.OcrResultString = cell.OcrResultString + tempResult[i];
                            }
                        }
                    }

                    m_DateStringCount++;

                    if (m_DateStringCount == 4)
                    {
                        string tempStandardDate = param.StandardDate.Trim();
                        int tempCount = tempStandardDate.Length;
                        if (tempCount >= 1)
                        {
                            for (int i = 0; i < tempCount; i++)
                            {
                                if (!DateString.Contains(tempStandardDate[i]))
                                {
                                    m_DateStringTestResult = 2;
                                    break;
                                }
                            }

                            if (m_DateStringTestResult != 2)
                            {
                                m_DateStringTestResult = 1;
                            }
                        }
                        else
                        {
                            m_DateStringTestResult = 1;
                        }
                    }
                }

                if (tempDateBitmap != null)
                {
                    tempDateBitmap.Dispose();
                }

                if (tempDateCroppedBitmap != null)
                {
                    tempDateCroppedBitmap.Dispose();
                }

                if (tempDateImage != null)
                {
                    tempDateImage.Dispose();
                }


                if (tempCameraImageInfo.CameraImage != null)
                {
                    tempCameraImageInfo.CameraImage.Dispose();
                }

            }
            catch (Exception ex)
            {
                if (tempDateBitmap != null)
                {
                    tempDateBitmap.Dispose();
                }

                if (tempDateCroppedBitmap != null)
                {
                    tempDateCroppedBitmap.Dispose();
                }

                if (tempDateImage != null)
                {
                    tempDateImage.Dispose();
                }


                if (tempCameraImageInfo.CameraImage != null)
                {
                    tempCameraImageInfo.CameraImage.Dispose();
                }

                OperateLog.Info("BottleTest_ocr:" + ex.Message.ToString());
            }


            /// 2025.03.19 易群生
            /// 等待4个相机的检测结果
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (i == CameraIndex)
            //    {
            //        continue;
            //    }
            //    WaitTestResultCountdownEvent[i - 1].Signal();
            //}

            //WaitTestResultCountdownEvent[CameraIndex - 1].Wait();
            //WaitTestResultCountdownEvent[CameraIndex - 1].Reset();

            if (AITestResult == 0)
            {
                if ((BlueCapTestResult == 0) || (CapTestResult == 0))
                {
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                    rec1Points.Add(new System.Windows.Point(100, param.CapMinRow));
                    rec1Points.Add(new System.Windows.Point(100, param.CapMaxRow));
                    rec1Points.Add(new System.Windows.Point(cell.Image.ImageWidth - 100, param.CapMaxRow));
                    rec1Points.Add(new System.Windows.Point(cell.Image.ImageWidth - 100, param.CapMinRow));

                    AddTestResult(cell, DefectSpecies[0].RecipeDefects[2].Name, rec1Points);
                }
                else
                if (LabelTestResult == 0)
                {
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                    rec1Points.Add(new System.Windows.Point(100, param.LabelMinRow));
                    rec1Points.Add(new System.Windows.Point(100, param.LabelMaxRow));
                    rec1Points.Add(new System.Windows.Point(cell.Image.ImageWidth - 100, param.LabelMaxRow));
                    rec1Points.Add(new System.Windows.Point(cell.Image.ImageWidth - 100, param.LabelMinRow));

                    AddTestResult(cell, DefectSpecies[0].RecipeDefects[3].Name, rec1Points);
                }
                else
                //if (cell.OcrResultString == param.StandardDate)
                //2025.03.18 易群生
                //三期结果输出
                if (m_DateStringTestResult == 1)
                {
                    AddTestResult(cell, "", null);
                }
                else
                {
                    SRegionInfo sRegioninfo = new SRegionInfo();
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));

                    if (DateString.Length < 1)
                    {
                        AddTestResult(cell, DefectSpecies[0].RecipeDefects[0].Name, rec1Points);
                    }
                    else
                    {
                        AddTestResult(cell, DefectSpecies[0].RecipeDefects[1].Name, rec1Points);
                    }
                }

            }
            else 
            {
                if (AITestResultAry[CameraIndex-1]==0)
                {
                    SRegionInfo sRegioninfo = new SRegionInfo();
                    List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));
                    rec1Points.Add(new System.Windows.Point(0, 0));

                    AddTestResult(cell, Detect_names[AITestResult - 1], rec1Points);
                }
            }

            //for (int i = 1; i <= 4; i++)
            //{
            //    if (i == CameraIndex)
            //    {
            //        continue;
            //    }
            //    WaitCombineResultCountdownEvent[i - 1].Signal();
            //}

            //WaitCombineResultCountdownEvent[CameraIndex - 1].Wait();
            //WaitCombineResultCountdownEvent[CameraIndex - 1].Reset();

            AITestResult = 0;
            BlueCapTestResult = 2;
            CapTestResult = 2;
            LabelTestResult = 0;
            m_DateStringTestResult = 0;

            for (int i = 0; i < 4; i++)
            {
                AITestResultAry[i] = 0;
            }

            //for (int i = 1; i <= 4; i++)
            //{
            //    if (i == CameraIndex)
            //    {
            //        continue;
            //    }
            //    WaitTestInitialCountdownEvent[i - 1].Signal();
            //}

            //WaitTestInitialCountdownEvent[CameraIndex - 1].Wait();
            //WaitTestInitialCountdownEvent[CameraIndex - 1].Reset();

            stopwatch.Stop();
            OperateLog.Info("BottleTestTime_1:" + CameraIndex.ToString() + "_" + stopwatch.ElapsedMilliseconds.ToString());

        }


    

    
        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 计算矩形长短边
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private double CalculateDistance(Point2f point1, Point2f point2)
        {
            double deltaX = point1.X - point2.X;
            double deltaY = point1.Y - point2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
    /// 2025.04.05 易群生
    /// 强制设定测试结果："ok"-强制设定测试结果全部为ok，"ng"-强制设定测试结果全部为ng,""-实际测试结果
    /// </summary>
    [ObservableProperty]
    [property: Category("测试结果设定")]
    [property: DisplayName("测试结果设定")]
    [property: Description("测试结果设定")]
    private String inspectionResult = "";

    /// <summary>
    /// 2025.03.15 易群生
    /// 柱形物体的半径，单位是mm
    /// </summary>
    [ObservableProperty]
    [property: Category("Algorithm")]
    [property: DisplayName("西林瓶的半径mm")]
    [property: Description("西林瓶的半径mm")]
    private double cylinderRadiusMM = 11;

    /// <summary>
    /// 2025.03.15 易群生
    /// 图像的像素实际尺寸，单位是mm
    /// </summary>
    [ObservableProperty]
    [property: Category("Algorithm")]
    [property: DisplayName("图像的像素尺寸mm")]
    [property: Description("图像的像素尺寸mm")]
    private double pixelSizeMM = 0.058;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最上面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("标签检测")]
    [property: DisplayName("标签上面位置")]
    [property: Description("标签上面位置")]
    private int labelMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("标签检测")]
    [property: DisplayName("标签下面位置")]
    [property: Description("标签下面位置")]
    private int labelMaxRow = 1000;

    /// <summary>
    /// 2025.03.20 易群生
    /// 瓶子直径
    /// </summary>
    [ObservableProperty]
    [property: Category("标签检测")]
    [property: DisplayName("瓶子直径")]
    [property: Description("瓶子直径")]
    private int bottleDiameter = 400;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置亮度上限
    /// </summary>
    [ObservableProperty]
    [property: Category("标签检测")]
    [property: DisplayName("标签亮度上限")]
    [property: Description("标签亮度上限")]
    private int labelBrightnessMax = 80;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置亮度下限
    /// </summary>
    [ObservableProperty]
    [property: Category("标签检测")]
    [property: DisplayName("标签亮度下限")]
    [property: Description("标签亮度下限")]
    private int labelBrightnessMin = 60;

    /// <summary>
    /// 2025.01.09 易群生
    /// 日期值
    /// </summary>
    [ObservableProperty]
    [property: Category("三期检测")]
    [property: DisplayName("日期值")]
    [property: Description("日期值")]
    private String standardDate = "20240802";

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最上面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("三期检测")]
    [property: DisplayName("三期上面位置")]
    [property: Description("三期上面位置")]
    private int dateMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("三期检测")]
    [property: DisplayName("三期下面位置")]
    [property: Description("三期下面位置")]
    private int dateMaxRow = 1000;

    /// <summary>
    /// 2025.03.20 易群生
    /// 日期亮度差
    /// </summary>
    [ObservableProperty]
    [property: Category("三期检测")]
    [property: DisplayName("日期亮度差")]
    [property: Description("日期亮度差")]
    private int dateDeltaBrightness = 20;


    /// <summary>
    /// 2025.03.10 易群生
    /// 日期方向
    /// </summary>
    [ObservableProperty]
    [property: Category("三期检测")]
    [property: DisplayName("日期方向")]
    [property: Description("日期方向")]
    private int dateAngleIndex = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 瓶盖位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("瓶盖上面位置")]
    [property: Description("瓶盖上面位置")]
    private int capMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 瓶盖位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("瓶盖下面位置")]
    [property: Description("瓶盖下面位置")]
    private int capMaxRow = 1000;


    /// <summary>
    /// 2025.03.20 易群生
    /// 蓝盖亮度
    /// </summary>
    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("蓝盖亮度上限")]
    [property: Description("蓝盖亮度上限")]
    private int blueCapBrightnessMax = 100;

    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("蓝盖亮度下限")]
    [property: Description("蓝盖亮度下限")]
    private int blueCapBrightnessMin = 100;

    /// <summary>
    /// 2025.03.20 易群生
    /// 蓝盖厚度
    /// </summary>
    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("蓝盖厚度")]
    [property: Description("蓝盖厚度")]
    private int blueCapThickness = 40;

    /// <summary>
    /// 2025.03.20 易群生
    /// 蓝盖直径
    /// </summary>
    [ObservableProperty]
    [property: Category("瓶盖检测参数")]
    [property: DisplayName("蓝盖直径")]
    [property: Description("蓝盖直径")]
    private int blueCapDiameter = 300;


    /// <summary>
    /// 2025.03.20 易群生
    /// 铝盖亮度
    /// </summary>
    //[ObservableProperty]
    //[property: Category("瓶盖检测参数")]
    //[property: DisplayName("铝盖亮度")]
    //[property: Description("铝盖亮度")]
    //private int capBrightness = 100;

    /// <summary>
    /// 2025.03.20 易群生
    /// 铝盖厚度
    /// </summary>
    //[ObservableProperty]
    //[property: Category("瓶盖检测参数")]
    //[property: DisplayName("铝盖厚度")]
    //[property: Description("铝盖厚度")]
    //private int capThickness = 40;

    /// <summary>
    /// 2025.03.20 易群生
    /// 铝盖直径
    /// </summary>
    //[ObservableProperty]
    //[property: Category("瓶盖检测参数")]
    //[property: DisplayName("铝盖直径")]
    //[property: Description("铝盖直径")]
    //private int capDiameter = 300;

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// 最小分数阈值
    /// </summary>
    [ObservableProperty]
    [property: Category("AI参数")]
    [property: DisplayName("最小分数阈值")]
    [property: Description("最小分数阈值")]
    private float score = 0.6f;

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// NMScore
    /// </summary>
    [ObservableProperty]
    [property: Category("AI参数")]
    [property: DisplayName("NMScore")]
    [property: Description("NMScore")]
    private float nms = 0.5f;

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// 缺陷类型数量
    /// </summary>
    [ObservableProperty]
    [property: Category("AI参数")]
    [property: DisplayName("缺陷类型数量")]
    [property: Description("已经标注的缺陷类型数量")]
    private int categ_num = 3;

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// 缺陷类型数量
    /// </summary>
    [ObservableProperty]
    [property: Category("AI参数")]
    [property: DisplayName("缺陷类型数量")]
    [property: Description("已经标注的缺陷类型数量")]
    private int input_size = 640;

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// 驱动设备
    /// </summary>
    [ObservableProperty]
    [property: Category("AI参数")]
    [property: DisplayName("驱动设备")]
    [property: Description("驱动设备")]
    private string currentDevice = "GPU.0";
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

        List<System.Windows.Point> pts = new List<System.Windows.Point>();
        for (int i = 0; i < regions.Count; i++)
        {
            pts.AddRange(regions[i].points);
        }
        return new SRegion(regionInfo, pts);
    }
};
