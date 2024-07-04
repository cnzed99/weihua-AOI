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

namespace WH.Controls
{
    /// <summary>
    /// LoginSetting.xaml 的交互逻辑
    /// </summary>
    public partial class LoginSetting : Window
    {
        CSettingViewModel viewModel;
        public LoginSetting()
        {
            viewModel = new CSettingViewModel();
            InitializeComponent();
            this.DataContext = viewModel;
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
            this.Close();
        }
    }
}
