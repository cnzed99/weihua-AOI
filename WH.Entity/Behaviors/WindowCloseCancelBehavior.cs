using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HandyControl.Interactivity;

namespace WH.Entity.Behaviors
{
    /// <summary>
    /// 20240826 TCG
    /// 窗体取消关闭时执行回调
    /// </summary>
    public class WindowCloseCancelBehavior : Behavior<Window>
    {
        /// <summary>
        /// 是否可以关闭
        /// </summary>
        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanClose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
            "CanClose",
            typeof(bool),
            typeof(WindowCloseCancelBehavior),
            new PropertyMetadata(true)
        );

        /// <summary>
        /// 取消关闭时所执行回调
        /// </summary>
        public ICommand CloseCanceledCallbackCommand
        {
            get { return (ICommand)GetValue(CloseCanceledCallbackCommandProperty); }
            set { SetValue(CloseCanceledCallbackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCanceledCallbackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCanceledCallbackCommandProperty =
            DependencyProperty.Register(
                "CloseCanceledCallbackCommand",
                typeof(ICommand),
                typeof(WindowCloseCancelBehavior),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty CloseCanceledCallbackMethodTargetProperty =
            DependencyProperty.Register(
                "CloseCanceledCallbackMethodTarget",
                typeof(object),
                typeof(CallMethodAction),
                new PropertyMetadata(null)
            );

        // Using a DependencyProperty as the backing store for MethodName.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty CloseCanceledCallbackMethodNameProperty =
            DependencyProperty.Register(
                "CloseCanceledCallbackMethodName",
                typeof(string),
                typeof(CallMethodAction),
                new PropertyMetadata(null)
            );
        private readonly MethodBinder _callbackMethod = new MethodBinder();
        public object CloseCanceledCallbackMethodTarget
        {
            get { return GetValue(CloseCanceledCallbackMethodTargetProperty); }
            set { SetValue(CloseCanceledCallbackMethodTargetProperty, value); }
        }

        public string CloseCanceledCallbackMethodName
        {
            get { return (string)GetValue(CloseCanceledCallbackMethodNameProperty); }
            set { SetValue(CloseCanceledCallbackMethodNameProperty, value); }
        }

        protected override void OnAttached()
        {
            var associatedObject = AssociatedObject;
            if (associatedObject == null)
                throw new InvalidOperationException();
            base.OnAttached();
            associatedObject.Closing += (sender, e) =>
            {
                if (e == null)
                    throw new ArgumentNullException(nameof(e));

                if (CanClose)
                    return;
                if (
                    CloseCanceledCallbackCommand != null
                    && CloseCanceledCallbackCommand.CanExecute(null)
                )
                    CloseCanceledCallbackCommand.Execute(null);
                if (
                    CloseCanceledCallbackMethodTarget != null
                    && CloseCanceledCallbackMethodName != null
                )
                    _callbackMethod.Invoke(
                        CloseCanceledCallbackMethodTarget,
                        CloseCanceledCallbackMethodName
                    );
                e.Cancel = true;
            };
        }
    }
}
