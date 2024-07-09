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
using WH.Entity;
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
        public DefectFilterSetWin(DefectFilter defectFilter, SpeciesFilter speciesFilter,CQualityConfig qualityConfig)
        {
            InitializeComponent();
            VM = new(defectFilter, speciesFilter, qualityConfig);
            this.DataContext = VM;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// VM
        /// </summary>
        public CDefectFilterSetVM VM { get; set; }
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
                        return "um²";
                    case EMFILTER.EMFILTER_NUM:
                        return "PCS";
                    case EMFILTER.EMFILTER_LONGLEN:
                        return "um";
                    case EMFILTER.EMFILTER_SHORTLEN:
                        return "um";
                    case EMFILTER.EMFILTER_PHI:
                        return "°";
                    case EMFILTER.EMFILTER_CONTLEN:
                        return "um";
                    case EMFILTER.EMFILTER_WIDTH:
                        return "um";
                    case EMFILTER.EMFILTER_HEIGHT:
                        return "um";
                    case EMFILTER.EMFILTER_PEAKHEI:
                        return "um";
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
            //CBindingProxy bindingProxy = ValidationParams.Data as CBindingProxy;
            OneSelectParams viewModel = ValidationParams.Data as OneSelectParams;
            if (double.TryParse(value.ToString(), out double result)/* && int.TryParse(textbox.Text, out int result2)*/)
            {
                if (IsGreater)
                {
                    if (ValidationParams.IsMinError)
                    {
                        if (result < ValidationParams.Min)
                        {
                            ValidationParams.Max = result;
                            ValidationParams.IsMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        else
                        {
                            ValidationParams.IsMinError = false;
                            ValidationParams.IsMaxError = false;
                            viewModel.Max = result;
                            viewModel.Min = ValidationParams.Min;
                        }
                    }
                    else
                    {
                        if (result < viewModel.Min)
                        {
                            ValidationParams.Max = result;
                            ValidationParams.IsMaxError = true;
                            return new ValidationResult(false, "不能小于最小值！");
                        }
                        ValidationParams.IsMaxError = false;
                    }
                }
                else
                {
                    if (ValidationParams.IsMaxError)
                    {
                        if (result > ValidationParams.Max)
                        {
                            ValidationParams.Min = result;
                            ValidationParams.IsMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        else
                        {
                            ValidationParams.IsMinError = false;
                            ValidationParams.IsMaxError = false;
                            viewModel.Min = result;
                            viewModel.Max = ValidationParams.Max;
                        }
                    }
                    else
                    {
                        if (result > viewModel.Max)
                        {
                            ValidationParams.Min = result;
                            ValidationParams.IsMinError = true;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        ValidationParams.IsMinError = false;
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
    public class ValidationParams : CBindingProxy
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最小值错误
        /// </summary>
        public bool IsMinError = false;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最小值
        /// </summary>
        public double Min = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最大值错误
        /// </summary>
        public bool IsMaxError = false;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最大值
        /// </summary>
        public double Max = 0;

        protected override Freezable CreateInstanceCore()
        {
            return new ValidationParams();
        }
    }
}
