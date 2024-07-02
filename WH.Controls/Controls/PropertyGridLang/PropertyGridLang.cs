using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// 属性编辑器扩展，可适应中英文，绑定Lang属性
    /// 属性特性用Resource键值
    /// </summary>
    public class PropertyGridLang : PropertyGrid
    {
        static PropertyGridLang()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridLang), new FrameworkPropertyMetadata(typeof(PropertyGridLang)));
        }

        public LanguageManager.LanguageManager Lang
        {
            get { return (LanguageManager.LanguageManager)GetValue(LangProperty); }
            set { SetValue(LangProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lang.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LangProperty =
            DependencyProperty.Register("Lang", typeof(LanguageManager.LanguageManager), typeof(PropertyGridLang), new PropertyMetadata(default(LanguageManager.LanguageManager), (d,e) =>
            {
                PropertyGridLang PropertyGridLang = (PropertyGridLang)d;
                if (PropertyGridLang.Lang != null)
                {
                    PropertyGridLang.Lang.PropertyChanged += (o, k) =>
                    {
                        PropertyGridLang.OnSelectedObjectChanged(PropertyGridLang.SelectedObject, PropertyGridLang.SelectedObject);
                    };
                    if (PropertyGridLang.SelectedObject != null)
                    {
                        PropertyGridLang.OnSelectedObjectChanged(PropertyGridLang.SelectedObject, PropertyGridLang.SelectedObject);
                    }
                }
            }));

        protected override PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor)
        {
            PropertyItem propertyItem = new PropertyItem();
            if (Lang != null)
            {
                propertyItem.Category = Lang[PropertyResolver.ResolveCategory(propertyDescriptor)];
                if (string.IsNullOrEmpty(propertyItem.Category))
                {
                    propertyItem.Category = PropertyResolver.ResolveCategory(propertyDescriptor);
                }
                propertyItem.DisplayName = Lang[PropertyResolver.ResolveDisplayName(propertyDescriptor)];
                if (string.IsNullOrEmpty(propertyItem.DisplayName))
                {
                    propertyItem.DisplayName = PropertyResolver.ResolveDisplayName(propertyDescriptor);
                }
                propertyItem.Description = Lang[PropertyResolver.ResolveDescription(propertyDescriptor)];
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
            propertyItem.Editor = PropertyResolver.ResolveEditor(propertyDescriptor);
            propertyItem.Value = SelectedObject;
            propertyItem.PropertyName = propertyDescriptor.Name;
            propertyItem.PropertyType = propertyDescriptor.PropertyType;
            propertyItem.PropertyTypeName = $"{propertyDescriptor.PropertyType.Namespace}.{propertyDescriptor.PropertyType.Name}";

            return propertyItem;
        }
    }
}
