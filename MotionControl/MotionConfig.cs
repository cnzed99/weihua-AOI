using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FocusControl;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace MotionControl
{
    /// <summary>
    /// 2024.7.8 李焕彬
    /// 运动配置
    /// </summary>
    public partial class CMotionConfig : CFocusConfigBase
    {
        [Browsable(false)]
        [JsonIgnore]
        [IgnoreModifyLog]
        string[] XIOName = new string[10]
        {
            "负限位",
            "正限位",
            "原点",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用"
        };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreModifyLog]
        string[] YIOName = new string[10]
        {
            "脉冲",
            "方向",
            "使能",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用",
            "备用"
        };

        public CMotionConfig()
            : base()
        {
            FocusType = "MotionControl";
            this.token = new Token("", "FocusControl");
            var signalIn = new ObservableCollection<CSignalIn>();
            for (int i = 0; i < 10; i++)
            {
                signalIn.Add(new CSignalIn(token, $"X{i}", i));
            }
            this.SignalIns = signalIn;
            var signalOut = new ObservableCollection<CSignalOut>();
            for (int i = 0; i < 10; i++)
            {
                signalOut.Add(new CSignalOut(token, $"Y{i}", i));
            }
            this.SignalOuts = signalOut;
            this.RegisterSets = new ObservableCollection<CElement>() { };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public override void Receive(OperateMessage message)
        {
            base.Receive(message);
            if (message.obj.GetType() == typeof(CMotionConfig))
            {
                OperateLog.Info($"{PrcessName}-运动控制-{message.message}");
                return;
            }
            foreach (var signal in SignalIns)
            {
                if (message.obj.GetType() == typeof(CSignalIn))
                {
                    if (signal == message.obj)
                    {
                        OperateLog.Info($"{PrcessName}-运动控制-{signal.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
            foreach (var signal in SignalOuts)
            {
                if (message.obj.GetType() == typeof(CSignalOut))
                {
                    if (signal == message.obj)
                    {
                        OperateLog.Info($"{PrcessName}-运动控制-{signal.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
            foreach (var reg in RegisterSets)
            {
                if (message.obj.GetType() == typeof(CElement))
                {
                    if (reg == message.obj)
                    {
                        OperateLog.Info($"{PrcessName}-运动控制-{reg.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 创建VM
        /// </summary>
        /// <returns>VM</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CFocusCtrlVMBase CreateCtrlVM()
        {
            var vm = new CMotionCtrlVM();
            vm.MotionConfig = this;
            return vm;
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// IP地址
        /// </summary>
        [ObservableProperty]
        [property: Category("1.连接信息")]
        [property: DisplayName("11.IP")]
        [property: Description("11.IP")]
        private string iP = "192.168.1.88";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 端口号
        /// </summary>
        [ObservableProperty]
        [property: Category("1.连接信息")]
        [property: DisplayName("12.Port")]
        [property: Description("12.Port")]
        private int port = 502;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 轴名称
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("11.轴名称")]
        [property: Description("11.轴名称")]
        private string axisName = "对焦轴";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 正限位
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("12.正限位(mm)")]
        [property: Description("12.正限位(mm)")]
        private float softLimitP = 16f;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 负限位
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("13.负限位(mm)")]
        [property: Description("13.负限位(mm)")]
        private float softLimitN = -15f;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 运行（纠偏）速度
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("14.运行速度(mm/s)")]
        [property: Description("14.运行速度(mm/s)")]
        private float speed = 10;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 加速度
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("15.加速度(mm²/s)")]
        [property: Description("15.加速度(mm²/s)")]
        private float acc = 50;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 回原点
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("11.回原点")]
        [property: Description("11.回原点")]
        private ushort addrGoHome = 101;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 使能
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("12.使能")]
        [property: Description("12.使能")]
        private ushort addrEnable = 100;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相对运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("13.相对运动")]
        [property: Description("13.相对运动")]
        private ushort addrMoveRela = 13;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 绝对运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("14.绝对运动")]
        [property: Description("14.绝对运动")]
        private ushort addrMoveAbs = 14;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 故障复位
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("15.故障复位")]
        [property: Description("15.故障复位")]
        private ushort addrReset = 10;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 停止运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("16.停止运动")]
        [property: Description("16.停止运动")]
        private ushort addrStop = 16;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 正转
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("17.正转")]
        [property: Description("17.正转")]
        private ushort addrFoward = 11;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 反转
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("18.反转")]
        [property: Description("18.反转")]
        private ushort addrBackward = 12;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 轴报警信号
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("19.轴报警信号")]
        [property: Description("19.轴报警信号")]
        private ushort addrIsAlarm = 90;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 驱动报警信号
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("20.驱动报警信号")]
        [property: Description("20.驱动报警信号")]
        private ushort addrDriveAlarm = 92;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("11.当前位置")]
        [property: Description("11.当前位置")]
        private ushort addrPosCur = 104;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 绝对位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("12.绝对位置")]
        [property: Description("12.绝对位置")]
        private ushort addrPosAbs = 102;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相对位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("13.相对位置")]
        [property: Description("13.相对位置")]
        private ushort addrPosRela = 102;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前速度
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("14.当前速度")]
        [property: Description("14.当前速度")]
        private ushort addrSpdCur = 108;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前扭矩
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("15.当前扭矩")]
        [property: Description("15.当前扭矩")]
        private ushort addrTorqueCur = 112;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 对焦基准位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("18.对焦基准位")]
        [property: Description("18.对焦基准位")]
        private ushort addrFocusPos = 118;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 纠偏期望位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("19.纠偏期望位")]
        [property: Description("19.纠偏期望位")]
        private ushort addrFocusDst = 120;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 纠偏感应值
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("20.纠偏感应值")]
        [property: Description("20.纠偏感应值")]
        private ushort addrSensorPos = 126;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 速度地址
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("21.速度")]
        [property: Description("21.速度")]
        private ushort addrSpeed = 100;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 加速度地址
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("22.加速度")]
        [property: Description("22.加速度")]
        private ushort addrAcc = 116;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 启动纠偏
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("23.启动纠偏")]
        [property: Description("23.启动纠偏")]
        private ushort addrStartFocus = 111;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 纠偏归零
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("24.纠偏归零")]
        [property: Description("24.纠偏归零")]
        private ushort addrSetZero = 113;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦基准位
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("11.最佳对焦位置(mm)")]
        [property: Description("11.最佳对焦位置(mm)")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private float focusPos = 0;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 粗调步长
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("12.粗调步长(mm)")]
        [property: Description("12.粗调步长(mm)")]
        [ObservableProperty]
        private float stepCoarse = 0.1f;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 精调步长
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("13.精调步长(mm)")]
        [property: Description("13.精调步长(mm)")]
        [ObservableProperty]
        private float stepFine = 0.002f;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 最小清晰度
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("14.最小清晰度")]
        [property: Description("14.最小清晰度")]
        [ObservableProperty]
        private float minDistinct = 10;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 精调范围
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("15.精调范围(mm)")]
        [property: Description("15.精调范围(mm)")]
        [ObservableProperty]
        private float fineRange = 0.3f;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 对焦速度
        /// </summary>
        [ObservableProperty]
        [property: Category("5.对焦参数")]
        [property: DisplayName("16.对焦速度(mm/s)")]
        [property: Description("16.对焦速度(mm/s)")]
        private float speedFocus = 5;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 输入信号
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("输入信号")]
        [ObservableProperty]
        private ObservableCollection<CSignalIn> signalIns = new ObservableCollection<CSignalIn>();

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 输出信号
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("输出信号")]
        [ObservableProperty]
        private ObservableCollection<CSignalOut> signalOuts =
            new ObservableCollection<CSignalOut>();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 寄存器读写
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("寄存器读写")]
        [ObservableProperty]
        private ObservableCollection<CElement> registerSets = new ObservableCollection<CElement>();
    }

    /// <summary>
    /// 2024.7.10 李焕彬
    /// 输入信号
    /// </summary>
    public partial class CSignalIn : ConfigModifyObservableBase
    {
        public CSignalIn()
        {
            this.token = new Token("", "FocusControl");
        }

        public CSignalIn(Token token, string name, int addr)
        {
            this.token = token;
            this.Name = name;
            this.Addr = addr;
        }

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 地址
        /// </summary>
        [property: DisplayName("地址")]
        [ObservableProperty]
        private int addr;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 有信号
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private bool hasSignal = false;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 名称
        /// </summary>
        [property: DisplayName("名称")]
        [ObservableProperty]
        private string name = "自定义";

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 描述
        /// </summary>
        [property: DisplayName("描述")]
        [ObservableProperty]
        private string description;

        public override string ToString()
        {
            return $"{Addr}-{Name}";
        }
    }

    /// <summary>
    /// 2024.7.10 李焕彬
    /// 输出信号
    /// </summary>
    public partial class CSignalOut : CSignalIn
    {
        public CSignalOut()
            : base() { }

        public CSignalOut(Token token, string name, int addr)
            : base(token, name, addr) { }

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 常开/常闭状态
        /// </summary>
        [property: DisplayName("常开/常闭状态")]
        [ObservableProperty]
        private bool set = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 写入地址M
        /// </summary>
        [property: DisplayName("写入地址M")]
        [ObservableProperty]
        private ushort addrM;
    }

    /// <summary>
    /// 2024.7.10 李焕彬
    /// 元件
    /// </summary>
    public partial class CElement : ConfigModifyObservableBase
    {
        public CElement()
            : base()
        {
            this.token = new Token("", "FocusControl");
        }

        public CElement(Token token)
        {
            this.token = token;
        }

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 名称
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("1.名称")]
        [property: Description("1.名称")]
        [ObservableProperty]
        private string name = "自定义";

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 类型
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("2.类型")]
        [property: Description("2.类型")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        [ObservableProperty]
        private EMELEMTYPE type;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 地址
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("3.地址")]
        [property: Description("3.地址")]
        [ObservableProperty]
        private ushort addr;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 写入值
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("4.写入值")]
        [property: Description("4.写入值")]
        [ObservableProperty]
        private float writeValue;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 读取值
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private float readValue = 0;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 描述
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("5.描述")]
        [property: Description("5.描述")]
        [ObservableProperty]
        private string description;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}";
        }
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 元件类型
    /// </summary>
    public enum EMELEMTYPE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 线圈
        /// </summary>
        [EnumString("线圈", "M")]
        EMELEMM,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 寄存器
        /// </summary>
        [EnumString("寄存器", "D")]
        EMELEMD,
    }
}
