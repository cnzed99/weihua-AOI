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

    public class GridHeigth2ExpanderHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class string2ComboxSelectItemPc : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                string name = values[0] as string;
                ObservableCollection<MaociAlgorParam> maociAlgorParams = values[1] as ObservableCollection<MaociAlgorParam>;
                if (name!=null && maociAlgorParams!=null)
                {
                    return maociAlgorParams.FirstOrDefault(o => o.Name == name) ?? Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();
            MaociAlgorParam maociAlgorParam = value as MaociAlgorParam;
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

    public class string2ComboxSelectItemFpga : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                string name = values[0] as string;
                ObservableCollection<MaociAlgorParamFpga> maociAlgorParams = values[1] as ObservableCollection<MaociAlgorParamFpga>;
                if (name != null && maociAlgorParams != null)
                {
                    return maociAlgorParams.FirstOrDefault(o => o.Name == name) ?? Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();
            MaociAlgorParamFpga maociAlgorParam = value as MaociAlgorParamFpga;
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
