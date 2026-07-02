using HalconDotNet;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using System.IO;
using System.Management;
using System.Windows.Threading;
using WH.Entity;
using WH.Entity.LogRecord;
using WH.Entity.MatConverter;
using WH.RunCell;
using WH.VisionLearning;
using ZipperLightHalconDet;

namespace ZipperInfo
{
    public class CZipperAutomaticAlgorithm
    {
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// UI线程调度器，MainWindow
        /// </summary>
        public static Dispatcher Dispatcher { get; set; }
        public static CZipperInfo ZipperInfo { get; set; } = new CZipperInfo();
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 处在哪个阶段
        /// </summary>
        public static int onWichStage = 0;
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 自动识别结束
        /// </summary>
        public static bool TestFinsh = false;
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
        IVisionModel WH_PullShape_Seg;

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
        protected string[] pullSharp_names;

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
        public static CLogRec AutoLogger { get; set; } = CLogRec.Create("Auto", "D:/Data");

        //CLightControlBase LightCtl_Zuo = null;
        //CLightControlBase LightCtl_You = null;

        public static bool findPulls = false;//检测到拉片
        public static bool findPuller = false; //检测到拉头
        public static bool findLogo = false; //检测到Logo

        public static bool findDownMass = false;//检测到下止
        public static bool findUpMass = false; //检测到上止

        public static bool findlianya = false;

        public static int findDownMassCount = 0;//检测到下止
        public static int findUpMassCount = 0; //检测到上止

        public static int findPullerCount = 0; //识别到拉头的次数
        public static int findPullsCount = 0; //识别到拉片的次数

        //public static int tempLightValue_zuo_change1 = 0;
        //public static int tempLightValue_zuo_change2 = 0;
        //public static int tempLightValue_you_change1 = 0;
        //public static int tempLightValue_you_change2 = 0;


        public static bool Station1_Stage1_OK = false;
        public static bool Station2_Stage1_OK = false;

        public static bool Station1_Stage2_OK = false;
        public static bool Station2_Stage2_OK = false;

        public static bool Pulls_Stage1_OK = false; //拉片识别OK
        public static bool Puller_Stage1_OK = false; //拉头识别OK

        public static bool[] findLogosidertype = new bool[2];  //0拉头  1拉片
        public static int startTriggerCount = 0;

        /// <summary>
        /// 工位1光源控制
        /// </summary>
        LightChangeBase LightChange;
        /// <summary>
        /// 工位2光源控制
        /// </summary>
        LightChangeBase LightChange2;

