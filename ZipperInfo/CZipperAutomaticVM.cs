using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        }

        bool startAutoTest;

       public  Action<bool> StartAutoTestEven;

        [RelayCommand]
        void SendPoints()
        {
            CZipperAutomaticAlgorithm.AutoLogger.Info($"开始识别,设置拉链长度{AutoData.ZipperLenght}");
            //写入拉链长度
            CZipperCommunicate.SendZipperLenght(AutoData.ZipperLenght);

            GetTriggerPoints(out List<float> points,out List<float> handandtalipoints, out int cutoffIndex,out int zipperCacheCount);
            StringBuilder stringBuilder = new StringBuilder("计算触发点位");
            for (int i = 0; i < points.Count; i++)
            {
                stringBuilder.Append($"第{i + 1}点:{points[i]},");
            }
            if (handandtalipoints.Count>1)
            {
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
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperLneght = AutoData.ZipperLenght;
                CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos = points;
                CZipperAutomaticAlgorithm.ZipperInfo.CutoffIndex = cutoffIndex;
                CZipperAutomaticAlgorithm.ZipperInfo.HandAndTaliPos = handandtalipoints;
                CZipperAutomaticAlgorithm.TestFinsh = false;
                CZipperAutomaticAlgorithm.onWichStage = 1;
                CZipperAutomaticAlgorithm.findPuller = false;
                CZipperAutomaticAlgorithm.findPulls = false;
                CZipperAutomaticAlgorithm.findLogo = false;
                StartAutoTestEven?.Invoke(startAutoTest);
                //将光源值先减小到较状态

                if (CLinghtManagement.LightControlDict.Count > 0)
                {

                    foreach (var item in CLinghtManagement.LightControlDict.Values)
                    {
                        Thread.Sleep(20);
                        item.BaseConfig.LightChannelList[0].Value = 5;
                        item.SetChannelValue(item.BaseConfig.LightChannelList[0]);
                        Thread.Sleep(20);
                        item.BaseConfig.LightChannelList[1].Value = 20;
                        item.SetChannelValue(item.BaseConfig.LightChannelList[1]);
                    }

                }
                Thread.Sleep(100);
                CZipperCommunicate.TestStart();
                SaveParameter(AutoData);

            }
        }


        /// <summary>
        /// 获取触发的点位置
        /// </summary>
        /// <param name="points">触发点位置</param>
        /// <param name="headandtalipoints">一条拉链中，第一张图片的触发位置和最后一张的出发位置</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="frontFinsshPos">切断时,切刀到相机已经有几条拉链完了拍照,影响NG OK分料</param>
        private void GetTriggerPoints(out List<float>points,out List<float> HeadandTalipoints, out int cutoffIndex, out int frontFinsshPos)
        {
            points = new List<float>();
            HeadandTalipoints = new List<float>();
            float frontLim = AutoData.DaoDitance - AutoData.CcdWidth / 2.0f;
            float backLim = AutoData.DaoDitance + AutoData.CcdWidth / 2.0f;

            int frontzippers = (int)(AutoData.DaoDitance / AutoData.ZipperLenght); //中间有几条完整的拉链
            //double yuLenght = DaoDitance % ZipperLenght;

            int nextzippewr = frontzippers + 1;
            frontFinsshPos = 0;
            float netZipperhandle = AutoData.ZipperLenght * frontzippers; //下一条拉链的头位置
            float netZipperTali = AutoData.ZipperLenght * nextzippewr; // 下一条拉链的尾位置

            int pullchange = 0; //在第几张之前切换ID

            if (netZipperhandle >= frontLim && netZipperTali > backLim)//类1 //如果下一条拉链的头位置比上视野大并且尾比下视野位置大
            {

                //第一个点
                pullchange = 0;
                frontFinsshPos = frontzippers-1;
                float firstpoint = netZipperhandle - frontLim;
                points.Add(firstpoint);
                HeadandTalipoints.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperhandle + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali)
                    {
                        float endpoint = netZipperTali - backLim;
                        points.Add(endpoint);
                        HeadandTalipoints.Add(endpoint);
                        break;
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        points.Add(point);
                    }
                }
            }
            else if (netZipperhandle <= frontLim && netZipperTali < backLim) //类2 //如果下一条拉链的头位置比上视野小并且尾比下视野位置小
            {
                //第一个点
                pullchange = 0;
                frontFinsshPos = frontzippers;
                float firstpoint = netZipperTali - frontLim;
                points.Add(firstpoint);
                HeadandTalipoints.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperTali + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali + AutoData.ZipperLenght)
                    {
                        float endpoint = netZipperTali + AutoData.ZipperLenght - backLim;
                        points.Add(endpoint);
                        HeadandTalipoints.Add(endpoint);
                        break;
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        points.Add(point);
                    }
                }
            }
            else if (netZipperhandle <= frontLim && netZipperTali >= backLim) //类3 //如果下一条拉链的头位置比上视野小并且尾比下视野位置大
            {
                int nextCount = 0;
                frontFinsshPos = frontzippers-1;
                float firstpoint = 0;
                float start = 0;
                for (int i = 1; i < 20; i++)//第一个点
                {
                    start = netZipperhandle +   AutoData.CcdWidth * i;
                    if (start > frontLim)
                    {
                        nextCount = i;
                        if (start + AutoData.CcdWidth > netZipperTali)
                        {
                            break;
                        }
                        else
                        {
                            firstpoint = start - frontLim;
                            points.Add(firstpoint);
                            break;
                        }
                    }
                }

                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = start + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali)
                    {

                        float endpoint = netZipperTali - backLim;
                        points.Add(endpoint);
                        pullchange = points.Count;
                        break;
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        points.Add(point);
                    }
                }

                for (int i = 0; i < nextCount; i++)
                {
                    float nexts = netZipperTali + AutoData.CcdWidth * i;
                    float point = nexts - frontLim;
                    points.Add(point);
                }
                float handpoint = netZipperTali - frontLim;
                float talipoint = netZipperTali + AutoData.ZipperLenght - AutoData.CcdWidth - frontLim;

                HeadandTalipoints.Add(handpoint);
                HeadandTalipoints.Add(talipoint);

            }

            cutoffIndex= points.Count- pullchange;
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
