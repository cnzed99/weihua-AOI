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
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Properties.Langs;
using QualityGrade;
using WH.Entity;
using WH.Entity.Attribute;
using WH.RecipeCellRootBase;

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
        public DefectFilterSetWin(
            DefectFilter defectFilter,
            SpeciesFilter speciesFilter,
            CQualityConfig qualityConfig,
            List<CFeacture> defectFeactures
        )
        {
            InitializeComponent();
            VM = new(defectFilter, speciesFilter, qualityConfig, defectFeactures);
            this.DataContext = VM;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// VM
        /// </summary>
        public CDefectFilterSetVM VM { get; set; }

        public static bool CanColse = true;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CanColse) 
            { 
                e.Cancel = true;
            }
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
            DefectFilterSetWin.CanColse = false;
            OneSelectParams viewModel = ValidationParams.Data as OneSelectParams;
            if (
                double.TryParse(value.ToString(), out double result) /* && int.TryParse(textbox.Text, out int result2)*/
            )
            {
                if (IsGreater)
                {
                    if (ValidationParams.IsMinError)
                    {
                        if (result < ValidationParams.Min)
                        {
                            ValidationParams.Max = result;
                            ValidationParams.IsMaxError = true;
                            DefectFilterSetWin.CanColse = false;
                            return new ValidationResult(false, "不能小于最小值！");   
                        }
                        else
                        {
                            ValidationParams.IsMinError = false;
                            ValidationParams.IsMaxError = false;
                            DefectFilterSetWin.CanColse = true;
                            viewModel.Max = result;
                            viewModel.Min = ValidationParams.Min;
                        }
                    }
                    else
                    {
                        DefectFilterSetWin.CanColse = true;
                        if (result < viewModel.Min)
                        {
                            ValidationParams.Max = result;
                            ValidationParams.IsMaxError = true;
                            DefectFilterSetWin.CanColse = false;
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
                            DefectFilterSetWin.CanColse = false;
                            return new ValidationResult(false, "不能大于最大值！");
                        }
                        else
                        {
                            ValidationParams.IsMinError = false;
                            ValidationParams.IsMaxError = false;
                            DefectFilterSetWin.CanColse = true;
                            viewModel.Min = result;
                            viewModel.Max = ValidationParams.Max;
                        }
                    }
                    else
                    {
                        DefectFilterSetWin.CanColse = true;
                        if (result > viewModel.Max)
                        {
                            ValidationParams.Min = result;
                            ValidationParams.IsMinError = true;
                            DefectFilterSetWin.CanColse = false;
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

    /// <summary>
    /// 2024.10.22 李焕彬
    /// 特征选项CombBox专用
    /// </summary>
    public class CmbCharacterConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (
                values[0] is RecipeDefect recipe
                && values[1] is string name
                && values[2] is List<CFeacture> defectFeactures
            )
            {
                switch (recipe.Category)
                {
                    case Category.区域:
                        if (name == Properties.Resource1.Select)
                        {
                            var feactures = defectFeactures.ToList();
                            feactures.Add(CFeacture.FeactureCount);
                            return feactures;
                        }
                        else
                        {
                            return defectFeactures;
                        }
                    case Category.值:
                        if (name == Properties.Resource1.Select)
                        {
                            return new List<CFeacture>
                            {
                                CFeacture.FeactureValue,
                                CFeacture.FeactureCount
                            };
                        }
                        else
                        {
                            return new List<CFeacture> { CFeacture.FeactureValue };
                        }
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
            throw new NotImplementedException();
        }
    }
}
