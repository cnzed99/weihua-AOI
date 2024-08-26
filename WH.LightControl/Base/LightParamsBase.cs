using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    public partial class CLightParamsBase : ConfigModifyObservableBase
    {
        /// <summary>
        /// 20240723 TCG
        /// 主通讯参数保存路径
        /// </summary>
        public static string s_LightConfigPath = "../SystemConfig/LightConfig.Json";

        public event Action<CLightControlBase> DataContextChangedEvent;
        /// <summary>
        /// 光源品牌名称
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源品牌")]
        [property: DisplayName("光源品牌")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string lightBrandName;


        private string lightStationName = "光源1";
        /// <summary>
        /// 光源工位名称
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源工位名称")]
        [property: DisplayName("光源工位名称")]
        [property: Editor(typeof(CComboxEditorLightPro), typeof(CComboxEditorLightPro))]
        public string LightStationName
        {
            get { return lightStationName; }
            set
            {
                string old = lightStationName;
              
                if (old != value)
                {
                    try
                    {
                        string lightkey = LightBrandName + "-" + value;
                        if (CLinghtManagement.LightControlDict.ContainsKey(lightkey))
                        {
                            CLinghtManagement.LightControlDict[lightkey].BaseConfig.DataContextChangedEvent?.Invoke(CLinghtManagement.LightControlDict[lightkey]);
                        }
                        else
                        {
                            SetProperty(ref lightStationName, value);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                //lightStationName = value;

            }
        }

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

    /// <summary>
    /// 属性编辑器combox控件扩展 光源模块专用
    /// 2024.08.26 鲍赞宝
    /// </summary>
    public class CComboxEditorLightPro : PropertyEditorBase
    {
        PropertyItem _propertyItem;

        HandyControl.Controls.ComboBox comboBox;
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            _propertyItem = propertyItem;
            var proptemp = propertyItem.Value as CLightParamsBase;
            if (proptemp != null)
            {
                comboBox = new HandyControl.Controls.ComboBox();
                comboBox.ItemsSource = new List<string>() { proptemp?.LightStationName };
                comboBox.Width = 423;
                comboBox.Margin = new Thickness(-154, 0, 0, 0);
                comboBox.DropDownOpened += ComboBox_DropDownOpened;
                comboBox.VerticalAlignment = VerticalAlignment.Bottom;
                comboBox.HorizontalAlignment = HorizontalAlignment.Left;
                comboBox.FontSize = 13;
                comboBox.SetBinding(HandyControl.Controls.ComboBox.SelectedItemProperty,
                    new Binding("LightStationName") { Mode = BindingMode.TwoWay, Source = propertyItem });
            }
            return comboBox;
        }

        public override DependencyProperty GetDependencyProperty() =>
            HandyControl.Controls.ComboBox.SelectedItemProperty;

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            List<string> camlist = new List<string>();
            if (_propertyItem != null)
            {
                CLightParamsBase light = _propertyItem.Value as CLightParamsBase;
                if (light != null)
                {
                    string suppli = light.LightBrandName;
                    var camsupplis = CLinghtManagement.LightControlDict.Where(c => c.Key.Contains(suppli));

                    foreach (var camFunc in camsupplis)
                    {
                        try
                        {
                            camlist.Add(camFunc.Value.BaseConfig.LightStationName);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                comboBox.ItemsSource = camlist;
            }

        }
    }


}
