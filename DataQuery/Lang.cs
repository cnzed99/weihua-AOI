using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlOperatesApi
{
    public class CDQLang
    {
        public static LanguageManager.CLanguageManager instance =
            new LanguageManager.CLanguageManager(
                "DataQuery.Properties.Resources",
                typeof(CDQLang).Assembly
            );
    }
}
