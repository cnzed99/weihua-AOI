using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WH.Entity.Attribute;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.18 李焕彬
    /// 枚举类型ComboBox扩展，SelectEnumValue绑定枚举值，自动获取ComboBox数据源和自动转换
    /// </summary>
    public class EnumComboBox : HandyControl.Controls.ComboBox
    {
        static EnumComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EnumComboBox),
                new FrameworkPropertyMetadata(typeof(EnumComboBox))
            );
        }

        public EnumComboBox() { }

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 枚举目标类型
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 选中枚举值
        /// </summary>
        public object SelectEnumValue
        {
            get { return (object)GetValue(SelectEnumValueProperty); }
            set { SetValue(SelectEnumValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectEnumValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectEnumValueProperty =
            DependencyProperty.Register(
                "SelectEnumValue",
                typeof(object),
                typeof(EnumComboBox),
                new FrameworkPropertyMetadata(
                    default(object),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        if (e.NewValue != null && e.NewValue is Enum enumValue)
                        {
                            EnumComboBox cmb = (EnumComboBox)d;
                            if (cmb.ItemsSource == null)
                            {
                                cmb.TargetType = enumValue.GetType();
                                cmb.ItemsSource = EnumStringAttribute.GetEnumNames(cmb.TargetType);
                            }
                            cmb.SelectedItem = EnumStringAttribute.GetEnumName(enumValue);
                        }
                    }
                )
            );

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 选中改变事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (e.AddedItems.Count > 0)
            {
                string select = e.AddedItems[0] as string;
                if (select != null)
                {
                    SelectEnumValue = EnumStringAttribute.GetEnumValue(select, TargetType);
                }
            }
        }
    }
}
