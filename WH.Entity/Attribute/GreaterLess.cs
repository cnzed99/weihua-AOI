using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Attribute
{
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

            if (((IComparable)value).CompareTo(otherValue) > 0)
            {
                return ValidationResult.Success;
            }

            return new("The current value is smaller than the other one");
        }
    }
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

            if (((IComparable)value).CompareTo(otherValue) < 0)
            {
                return ValidationResult.Success;
            }

            return new("The current value is smaller than the other one");
        }
    }
}
