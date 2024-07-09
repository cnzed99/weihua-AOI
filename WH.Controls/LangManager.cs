using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace WH.Controls.Properties
{
    public class LangManager
    {
        public static LanguageManager.CLanguageManager instance = new LanguageManager.CLanguageManager("WH.Controls.Properties.Resources", typeof(LangManager).Assembly);
        public static LanguageManager.CLanguageManager welPage = new LanguageManager.CLanguageManager("WH.Controls.Controls.WelcomePage.WelPageResources", typeof(LangManager).Assembly);
    }
}
