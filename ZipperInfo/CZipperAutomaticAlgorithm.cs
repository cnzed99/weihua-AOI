using HalconDotNet;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using System.IO;
using System.Windows.Threading;
using WH.Entity;
using WH.Entity.LogRecord;
using WH.LightControl;
using WH.RunCell;
using ZipperLightHalconDet;
using WH.VisionLearning;
using System.Windows.Media;
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
        //YOLO yolo_search_det = new();
        IVisionModel yolo_search_det;

        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 识别拉头对象
        /// </summary>
        //YOLO yolo_search_det = new();
        IVisionModel yolo_Logo_Pull_det;

        /// <summary>
        /// 拉片分割模型
        /// </summary>
        IVisionModel yolo_PullShape_Seg;

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
        /// 超时统计
        /// </summary>
        int timeOutCount = 0;


        public static bool zuo_lightOK;

        public static bool you_lightOK;
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

        public static int tempLightValue_zuo_change1 = 0;
        public static int tempLightValue_zuo_change2 = 0;
        public static int tempLightValue_you_change1 = 0;
        public static int tempLightValue_you_change2 = 0;

        public static bool[] findLogosidertype = new bool[2];  //0拉头  1拉片

        LightChangeBase LightChange;

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
                    ZipperInfo.LogoTypeStrs = logostrs.ToArray();
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
            if (Searchmodelpath != "" && pullmodelpath != "" && pullSegmodelpath != "")
            {
                IniYolo(Searchmodelpath, pullmodelpath, pullSegmodelpath);
            }

            //if (CLinghtManagement.LightControlDict.Count >= 2)
            //{
            //    LightCtl_Zuo = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port?.Name == "COM1");
            //    LightCtl_You = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port?.Name == "COM2");
            //}

            LightChange = new COPTLinghtChange("COM1");

        }

        int addOrSubCount = 0;
        int nochangeCount = 0;
        int maxtimeout = 0;
        int mintimeout = 0;
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

            Mat img = new Mat();
            Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
            orgimg.Dispose();
            //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            ProgressBarViewModel.AutoMessage = "正在识别中...";
            if (onWichStage == 1)
            {
                if (cell.CamName == "右相机") //调光源只用一边的结果
                {
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

                    // HOperatorSet.ReadImage(out CameraImage,"C:\\Users\\Administrator.B\\Desktop\\新建文件夹\\124032_1_0_OK_OK_163858938_102.png");
                    ZipperLightHelper.Instance.ZipperLightDetection(CameraImage, 10, 0.7,
                        CZipperAutomaticAlgorithm.ZipperInfo.AutoData.ZipperMinBgMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.AutoData.ZipperMaxBgMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.AutoData.ZipperMinMean,
                        CZipperAutomaticAlgorithm.ZipperInfo.AutoData.ZipperMaxMean,
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
                    AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                    if (hv_VState == 1)
                    {
                        // Console.WriteLine($"需增加亮度");
                        if (tempVState != hv_VState)
                        {
                            addOrSubCount++;
                        }
                        if (addOrSubCount > 4)
                        {
                            AutoLogger.Info($"onWichStage=1,超过4次没变化,进入下一阶段");
                            //进入下阶段
                            CLinghtManagement.SaveLightParams();
                            addOrSubCount = 0;
                            timeOutCount = 0;
                            onWichStage = 2;
                            LightChange.MaxTimeOutCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            ProgressBarViewModel.ProgressBarValue = 40;
                            CZipperCommunicate.FirststageFinsh();
                            CZipperCommunicate.SendCamFPS(40);
                            img.Dispose();
                            return;
                        }
                        tempVState = hv_VState;
                        int val = hv_VStride.I;

                        if (val == 0)
                        {
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},推荐调整值:{val}");
                            val = 2;
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},修改调整值为:2");
                        }
                        LightChange.ChangeLineValue1(true, val);
                        if (LightChange.MaxTimeOutCount >= 5)
                        {
                            LightChange.MaxTimeOutCount = 0;
                            //进入下阶段
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:最大值130,进入下一阶段");
                            CLinghtManagement.SaveLightParams();
                            addOrSubCount = 0;
                            timeOutCount = 0;
                            onWichStage = 2;
                            LightChange.MaxTimeOutCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            ProgressBarViewModel.ProgressBarValue = 40;
                            CZipperCommunicate.FirststageFinsh();
                            CZipperCommunicate.SendCamFPS(40);
                            img.Dispose();
                            return;
                        }



                    }
                    else if (hv_VState == 2)
                    {
                        // Console.WriteLine($"需减少亮度");

                        if (tempVState != hv_VState)
                        {
                            addOrSubCount++;
                        }
                        if (addOrSubCount > 4)
                        {
                            AutoLogger.Info($"onWichStage=1,超过4次没变化,进入下一阶段");
                            //进入下阶段
                            CLinghtManagement.SaveLightParams();
                            addOrSubCount = 0;
                            timeOutCount = 0;
                            onWichStage = 2;
                            LightChange.MaxTimeOutCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            ProgressBarViewModel.ProgressBarValue = 40;
                            CZipperCommunicate.FirststageFinsh();
                            CZipperCommunicate.SendCamFPS(40);
                            img.Dispose();
                            return;
                        }
                        tempVState = hv_VState;
                        int val = hv_VStride.I;
                        if (val == 0)
                        {
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},推荐调整值:{val}");
                            val = 2;
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},修改调整值为:2");
                        }
                        LightChange.ChangeLineValue1(true, -val);
                        if (LightChange.MinTimeOutCount >= 5)
                        {
                            mintimeout = 0;
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:最小值20,进入下一阶段");
                            //进入下阶段
                            CLinghtManagement.SaveLightParams();
                            addOrSubCount = 0;
                            timeOutCount = 0;
                            onWichStage = 2;
                            LightChange.MaxTimeOutCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            ProgressBarViewModel.ProgressBarValue = 40;
                            CZipperCommunicate.FirststageFinsh();
                            CZipperCommunicate.SendCamFPS(40);
                            img.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        nochangeCount++;
                        // AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:{LightCtl_You.BaseConfig.LightChannelList[0].Value},无需调整次数{nochangeCount}");

                        // HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
                        if (nochangeCount >= 3)
                        {
                            nochangeCount = 0;
                            //进入下阶段
                            // AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:{LightCtl_You.BaseConfig.LightChannelList[0].Value},进入下一阶段");
                            // CLinghtManagement.SaveLightParams();
                            addOrSubCount = 0;
                            timeOutCount = 0;
                            onWichStage = 2;
                            ProgressBarViewModel.ProgressBarValue = 40;
                            LightChange.MaxTimeOutCount = 0;
                            LightChange.MinTimeOutCount = 0;
                            CZipperCommunicate.FirststageFinsh();
                            CZipperCommunicate.SendCamFPS(40);
                            img.Dispose();
                            return;
                        }

                    }
                }
            }
            else if (onWichStage == 2) //第二阶段:识别下止类型 上止 拉头
            {
                if (cell.CamName == "右相机")
                {
                    timeOutCount++;
                    DetResult resultDet;
                    resultDet = yolo_search_det.Predict(img) as DetResult;
                    AutoLogger.Info($"{cell.CamName}:onWichStage=2,timeOutCount={timeOutCount},识别到目标个数为:{resultDet.datas.Count}");
                    if (resultDet.datas.Count > 0)
                    {
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_search_names[nameindex];
                            if (labelstr.Contains("注塑下止") && !findDownMass)
                            {
                                findDownMassCount++;
                                if (findDownMassCount >= 2)
                                {
                                    findDownMass = true;
                                }
                                AutoLogger.Info($"{cell.CamName}:onWichStage=2,识别到注塑下止");
                                ZipperInfo.ZipperDownMassType = STOPMASS.注塑;
                                Dispatcher.Invoke(() =>
                                {
                                    AutoLogger.Info($"{cell.CamName}:onWichStage=2,更新下止图片");
                                    ZipperInfo.ZipperDownmssImg = cell.Image?.ToBitmapSource().Clone();
                                });
                                timeOutCount = 0;
                                //List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                                //{
                                //       new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                //       new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                //       new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //       new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //       new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                //};
                                //System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                                //cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                                //cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));

                                // 
                                // AutoLogger.Info($"{cell.CamName}:onWichStage=2,准备进入第二阶段");
                                //进入下阶段                       
                                // CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                                //  onWichStage = 3;
                                //  return;
                            }

                            if (labelstr.Contains("注塑上止") && !findUpMass)
                            {
                                findUpMassCount++;
                                if (findUpMassCount >= 2)
                                {
                                    findUpMass = true;
                                }
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},识别到{labelstr}");

                                ZipperInfo.ZipperUpMassType = STOPMASS.注塑;
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},设置拉链上止为:{ZipperInfo.ZipperUpMassType}");
                                Dispatcher.Invoke(() =>
                                {
                                    ZipperInfo.ZipperUpmssImg = cell.Image?.ToBitmapSource().Clone();
                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},更新上止图片");
                                });

                                //List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                                //{
                                //    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                //    new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                //    new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y)

                                //};
                                //System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                                //cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                                //cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));

                                timeOutCount = 0;
                                //TestFinsh = true;
                                ////结束
                                //AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},完成识别");
                                //CZipperCommunicate.TestFinish();
                                //Thread.Sleep(10);
                                //CZipperCommunicate.ThirdstageFinsh();
                                //// TestFinshEven?.Invoke(TestFinsh);
                                //return;
                            }

                            if (labelstr.Contains("链牙") && !findlianya)
                            {
                                //这里要改成识别链牙在哪边
                                findlianya = true;
                                ZipperInfo.ZipperSliderType = PULLTYPE.反穿;
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},设置拉链为反穿");

                            }

                            if (labelstr.Contains("拉头") && !findPuller)
                            {
                                // ProgressBarViewModel.AutoMessage = "正在寻找拉头位置...";
                                ProgressBarViewModel.ProgressBarValue = 50;
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},识别到拉头,拉头图像位置X:{resultDet.datas[i].box.X},拉头离图像边缘距离:{cell.Image?.ImageWidth - resultDet.datas[i].box.X}");
                                int centerx = resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2;
                                int centery = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2;
                                //List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                                // {
                                //        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                //        new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                //        new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                //        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y)
                                // };
                                //System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                                //cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                                //cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));
                                // int eiddis = 700;

                                if (resultDet.datas[i].box.X > 150 && (cell.Image.ImageWidth - resultDet.datas[i].box.X) > 850) //
                                {
                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},识别到拉头,拉头离图像边缘距离:{resultDet.datas[i].box.X} > 150 && {(cell.Image.ImageWidth - resultDet.datas[i].box.X)} > 850");
                                    float pos = CZipperCommunicate.GetGrippawlLocation();
                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},获取当前机械轴位置:{pos}");
                                    if (pos < 1)
                                    {
                                        AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},获取当前机械轴位置:{pos},小于1.return");
                                        return;
                                    }

                                    // pos = pos - 25; //因为有延迟,实际位置比读取的位置有偏差,顾减去25 经验值
                                    //AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},获取当前机械轴-25位置:{pos}");
                                    List<float> templist = new List<float>();
                                    for (int j = 0; j < ZipperInfo.ZipperTriggerPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.ZipperTriggerPos[j];
                                        templist.Add(temppos);

                                    }
                                    //List<int> temphandP = new List<int>();
                                    //for (int j = 0; j < ZipperInfo.HandAndTaliPos.Count; j++)
                                    //{
                                    //    int temppos = (int)ZipperInfo.HandAndTaliPos[j] * 10;
                                    //    temphandP.Add(temppos);

                                    //}
                                    //AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},起点下限位置:{temphandP[0]}");
                                    //AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},终点上限位置:{temphandP[temphandP.Count - 1]}");
                                    //if (pos > temphandP[temphandP.Count - 1] || pos < temphandP[0])
                                    //{
                                    //    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},机械轴位置:{pos}>{temphandP[temphandP.Count - 1]},{pos} < {temphandP[0]},retrun");
                                    //    return;
                                    //}
                                    if (templist.Count > 0)
                                    {
                                        // AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},满足条件{pos} > {temphandP[temphandP.Count - 1]} || {pos} < {temphandP[0]}");
                                        //int crippoint = (int)ZipperInfo.ZipperLneght * 10;
                                        //if (pos > templist[templist.Count - 1] && pos > crippoint) //如果超过了这个临界点,说明拉头在下一次拉取的图片中
                                        //{
                                        //    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},机械轴位置:{pos}大于临界点{templist[templist.Count - 1]},{pos}大于临界点{crippoint}");
                                        //    pos = pos - crippoint;
                                        //    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},机械轴位置:减去一个拉链长度,轴坐标为:{pos}");
                                        //}

                                        templist.Add(pos);
                                        templist.Sort(); //升序排序
                                        int pindex = templist.IndexOf(pos);
                                        AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},排序,总拍照次数为:{templist.Count},拉头序号是第{pindex + 1}张图片");
                                        int rang = 10;
                                        if (templist.Count >= 3)
                                        {

                                            if (pindex == 0)
                                            {
                                                if (ZipperInfo.TriggerType == 3)
                                                {
                                                    float dis = Math.Abs(pos - templist[pindex + 1]);
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},pindex={pindex},{dis}>{rang}");
                                                    if (dis > rang)
                                                    {
                                                        AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{pos} - {templist[pindex + 1]}>{rang},停止轴运动,进入下一级段");
                                                        CZipperCommunicate.AixtStop();
                                                        CZipperCommunicate.SendCamFPS(300);
                                                        timeOutCount = 0;
                                                        onWichStage = 3;
                                                        img.Dispose();
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},pindex={pindex},类型{ZipperInfo.TriggerType}拉头位置不能是第一个， return");
                                                    return; //不能在第一位
                                                }
                                            }
                                            else if (pindex == templist.Count - 1)
                                            {
                                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},拉链触发点位类型是类{ZipperInfo.TriggerType}");
                                                if (ZipperInfo.TriggerType == 3)
                                                {
                                                    float dis = Math.Abs(pos - templist[pindex - 1]);
                                                    if (dis > rang)
                                                    {
                                                        AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{pos} - {templist[pindex - 1]}>{rang},停止轴运动,进入下一级段");
                                                        CZipperCommunicate.AixtStop();
                                                        CZipperCommunicate.SendCamFPS(300);
                                                        timeOutCount = 0;
                                                        onWichStage = 3;
                                                        img.Dispose();
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},pindex={pindex},类型{ZipperInfo.TriggerType}拉头位置不能是最后一个， return");
                                                    img.Dispose();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                float dis = Math.Abs(pos - templist[pindex - 1]);
                                                float dis2 = Math.Abs(pos - templist[pindex + 1]);
                                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{dis}>{rang} &&{dis2}>{rang}");
                                                if (dis > rang && dis2 > rang) //大于15mm
                                                {
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{pos} - {templist[pindex - 1]}>{rang} &&{pos} - {templist[pindex + 1]}>{rang}");
                                                    //写轴坐标位置
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{templist[pindex - 1]}<{pos}<{templist[pindex + 1]}停止轴运动,进入下一级段");
                                                    CZipperCommunicate.AixtStop();
                                                    CZipperCommunicate.SendCamFPS(300);
                                                    timeOutCount = 0;
                                                    onWichStage = 3;
                                                    img.Dispose();
                                                    return;
                                                }
                                            }
                                        }
                                        else if (templist.Count == 2)
                                        {
                                            if (pindex == 0)
                                            {
                                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},pindex={pindex},templist.Count==2，类型{ZipperInfo.TriggerType}拉头位置不能是第一个， return");
                                                return; //不能排在第一位
                                            }
                                            else
                                            {
                                                float dis = Math.Abs(templist[1] - templist[0]);
                                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},{dis}>{rang}");
                                                if (dis > rang) //大于7.5mm
                                                {

                                                    //写轴坐标位置
                                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},templist.Count == 2,{templist[0]}<{templist[1]},停止轴运动,进入下一级段");
                                                    CZipperCommunicate.AixtStop();
                                                    CZipperCommunicate.SendCamFPS(300);
                                                    timeOutCount = 0;
                                                    onWichStage = 3;
                                                    img.Dispose();
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},findUpMass={findUpMass}");
                            // 40ms拍一张照片，轴移动1mm  超时次数可以用拉链长度来替代
                            if (timeOutCount > ZipperInfo.ZipperLneght && !findUpMass)
                            {
                                findUpMass = true;
                                ZipperInfo.ZipperUpMassType = STOPMASS.无;
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},ZipperInfo.ZipperUpMassType=STOPMASS.无");
                            }
                            AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},findDownMass={findDownMass}");
                            // 40ms拍一张照片，轴移动1mm  超时次数可以用拉链长度来替代
                            if (timeOutCount > ZipperInfo.ZipperLneght && !findDownMass)
                            {
                                findDownMass = true;
                                ZipperInfo.ZipperDownMassType = STOPMASS.无;
                                AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},ZipperInfo.ZipperDownMassType=STOPMASS.无");
                            }

                            if (findDownMass && findUpMass && findPulls && findPuller && !TestFinsh)
                            {

                                if (!findlianya)
                                {
                                    ZipperInfo.ZipperSliderType = PULLTYPE.正穿;
                                    AutoLogger.Info($"onWichStage=2,timeOutCount={timeOutCount},设置拉链为正穿");
                                }
                                if (findLogosidertype[0] && findLogosidertype[1]) //两面都找到logo
                                {
                                    ZipperInfo.FindLogoSider = 2;
                                    AutoLogger.Info($"onWichStage=2,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},两面都有logo");
                                }
                                else if (!findLogosidertype[0] && findLogosidertype[1]) //拉片面找到logo
                                {
                                    ZipperInfo.FindLogoSider = 1;
                                    AutoLogger.Info($"onWichStage=2,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},拉片面找到logo");
                                }
                                else if (findLogosidertype[0] && !findLogosidertype[1]) //拉头面找到logo
                                {
                                    ZipperInfo.FindLogoSider = 3;
                                    AutoLogger.Info($"onWichStage=2,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},拉头面找到logo");
                                }
                                else
                                {
                                    ZipperInfo.FindLogoSider = 0;  //两面都没找到logo
                                    AutoLogger.Info($"onWichStage=2,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},两面都没有logo");
                                }
                                ProgressBarViewModel.AutoMessage = "识别拉链完成...";
                                ProgressBarViewModel.ProgressBarValue = 100;
                                Thread.Sleep(500);
                                timeOutCount = 0;
                                onWichStage = 0;
                                CZipperCommunicate.AixtStop();//停止轴
                                CZipperCommunicate.CamTriggerStop(); //停止拍照
                                LightChange.LineValueReset();
                                // CLinghtManagement.SaveLightParams();
                                //Dispatcher.Invoke(() =>
                                //{
                                TestFinsh = true;
                                ProgressBarViewModel.ProgressFinshEven?.Invoke();
                                // });
                                // CZipperAutomaticAlgorithm.TestFinshEven(true);
                                AutoLogger.Info($"{cell.CamName}:onWichStage=2,timeOutCount={timeOutCount},上下止,拉头,拉头拉片,Logo全部识别到,结束");
                            }

                        }

                    }
                }
            }
            else if (onWichStage == 3) ////第三阶段 识别拉头 计算拉头亮度,设置光源值
            {
                #region 只测右相机
                if (cell.CamName == "右相机")
                {

                    // ProgressBarViewModel.AutoMessage = "正在调整拉头亮度...";
                    timeOutCount++;
                    if (timeOutCount >= 10)
                    {
                        findPuller = false;
                        onWichStage = 2;
                        CZipperCommunicate.AixtContinue(true);
                        CZipperCommunicate.SendCamFPS(40);
                        AutoLogger.Info($"onWichStage=3,超过10次没有找到拉头，重新跳转到阶段2");
                        img.Dispose();
                        return;
                    }
                    DetResult resultDet;
                    resultDet = yolo_search_det.Predict(img) as DetResult;
                    if (resultDet.datas.Count > 0)
                    {
                        AutoLogger.Info($"onWichStage=3,开始识别拉头");
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_search_names[nameindex];
                            if (labelstr.Contains("拉头"))
                            {
                                timeOutCount = 0;
                                ProgressBarViewModel.ProgressBarValue = 60;
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

                                int bx = resultDet.datas[i].box.X;
                                int by = resultDet.datas[i].box.Y;
                                int w = resultDet.datas[i].box.Width;
                                int h = resultDet.datas[i].box.Height;
                                //  Mat cutmat = img[new Rect(bx, by, w, h)];
                                // HOperatorSet.GenRectangle1(out HObject rec1, by, bx, by + h, bx + w);
                                HOperatorSet.CropRectangle1(CameraImage, out HObject cutimg, by, bx, by + h, bx + w);
                                //  HOperatorSet.WriteImage(cutimg, "png", 0, "C:\\Users\\Administrator\\Desktop\\新建文件夹 (4)\\111.png");
                                ZipperLightHelper.Instance.PullerLightDetection(cutimg, 10, 0.7,
                                    CZipperAutomaticAlgorithm.ZipperInfo.AutoData.PullMinBgMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.AutoData.PullMaxBgMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.AutoData.PullMinMean,
                                    CZipperAutomaticAlgorithm.ZipperInfo.AutoData.PullMaxMean,
                                    out var hv_VState, out var hv_VStride);
                                AutoLogger.Info($"onWichStage=3,光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                                CameraImage.Dispose();
                                cutimg.Dispose();
                                // CLightControlBase cLightControl = null;
                                //  cLightControl = LightCtl_You;
                                if (hv_VState == 1)
                                {
                                    // Console.WriteLine($"需增加亮度");

                                    if (tempVState != hv_VState)
                                    {
                                        addOrSubCount++;
                                    }
                                    if (addOrSubCount > 4)
                                    {
                                        AutoLogger.Info($"onWichStage=3,超过4次没变化,进入阶段4");
                                        addOrSubCount = 0;
                                        maxtimeout = 0;
                                        //进入下阶段
                                        onWichStage = 4;
                                        LightChange.MaxTimeOutCount = 0;
                                        CZipperCommunicate.SendCamFPS(40);
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                        img.Dispose();
                                        return;
                                    }
                                    tempVState = hv_VState;

                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    LightChange.ChangeLineValue1(false, val);
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    if (LightChange.MaxTimeOutCount >= 5)
                                    {
                                        AutoLogger.Info($"onWichStage=3,maxtimeout超过5次，进入阶段4");
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        onWichStage = 4;
                                        LightChange.MaxTimeOutCount = 0;
                                        //LightChange.MinTimeOutCount = 0;
                                        CZipperCommunicate.SendCamFPS(40);
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                    }


                                }
                                else if (hv_VState == 2)
                                {
                                    // Console.WriteLine($"需减少亮度");
                                    if (tempVState != hv_VState)
                                    {
                                        addOrSubCount++;
                                    }
                                    if (addOrSubCount > 4)
                                    {
                                        AutoLogger.Info($"onWichStage=3,超过4次没变化,进入阶段4");
                                        addOrSubCount = 0;
                                        //maxtimeout = 0;
                                        //进入下阶段
                                        onWichStage = 4;
                                        //LightChange.MaxTimeOutCount = 0;
                                        LightChange.MinTimeOutCount = 0;
                                        CZipperCommunicate.SendCamFPS(40);
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                        img.Dispose();
                                        return;
                                    }
                                    tempVState = hv_VState;

                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    LightChange.ChangeLineValue1(false, -val);
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    if (LightChange.MinTimeOutCount >= 5)
                                    {
                                        AutoLogger.Info($"onWichStage=3,maxtimeout超过5次，进入阶段4");
                                        //mintimeout = 0;
                                        //进入下阶段                                         
                                        onWichStage = 4;
                                        // LightChange.MaxTimeOutCount = 0;
                                        LightChange.MinTimeOutCount = 0;
                                        CZipperCommunicate.SendCamFPS(40);
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                    }


                                }
                                else
                                {
                                    AutoLogger.Info($"onWichStage=3,光源调整hv_VState={hv_VState.I},推荐调整值:{hv_VStride.I}");
                                    //进入下阶段
                                    // LightChange.LineValueReset();
                                    // AutoLogger.Info($"onWichStage=3,设置光源值为{LightCtl_You.BaseConfig.LightChannelList[0].Value}");
                                    AutoLogger.Info($"onWichStage=3,进入阶段4");
                                    onWichStage = 4;
                                    LightChange.MaxTimeOutCount = 0;
                                    LightChange.MinTimeOutCount = 0;
                                    CZipperCommunicate.SendCamFPS(40);
                                    timeOutCount = 0;
                                    CLinghtManagement.SaveLightParams();
                                    // CZipperCommunicate.SceondstageFinsh();
                                }
                            }
                        }
                    }

                }
                #endregion
            }
            else if (onWichStage == 4) ////第二阶段 识别拉头,拉头拉片,LOGO类型
            {
                //  ProgressBarViewModel.AutoMessage = "正在识别拉片 LOGO...";
                ProgressBarViewModel.ProgressBarValue = 80;
                timeOutCount++;
                DetResult resultDet;
                resultDet = yolo_search_det.Predict(img) as DetResult;
                AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到目标个数为:{resultDet.datas.Count}");
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_search_names[nameindex];
                        if (labelstr.Contains("拉头") && !findPuller)
                        {
                            if (labelstr.Contains("金属"))
                            {
                                ZipperInfo.PullMaterlsType = PULLMATERIALSTYPE.金属;
                                ProgressBarViewModel.ProgressBarValue = 82;
                            }
                            else
                            {
                                ZipperInfo.PullMaterlsType = PULLMATERIALSTYPE.烤漆;
                                ProgressBarViewModel.ProgressBarValue = 82;
                            }
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到拉头findPuller = true");
                            timeOutCount = 0;
                            findPullerCount++;

                            float pos = CZipperCommunicate.GetGrippawlLocation();
                            int crippoint = (int)ZipperInfo.ZipperLneght;
                            if (pos > crippoint) //如果超过了这个临界点,说明拉头在下一次拉取的图片中
                            {
                                AutoLogger.Info($"onWichStage=4,timeOutCount={timeOutCount},机械轴位置:{pos}大于临界点{crippoint}");
                                pos = pos - crippoint;
                                AutoLogger.Info($"onWichStage=4,timeOutCount={timeOutCount},机械轴位置:减去一个拉链长度,轴坐标为:{pos}");
                            }
                            CZipperCommunicate.SendPullLocation(pos);
                            ZipperInfo.ZipperPullerCX = resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2;
                            ZipperInfo.ZipperPullerCY = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2;

                            AutoLogger.Info($"onWichStage=4,timeOutCount={timeOutCount},想PLC写入拉头位置:{pos}");
                            Dispatcher.Invoke(() =>
                            {
                                ZipperInfo.ZipperPullerImg = cell.Image.ToBitmapSource().Clone();

                            });
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到拉头,当前轴停止位置:{pos},写入位置{pos},findPuller=true,更新拉头图片");
                            int lx = resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2 - 400;
                            int ly = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2 - 320;
                            int recw = 800;
                            int rech = 640;

                            if ((lx + recw) > img.Width)
                            {
                                lx = img.Width - recw;
                            }
                            if (lx < 0)
                            {
                                lx = 0;
                            }

                            if ((ly + rech) > img.Height)
                            {
                                ly = img.Height - rech;
                            }
                            if (ly < 0)
                            {
                                ly = 0;
                            }

                            Mat croppullMat = img[new Rect(lx, ly, recw, rech)];

                            DetResult pullResult = yolo_Logo_Pull_det.Predict(croppullMat) as DetResult;
                            for (int j = 0; j < pullResult.count; j++)
                            {
                                int pulllabelindex = int.Parse(pullResult[j].lable);
                                string pullabelname = de_Logo_pull_names[pulllabelindex];
                                if (!pullabelname.Contains("拉"))
                                {
                                    ProgressBarViewModel.ProgressBarValue = 85;
                                    findLogo = true;
                                    findLogosidertype[0] = true; //找到拉头上的logo
                                    ZipperInfo.ZipperLogoType = pullabelname;
                                    AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},在拉头侧识别到Logo:{ZipperInfo.ZipperLogoType},findLogo=true更新Logo图片");

                                }
                            }
                            int px = 0, py = 0;
                            if (ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                            {
                                px = resultDet.datas[i].box.X + 180;
                                py = resultDet.datas[i].box.Y + 60;
                            }
                            else
                            {
                                px = resultDet.datas[i].box.X + 123;
                                py = resultDet.datas[i].box.Y + 30;

                            }
                            int rew = 15;
                            int reh = 20;
                            Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                            // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                            Mat hsvImage = new Mat();
                            Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                            Scalar hsvMean = Cv2.Mean(hsvImage);
                            // HSV通道说明：
                            // H: 0-179 (色调)
                            // S: 0-255 (饱和度)
                            // V: 0-255 (明度)
                            ZipperInfo.PullerMeanH = Math.Round(hsvMean.Val0, 2);
                            ZipperInfo.PullerMeanS = Math.Round(hsvMean.Val1, 2);
                            // double vMean = hsvMean.Val2;
                            hsvImage.Dispose();

                            if (findPullerCount >= 3)
                            {
                                ProgressBarViewModel.ProgressBarValue = 87;
                                findPuller = true;
                            }
                        }
                        if (labelstr.Contains("拉片") && !findPulls)
                        {

                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到拉头拉片findPulls = true");
                            timeOutCount = 0;
                            findPullsCount++;
                            Dispatcher.Invoke(() =>
                            {
                                ZipperInfo.ZipperPullsImg = cell.Image.ToBitmapSource().Clone();
                            });
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到拉头拉片,findPulls=true更新拉片图片");

                            int lx = resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2 - 400;
                            int ly = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2 - 320;
                            int recw = 800;
                            int rech = 640;

                            if ((lx + recw) > img.Width)
                            {
                                lx = img.Width - recw;
                            }
                            if (lx < 0)
                            {
                                lx = 0;
                            }

                            if ((ly + rech) > img.Height)
                            {
                                ly = img.Height - rech;
                            }
                            if (ly < 0)
                            {
                                ly = 0;
                            }

                            Mat croppullMat = img[new Rect(lx, ly, recw, rech)];

                            DetResult pullResult = yolo_Logo_Pull_det.Predict(croppullMat) as DetResult;
                            for (int j = 0; j < pullResult.count; j++)
                            {
                                int pulllabelindex = int.Parse(pullResult[j].lable);
                                string pullabelname = de_Logo_pull_names[pulllabelindex];
                                if (!pullabelname.Contains("拉"))
                                {
                                    findLogo = true;
                                    findLogosidertype[1] = true; //找到拉片上的logo
                                    ZipperInfo.ZipperLogoType = pullabelname;
                                    AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},在拉片侧识别到Logo:{ZipperInfo.ZipperLogoType},findLogo=true更新Logo图片");

                                }
                            }
                            //int px = resultDet.datas[i].box.X + 30;
                            //int py = resultDet.datas[i].box.Y + 50;
                            //int rew = 140;
                            //int reh = 70;
                            int cx = (resultDet.datas[i].box.X + resultDet.datas[i].box.Right) / 2;
                            int cy = (resultDet.datas[i].box.Y + resultDet.datas[i].box.Bottom) / 2;

                            int rew = 80;
                            int reh = 30;
                            int px = cx + 30;
                            int py = cy - reh / 2;
                            Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                            // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (22)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                            Mat hsvImage = new Mat();
                            Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                            Scalar hsvMean = Cv2.Mean(hsvImage);
                            // HSV通道说明：
                            // H: 0-179 (色调)
                            // S: 0-255 (饱和度)
                            // V: 0-255 (明度)
                            ZipperInfo.PullsMeanH = Math.Round(hsvMean.Val0, 2);
                            ZipperInfo.PullsMeanS = Math.Round(hsvMean.Val1, 2);
                            // double vMean = hsvMean.Val2;
                            hsvImage.Dispose();
                            if (findPullsCount >= 3)
                            {
                                SegResult pullsegResult = yolo_PullShape_Seg.Predict(croppullMat) as SegResult;

                                if (pullsegResult == null || pullsegResult.datas.Count == 0) return;

                                List<Point[]> contoursList = new List<Point[]>();
                                //double allperimeter = 0; //周长总长
                                //double allarea = 0; //总面积
                                foreach (var seg in pullsegResult.datas)
                                // if (pullsegResult.count > 0)
                                {
                                    //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (2)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", seg.mask);
                                    Mat maskgray = new Mat();
                                    Cv2.CvtColor(seg.mask, maskgray, ColorConversionCodes.BGR2GRAY);
                                    Mat binary = new Mat();
                                    Cv2.Threshold(maskgray, binary, 10, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                                    Point[][] contours;
                                    HierarchyIndex[] hierarchy;
                                    Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                                    maskgray.Dispose();
                                    binary.Dispose();

                                    Point[] maxps = contours?.MaxBy(p => p.Length);
                                    if (maxps != null)
                                    {
                                        contoursList.Add(maxps);
                                    }

                                }
                                // ZipperInfo.PullSegOrgArea = allarea;

                                Point[] maxPointsContour = contoursList.OrderByDescending(contour => contour.Length).First();
                                ZipperInfo.OrgContours = maxPointsContour;
                                findPulls = true;
                                ProgressBarViewModel.ProgressBarValue = 89;
                            }
                        }

                    }
                    if (findPuller && findPulls)
                    {
                        if (findLogo == false)
                        {
                            ZipperInfo.ZipperLogoType = "无LOGO";
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},识别到Logo:无Logo,findLogo=true更新Logo图片");
                        }
                        if (!findDownMass || !findUpMass)
                        {
                            //  ProgressBarViewModel.AutoMessage = "正在识别上下止...";
                            ProgressBarViewModel.ProgressBarValue = 90;
                            LightChange.LineValueReset();
                            CLinghtManagement.SaveLightParams();
                            timeOutCount = 0;
                            onWichStage = 2;
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},没有识别到上下止,转到阶段2");
                            CZipperCommunicate.AixtContinue(true);//继续
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},轴继续拉动,转到阶段2");
                        }

                        if (findDownMass && findUpMass && !TestFinsh) //如果都找到了下止 上止  拉头 拉片就结束
                        {
                            if (!findlianya)
                            {
                                ZipperInfo.ZipperSliderType = PULLTYPE.正穿;
                                AutoLogger.Info($"onWichStage=4,timeOutCount={timeOutCount},设置拉链为正穿");
                            }
                            if (findLogosidertype[0] && findLogosidertype[1]) //两面都找到logo
                            {
                                ZipperInfo.FindLogoSider = 2;
                                AutoLogger.Info($"onWichStage=4,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},两面都有logo");
                            }
                            else if (!findLogosidertype[0] && findLogosidertype[1]) //拉片面找到logo
                            {
                                ZipperInfo.FindLogoSider = 1;
                                AutoLogger.Info($"onWichStage=4,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},拉片面找到logo");
                            }
                            else if (findLogosidertype[0] && !findLogosidertype[1]) //拉头面找到logo
                            {
                                ZipperInfo.FindLogoSider = 3;
                                AutoLogger.Info($"onWichStage=4,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},拉头面找到logo");
                            }
                            else
                            {
                                ZipperInfo.FindLogoSider = 0;  //两面都没找到logo
                                AutoLogger.Info($"onWichStage=4,ZipperInfo.FindLogoSider={ZipperInfo.FindLogoSider},两面都没有logo");
                            }
                            ProgressBarViewModel.AutoMessage = "识别拉链完成...";
                            ProgressBarViewModel.ProgressBarValue = 100;
                            Thread.Sleep(500);
                            timeOutCount = 0;
                            onWichStage = 0;
                            CZipperCommunicate.AixtStop();//停止轴
                            CZipperCommunicate.CamTriggerStop();
                            LightChange.LineValueReset();
                            CLinghtManagement.SaveLightParams();
                            //Dispatcher.Invoke(() =>
                            //{
                            TestFinsh = true;
                            ProgressBarViewModel.ProgressFinshEven?.Invoke();
                            // });
                            AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},上下止,拉头,拉头拉片,Logo全部识别到,结束");
                        }


                    }
                    if (timeOutCount > 100)
                    {
                        AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},超时没有找到拉头或拉片,转到阶段2");
                        findPuller = true;
                        findPulls = true;
                        // ProgressBarViewModel.AutoMessage = "正在识别上下止...";
                        ProgressBarViewModel.ProgressBarValue = 85;
                        LightChange.LineValueReset();
                        CLinghtManagement.SaveLightParams();
                        timeOutCount = 0;
                        onWichStage = 2;
                        CZipperCommunicate.AixtContinue(true);//继续
                        AutoLogger.Info($"{cell.CamName}:onWichStage=4,timeOutCount={timeOutCount},轴继续拉动,转到阶段2");

                    }

                }
            }
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
        public void AutoSettingTriggerPos(Cell cell)
        {
            if (cell == null) return;
            Mat orgimg = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
          MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
          cell.Image.ImageData);
            Mat img = new Mat();
            Cv2.CvtColor(orgimg, img, ColorConversionCodes.BGR2RGB);
            orgimg.Dispose();
            if (onWichStage2 == 1)
            {
                if (cell.PhotoIndex == 1) //拉链下止
                {
                    if (cell.CamName == "右相机")
                    {
                        DetResult resultDet;
                        resultDet = yolo_search_det.Predict(img) as DetResult;
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
                                            float zipperlenght = ZipperInfo.AutoData.ZipperLenght;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：当前拉链长度为：{zipperlenght}");
                                            zipperlenght = zipperlenght - 0.5f;
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：设置拉链长度为：{zipperlenght}");
                                            ChangePoints(zipperlenght);

                                        }
                                        else if (ave > 350)
                                        {
                                            float zipperlenght = ZipperInfo.AutoData.ZipperLenght;
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
                    if (cell.CamName == "右相机")
                    {
                        DetResult resultDet;
                        resultDet = yolo_search_det.Predict(img) as DetResult;
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
                                        if (ave > cell.Image.ImageWidth - 240)
                                        {
                                            EndPosTemp++;
                                            if (EndPosTemp <= 0) { EndPosTemp = 1; }
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：第一个点位设置为：{EndPosTemp}");
                                            ChangePoints2(ZipperInfo.AutoData.ZipperLenght, EndPosTemp);
                                        }
                                        else if (ave < cell.Image.ImageWidth - 380)
                                        {

                                            EndPosTemp--;
                                            if (EndPosTemp <= 0) { EndPosTemp = 1; }
                                            AutoLogger.Info($"{cell.CamName}:自动调整位置：第一个点位设置为：{EndPosTemp}");
                                            ChangePoints2(ZipperInfo.AutoData.ZipperLenght, EndPosTemp);
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
            }

        }

        private void ChangePoints(float zipperlenght)
        {
            ZipperInfo.AutoData.ZipperLenght = zipperlenght;
            CGetZipperTriggerPoint.GetTriggerPoints(ZipperInfo.AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount, out _);
            CGetZipperTriggerPoint.CheckPullPos(points);
            CZipperCommunicate.SendZipperLenght(zipperlenght, ZipperInfo.AutoData.QuekouLenght);
            //写入拍照的总图片数量
            CZipperCommunicate.SendPhotoCount(points.Count);
            //计算拉链触发点位 ID改变位置
            CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);
        }

        private void ChangePoints2(float zipperlenght, float upchangevalue)
        {
            ZipperInfo.AutoData.ZipperLenght = zipperlenght;
            CGetZipperTriggerPoint.GetTriggerPoints(ZipperInfo.AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount, out int triggerType);
            CZipperCommunicate.SendZipperLenght(zipperlenght, ZipperInfo.AutoData.QuekouLenght);
            if (handandtalipoints.Count > 1)
            {
                if (triggerType == 3)
                {

                    int firstIndex = points.IndexOf(handandtalipoints[0]);
                    int endposIndex = firstIndex - 1;
                    if (endposIndex >= 0)
                    {
                        points[endposIndex] = upchangevalue;
                    }
                }
                else
                {
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
        private void IniYolo(string searchmodelpath, string pullmodelpath, string pullSegmodelpath)
        {
            if (!File.Exists(searchmodelpath) && !File.Exists(pullmodelpath) && !File.Exists(pullSegmodelpath))
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
            yolo_search_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, searchmodelpath, engineType,
          CurrentDevice, search_Categ_num, Score, Nms, 480);
            //  });

            //Task task1 = Task.Run(() =>
            //{
            int pull_Categ_num = de_Logo_pull_names.Length;
            float pullScore = 0.6f;
            float pullNms = 0.8f;
            yolo_Logo_Pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pullmodelpath, engineType,
          CurrentDevice, pull_Categ_num, pullScore, pullNms, 640);
            // });

            int pullSeg_Categ_num = pullSharp_names.Length;
            float segScore = 0.6f;
            float segNms = 0.5f;
            yolo_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSegmodelpath, engineType,
          CurrentDevice, pullSeg_Categ_num, segScore, segNms, 640);
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
