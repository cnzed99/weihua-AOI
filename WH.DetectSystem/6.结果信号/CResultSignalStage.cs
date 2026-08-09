using AlarmSetCtrl;
using CommunicationModule;
using Mysqlx;
using ProjProduceData;
using QualityGrade;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// cell检测结果 汇报 统计 存储
    /// </summary>
    public static class CResultSignalStage
    {
        public static void Excute(this CQualityConfig quality, Cell cell)
        {

           // CCommunicationManagement.SendAlarmSignal(alarm.AlarmAgreement);
        }
    }
}
