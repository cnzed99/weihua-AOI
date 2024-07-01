using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Specialized;
using System.ComponentModel;
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            var newValue = this.GetType().GetProperty(e.PropertyName).GetValue(this);
            if (oldValue == null || newValue == null) return;
            var sb = new StringBuilder();
            sb.Append(e.PropertyName);
            sb.Append(":");
            sb.Append(oldValue?.ToString());
            sb.Append("=>");
            sb.Append(newValue.ToString());

            WeakReferenceMessenger.Default.Send(new OperateMessage(this, sb.ToString()));
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

            WeakReferenceMessenger.Default.Send(new OperateMessage(this, sb.ToString()));
        }
    }
}
