using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.LogRecord;
using WH.LightControl;
using WH.RunCell;

namespace ZipperInfo
{
    public class COPTLinghtChange : LightChangeBase
    {
        public CLightControlBase LightControl = null;
        CLogRec AutoLogger;
        public COPTLinghtChange(string portname, CLogRec autologger)
        {
            PortName = portname;
            AutoLogger= autologger;
            var li = CLinghtManagement.LightControlDict.Values.FirstOrDefault(c => c.BaseConfig.Port?.Name == PortName);
            if (li != null) { LightControl = li; }
        }

        public override void ChangeLineValue1(bool tempsave, int val)
        {
            try
            {
                LightControl.BaseConfig.LightChannelList[0].Value += val;
                if (LightControl.BaseConfig.LightChannelList[0].Value > 110)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value = 110;
                    MaxTimeOutCount++;
                }
                if (LightControl.BaseConfig.LightChannelList[0].Value < 30)
                {
                    LightControl.BaseConfig.LightChannelList[0].Value = 30;
                    MinTimeOutCount++;
                }

                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[1].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[1]);

                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[4].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[4]);

                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[5].Value = LightControl.BaseConfig.LightChannelList[0].Value;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[5]);
                if (tempsave)
                {
                    TempLightValue_Change1 = LightControl.BaseConfig.LightChannelList[0].Value;
                    AutoLogger.Info($"{PortName} :TempLightValue_Change1光源值设置为：{LightControl.BaseConfig.LightChannelList[0].Value}");
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
                LightControl.BaseConfig.LightChannelList[0].Value = val;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                Thread.Sleep(10);
                LightControl.BaseConfig.LightChannelList[4].Value = val;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[4]);
               
            }
            catch (Exception)
            {
            }

        }
        public override void LineValueReset()
        {
            try
            {
                AutoLogger.Info($"{PortName} :通道1和5光源值设置为：TempLightValue_Change1={TempLightValue_Change1}");
                Thread.Sleep(30);
                LightControl.BaseConfig.LightChannelList[0].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[0]);
                Thread.Sleep(30);
                LightControl.BaseConfig.LightChannelList[4].Value = TempLightValue_Change1;
                LightControl.SetChannelValue(LightControl.BaseConfig.LightChannelList[4]);
                CLinghtManagement.SaveLightParams();
            }
            catch (Exception)
            {
            }
        }
    }
}
