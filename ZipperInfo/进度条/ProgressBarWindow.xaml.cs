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

namespace ZipperInfo
{
    /// <summary>
    /// ProgressBarWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressBarWindow : HandyControl.Controls.Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            CZipperCommunicateBase.CamTriggerStop(); //停止拍照
            CZipperAutomaticAlgorithm.TestFinshEven(false);
            CZipperCommunicateBase.AixtContinue(true);
            CZipperCommunicateBase.TestFinish();
            this.Close();   
        }
    }
}
