using HalconDotNet;
using OpenCvSharp;
using OpenCvSharp.ML;
using OpenVinoSharp.Extensions.result;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using WH.Entity;
using WH.Entity.LogRecord;
using WH.LightControl;
using WH.RecipeCellRootBase;
using WH.RunCell;
using ZipperLightHalconDet;
using WH.VisionLearning;

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
        /// 识别名
        /// </summary>
        string[] de_names;
        /// <summary>
        /// 超时统计
        /// </summary>
        int timeOutCount = 0;


        private static bool zuo_lightOK;

        private static bool you_lightOK;
        /// <summary>
        /// 2025.7.2 鲍赞宝
        /// 自动识别模块日志
        /// </summary>
        public static CLogRec AutoLogger { get; set; } = CLogRec.Create("Auto", "D:/Data");

        CLightControlBase LightCtl_Zuo = null;
        CLightControlBase LightCtl_You = null;

        public static bool findPulls = false;//检测到拉片
        public static bool findPuller = false; //检测到拉头
        public static bool findLogo = false; //检测到Logo

        public static Action<bool> TestFinshEven;
        public CZipperAutomaticAlgorithm()
        {
            string modelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\AutoMatic";
            string txtpath = modelDirPath + "\\classes.txt";
            //string modelpath = modelDirPath + "\\lalianAuto.onnx";
            string modelpath = modelDirPath + "\\lalianAuto.model";
            if (File.Exists(txtpath))
            {
                de_names = File.ReadAllLines(txtpath);
                IniYolo(modelpath);
            }
            if (CLinghtManagement.LightControlDict.Count >= 2)
            {
                LightCtl_Zuo = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port.Name == "COM1");
                LightCtl_You = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Port.Name == "COM2");
            }
        }

        int addOrSubCount = 0;
        int maxtimeout = 0;
        int mintimeout = 0;
        int tempVState = 0;
        HObject CameraImage = new HObject();
        public void ZipperAutomaticAlgorithmRun(Cell cell)
        {
            //第一阶段: 计算光源值
            Mat img = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
                 MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                 cell.Image.ImageData);
            //Mat img = new Mat();
            //Cv2.CvtColor(mat, img, ColorConversionCodes.BGR2RGB);
            // img.ImWrite($"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            //相机采集图片

            //  HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
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
                    ZipperLightHelper.Instance.ZipperLightDetection(CameraImage, 10, 2.0, out var hv_VState, out var hv_VStride);
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
                            onWichStage = 2;                        
                            return;
                        }
                        tempVState = hv_VState;

                        if (LightCtl_Zuo != null && LightCtl_You != null)
                        {
                            int val = hv_VStride.I;
                            if (val == 0)
                            {
                                AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},推荐调整值:{val}");
                                val = 2;
                                AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},修改调整值为:2");
                            }
                            LightCtl_Zuo.BaseConfig.LightChannelList[0].Value += val;
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value > 200)
                            {
                                LightCtl_Zuo.BaseConfig.LightChannelList[0].Value = 200;
                                maxtimeout++;
                            }
                           
                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[0]);
                            Thread.Sleep(10);
                            LightCtl_Zuo.BaseConfig.LightChannelList[1].Value= LightCtl_Zuo.BaseConfig.LightChannelList[0].Value;
                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[1]);
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前设置光源值为:{LightCtl_Zuo.BaseConfig.LightChannelList[0].Value}");

                            Thread.Sleep(20);
                            LightCtl_You.BaseConfig.LightChannelList[0].Value += val;
                            if (LightCtl_You.BaseConfig.LightChannelList[0].Value > 200)
                            {
                                LightCtl_You.BaseConfig.LightChannelList[0].Value = 200;
                            }
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[0]);
                            Thread.Sleep(10);
                            LightCtl_You.BaseConfig.LightChannelList[1].Value = LightCtl_You.BaseConfig.LightChannelList[0].Value;
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[1]);
                            if (maxtimeout >= 5)
                            {
                                maxtimeout = 0;
                                //进入下阶段
                                AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:最大值200,进入下一阶段");
                                CLinghtManagement.SaveLightParams();
                                addOrSubCount = 0;
                                onWichStage = 2;
                                return;
                            }

                        }

                    }
                    else if (hv_VState == 2)
                    {
                        Console.WriteLine($"需减少亮度");
                        if (LightCtl_Zuo != null && LightCtl_You != null)
                        {
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
                                onWichStage = 2;
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
                            LightCtl_Zuo.BaseConfig.LightChannelList[0].Value -= val;
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value < 5)
                            {
                                LightCtl_Zuo.BaseConfig.LightChannelList[0].Value = 5;
                                mintimeout++;
                            }

                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[0]);
                            Thread.Sleep(10);
                            LightCtl_Zuo.BaseConfig.LightChannelList[1].Value= LightCtl_Zuo.BaseConfig.LightChannelList[0].Value;
                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[1]);
                            AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前设置光源值为:{LightCtl_Zuo.BaseConfig.LightChannelList[0].Value}");

                            Thread.Sleep(20);
                            LightCtl_You.BaseConfig.LightChannelList[0].Value -= val;
                            if (LightCtl_You.BaseConfig.LightChannelList[0].Value < 5)
                            {
                                LightCtl_You.BaseConfig.LightChannelList[0].Value = 5;
                            }
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[0]);
                            Thread.Sleep(10);
                            LightCtl_You.BaseConfig.LightChannelList[1].Value = LightCtl_You.BaseConfig.LightChannelList[0].Value;
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[1]);
                            if (mintimeout >= 5)
                            {
                                mintimeout = 0;
                                AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:最小值5,进入下一阶段");
                                //进入下阶段
                                CLinghtManagement.SaveLightParams();
                                addOrSubCount = 0;
                                onWichStage = 2;
                                return;
                            }


                        }

                    }
                    else
                    {
                        //进入下阶段
                        AutoLogger.Info($"onWichStage=1,光源调整hv_VState={hv_VState.I},当前光源值为:{LightCtl_You.BaseConfig.LightChannelList[0].Value},进入下一阶段");
                        CLinghtManagement.SaveLightParams();
                        addOrSubCount = 0;
                        onWichStage = 2;
                    }
                }
            }
            else if (onWichStage == 2) //第一阶段:识别下止类型
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
                        string labelstr = de_names[nameindex];
                        if (labelstr.Contains("注塑下止"))
                        {
                            AutoLogger.Info($"{cell.CamName}:onWichStage=2,识别到注塑下止");
                            ZipperInfo.ZipperDownMassType = STOPMASS.注塑;
                            Dispatcher.Invoke(() =>
                            {
                                AutoLogger.Info($"{cell.CamName}:onWichStage=2,更新下止图片");
                                ZipperInfo.ZipperDownmssImg = cell.Image.ToBitmapSource().Clone();
                            });

                            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                             {
                                    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                    new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                    new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                    new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                             };
                            System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                            cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                            cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));

                            timeOutCount = 0;
                            AutoLogger.Info($"{cell.CamName}:onWichStage=2,准备进入第二阶段");
                            //进入下阶段                       
                            CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                            onWichStage = 3;
                            return;
                        }
                    }

                }
                if (timeOutCount >= 15) //超过15次识别不到默认为无下止
                {
                    AutoLogger.Info($"{cell.CamName}:onWichStage=2,{timeOutCount}次,没有识别到下止");
                    ZipperInfo.ZipperDownMassType = STOPMASS.无;
                    Dispatcher.Invoke(() =>
                    {
                        ZipperInfo.ZipperDownmssImg = cell.Image.ToBitmapSource().Clone();
                    });
                    AutoLogger.Info($"{cell.CamName}:onWichStage=2,设置下止类型为:无,更新下止图片,进入第二阶段");
                    timeOutCount = 0;                   
                    CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                    onWichStage = 3;
                }
            }
            else if (onWichStage == 3) //第二阶段 识别拉头  缓慢拉拉链移动 实时获取拉头的位置
            {
                if (cell.CamName == "右相机") //只看右边相机的位置
                {
                    // timeOutCount++;
                    DetResult resultDet;
                    resultDet = yolo_search_det.Predict(img) as DetResult;
                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},识别到目标个数为:{resultDet.datas.Count}");
                    if (resultDet.datas.Count > 0)
                    {
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_names[nameindex];
                            if (labelstr.Contains("拉头"))
                            {
                                AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},识别到拉头,拉头图像位置X:{resultDet.datas[i].box.X},拉头离图像边缘距离:{cell.Image.ImageWidth - resultDet.datas[i].box.X}");
                                // int centerx = resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2;
                                // int centery = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2;
                                List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                                 {
                                        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                        new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                        new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                        new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y)
                                 };
                                System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                                cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                                cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));
                               
                                if (cell.Image.ImageWidth - resultDet.datas[i].box.X > 800) //
                                {
                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},识别到拉头,拉头离图像边缘距离:{cell.Image.ImageWidth - resultDet.datas[i].box.X}>800");
                                    int pos = CZipperCommunicate.GetGrippawlLocation();
                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},获取当前机械轴位置:{pos}");
                                    pos = pos - 25; //因为有延迟,实际位置比读取的位置有偏差,顾减去25 经验值
                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},获取当前机械轴-25位置:{pos}");
                                    List<int> templist = new List<int>();
                                    for (int j = 0; j < ZipperInfo.ZipperTriggerPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.ZipperTriggerPos[j] * 10;
                                        templist.Add(temppos);

                                    }
                                    List<int> temphandP = new List<int>();
                                    for (int j = 0; j < ZipperInfo.HandAndTaliPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.HandAndTaliPos[j] * 10;
                                        temphandP.Add(temppos);
                                       
                                    }
                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},起点下限位置:{temphandP[0]}");
                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},终点上限位置:{temphandP[temphandP.Count-1]}");
                                    if (pos > temphandP[temphandP.Count - 1] || pos < temphandP[0])
                                    {
                                        AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},机械轴位置:{pos}>{temphandP[temphandP.Count - 1]},{pos} < {temphandP[0]},retrun");
                                        return;
                                    }
                                    if (templist.Count > 0)
                                    {
                                        AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},满足条件{pos} > {temphandP[temphandP.Count - 1]} || {pos} < {temphandP[0]}");
                                        int crippoint = (int)ZipperInfo.ZipperLneght * 10;
                                        if (pos > templist[templist.Count - 1] && pos > crippoint) //如果超过了这个临界点,说明拉头在下一次拉取的图片中
                                        {
                                            AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},机械轴位置:{pos}大于临界点{templist[templist.Count - 1]},{pos}大于临界点{crippoint}");
                                            pos = pos - crippoint;
                                            AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},机械轴位置:减去一个拉链长度,轴坐标为:{pos}");
                                        }

                                        templist.Add(pos);
                                        templist.Sort(); //升序排序
                                        int pindex = templist.IndexOf(pos);
                                        AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},排序,总拍照次数为:{templist.Count},拉头序号是第{pindex}张图片");
                                        if (templist.Count >= 3)
                                        {
                                            if (pindex == 0)
                                            {
                                               int dis= Math.Abs(pos - templist[pindex + 1]);
                                                AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},pindex={pindex},{dis}>250");
                                                if (dis > 250)
                                                {
                                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},{pos} - {templist[pindex + 1]}>250,停止轴运动,进入下一级段");
                                                    CZipperCommunicate.AixtStop();
                                                    onWichStage = 4;
                                                    return;
                                                }
                                            }
                                            else if (pindex == templist.Count - 1)
                                            {
                                                int dis = Math.Abs(pos - templist[pindex - 1]);
                                                AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},pindex={pindex},{dis}>250");
                                                if (dis > 250)
                                                {
                                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},{pos} - {templist[pindex - 1]}>250,停止轴运动,进入下一级段");
                                                    CZipperCommunicate.AixtStop();
                                                    onWichStage = 4;
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                int dis = Math.Abs(pos - templist[pindex - 1]);
                                                int dis2 = Math.Abs(pos - templist[pindex + 1]);
                                                AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},{dis}>250 &&{dis2}>250");
                                                if (dis > 250 && dis2 > 250) //大于25mm
                                                {
                                                    //if (ZipperInfo.TriggerIndex == 0)
                                                    //{
                                                    //    ZipperInfo.PullchangeIndex = pindex;
                                                    //}
                                                    //else
                                                    //{
                                                    //    // 第一部分：从第4个元素开始的所有元素
                                                    //    List<int> start = templist.Skip(ZipperInfo.TriggerIndex).ToList(); // 跳过前3个，取剩余元素 
                                                    //    // 第二部分：前3个元素（第4个之前）
                                                    //    List<int> end = templist.Take(ZipperInfo.TriggerIndex).ToList(); // 取前3个元素 

                                                    //    start.AddRange(end);

                                                    //    int pullposindex = start.IndexOf(pos);
                                                    //    ZipperInfo.PullchangeIndex = pullposindex;
                                                    //}
                                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},{pos} - {templist[pindex - 1]}>250 &&{pos} - {templist[pindex + 1]}>250");
                                                    //写轴坐标位置
                                                    AutoLogger.Info($"onWichStage=3,timeOutCount={timeOutCount},{templist[pindex - 1]}<{pos}<{templist[pindex + 1]}停止轴运动,进入下一级段");
                                                    CZipperCommunicate.AixtStop();
                                                    onWichStage = 4;
                                                    return;
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
            else if (onWichStage == 4) ////第二阶段 识别拉头  计算拉头亮度,设置光源值
            {
                DetResult resultDet;
                resultDet = yolo_search_det.Predict(img) as DetResult;
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr.Contains("拉头"))
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

                            int bx = resultDet.datas[i].box.X;
                            int by = resultDet.datas[i].box.Y;
                            int w = resultDet.datas[i].box.Width;
                            int h = resultDet.datas[i].box.Height;
                            //  Mat cutmat = img[new Rect(bx, by, w, h)];
                            HOperatorSet.GenRectangle1(out HObject rec1, by, bx, by + h, bx + w);
                            HOperatorSet.ReduceDomain(CameraImage, rec1, out HObject cutimg);

                            ZipperLightHelper.Instance.PullerLightDetection(cutimg, 10, 2.0, out var hv_VState, out var hv_VStride);
                            CameraImage.Dispose();
                            cutimg.Dispose();
                            CLightControlBase cLightControl = null;
                            if (cell.CamName == "右相机")
                            {
                                cLightControl = LightCtl_You;
                            }
                            else
                            {
                                cLightControl = LightCtl_Zuo;
                            }
                            if (hv_VState == 1)
                            {
                                // Console.WriteLine($"需增加亮度");
                                if (cLightControl != null)
                                {
                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    cLightControl.BaseConfig.LightChannelList[1].Value += val;
                                    if (cLightControl.BaseConfig.LightChannelList[1].Value > 200)
                                    {
                                        cLightControl.BaseConfig.LightChannelList[1].Value = 200;
                                        maxtimeout++;
                                    }
                                    cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                                    if (maxtimeout >= 5)
                                    {
                                        maxtimeout = 0;
                                        //进入下阶段
                                        onWichStage = 5;
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                    }
                                }

                            }
                            else if (hv_VState == 2)
                            {
                                Console.WriteLine($"需减少亮度");
                                if (cLightControl != null)
                                {
                                    int val = hv_VStride.I;
                                    if (val == 0)
                                    {
                                        val = 2;
                                    }
                                    cLightControl.BaseConfig.LightChannelList[1].Value -= val;
                                    if (cLightControl.BaseConfig.LightChannelList[1].Value < 5)
                                    {
                                        cLightControl.BaseConfig.LightChannelList[1].Value = 5;
                                        mintimeout++;
                                    }
                                    cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                                    if (mintimeout >= 5)
                                    {
                                        mintimeout = 0;
                                        //进入下阶段
                                        onWichStage = 5;
                                        timeOutCount = 0;
                                        CLinghtManagement.SaveLightParams();
                                    }
                                }

                            }
                            else
                            {
                                //进入下阶段
                                onWichStage = 5;
                                timeOutCount = 0;
                                CLinghtManagement.SaveLightParams();
                                // CZipperCommunicate.SceondstageFinsh();
                            }
                        }
                    }

                }


            }
            else if (onWichStage == 5) ////第二阶段 识别拉头,拉头拉片,LOGO类型
            {
                timeOutCount++;
                DetResult resultDet;
                resultDet = yolo_search_det.Predict(img) as DetResult;
                AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},识别到目标个数为:{resultDet.datas.Count}");
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr == "拉头")
                        {
                            int pos = CZipperCommunicate.GetGrippawlLocation();
                            int crippoint = (int)ZipperInfo.ZipperLneght * 10;
                            if (pos > crippoint) //如果超过了这个临界点,说明拉头在下一次拉取的图片中
                            {
                                AutoLogger.Info($"onWichStage=5,timeOutCount={timeOutCount},机械轴位置:{pos}大于临界点{crippoint}");
                                pos = pos - crippoint;
                                AutoLogger.Info($"onWichStage=5,timeOutCount={timeOutCount},机械轴位置:减去一个拉链长度,轴坐标为:{pos}");
                            }
                            CZipperCommunicate.SendPullLocation(pos);
                            ZipperInfo.ZipperPullerCX =  resultDet.datas[i].box.X + resultDet.datas[i].box.Width / 2;
                            ZipperInfo.ZipperPullerCY = resultDet.datas[i].box.Y + resultDet.datas[i].box.Height / 2;

                            AutoLogger.Info($"onWichStage=5,timeOutCount={timeOutCount},想PLC写入拉头位置:{pos}");
                            findPuller = true;
                            Dispatcher.Invoke(() =>
                            {
                                ZipperInfo.ZipperPullerImg=cell.Image.ToBitmapSource().Clone();

                            });
                            AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},识别到拉头,当前轴停止位置:{pos},写入位置{pos},findPuller=true,更新拉头图片");
                        }
                        if (labelstr == "拉头拉片")
                        {
                            findPulls = true;
                            Dispatcher.Invoke(() =>
                            {
                                ZipperInfo.ZipperPullsImg = cell.Image.ToBitmapSource().Clone();
                            });
                            AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},识别到拉头拉片,findPulls=true更新拉头图片");

                        }
                        if (labelstr == "SBS")
                        {
                            findLogo = true;
                            ZipperInfo.ZipperLogoType = LOGOTYPE.SBS;
                            AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},识别到Logo:SBS,findLogo=true更新Logo图片");
                        }
                    }
                    if (findPuller && findPulls && findLogo)
                    {
                        timeOutCount = 0;
                        AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},拉头,拉头拉片,Logo全部识别到,进入第三阶段");                       
                        CZipperCommunicate.SceondstageFinsh();
                        onWichStage = 6;
                        return;

                    }
                }
                if (timeOutCount >= 15)
                {
                    timeOutCount = 0;
                    AutoLogger.Info($"{cell.CamName}:onWichStage=5,timeOutCount={timeOutCount},识别拉头拉片，logo超时,进入第三阶段");            
                    CZipperCommunicate.SceondstageFinsh();
                    onWichStage = 6;
                    return;
                }


            }
            else if (onWichStage == 6)  //第三阶段 识别上止类型
            {
                if (cell.CamName == "右相机")
                {
                    timeOutCount++;
                    DetResult resultDet;
                    resultDet = yolo_search_det.Predict(img) as DetResult;
                    AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},识别到目标个数为:{resultDet.datas.Count}");
                    if (resultDet.datas.Count > 0)
                    {
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_names[nameindex];
                            if (labelstr.Contains("注塑上止"))
                            {
                                AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},识别到{labelstr}");
                                if (labelstr.Contains("正面")) //这里要改成识别链牙在哪边
                                {
                                    ZipperInfo.ZipperSliderType = PULLTYPE.反穿;
                                    AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},设置拉链为反穿");
                                }
                                else
                                {
                                    ZipperInfo.ZipperSliderType = PULLTYPE.正穿;
                                    AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},设置拉链为正穿");
                                }

                                ZipperInfo.ZipperUpMassType = STOPMASS.注塑;
                                AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},设置拉链上止为:{ZipperInfo.ZipperUpMassType}");
                                Dispatcher.Invoke(() =>
                                {
                                    ZipperInfo.ZipperUpmssImg = cell.Image.ToBitmapSource().Clone();
                                    AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},更新上止图片");
                                });

                                List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
                            {
                                new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y),
                                new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y),
                                new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height),
                                new System.Windows.Point(resultDet.datas[i].box.X, resultDet.datas[i].box.Y)

                            };
                                System.Windows.Point txtpoint = new System.Windows.Point(resultDet.datas[i].box.X + resultDet.datas[i].box.Width, resultDet.datas[i].box.Y + resultDet.datas[i].box.Height);
                                cell.DrawEdges.Add(new CEdgeDraw(rec1Points, Brushes.Pink));
                                cell.DrawEdges.Add(new CEdgeDraw(labelstr, txtpoint, Brushes.Pink));

                                timeOutCount = 0;
                                TestFinsh = true;
                                //结束
                                AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},完成识别");
                                CZipperCommunicate.TestFinish();
                                Thread.Sleep(10);
                                CZipperCommunicate.ThirdstageFinsh();
                               // TestFinshEven?.Invoke(TestFinsh);
                                return;
                            }
                        }

                    }
                    if (timeOutCount >= 15) //超过15次识别不到默认为无上止
                    {
                        ZipperInfo.ZipperUpMassType = STOPMASS.无;
                        AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},设置拉链上止为:{ZipperInfo.ZipperUpMassType}");
                        Dispatcher.Invoke(() =>
                        {
                            ZipperInfo.ZipperUpmssImg = cell.Image.ToBitmapSource().Clone();
                            AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},更新上止图片");
                        });

                        timeOutCount = 0;
                        onWichStage = 0;
                        TestFinsh = true;
                        //结束
                        AutoLogger.Info($"onWichStage=6,timeOutCount={timeOutCount},完成识别");
                        CZipperCommunicate.TestFinish();
                        Thread.Sleep(10);
                        CZipperCommunicate.ThirdstageFinsh();

                      //  TestFinshEven?.Invoke(TestFinsh);
                        return;

                    }

                  
                }
            }
        }
        private void IniYolo(string modelpath)
        {
            if (!File.Exists(modelpath))
            {
                return;
            }
           
            //yolo_search_det.Dispose();
          
           // ModelType model_type_det = ModelType.VisionModelDet;
           // EngineType engine_type = EngineType.OpenVINO;
            string CurrentDevice = "GPU.0";
            int common_Categ_num = de_names.Length;
            float Score = 0.6f;
            float Nms = 0.5f;
            int Input_size = 640;

            yolo_search_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, modelpath, EngineType.OpenVINO,
                CurrentDevice, common_Categ_num, Score, Nms, Input_size);
            //yolo_search_det = YOLO.GetYolo(
            //    model_type_det,
            //    modelpath,
            //    engine_type,
            //    CurrentDevice,
            //    common_Categ_num,
            //    Score,
            //    Nms,
            //    Input_size
            //);
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
                    settingsModel = ConfigAPI.Load<CZipperInfo>(ParameterPath);
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
