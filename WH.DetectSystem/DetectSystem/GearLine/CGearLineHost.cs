using System;
using System.Collections.Generic;
using System.Linq;
using CommunicationModule;
using GearInfo;
using Modbus;
using WH.DetectSystem.Models;
using WH.Entity.LogRecord;

namespace WH.DetectSystem.DetectSystem.GearLine
{
    /// <summary>
    /// 盘齿工程挂接：业务 com、OnComAttached、开工程后下发配方张数。
    /// 底层 Modbus 连接仍由平台 OpenAllComm 管理。
    /// </summary>
    public static class CGearLineHost
    {
        public static void Detach()
        {
            CGearCommunicate.Detach();
        }

        public static void Attach()
        {
            CGearCommunicate.com = CCommunicationManagement.CommDic.Values.FirstOrDefault() as CModbusCommPart;
            CGearCommunicate.OnComAttached();
        }

        public static void SendLoadedRecipePhotoAndFocus(IList<CMainModel> mainVms, CLogRec sysLog)
        {
            if (mainVms == null || mainVms.Count == 0)
            {
                return;
            }
            List<(string processName, int photoTotalCount)> processes = new List<(string, int)>(mainVms.Count);
            foreach (var vm in mainVms)
            {
                if (vm == null || string.IsNullOrEmpty(vm.Name))
                {
                    continue;
                }
                processes.Add((vm.Name, vm.PhotoTotalCount));
            }
            try
            {
                CGearCommunicate.SendRecipePhotoAndFocus(processes);
            }
            catch (Exception ex)
            {
                sysLog?.Warn("配方张数下发失败: " + ex.Message);
            }
        }
    }
}
