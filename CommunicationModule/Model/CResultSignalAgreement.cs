using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace CommunicationModule
{
    /// <summary>
    /// 2026.8.3 鲍赞宝
    /// 结果信号协议
    /// </summary>
    public partial class CResultSignalAgreement : ConfigModifyObservableBase
    {
        /// <summary>
        ///  2026.8.3 鲍赞宝
        /// 所属通讯名
        /// </summary>
        [IgnoreModifyLog]
        public string ComName { get; set; }

        /// <summary>
        /// 所属相机的GUID
        /// </summary>
        [IgnoreModifyLog]
        public string GUID { get; set; }

        /// <summary>
        /// 2024.8.6 鲍赞宝
        /// 结果信号名称
        /// </summary>
        [property: DisplayName("信号名称")]
        [NotifyPropertyChangedFor(nameof(Abbr))]
        [ObservableProperty]
        private string name = "自定义";

        public CResultSignalAgreement()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public CResultSignalAgreement(string name, IList list, CCommunicationSettingBase settings)
        {
            this.token = new Token("", this.GetType().Namespace);
            this.Name = name;
            ComName = settings.Name;
            GUID = settings.Guid;
            Protocol = list;
        }

        /// <summary>
        ///  2026.8.3 鲍赞宝
        /// 协议
        /// </summary>
        [ObservableProperty]
        private IList protocol;

        /// <summary>
        /// 2026.8.3 鲍赞宝
        /// 缩写
        /// </summary>
        [IgnoreModifyLog]
        public string Abbr => ComName + ":" + Name;

        /// <summary>
        ///  2026.8.3 鲍赞宝
        /// ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ComName + ":" + Name;
        }
    }
}
