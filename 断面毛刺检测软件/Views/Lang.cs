using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件.Views
{
    public class CLang
    {
        public static LanguageManager.CLanguageManager SystemSettings =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.SystemSetting.SystemSettingResources",
                typeof(CLang).Assembly
            );
        public static LanguageManager.CLanguageManager NewProj =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.NewProj.NewProjResources",
                typeof(CLang).Assembly
            );
        public static LanguageManager.CLanguageManager ModifyProj =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.ModifyProj.ModifyProjResources",
                typeof(CLang).Assembly
            );
        public static LanguageManager.CLanguageManager OffLineTest =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.OfflineTest.OfflineTestResources",
                typeof(CLang).Assembly
            );
        public static LanguageManager.CLanguageManager TimeTriggerTest =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.TimeTriggerTest.TimeTriggerTestResources",
                typeof(CLang).Assembly
            );
        public static LanguageManager.CLanguageManager NewProcess =
            new LanguageManager.CLanguageManager(
                "断面毛刺检测软件.Views.NewProcess.NewProcessResources",
                typeof(CLang).Assembly
            );
    }
}
