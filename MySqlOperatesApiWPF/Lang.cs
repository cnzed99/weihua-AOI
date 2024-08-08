using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlOperatesApi
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager instance =
            new LanguageManager.CLanguageManager(
                "MySqlOperatesApi.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
