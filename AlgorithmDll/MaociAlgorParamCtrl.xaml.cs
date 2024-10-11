using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// MaociAlgorParamCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class MaociAlgorParamCtrl : UserControl
    {
        public MaociAlgorParamCtrl()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// Grid高度一半转换器
    /// </summary>
    public class GridHeigth2ExpanderHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数ComboBox转换器
    /// </summary>
    public class string2ComboxSelectItemPc : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values.Length == 2)
            {
                string name = values[0] as string;
                ObservableCollection<CPcParamBase> maociAlgorParams =
                    values[1] as ObservableCollection<CPcParamBase>;
                if (name != null && maociAlgorParams != null)
                {
                    return maociAlgorParams.FirstOrDefault(o => o.Name == name)
                        ?? Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            List<object> list = new List<object>();
            CPcParamBase maociAlgorParam = value as CPcParamBase;
            if (maociAlgorParam != null)
            {
                list.Add(maociAlgorParam.Name);
            }
            else
            {
                list.Add(Binding.DoNothing);
            }
            list.Add(Binding.DoNothing);

            return list.ToArray();
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// Fpga算法参数ComboBox转换器
    /// </summary>
    public class string2ComboxSelectItemFpga : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values.Length == 2)
            {
                string name = values[0] as string;
                ObservableCollection<CFpgaParamBase> maociAlgorParams =
                    values[1] as ObservableCollection<CFpgaParamBase>;
                if (name != null && maociAlgorParams != null)
                {
                    return maociAlgorParams.FirstOrDefault(o => o.Name == name)
                        ?? Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            List<object> list = new List<object>();
            CFpgaParamBase maociAlgorParam = value as CFpgaParamBase;
            if (maociAlgorParam != null)
            {
                list.Add(maociAlgorParam.Name);
            }
            else
            {
                list.Add(Binding.DoNothing);
            }
            list.Add(Binding.DoNothing);

            return list.ToArray();
        }
    }
}
