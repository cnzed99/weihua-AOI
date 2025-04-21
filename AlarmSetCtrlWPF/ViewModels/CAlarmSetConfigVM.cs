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
                    && Qualities.Contains(qua)
                )
                    MAlarm.Source = SourceAlarm.Source;

                SourceList = Qualities;
            }
            else
            {
                if (
                    SourceAlarm is not null
                    && SourceAlarm.Source is DefectFilter de
                    && DefectList.Contains(de)
                )
                    MAlarm.Source = SourceAlarm.Source;
                SourceList = DefectList;
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

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 缺陷等级列表
        /// 20240711 TCG 初始化时传引用过来
        /// </summary>
        [ObservableProperty]
        ObservableCollection<Quality> qualities = new();

        /// <summary>
        /// 20240711 TCG
        /// 缺陷列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<DefectFilter> defectList = new();

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 缺陷配置
        /// </summary>
        List<CFilterConfig> filterConfigs;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计
        /// </summary>
        /// <param name="filterConfigsNew"></param>
        public void SetFilter(List<CFilterConfig> filterConfigsNew)
        {
            if (this.filterConfigs != null)
            {
                foreach (var item in this.filterConfigs)
                {
                    item.DefectList.CollectionChanged -= DefectList_CollectionChanged;
                }
            }
            this.filterConfigs = filterConfigsNew;
            UpdateDefects();
            foreach (var item in filterConfigs)
            {
                item.DefectList.CollectionChanged += DefectList_CollectionChanged;
            }
        }

        public void UpdateDefects()
        {
            List<string> defectExists = new List<string>();
            foreach (var config in filterConfigs)
            {
                foreach (var defect in config.DefectList)
                {
                    defectExists.Add(defect.Name);
                    if (DefectList.FirstOrDefault(o => o.Name == defect.Name) == null)
                    {
                        DefectList.Add(defect);
                    }
                }
            }
            for (int i = DefectList.Count - 1; i >= 0; i--)
            {
                if (!defectExists.Contains(DefectList[i].Name))
                {
                    DefectList.RemoveAt(i);
                }
            }
            Synchronization();
        }

        private void DefectList_CollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            UpdateDefects();
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计
        /// </summary>
        /// <param name="qualityConfigNew"></param>
        public void SetQuality(CQualityConfig qualityConfigNew)
        {
            this.Qualities = qualityConfigNew.Qualities;
            Synchronization(true);
        }

        /// <summary>
        /// 20240715 TCG
        /// 同步缺陷和质量等级实例，同时移除不适用的报警源 报警配置
        /// </summary>
        protected void Synchronization(bool isQuality = false)
        {
            var DeList = DefectList.ToList();
            var QaList = Qualities.ToList();

            var alarmNeedRemove = new List<Alarm>();
            foreach (var alarm in CAlarmSet.AlarmList)
            {
                if (!isQuality && alarm.Source is DefectFilter de)
                {
                    var index = DeList.FindIndex(d => d.Name == de.Name);
                    if (index >= 0)
                    {
                        alarm.Source = DeList[index];
                    }
                    else
                    {
                        alarmNeedRemove.Add(alarm);
                    }
                }
                else if (isQuality && alarm.Source is Quality qa)
                {
                    var index = QaList.FindIndex(d => d.Name == qa.Name);
                    if (index >= 0)
                    {
                        alarm.Source = QaList[index];
                    }
                    else
                    {
                        alarmNeedRemove.Add(alarm);
                    }
                }
            }

            foreach (var alarm in CAlarmSet.AlarmList)
            {
                if (alarm.AlarmAgreement == null)
                    continue;
                if (
                    alarm.AlarmAgreement?.GUID != null
                    && CCommunicationManagement.CommParamDic.TryGetValue(
                        alarm.AlarmAgreement?.GUID,
                        out CCommunicationSettingBase comParams
                    )
                )
                {
                    var index = comParams
                        .AlarmAgreements.ToList()
                        .FindIndex(al =>
                            (al.Name == alarm.AlarmAgreement.Name)
                            && (al.ComName == alarm.AlarmAgreement.ComName)
                        );
                    if (index >= 0)
                    {
                        alarm.AlarmAgreement = comParams.AlarmAgreements[index];
                    }
                }
                else
                {
                    alarmNeedRemove.Add(alarm);
                }
            }

            //移除不适用的报警设置
            foreach (var alarm in alarmNeedRemove)
            {
                CAlarmSet.AlarmList.Remove(alarm);
            }
        }
    }
}
