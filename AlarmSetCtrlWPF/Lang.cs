using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmSetCtrl
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager s_AlarmSetCtrlLang =
            new LanguageManager.CLanguageManager(
                "AlarmSetCtrl.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
