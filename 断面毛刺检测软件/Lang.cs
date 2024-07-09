using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager Instance = new LanguageManager.CLanguageManager("断面毛刺检测软件.Properties.Resources", typeof(CLang).Assembly);
    }
}
