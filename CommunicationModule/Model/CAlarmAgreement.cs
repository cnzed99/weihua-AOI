using System;
using System.Collections;
using System.Collections.Generic;
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
    /// 2024.7.19 李焕彬
    /// 报警协议
    /// </summary>
    public partial class CAlarmAgreement : ConfigModifyObservableBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Abbr))]
        string name;

        /// <summary>
        /// 20240723 TCG
        /// 所属通讯名
        /// </summary>
        [IgnoreModifyLog]
        public string ComName { get; set; }

        /// <summary>
        /// 所属相机的GUID
        /// </summary>
        [IgnoreModifyLog]
        public string GUID { get; set; }

        public CAlarmAgreement()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public CAlarmAgreement(string name, IList list, CCommunicationSettingBase settings)
        {
            this.token = new Token("", this.GetType().Namespace);
            this.Name = name;
            ComName = settings.Name;
            GUID = settings.Guid;
            Protocol = list;
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 协议
        /// </summary>
        [ObservableProperty]
        private IList protocol;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 缩写
        /// </summary>
        public string Abbr => ComName + ":" + Name;
    }
}
