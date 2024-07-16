using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveImageManage
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager s_SaveImageLang =
            new LanguageManager.CLanguageManager(
                "SaveImageManageWPF.Properties.SaveimageResources",
                typeof(CLang).Assembly
            );
    }
}
