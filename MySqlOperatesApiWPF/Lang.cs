using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlOperatesApiWPF
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager instance =
            new LanguageManager.CLanguageManager(
                "MySqlOperatesApiWPF.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
