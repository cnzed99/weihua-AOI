using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlarmSetCtrl;
using CommunicationModule;
using CommunityToolkit.Mvvm.Messaging;
using QualityGrade;
using SDFilter;
using WH.Entity.CommonLib;
using WH.RunCell;

namespace WH.DetectSystem._4_报警处理
{
    public static class AlarmStage
    {
        public static void Excute(this CAlarmSetConfig alarmSetConfig, Cell cell)
        {
            foreach (Alarm alarm in alarmSetConfig.AlarmList)
            {
                if (alarm.AddCellAndJudge(cell))
                {
                    WeakReferenceMessenger.Default.Send(new AlarmPopMessage(alarm), alarm.token);
                    CCommunicationManagement.SendAlarmSignal(alarm.AlarmAgreement);
                }
            }
        }

        /// <summary>
        /// 20240715 TCG
        /// 增加Cell并判断规则
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>规则NG数量满足为true,否则为false</returns>
        public static bool AddCellAndJudge(this Alarm alarm, Cell cell)
        {
            bool ret = false;
            if (alarm.IsTimeLimit || alarm.IsTotalLimit)
            {
                var alarmcell = (cell.CreateTime, true);
                switch (alarm.Type)
                {
                    case ALARMTYPE.ALARMTYPE_GRADE:
                        alarmcell.Item2 = cell.Quality == (Quality)alarm.Source;
                        break;
                    case ALARMTYPE.ALARMTYPE_DEFECT:
                        alarmcell.Item2 = cell.Detection.DefectFilter == (DefectFilter)alarm.Source;
                        break;
                }
                alarm.TotalCellList.Add(alarmcell);
                //在规定时间内出现指定数量NG
                if (alarm.IsTimeLimit)
                {
                    //超时限 删除
                    while (cell.CreateTime - alarm.TotalCellList[0].createTime > alarm.TimeSpan)
                    {
                        alarm.TotalCellList.RemoveAt(0);
                    }
                }
                if (alarm.IsTotalLimit)
                {
                    //保持最近限定数量内
                    while (alarm.TotalCellList.Count > alarm.Total)
                    {
                        alarm.TotalCellList.RemoveAt(0);
                    }
                    int ngNumber = alarm.TotalCellList.FindAll(a => a.isCellNg).Count();
                    if (ngNumber >= alarm.NgCount) //达到报警标准
                    {
                        alarm.TotalCellList.Clear();
                        ret = true;
                    }
                }
                return ret;
            }
            else //NG数量达标即报警
            {
                switch (alarm.Type)
                {
                    case ALARMTYPE.ALARMTYPE_GRADE:
                        if (cell.Quality == alarm.Source)
                            alarm.TotalNG++;
                        break;
                    case ALARMTYPE.ALARMTYPE_DEFECT:
                        if (cell.Detection.DefectFilter == alarm.Source)
                            alarm.TotalNG++;
                        break;
                }
                if (alarm.TotalNG >= alarm.NgCount)
                {
                    alarm.TotalNG = 0;
                    return true;
                }
                return false;
            }
        }
    }
}
