using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager instance =
            new LanguageManager.CLanguageManager(
                "DataQuery.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
