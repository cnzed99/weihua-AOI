using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDFilter
{
    public class Lang
    {
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("SDFilter.Properties.Resource1", typeof(Lang).Assembly);
    }
}
