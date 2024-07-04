using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QualityGrade;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 过滤分选设置窗口VM
    /// </summary>
    public partial class DefectFilterSetVM : ObservableValidator
    {
        public DefectFilterSetVM() { }
        public DefectFilterSetVM(DefectFilter defectFilter, SpeciesFilter speciesFilter) 
        {
            this.DefectFilter = defectFilter;
            RecipeDefects = speciesFilter.RecipeDefects.ToList();
            DefectName = defectFilter.Name;
            recipeDefect = RecipeDefects.Find(o => o.DefectFilters.Contains(defectFilter));

            var res = WeakReferenceMessenger.Default.Send(new RequestMessage<ObservableCollection<Quality>>());
            Qualities = res.Response;
        }

        [ObservableProperty]
        private ObservableCollection<Quality> qualities;

        [ObservableProperty]
        private DefectFilter defectFilter;

        private string defectName;
        /// <summary>
        /// 缺陷名
        /// </summary>
        [Required]
        [CustomValidation(typeof(DefectFilterSetVM), nameof(ValidateDefectName))]
        public string DefectName
        {
            get => defectName;
            set
            {
                var oldValue = defectName;
                if(SetProperty(ref defectName, value, true))
                {
                    WeakReferenceMessenger.Default.Send<PropertyChangedMessage<string>, string>(new PropertyChangedMessage<string>(this, nameof(DefectName), oldValue, defectName), nameof(DefectName));
                }
                if (GetErrors(nameof(DefectName)).Count() == 0)
                {
                    DefectFilter.Name = defectName;
                }
            }
        }

        bool IsValidString(string input)
        {
            string pattern = @"^(?:[\u4e00-\u9fa5a-zA-Z_])[\w\u4e00-\u9fa5]*$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        public static ValidationResult ValidateDefectName(string name, ValidationContext context)
        {
            DefectFilterSetVM instance = (DefectFilterSetVM)context.ObjectInstance;
            if (name == string.Empty)
                return new ValidationResult(Properties.Resource1.NameNotNull);
            if (!instance.IsValidString(name))
                return new ValidationResult(Properties.Resource1.NameInValid);
            if (instance.RecipeDefects.Exists(o => o.DefectFilters.ToList().Exists(o => o != instance.DefectFilter && o.Name == name)))
                return new ValidationResult(Properties.Resource1.NameRepeat);
            return ValidationResult.Success;
        }

        public List<RecipeDefect> RecipeDefects { get; set; }

        private RecipeDefect recipeDefect;

        public RecipeDefect RecipeDefect
        {
            get { return recipeDefect; }
            set {
                recipeDefect.DefectFilters.Remove(DefectFilter);
                SetProperty(ref recipeDefect, value);
                recipeDefect.DefectFilters.Add(DefectFilter);
            }
        }



        [RelayCommand]
        public void AddFilterConfig()
        {
            DefectFilter?.FilterList.Add(new FilterAndSelect());
        }

        [RelayCommand]
        public void DeleteFilterConfig(FilterAndSelect filterConfig)
        {
            DefectFilter?.FilterList.Remove(filterConfig);
        }

        [RelayCommand]
        public void AddSelectConfig(FilterAndSelect filterConfig)
        {
            filterConfig.Filter.Add(new SelectConfig());
        }

        [RelayCommand]
        public void AddSelectConfig2(FilterAndSelect filterConfig)
        {
            filterConfig.SelectList.Add(new SelectConfig());
        }

        [RelayCommand]
        public void DeleteSelectConfig(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                SelectConfig selectConfig = objArr[0] as SelectConfig;
                ObservableCollection<SelectConfig> selectConfigs = objArr[1] as ObservableCollection<SelectConfig>;
                if(selectConfigs != null && selectConfig != null) selectConfigs.Remove(selectConfig);
            }
        }

        [RelayCommand]
        public void AddOneSelectParam(SelectConfig selectConfig)
        {
            selectConfig.SelectParams.Add(new OneSelectParams());
        }

        [RelayCommand]
        public void DeleteOneSelectParam(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                OneSelectParams oneSelectParams = objArr[0] as OneSelectParams;
                ObservableCollection<OneSelectParams> lsSelectParams = objArr[1] as ObservableCollection<OneSelectParams>;
                if (oneSelectParams != null && lsSelectParams != null) lsSelectParams.Remove(oneSelectParams);
            }     
        }
    }
}
