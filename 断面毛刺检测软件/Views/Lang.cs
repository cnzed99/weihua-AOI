using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件.Views
{
    public class Lang
    {
        public static LanguageManager.LanguageManager systemSettings = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.SystemSetting.SystemSettingResources", typeof(Lang).Assembly);
        public static LanguageManager.LanguageManager newProj = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.NewProj.NewProjResources", typeof(Lang).Assembly);
    }
}
