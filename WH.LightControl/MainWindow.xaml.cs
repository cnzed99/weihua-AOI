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

namespace WH.LightControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CLinghtManagement.LoadLightParams();

            //var vm = new LSWLightControlVM();

            //this.DataContext = vm;

            this.DataContext = CLinghtManagement.LightControlDict.Values.ToList()[0];


            foreach (var item in CLinghtManagement.LightControlDict.Values)
            {
                item.BaseConfig.DataContextChangedEvent += (d) =>
                {
                    if (d != null)
                    {
                        this.DataContext = d;
                    }
                };
            }



        }
    }
}
