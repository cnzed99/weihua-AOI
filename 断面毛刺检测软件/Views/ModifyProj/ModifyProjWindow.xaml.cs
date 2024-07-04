using Autofac;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
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
using WH.Entity.Messages;
using 断面毛刺检测软件.Views.ModifyProj;

namespace 断面毛刺检测软件.Views
{
    /// <summary>
    /// NewProj.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyProjWindow : HandyControl.Controls.Window
    {
       
        public ModifyProjWindow()
        {
            InitializeComponent();
            this.DataContext = new CModifyProjVM( App.Container.Resolve<CMainVM>());
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (_, m) => { if (m.Sender?.Target == this.DataContext) Close(); });
        }

        private void SelectPath_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = CMainVM.projFilter;
            saveFileDialog.Title = ModifyProjResources.Title;
            if (saveFileDialog.ShowDialog() is true)
            {
                this.tb_path.Text = saveFileDialog.FileName;
            }
        }
    }
}
