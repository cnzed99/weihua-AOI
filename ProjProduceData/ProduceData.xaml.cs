using System;
using System.Collections.Generic;
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
    /// ProduceData.xaml 的交互逻辑
    /// </summary>
    public partial class ProduceData : UserControl
    {
        public ProduceData()
        {
            InitializeComponent();
        }
    }

    public class MultiCalcConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is int && values[1] is int)) return Binding.DoNothing;
            if ((int)values[0] == 0) return 0;

            string para = (string)parameter;
            if (para == "OK")
            {
                return (int)values[0] - (int)values[1];
            }
            else if (para == "Ok%")
            {
                return ((int)values[0]-(int)values[1]) / (int)values[0];
            }
            else if (para == "Ng%")
            {
                return (int)values[1] / (int)values[0];
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
