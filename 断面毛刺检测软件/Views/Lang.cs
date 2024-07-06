using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件.Views
{
    public class CLang
    {
        public static LanguageManager.LanguageManager SystemSettings = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.SystemSetting.SystemSettingResources", typeof(CLang).Assembly);
        public static LanguageManager.LanguageManager NewProj = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.NewProj.NewProjResources", typeof(CLang).Assembly);
        public static LanguageManager.LanguageManager ModifyProj = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.ModifyProj.ModifyProjResources", typeof(CLang).Assembly);
        public static LanguageManager.LanguageManager OffLineTest = new LanguageManager.LanguageManager("断面毛刺检测软件.Views.OfflineTest.OfflineTestResources", typeof(CLang).Assembly);
    }
}
