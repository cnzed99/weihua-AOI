using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace 断面毛刺检测软件.Views
{
    public static class OfflineTestExtend
    {
       
        private static readonly object _lockobj = new object();
        public static void GetImageExcute(this Cell cell, bool fromFile = false, int angle = 0)
        {
            try
            {
                lock (_lockobj)
                {
                    if (fromFile)//非运行模式，调试模式
                    {
                        if (File.Exists(cell.ImageFile))
                        {
                            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(cell.ImageFile));

                            cell.Image = memoryStream;
                            return;
                        }

                        throw new Exception("图像文件不存在：" + cell.ImageFile);
                    }

                }


            }
            catch (Exception e)
            {
                
                throw e;
            }

            //Task.Delay(200);
            //采集图片。。。
        }
    }
}
