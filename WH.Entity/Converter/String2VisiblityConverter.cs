using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WH.Entity.Converter
{
    /// <summary>
    /// 2024.9.3 李焕彬
    /// string与visiblity转换,value==parameter时返回Visibility.Collapsed
    /// </summary>
    public class String2VisiblityReConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                return value.ToString() == parameter.ToString()
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
