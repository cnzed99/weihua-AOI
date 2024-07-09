using Mapster.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace WH.Entity.Attribute
{
    /// <summary>
    /// 20240708 TCG
    /// 给枚举添加中英文特性
    /// </summary>
    public class EnumStringAttribute : System.Attribute
    {
        public string ZhName;
        public string EnName;
        public EnumStringAttribute(string zh, string en)
        {
            ZhName = zh; EnName = en;
        }
        /// <summary>
        /// 20240708 TCG
        /// 将枚举集合转换为对应的中英文特性值集合
        /// </summary>
        /// <param name="enums"></param>
        /// <returns></returns>
        public static List<string> Enums2Strings(IList enums)
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
                            names.Add(attr.ZhName);
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
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取输入枚举类型对应的中英文特性值集合
        /// </summary>
        /// <param name="value">输入枚举类型</param>
        /// <returns>枚举特性名集合</returns>
        public static List<string> GetEnumNames(Type type)
        {
            List<string> names = new List<string>();
            foreach (var item in Enum.GetValues(type))
            {
                var attr = (EnumStringAttribute)item.GetType().GetField(item.ToString()).GetCustomAttribute(typeof(EnumStringAttribute));
                if (attr != null)
                {
                    switch (CultureInfo.CurrentCulture.Name)
                    {
                        case "zh-CN":
                            names.Add(attr.ZhName);
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

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取枚举值对应的中英文枚举特性值
        /// </summary>
        /// <param name="value">输入枚举值</param>
        /// <returns>枚举名</returns>
        public static string GetEnumName(Enum value)
        {
            var attr = (EnumStringAttribute)value.GetType().GetField(value.ToString()).GetCustomAttribute(typeof(EnumStringAttribute));
            if (attr != null)
            {
                switch (CultureInfo.CurrentCulture.Name)
                {
                    case "zh-CN":
                        return attr.ZhName;
                    default:
                        return attr.EnName;
                }
            }

            return null;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取枚举特性名对应枚举值
        /// </summary>
        /// <param name="value">枚举特性名</param>
        /// <param name="targetType">枚举类型</param>
        /// <returns>枚举值</returns>
        public static object GetEnumValue(string value, Type targetType)
        {
            var fieldInfo = targetType.GetFields().FirstOrDefault(finfo =>
            {
                var atr = ((EnumStringAttribute)finfo.GetCustomAttribute(typeof(EnumStringAttribute)));
                if (atr is null) return false;
                switch (CultureInfo.CurrentCulture.Name)
                {
                    case "zh-CN":
                        if (atr.ZhName == value.ToString())
                        {
                            return true;
                        }  
                        else
                        {
                            return false;
                        }
                    default:
                        if (atr.EnName == value.ToString())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                }
            });
            return fieldInfo.GetValue(null);
        }
    }
}
