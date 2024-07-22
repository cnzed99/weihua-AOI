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

namespace Modbus
{
    /// <summary>
    /// 2024.7.22 李焕彬
    /// TestControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestControl : UserControl, IDisposable
    {
        public TestControl(CModbusCommPart cModbusCommPart)
        {
            InitializeComponent();
            testControlVM = new TestControlVM(cModbusCommPart);
            this.DataContext = testControlVM;
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// VM
        /// </summary>
        TestControlVM testControlVM;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 释放
        /// </summary>
        public void Dispose()
        {
            testControlVM?.Close();
        }
    }
}
