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

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// OpenCommunication.xaml 的交互逻辑
    /// </summary>
    public partial class OpenCommunication : HandyControl.Controls.Window
    {
        public OpenCommunication(CCommunicationSettingBase settingBase)
        {
            InitializeComponent();
            communicationVM = new COpenCommunicationVM(settingBase);
            this.DataContext = communicationVM;
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// VM
        /// </summary>
        public COpenCommunicationVM communicationVM;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 关闭事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
