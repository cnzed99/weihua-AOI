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
using WH.Entity.Attribute;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 检测设置 弹窗界面
    /// </summary>
    public partial class DefectFilterSetWin : HandyControl.Controls.Window
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="defectFilter">过滤器</param>
        /// <param name="speciesFilter">所属类别</param>
        public DefectFilterSetWin(DefectFilter defectFilter, SpeciesFilter speciesFilter)
        {
            InitializeComponent();
            VM = new(defectFilter, speciesFilter);
            this.DataContext = VM;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// VM
        /// </summary>
        public DefectFilterSetVM VM { get; set; }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 特征单位转换器
    /// </summary>
    public class TextUnitsConverter : IValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 特征单位转换器
        /// </summary>
        /// <param name="value">特征</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>单位</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                EMFILTER feature = (EMFILTER)value;
                switch (feature)
                {
                    case EMFILTER.EMFILTER_AREA:
                        return "mm²";
                    case EMFILTER.EMFILTER_NUM:
                        return "PCS";
                    case EMFILTER.EMFILTER_LONGLEN:
                        return "mm";
                    case EMFILTER.EMFILTER_SHORTLEN:
                        return "mm";
                    case EMFILTER.EMFILTER_PHI:
                        return "rad";
                    case EMFILTER.EMFILTER_CONTLEN:
                        return "mm";
                    case EMFILTER.EMFILTER_WIDTH:
                        return "mm";
                    case EMFILTER.EMFILTER_HEIGHT:
                        return "mm";
                    case EMFILTER.EMFILTER_PEAKHEI:
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

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 特征类型转换器
    /// </summary>
    public class ItemSourceCharacterConverter : IMultiValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 特征类型转换
        /// </summary>
        /// <param name="values">过滤/分选</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>特征类型集</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null)
            {
                List<string> enums = EnumStringAttribute.GetEnumNames(typeof(EMFILTER));
                string name = (string)values[0];
                if (name.Contains("过滤"))
                {
                    enums.Remove("数量");
                    return enums;
                }
                else
                {
                    return enums;
                }
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 最小最大验证
    /// </summary>
    class MinMaxValidation : ValidationRule
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 是否最大
        /// </summary>
        public bool IsGreater { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 验证参数
        /// </summary>
        public ValidationParams ValidationParams { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 验证方法
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingProxy bindingProxy = ValidationParams.Data as BindingProxy;
            OneSelectParams viewModel = bindingProxy.Data as OneSelectParams;
            if (int.TryParse(value.ToString(), out int result)/* && int.TryParse(textbox.Text, out int result2)*/)
            {
                if (IsGreater)
                {
                    if (bindingProxy.IsMinError)
                    {
                        if (result < bindingProxy.Min)
                        {
                            bindingProxy.Max = result;
                            bindingProxy.IsMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        else
                        {
                            bindingProxy.IsMinError = false;
                            bindingProxy.IsMaxError = false;
                            viewModel.Max = result;
                            viewModel.Min = bindingProxy.Min;
                        }
                    }
                    else
                    {
                        if (result < viewModel.Min)
                        {
                            bindingProxy.Max = result;
                            bindingProxy.IsMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        bindingProxy.IsMaxError = false;
                    }
                }
                else
                {
                    if (bindingProxy.IsMaxError)
                    {
                        if (result > bindingProxy.Max)
                        {
                            bindingProxy.Min = result;
                            bindingProxy.IsMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        else
                        {
                            bindingProxy.IsMinError = false;
                            bindingProxy.IsMaxError = false;
                            viewModel.Min = result;
                            viewModel.Max = bindingProxy.Max;
                        }
                    }
                    else
                    {
                        if (result > viewModel.Max)
                        {
                            bindingProxy.Min = result;
                            bindingProxy.IsMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        bindingProxy.IsMinError = false;
                    }
                }
            }


            return new ValidationResult(true, "");
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 验证参数
    /// </summary>
    public class ValidationParams : DependencyObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 验证参数
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 验证参数依赖属性
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(ValidationParams), new PropertyMetadata(null));
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// BindingProxy
    /// </summary>
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 存储数据
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 存储数据依赖属性
        /// </summary>
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最小值错误
        /// </summary>
        public bool IsMinError = false;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最小值
        /// </summary>
        public int Min = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最大值错误
        /// </summary>
        public bool IsMaxError = false;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最大值
        /// </summary>
        public int Max = 0;
    }

    /// <summary>
    ///  2024.7.4 李焕彬
    /// 质量等级转换器
    /// </summary>
    public class QualitySelectConverter : IMultiValueConverter
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级转换器
        /// </summary>
        /// <param name="values">质量等级值、质量集</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>质量</returns>
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

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级转换器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>质量等级值</returns>
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
