using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    /// <summary>
    ///  20240825 鲍赞宝
    /// 光源参数类基类
    /// </summary>
    public partial class CLightParamsBase : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        public CLightParamsBase()
        {
            this.token = new Token("", "LightModule"); //固定token,使得插件参数更改可以通知到管理类的消息处理函数
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                this,
                new Token("", "WH.LightControl")
            );
        }

        /// <summary>
        /// 20240723 TCG
        /// 主通讯参数保存路径
        /// </summary>
       // public static string s_LightConfigPath = "../SystemConfig/LightConfig.Json";

       // public event Action<CLightControlBase> DataContextChangedEvent;

        /// <summary>
        /// 光源品牌名称
        /// 2024.08.25 鲍赞宝
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源品牌")]
        [property: DisplayName("光源品牌")]
        [property: Browsable(false)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string lightBrandName;

        /// <summary>
        /// 光源工位名称
        /// 2024.08.25 鲍赞宝
        /// </summary>
        [property: Category("基础信息")]
        [property: Description("1.光源工位名称")]
        [property: DisplayName("光源工位名称")]
        [property: Browsable(false)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string lightStationName = "光源1";

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

        /// <summary>
        /// 20240828 TCG
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj is CLight light)
            {
                Messenger.Send(new OperateMessage(this, light.Channel + message.message), token);
                return;
            }
        }
    }

    /// <summary>
    /// 20240724 TCG
    /// 单个光源通道
    /// </summary>
    public partial class CLight : ConfigModifyObservableBase
    {
        /// <summary>
        /// 通道名
        /// </summary>
        [property: DisplayName("通道名")]
        [ObservableProperty]
        string channel = string.Empty;

        /// <summary>
        /// 通道值
        /// </summary>
        [property: IgnoreModifyLog]
        [property: DisplayName("通道值")]
        [ObservableProperty]
        int value = 50;

        /// <summary>
        /// 当前通道值
        /// </summary>
        [property: DisplayName("当前设定值")]
        [ObservableProperty]
        int lightvalue = 50;

        public CLight(string channel)
        {
            this.token = new Token("", "WH.LightControl"); //固定token,使得插件参数更改可以通知到管理类的消息处理函数
            Channel = channel;
        }
    }
}
