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
using WH.Entity;
using WH.LightControl;
using WH.RunCell;

namespace ZipperInfo
{
    public class CZipperAutomaticAlgorithm
    {
        public static CZipperInfo ZipperInfo { get; set; }=new CZipperInfo();
        /// <summary>
        /// 处在哪个阶段
        /// </summary>
        public int onWichStage = 0;
        /// <summary>
        /// 识别模型对象
        /// </summary>
        YOLO yolo_search_det;
        /// <summary>
        /// 识别名
        /// </summary>
        string[] de_names;
        /// <summary>
        /// 超时统计
        /// </summary>
        int timeOutCount = 0;
        public CZipperAutomaticAlgorithm()
        {
            string modelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\AutoMatic";
            string txtpath = modelDirPath + "\\classes.txt";
            string modelpath = modelDirPath + "\\best.onnx";
            if (File.Exists(txtpath))
            {
                de_names = File.ReadAllLines(txtpath);
                IniYolo(modelpath);
            }

        }

        public void ZipperAutomaticAlgorithmRun(Cell cell)
        {
            //第一阶段: 计算光源值
            Mat img = new Mat(cell.Image.ImageHeight, cell.Image.ImageWidth,
                 MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                 cell.Image.ImageData);
            if (onWichStage == 1)
            {
                if (cell.CamName == "右相机") //调光源只用一边的结果
                {

                    ZipperLightHelper.Instance.ZipperLightDetection(img, 10, 0.5, out var hv_VState, out var hv_VStride);
                    CLightControlBase cLightControl = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Name == "COM2");
                    if (hv_VState == 1)
                    {
                        Console.WriteLine($"需增加亮度");
                        cLightControl.BaseConfig.LightChannelList[0].Value += hv_VStride;
                        cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[0]);
                    }
                    else if (hv_VState == 2)
                    {
                        Console.WriteLine($"需减少亮度");
                        cLightControl.BaseConfig.LightChannelList[0].Value -= hv_VStride;
                        cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[0]);
                    }
                    else
                    {
                        //进入下阶段
                        CLinghtManagement.SaveLightParams();
                        onWichStage = 2;
                    }
                }
            }
            else if (onWichStage == 2) //第一阶段:识别上止类型
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
                            ZipperInfo.ZipperDownMass = STOPMASS.注塑;
                            timeOutCount = 0;
                            //进入下阶段
                            onWichStage = 3;
                            CZipperCommunicate.FirststageFinsh(); //第一阶段完成
                        }
                    }

                }
                if (timeOutCount >= 15) //超过15次识别不到默认为无上止
                {
                    ZipperInfo.ZipperUpMass = STOPMASS.无;
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
                                if (cell.Image.ImageWidth- resultDet.datas[i].box.X > 600) //
                                {
                                   
                                    int pos = CZipperCommunicate.GetGrippawlLocation();

                                    List<int> templist = new List<int>();
                                    for (int j = 0; j < ZipperInfo.ZipperTriggerPos.Count; j++)
                                    {
                                        int temppos = (int)ZipperInfo.ZipperTriggerPos[j] * 10;
                                        templist.Add(temppos);

                                    }
                                    templist.Add(pos);
                                    templist.Sort(); //升序排序
                                    int pindex = templist.IndexOf(pos);
                                    if (templist.Count >= 3)
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
                                            CZipperCommunicate.SendPullLocation(pos);
                                            onWichStage = 4;
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
                resultDet = yolo_search_det.predict(img, 0.6f, 0.5f) as DetResult;
                if (resultDet.datas.Count > 0)
                {
                    for (int i = 0; i < resultDet.datas.Count; i++)
                    {
                        int nameindex = int.Parse(resultDet.datas[i].lable);
                        string labelstr = de_names[nameindex];
                        if (labelstr.Contains("拉头"))
                        {
                            int bx = resultDet.datas[i].box.X;
                            int by = resultDet.datas[i].box.Y;
                            int w = resultDet.datas[i].box.Width;
                            int h = resultDet.datas[i].box.Height;
                            Mat cutmat = img[new Rect(bx, by, w, h)];
                            ZipperLightHelper.Instance.ZipperLightDetection(cutmat, 10, 0.5, out var hv_VState, out var hv_VStride);
                            CLightControlBase cLightControl;
                            if (cell.CamName == "右相机")
                            {
                                cLightControl = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Name == "COM2");
                            }
                            else
                            {
                                cLightControl = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Name == "COM1");
                            }
                            if (hv_VState == 1)
                            {
                                Console.WriteLine($"需增加亮度");
                               // CLightControlBase cLightControl = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Name == "COM2");
                                cLightControl.BaseConfig.LightChannelList[1].Value += hv_VStride;
                                cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                            }
                            else if (hv_VState == 2)
                            {
                                Console.WriteLine($"需减少亮度");
                               // CLightControlBase cLightControl = CLinghtManagement.LightControlDict.Values.First(c => c.BaseConfig.Name == "COM2");
                                cLightControl.BaseConfig.LightChannelList[1].Value -= hv_VStride;
                                cLightControl.SetChannelValue(cLightControl.BaseConfig.LightChannelList[1]);
                            }
                            else
                            {
                                //进入下阶段
                                onWichStage = 5;
                                timeOutCount = 0;
                                CLinghtManagement.SaveLightParams();
                                CZipperCommunicate.SceondstageFinsh();
                            }
                        }
                    }

                }
            }
            else if (onWichStage == 5)  //第三阶段 识别上止类型
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
                            ZipperInfo.ZipperUpMass = STOPMASS.注塑;
                            timeOutCount = 0;
                            //结束
                            CZipperCommunicate.TestFinish();
                            CZipperCommunicate.ThirdstageFinsh() ;
                        }
                    }

                }
                if (timeOutCount >= 15) //超过15次识别不到默认为无上止
                {
                    ZipperInfo.ZipperUpMass = STOPMASS.无;
                    timeOutCount = 0;
                    onWichStage = 0;
                    //结束
                    CZipperCommunicate.TestFinish();
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
