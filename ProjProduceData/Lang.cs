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
    public class Lang
    {
        public static LanguageManager.LanguageManager s_Instance = new LanguageManager.LanguageManager("ProjProduceData.Properties.Resources", typeof(Lang).Assembly);
    }
}
