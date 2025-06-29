using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;

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

       public  Action<bool,List<float>,int> StartAutoTestEven;

        [RelayCommand]
        void SendPoints()
        {
            //写入拉链长度
            CZipperCommunicate.SendZipperLenght(AutoData.ZipperLenght);

            GetTriggerPoints(out List<float> points, out int cutoffIndex,out int zipperCacheCount);
           
            if (points != null && points.Count > 0)
            {
                //写入拍照的总图片数量
                CZipperCommunicate.SendPhotoCount(points.Count);
                //计算拉链触发点位 ID改变位置
                CZipperCommunicate.SendPoints(points, cutoffIndex,zipperCacheCount);               
                Thread.Sleep(100);
                startAutoTest=true;
                StartAutoTestEven?.Invoke(startAutoTest, points, cutoffIndex);
                //将光源值先减小到较状态
                Thread.Sleep(100);
               // CZipperCommunicate.TestStart();
                SaveParameter(AutoData);

            }
        }

        /// <summary>
        /// 获取触发的点位置
        /// </summary>
        /// <param name="points">触发点位置</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="frontFinsshPos">切断时,切刀到相机已经有几条拉链完了拍照</param>
        private void GetTriggerPoints(out List<float>points,out int cutoffIndex, out int frontFinsshPos)
        {
            points = new List<float>();
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
                frontFinsshPos = frontzippers;
                float firstpoint = netZipperhandle - frontLim;
                points.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperhandle + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali)
                    {
                        float endpoint = netZipperTali - backLim;
                        points.Add(endpoint);
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
                frontFinsshPos = frontzippers + 1;
                float firstpoint = netZipperTali - frontLim;
                points.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperTali + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali + AutoData.ZipperLenght)
                    {
                        float endpoint = netZipperTali + AutoData.ZipperLenght - backLim;
                        points.Add(endpoint);
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
                frontFinsshPos = frontzippers;
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
