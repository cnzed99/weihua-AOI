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

namespace FocusControl
{
    public partial class CFocusConfigBase : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        public CFocusConfigBase() { }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 所属制程名
        /// </summary>
        [property: IgnoreModifyLog]
        public string PrcessName { get; set; }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 类型，插件dll名
        /// </summary>
        public string FocusType { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public virtual void Receive(OperateMessage message) { }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 创建VM
        /// </summary>
        /// <returns>VM</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual CFocusCtrlVMBase CreateCtrlVM()
        {
            throw new NotImplementedException();
        }
    }
}
