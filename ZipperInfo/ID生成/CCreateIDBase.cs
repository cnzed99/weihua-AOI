using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WH.Entity.LogRecord;

namespace ZipperInfo
{
    public class CCreateIDBase
    {

        public static CLogRec ZipperIDsORTLogger { get; set; } = CLogRec.Create("IDSort", "D:/Data");
       // public static readonly BoundedChannelOptions s_WaitIDchannelOptions =
   // new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
        /// <summary>
        /// 等待ID队列
        /// </summary>
      //  public readonly Channel<ZipperID> m_WaitIDChannel = Channel.CreateBounded<ZipperID>(s_WaitIDchannelOptions);

        Thread WaitIDThread = null;

        public bool Connend = false;
        /// <summary>
        /// ID数据传出事件
        /// </summary>
        public event EventHandler<ZipperID> IDSendEvent;
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
                    CZipperCommunicate.GetID(out int productID);
                    if (productID <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (productID != TempproductID)
                    {
                        TempproductID = productID;
                        List<float> copyPos = new List<float>();
                        float ipullpos = CZipperCommunicate.GetPullLocation(); //为了防止中途从触摸屏改掉拉头位置，所以时刻监控它的值在进行比较
                        if (ipullpos != TempPullPos)
                        {
                            Idlist.Clear();
                            TempPullPos = ipullpos;
                            float fpullpos = ipullpos;
                            for (int i = 0; i < CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperTriggerPos.Count; i++)
                            {
                                copyPos.Add(CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperTriggerPos[i]);
                            }
                            copyPos.Add(fpullpos);
                            copyPos.Sort();
                            if (CZipperAutomaticAlgorithm.ZipperInfo.TempData1.TriggerType == 3)
                            {
                                float handpos = CZipperAutomaticAlgorithm.ZipperInfo.TempData1.HandAndTaliPos[0];
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
                            for (int i = 1; i <= CZipperAutomaticAlgorithm.ZipperInfo.TempData1.ZipperTriggerPos.Count; i++)
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
                            // m_WaitIDChannel.Writer.TryWrite(zipperID);
                            SendBaseEven(zipperID);
                        }

                    }

                }
                catch (Exception)
                {
                }
                Thread.Sleep(1);
            }
        }

        protected virtual void SendBaseEven(ZipperID zipperID)
        {
            IDSendEvent?.Invoke(null, zipperID);
        }

    }
}
