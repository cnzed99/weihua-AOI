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
    /// 2024.7.4 李焕彬
    /// 产量统计控件
    /// ProduceData.xaml 的交互逻辑
    /// </summary>
    public partial class ProduceData : UserControl
    {
        public ProduceData()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 数值显示转换器
    /// </summary>
    public class MultiCalcConverter : IMultiValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数值显示转换
        /// </summary>
        /// <param name="values">总数/NG数</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">参数</param>
        /// <param name="culture"></param>
        /// <returns>显示</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is double && values[1] is double)) return Binding.DoNothing;
            double total = (double)values[0];
            double ng = (double)values[1];
            if (total == 0) return 0;

            string para = (string)parameter;
            if (para == "OK")
            {
                return total - ng;
            }
            else if (para == "OK%")
            {
                return (total - ng) / total;
            }
            else if (para == "NG%")
            {
                return ng / total;
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
