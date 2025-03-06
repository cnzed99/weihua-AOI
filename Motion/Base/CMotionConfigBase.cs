using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace Motion
{
    public partial class CMotionConfigBase : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        public CMotionConfigBase()
        {
            this.token = new Token("", "Motion");
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 所属制程名
        /// </summary>
        [property: IgnoreModifyLog]
        public string PrcessName { get; set; }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public virtual void Receive(OperateMessage message) { }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 创建VM
        /// </summary>
        /// <returns>VM</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual CMotionVMBase CreateCtrlVM()
        {
            throw new NotImplementedException();
        }
    }
}
