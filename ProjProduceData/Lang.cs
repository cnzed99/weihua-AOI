using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjProduceData
{
    public class Lang
    {
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("ProjProduceData.Properties.Resources", typeof(Lang).Assembly);
    }
}
