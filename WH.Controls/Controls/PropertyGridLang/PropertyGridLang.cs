using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.2 李焕彬
    /// 属性编辑器扩展，可适应中英文，绑定lang属性
    /// 属性特性用Resource键值
    /// </summary>
    public class PropertyGridLang : PropertyGrid
    {
        static PropertyGridLang()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridLang), new FrameworkPropertyMetadata(typeof(PropertyGridLang)));
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 语言
        /// </summary>
        public LanguageManager.LanguageManager lang
        {
            get { return (LanguageManager.LanguageManager)GetValue(langProperty); }
            set { SetValue(langProperty, value); }
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 语言属性
        /// </summary>
        // Using a DependencyProperty as the backing store for lang.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty langProperty =
            DependencyProperty.Register("lang", typeof(LanguageManager.LanguageManager), typeof(PropertyGridLang), new PropertyMetadata(default(LanguageManager.LanguageManager), (d,e) =>
            {
                PropertyGridLang PropertyGridLang = (PropertyGridLang)d;
                if (PropertyGridLang.lang != null)
                {
                    PropertyGridLang.lang.PropertyChanged += (o, k) =>
                    {
                        PropertyGridLang.OnSelectedObjectChanged(PropertyGridLang.SelectedObject, PropertyGridLang.SelectedObject);
                    };
                    if (PropertyGridLang.SelectedObject != null)
                    {
                        PropertyGridLang.OnSelectedObjectChanged(PropertyGridLang.SelectedObject, PropertyGridLang.SelectedObject);
                    }
                }
            }));

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 属性创建重载
        /// </summary>
        /// <param name="propertyDescriptor"></param>
        /// <returns></returns>
        protected override PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor)
        {
            PropertyItem propertyItem = new PropertyItem();
            if (lang != null)
            {
                propertyItem.Category = lang[PropertyResolver.ResolveCategory(propertyDescriptor)];
                if (string.IsNullOrEmpty(propertyItem.Category))
                {
                    propertyItem.Category = PropertyResolver.ResolveCategory(propertyDescriptor);
                }
                propertyItem.DisplayName = lang[PropertyResolver.ResolveDisplayName(propertyDescriptor)];
                if (string.IsNullOrEmpty(propertyItem.DisplayName))
                {
                    propertyItem.DisplayName = PropertyResolver.ResolveDisplayName(propertyDescriptor);
                }
                propertyItem.Description = lang[PropertyResolver.ResolveDescription(propertyDescriptor)];
                if (string.IsNullOrEmpty(propertyItem.Description))
                {
                    propertyItem.Description = PropertyResolver.ResolveDescription(propertyDescriptor);
                }
            }
            else
            {
                propertyItem.Category = PropertyResolver.ResolveCategory(propertyDescriptor);
                propertyItem.DisplayName = PropertyResolver.ResolveDisplayName(propertyDescriptor);
                propertyItem.Description = PropertyResolver.ResolveDescription(propertyDescriptor);
            }
            propertyItem.IsReadOnly = PropertyResolver.ResolveIsReadOnly(propertyDescriptor);
            propertyItem.DefaultValue = PropertyResolver.ResolveDefaultValue(propertyDescriptor);
            List<Type> types = new List<Type>() { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int),
            typeof(uint),typeof(long),typeof(ulong),typeof(float),typeof(double)};
            if(types.Contains(propertyDescriptor.PropertyType))
            {
                propertyItem.Editor = new PlainTextPropertyEditor();
            }
            else
            {
                propertyItem.Editor = PropertyResolver.ResolveEditor(propertyDescriptor);
            }
            propertyItem.Value = SelectedObject;
            propertyItem.PropertyName = propertyDescriptor.Name;
            propertyItem.PropertyType = propertyDescriptor.PropertyType;
            propertyItem.PropertyTypeName = $"{propertyDescriptor.PropertyType.Namespace}.{propertyDescriptor.PropertyType.Name}";
            return propertyItem;
        }
    }
}
