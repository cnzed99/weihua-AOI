using HalconDotNet;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenVinoSharp.Extensions.result;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Threading;
using WH.Entity;
using WH.Entity.LogRecord;
using WH.Entity.MatConverter;
using WH.LightControl;
using WH.RunCell;
using WH.VisionLearning;
using ZipperLightHalconDet;

namespace ZipperInfo
{
    public class CZipperAutomaticAlgorithm
    {

        public static CZipperAutomaticAlgorithm Instance => _instance.Value;
        private static readonly Lazy<CZipperAutomaticAlgorithm> _instance = new Lazy<CZipperAutomaticAlgorithm>(() => new CZipperAutomaticAlgorithm());

        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// UI线程调度器，MainWindow
        /// </summary>
        public Dispatcher Dispatcher { get; set; }
        public CZipperInfo ZipperInfo { get; set; } = new CZipperInfo();
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 处在哪个阶段
        /// </summary>
        public int onWichStage = 0;
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 自动识别结束
        /// </summary>
        public bool TestFinsh = false;
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 识别模型对象
        /// </summary>
        //WH WH_search_det = new();
        IVisionModel WH_search_det;

        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 识别拉头对象
        /// </summary>
        //WH WH_search_det = new();
        IVisionModel WH_Logo_Pull_det;

        /// <summary>
        /// 拉片分割模型
        /// </summary>
        IVisionModel WH_UpStopMassDefe_obb;

        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 工位2自动识别模型对象
        /// </summary>
        //WH WH_search_det = new();
        IVisionModel WH_search_det2;

        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 识别名
        /// </summary>
        string[] de_search_names;

        /// <summary>
        /// 2025.8.10 鲍赞宝
        /// 识别名
        /// </summary>
        string[] de_Logo_pull_names;
        /// <summary>
        /// 拉片分割缺陷名称
        /// </summary>
        string[] upStopMassDefe_names;

        /// <summary>
        /// 2026.4.12 鲍赞宝
        /// 识别名
        /// </summary>
        string[] de_search_names2;

        /// <summary>
        /// 超时统计
        /// </summary>
        int timeOutCount = 0;


        //public static bool zuo_lightOK;

        //public static bool you_lightOK;
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 自动识别模块日志
        /// </summary>
        public CLogRec AutoLogger { get; set; } = CLogRec.Create("Auto", "D:/Data");

        //CLightControlBase LightCtl_Zuo = null;
        //CLightControlBase LightCtl_You = null;

        public bool findPulls = false;//检测到拉片
        public bool findPuller = false; //检测到拉头
        public bool findLogo = false; //检测到Logo

        public bool findDownMass = false;//检测到下止
        public bool findUpMass = false; //检测到上止

        public bool findlianya = false;

        public int findDownMassCount = 0;//检测到下止
        public int findUpMassCount = 0; //检测到上止

        public int findPullerCount = 0; //识别到拉头的次数
        public int findPullsCount = 0; //识别到拉片的次数

        //public static int tempLightValue_zuo_change1 = 0;
        //public static int tempLightValue_zuo_change2 = 0;
        //public static int tempLightValue_you_change1 = 0;
        //public static int tempLightValue_you_change2 = 0;


        public bool Cloth_Stage1_OK = false; //布带工位识别1OK
                                             // public static bool Station2_Stage1_OK = false;

        public bool Cloth_Stage2_OK = false;//布带工位识别2OK
        public bool UpMass_Stage2_OK = false; //上止工位识别2OK

        public bool Pulls_Stage1_OK = false; //拉片识别1OK
        public bool Puller_Stage1_OK = false; //拉头识别1OK

        public bool Pulls_Stage2_OK = false; //拉片识别2OK
        public bool Puller_Stage2_OK = false; //拉头识别2OK

        public bool[] findLogosidertype = new bool[2];  //0拉头  1拉片
        public int startTriggerCount = 0;

        /// <summary>
        /// 工位1光源控制
        /// </summary>
        LightChangeBase LightChange;
        /// <summary>
        /// 拉片光源控制
        /// </summary>
        LightChangeBase LightChange_Puller;

        /// <summary>
        /// 拉头光源控制
        /// </summary>
        LightChangeBase LightChange_Pulls;

        /// <summary>
        /// 上止光源控制
        /// </summary>
        LightChangeBase LightChange_UpMass;
        /// <summary>
        /// 自动识别完成事件
        /// </summary>
        public Action<bool> TestFinshEven;
        /// <summary>
        /// 拉链信息改变事件
        /// </summary>
        // public static Action ZipperInfoChangeEven;
        string UpMassOrgImagesPath = ".\\AlgorithmPlug\\UpMassZipperAlgorihm\\Models\\UpStopMassImages";
        public void IniAutomaticAlgorithm()
        {
            string SearchmodelDirPath = ".\\AlgorithmPlug\\MetalZipperAlgorihm\\Models\\BigDetModel\\";
            string Searchtxtpath;
            string Searchmodelpath = "";

            string pullmodelDirPath = ".\\AlgorithmPlug\\PullZipperAlgorihm\\Models\\PullMetaModel\\Back";
            string pulltxtpath;
            string pullmodelpath = "";

            string upmassmodelDirPath = ".\\AlgorithmPlug\\UpMassZipperAlgorihm\\Models\\UpStopMassModel";
            string upmasstxtpath;
            string upmassmodelpath = "";



            //string SearchmodelDirPath2 = ".\\AlgorithmPlug\\ZipperTestAlgorihm2\\Models\\Pull\\PullSearch";
            //string Searchtxtpath2;
            //string Searchmodelpath2 = "";

            if (Directory.Exists(SearchmodelDirPath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model"};
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(SearchmodelDirPath, pattern))
                .ToList();

                var classNames = Directory.GetFiles(SearchmodelDirPath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    Searchmodelpath = files[0];
                    Searchtxtpath = classNames[0];
                    de_search_names = File.ReadAllLines(Searchtxtpath);
                }
            }

            if (Directory.Exists(pullmodelDirPath))
            {
                string[] pullPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model" };
                var files = pullPatterns
                .SelectMany(pattern => Directory.GetFiles(pullmodelDirPath, pattern))
                .ToList();

                var classNames = Directory.GetFiles(pullmodelDirPath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    pullmodelpath = files[0];
                    pulltxtpath = classNames[0];
                    de_Logo_pull_names = File.ReadAllLines(pulltxtpath);
                }
            }
            if (Directory.Exists(upmassmodelDirPath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model" };
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(upmassmodelDirPath, pattern))
                .ToList();

                var classNames = Directory.GetFiles(upmassmodelDirPath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    upmassmodelpath = files[0];
                    upmasstxtpath = classNames[0];
                    upStopMassDefe_names = File.ReadAllLines(upmasstxtpath);
                }
            }

            //if (Directory.Exists(SearchmodelDirPath2))
            //{
            //    string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
            //    var files = searchPatterns
            //    .SelectMany(pattern => Directory.GetFiles(SearchmodelDirPath2, pattern))
            //    .ToList();

            //    var classNames = Directory.GetFiles(SearchmodelDirPath2, "*.txt", SearchOption.AllDirectories);

            //    if (files.Count > 0 && classNames.Length > 0)
            //    {
            //        Searchmodelpath2 = files[0];
            //        Searchtxtpath2 = classNames[0];
            //        de_search_names2 = File.ReadAllLines(Searchtxtpath2);
            //    }
            //}

            //if (Searchmodelpath != "" && pullmodelpath != "" && pullSegmodelpath != "" && Searchmodelpath2 != "")
            //{
            //    IniWH(Searchmodelpath, pullmodelpath, pullSegmodelpath, Searchmodelpath2);
            //}
            List<string> logostrs = new List<string>();
            logostrs.Add("无LOGO");
            logostrs.Add("有LOGO");
            ZipperInfo.TempData1.LogoTypeStrs = logostrs.ToArray();
            if (Searchmodelpath != "" && pullmodelpath != "")
            {
                IniWH(Searchmodelpath, pullmodelpath, upmassmodelpath);
            }

            LightChange = new COPTLinghtChange("COM1", AutoLogger);
            LightChange_Puller = new CXRPullerLinghtChange("COM3");
            LightChange_Pulls = new CXRPullsLinghtChange("COM3");
            LightChange_UpMass = new CXRUpMassLinghtChange("COM3");
        }


