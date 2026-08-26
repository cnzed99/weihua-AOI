using System.Linq;
using CommunicationModule;
using Modbus;
using XinGearInfo;

namespace WH.DetectSystem.DetectSystem.XinGearLine
{
    /// <summary>
    /// 【新兴盘齿方案11-注释】新兴工程挂接：赋 com、OnComAttached。波次1 空壳不写 PLC。
    /// 底层 Modbus 连接仍由平台 OpenAllComm 管理；Detach 不停底层连接。
    /// </summary>
    public static class CXinGearLineHost
    {
        public static void Detach()
        {
            CXinGearCommunicate.Detach();
        }

        public static void Attach()
        {
            CXinGearCommunicate.com = CCommunicationManagement.CommDic.Values.FirstOrDefault() as CModbusCommPart;
            CXinGearCommunicate.OnComAttached();
        }
    }
}
