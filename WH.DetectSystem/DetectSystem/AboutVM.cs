using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WH.DetectSystem.ViewModels
{
    public partial class AboutVM : ObservableObject
    {
        [ObservableProperty]
        string title = "关于 断面毛刺检测软件";

        [ObservableProperty]
        string subTitle = "断面毛刺检测软件";

        [ObservableProperty]
        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        [ObservableProperty]
        string right = "保留所有权利。";

        [ObservableProperty]
        string warning =
            "本计算机程序受到著作权法和国际公约的保护。未经授权擅自复制或传播本程序的部分或全部," + "将受到严厉的民事和刑事制裁，并将在法律许可的最大限度内受到起诉。";
    }
}
