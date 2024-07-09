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
    /// 20240708 TCG
    /// radiobutton 转换器，check为true时，返回Parameter
    /// </summary>
    public class CheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }
            string checkvalue = value.ToString();
            string targetvalue = parameter.ToString();
            bool r = checkvalue.Equals(targetvalue);
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }

            if ((bool)value)
            {
                return parameter.ToString();
            }
            return null;
        }
    }
}
