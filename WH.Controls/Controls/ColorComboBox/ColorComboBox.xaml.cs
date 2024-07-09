using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using WH.Entity.CommonLib;

namespace WH.Controls
{
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 颜色ComBox控件 颜色值绑定SelectColor
    /// ColorComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class ColorComboBox : HandyControl.Controls.ComboBox
    {
        public ColorComboBox()
        {
            InitializeComponent();

            this.CmbColor.ItemsSource = CBrushPro.s_Instance.KnownColors;
        }

        /// <summary>
        /// 选中颜色
        /// </summary>
        public CKnownColor SelectColor
        {
            get { return (CKnownColor)GetValue(SelectBrushProperty); }
            set { SetValue(SelectBrushProperty, value); }
        }

        /// <summary>
        /// 选中颜色属性
        /// </summary>
        // Using a DependencyProperty as the backing store for SelectColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBrushProperty =
            DependencyProperty.Register("SelectColor", typeof(CKnownColor), typeof(ColorComboBox), new PropertyMetadata(default(CKnownColor), (d, e) =>
            {
                if (e.NewValue != null)
                {
                    ColorComboBox cmb = (ColorComboBox)d;
                    cmb.CmbColor.SelectedItem = e.NewValue;
                }
            }));

        /// <summary>
        /// 选中改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CKnownColor knownColor = e.AddedItems[0] as CKnownColor;
                if (knownColor != null)
                {
                    SelectColor = knownColor;
                }
            }
        }
    }
}
