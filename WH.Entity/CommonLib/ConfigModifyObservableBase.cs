using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WH.Entity.Attribute;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.2 李焕彬
    /// 操作消息
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="message">消息</param>
    public class OperateMessage
    {
        public object obj { get; set; }
        public string message { get; set; }

        public OperateMessage(object obj, string message)
        {
            this.obj = obj;
            this.message = message;
        }
    }

    /// <summary>
    /// 记录参数修改的基类 在属性或集合发生变化时在默认通道发送OperateMessage
    /// 属性需要添加<code>[property: DisplayName("")]</code>
    /// 忽略时添加<code>[property: IgnoreModifyLog]</code>
    /// </summary>
    public abstract partial class ConfigModifyObservableBase : ObservableRecipient
    {
        public ConfigModifyObservableBase() { }

        /// <summary>
        /// 20240712 TCG
        /// 名称
        /// </summary>
        [ObservableProperty]
        string name;

        /// <summary>
        /// 20240706 TCG
        /// 参数修改消息通道
        /// </summary>
        public Token token;

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
            var ignore = (IgnoreModifyLogAttribute)
                this.GetType()
                    .GetProperty(e.PropertyName)
                    .GetCustomAttribute(typeof(IgnoreModifyLogAttribute));
            if (ignore is not null)
                return;
            ///要加特特性，不准跳过
            var attr = (DisplayNameAttribute)
                this.GetType()
                    .GetProperty(e.PropertyName)
                    .GetCustomAttribute(typeof(DisplayNameAttribute));
            var sb = new StringBuilder();
            if (newValue is not null && newValue is INotifyCollectionChanged collect)
            {
                collect.CollectionChanged += (s, ee) =>
                {
                    CollectionChanged(ee, attr?.DisplayName);
                };
            }
            if (oldValue == null || newValue == null || oldValue.ToString() == newValue.ToString())
                return;

            sb.Append(attr?.DisplayName);
            sb.Append(":");
            //枚举类型均需加上特性 EnumString
            if (oldValue is Enum oldEnum)
            {
                sb.Append(EnumStringAttribute.GetEnumName(oldEnum));
            }
            else
            {
                sb.Append(oldValue?.ToString());
            }

            sb.Append("=>");
            if (newValue is Enum newEnum)
            {
                sb.Append(EnumStringAttribute.GetEnumName(newEnum));
            }
            else
            {
                sb.Append(newValue.ToString());
            }

            Messenger.Send(new OperateMessage(this, sb.ToString()), token);
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

            Messenger.Send(new OperateMessage(this, sb.ToString()), token);
        }

        public static void UpdateToken(object instance, Token token)
        {
            if (instance is ConfigModifyObservableBase config)
            {
                config.token = token;
                var properties = instance.GetType().GetRuntimeFields();
                foreach (var property in properties)
                {
                    var value = property.GetValue(instance);
                    if (value is null)
                        continue;

                    UpdateToken(value, token);
                }
            }
            if (
                instance.GetType().IsGenericType
                && instance.GetType().GetGenericTypeDefinition() == typeof(ObservableCollection<>)
            )
            {
                Type genericType = instance.GetType().GetGenericArguments()[0];
                PropertyInfo itemsProperty = instance.GetType().GetRuntimeProperties().ToList()[1];

                if (itemsProperty != null)
                {
                    IEnumerable items = itemsProperty.GetValue(instance) as IEnumerable;
                    if (items != null)
                    {
                        foreach (object item in items)
                        {
                            UpdateToken(item, token);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 20240706 TCG
    /// 消息通道类型
    ///
    /// </summary>
    public class Token : IEquatable<Token>
    {
        public string ProGuid { get; set; }

        public string SubChannel { get; set; }

        public Token(string projGuid, string subChannel)
        {
            this.ProGuid = projGuid;
            this.SubChannel = subChannel;
        }

        public bool Equals(Token other)
        {
            if (ProGuid == other.ProGuid && SubChannel == other.SubChannel)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (ProGuid + SubChannel).GetHashCode();
        }
    }
}
