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
using System.Windows.Controls.Primitives;
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
    public partial class COpenSetWindowButton : ToggleButton
    {
        public COpenSetWindowButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 打开设置界面
        /// </summary>
        public OpenSetWindowProperty OpenSetWindow
        {
            get { return (OpenSetWindowProperty)GetValue(OpenSetWindowProperty); }
            set { SetValue(OpenSetWindowProperty, value); }
        }

        /// <summary>
        /// 打开设置界面
        /// </summary>
        // Using a DependencyProperty as the backing store for SelectColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenSetWindowProperty =
            DependencyProperty.Register(
                "OpenSetWindow",
                typeof(OpenSetWindowProperty),
                typeof(COpenSetWindowButton),
                new PropertyMetadata(
                    default(CKnownColor),
                    (d, e) =>
                    {
                        if (e.NewValue != null)
                        {
                            ToggleButton cmb = (ToggleButton)d;
                            cmb.DataContext = e.NewValue;
                        }
                    }
                )
            );
    }
}