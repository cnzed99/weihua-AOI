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
        internal static void GetTriggerPoints(CAutomaticModel AutoData, out List<float> outpoints, out List<float> HeadandTalipoints,
            out int cutoffIndex, out int frontFinsshPos, out int triggerType)
        {
            triggerType = 1;
            List<float> points = new List<float>();
            HeadandTalipoints = new List<float>();
            //***************测试
            //float tempdis = AutoData.ZipperLenght - AutoData.CcdWidth;
            //int discount = (int)(tempdis / 18.0f);
            //AutoData.DaoDitance = 815 - discount;
            // ************
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
                if (firstpoint < 1) //触发点不能为零，为零启动后不触发相机
                {
                    firstpoint = 1;
                }
                points.Add(firstpoint);
                HeadandTalipoints.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperhandle + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali)
                    {
                        float endpoint = netZipperTali - backLim;
                        if (endpoint < 1)
                        {
                            endpoint = 1;
                        }
                        points.Add(endpoint);
                        HeadandTalipoints.Add(endpoint);
                        break;
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        if (point < 1)
                        {
                            point = 1;
                        }
                        points.Add(point);
                    }
                }
                triggerType = 1;
            }
            else if (netZipperhandle <= frontLim && netZipperTali < backLim) //类2 //如果下一条拉链的头位置比上视野小并且尾比下视野位置小
            {
                //第一个点
                pullchange = 0;
                frontFinsshPos = frontzippers + 1;
                float firstpoint = netZipperTali - frontLim;
                if (firstpoint < 1) //触发点不能为零，为零启动后不触发相机
                {
                    firstpoint = 1;
                }
                points.Add(firstpoint);
                HeadandTalipoints.Add(firstpoint);
                //后续的点
                for (int i = 1; i < 20; i++)
                {
                    float nextpoint = netZipperTali + AutoData.CcdWidth * i;
                    if (nextpoint + AutoData.CcdWidth >= netZipperTali + AutoData.ZipperLenght)
                    {
                        float endpoint = netZipperTali + AutoData.ZipperLenght - backLim;
                        if (endpoint < 1)
                        {
                            endpoint = 1;
                        }
                        points.Add(endpoint);
                        HeadandTalipoints.Add(endpoint);
                        break;
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        if (point < 1)
                        {
                            point = 1;
                        }
                        points.Add(point);
                    }
                }
                triggerType = 2;
            }
            else if (netZipperhandle <= frontLim && netZipperTali >= backLim) //类3 //如果下一条拉链的头位置比上视野小并且尾比下视野位置大
            {
                int nextCount = 0;
                frontFinsshPos = frontzippers;
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
                            if (firstpoint < 1)
                            {
                                firstpoint = 1;
                            }
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
                        if (endpoint < 1)
                        {
                            endpoint = 1;
                        }
                        bool havefind = points.Contains(endpoint);
                        if (havefind)
                        {
                            continue;
                        }
                        else
                        {
                            points.Add(endpoint);
                            pullchange = points.Count;
                            break;
                        }
                    }
                    else
                    {
                        float point = nextpoint - frontLim;
                        if (point < 1)
                        {
                            point = 1;
                        }
                        points.Add(point);
                    }
                }

                for (int i = 0; i < nextCount; i++)
                {
                    float nexts = netZipperTali + AutoData.CcdWidth * i;
                    float point = nexts - frontLim;
                    if (point < 1)
                    {
                        point = 1;
                    }
                    points.Add(point);
                }
                float handpoint = netZipperTali - frontLim;
                float talipoint = netZipperTali + AutoData.ZipperLenght - AutoData.CcdWidth - frontLim;

                HeadandTalipoints.Add(handpoint);
                HeadandTalipoints.Add(talipoint);
                triggerType = 3;
            }
            outpoints = ProcessList(points, AutoData.ZipperLenght, HeadandTalipoints, triggerType);
            //cutoffIndex = points.Count - pullchange;
            cutoffIndex = pullchange;
            if (cutoffIndex == points.Count || points.Count == 1)
            {
                cutoffIndex = 0;
            }
        }

        /// <summary>
        /// 校验拉头位置
        /// </summary>
        internal static void CheckPullPos(List<float> points)
        {
            try
            {
                List<float> copyPoints = new List<float>();
                for (int i = 0; i < points.Count; i++)
                {
                    copyPoints.Add(points[i]);
                }
                float fpullpos = CZipperCommunicate.GetPullLocation(); //拉头位置 单位mm
                copyPoints.Add(fpullpos);
                copyPoints.Sort();
                int pullindex = copyPoints.IndexOf(fpullpos);
                if (pullindex == 0) //第一个
                {
                    float absvalue = Math.Abs(copyPoints[pullindex] - copyPoints[1]);
                    if (absvalue < 10)
                    {
                        int pos = (int)(copyPoints[1] - 10.0f);
                        if (pos < 0)
                        {
                            pos = 1;
                        }
                        CZipperCommunicate.SendPullLocation(pos);
                    }
                }
                else if (pullindex == copyPoints.Count - 1) //最后一个
                {
                    float absvalue = Math.Abs(copyPoints[pullindex] - copyPoints[pullindex - 1]);
                    if (absvalue < 10)
                    {
                        int pos = (int)(copyPoints[pullindex - 1] + 10.0f);
                        CZipperCommunicate.SendPullLocation(pos);
                    }
                }
                else //中间
                {
                    float absvalue = Math.Abs(copyPoints[pullindex] - copyPoints[pullindex - 1]);
                    if (absvalue < 10)
                    {
                        int pos = (int)(copyPoints[pullindex - 1] + 10.0f);
                        CZipperCommunicate.SendPullLocation(pos);
                    }
                    float absvalue1 = Math.Abs(copyPoints[pullindex] - copyPoints[pullindex + 1]);
                    if (absvalue1 < 10)
                    {
                        int pos = (int)(copyPoints[pullindex + 1] - 10.0f);
                        CZipperCommunicate.SendPullLocation(pos);
                    }
                }
            }
            catch (Exception)
            {

            }
        }



        /// <summary>
        /// 校验,两两之间不能接近小于10
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<float> ProcessList(List<float> input, float zipperLenght, List<float> HandAndTaliPos, int triggerType)
        {
            if (input == null)
                return null;
            if (input.Count == 2)
            {
                if (input[0] > input[1] - 10.0) //如果第一个点大于第二个点，说明拉链的长度小于114.那就只输出一个触发点
                {
                    input.RemoveAt(1);
                    return input;
                }
            }

            // List<float> output = new List<float>(input);
            float diff = 8;
            float lastdiff = zipperLenght - input[input.Count - 1];
            float subvalue = diff - lastdiff;
            List<float> output;
            if (lastdiff <= diff) //15为经验值
            {
                output = new List<float>(input);
                List<float> copyPos = new List<float>(input);
                List<float> zipperPos = new List<float>();
                copyPos.Sort();
                if (triggerType == 3)
                {

                    float handpos = HandAndTaliPos[0];
                    int handIndex = copyPos.IndexOf(handpos);
                    float lastvalue = input[input.Count - 1];
                    List<float> taskpos = copyPos.Take(handIndex).ToList(); //头
                    zipperPos = copyPos.Skip(handIndex).ToList(); //尾
                    zipperPos.AddRange(taskpos);

                    int lastIndex = zipperPos.IndexOf(lastvalue);
                    List<float> lastLists = new List<float>();
                    for (int i = lastIndex; i < zipperPos.Count - 1; i++)
                    {
                        lastLists.Add(zipperPos[i]);
                    }
                    for (int i = 0; i < lastLists.Count; i++)
                    {
                        int index = output.IndexOf(lastLists[i]);
                        float subtemp = output[index] - subvalue;
                        output[index] = subtemp;
                        if (output[index] < 1)
                        {
                            output[index] = 1;
                        }
                    }

                }
                else
                {
                    output = new List<float>(input);
                    output[output.Count - 1] = zipperLenght - diff;
                }
            }
            else
            {
                output = new List<float>(input);
            }

            // return output;


            for (int i = 1; i < output.Count; i++) //如果最后一个点离它前面一个点很近就不触发，需要把前一个点前移
            {
                float diff2 = output[i] - output[i - 1];
                if (diff2 < 10 && diff2 >= 0)
                {
                    output[i - 1] -= (10 - diff2);
                    if (output[i - 1] < 1)
                    {
                        output[i - 1] = 1;
                    }
                }
            }
            //float sub = 5; //鲍赞宝 20260515 判断每一个触发点离终点的距离不能太近，
            //               //太近有可能走不到位不触发，导致少触发，减少5mm确保走到位触发
            //for (int i = 0; i < output.Count; i++)
            //{
            //    float newsub = zipperLenght - sub;
            //    if (output[i] > newsub)
            //    {
            //        output[i] = newsub;
            //    }
            //}
            return output;
        }
    }
}
