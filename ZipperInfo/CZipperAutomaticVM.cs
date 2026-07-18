using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WH.Entity;
using WH.LightControl;

namespace ZipperInfo
{
    public partial class CZipperAutomaticVM : ObservableObject
    {
        public List<CAutomaticModel> AutoData { get; set; }

        private float showZipperLenght;
        //代理属性，为了方便同步更新两套参数的值
        public float ShowZipperLenght
        {
            get { return showZipperLenght; }
            set
            {
                showZipperLenght = value;
                AutoData[0].ShowZipperLenght = value;
                AutoData[1].ShowZipperLenght = value;
                OnPropertyChanged();
            }
        }

        float quekoulenght = 3.6f;
        //代理属性，为了方便同步更新两套参数的值
        public float QuekouLenght
        {
            get { return quekoulenght; }
            set
            {
                quekoulenght = value;
                AutoData[0].QuekouLenght = value;
                AutoData[1].QuekouLenght = value;
                OnPropertyChanged();
            }
        }
        public CZipperAutomaticVM()
        {
            AutoData = LoadParameter();
            if (AutoData.Count >= 2)
            {
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData = AutoData[0];
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.AutoData = AutoData[1];
                ShowZipperLenght = AutoData[0].ShowZipperLenght;
                QuekouLenght = AutoData[0].QuekouLenght;
                if (AutoData[0].LinghtValueInfos.Count==0)
                {
                    AutoData[0].LinghtValueInfos.Add(new() { ColorName = "黄铜", ClothLinghtValue = 35, UpMassLinghtValue = 255 });
                    AutoData[0].LinghtValueInfos.Add(new() { ColorName = "黄电白", ClothLinghtValue = 45, UpMassLinghtValue = 255 });
                    AutoData[0].LinghtValueInfos.Add(new() { ColorName = "亮黑镍", ClothLinghtValue = 50, UpMassLinghtValue = 255 });
                    AutoData[0].LinghtValueInfos.Add(new() { ColorName = "古银", ClothLinghtValue = 60, UpMassLinghtValue = 255 });
                    AutoData[0].LinghtValueInfos.Add(new() { ColorName = "青古银", ClothLinghtValue = 70, UpMassLinghtValue = 255 });
                }
            }
        }

        bool startAutoTest;

        public Action<bool> StartAutoTestEven;

