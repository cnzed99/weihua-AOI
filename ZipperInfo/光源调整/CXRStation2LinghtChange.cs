using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.LogRecord;
using WH.LightControl;

namespace ZipperInfo
{
    /// <summary>
    /// 拉头光源调整类
    /// </summary>
    public class CXRPullerLinghtChange : LightChangeBase
    {
        public CLightControlBase LightControl = null;
        //CLogRec AutoLogger;
        public CXRPullerLinghtChange(string portname)
        {
            PortName = portname;
           // AutoLogger = autologger;
            var li = CLinghtManagement.LightControlDict.Values.FirstOrDefault(c => c.BaseConfig.Port?.Name == PortName);
            if (li != null) { LightControl = li; }
        }

        public override void ChangeLineValue1(bool tempsave, int val)
        {
            try
            {
                if (LightControl != null)
                {
                    LightControl.BaseConfig.LightChannelList[1].Value += val;
                    if (LightControl.BaseConfig.LightChannelList[1].Value >= 255)
                    {
                        LightControl.BaseConfig.LightChannelList[1].Value = 255;
                        MaxTimeOutCount++;
                    }
                    if (LightControl.BaseConfig.LightChannelList[1].Value <= 15)
                    {
                        LightControl.BaseConfig.LightChannelList[1].Value = 15;
                        MinTimeOutCount++;
                    }

                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
                }
               


            }
            catch (Exception)
            {
            }

        }
        public override void ChangeLineValue2(int val)
        {
            try
            {
                if (LightControl != null)
                {
                    LightControl.BaseConfig.LightChannelList[1].Value = val;
                    if (LightControl.BaseConfig.LightChannelList[1].Value >= 255)
                    {
                        LightControl.BaseConfig.LightChannelList[1].Value = 255;
                    }
                    if (LightControl.BaseConfig.LightChannelList[1].Value <= 15)
                    {
                        LightControl.BaseConfig.LightChannelList[1].Value = 15;
                    }

                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
                    //  TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
                }

            }
            catch (Exception)
            {
            }

        }
        //public override void LineValueReset()
        //{
        //}
    }
    /// <summary>
    /// 拉片光源调整类
    /// </summary>
    public class CXRPullsLinghtChange : LightChangeBase
    {
        public CLightControlBase LightControl = null;
        //CLogRec AutoLogger;
        public CXRPullsLinghtChange(string portname)
        {
            PortName = portname;
           // AutoLogger = autologger;
            var li = CLinghtManagement.LightControlDict.Values.FirstOrDefault(c => c.BaseConfig.Port?.Name == PortName);
            if (li != null) { LightControl = li; }
        }

        public override void ChangeLineValue1(bool tempsave, int val)
        {
            try
            {
                if (LightControl != null)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value += val;
                    if (LightControl.BaseConfig.LightChannelList[0].Value >= 255)
                    {
                        LightControl.BaseConfig.LightChannelList[0].Value = 255;
                        MaxTimeOutCount++;
                    }
                    if (LightControl.BaseConfig.LightChannelList[0].Value <= 50)
                    {
                        LightControl.BaseConfig.LightChannelList[0].Value = 50;
                        MinTimeOutCount++;
                    }

                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                    //Thread.Sleep(10);
                    //LightControl.BaseConfig.LightChannelList[1].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                    //LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
                    ////if (tempsave)
                    ////{
                    ////    TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
                    ////    AutoLogger.Info($"{PortName} :TempLightValue_Change1光源值设置为：{LightControl.BaseConfig.LightChannelList[0].Value}");
                    ////}
                }
            }
            catch (Exception)
            {
            }

        }
        public override void ChangeLineValue2(int val)
        {
            try
            {
                if (LightControl != null)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value = val;
                    if (LightControl.BaseConfig.LightChannelList[0].Value >= 255)
                    {
                        LightControl.BaseConfig.LightChannelList[0].Value = 255;
                    }
                    if (LightControl.BaseConfig.LightChannelList[0].Value <= 50)
                    {
                        LightControl.BaseConfig.LightChannelList[0].Value = 50;
                    }

                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                }
             
            }
            catch (Exception)
            {
            }

        }
        //public override void LineValueReset()
        //{
        //}
    }

    /// <summary>
    /// 上止光源调整类
    /// </summary>
    public class CXRUpMassLinghtChange : LightChangeBase
    {
        public CLightControlBase LightControl = null;
        //CLogRec AutoLogger;
        public CXRUpMassLinghtChange(string portname)
        {
            PortName = portname;
            // AutoLogger = autologger;
            var li = CLinghtManagement.LightControlDict.Values.FirstOrDefault(c => c.BaseConfig.Port?.Name == PortName);
            if (li != null) { LightControl = li; }
        }

        //public override void ChangeLineValue1(bool tempsave, int val)
        //{
        //    try
        //    {
        //        LightControl.BaseConfig.LightChannelList[0].Value += val;
        //        if (LightControl.BaseConfig.LightChannelList[0].Value >= 255)
        //        {
        //            LightControl.BaseConfig.LightChannelList[0].Value = 255;
        //            MaxTimeOutCount++;
        //        }
        //        if (LightControl.BaseConfig.LightChannelList[0].Value <= 90)
        //        {
        //            LightControl.BaseConfig.LightChannelList[0].Value = 90;
        //            MinTimeOutCount++;
        //        }

        //        LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
        //        //Thread.Sleep(10);
        //        //LightControl.BaseConfig.LightChannelList[1].Value = LightControl.BaseConfig.LightChannelList[0].Value;
        //        //LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
        //        ////if (tempsave)
        //        ////{
        //        ////    TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
        //        ////    AutoLogger.Info($"{PortName} :TempLightValue_Change1光源值设置为：{LightControl.BaseConfig.LightChannelList[0].Value}");
        //        ////}

        //    }
        //    catch (Exception)
        //    {
        //    }

        //}
        public override void ChangeLineValue2(int val)
        {
            try
            {
                if (LightControl != null)
                {
                    LightControl.BaseConfig.LightChannelList[2].Value = val;
                    if (LightControl.BaseConfig.LightChannelList[2].Value >= 255)
                    {
                        LightControl.BaseConfig.LightChannelList[2].Value = 255;
                    }
                    if (LightControl.BaseConfig.LightChannelList[2].Value <= 0)
                    {
                        LightControl.BaseConfig.LightChannelList[2].Value = 0;
                    }

                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[2]);
                    LightControl.BaseConfig.LightChannelList[3].Value = LightControl.BaseConfig.LightChannelList[2].Value;
                    LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[3]);
                }
            }
            catch (Exception)
            {
            }

        }
        //public override void LineValueReset()
        //{
        //}
    }
}
