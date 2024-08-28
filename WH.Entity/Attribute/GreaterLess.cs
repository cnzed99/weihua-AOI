using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Attribute
{

    /// <summary>
    /// 20240708 TCG
    /// 与另一属性比较大小，需大于
    /// 特性无法阻止参数的修改，仅起到提示作用
    /// </summary>
    public sealed class GreaterThanAttribute : ValidationAttribute
    {
        public GreaterThanAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object
                instance = validationContext.ObjectInstance,//获取当前实例
                                                            //获取实例属性 B
                otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

            TimeSpan thisvalue = ((DateTime)value).TimeOfDay;
            TimeSpan otherTime = ((DateTime)otherValue).TimeOfDay;

            if (((IComparable)thisvalue).CompareTo(otherTime) > 0)
            {
                return ValidationResult.Success;
            }

            return new("当前值应大于另一个值");
        }
    }
    /// <summary>
    /// 20240708 TCG
    /// 与另一属性比较大小，需小于
    /// 特性无法阻止参数的修改，仅起到提示作用
    /// </summary>
    public sealed class LessThanAttribute : ValidationAttribute
    {
        public LessThanAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object
                instance = validationContext.ObjectInstance,//获取当前实例
                                                            //获取实例属性 B
                otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

            TimeSpan thisvalue = ((DateTime)value).TimeOfDay;
            TimeSpan otherTime = ((DateTime)otherValue).TimeOfDay;

            if (((IComparable)thisvalue).CompareTo(otherTime) < 0)
            {
                return ValidationResult.Success;
            }

            return new("当前值应小于另一个值");
        }
    }


    /// <summary>
    /// 20240824 鲍赞宝
    /// 时间比较专用 只比较时间部分,忽略日期
    /// </summary>
    public sealed class DateTimeGreaterThanAttribute : ValidationAttribute
    {
        public DateTimeGreaterThanAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object
                instance = validationContext.ObjectInstance,//获取当前实例
                                                            //获取实例属性 B
                otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

            if (value is DateTime thisDateTime && otherValue is DateTime otherDateTime)
            {
                TimeSpan thisvalue = thisDateTime.TimeOfDay;
                TimeSpan otherTime = otherDateTime.TimeOfDay;

                if (((IComparable)thisvalue).CompareTo(otherTime) > 0)
                {
                    return ValidationResult.Success;
                }
            }
            return new("当前值应大于另一个值");
        }
    }
    /// <summary>
    /// 20240824 鲍赞宝
    /// 时间比较专用 只比较时间部分,忽略日期
    /// </summary>
    public sealed class DateTimeLessThanAttribute : ValidationAttribute
    {
        public DateTimeLessThanAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object
                instance = validationContext.ObjectInstance,//获取当前实例
                                                            //获取实例属性 B
                otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

            if (value is DateTime thisDateTime && otherValue is DateTime otherDateTime)
            {
                TimeSpan thisvalue = thisDateTime.TimeOfDay;
                TimeSpan otherTime = otherDateTime.TimeOfDay;

                if (((IComparable)thisvalue).CompareTo(otherTime) < 0)
                {
                    return ValidationResult.Success;
                }
            }
            return new("当前值应小于另一个值");
        }
    }
}