        /// <summary>
        /// 自动识别完成事件
        /// </summary>
        public static Action<bool> TestFinshEven;
        /// <summary>
        /// 拉链信息改变事件
        /// </summary>
       // public static Action ZipperInfoChangeEven;
        public CZipperAutomaticAlgorithm()
        {
            string SearchmodelDirPath = ".\\AlgorithmPlug\\MetalZipperAlgorihm\\Models\\Auto\\";
            string Searchtxtpath;
            string Searchmodelpath = "";

            //string pullmodelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\Pull\\PullLogoModel";
            //string pulltxtpath;
            //string pullmodelpath = "";

            //string pullSegmodelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\Pull\\PullSegModel";
            //string pullSegtxtpath;
            //string pullSegmodelpath = "";

            //string SearchmodelDirPath2 = ".\\AlgorithmPlug\\ZipperTestAlgorihm2\\Models\\Pull\\PullSearch";
            //string Searchtxtpath2;
            //string Searchmodelpath2 = "";

            if (Directory.Exists(SearchmodelDirPath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
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

            //if (Directory.Exists(pullmodelDirPath))
            //{
            //    string[] pullPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
            //    var files = pullPatterns
            //    .SelectMany(pattern => Directory.GetFiles(pullmodelDirPath, pattern))
            //    .ToList();

            //    var classNames = Directory.GetFiles(pullmodelDirPath, "*.txt", SearchOption.AllDirectories);

            //    if (files.Count > 0 && classNames.Length > 0)
            //    {
            //        pullmodelpath = files[0];
            //        pulltxtpath = classNames[0];
            //        de_Logo_pull_names = File.ReadAllLines(pulltxtpath);
            //        List<string> logostrs = de_Logo_pull_names.ToList();
            //        logostrs.Add("无LOGO");
            //        ZipperInfo.TempData1.LogoTypeStrs = logostrs.ToArray();
            //    }
            //}
            //if (Directory.Exists(pullSegmodelDirPath))
            //{
            //    string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
            //    var files = searchPatterns
            //    .SelectMany(pattern => Directory.GetFiles(pullSegmodelDirPath, pattern))
            //    .ToList();

            //    var classNames = Directory.GetFiles(pullSegmodelDirPath, "*.txt", SearchOption.AllDirectories);

            //    if (files.Count > 0 && classNames.Length > 0)
            //    {
            //        pullSegmodelpath = files[0];
            //        pullSegtxtpath = classNames[0];
            //        pullSharp_names = File.ReadAllLines(pullSegtxtpath);
            //    }
            //}

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

            if (Searchmodelpath != "" )
            {
                IniWH(Searchmodelpath);
            }

            LightChange = new COPTLinghtChange("COM1", AutoLogger);
            // LightChange2 = new CXRLinghtChange("COM2", AutoLogger);

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
            //onWichStage = 2;
            Mat img = new Mat();
            Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
            //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            ProgressBarViewModel.AutoMessage = "正在识别中...";
            if (onWichStage == 1)
            {
                if (cell.CamName == "右相机" && !Station1_Stage1_OK) //调光源只用一边的结果
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
                        CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.ZipperMinBgMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.ZipperMaxBgMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.ZipperMinMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.ZipperMaxMean,
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
                            Station1_Stage1_OK = true;
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
                            Station1_Stage1_OK = true;
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
                            Station1_Stage1_OK = true;
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
                            Station1_Stage1_OK = true;
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
                            Station1_Stage1_OK = true;
                        }

                    }
                }
                if (cell.CamName == "拉片相机"&&!Pulls_Stage1_OK)
                {
                    GetContoursAndHSV(cell, out Point[] PullPoints, out Point[] HolesPoints,
                        out float Hvalue, out float Svalue, out float Vvalue,
                        out HTuple ModelID, out HTuple recRow1, out HTuple recCol1, out HTuple recRow2, out HTuple recCol2);
                    ZipperInfo.TempData1.OrgContours = PullPoints;
                    ZipperInfo.TempData1.HoleOrgContours = HolesPoints;
                    ZipperInfo.TempData1.PullsMeanH = Hvalue;
                    ZipperInfo.TempData1.PullsMeanS = Svalue;
                    ZipperInfo.TempData1.PullsMeanV = Vvalue;
                    if (ModelID.Length>0)
                    {
                        ZipperInfo.TempData1.ModelID = ModelID;
                        if (recRow1.D != 0 && recCol1.D != 0)
                        {
                            int px = recCol1;
                            int py = recRow1;
                            int rew = (recCol2 - recCol1);
                            int reh = (recRow2 - recRow1);
                            Mat logoCutimg = img[new Rect(px, py, rew, reh)];
                            //Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", logoCutimg);
                            Dispatcher.Invoke(() =>
                            {
                                ZipperInfo.TempData1.ZipperLogoImg = MatConverter.Mat2BitmapSource(logoCutimg);
                            });
                        }

                    }
                    else
                    {
                        ZipperInfo.TempData1.ModelID = null;
                    }
                    Pulls_Stage1_OK=true;

                }
                if (cell.CamName == "拉头相机"&&!Puller_Stage1_OK)
                {
                    int px = 190, py = 280;
                    int rew = 100, reh = 50;
                    Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                    // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                    Mat hsvImage = new Mat();
                    Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                    Scalar hsvMean = Cv2.Mean(hsvImage);
                    float hMean = (float)hsvMean.Val0;
                    float sMean = (float)hsvMean.Val1;
                    float vMean = (float)hsvMean.Val2;
                    ZipperInfo.TempData1.PullerMeanH = hMean;
                    ZipperInfo.TempData1.PullerMeanS = sMean;
                    ZipperInfo.TempData1.PullerMeanV = vMean;
                    Puller_Stage1_OK=true;
                }

                if (Station1_Stage1_OK && Pulls_Stage1_OK&&Puller_Stage1_OK)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ZipperInfo.TempData1.ZipperDownmssImg = cell.Image?.ToBitmapSource().Clone();
                    });
                    img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-YOU-{LightChange.TempLightValue_Change1}.png");
                    timeOutCount = 0;
                    addOrSubCount = 0;
                    nochangeCount = 0;
                    LightChange.MaxTimeOutCount = 0;
                    LightChange.MinTimeOutCount = 0;
                    //LightChange2.MaxTimeOutCount = 0;
                    //LightChange2.MinTimeOutCount = 0;
                    ProgressBarViewModel.ProgressBarValue = 50;
                   // CZipperCommunicate.FirststageFinsh();
                    onWichStage = 2;


                }
            }
            else if (onWichStage == 2) ////第二阶段 识别链牙亮度
            {
                #region 只测右相机
                if (cell.CamName == "右相机")
                {
                    DetResult resultDet;
                    resultDet = WH_search_det.Predict(img) as DetResult;
                    if (resultDet.datas.Count > 0)
                    {
                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},开始识别链牙亮度");
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_search_names[nameindex];
                            if (labelstr.Contains("链牙"))
                            {
                                timeOutCount = 0;
                                ProgressBarViewModel.ProgressBarValue = 60;
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

                                int bx = resultDet.datas[i].box.X;
                                int by = resultDet.datas[i].box.Y+40;
                                int w = resultDet.datas[i].box.Width;
                                int h = resultDet.datas[i].box.Height-80;
                                //  Mat cutmat = img[new Rect(bx, by, w, h)];
                                // HOperatorSet.GenRectangle1(out HObject rec1, by, bx, by + h, bx + w);
                                HOperatorSet.CropRectangle1(CameraImage, out HObject cutimg, by, bx, by + h, bx + w);
                               // HOperatorSet.WriteImage(cutimg, "png", 0, $"C:\\Users\\Administrator.B\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
                                ZipperLightHelper.Instance.PullerLightDetection(cutimg, 10, 0.7,
                                    CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.PullMinBgMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.PullMaxBgMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.PullMinMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData.PullMaxMean,
                                    out var hv_VState, out var hv_VStride);
                                AutoLogger.Info($"{cell.CamName}:onWichStage=={onWichStage},光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                                CameraImage.Dispose();
                                cutimg.Dispose();
                                if (hv_VState.I == 1)
                                {
                                    if (tempVState != hv_VState.I)
                                    {
                                        addOrSubCount++;
                                    }
                                    if (addOrSubCount > 4)
                                    {
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},超过4次没变化,完成调整");
                                        addOrSubCount = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MaxTimeOutCount = 0;
                                        timeOutCount = 0;
                                        Dispatcher.Invoke(() =>
                                        {
                                            ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                        });
                                        img.Dispose();
                                        if (labelstr.Contains("金属"))
                                        {
                                            ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                                            ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                                        }
                                        onWichStage = 0;
                                        ProgressBarViewModel.ProgressBarValue = 100;
                                        CZipperCommunicate.CamTriggerStop(); //停止拍照
                                        LightChange.LineValueReset();
                                        Thread.Sleep(300);
                                        TestFinsh = true;
                                        ProgressBarViewModel.ProgressFinshEven?.Invoke();
                                    }
                                    tempVState = hv_VState.I;
                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    LightChange.ChangeLineValue1(false, val);
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    if (LightChange.MaxTimeOutCount >= 5)
                                    {
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},maxtimeout超过5次，完成调整");
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Tooth-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MaxTimeOutCount = 0;
                                        timeOutCount = 0;
                                        onWichStage = 0;
                                        if (labelstr.Contains("金属"))
                                        {
                                            ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                                            ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                                        }
                                        Dispatcher.Invoke(() =>
                                        {
                                            ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                        });
                                        ProgressBarViewModel.ProgressBarValue = 100;
                                        CZipperCommunicate.CamTriggerStop(); //停止拍照
                                        LightChange.LineValueReset();
                                        Thread.Sleep(300);
                                        TestFinsh = true;
                                        ProgressBarViewModel.ProgressFinshEven?.Invoke();
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
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},超过4次没变化,完成调整");
                                        addOrSubCount = 0;
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Tooth-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MinTimeOutCount = 0;
                                        timeOutCount = 0;
                                        img.Dispose();
                                        onWichStage = 0;
                                        if (labelstr.Contains("金属"))
                                        {
                                            ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                                            ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                                        }
                                        Dispatcher.Invoke(() =>
                                        {
                                            ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                        });
                                        ProgressBarViewModel.ProgressBarValue = 100;
                                        CZipperCommunicate.CamTriggerStop(); //停止拍照
                                        LightChange.LineValueReset();
                                        Thread.Sleep(300);
                                        TestFinsh = true;
                                        ProgressBarViewModel.ProgressFinshEven?.Invoke();
                                    }
                                    tempVState = hv_VState.I;
                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    LightChange.ChangeLineValue1(false, -val);
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    if (LightChange.MinTimeOutCount >= 5)
                                    {
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},maxtimeout超过5次，完成调整");
                                        //mintimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Tooth-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MinTimeOutCount = 0;
                                        timeOutCount = 0;
                                        onWichStage = 0;
                                        if (labelstr.Contains("金属"))
                                        {
                                            ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                                            ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                                        }
                                        Dispatcher.Invoke(() =>
                                        {
                                            ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                        });
                                        ProgressBarViewModel.ProgressBarValue = 100;
                                        CZipperCommunicate.CamTriggerStop(); //停止拍照
                                        LightChange.LineValueReset();
                                        Thread.Sleep(300);
                                        TestFinsh = true;
                                        ProgressBarViewModel.ProgressFinshEven?.Invoke();
                                    }
                                }
                                else
                                {
                                    AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                                    //进入下阶段
                                    AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},完成调整");
                                    img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Tooth-{LightChange.TempLightValue_Change1}.png");
                                    LightChange.MaxTimeOutCount = 0;
                                    LightChange.MinTimeOutCount = 0;
                                    timeOutCount = 0;
                                    onWichStage = 0;
                                    if (labelstr.Contains("金属"))
                                    {
                                        ZipperInfo.ZipperUpMassType = STOPMASS.金属;
                                        ZipperInfo.ZipperDownMassType = STOPMASS.金属;
                                    }
                                    Dispatcher.Invoke(() =>
                                    {
                                        ZipperInfo.TempData1.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                    });
                                    ProgressBarViewModel.ProgressBarValue = 100;
                                    CZipperCommunicate.CamTriggerStop(); //停止拍照
                                    LightChange.LineValueReset();
                                    Thread.Sleep(300);
                                    TestFinsh = true;
                                    ProgressBarViewModel.ProgressFinshEven?.Invoke();
                                }
                            }
                        }
                    }

                }
                #endregion
            }
            img.Dispose();
        }



        private void GetContoursAndHSV(Cell cell, out Point[] pullPoints, out Point[] holdPoints,
            out float Hvalue, out float Svalue, out float Vvalue,
            out HTuple ModelID, out HTuple recRow1, out HTuple recCol1, out HTuple recRow2, out HTuple recCol2)
        {
            recRow1 = new HTuple(); recRow2 = new HTuple();
            recCol1 = new HTuple(); recCol2 = new HTuple();
            // Local iconic variables 
            ModelID = new HTuple();
            HObject ho_Image = null, ho_GrayImage = null, ho_Region = null;
            HObject ho_ConnectedRegions = null, ho_SelectedRegions = null;
            HObject ho_Rectangle = null, ho_ImageReduced = null, ho_Region1 = null;
            HObject ho_RegionFillUp = null, ho_ConnectedRegions1 = null;
            HObject ho_SelectedRegions1 = null, ho_RegionDifference = null;
            HObject ho_ConnectedRegions3 = null, ho_SelectedRegions3 = null;
            HObject ho_Contour_lapian = null, ho_ImageReduced1 = null, ho_Region2 = null;
            HObject ho_RegionFillUp3 = null, ho_RegionClosing = null, ho_ConnectedRegions2 = null;
            HObject ho_SelectedRegions2 = null, ho_RegionFillUp1 = null;
            HObject ho_SelectedRegions6 = null, ho_Contours_hole = null;
            HObject ho_RegionDifference1 = null, ho_RegionOpening1 = null;
            HObject ho_ImageR = null, ho_ImageG = null, ho_ImageB = null;
            HObject ho_ImageH = null, ho_ImageS = null, ho_ImageV = null;
            HObject ho_RegionErosion = null, ho_ImageReduced2 = null, ho_ImageEmphasize = null;
            HObject ho_Region3 = null, ho_RegionFillUp2 = null, ho_RegionDifference2 = null;
            HObject ho_ConnectedRegions4 = null, ho_RegionOpening2 = null;
            HObject ho_ConnectedRegions5 = null, ho_SelectedRegions4 = null;
            HObject ho_SelectedRegions5 = null, ho_RegionDifference3 = null;
            HObject ho_RegionUnion = null, ho_Rectangle1 = null, ho_ImageReduced3 = null;
            HObject ho_Image1 = null;

            // Local control variables 

            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Col = new HTuple(), hv_Area = new HTuple();
            HTuple hv_Row3 = new HTuple(), hv_Column3 = new HTuple();
            HTuple hv_Row4 = new HTuple(), hv_Col4 = new HTuple();
            HTuple hv_MeanH = new HTuple(), hv_DevH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_DevS = new HTuple();
            HTuple hv_MeanV = new HTuple(), hv_DevV = new HTuple();
            HTuple hv_Area1 = new HTuple(), hv_Row5 = new HTuple();
            HTuple hv_Column5 = new HTuple(), hv_Row11 = new HTuple();
            HTuple hv_Column11 = new HTuple(), hv_Row21 = new HTuple();
            HTuple hv_Column21 = new HTuple(), hv_ModelID = new HTuple();
            HTuple hv_Row6 = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Angle = new HTuple(), hv_Score = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_Contour_lapian);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_Region2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp3);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions6);
            HOperatorSet.GenEmptyObj(out ho_Contours_hole);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference1);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageH);
            HOperatorSet.GenEmptyObj(out ho_ImageS);
            HOperatorSet.GenEmptyObj(out ho_ImageV);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_Region3);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp2);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions5);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions5);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference3);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced3);
            HOperatorSet.GenEmptyObj(out ho_Image1);
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
                HOperatorSet.Threshold(ho_GrayImage, out ho_Region, 180, 255);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                    70);
                hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
                HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_Row1, out hv_Column1,
                    out hv_Row2, out hv_Column2);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2,
                        hv_Column2 - 30);
                }
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_ImageReduced);
                //获取拉片外轮廓
                ho_Region1.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, 220, 255);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUp(ho_Region1, out ho_RegionFillUp);
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions1);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions1,
                    "max_area", 70);
                ho_RegionDifference.Dispose();
                HOperatorSet.Difference(ho_Rectangle, ho_SelectedRegions1, out ho_RegionDifference
                    );
                ho_ConnectedRegions3.Dispose();
                HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions3);
                ho_SelectedRegions3.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions3, out ho_SelectedRegions3,
                    "max_area", 70);
                ho_Contour_lapian.Dispose();
                HOperatorSet.GenContourRegionXld(ho_SelectedRegions3, out ho_Contour_lapian,
                    "border");
                hv_Row.Dispose(); hv_Col.Dispose();
                HOperatorSet.GetContourXld(ho_Contour_lapian, out hv_Row, out hv_Col);
                //获取拉片内部孔轮廓
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageReduced, ho_RegionDifference, out ho_ImageReduced1
                    );
                ho_Region2.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced1, out ho_Region2, 250, 255);
                ho_RegionFillUp3.Dispose();
                HOperatorSet.FillUp(ho_Region2, out ho_RegionFillUp3);
                //opening_circle (RegionFillUp3, RegionOpening, 8.5)
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionFillUp3, out ho_RegionClosing, 5.5);
                ho_ConnectedRegions2.Dispose();
                HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions2);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_SelectedRegions2, (new HTuple("area")).TupleConcat(
                    "convexity"), "and", (new HTuple(6500)).TupleConcat(0.9), (new HTuple(9999999999)).TupleConcat(
                    1));
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions2, out ho_RegionFillUp1);

                ho_SelectedRegions6.Dispose();
                HOperatorSet.SelectShapeStd(ho_RegionFillUp1, out ho_SelectedRegions6, "max_area",
                    70);
                hv_Area.Dispose(); hv_Row3.Dispose(); hv_Column3.Dispose();
                HOperatorSet.AreaCenter(ho_SelectedRegions6, out hv_Area, out hv_Row3, out hv_Column3);
                ho_Contours_hole.Dispose();
                HOperatorSet.GenContourRegionXld(ho_SelectedRegions6, out ho_Contours_hole,
                    "border");
                hv_Row4.Dispose(); hv_Col4.Dispose();
                HOperatorSet.GetContourXld(ho_Contours_hole, out hv_Row4, out hv_Col4);

                //计算拉片区域RGB

                ho_RegionDifference1.Dispose();
                HOperatorSet.Difference(ho_SelectedRegions3, ho_RegionClosing, out ho_RegionDifference1
                    );
                ho_RegionOpening1.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionDifference1, out ho_RegionOpening1, 3.5);
                ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_ImageR, out ho_ImageG, out ho_ImageB
                    );
                ho_ImageH.Dispose(); ho_ImageS.Dispose(); ho_ImageV.Dispose();
                HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageH, out ho_ImageS,
                    out ho_ImageV, "hsv");
                hv_MeanH.Dispose(); hv_DevH.Dispose();
                HOperatorSet.Intensity(ho_RegionOpening1, ho_ImageH, out hv_MeanH, out hv_DevH);
                hv_MeanS.Dispose(); hv_DevS.Dispose();
                HOperatorSet.Intensity(ho_RegionOpening1, ho_ImageS, out hv_MeanS, out hv_DevS);
                hv_MeanV.Dispose(); hv_DevV.Dispose();
                HOperatorSet.Intensity(ho_RegionOpening1, ho_ImageV, out hv_MeanV, out hv_DevV);
                //得到颜色值
                Hvalue = (float)hv_MeanH.D;
                Svalue = (float)hv_MeanS.D;
                Vvalue = (float)hv_MeanV.D;
                //得到拉片轮廓点和孔轮廓点
                pullPoints = new Point[hv_Row.Length];
                for (int i = 0; i < hv_Row.Length; i++)
                {
                    pullPoints[i] = new Point((int)hv_Col[i].D, (int)hv_Row[i].D);
                }
                holdPoints = new Point[hv_Row4.Length];
                for (int i = 0; i < hv_Row4.Length; i++)
                {
                    holdPoints[i] = new Point((int)hv_Col4[i].D, (int)hv_Row4[i].D);
                }

                //查找LOGO
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_SelectedRegions3, out ho_RegionErosion, 7.5);
                ho_ImageReduced2.Dispose();
                HOperatorSet.ReduceDomain(ho_GrayImage, ho_RegionErosion, out ho_ImageReduced2
                    );
                ho_ImageEmphasize.Dispose();
                HOperatorSet.Emphasize(ho_ImageReduced2, out ho_ImageEmphasize, 17, 17, 1.5);
                ho_Region3.Dispose();
                HOperatorSet.Threshold(ho_ImageEmphasize, out ho_Region3, 35, 255);
                ho_RegionFillUp2.Dispose();
                HOperatorSet.FillUp(ho_Region3, out ho_RegionFillUp2);
                ho_RegionDifference2.Dispose();
                HOperatorSet.Difference(ho_RegionFillUp2, ho_Region3, out ho_RegionDifference2
                    );
                ho_ConnectedRegions4.Dispose();
                HOperatorSet.Connection(ho_RegionDifference2, out ho_ConnectedRegions4);
                ho_RegionOpening2.Dispose();
                HOperatorSet.OpeningCircle(ho_ConnectedRegions4, out ho_RegionOpening2, 1.5);
                ho_ConnectedRegions5.Dispose();
                HOperatorSet.Connection(ho_RegionOpening2, out ho_ConnectedRegions5);
                ho_SelectedRegions4.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions5, out ho_SelectedRegions4, "area",
                    "and", 300, 99999999999);
                ho_SelectedRegions5.Dispose();
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions5);
                if ((int)(new HTuple((new HTuple(hv_Row3.TupleLength())).TupleGreater(0))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_SelectedRegions5.Dispose();
                        HOperatorSet.SelectShape(ho_SelectedRegions4, out ho_SelectedRegions5, (new HTuple("row")).TupleConcat(
                            "column"), "and", ((hv_Row3 - 20)).TupleConcat(hv_Column3 - 20), ((hv_Row3 + 20)).TupleConcat(
                            hv_Column3 + 20));
                    }
                }
                ho_RegionDifference3.Dispose();
                HOperatorSet.Difference(ho_SelectedRegions4, ho_SelectedRegions5, out ho_RegionDifference3
                    );
                hv_Area1.Dispose(); hv_Row5.Dispose(); hv_Column5.Dispose();
                HOperatorSet.AreaCenter(ho_RegionDifference3, out hv_Area1, out hv_Row5, out hv_Column5);
                ho_RegionUnion.Dispose();
                HOperatorSet.Union1(ho_RegionDifference3, out ho_RegionUnion);
                hv_Row11.Dispose(); hv_Column11.Dispose(); hv_Row21.Dispose(); hv_Column21.Dispose();
                HOperatorSet.SmallestRectangle1(ho_RegionUnion, out hv_Row11, out hv_Column11,
                    out hv_Row21, out hv_Column21);
                ho_Rectangle1.Dispose();
                HOperatorSet.GenEmptyObj(out ho_Rectangle1);
                if ((int)((new HTuple((new HTuple((new HTuple(hv_Row11.TupleGreater(0))).TupleAnd(
          new HTuple(hv_Column11.TupleGreater(0))))).TupleAnd(new HTuple(hv_Row21.TupleGreater(
          0))))).TupleAnd(new HTuple(hv_Column21.TupleGreater(0)))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Rectangle1.Dispose();
                        recRow1 = hv_Row11 - 10;
                        recRow2 = hv_Row21 + 10;
                        recCol1 = hv_Column11 - 10;
                        recCol2 = hv_Column21 + 10;
                        HOperatorSet.GenRectangle1(out ho_Rectangle1, recRow1, recCol1,
                            recRow2, recCol2);
                    }
                    ho_ImageReduced3.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle1, out ho_ImageReduced3);
                    hv_ModelID.Dispose();
                    HOperatorSet.CreateShapeModel(ho_ImageReduced3, "auto", -15, 30, "auto",
                        "auto", "use_polarity", "auto", "auto", out hv_ModelID);
                    ModelID = hv_ModelID;
                }

            }
            catch (Exception)
            {
                pullPoints = null;
                holdPoints = null;
                Hvalue = 0;
                Svalue = 0;
                Vvalue = 0;
            }


            ho_Image.Dispose();
            ho_GrayImage.Dispose();
            ho_Region.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region1.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDifference.Dispose();
            ho_ConnectedRegions3.Dispose();
            ho_SelectedRegions3.Dispose();
            ho_Contour_lapian.Dispose();
            ho_ImageReduced1.Dispose();
            ho_Region2.Dispose();
            ho_RegionFillUp3.Dispose();
            ho_RegionClosing.Dispose();
            ho_ConnectedRegions2.Dispose();
            ho_SelectedRegions2.Dispose();
            ho_RegionFillUp1.Dispose();
            ho_SelectedRegions6.Dispose();
            ho_Contours_hole.Dispose();
            ho_RegionDifference1.Dispose();
            ho_RegionOpening1.Dispose();
            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageH.Dispose();
            ho_ImageS.Dispose();
            ho_ImageV.Dispose();

            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();
            hv_Row.Dispose();
            hv_Col.Dispose();
            hv_Area.Dispose();
            hv_Row3.Dispose();
            hv_Column3.Dispose();
            hv_Row4.Dispose();
            hv_Col4.Dispose();
            hv_MeanH.Dispose();
            hv_DevH.Dispose();
            hv_MeanS.Dispose();
            hv_DevS.Dispose();
            hv_MeanV.Dispose();
            hv_DevV.Dispose();
        }


        #endregion

        #region 自动调整拉链位置算法
        public static int onWichStage2 = 0;
        public static int DownmassAutoCount = 0;
        public static int UpmassAutoCount = 0;
        List<double> downmassPoints = new List<double>();
        List<double> upmassPoints = new List<double>();

        public static bool AutoSettingPosFinsh = false;
        public static int UpmassAutoOK = 0;
        public static int DownmassAutoOK = 0;
        public static float EndPosTemp = 0;
        public static int AutoSettingTimeoutCount = 0;
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
                    if (cell.PhotoIndex == 1) //拉链下止
                    {
                        if (ZipperInfo.ZipperDownMassType == STOPMASS.无)
                        {
                            float zipperlenght = ZipperInfo.TempData1.AutoData.ZipperLenght;
                            AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                            zipperlenght = zipperlenght + 3.0f;
                            AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                            ChangePoints(zipperlenght);
                            onWichStage2 = 2;
                            return;
                        }
                        else
                        {
                            DetResult resultDet;
                            resultDet = WH_search_det.Predict(img) as DetResult;
                            if (resultDet.datas.Count > 0)
                            {
                                for (int i = 0; i < resultDet.datas.Count; i++)
                                {
                                    int nameindex = int.Parse(resultDet.datas[i].lable);
                                    string labelstr = de_search_names[nameindex];

                                    if (labelstr.Contains("下止"))
                                    {
                                        AutoLogger.Info($"{cell.CamName}:自动调整位置：识别到下止");
                                        DownmassAutoCount++;
                                        double cenx = resultDet.datas[i].box.Left + resultDet.datas[i].box.Width / 2;
                                        AutoLogger.Info($"{cell.CamName}:自动调整位置：下止中心位置{cenx.ToString("f1")}");
                                        downmassPoints.Add(cenx);
                                        if (DownmassAutoCount >= 2)
                                        {
                                            double ave = downmassPoints.Average();
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：下止平均中心位置{ave.ToString("f1")}");
                                            DownmassAutoCount = 0;
                                            downmassPoints.Clear();
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：原下止触发点位为：{EndPosTemp}");
                                            if (ave < 230)
                                            {
                                                float zipperlenght = ZipperInfo.TempData1.AutoData.ZipperLenght;
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                                                zipperlenght = zipperlenght - 0.5f;
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                                                ChangePoints(zipperlenght);

                                            }
                                            else if (ave > 300)
                                            {
                                                float zipperlenght = ZipperInfo.TempData1.AutoData.ZipperLenght;
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                                                zipperlenght = zipperlenght + 0.5f;
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                                                ChangePoints(zipperlenght);
                                            }
                                            else
                                            {
                                                DownmassAutoOK++;
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：下止在范围内{DownmassAutoOK}次");
                                                if (DownmassAutoOK >= 2)
                                                {
                                                    AutoLogger.Info($"{cell.CamName}:自动调整位置：下止位置调整完毕，进入阶段2，调整上止");
                                                    onWichStage2 = 2;
                                                    // AutoSettingPosFinsh = true;
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                        }
                    }
                }
                if (onWichStage2 == 2)
                {
                    if (cell.PhotoIndex == cell.PhotoTatolCount - 1) //拉链下止
                    {
                        DetResult resultDet;
                        resultDet = WH_search_det.Predict(img) as DetResult;
                        if (resultDet.datas.Count > 0)
                        {
                            for (int i = 0; i < resultDet.datas.Count; i++)
                            {
                                int nameindex = int.Parse(resultDet.datas[i].lable);
                                string labelstr = de_search_names[nameindex];

                                if (labelstr.Contains("上止"))
                                {
                                    AutoLogger.Info($"{cell.CamName}:自动调整位置：识别到上止");
                                    UpmassAutoCount++;
                                    double cenx = resultDet.datas[i].box.Left + resultDet.datas[i].box.Width / 2;
                                    AutoLogger.Info($"{cell.CamName}:自动调整位置：上止中心位置{cenx.ToString("f1")}");
                                    upmassPoints.Add(cenx);
                                    if (UpmassAutoCount >= 4)
                                    {
                                        double ave = upmassPoints.Average();
                                        AutoLogger.Info($"{cell.CamName}:自动调整位置：上止平均中心位置{ave.ToString("f1")}");
                                        UpmassAutoCount = 0;
                                        upmassPoints.Clear();
                                        if (ave > cell.Image.ImageWidth - 290)
                                        {
                                            EndPosTemp++;
                                            if (EndPosTemp <= 0) { EndPosTemp = 1; }
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：第一个点位设置为：{EndPosTemp}");
                                            ChangePoints2(ZipperInfo.TempData1.AutoData.ZipperLenght, EndPosTemp);
                                        }
                                        else if (ave < cell.Image.ImageWidth - 370)
                                        {

                                            EndPosTemp--;
                                            if (EndPosTemp <= 0) { EndPosTemp = 1; }
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：第一个点位设置为：{EndPosTemp}");
                                            ChangePoints2(ZipperInfo.TempData1.AutoData.ZipperLenght, EndPosTemp);
                                        }
                                        else
                                        {
                                            UpmassAutoOK++;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：上止在范围内{UpmassAutoOK}次");
                                            if (UpmassAutoOK >= 2)
                                            {
                                                AutoLogger.Info($"{cell.CamName}:自动调整位置：下止位置调节完成，结束");
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
        private void IniWH(string searchmodelpath, string pullmodelpath, string pullSegmodelpath, string searchmodelpath2)
        {
            if (!File.Exists(searchmodelpath) && !File.Exists(pullmodelpath) && !File.Exists(pullSegmodelpath) && !File.Exists(searchmodelpath))
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
            //Task task = Task.Run(() =>
            //{
            int search_Categ_num = de_search_names.Length;
            float Score = 0.45f;
            float Nms = 0.5f;
            WH_search_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath, engineType,
          CurrentDevice, search_Categ_num, Score, Nms, 480);
            //  });

            //Task task1 = Task.Run(() =>
            //{
            int pull_Categ_num = de_Logo_pull_names.Length;
            float pullScore = 0.6f;
            float pullNms = 0.8f;
            WH_Logo_Pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pullmodelpath, engineType,
          CurrentDevice, pull_Categ_num, pullScore, pullNms, 640);
            // });

            int pullSeg_Categ_num = pullSharp_names.Length;
            float segScore = 0.6f;
            float segNms = 0.5f;
            WH_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSegmodelpath, engineType,
          CurrentDevice, pullSeg_Categ_num, segScore, segNms, 640);

            int search_Categ_num2 = de_search_names2.Length;
            float Score2 = 0.45f;
            float Nms2 = 0.5f;
            WH_search_det2 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath2, engineType,
          CurrentDevice, search_Categ_num2, Score2, Nms2, 480);
        }

        private void IniWH(string searchmodelpath)
        {
            if (!File.Exists(searchmodelpath))
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
          CurrentDevice, search_Categ_num, Score, Nms, 640);

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

        public static void SaveParameter(CZipperInfo data)
        {
            try
            {
                ConfigAPI.Save(data, ParameterPath);
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
                    if (settingsModel == null)
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
