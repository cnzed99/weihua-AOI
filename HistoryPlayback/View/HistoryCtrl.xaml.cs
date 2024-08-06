
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace HistoryPlayback
{
    /// <summary>
    /// HistoryCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryCtrl : UserControl
    {
      // public CHistortVM HistortVM { get; set; }

        public HistoryCtrl()
        {
            InitializeComponent();
           // HistortVM = new CHistortVM();

           // this.DataContext = HistortVM;
        }
    }
}
