using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯参数
    /// </summary>
    public partial class CCommunicationSettingBase : ConfigModifyObservableBase
    {
        public CCommunicationSettingBase()
        {
            this.token = new Token("", "CommunicationModule");
            AlarmAgreements = new ObservableCollection<CAlarmAgreement>();
        }

        #region 信息
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 名称
        /// </summary>
        [property: Category("通讯基础信息")]
        [property: DisplayName("1.名称")]
        [property: Description("1.名称")]
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯类型
        /// </summary>
        [property: Category("通讯基础信息")]
        [property: DisplayName("2.通讯类型")]
        [property: Description("2.通讯类型")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string commType = "";

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 当前版本
        /// </summary>
        [property: Category("通讯基础信息")]
        [property: DisplayName("3.当前版本")]
        [property: Description("3.当前版本")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string versions = "V1.0";

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯参数
        /// </summary>
        [ObservableProperty]
        private string guid = System.Guid.NewGuid().ToString();

        #endregion

        #region 设置

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 连接设备
        /// </summary>
        [property: Category("通讯基础设置")]
        [property: DisplayName("1.连接设备")]
        [property: Description("1.连接设备")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(PropertyEditorBase))]
        [ObservableProperty]
        private EMMACHINETYPE connectDevice;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 目标IP
        /// </summary>
        [property: Category("通讯基础设置")]
        [property: DisplayName("2.目标IP")]
        [property: Description("2.目标IP")]
        [ObservableProperty]
        private string remoteIP = "192.168.1.88";

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 目标端口
        /// </summary>
        [property: Category("通讯基础设置")]
        [property: DisplayName("3.目标端口")]
        [property: Description("3.目标端口")]
        [ObservableProperty]
        private uint remotePort = 502;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 是否启用
        /// </summary>
        [property: Category("通讯基础设置")]
        [property: DisplayName("4.是否启用")]
        [property: Description("4.是否启用")]
        [ObservableProperty]
        private bool enable = false;

        #endregion

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 报警协议
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CAlarmAgreement> alarmAgreements;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
