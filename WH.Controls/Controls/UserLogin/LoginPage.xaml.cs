using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using WH.Entity.Messages;

namespace WH.Controls
{
    /// <summary>
    /// WelComePage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Window
    {
        public CLoginViewModel viewModel { get; set; }

        public LoginPage(CLoginViewModel vm)
        {
            InitializeComponent();

            viewModel = vm;
            this.DataContext = viewModel;
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(
                this,
                (_, m) =>
                {
                    if (m.Sender?.Target == this.DataContext)
                        Close();
                }
            );
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
                if (!viewModel.LoggedSuccess)
                    viewModel.LoginPerson.Init();
            }
            this.Close();
        }
    }
}
