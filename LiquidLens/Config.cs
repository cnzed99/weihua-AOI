using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FocusControl;
using HandyControl.Controls;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace LiquidLens
{
    /// <summary>
    /// 2024.9.28 李焕彬
    /// 液态镜头
    /// </summary>
    public partial class CConfig : CFocusConfigBase
    {
        public CConfig()
        {
            FocusType = "LiquidLens";
            this.token = new Token("", this.GetType().Namespace);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public override void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CConfig))
            {
                OperateLog.Info($"{PrcessName}-液态镜头-{message.message}");
                return;
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
            var vm = new CSetCtrlVM();
            vm.Config = this;
            return vm;
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 串口号
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("00.串口号")]
        [property: Editor(typeof(COMDevicePropertyEditor), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private COMDevice port = new COMDevice();

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 波特率
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("01.波特率")]
        [ObservableProperty]
        private BAUDRATE baudRate = BAUDRATE.BAUDRATE_115200;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 奇偶校验
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("02.奇偶校验")]
        [ObservableProperty]
        private Parity parity = Parity.None;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 数据位
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("03.数据位")]
        [ObservableProperty]
        private DATABITS dataBits = DATABITS.DATABITS_8;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 停止位
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("04.停止位")]
        [ObservableProperty]
        private StopBits stopBits = StopBits.One;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 握手协议
        /// </summary>
        [property: Category("1.液态镜头连接")]
        [property: DisplayName("05.握手协议")]
        [ObservableProperty]
        private Handshake handShake = Handshake.None;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 物距正限位
        /// </summary>
        [ObservableProperty]
        [property: Category("2.限位参数")]
        [property: DisplayName("00.正限位(mm)")]
        private float softLimitP = 115f;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 物距负限位
        /// </summary>
        [ObservableProperty]
        [property: Category("2.限位参数")]
        [property: DisplayName("01.负限位(mm)")]
        private float softLimitN = 105f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K0
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("00.K0")]
        private double volFactor0 = -431.18f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K1
        /// </summary>
        [property: Category("3.物距电压公式")]
        [property: DisplayName("01.K1")]
        [ObservableProperty]
        private double volFactor1 = 19.1302027373f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K2
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("02.K2")]
        private double volFactor2 = -0.2642850952f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K3
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("03.K3")]
        private double volFactor3 = 0.0015177977f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K4
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("04.K4")]
        private double volFactor4 = -0.0000031956f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K5
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("05.K5")]
        private double volFactor5 = 0.0f;

        /// <summary>
        /// 2024.9.27 李焕彬
        /// 物距电压公式系数K6
        /// </summary>
        [ObservableProperty]
        [property: Category("3.物距电压公式")]
        [property: DisplayName("06.K6")]
        private double volFactor6 = 0.0f;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 串口号
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("00.串口号")]
        [property: Editor(typeof(COMDevicePropertyEditor), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private COMDevice portSensor = new COMDevice();

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 波特率
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("01.波特率")]
        [ObservableProperty]
        private BAUDRATE baudRateSensor = BAUDRATE.BAUDRATE_38400;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 奇偶校验
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("02.奇偶校验")]
        [ObservableProperty]
        private Parity paritySensor = Parity.None;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 数据位
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("03.数据位")]
        [ObservableProperty]
        private DATABITS dataBitsSensor = DATABITS.DATABITS_8;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 停止位
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("04.停止位")]
        [ObservableProperty]
        private StopBits stopBitsSensor = StopBits.One;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 握手协议
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("05.握手协议")]
        [ObservableProperty]
        private Handshake handShakeSensor = Handshake.None;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 从站地址
        /// </summary>
        [property: Category("4.传感器连接")]
        [property: DisplayName("06.从站地址")]
        [ObservableProperty]
        private byte slaveSensor = 2;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 对焦基准位
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("00.最佳对焦位置(mm)")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private float focusPos = 0;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 粗调步长
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("01.粗调步长(mm)")]
        [ObservableProperty]
        private float stepCoarse = 0.1f;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 精调步长
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("02.精调步长(mm)")]
        [ObservableProperty]
        private float stepFine = 0.002f;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 最小清晰度
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("03.最小清晰度")]
        [ObservableProperty]
        private float minDistinct = 10;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 精调范围
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("04.精调范围(mm)")]
        [ObservableProperty]
        private float fineRange = 0.3f;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 传感器位置
        /// </summary>
        [property: Category("5.对焦参数")]
        [property: DisplayName("05.传感器位置(mm)")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private double focusSensorPos = 0;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 由物距获取电压
        /// </summary>
        /// <param name="dDis">物距</param>
        /// <returns>电压</returns>
        public double GetVoltage(double dDis)
        {
            return VolFactor0
                + VolFactor1 * dDis
                + VolFactor2 * Math.Pow(dDis, 2)
                + VolFactor3 * Math.Pow(dDis, 3)
                + VolFactor4 * Math.Pow(dDis, 4)
                + VolFactor5 * Math.Pow(dDis, 5)
                + VolFactor6 * Math.Pow(dDis, 6);
        }
    }
}
