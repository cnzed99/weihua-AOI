using System.Collections.ObjectModel;
using System.Data;
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

namespace DataQuery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DataQueryWindow : HandyControl.Controls.Window
    {
        //public CDataQueryVM dataVM { get; set; }

        public DataQueryWindow()
        {
            InitializeComponent();
            // dataVM = new CDataQueryVM();
            //// myListView.ItemsSource = dataVM.DataTable;
            // this.DataContext = dataVM;
        }

        //private void BTN_CLICK(object sender, RoutedEventArgs e)
        //{
        //   DataSet dataSet = dataVM.hhhhh();

        //    // 假设你从数据库查询得到的DataSet叫做 dataSet
        //    DataTableCollection tables = dataSet.Tables;
        //    if (tables.Count > 0)
        //    {
        //        // 假设第一个表格是你想要显示的表格
        //        DataTable table = tables[0];

        //        // 将DataTable转换成一个集合，比如ObservableCollection
        //        // ObservableCollection<DataView> dataRows = new ObservableCollection<DataView>(table.DefaultView);

        //        // 设置ListView的ItemsSource
        //        myListView.ItemsSource = table.DefaultView;//dataRows;
        //    }
        //}
    }
}
