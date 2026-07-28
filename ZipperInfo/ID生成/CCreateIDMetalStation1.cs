using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    /// <summary>
    /// 三合一 1号工位ID生成类
    /// </summary>
    public class CCreateIDMetalStation1 : CCreateIDBase
    {
        /// <summary>
        /// 三合一ID生成
        /// ID监控线程，监控拉头位置，生成ID并发送出去
        /// </summary>
        public override void MonitoringID()
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
                        Idlist.Clear();
                        int count = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperTriggerPos.Count;
                        for (int i = 1; i <= count; i++)
                        {
                            Idlist.Add(i * 100); //100倍的ID用来区分高曝光和低曝光，1234为高曝光图片，100 200 300  400为低曝光图片
                            Idlist.Add(i); 
                           
                        }

                        string str = "";
                        for (int i = 0; i < Idlist.Count; i++)
                        {
                            str = str + $"第{i + 1}点;{Idlist[i]} ";
                        }
                        ZipperIDsORTLogger.Info(str);

                        for (int i = 0; i < Idlist.Count; i++)
                        {
                            ZipperID zipperID = new ZipperID(productID, Idlist[i]);
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
    }
}
