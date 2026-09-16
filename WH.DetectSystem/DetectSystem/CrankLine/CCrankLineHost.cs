using System.Linq;
using CommunicationModule;
using CrankInfo;
using Modbus;
using WH.Entity.LogRecord;

namespace WH.DetectSystem.DetectSystem.CrankLine
{
    /// <summary>
    /// 底层 Modbus 连接
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