        int addOrSubCount = 0;
        int nochangeCount = 0;
        int tempVState = 0;

        HObject CameraImage = new HObject();

        #region 新算法
        public void ZipperAutomaticAlgorithmRun(Cell cell)
        {
            if (cell.Image == null) return;
            //第一阶段: 计算光源值
            Mat orgimg = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
                 MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                 cell.Image.ImageData);
            //Mat img = new Mat();
            //Cv2.CvtColor(mat, img, ColorConversionCodes.BGR2RGB);
            // img.ImWrite($"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            //相机采集图片
            // onWichStage = 2;
            Mat img = new Mat();
            Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
            //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            ProgressBarViewModel.AutoMessage = "正在识别中...";
            if (onWichStage == 1)
            {
                if (cell.CamName == "右相机" && !Cloth_Stage1_OK) //调光源只用一边的结果
                {
                    if (cell.ImageFile == "")
                    {
                        HOperatorSet.GenImageInterleaved(out CameraImage,
                            cell.Image.ImageData,
                            "rgb",
                            cell.Image.ImageWidth,
                            cell.Image.ImageHeight,
                            -1,
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
                        HOperatorSet.ReadImage(out CameraImage, cell.ImageFile);
                    }
                    ZipperLightHelper.Instance.ZipperLightDetection(CameraImage, 10, 0.7,
                        CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.ZipperMinBgMean,
                        CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.ZipperMaxBgMean,
                        CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.ZipperMinMean,
                        CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.ZipperMaxMean,
                        out var hv_VState, out var hv_VStride, out bool isWhiteZipper);
                    //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{hv_VState}_{hv_VStride}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
                    //ProgressBarViewModel.AutoMessage = "正在识别拉链颜色...";
                    if (isWhiteZipper)
                    {
                        ZipperInfo.WhiteZippers = true;
                    }
                    else
                    {
                        ZipperInfo.WhiteZippers = false;
                    }

                    ProgressBarViewModel.ProgressBarValue = 20;
                    CameraImage.Dispose();
                    AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                    if (hv_VState.I == 1)
                    {
                        // Console.WriteLine($"需增加亮度");
                        if (tempVState != hv_VState.I)
                        {
                            addOrSubCount++;
                        }
                        if (addOrSubCount > 4)
                        {
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},超过4次没变化,进入下一阶段");
                            //进入下阶段
                            addOrSubCount = 0;
                            Cloth_Stage1_OK = true;
                        }
                        tempVState = hv_VState.I;
                        int val = hv_VStride.I;

                        if (val == 0)
                        {
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},推荐调整值:{val}");
                            val = 2;
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},修改调整值为:2");
                        }
                        LightChange.ChangeLineValue1(true, val);
                        if (LightChange.MaxTimeOutCount >= 5)
                        {
                            LightChange.MaxTimeOutCount = 0;
                            //进入下阶段
                            AutoLogger.Info($"{cell.CamName}:onWichStage=1,光源调整hv_VState={hv_VState.I}," +
                                $"当前光源值为:最大值130,进入下一阶段");
                            addOrSubCount = 0;
                            Cloth_Stage1_OK = true;
                        }
                    }
                    else if (hv_VState.I == 2)
                    {
                        // Console.WriteLine($"需减少亮度");

                        if (tempVState != hv_VState.I)
                        {
                            addOrSubCount++;
                        }
                        if (addOrSubCount > 4)
                        {
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},超过4次没变化,进入下一阶段");
                            //进入下阶段
                            addOrSubCount = 0;
                            Cloth_Stage1_OK = true;
                        }
                        tempVState = hv_VState.I;
                        int val = hv_VStride.I;
                        if (val == 0)
                        {
                            AutoLogger.Info($"{cell.CamName} :onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},推荐调整值:{val}");
                            val = 2;
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},修改调整值为:2");
                        }
                        LightChange.ChangeLineValue1(true, -val);
                        if (LightChange.MinTimeOutCount >= 5)
                        {
                            AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I}," +
                                $"当前光源值为:最小值30,进入下一阶段");
                            //进入下阶段
                            addOrSubCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            Cloth_Stage1_OK = true;
                        }
                    }
                    else
                    {
                        nochangeCount++;
                        // HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
                        if (nochangeCount >= 3)
                        {
                            nochangeCount = 0;
                            //进入下阶段
                            AutoLogger.Info($"onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},进入下一阶段2");
                            addOrSubCount = 0;
                            Cloth_Stage1_OK = true;
                        }

                    }
                }
                if (cell.CamName == "拉片相机" && !Pulls_Stage1_OK)
                {
                    if (ZipperInfo.TempData1.AutoData.PullsHaveFilm == PULLSHAVEFILM.有膜)
                    {
                        LightChange_Pulls.ChangeLineValue2(255);
                        Pulls_Stage1_OK = true;
                    }
                    else
                    {
                        HObject ho_Image = new HObject();
                        ho_Image.Dispose();
                        if (cell.ImageFile == "")
                        {
                            HOperatorSet.GenImageInterleaved(out ho_Image,
                                cell.Image.ImageData,
                                "rgb",
                                cell.Image.ImageWidth,
                                cell.Image.ImageHeight,
                                -1,
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
                            HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                        }
                        HObject ho_RectangleOrg;
                        HOperatorSet.GenEmptyObj(out ho_RectangleOrg);
                        ho_RectangleOrg.Dispose();
                        HOperatorSet.GenRectangle1(out ho_RectangleOrg, 10, 0, 860, 930);
                        GetPullsRegion(ho_Image, ho_RectangleOrg, out HObject ho_GrayImage, out HObject ho_pullRegion, out _, out _, out _, out _);
                        GetHoleRegion(ho_Image, ho_GrayImage, ho_pullRegion,
                                       out HObject ho_HoleRegion, out HTuple hv_MeanH, out HTuple hv_MeanS, out HTuple hv_MeanV);
                        if (hv_MeanV.D < 90)
                        {
                            LightChange_Pulls.ChangeLineValue1(false, 10);
                        }
                        else if (hv_MeanV.D > 120)
                        {
                            LightChange_Pulls.ChangeLineValue1(false, -10);
                        }
                        else
                        {
                            Pulls_Stage1_OK = true;
                        }

                        if (LightChange_Pulls.MinTimeOutCount > 5 || LightChange_Pulls.MaxTimeOutCount > 5)
                        {
                            Pulls_Stage1_OK = true;
                        }

                        ho_Image.Dispose();
                        ho_GrayImage.Dispose();
                        ho_pullRegion.Dispose();
                        ho_HoleRegion.Dispose();
                        hv_MeanH.Dispose();
                        hv_MeanS.Dispose();
                        hv_MeanV.Dispose();
                    }
                }
                if (cell.CamName == "拉头相机" && !Puller_Stage1_OK)
                {

                    // GetPullerHSV(cell, out float Hvalue, out float Svalue, out float Vvalue);
                    int px = 255, py = 100;
                    int rew = 180, reh = 50;
                    Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                    // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                    Mat hsvImage = new Mat();
                    Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                    Scalar hsvMean = Cv2.Mean(hsvImage);
                    //float Hvalue = (float)hsvMean.Val0;
                    //float Svalue = (float)hsvMean.Val1;
                    float Vvalue = (float)hsvMean.Val2;

                    if (Vvalue < 90)
                    {
                        LightChange_Puller.ChangeLineValue1(false, 10);
                    }
                    else if (Vvalue > 120)
                    {
                        LightChange_Puller.ChangeLineValue1(false, -10);
                    }
                    else
                    {
                        Puller_Stage1_OK = true;
                    }
                    if (LightChange_Puller.MinTimeOutCount > 5 || LightChange_Puller.MaxTimeOutCount > 5)
                    {
                        Puller_Stage1_OK = true;
                    }
                }


                if (Cloth_Stage1_OK && Pulls_Stage1_OK && Puller_Stage1_OK)
                {
                    if (cell.CamName == "右相机")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                        });
                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-YOU-{LightChange.TempLightValue_Change1}.png");
                        timeOutCount = 0;
                        addOrSubCount = 0;
                        nochangeCount = 0;
                        LightChange.MaxTimeOutCount = 0;
                        LightChange.MinTimeOutCount = 0;
                        LightChange_Pulls.MaxTimeOutCount = 0;
                        LightChange_Pulls.MinTimeOutCount = 0;
                        LightChange_Puller.MaxTimeOutCount = 0;
                        LightChange_Puller.MinTimeOutCount = 0;
                        ProgressBarViewModel.ProgressBarValue = 50;
                        onWichStage = 2;
                    }
                }
            }
            else if (onWichStage == 2) ////第二阶段 识别链牙亮度
            {

                if (cell.CamName == "右相机" && !Cloth_Stage2_OK)
                {
                    startTriggerCount++;
                    LinghtValueInfo selectColor = ZipperInfo?.TempData1?.AutoData?.LinghtValueInfos?
                  .FirstOrDefault(c => c != null && c.IsSelected);
                    int linghtvalue = selectColor?.ClothLinghtValue ?? 35;
                    int UpMassLinghtValue = selectColor?.UpMassLinghtValue ?? 255;
                    string colorname = selectColor?.ColorName ?? string.Empty;
                    LightChange.ChangeLineValue2(linghtvalue);
                    LightChange_UpMass.ChangeLineValue2(UpMassLinghtValue);
                    ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                    ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                    if (startTriggerCount > 3)
                    {
                        #region 上止色差
                        UpMassUpdate();
                        #endregion
                        Dispatcher.Invoke(() =>
                        {
                            ZipperInfo.TempData1.ZipperDownmssImg = cell.Image?.ToBitmapSource().Clone();
                        });

                        Cloth_Stage2_OK = true;
                        startTriggerCount = 0;
                    }
                }
            }
            if (cell.CamName == "拉片相机" && Pulls_Stage1_OK)
            {
                HObject ho_Image = new HObject();
                if (cell.ImageFile == "")
                {
                    HOperatorSet.GenImageInterleaved(out ho_Image,
                        cell.Image.ImageData,
                        "rgb",
                        cell.Image.ImageWidth,
                        cell.Image.ImageHeight,
                        -1,
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
                    HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                }
                GetContoursAndHSV(ho_Image, out double[] ho_Rectangle, out Point[] PullPoints, out Point[] HolesPoints,
                    out float Hvalue, out float Svalue, out float Vvalue,
                    out HTuple ModelID_logo, out HTuple recRow1, out HTuple recCol1,
                    out HTuple recRow2, out HTuple recCol2,
                    out HTuple modelID_pull, out HTuple hv_RowRef, out HTuple hv_ColumnRef, out HTuple pullArea);
                ZipperInfo.TempData1.OrgContours = PullPoints;
                ZipperInfo.TempData1.HoleOrgContours = HolesPoints;
                ZipperInfo.TempData1.PullsMeanH = Hvalue;
                ZipperInfo.TempData1.PullsMeanS = Svalue;
                ZipperInfo.TempData1.PullsMeanV = Vvalue;
                ZipperInfo.TempData1.ModelID_Pull = modelID_pull;
                ZipperInfo.TempData1.PullModelRow = hv_RowRef.D;
                ZipperInfo.TempData1.PullModelCol = hv_ColumnRef.D;
                ZipperInfo.TempData1.BackRectangle = ho_Rectangle;
                ZipperInfo.TempData1.PullSegOrgArea = pullArea;
                img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Lapian-{LightChange_Pulls.TempLightValue_Change1}.png");
                if (ModelID_logo.Length > 0)
                {
                    ZipperInfo.TempData1.ModelID_Logo = ModelID_logo;
                    ZipperInfo.ZipperLogoType = "有LOGO";
                    if (recRow1.D != 0 && recCol1.D != 0)
                    {
                        int px = recCol1;
                        int py = recRow1;
                        int rew = (recCol2 - recCol1);
                        int reh = (recRow2 - recRow1);
                        Mat logoCutimg = img[new Rect(px, py, rew, reh)];
                        Dispatcher.Invoke(() =>
                        {
                            ZipperInfo.TempData1.ZipperLogoImg = MatConverter.Mat2BitmapSource(logoCutimg);
                        });
                    }

                }
                else
                {
                    ZipperInfo.TempData1.ModelID_Logo = null;
                    ZipperInfo.ZipperLogoType = "无LOGO";
                    Dispatcher.Invoke(() =>
                    {
                        ZipperInfo.TempData1.ZipperLogoImg = null;
                    });

                }
                if (modelID_pull.Length > 0)
                {
                    ZipperInfo.TempData1.ModelID_Pull = modelID_pull;
                    int px2 = (int)ho_Rectangle[1];
                    int py2 = (int)ho_Rectangle[0];
                    int rew2 = (int)(ho_Rectangle[3] - ho_Rectangle[1]);
                    int reh2 = (int)(ho_Rectangle[2] - ho_Rectangle[0]);
                    Mat pullsCutimg = img[new Rect(px2, py2, rew2, reh2)];
                    Dispatcher.Invoke(() =>
                    {
                        ZipperInfo.TempData1.ZipperPullsImg = MatConverter.Mat2BitmapSource(pullsCutimg);
                    });
                }

                Pulls_Stage2_OK = true;

            }
            if (cell.CamName == "拉头相机" && Puller_Stage1_OK)
            {
                DetResult pullResult = WH_Logo_Pull_det.Predict(img) as DetResult;
                for (int j = 0; j < pullResult.count; j++)
                {
                    int pulllabelindex = int.Parse(pullResult[j].lable);
                    string pullabelname = de_Logo_pull_names[pulllabelindex];
                    if (pullabelname.Contains("SAB"))
                    {
                        ZipperInfo.TempData1.PullerHaveSAB = HAVESAB.有SAB;
                    }
                    else
                    {
                        ZipperInfo.TempData1.PullerHaveSAB = HAVESAB.无SAB;
                    }
                }
                GetPullerHSV(cell, out float Hvalue, out float Svalue, out float Vvalue);
                ZipperInfo.TempData1.PullerMeanH = Hvalue;
                ZipperInfo.TempData1.PullerMeanS = Svalue;
                ZipperInfo.TempData1.PullerMeanV = Vvalue;
                Puller_Stage2_OK = true;
                img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Latou-{LightChange_Puller.TempLightValue_Change1}.png");
            }
            if (Cloth_Stage2_OK && Pulls_Stage2_OK && Puller_Stage2_OK)
            {
                ProgressBarViewModel.ProgressBarValue = 100;
                CZipperCommunicate.CamTriggerStop(); //停止拍照
                CLinghtManagement.SaveLightParams();
                Thread.Sleep(300);
                TestFinsh = true;
                ProgressBarViewModel.ProgressFinshEven?.Invoke();
            }

            img.Dispose();
        }

        private void GetContoursAndHSV(HObject ho_Image, out double[] ho_Rectangle, out Point[] pullPoints, out Point[] holdPoints,
    out float Hvalue, out float Svalue, out float Vvalue,
    out HTuple ModelID_Logo, out HTuple recRow1_logo, out HTuple recCol1_logo,
    out HTuple recRow2_logo, out HTuple recCol2_logo,
    out HTuple ModelID_pull, out HTuple hv_RowRef, out HTuple hv_ColumnRef, out HTuple pullArea)
        {

            // Local iconic variables 

            // HObject ho_Image,
            HObject ho_GrayImage; HObject ho_RectangleOrg; HObject ho_BackRectangle;
            HObject ho_pullRegion, ho_Contours_pull, ho_HoleRegion;
            HObject ho_Contours_hole, ho_Image1 = null, ho_ImageAffineTrans = null;
            HObject ho_ImageReduced = null;

            // Local control variables 

            HTuple hv_Rows_pull = new HTuple(), hv_Cols_pull = new HTuple();
            HTuple hv_Area2 = new HTuple(), hv_Row6 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_PointOrder1 = new HTuple();
            HTuple hv_Length1 = new HTuple(), hv_MeanH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_MeanV = new HTuple();
            HTuple hv_Rows_hole = new HTuple(), hv_Cols_hole = new HTuple();
            HTuple hv_row1_re = new HTuple(), hv_col1_re = new HTuple();
            HTuple hv_row2_re = new HTuple(), hv_col2_re = new HTuple();
            HTuple hv_ModelID_logo = new HTuple();
            hv_RowRef = new HTuple(); hv_ColumnRef = new HTuple();
            HTuple hv_ModelID_pull = new HTuple();
            // HTuple hv_Files = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Score = new HTuple(), hv_BcanCreate = new HTuple();
            HTuple hv_IsHandle = new HTuple(), hv_Score1 = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Angle = new HTuple();
            // Initialize local and output iconic variables 
            // HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_pullRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_pull);
            HOperatorSet.GenEmptyObj(out ho_HoleRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_hole);
            HOperatorSet.GenEmptyObj(out ho_Image1);
            HOperatorSet.GenEmptyObj(out ho_ImageAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_RectangleOrg);
            HOperatorSet.GenEmptyObj(out ho_BackRectangle);
            ModelID_Logo = new HTuple();
            recRow1_logo = new HTuple();
            recCol1_logo = new HTuple();
            recRow2_logo = new HTuple();
            recCol2_logo = new HTuple();
            ModelID_pull = new HTuple();
            pullArea = new HTuple();

            try
            {
                //获取拉片区域


                ho_RectangleOrg.Dispose();
                HOperatorSet.GenRectangle1(out ho_RectangleOrg, 10, 0, 860, 930);

                ho_GrayImage.Dispose(); ho_pullRegion.Dispose(); pullArea.Dispose(); ho_BackRectangle.Dispose();
                GetPullsRegion(ho_Image, ho_RectangleOrg, out ho_GrayImage, out ho_pullRegion, out HTuple hv_Backrow1,
                    out HTuple hv_BackColumn1, out HTuple hv_Backrow2, out HTuple hv_BackColumn2);
                HOperatorSet.AreaCenter(ho_pullRegion, out pullArea, out _, out _);

                ho_Rectangle = new double[4] { hv_Backrow1.D, hv_BackColumn1.D, hv_Backrow2.D, hv_BackColumn2.D };

                //获取拉片区域轮廓
                ho_Contours_pull.Dispose();
                HOperatorSet.GenContourRegionXld(ho_pullRegion, out ho_Contours_pull, "border");
                hv_Rows_pull.Dispose(); hv_Cols_pull.Dispose();
                HOperatorSet.GetContourXld(ho_Contours_pull, out hv_Rows_pull, out hv_Cols_pull);
                hv_Area2.Dispose(); hv_Row6.Dispose(); hv_Column4.Dispose(); hv_PointOrder1.Dispose();
                HOperatorSet.AreaCenterXld(ho_Contours_pull, out hv_Area2, out hv_Row6, out hv_Column4,
                    out hv_PointOrder1);
                hv_Length1.Dispose();
                HOperatorSet.LengthXld(ho_Contours_pull, out hv_Length1);
                //获取拉片HSV值


                //获取拉片中的孔洞
                ho_HoleRegion.Dispose(); hv_MeanH.Dispose(); hv_MeanS.Dispose(); hv_MeanV.Dispose();
                GetHoleRegion(ho_Image, ho_GrayImage, ho_pullRegion, out ho_HoleRegion, out hv_MeanH,
                    out hv_MeanS, out hv_MeanV);
                ho_Contours_hole.Dispose();
                HOperatorSet.GenContourRegionXld(ho_HoleRegion, out ho_Contours_hole, "border");
                hv_Rows_hole.Dispose(); hv_Cols_hole.Dispose();
                HOperatorSet.GetContourXld(ho_Contours_hole, out hv_Rows_hole, out hv_Cols_hole);

                //获取LOGO模版
                hv_row1_re.Dispose(); hv_col1_re.Dispose(); hv_row2_re.Dispose(); hv_col2_re.Dispose(); hv_ModelID_logo.Dispose();
                GetLogoModel(ho_pullRegion, ho_HoleRegion, ho_GrayImage, true, out hv_row1_re, out hv_col1_re,
                    out hv_row2_re, out hv_col2_re, out hv_ModelID_logo);

                //*获取拉片模版
                hv_RowRef.Dispose(); hv_ColumnRef.Dispose(); hv_ModelID_pull.Dispose();
                GetPullSharpModel(ho_pullRegion, ho_GrayImage, out hv_RowRef, out hv_ColumnRef,
                    out hv_ModelID_pull);

                //传出Logo 模版和参数
                ModelID_Logo.Dispose();
                recRow1_logo.Dispose();
                recCol1_logo.Dispose();
                recRow2_logo.Dispose();
                recCol2_logo.Dispose();
                hv_IsHandle.Dispose();
                HOperatorSet.TupleIsHandle(hv_ModelID_logo, out hv_IsHandle);
                if ((int)(hv_IsHandle) != 0)
                {
                    ModelID_Logo = hv_ModelID_logo;
                    recRow1_logo = hv_row1_re;
                    recCol1_logo = hv_col1_re;
                    recRow2_logo = hv_row2_re;
                    recCol2_logo = hv_col2_re;
                }

                //传出拉片模版
                //得到拉片模版
                ModelID_pull.Dispose();
                ModelID_pull = hv_ModelID_pull;
                //得到颜色值
                Hvalue = (float)hv_MeanH.D;
                Svalue = (float)hv_MeanS.D;
                Vvalue = (float)hv_MeanV.D;

                //得到拉片轮廓点和孔轮廓点
                pullPoints = new Point[hv_Rows_pull.Length];
                for (int i = 0; i < hv_Rows_pull.Length; i++)
                {
                    pullPoints[i] = new Point((int)hv_Cols_pull[i].D, (int)hv_Rows_pull[i].D);
                }
                holdPoints = new Point[hv_Rows_hole.Length];
                for (int i = 0; i < hv_Rows_hole.Length; i++)
                {
                    holdPoints[i] = new Point((int)hv_Cols_hole[i].D, (int)hv_Rows_hole[i].D);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // ho_Image.Dispose();
                ho_GrayImage.Dispose();
                //ho_Rectangle.Dispose();
                ho_pullRegion.Dispose();
                ho_Contours_pull.Dispose();
                ho_HoleRegion.Dispose();
                ho_Contours_hole.Dispose();
                ho_Image1.Dispose();
                ho_ImageAffineTrans.Dispose();
                ho_ImageReduced.Dispose();

                hv_Rows_pull.Dispose();
                hv_Cols_pull.Dispose();
                hv_Area2.Dispose();
                hv_Row6.Dispose();
                hv_Column4.Dispose();
                hv_PointOrder1.Dispose();
                hv_Length1.Dispose();
                hv_MeanH.Dispose();
                hv_MeanS.Dispose();
                hv_MeanV.Dispose();
                hv_Rows_hole.Dispose();
                hv_Cols_hole.Dispose();
                hv_row1_re.Dispose();
                hv_col1_re.Dispose();
                hv_row2_re.Dispose();
                hv_col2_re.Dispose();
                //hv_ModelID_logo.Dispose();
                //hv_RowRef.Dispose();
                //hv_ColumnRef.Dispose();
                //hv_ModelID_pull.Dispose();
                //hv_Files.Dispose();
                //hv_Index.Dispose();
                hv_Score.Dispose();
                hv_BcanCreate.Dispose();
                hv_IsHandle.Dispose();
                hv_Score1.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Angle.Dispose();
                ho_RectangleOrg.Dispose();
            }
        }


        private void GetPullerHSV(Cell cell, out float Hvalue, out float Svalue, out float Vvalue)
        {
            Hvalue = 0; Svalue = 0; Vvalue = 0;

            HObject ho_Image = null, ho_GrayImage = null, ho_Region = null;
            HObject ho_ConnectedRegions = null, ho_RegionOpening = null;
            HObject ho_ConnectedRegions1 = null, ho_SelectedRegions = null;
            HObject ho_RegionFillUp = null, ho_RegionDifference = null;
            HObject ho_ConnectedRegions2 = null, ho_SelectedRegions1 = null;
            HObject ho_RegionErosion = null, ho_ImageR = null, ho_ImageG = null;
            HObject ho_ImageB = null, ho_ImageResultH = null, ho_ImageResultS = null;
            HObject ho_ImageResultV = null;

            // Local control variables 

            HTuple hv_MeanH = new HTuple(), hv_DevH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_DevS = new HTuple();
            HTuple hv_MeanV = new HTuple(), hv_DevV = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageResultH);
            HOperatorSet.GenEmptyObj(out ho_ImageResultS);
            HOperatorSet.GenEmptyObj(out ho_ImageResultV);
            try
            {
                if (cell.ImageFile == "")
                {
                    HOperatorSet.GenImageInterleaved(out ho_Image,
                        cell.Image.ImageData,
                        "rgb",
                        cell.Image.ImageWidth,
                        cell.Image.ImageHeight,
                        -1,
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
                    HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                }

                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_GrayImage, out ho_Region, 0, 48);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_ConnectedRegions, out ho_RegionOpening, 1.5);
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions1);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions, "max_area",
                    70);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions, out ho_RegionFillUp);
                ho_RegionDifference.Dispose();
                HOperatorSet.Difference(ho_RegionFillUp, ho_SelectedRegions, out ho_RegionDifference
                    );
                ho_ConnectedRegions2.Dispose();
                HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions2);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions2, out ho_SelectedRegions1,
                    "max_area", 70);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_SelectedRegions1, out ho_RegionErosion, 5.5);

                ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_ImageR, out ho_ImageG, out ho_ImageB
                    );
                ho_ImageResultH.Dispose(); ho_ImageResultS.Dispose(); ho_ImageResultV.Dispose();
                HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageResultH,
                    out ho_ImageResultS, out ho_ImageResultV, "hsv");
                hv_MeanH.Dispose(); hv_DevH.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultH, out hv_MeanH, out hv_DevH);
                hv_MeanS.Dispose(); hv_DevS.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultS, out hv_MeanS, out hv_DevS);
                hv_MeanV.Dispose(); hv_DevV.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultV, out hv_MeanV, out hv_DevV);

                if (hv_MeanH.D != 0)
                {
                    Hvalue = (float)hv_MeanH.D;
                }
                if (hv_MeanS.D != 0)
                {
                    Svalue = (float)hv_MeanS.D;
                }
                if (hv_MeanV.D != 0)
                {
                    Vvalue = (float)hv_MeanV.D;
                }
            }
            catch (Exception)
            {
                Hvalue = 128;
                Svalue = 128;
                Vvalue = 128;
                throw;
            }
            finally
            {
                ho_Image.Dispose();
                ho_GrayImage.Dispose();
                ho_Region.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionFillUp.Dispose();
                ho_RegionDifference.Dispose();
                ho_ConnectedRegions2.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();
                ho_ImageResultH.Dispose();
                ho_ImageResultS.Dispose();
                ho_ImageResultV.Dispose();
                hv_MeanH.Dispose();
                hv_DevH.Dispose();
                hv_MeanS.Dispose();
                hv_DevS.Dispose();
                hv_MeanV.Dispose();
                hv_DevV.Dispose();
            }

        }

        private void GetLogoModel(HObject ho_pullRegion, HObject ho_HoleRegion, HObject ho_GrayImage,
           bool hv_BcanCreate, out HTuple hv_row1_re, out HTuple hv_col1_re, out HTuple hv_row2_re,
           out HTuple hv_col2_re, out HTuple hv_ModelID_logo)
        {




            // Local iconic variables 

            HObject ho_RegionErosion, ho_ImageReduced;
            HObject ho_ImageEmphasize, ho_Region, ho_RegionFillUp, ho_RegionDifference;
            HObject ho_RegionFillUp1, ho_ConnectedRegions, ho_RegionOpening;
            HObject ho_ConnectedRegions1, ho_SelectedRegions, ho_SelectedRegions1;
            HObject ho_RegionDifference2, ho_RegionUnion, ho_Rectangle1;
            HObject ho_ImageReduced1 = null;

            // Local control variables 

            HTuple hv_Area = new HTuple(), hv_Row_hole = new HTuple();
            HTuple hv_Column_hole = new HTuple(), hv_Area3 = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row11 = new HTuple(), hv_Column11 = new HTuple();
            HTuple hv_Row21 = new HTuple(), hv_Column21 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            hv_row1_re = new HTuple();
            hv_col1_re = new HTuple();
            hv_row2_re = new HTuple();
            hv_col2_re = new HTuple();
            hv_ModelID_logo = new HTuple();
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_pullRegion, out ho_RegionErosion, 7.5);
            hv_Area.Dispose(); hv_Row_hole.Dispose(); hv_Column_hole.Dispose();
            HOperatorSet.AreaCenter(ho_HoleRegion, out hv_Area, out hv_Row_hole, out hv_Column_hole);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_RegionErosion, out ho_ImageReduced
                );
            ho_ImageEmphasize.Dispose();
            HOperatorSet.Emphasize(ho_ImageReduced, out ho_ImageEmphasize, 17, 17, 1);
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageEmphasize, out ho_Region, 35, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region, out ho_RegionFillUp);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionFillUp, ho_Region, out ho_RegionDifference);
            ho_RegionFillUp1.Dispose();
            HOperatorSet.FillUp(ho_RegionDifference, out ho_RegionFillUp1);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp1, out ho_ConnectedRegions);
            ho_RegionOpening.Dispose();
            HOperatorSet.OpeningCircle(ho_ConnectedRegions, out ho_RegionOpening, 1.5);
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions1);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions, "area",
                "and", 600, 999999999999);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            if ((int)(new HTuple((new HTuple(hv_Row_hole.TupleLength())).TupleGreater(0))) != 0)
            {
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_SelectedRegions, out ho_SelectedRegions1, (new HTuple("row")).TupleConcat(
                        "column"), "and", ((hv_Row_hole - 20)).TupleConcat(hv_Column_hole - 20), ((hv_Row_hole + 20)).TupleConcat(
                        hv_Column_hole + 20));
                }
            }
            ho_RegionDifference2.Dispose();
            HOperatorSet.Difference(ho_SelectedRegions, ho_SelectedRegions1, out ho_RegionDifference2
                );
            hv_Area3.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
            HOperatorSet.AreaCenter(ho_RegionDifference2, out hv_Area3, out hv_Row1, out hv_Column1);
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionDifference2, out ho_RegionUnion);
            hv_Row11.Dispose(); hv_Column11.Dispose(); hv_Row21.Dispose(); hv_Column21.Dispose();
            HOperatorSet.SmallestRectangle1(ho_RegionUnion, out hv_Row11, out hv_Column11,
                out hv_Row21, out hv_Column21);

            ho_Rectangle1.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            hv_row1_re.Dispose();
            hv_row1_re = 0;
            hv_row2_re.Dispose();
            hv_row2_re = 0;
            hv_col1_re.Dispose();
            hv_col1_re = 0;
            hv_col2_re.Dispose();
            hv_col2_re = 0;
            hv_ModelID_logo.Dispose();
            hv_ModelID_logo = 0;
            if ((int)((new HTuple((new HTuple((new HTuple(hv_Row11.TupleGreater(0))).TupleAnd(
                new HTuple(hv_Column11.TupleGreater(0))))).TupleAnd(new HTuple(hv_Row21.TupleGreater(
                0))))).TupleAnd(new HTuple(hv_Column21.TupleGreater(0)))) != 0)
            {
                hv_row1_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_row1_re = hv_Row11;
                }
                hv_row2_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_row2_re = hv_Row21;
                }
                hv_col1_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_col1_re = hv_Column11;
                }
                hv_col2_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_col2_re = hv_Column21;
                }
                ho_Rectangle1.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle1, hv_row1_re, hv_col1_re, hv_row2_re,
                    hv_col2_re);
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle1, out ho_ImageReduced1
                    );

                if (hv_BcanCreate)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ModelID_logo.Dispose();
                        HOperatorSet.CreateShapeModel(ho_ImageReduced1, "auto", (new HTuple(-10)).TupleDeg()
                            , (new HTuple(20)).TupleDeg(), "auto", "auto", "use_polarity", "auto",
                            "auto", out hv_ModelID_logo);
                    }
                }

            }
            ho_RegionErosion.Dispose();
            ho_ImageReduced.Dispose();
            ho_ImageEmphasize.Dispose();
            ho_Region.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionDifference.Dispose();
            ho_RegionFillUp1.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_RegionOpening.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDifference2.Dispose();
            ho_RegionUnion.Dispose();
            ho_Rectangle1.Dispose();
            ho_ImageReduced1.Dispose();

            hv_Area.Dispose();
            hv_Row_hole.Dispose();
            hv_Column_hole.Dispose();
            hv_Area3.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row11.Dispose();
            hv_Column11.Dispose();
            hv_Row21.Dispose();
            hv_Column21.Dispose();

            return;
        }

        private void GetPullSharpModel(HObject ho_pullRegion, HObject ho_GrayImage, out HTuple hv_RowRef,
            out HTuple hv_ColumnRef, out HTuple hv_ModelID)
        {



            // Local iconic variables 

            HObject ho_Rectangle1, ho_ImageReduced1;

            // Local control variables 

            HTuple hv_Row11 = new HTuple(), hv_Column11 = new HTuple();
            HTuple hv_Row21 = new HTuple(), hv_Column21 = new HTuple();
            HTuple hv_Area = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            hv_RowRef = new HTuple();
            hv_ColumnRef = new HTuple();
            hv_ModelID = new HTuple();
            hv_Row11.Dispose(); hv_Column11.Dispose(); hv_Row21.Dispose(); hv_Column21.Dispose();
            HOperatorSet.SmallestRectangle1(ho_pullRegion, out hv_Row11, out hv_Column11,
                out hv_Row21, out hv_Column21);
            ho_Rectangle1.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle1, hv_Row11, hv_Column11, hv_Row21,
                hv_Column21);
            hv_Area.Dispose(); hv_RowRef.Dispose(); hv_ColumnRef.Dispose();
            HOperatorSet.AreaCenter(ho_Rectangle1, out hv_Area, out hv_RowRef, out hv_ColumnRef);
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle1, out ho_ImageReduced1);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_ModelID.Dispose();
                HOperatorSet.CreateShapeModel(ho_ImageReduced1, "auto", (new HTuple(-10)).TupleDeg()
                    , (new HTuple(20)).TupleDeg(), "auto", "auto", "use_polarity", "auto", "auto",
                    out hv_ModelID);
            }
            ho_Rectangle1.Dispose();
            ho_ImageReduced1.Dispose();

            hv_Row11.Dispose();
            hv_Column11.Dispose();
            hv_Row21.Dispose();
            hv_Column21.Dispose();
            hv_Area.Dispose();

            return;
        }

        private void GetPullsRegion(HObject ho_Image, HObject ho_Rectangle, out HObject ho_GrayImage,
       out HObject ho_pullRegion, out HTuple hv_Backrow1, out HTuple hv_BackColumn1,
       out HTuple hv_Backrow2, out HTuple hv_BackColumn2)
        {



            // Local iconic variables 

            HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_ImageReduced;
            HObject ho_Region1, ho_ConnectedRegions1, ho_SelectedRegions1;
            HObject ho_RegionFillUp, ho_RegionDifference, ho_ConnectedRegions2;
            HObject ho_SelectedRegions2;

            // Local control variables 

            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_pullRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            hv_Backrow1 = new HTuple();
            hv_BackColumn1 = new HTuple();
            hv_Backrow2 = new HTuple();
            hv_BackColumn2 = new HTuple();
            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_Image, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageR, ho_Rectangle, out ho_ImageReduced);

            //获取拉片
            ho_Region1.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, 0, 65);
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_Region1, out ho_ConnectedRegions1);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions1, "max_area",
                70);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions1, out ho_RegionFillUp);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_Rectangle, ho_RegionFillUp, out ho_RegionDifference
                );
            ho_ConnectedRegions2.Dispose();
            HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions2);
            //opening_circle (ConnectedRegions2, RegionOpening, 3.5)
            ho_SelectedRegions2.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions2, out ho_SelectedRegions2, "max_area",
                70);
            ho_pullRegion.Dispose();
            HOperatorSet.Difference(ho_Rectangle, ho_SelectedRegions2, out ho_pullRegion);
            hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_pullRegion, out hv_Row1, out hv_Column1, out hv_Row2,
                out hv_Column2);
            hv_Backrow1.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Backrow1 = hv_Row1 - 150;
            }
            hv_BackColumn1.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_BackColumn1 = hv_Column1 - 50;
            }
            hv_Backrow2.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Backrow2 = hv_Row2 + 150;
            }
            hv_BackColumn2.Dispose();
            hv_BackColumn2 = new HTuple(hv_Column2);

            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region1.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionDifference.Dispose();
            ho_ConnectedRegions2.Dispose();
            ho_SelectedRegions2.Dispose();

            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();

            return;
        }
        private void GetHoleRegion(HObject ho_Image, HObject ho_GrayImage, HObject ho_pullRegion,
        out HObject ho_HoleRegion, out HTuple hv_MeanH, out HTuple hv_MeanS, out HTuple hv_MeanV)
        {



            // Local iconic variables 

            HObject ho_ImageReduced, ho_ImageR, ho_ImageG;
            HObject ho_ImageB, ho_ImageH2, ho_ImageS2, ho_ImageI2, ho_Region;
            HObject ho_RegionFillUp, ho_ConnectedRegions, ho_SelectedRegions2;
            HObject ho_RegionFillUp3, ho_RegionDifference, ho_RegionDifference1;
            HObject ho_RegionErosion, ho_ImageReduced1, ho_Region1;
            HObject ho_ImageH, ho_ImageS, ho_ImageV;

            // Local control variables 

            HTuple hv_DevH = new HTuple(), hv_DevS = new HTuple();
            HTuple hv_DevV = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_HoleRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageH2);
            HOperatorSet.GenEmptyObj(out ho_ImageS2);
            HOperatorSet.GenEmptyObj(out ho_ImageI2);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp3);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference1);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_ImageH);
            HOperatorSet.GenEmptyObj(out ho_ImageS);
            HOperatorSet.GenEmptyObj(out ho_ImageV);
            hv_MeanH = new HTuple();
            hv_MeanS = new HTuple();
            hv_MeanV = new HTuple();
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_pullRegion, out ho_ImageReduced);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_ImageReduced, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );
            ho_ImageH2.Dispose(); ho_ImageS2.Dispose(); ho_ImageI2.Dispose();
            HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageH2, out ho_ImageS2,
                out ho_ImageI2, "hsi");
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageS2, out ho_Region, 75, 255);
            //fill_up (Region, RegionFillUp)
            //closing_circle (Region, RegionClosing, 5.5)
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
            ho_SelectedRegions2.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions2, (new HTuple("area")).TupleConcat(
                "convexity"), "and", (new HTuple(6500)).TupleConcat(0.9), (new HTuple(9999999999)).TupleConcat(
                1));
            ho_RegionFillUp3.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions2, out ho_RegionFillUp3);
            ho_HoleRegion.Dispose();
            HOperatorSet.SelectShapeStd(ho_RegionFillUp3, out ho_HoleRegion, "max_area",
                70);
            //*获取拉片HSV
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_pullRegion, ho_Region, out ho_RegionDifference);
            //opening_circle (RegionDifference, RegionOpening, 3.5)
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_RegionDifference, out ho_RegionFillUp);
            ho_RegionDifference1.Dispose();
            HOperatorSet.Difference(ho_RegionFillUp, ho_HoleRegion, out ho_RegionDifference1
                );
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionDifference1, out ho_RegionErosion, 5.5);
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_RegionErosion, out ho_ImageReduced1
                );
            ho_Region1.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced1, out ho_Region1, 20, 255);
            ho_ImageH.Dispose(); ho_ImageS.Dispose(); ho_ImageV.Dispose();
            HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageH, out ho_ImageS,
                out ho_ImageV, "hsv");
            hv_MeanH.Dispose(); hv_DevH.Dispose();
            HOperatorSet.Intensity(ho_Region1, ho_ImageH, out hv_MeanH, out hv_DevH);
            hv_MeanS.Dispose(); hv_DevS.Dispose();
            HOperatorSet.Intensity(ho_Region1, ho_ImageS, out hv_MeanS, out hv_DevS);
            hv_MeanV.Dispose(); hv_DevV.Dispose();
            HOperatorSet.Intensity(ho_Region1, ho_ImageV, out hv_MeanV, out hv_DevV);
            ho_ImageReduced.Dispose();
            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageH2.Dispose();
            ho_ImageS2.Dispose();
            ho_ImageI2.Dispose();
            ho_Region.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions2.Dispose();
            ho_RegionFillUp3.Dispose();
            ho_RegionDifference.Dispose();
            ho_RegionDifference1.Dispose();
            ho_RegionErosion.Dispose();
            ho_ImageReduced1.Dispose();
            ho_Region1.Dispose();
            ho_ImageH.Dispose();
            ho_ImageS.Dispose();
            ho_ImageV.Dispose();

            hv_DevH.Dispose();
            hv_DevS.Dispose();
            hv_DevV.Dispose();

            return;

        }
        private Mat GetRoatImage(ObbData obb, Mat img)
        {
            if (obb == null) return null;
            Mat img2 = new Mat();
            if (img.Channels() == 4)
            {
                Cv2.CvtColor(img, img2, ColorConversionCodes.BGRA2BGR);
            }
            else
            {
                img2 = img;
            }
            // 定义斜矩形
            // 定义斜矩形：中心(200,200)，宽80高40，旋转 -30°
            RotatedRect rrect = new RotatedRect(
                obb.box.Center,
                obb.box.Size,
                obb.box.Angle);

            //  坑：OpenCV 的 RotatedRect.Angle 范围是 [-90,0)，
            // 当 |angle|>45 时，width/height 会被自动互换，angle 也偏移
            // 所以取 Size 时建议这样保稳：
            float w = rrect.Size.Width - 10;
            float h = rrect.Size.Height - 5;
            if (Math.Abs(rrect.Angle) > 45)
            {
                (w, h) = (h, w);   // 互换
            }

            Mat patch = new Mat();
            Cv2.GetRectSubPix(img2, new Size(w, h), rrect.Center, patch);

            return patch;

        }

        private Mat GetRoatImage(ObbData obb, Mat img, string posName)
        {
            if (obb == null) return null;

            //  Point2f[] points = obb.box.Points();
            //List<Point2f> poinstList = points.ToList();
            //poinstList.Sort((a, b) => a.Y.CompareTo(b.Y));

            Rect rect = obb.box.BoundingRect();
            int x, y, w, h;
            if (posName == "上牙")
            {
                x = rect.X + 10;
                y = rect.Y + 5;
                w = rect.Width - 15;
                h = 10;
            }
            else
            {
                x = rect.X + 10;
                y = rect.Bottom - 25;
                w = rect.Width - 20;
                h = 15;
            }

            Mat patch = new Mat(img, new Rect(x, y, w, h));
            return patch;

        }

        public  void UpMassUpdate()
        {
            LinghtValueInfo selectColor = ZipperInfo?.TempData1?.AutoData?.LinghtValueInfos?
          .FirstOrDefault(c => c != null && c.IsSelected);
            int UpMassLinghtValue = selectColor?.UpMassLinghtValue ?? 255;
            string colorname = selectColor?.ColorName ?? string.Empty;
            LightChange_UpMass.ChangeLineValue2(UpMassLinghtValue);
            if (colorname != string.Empty)
            {
                string[] upPatterns = { "*.png", "*.bmp", "*.jpg" };
                List<string> files = upPatterns
                .SelectMany(pattern => Directory.GetFiles(UpMassOrgImagesPath, pattern))
                .ToList();
                int fileindex = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].Contains(colorname))
                    {
                        fileindex = i;
                        break;
                    }
                }
                Mat upimg = Cv2.ImRead(files[fileindex]);
                ObbResult upResult = WH_UpStopMassDefe_obb.Predict(upimg) as ObbResult;
                if (upResult != null && upResult.datas.Count > 0) //如果有大缺陷直接退出
                {
                    string shangzhiIndexstr = Array.FindIndex(upStopMassDefe_names, s => s.Contains("上止")).ToString();
                    List<ObbData> shangzhiorg = upResult.datas.FindAll(c => c.lable == shangzhiIndexstr).ToList(); //上止
                    if (shangzhiorg.Count == 2)
                    {
                        shangzhiorg.Sort((a, b) => a.box.Center.Y.CompareTo(b.box.Center.Y)); //按Y坐标排序
                                                                                              //Mat uppatch = GetRoatImage(shangzhiorg[0], img, "上牙");
                        Mat uppatch = GetRoatImage(shangzhiorg[0], upimg); //上牙
                        Mat hsvImage = new Mat();
                        Cv2.CvtColor(uppatch, hsvImage, ColorConversionCodes.BGR2HSV);
                        Scalar hsvMean0 = Cv2.Mean(hsvImage);
                        float Hvalue0 = (float)hsvMean0.Val0;
                        float Svalue0 = (float)hsvMean0.Val1;
                        float Vvalue0 = (float)hsvMean0.Val2;
                        ZipperInfo.TempData1.UpMass_1_MeanH = Hvalue0;
                        ZipperInfo.TempData1.UpMass_1_MeanS = Svalue0;
                        ZipperInfo.TempData1.UpMass_1_MeanV = Vvalue0;

                        Mat uppatch1 = GetRoatImage(shangzhiorg[1], upimg); //下牙
                        Mat hsvImage1 = new Mat();
                        Cv2.CvtColor(uppatch1, hsvImage1, ColorConversionCodes.BGR2HSV);
                        Scalar hsvMean1 = Cv2.Mean(hsvImage1);
                        float Hvalue1 = (float)hsvMean1.Val0;
                        float Svalue1 = (float)hsvMean1.Val1;
                        float Vvalue1 = (float)hsvMean1.Val2;
                        ZipperInfo.TempData1.UpMass_2_MeanH = Hvalue1;
                        ZipperInfo.TempData1.UpMass_2_MeanS = Svalue1;
                        ZipperInfo.TempData1.UpMass_2_MeanV = Vvalue1;
                        hsvImage.Dispose();
                        hsvImage1.Dispose();
                    }
                }
                upimg.Dispose();

            }
        }

        #endregion

        #region 自动调整拉链位置算法
        public int onWichStage2 = 0;
        public int DownmassAutoCount = 0;
        public int UpmassAutoCount = 0;
        List<double> downmassPoints = new List<double>();
        List<double> upmassPoints = new List<double>();

        public bool AutoSettingPosFinsh = false;
        public int UpmassAutoOK = 0;
        public int DownmassAutoOK = 0;
        public float EndPosTemp = 0;
        public int AutoSettingTimeoutCount = 0;
        public void AutoSettingTriggerPos(Cell cell)
        {
            if (cell == null) return;
            if (cell.CamName == "右相机")
            {
                Mat orgimg = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
          MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
          cell.Image.ImageData);
                Mat img = new Mat();
                Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
                orgimg.Dispose();
                AutoSettingTimeoutCount++;
                if (AutoSettingTimeoutCount > 40)
                {
                    AutoSettingPosFinsh = true;
                    return;
                }
                if (onWichStage2 == 1)
                {
                    if (cell.PhotoIndex == 100) //拉链下止
                    {
                        DetResult resultDet;
                        resultDet = WH_search_det.Predict(img) as DetResult;
                        if (resultDet.datas.Count > 0)
                        {
                            for (int i = 0; i < resultDet.datas.Count; i++)
                            {
                                int nameindex = int.Parse(resultDet.datas[i].lable);
                                string labelstr = de_search_names[nameindex];

                                if (labelstr.Contains("方块插销"))
                                {
                                    AutoLogger.Info($"{cell.CamName}:自动调整位置：识别到方块插销");
                                    DownmassAutoCount++;
                                    double cenx = resultDet.datas[i].box.Left + resultDet.datas[i].box.Width / 2;
                                    AutoLogger.Info($"{cell.CamName}:自动调整位置：方块插销中心位置{cenx.ToString("f1")}");
                                    downmassPoints.Add(cenx);
                                    if (DownmassAutoCount >= 2)
                                    {
                                        double ave = downmassPoints.Average();
                                        AutoLogger.Info($"{cell.CamName}:自动调整位置：方块插销平均中心位置{ave.ToString("f1")}");
                                        DownmassAutoCount = 0;
                                        downmassPoints.Clear();
                                        AutoLogger.Info($"{cell.CamName}:自动调整位置：原方块插销触发点位为：{EndPosTemp}");
                                        if (ave < 185)
                                        {
                                            float zipperlenght = ZipperInfo.TempData1.AutoData.ZipperLenght;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                                            zipperlenght = zipperlenght - 1f;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                                            ChangePoints(zipperlenght);

                                        }
                                        else if (ave > 265)
                                        {
                                            float zipperlenght = ZipperInfo.TempData1.AutoData.ZipperLenght;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                                            zipperlenght = zipperlenght + 1f;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                                            ChangePoints(zipperlenght);
                                        }
                                        else
                                        {
                                            DownmassAutoOK++;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：方块插销在范围内{DownmassAutoOK}次");
                                            if (DownmassAutoOK >= 2)
                                            {
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：方块插销位置调节完成，结束");
                                                AutoSettingPosFinsh = true;
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                img.Dispose();
            }
        }

        private void ChangePoints(float zipperlenght)
        {
            ZipperInfo.TempData1.AutoData.ZipperLenght = zipperlenght;
            CGetZipperTriggerPoint.GetTriggerPoints(ZipperInfo.TempData1.AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount, out _);
            CGetZipperTriggerPoint.CheckPullPos(points);
            CZipperCommunicate.SendZipperLenght(zipperlenght);
            //写入拍照的总图片数量
            CZipperCommunicate.SendPhotoCount(points.Count);
            //计算拉链触发点位 ID改变位置
            CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);
        }

        private void ChangePoints2(float zipperlenght, float upchangevalue)
        {
            ZipperInfo.TempData1.AutoData.ZipperLenght = zipperlenght;
            CGetZipperTriggerPoint.GetTriggerPoints(ZipperInfo.TempData1.AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount, out int triggerType);
            CZipperCommunicate.SendZipperLenght(zipperlenght);
            if (handandtalipoints.Count > 1)
            {
                if (triggerType == 3)
                {

                    int firstIndex = points.IndexOf(handandtalipoints[0]);
                    int endposIndex = firstIndex - 1;
                    if (endposIndex >= 0)
                    {
                        if (endposIndex > 0)
                        {
                            float absvalue = Math.Abs(upchangevalue - points[endposIndex - 1]);
                            if (absvalue < 20)
                            {
                                upchangevalue = points[endposIndex - 1] + 20;
                            }
                        }

                        points[endposIndex] = upchangevalue;
                    }
                }
                else
                {
                    float absvalue = Math.Abs(upchangevalue - points[points.Count - 2]);
                    if (absvalue < 20)
                    {
                        upchangevalue = points[points.Count - 2] + 20;
                    }
                    points[points.Count - 1] = upchangevalue;
                }
            }
            CGetZipperTriggerPoint.CheckPullPos(points);
            //写入拍照的总图片数量
            CZipperCommunicate.SendPhotoCount(points.Count);
            //计算拉链触发点位 ID改变位置
            CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);
        }




        #endregion
        //private void IniWH(string searchmodelpath, string pullmodelpath, string pullSegmodelpath, string searchmodelpath2)
        //{
        //    if (!File.Exists(searchmodelpath) && !File.Exists(pullmodelpath) && !File.Exists(pullSegmodelpath) && !File.Exists(searchmodelpath))
        //    {
        //        return;
        //    }
        //    EngineType engineType;
        //    string CurrentDevice;
        //    if (HasDedicatedGraphicsCard()) //有显卡
        //    {
        //        CurrentDevice = "GPU.0";
        //        engineType = EngineType.TensorRT;
        //    }
        //    else
        //    {
        //        CurrentDevice = "CPU";
        //        engineType = EngineType.OpenVINO;
        //    }
        //    //Task task = Task.Run(() =>
        //    //{
        //    int search_Categ_num = de_search_names.Length;
        //    float Score = 0.45f;
        //    float Nms = 0.5f;
        //    WH_search_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath, engineType,
        //  CurrentDevice, search_Categ_num, Score, Nms, 480);
        //    //  });

        //    //Task task1 = Task.Run(() =>
        //    //{
        //    int pull_Categ_num = de_Logo_pull_names.Length;
        //    float pullScore = 0.6f;
        //    float pullNms = 0.8f;
        //    WH_Logo_Pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pullmodelpath, engineType,
        //  CurrentDevice, pull_Categ_num, pullScore, pullNms, 640);
        //    // });

        //    int pullSeg_Categ_num = pullSharp_names.Length;
        //    float segScore = 0.6f;
        //    float segNms = 0.5f;
        //    WH_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSegmodelpath, engineType,
        //  CurrentDevice, pullSeg_Categ_num, segScore, segNms, 640);

        //    int search_Categ_num2 = de_search_names2.Length;
        //    float Score2 = 0.45f;
        //    float Nms2 = 0.5f;
        //    WH_search_det2 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath2, engineType,
        //  CurrentDevice, search_Categ_num2, Score2, Nms2, 480);
        //}

        private void IniWH(string searchmodelpath, string pullmodelpath, string upmassmodelpath)
        {
            if (!File.Exists(searchmodelpath))
            {
                return;
            }
            if (!File.Exists(pullmodelpath))
            {
                return;
            }
            if (!File.Exists(upmassmodelpath))
            {
                return;
            }
            EngineType engineType;
            string CurrentDevice;
            if (HasDedicatedGraphicsCard()) //有显卡
            {
                CurrentDevice = "GPU.0";
                engineType = EngineType.TensorRT;
            }
            else
            {
                CurrentDevice = "CPU";
                engineType = EngineType.OpenVINO;
            }

            int search_Categ_num = de_search_names.Length;
            float Score = 0.45f;
            float Nms = 0.5f;
            WH_search_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath, engineType,
          CurrentDevice, search_Categ_num, Score, Nms, 1024);

            int pull_Categ_num = de_Logo_pull_names.Length;
            float pullScore = 0.4f;
            float pullNms = 0.5f;
            WH_Logo_Pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pullmodelpath, engineType,
          CurrentDevice, pull_Categ_num, pullScore, pullNms, 512);

            int upmass_Categ_num = upStopMassDefe_names.Length;
            float upmassScore = 0.4f;
            float upNms = 0.4f;
            WH_UpStopMassDefe_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, upmassmodelpath, engineType,
CurrentDevice, upmass_Categ_num, upmassScore, upNms, 512);

        }

        public static bool HasDedicatedGraphicsCard()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    // 常见独立显卡关键词
                    if (name.Contains("NVIDIA") ||
                        name.Contains("AMD") ||
                        name.Contains("Radeon"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false; // 如果查询失败，返回false
            }
        }



        #region 保存参数
        public static string ParameterPath = "..\\SystemConfig\\ZipperInfoData.Json";
        public static string Model_Logo_Path = "..\\SystemConfig\\ModelID_Logo.shm";
        public static string Model_Pull_Path = "..\\SystemConfig\\ModelID_Pull.shm";
        public static string Region_Back_Path = "..\\SystemConfig\\Region_Back.hobj";
        public static void SaveParameter(CZipperInfo data)
        {
            try
            {
                ConfigAPI.Save(data, ParameterPath);
                if (data.TempData1.ModelID_Logo != null && data.TempData1.ModelID_Logo.Length > 0)
                {
                    HOperatorSet.WriteShapeModel(data.TempData1.ModelID_Logo, Model_Logo_Path);
                }
                if (data.TempData1.ModelID_Pull != null && data.TempData1.ModelID_Pull.Length > 0)
                {
                    HOperatorSet.WriteShapeModel(data.TempData1.ModelID_Pull, Model_Pull_Path);
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数

        public static CZipperInfo LoadParameter()
        {
            CZipperInfo settingsModel = new CZipperInfo();
            try
            {
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.LoadDeserialize<CZipperInfo>(ParameterPath);
                    if (settingsModel != null)
                    {
                        if (File.Exists(Model_Logo_Path))
                        {
                            HOperatorSet.ReadShapeModel(Model_Logo_Path, out HTuple modelID_logo);
                            settingsModel.TempData1.ModelID_Logo = modelID_logo;

                        }
                        if (File.Exists(Model_Pull_Path))
                        {
                            HOperatorSet.ReadShapeModel(Model_Pull_Path, out HTuple modelID_pull);
                            settingsModel.TempData1.ModelID_Pull = modelID_pull;
                        }
                    }
                    else
                    {
                        settingsModel = new CZipperInfo();
                    }
                }
                else
                {
                    settingsModel = new CZipperInfo();
                }
            }
            catch (Exception)
            {
                settingsModel = new CZipperInfo();
            }
            return settingsModel;
        }

        #endregion
    }
}
