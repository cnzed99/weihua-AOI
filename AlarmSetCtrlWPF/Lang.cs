using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmSetCtrlWPF
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager s_AlarmSetCtrlLang =
            new LanguageManager.CLanguageManager(
                "AlarmSetCtrlWPF.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
