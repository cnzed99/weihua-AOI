using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDll
{
    public class Lang
    {
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("AlgorithmDll.Properties.Resources", typeof(Lang).Assembly);
    }
}
