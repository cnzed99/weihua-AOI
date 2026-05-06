using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public class CCreateIDStation3: CCreateIDBase
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
                        ZipperID zipperID = new ZipperID(productID, 1);
                        SendBaseEven(zipperID);
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
