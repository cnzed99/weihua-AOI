using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CommunicationModule;
using HandyControl.Controls;
using Modbus;
using WH.DetectSystem.Models;
using WH.Entity.LogRecord;
using ZipperInfo;

namespace WH.DetectSystem.DetectSystem.ZipperLine
{
    public static class CZipperLineHost
    {
        public static void Detach(IEnumerable<CProcessGroupModel> groups)
        {
            if (groups != null)
            {
                foreach (CProcessGroupModel group in groups)
                {
                    if (group == null)
                    {
                        continue;
                    }
                    group.StopZipperIdThread();
                    if (group.CMainModels == null)
                    {
                        continue;
                    }
                    foreach (CMainModel model in group.CMainModels)
                    {
                        model?.UnsubscribeZipperAutoFinish();
                    }
                }
            }
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
