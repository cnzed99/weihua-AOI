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
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace MotionControl
{
    /// <summary>
    /// 2024.7.8 李焕彬
    /// 运动配置
    /// </summary>
    public partial class CMotionConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 操作日志
        /// </summary>
        [property: Browsable(false)]
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public CMotionConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
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
            this.RegisterSets = new ObservableCollection<CRegisterSet>()
            {
                new(this.token, "速度", "D100"),
                new(this.token, "加速度", "D116")
            };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CMotionConfig))
            {
                OperateLog.Info($"运动控制-{message.message}");
                return;
            }
            foreach (var signal in SignalIns)
            {
                if (message.obj.GetType() == typeof(CSignalIn))
                {
                    if (signal == message.obj)
                    {
                        OperateLog.Info($"运动控制-{signal.Name}:{message.message}");
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
                        OperateLog.Info($"运动控制-{signal.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
            foreach (var reg in RegisterSets)
            {
                if (message.obj.GetType() == typeof(CRegisterSet))
                {
                    if (reg == message.obj)
                    {
                        OperateLog.Info($"运动控制-{reg.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
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
        [property: DisplayName("12.正限位")]
        [property: Description("12.正限位")]
        private float softLimitP = 10000;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 负限位
        /// </summary>
        [ObservableProperty]
        [property: Category("2.轴信息")]
        [property: DisplayName("13.负限位")]
        [property: Description("13.负限位")]
        private float softLimitN = -10000;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦基准位
        /// </summary>
        [property: Category("2.轴信息")]
        [property: DisplayName("14.对焦基准位")]
        [property: Description("14.对焦基准位")]
        [ObservableProperty]
        private float focusPos = 0;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 回原点
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("11.回原点")]
        [property: Description("11.回原点")]
        private string addrGoHome = "M101";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 使能
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("12.使能")]
        [property: Description("12.使能")]
        private string addrEnable = "M100";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相对运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("13.相对运动")]
        [property: Description("13.相对运动")]
        private string addrMoveRela = "M13";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 绝对运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("14.绝对运动")]
        [property: Description("14.绝对运动")]
        private string addrMoveAbs = "M14";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 故障复位
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("15.故障复位")]
        [property: Description("15.故障复位")]
        private string addrReset = "M10";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 停止运动
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("16.停止运动")]
        [property: Description("16.停止运动")]
        private string addrStop = "M16";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 正转
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("17.正转")]
        [property: Description("17.正转")]
        private string addrFoward = "M11";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 反转
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("18.反转")]
        [property: Description("18.反转")]
        private string addrBackward = "M12";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 轴报警信号
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("19.轴报警信号")]
        [property: Description("19.轴报警信号")]
        private string addrIsAlarm = "M90";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 驱动报警信号
        /// </summary>
        [ObservableProperty]
        [property: Category("3.地址信息M")]
        [property: DisplayName("20.驱动报警信号")]
        [property: Description("20.驱动报警信号")]
        private string addrDriveAlarm = "M92";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("11.当前位置")]
        [property: Description("11.当前位置")]
        private string addrPosCur = "D104";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 绝对位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("12.绝对位置")]
        [property: Description("12.绝对位置")]
        private string addrPosAbs = "D102";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相对位置
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("13.相对位置")]
        [property: Description("13.相对位置")]
        private string addrPosRela = "D102";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前速度
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("14.当前速度")]
        [property: Description("14.当前速度")]
        private string addrSpdCur = "D108";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 当前扭矩
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("15.当前扭矩")]
        [property: Description("15.当前扭矩")]
        private string addrTorqueCur = "D112";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 正限位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("16.正限位")]
        [property: Description("16.正限位")]
        private string addrSoftLimitP = "D122";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 负限位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("17.负限位")]
        [property: Description("17.负限位")]
        private string addrSoftLimitN = "D124";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 对焦基准位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("18.对焦基准位")]
        [property: Description("18.对焦基准位")]
        private string addrFocusPos = "D118";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 对焦基准位
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("19.纠偏期望位")]
        [property: Description("19.纠偏期望位")]
        private string addrFocusDst = "D120";

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 纠偏感应值
        /// </summary>
        [ObservableProperty]
        [property: Category("4.地址信息D")]
        [property: DisplayName("20.纠偏感应值")]
        [property: Description("20.纠偏感应值")]
        private string addrSensorPos = "D126";

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
        private ObservableCollection<CRegisterSet> registerSets =
            new ObservableCollection<CRegisterSet>();
    }

    /// <summary>
    /// 2024.7.10 李焕彬
    /// 输入信号
    /// </summary>
    public partial class CSignalIn : ConfigModifyObservableBase
    {
        public CSignalIn()
        {
            this.token = new Token("", this.GetType().Namespace);
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
        private string addrM;
    }

    /// <summary>
    /// 2024.7.12 李焕彬
    /// 寄存器D写入读取
    /// </summary>
    public partial class CRegisterSet : ConfigModifyObservableBase
    {
        public CRegisterSet()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public CRegisterSet(Token token, string name = null, string addr = null)
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
        private string addr;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 读取值
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private float valueRead;

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 写入值
        /// </summary>
        [property: DisplayName("写入值")]
        [ObservableProperty]
        private float valueWrite = 5;

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
}
