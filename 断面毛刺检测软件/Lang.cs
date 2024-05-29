using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件
{
    public class Lang
    {
        public static LanguageManager.LanguageManager instance = new LanguageManager.LanguageManager("断面毛刺检测软件.Properties.Resources", typeof(Lang).Assembly);
    }
}
