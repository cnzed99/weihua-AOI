using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcControl
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 语言
    /// </summary>
    public class CLang
    {
        public static LanguageManager.CLanguageManager s_Instance =
            new LanguageManager.CLanguageManager(
                "PlcControl.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
