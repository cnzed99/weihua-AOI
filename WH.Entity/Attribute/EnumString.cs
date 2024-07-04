using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.Attribute
{
    public class EnumStringAttribute : System.Attribute
    {
        public string ZhName;
        public string EnName;
        public EnumStringAttribute(string zh, string en)
        {
            ZhName = zh; EnName = en;
        }
    }
}
