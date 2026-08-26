using System.Linq;
using CommunicationModule;
using CrankInfo;
using Modbus;
using WH.Entity.LogRecord;

namespace WH.DetectSystem.DetectSystem.CrankLine
{
    /// <summary>
    /// 【曲轴方案2-注释】曲轴工程挂接：赋 com、OnComAttached（心跳/ID 轮询）。不写 HD1200。
    /// 底层 Modbus 连接仍由平台 OpenAllComm 管理；Detach 不停底层连接。
    /// </summary>
    public static class CCrankLineHost
    {
        public static void Detach()
        {
            CCrankCommunicate.Detach();
        }

        public static void Attach()
        {
            CCrankCommunicate.com = CCommunicationManagement.CommDic.Values.FirstOrDefault() as CModbusCommPart;
            CCrankCommunicate.OnComAttached();
        }
    }
}