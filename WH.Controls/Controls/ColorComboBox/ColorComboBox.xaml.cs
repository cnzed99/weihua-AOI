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
    public partial class ColorComboBox : UserControl
    {
        public ColorComboBox()
        {
            InitializeComponent();

            this.CmbColor.ItemsSource = BrushPro.instance.KnownColors;
        }

        public Brush SelectColor
        {
            get { return (Brush)GetValue(SelectBrushProperty); }
            set { SetValue(SelectBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBrushProperty =
            DependencyProperty.Register("SelectColor", typeof(Brush), typeof(ColorComboBox), new PropertyMetadata(default(Brush), (d, e) =>
            {
                if (e.NewValue != null)
                {
                    ColorComboBox cmb = (ColorComboBox)d;
                    foreach (var item in BrushPro.instance.KnownColors)
                    {
                        if (item.brush == ((Brush)e.NewValue))
                        {
                            cmb.CmbColor.SelectedItem = item;
                            break;
                        }
                    }
                }
            }));

        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                KnownColor colorObject = e.AddedItems[0] as KnownColor;
                if (colorObject != null)
                {
                    SelectColor = colorObject.brush;
                }
            }
        }
    }
}
