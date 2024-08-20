using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjProduceData
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 语言
    /// </summary>
    public class CLang
    {
        public static LanguageManager.CLanguageManager s_Instance =
            new LanguageManager.CLanguageManager(
                "ProjProduceData.Properties.Resources",
                typeof(CLang).Assembly
            );
    }
}
