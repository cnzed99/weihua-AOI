using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WH.Entity
{
    /// <summary>
    /// 20240708 TCG
    /// 用于不在可视化树中的对象的绑定 
    /// 不在可视化树上的对象，无法继承和直接绑定到DataContext
    /// </summary>
    public class CBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new CBindingProxy();
        }
        /// <summary>
        /// 20240708 TCG
        /// 依赖属性
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>
        /// 20240708 TCG
        /// 注入依赖属性
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(CBindingProxy), new PropertyMetadata(null));
    }
}
