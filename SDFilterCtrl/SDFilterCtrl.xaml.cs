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
    /// SDFilterCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class SDFilterCtrl : UserControl
    {
        public SDFilterCtrl()
        {
            InitializeComponent();
        }
    }

    public class Boolean2BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "Background")
            {
                return (value != null && (bool)value) ? ResourceHelper.GetResource<Brush>(ResourceToken.InfoBrush) : new SolidColorBrush(Color.FromArgb(255, 179, 9, 12));
            }
            else if ((string)parameter == "Background2")
            {
                return (value != null && (bool)value) ? Brushes.Transparent : new SolidColorBrush(Color.FromArgb(255, 179, 9, 12));
            }
            else if ((string)parameter == "Background3")
            {
                return (value != null && (bool)value) ? new SolidColorBrush(Color.FromArgb(255, 50, 75, 100)) : new SolidColorBrush(Color.FromArgb(255, 179, 9, 12));
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class OneSelectParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 5 && values.All(obj => obj != null)
                && values[0] is DetectFeature && values[1] is double && values[2] is double && values[3] is bool && values[4] is bool)
            {
                string Character = values[0].ToString();
                double Min = (double)values[1];
                double Max = (double)values[2];
                bool MaxLimit = (bool)values[3];
                bool MinLimit = (bool)values[4];
                if (MaxLimit && MinLimit)
                {
                    return $"{Min}≤{Character}≤{Max}";
                }
                else if (MinLimit)//限制最小
                {
                    return $"{Min}≤{Character}≤{double.PositiveInfinity}";
                }
                else if (MaxLimit)//限制最大
                {
                    return $"{double.NegativeInfinity}≤{Character}≤{Max}";
                }
                else//都不限制
                {
                    return $"{double.NegativeInfinity}≤ {Character} ≤{double.PositiveInfinity}";
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColoectionIndexConverter : IMultiValueConverter
    {
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
