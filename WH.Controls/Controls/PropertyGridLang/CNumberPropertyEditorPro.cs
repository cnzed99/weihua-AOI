using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WH.Controls.Controls.PropertyGridLang
{
    /// <summary>
    /// 2024.7.24 李焕彬
    /// 属性编辑器数字控件扩展，绑定触发模式改为失去焦点触发
    /// </summary>
    public class CPlainTextPropertyEditorProPro : PlainTextPropertyEditor
    {
        /// <summary>
        /// 2024.7.24 李焕彬
        /// 重载设置绑定触发模式为失去焦点触发
        /// </summary>
        /// <param name="propertyItem">属性</param>
        /// <returns>绑定触发模式</returns>
        public override UpdateSourceTrigger GetUpdateSourceTrigger(PropertyItem propertyItem)
        {
            return UpdateSourceTrigger.LostFocus;
        }
    }
}
