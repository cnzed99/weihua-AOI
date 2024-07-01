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
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("WH.Controls.Properties.Resources", typeof(LangManager).Assembly);
        public static LanguageManager.LanguageManager welPage = new LanguageManager.LanguageManager("WH.Controls.Controls.WelcomePage.WelPageResources", typeof(LangManager).Assembly);
    }
}
