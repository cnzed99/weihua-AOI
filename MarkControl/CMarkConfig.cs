using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace MarkControl
{
    /// <summary>
    /// 2024.7.15 李焕彬
    /// 打标配置
    /// </summary>
    public partial class CMarkConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 操作日志
        /// </summary>
        [property: Browsable(false)]
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public CMarkConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CMarkConfig))
            {
                OperateLog.Info($"打标模块-{message.message}");
                return;
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 从站号，从1开始
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("11.从站号")]
        [property: Description("SlaveIdInfo")]
        private int slaveId = 1;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器通道，从0开始
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("12.编码器通道")]
        [property: Description("EncoderIdInfo")]
        private int encoderId = 0;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 触发通道，从0开始
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("13.触发通道")]
        [property: Description("TriggerIdInfo")]
        private int triggerId = 0;

        private EMENCODERMODE encoderMode = EMENCODERMODE.EMENCODEMONE;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器倍频，1-1倍频，2-2倍频，4-4倍频
        /// </summary>
        [property: Category("1.卡配置")]
        [property: DisplayName("14.编码器倍频")]
        [property: Description("EncoderModeInfo")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        public EMENCODERMODE EncoderMode
        {
            get { return encoderMode; }
            set
            {
                SetProperty(ref encoderMode, value);
                UpdateMmPerPulse();
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器计数方向,0-正向，1-反向
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("15.编码器计数方向")]
        [property: Description("EncoderDirInfo")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        private EMENCODEDIR encoderDir = EMENCODEDIR.EMENCODEDIRFORW;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 触发输出模式，0是GPIO，1是脉冲，2是PWM
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("16.触发输出模式")]
        [property: Description("OutModeInfo")]
        [property: ReadOnly(true)]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        private EMOUTMODE outMode = EMOUTMODE.EMOUTMODEPULSE;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 触发模式，0-脉冲信号，1-电平翻转
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("17.触发模式")]
        [property: Description("TrigModeInfo")]
        [property: ReadOnly(true)]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        private EMTRIGMODE trigMode = EMTRIGMODE.EMTRIGMODEPULSE;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 脉冲宽度,单位10ns
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("18.脉冲宽度(10ns)")]
        [property: Description("PulseWidthInfo")]
        private int pulseWidth = 100000;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 比较器号,0-3
        /// </summary>
        [ObservableProperty]
        [property: Category("1.卡配置")]
        [property: DisplayName("19.比较器号")]
        [property: Description("CmpNOInfo")]
        private int cmpNO = 0;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 打标补偿
        /// </summary>
        [ObservableProperty]
        [property: Category("2.打标补偿")]
        [property: DisplayName("11.打标补偿(mm)")]
        [property: Description("OffestInfo")]
        private double offest = 0;

        private double pulsePerRound = 2000;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 分辨率，一圈脉冲数
        /// </summary>
        [property: Category("3.编码器信息")]
        [property: DisplayName("11.一圈脉冲数")]
        [property: Description("PulsePerRoundInfo")]
        public double PulsePerRound
        {
            get { return pulsePerRound; }
            set
            {
                SetProperty(ref pulsePerRound, value);
                UpdateMmPerPulse();
            }
        }

        private double diameter = 60;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 滚轮直径
        /// </summary>
        [property: Category("3.编码器信息")]
        [property: DisplayName("12.滚轮直径(mm)")]
        [property: Description("DiameterInfo")]
        public double Diameter
        {
            get { return diameter; }
            set
            {
                SetProperty(ref diameter, value);
                UpdateMmPerPulse();
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 一个脉冲多少mm
        /// </summary>
        [ObservableProperty]
        [property: Category("3.编码器信息")]
        [property: DisplayName("13.毫米每脉冲")]
        [property: Description("MmPerPulseInfo")]
        [property: IgnoreModifyLog()]
        [property: ReadOnly(true)]
        private double mmPerPulse;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 更新毫米每脉冲
        /// </summary>
        private void UpdateMmPerPulse()
        {
            MmPerPulse = double.Pi * Diameter / ((int)PulsePerRound * (int)EncoderMode);
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 获取补偿值，单位脉冲
        /// </summary>
        /// <returns>补偿值</returns>
        public double GetPulseOffest()
        {
            return Offest / MmPerPulse;
        }
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 编码器倍频
    /// </summary>
    public enum EMENCODERMODE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 1倍频
        /// </summary>
        [EnumString("1倍频", "1")]
        EMENCODEMONE = 1,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 2倍频
        /// </summary>
        [EnumString("2倍频", "2")]
        EMENCODEMTWO = 2,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 4倍频
        /// </summary>
        [EnumString("4倍频", "4")]
        EMENCODEMFOUR = 4,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 编码器计数方向
    /// </summary>
    public enum EMENCODEDIR
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 正向
        /// </summary>
        [EnumString("正向", "Forward")]
        EMENCODEDIRFORW = 0,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 反向
        /// </summary>
        [EnumString("反向", "Back")]
        EMENCODEDIRBACK = 1,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 触发输出模式
    /// </summary>
    public enum EMOUTMODE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// GPIO
        /// </summary>
        [EnumString("GPIO", "GPIO")]
        EMOUTMODEGPIO = 0,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 脉冲
        /// </summary>
        [EnumString("脉冲", "PULSE")]
        EMOUTMODEPULSE = 1,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// PWM
        /// </summary>
        [EnumString("PWM", "PWM")]
        EMOUTMODEPWM = 2,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 触发模式
    /// </summary>
    public enum EMTRIGMODE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 脉冲信号
        /// </summary>
        [EnumString("脉冲信号", "Pulse")]
        EMTRIGMODEPULSE = 0,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 电平翻转
        /// </summary>
        [EnumString("电平翻转", "Level")]
        EMTRIGMODELEVEL = 1,
    }
}
