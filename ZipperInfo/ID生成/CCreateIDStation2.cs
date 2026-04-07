using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public class CCreateIDStation2: CCreateIDBase
    {
        public override void MonitoringID()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (Connend)
            {
                try
                {
                    CZipperCommunicate.GetID2(out int productID);
                    if (productID <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (productID != TempproductID)
                    {
                        TempproductID = productID;
                        List<float> copyPos = new List<float>();
                        float ipullpos = CZipperCommunicate.GetPullLocation2(); //为了防止中途从触摸屏改掉拉头位置，所以时刻监控它的值在进行比较
                        if (ipullpos != TempPullPos)
                        {
                            Idlist.Clear();
                            TempPullPos = ipullpos;
                            float fpullpos = ipullpos;
                            for (int i = 0; i < CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperTriggerPos.Count; i++)
                            {
                                copyPos.Add(CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperTriggerPos[i]);
                            }
                            copyPos.Add(fpullpos);
                            copyPos.Sort();
                            if (CZipperAutomaticAlgorithm.ZipperInfo.TempData2.TriggerType == 3)
                            {
                                float handpos = CZipperAutomaticAlgorithm.ZipperInfo.TempData2.HandAndTaliPos[0];
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
                            for (int i = 1; i <= CZipperAutomaticAlgorithm.ZipperInfo.TempData2.ZipperTriggerPos.Count; i++)
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
    }
}
