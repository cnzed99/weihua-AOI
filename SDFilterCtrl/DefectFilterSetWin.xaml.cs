using CommunityToolkit.Mvvm.ComponentModel;
using QualityGrade;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// DefectFilterSetWin.xaml 的交互逻辑
    /// </summary>
    public partial class DefectFilterSetWin : HandyControl.Controls.Window
    {
        public DefectFilterSetWin(DefectFilter defectFilter, SpeciesFilter speciesFilter)
        {
            InitializeComponent();
            VM = new(defectFilter, speciesFilter);
            this.DataContext = VM;
        }

        public DefectFilterSetVM VM { get; set; }
    }

    public class TextUnitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                DetectFeature feature = (DetectFeature)value;
                switch (feature)
                {
                    case DetectFeature.面积:
                        return "mm²";
                    case DetectFeature.数量:
                        return "PCS";
                    case DetectFeature.长边:
                        return "mm";
                    case DetectFeature.短边:
                        return "mm";
                    case DetectFeature.角度:
                        return "rad";
                    case DetectFeature.周长:
                        return "mm";
                    case DetectFeature.宽度:
                        return "mm";
                    case DetectFeature.高度:
                        return "mm";
                    case DetectFeature.顶点高度:
                        return "mm";
                    default:
                        return "";
                }
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CmbCharacterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null)
            {
                string name = (string)values[0];
                List<string> items = Enum.GetNames(typeof(DetectFeature)).ToList();
                if (name.Contains("过滤")) items.Remove("数量");
                return items;
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class MinMaxValidation : ValidationRule
    {
        public bool isGreater { get; set; }
        public ValidationParams ValidationParams { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingProxy bindingProxy = ValidationParams.Data as BindingProxy;
            OneSelectParams viewModel = bindingProxy.Data as OneSelectParams;
            if (int.TryParse(value.ToString(), out int result)/* && int.TryParse(textbox.Text, out int result2)*/)
            {
                if (isGreater)
                {
                    if (bindingProxy.isMinError)
                    {
                        if (result < bindingProxy.Min)
                        {
                            bindingProxy.Max = result;
                            bindingProxy.isMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        else
                        {
                            bindingProxy.isMinError = false;
                            bindingProxy.isMaxError = false;
                            viewModel.Max = result;
                            viewModel.Min = bindingProxy.Min;
                        }
                    }
                    else
                    {
                        if (result < viewModel.Min)
                        {
                            bindingProxy.Max = result;
                            bindingProxy.isMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        bindingProxy.isMaxError = false;
                    }
                }
                else
                {
                    if (bindingProxy.isMaxError)
                    {
                        if (result > bindingProxy.Max)
                        {
                            bindingProxy.Min = result;
                            bindingProxy.isMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        else
                        {
                            bindingProxy.isMinError = false;
                            bindingProxy.isMaxError = false;
                            viewModel.Min = result;
                            viewModel.Max = bindingProxy.Max;
                        }
                    }
                    else
                    {
                        if (result > viewModel.Max)
                        {
                            bindingProxy.Min = result;
                            bindingProxy.isMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        bindingProxy.isMinError = false;
                    }
                }
            }


            return new ValidationResult(true, "");
        }
    }

    public class ValidationParams : DependencyObject
    {
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(ValidationParams), new PropertyMetadata(null));
    }

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        public bool isMinError = false;

        public int Min = 0;

        public bool isMaxError = false;

        public int Max = 0;
    }

    public class ExpanderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? $"-" : $"+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IsFocusedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (bool)value ? Brushes.Green : Brushes.Red;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class QualitySelectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null && values[1] != null)
            {
                int Level = (int)values[0];
                ObservableCollection<Quality> qualities = values[1] as ObservableCollection<Quality>;
                return qualities.FirstOrDefault(o => o.Priority == Level);
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();
            if (value != null)
            {
                Quality quality = value as Quality;
                list.Add(quality.Priority);
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
