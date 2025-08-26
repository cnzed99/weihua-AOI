using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZipperInfo
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ZipperInfoUserControl : UserControl
    {
        public ZipperInfoVM ZipperInfoVM { get; set; }
        public ZipperInfoUserControl()
        {
            InitializeComponent();
            //ZipperInfoVM = new ZipperInfoVM();
            //this.DataContext = ZipperInfoVM;
        }
    }

}
