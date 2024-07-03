using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace WH.Entity.CommonLib
{
    public record OperateMessage(object obj, string message);
    /// <summary>
    /// 记录参数修改 在属性或集合发生变化时在默认通道发送OperateMessage
    /// </summary>
    public abstract class ObservableLog : ObservableObject
    {
        private object oldValue = "";
        /// <summary>
        /// 属性更改时发生，如果是集合，集合成员更改绑定到CollectionChanged
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            var newValue = this.GetType().GetProperty(e.PropertyName).GetValue(this);
            var ignore = (JsonIgnoreAttribute)this.GetType().GetProperty(e.PropertyName).GetCustomAttribute(typeof(JsonIgnoreAttribute));
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

        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            base.OnPropertyChanging(e);
            oldValue = this.GetType().GetProperty(e.PropertyName).GetValue(this);
        }

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
