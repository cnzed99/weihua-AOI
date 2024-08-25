using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    public partial class LightParamsBase : ConfigModifyObservableBase
    {
        /// <summary>
        /// 20240723 TCG
        /// 主通讯参数保存路径
        /// </summary>
        public static string s_LightConfigPath = "../SystemConfig/LightConfig.Json";
        /// <summary>
        /// 光源品牌名称
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源品牌")]
        [property: DisplayName("光源品牌")]
        [ObservableProperty]
        private string lightBrandName;

        /// <summary>
        /// 光源工位名称
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源工位名称")]
        [property: DisplayName("光源工位名称")]
        [ObservableProperty]
        private string lightStationName="光源1";

        /// <summary>
        /// 20240724 TCG
        /// 串口号
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.串口号")]
        [property: DisplayName("串口号")]
        [property: Editor(typeof(COMDevicePropertyEditor), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private COMDevice port;

        /// <summary>
        /// 20240724 TCG
        /// 波特率
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("2.波特率")]
        [property: DisplayName("波特率")]
        [ObservableProperty]
        private BAUDRATE baudRate = BAUDRATE.BAUDRATE_19200;

        /// <summary>
        /// 20240724 TCG
        /// 奇偶校验
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("3.奇偶校验")]
        [property: DisplayName("奇偶校验")]
        [ObservableProperty]
        private Parity parity = Parity.None;

        /// <summary>
        /// 20240724 TCG
        /// 数据位
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("4.数据位")]
        [property: DisplayName("数据位")]
        [ObservableProperty]
        private DATABITS dataBits = DATABITS.DATABITS_8;

        /// <summary>
        /// 20240724 TCG
        /// 停止位
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("5.停止位")]
        [property: DisplayName("停止位")]
        [ObservableProperty]
        private StopBits stopBits = StopBits.One;

        /// <summary>
        /// 20240724 TCG
        /// 握手协议
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("6.握手协议")]
        [property: DisplayName("握手协议")]
        [ObservableProperty]
        private Handshake handShake = Handshake.None;

        /// <summary>
        /// 20240724 TCG
        /// 光源通道集合
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("光源通道集合")]
        ObservableCollection<CLight> lightChannelList;
    }

    /// <summary>
    /// 20240724 TCG
    /// 单个光源通道
    /// </summary>
    public partial class CLight : ObservableObject
    {
        /// <summary>
        /// 通道名
        /// </summary>
        [ObservableProperty]
        string channel = string.Empty;

        /// <summary>
        /// 通道值
        /// </summary>
        [ObservableProperty]
        int value = 50;

        public CLight(string channel)
        {
            Channel = channel;
        }
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供四个默认COM枚举值
    /// </summary>
    public enum ECOM
    {
        [EnumString("COM1", "COM1")]
        ECOM_COM1 = 1,

        [EnumString("COM2", "COM2")]
        ECOM_COM2 = 2,

        [EnumString("COM3", "COM3")]
        ECOM_COM3 = 3,

        [EnumString("COM4", "COM4")]
        ECOM_COM4 = 4,
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供常用波特率枚举
    /// </summary>
    public enum BAUDRATE
    {
        [EnumString("2400", "2400")]
        BAUDRATE_2400 = 2400,

        [EnumString("4800", "4800")]
        BAUDRATE_4800 = 4800,

        [EnumString("9600", "9600")]
        BAUDRATE_9600 = 9600,

        [EnumString("19200", "19200")]
        BAUDRATE_19200 = 19200,

        [EnumString("38400", "38400")]
        BAUDRATE_38400 = 38400,

        [EnumString("57600", "57600")]
        BAUDRATE_57600 = 57600,

        [EnumString("115200", "115200")]
        BAUDRATE_115200 = 115200,
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供数据位枚举
    /// </summary>
    public enum DATABITS
    {
        [EnumString("8", "8")]
        DATABITS_8 = 8,

        [EnumString("16", "16")]
        DATABITS_16 = 16,
    }
}
