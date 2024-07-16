using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QualityGrade;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 过滤分选设置窗口VM
    /// </summary>
    public partial class CDefectFilterSetVM : ObservableValidator
    {
        public List<EMFILTER> FilterCharacters { get; set; }

        public CDefectFilterSetVM()
        {
            FilterCharacters = new List<EMFILTER>()
            {
                EMFILTER.EMFILTER_PEAKHEI,
                EMFILTER.EMFILTER_AREA,
                EMFILTER.EMFILTER_LONGLEN,
                EMFILTER.EMFILTER_SHORTLEN,
                EMFILTER.EMFILTER_PHI,
                EMFILTER.EMFILTER_CONTLEN,
                EMFILTER.EMFILTER_WIDTH,
                EMFILTER.EMFILTER_HEIGHT
            };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="defectFilter">过滤器</param>
        /// <param name="speciesFilter">类别</param>
        public CDefectFilterSetVM(
            DefectFilter defectFilter,
            SpeciesFilter speciesFilter,
            CQualityConfig qualityConfig
        )
            : this()
        {
            this.DefectFilter = defectFilter;
            RecipeDefects = speciesFilter.RecipeDefects.ToList();
            DefectName = defectFilter.Name;
            recipeDefect = RecipeDefects.Find(o => o.DefectFilters.Contains(defectFilter));
            //向质量等级请求数据
            //var res = WeakReferenceMessenger.Default.Send(new RequestMessage<ObservableCollection<Quality>>(), "GetQuality");
            Qualities = qualityConfig.Qualities;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Quality> qualities;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤器
        /// </summary>
        [ObservableProperty]
        private DefectFilter defectFilter;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷名
        /// </summary>
        private string defectName;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷名
        /// </summary>
        [Required]
        [CustomValidation(typeof(CDefectFilterSetVM), nameof(ValidateDefectName))]
        public string DefectName
        {
            get => defectName;
            set
            {
                var oldValue = defectName;
                if (SetProperty(ref defectName, value, true))
                {
                    WeakReferenceMessenger.Default.Send<PropertyChangedMessage<string>, string>(
                        new PropertyChangedMessage<string>(
                            this,
                            nameof(DefectName),
                            oldValue,
                            defectName
                        ),
                        nameof(DefectName)
                    );
                }
                if (GetErrors(nameof(DefectName)).Count() == 0)
                {
                    DefectFilter.Name = defectName;
                }
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 字符串验证
        /// </summary>
        /// <param name="input">输入名</param>
        /// <returns></returns>
        bool IsValidString(string input)
        {
            string pattern = @"^(?:[\u4e00-\u9fa5a-zA-Z_])[\w\u4e00-\u9fa5]*$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷名验证
        /// </summary>
        /// <param name="name">输入名</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidationResult ValidateDefectName(string name, ValidationContext context)
        {
            CDefectFilterSetVM s_Instance = (CDefectFilterSetVM)context.ObjectInstance;
            if (name == string.Empty)
                return new ValidationResult(Properties.Resource1.NameNotNull);
            if (!s_Instance.IsValidString(name))
                return new ValidationResult(Properties.Resource1.NameInValid);
            if (
                s_Instance.RecipeDefects.Exists(o =>
                    o.DefectFilters.ToList()
                        .Exists(o => o != s_Instance.DefectFilter && o.Name == name)
                )
            )
                return new ValidationResult(Properties.Resource1.NameRepeat);
            return ValidationResult.Success;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法缺陷集
        /// </summary>
        public List<RecipeDefect> RecipeDefects { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 当前算法
        /// </summary>
        private RecipeDefect recipeDefect;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 当前算法
        /// </summary>
        public RecipeDefect RecipeDefect
        {
            get { return recipeDefect; }
            set
            {
                recipeDefect.DefectFilters.Remove(DefectFilter);
                SetProperty(ref recipeDefect, value);
                recipeDefect.DefectFilters.Add(DefectFilter);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加过滤分选
        /// </summary>
        [RelayCommand]
        public void AddFilterConfig()
        {
            DefectFilter?.FilterList.Add(new FilterAndSelect(DefectFilter.token));
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除过滤分选
        /// </summary>
        /// <param name="filterConfig">目标</param>
        [RelayCommand]
        public void DeleteFilterConfig(FilterAndSelect filterConfig)
        {
            DefectFilter?.FilterList.Remove(filterConfig);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加过滤项
        /// </summary>
        /// <param name="filterConfig">所属过滤分选器</param>
        [RelayCommand]
        public void AddSelectConfig(FilterAndSelect filterConfig)
        {
            filterConfig.Filter.Add(new SelectConfig(filterConfig.token));
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加分选项
        /// </summary>
        /// <param name="filterConfig">所属过滤分选器</param>
        [RelayCommand]
        public void AddSelectConfig2(FilterAndSelect filterConfig)
        {
            filterConfig.SelectList.Add(new SelectConfig(filterConfig.token));
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除过滤/分选
        /// </summary>
        /// <param name="obj">删除目标、目标所属容器</param>
        [RelayCommand]
        public void DeleteSelectConfig(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                SelectConfig selectConfig = objArr[0] as SelectConfig;
                ObservableCollection<SelectConfig> selectConfigs =
                    objArr[1] as ObservableCollection<SelectConfig>;
                if (selectConfigs != null && selectConfig != null)
                    selectConfigs.Remove(selectConfig);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加条件
        /// </summary>
        /// <param name="selectConfig">所属过滤/分选</param>
        [RelayCommand]
        public void AddOneSelectParam(SelectConfig selectConfig)
        {
            selectConfig.SelectParams.Add(new OneSelectParams(selectConfig.token));
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除条件
        /// </summary>
        /// <param name="obj">删除目标、目标所属容器</param>
        [RelayCommand]
        public void DeleteOneSelectParam(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                OneSelectParams oneSelectParams = objArr[0] as OneSelectParams;
                ObservableCollection<OneSelectParams> lsSelectParams =
                    objArr[1] as ObservableCollection<OneSelectParams>;
                if (oneSelectParams != null && lsSelectParams != null)
                    lsSelectParams.Remove(oneSelectParams);
            }
        }
    }
}
