using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WH.Entity.Converter
{
    /// <summary>
    /// 字节到Gb转换
    /// </summary>
    public class Byte2GbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null)
            {
                long space = (long)value / (1024 * 1024 * 1024);
                return space;
            }
            return 0;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is not null)
            {
                long space = (long)value * (1024 * 1024 * 1024);
                return space;
            }
            return 0;
        }
    }
}
