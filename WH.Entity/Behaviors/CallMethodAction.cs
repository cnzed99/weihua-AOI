using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Interactivity;

namespace WH.Entity.Behaviors
{
    /// <summary>
    /// 20240826 TCG
    /// 通过行为调用方法而不是命令Command
    /// <hc:Interaction.Triggers>
    ///     <hc:EventTrigger>
    ///         <wh:CallMethodAction MethodTarget = "{Binding .}" MethodName="method" MethodParameter="{Binding param}"/>
    ///     </hc:EventTrigger>
    /// </hc:Interaction.Triggers>
    /// </summary>
    public class CallMethodAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty MethodTargetProperty =
            DependencyProperty.Register(
                "MethodTarget",
                typeof(object),
                typeof(CallMethodAction),
                new PropertyMetadata(null)
            );

        // Using a DependencyProperty as the backing store for MethodName.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register(
            "MethodName",
            typeof(string),
            typeof(CallMethodAction),
            new PropertyMetadata(null)
        );

        // Using a DependencyProperty as the backing store for MethodParameter.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty MethodParameterProperty =
            DependencyProperty.Register(
                "MethodParameter",
                typeof(object),
                typeof(CallMethodAction),
                new PropertyMetadata(null, OnMethodParameterChanged)
            );

        private readonly MethodBinderWithArgument _callbackMethod = new MethodBinderWithArgument();
        private readonly MethodBinder _method = new MethodBinder();

        private bool _parameterSet;

        public object MethodTarget
        {
            get { return GetValue(MethodTargetProperty); }
            set { SetValue(MethodTargetProperty, value); }
        }

        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        public object MethodParameter
        {
            get { return GetValue(MethodParameterProperty); }
            set { SetValue(MethodParameterProperty, value); }
        }

        private static void OnMethodParameterChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e
        )
        {
            var action =
                sender as CallMethodAction
                ?? throw new ArgumentException(
                    $"Value must be a {nameof(CallMethodAction)}.",
                    nameof(sender)
                );

            action._parameterSet = true;
        }

        protected override void Invoke(object parameter)
        {
            if (MethodTarget == null)
                return;
            if (MethodName == null)
                return;

            if (!_parameterSet)
                _method.Invoke(MethodTarget, MethodName);
            else
                _callbackMethod.Invoke(MethodTarget, MethodName, MethodParameter);
        }
    }
}
