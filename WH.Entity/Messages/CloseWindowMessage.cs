using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace WH.Entity.Messages
{
    /// <summary>
    /// 20240704 TCG
    /// VM通知View 关闭窗体消息
    /// </summary>
    public class CloseWindowMessage
    {
        public WeakReference Sender { get; set; }
        public bool DialogResult { get; set; }
    }

    public class CMessengers : IMessenger
    {
        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered<TMessage, TToken>(object recipient, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void Register<TRecipient, TMessage, TToken>(
            TRecipient recipient,
            TToken token,
            MessageHandler<TRecipient, TMessage> handler
        )
            where TRecipient : class
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public TMessage Send<TMessage, TToken>(TMessage message, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void Unregister<TMessage, TToken>(object recipient, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll(object recipient)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll<TToken>(object recipient, TToken token)
            where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }
    }
}
