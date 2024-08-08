using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.18 李焕彬
    /// 属性编辑器集合编辑器扩展
    /// </summary>
    public class CCollectionPropertyEditor : PropertyEditorBase
    {
        /// <summary>
        /// 2024.7.18 李焕彬
        /// 创建具体操作控件
        /// </summary>
        /// <param name="propertyItem">PropertyItem</param>
        /// <returns>具体操作控件</returns>
        public override FrameworkElement CreateElement(PropertyItem propertyItem) =>
            new CCollectionButton
            {
                IsEnabled = !propertyItem.IsReadOnly,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 获取具体操作控件中需要绑定的依赖属性
        /// </summary>
        /// <returns></returns>
        public override DependencyProperty GetDependencyProperty() => CCollectionButton.TagProperty;
    }
}
