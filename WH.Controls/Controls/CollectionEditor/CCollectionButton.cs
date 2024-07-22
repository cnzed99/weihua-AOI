using System;
using System.Collections;
using System.Collections.Generic;
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
    /// 2024.7.19 李焕彬
    /// 属性编辑器集合编辑器按钮
    /// </summary>
    public class CCollectionButton : Button
    {
        static CCollectionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CCollectionButton),
                new FrameworkPropertyMetadata(typeof(CCollectionButton))
            );
        }

        public CCollectionButton()
        {
            this.Content = Properties.Resources.Collection;
            this.Click += (s, e) =>
            {
                CCollectionButton button = s as CCollectionButton;
                CollectionEditor collectionEditor = new CollectionEditor();
                collectionEditor.Collection = (IList)button.Tag;
                collectionEditor.ShowDialog();
            };
        }
    }
}
