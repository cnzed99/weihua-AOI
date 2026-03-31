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

    public class CZipperCommunicateBase
    {
        #region 静态方法

        public static CLogRec ZipperSetResultLogger { get; set; } = CLogRec.Create("SetResult", "D:/Data");
        public static CLogRec ZipperIDsORTLogger { get; set; } = CLogRec.Create("IDSort", "D:/Data");
        //static object lockobj = new object();
        //public CCommunicationBase com;
        /// <summary>
        /// 获取拉链的ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="productID">产品ID</param>
        /// <param name="photoID">图片ID</param>
        //public static void GetID(out int productID, out int photoID)
        public virtual void GetID(out int productID)
        {
            productID = -1;
        }

        /// <summary>
        /// 读取拉头触发的位置 
        /// 2025-5-29 鲍赞宝
        /// </summary>
        public virtual float GetPullLocation()
        {
            return -1;
        }


        /// <summary>
        /// 获取单条拉链拍照的总张数 2025-5-29 鲍赞宝
        /// </summary>
        public virtual int GetPhotoCount()
        {
            return -1;
        }
        /// <summary>
        /// 写入拍照的总张数 2025-6-26 鲍赞宝
        /// </summary>
        public virtual void SendPhotoCount(int count) { }

        /// <summary>
        /// 获取单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        public virtual int GetZipperLenght()
        {
            return -1;
        }
        /// <summary>
        /// 写入单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="lenght">拉链长度 单位mm</param>
        public virtual void SendZipperLenght(float lenght) { }

        /// <summary>
        /// 清除PLC报警
        /// </summary>
        public virtual void ClearWarn() { }

        /// <summary>
        /// 写入拉头触发的位置 2025-5-29 鲍赞宝
        /// </summary>
        public virtual void SendPullLocation(float location) { }

        /// <summary>
        /// 向PLC写入结果
        /// </summary>
        /// <param name="result">OK:1 NG:2</param>
        public virtual void SendResult(string id, ZIPPERESULT result) { }

        /// <summary>
        /// 获取当前轴的位置坐标 2025-5-29 鲍赞宝
        /// </summary>
        public virtual float GetGrippawlLocation() { return -1; }


        /// <summary>
        /// 获取设备的模式状态 
        /// 2025-8-25 鲍赞宝
        /// </summary>
        public virtual bool GetDeviceState() { return false; }

        /// <summary>
        /// 写入触发的点位置
        /// </summary>
        /// <param name="LocationPoints">触发拍照的点位</param>
        /// <param name="HandandtaliPoints">第一张图片和最后一张图片的点位</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="cahceCount">切断时,切刀到相机已经有几条拉链完了拍照</param>
        public virtual void SendPoints(List<float> LocationPoints, List<float> HandandtaliPoints, int cutoffIndex, int cahceCount)
        {

        }

        /// <summary>
        /// 写入合链起始位
        /// </summary>
        public virtual void SendHelianStastPos(float pos) { }


        /// <summary>
        /// 写入合链终点位
        /// </summary>
        public virtual void SendHelianEndPos(float pos) { }


        #region 自动识别拉链
        /// <summary>
        /// 自动识别测试开始
        /// </summary>
        public virtual void TestStart() { }

        /// <summary>
        /// 第一阶段位置完成
        /// </summary>
        public virtual void FirststageFinsh() { }

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
        //public static void ThirdstageFinsh()
        //{
        //    //try
        //    //{
        //    if (com != null)
        //    {
        //        com.WriteSingleCoil(22, true);
        //    }

        //    //}
        //    //catch (Exception)
        //    //{
        //    //}

        //}
        /// <summary>
        /// 自动识别测试完成
        /// </summary>
        public virtual void TestFinish() { }

        /// <summary>
        /// 轴运动停止
        /// </summary>
        public virtual void AixtStop() { }

        /// <summary>
        /// 轴运动继续
        /// </summary>
        public virtual void AixtContinue(bool con) { }


        /// <summary>
        /// 停止拍照
        ///  2025-7-30鲍赞宝
        /// </summary>
        public virtual void CamTriggerStop() { }


        /// <summary>
        /// 设置PLC触发相机的拍照频率 
        /// 2025-7-30鲍赞宝
        /// </summary>
        public virtual void SendCamFPS(int time) { }

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

       public bool Connend = false;
        public void IntThread()
        {
            Connend = true;
            WaitIDThread = new Thread(MonitoringID);
            WaitIDThread.IsBackground = true;
            WaitIDThread.Start();
        }

        public int TempproductID = -1;
        public float TempPullPos = -1.0f;
        public int pullIndex = 0;
        public List<int> Idlist = new List<int>();
        public virtual void MonitoringID()
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
                        float ipullpos = GetPullLocation(); //为了防止中途从触摸屏改掉拉头位置，所以时刻监控它的值在进行比较
                        if (ipullpos != TempPullPos)
                        {
                            Idlist.Clear();
                            TempPullPos = ipullpos;
                            float fpullpos = ipullpos;
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
                            for (int i = 1; i <= CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos.Count; i++)
                            {
                                Idlist.Add(i);
                            }
                            Idlist.Insert(pullIndex, 100);
                            string str = "";
                            for (int i = 0; i < Idlist.Count; i++)
                            {
                                str = str + $"第{i + 1}点;{Idlist[i]} ";
                            }
                            ZipperIDsORTLogger.Info(str);
                        }

                        for (int i = 0; i < Idlist.Count; i++)
                        {
                            ZipperID zipperID = new ZipperID(productID, Idlist[i]);
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

