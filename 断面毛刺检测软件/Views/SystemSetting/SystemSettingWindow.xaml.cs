

namespace 断面毛刺检测软件.Views
{
    /// <summary>
    /// SystemSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SystemSettingWindow : HandyControl.Controls.Window
    {
        public SystemSettingWindow()
        {
            InitializeComponent();
            this.Closing += SystemSettingWindow_Closing;
        }

        private void SystemSettingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
