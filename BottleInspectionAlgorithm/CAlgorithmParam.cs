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
using PaddleOCRSharp;
using System.Drawing.Imaging;
using OpenCvSharp;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ValueConverters;
using System;
using System.Diagnostics;
using WH.Entity.LogRecord;

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
        private static PaddleOCREngine m_PaddleOCREngine;

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
        /// 多相机标定参数
        /// </summary>
        private static HTuple m_CameraSetupModel;

        /// <summary>
        /// 2025.03.15 易群生
        /// 初始化柱面图像展开的相机标定参数
        /// </summary>
        private static bool m_bFlagInitialCameraSetupModel = true;

        /// <summary>
        /// 2025.03.15 易群生
        /// 二次初始化柱面图像展开的相机标定参数
        /// </summary>
        private bool m_bFlagInitialCameraSetupModel2 = true;

        /// 2025.03.18 易群生
        /// 二次初始化柱面图像展开的相机标定参数的锁
        /// </summary>
        private static readonly object m_ExpasionParameter_globalLock = new object();

        /// <summary>
        /// 2025.03.15 易群生
        /// 柱形物体找边调整参数
        /// </summary>
        private static double m_FineAdjustmentMatchingWidth;
        private static double m_FineAdjustmentMaxShift;
        private static double m_BlendingSeam;
        private static double m_SilhouetteMeasureDistance;
        private static double m_SilhouetteMeasureLength2;
        private static double m_SilhouetteMeasureSigma;
        private static double m_SilhouetteMeasureThreshold;
        private static double m_SilhouetteMaxTilt;

        /// <summary>
        /// 2025.03.15 易群生
        /// 图像的像素实际尺寸，单位是m
        /// </summary>
        private double m_PixelSize;

        /// <summary>
        /// 2025.03.15 易群生
        /// 柱形物体的半径，单位是m
        /// </summary>
        private double m_CylinderRadius;

        /// <summary>
        /// 2025.03.15 易群生
        /// 柱形物体找边句柄
        /// </summary>
        private HTuple m_MeasureHandles;

        /// <summary>
        /// 2025.03.15 易群生
        /// 径向畸变校正图
        /// </summary>
        private HObject m_RectificationMaps;

        /// <summary>
        /// 2025.03.15 易群生
        /// 相机数量
        /// </summary>
        private HTuple m_CameraNum;

        /// <summary>
        /// 2025.03.15 易群生
        /// 图像展开参数
        /// </summary>
        private HTuple m_CameraSetupModelZeroDist;
        private HTuple m_PoseCylinderApprox;
        private HTuple m_HomMat3DCylinderApprox;

        private HTuple m_MinPairDist;
        private HTuple m_MaxPairDist;

        private HTuple m_NumSlices;
        private HTuple m_NumPointsPerSlice;

        private HTuple m_MinZI;
        private HTuple m_MaxZI;

        private HTuple m_CylinderPointsX;
        private HTuple m_CylinderPointsY;
        private HTuple m_CylinderPointsZ;

        private HTuple m_MosaicHeight;
        private HTuple m_MosaicWidth;


        /// <summary>
        /// 2025.03.18 易群生
        /// 4个相机待展开的图像
        /// </summary>
        private static HObject m_ProcessImageAry;

        /// <summary>
        /// 2025.03.18 易群生
        /// 等待4个相机图像展开的锁
        /// </summary>
        private static readonly object m_WaitImage_globalLock = new object();

        /// <summary>
        /// 2025.03.17 易群生
        /// 4个相机同时拍照的图片，用以进行柱形图像展开
        /// </summary>
        private static List<CameraImageInfoStruct> CameraImageStructList = new List<CameraImageInfoStruct>();

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
        private volatile static int CapTestResult = 2;

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
        /// 等待其他3个相机图像的到来
        /// </summary>
        static CountdownEvent[] WaitImageCountdownEvent;

        /// <summary>
        /// 等待其他3个相机的测试结果
        /// </summary>
        static CountdownEvent[] WaitTestResultCountdownEvent;

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

            HOperatorSet.GenEmptyObj(out m_ProcessImageAry);

            if (m_bFlagInitialCameraSetupModel)
            {
                InitialCylinderExpansion();
                m_bFlagInitialCameraSetupModel = false;

                WaitImageCountdownEvent = new CountdownEvent[4];
                WaitTestResultCountdownEvent = new CountdownEvent[4];

                for (int i = 0; i < 4; i++)
                {
                    WaitImageCountdownEvent[i] = new CountdownEvent(3);
                    WaitTestResultCountdownEvent[i] = new CountdownEvent(3);
                }
            }

            PhotoID = 0;


            m_bFlagInitialAlgorithmParam = true;

            if (m_OcrLibInitialFlag)
            {
                m_OCRHandle = null;
                m_PaddleOCREngine = null;

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
        }

        /// <summary>
        /// 2025.03.14 易群生
        /// 初始化图像展开参数
        /// </summary>
        /// <returns></returns>
        private bool InitialCylinderExpansion()
        {
            string tempCameraSetupModelPath = AppDomain.CurrentDomain.BaseDirectory + "AlgorithmPlug\\BottleInspectionAlgorithm\\";

            m_CameraSetupModel = new HTuple();

            HTuple hv_CamPose0 = new HTuple();
            HTuple tempSilhouetteMaxTilt = new HTuple();
            try
            {
                m_CameraSetupModel.Dispose();
                HOperatorSet.ReadCameraSetupModel(tempCameraSetupModelPath + "Camera_Setup_model.csm", out m_CameraSetupModel);

                hv_CamPose0.Dispose();
                HOperatorSet.GetCameraSetupParam(m_CameraSetupModel, 0, "pose", out hv_CamPose0);
                HOperatorSet.SetCameraSetupParam(m_CameraSetupModel, "general", "coord_transf_pose",
                    hv_CamPose0);

                m_FineAdjustmentMatchingWidth = 50;
                m_FineAdjustmentMaxShift = 15;
                m_BlendingSeam = 10;
                m_SilhouetteMeasureDistance = 10;
                m_SilhouetteMeasureLength2 = 30;
                m_SilhouetteMeasureSigma = 3.5;
                m_SilhouetteMeasureThreshold = 30;

                tempSilhouetteMaxTilt.Dispose();
                HOperatorSet.TupleRad(10, out tempSilhouetteMaxTilt);
                m_SilhouetteMaxTilt = tempSilhouetteMaxTilt.D;

                tempSilhouetteMaxTilt.Dispose();
                hv_CamPose0.Dispose();
                return true;
            }
            catch (Exception)
            {
                tempSilhouetteMaxTilt.Dispose();
                hv_CamPose0.Dispose();
                return false;
            }
        }


        /// <summary>
        /// 2025.03.15 易群生
        /// Get the names of the parameters in a camera parameter tuple
        /// </summary>
        /// <param name="hv_CameraParam"></param>
        /// <param name="hv_CameraType"></param>
        /// <param name="hv_ParamNames"></param>
        private void get_cam_par_names(HTuple hv_CameraParam, out HTuple hv_CameraType,
            out HTuple hv_ParamNames)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_CameraParamAreaScanDivision = new HTuple();
            HTuple hv_CameraParamAreaScanPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricDivision = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanImageSideTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanObjectSideTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanHypercentricDivision = new HTuple();
            HTuple hv_CameraParamAreaScanHypercentricPolynomial = new HTuple();
            HTuple hv_CameraParamLinesScanDivision = new HTuple();
            HTuple hv_CameraParamLinesScanPolynomial = new HTuple();
            HTuple hv_CameraParamLinesScanTelecentricDivision = new HTuple();
            HTuple hv_CameraParamLinesScanTelecentricPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTiltDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTiltPolynomialLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricPolynomialLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy = new HTuple();
            // Initialize local and output iconic variables 
            hv_CameraType = new HTuple();
            hv_ParamNames = new HTuple();
            try
            {
                //get_cam_par_names returns for each element in the camera
                //parameter tuple that is passed in CameraParam the name
                //of the respective camera parameter. The parameter names
                //are returned in ParamNames. Additionally, the camera
                //type is returned in CameraType. Alternatively, instead of
                //the camera parameters, the camera type can be passed in
                //CameraParam in form of one of the following strings:
                //  - 'area_scan_division'
                //  - 'area_scan_polynomial'
                //  - 'area_scan_tilt_division'
                //  - 'area_scan_tilt_polynomial'
                //  - 'area_scan_telecentric_division'
                //  - 'area_scan_telecentric_polynomial'
                //  - 'area_scan_tilt_bilateral_telecentric_division'
                //  - 'area_scan_tilt_bilateral_telecentric_polynomial'
                //  - 'area_scan_tilt_object_side_telecentric_division'
                //  - 'area_scan_tilt_object_side_telecentric_polynomial'
                //  - 'area_scan_hypercentric_division'
                //  - 'area_scan_hypercentric_polynomial'
                //  - 'line_scan_division'
                //  - 'line_scan_polynomial'
                //  - 'line_scan_telecentric_division'
                //  - 'line_scan_telecentric_polynomial'
                //
                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanDivision = new HTuple();
                hv_CameraParamAreaScanDivision[0] = "focus";
                hv_CameraParamAreaScanDivision[1] = "kappa";
                hv_CameraParamAreaScanDivision[2] = "sx";
                hv_CameraParamAreaScanDivision[3] = "sy";
                hv_CameraParamAreaScanDivision[4] = "cx";
                hv_CameraParamAreaScanDivision[5] = "cy";
                hv_CameraParamAreaScanDivision[6] = "image_width";
                hv_CameraParamAreaScanDivision[7] = "image_height";
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanPolynomial = new HTuple();
                hv_CameraParamAreaScanPolynomial[0] = "focus";
                hv_CameraParamAreaScanPolynomial[1] = "k1";
                hv_CameraParamAreaScanPolynomial[2] = "k2";
                hv_CameraParamAreaScanPolynomial[3] = "k3";
                hv_CameraParamAreaScanPolynomial[4] = "p1";
                hv_CameraParamAreaScanPolynomial[5] = "p2";
                hv_CameraParamAreaScanPolynomial[6] = "sx";
                hv_CameraParamAreaScanPolynomial[7] = "sy";
                hv_CameraParamAreaScanPolynomial[8] = "cx";
                hv_CameraParamAreaScanPolynomial[9] = "cy";
                hv_CameraParamAreaScanPolynomial[10] = "image_width";
                hv_CameraParamAreaScanPolynomial[11] = "image_height";
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricDivision = new HTuple();
                hv_CameraParamAreaScanTelecentricDivision[0] = "magnification";
                hv_CameraParamAreaScanTelecentricDivision[1] = "kappa";
                hv_CameraParamAreaScanTelecentricDivision[2] = "sx";
                hv_CameraParamAreaScanTelecentricDivision[3] = "sy";
                hv_CameraParamAreaScanTelecentricDivision[4] = "cx";
                hv_CameraParamAreaScanTelecentricDivision[5] = "cy";
                hv_CameraParamAreaScanTelecentricDivision[6] = "image_width";
                hv_CameraParamAreaScanTelecentricDivision[7] = "image_height";
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial = new HTuple();
                hv_CameraParamAreaScanTelecentricPolynomial[0] = "magnification";
                hv_CameraParamAreaScanTelecentricPolynomial[1] = "k1";
                hv_CameraParamAreaScanTelecentricPolynomial[2] = "k2";
                hv_CameraParamAreaScanTelecentricPolynomial[3] = "k3";
                hv_CameraParamAreaScanTelecentricPolynomial[4] = "p1";
                hv_CameraParamAreaScanTelecentricPolynomial[5] = "p2";
                hv_CameraParamAreaScanTelecentricPolynomial[6] = "sx";
                hv_CameraParamAreaScanTelecentricPolynomial[7] = "sy";
                hv_CameraParamAreaScanTelecentricPolynomial[8] = "cx";
                hv_CameraParamAreaScanTelecentricPolynomial[9] = "cy";
                hv_CameraParamAreaScanTelecentricPolynomial[10] = "image_width";
                hv_CameraParamAreaScanTelecentricPolynomial[11] = "image_height";
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltDivision = new HTuple();
                hv_CameraParamAreaScanTiltDivision[0] = "focus";
                hv_CameraParamAreaScanTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanTiltDivision[2] = "image_plane_dist";
                hv_CameraParamAreaScanTiltDivision[3] = "tilt";
                hv_CameraParamAreaScanTiltDivision[4] = "rot";
                hv_CameraParamAreaScanTiltDivision[5] = "sx";
                hv_CameraParamAreaScanTiltDivision[6] = "sy";
                hv_CameraParamAreaScanTiltDivision[7] = "cx";
                hv_CameraParamAreaScanTiltDivision[8] = "cy";
                hv_CameraParamAreaScanTiltDivision[9] = "image_width";
                hv_CameraParamAreaScanTiltDivision[10] = "image_height";
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanTiltPolynomial[0] = "focus";
                hv_CameraParamAreaScanTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanTiltPolynomial[6] = "image_plane_dist";
                hv_CameraParamAreaScanTiltPolynomial[7] = "tilt";
                hv_CameraParamAreaScanTiltPolynomial[8] = "rot";
                hv_CameraParamAreaScanTiltPolynomial[9] = "sx";
                hv_CameraParamAreaScanTiltPolynomial[10] = "sy";
                hv_CameraParamAreaScanTiltPolynomial[11] = "cx";
                hv_CameraParamAreaScanTiltPolynomial[12] = "cy";
                hv_CameraParamAreaScanTiltPolynomial[13] = "image_width";
                hv_CameraParamAreaScanTiltPolynomial[14] = "image_height";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[0] = "focus";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[2] = "tilt";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[3] = "rot";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[4] = "sx";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[5] = "sy";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[6] = "cx";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[7] = "cy";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[8] = "image_width";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[9] = "image_height";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[0] = "focus";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[6] = "tilt";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[7] = "rot";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[8] = "sx";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[9] = "sy";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[10] = "cx";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[11] = "cy";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[12] = "image_width";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[13] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[0] = "magnification";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[2] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[3] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[4] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[5] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[6] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[7] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[8] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[9] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[0] = "magnification";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[6] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[7] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[8] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[9] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[10] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[11] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[12] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[13] = "image_height";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[0] = "magnification";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[2] = "image_plane_dist";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[3] = "tilt";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[4] = "rot";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[5] = "sx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[6] = "sy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[7] = "cx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[8] = "cy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[9] = "image_width";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[10] = "image_height";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[0] = "magnification";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[6] = "image_plane_dist";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[7] = "tilt";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[8] = "rot";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[9] = "sx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[10] = "sy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[11] = "cx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[12] = "cy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[13] = "image_width";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[14] = "image_height";
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricDivision = new HTuple();
                hv_CameraParamAreaScanHypercentricDivision[0] = "focus";
                hv_CameraParamAreaScanHypercentricDivision[1] = "kappa";
                hv_CameraParamAreaScanHypercentricDivision[2] = "sx";
                hv_CameraParamAreaScanHypercentricDivision[3] = "sy";
                hv_CameraParamAreaScanHypercentricDivision[4] = "cx";
                hv_CameraParamAreaScanHypercentricDivision[5] = "cy";
                hv_CameraParamAreaScanHypercentricDivision[6] = "image_width";
                hv_CameraParamAreaScanHypercentricDivision[7] = "image_height";
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial = new HTuple();
                hv_CameraParamAreaScanHypercentricPolynomial[0] = "focus";
                hv_CameraParamAreaScanHypercentricPolynomial[1] = "k1";
                hv_CameraParamAreaScanHypercentricPolynomial[2] = "k2";
                hv_CameraParamAreaScanHypercentricPolynomial[3] = "k3";
                hv_CameraParamAreaScanHypercentricPolynomial[4] = "p1";
                hv_CameraParamAreaScanHypercentricPolynomial[5] = "p2";
                hv_CameraParamAreaScanHypercentricPolynomial[6] = "sx";
                hv_CameraParamAreaScanHypercentricPolynomial[7] = "sy";
                hv_CameraParamAreaScanHypercentricPolynomial[8] = "cx";
                hv_CameraParamAreaScanHypercentricPolynomial[9] = "cy";
                hv_CameraParamAreaScanHypercentricPolynomial[10] = "image_width";
                hv_CameraParamAreaScanHypercentricPolynomial[11] = "image_height";
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanDivision = new HTuple();
                hv_CameraParamLinesScanDivision[0] = "focus";
                hv_CameraParamLinesScanDivision[1] = "kappa";
                hv_CameraParamLinesScanDivision[2] = "sx";
                hv_CameraParamLinesScanDivision[3] = "sy";
                hv_CameraParamLinesScanDivision[4] = "cx";
                hv_CameraParamLinesScanDivision[5] = "cy";
                hv_CameraParamLinesScanDivision[6] = "image_width";
                hv_CameraParamLinesScanDivision[7] = "image_height";
                hv_CameraParamLinesScanDivision[8] = "vx";
                hv_CameraParamLinesScanDivision[9] = "vy";
                hv_CameraParamLinesScanDivision[10] = "vz";
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanPolynomial = new HTuple();
                hv_CameraParamLinesScanPolynomial[0] = "focus";
                hv_CameraParamLinesScanPolynomial[1] = "k1";
                hv_CameraParamLinesScanPolynomial[2] = "k2";
                hv_CameraParamLinesScanPolynomial[3] = "k3";
                hv_CameraParamLinesScanPolynomial[4] = "p1";
                hv_CameraParamLinesScanPolynomial[5] = "p2";
                hv_CameraParamLinesScanPolynomial[6] = "sx";
                hv_CameraParamLinesScanPolynomial[7] = "sy";
                hv_CameraParamLinesScanPolynomial[8] = "cx";
                hv_CameraParamLinesScanPolynomial[9] = "cy";
                hv_CameraParamLinesScanPolynomial[10] = "image_width";
                hv_CameraParamLinesScanPolynomial[11] = "image_height";
                hv_CameraParamLinesScanPolynomial[12] = "vx";
                hv_CameraParamLinesScanPolynomial[13] = "vy";
                hv_CameraParamLinesScanPolynomial[14] = "vz";
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricDivision = new HTuple();
                hv_CameraParamLinesScanTelecentricDivision[0] = "magnification";
                hv_CameraParamLinesScanTelecentricDivision[1] = "kappa";
                hv_CameraParamLinesScanTelecentricDivision[2] = "sx";
                hv_CameraParamLinesScanTelecentricDivision[3] = "sy";
                hv_CameraParamLinesScanTelecentricDivision[4] = "cx";
                hv_CameraParamLinesScanTelecentricDivision[5] = "cy";
                hv_CameraParamLinesScanTelecentricDivision[6] = "image_width";
                hv_CameraParamLinesScanTelecentricDivision[7] = "image_height";
                hv_CameraParamLinesScanTelecentricDivision[8] = "vx";
                hv_CameraParamLinesScanTelecentricDivision[9] = "vy";
                hv_CameraParamLinesScanTelecentricDivision[10] = "vz";
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial = new HTuple();
                hv_CameraParamLinesScanTelecentricPolynomial[0] = "magnification";
                hv_CameraParamLinesScanTelecentricPolynomial[1] = "k1";
                hv_CameraParamLinesScanTelecentricPolynomial[2] = "k2";
                hv_CameraParamLinesScanTelecentricPolynomial[3] = "k3";
                hv_CameraParamLinesScanTelecentricPolynomial[4] = "p1";
                hv_CameraParamLinesScanTelecentricPolynomial[5] = "p2";
                hv_CameraParamLinesScanTelecentricPolynomial[6] = "sx";
                hv_CameraParamLinesScanTelecentricPolynomial[7] = "sy";
                hv_CameraParamLinesScanTelecentricPolynomial[8] = "cx";
                hv_CameraParamLinesScanTelecentricPolynomial[9] = "cy";
                hv_CameraParamLinesScanTelecentricPolynomial[10] = "image_width";
                hv_CameraParamLinesScanTelecentricPolynomial[11] = "image_height";
                hv_CameraParamLinesScanTelecentricPolynomial[12] = "vx";
                hv_CameraParamLinesScanTelecentricPolynomial[13] = "vy";
                hv_CameraParamLinesScanTelecentricPolynomial[14] = "vz";
                //Legacy parameter names
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanTiltDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanTiltDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanTiltDivisionLegacy[2] = "tilt";
                hv_CameraParamAreaScanTiltDivisionLegacy[3] = "rot";
                hv_CameraParamAreaScanTiltDivisionLegacy[4] = "sx";
                hv_CameraParamAreaScanTiltDivisionLegacy[5] = "sy";
                hv_CameraParamAreaScanTiltDivisionLegacy[6] = "cx";
                hv_CameraParamAreaScanTiltDivisionLegacy[7] = "cy";
                hv_CameraParamAreaScanTiltDivisionLegacy[8] = "image_width";
                hv_CameraParamAreaScanTiltDivisionLegacy[9] = "image_height";
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanTiltPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanTiltPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanTiltPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanTiltPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanTiltPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanTiltPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanTiltPolynomialLegacy[6] = "tilt";
                hv_CameraParamAreaScanTiltPolynomialLegacy[7] = "rot";
                hv_CameraParamAreaScanTiltPolynomialLegacy[8] = "sx";
                hv_CameraParamAreaScanTiltPolynomialLegacy[9] = "sy";
                hv_CameraParamAreaScanTiltPolynomialLegacy[10] = "cx";
                hv_CameraParamAreaScanTiltPolynomialLegacy[11] = "cy";
                hv_CameraParamAreaScanTiltPolynomialLegacy[12] = "image_width";
                hv_CameraParamAreaScanTiltPolynomialLegacy[13] = "image_height";
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanTelecentricDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[2] = "sx";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[3] = "sy";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[4] = "cx";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[5] = "cy";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[6] = "image_width";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[7] = "image_height";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[6] = "sx";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[7] = "sy";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[8] = "cx";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[9] = "cy";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[10] = "image_width";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[11] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[2] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[3] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[4] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[5] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[6] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[7] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[8] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[9] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[6] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[7] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[8] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[9] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[10] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[11] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[12] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[13] = "image_height";
                //
                //If the camera type is passed in CameraParam
                if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleEqual(
                    1))) != 0)
                {
                    if ((int)(((hv_CameraParam.TupleSelect(0))).TupleIsString()) != 0)
                    {
                        hv_CameraType.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CameraType = hv_CameraParam.TupleSelect(
                                0);
                        }
                        if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricPolynomial);
                            }
                        }
                        else if ((int)((new HTuple(hv_CameraType.TupleEqual("line_scan_division"))).TupleOr(
                            new HTuple(hv_CameraType.TupleEqual("line_scan")))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricPolynomial);
                            }
                        }
                        else
                        {
                            throw new HalconException(("Unknown camera type '" + hv_CameraType) + "' passed in CameraParam.");
                        }

                        hv_CameraParamAreaScanDivision.Dispose();
                        hv_CameraParamAreaScanPolynomial.Dispose();
                        hv_CameraParamAreaScanTelecentricDivision.Dispose();
                        hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                        hv_CameraParamAreaScanTiltDivision.Dispose();
                        hv_CameraParamAreaScanTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanHypercentricDivision.Dispose();
                        hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                        hv_CameraParamLinesScanDivision.Dispose();
                        hv_CameraParamLinesScanPolynomial.Dispose();
                        hv_CameraParamLinesScanTelecentricDivision.Dispose();
                        hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                        hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                        hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                        return;
                    }
                }
                //
                //If the camera parameters are passed in CameraParam
                if ((int)(((((hv_CameraParam.TupleSelect(0))).TupleIsString())).TupleNot()) != 0)
                {
                    //Format of camera parameters for HALCON 12 and earlier
                    switch ((new HTuple(hv_CameraParam.TupleLength()
                        )).I)
                    {
                        //
                        //Area Scan
                        case 8:
                            //CameraType: 'area_scan_division' or 'area_scan_telecentric_division'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTelecentricDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_telecentric_division";
                            }
                            break;
                        case 10:
                            //CameraType: 'area_scan_tilt_division' or 'area_scan_telecentric_tilt_division'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTiltDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_bilateral_telecentric_division";
                            }
                            break;
                        case 12:
                            //CameraType: 'area_scan_polynomial' or 'area_scan_telecentric_polynomial'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanPolynomial);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_polynomial";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTelecentricPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_telecentric_polynomial";
                            }
                            break;
                        case 14:
                            //CameraType: 'area_scan_tilt_polynomial' or 'area_scan_telecentric_tilt_polynomial'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTiltPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_polynomial";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_bilateral_telecentric_polynomial";
                            }
                            break;
                        //
                        //Line Scan
                        case 11:
                            //CameraType: 'line_scan' or 'line_scan_telecentric'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamLinesScanDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "line_scan_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamLinesScanTelecentricDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "line_scan_telecentric_division";
                            }
                            break;
                        default:
                            throw new HalconException("Wrong number of values in CameraParam.");
                            break;
                    }
                }
                else
                {
                    //Format of camera parameters since HALCON 13
                    hv_CameraType.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CameraType = hv_CameraParam.TupleSelect(
                            0);
                    }
                    if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            11))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            15))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            11))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            15))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricPolynomial);
                        }
                    }
                    else if ((int)((new HTuple(hv_CameraType.TupleEqual("line_scan_division"))).TupleOr(
                        new HTuple(hv_CameraType.TupleEqual("line_scan")))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricPolynomial);
                        }
                    }
                    else
                    {
                        throw new HalconException("Unknown camera type in CameraParam.");
                    }
                }

                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// Get the value of a specified camera parameter from the camera parameter tuple. 
        /// </summary>
        /// <param name="hv_CameraParam"></param>
        /// <param name="hv_ParamName"></param>
        /// <param name="hv_ParamValue"></param>
        private void get_cam_par_data(HTuple hv_CameraParam, HTuple hv_ParamName, out HTuple hv_ParamValue)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_CameraType = new HTuple(), hv_CameraParamNames = new HTuple();
            HTuple hv_Index = new HTuple(), hv_ParamNameInd = new HTuple();
            HTuple hv_I = new HTuple();
            // Initialize local and output iconic variables 
            hv_ParamValue = new HTuple();
            try
            {
                //get_cam_par_data returns in ParamValue the value of the
                //parameter that is given in ParamName from the tuple of
                //camera parameters that is given in CameraParam.
                //
                //Get the parameter names that correspond to the
                //elements in the input camera parameter tuple.
                hv_CameraType.Dispose(); hv_CameraParamNames.Dispose();
                get_cam_par_names(hv_CameraParam, out hv_CameraType, out hv_CameraParamNames);
                //
                //Find the index of the requested camera data and return
                //the corresponding value.
                hv_ParamValue.Dispose();
                hv_ParamValue = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_ParamName.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    hv_ParamNameInd.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ParamNameInd = hv_ParamName.TupleSelect(
                            hv_Index);
                    }
                    if ((int)(new HTuple(hv_ParamNameInd.TupleEqual("camera_type"))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_ParamValue = hv_ParamValue.TupleConcat(
                                    hv_CameraType);
                                hv_ParamValue.Dispose();
                                hv_ParamValue = ExpTmpLocalVar_ParamValue;
                            }
                        }
                        continue;
                    }
                    hv_I.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_I = hv_CameraParamNames.TupleFind(
                            hv_ParamNameInd);
                    }
                    if ((int)(new HTuple(hv_I.TupleNotEqual(-1))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_ParamValue = hv_ParamValue.TupleConcat(
                                    hv_CameraParam.TupleSelect(hv_I));
                                hv_ParamValue.Dispose();
                                hv_ParamValue = ExpTmpLocalVar_ParamValue;
                            }
                        }
                    }
                    else
                    {
                        throw new HalconException("Unknown camera parameter " + hv_ParamNameInd);
                    }
                }

                hv_CameraType.Dispose();
                hv_CameraParamNames.Dispose();
                hv_Index.Dispose();
                hv_ParamNameInd.Dispose();
                hv_I.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_CameraType.Dispose();
                hv_CameraParamNames.Dispose();
                hv_Index.Dispose();
                hv_ParamNameInd.Dispose();
                hv_I.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_P"></param>
        /// <param name="hv_La"></param>
        /// <param name="hv_Lb"></param>
        /// <param name="hv_Foot"></param>
        private void point_to_line_perpendicular_foot(HTuple hv_P, HTuple hv_La, HTuple hv_Lb,
            out HTuple hv_Foot)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_T0 = new HTuple();
            // Initialize local and output iconic variables 
            hv_Foot = new HTuple();
            try
            {
                hv_T0.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_T0 = ((((hv_P - hv_La) * hv_Lb)).TupleSum()
                        ) / (((hv_Lb * hv_Lb)).TupleSum());
                }
                hv_Foot.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Foot = hv_La + (hv_T0 * hv_Lb);
                }

                hv_T0.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_T0.Dispose();

                throw HDevExpDefaultException;
            }
        }


        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_CameraSetupModelZeroDist"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_PoseCylinderApprox"></param>
        /// <param name="hv_CylinderRadius"></param>
        /// <param name="hv_MinZ"></param>
        /// <param name="hv_MaxZ"></param>
        /// <param name="hv_Width"></param>
        /// <param name="hv_MinPairDist"></param>
        /// <param name="hv_MaxPairDist"></param>
        private void determine_min_max_silhouette_distance(HTuple hv_CameraSetupModelZeroDist,
            HTuple hv_NumCameras, HTuple hv_PoseCylinderApprox, HTuple hv_CylinderRadius,
            HTuple hv_MinZ, HTuple hv_MaxZ, HTuple hv_Width, out HTuple hv_MinPairDist,
            out HTuple hv_MaxPairDist)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_SerializedItemHandle = new HTuple();
            HTuple hv_CameraSetupModelPCA = new HTuple(), hv_MaxShift = new HTuple();
            HTuple hv_MinDist = new HTuple(), hv_MaxDist = new HTuple();
            HTuple hv_Cam = new HTuple(), hv_CamParam = new HTuple();
            HTuple hv_CamPose = new HTuple(), hv_HomMat3D = new HTuple();
            HTuple hv_HomMat3DInvert = new HTuple(), hv_Sx = new HTuple();
            HTuple hv_Sy = new HTuple(), hv_Focus = new HTuple(), hv_S = new HTuple();
            HTuple hv_PMinCam = new HTuple(), hv_DistPMinCam = new HTuple();
            HTuple hv_Qx = new HTuple(), hv_Qy = new HTuple(), hv_Qz = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_CylRadPx = new HTuple(), hv_Shift1 = new HTuple();
            HTuple hv_Shift2 = new HTuple(), hv_PMaxCam = new HTuple();
            HTuple hv_DistPMaxCam = new HTuple(), hv_Foot = new HTuple();
            HTuple hv_PFootCam = new HTuple(), hv_DistPFootCam = new HTuple();
            // Initialize local and output iconic variables 
            hv_MinPairDist = new HTuple();
            hv_MaxPairDist = new HTuple();
            try
            {
                hv_SerializedItemHandle.Dispose();
                HOperatorSet.SerializeCameraSetupModel(hv_CameraSetupModelZeroDist, out hv_SerializedItemHandle);
                hv_CameraSetupModelPCA.Dispose();
                HOperatorSet.DeserializeCameraSetupModel(hv_SerializedItemHandle, out hv_CameraSetupModelPCA);
                HOperatorSet.SetCameraSetupParam(hv_CameraSetupModelPCA, "general", "coord_transf_pose",
                    hv_PoseCylinderApprox);
                hv_MaxShift.Dispose();
                hv_MaxShift = 0;
                hv_MinDist.Dispose();
                hv_MinDist = new HTuple();
                hv_MaxDist.Dispose();
                hv_MaxDist = new HTuple();
                HTuple end_val6 = hv_NumCameras - 1;
                HTuple step_val6 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val6, step_val6); hv_Cam = hv_Cam.TupleAdd(step_val6))
                {
                    hv_CamParam.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelPCA, hv_Cam, "params",
                        out hv_CamParam);
                    hv_CamPose.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelPCA, hv_Cam, "pose",
                        out hv_CamPose);
                    hv_HomMat3D.Dispose();
                    HOperatorSet.PoseToHomMat3d(hv_CamPose, out hv_HomMat3D);
                    hv_HomMat3DInvert.Dispose();
                    HOperatorSet.HomMat3dInvert(hv_HomMat3D, out hv_HomMat3DInvert);
                    hv_Sx.Dispose();
                    get_cam_par_data(hv_CamParam, "sx", out hv_Sx);
                    hv_Sy.Dispose();
                    get_cam_par_data(hv_CamParam, "sy", out hv_Sy);
                    hv_Focus.Dispose();
                    get_cam_par_data(hv_CamParam, "focus", out hv_Focus);
                    hv_S.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_S = 0.5 * (hv_Sx + hv_Sy);
                    }

                    //Determine the distance of the cylinder axis from the camera and
                    //determine the maximum shift of the cylinder such that it is still
                    //completely visible in the image
                    //- at position MinZ
                    hv_PMinCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PMinCam = (hv_CamPose.TupleSelectRange(
                            0, 2)) - (((new HTuple(0)).TupleConcat(0)).TupleConcat(hv_MinZ));
                    }
                    hv_DistPMinCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistPMinCam = ((((hv_PMinCam * hv_PMinCam)).TupleSum()
                            )).TupleSqrt();
                    }
                    hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, 0, 0, hv_MinZ, out hv_Qx,
                        out hv_Qy, out hv_Qz);
                    hv_Row.Dispose(); hv_Column.Dispose();
                    HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam, out hv_Row,
                        out hv_Column);
                    hv_CylRadPx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CylRadPx = ((hv_Focus / hv_DistPMinCam) * hv_CylinderRadius) / hv_S;
                    }
                    hv_Shift1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift1 = (((hv_Column - hv_CylRadPx) * hv_S) * hv_DistPMinCam) / hv_Focus;
                    }
                    hv_Shift2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift2 = (((hv_Width - (hv_Column + hv_CylRadPx)) * hv_S) * hv_DistPMinCam) / hv_Focus;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_MaxShift = ((((hv_MaxShift.TupleConcat(
                                hv_Shift1))).TupleConcat(hv_Shift2))).TupleMax();
                            hv_MaxShift.Dispose();
                            hv_MaxShift = ExpTmpLocalVar_MaxShift;
                        }
                    }

                    //- at position MaxZ
                    hv_PMaxCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PMaxCam = (hv_CamPose.TupleSelectRange(
                            0, 2)) - (((new HTuple(0)).TupleConcat(0)).TupleConcat(hv_MaxZ));
                    }
                    hv_DistPMaxCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistPMaxCam = ((((hv_PMaxCam * hv_PMaxCam)).TupleSum()
                            )).TupleSqrt();
                    }
                    hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, 0, 0, hv_MaxZ, out hv_Qx,
                        out hv_Qy, out hv_Qz);
                    hv_Row.Dispose(); hv_Column.Dispose();
                    HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam, out hv_Row,
                        out hv_Column);
                    hv_CylRadPx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CylRadPx = ((hv_Focus / hv_DistPMaxCam) * hv_CylinderRadius) / hv_S;
                    }
                    hv_Shift1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift1 = (((hv_Column - hv_CylRadPx) * hv_S) * hv_DistPMaxCam) / hv_Focus;
                    }
                    hv_Shift2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift2 = (((hv_Width - (hv_Column + hv_CylRadPx)) * hv_S) * hv_DistPMaxCam) / hv_Focus;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_MaxShift = ((((hv_MaxShift.TupleConcat(
                                hv_Shift1))).TupleConcat(hv_Shift2))).TupleMax();
                            hv_MaxShift.Dispose();
                            hv_MaxShift = ExpTmpLocalVar_MaxShift;
                        }
                    }

                    //- at a position where the camera looks "perpendicular" to the cylinder
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Foot.Dispose();
                        point_to_line_perpendicular_foot(hv_CamPose.TupleSelectRange(0, 2), ((new HTuple(0)).TupleConcat(
                            0)).TupleConcat(0), ((new HTuple(0)).TupleConcat(0)).TupleConcat(1),
                            out hv_Foot);
                    }
                    hv_PFootCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PFootCam = (hv_CamPose.TupleSelectRange(
                            0, 2)) - hv_Foot;
                    }
                    hv_DistPFootCam.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistPFootCam = ((((hv_PFootCam * hv_PFootCam)).TupleSum()
                            )).TupleSqrt();
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                        HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, 0, 0, hv_Foot.TupleSelect(
                            2), out hv_Qx, out hv_Qy, out hv_Qz);
                    }
                    hv_Row.Dispose(); hv_Column.Dispose();
                    HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam, out hv_Row,
                        out hv_Column);
                    hv_CylRadPx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CylRadPx = ((hv_Focus / hv_DistPFootCam) * hv_CylinderRadius) / hv_S;
                    }
                    hv_Shift1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift1 = (((hv_Column - hv_CylRadPx) * hv_S) * hv_DistPFootCam) / hv_Focus;
                    }
                    hv_Shift2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Shift2 = (((hv_Width - (hv_Column + hv_CylRadPx)) * hv_S) * hv_DistPFootCam) / hv_Focus;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_MaxShift = ((((hv_MaxShift.TupleConcat(
                                hv_Shift1))).TupleConcat(hv_Shift2))).TupleMax();
                            hv_MaxShift.Dispose();
                            hv_MaxShift = ExpTmpLocalVar_MaxShift;
                        }
                    }


                    if (hv_MinDist == null)
                        hv_MinDist = new HTuple();
                    hv_MinDist[hv_Cam] = ((((hv_DistPMinCam.TupleConcat(hv_DistPMaxCam))).TupleConcat(
                        hv_DistPFootCam))).TupleMin();
                    if (hv_MaxDist == null)
                        hv_MaxDist = new HTuple();
                    hv_MaxDist[hv_Cam] = ((((hv_DistPMinCam.TupleConcat(hv_DistPMaxCam))).TupleConcat(
                        hv_DistPFootCam))).TupleMax();
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_MaxShift = 1.05 * hv_MaxShift;
                        hv_MaxShift.Dispose();
                        hv_MaxShift = ExpTmpLocalVar_MaxShift;
                    }
                }

                //Approximately determine the minimum and maximum distance between the
                //opposite silhouettes of the cylinder in the images assuming that this
                //distance is measured perpendicular to the cylinder axis
                hv_MinPairDist.Dispose();
                hv_MinPairDist = new HTuple();
                hv_MaxPairDist.Dispose();
                hv_MaxPairDist = new HTuple();
                HTuple end_val61 = hv_NumCameras - 1;
                HTuple step_val61 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val61, step_val61); hv_Cam = hv_Cam.TupleAdd(step_val61))
                {
                    if (hv_MinPairDist == null)
                        hv_MinPairDist = new HTuple();
                    hv_MinPairDist[hv_Cam] = (((2.0 * hv_Focus) / ((hv_MaxDist.TupleSelect(hv_Cam)) + hv_MaxShift)) * hv_CylinderRadius) / hv_S;
                    if (hv_MaxPairDist == null)
                        hv_MaxPairDist = new HTuple();
                    hv_MaxPairDist[hv_Cam] = (((2.0 * hv_Focus) / ((hv_MinDist.TupleSelect(hv_Cam)) - hv_MaxShift)) * hv_CylinderRadius) / hv_S;
                }

                hv_SerializedItemHandle.Dispose();
                hv_CameraSetupModelPCA.Dispose();
                hv_MaxShift.Dispose();
                hv_MinDist.Dispose();
                hv_MaxDist.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Sx.Dispose();
                hv_Sy.Dispose();
                hv_Focus.Dispose();
                hv_S.Dispose();
                hv_PMinCam.Dispose();
                hv_DistPMinCam.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_CylRadPx.Dispose();
                hv_Shift1.Dispose();
                hv_Shift2.Dispose();
                hv_PMaxCam.Dispose();
                hv_DistPMaxCam.Dispose();
                hv_Foot.Dispose();
                hv_PFootCam.Dispose();
                hv_DistPFootCam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_SerializedItemHandle.Dispose();
                hv_CameraSetupModelPCA.Dispose();
                hv_MaxShift.Dispose();
                hv_MinDist.Dispose();
                hv_MaxDist.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Sx.Dispose();
                hv_Sy.Dispose();
                hv_Focus.Dispose();
                hv_S.Dispose();
                hv_PMinCam.Dispose();
                hv_DistPMinCam.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_CylRadPx.Dispose();
                hv_Shift1.Dispose();
                hv_Shift2.Dispose();
                hv_PMaxCam.Dispose();
                hv_DistPMaxCam.Dispose();
                hv_Foot.Dispose();
                hv_PFootCam.Dispose();
                hv_DistPFootCam.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_PixelSize"></param>
        /// <param name="hv_CylinderRadius"></param>
        /// <param name="hv_MinZ"></param>
        /// <param name="hv_MaxZ"></param>
        /// <param name="hv_NumSlices"></param>
        /// <param name="hv_NumPointsPerSlice"></param>
        /// <param name="hv_MinZI"></param>
        /// <param name="hv_MaxZI"></param>
        /// <param name="hv_PxSampled"></param>
        /// <param name="hv_PySampled"></param>
        /// <param name="hv_PzSampled"></param>
        private void gen_cylinder_model(HTuple hv_PixelSize, HTuple hv_CylinderRadius,
              HTuple hv_MinZ, HTuple hv_MaxZ, out HTuple hv_NumSlices, out HTuple hv_NumPointsPerSlice,
              out HTuple hv_MinZI, out HTuple hv_MaxZI, out HTuple hv_PxSampled, out HTuple hv_PySampled,
              out HTuple hv_PzSampled)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Circumference = new HTuple(), hv_ActualPixelSize = new HTuple();
            HTuple hv_AngleStep = new HTuple(), hv_FullAngle = new HTuple();
            HTuple hv_Angles = new HTuple(), hv_NumNM = new HTuple();
            HTuple hv_TupleOnes = new HTuple(), hv_I = new HTuple();
            HTuple hv_Indices = new HTuple();
            // Initialize local and output iconic variables 
            hv_NumSlices = new HTuple();
            hv_NumPointsPerSlice = new HTuple();
            hv_MinZI = new HTuple();
            hv_MaxZI = new HTuple();
            hv_PxSampled = new HTuple();
            hv_PySampled = new HTuple();
            hv_PzSampled = new HTuple();
            try
            {

                //Determine the number of points per slice
                hv_Circumference.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Circumference = (2.0 * hv_CylinderRadius) * 3.1415926;
                }
                hv_NumPointsPerSlice.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumPointsPerSlice = (((hv_Circumference / hv_PixelSize)).TupleInt()
                        ) + 1;
                }
                hv_ActualPixelSize.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ActualPixelSize = hv_Circumference / hv_NumPointsPerSlice;
                }
                hv_AngleStep.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_AngleStep = hv_ActualPixelSize / hv_CylinderRadius;
                }
                hv_FullAngle.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_FullAngle = hv_AngleStep * (hv_NumPointsPerSlice - 1);
                }
                hv_Angles.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Angles = ((-0.5 * hv_FullAngle) - hv_AngleStep) + ((HTuple.TupleGenConst(
                        hv_NumPointsPerSlice, hv_AngleStep)).TupleCumul());
                }

                //Determine the number of slices
                hv_MinZI.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MinZI = (((((hv_MinZ.TupleFabs()
                        ) / hv_ActualPixelSize)).TupleInt()) + 1) * (hv_MinZ.TupleSgn());
                }
                hv_MaxZI.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MaxZI = (((((hv_MaxZ.TupleFabs()
                        ) / hv_ActualPixelSize)).TupleInt()) + 1) * (hv_MaxZ.TupleSgn());
                }
                hv_NumSlices.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumSlices = (hv_MinZI.TupleAbs()
                        ) + (hv_MaxZI.TupleAbs());
                }

                hv_NumNM.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumNM = hv_NumSlices * hv_NumPointsPerSlice;
                }
                hv_PxSampled.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PxSampled = HTuple.TupleGenConst(
                        hv_NumNM, 0);
                }
                hv_PySampled.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PySampled = HTuple.TupleGenConst(
                        hv_NumNM, 0);
                }
                hv_PzSampled.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PzSampled = HTuple.TupleGenConst(
                        hv_NumNM, 0);
                }
                hv_TupleOnes.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_TupleOnes = HTuple.TupleGenConst(
                        new HTuple(hv_Angles.TupleLength()), 1);
                }
                HTuple end_val19 = hv_NumSlices - 1;
                HTuple step_val19 = 1;
                for (hv_I = 0; hv_I.Continue(end_val19, step_val19); hv_I = hv_I.TupleAdd(step_val19))
                {
                    hv_Indices.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Indices = HTuple.TupleGenSequence(
                            hv_I * hv_NumPointsPerSlice, ((hv_I + 1) * hv_NumPointsPerSlice) - 1, 1);
                    }
                    if (hv_PxSampled == null)
                        hv_PxSampled = new HTuple();
                    hv_PxSampled[hv_Indices] = (hv_Angles.TupleCos()) * hv_CylinderRadius;
                    if (hv_PySampled == null)
                        hv_PySampled = new HTuple();
                    hv_PySampled[hv_Indices] = (hv_Angles.TupleSin()) * hv_CylinderRadius;
                    if (hv_PzSampled == null)
                        hv_PzSampled = new HTuple();
                    hv_PzSampled[hv_Indices] = ((hv_MaxZI - hv_I) * hv_ActualPixelSize) * hv_TupleOnes;
                }


                hv_Circumference.Dispose();
                hv_ActualPixelSize.Dispose();
                hv_AngleStep.Dispose();
                hv_FullAngle.Dispose();
                hv_Angles.Dispose();
                hv_NumNM.Dispose();
                hv_TupleOnes.Dispose();
                hv_I.Dispose();
                hv_Indices.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Circumference.Dispose();
                hv_ActualPixelSize.Dispose();
                hv_AngleStep.Dispose();
                hv_FullAngle.Dispose();
                hv_Angles.Dispose();
                hv_NumNM.Dispose();
                hv_TupleOnes.Dispose();
                hv_I.Dispose();
                hv_Indices.Dispose();

                throw HDevExpDefaultException;
            }
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
            try
            {
                string tempOCRLibPath = AppDomain.CurrentDomain.BaseDirectory + "AlgorithmPlug\\BottleInspectionAlgorithm";

                //百度OCR库
                OCRModelConfig config = new OCRModelConfig();
                string modelPathroot = tempOCRLibPath + @"\inference";

                config.det_infer = modelPathroot + @"\en_PP-OCRv3_det_slim_infer";
                config.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
                config.rec_infer = modelPathroot + @"\ch_PP-OCRv4_rec_server_infer";
                config.keys = modelPathroot + @"\ppocr_keys.txt";

                //OCR参数
                OCRParameter oCRParameter = new OCRParameter();
                oCRParameter.cpu_math_library_num_threads = 10;//预测并发线程数
                oCRParameter.enable_mkldnn = true;//web部署该值建议设置为0,否则出错，内存如果使用很大，建议该值也设置为0.
                oCRParameter.cls = false; //是否执行文字方向分类；默认false
                oCRParameter.det = true;//是否开启方向检测，用于检测识别180旋转
                oCRParameter.use_angle_cls = false;//是否开启方向检测，用于检测识别180旋转
                oCRParameter.det_db_score_mode = true;//是否使用多段线，即文字区域是用多段线还是用矩形，

                oCRParameter.rec_img_h = 24;
                oCRParameter.rec_img_w = 40;

                //初始化OCR引擎
                m_PaddleOCREngine = new PaddleOCREngine(config, oCRParameter);

                return true;
            }
            catch (Exception)
            {
                return false;
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
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="ho_RectificationMaps"></param>
        /// <param name="hv_CameraSetupModel"></param>
        /// <param name="hv_CameraSetupModelZeroDist"></param>
        /// <param name="hv_NumCameras"></param>
        private void prepare_distortion_removal(out HObject ho_RectificationMaps, HTuple hv_CameraSetupModel,
                out HTuple hv_CameraSetupModelZeroDist, out HTuple hv_NumCameras)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Map = null;

            // Local control variables 

            HTuple hv_SerializedItemHandle = new HTuple();
            HTuple hv_Cam = new HTuple(), hv_CamParIn = new HTuple();
            HTuple hv_CameraType = new HTuple(), hv_IsPolynomial = new HTuple();
            HTuple hv_CamParOut = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RectificationMaps);
            HOperatorSet.GenEmptyObj(out ho_Map);
            hv_CameraSetupModelZeroDist = new HTuple();
            hv_NumCameras = new HTuple();
            try
            {
                hv_SerializedItemHandle.Dispose();
                HOperatorSet.SerializeCameraSetupModel(hv_CameraSetupModel, out hv_SerializedItemHandle);
                hv_CameraSetupModelZeroDist.Dispose();
                HOperatorSet.DeserializeCameraSetupModel(hv_SerializedItemHandle, out hv_CameraSetupModelZeroDist);
                //Create maps to remove the radial distortions from the images efficently
                hv_NumCameras.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, "general", "num_cameras",
                    out hv_NumCameras);
                ho_RectificationMaps.Dispose();
                HOperatorSet.GenEmptyObj(out ho_RectificationMaps);
                HTuple end_val5 = hv_NumCameras - 1;
                HTuple step_val5 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val5, step_val5); hv_Cam = hv_Cam.TupleAdd(step_val5))
                {
                    hv_CamParIn.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "params",
                        out hv_CamParIn);
                    //Calculate camera parameters for the distortion-free images
                    hv_CameraType.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "type",
                        out hv_CameraType);
                    hv_IsPolynomial.Dispose();
                    HOperatorSet.TupleRegexpTest(hv_CameraType, "_polynomial$", out hv_IsPolynomial);
                    if ((int)(hv_IsPolynomial) != 0)
                    {
                        hv_CamParOut.Dispose();
                        HOperatorSet.ChangeRadialDistortionCamPar("fixed", hv_CamParIn, ((((new HTuple(0.0)).TupleConcat(
                            0.0)).TupleConcat(0.0)).TupleConcat(0.0)).TupleConcat(0.0), out hv_CamParOut);
                    }
                    else
                    {
                        hv_CamParOut.Dispose();
                        HOperatorSet.ChangeRadialDistortionCamPar("fixed", hv_CamParIn, 0.0, out hv_CamParOut);
                    }
                    //Set these new parameter in the setup
                    HOperatorSet.SetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "params",
                        hv_CamParOut);
                    //Prepare the rectification maps
                    ho_Map.Dispose();
                    HOperatorSet.GenRadialDistortionMap(out ho_Map, hv_CamParIn, hv_CamParOut,
                        "bilinear");
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_RectificationMaps, ho_Map, out ExpTmpOutVar_0);
                        ho_RectificationMaps.Dispose();
                        ho_RectificationMaps = ExpTmpOutVar_0;
                    }
                }
                ho_Map.Dispose();

                hv_SerializedItemHandle.Dispose();
                hv_Cam.Dispose();
                hv_CamParIn.Dispose();
                hv_CameraType.Dispose();
                hv_IsPolynomial.Dispose();
                hv_CamParOut.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Map.Dispose();

                hv_SerializedItemHandle.Dispose();
                hv_Cam.Dispose();
                hv_CamParIn.Dispose();
                hv_CameraType.Dispose();
                hv_IsPolynomial.Dispose();
                hv_CamParOut.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// Fit a plane to the given 3D points 
        /// </summary>
        /// <param name="hv_X"></param>
        /// <param name="hv_Y"></param>
        /// <param name="hv_Z"></param>
        /// <param name="hv_NX"></param>
        /// <param name="hv_NY"></param>
        /// <param name="hv_NZ"></param>
        /// <param name="hv_C"></param>
        private void fit_plane(HTuple hv_X, HTuple hv_Y, HTuple hv_Z, out HTuple hv_NX,
            out HTuple hv_NY, out HTuple hv_NZ, out HTuple hv_C)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Num = new HTuple(), hv_XM = new HTuple();
            HTuple hv_YM = new HTuple(), hv_ZM = new HTuple(), hv_DX = new HTuple();
            HTuple hv_DY = new HTuple(), hv_DZ = new HTuple(), hv_MA11 = new HTuple();
            HTuple hv_MA22 = new HTuple(), hv_MA33 = new HTuple();
            HTuple hv_MA12 = new HTuple(), hv_MA13 = new HTuple();
            HTuple hv_MA23 = new HTuple(), hv_MatrixID = new HTuple();
            HTuple hv_EigenvaluesID = new HTuple(), hv_EigenvectorsID = new HTuple();
            // Initialize local and output iconic variables 
            hv_NX = new HTuple();
            hv_NY = new HTuple();
            hv_NZ = new HTuple();
            hv_C = new HTuple();
            try
            {
                //number of points
                hv_Num.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Num = new HTuple(hv_X.TupleLength()
                        );
                }

                //center of gravity of all points
                hv_XM.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_XM = hv_X.TupleMean()
                        ;
                }
                hv_YM.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_YM = hv_Y.TupleMean()
                        ;
                }
                hv_ZM.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ZM = hv_Z.TupleMean()
                        ;
                }

                //symmetric matrix M(A)
                hv_DX.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DX = hv_X - hv_XM;
                }
                hv_DY.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DY = hv_Y - hv_YM;
                }
                hv_DZ.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DZ = hv_Z - hv_ZM;
                }
                hv_MA11.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA11 = ((hv_DX * hv_DX)).TupleSum()
                        ;
                }
                hv_MA22.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA22 = ((hv_DY * hv_DY)).TupleSum()
                        ;
                }
                hv_MA33.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA33 = ((hv_DZ * hv_DZ)).TupleSum()
                        ;
                }
                hv_MA12.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA12 = ((hv_DX * hv_DY)).TupleSum()
                        ;
                }
                hv_MA13.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA13 = ((hv_DX * hv_DZ)).TupleSum()
                        ;
                }
                hv_MA23.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MA23 = ((hv_DY * hv_DZ)).TupleSum()
                        ;
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MatrixID.Dispose();
                    HOperatorSet.CreateMatrix(3, 3, ((((((((((((((hv_MA11.TupleConcat(hv_MA12))).TupleConcat(
                        hv_MA13))).TupleConcat(hv_MA12))).TupleConcat(hv_MA22))).TupleConcat(hv_MA23))).TupleConcat(
                        hv_MA13))).TupleConcat(hv_MA23))).TupleConcat(hv_MA33), out hv_MatrixID);
                }

                //the normal vector is the eigenvector that belongs to the smallest
                //eigenvalue auf the matrix MatrixID
                hv_EigenvaluesID.Dispose(); hv_EigenvectorsID.Dispose();
                HOperatorSet.EigenvaluesSymmetricMatrix(hv_MatrixID, "true", out hv_EigenvaluesID,
                    out hv_EigenvectorsID);
                hv_NX.Dispose();
                HOperatorSet.GetValueMatrix(hv_EigenvectorsID, 0, 0, out hv_NX);
                hv_NY.Dispose();
                HOperatorSet.GetValueMatrix(hv_EigenvectorsID, 1, 0, out hv_NY);
                hv_NZ.Dispose();
                HOperatorSet.GetValueMatrix(hv_EigenvectorsID, 2, 0, out hv_NZ);

                //the constant of the normal-constant form of the plane (n*P = c)
                hv_C.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_C = ((hv_NX * hv_XM) + (hv_NY * hv_YM)) + (hv_NZ * hv_ZM);
                }
                if ((int)(new HTuple(hv_C.TupleLess(0.0))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_NX = -hv_NX;
                            hv_NX.Dispose();
                            hv_NX = ExpTmpLocalVar_NX;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_NY = -hv_NY;
                            hv_NY.Dispose();
                            hv_NY = ExpTmpLocalVar_NY;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_NZ = -hv_NZ;
                            hv_NZ.Dispose();
                            hv_NZ = ExpTmpLocalVar_NZ;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_C = -hv_C;
                            hv_C.Dispose();
                            hv_C = ExpTmpLocalVar_C;
                        }
                    }
                }

                hv_Num.Dispose();
                hv_XM.Dispose();
                hv_YM.Dispose();
                hv_ZM.Dispose();
                hv_DX.Dispose();
                hv_DY.Dispose();
                hv_DZ.Dispose();
                hv_MA11.Dispose();
                hv_MA22.Dispose();
                hv_MA33.Dispose();
                hv_MA12.Dispose();
                hv_MA13.Dispose();
                hv_MA23.Dispose();
                hv_MatrixID.Dispose();
                hv_EigenvaluesID.Dispose();
                hv_EigenvectorsID.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Num.Dispose();
                hv_XM.Dispose();
                hv_YM.Dispose();
                hv_ZM.Dispose();
                hv_DX.Dispose();
                hv_DY.Dispose();
                hv_DZ.Dispose();
                hv_MA11.Dispose();
                hv_MA22.Dispose();
                hv_MA33.Dispose();
                hv_MA12.Dispose();
                hv_MA13.Dispose();
                hv_MA23.Dispose();
                hv_MatrixID.Dispose();
                hv_EigenvaluesID.Dispose();
                hv_EigenvectorsID.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// Calculates the cross product of the two 3d vectors given in V1 and V2 
        /// </summary>
        /// <param name="hv_V1"></param>
        /// <param name="hv_V2"></param>
        /// <param name="hv_CrossProduct"></param>
        private void cross_product(HTuple hv_V1, HTuple hv_V2, out HTuple hv_CrossProduct)
        {



            // Local iconic variables 
            // Initialize local and output iconic variables 
            hv_CrossProduct = new HTuple();
            //Determine the cross product of the two given vectors
            //Note that only the first three values of the given vectors are taken into account
            hv_CrossProduct.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_CrossProduct = new HTuple();
                hv_CrossProduct = hv_CrossProduct.TupleConcat(((hv_V1.TupleSelect(
                    1)) * (hv_V2.TupleSelect(2))) - ((hv_V1.TupleSelect(2)) * (hv_V2.TupleSelect(1))));
                hv_CrossProduct = hv_CrossProduct.TupleConcat(((hv_V1.TupleSelect(
                    2)) * (hv_V2.TupleSelect(0))) - ((hv_V1.TupleSelect(0)) * (hv_V2.TupleSelect(2))));
                hv_CrossProduct = hv_CrossProduct.TupleConcat(((hv_V1.TupleSelect(
                    0)) * (hv_V2.TupleSelect(1))) - ((hv_V1.TupleSelect(1)) * (hv_V2.TupleSelect(0))));
            }


            return;
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_CameraSetupModelZeroDist"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_PoseCylinderApprox"></param>
        /// <param name="hv_HomMat3DCylinderApprox"></param>
        private void determine_approximate_cylinder_pose_in_center_of_cameras(HTuple hv_CameraSetupModelZeroDist,
            HTuple hv_NumCameras, out HTuple hv_PoseCylinderApprox, out HTuple hv_HomMat3DCylinderApprox)
        {

            // Local iconic variables 

            // Local control variables 

            HTuple hv_XCam = new HTuple(), hv_YCam = new HTuple();
            HTuple hv_ZCam = new HTuple(), hv_Cam = new HTuple(), hv_CamPose = new HTuple();
            HTuple hv_CylinderPointApprox = new HTuple(), hv_LenI = new HTuple();
            HTuple hv_CylinderXTmp = new HTuple(), hv_NX = new HTuple();
            HTuple hv_NY = new HTuple(), hv_NZ = new HTuple(), hv_C = new HTuple();
            HTuple hv_CylinderZTmp = new HTuple(), hv_CylinderYTmp = new HTuple();
            // Initialize local and output iconic variables 
            hv_PoseCylinderApprox = new HTuple();
            hv_HomMat3DCylinderApprox = new HTuple();
            try
            {
                hv_XCam.Dispose();
                hv_XCam = new HTuple();
                hv_YCam.Dispose();
                hv_YCam = new HTuple();
                hv_ZCam.Dispose();
                hv_ZCam = new HTuple();
                HTuple end_val3 = hv_NumCameras - 1;
                HTuple step_val3 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val3, step_val3); hv_Cam = hv_Cam.TupleAdd(step_val3))
                {
                    hv_CamPose.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "pose",
                        out hv_CamPose);
                    if (hv_XCam == null)
                        hv_XCam = new HTuple();
                    hv_XCam[hv_Cam] = hv_CamPose.TupleSelect(0);
                    if (hv_YCam == null)
                        hv_YCam = new HTuple();
                    hv_YCam[hv_Cam] = hv_CamPose.TupleSelect(1);
                    if (hv_ZCam == null)
                        hv_ZCam = new HTuple();
                    hv_ZCam[hv_Cam] = hv_CamPose.TupleSelect(2);
                }
                hv_CylinderPointApprox.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CylinderPointApprox = new HTuple();
                    hv_CylinderPointApprox = hv_CylinderPointApprox.TupleConcat(hv_XCam.TupleMean()
                        );
                    hv_CylinderPointApprox = hv_CylinderPointApprox.TupleConcat(hv_YCam.TupleMean()
                        );
                    hv_CylinderPointApprox = hv_CylinderPointApprox.TupleConcat(hv_ZCam.TupleMean()
                        );
                }
                hv_LenI.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_LenI = 1.0 / (((((hv_CylinderPointApprox * hv_CylinderPointApprox)).TupleSum()
                        )).TupleSqrt());
                }
                hv_CylinderXTmp.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CylinderXTmp = hv_CylinderPointApprox * hv_LenI;
                }
                hv_NX.Dispose(); hv_NY.Dispose(); hv_NZ.Dispose(); hv_C.Dispose();
                fit_plane(hv_XCam, hv_YCam, hv_ZCam, out hv_NX, out hv_NY, out hv_NZ, out hv_C);
                hv_CylinderZTmp.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CylinderZTmp = new HTuple();
                    hv_CylinderZTmp = hv_CylinderZTmp.TupleConcat(hv_NX, hv_NY, hv_NZ);
                }
                //Ensure correct orientation of the cylinder axis
                if ((int)(new HTuple(hv_NX.TupleGreater(0))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_CylinderZTmp = -hv_CylinderZTmp;
                            hv_CylinderZTmp.Dispose();
                            hv_CylinderZTmp = ExpTmpLocalVar_CylinderZTmp;
                        }
                    }
                }
                hv_CylinderYTmp.Dispose();
                cross_product(hv_CylinderZTmp, hv_CylinderXTmp, out hv_CylinderYTmp);
                hv_HomMat3DCylinderApprox.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_HomMat3DCylinderApprox = new HTuple();
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderXTmp.TupleSelect(
                        0));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderYTmp.TupleSelect(
                        0));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderZTmp.TupleSelect(
                        0));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderPointApprox.TupleSelect(
                        0));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderXTmp.TupleSelect(
                        1));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderYTmp.TupleSelect(
                        1));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderZTmp.TupleSelect(
                        1));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderPointApprox.TupleSelect(
                        1));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderXTmp.TupleSelect(
                        2));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderYTmp.TupleSelect(
                        2));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderZTmp.TupleSelect(
                        2));
                    hv_HomMat3DCylinderApprox = hv_HomMat3DCylinderApprox.TupleConcat(hv_CylinderPointApprox.TupleSelect(
                        2));
                }
                hv_PoseCylinderApprox.Dispose();
                HOperatorSet.HomMat3dToPose(hv_HomMat3DCylinderApprox, out hv_PoseCylinderApprox);

                hv_XCam.Dispose();
                hv_YCam.Dispose();
                hv_ZCam.Dispose();
                hv_Cam.Dispose();
                hv_CamPose.Dispose();
                hv_CylinderPointApprox.Dispose();
                hv_LenI.Dispose();
                hv_CylinderXTmp.Dispose();
                hv_NX.Dispose();
                hv_NY.Dispose();
                hv_NZ.Dispose();
                hv_C.Dispose();
                hv_CylinderZTmp.Dispose();
                hv_CylinderYTmp.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_XCam.Dispose();
                hv_YCam.Dispose();
                hv_ZCam.Dispose();
                hv_Cam.Dispose();
                hv_CamPose.Dispose();
                hv_CylinderPointApprox.Dispose();
                hv_LenI.Dispose();
                hv_CylinderXTmp.Dispose();
                hv_NX.Dispose();
                hv_NY.Dispose();
                hv_NZ.Dispose();
                hv_C.Dispose();
                hv_CylinderZTmp.Dispose();
                hv_CylinderYTmp.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_L1a"></param>
        /// <param name="hv_L1b"></param>
        /// <param name="hv_L2a"></param>
        /// <param name="hv_L2b"></param>
        /// <param name="hv_FootOnLine"></param>
        /// <param name="hv_P"></param>
        private void skew_lines_perpendicular_foot(HTuple hv_L1a, HTuple hv_L1b, HTuple hv_L2a,
            HTuple hv_L2b, HTuple hv_FootOnLine, out HTuple hv_P)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_a1 = new HTuple(), hv_b1 = new HTuple();
            HTuple hv_a2 = new HTuple(), hv_b2 = new HTuple(), hv_da = new HTuple();
            HTuple hv_b1cb2 = new HTuple(), hv_b1cb2cb2 = new HTuple();
            HTuple hv_nom = new HTuple(), hv_den = new HTuple(), hv_t = new HTuple();
            // Initialize local and output iconic variables 
            hv_P = new HTuple();
            try
            {
                switch (hv_FootOnLine.I)
                {
                    case 1:
                        hv_a1.Dispose();
                        hv_a1 = new HTuple(hv_L1a);
                        hv_b1.Dispose();
                        hv_b1 = new HTuple(hv_L1b);
                        hv_a2.Dispose();
                        hv_a2 = new HTuple(hv_L2a);
                        hv_b2.Dispose();
                        hv_b2 = new HTuple(hv_L2b);
                        break;
                    case 2:
                        hv_a1.Dispose();
                        hv_a1 = new HTuple(hv_L2a);
                        hv_b1.Dispose();
                        hv_b1 = new HTuple(hv_L2b);
                        hv_a2.Dispose();
                        hv_a2 = new HTuple(hv_L1a);
                        hv_b2.Dispose();
                        hv_b2 = new HTuple(hv_L1b);
                        break;
                    default:
                        throw new HalconException("Wrong value of FootOnLine: " + hv_FootOnLine);
                        break;
                }

                hv_da.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_da = hv_a1 - hv_a2;
                }
                hv_b1cb2.Dispose();
                cross_product(hv_b1, hv_b2, out hv_b1cb2);
                hv_b1cb2cb2.Dispose();
                cross_product(hv_b1cb2, hv_b2, out hv_b1cb2cb2);
                hv_nom.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_nom = ((hv_da * hv_b1cb2cb2)).TupleSum()
                        ;
                }
                hv_den.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_den = ((hv_b1 * hv_b1cb2cb2)).TupleSum()
                        ;
                }
                if ((int)(new HTuple(((hv_den.TupleFabs())).TupleLess(1e-20))) != 0)
                {
                    //Lines are parallel
                    hv_P.Dispose();
                    hv_P = new HTuple(hv_a1);
                }
                else
                {
                    //Foot point exists
                    hv_t.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_t = (-hv_nom) / hv_den;
                    }
                    hv_P.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_P = hv_a1 + (hv_t * hv_b1);
                    }
                }

                hv_a1.Dispose();
                hv_b1.Dispose();
                hv_a2.Dispose();
                hv_b2.Dispose();
                hv_da.Dispose();
                hv_b1cb2.Dispose();
                hv_b1cb2cb2.Dispose();
                hv_nom.Dispose();
                hv_den.Dispose();
                hv_t.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_a1.Dispose();
                hv_b1.Dispose();
                hv_a2.Dispose();
                hv_b2.Dispose();
                hv_da.Dispose();
                hv_b1cb2.Dispose();
                hv_b1cb2cb2.Dispose();
                hv_nom.Dispose();
                hv_den.Dispose();
                hv_t.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_LabelMinRow"></param>
        /// <param name="hv_LabelMaxRow"></param>
        /// <param name="hv_Width"></param>
        /// <param name="hv_CameraSetupModelZeroDist"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_HomMat3DCylinderApprox"></param>
        /// <param name="hv_MinZ"></param>
        /// <param name="hv_MaxZ"></param>
        private void determine_required_cylinder_model_extent(HTuple hv_LabelMinRow, HTuple hv_LabelMaxRow,
            HTuple hv_Width, HTuple hv_CameraSetupModelZeroDist, HTuple hv_NumCameras, HTuple hv_HomMat3DCylinderApprox,
            out HTuple hv_MinZ, out HTuple hv_MaxZ)
        {



            // Local iconic variables 

            HObject ho_ContourROI;

            // Local control variables 

            HTuple hv_Cam = new HTuple(), hv_CamParam = new HTuple();
            HTuple hv_CamPose = new HTuple(), hv_HomMat3D = new HTuple();
            HTuple hv_HomMat3DCompose = new HTuple(), hv_Qx = new HTuple();
            HTuple hv_Qy = new HTuple(), hv_Qz = new HTuple(), hv_AxisRow = new HTuple();
            HTuple hv_AxisColumn = new HTuple(), hv_IntersectionRow = new HTuple();
            HTuple hv_IntersectionColumn = new HTuple(), hv_IsOverlapping = new HTuple();
            HTuple hv_PxC = new HTuple(), hv_PyC = new HTuple(), hv_PzC = new HTuple();
            HTuple hv_QxC = new HTuple(), hv_QyC = new HTuple(), hv_QzC = new HTuple();
            HTuple hv_HomMat3DInvert = new HTuple(), hv_Px = new HTuple();
            HTuple hv_Py = new HTuple(), hv_Pz = new HTuple(), hv_P1 = new HTuple();
            HTuple hv_P2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ContourROI);
            hv_MinZ = new HTuple();
            hv_MaxZ = new HTuple();
            try
            {
                hv_MinZ.Dispose();
                hv_MinZ = 999;
                hv_MaxZ.Dispose();
                hv_MaxZ = -999;
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_ContourROI.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_ContourROI, ((((((hv_LabelMinRow.TupleConcat(
                        hv_LabelMaxRow))).TupleConcat(hv_LabelMaxRow))).TupleConcat(hv_LabelMinRow))).TupleConcat(
                        hv_LabelMinRow), ((((((new HTuple(0)).TupleConcat(0)).TupleConcat(hv_Width - 1))).TupleConcat(
                        hv_Width - 1))).TupleConcat(0));
                }
                HTuple end_val3 = hv_NumCameras - 1;
                HTuple step_val3 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val3, step_val3); hv_Cam = hv_Cam.TupleAdd(step_val3))
                {
                    hv_CamParam.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "params",
                        out hv_CamParam);
                    hv_CamPose.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDist, hv_Cam, "pose",
                        out hv_CamPose);
                    //Project the cylinder axis into the image
                    hv_HomMat3D.Dispose();
                    HOperatorSet.PoseToHomMat3d(hv_CamPose, out hv_HomMat3D);
                    hv_HomMat3DCompose.Dispose();
                    HOperatorSet.HomMat3dCompose(hv_HomMat3D, hv_HomMat3DCylinderApprox, out hv_HomMat3DCompose);
                    hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DCompose, (new HTuple(0)).TupleConcat(
                        0), (new HTuple(0)).TupleConcat(0), (new HTuple(-1)).TupleConcat(1),
                        out hv_Qx, out hv_Qy, out hv_Qz);
                    hv_AxisRow.Dispose(); hv_AxisColumn.Dispose();
                    HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam, out hv_AxisRow,
                        out hv_AxisColumn);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_IntersectionRow.Dispose(); hv_IntersectionColumn.Dispose(); hv_IsOverlapping.Dispose();
                        HOperatorSet.IntersectionLineContourXld(ho_ContourROI, hv_AxisRow.TupleSelect(
                            0), hv_AxisColumn.TupleSelect(0), hv_AxisRow.TupleSelect(1), hv_AxisColumn.TupleSelect(
                            1), out hv_IntersectionRow, out hv_IntersectionColumn, out hv_IsOverlapping);
                    }
                    hv_PxC.Dispose(); hv_PyC.Dispose(); hv_PzC.Dispose(); hv_QxC.Dispose(); hv_QyC.Dispose(); hv_QzC.Dispose();
                    HOperatorSet.GetLineOfSight(hv_IntersectionRow, hv_IntersectionColumn, hv_CamParam,
                        out hv_PxC, out hv_PyC, out hv_PzC, out hv_QxC, out hv_QyC, out hv_QzC);
                    hv_HomMat3DInvert.Dispose();
                    HOperatorSet.HomMat3dInvert(hv_HomMat3DCompose, out hv_HomMat3DInvert);
                    hv_Px.Dispose(); hv_Py.Dispose(); hv_Pz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, hv_PxC, hv_PyC, hv_PzC,
                        out hv_Px, out hv_Py, out hv_Pz);
                    hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, hv_QxC, hv_QyC, hv_QzC,
                        out hv_Qx, out hv_Qy, out hv_Qz);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_P1.Dispose();
                        skew_lines_perpendicular_foot(((new HTuple(0)).TupleConcat(0)).TupleConcat(
                            0), ((new HTuple(0)).TupleConcat(0)).TupleConcat(1), ((((hv_Px.TupleSelect(
                            0))).TupleConcat(hv_Py.TupleSelect(0)))).TupleConcat(hv_Pz.TupleSelect(
                            0)), (((((hv_Qx.TupleSelect(0)) - (hv_Px.TupleSelect(0)))).TupleConcat(
                            (hv_Qy.TupleSelect(0)) - (hv_Py.TupleSelect(0))))).TupleConcat((hv_Qz.TupleSelect(
                            0)) - (hv_Pz.TupleSelect(0))), 1, out hv_P1);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_P2.Dispose();
                        skew_lines_perpendicular_foot(((new HTuple(0)).TupleConcat(0)).TupleConcat(
                            0), ((new HTuple(0)).TupleConcat(0)).TupleConcat(1), ((((hv_Px.TupleSelect(
                            1))).TupleConcat(hv_Py.TupleSelect(1)))).TupleConcat(hv_Pz.TupleSelect(
                            1)), (((((hv_Qx.TupleSelect(1)) - (hv_Px.TupleSelect(1)))).TupleConcat(
                            (hv_Qy.TupleSelect(1)) - (hv_Py.TupleSelect(1))))).TupleConcat((hv_Qz.TupleSelect(
                            1)) - (hv_Pz.TupleSelect(1))), 1, out hv_P2);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_MinZ = ((((hv_MinZ.TupleConcat(
                                hv_P1.TupleSelect(2)))).TupleConcat(hv_P2.TupleSelect(2)))).TupleMin()
                                ;
                            hv_MinZ.Dispose();
                            hv_MinZ = ExpTmpLocalVar_MinZ;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_MaxZ = ((((hv_MaxZ.TupleConcat(
                                hv_P1.TupleSelect(2)))).TupleConcat(hv_P2.TupleSelect(2)))).TupleMax()
                                ;
                            hv_MaxZ.Dispose();
                            hv_MaxZ = ExpTmpLocalVar_MaxZ;
                        }
                    }
                }
                ho_ContourROI.Dispose();

                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DCompose.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_AxisRow.Dispose();
                hv_AxisColumn.Dispose();
                hv_IntersectionRow.Dispose();
                hv_IntersectionColumn.Dispose();
                hv_IsOverlapping.Dispose();
                hv_PxC.Dispose();
                hv_PyC.Dispose();
                hv_PzC.Dispose();
                hv_QxC.Dispose();
                hv_QyC.Dispose();
                hv_QzC.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Px.Dispose();
                hv_Py.Dispose();
                hv_Pz.Dispose();
                hv_P1.Dispose();
                hv_P2.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ContourROI.Dispose();

                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DCompose.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_AxisRow.Dispose();
                hv_AxisColumn.Dispose();
                hv_IntersectionRow.Dispose();
                hv_IntersectionColumn.Dispose();
                hv_IsOverlapping.Dispose();
                hv_PxC.Dispose();
                hv_PyC.Dispose();
                hv_PzC.Dispose();
                hv_QxC.Dispose();
                hv_QyC.Dispose();
                hv_QzC.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Px.Dispose();
                hv_Py.Dispose();
                hv_Pz.Dispose();
                hv_P1.Dispose();
                hv_P2.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="ho_Images"></param>
        /// <param name="ho_RectificationMaps"></param>
        /// <param name="ho_ImagesRectified"></param>
        /// <param name="hv_NumCameras"></param>
        private void eliminate_radial_distortions(HObject ho_Images, HObject ho_RectificationMaps,
            out HObject ho_ImagesRectified, HTuple hv_NumCameras)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Image = null, ho_Map = null, ho_ImageRectified = null;

            // Local control variables 

            HTuple hv_Cam = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImagesRectified);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Map);
            HOperatorSet.GenEmptyObj(out ho_ImageRectified);
            try
            {
                ho_ImagesRectified.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ImagesRectified);
                HTuple end_val1 = hv_NumCameras - 1;
                HTuple step_val1 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val1, step_val1); hv_Cam = hv_Cam.TupleAdd(step_val1))
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Image.Dispose();
                        HOperatorSet.SelectObj(ho_Images, out ho_Image, hv_Cam + 1);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Map.Dispose();
                        HOperatorSet.SelectObj(ho_RectificationMaps, out ho_Map, hv_Cam + 1);
                    }
                    ho_ImageRectified.Dispose();
                    HOperatorSet.MapImage(ho_Image, ho_Map, out ho_ImageRectified);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_ImagesRectified, ho_ImageRectified, out ExpTmpOutVar_0
                            );
                        ho_ImagesRectified.Dispose();
                        ho_ImagesRectified = ExpTmpOutVar_0;
                    }
                }
                ho_Image.Dispose();
                ho_Map.Dispose();
                ho_ImageRectified.Dispose();

                hv_Cam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();
                ho_Map.Dispose();
                ho_ImageRectified.Dispose();

                hv_Cam.Dispose();

                throw HDevExpDefaultException;
            }
        }


        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="hv_A1x"></param>
        /// <param name="hv_A1y"></param>
        /// <param name="hv_A1z"></param>
        /// <param name="hv_B1x"></param>
        /// <param name="hv_B1y"></param>
        /// <param name="hv_B1z"></param>
        /// <param name="hv_A2x"></param>
        /// <param name="hv_A2y"></param>
        /// <param name="hv_A2z"></param>
        /// <param name="hv_B2x"></param>
        /// <param name="hv_B2y"></param>
        /// <param name="hv_B2z"></param>
        /// <param name="hv_Dist"></param>
        private void distance_skew_lines(HTuple hv_A1x, HTuple hv_A1y, HTuple hv_A1z, HTuple hv_B1x,
              HTuple hv_B1y, HTuple hv_B1z, HTuple hv_A2x, HTuple hv_A2y, HTuple hv_A2z, HTuple hv_B2x,
              HTuple hv_B2y, HTuple hv_B2z, out HTuple hv_Dist)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_DAx = new HTuple(), hv_DAy = new HTuple();
            HTuple hv_DAz = new HTuple(), hv_BCBx = new HTuple(), hv_BCBy = new HTuple();
            HTuple hv_BCBz = new HTuple();
            // Initialize local and output iconic variables 
            hv_Dist = new HTuple();
            try
            {
                hv_DAx.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DAx = hv_A1x - hv_A2x;
                }
                hv_DAy.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DAy = hv_A1y - hv_A2y;
                }
                hv_DAz.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DAz = hv_A1z - hv_A2z;
                }
                hv_BCBx.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BCBx = (hv_B1y * hv_B2z) - (hv_B1z * hv_B2y);
                }
                hv_BCBy.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BCBy = (hv_B1z * hv_B2x) - (hv_B1x * hv_B2z);
                }
                hv_BCBz.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BCBz = (hv_B1x * hv_B2y) - (hv_B1y * hv_B2x);
                }
                hv_Dist.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Dist = (((((hv_DAx * hv_BCBx) + (hv_DAy * hv_BCBy)) + (hv_DAz * hv_BCBz))).TupleFabs()
                        ) / (((((hv_BCBx * hv_BCBx) + (hv_BCBy * hv_BCBy)) + (hv_BCBz * hv_BCBz))).TupleSqrt()
                        );
                }

                hv_DAx.Dispose();
                hv_DAy.Dispose();
                hv_DAz.Dispose();
                hv_BCBx.Dispose();
                hv_BCBy.Dispose();
                hv_BCBz.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_DAx.Dispose();
                hv_DAy.Dispose();
                hv_DAz.Dispose();
                hv_BCBx.Dispose();
                hv_BCBy.Dispose();
                hv_BCBz.Dispose();

                throw HDevExpDefaultException;
            }
        }


        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="ho_Regions"></param>
        /// <param name="ho_ImagesRectified"></param>
        /// <param name="ho_ExpansionImage"></param>
        /// <param name="hv_ProcessImageIndex"></param>
        /// <param name="hv_FineAdjustmentMaxShift"></param>
        /// <param name="hv_FineAdjustmentMatchingWidth"></param>
        /// <param name="hv_BlendingSeam"></param>
        /// <param name="hv_CameraSetupModelZeroDistInCylinderOrigin"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_MosaicWidth"></param>
        /// <param name="hv_MosaicHeight"></param>
        /// <param name="hv_CylinderPointsX"></param>
        /// <param name="hv_CylinderPointsY"></param>
        /// <param name="hv_CylinderPointsZ"></param>
        /// <param name="hv_NumSlices"></param>
        /// <param name="hv_NumPointsPerSlice"></param>
        /// <param name="hv_LabelMinRow"></param>
        /// <param name="hv_LabelMaxRow"></param>
        /// <param name="hv_ExpansionCameraIndex"></param>
        private void CylinderExpansionImages(HObject ho_Regions, HObject ho_ImagesRectified,
             out HObject ho_ExpansionImage, HTuple hv_ProcessImageIndex, HTuple hv_FineAdjustmentMaxShift,
             HTuple hv_FineAdjustmentMatchingWidth, HTuple hv_BlendingSeam, HTuple hv_CameraSetupModelZeroDistInCylinderOrigin,
             HTuple hv_NumCameras, HTuple hv_MosaicWidth, HTuple hv_MosaicHeight, HTuple hv_CylinderPointsX,
             HTuple hv_CylinderPointsY, HTuple hv_CylinderPointsZ, HTuple hv_NumSlices, HTuple hv_NumPointsPerSlice,
             HTuple hv_LabelMinRow, HTuple hv_LabelMaxRow, HTuple hv_ExpansionCameraIndex)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImagesRectifiedOne, ho_ImagesRectified_R = null;
            HObject ho_ImagesRectified_G = null, ho_ImagesRectified_B = null;
            HObject ho_RegionsDilation, ho_UnrolledImages, ho_UnrolledImages_G = null;
            HObject ho_UnrolledImages_B = null, ho_UnrolledImages_R = null;
            HObject ho_ImageUnrolled = null, ho_ImageUnrolled_R = null;
            HObject ho_ImageUnrolled_G = null, ho_ImageUnrolled_B = null;
            HObject ho_Region = null, ho_ImageGray = null, ho_ImageGray_G = null;
            HObject ho_ImageGray_B = null, ho_ImageGray_R = null, ho_UnrolledImagesRegion;
            HObject ho_RegionClosing, ho_ConnectedRegions, ho_TiledImage = null;
            HObject ho_ImagePart_left = null, ho_ImagePart_right = null;
            HObject ho_ImagePartObjectsConcat = null, ho_ImagePartObjectsConcat_G = null;
            HObject ho_TiledImage_G = null, ho_ImagePartObjectsConcat_B = null;
            HObject ho_TiledImage_B = null, ho_ImagePartObjectsConcat_R = null;
            HObject ho_TiledImage_R = null, ho_Region1;

            // Local control variables 

            HTuple hv_Channels = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_SamplePX = new HTuple(), hv_SamplePY = new HTuple();
            HTuple hv_SamplePZ = new HTuple(), hv_CamParam0 = new HTuple();
            HTuple hv_CamPose0 = new HTuple(), hv_HomMat3D0 = new HTuple();
            HTuple hv_HomMat3D0Invert = new HTuple(), hv_Qx = new HTuple();
            HTuple hv_Qy = new HTuple(), hv_Qz = new HTuple(), hv_ImageSampleRow = new HTuple();
            HTuple hv_ImageSampleColumn = new HTuple(), hv_MinSlice = new HTuple();
            HTuple hv_MaxSlice = new HTuple(), hv_DilWidth = new HTuple();
            HTuple hv_Cam = new HTuple(), hv_CamParam = new HTuple();
            HTuple hv_ImageWidth = new HTuple(), hv_ImageHeight = new HTuple();
            HTuple hv_CamPose = new HTuple(), hv_HomMat3D = new HTuple();
            HTuple hv_HomMat3DInvert = new HTuple(), hv_Rows = new HTuple();
            HTuple hv_Columns = new HTuple(), hv_LinCoord = new HTuple();
            HTuple hv_ImageRow = new HTuple(), hv_ImageColumn = new HTuple();
            HTuple hv_Mask = new HTuple(), hv_ImageRowSub = new HTuple();
            HTuple hv_ImageColumnSub = new HTuple(), hv_Grayval = new HTuple();
            HTuple hv_Grayval_G = new HTuple(), hv_Grayval_B = new HTuple();
            HTuple hv_Grayval_R = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Height = new HTuple(), hv_Number = new HTuple();
            HTuple hv_WidthLeft = new HTuple(), hv_HeightLeft = new HTuple();
            HTuple hv_WidthRight = new HTuple(), hv_HeightRight = new HTuple();
            HTuple hv_OffsetRow = new HTuple(), hv_OffsetColumn = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_CylinderPointsX_COPY_INP_TMP = new HTuple(hv_CylinderPointsX);
            HTuple hv_CylinderPointsY_COPY_INP_TMP = new HTuple(hv_CylinderPointsY);
            HTuple hv_CylinderPointsZ_COPY_INP_TMP = new HTuple(hv_CylinderPointsZ);
            HTuple hv_MosaicHeight_COPY_INP_TMP = new HTuple(hv_MosaicHeight);

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ExpansionImage);
            HOperatorSet.GenEmptyObj(out ho_ImagesRectifiedOne);
            HOperatorSet.GenEmptyObj(out ho_ImagesRectified_R);
            HOperatorSet.GenEmptyObj(out ho_ImagesRectified_G);
            HOperatorSet.GenEmptyObj(out ho_ImagesRectified_B);
            HOperatorSet.GenEmptyObj(out ho_RegionsDilation);
            HOperatorSet.GenEmptyObj(out ho_UnrolledImages);
            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_G);
            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_B);
            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_R);
            HOperatorSet.GenEmptyObj(out ho_ImageUnrolled);
            HOperatorSet.GenEmptyObj(out ho_ImageUnrolled_R);
            HOperatorSet.GenEmptyObj(out ho_ImageUnrolled_G);
            HOperatorSet.GenEmptyObj(out ho_ImageUnrolled_B);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ImageGray);
            HOperatorSet.GenEmptyObj(out ho_ImageGray_G);
            HOperatorSet.GenEmptyObj(out ho_ImageGray_B);
            HOperatorSet.GenEmptyObj(out ho_ImageGray_R);
            HOperatorSet.GenEmptyObj(out ho_UnrolledImagesRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_TiledImage);
            HOperatorSet.GenEmptyObj(out ho_ImagePart_left);
            HOperatorSet.GenEmptyObj(out ho_ImagePart_right);
            HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat);
            HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_G);
            HOperatorSet.GenEmptyObj(out ho_TiledImage_G);
            HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_B);
            HOperatorSet.GenEmptyObj(out ho_TiledImage_B);
            HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_R);
            HOperatorSet.GenEmptyObj(out ho_TiledImage_R);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            try
            {
                ho_ImagesRectifiedOne.Dispose();
                HOperatorSet.SelectObj(ho_ImagesRectified, out ho_ImagesRectifiedOne, 1);
                hv_Channels.Dispose();
                HOperatorSet.CountChannels(ho_ImagesRectifiedOne, out hv_Channels);
                if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                {
                    ho_ImagesRectified_R.Dispose(); ho_ImagesRectified_G.Dispose(); ho_ImagesRectified_B.Dispose();
                    HOperatorSet.Decompose3(ho_ImagesRectified, out ho_ImagesRectified_R, out ho_ImagesRectified_G,
                        out ho_ImagesRectified_B);
                }

                //Determine the part of the cylinder that must be used according to the
                //given area of the label in the first image
                hv_Indices.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Indices = new HTuple();
                    hv_Indices[0] = 0;
                    hv_Indices = hv_Indices.TupleConcat((HTuple.TupleGenConst(
                        hv_NumSlices - 1, hv_NumPointsPerSlice)).TupleCumul());
                }
                hv_SamplePX.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_SamplePX = hv_CylinderPointsX_COPY_INP_TMP.TupleSelect(
                        hv_Indices);
                }
                hv_SamplePY.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_SamplePY = hv_CylinderPointsY_COPY_INP_TMP.TupleSelect(
                        hv_Indices);
                }
                hv_SamplePZ.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_SamplePZ = hv_CylinderPointsZ_COPY_INP_TMP.TupleSelect(
                        hv_Indices);
                }
                hv_CamParam0.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                    0, "params", out hv_CamParam0);
                hv_CamPose0.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                    0, "pose", out hv_CamPose0);
                hv_HomMat3D0.Dispose();
                HOperatorSet.PoseToHomMat3d(hv_CamPose0, out hv_HomMat3D0);
                hv_HomMat3D0Invert.Dispose();
                HOperatorSet.HomMat3dInvert(hv_HomMat3D0, out hv_HomMat3D0Invert);
                hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                HOperatorSet.AffineTransPoint3d(hv_HomMat3D0Invert, hv_SamplePX, hv_SamplePY,
                    hv_SamplePZ, out hv_Qx, out hv_Qy, out hv_Qz);
                hv_ImageSampleRow.Dispose(); hv_ImageSampleColumn.Dispose();
                HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam0, out hv_ImageSampleRow,
                    out hv_ImageSampleColumn);
                if ((int)(new HTuple(((hv_ImageSampleRow.TupleFirstN(1))).TupleLess(hv_ImageSampleRow.TupleLastN(
                    1)))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ImageSampleRow = hv_ImageSampleRow.TupleInverse()
                                ;
                            hv_ImageSampleRow.Dispose();
                            hv_ImageSampleRow = ExpTmpLocalVar_ImageSampleRow;
                        }
                    }
                }
                hv_MinSlice.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MinSlice = ((((((((((hv_ImageSampleRow - hv_LabelMaxRow)).TupleSgn()
                        )).TupleFindLast(1))).TupleSelect(0))).TupleConcat(0))).TupleMax();
                }
                hv_MaxSlice.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MaxSlice = ((((((((((hv_ImageSampleRow - hv_LabelMinRow)).TupleSgn()
                        )).TupleFindLast(1))).TupleSelect(0))).TupleConcat(hv_NumSlices - 1))).TupleMin()
                        ;
                }

                hv_MosaicHeight_COPY_INP_TMP.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_MosaicHeight_COPY_INP_TMP = (hv_MaxSlice - hv_MinSlice) + 1;
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_CylinderPointsX = hv_CylinderPointsX_COPY_INP_TMP.TupleSelectRange(
                            hv_MinSlice * hv_NumPointsPerSlice, ((hv_MaxSlice * hv_NumPointsPerSlice) + hv_NumPointsPerSlice) - 1);
                        hv_CylinderPointsX_COPY_INP_TMP.Dispose();
                        hv_CylinderPointsX_COPY_INP_TMP = ExpTmpLocalVar_CylinderPointsX;
                    }
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_CylinderPointsY = hv_CylinderPointsY_COPY_INP_TMP.TupleSelectRange(
                            hv_MinSlice * hv_NumPointsPerSlice, ((hv_MaxSlice * hv_NumPointsPerSlice) + hv_NumPointsPerSlice) - 1);
                        hv_CylinderPointsY_COPY_INP_TMP.Dispose();
                        hv_CylinderPointsY_COPY_INP_TMP = ExpTmpLocalVar_CylinderPointsY;
                    }
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_CylinderPointsZ = hv_CylinderPointsZ_COPY_INP_TMP.TupleSelectRange(
                            hv_MinSlice * hv_NumPointsPerSlice, ((hv_MaxSlice * hv_NumPointsPerSlice) + hv_NumPointsPerSlice) - 1);
                        hv_CylinderPointsZ_COPY_INP_TMP.Dispose();
                        hv_CylinderPointsZ_COPY_INP_TMP = ExpTmpLocalVar_CylinderPointsZ;
                    }
                }

                //Extend the regions such that they overlap each other to allow
                //some matching for the fine adjustment
                hv_DilWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DilWidth = (((((((hv_FineAdjustmentMatchingWidth + (2 * hv_FineAdjustmentMaxShift))).TupleConcat(
                        hv_BlendingSeam + hv_FineAdjustmentMaxShift))).TupleMax()) / 2) * 2) + 1;
                }
                ho_RegionsDilation.Dispose();
                HOperatorSet.DilationRectangle1(ho_Regions, out ho_RegionsDilation, hv_DilWidth,
                    1);

                //yqs
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClipRegion(ho_RegionsDilation, out ExpTmpOutVar_0, 0, 0, hv_MosaicHeight_COPY_INP_TMP - 1,
                        hv_MosaicWidth - 1);
                    ho_RegionsDilation.Dispose();
                    ho_RegionsDilation = ExpTmpOutVar_0;
                }

                //Unrole the images from the individual cameras such that they overlap each other

                ho_UnrolledImages.Dispose();
                HOperatorSet.GenEmptyObj(out ho_UnrolledImages);
                if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                {
                    switch (hv_ProcessImageIndex.I)
                    {
                        case 1:
                            ho_UnrolledImages_G.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_G);
                            ho_UnrolledImages_B.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_B);
                            break;
                        case 2:
                            ho_UnrolledImages_R.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_R);
                            ho_UnrolledImages_B.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_B);
                            break;
                        case 3:
                            ho_UnrolledImages_R.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_R);
                            ho_UnrolledImages_G.Dispose();
                            HOperatorSet.GenEmptyObj(out ho_UnrolledImages_G);
                            break;
                    }
                }

                hv_Cam.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Cam = hv_ExpansionCameraIndex - 1;
                }
                //for Cam := 0 to NumCameras - 1 by 1

                ho_ImageUnrolled.Dispose();
                HOperatorSet.GenImageConst(out ho_ImageUnrolled, "byte", hv_MosaicWidth,
                    hv_MosaicHeight_COPY_INP_TMP);
                if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                {
                    ho_ImageUnrolled_R.Dispose();
                    HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_R);
                    ho_ImageUnrolled_G.Dispose();
                    HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_G);
                    ho_ImageUnrolled_B.Dispose();
                    HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_B);

                    switch (hv_ProcessImageIndex.I)
                    {
                        case 1:
                            ho_ImageUnrolled_G.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_G);
                            ho_ImageUnrolled_B.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_B);
                            break;
                        case 2:
                            ho_ImageUnrolled_R.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_R);
                            ho_ImageUnrolled_B.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_B);
                            break;
                        case 3:
                            ho_ImageUnrolled_R.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_R);
                            ho_ImageUnrolled_G.Dispose();
                            HOperatorSet.CopyImage(ho_ImageUnrolled, out ho_ImageUnrolled_G);
                            break;
                    }
                }

                hv_CamParam.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                    hv_Cam, "params", out hv_CamParam);
                hv_ImageWidth.Dispose();
                get_cam_par_data(hv_CamParam, "image_width", out hv_ImageWidth);
                hv_ImageHeight.Dispose();
                get_cam_par_data(hv_CamParam, "image_height", out hv_ImageHeight);
                hv_CamPose.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                    hv_Cam, "pose", out hv_CamPose);
                hv_HomMat3D.Dispose();
                HOperatorSet.PoseToHomMat3d(hv_CamPose, out hv_HomMat3D);
                hv_HomMat3DInvert.Dispose();
                HOperatorSet.HomMat3dInvert(hv_HomMat3D, out hv_HomMat3DInvert);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Region.Dispose();
                    HOperatorSet.SelectObj(ho_RegionsDilation, out ho_Region, hv_Cam + 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Region, out hv_Rows, out hv_Columns);
                hv_LinCoord.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_LinCoord = (hv_Rows * hv_NumPointsPerSlice) + hv_Columns;
                }

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Qx.Dispose(); hv_Qy.Dispose(); hv_Qz.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3DInvert, hv_CylinderPointsX_COPY_INP_TMP.TupleSelect(
                        hv_LinCoord), hv_CylinderPointsY_COPY_INP_TMP.TupleSelect(hv_LinCoord),
                        hv_CylinderPointsZ_COPY_INP_TMP.TupleSelect(hv_LinCoord), out hv_Qx,
                        out hv_Qy, out hv_Qz);
                }
                hv_ImageRow.Dispose(); hv_ImageColumn.Dispose();
                HOperatorSet.Project3dPoint(hv_Qx, hv_Qy, hv_Qz, hv_CamParam, out hv_ImageRow,
                    out hv_ImageColumn);

                hv_Mask.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Mask = (new HTuple((new HTuple(((hv_ImageRow.TupleGreaterEqualElem(
                        0))).TupleAnd(hv_ImageRow.TupleLessEqualElem(hv_ImageHeight - 1)))).TupleAnd(
                        hv_ImageColumn.TupleGreaterEqualElem(0)))).TupleAnd(hv_ImageColumn.TupleLessEqualElem(
                        hv_ImageWidth - 1));
                }
                hv_ImageRowSub.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ImageRowSub = hv_ImageRow.TupleSelectMask(
                        hv_Mask);
                }
                hv_ImageColumnSub.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ImageColumnSub = hv_ImageColumn.TupleSelectMask(
                        hv_Mask);
                }

                if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                {
                    switch (hv_ProcessImageIndex.I)
                    {
                        case 1:
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_R, out ho_ImageGray, hv_Cam + 1);
                            }
                            hv_Grayval.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages, ho_ImageUnrolled, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages.Dispose();
                                ho_UnrolledImages = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_G.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_G, out ho_ImageGray_G, hv_Cam + 1);
                            }
                            hv_Grayval_G.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_G, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_G);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_G, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_G);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_G, ho_ImageUnrolled_G, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_G.Dispose();
                                ho_UnrolledImages_G = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_B.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_B, out ho_ImageGray_B, hv_Cam + 1);
                            }
                            hv_Grayval_B.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_B, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_B);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_B, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_B);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_B, ho_ImageUnrolled_B, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_B.Dispose();
                                ho_UnrolledImages_B = ExpTmpOutVar_0;
                            }

                            break;
                        case 2:
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_R.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_R, out ho_ImageGray_R, hv_Cam + 1);
                            }
                            hv_Grayval_R.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_R, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_R);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_R, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_R);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_R, ho_ImageUnrolled_R, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_R.Dispose();
                                ho_UnrolledImages_R = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_G, out ho_ImageGray, hv_Cam + 1);
                            }
                            hv_Grayval.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages, ho_ImageUnrolled, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages.Dispose();
                                ho_UnrolledImages = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_B.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_B, out ho_ImageGray_B, hv_Cam + 1);
                            }
                            hv_Grayval_B.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_B, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_B);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_B, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_B);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_B, ho_ImageUnrolled_B, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_B.Dispose();
                                ho_UnrolledImages_B = ExpTmpOutVar_0;
                            }
                            break;
                        case 3:
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_R.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_R, out ho_ImageGray_R, hv_Cam + 1);
                            }
                            hv_Grayval_R.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_R, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_R);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_R, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_R);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_R, ho_ImageUnrolled_R, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_R.Dispose();
                                ho_UnrolledImages_R = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray_G.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_G, out ho_ImageGray_G, hv_Cam + 1);
                            }
                            hv_Grayval_G.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray_G, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval_G);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled_G, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval_G);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages_G, ho_ImageUnrolled_G, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages_G.Dispose();
                                ho_UnrolledImages_G = ExpTmpOutVar_0;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ImageGray.Dispose();
                                HOperatorSet.SelectObj(ho_ImagesRectified_B, out ho_ImageGray, hv_Cam + 1);
                            }
                            hv_Grayval.Dispose();
                            HOperatorSet.GetGrayvalInterpolated(ho_ImageGray, hv_ImageRowSub, hv_ImageColumnSub,
                                "bilinear", out hv_Grayval);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetGrayval(ho_ImageUnrolled, hv_Rows.TupleSelectMask(hv_Mask),
                                    hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval);
                            }
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_UnrolledImages, ho_ImageUnrolled, out ExpTmpOutVar_0
                                    );
                                ho_UnrolledImages.Dispose();
                                ho_UnrolledImages = ExpTmpOutVar_0;
                            }
                            break;
                    }

                }
                else
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_ImageGray.Dispose();
                        HOperatorSet.SelectObj(ho_ImagesRectified, out ho_ImageGray, hv_Cam + 1);
                    }
                    hv_Grayval.Dispose();
                    HOperatorSet.GetGrayvalInterpolated(ho_ImageGray, hv_ImageRowSub, hv_ImageColumnSub,
                        "bilinear", out hv_Grayval);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetGrayval(ho_ImageUnrolled, hv_Rows.TupleSelectMask(hv_Mask),
                            hv_Columns.TupleSelectMask(hv_Mask), hv_Grayval);
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_UnrolledImages, ho_ImageUnrolled, out ExpTmpOutVar_0
                            );
                        ho_UnrolledImages.Dispose();
                        ho_UnrolledImages = ExpTmpOutVar_0;
                    }
                }


                //endfor

                ho_UnrolledImagesRegion.Dispose();
                HOperatorSet.Threshold(ho_UnrolledImages, out ho_UnrolledImagesRegion, 1, 255);
                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_UnrolledImages, out hv_Width, out hv_Height);
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingRectangle1(ho_UnrolledImagesRegion, out ho_RegionClosing,
                    1, hv_Height);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
                hv_Number.Dispose();
                HOperatorSet.CountObj(ho_ConnectedRegions, out hv_Number);

                if ((int)(new HTuple(hv_Number.TupleEqual(1))) != 0)
                {
                    if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                    {
                        switch (hv_ProcessImageIndex.I)
                        {
                            case 1:
                                ho_TiledImage.Dispose();
                                HOperatorSet.Compose3(ho_UnrolledImages, ho_UnrolledImages_G, ho_UnrolledImages_B,
                                    out ho_TiledImage);
                                break;
                            case 2:
                                ho_TiledImage.Dispose();
                                HOperatorSet.Compose3(ho_UnrolledImages_R, ho_UnrolledImages, ho_UnrolledImages_B,
                                    out ho_TiledImage);
                                break;

                            case 3:
                                ho_TiledImage.Dispose();
                                HOperatorSet.Compose3(ho_UnrolledImages_R, ho_UnrolledImages_G, ho_UnrolledImages,
                                    out ho_TiledImage);
                                break;
                        }
                    }

                }
                else
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_ImagePart_left.Dispose();
                        HOperatorSet.CropRectangle1(ho_UnrolledImages, out ho_ImagePart_left, 0,
                            0, hv_Height - 1, (hv_Width / 2) - 1);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_ImagePart_right.Dispose();
                        HOperatorSet.CropRectangle1(ho_UnrolledImages, out ho_ImagePart_right, 0,
                            hv_Width / 2, hv_Height - 1, hv_Width - 1);
                    }

                    hv_WidthLeft.Dispose(); hv_HeightLeft.Dispose();
                    HOperatorSet.GetImageSize(ho_ImagePart_left, out hv_WidthLeft, out hv_HeightLeft);
                    hv_WidthRight.Dispose(); hv_HeightRight.Dispose();
                    HOperatorSet.GetImageSize(ho_ImagePart_right, out hv_WidthRight, out hv_HeightRight);

                    ho_ImagePartObjectsConcat.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat, ho_ImagePart_right, out ExpTmpOutVar_0
                            );
                        ho_ImagePartObjectsConcat.Dispose();
                        ho_ImagePartObjectsConcat = ExpTmpOutVar_0;
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat, ho_ImagePart_left, out ExpTmpOutVar_0
                            );
                        ho_ImagePartObjectsConcat.Dispose();
                        ho_ImagePartObjectsConcat = ExpTmpOutVar_0;
                    }

                    hv_OffsetRow.Dispose();
                    hv_OffsetRow = new HTuple();
                    hv_OffsetRow[0] = 0;
                    hv_OffsetRow[1] = 0;
                    hv_OffsetColumn.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_OffsetColumn = new HTuple();
                        hv_OffsetColumn[0] = 0;
                        hv_OffsetColumn = hv_OffsetColumn.TupleConcat(hv_WidthRight);
                    }

                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_TiledImage.Dispose();
                        HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat, out ho_TiledImage,
                            hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(
                            -1), (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                            hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                    }

                    if ((int)(new HTuple(hv_Channels.TupleEqual(3))) != 0)
                    {

                        switch (hv_ProcessImageIndex.I)
                        {
                            case 1:
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_G, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_G, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_G.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_G);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_G, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_G.Dispose();
                                    ho_ImagePartObjectsConcat_G = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_G, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_G.Dispose();
                                    ho_ImagePartObjectsConcat_G = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_G.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_G, out ho_TiledImage_G,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_B, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_B, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_B.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_B);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_B, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_B.Dispose();
                                    ho_ImagePartObjectsConcat_B = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_B, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_B.Dispose();
                                    ho_ImagePartObjectsConcat_B = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_B.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_B, out ho_TiledImage_B,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Compose3(ho_TiledImage, ho_TiledImage_G, ho_TiledImage_B,
                                        out ExpTmpOutVar_0);
                                    ho_TiledImage.Dispose();
                                    ho_TiledImage = ExpTmpOutVar_0;
                                }
                                break;

                            case 2:
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_R, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_R, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_R.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_R);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_R, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_R.Dispose();
                                    ho_ImagePartObjectsConcat_R = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_R, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_R.Dispose();
                                    ho_ImagePartObjectsConcat_R = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_R.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_R, out ho_TiledImage_R,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_B, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_B, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_B.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_B);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_B, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_B.Dispose();
                                    ho_ImagePartObjectsConcat_B = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_B, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_B.Dispose();
                                    ho_ImagePartObjectsConcat_B = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_B.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_B, out ho_TiledImage_B,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Compose3(ho_TiledImage_R, ho_TiledImage, ho_TiledImage_B,
                                        out ExpTmpOutVar_0);
                                    ho_TiledImage.Dispose();
                                    ho_TiledImage = ExpTmpOutVar_0;
                                }
                                break;

                            case 3:
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_R, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_R, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_R.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_R);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_R, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_R.Dispose();
                                    ho_ImagePartObjectsConcat_R = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_R, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_R.Dispose();
                                    ho_ImagePartObjectsConcat_R = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_R.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_R, out ho_TiledImage_R,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_left.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_G, out ho_ImagePart_left,
                                        0, 0, hv_Height - 1, (hv_Width / 2) - 1);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ImagePart_right.Dispose();
                                    HOperatorSet.CropRectangle1(ho_UnrolledImages_G, out ho_ImagePart_right,
                                        0, hv_Width / 2, hv_Height - 1, hv_Width - 1);
                                }

                                ho_ImagePartObjectsConcat_G.Dispose();
                                HOperatorSet.GenEmptyObj(out ho_ImagePartObjectsConcat_G);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_G, ho_ImagePart_right,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_G.Dispose();
                                    ho_ImagePartObjectsConcat_G = ExpTmpOutVar_0;
                                }
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_ImagePartObjectsConcat_G, ho_ImagePart_left,
                                        out ExpTmpOutVar_0);
                                    ho_ImagePartObjectsConcat_G.Dispose();
                                    ho_ImagePartObjectsConcat_G = ExpTmpOutVar_0;
                                }

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_TiledImage_G.Dispose();
                                    HOperatorSet.TileImagesOffset(ho_ImagePartObjectsConcat_G, out ho_TiledImage_G,
                                        hv_OffsetRow, hv_OffsetColumn, (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), (new HTuple(-1)).TupleConcat(-1),
                                        (new HTuple(-1)).TupleConcat(-1), hv_WidthLeft + hv_WidthRight, hv_HeightRight);
                                }

                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Compose3(ho_TiledImage_R, ho_TiledImage_G, ho_TiledImage,
                                        out ExpTmpOutVar_0);
                                    ho_TiledImage.Dispose();
                                    ho_TiledImage = ExpTmpOutVar_0;
                                }
                                break;

                        }

                    }

                }


                ho_Region1.Dispose();
                HOperatorSet.Threshold(ho_TiledImage, out ho_Region1, 1, 255);
                hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
                HOperatorSet.SmallestRectangle1(ho_Region1, out hv_Row1, out hv_Column1, out hv_Row2,
                    out hv_Column2);
                ho_ExpansionImage.Dispose();
                HOperatorSet.CropRectangle1(ho_TiledImage, out ho_ExpansionImage, hv_Row1,
                    hv_Column1, hv_Row2, hv_Column2);

                ho_ImagesRectifiedOne.Dispose();
                ho_ImagesRectified_R.Dispose();
                ho_ImagesRectified_G.Dispose();
                ho_ImagesRectified_B.Dispose();
                ho_RegionsDilation.Dispose();
                ho_UnrolledImages.Dispose();
                ho_UnrolledImages_G.Dispose();
                ho_UnrolledImages_B.Dispose();
                ho_UnrolledImages_R.Dispose();
                ho_ImageUnrolled.Dispose();
                ho_ImageUnrolled_R.Dispose();
                ho_ImageUnrolled_G.Dispose();
                ho_ImageUnrolled_B.Dispose();
                ho_Region.Dispose();
                ho_ImageGray.Dispose();
                ho_ImageGray_G.Dispose();
                ho_ImageGray_B.Dispose();
                ho_ImageGray_R.Dispose();
                ho_UnrolledImagesRegion.Dispose();
                ho_RegionClosing.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_TiledImage.Dispose();
                ho_ImagePart_left.Dispose();
                ho_ImagePart_right.Dispose();
                ho_ImagePartObjectsConcat.Dispose();
                ho_ImagePartObjectsConcat_G.Dispose();
                ho_TiledImage_G.Dispose();
                ho_ImagePartObjectsConcat_B.Dispose();
                ho_TiledImage_B.Dispose();
                ho_ImagePartObjectsConcat_R.Dispose();
                ho_TiledImage_R.Dispose();
                ho_Region1.Dispose();

                hv_CylinderPointsX_COPY_INP_TMP.Dispose();
                hv_CylinderPointsY_COPY_INP_TMP.Dispose();
                hv_CylinderPointsZ_COPY_INP_TMP.Dispose();
                hv_MosaicHeight_COPY_INP_TMP.Dispose();
                hv_Channels.Dispose();
                hv_Indices.Dispose();
                hv_SamplePX.Dispose();
                hv_SamplePY.Dispose();
                hv_SamplePZ.Dispose();
                hv_CamParam0.Dispose();
                hv_CamPose0.Dispose();
                hv_HomMat3D0.Dispose();
                hv_HomMat3D0Invert.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_ImageSampleRow.Dispose();
                hv_ImageSampleColumn.Dispose();
                hv_MinSlice.Dispose();
                hv_MaxSlice.Dispose();
                hv_DilWidth.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_LinCoord.Dispose();
                hv_ImageRow.Dispose();
                hv_ImageColumn.Dispose();
                hv_Mask.Dispose();
                hv_ImageRowSub.Dispose();
                hv_ImageColumnSub.Dispose();
                hv_Grayval.Dispose();
                hv_Grayval_G.Dispose();
                hv_Grayval_B.Dispose();
                hv_Grayval_R.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Number.Dispose();
                hv_WidthLeft.Dispose();
                hv_HeightLeft.Dispose();
                hv_WidthRight.Dispose();
                hv_HeightRight.Dispose();
                hv_OffsetRow.Dispose();
                hv_OffsetColumn.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImagesRectifiedOne.Dispose();
                ho_ImagesRectified_R.Dispose();
                ho_ImagesRectified_G.Dispose();
                ho_ImagesRectified_B.Dispose();
                ho_RegionsDilation.Dispose();
                ho_UnrolledImages.Dispose();
                ho_UnrolledImages_G.Dispose();
                ho_UnrolledImages_B.Dispose();
                ho_UnrolledImages_R.Dispose();
                ho_ImageUnrolled.Dispose();
                ho_ImageUnrolled_R.Dispose();
                ho_ImageUnrolled_G.Dispose();
                ho_ImageUnrolled_B.Dispose();
                ho_Region.Dispose();
                ho_ImageGray.Dispose();
                ho_ImageGray_G.Dispose();
                ho_ImageGray_B.Dispose();
                ho_ImageGray_R.Dispose();
                ho_UnrolledImagesRegion.Dispose();
                ho_RegionClosing.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_TiledImage.Dispose();
                ho_ImagePart_left.Dispose();
                ho_ImagePart_right.Dispose();
                ho_ImagePartObjectsConcat.Dispose();
                ho_ImagePartObjectsConcat_G.Dispose();
                ho_TiledImage_G.Dispose();
                ho_ImagePartObjectsConcat_B.Dispose();
                ho_TiledImage_B.Dispose();
                ho_ImagePartObjectsConcat_R.Dispose();
                ho_TiledImage_R.Dispose();
                ho_Region1.Dispose();

                hv_CylinderPointsX_COPY_INP_TMP.Dispose();
                hv_CylinderPointsY_COPY_INP_TMP.Dispose();
                hv_CylinderPointsZ_COPY_INP_TMP.Dispose();
                hv_MosaicHeight_COPY_INP_TMP.Dispose();
                hv_Channels.Dispose();
                hv_Indices.Dispose();
                hv_SamplePX.Dispose();
                hv_SamplePY.Dispose();
                hv_SamplePZ.Dispose();
                hv_CamParam0.Dispose();
                hv_CamPose0.Dispose();
                hv_HomMat3D0.Dispose();
                hv_HomMat3D0Invert.Dispose();
                hv_Qx.Dispose();
                hv_Qy.Dispose();
                hv_Qz.Dispose();
                hv_ImageSampleRow.Dispose();
                hv_ImageSampleColumn.Dispose();
                hv_MinSlice.Dispose();
                hv_MaxSlice.Dispose();
                hv_DilWidth.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_CamPose.Dispose();
                hv_HomMat3D.Dispose();
                hv_HomMat3DInvert.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_LinCoord.Dispose();
                hv_ImageRow.Dispose();
                hv_ImageColumn.Dispose();
                hv_Mask.Dispose();
                hv_ImageRowSub.Dispose();
                hv_ImageColumnSub.Dispose();
                hv_Grayval.Dispose();
                hv_Grayval_G.Dispose();
                hv_Grayval_B.Dispose();
                hv_Grayval_R.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Number.Dispose();
                hv_WidthLeft.Dispose();
                hv_HeightLeft.Dispose();
                hv_WidthRight.Dispose();
                hv_HeightRight.Dispose();
                hv_OffsetRow.Dispose();
                hv_OffsetColumn.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="ho_Regions"></param>
        /// <param name="hv_CameraSetupModelZeroDistInCylinderOrigin"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_CylinderPointsX"></param>
        /// <param name="hv_CylinderPointsY"></param>
        /// <param name="hv_CylinderPointsZ"></param>
        /// <param name="hv_NumPointsPerSlice"></param>
        /// <param name="hv_MinZI"></param>
        /// <param name="hv_MaxZI"></param>
        /// <param name="hv_MosaicWidth"></param>
        /// <param name="hv_MosaicHeight"></param>
        private void determine_source_cameras_for_mosaic_parts(out HObject ho_Regions,
          HTuple hv_CameraSetupModelZeroDistInCylinderOrigin, HTuple hv_NumCameras, HTuple hv_CylinderPointsX,
          HTuple hv_CylinderPointsY, HTuple hv_CylinderPointsZ, HTuple hv_NumPointsPerSlice,
          HTuple hv_MinZI, HTuple hv_MaxZI, HTuple hv_MosaicWidth, HTuple hv_MosaicHeight)
        {



            // Local iconic variables 

            HObject ho_ImageLabel1, ho_ImageLabel;

            // Local control variables 

            HTuple hv_CenterSlice = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Positive = new HTuple(), hv_Slice0X = new HTuple();
            HTuple hv_Slice0Y = new HTuple(), hv_Slice0Z = new HTuple();
            HTuple hv_CamAngle = new HTuple(), hv_BestFromCamIdx0 = new HTuple();
            HTuple hv_Cam = new HTuple(), hv_CamPose = new HTuple();
            HTuple hv_Vx = new HTuple(), hv_Vy = new HTuple(), hv_Vz = new HTuple();
            HTuple hv_Scale = new HTuple(), hv_Angle = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ImageLabel1);
            HOperatorSet.GenEmptyObj(out ho_ImageLabel);
            try
            {
                //- First, determine for each point of the central slice from which camera it is seen best
                hv_CenterSlice.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CenterSlice = (hv_MaxZI - hv_MinZI) / 2;
                }
                hv_Indices.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Indices = HTuple.TupleGenSequence(
                        hv_CenterSlice * hv_NumPointsPerSlice, ((hv_CenterSlice * hv_NumPointsPerSlice) + hv_NumPointsPerSlice) - 1, 1);
                }
                //Check for negative indices. This can occur
                //if InteractivelyDefineRegion is true.
                hv_Positive.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Positive = hv_Indices.TupleGreaterEqualElem(
                        0);
                }
                if ((int)(new HTuple(((hv_Positive.TupleSum())).TupleNotEqual(new HTuple(hv_Positive.TupleLength()
                    )))) != 0)
                {
                    throw new HalconException("Not enough points on silhouette.\nPlease choose a bigger part of the bottle to be unrolled.");
                }
                hv_Slice0X.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Slice0X = hv_CylinderPointsX.TupleSelect(
                        hv_Indices);
                }
                hv_Slice0Y.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Slice0Y = hv_CylinderPointsY.TupleSelect(
                        hv_Indices);
                }
                hv_Slice0Z.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Slice0Z = hv_CylinderPointsZ.TupleSelect(
                        hv_Indices);
                }
                hv_CamAngle.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CamAngle = HTuple.TupleGenConst(
                        new HTuple(hv_Slice0X.TupleLength()), -1);
                }
                hv_BestFromCamIdx0.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BestFromCamIdx0 = HTuple.TupleGenConst(
                        new HTuple(hv_Slice0X.TupleLength()), -1);
                }
                HTuple end_val14 = hv_NumCameras - 1;
                HTuple step_val14 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val14, step_val14); hv_Cam = hv_Cam.TupleAdd(step_val14))
                {
                    hv_CamPose.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                        hv_Cam, "pose", out hv_CamPose);
                    hv_Vx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Vx = (hv_CamPose.TupleSelect(
                            0)) - hv_Slice0X;
                    }
                    hv_Vy.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Vy = (hv_CamPose.TupleSelect(
                            1)) - hv_Slice0Y;
                    }
                    hv_Vz.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Vz = (hv_CamPose.TupleSelect(
                            2)) - hv_Slice0Z;
                    }
                    hv_Scale.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Scale = 1.0 / (((((hv_Vx * hv_Vx) + (hv_Vy * hv_Vy)) + (hv_Vz * hv_Vz))).TupleSqrt()
                            );
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Vx = hv_Vx * hv_Scale;
                            hv_Vx.Dispose();
                            hv_Vx = ExpTmpLocalVar_Vx;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Vy = hv_Vy * hv_Scale;
                            hv_Vy.Dispose();
                            hv_Vy = ExpTmpLocalVar_Vy;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Vz = hv_Vz * hv_Scale;
                            hv_Vz.Dispose();
                            hv_Vz = ExpTmpLocalVar_Vz;
                        }
                    }

                    hv_Angle.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Angle = ((hv_Slice0X * hv_Vx) + (hv_Slice0Y * hv_Vy)) + (hv_Slice0Z * hv_Vz);
                    }

                    hv_Indices.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Indices = ((hv_Angle.TupleGreaterElem(
                            hv_CamAngle))).TupleFind(1);
                    }
                    if ((int)(new HTuple(hv_Indices.TupleNotEqual(-1))) != 0)
                    {
                        if (hv_BestFromCamIdx0 == null)
                            hv_BestFromCamIdx0 = new HTuple();
                        hv_BestFromCamIdx0[hv_Indices] = HTuple.TupleGenConst(new HTuple(hv_Indices.TupleLength()
                            ), hv_Cam);
                        if (hv_CamAngle == null)
                            hv_CamAngle = new HTuple();
                        hv_CamAngle[hv_Indices] = hv_Angle.TupleSelect(hv_Indices);
                    }
                }
                //- Then, determine the regions in the mosaic image that will be determined from
                //a particular camera image
                ho_ImageLabel1.Dispose();
                HOperatorSet.GenImageConst(out ho_ImageLabel1, "byte", hv_MosaicWidth, 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_ImageLabel1, HTuple.TupleGenConst(hv_MosaicWidth,
                        0), HTuple.TupleGenSequence(0, hv_MosaicWidth - 1, 1), hv_BestFromCamIdx0);
                }
                ho_ImageLabel.Dispose();
                HOperatorSet.ZoomImageSize(ho_ImageLabel1, out ho_ImageLabel, hv_MosaicWidth,
                    hv_MosaicHeight, "nearest_neighbor");
                ho_Regions.Dispose();
                HOperatorSet.LabelToRegion(ho_ImageLabel, out ho_Regions);
                ho_ImageLabel1.Dispose();
                ho_ImageLabel.Dispose();

                hv_CenterSlice.Dispose();
                hv_Indices.Dispose();
                hv_Positive.Dispose();
                hv_Slice0X.Dispose();
                hv_Slice0Y.Dispose();
                hv_Slice0Z.Dispose();
                hv_CamAngle.Dispose();
                hv_BestFromCamIdx0.Dispose();
                hv_Cam.Dispose();
                hv_CamPose.Dispose();
                hv_Vx.Dispose();
                hv_Vy.Dispose();
                hv_Vz.Dispose();
                hv_Scale.Dispose();
                hv_Angle.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageLabel1.Dispose();
                ho_ImageLabel.Dispose();

                hv_CenterSlice.Dispose();
                hv_Indices.Dispose();
                hv_Positive.Dispose();
                hv_Slice0X.Dispose();
                hv_Slice0Y.Dispose();
                hv_Slice0Z.Dispose();
                hv_CamAngle.Dispose();
                hv_BestFromCamIdx0.Dispose();
                hv_Cam.Dispose();
                hv_CamPose.Dispose();
                hv_Vx.Dispose();
                hv_Vy.Dispose();
                hv_Vz.Dispose();
                hv_Scale.Dispose();
                hv_Angle.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /// <summary>
        /// 2025.03.15 易群生
        /// </summary>
        /// <param name="ho_ImagesRectified"></param>
        /// <param name="hv_CylinderRadius"></param>
        /// <param name="hv_MeasureHandles"></param>
        /// <param name="hv_Width"></param>
        /// <param name="hv_Height"></param>
        /// <param name="hv_CameraSetupModelZeroDist"></param>
        /// <param name="hv_NumCameras"></param>
        /// <param name="hv_PoseCylinderApprox"></param>
        /// <param name="hv_MinPairDist"></param>
        /// <param name="hv_MaxPairDist"></param>
        /// <param name="hv_SilhouetteMeasureSigma"></param>
        /// <param name="hv_SilhouetteMeasureThreshold"></param>
        /// <param name="hv_SilhouetteMaxTilt"></param>
        /// <param name="hv_MaxError"></param>
        /// <param name="hv_CameraSetupModelZeroDistInCylinderOrigin"></param>
        public void determine_rotation_axis_3d(HObject ho_ImagesRectified, HTuple hv_CylinderRadius,
              HTuple hv_MeasureHandles, HTuple hv_Width, HTuple hv_Height, HTuple hv_CameraSetupModelZeroDist,
              HTuple hv_NumCameras, HTuple hv_PoseCylinderApprox, HTuple hv_MinPairDist, HTuple hv_MaxPairDist,
              HTuple hv_SilhouetteMeasureSigma, HTuple hv_SilhouetteMeasureThreshold, HTuple hv_SilhouetteMaxTilt,
              HTuple hv_MaxError, out HTuple hv_CameraSetupModelZeroDistInCylinderOrigin)
        {




            // Local iconic variables 

            HObject ho_ImageRectified = null;

            // Local control variables 

            HTuple hv_SerializedItemHandle = new HTuple();
            HTuple hv_CameraSetupModelZeroDistCylApprox = new HTuple();
            HTuple hv_A2x = new HTuple(), hv_A2y = new HTuple(), hv_A2z = new HTuple();
            HTuple hv_B2x = new HTuple(), hv_B2y = new HTuple(), hv_B2z = new HTuple();
            HTuple hv_SilhRow = new HTuple(), hv_SilhCol = new HTuple();
            HTuple hv_SilhCam = new HTuple(), hv_SilhRowElim = new HTuple();
            HTuple hv_SilhColElim = new HTuple(), hv_SilhCamElim = new HTuple();
            HTuple hv_Cam = new HTuple(), hv_CamParam = new HTuple();
            HTuple hv_CamPose = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_FuzzyFunction = new HTuple();
            HTuple hv_I = new HTuple(), hv_RowEdgeFirst = new HTuple();
            HTuple hv_ColumnEdgeFirst = new HTuple(), hv_AmplitudeFirst = new HTuple();
            HTuple hv_RowEdgeSecond = new HTuple(), hv_ColumnEdgeSecond = new HTuple();
            HTuple hv_AmplitudeSecond = new HTuple(), hv_RowPairCenter = new HTuple();
            HTuple hv_ColumnPairCenter = new HTuple(), hv_FuzzyScore = new HTuple();
            HTuple hv_IntraDistance = new HTuple(), hv_SIF = new HTuple();
            HTuple hv_SIS = new HTuple(), hv_PX = new HTuple(), hv_PY = new HTuple();
            HTuple hv_PZ = new HTuple(), hv_QX = new HTuple(), hv_QY = new HTuple();
            HTuple hv_QZ = new HTuple(), hv_HomMat3D = new HTuple();
            HTuple hv_PX0 = new HTuple(), hv_PY0 = new HTuple(), hv_PZ0 = new HTuple();
            HTuple hv_QX0 = new HTuple(), hv_QY0 = new HTuple(), hv_QZ0 = new HTuple();
            HTuple hv_CamI = new HTuple(), hv_NumPoints = new HTuple();
            HTuple hv_A1x = new HTuple(), hv_A1y = new HTuple(), hv_A1z = new HTuple();
            HTuple hv_B1x = new HTuple(), hv_B1y = new HTuple(), hv_B1z = new HTuple();
            HTuple hv_Dx = new HTuple(), hv_ErrorLog = new HTuple();
            HTuple hv_Iter = new HTuple(), hv_IterInter = new HTuple();
            HTuple hv_MaxIter = new HTuple(), hv_MaxIterInter = new HTuple();
            HTuple hv_LastError = new HTuple(), hv_SDevFactor = new HTuple();
            HTuple hv_Dist = new HTuple(), hv_E = new HTuple(), hv_Delta = new HTuple();
            HTuple hv_DistTmp = new HTuple(), hv_DistDAx = new HTuple();
            HTuple hv_DistDAy = new HTuple(), hv_DistDBx = new HTuple();
            HTuple hv_DistDBy = new HTuple(), hv_A = new HTuple();
            HTuple hv_SeqR = new HTuple(), hv_SeqC = new HTuple();
            HTuple hv_y = new HTuple(), hv_X = new HTuple(), hv_Values = new HTuple();
            HTuple hv_SDevE = new HTuple(), hv_MaskUse = new HTuple();
            HTuple hv_SilhCamTmp = new HTuple(), hv_CamOk = new HTuple();
            HTuple hv_NumPointsRemaining = new HTuple(), hv_CamPose0 = new HTuple();
            HTuple hv_Foot = new HTuple(), hv_XAxis = new HTuple();
            HTuple hv_ZAxis = new HTuple(), hv_YAxis = new HTuple();
            HTuple hv_HomMat3DCylinderEst = new HTuple(), hv_PoseCylinderEst = new HTuple();
            HTuple hv_PoseCylinder = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageRectified);
            hv_CameraSetupModelZeroDistInCylinderOrigin = new HTuple();
            try
            {
                //Set origin of camera setup model to approximate cylinder pose
                hv_SerializedItemHandle.Dispose();
                HOperatorSet.SerializeCameraSetupModel(hv_CameraSetupModelZeroDist, out hv_SerializedItemHandle);
                hv_CameraSetupModelZeroDistCylApprox.Dispose();
                HOperatorSet.DeserializeCameraSetupModel(hv_SerializedItemHandle, out hv_CameraSetupModelZeroDistCylApprox);
                HOperatorSet.SetCameraSetupParam(hv_CameraSetupModelZeroDistCylApprox, "general",
                    "coord_transf_pose", hv_PoseCylinderApprox);

                //Collect possible silhouette points from all images
                hv_A2x.Dispose();
                hv_A2x = new HTuple();
                hv_A2y.Dispose();
                hv_A2y = new HTuple();
                hv_A2z.Dispose();
                hv_A2z = new HTuple();
                hv_B2x.Dispose();
                hv_B2x = new HTuple();
                hv_B2y.Dispose();
                hv_B2y = new HTuple();
                hv_B2z.Dispose();
                hv_B2z = new HTuple();
                //Collect image coordinates for visualization and analysis
                hv_SilhRow.Dispose();
                hv_SilhRow = new HTuple();
                hv_SilhCol.Dispose();
                hv_SilhCol = new HTuple();
                hv_SilhCam.Dispose();
                hv_SilhCam = new HTuple();

                hv_SilhRowElim.Dispose();
                hv_SilhRowElim = new HTuple();
                hv_SilhColElim.Dispose();
                hv_SilhColElim = new HTuple();
                hv_SilhCamElim.Dispose();
                hv_SilhCamElim = new HTuple();

                HTuple end_val21 = hv_NumCameras - 1;
                HTuple step_val21 = 1;
                for (hv_Cam = 0; hv_Cam.Continue(end_val21, step_val21); hv_Cam = hv_Cam.TupleAdd(step_val21))
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_ImageRectified.Dispose();
                        HOperatorSet.SelectObj(ho_ImagesRectified, out ho_ImageRectified, hv_Cam + 1);
                    }
                    hv_CamParam.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistCylApprox, hv_Cam,
                        "params", out hv_CamParam);
                    hv_CamPose.Dispose();
                    HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistCylApprox, hv_Cam,
                        "pose", out hv_CamPose);
                    hv_Row.Dispose();
                    hv_Row = new HTuple();
                    hv_Column.Dispose();
                    hv_Column = new HTuple();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_FuzzyFunction.Dispose();
                        HOperatorSet.CreateFunct1dPairs((((((((hv_MinPairDist.TupleSelect(hv_Cam)) - 10)).TupleConcat(
                            hv_MinPairDist.TupleSelect(hv_Cam)))).TupleConcat((hv_MaxPairDist.TupleSelect(
                            hv_Cam)) / (hv_SilhouetteMaxTilt.TupleCos())))).TupleConcat(((hv_MaxPairDist.TupleSelect(
                            hv_Cam)) / (hv_SilhouetteMaxTilt.TupleCos())) + 10), (((new HTuple(0)).TupleConcat(
                            1)).TupleConcat(1)).TupleConcat(0), out hv_FuzzyFunction);
                    }
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_MeasureHandles.TupleLength()
                        )) - 1); hv_I = (int)hv_I + 1)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetFuzzyMeasure(hv_MeasureHandles.TupleSelect(hv_I), "size",
                                hv_FuzzyFunction);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_RowEdgeFirst.Dispose(); hv_ColumnEdgeFirst.Dispose(); hv_AmplitudeFirst.Dispose(); hv_RowEdgeSecond.Dispose(); hv_ColumnEdgeSecond.Dispose(); hv_AmplitudeSecond.Dispose(); hv_RowPairCenter.Dispose(); hv_ColumnPairCenter.Dispose(); hv_FuzzyScore.Dispose(); hv_IntraDistance.Dispose();
                            HOperatorSet.FuzzyMeasurePairing(ho_ImageRectified, hv_MeasureHandles.TupleSelect(
                                hv_I), hv_SilhouetteMeasureSigma, hv_SilhouetteMeasureThreshold, 0.5,
                                "all", "no_restriction", 0, out hv_RowEdgeFirst, out hv_ColumnEdgeFirst,
                                out hv_AmplitudeFirst, out hv_RowEdgeSecond, out hv_ColumnEdgeSecond,
                                out hv_AmplitudeSecond, out hv_RowPairCenter, out hv_ColumnPairCenter,
                                out hv_FuzzyScore, out hv_IntraDistance);
                        }
                        //Select only first and last edge if the background can be assumed
                        //to be textureless
                        if ((int)(new HTuple((new HTuple(hv_RowEdgeFirst.TupleLength())).TupleGreater(
                            0))) != 0)
                        {
                            hv_SIF.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_SIF = (new HTuple(((-hv_RowEdgeFirst)).TupleSortIndex()
                                    )).TupleSelect(0);
                            }
                            hv_SIS.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_SIS = (new HTuple(hv_RowEdgeSecond.TupleSortIndex()
                                    )).TupleSelect(0);
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_Row = ((hv_Row.TupleConcat(
                                        hv_RowEdgeFirst.TupleSelect(hv_SIF)))).TupleConcat(hv_RowEdgeSecond.TupleSelect(
                                        hv_SIS));
                                    hv_Row.Dispose();
                                    hv_Row = ExpTmpLocalVar_Row;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_Column = ((hv_Column.TupleConcat(
                                        hv_ColumnEdgeFirst.TupleSelect(hv_SIF)))).TupleConcat(hv_ColumnEdgeSecond.TupleSelect(
                                        hv_SIS));
                                    hv_Column.Dispose();
                                    hv_Column = ExpTmpLocalVar_Column;
                                }
                            }
                        }
                    }

                    //Only for visualization of selected points
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_SilhRow = hv_SilhRow.TupleConcat(
                                hv_Row);
                            hv_SilhRow.Dispose();
                            hv_SilhRow = ExpTmpLocalVar_SilhRow;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_SilhCol = hv_SilhCol.TupleConcat(
                                hv_Column);
                            hv_SilhCol.Dispose();
                            hv_SilhCol = ExpTmpLocalVar_SilhCol;
                        }
                    }

                    //For visualization and check of the distribution of the remaining points
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_SilhCam = hv_SilhCam.TupleConcat(
                                HTuple.TupleGenConst(new HTuple(hv_Row.TupleLength()), hv_Cam));
                            hv_SilhCam.Dispose();
                            hv_SilhCam = ExpTmpLocalVar_SilhCam;
                        }
                    }

                    hv_PX.Dispose(); hv_PY.Dispose(); hv_PZ.Dispose(); hv_QX.Dispose(); hv_QY.Dispose(); hv_QZ.Dispose();
                    HOperatorSet.GetLineOfSight(hv_Row, hv_Column, hv_CamParam, out hv_PX, out hv_PY,
                        out hv_PZ, out hv_QX, out hv_QY, out hv_QZ);
                    hv_HomMat3D.Dispose();
                    HOperatorSet.PoseToHomMat3d(hv_CamPose, out hv_HomMat3D);
                    hv_PX0.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PX0 = hv_HomMat3D.TupleSelect(
                            3);
                    }
                    hv_PY0.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PY0 = hv_HomMat3D.TupleSelect(
                            7);
                    }
                    hv_PZ0.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PZ0 = hv_HomMat3D.TupleSelect(
                            11);
                    }
                    hv_QX0.Dispose(); hv_QY0.Dispose(); hv_QZ0.Dispose();
                    HOperatorSet.AffineTransPoint3d(hv_HomMat3D, hv_QX, hv_QY, hv_QZ, out hv_QX0,
                        out hv_QY0, out hv_QZ0);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_A2x = hv_A2x.TupleConcat(
                                HTuple.TupleGenConst(new HTuple(hv_Row.TupleLength()), hv_PX0));
                            hv_A2x.Dispose();
                            hv_A2x = ExpTmpLocalVar_A2x;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_A2y = hv_A2y.TupleConcat(
                                HTuple.TupleGenConst(new HTuple(hv_Row.TupleLength()), hv_PY0));
                            hv_A2y.Dispose();
                            hv_A2y = ExpTmpLocalVar_A2y;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_A2z = hv_A2z.TupleConcat(
                                HTuple.TupleGenConst(new HTuple(hv_Row.TupleLength()), hv_PZ0));
                            hv_A2z.Dispose();
                            hv_A2z = ExpTmpLocalVar_A2z;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_B2x = hv_B2x.TupleConcat(
                                hv_QX0 - hv_PX0);
                            hv_B2x.Dispose();
                            hv_B2x = ExpTmpLocalVar_B2x;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_B2y = hv_B2y.TupleConcat(
                                hv_QY0 - hv_PY0);
                            hv_B2y.Dispose();
                            hv_B2y = ExpTmpLocalVar_B2y;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_B2z = hv_B2z.TupleConcat(
                                hv_QZ0 - hv_PZ0);
                            hv_B2z.Dispose();
                            hv_B2z = ExpTmpLocalVar_B2z;
                        }
                    }
                }

                //Determine the number of points from each camera
                HTuple end_val63 = hv_NumCameras - 1;
                HTuple step_val63 = 1;
                for (hv_CamI = 0; hv_CamI.Continue(end_val63, step_val63); hv_CamI = hv_CamI.TupleAdd(step_val63))
                {
                    if (hv_NumPoints == null)
                        hv_NumPoints = new HTuple();
                    hv_NumPoints[hv_CamI] = ((((hv_SilhCam.TupleFind(hv_CamI))).TupleNotEqualElem(
                        -1))).TupleSum();
                }

                //Initial values for the cylinder axis (x := A1 + t*B1)
                hv_A1x.Dispose();
                hv_A1x = 0.0;
                hv_A1y.Dispose();
                hv_A1y = 0.0;
                hv_A1z.Dispose();
                hv_A1z = 0;
                hv_B1x.Dispose();
                hv_B1x = 0.0;
                hv_B1y.Dispose();
                hv_B1y = 0.0;
                hv_B1z.Dispose();
                hv_B1z = 1;
                hv_Dx.Dispose();
                hv_Dx = 99999;
                hv_ErrorLog.Dispose();
                hv_ErrorLog = new HTuple();
                hv_Iter.Dispose();
                hv_Iter = 0;
                hv_IterInter.Dispose();
                hv_IterInter = 0;
                hv_MaxIter.Dispose();
                hv_MaxIter = 100;
                hv_MaxIterInter.Dispose();
                hv_MaxIterInter = 5;
                hv_LastError.Dispose();
                hv_LastError = 99999;
                hv_SDevFactor.Dispose();
                hv_SDevFactor = 5.0;
                while ((int)(new HTuple(hv_Iter.TupleLess(hv_MaxIter))) != 0)
                {

                    hv_Dist.Dispose();
                    distance_skew_lines(hv_A1x, hv_A1y, hv_A1z, hv_B1x, hv_B1y, hv_B1z, hv_A2x,
                        hv_A2y, hv_A2z, hv_B2x, hv_B2y, hv_B2z, out hv_Dist);
                    hv_E.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_E = hv_CylinderRadius - hv_Dist;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ErrorLog = hv_ErrorLog.TupleConcat(
                                hv_E.TupleMean());
                            hv_ErrorLog.Dispose();
                            hv_ErrorLog = ExpTmpLocalVar_ErrorLog;
                        }
                    }
                    hv_Delta.Dispose();
                    hv_Delta = 0.001;
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistTmp.Dispose();
                        distance_skew_lines(hv_A1x + hv_Delta, hv_A1y, hv_A1z, hv_B1x, hv_B1y, hv_B1z,
                            hv_A2x, hv_A2y, hv_A2z, hv_B2x, hv_B2y, hv_B2z, out hv_DistTmp);
                    }
                    hv_DistDAx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistDAx = (hv_DistTmp - hv_Dist) / hv_Delta;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistTmp.Dispose();
                        distance_skew_lines(hv_A1x, hv_A1y + hv_Delta, hv_A1z, hv_B1x, hv_B1y, hv_B1z,
                            hv_A2x, hv_A2y, hv_A2z, hv_B2x, hv_B2y, hv_B2z, out hv_DistTmp);
                    }
                    hv_DistDAy.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistDAy = (hv_DistTmp - hv_Dist) / hv_Delta;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistTmp.Dispose();
                        distance_skew_lines(hv_A1x, hv_A1y, hv_A1z, hv_B1x + hv_Delta, hv_B1y, hv_B1z,
                            hv_A2x, hv_A2y, hv_A2z, hv_B2x, hv_B2y, hv_B2z, out hv_DistTmp);
                    }
                    hv_DistDBx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistDBx = (hv_DistTmp - hv_Dist) / hv_Delta;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistTmp.Dispose();
                        distance_skew_lines(hv_A1x, hv_A1y, hv_A1z, hv_B1x, hv_B1y + hv_Delta, hv_B1z,
                            hv_A2x, hv_A2y, hv_A2z, hv_B2x, hv_B2y, hv_B2z, out hv_DistTmp);
                    }
                    hv_DistDBy.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_DistDBy = (hv_DistTmp - hv_Dist) / hv_Delta;
                    }

                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_A.Dispose();
                        HOperatorSet.CreateMatrix(new HTuple(hv_A2x.TupleLength()), 4, 0, out hv_A);
                    }
                    hv_SeqR.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_SeqR = HTuple.TupleGenSequence(
                            0, (new HTuple(hv_A2x.TupleLength())) - 1, 1);
                    }
                    hv_SeqC.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_SeqC = HTuple.TupleGenConst(
                            new HTuple(hv_A2x.TupleLength()), 0);
                    }
                    HOperatorSet.SetValueMatrix(hv_A, hv_SeqR, hv_SeqC, hv_DistDAx);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetValueMatrix(hv_A, hv_SeqR, hv_SeqC + 1, hv_DistDAy);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetValueMatrix(hv_A, hv_SeqR, hv_SeqC + 2, hv_DistDBx);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetValueMatrix(hv_A, hv_SeqR, hv_SeqC + 3, hv_DistDBy);
                    }

                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_y.Dispose();
                        HOperatorSet.CreateMatrix(new HTuple(hv_A2x.TupleLength()), 1, 0, out hv_y);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetValueMatrix(hv_y, hv_SeqR, hv_SeqC, hv_CylinderRadius - hv_Dist);
                    }

                    //Solve the least squares equation system (x := (ATA)^-1*ATy)
                    hv_X.Dispose();
                    HOperatorSet.SolveMatrix(hv_A, "general", 0, hv_y, out hv_X);
                    hv_Values.Dispose();
                    HOperatorSet.GetFullMatrix(hv_X, out hv_Values);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_A1x = hv_A1x + (hv_Values.TupleSelect(
                                0));
                            hv_A1x.Dispose();
                            hv_A1x = ExpTmpLocalVar_A1x;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_A1y = hv_A1y + (hv_Values.TupleSelect(
                                1));
                            hv_A1y.Dispose();
                            hv_A1y = ExpTmpLocalVar_A1y;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_B1x = hv_B1x + (hv_Values.TupleSelect(
                                2));
                            hv_B1x.Dispose();
                            hv_B1x = ExpTmpLocalVar_B1x;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_B1y = hv_B1y + (hv_Values.TupleSelect(
                                3));
                            hv_B1y.Dispose();
                            hv_B1y = ExpTmpLocalVar_B1y;
                        }
                    }

                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Iter = hv_Iter + 1;
                            hv_Iter.Dispose();
                            hv_Iter = ExpTmpLocalVar_Iter;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_IterInter = hv_IterInter + 1;
                            hv_IterInter.Dispose();
                            hv_IterInter = ExpTmpLocalVar_IterInter;
                        }
                    }
                    //Eliminate gross errors
                    if ((int)(new HTuple((new HTuple(hv_ErrorLog.TupleLength())).TupleGreater(
                        3))) != 0)
                    {
                        if ((int)((new HTuple((new HTuple(((hv_ErrorLog.TupleSelectRange((new HTuple(hv_ErrorLog.TupleLength()
                            )) - 3, (new HTuple(hv_ErrorLog.TupleLength())) - 1))).TupleDeviation())).TupleLess(
                            1e-5))).TupleOr(new HTuple(hv_IterInter.TupleGreaterEqual(hv_MaxIterInter)))) != 0)
                        {
                            hv_IterInter.Dispose();
                            hv_IterInter = 0;
                            if ((int)((new HTuple((new HTuple(((((hv_ErrorLog.TupleSelect((new HTuple(hv_ErrorLog.TupleLength()
                                )) - 1))).TupleFabs())).TupleLess(hv_MaxError))).TupleAnd(new HTuple(((((hv_LastError - (hv_ErrorLog.TupleSelect(
                                (new HTuple(hv_ErrorLog.TupleLength())) - 1)))).TupleFabs())).TupleLess(
                                1e-5))))).TupleAnd(new HTuple(hv_SDevFactor.TupleLessEqual(3.0)))) != 0)
                            {
                                //Quit least squares estimation loop
                                hv_MaxIter.Dispose();
                                hv_MaxIter = -1;
                                continue;
                            }
                            hv_LastError.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_LastError = hv_ErrorLog.TupleSelect(
                                    (new HTuple(hv_ErrorLog.TupleLength())) - 1);
                            }

                            //Determine gross errors and eliminate them
                            hv_SDevE.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_SDevE = ((hv_E.TupleFabs()
                                    )).TupleDeviation();
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SDevFactor = (((new HTuple(3.0)).TupleConcat(
                                        hv_SDevFactor - 0.5))).TupleMax();
                                    hv_SDevFactor.Dispose();
                                    hv_SDevFactor = ExpTmpLocalVar_SDevFactor;
                                }
                            }
                            hv_MaskUse.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_MaskUse = ((hv_E.TupleFabs()
                                    )).TupleLessElem(hv_SDevFactor * hv_SDevE);
                            }
                            //Ensure that some errors are eliminated
                            while ((int)((new HTuple(((hv_MaskUse.TupleMin())).TupleEqual(1))).TupleAnd(
                                new HTuple(hv_SDevFactor.TupleGreater(3.0)))) != 0)
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_SDevFactor = (((new HTuple(3.0)).TupleConcat(
                                            hv_SDevFactor - 0.1))).TupleMax();
                                        hv_SDevFactor.Dispose();
                                        hv_SDevFactor = ExpTmpLocalVar_SDevFactor;
                                    }
                                }
                                hv_MaskUse.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_MaskUse = ((hv_E.TupleFabs()
                                        )).TupleLessElem(hv_SDevFactor * hv_SDevE);
                                }
                            }

                            //Ensure that there are at least 20% of the initial points in at least three cameras
                            hv_SilhCamTmp.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_SilhCamTmp = hv_SilhCam.TupleSelectMask(
                                    hv_MaskUse);
                            }
                            hv_CamOk.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_CamOk = HTuple.TupleGenConst(
                                    hv_NumCameras, 0);
                            }
                            HTuple end_val142 = hv_NumCameras - 1;
                            HTuple step_val142 = 1;
                            for (hv_CamI = 0; hv_CamI.Continue(end_val142, step_val142); hv_CamI = hv_CamI.TupleAdd(step_val142))
                            {
                                if (hv_NumPointsRemaining == null)
                                    hv_NumPointsRemaining = new HTuple();
                                hv_NumPointsRemaining[hv_CamI] = ((((hv_SilhCamTmp.TupleFind(hv_CamI))).TupleNotEqualElem(
                                    -1))).TupleSum();
                            }
                            if ((int)(new HTuple(((((((hv_NumPointsRemaining / (hv_NumPoints.TupleReal()
                                ))).TupleGreaterEqualElem(0.3))).TupleSum())).TupleLessEqual(3))) != 0)
                            {
                                //Quit least squares estimation loop because the distribution of the points would be bad
                                hv_MaxIter.Dispose();
                                hv_MaxIter = -1;
                                continue;
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_A2x = hv_A2x.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_A2x.Dispose();
                                    hv_A2x = ExpTmpLocalVar_A2x;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_A2y = hv_A2y.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_A2y.Dispose();
                                    hv_A2y = ExpTmpLocalVar_A2y;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_A2z = hv_A2z.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_A2z.Dispose();
                                    hv_A2z = ExpTmpLocalVar_A2z;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_B2x = hv_B2x.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_B2x.Dispose();
                                    hv_B2x = ExpTmpLocalVar_B2x;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_B2y = hv_B2y.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_B2y.Dispose();
                                    hv_B2y = ExpTmpLocalVar_B2y;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_B2z = hv_B2z.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_B2z.Dispose();
                                    hv_B2z = ExpTmpLocalVar_B2z;
                                }
                            }

                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SilhRowElim = hv_SilhRowElim.TupleConcat(
                                        hv_SilhRow.TupleSelectMask(1 - hv_MaskUse));
                                    hv_SilhRowElim.Dispose();
                                    hv_SilhRowElim = ExpTmpLocalVar_SilhRowElim;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SilhColElim = hv_SilhColElim.TupleConcat(
                                        hv_SilhCol.TupleSelectMask(1 - hv_MaskUse));
                                    hv_SilhColElim.Dispose();
                                    hv_SilhColElim = ExpTmpLocalVar_SilhColElim;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SilhCamElim = hv_SilhCamElim.TupleConcat(
                                        hv_SilhCam.TupleSelectMask(1 - hv_MaskUse));
                                    hv_SilhCamElim.Dispose();
                                    hv_SilhCamElim = ExpTmpLocalVar_SilhCamElim;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SilhRow = hv_SilhRow.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_SilhRow.Dispose();
                                    hv_SilhRow = ExpTmpLocalVar_SilhRow;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_SilhCol = hv_SilhCol.TupleSelectMask(
                                        hv_MaskUse);
                                    hv_SilhCol.Dispose();
                                    hv_SilhCol = ExpTmpLocalVar_SilhCol;
                                }
                            }
                            hv_SilhCam.Dispose();
                            hv_SilhCam = new HTuple(hv_SilhCamTmp);

                        }
                    }
                }
                hv_CamPose0.Dispose();
                HOperatorSet.GetCameraSetupParam(hv_CameraSetupModelZeroDistCylApprox, 0, "pose",
                    out hv_CamPose0);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Foot.Dispose();
                    point_to_line_perpendicular_foot(hv_CamPose0.TupleSelectRange(0, 2), ((hv_A1x.TupleConcat(
                        hv_A1y))).TupleConcat(hv_A1z), ((hv_B1x.TupleConcat(hv_B1y))).TupleConcat(
                        hv_B1z), out hv_Foot);
                }
                hv_XAxis.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_XAxis = hv_Foot - (hv_CamPose0.TupleSelectRange(
                        0, 2));
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_XAxis = hv_XAxis / (((((hv_XAxis * hv_XAxis)).TupleSum()
                            )).TupleSqrt());
                        hv_XAxis.Dispose();
                        hv_XAxis = ExpTmpLocalVar_XAxis;
                    }
                }
                hv_ZAxis.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ZAxis = new HTuple();
                    hv_ZAxis = hv_ZAxis.TupleConcat(hv_B1x, hv_B1y, hv_B1z);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_ZAxis = hv_ZAxis / (((((hv_ZAxis * hv_ZAxis)).TupleSum()
                            )).TupleSqrt());
                        hv_ZAxis.Dispose();
                        hv_ZAxis = ExpTmpLocalVar_ZAxis;
                    }
                }
                hv_YAxis.Dispose();
                cross_product(hv_ZAxis, hv_XAxis, out hv_YAxis);
                hv_HomMat3DCylinderEst.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_HomMat3DCylinderEst = new HTuple();
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_XAxis.TupleSelect(
                        0));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_YAxis.TupleSelect(
                        0));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_ZAxis.TupleSelect(
                        0));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_A1x);
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_XAxis.TupleSelect(
                        1));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_YAxis.TupleSelect(
                        1));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_ZAxis.TupleSelect(
                        1));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_A1y);
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_XAxis.TupleSelect(
                        2));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_YAxis.TupleSelect(
                        2));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_ZAxis.TupleSelect(
                        2));
                    hv_HomMat3DCylinderEst = hv_HomMat3DCylinderEst.TupleConcat(hv_A1z);
                }
                hv_PoseCylinderEst.Dispose();
                HOperatorSet.HomMat3dToPose(hv_HomMat3DCylinderEst, out hv_PoseCylinderEst);
                //Convert cylinder pose into a homogeneous transformation matrix
                hv_PoseCylinder.Dispose();
                HOperatorSet.PoseCompose(hv_PoseCylinderEst, ((((((new HTuple(0)).TupleConcat(
                    0)).TupleConcat(0)).TupleConcat(0)).TupleConcat(0)).TupleConcat(0)).TupleConcat(
                    0), out hv_PoseCylinder);
                //Create a copy of the camera setup model with the origin set to the origin of the cylinder
                hv_SerializedItemHandle.Dispose();
                HOperatorSet.SerializeCameraSetupModel(hv_CameraSetupModelZeroDistCylApprox,
                    out hv_SerializedItemHandle);
                hv_CameraSetupModelZeroDistInCylinderOrigin.Dispose();
                HOperatorSet.DeserializeCameraSetupModel(hv_SerializedItemHandle, out hv_CameraSetupModelZeroDistInCylinderOrigin);
                HOperatorSet.SetCameraSetupParam(hv_CameraSetupModelZeroDistInCylinderOrigin,
                    "general", "coord_transf_pose", hv_PoseCylinder);


                ho_ImageRectified.Dispose();

                hv_SerializedItemHandle.Dispose();
                hv_CameraSetupModelZeroDistCylApprox.Dispose();
                hv_A2x.Dispose();
                hv_A2y.Dispose();
                hv_A2z.Dispose();
                hv_B2x.Dispose();
                hv_B2y.Dispose();
                hv_B2z.Dispose();
                hv_SilhRow.Dispose();
                hv_SilhCol.Dispose();
                hv_SilhCam.Dispose();
                hv_SilhRowElim.Dispose();
                hv_SilhColElim.Dispose();
                hv_SilhCamElim.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_FuzzyFunction.Dispose();
                hv_I.Dispose();
                hv_RowEdgeFirst.Dispose();
                hv_ColumnEdgeFirst.Dispose();
                hv_AmplitudeFirst.Dispose();
                hv_RowEdgeSecond.Dispose();
                hv_ColumnEdgeSecond.Dispose();
                hv_AmplitudeSecond.Dispose();
                hv_RowPairCenter.Dispose();
                hv_ColumnPairCenter.Dispose();
                hv_FuzzyScore.Dispose();
                hv_IntraDistance.Dispose();
                hv_SIF.Dispose();
                hv_SIS.Dispose();
                hv_PX.Dispose();
                hv_PY.Dispose();
                hv_PZ.Dispose();
                hv_QX.Dispose();
                hv_QY.Dispose();
                hv_QZ.Dispose();
                hv_HomMat3D.Dispose();
                hv_PX0.Dispose();
                hv_PY0.Dispose();
                hv_PZ0.Dispose();
                hv_QX0.Dispose();
                hv_QY0.Dispose();
                hv_QZ0.Dispose();
                hv_CamI.Dispose();
                hv_NumPoints.Dispose();
                hv_A1x.Dispose();
                hv_A1y.Dispose();
                hv_A1z.Dispose();
                hv_B1x.Dispose();
                hv_B1y.Dispose();
                hv_B1z.Dispose();
                hv_Dx.Dispose();
                hv_ErrorLog.Dispose();
                hv_Iter.Dispose();
                hv_IterInter.Dispose();
                hv_MaxIter.Dispose();
                hv_MaxIterInter.Dispose();
                hv_LastError.Dispose();
                hv_SDevFactor.Dispose();
                hv_Dist.Dispose();
                hv_E.Dispose();
                hv_Delta.Dispose();
                hv_DistTmp.Dispose();
                hv_DistDAx.Dispose();
                hv_DistDAy.Dispose();
                hv_DistDBx.Dispose();
                hv_DistDBy.Dispose();
                hv_A.Dispose();
                hv_SeqR.Dispose();
                hv_SeqC.Dispose();
                hv_y.Dispose();
                hv_X.Dispose();
                hv_Values.Dispose();
                hv_SDevE.Dispose();
                hv_MaskUse.Dispose();
                hv_SilhCamTmp.Dispose();
                hv_CamOk.Dispose();
                hv_NumPointsRemaining.Dispose();
                hv_CamPose0.Dispose();
                hv_Foot.Dispose();
                hv_XAxis.Dispose();
                hv_ZAxis.Dispose();
                hv_YAxis.Dispose();
                hv_HomMat3DCylinderEst.Dispose();
                hv_PoseCylinderEst.Dispose();
                hv_PoseCylinder.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageRectified.Dispose();

                hv_SerializedItemHandle.Dispose();
                hv_CameraSetupModelZeroDistCylApprox.Dispose();
                hv_A2x.Dispose();
                hv_A2y.Dispose();
                hv_A2z.Dispose();
                hv_B2x.Dispose();
                hv_B2y.Dispose();
                hv_B2z.Dispose();
                hv_SilhRow.Dispose();
                hv_SilhCol.Dispose();
                hv_SilhCam.Dispose();
                hv_SilhRowElim.Dispose();
                hv_SilhColElim.Dispose();
                hv_SilhCamElim.Dispose();
                hv_Cam.Dispose();
                hv_CamParam.Dispose();
                hv_CamPose.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_FuzzyFunction.Dispose();
                hv_I.Dispose();
                hv_RowEdgeFirst.Dispose();
                hv_ColumnEdgeFirst.Dispose();
                hv_AmplitudeFirst.Dispose();
                hv_RowEdgeSecond.Dispose();
                hv_ColumnEdgeSecond.Dispose();
                hv_AmplitudeSecond.Dispose();
                hv_RowPairCenter.Dispose();
                hv_ColumnPairCenter.Dispose();
                hv_FuzzyScore.Dispose();
                hv_IntraDistance.Dispose();
                hv_SIF.Dispose();
                hv_SIS.Dispose();
                hv_PX.Dispose();
                hv_PY.Dispose();
                hv_PZ.Dispose();
                hv_QX.Dispose();
                hv_QY.Dispose();
                hv_QZ.Dispose();
                hv_HomMat3D.Dispose();
                hv_PX0.Dispose();
                hv_PY0.Dispose();
                hv_PZ0.Dispose();
                hv_QX0.Dispose();
                hv_QY0.Dispose();
                hv_QZ0.Dispose();
                hv_CamI.Dispose();
                hv_NumPoints.Dispose();
                hv_A1x.Dispose();
                hv_A1y.Dispose();
                hv_A1z.Dispose();
                hv_B1x.Dispose();
                hv_B1y.Dispose();
                hv_B1z.Dispose();
                hv_Dx.Dispose();
                hv_ErrorLog.Dispose();
                hv_Iter.Dispose();
                hv_IterInter.Dispose();
                hv_MaxIter.Dispose();
                hv_MaxIterInter.Dispose();
                hv_LastError.Dispose();
                hv_SDevFactor.Dispose();
                hv_Dist.Dispose();
                hv_E.Dispose();
                hv_Delta.Dispose();
                hv_DistTmp.Dispose();
                hv_DistDAx.Dispose();
                hv_DistDAy.Dispose();
                hv_DistDBx.Dispose();
                hv_DistDBy.Dispose();
                hv_A.Dispose();
                hv_SeqR.Dispose();
                hv_SeqC.Dispose();
                hv_y.Dispose();
                hv_X.Dispose();
                hv_Values.Dispose();
                hv_SDevE.Dispose();
                hv_MaskUse.Dispose();
                hv_SilhCamTmp.Dispose();
                hv_CamOk.Dispose();
                hv_NumPointsRemaining.Dispose();
                hv_CamPose0.Dispose();
                hv_Foot.Dispose();
                hv_XAxis.Dispose();
                hv_ZAxis.Dispose();
                hv_YAxis.Dispose();
                hv_HomMat3DCylinderEst.Dispose();
                hv_PoseCylinderEst.Dispose();
                hv_PoseCylinder.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void CylinderExpansionImageFunction(Cell cell, HObject IntoImage, int CameraIndex, out HObject ExpansionImage, Boolean bFlagInitialAlgorithmParam = false)
        {
            SOCRDateAlgorParam param =
            new(
                (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect),
                cell.MmPerPixel * 1000
            );

            HOperatorSet.GenEmptyObj(out ExpansionImage);

            HObject ho_ObjectSelected = null;
            HObject ho_ImagesRectified = null, ho_Regions = null;

            // Local control variables 
            HTuple hv_LabelMinRow = new HTuple();
            HTuple hv_LabelMaxRow = new HTuple();
            HTuple hv_Row = new HTuple(), hv_MeasureHandle = new HTuple();
            HTuple hv_Index = new HTuple();
            HTuple hv_MinZ = new HTuple();
            HTuple hv_MaxZ = new HTuple();

            HTuple hv_CameraSetupModelZeroDistInCylinderOrigin = new HTuple();
            HTuple hv_ProcessImageIndex = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_ImagesRectified);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            try
            {
                //
                //Control parameters for fine tuning:
                //- Parameters for the fine tuning of the fine adjustment.
                //- Parameters for the fine tuning of the silhouette extraction.

                hv_LabelMinRow.Dispose();
                hv_LabelMinRow = param.LabelMinRow;
                hv_LabelMaxRow.Dispose();
                hv_LabelMaxRow = param.LabelMaxRow;
                //

                if (bFlagInitialAlgorithmParam)
                {

                    m_PixelSize = 0.001 * param.PixelSizeMM;
                    m_CylinderRadius = 0.001 * param.CylinderRadiusMM;

                    HTuple end_val58 = hv_LabelMaxRow;
                    HTuple step_val58 = m_SilhouetteMeasureDistance;

                    m_MeasureHandles = new HTuple();
                    for (hv_Row = hv_LabelMinRow; hv_Row.Continue(end_val58, step_val58); hv_Row = hv_Row.TupleAdd(step_val58))
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_MeasureHandle.Dispose();
                            HOperatorSet.GenMeasureRectangle2(hv_Row, cell.Image.ImageWidth / 2, (new HTuple(0)).TupleRad()
                                , (cell.Image.ImageWidth / 2) - 100, m_SilhouetteMeasureLength2, cell.Image.ImageWidth, cell.Image.ImageHeight,
                                "nearest_neighbor", out hv_MeasureHandle);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_MeasureHandles = m_MeasureHandles.TupleConcat(
                                    hv_MeasureHandle);
                                m_MeasureHandles.Dispose();
                                m_MeasureHandles = ExpTmpLocalVar_MeasureHandles;
                            }
                        }
                    }

                    //end_val58.Dispose();
                    //step_val58.Dispose();

                    //Create rectification maps for the elimination of the radial distortion
                    //and create a camera setup model for the rectified images.          
                    m_CameraSetupModelZeroDist = new HTuple();
                    m_CameraSetupModelZeroDist.Dispose();

                    m_CameraNum = new HTuple();
                    m_CameraNum.Dispose();
                    prepare_distortion_removal(out m_RectificationMaps, m_CameraSetupModel, out m_CameraSetupModelZeroDist, out m_CameraNum);

                    //Determine an approximate pose of the cylinder
                    //assuming that it stands upright in the center of the cameras.
                    m_PoseCylinderApprox = new HTuple();
                    m_PoseCylinderApprox.Dispose();

                    m_HomMat3DCylinderApprox = new HTuple();
                    m_HomMat3DCylinderApprox.Dispose();
                    determine_approximate_cylinder_pose_in_center_of_cameras(m_CameraSetupModelZeroDist,
                        m_CameraNum, out m_PoseCylinderApprox, out m_HomMat3DCylinderApprox);

                    //Determine approximately the required size (height) of the 3D model
                    //of the cylinder such that it covers the area between the given borders.
                    hv_MinZ.Dispose(); hv_MaxZ.Dispose();
                    determine_required_cylinder_model_extent(hv_LabelMinRow, hv_LabelMaxRow, cell.Image.ImageWidth,
                        m_CameraSetupModelZeroDist, m_CameraNum, m_HomMat3DCylinderApprox,
                        out hv_MinZ, out hv_MaxZ);
                    //
                    //Determine the minimal and maximal distances between the opposite silhouettes.
                    m_MinPairDist = new HTuple();
                    m_MinPairDist.Dispose();

                    m_MaxPairDist = new HTuple();
                    m_MaxPairDist.Dispose();
                    determine_min_max_silhouette_distance(m_CameraSetupModelZeroDist, m_CameraNum,
                        m_PoseCylinderApprox, m_CylinderRadius, hv_MinZ, hv_MaxZ, cell.Image.ImageWidth, out m_MinPairDist,
                        out m_MaxPairDist);
                    //
                    //Create the 3D object model of the cylinder and get the 3D points.
                    m_NumSlices = new HTuple();
                    m_NumSlices.Dispose();

                    m_NumPointsPerSlice = new HTuple();
                    m_NumPointsPerSlice.Dispose();

                    m_MinZI = new HTuple();
                    m_MinZI.Dispose();

                    m_MaxZI = new HTuple();
                    m_MaxZI.Dispose();

                    m_CylinderPointsX = new HTuple();
                    m_CylinderPointsX.Dispose();

                    m_CylinderPointsY = new HTuple();
                    m_CylinderPointsY.Dispose();

                    m_CylinderPointsZ = new HTuple();
                    m_CylinderPointsZ.Dispose();
                    gen_cylinder_model(m_PixelSize, m_CylinderRadius, hv_MinZ, hv_MaxZ, out m_NumSlices,
                        out m_NumPointsPerSlice, out m_MinZI, out m_MaxZI, out m_CylinderPointsX,
                        out m_CylinderPointsY, out m_CylinderPointsZ);
                    //
                    //Determine the size of the final mosaic image.
                    m_MosaicHeight = new HTuple();
                    m_MosaicHeight.Dispose();
                    m_MosaicHeight = new HTuple(m_NumSlices);

                    m_MosaicWidth = new HTuple();
                    m_MosaicWidth.Dispose();
                    m_MosaicWidth = new HTuple(m_NumPointsPerSlice);

                }

                ho_ImagesRectified.Dispose();
                eliminate_radial_distortions(IntoImage, m_RectificationMaps, out ho_ImagesRectified,
                    m_CameraNum);
                //
                //Determine the pose of the rotation axis in 3D and create an additional camera setup model
                //with the origin on the rotation axis.
                //using (DevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CameraSetupModelZeroDistInCylinderOrigin.Dispose();
                    determine_rotation_axis_3d(ho_ImagesRectified, m_CylinderRadius,
                        m_MeasureHandles, cell.Image.ImageWidth, cell.Image.ImageHeight, m_CameraSetupModelZeroDist,
                        m_CameraNum, m_PoseCylinderApprox, m_MinPairDist, m_MaxPairDist,
                        m_SilhouetteMeasureSigma, m_SilhouetteMeasureThreshold, m_SilhouetteMaxTilt,
                        0.1 * m_PixelSize, out hv_CameraSetupModelZeroDistInCylinderOrigin);
                }
                //
                //确定拼接图像各组成部分的位置
                ho_Regions.Dispose();
                determine_source_cameras_for_mosaic_parts(out ho_Regions, hv_CameraSetupModelZeroDistInCylinderOrigin,
                    m_CameraNum, m_CylinderPointsX, m_CylinderPointsY, m_CylinderPointsZ,
                    m_NumPointsPerSlice, m_MinZI, m_MaxZI, m_MosaicWidth, m_MosaicHeight);
                //
                //展开图像
                hv_ProcessImageIndex.Dispose();
                hv_ProcessImageIndex = 3;
                CylinderExpansionImages(ho_Regions, ho_ImagesRectified, out ExpansionImage,
                    hv_ProcessImageIndex, m_FineAdjustmentMaxShift, m_FineAdjustmentMatchingWidth,
                    m_BlendingSeam, hv_CameraSetupModelZeroDistInCylinderOrigin, m_CameraNum,
                    m_MosaicWidth, m_MosaicHeight, m_CylinderPointsX, m_CylinderPointsY,
                    m_CylinderPointsZ, m_NumSlices, m_NumPointsPerSlice, hv_LabelMinRow,
                    hv_LabelMaxRow, CameraIndex);

            }
            catch (HalconException HDevExpDefaultException)
            {
                //ho_RectificationMaps.Dispose();
                ho_ObjectSelected.Dispose();
                ho_ImagesRectified.Dispose();
                ho_Regions.Dispose();

                hv_LabelMinRow.Dispose();
                hv_LabelMaxRow.Dispose();

                hv_Row.Dispose();
                hv_MeasureHandle.Dispose();

                hv_Index.Dispose();

                hv_MinZ.Dispose();
                hv_MaxZ.Dispose();

                hv_CameraSetupModelZeroDistInCylinderOrigin.Dispose();
                hv_ProcessImageIndex.Dispose();

                throw HDevExpDefaultException;
            }

            ho_ObjectSelected.Dispose();
            ho_ImagesRectified.Dispose();
            ho_Regions.Dispose();

            hv_LabelMinRow.Dispose();
            hv_LabelMaxRow.Dispose();

            hv_Row.Dispose();
            hv_MeasureHandle.Dispose();

            hv_Index.Dispose();
            hv_MinZ.Dispose();
            hv_MaxZ.Dispose();

            hv_CameraSetupModelZeroDistInCylinderOrigin.Dispose();
            hv_ProcessImageIndex.Dispose();
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
        /// <param name="hv_OutFlagCap"></param>
        private void InspectionLabel(HObject ho_IntoImage, HTuple hv_IntoThreshold, HTuple hv_MinRow,
            HTuple hv_MaxRow, out HTuple hv_OutFlagCap)
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
            HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, 300, 1);
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
            if ((int)(new HTuple(hv_tempLength.TupleGreater(300))) != 0)
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
        /// 瓶盖缺失检测算法
        /// </summary>
        /// <param name="ho_IntoImage"></param>
        /// <param name="hv_IntoThreshold"></param>
        /// <param name="hv_MinRow"></param>
        /// <param name="hv_MaxRow"></param>
        /// <param name="hv_OutFlagCap"></param>
        private void InspectionCap(HObject ho_IntoImage, HTuple hv_IntoThreshold, HTuple hv_MinRow,
            HTuple hv_MaxRow, out HTuple hv_OutFlagCap)
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
            HOperatorSet.OpeningRectangle1(ho_Region, out ho_RegionOpening, 1, 50);
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
            if ((int)(new HTuple(hv_tempLength.TupleGreater(300))) != 0)
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            CameraImageInfoStruct tempCameraImageInfo;
            tempCameraImageInfo.CameraIndex = 1;
            tempCameraImageInfo.CameraImage = null;

            PhotoID++;
            cell.ID = PhotoID.ToString();

            //2025.03.16 易群生
            //等待4个相机图像

            if (cell.CamSerial == "DSGP23500001117")
            {
                tempCameraImageInfo.CameraIndex = 1;
            }
            else
            if (cell.CamSerial == "DSGP23500001116")
            {
                tempCameraImageInfo.CameraIndex = 2;
            }
            else
            if (cell.CamSerial == "DSGP23500001126")
            {
                tempCameraImageInfo.CameraIndex = 3;
            }
            else
            {
                tempCameraImageInfo.CameraIndex = 4;
            }

            CameraIndex = tempCameraImageInfo.CameraIndex;

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

            lock (m_WaitImage_globalLock)
            {
                if (CameraImageStructList.Count == 0)
                {
                    CameraImageStructList.Add(tempCameraImageInfo);
                }
                else
                if (CameraImageStructList[CameraImageStructList.Count - 1].CameraIndex < tempCameraImageInfo.CameraIndex)
                {
                    CameraImageStructList.Add(tempCameraImageInfo);
                }
                else
                {
                    int tempCount = CameraImageStructList.Count;
                    for (int i = 0; i < tempCount; i++)
                    {
                        if (CameraImageStructList[i].CameraIndex > tempCameraImageInfo.CameraIndex)
                        {
                            CameraImageStructList.Insert(i, tempCameraImageInfo);
                            break;
                        }
                    }
                }


                if (CameraImageStructList.Count == 4)
                {
                    for (int i = 0; i < CameraImageStructList.Count; i++)
                    {
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(m_ProcessImageAry, CameraImageStructList[i].CameraImage, out ExpTmpOutVar_0
                                );
                            m_ProcessImageAry.Dispose();
                            m_ProcessImageAry = ExpTmpOutVar_0;
                        }
                    }

                }
            }

            for (int i = 1; i <= 4; i++)
            {
                if (i==CameraIndex)
                {
                    continue;
                }
                WaitImageCountdownEvent[i-1].Signal();
            }
            
            WaitImageCountdownEvent[CameraIndex-1].Wait();
            WaitImageCountdownEvent[CameraIndex - 1].Reset();


            SOCRDateAlgorParam param =
                new(
                    (CPcParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect),
                    cell.MmPerPixel * 1000
                );

            if ((param.CapMinRow > 0) && (param.CapMaxRow > 0))
            {
                HTuple bFlagCap = null;
                try
                {
                    InspectionCap(tempCameraImageInfo.CameraImage, 50, param.CapMinRow, param.CapMaxRow, out bFlagCap);

                    if (bFlagCap == 1)
                    {
                        CapTestResult = 1;
                    }
                    else
                    {
                        CapTestResult = 0;
                    }
                }
                catch (Exception ex)
                {
                    if (bFlagCap != null)
                    {
                        bFlagCap.Dispose();
                    }

                    OperateLog.Info("BottleTest_InspectionCap:" + ex.Message.ToString());
                }

                if (bFlagCap != null)
                {
                    bFlagCap.Dispose();
                }
            }

            /// 2025.03.19 易群生
            /// 标签缺失检测
            HTuple bFlagLabel = null;
            try
            {
                InspectionLabel(tempCameraImageInfo.CameraImage, 50, param.LabelMinRow, param.LabelMaxRow - 150, out bFlagLabel);
                lock (m_WaitImage_globalLock)
                {
                    if (bFlagLabel.I == 1)
                    {
                        LabelTestResult = 1;
                    }
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

            if (bFlagLabel != null)
            {
                bFlagLabel.Dispose();
            }

            OCRResult ocrResult = new OCRResult();
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
                                m_DateCropY = param.DateMinRow - param.LabelMinRow; // 切图的起始Y坐标
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
                                        tempPoint.Y = hv_OutRow1.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = hv_OutColumn2.D + m_DateCropX;
                                        tempPoint.Y = hv_OutRow2.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        break;
                                    case 90:
                                        tempPoint.X = tempW1 - hv_OutRow2.D - 1 + m_DateCropX;
                                        tempPoint.Y = hv_OutColumn1.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = tempW1 - hv_OutRow1.D - 1 + m_DateCropX;
                                        tempPoint.Y = hv_OutColumn2.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);
                                        break;

                                    case 180:
                                        tempPoint.X = tempW1 - hv_OutColumn2.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutRow2.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = tempW1 - hv_OutColumn1.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutRow1.D + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        break;

                                    case 270:
                                        tempPoint.X = hv_OutRow1.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutColumn2.D - 1 + m_DateCropY + param.LabelMinRow;
                                        tempPointList.Add(tempPoint);

                                        tempPoint.X = hv_OutRow2.D + m_DateCropX;
                                        tempPoint.Y = tempH1 - hv_OutColumn1.D - 1 + m_DateCropY + param.LabelMinRow;
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

                        break;

                    case 1:
                        //2025.03.18 易群生
                        //百度 Ocr

                        HObject tempInterleaveImage;
                        HOperatorSet.GenEmptyObj(out tempInterleaveImage);
                        tempInterleaveImage.Dispose();

                        HTuple tempPointer = null, tempType = null, tempW = null, tempH = null;

                        try
                        {
                            HOperatorSet.InterleaveChannels(tempCameraImageInfo.CameraImage, out tempInterleaveImage, "argb", "match", 255);
                            HOperatorSet.GetImagePointer1(tempInterleaveImage, out tempPointer, out tempType, out tempW, out tempH);
                            IntPtr tempPtr = tempPointer;
                            tempDateBitmap = new System.Drawing.Bitmap(tempW / 4, tempH, tempW,
                                System.Drawing.Imaging.PixelFormat.Format32bppRgb, tempPtr);
                        }
                        catch (Exception ex)
                        {
                            if (tempInterleaveImage != null)
                            {
                                tempInterleaveImage.Dispose();
                            }

                            if (tempPointer != null)
                            {
                                tempPointer.Dispose();
                            }

                            if (tempType != null)
                            {
                                tempType.Dispose();

                            }

                            if (tempW != null)
                            {
                                tempW.Dispose();
                            }

                            if (tempH != null)
                            {
                                tempH.Dispose();
                            }

                            OperateLog.Info("BottleTest_ocr_2:" + ex.Message.ToString());
                        }

                        if (tempInterleaveImage != null)
                        {
                            tempInterleaveImage.Dispose();
                        }

                        if (tempPointer != null)
                        {
                            tempPointer.Dispose();
                        }

                        if (tempType != null)
                        {
                            tempType.Dispose();

                        }

                        if (tempW != null)
                        {
                            tempW.Dispose();
                        }

                        if (tempH != null)
                        {
                            tempH.Dispose();
                        }

                        //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(cell.Image.ImageWidth, cell.Image.ImageHeight, cell.Image.ImageWidth*4, 
                        //System.Drawing.Imaging.PixelFormat.Format32bppRgb, cell.Image.ImageData);
                        //tempDateBitmap.Save("D:\\output_" + tempCameraImageInfo.CameraIndex.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        if (m_bFlagInitialAlgorithmParam)
                        {
                            m_DateCropX = 0; // 切图的起始X坐标
                            m_DateCropY = param.DateMinRow - param.LabelMinRow; // 切图的起始Y坐标
                            m_DateCropWidth = tempW / 4; // 切图的宽度
                            m_DateCropHeight = param.DateMaxRow - param.DateMinRow + 1; // 切图的高度

                            m_DateCropRect = new System.Drawing.Rectangle(m_DateCropX, m_DateCropY, m_DateCropWidth, m_DateCropHeight);
                            m_bFlagInitialAlgorithmParam = false;
                        }

                        tempDateCroppedBitmap = tempDateBitmap.Clone(m_DateCropRect, tempDateBitmap.PixelFormat);

                        switch (param.DateAngleIndex)
                        {
                            case 90:
                                tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;

                            case 180:
                                tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;

                            case 270:
                                tempDateCroppedBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            default:
                                break;
                        }

                        //tempDateCroppedBitmap.Save("D:\\croppedImage_" + tempCameraImageInfo.CameraIndex.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        ocrResult = m_PaddleOCREngine.DetectText(tempDateCroppedBitmap);

                        if (ocrResult.TextBlocks.Count > 0)
                        {
                            //挑选出数字日期
                            tempResult = Regex.Replace(ocrResult.Text, @"[^\d]", "");
                            cell.OcrResultString = tempResult;

                            if (tempResult != "")
                            {
                                tempHaveTextFlag = true;

                                System.Windows.Point tempLTPoint = new System.Windows.Point();
                                System.Windows.Point tempRBPoint = new System.Windows.Point();


                                double tempMinX0, tempMinY0, tempMaxX0, tempMaxY0;
                                tempMinX0 = Math.Min(ocrResult.TextBlocks[0].BoxPoints[0].X, ocrResult.TextBlocks[0].BoxPoints[3].X);
                                tempMinY0 = Math.Min(ocrResult.TextBlocks[0].BoxPoints[0].Y, ocrResult.TextBlocks[0].BoxPoints[1].Y);

                                tempMaxX0 = Math.Max(ocrResult.TextBlocks[0].BoxPoints[1].X, ocrResult.TextBlocks[0].BoxPoints[2].X);
                                tempMaxY0 = Math.Max(ocrResult.TextBlocks[0].BoxPoints[2].Y, ocrResult.TextBlocks[0].BoxPoints[3].Y);

                                double tempMinX, tempMinY, tempMaxX, tempMaxY;
                                for (int j = 1; j < ocrResult.TextBlocks.Count; j++)
                                {
                                    tempMinX = Math.Min(ocrResult.TextBlocks[j].BoxPoints[0].X, ocrResult.TextBlocks[j].BoxPoints[3].X);
                                    tempMinY = Math.Min(ocrResult.TextBlocks[j].BoxPoints[0].Y, ocrResult.TextBlocks[j].BoxPoints[1].Y);

                                    if (tempMinX0 > tempMinX)
                                    {
                                        tempMinX0 = tempMinX;
                                    }

                                    if (tempMinY0 > tempMinY)
                                    {
                                        tempMinY0 = tempMinY;
                                    }

                                    tempMaxX = Math.Max(ocrResult.TextBlocks[j].BoxPoints[1].X, ocrResult.TextBlocks[j].BoxPoints[2].X);
                                    tempMaxY = Math.Max(ocrResult.TextBlocks[j].BoxPoints[2].Y, ocrResult.TextBlocks[j].BoxPoints[3].Y);

                                    if (tempMaxX0 < tempMaxX)
                                    {
                                        tempMaxX0 = tempMaxX;
                                    }

                                    if (tempMaxY0 < tempMaxY)
                                    {
                                        tempMaxY0 = tempMaxY;
                                    }
                                }

                                switch (param.DateAngleIndex)
                                {
                                    case 0:
                                        tempLTPoint.X = tempMinX0 + m_DateCropX;
                                        tempLTPoint.Y = tempMinY0 + m_DateCropY;
                                        tempRBPoint.X = tempMaxX0 + m_DateCropX;
                                        tempRBPoint.Y = tempMaxY0 + m_DateCropY;
                                        break;
                                    case 90:
                                        tempLTPoint.X = tempMinY0 + m_DateCropX;
                                        tempLTPoint.Y = m_DateCropWidth - tempMaxX0 + m_DateCropY;
                                        tempRBPoint.X = tempMaxY0 + m_DateCropX;
                                        tempRBPoint.Y = m_DateCropWidth - tempMinX0 + m_DateCropY;
                                        break;

                                    case 180:
                                        tempLTPoint.X = m_DateCropWidth - tempMaxX0 + m_DateCropX;
                                        tempLTPoint.Y = m_DateCropHeight - tempMaxY0 + m_DateCropY;
                                        tempRBPoint.X = m_DateCropWidth - tempMinX0 + m_DateCropX;
                                        tempRBPoint.Y = m_DateCropHeight - tempMinY0 + m_DateCropY;

                                        break;

                                    case 270:
                                        tempLTPoint.X = m_DateCropHeight - tempMaxY0 + m_DateCropX;
                                        tempLTPoint.Y = tempMinX0 + m_DateCropY;
                                        tempRBPoint.X = m_DateCropHeight - tempMinY0 + m_DateCropX;
                                        tempRBPoint.Y = tempMaxX0 + m_DateCropY;

                                        break;
                                    default:
                                        break;
                                }

                                tempPointList.Add(tempLTPoint);
                                tempPointList.Add(tempRBPoint);
                                cell.DrawEdges.Add(new CEdgeDraw(tempPointList, System.Windows.Media.Brushes.Blue));
                            }
                        }

                        break;


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


            /// 2025.03.19 易群生
            /// 等待4个相机的检测结果
            for (int i = 1; i <= 4; i++)
            {
                if (i== CameraIndex)
                {
                    continue;
                }
                WaitTestResultCountdownEvent[i-1].Signal();
            }

            WaitTestResultCountdownEvent[CameraIndex - 1].Wait();
            WaitTestResultCountdownEvent[CameraIndex - 1].Reset();

            if (CapTestResult == 0)
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
                SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);

                if (DateString.Length < 1)
                {
                    AddTestResult(cell, DefectSpecies[0].RecipeDefects[0].Name, rec1Points);
                }
                else
                {
                    AddTestResult(cell, DefectSpecies[0].RecipeDefects[1].Name, rec1Points);
                }
            }

            CameraImageStructList.Clear();

            CapTestResult = 2;
            LabelTestResult = 0;
            m_DateStringTestResult = 0;

            stopwatch.Stop();
            OperateLog.Info("BottleTestTime_1:" + CameraIndex.ToString() + "_" + stopwatch.ElapsedMilliseconds.ToString());

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
    /// 2025.03.20 易群生
    /// 标签位置最上面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("标签位置最上面的行位置")]
    [property: Description("标签位置最上面的行位置")]
    private int labelMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("标签位置最下面的行位置")]
    [property: Description("标签位置最下面的行位置")]
    private int labelMaxRow = 1000;


    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最上面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("三期位置最上面的行位置")]
    [property: Description("三期位置最上面的行位置")]
    private int dateMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 标签位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("三期位置最下面的行位置")]
    [property: Description("三期位置最下面的行位置")]
    private int dateMaxRow = 1000;

    /// <summary>
    /// 2025.03.20 易群生
    /// 瓶盖位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("瓶盖位置最上面的行位置")]
    [property: Description("瓶盖位置最上面的行位置")]
    private int capMinRow = 0;

    /// <summary>
    /// 2025.03.20 易群生
    /// 瓶盖位置最下面的行位置
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("瓶盖位置最下面的行位置")]
    [property: Description("瓶盖位置最下面的行位置")]
    private int capMaxRow = 1000;

    /// <summary>
    /// 2025.03.10 易群生
    /// 日期黑色背景阈值
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("日期方向")]
    [property: Description("日期方向说明")]
    private int dateAngleIndex = 0;


    /// <summary>
    /// 2025.03.15 易群生
    /// 柱形物体的半径，单位是mm
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("柱形物体的半径mm")]
    [property: Description("柱形物体的半径mm")]
    private double cylinderRadiusMM = 11;

    /// <summary>
    /// 2025.03.15 易群生
    /// 图像的像素实际尺寸，单位是mm
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("图像的像素实际尺寸")]
    [property: Description("图像的像素实际尺寸")]
    private double pixelSizeMM = 0.058;
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