        [RelayCommand]
        void SendPoints(object win)
        {

            CZipperAutomaticAlgorithm.AutoLogger.Info($"开始识别,设置拉链长度{AutoData[0].ZipperLenght}");
            //写入拉链长度
            // CZipperCommunicate.ClearWarn();
            CZipperCommunicate.SendZipperLenght(AutoData[0].ZipperLenght);
            CGetZipperTriggerPoint.GetTriggerPoints(AutoData[0], out List<float> points, out List<float> handandtalipoints,
                out int cutoffIndex, out int zipperCacheCount, out int triggerType);

            CGetZipperTriggerPoint.GetTriggerPoints(AutoData[1], out List<float> points_2, out List<float> handandtalipoints_2,
           out int cutoffIndex_2, out int zipperCacheCount_2, out int triggerType_2);

            StringBuilder stringBuilder = new StringBuilder("计算触发点位");
            for (int i = 0; i < points.Count; i++)
            {
                stringBuilder.Append($"第{i + 1}点:{points[i]},");
            }
            if (handandtalipoints.Count > 1)
            {
                if (triggerType == 3)
                {

                    int firstIndex = points.IndexOf(handandtalipoints[0]);
                    int endposIndex = firstIndex - 1;
                    if (endposIndex >= 0)
                    {
                        CZipperAutomaticAlgorithm.EndPosTemp = points[endposIndex];
                    }
                }
                else
                {
                    CZipperAutomaticAlgorithm.EndPosTemp = points.Last();
                }


                stringBuilder.Append($"起点:{handandtalipoints[0]},终点:{handandtalipoints[handandtalipoints.Count - 1]}");
            }
            stringBuilder.Append($",切断时已经拍了{cutoffIndex}张照片");
            stringBuilder.Append($",切断时,切刀到相机有{zipperCacheCount}条拉链已经拍完照片");
            CZipperAutomaticAlgorithm.AutoLogger.Info(stringBuilder.ToString());
            stringBuilder.Clear();
            if (points != null && points.Count > 0)
            {
                //写入拍照的总图片数量
                CZipperCommunicate.SendPhotoCount(points.Count);
                //计算拉链触发点位 ID改变位置
                CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);

                //计算拉链触发点位 ID改变位置
                CZipperCommunicate.SendPoints2(points_2, handandtalipoints_2, cutoffIndex_2, zipperCacheCount_2);
                Thread.Sleep(100);
                startAutoTest = true;
               // CZipperCommunicate.AixtContinue(false);
                CZipperAutomaticAlgorithm.ZipperInfo.ShowZipperLenght = AutoData[0].ShowZipperLenght;

                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData = AutoData[0];
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperTriggerPos = points;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperImagesCount = points.Count;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.CutoffIndex = cutoffIndex;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.HandAndTaliPos = handandtalipoints;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.TriggerType = triggerType;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperDownmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperUpmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullsImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerCX = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerCY = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.FindLogoSider = 0;
                CZipperAutomaticAlgorithm.Cloth_Stage1_OK = false;
               // CZipperAutomaticAlgorithm.Station1_Stage2_OK = false;
                CZipperAutomaticAlgorithm.AutoSettingTimeoutCount = 0;
                CZipperAutomaticAlgorithm.Puller_Stage1_OK = false;
                CZipperAutomaticAlgorithm.Pulls_Stage1_OK = false;
                CZipperAutomaticAlgorithm.Puller_Stage2_OK = false;
                CZipperAutomaticAlgorithm.Pulls_Stage2_OK = false;

                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.AutoData = AutoData[1];
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperTriggerPos = points_2;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.CutoffIndex = cutoffIndex_2;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.HandAndTaliPos = handandtalipoints_2;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.TriggerType = triggerType_2;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperDownmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperUpmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperPullerImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperPullsImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperPullerCX = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperPullerCY = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData2.FindLogoSider = 0;
                CZipperAutomaticAlgorithm.Cloth_Stage1_OK = false;
                CZipperAutomaticAlgorithm.Cloth_Stage2_OK = false;

                CZipperAutomaticAlgorithm.TestFinsh = false;
                CZipperAutomaticAlgorithm.onWichStage = 1;
                CZipperAutomaticAlgorithm.findPuller = false;
                CZipperAutomaticAlgorithm.findPulls = false;
                CZipperAutomaticAlgorithm.findLogo = false;
                CZipperAutomaticAlgorithm.findUpMass = false;
                CZipperAutomaticAlgorithm.findDownMass = false;
                CZipperAutomaticAlgorithm.findlianya = false;
                CZipperAutomaticAlgorithm.findUpMassCount = 0;
                CZipperAutomaticAlgorithm.findDownMassCount = 0;
                //CZipperAutomaticAlgorithm.tempLightValue_zuo_change1 = 0;
                //CZipperAutomaticAlgorithm.tempLightValue_zuo_change2 = 0;
                //CZipperAutomaticAlgorithm.tempLightValue_you_change1 = 0;
                //CZipperAutomaticAlgorithm.tempLightValue_you_change2 = 0;
                //CZipperAutomaticAlgorithm.zuo_lightOK = false;
                //CZipperAutomaticAlgorithm.you_lightOK = false;
                CZipperAutomaticAlgorithm.findLogosidertype[0] = false;
                CZipperAutomaticAlgorithm.findLogosidertype[1] = false;
                CZipperAutomaticAlgorithm.findPullerCount = 0;
                CZipperAutomaticAlgorithm.findPullsCount = 0;
                //CZipperCommunicate.SendHelianStastPos(AutoData[0].ZipperLenght - AutoData[0].QuekouLenght * 10);
                //CZipperCommunicate.SendHelianEndPos(AutoData[0].ZipperLenght - AutoData[0].QuekouLenght * 10);
                CZipperAutomaticAlgorithm.AutoSettingPosFinsh = false;
                CZipperAutomaticAlgorithm.startTriggerCount = 0;
                CZipperAutomaticAlgorithm.onWichStage2 = 1;

                //将光源值先减小到较状态
                try
                {
                    if (CLinghtManagement.LightControlDict.Count > 0)
                    {

                        foreach (var item in CLinghtManagement.LightControlDict.Values)
                        {
                            if (item.IsOpen())
                            {
                                Thread.Sleep(20);
                                item.BaseConfig.LightChannelList[0].Value = 30;
                                item.SetChannelValue(item.BaseConfig.LightChannelList[0]);
                                Thread.Sleep(20);
                                item.BaseConfig.LightChannelList[1].Value = 30;
                                item.SetChannelValue(item.BaseConfig.LightChannelList[1]);
                                Thread.Sleep(20);
                                item.BaseConfig.LightChannelList[4].Value = 30;
                                item.SetChannelValue(item.BaseConfig.LightChannelList[4]);
                            }
                        }

                    }
                }
                catch (Exception)
                {
                }

                Thread.Sleep(100);
                
                CZipperCommunicate.SendCamFPS(400); //起始400ms触发一次
                CZipperCommunicate.TestStart();
                SaveParameter(AutoData);
                var window = win as HandyControl.Controls.Window;
                window?.Close();
                ProgressBarViewModel.AutoMessage = "准备执行拉链自动识别程序...";
                ProgressBarViewModel.ProgressBarValue = 0;
                StartAutoTestEven?.Invoke(startAutoTest);
            }
        }

