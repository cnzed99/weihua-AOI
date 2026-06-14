using HalconDotNet;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using System.IO;
using System.Windows.Threading;
using WH.Entity;
using WH.Entity.LogRecord;
using WH.RunCell;
using ZipperLightHalconDet;
using WH.VisionLearning;
using System.Management;

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
            string SearchmodelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\Pull\\PullSearch";
            string Searchtxtpath;
            string Searchmodelpath = "";

            string pullmodelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\Pull\\PullLogoModel";
            string pulltxtpath;
            string pullmodelpath = "";

            string pullSegmodelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\Pull\\PullSegModel";
            string pullSegtxtpath;
            string pullSegmodelpath = "";

            string SearchmodelDirPath2 = ".\\AlgorithmPlug\\ZipperTestAlgorihm2\\Models\\Pull\\PullSearch";
            string Searchtxtpath2;
            string Searchmodelpath2 = "";

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

            if (Directory.Exists(pullmodelDirPath))
            {
                string[] pullPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
                var files = pullPatterns
                .SelectMany(pattern => Directory.GetFiles(pullmodelDirPath, pattern))
                .ToList();

                var classNames = Directory.GetFiles(pullmodelDirPath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    pullmodelpath = files[0];
                    pulltxtpath = classNames[0];
                    de_Logo_pull_names = File.ReadAllLines(pulltxtpath);
                    List<string> logostrs = de_Logo_pull_names.ToList();
                    logostrs.Add("无LOGO");
                    ZipperInfo.TempData1.LogoTypeStrs = logostrs.ToArray();
                }
            }
            if (Directory.Exists(pullSegmodelDirPath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(pullSegmodelDirPath, pattern))
                .ToList();

                var classNames = Directory.GetFiles(pullSegmodelDirPath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    pullSegmodelpath = files[0];
                    pullSegtxtpath = classNames[0];
                    pullSharp_names = File.ReadAllLines(pullSegtxtpath);
                }
            }

            if (Directory.Exists(SearchmodelDirPath2))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(SearchmodelDirPath2, pattern))
                .ToList();

                var classNames = Directory.GetFiles(SearchmodelDirPath2, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    Searchmodelpath2 = files[0];
                    Searchtxtpath2 = classNames[0];
                    de_search_names2 = File.ReadAllLines(Searchtxtpath2);
                }
            }

            if (Searchmodelpath != "" && pullmodelpath != "" && pullSegmodelpath != "" && Searchmodelpath2 != "")
            {
                IniWH(Searchmodelpath, pullmodelpath, pullSegmodelpath, Searchmodelpath2);
            }

            //if (CLinghtManagement.LightControlDict.Count >= 2)
            //{
            //    LightCtl_Zuo = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port?.Name == "COM1");
            //    LightCtl_You = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port?.Name == "COM2");
            //}

            LightChange = new COPTLinghtChange("COM1", AutoLogger);
           // LightChange2 = new CXRLinghtChange("COM2", AutoLogger);

        }

        int addOrSubCount = 0;
        int nochangeCount = 0;
        int tempVState = 0;

        int addOrSubCount2 = 0;
        int nochangeCount2 = 0;

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

            Mat img = new Mat();
            Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
            //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            ProgressBarViewModel.AutoMessage = "正在识别中...";
            if (onWichStage == 1)
            {  
                if (cell.CamName == "右相机" && !Station1_Stage1_OK) //调光源只用一边的结果
                {
                    HOperatorSet.GenImageInterleaved(
                            out CameraImage,
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

                    // HOperatorSet.ReadImage(out CameraImage,"C:\\Users\\Administrator.B\\Desktop\\新建文件夹\\124032_1_0_OK_OK_163858938_102.png");
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
                            img.Dispose();
                            return;
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
                            img.Dispose();
                            return;
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
                            img.Dispose();
                            return;
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
                            img.Dispose();
                            return;
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
                            img.Dispose();
                        }

                    }
                }
                if (Station1_Stage1_OK && cell.CamName == "右相机")
                {
                    img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-YOU-{LightChange.TempLightValue_Change1}.png");
                    timeOutCount = 0;
                    addOrSubCount = 0;
                    addOrSubCount2 = 0;
                    nochangeCount = 0;
                    LightChange.MaxTimeOutCount = 0;
                    LightChange.MinTimeOutCount = 0;
                    LightChange2.MaxTimeOutCount = 0;
                    LightChange2.MinTimeOutCount = 0;
                    ProgressBarViewModel.ProgressBarValue = 50;
                    CZipperCommunicate.FirststageFinsh();
                    onWichStage = 2;
                }
            }
            else if (onWichStage == 2) ////第三阶段 识别链牙亮度
            {
                #region 只测右相机
                if (cell.CamName == "右相机")
                {
                    DetResult resultDet;
                    resultDet = WH_search_det.Predict(img) as DetResult;
                    if (resultDet.datas.Count > 0)
                    {
                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},开始识别拉头亮度");
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_search_names[nameindex];
                            if (labelstr.Contains("链牙"))
                            {
                                timeOutCount = 0;
                                ProgressBarViewModel.ProgressBarValue = 60;
                                HOperatorSet.GenImageInterleaved(
                                 out CameraImage,
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

                                int bx = resultDet.datas[i].box.X;
                                int by = resultDet.datas[i].box.Y;
                                int w = resultDet.datas[i].box.Width;
                                int h = resultDet.datas[i].box.Height;
                                //  Mat cutmat = img[new Rect(bx, by, w, h)];
                                // HOperatorSet.GenRectangle1(out HObject rec1, by, bx, by + h, bx + w);
                                HOperatorSet.CropRectangle1(CameraImage, out HObject cutimg, by, bx, by + h, bx + w);
                                //  HOperatorSet.WriteImage(cutimg, "png", 0, "C:\\Users\\Administrator\\Desktop\\新建文件夹 (4)\\111.png");
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
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},超过4次没变化,进入阶段4");
                                        addOrSubCount = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MaxTimeOutCount = 0;
                                        CZipperCommunicate.SendCamFPS(40);
                                        timeOutCount = 0;
                                        img.Dispose();
                                        return;
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
                                        AutoLogger.Info($"{cell.CamName}:onWichStage={onWichStage},maxtimeout超过5次，进入阶段4");
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                        LightChange.MaxTimeOutCount = 0;
                                        timeOutCount = 0;
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
                                        AutoLogger.Info($"{cell.CamName}:onWichStage=3,超过4次没变化,进入阶段4");
                                        addOrSubCount = 0;
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                        //LightChange.MaxTimeOutCount = 0;
                                        LightChange.MinTimeOutCount = 0;
                                        timeOutCount = 0;
                                        // CLinghtManagement.SaveLightParams();
                                        img.Dispose();
                                        return;
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
                                        AutoLogger.Info($"{cell.CamName}:onWichStage=3,maxtimeout超过5次，进入阶段4");
                                        //mintimeout = 0;
                                        //进入下阶段
                                        img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                        // LightChange.MaxTimeOutCount = 0;
                                        LightChange.MinTimeOutCount = 0;
                                        timeOutCount = 0;
                                    }
                                }
                                else
                                {
                                    AutoLogger.Info($"{cell.CamName}:onWichStage=3,光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                                    //进入下阶段
                                    // LightChange.LineValueReset();
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    AutoLogger.Info($"{cell.CamName}:onWichStage=3,进入阶段4");
                                    img?.ImWrite($"D:\\LightValueImages\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}-Pull-{LightChange.TempLightValue_Change1}.png");
                                    LightChange.MaxTimeOutCount = 0;
                                    LightChange.MinTimeOutCount = 0;
                                    timeOutCount = 0;
                                }
                            }
                        }
                    }

                }
                #endregion
            }
            orgimg.Dispose();
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
