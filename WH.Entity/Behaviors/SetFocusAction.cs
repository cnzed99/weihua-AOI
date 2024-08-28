using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Interactivity;

namespace WH.Entity.Behaviors
{
    /// <summary>
    /// 20240826 TCG
    /// 设置控件焦点
    /// </summary>
    public class SetFocusAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            AssociatedObject?.Focus();
        }
    }
}
