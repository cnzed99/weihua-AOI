using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    internal class CGetZipperTriggerPoint
    {
        /// <summary>
        /// 获取触发的点位置
        /// </summary>
        /// <param name="points">触发点位置</param>
        /// <param name="headandtalipoints">一条拉链中，第一张图片的触发位置和最后一张的出发位置</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="frontFinsshPos">切断时,切刀到相机已经有几条拉链完了拍照,影响NG OK分料</param>
        internal static void GetTriggerPoints(CAutomaticModel AutoData, out List<float> outpoints, out List<float> HeadandTalipoints, out int cutoffIndex, out int frontFinsshPos)
        {
            List<float> points = new List<float>();
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
                frontFinsshPos = frontzippers - 1;
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
                frontFinsshPos = frontzippers - 1;
                float firstpoint = 0;
                float start = 0;
                for (int i = 1; i < 20; i++)//第一个点
                {
                    start = netZipperhandle + AutoData.CcdWidth * i;
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

            cutoffIndex = points.Count - pullchange;

            outpoints = ProcessList(points);
        }




        /// <summary>
        /// 校验,两两之间不能接近小于10
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<float> ProcessList(List<float> input)
        {
            if (input == null || input.Count <= 2)
                return input;

            List<float> output = new List<float>(input);

            for (int i = 1; i < output.Count; i++)
            {
                float diff = output[i] - output[i - 1];
                if (diff < 10 && diff > 0)
                {
                    output[i - 1] -= 10;
                }
            }

            return output;
        }
    }
}
