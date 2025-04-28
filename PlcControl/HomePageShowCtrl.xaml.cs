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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlcControl
{
    /// <summary>
    /// HomePageShowCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageShowCtrl : UserControl
    {
        public HomePageShowCtrl(CMotionCtrlVM motionCtrlVM)
        {
            InitializeComponent();
            this.DataContext = motionCtrlVM;
        }

        private void CapcityBtn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}