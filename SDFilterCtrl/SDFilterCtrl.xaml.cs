using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// 2024.6.25 李焕彬
    /// 检测设置控件
    /// SDFilterCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class SDFilterCtrl : UserControl
    {
        public SDFilterCtrl()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 过滤分选条件&&||转换器
    /// </summary>
    public class ColoectionIndexConverter : IMultiValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 判断是否要加&& ||
        /// </summary>
        /// <param name="values">目标条件、目标条件所属容器</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>&&/||</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values.All(obj => obj != null) && values[1] is IList items)
            {
                return (string)parameter == "and" ? (items.IndexOf(values[0]) == 0 ? "" : " && ") : ((items.IndexOf(values[0]) == 0 || (int)values[2] == 0) ? "" : " || ");
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
