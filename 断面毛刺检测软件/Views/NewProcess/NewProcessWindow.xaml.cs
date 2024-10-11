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
using Autofac;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Properties.Langs;
using Microsoft.Win32;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.Entity.Messages;

namespace 断面毛刺检测软件.Views
{
    /// <summary>
    /// NewProcessWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewProcessWindow : HandyControl.Controls.Window
    {
        public NewProcessWindow()
        {
            InitializeComponent();
            this.DataContext = new CNewProcessVM(App.Container.Resolve<CMainModelsModelVM>());
            this.Title = NewProcess.NewProcessResources.AddProcess;
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(
                this,
                (_, m) =>
                {
                    if (m.Sender?.Target == this.DataContext)
                    {
                        this.DialogResult = m.DialogResult;
                        Close();
                    }
                }
            );
        }

        public NewProcessWindow(CMainModel mainVM)
        {
            InitializeComponent();
            this.DataContext = new CModifyProcessVM(
                App.Container.Resolve<CMainModelsModelVM>(),
                mainVM
            );
            this.Title = NewProcess.NewProcessResources.EditProcess;
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(
                this,
                (_, m) =>
                {
                    if (m.Sender?.Target == this.DataContext)
                    {
                        this.DialogResult = m.DialogResult;
                        Close();
                    }
                }
            );
        }
    }
}
