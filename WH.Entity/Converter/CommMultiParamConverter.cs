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
    /// 2024.7.2 李焕彬
    /// Command多参数时转换器
    /// </summary>
    public class CommMultiParamConverter : IMultiValueConverter
    {
        /// <summary>
        /// 2024.7.2 李焕彬
        /// </summary>
        /// <param name="values">多参数数组</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
