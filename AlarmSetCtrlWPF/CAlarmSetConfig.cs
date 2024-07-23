using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using QualityGrade;
using SDFilter;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace AlarmSetCtrl
{
    /// <summary>
    /// 20240715 TCG
    /// 报警规则 配置管理 与缺陷、质量等级挂钩
    /// </summary>
    public partial class CAlarmSetConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 20240711 TCG
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 20240711 TCG
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec SysLog { get; set; } = CLogRec.Create("Info", "./Log", "Error");

        public CAlarmSetConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        /// <summary>
        /// 20240711 TCG
        /// 初始化报警
        /// </summary>
        /// <param name="cAlarmSet"></param>
        /// <param name="filterConfig"></param>
        /// <param name="qualityConfig"></param>
        public void SetCAlarm(CFilterConfig filterConfig, CQualityConfig qualityConfig)
        {
            this.DefectList = filterConfig.DefectList;
            this.Qualities = qualityConfig.Qualities;

            Synchronization();
        }

        /// <summary>
        /// 20240715 TCG
        /// 同步缺陷和质量等级实例，同时移除不适用的报警源 报警配置
        /// </summary>
        protected void Synchronization()
        {
            var DeList = DefectList.ToList();
            var QaList = Qualities.ToList();

            var alarmNeedRemove = new List<Alarm>();
            foreach (var alarm in AlarmList)
            {
                if (alarm.Source is DefectFilter de)
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
                else if (alarm.Source is Quality qa)
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

            foreach (var alarm in AlarmList)
            {
                if (
                    CCommunicationManagement.CommParamDic.TryGetValue(
                        alarm.AlarmAgreement?.GUID,
                        out CCommunicationSettingBase comParams
                    )
                )
                {
                    var index = comParams
                        .AlarmAgreements.ToList()
                        .FindIndex(al => al.ToString() == alarm.AlarmAgreement.ToString());
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
                AlarmList.Remove(alarm);
            }
        }

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 报警规则集合
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("报警规则")]
        ObservableCollection<Alarm> alarmList = new ObservableCollection<Alarm>();

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 缺陷等级列表
        /// 20240711 TCG 初始化时传引用过来
        /// </summary>
        [property: JsonIgnore]
        [ObservableProperty]
        [property: IgnoreModifyLog]
        ObservableCollection<Quality> qualities = new();

        /// <summary>
        /// 20240711 TCG
        /// 缺陷列表
        /// </summary>
        [property: JsonIgnore]
        [ObservableProperty]
        [property: IgnoreModifyLog]
        private ObservableCollection<DefectFilter> defectList = new();

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 是否启用报警设置
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("是否启用报警设置")]
        bool isStartAlarm;

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 监控数量
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("监控数量")]
        private int alarmNum;

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 单片报警
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("单片报警")]
        private bool alarmSingle;

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 软件停止
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("软件停止")]
        private bool softwareStop;

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 自动复位停机
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("自动复位停机")]
        private bool alarmAutoStopReset;

        /// <summary>
        /// 2024.6.25 鲍赞宝
        /// 复位等待时间
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("复位等待时间")]
        private int resetWaitTime;

        /// <summary>
        /// 20240711 TCG
        /// 收到修改信息 打印到日志
        /// </summary>
        /// <param name="message"></param>
        public void Receive(OperateMessage message)
        {
            //记录修改信息
            if (message.obj.GetType() == typeof(CAlarmSetConfig))
            {
                OperateLog.Info($"报警设置-{message.message}");
                return;
            }
            foreach (var alarm in AlarmList)
            {
                if (message.obj is Alarm al)
                {
                    if (al == alarm)
                    {
                        OperateLog.Info($"报警设置-{al.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }
    }

    /// <summary>
    ///  2024.6.25 鲍赞宝
    /// 报警规则参数类
    /// </summary>
    public partial class Alarm : ConfigModifyObservableBase
    {
        public Alarm()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        /// <summary>
        /// 报警名称
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("报警名称")]
        private string name = "新规则";

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 报警类型
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("报警类型")]
        private ALARMTYPE type = ALARMTYPE.ALARMTYPE_GRADE;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 是否弹窗
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("是否弹窗")]
        private bool isPopWin = true;

        [DisplayName("是否独立控制")]
        public bool isIndependent => IsTimeLimit || IsTotalLimit;

        [property: DisplayName("报警源")]
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RegularShow))]
        private object source = new object();

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 权限等级
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("权限等级")]
        private PermitLevel permitLevel = PermitLevel.OP;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则时间限制
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("规则时间限制")]
        [NotifyPropertyChangedFor(nameof(RegularShow))]
        [NotifyPropertyChangedFor(nameof(isIndependent))]
        private bool isTimeLimit = true;

        partial void OnIsTimeLimitChanged(bool value)
        {
            TotalCellList.Clear();
            TotalNG = 0;
        }

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则时间间隔
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("规则时间间隔")]
        [NotifyPropertyChangedFor(nameof(RegularShow))]
        private int time = 10;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 时间单位
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("时间单位")]
        [NotifyPropertyChangedFor(nameof(RegularShow))]
        private TIMEUNIT timeUnit = TIMEUNIT.TIMEUNIT_HOUR;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则时间间隔
        /// </summary>
        [JsonIgnore]
        public TimeSpan TimeSpan
        {
            get
            {
                switch (TimeUnit)
                {
                    case TIMEUNIT.TIMEUNIT_HOUR:
                        return new TimeSpan(Time, 0, 0);
                    case TIMEUNIT.TIMEUNIT_MINUTE:
                        return new TimeSpan(0, Time, 0);
                    case TIMEUNIT.TIMEUNIT_SECOND:
                        return new TimeSpan(0, 0, Time);
                }
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则总数控制
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("规则总数控制")]
        [NotifyPropertyChangedFor(nameof(RegularShow))]
        [NotifyPropertyChangedFor(nameof(isIndependent))]
        private bool isTotalLimit = true;

        partial void OnIsTotalLimitChanged(bool value)
        {
            TotalCellList.Clear();
            TotalNG = 0;
        }

        private int total = 500;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则总数
        /// </summary>
        [DisplayName("规则总数")]
        public int Total
        {
            get { return total; }
            set
            {
                if (IsTotalLimit)
                {
                    if (value < ngCount)
                    {
                        NgCount = value;
                    }
                }
                TotalNG = 0;
                TotalCellList.Clear();
                SetProperty(ref total, value);
                OnPropertyChanged(nameof(RegularShow));
            }
        }

        private int ngCount = 100;

        /// <summary>
        ///  2024.6.25 鲍赞宝
        /// 规则NG数
        /// </summary>
        [DisplayName("规则NG数")]
        [IgnoreModifyLog]
        public int NgCount
        {
            get { return ngCount; }
            set
            {
                if (IsTotalLimit)
                {
                    if (value > total)
                    {
                        value = total;
                    }
                }
                TotalCellList.Clear();
                TotalNG = 0;
                SetProperty(ref ngCount, value);
                OnPropertyChanged(nameof(RegularShow));
            }
        }

        /// <summary>
        /// 20240715 TCG
        /// 报警规则描述
        /// </summary>
        [JsonIgnore]
        [IgnoreModifyLog]
        public string RegularShow
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (IsTimeLimit)
                    sb.Append($"在最近{Time}{EnumStringAttribute.GetEnumName(TimeUnit)}内");
                if (IsTotalLimit)
                    sb.Append($"连续{Total}片中");
                sb.Append($"出现{NgCount}片{Source}");
                return sb.ToString();
            }
        }

        /// <summary>
        /// 20240723 TCG
        /// 通讯报警协议
        /// </summary>
        [property: DisplayName("报警协议")]
        [ObservableProperty]
        CAlarmAgreement alarmAgreement = new();

        /// <summary>
        /// 20240715 TCG
        /// 总数规则列表
        /// </summary>
        [JsonIgnore]
        public List<(DateTime createTime, bool isCellNg)> TotalCellList = new();

        /// <summary>
        /// 20240712 TCG
        /// 独立控制的 总NG数量
        /// </summary>
        public int TotalNG;

        /// <summary>
        /// 20240715 鲍赞宝
        /// 复制类型
        /// </summary>
        /// <param name="a"></param>
        public void Copy(Alarm a)
        {
            this.Name = a.Name;
            this.Type = a.Type;
            this.IsPopWin = a.IsPopWin;
            this.Source = a.Source;
            this.PermitLevel = a.PermitLevel;
            this.IsTimeLimit = a.IsTimeLimit;
            this.IsTotalLimit = a.IsTotalLimit;
            this.Time = a.Time;
            this.TimeUnit = a.TimeUnit;
            this.Total = a.Total;
            this.NgCount = a.NgCount;
            this.AlarmAgreement = a.AlarmAgreement;
        }

        public override string ToString()
        {
            return RegularShow;
        }
    }

    /// <summary>
    /// 20240711 TCG
    /// 枚举格式 未改
    /// </summary>
    public enum ALARMTYPE
    {
        [EnumString("档位类型", "Grade")]
        ALARMTYPE_GRADE = 1,

        [EnumString("缺陷类型", "Type")]
        ALARMTYPE_DEFECT = 2,
    }

    /// <summary>
    /// 20240711 TCG
    /// 枚举格式 未改
    /// </summary>
    public enum PermitLevel
    {
        [EnumString("操作员", "OP")]
        OP = 1,

        [EnumString("工艺员", "MS")]
        MS = 2,

        [EnumString("技术员", "SE")]
        SE = 4
    }

    /// <summary>
    /// 20240715 TCG
    /// 时间单位枚举
    /// </summary>
    public enum TIMEUNIT
    {
        [EnumString("小时", "Hour")]
        TIMEUNIT_HOUR = 1,

        [EnumString("分钟", "Minute")]
        TIMEUNIT_MINUTE = 2,

        [EnumString("秒", "Second")]
        TIMEUNIT_SECOND = 4,
    }

    /// <summary>
    /// 20240715 TCG
    /// 弹窗消息 通过消息通道发送
    /// </summary>
    /// <param name="alarm"></param>
    public record AlarmPopMessage(Alarm alarm);
}
