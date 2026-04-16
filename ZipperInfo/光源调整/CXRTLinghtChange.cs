using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.LightControl;

namespace ZipperInfo
{
    public class CXRLinghtChange : LightChangeBase
    {
        public CLightControlBase LightControl = null;

        public CXRLinghtChange(string portname)
        {
            PortName = portname;

            var li = CLinghtManagement.LightControlDict.Values.FirstOrDefault(c => c.BaseConfig.Port?.Name == PortName);
            if (li != null) { LightControl = li; }
        }

        public override void ChangeLineValue1(bool tempsave, int val)
        {
            try
            {
                val = val * 5; //祥瑞光控的特点，曝光时间精度只能控制到10us,
                LightControl.BaseConfig.LightChannelList[0].Value += val;
                if (LightControl.BaseConfig.LightChannelList[0].Value > 650)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value = 650;
                    MaxTimeOutCount++;
                }
                if (LightControl.BaseConfig.LightChannelList[0].Value < 150)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value = 150;
                    MinTimeOutCount++;
                }

                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[1].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);

                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[2].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[2]);

                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[3].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[3]);
                if (tempsave)
                {
                    TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
                }

            }
            catch (Exception)
            {
            }

        }
        //public override void ChangeLineValue2(int val)
        //{
        //    try
        //    {
        //        LightControl.BaseConfig.LightChannelList[1].Value += val;
        //        if (LightControl.BaseConfig.LightChannelList[1].Value > 130)
        //        {
        //            LightControl.BaseConfig.LightChannelList[1].Value = 130;
        //        }
        //        if (LightControl.BaseConfig.LightChannelList[1].Value < 30)
        //        {
        //            LightControl.BaseConfig.LightChannelList[1].Value = 30;
        //        }

        //        LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
        //        Thread.Sleep(10);
        //        LightControl.BaseConfig.LightChannelList[5].Value = LightControl.BaseConfig.LightChannelList[1].Value;
        //        LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[5]);

        //        //  TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
        //    }
        //    catch (Exception)
        //    {
        //    }

        //}
        public override void LineValueReset()
        {
            try
            {
                LightControl.BaseConfig.LightChannelList[0].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                Thread.Sleep(30);
                LightControl.BaseConfig.LightChannelList[1].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);
                Thread.Sleep(30);
                LightControl.BaseConfig.LightChannelList[2].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[2]);
                Thread.Sleep(30);
                LightControl.BaseConfig.LightChannelList[3].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[3]);

            }
            catch (Exception)
            {
            }
        }
    }
}
