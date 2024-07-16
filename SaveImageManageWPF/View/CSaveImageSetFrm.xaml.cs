using System.Drawing;
using System.IO;
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
using CommunityToolkit.Mvvm.Messaging;
using WH.Entity.Messages;

namespace SaveImageManage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CSaveImageSetFrm : HandyControl.Controls.Window
    {
        //  public  CSaveImageViewModel viewModel {  get; set; }
        public CSaveImageSetFrm()
        {
            InitializeComponent();
            //  viewModel=new CSaveImageViewModel();
            //  this.DataContext = viewModel;
            //  WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (_, m) => { if (m.Sender?.Target == this.DataContext) Close(); });
        }
    }
}
