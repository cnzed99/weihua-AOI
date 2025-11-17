using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunicationModule;
using Modbus;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace ZipperInfo
{

    public class CZipperCommunicate
    {
        #region 静态方法

        public static CLogRec ZipperSetResultLogger { get; set; } = CLogRec.Create("SetResult", "D:/Data");
        static object lockobj = new object();
        public static CModbusCommPart com;
        /// <summary>
        /// 获取拉链的ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="productID">产品ID</param>
        /// <param name="photoID">图片ID</param>
        //public static void GetID(out int productID, out int photoID)
        public static void GetID(out int productID)
        {
            //try
            //{
            lock (lockobj)
            {
                productID = -1;
                // photoID = -1;
                if (com != null)
                {
                    productID = com.ReadHoldingRegisterInt32(41192);
                    // photoID = com.ReadHoldingRegisterInt32(41194);
                }
            }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }
        /// <summary>
        /// 获取左相机拉头图片位置ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="pullID"></param>
        public static void GetZuoPullID(out int pullID)
        {
            //try
            //{
            lock (lockobj)
            {
                pullID = -1;
                if (com != null)
                {
                    pullID = com.ReadHoldingRegisterInt16(41226);
                }
            }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }
        /// <summary>
        /// 获取右相机拉头图片位置ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="pullID"></param>
        public static void GetYouPullID(out int pullID)
        {
            //try
            //{
            lock (lockobj)
            {
                pullID = -1;
                if (com != null)
                {
                    pullID = com.ReadHoldingRegisterInt16(41227);
                }
            }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }
        /// <summary>
        /// 写入拉头图片位置ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="pullID"></param>
        public static void SendZuoPullID(short pullID)
        {
            //try
            //{
            lock (lockobj)
            {
                if (com != null)
                {
                    com.WriteSingleRegisterInt16(41226, pullID);
                }
            }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }

        /// <summary>
        /// 读取拉头触发的位置 
        /// 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetPullLocation()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadHoldingRegisterInt32(41198);
            }
            else
            {
                return -1;
            }
            //}
            //catch (Exception)
            //{
            //    return -1;
            //}

        }
        /// <summary>
        /// 写入拉头图片位置ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="pullID"></param>
        public static void SendYouPullID(short pullID)
        {
            //try
            //{
            lock (lockobj)
            {
                if (com != null)
                {
                    com.WriteSingleRegisterInt16(41227, pullID);
                }
            }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }

        /// <summary>
        /// 获取单条拉链拍照的总张数 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetPhotoCount()
        {
            //try
            //{
            lock (lockobj)
            {
                if (com != null)
                {
                    return com.ReadHoldingRegisterInt32(41200);
                }
                else
                {
                    return -1;
                }
            }
            //}
            //catch (Exception)
            //{
            //    return -1;
            //}
        }
        /// <summary>
        /// 写入拍照的总张数 2025-6-26 鲍赞宝
        /// </summary>
        public static void SendPhotoCount(int count)
        {
            //try
            //{

            if (com != null)
            {
                com.WriteSingleRegisterInt32(41200, count);
            }

            //}
            //catch (Exception)
            //{
            //    return -1;
            //}
        }
        /// <summary>
        /// 获取单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetZipperLenght()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadHoldingRegisterInt32(41202);

            }
            else
            {
                return -1;
            }
            //}
            //catch (Exception)
            //{
            //    return -1;
            //}

        }
        /// <summary>
        /// 写入单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="lenght">拉链长度 单位mm</param>
        public static void SendZipperLenght(float lenght)
        {
            //try
            //{
            if (com != null)
            {
                //根据拉链长度来判断是长拉链还是短拉链 大于180mm是长拉链，小于18cm是短拉链，长拉链使用位置钩针模式，短拉链使用缺口钩针模式
                //if (lenght>180)
                //{
                //    com.WriteSingleCoil(49418,true);
                //}
                //else
                //{
                //    com.WriteSingleCoil(49418, false);
                //}
                //根据拉链的长度自动计算钩针勾起的位置=拉链长度-30mm
                float fgoulenght = lenght - 30;
                int igoulenght = (int)fgoulenght * 100; //plc的单位转换问题
                int tlenght = (int)lenght * 10;
                com.WriteSingleRegisterInt32(41202, tlenght);
                com.WriteSingleRegisterInt32(41304, igoulenght);

                float NGLocation = (lenght + 115.0f) * 100;  //NG料的放料位置，根据拉链长度来计算
                if (NGLocation > 40000) //限制最大后退距离
                {
                    NGLocation = 40000; 
                }
                com.WriteSingleRegisterInt32(41306, (int)NGLocation);
            }

            //}
            //catch (Exception)
            //{
            //}

        }

        /// <summary>
        /// 写入拉头触发的位置 2025-5-29 鲍赞宝
        /// </summary>
        public static void SendPullLocation(int location)
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleRegisterInt32(41198, location);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        /// <summary>
        /// 向PLC写入结果
        /// </summary>
        /// <param name="result">OK:1 NG:2</param>
        public static void SendResult(string id, ZIPPERESULT result)
        {
            if (com != null)
            {
                com.WriteSingleRegisterInt32(41196, (int)result);
                ZipperSetResultLogger.Info($"发送ID:{id}->结果:{result}");
            }

        }
        /// <summary>
        /// 获取当前轴的位置坐标 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetGrippawlLocation()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadHoldingRegisterInt32(41230);
            }
            else
            {
                return -1;
            }
            //}
            //catch (Exception)
            //{
            //    return -1;
            //}

        }

        /// <summary>
        /// 获取设备的模式状态 
        /// 2025-8-25 鲍赞宝
        /// </summary>
        public static bool GetDeviceState()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadCoil(12);
            }
            else
            {
                return true;
            }
            //}
            //catch (Exception)
            //{
            //    return -1;
            //}

        }

        /// <summary>
        /// 写入触发的点位置,
        /// </summary>
        /// <param name="LocationPoints">触发的点位</param>
        /// <param name="triggerndex">在第几张后改变ID</param>

        /// <summary>
        /// 写入触发的点位置
        /// </summary>
        /// <param name="LocationPoints">触发拍照的点位</param>
        /// <param name="HandandtaliPoints">第一张图片和最后一张图片的点位</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="cahceCount">切断时,切刀到相机已经有几条拉链完了拍照</param>
        public static void SendPoints(List<float> LocationPoints, List<float> HandandtaliPoints, int cutoffIndex, int cahceCount)
        {
            List<ushort> address = new List<ushort>();
            int startaddress = 41338;
            address.Add((ushort)startaddress);
            for (int i = 1; i < 10; i++)
            {
                startaddress += 2;
                address.Add((ushort)startaddress);
            }
            if (com != null)
            {
                for (int k = 0; k < address.Count; k++)
                {
                    //int pos = (int)LocationPoints[k] * 10;
                    //转成脉冲
                    int plus = 0;
                    int a = 400000;
                    int b = 19050;
                    if (k <= LocationPoints.Count - 1)
                    {
                        plus = (int)LocationPoints[k] * a / b;
                    }
                    else
                    {
                        plus = 9999 * a / b;
                    }
                    com.WriteSingleRegisterInt32(address[k], plus);

                }
                com.WriteSingleRegisterInt32(41216, cutoffIndex); //写拉链
                com.WriteSingleRegisterInt32(41322, cahceCount); //写切刀到拉链之间缓存的拉链数量


                int handpos = 0;
                //int a = 400000;
                //int b = 19050;
                //plus = (int)LocationPoints[k] * a / b;

                handpos = (int)HandandtaliPoints[0] * 10;
                com.WriteSingleRegisterInt32(41248, handpos); //写拉链拍照下止的触发位 自动识别的时候用
                handpos = (int)HandandtaliPoints[1] * 10;
                com.WriteSingleRegisterInt32(41250, handpos); //写拉链拍照上止的触发位 自动识别的时候用


            }

        }

        #region 自动识别拉链
        /// <summary>
        /// 自动识别测试开始
        /// </summary>
        public static void TestStart()
        {
            if (com != null)
            {
                com.WriteSingleCoil(13, true);
            }
        }

        /// <summary>
        /// 第一阶段位置完成
        /// </summary>
        public static void FirststageFinsh()
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleCoil(20, true);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        /// <summary>
        /// 第二阶段完成
        /// </summary>
        //public static void SceondstageFinsh()
        //{
        //    //try
        //    //{
        //    if (com != null)
        //    {
        //        com.WriteSingleCoil(21, true);
        //    }

        //    //}
        //    //catch (Exception)
        //    //{
        //    //}

        //}
        /// <summary>
        /// 第三阶段完成
        /// </summary>
        public static void ThirdstageFinsh()
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleCoil(22, true);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        /// <summary>
        /// 自动识别测试完成
        /// </summary>
        public static void TestFinish()
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleCoil(23, true);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        /// <summary>
        /// 轴运动停止
        /// </summary>
        public static void AixtStop()
        {
            if (com != null)
            {
                com.WriteSingleCoil(24, true);
            }
        }
        /// <summary>
        /// 轴运动继续
        /// </summary>
        public static void AixtContinue(bool con)
        {
            if (com != null)
            {
                com.WriteSingleCoil(25, con);
            }
        }

        /// <summary>
        /// 停止拍照
        ///  2025-7-30鲍赞宝
        /// </summary>
        public static void CamTriggerStop()
        {
            if (com != null)
            {
                com.WriteSingleCoil(21, true);
            }
        }

        /// <summary>
        /// 设置PLC触发相机的拍照频率 
        /// 2025-7-30鲍赞宝
        /// </summary>
        public static void SendCamFPS(int time)
        {
            //try
            //{

            if (com != null)
            {
                com.WriteSingleRegisterInt32(41256, time);
            }

            //}
            //catch (Exception)
            //{
            //    return -1;
            //}
        }
        /// <summary>
        /// 设置拉链后退的距离 1000=1cm 
        /// 2025-9-18鲍赞宝
        /// </summary>
        //public static void SendWolkBack(int lenght)
        //{
        //    //try
        //    //{

        //    if (com != null)
        //    {
        //        com.WriteSingleRegisterInt32(41302, lenght);
        //    }

        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return -1;
        //    //}
        //}
        #endregion
        #endregion

        #region 实例

        public static readonly BoundedChannelOptions s_WaitIDchannelOptions =
    new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
        /// <summary>
        /// 等待ID队列
        /// </summary>
        public readonly Channel<ZipperID> m_WaitIDChannel = Channel.CreateBounded<ZipperID>(s_WaitIDchannelOptions);

        Thread WaitIDThread = null;

        bool Connend = false;
        public void IntThread()
        {
            Connend = true;
            WaitIDThread = new Thread(MonitoringID);
            WaitIDThread.IsBackground = true;
            WaitIDThread.Start();
        }

        int TempproductID = -1;
        int TempPullPos = -1;
        int pullIndex=0;
        private void MonitoringID()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (Connend)
            {
                try
                {
                    GetID(out int productID);
                    if (productID <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (productID != TempproductID)
                    {
                        TempproductID = productID;
                        List<float> copyPos = new List<float>();
                        int ipullpos = GetPullLocation(); //为了防止中途从触摸屏改掉拉头位置，所以时刻监控它的值在进行比较
                        if (ipullpos != TempPullPos)
                        {
                            TempPullPos = ipullpos;
                            float fpullpos = ipullpos / 10.0f;
                            for (int i = 0; i < CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos.Count; i++)
                            {
                                copyPos.Add(CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos[i]);
                            }
                            copyPos.Add(fpullpos);
                            copyPos.Sort();
                            if (CZipperAutomaticAlgorithm.ZipperInfo.TriggerType == 3)
                            {
                                float handpos = CZipperAutomaticAlgorithm.ZipperInfo.HandAndTaliPos[0];
                                int handIndex = copyPos.IndexOf(handpos);

                                List<float> taskpos = copyPos.Take(handIndex).ToList(); //头
                                List<float> splitpos = copyPos.Skip(handIndex).ToList(); //尾

                                splitpos.AddRange(taskpos);
                                pullIndex = splitpos.IndexOf(fpullpos);
                            }
                            else 
                            {
                                pullIndex = copyPos.IndexOf(fpullpos);
                            }
                       
                        }

                        List<int> idlist = new List<int>();
                        for (int i = 1; i <= CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos.Count; i++)
                        {
                            idlist.Add(i);
                        }
                        idlist.Insert(pullIndex, 100);
                        //string str="";
                        //for (int i = 0; i < idlist.Count; i++)
                        //{
                        //    str = str + $"第{i}点：{idlist[i]}";
                        //}

                        for (int i = 0; i < idlist.Count; i++)
                        {
                            ZipperID zipperID = new ZipperID(productID, idlist[i]);
                            m_WaitIDChannel.Writer.TryWrite(zipperID);
                        }
                    }

                }
                catch (Exception)
                {
                }
                Thread.Sleep(1);
            }
        }

        #endregion
    }

    public enum ZIPPERESULT
    {
        OK = 1,
        NG = 2

    }

    public struct ZipperID
    {
        /// <summary>
        /// 产品ID
        /// 2025-10-19鲍赞宝
        /// </summary>
        public int ProductID { get; set; } = 0;
        /// <summary>
        /// 图片ID
        /// 2025-10-19鲍赞宝
        /// </summary>
        public int PhotoID { get; set; } = 0;
        public ZipperID(int productID, int photoID)
        {
            ProductID = productID;
            PhotoID = photoID;
        }
        public ZipperID() { }

    }

}

