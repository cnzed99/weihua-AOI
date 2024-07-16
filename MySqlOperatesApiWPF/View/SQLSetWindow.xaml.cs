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
using WH.RunCell;

namespace MySqlOperatesApiWPF
{

    public partial class SQLSetWindow : HandyControl.Controls.Window
    {
        /// <summary>
        /// Mysql数据库
        /// </summary>
      //  public MySqlViewModel MySqlVM { get; set; }
        public SQLSetWindow()
        {
            InitializeComponent();
            //MySqlVM = new MySqlViewModel();
            //this.DataContext= MySqlVM;
        }

        //int bb = 10;
        //private void test_click(object sender, RoutedEventArgs e)
        //{
        //    Cell cell = new Cell();


        //    bb++;
        //    cell.ID= "123" + bb.ToString();
        // cell.WaferID="456" + bb.ToString();
        //    Random random = new Random();
        //    int gg = random.Next();
        //    int hg = gg % 7;
        //    bool hh = hg == 0 ? true : false;
        //    cell.Detection = new CellDetection();
        //    if (hh)
        //    {
        //        cell.IsOK = true;
        //    }
        //    else
        //    {
        //        cell.Detection.Type = "边缘类";
        //        if (hg == 1)
        //        {
        //            cell.QualityName = "G3";
        //            cell.Detection.Name = "崩边";
        //        }
        //        else if (hg == 2)
        //        {
        //            cell.QualityName = "G2";
        //            cell.Detection.Name = "缺角";
        //        }
        //        else if (hg == 3)
        //        {
        //            cell.QualityName = "G2";
        //            cell.Detection.Name = "异色";
        //        }
        //        else if (hg == 4)
        //        {
        //            cell.QualityName = "G3";
        //            cell.Detection.Name = "大饼片";
        //        }
        //        else
        //        {
        //            cell.QualityName = "G3";
        //            cell.Detection.Name = "划伤";
        //        }

        //    }
        //    DateTime time = DateTime.Now.AddDays(2);
        //    MySqlVM.mysqlExecute.AddData(cell, time.ToString("D"));
        //}


    }
}