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
    public partial class CZipperAutomaticVM: ObservableObject
    {
        public CAutomaticModel AutoData { get; set; }

        public CZipperAutomaticVM()
        {
            AutoData= LoadParameter();
            CZipperAutomaticAlgorithm.ZipperInfo.AutoData = AutoData;
        }

        bool startAutoTest;

       public  Action<bool> StartAutoTestEven;

        [RelayCommand]
        void SendPoints(object win)
        {

            CZipperAutomaticAlgorithm.AutoLogger.Info($"开始识别,设置拉链长度{AutoData.ZipperLenght}");
            //写入拉链长度
            CZipperCommunicate.ClearWarn();
            CZipperCommunicate.SendZipperLenght(AutoData.ZipperLenght);

           CGetZipperTriggerPoint.GetTriggerPoints(AutoData,out List<float> points,out List<float> handandtalipoints, out int cutoffIndex,out int zipperCacheCount,out int triggerType);
            StringBuilder stringBuilder = new StringBuilder("计算触发点位");
            for (int i = 0; i < points.Count; i++)
            {
                stringBuilder.Append($"第{i + 1}点:{points[i]},");
            }
            if (handandtalipoints.Count>1)
            {
                CZipperAutomaticAlgorithm.FirstPosTemp = handandtalipoints[0];
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
                CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex,zipperCacheCount);               
                Thread.Sleep(100);
                startAutoTest=true;
                CZipperCommunicate.AixtContinue(false);
               // CZipperCommunicate.SendWolkBack(AutoData.WalkBackLenght_slow);
                CZipperAutomaticAlgorithm.ZipperInfo.AutoData = AutoData;
                // CZipperAutomaticAlgorithm.ZipperInfo.ZipperLneght = AutoData.ZipperLenght;
                CZipperAutomaticAlgorithm.ZipperInfo.ShowZipperLenght = AutoData.ShowZipperLenght;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos = points;
                CZipperAutomaticAlgorithm.ZipperInfo.CutoffIndex = cutoffIndex;
                CZipperAutomaticAlgorithm.ZipperInfo.HandAndTaliPos = handandtalipoints;
                CZipperAutomaticAlgorithm.ZipperInfo.TriggerType = triggerType;
                CZipperAutomaticAlgorithm.TestFinsh = false;
                CZipperAutomaticAlgorithm.onWichStage = 1;
                CZipperAutomaticAlgorithm.findPuller = false;
                CZipperAutomaticAlgorithm.findPulls = false;
                CZipperAutomaticAlgorithm.findLogo = false;
                CZipperAutomaticAlgorithm.findUpMass= false;
                CZipperAutomaticAlgorithm.findDownMass = false;
                CZipperAutomaticAlgorithm.findlianya = false;
                CZipperAutomaticAlgorithm.findUpMassCount = 0;
                CZipperAutomaticAlgorithm.findDownMassCount = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperDownmssImg=null;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperUpmssImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperPullerImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperPullsImg = null;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperPullerCX = 0;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperPullerCY = 0;
                CZipperAutomaticAlgorithm.tempLightValue_zuo_change1 = 0;
                CZipperAutomaticAlgorithm.tempLightValue_zuo_change2 = 0;
                CZipperAutomaticAlgorithm.tempLightValue_you_change1 = 0;
                CZipperAutomaticAlgorithm.tempLightValue_you_change2 = 0;
                CZipperAutomaticAlgorithm.zuo_lightOK=false;
                CZipperAutomaticAlgorithm.you_lightOK = false;
                CZipperAutomaticAlgorithm.findLogosidertype[0]=false;
                CZipperAutomaticAlgorithm.findLogosidertype[1] = false;
                CZipperAutomaticAlgorithm.ZipperInfo.FindLogoSider = 0;
                CZipperAutomaticAlgorithm.findPullerCount = 0;
                CZipperAutomaticAlgorithm.findPullsCount = 0;
                CZipperCommunicate.SendHelianStastPos((int)(AutoData.ZipperLenght-36) * 10);
                CZipperCommunicate.SendHelianEndPos((int)(AutoData.ZipperLenght - 36) * 10);
                CZipperAutomaticAlgorithm.AutoSettingPosFinsh=false;
                CZipperAutomaticAlgorithm.onWichStage2 = 1;
               
                //将光源值先减小到较状态

                if (CLinghtManagement.LightControlDict.Count > 0)
                {

                    foreach (var item in CLinghtManagement.LightControlDict.Values)
                    {
                        Thread.Sleep(20);
                        item.BaseConfig.LightChannelList[0].Value = 20;
                        item.SetChannelValue(item.BaseConfig.LightChannelList[0]);
                        Thread.Sleep(20);
                        item.BaseConfig.LightChannelList[1].Value = 20;
                        item.SetChannelValue(item.BaseConfig.LightChannelList[1]);
                    }

                }
                Thread.Sleep(100);

                CZipperCommunicate.SendCamFPS(300); //起始300ms触发一次
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



        public static string ParameterPath = "..\\SystemConfig\\ZipperAutoData.Json";

        #region 保存参数

        public static void SaveParameter(CAutomaticModel data)
        {
            try
            {
                ConfigAPI.Save(data, ParameterPath);
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数

        public static CAutomaticModel LoadParameter()
        {
            CAutomaticModel settingsModel = new CAutomaticModel();
            try
            {
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.Load<CAutomaticModel>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new CAutomaticModel();
                    }
                }
                else
                {
                    settingsModel = new CAutomaticModel();
                }
            }
            catch (Exception)
            {
                settingsModel = new CAutomaticModel();
            }
            return settingsModel;
        }

        #endregion

    }
}
