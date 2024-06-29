using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using WH.Entity.CommonLib;

namespace WH.Entity.Converter
{
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 颜色值Brush转color name
    /// </summary>
    public class KnownColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Brush brush = (Brush)value;
                var find = BrushPro.instance.KnownColors.FirstOrDefault(o => o.brush == brush);
                return find?.name;
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
