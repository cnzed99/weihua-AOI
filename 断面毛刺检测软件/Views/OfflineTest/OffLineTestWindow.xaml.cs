using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WH.DetectSystem.ViewModels;


namespace 断面毛刺检测软件.Views
{
    /// <summary>
    /// OffLineTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OffLineTestWindow : HandyControl.Controls.Window
    {
        public OffLineTestWindow()
        {
            InitializeComponent();
            this.ContainerPanel.Children.Add(new OffLineTestCtrl(App.Container.Resolve<CMainVM>()));
        }
    }
}
