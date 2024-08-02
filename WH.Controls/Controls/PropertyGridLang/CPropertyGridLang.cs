using System;
using System.Collections;
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
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using HandyControl.Tools.Extension;
using WH.Controls.Controls.PropertyGridLang;
using WH.Entity.Attribute;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.2 李焕彬
    /// 属性编辑器扩展，可适应中英文，绑定lang属性
    /// 属性特性用Resource键值
    /// </summary>
    [TemplatePart(Name = ElementItemsControl, Type = typeof(ItemsControl))]
    [TemplatePart(Name = ElementSearchBar, Type = typeof(SearchBar))]
    public class CPropertyGridLang : PropertyGrid
    {
        static CPropertyGridLang()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CPropertyGridLang),
                new FrameworkPropertyMetadata(typeof(CPropertyGridLang))
            );
        }

        public CPropertyGridLang()
            : base()
        {
            ShowSortButton = false;
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// ElementItemsControl名
        /// </summary>
        private const string ElementItemsControl = "PART_ItemsControl";

        /// <summary>
        /// 2024.7.16 李焕彬
        /// ElementSearchBar名
        /// </summary>
        private const string ElementSearchBar = "PART_SearchBar";

        /// <summary>
        /// 2024.7.16 李焕彬
        /// _itemsControl
        /// </summary>
        private ItemsControl _itemsControl;

        /// <summary>
        /// 2024.7.16 李焕彬
        ///  _dataView
        /// </summary>
        private ICollectionView _dataView;

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 查找按钮
        /// </summary>
        private SearchBar _searchBar;

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 查找Key
        /// </summary>
        private string _searchKey;

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 显示对象改变时发生
        /// </summary>
        /// <param name="d">依赖属性</param>
        /// <param name="e">事件参数</param>
        private static void OnSelectedObjectChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            var ctl = (CPropertyGridLang)d;
            ctl.OnSelectedObjectChanged(e.OldValue, e.NewValue);
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 显示对象改变时发生
        /// </summary>
        /// <param name="oldValue">旧对象</param>
        /// <param name="newValue">新对象</param>
        protected override void OnSelectedObjectChanged(object oldValue, object newValue)
        {
            UpdateItems(newValue);
            RaiseEvent(
                new RoutedPropertyChangedEventArgs<object>(
                    oldValue,
                    newValue,
                    SelectedObjectChangedEvent
                )
            );
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 应用模板
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_searchBar != null)
            {
                _searchBar.SearchStarted -= SearchBar_SearchStarted;
            }

            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild(ElementItemsControl) as ItemsControl;
            _searchBar = GetTemplateChild(ElementSearchBar) as SearchBar;

            if (_searchBar != null)
            {
                _searchBar.SearchStarted += SearchBar_SearchStarted;
            }

            UpdateItems(SelectedObject);
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// Browsable特性默认为true,需要增加有无其他如DisplayName等特性判断
        /// </summary>
        /// <param name="propertyDescriptor"></param>
        /// <returns>true显示，false不显示</returns>
        private bool IsBrowsable(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.IsBrowsable)
            {
                bool hasDisplayNameAttribute = false;
                bool hasCategoryAttribute = false;
                foreach (var item in propertyDescriptor.Attributes)
                {
                    if (item is DisplayNameAttribute)
                    {
                        hasDisplayNameAttribute = true;
                    }
                    if (item is CategoryAttribute)
                    {
                        hasCategoryAttribute = true;
                    }
                    if (hasDisplayNameAttribute && hasCategoryAttribute)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 更新控件
        /// </summary>
        /// <param name="obj">显示对象</param>
        private void UpdateItems(object obj)
        {
            if (obj == null || _itemsControl == null)
                return;

            _dataView = CollectionViewSource.GetDefaultView(
                TypeDescriptor
                    .GetProperties(obj.GetType())
                    .OfType<PropertyDescriptor>()
                    .Where(item => IsBrowsable(item))
                    .Select(CreatePropertyItem)
                    .Do(item => item.InitElement())
            );
            foreach (UIElement item in _dataView)
            {
                item.IsEnabled = IsEditable;
            }

            SortByCategory(null, null);
            _itemsControl.ItemsSource = _dataView;
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 目录排列
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">路径参数</param>
        private void SortByCategory(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null)
                return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();
                _dataView.SortDescriptions.Add(
                    new SortDescription(
                        PropertyItem.CategoryProperty.Name,
                        ListSortDirection.Ascending
                    )
                );
                _dataView.SortDescriptions.Add(
                    new SortDescription(
                        PropertyItem.DisplayNameProperty.Name,
                        ListSortDirection.Ascending
                    )
                );
                _dataView.GroupDescriptions.Add(
                    new PropertyGroupDescription(PropertyItem.CategoryProperty.Name)
                );
            }
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 名称排列
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">路径参数</param>
        private void SortByName(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null)
                return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();
                _dataView.SortDescriptions.Add(
                    new SortDescription(
                        PropertyItem.PropertyNameProperty.Name,
                        ListSortDirection.Ascending
                    )
                );
            }
        }

        /// <summary>
        /// 2024.7.16 李焕彬
        /// 查找
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">路径参数</param>
        private void SearchBar_SearchStarted(object sender, FunctionEventArgs<string> e)
        {
            if (_dataView == null)
                return;

            _searchKey = e.Info;
            if (string.IsNullOrEmpty(_searchKey))
            {
                foreach (UIElement item in _dataView)
                {
                    item.Show();
                }
            }
            else
            {
                foreach (PropertyItem item in _dataView)
                {
                    item.Show(
                        item.PropertyName.ToLower().Contains(_searchKey)
                            || item.DisplayName.ToLower().Contains(_searchKey)
                    );
                }
            }
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 语言
        /// </summary>
        public LanguageManager.CLanguageManager lang
        {
            get { return (LanguageManager.CLanguageManager)GetValue(langProperty); }
            set { SetValue(langProperty, value); }
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 语言属性
        /// </summary>
        // Using a DependencyProperty as the backing store for lang.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty langProperty = DependencyProperty.Register(
            "lang",
            typeof(LanguageManager.CLanguageManager),
            typeof(CPropertyGridLang),
            new PropertyMetadata(
                default(LanguageManager.CLanguageManager),
                (d, e) =>
                {
                    CPropertyGridLang propertyGrid = (CPropertyGridLang)d;
                    if (propertyGrid.lang != null)
                    {
                        propertyGrid.lang.PropertyChanged += (o, k) =>
                        {
                            propertyGrid.OnSelectedObjectChanged(
                                propertyGrid.SelectedObject,
                                propertyGrid.SelectedObject
                            );
                        };
                        if (propertyGrid.SelectedObject != null)
                        {
                            propertyGrid.OnSelectedObjectChanged(
                                propertyGrid.SelectedObject,
                                propertyGrid.SelectedObject
                            );
                        }
                    }
                }
            )
        );

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
            "IsEditable",
            typeof(bool),
            typeof(CPropertyGridLang),
            new PropertyMetadata(
                true,
                (d, e) =>
                {
                    CPropertyGridLang propertyGrid = (CPropertyGridLang)d;
                    if (propertyGrid._dataView != null)
                    {
                        foreach (UIElement item in propertyGrid._dataView)
                        {
                            item.IsEnabled = propertyGrid.IsEditable;
                        }
                    }
                }
            )
        );

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 属性创建重载
        /// </summary>
        /// <param name="propertyDescriptor"></param>
        /// <returns></returns>
        protected override PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor)
        {
            try
            {
                PropertyItem propertyItem = new PropertyItem();
                if (lang != null)
                {
                    propertyItem.Category = lang[
                        PropertyResolver.ResolveCategory(propertyDescriptor)
                    ];
                    if (string.IsNullOrEmpty(propertyItem.Category))
                    {
                        propertyItem.Category = PropertyResolver.ResolveCategory(
                            propertyDescriptor
                        );
                    }
                    propertyItem.DisplayName = lang[
                        PropertyResolver.ResolveDisplayName(propertyDescriptor)
                    ];
                    if (string.IsNullOrEmpty(propertyItem.DisplayName))
                    {
                        propertyItem.DisplayName = PropertyResolver.ResolveDisplayName(
                            propertyDescriptor
                        );
                    }
                    propertyItem.Description = lang[
                        PropertyResolver.ResolveDescription(propertyDescriptor)
                    ];
                    if (string.IsNullOrEmpty(propertyItem.Description))
                    {
                        propertyItem.Description = PropertyResolver.ResolveDescription(
                            propertyDescriptor
                        );
                    }
                }
                else
                {
                    propertyItem.Category = PropertyResolver.ResolveCategory(propertyDescriptor);
                    propertyItem.DisplayName = PropertyResolver.ResolveDisplayName(
                        propertyDescriptor
                    );
                    propertyItem.Description = PropertyResolver.ResolveDescription(
                        propertyDescriptor
                    );
                }
                propertyItem.IsReadOnly = PropertyResolver.ResolveIsReadOnly(propertyDescriptor);
                propertyItem.DefaultValue = PropertyResolver.ResolveDefaultValue(
                    propertyDescriptor
                );

                List<Type> types = new List<Type>()
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double)
                };
                bool hasEditor = false;
                foreach (var item in propertyDescriptor.Attributes)
                {
                    if (item is EditorAttribute)
                    {
                        hasEditor = true;
                        break;
                    }
                }
                if (hasEditor)
                {
                    propertyItem.Editor = PropertyResolver.ResolveEditor(propertyDescriptor);
                }
                else if (types.Contains(propertyDescriptor.PropertyType))
                {
                    propertyItem.Editor = new CNumberPropertyEditor();
                }
                else if (typeof(IList).IsAssignableFrom(propertyDescriptor.PropertyType))
                {
                    propertyItem.Editor = new CCollectionPropertyEditor();
                }
                else
                {
                    propertyItem.Editor = PropertyResolver.ResolveEditor(propertyDescriptor);
                }
                propertyItem.Value = SelectedObject;
                propertyItem.PropertyName = propertyDescriptor.Name;
                propertyItem.PropertyType = propertyDescriptor.PropertyType;
                propertyItem.PropertyTypeName =
                    $"{propertyDescriptor.PropertyType.Namespace}.{propertyDescriptor.PropertyType.Name}";
                return propertyItem;
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                throw;
            }
        }
    }
}
