using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using log4net.Filter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QualityGrade;
using SDFilter;
using SVGImage.SVG.Filters;
using WH.Controls;
using WH.Entity.CommonLib;
using WH.RunCell;
using MessageBox = HandyControl.Controls.MessageBox;

namespace AlarmSetCtrl
{
    /// <summary>
    /// 报警设置ViewModel
    /// </summary>
    public partial class CAlarmSetConfigVM : ObservableObject
    {
        public CAlarmSetConfigVM() { }

        [ObservableProperty]
        public CAlarmSetConfig cAlarmSet = new CAlarmSetConfig();

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson;

        /// <summary>
        /// 20270719 TCG
        /// 在打开项目时 重置选中项
        /// </summary>
        public void Reset()
        {
            MAlarm = null;
            MAlarm = new Alarm();
            SourceAlarm = null;
        }

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 添加报警规则
        /// </summary>
        [RelayCommand]
        void AddAlarm()
        {
            Alarm alarm1 = new Alarm();
            alarm1.Copy(MAlarm);
            alarm1.token.ProGuid = CAlarmSet.token.ProGuid;
            CAlarmSet.AlarmList.Add(alarm1);
        }

        dynamic sourceList = null;
        public dynamic SourceList
        {
            get => sourceList;
            set
            {
                sourceList = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        ObservableCollection<CAlarmAgreement> alarmAgreements = new();

        /// <summary>
        /// 当前报警类型更改时
        /// </summary>
        [RelayCommand]
        void SelectedAlarmTypeChanged()
        {
            if (MAlarm?.Type == ALARMTYPE.ALARMTYPE_GRADE)
            {
                if (
                    SourceAlarm is not null
                    && SourceAlarm.Source is Quality qua
                    && CAlarmSet.Qualities.Contains(qua)
                )
                    MAlarm.Source = SourceAlarm.Source;

                SourceList = CAlarmSet.Qualities;
            }
            else
            {
                if (
                    SourceAlarm is not null
                    && SourceAlarm.Source is DefectFilter de
                    && CAlarmSet.DefectList.Contains(de)
                )
                    MAlarm.Source = SourceAlarm.Source;
                SourceList = CAlarmSet.DefectList;
            }
        }

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// Listview选择的当前项
        /// </summary>
        /// <param name="selectedItem"></param>
        [RelayCommand]
        void SelectedAlarmChanged(object selectedItem)
        {
            SourceAlarm = selectedItem as Alarm;
            if (SourceAlarm != null)
            {
                RefreshAlarmAgreements();
                var alarm = new Alarm();
                alarm.Copy(SourceAlarm);
                MAlarm = alarm;
            }
        }

        /// <summary>
        /// 更新报警列表
        /// </summary>
        [RelayCommand]
        void RefreshAlarmAgreements()
        {
            AlarmAgreements.Clear();
            foreach (var comParams in CCommunicationManagement.CommParamDic.Values)
            {
                foreach (var AlarmAgree in comParams.AlarmAgreements)
                {
                    if (!AlarmAgreements.Contains(AlarmAgree))
                    {
                        AlarmAgreements.Add(AlarmAgree);
                    }
                }
            }
        }

        /// <summary>
        /// 移除 报警
        /// </summary>
        /// <param name="alarm"></param>
        [RelayCommand]
        public void RemoveAlarm(Alarm alarm)
        {
            if (alarm != null)
            {
                Growl.AskGlobal(
                    CAlarmSet.PrcessName + "-" + Properties.Resources.DelecteAsk,
                    b =>
                    {
                        if (b)
                        {
                            CAlarmSet.AlarmList.Remove(alarm);
                        }
                        return true;
                    }
                );
            }
        }

        /// <summary>
        /// 修改报警
        /// </summary>
        [RelayCommand]
        void AmendAlarm(Alarm alarm)
        {
            if (alarm != null)
            {
                alarm.Copy(MAlarm);
            }
        }

        /// <summary>
        /// 20240712 TCG
        /// 临时修改对象
        /// </summary>
        [ObservableProperty]
        Alarm mAlarm;

        /// <summary>
        /// 20240712 TCG
        /// 源对象 报警类型
        /// </summary>
        [ObservableProperty]
        Alarm sourceAlarm;
    }
}