        [RelayCommand]
        void CloseWin(object win)
        {
            var window = win as HandyControl.Controls.Window;
            window?.Close();
        }
        [RelayCommand]
        void SendPoints2()
        {
            CZipperCommunicate.SendZipperLenght(AutoData[0].ZipperLenght);
            CGetZipperTriggerPoint.GetTriggerPoints(AutoData[0], out List<float> points, out List<float> handandtalipoints,
                out int cutoffIndex, out int zipperCacheCount, out int triggerType);

            if (points != null && points.Count > 0)
            {
                //写入拍照的总图片数量
                CZipperCommunicate.SendPhotoCount(points.Count);
                //计算拉链触发点位 ID改变位置
                CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);

                CZipperAutomaticAlgorithm.ZipperInfo.ShowZipperLenght = AutoData[0].ShowZipperLenght;

                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.AutoData = AutoData[0];
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperTriggerPos = points;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.CutoffIndex = cutoffIndex;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.HandAndTaliPos = handandtalipoints;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.TriggerType = triggerType;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperDownmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperUpmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullsImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerCX = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperPullerCY = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.TempData1.FindLogoSider = 0;
                CZipperAutomaticAlgorithm.Cloth_Stage1_OK = false;
               // CZipperAutomaticAlgorithm.Station1_Stage2_OK = false;
                CZipperAutomaticAlgorithm.AutoSettingTimeoutCount = 0;
                SaveParameter(AutoData);
            }
        }

        public static string ParameterPath = "..\\SystemConfig\\ZipperAutoData.Json";

        #region 保存参数

        public static void SaveParameter(List<CAutomaticModel> data)
        {
            try
            {
                ConfigAPI.Save(data, ParameterPath);
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数

        public static List<CAutomaticModel> LoadParameter()
        {
            List<CAutomaticModel> settingsModel = new List<CAutomaticModel>();
            try
            {
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.LoadDeserialize<List<CAutomaticModel>>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new List<CAutomaticModel>();
                        CAutomaticModel st1 = new CAutomaticModel();
                        CAutomaticModel st2 = new CAutomaticModel();
                        settingsModel.Add(st1);
                        settingsModel.Add(st2);
                    }
                }
                else
                {
                    CAutomaticModel st1 = new CAutomaticModel();
                    CAutomaticModel st2 = new CAutomaticModel();
                    settingsModel.Add(st1);
                    settingsModel.Add(st2);
                }
            }
            catch (Exception)
            {
                CAutomaticModel st1 = new CAutomaticModel();
                CAutomaticModel st2 = new CAutomaticModel();
                settingsModel.Add(st1);
                settingsModel.Add(st2);
            }
            return settingsModel;
        }

        #endregion

    }
}
