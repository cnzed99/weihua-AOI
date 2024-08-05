using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using WH.Entity.CommonLib;

namespace WH.Controls
{
    public class CDynamicPropertyEditor : PropertyEditorBase
    {
        /// <summary>
        /// 20240802 TCG
        /// 创建具体操作控件
        /// </summary>
        /// <param name="propertyItem">PropertyItem</param>
        /// <returns>具体操作控件</returns>
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            if (propertyItem.PropertyType.IsEnum)
            {
                return new EnumComboBox { IsEnabled = !propertyItem.IsReadOnly, };
            }
            else if (propertyItem.PropertyType.IsSubclassOf(typeof(CKnownColor)))
            {
                return new ColorComboBox { IsEnabled = !propertyItem.IsReadOnly, };
            }
            else
            {
                return new TextBlock() { IsEnabled = !propertyItem.IsReadOnly, };
            }
        }

        /// <summary>
        /// 20240802 TCG
        /// 获取具体操作控件中需要绑定的依赖属性
        /// </summary>
        /// <returns></returns>
        public override DependencyProperty GetDependencyProperty() =>
            EnumComboBox.SelectEnumValueProperty;
    }
}
