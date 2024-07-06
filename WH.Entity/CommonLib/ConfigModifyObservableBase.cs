using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using WH.Entity.Attribute;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.2 李焕彬
    /// 操作消息
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="message">消息</param>
    public record OperateMessage(object obj, string message);

    /// <summary>
    /// 2024.7.2 李焕彬
    /// 记录参数修改 在属性或集合发生变化时在默认通道发送OperateMessage
    /// </summary>
    public abstract class ConfigModifyObservableBase : ObservableObject
    {
        /// <summary>
        /// 2024.7.2 李焕彬
        /// 属性旧值
        /// </summary>
        private object oldValue = "";

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 属性更改时发生，如果是集合，集合成员更改绑定到CollectionChanged
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            var newValue = this.GetType().GetProperty(e.PropertyName).GetValue(this);
            var ignore = (IgnoreModifyLogAttribute)this.GetType().GetProperty(e.PropertyName).GetCustomAttribute(typeof(IgnoreModifyLogAttribute));
            if (ignore is not null) return;
            ///要加特特性，不准跳过
            var attr = (DisplayNameAttribute)this.GetType().GetProperty(e.PropertyName).GetCustomAttribute(typeof(DisplayNameAttribute));
            var sb = new StringBuilder();
            if (newValue is not null && newValue is INotifyCollectionChanged collect)
            {
                collect.CollectionChanged += (s, ee) => { CollectionChanged(ee, attr.DisplayName); };
            }
            if (oldValue == null || newValue == null||oldValue.ToString() == newValue.ToString()) return;
           
            sb.Append(attr.DisplayName);
            sb.Append(":");
            sb.Append(oldValue?.ToString());
            sb.Append("=>");
            sb.Append(newValue.ToString());
            
            WeakReferenceMessenger.Default.Send(new OperateMessage(this, sb.ToString()), this.GetType().Namespace);
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 重载属性改变
        /// </summary>
        /// <param name="e">属性名</param>
        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            base.OnPropertyChanging(e);
            oldValue = this.GetType().GetProperty(e.PropertyName).GetValue(this);
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 集合改变事件
        /// </summary>
        /// <param name="e">事件</param>
        /// <param name="PropertyName">属性名</param>
        protected void CollectionChanged(NotifyCollectionChangedEventArgs e, string PropertyName)
        {
            var sb = new StringBuilder();
            sb.Append(PropertyName);
            sb.Append(":");
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                sb.Append("增加=>");
                sb.Append(e.NewItems[0].ToString());
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                sb.Append("删除=>");
                sb.Append(e.OldItems[0].ToString());
            }

            WeakReferenceMessenger.Default.Send(new OperateMessage(this, sb.ToString()), this.GetType().Namespace);
        }
    }
}
