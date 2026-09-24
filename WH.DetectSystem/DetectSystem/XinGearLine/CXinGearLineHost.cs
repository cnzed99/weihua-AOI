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
    /// 新兴工程挂接：赋 com、OnComAttached（点位/心跳/ID 轮询）。开机写三路张数。
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
            CXinGearCommunicate.com = CCommunicationManagement.CommDic.Values.OfType<CModbusCommPart>().FirstOrDefault();
            CXinGearCommunicate.OnComAttached();
        }

        /// <summary>打开新兴工程后按制程下发固定端面张数和型号侧面张数。</summary>
        public static bool SendLoadedRecipePhotoCount(IList<CMainModel> mainVms, CLogRec sysLog)
        {
            int sideCount = mainVms?.FirstOrDefault(vm => vm?.Name == "侧面")?.PhotoTotalCount ?? 0;
            if (sideCount <= 0)
            {
                sysLog?.Warn("侧面制程或拍照张数缺失");
                return false;
            }
            if (CXinGearCommunicate.TrySendRecipePhotoCount(sideCount, out string error))
                return true;
            sysLog?.Warn("配方张数下发失败: " + error);
            return false;
        }
    }
}