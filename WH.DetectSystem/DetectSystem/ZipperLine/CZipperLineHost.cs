using System;
using System.Linq;
using System.Windows.Threading;
using CommunicationModule;
using HandyControl.Controls;
using Modbus;
using WH.Entity.LogRecord;
using ZipperInfo;

namespace WH.DetectSystem.DetectSystem.ZipperLine
{
    /// <summary>
    /// 拉链工程挂接：业务 com、LoadParameter、IniAutomaticAlgorithm。
    /// 底层 Modbus 连接仍由平台 OpenAllComm 管理。
    /// </summary>
    public static class CZipperLineHost
    {
        public static void Detach()
        {
            CZipperCommunicate.Detach();
        }

        public static void Attach(Dispatcher dispatcher, CLogRec sysLog)
        {
            try
            {
                CZipperAutomaticAlgorithm.Instance.Dispatcher = dispatcher;
                CZipperCommunicate.com = CCommunicationManagement.CommDic.Values.FirstOrDefault() as CModbusCommPart;
                CZipperAutomaticAlgorithm.Instance.ZipperInfo = CZipperAutomaticAlgorithm.LoadParameter();
                CZipperAutomaticAlgorithm.Instance.IniAutomaticAlgorithm();
            }
            catch (Exception ex)
            {
                sysLog?.Error("拉链协议/自动识别初始化失败: " + ex.Message);
                Growl.Error("拉链协议/自动识别初始化失败: " + ex.Message);
            }
        }
    }
}
