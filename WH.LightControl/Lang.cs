using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.LightControl
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager LSWLightLang =
            new LanguageManager.CLanguageManager(
                "WH.LightControl.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
