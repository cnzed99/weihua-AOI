using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryPlayback
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager instance =
            new LanguageManager.CLanguageManager(
                "HistoryPlayback.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
