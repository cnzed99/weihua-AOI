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
            //lock (lockobj)
            //{
                productID = -1;
                // photoID = -1;
                if (com != null)
                {
                    productID = com.ReadHoldingRegisterInt32(42292);
                }
           // }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }
        /// <summary>
        /// 工位2 产品ID
        /// </summary>
        /// <param name="productID"></param>
        public static void GetID2(out int productID)
        {
            //try
            //{
            //lock (lockobj)
            //{
                productID = -1;
                if (com != null)
                {
                    productID = com.ReadHoldingRegisterInt32(42310);
                }
           // }
            //}
            //catch (Exception)
            //{
            //    productID = -1;
            //    photoID = -1;
            //}


        }
        /// <summary>
        /// 工位3 产品ID
        /// </summary>
        /// <param name="productID"></param>
        public static void GetID3(out int productID)
        {
            //try
            //{
            //lock (lockobj)
            //{
                productID = -1;
                if (com != null)
                {
                    productID = com.ReadHoldingRegisterInt32(42318);
                }
            //}
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
        public static float GetPullLocation()
        {
            ////try
            ////{
            //if (com != null)
            //{
            //    return com.ReadHoldingRegisterReal(41198);
            //}
            //else
            //{
            //    return -1;
            //}
            ////}
            ////catch (Exception)
            ////{
            ////    return -1;
            ////}
            return -1;
        }
        /// <summary>
        /// 读取拉头触发的位置 
        /// 2025-5-29 鲍赞宝
        /// </summary>
        public static float GetPullLocation2()
        {
            ////try
            ////{
            //if (com != null)
            //{
            //    return com.ReadHoldingRegisterReal(41418);
            //}
            //else
            //{
            //    return -1;
            //}
            ////}
            ////catch (Exception)
            ////{
            ////    return -1;
            ////}

            return -1;
        }


        /// <summary>
        /// 获取单条拉链拍照的总张数 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetPhotoCount()
        {
            //try
            //{
            //lock (lockobj)
            //{
                if (com != null)
                {
                    int count = com.ReadHoldingRegisterInt32(42300);
                    return count * 2;
                    //return count;
                }
                else
                {
                    return -1;
                }
           // }
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
                com.WriteSingleRegisterInt32(42300, count);
                // com.WriteSingleRegisterInt32(41424, count);
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
        public static float GetZipperLenght()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadHoldingRegisterInt32(41658)/10.0f;

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
                //根据拉链的长度自动计算钩针勾起的位置=拉链长度-50mm
                // float fgoulenght = lenght - 36;
                com.WriteSingleRegisterReal(42304, lenght);
                // com.WriteSingleRegisterReal(41202, lenght); //工位1

                //float NGLocation = lenght + 100.0f;  //NG料的放料位置，根据拉链长度来计算
                //if (NGLocation > 4000) //限制最大后退距离
                //{
                //    NGLocation = 4000;
                //}
                //com.WriteSingleRegisterReal(41306, NGLocation);

                // SendHelianEndPos(tlenght);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        public static void ClearWarn()
        {
            if (com != null)
            {
                com.WriteSingleCoil(43, true);  //先清除报警
                Thread.Sleep(100);
                com.WriteSingleCoil(43, false);  //先清除报警
            }

        }
        /// <summary>
        /// 写入拉头触发的位置 2025-5-29 鲍赞宝
        /// </summary>
        public static void SendPullLocation(float location)
        {
            //try
            //{
            //if (com != null)
            //{
            //    com.WriteSingleRegisterReal(41198, location);
            //}

            //}
            //catch (Exception)
            //{
            //}

        }
        public static void SendPullLocation2(float location)
        {
            //try
            //{
            //if (com != null)
            //{
            //    com.WriteSingleRegisterReal(41418, location);
            //}

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
                com.WriteSingleRegisterInt32(42296, (int)result);
                ZipperSetResultLogger.Info($"工位1发送ID:{id}->结果:{result}");
            }

        }
        /// <summary>
        /// 工位2 写入结果
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        public static void SendResult2(string id, ZIPPERESULT result)
        {
            if (com != null)
            {
                com.WriteSingleRegisterInt32(42314, (int)result);
                ZipperSetResultLogger.Info($"工位2发送ID:{id}->结果:{result}");
            }

        }

        /// <summary>
        /// 工位3 写入结果
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        public static void SendResult3(string id, ZIPPERESULT result)
        {
            if (com != null)
            {
                com.WriteSingleRegisterInt32(42322, (int)result);
                ZipperSetResultLogger.Info($"工位3发送ID:{id}->结果:{result}");
            }

        }
        /// <summary>
        /// 获取当前轴的位置坐标 2025-5-29 鲍赞宝
        /// </summary>
        //public static float GetGrippawlLocation()
        //{
        //    //try
        //    //{
        //    //if (com != null)
        //    //{
        //    //    return com.ReadHoldingRegisterReal(1100);
        //    //}
        //    //else
        //    //{
        //    //    return -1;
        //    //}
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return -1;
        //    //}

        //}

        /// <summary>
        /// 获取设备的模式状态 
        /// 2025-8-25 鲍赞宝
        /// </summary>
        //public static bool GetDeviceState()
        //{
        //    //try
        //    //{
        //    if (com != null)
        //    {
        //        return com.ReadCoil(12);
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return -1;
        //    //}

        //}

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
            int startaddress = 42338;
            address.Add((ushort)startaddress);
            for (int i = 1; i < 15; i++)
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
                    //int plus = 0;
                    //int a = 400000;
                    //int b = 19050;

                    float plus = 0;
                    if (k <= LocationPoints.Count - 1)
                    {
                        plus = (int)LocationPoints[k];// * a / b;
                    }
                    else
                    {
                        plus = 99999; //* a / b;
                    }
                    com.WriteSingleRegisterReal(address[k], plus);

                }
                com.WriteSingleRegisterInt32(42306, cutoffIndex); //切断时已经拍了几张照片
                com.WriteSingleRegisterInt32(42308, cahceCount); //写切刀到拉链之间缓存的拉链数量

                float IDchangepoint = LocationPoints[cutoffIndex] - 12.0f;
                if (IDchangepoint <= 0)
                {
                    IDchangepoint = 0.5f;
                }
                com.WriteSingleRegisterReal(42488, IDchangepoint); //写改变ID触发的位置
                //int handpos = 0;
                ////int a = 400000;
                ////int b = 19050;
                ////plus = (int)LocationPoints[k] * a / b;

                //handpos = (int)HandandtaliPoints[0];
                //com.WriteSingleRegisterInt32(41248, handpos); //写拉链拍照下止的触发位 自动识别的时候用
                //handpos = (int)HandandtaliPoints[1];
                //com.WriteSingleRegisterInt32(41250, handpos); //写拉链拍照上止的触发位 自动识别的时候用


            }

        }

        public static void SendPoints2(List<float> LocationPoints, List<float> HandandtaliPoints, int cutoffIndex, int cahceCount)
        {
            //List<ushort> address = new List<ushort>();
            //int startaddress = 41438;
            //address.Add((ushort)startaddress);
            //for (int i = 1; i < 10; i++)
            //{
            //    startaddress += 2;
            //    address.Add((ushort)startaddress);
            //}
            //if (com != null)
            //{
            //    for (int k = 0; k < address.Count; k++)
            //    {
            //        //int pos = (int)LocationPoints[k] * 10;
            //        //转成脉冲
            //        //int plus = 0;
            //        //int a = 400000;
            //        //int b = 19050;

            //        float plus = 0;
            //        if (k <= LocationPoints.Count - 1)
            //        {
            //            plus = (int)LocationPoints[k];// * a / b;
            //        }
            //        else
            //        {
            //            plus = 99999; //* a / b;
            //        }
            //        com.WriteSingleRegisterReal(address[k], plus);

            //    }
            //    com.WriteSingleRegisterInt32(41434, cutoffIndex); //切断时已经拍了几张照片
            //    com.WriteSingleRegisterInt32(41436, cahceCount); //写切刀到拉链之间缓存的拉链数量


            //    //int handpos = 0;
            //    ////int a = 400000;
            //    ////int b = 19050;
            //    ////plus = (int)LocationPoints[k] * a / b;

            //    //handpos = (int)HandandtaliPoints[0];
            //    //com.WriteSingleRegisterInt32(41248, handpos); //写拉链拍照下止的触发位 自动识别的时候用
            //    //handpos = (int)HandandtaliPoints[1];
            //    //com.WriteSingleRegisterInt32(41250, handpos); //写拉链拍照上止的触发位 自动识别的时候用


            //}

        }

        /// <summary>
        /// 写入合链起始位
        /// </summary>
        public static void SendHelianStastPos(float pos)
        {

            //if (com != null)
            //{
            //    com.WriteSingleRegisterReal(41258, pos);
            //}
        }

        /// <summary>
        /// 写入合链终点位
        /// </summary>
        public static void SendHelianEndPos(float pos)
        {

            //if (com != null)
            //{
            //    com.WriteSingleRegisterReal(41260, pos);
            //}
        }

        #region 自动识别拉链
        /// <summary>
        /// 自动识别测试开始
        /// </summary>
        public static void TestStart()
        {
            if (com != null)
            {
                com.WriteSingleCoil(1200, true);
            }
        }

        /// <summary>
        /// 第一阶段位置完成
        /// </summary>
        //public static void FirststageFinsh()
        //{
        //    ////try
        //    ////{
        //    //if (com != null)
        //    //{
        //    //    com.WriteSingleCoil(1201, true);
        //    //}

        //    ////}
        //    ////catch (Exception)
        //    ////{
        //    ////}

        //}
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
        public static void TestFinish()
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleCoil(1203, true);
            }

            //}
            //catch (Exception)
            //{
            //}

        }
        /// <summary>
        /// 轴运动停止
        /// </summary>
        //public static void AixtStop()
        //{
        //    //if (com != null)
        //    //{
        //    //    com.WriteSingleCoil(24, true);
        //    //}
        //}
        /// <summary>
        /// 轴运动继续
        /// </summary>
        //public static void AixtContinue(bool con)
        //{
        //    //if (com != null)
        //    //{
        //    //    com.WriteSingleCoil(25, con);
        //    //}
        //}

        /// <summary>
        /// 停止拍照
        ///  2025-7-30鲍赞宝
        /// </summary>
        public static void CamTriggerStop()
        {
            if (com != null)
            {
                com.WriteSingleCoil(1202, true);
            }
        }

        /// <summary>
        /// 设置PLC触发相机的拍照频率 
        /// 2025-7-30鲍赞宝
        /// </summary>
        public static void SendCamFPS(int time)
        {
            ////try
            ////{

            if (com != null)
            {
                com.WriteSingleRegisterInt32(42288, time);
            }

            ////}
            ////catch (Exception)
            ////{
            ////    return -1;
            ////}
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
    }

    public enum ZIPPERESULT
    {
        OK = 1,
        NG = 2,
        NG2 = 3,
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

