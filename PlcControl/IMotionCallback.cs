using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcControl
{
    public interface IMotionCallback
    {
        void WriteSingleRegisterInt32(ushort addr, int value);
    }
}
