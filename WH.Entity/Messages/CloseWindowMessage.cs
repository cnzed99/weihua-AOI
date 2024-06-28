using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Messages
{
    public class CloseWindowMessage
    {
        public WeakReference Sender { get; set; }
    }
}
