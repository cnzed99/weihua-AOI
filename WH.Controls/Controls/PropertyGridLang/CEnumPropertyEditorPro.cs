using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using HandyControl.Controls;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.18 李焕彬
    /// 属性编辑器枚举控件扩展，适应枚举值特性
    /// </summary>
    public class CEnumPropertyEditorPro : PropertyEditorBase
    {
        /// <summary>
        /// 2024.7.18 李焕彬
        /// 创建具体操作控件
        /// </summary>
        /// <param name="propertyItem">PropertyItem</param>
        /// <returns>具体操作控件</returns>
        public override FrameworkElement CreateElement(PropertyItem propertyItem) =>
            new EnumComboBox { IsEnabled = !propertyItem.IsReadOnly, };

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 获取具体操作控件中需要绑定的依赖属性
        /// </summary>
        /// <returns></returns>
        public override DependencyProperty GetDependencyProperty() =>
            EnumComboBox.SelectEnumValueProperty;
    }
}
