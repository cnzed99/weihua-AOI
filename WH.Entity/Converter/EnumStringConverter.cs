using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WH.Entity.Attribute;

namespace WH.Entity.Converter
{
    /// <summary>
    /// 20240708 TCG
    /// 将枚举集合转换为中英文特性字符 枚举需添加特性EnumStringAttribute(zh,en)
    /// </summary>
    public class EnumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList enums)
            {
                List<string> names = new List<string>();
                foreach (var item in enums)
                {
                    var attr = (EnumStringAttribute)item.GetType().GetField(item.ToString()).GetCustomAttribute(typeof(EnumStringAttribute));
                    if (attr != null)
                    {
                        switch (CultureInfo.CurrentCulture.Name)
                        {
                            case "zh-CN":
                                names.Add( attr.ZhName);
                                break;
                            default:
                                names.Add(attr.EnName);
                                break;
                        }
                    }
                    else
                    {
                        names.Add(item.ToString());
                    }
                }
                return names;
               
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 添加了特性EnumStringAttribute(zh,en)的中英文字符与枚举值相互转换
    /// </summary>
    public class StringAttrEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Enum item)
            {
                return EnumStringAttribute.GetEnumName((Enum)value) ?? value.ToString();
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EnumStringAttribute.GetEnumValue((string)value, targetType) ?? value.ToString();
        }
    }
}
