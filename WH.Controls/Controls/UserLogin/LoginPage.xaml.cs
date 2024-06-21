using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Drawing;
using CommunityToolkit.Mvvm.Messaging;


namespace WH.Controls
{
    /// <summary>
    /// WelComePage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Window
    {
       public LoginViewModel? viewModel { get; set; }


        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            
            viewModel = vm;
            this.DataContext = viewModel;
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (_, m) => { if (m.Sender?.Target == this.DataContext) Close(); });
        }


        private void WinMove_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

       

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {

                if (!viewModel.LoggedSuccess) viewModel.LoginPerson.Init();
            }
            this.Close();

        }
    }
}
