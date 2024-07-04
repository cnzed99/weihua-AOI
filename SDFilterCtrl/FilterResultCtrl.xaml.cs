using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace SDFilter
{
    /// <summary>
    /// 2024.6.28 李焕彬
    /// 检测结果控件
    /// FilterResultCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class FilterResultCtrl : UserControl
    {
        public FilterResultCtrl()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 值一半转换器
    /// </summary>
    public class ValueHalfConverter : IValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 值一半转换器
        /// </summary>
        /// <param name="value">目标值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>一半后</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value/2-1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
