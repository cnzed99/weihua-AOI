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
using Motion;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace PlcControl
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 运动配置
    /// </summary>
    public partial class CMotionConfig : CMotionConfigBase
    {
        [ObservableProperty]
        private ushort isStartAddr = 484;

        public CMotionConfig()
            : base()
        {
            this.SignalIns = new ObservableCollection<CSignalIn>()
            {
                new CSignalIn(token, "下位机启动状态", 1603, "启动时亮灯，关闭时熄灭"),
                new CSignalIn(token, "料仓满料", 1604, "入料仓或出料仓计数达到料仓容量"),
                new CSignalIn(token, "卡瓶", 1602, "卡瓶时亮灯"),
                new CSignalIn(token, "复位中", 321, "复位时亮灯"),
            };
            this.SignalOuts = new ObservableCollection<CSignalOut>()
            {
                new CSignalOut(token, "入口料仓清零", 1606, "打开关闭一次，入口料仓清零"),
                new CSignalOut(token, "出口料仓清零", 1605, "打开关闭一次，出口料仓清零"),
                new CSignalOut(token, "入口手动吹料", 482),
                new CSignalOut(token, "出口手动吹料", 483),
                new CSignalOut(token, "启动输送带", 481),
                new CSignalOut(token, "连续触发相机", 194, "打开时连续触发相机，不要在运行时启动！！"),
            };
            this.RegisterSets = new ObservableCollection<CElement>()
            {
                new CElement(token, "入口计数", EMELEMTYPE.EMELEMD_INT, 0, 0),
                new CElement(token, "出口计数", EMELEMTYPE.EMELEMD_INT, 2, 0),
                new CElement(token, "拍照计数", EMELEMTYPE.EMELEMD_INT, 308, 0),
                new CElement(token, "入口料仓计数", EMELEMTYPE.EMELEMD_INT, 302, 0),
                new CElement(token, "出口料仓计数", EMELEMTYPE.EMELEMD_INT, 300, 0),
                //new CElement(token, "料仓容量", EMELEMTYPE.EMELEMD_DINT, 304, 999),
                new CElement(token, "入口吹料时长/10ms", EMELEMTYPE.EMELEMD_INT, 16, 5),
                new CElement(token, "出口吹料时长/10ms", EMELEMTYPE.EMELEMD_INT, 18, 4),
                new CElement(token, "卡瓶超时/100ms", EMELEMTYPE.EMELEMD_INT, 6, 100),
                new CElement(token, "倒瓶超时", EMELEMTYPE.EMELEMD_INT, 306, 5),
                new CElement(token, "感应器滤波/10ms", EMELEMTYPE.EMELEMD_INT, 14, 0),
                new CElement(token, "拍照延迟", EMELEMTYPE.EMELEMD_INT, 308, 0),
            };
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public override void Receive(OperateMessage message)
        {
            base.Receive(message);
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
                if (message.obj.GetType() == typeof(CElement))
                {
                    if (reg == message.obj)
                    {
                        OperateLog.Info($"运动控制-寄存器-{reg.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// IP地址
        /// </summary>
        [ObservableProperty]
        [property: Category("1.连接信息")]
        [property: DisplayName("11.IP")]
        [property: Description("11.IP")]
        private string iP = "192.168.250.100";

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 端口号
        /// </summary>
        [ObservableProperty]
        [property: Category("1.连接信息")]
        [property: DisplayName("12.Port")]
        [property: Description("12.Port")]
        private int port = 502;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 端口号
        /// </summary>
        [ObservableProperty]
        [property: Category("2.参数设置")]
        [property: DisplayName("11.Capcity")]
        [property: Description("11.Capcity")]
        private short binCapcity = 999;

        //partial void OnBinCapcityChanged(short value)
        //{
        //    MontionFunc?.WriteSingleRegisterInt32(304,(Int32)value);
        //}
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 输入信号
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("输入信号")]
        [ObservableProperty]
        private ObservableCollection<CSignalIn> signalIns = new ObservableCollection<CSignalIn>();

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 输出信号
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("输出信号")]
        [ObservableProperty]
        private ObservableCollection<CSignalOut> signalOuts =
            new ObservableCollection<CSignalOut>();

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 寄存器读写
        /// </summary>
        [property: Browsable(false)]
        [property: DisplayName("寄存器读写")]
        [ObservableProperty]
        private ObservableCollection<CElement> registerSets = new ObservableCollection<CElement>();
    }

    /// <summary>
    /// 2025.3.6 李焕彬
    /// 输入信号
    /// </summary>
    public partial class CSignalIn : ConfigModifyObservableBase
    {
        public CSignalIn()
        {
            this.token = new Token("", "PlcControl");
        }

        public CSignalIn(Token token, string name, ushort addr, string description = "null")
        {
            this.token = token;
            this.Name = name;
            this.Addr = addr;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 地址
        /// </summary>
        [property: DisplayName("地址")]
        [ObservableProperty]
        private ushort addr;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 有信号
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private bool hasSignal = false;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 名称
        /// </summary>
        [property: DisplayName("名称")]
        [ObservableProperty]
        private string name = "自定义";

        /// <summary>
        /// 2025.3.6 李焕彬
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
    /// 2025.3.6 李焕彬
    /// 输出信号
    /// </summary>
    public partial class CSignalOut : CSignalIn
    {
        public CSignalOut()
            : base() { }

        public CSignalOut(Token token, string name, ushort addr, string description = "null")
            : base(token, name, addr, description) { }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 常开/常闭状态
        /// </summary>
        [property: DisplayName("常开/常闭状态")]
        [ObservableProperty]
        private bool set = false;
    }

    /// <summary>
    /// 2025.3.6 李焕彬
    /// 元件
    /// </summary>
    public partial class CElement : ConfigModifyObservableBase
    {
        public CElement()
            : base()
        {
            this.token = new Token("", "PlcControl");
        }

        public CElement(Token token)
        {
            this.token = token;
        }

        public CElement(
            Token token,
            string name,
            EMELEMTYPE type,
            ushort addr,
            float writeValue,
            string description = "null"
        )
        {
            this.token = token;
            this.Name = name;
            this.Type = type;
            this.Addr = addr;
            this.Description = description;
            this.WriteValue = writeValue;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 名称
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("1.名称")]
        [property: Description("1.名称")]
        [ObservableProperty]
        private string name = "自定义";

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 类型
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("2.类型")]
        [property: Description("2.类型")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        [ObservableProperty]
        private EMELEMTYPE type;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 地址
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("3.地址")]
        [property: Description("3.地址")]
        [ObservableProperty]
        private ushort addr;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入值
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("4.写入值")]
        [property: Description("4.写入值")]
        [ObservableProperty]
        private float writeValue = 0;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取值
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private float readValue = 0;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 描述
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("5.描述")]
        [property: Description("5.描述")]
        [ObservableProperty]
        private string description;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}";
        }
    }

    /// <summary>
    /// 2025.3.6 李焕彬
    /// 元件类型
    /// </summary>
    public enum EMELEMTYPE
    {
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 线圈
        /// </summary>
        [EnumString("线圈", "M")]
        EMELEMM,

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 寄存器(REAL)
        /// </summary>
        [EnumString("寄存器(REAL)", "D(REAL)")]
        EMELEMD_REAL,

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 寄存器(INT)
        /// </summary>
        [EnumString("寄存器(INT)", "D(INT)")]
        EMELEMD_INT,

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 寄存器(DINT)
        /// </summary>
        [EnumString("寄存器(DINT)", "D(DINT)")]
        EMELEMD_DINT,
    }
}