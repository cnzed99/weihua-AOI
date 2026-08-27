using System;
using System.Collections.Generic;
using System.Linq;
using CommunicationModule;
using Modbus;
using WH.DetectSystem.Models;
using WH.Entity.LogRecord;
using XinGearInfo;

namespace WH.DetectSystem.DetectSystem.XinGearLine
{
    /// <summary>
    /// 【新兴盘齿方案11-注释】 新兴工程挂接：赋 com、OnComAttached（点位/心跳/ID 轮询）。开机写三路张数。
    /// 底层 Modbus 连接仍走平台 OpenAllComm。Detach 不停底层连接。
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

        /// <summary>
        /// 【新兴盘齿方案2-注释】 开工程成功后按制程 Name 收集 PhotoTotalCount，写一次 HD1200 三路。
        /// </summary>
        public static void SendLoadedRecipePhotoCount(IList<CMainModel> mainVms, CLogRec sysLog)
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
                CXinGearCommunicate.SendRecipePhotoCount(processes);
            }
            catch (Exception ex)
            {
                sysLog?.Warn("配方张数下发失败: " + ex.Message);
            }
        }
    }
}