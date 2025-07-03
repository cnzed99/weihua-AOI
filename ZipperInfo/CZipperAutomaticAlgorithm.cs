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
using System.Windows.Threading;
using WH.Entity;
using WH.LightControl;
using WH.RecipeCellRootBase;
using WH.RunCell;
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
        /// 处在哪个阶段
        /// </summary>
        public static int onWichStage = 0;
        /// <summary>
        /// 自动识别结束
        /// </summary>
        public static bool TestFinsh = false;
        /// <summary>
        /// 识别模型对象
        /// </summary>
        YOLO yolo_search_det = new();
        /// <summary>
        /// 识别名
        /// </summary>
        string[] de_names;
        /// <summary>
        /// 超时统计
        /// </summary>
        int timeOutCount = 0;

        CLightControlBase LightCtl_Zuo = null;
        CLightControlBase LightCtl_You = null;

        public static bool findPulls = false;//检测到拉片
        public static bool findPuller=false; //检测到拉头
        public static bool findLogo = false; //检测到Logo
        public CZipperAutomaticAlgorithm()
        {
            string modelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\AutoMatic";
            string txtpath = modelDirPath + "\\classes.txt";
            string modelpath = modelDirPath + "\\lalianAuto.onnx";
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

        int tempVState = 0;
        HObject CameraImage = new HObject();
        public void ZipperAutomaticAlgorithmRun(Cell cell)
        {
            //第一阶段: 计算光源值
            Mat img = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
                 MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                 cell.Image.ImageData);

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
            // HOperatorSet.WriteImage(CameraImage, "png", 0, $"C:\\Users\\Administrator\\Desktop\\新建文件夹\\{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.png");
            if (onWichStage == 1)
            {
                if (cell.CamName == "右相机") //调光源只用一边的结果
                {
                    ZipperLightHelper.Instance.ZipperLightDetection(CameraImage, 10, 2.0, out var hv_VState, out var hv_VStride);
                    CameraImage.Dispose();

                    if (hv_VState == 1)
                    {
                        Console.WriteLine($"需增加亮度");
                        if (tempVState != hv_VState)
                        {
                            addOrSubCount++;
                        }
                        if (addOrSubCount > 4)
                        {
                            //进入下阶段
                            CLinghtManagement.SaveLightParams();
                            onWichStage = 2;
                            addOrSubCount = 0;
                            return;
                        }
                        tempVState = hv_VState;

                        if (LightCtl_Zuo != null && LightCtl_Zuo != null)
                        {
                            int val = hv_VStride.I;
                            if (val == 0)
                            {
                                val = 2;
                            }
                            LightCtl_Zuo.BaseConfig.LightChannelList[0].Value += val;
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value > 200)
                            {
                                LightCtl_Zuo.BaseConfig.LightChannelList[0].Value = 200;
                            }
                           // LightCtl_Zuo.BaseConfig.LightChannelList[1].Value = LightCtl_Zuo.BaseConfig.LightChannelList[0].Value;
                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[0]);
                            //  Thread.Sleep(20);
                            //  LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[1]);

                             Thread.Sleep(20);
                            LightCtl_You.BaseConfig.LightChannelList[0].Value += val;
                            if (LightCtl_You.BaseConfig.LightChannelList[0].Value > 200)
                            {
                                LightCtl_You.BaseConfig.LightChannelList[0].Value = 200;
                            }
                            // LightCtl_You.BaseConfig.LightChannelList[1].Value = LightCtl_You.BaseConfig.LightChannelList[0].Value;
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[0]);
                            // Thread.Sleep(20);
                            // LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[1]);
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value>=200)
                            {
                                //进入下阶段
                                CLinghtManagement.SaveLightParams();
                                onWichStage = 2;
                                addOrSubCount = 0;
                                return;
                            }
                            //    //进入下阶段
                            //    CLinghtManagement.SaveLightParams();
                            //    onWichStage = 2;
                            //}

                        }

                    }
                    else if (hv_VState == 2)
                    {
                        Console.WriteLine($"需减少亮度");
                        if (LightCtl_Zuo != null && LightCtl_Zuo != null)
                        {
                            if (tempVState != hv_VState)
                            {
                                addOrSubCount++;
                            }
                            if (addOrSubCount > 4)
                            {
                                //进入下阶段
                                CLinghtManagement.SaveLightParams();
                                onWichStage = 2;
                                return;
                            }
                            tempVState = hv_VState;
                            int val = hv_VStride.I;
                            if (val == 0)
                            {
                                val = 2;
                            }
                            LightCtl_Zuo.BaseConfig.LightChannelList[0].Value -= val;
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value < 5)
                            {
                                LightCtl_Zuo.BaseConfig.LightChannelList[0].Value = 5;
                            }
                            //  LightCtl_Zuo.BaseConfig.LightChannelList[1].Value = LightCtl_Zuo.BaseConfig.LightChannelList[0].Value;
                            LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[0]);
                            // Thread.Sleep(20);
                            // LightCtl_Zuo.SetChannelValue(LightCtl_Zuo.BaseConfig.LightChannelList[1]);

                             Thread.Sleep(20);
                            LightCtl_You.BaseConfig.LightChannelList[0].Value -= val;
                            if (LightCtl_You.BaseConfig.LightChannelList[0].Value < 5)
                            {
                                LightCtl_You.BaseConfig.LightChannelList[0].Value = 5;
                            }
                            //  LightCtl_You.BaseConfig.LightChannelList[1].Value = LightCtl_You.BaseConfig.LightChannelList[0].Value;
                            LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[0]);
                            // Thread.Sleep(20);
                            // LightCtl_You.SetChannelValue(LightCtl_You.BaseConfig.LightChannelList[1]);
                            if (LightCtl_Zuo.BaseConfig.LightChannelList[0].Value <= 5)
                            {
                                //进入下阶段
                                CLinghtManagement.SaveLightParams();
                                onWichStage = 2;
                                addOrSubCount = 0;
                                return;
                            }


                        }

                    }
                    else
                    {
                        //进入下阶段
                        CLinghtManagement.SaveLightParams();
                        onWichStage = 2;
                    }
                }
            }
            else if (onWichStage == 2) //第一阶段:识别下止类型
            {
                timeOutCount++;
                DetResult resultDet;
                resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr.Contains("注塑下止"))
                        {
                            ZipperInfo.ZipperDownMassType = STOPMASS.注塑;
                            Dispatcher.BeginInvoke(() =>
                            {
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
                            //进入下阶段
                            onWichStage = 3;
                            CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                            return;
                        }
                    }

                }
                if (timeOutCount >= 15) //超过15次识别不到默认为无下止
                {
                    ZipperInfo.ZipperDownMassType = STOPMASS.无;
                    Dispatcher.BeginInvoke(() =>
                    {
                        ZipperInfo.ZipperDownmssImg = cell.Image.ToBitmapSource().Clone();
                    });
                    
                    timeOutCount = 0;
                    onWichStage = 3;
                    CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                }
            }
            else if (onWichStage == 3) //第二阶段 识别拉头  缓慢拉拉链移动 实时获取拉头的位置
            {
                if (cell.CamName == "右相机") //只看右边相机的位置
                {
                    // timeOutCount++;
                    DetResult resultDet;
                    resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                    if (resultDet.datas.Count > 0)
                    {
                        for (int i = 0; i < resultDet.datas.Count; i++)
                        {
                            int nameindex = int.Parse(resultDet.datas[i].lable);
                            string labelstr = de_names[nameindex];
                            if (labelstr.Contains("拉头"))
                            {
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

                                    int pos = CZipperCommunicate.GetGrippawlLocation();

                                    List<int> templist = new List<int>();                                   
                                    for (int j = 0; j < ZipperInfo.ZipperTriggerPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.ZipperTriggerPos[j] * 10;
                                        templist.Add(temppos);

                                    }
                                    List<int> temphandP = new List<int>();
                                    for (int j = 0; j < ZipperInfo.HandAndTaliPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.ZipperTriggerPos[j] * 10;
                                        temphandP.Add(temppos);

                                    }
                                    if (pos > temphandP[temphandP.Count - 1] || pos < temphandP[0])
                                    {
                                        return;
                                    }
                                    if (templist.Count > 0)
                                    {
                                        int crippoint =(int) ZipperInfo.ZipperLneght * 10;
                                        if (pos > templist[templist.Count - 1]&&pos> crippoint) //如果超过了这个临界点,说明拉头在下一次拉取的图片中
                                        {
                                            pos = pos - crippoint;
                                        }

                                        templist.Add(pos);
                                        templist.Sort(); //升序排序
                                        int pindex = templist.IndexOf(pos);
                                        if (templist.Count >= 3)
                                        {
                                            if (pindex == 0)
                                            {

                                                if (Math.Abs(pos - templist[pindex + 1]) > 100)
                                                {
                                                    
                                                    CZipperCommunicate.AixtStop();
                                                    //Dispatcher.BeginInvoke(() =>
                                                    //{
                                                    //    ZipperInfo.ZipperPullerImg = cell.Image.ToBitmapSource().Clone();
                                                         
                                                    //});
                                                   
                                                    CZipperCommunicate.SendPullLocation(pos);
                                                    CZipperCommunicate.SceondstageFinsh();
                                                    onWichStage = 5;
                                                    return;
                                                }
                                            }
                                            else if (pindex == templist.Count - 1)
                                            {
                                                if (Math.Abs(pos - templist[pindex - 1]) > 100)
                                                {
                                                    CZipperCommunicate.AixtStop();
                                                    //Dispatcher.BeginInvoke(() =>
                                                    //{
                                                    //    ZipperInfo.ZipperPullerImg = cell.Image.ToBitmapSource().Clone();
                                                    //});
                                                 
                                                    CZipperCommunicate.SendPullLocation(pos);
                                                    CZipperCommunicate.SceondstageFinsh();
                                                    onWichStage = 5;
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (Math.Abs(pos - templist[pindex - 1]) > 100 && Math.Abs(pos - templist[pindex + 1]) > 100) //大于10mm
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
                                                    //写轴坐标位置
                                                    CZipperCommunicate.AixtStop();
                                                    //Dispatcher.BeginInvoke(() =>
                                                    //{
                                                    //    ZipperInfo.ZipperPullerImg = cell.Image.ToBitmapSource().Clone();

                                                    //});
                                                    
                                                    CZipperCommunicate.SendPullLocation(pos);
                                                    CZipperCommunicate.SceondstageFinsh();
                                                    onWichStage = 5;
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
                //DetResult resultDet;
                //resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                //if (resultDet.datas.Count > 0)
                //{
                //    for (int i = 0; i < resultDet.datas.Count; i++)
                //    {
                //        int nameindex = int.Parse(resultDet.datas[i].lable);
                //        string labelstr = de_names[nameindex];
                //        if (labelstr.Contains("拉头"))
                //        {
                //            int bx = resultDet.datas[i].box.X;
                //            int by = resultDet.datas[i].box.Y;
                //            int w = resultDet.datas[i].box.Width;
                //            int h = resultDet.datas[i].box.Height;
                //            Mat cutmat = img[new Rect(bx, by, w, h)];
                //            HOperatorSet.GenRectangle1(out HObject rec1, by, bx, by + h, bx + w);
                //            HOperatorSet.ReduceDomain(CameraImage, rec1, out HObject cutimg);

                //            ZipperLightHelper.Instance.ZipperLightDetection(cutimg, 10, 2.0, out var hv_VState, out var hv_VStride);
                //            CameraImage.Dispose();
                //            CLightControlBase cLightControl = null;
                //            if (cell.CamName == "右相机")
                //            {
                //                cLightControl = LightCtl_You;
                //            }
                //            else
                //            {

                //                cLightControl = LightCtl_Zuo;
                //            }
                //            if (hv_VState == 1)
                //            {
                //                Console.WriteLine($"需增加亮度");
                //                if (cLightControl != null)
                //                {
                //                    int val = hv_VStride.I;
                //                    if (val == 0)
                //                    {
                //                        val = 2;
                //                    }
                //                    cLightControl.BaseConfig.LightChannelList[1].Value += val;
                //                    if (cLightControl.BaseConfig.LightChannelList[1].Value > 200)
                //                    {
                //                        cLightControl.BaseConfig.LightChannelList[1].Value = 200;
                //                    }
                //                    cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                //                }

                //            }
                //            else if (hv_VState == 2)
                //            {
                //                Console.WriteLine($"需减少亮度");
                //                if (cLightControl != null)
                //                {
                //                    int val = hv_VStride.I;
                //                    if (val == 0)
                //                    {
                //                        val = 2;
                //                    }
                //                    cLightControl.BaseConfig.LightChannelList[1].Value -= val;
                //                    if (cLightControl.BaseConfig.LightChannelList[1].Value < 5)
                //                    {
                //                        cLightControl.BaseConfig.LightChannelList[1].Value = 5;
                //                    }
                //                    cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                //                }

                //            }
                //            else
                //            {
                //                //进入下阶段
                //                onWichStage = 5;
                //                timeOutCount = 0;
                //                CLinghtManagement.SaveLightParams();
                //               // CZipperCommunicate.SceondstageFinsh();
                //            }
                //        }
                //    }

                //}

                //onWichStage = 5;
                //CZipperCommunicate.SceondstageFinsh();
            }
            else if (onWichStage == 5) ////第二阶段 识别拉头,拉头拉片,LOGO类型
            {
                timeOutCount++;
                DetResult resultDet;
                resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr=="拉头")
                        {
                            findPuller=true;
                            Dispatcher.BeginInvoke(() =>
                            {
                                ZipperInfo.ZipperPullerImg = cell.Image.ToBitmapSource().Clone();
                            });

                        }
                        if (labelstr=="拉头拉片")
                        {
                            findPulls = true;
                            Dispatcher.BeginInvoke(() =>
                            {
                                ZipperInfo.ZipperPullsImg = cell.Image.ToBitmapSource().Clone();
                            });
                           
                        }
                        if (labelstr == "SBS")
                        {
                            findLogo = true;
                            ZipperInfo.ZipperLogoType=LOGOTYPE.SBS;
                        }
                    }
                    if (findPuller && findPulls && findLogo)
                    {
                        timeOutCount = 0;
                        onWichStage = 6;
                        CZipperCommunicate.SceondstageFinsh();
                        return;

                    }
                }
                if (timeOutCount>=15)
                {
                    timeOutCount = 0;
                    onWichStage = 6;
                    CZipperCommunicate.SceondstageFinsh();
                    return;
                }

              
            }
            else if (onWichStage == 6)  //第三阶段 识别上止类型
            {
                timeOutCount++;
                DetResult resultDet;
                resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr.Contains("注塑上止"))
                        {
                            if (labelstr.Contains("正面") && cell.CamName == "右相机") //这里要改成识别链牙在哪边
                            {
                                ZipperInfo.ZipperSliderType = PULLTYPE.反穿;
                            }
                            else
                            {
                                ZipperInfo.ZipperSliderType = PULLTYPE.正穿;
                            }

                            ZipperInfo.ZipperUpMassType = STOPMASS.U型尼龙;
                            Dispatcher.BeginInvoke(() =>
                            {
                                ZipperInfo.ZipperUpmssImg = cell.Image.ToBitmapSource().Clone();

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
                            TestFinsh=true;
                            //结束
                            CZipperCommunicate.TestFinish();
                            Thread.Sleep(10);
                            CZipperCommunicate.ThirdstageFinsh();
                        }
                    }

                }
                if (timeOutCount >= 15) //超过15次识别不到默认为无上止
                {
                    ZipperInfo.ZipperUpMassType = STOPMASS.U型尼龙;
                    Dispatcher.BeginInvoke(() =>
                    {
                        ZipperInfo.ZipperUpmssImg = cell.Image.ToBitmapSource().Clone();
                    });
                   
                    timeOutCount = 0;
                    onWichStage = 0;
                    TestFinsh = true;
                    //结束
                    CZipperCommunicate.TestFinish();
                    Thread.Sleep(10);
                    CZipperCommunicate.ThirdstageFinsh();

                }
            }
        }
        private void IniYolo(string modelpath)
        {
            if (!File.Exists(modelpath))
            {
                return;
            }
            yolo_search_det.Dispose();
            ModelType model_type_det = ModelType.YOLOv8Det;
            EngineType engine_type = EngineType.OpenVINO;
            string CurrentDevice = "GPU.0";
            int common_Categ_num = de_names.Length;
            float Score = 0.6f;
            float Nms = 0.5f;
            InputImgSize Input_size = InputImgSize.IN640;
            yolo_search_det = YOLO.GetYolo(
                model_type_det,
                modelpath,
                engine_type,
                CurrentDevice,
                common_Categ_num,
                Score,
                Nms,
                Input_size
            );
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
