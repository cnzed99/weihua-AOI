using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using WH.Controls;
using System.Reflection;

namespace Modbus
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// Modbus通讯参数
    /// </summary>
    public partial class CModbusSetting: CCommunicationSettingBase
    {
        public CModbusSetting() : base()
        {
            TestElems = new ObservableCollection<CElement>();
        }

        /// <summary>
        /// 2024.7.10 李焕彬
        /// 测试元件
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CElement> testElems;
    }

    /// <summary>
    /// 2024.7.10 李焕彬
    /// 元件
    /// </summary>
    public partial class CElement : ConfigModifyObservableBase
    {
        public CElement() : base()
        {
            this.token = new Token("", "CommunicationModule");
        }

        public CElement(Token token, string name, ushort addr)
        {
            this.token = token;
            this.Name = name;
            this.Addr = addr;
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
