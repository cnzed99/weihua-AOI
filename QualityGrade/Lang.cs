using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityGrade
{
    public class Lang
    {
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("QualityGrade.Properties.Resource1", typeof(Lang).Assembly);
    }
}
