using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Entity.CommonLib;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.19 李焕彬
    /// 报警协议
    /// </summary>
    public partial class CAlarmAgreement : ConfigModifyObservableBase
    {
        public string ComName { get; set; }
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
        /// ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ComName + ":" + Name;
        }
    }
}
