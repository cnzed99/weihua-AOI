using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
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
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Properties.Langs;
using LanguageManager;
using Microsoft.VisualBasic;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.19 李焕彬
    /// 集合编辑器
    /// CollectionEditor.xaml 的交互逻辑
    /// </summary>
    public partial class CollectionEditor : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        public CollectionEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 集合
        /// </summary>
        public IList Collection
        {
            get { return (IList)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Collection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register(
            "Collection",
            typeof(IList),
            typeof(CollectionEditor),
            new PropertyMetadata(
                default(IList),
                (d, e) =>
                {
                    if (e.NewValue != null && e.NewValue is IList list)
                    {
                        CollectionEditor editor = (CollectionEditor)d;
                        editor.listBox.ItemsSource = list;
                    }
                }
            )
        );

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private CLanguageManager lang;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 语言
        /// </summary>
        public CLanguageManager Lang
        {
            get { return lang; }
            set
            {
                lang = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lang)));
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Collection != null)
            {
                var type = Collection.GetType().GetGenericArguments()[0];
                Collection.Add(Activator.CreateInstance(type));
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Collection != null && this.listBox.SelectedItem != null)
            {
                Collection.Remove(this.listBox.SelectedItem);
            }
        }
    }
}
