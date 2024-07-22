using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace TCPIP
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// Modbus通讯参数
    /// </summary>
    public partial class CTcpIpCommunicationSetting : CCommunicationSettingBase
    {
        public CTcpIpCommunicationSetting()
            : base() { }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 本地IP地址
        /// </summary>
        [property: Category("1.基础设置")]
        [property: DisplayName("1.本地IP")]
        [property: Description("1.本地IP地址")]
        [ObservableProperty]
        private string localIP = "192.168.250.77";

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 本地端口号
        /// </summary>
        [property: Category("1.基础设置")]
        [property: DisplayName("2.本地端口")]
        [property: Description("2.本地端口号")]
        [ObservableProperty]
        private uint localPort = 2060;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 默认转码类型
        /// </summary>
        [property: Category("1.基础设置")]
        [property: DisplayName("3.默认转码类型")]
        [property: Description("3.默认转码类型")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private EMENCODING encodingType = EMENCODING.EMENCODENONE;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 默认进制转换
        /// </summary>
        [property: Category("1.基础设置")]
        [property: DisplayName("4.默认进制转换")]
        [property: Description("4.默认进制转换")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private EMSYSCONVERT sysConvertType = EMSYSCONVERT.EMCONVERTDECIMAL;
    }

    /// <summary>
    /// 2024.7.19 李焕彬
    /// 指令信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class CDataInfo : ConfigModifyObservableBase
    {
        public CDataInfo()
            : base()
        {
            this.token = new Token("", "CommunicationModule");
        }

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 自定义名称
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("1.名称")]
        [property: Description("1.名称")]
        [ObservableProperty]
        private string name = "自定义";

        private int dataIndex = 0;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 字段索引
        /// </summary>
        [Category("1.配置")]
        [Description("2.字段索引")]
        [DisplayName("2.字段索引")]
        public int DataIndex
        {
            get { return dataIndex; }
            set
            {
                if (value + DataLength >= CTcpIpCommPart.MaxbuffLength)
                    Growl.Error($"索引加长度超出最大数据长度{CTcpIpCommPart.MaxbuffLength}!");
                else
                    SetProperty(ref dataIndex, value);
            }
        }

        private string dataValue = "0";

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 字段值
        /// </summary>
        [property: Category("1.配置")]
        [property: Description("3.字段值")]
        [property: DisplayName("3.字段值")]
        public string DataValue
        {
            get { return dataValue; }
            set { SetProperty(ref dataValue, value); }
        }

        private int dataLength = 1;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 字段长度
        /// </summary>
        [Category("1.配置")]
        [Description("4.字段长度")]
        [DisplayName("4.字段长度")]
        public int DataLength
        {
            get { return dataLength; }
            set
            {
                if (value + DataIndex >= CTcpIpCommPart.MaxbuffLength)
                    Growl.Error($"索引加长度超出最大数据长度{CTcpIpCommPart.MaxbuffLength}!");
                else
                    SetProperty(ref dataLength, value);
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 转码类型
        /// </summary>
        [property: Category("1.配置")]
        [property: Description("5.转码类型")]
        [property: DisplayName("5.转码类型")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        [ObservableProperty]
        private EMENCODING encodingType = EMENCODING.EMENCODEASCII;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 进制转换
        /// </summary>
        [property: Category("1.配置")]
        [property: DisplayName("6.进制转换")]
        [property: Description("6.进制转换")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        [ObservableProperty]
        private EMSYSCONVERT sysConvertType = EMSYSCONVERT.EMCONVERTDECIMAL;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 说明
        /// </summary>
        [property: Category("1.配置")]
        [property: Description("7.说明")]
        [property: DisplayName("7.说明")]
        [ObservableProperty]
        private string note = "";

        /// <summary>
        /// 2024.7.21 李焕彬
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 字符串转为字节
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>字节</returns>
        public byte[] GetBytes(string str)
        {
            return CConvertBytes.GetBytes(str, SysConvertType, EncodingType);
        }

        /// <summary>
        /// 字节转字符串
        /// </summary>
        /// <param name="bytes">字节</param>
        /// <returns>字符串</returns>
        public string GetString(byte[] bytes)
        {
            return CConvertBytes.GetString(bytes, SysConvertType, EncodingType);
        }
    }
}
