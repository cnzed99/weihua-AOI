using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace ProjProduceData
{
    /// <summary>
    /// DefectsDataV.xaml 的交互逻辑
    /// </summary>
    public partial class DefectsDataV : UserControl
    {
        public DefectsDataV()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = false;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var srollViewer = (ScrollViewer)sender;
            srollViewer.ScrollToVerticalOffset(srollViewer.VerticalOffset - e.Delta);
        }
    }

    
}
