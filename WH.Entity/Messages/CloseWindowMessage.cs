using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Messages
{
    /// <summary>
    /// 20240704 TCG
    /// VM通知View 关闭窗体消息
    /// </summary>
    public class CloseWindowMessage
    {
        public WeakReference Sender { get; set; }
    }
}
