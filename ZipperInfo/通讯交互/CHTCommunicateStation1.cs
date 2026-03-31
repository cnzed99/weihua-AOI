using CommunicationModule;
using Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WH.Entity.LogRecord;

namespace ZipperInfo
{
    public class CHTCommunicateStation1: CZipperCommunicateBase
    {
        /// <summary>
        /// Modbus 通讯
        /// </summary>
        public CModbusCommPart com;

        /// <summary>
        /// 获取拉链的ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="productID">产品ID</param>
        /// <param name="photoID">图片ID</param>
        //public static void GetID(out int productID, out int photoID)
        public override void GetID(out int productID)
        {
        
                productID = -1;

            if (com != null)
            {
                productID = com.ReadHoldingRegisterInt32(41192);

            }
        }
       

        /// <summary>
        /// 读取拉头触发的位置 
        /// 2025-5-29 鲍赞宝
        /// </summary>
        public override float GetPullLocation()
        {
          
            if (com != null)
            {
                return com.ReadHoldingRegisterReal(41198);
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// 获取单条拉链拍照的总张数 2025-5-29 鲍赞宝
        /// </summary>
        public override int GetPhotoCount()
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
        /// <summary>
        /// 写入拍照的总张数 2025-6-26 鲍赞宝
        /// </summary>
        public override void SendPhotoCount(int count)
        {
            if (com != null)
            {
                com.WriteSingleRegisterInt32(41200, count);
            }
        }
        /// <summary>
        /// 获取单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        public override int GetZipperLenght()
        {
            if (com != null)
            {
                return com.ReadHoldingRegisterInt32(41202);

            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// 写入单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="lenght">拉链长度 单位mm</param>
        public override void SendZipperLenght(float lenght)
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
                float fgoulenght = lenght - 36;
                //int igoulenght = (int)fgoulenght * 100; //plc的单位转换问题
                // int tlenght = (int)lenght * 10;
                com.WriteSingleRegisterReal(41202, lenght);
                com.WriteSingleRegisterReal(41304, fgoulenght);

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
        public override void ClearWarn()
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
        public override void SendPullLocation(float location)
        {
            //try
            //{
            if (com != null)
            {
                com.WriteSingleRegisterReal(41198, location);
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
        public override void SendResult(string id, ZIPPERESULT result)
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
        public override float GetGrippawlLocation()
        {
            //try
            //{
            if (com != null)
            {
                return com.ReadHoldingRegisterReal(1100);
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
        public override bool GetDeviceState()
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
        /// 写入触发的点位置
        /// </summary>
        /// <param name="LocationPoints">触发拍照的点位</param>
        /// <param name="HandandtaliPoints">第一张图片和最后一张图片的点位</param>
        /// <param name="cutoffIndex">切断时已经拍了几张照片</param>
        /// <param name="cahceCount">切断时,切刀到相机已经有几条拉链完了拍照</param>
        public override void SendPoints(List<float> LocationPoints, List<float> HandandtaliPoints, int cutoffIndex, int cahceCount)
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
                com.WriteSingleRegisterInt32(41216, cutoffIndex); //写拉链缓存数量（切断时切刀到相机位之间缓存的拉链数量）
                com.WriteSingleRegisterInt32(41322, cahceCount); //写切刀到拉链之间缓存的拉链数量


                int handpos = 0;
                //int a = 400000;
                //int b = 19050;
                //plus = (int)LocationPoints[k] * a / b;

                handpos = (int)HandandtaliPoints[0];
                com.WriteSingleRegisterInt32(41248, handpos); //写拉链拍照下止的触发位 自动识别的时候用
                handpos = (int)HandandtaliPoints[1];
                com.WriteSingleRegisterInt32(41250, handpos); //写拉链拍照上止的触发位 自动识别的时候用


            }

        }

        /// <summary>
        /// 写入合链起始位
        /// </summary>
        public override void SendHelianStastPos(float pos)
        {

            if (com != null)
            {
                com.WriteSingleRegisterReal(41258, pos);
            }
        }

        /// <summary>
        /// 写入合链终点位
        /// </summary>
        public override void SendHelianEndPos(float pos)
        {

            if (com != null)
            {
                com.WriteSingleRegisterReal(41260, pos);
            }
        }

        #region 自动识别拉链
        /// <summary>
        /// 自动识别测试开始
        /// </summary>
        public override void TestStart()
        {
            if (com != null)
            {
                com.WriteSingleCoil(13, true);
            }
        }

        /// <summary>
        /// 第一阶段位置完成
        /// </summary>
        public override void FirststageFinsh()
        {
            if (com != null)
            {
                com.WriteSingleCoil(20, true);
            }
        }

        /// <summary>
        /// 自动识别测试完成
        /// </summary>
        public override void TestFinish()
        {
            if (com != null)
            {
                com.WriteSingleCoil(23, true);
            }

        }

        /// <summary>
        /// 轴运动停止
        /// </summary>
        public override void AixtStop()
        {
            if (com != null)
            {
                com.WriteSingleCoil(24, true);
            }
        }
        /// <summary>
        /// 轴运动继续
        /// </summary>
        public override void AixtContinue(bool con)
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
        public override void CamTriggerStop()
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
        public override void SendCamFPS(int time)
        {
            if (com != null)
            {
                com.WriteSingleRegisterInt32(41256, time);
            }
        }


        #endregion

        #region 实例
        //public override  void MonitoringID()
        //{
        //    Thread.CurrentThread.Priority = ThreadPriority.Highest;
        //    while (Connend)
        //    {
        //        try
        //        {
        //            GetID(out int productID);
        //            if (productID <= 0)
        //            {
        //                Thread.Sleep(1);
        //                continue;
        //            }

        //            if (productID != TempproductID)
        //            {
        //                TempproductID = productID;
        //                List<float> copyPos = new List<float>();
        //                float ipullpos = GetPullLocation(); //为了防止中途从触摸屏改掉拉头位置，所以时刻监控它的值在进行比较
        //                if (ipullpos != TempPullPos)
        //                {
        //                    Idlist.Clear();
        //                    TempPullPos = ipullpos;
        //                    float fpullpos = ipullpos;
        //                    for (int i = 0; i < CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos.Count; i++)
        //                    {
        //                        copyPos.Add(CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos[i]);
        //                    }
        //                    copyPos.Add(fpullpos);
        //                    copyPos.Sort();
        //                    if (CZipperAutomaticAlgorithm.ZipperInfo.TriggerType == 3)
        //                    {
        //                        float handpos = CZipperAutomaticAlgorithm.ZipperInfo.HandAndTaliPos[0];
        //                        int handIndex = copyPos.IndexOf(handpos);

        //                        List<float> taskpos = copyPos.Take(handIndex).ToList(); //头
        //                        List<float> splitpos = copyPos.Skip(handIndex).ToList(); //尾

        //                        splitpos.AddRange(taskpos);
        //                        pullIndex = splitpos.IndexOf(fpullpos);
        //                    }
        //                    else
        //                    {
        //                        pullIndex = copyPos.IndexOf(fpullpos);
        //                    }
        //                    for (int i = 1; i <= CZipperAutomaticAlgorithm.ZipperInfo.ZipperTriggerPos.Count; i++)
        //                    {
        //                        Idlist.Add(i);
        //                    }
        //                    Idlist.Insert(pullIndex, 100);
        //                    string str = "";
        //                    for (int i = 0; i < Idlist.Count; i++)
        //                    {
        //                        str = str + $"第{i + 1}点;{Idlist[i]} ";
        //                    }
        //                    ZipperIDsORTLogger.Info(str);
        //                }

        //                for (int i = 0; i < Idlist.Count; i++)
        //                {
        //                    ZipperID zipperID = new ZipperID(productID, Idlist[i]);
        //                    m_WaitIDChannel.Writer.TryWrite(zipperID);
        //                }

        //            }

        //        }
        //        catch (Exception)
        //        {
        //        }
        //        Thread.Sleep(1);
        //    }
        //}

        #endregion
    }
}
