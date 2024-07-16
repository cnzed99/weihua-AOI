using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QualityGrade;
using WH.Controls.SingleInstance;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 检测设置窗口VM
    /// </summary>
    public partial class CSDFilterCtrlVM : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤分选配置
        /// </summary>
        [ObservableProperty]
        private CFilterConfig filterConfig;

        private CQualityConfig qualityConfig;

        public void SetSDFilterVM(CFilterConfig filterConfig, CQualityConfig qualityConfig)
        {
            FilterConfig = filterConfig;
            this.qualityConfig = qualityConfig;
            Synchronization(qualityConfig);
        }

        /// <summary>
        /// 20240715 TCG
        /// 同步毛刺等级实例
        /// </summary>
        /// <param name="MaociQuality"></param>
        private void Synchronization(CQualityConfig MaociQuality)
        {
            #region 同步毛刺过滤配置
            foreach (var spFilter in FilterConfig.SpeciesFilters)
            {
                foreach (var reFilger in spFilter.RecipeDefects)
                {
                    foreach (var deFilter in reFilger.DefectFilters)
                    {
                        //新建配方 质量等级没有赋值时赋值最差
                        if (deFilter.QualityLevel is null)
                        {
                            deFilter.QualityLevel = MaociQuality.Qualities.Last();
                        }
                        else
                        {
                            var findquality = MaociQuality.Qualities.FirstOrDefault(o =>
                                o.Priority == deFilter.QualityLevel.Priority
                            );
                            deFilter.QualityLevel = null;
                            deFilter.QualityLevel = findquality;
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加过滤器
        /// </summary>
        /// <param name="speciesFilter">目标类别</param>
        [RelayCommand]
        void AddFilter(SpeciesFilter speciesFilter)
        {
            if (speciesFilter.RecipeDefects.Count == 0)
                return;
            RecipeDefect recipeDefect = speciesFilter.RecipeDefects.First();
            int index = 0;
            for (int i = recipeDefect.DefectFilters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(
                    recipeDefect.DefectFilters[i].Name,
                    recipeDefect.Name + "[0-9]+"
                );
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(recipeDefect.Name.Length)) + 1;
                    break;
                }
            }
            recipeDefect.DefectFilters.Add(
                new DefectFilter(recipeDefect.Name + index, FilterConfig.token)
            );
            WeakReferenceMessenger.Default.Send<CFilterConfig>(FilterConfig);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除过滤器
        /// </summary>
        /// <param name="obj">删除目标、所属算法缺陷</param>
        [RelayCommand]
        void RemoveFilter(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                DefectFilter defectFilter = (DefectFilter)objArr[0];
                RecipeDefect recipeDefect = (RecipeDefect)objArr[1];
                recipeDefect.DefectFilters.Remove(defectFilter);
                WeakReferenceMessenger.Default.Send<CFilterConfig>(FilterConfig);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 编辑过滤器
        /// </summary>
        /// <param name="obj">编辑目标、所属检测类</param>
        [RelayCommand]
        void EditFilter(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                DefectFilter defectFilter = (DefectFilter)objArr[0];
                SpeciesFilter speciesFilter = (SpeciesFilter)objArr[1];
                DefectFilterSetWin defectFilterSetWin = SingleInstance.Add(
                    new DefectFilterSetWin(defectFilter, speciesFilter, qualityConfig),
                    defectFilter.Name
                );
                defectFilterSetWin.Title = defectFilter.Name;
                defectFilterSetWin.Show();
                defectFilterSetWin.Activate();

                //每次关闭打开刷新ResultList
                List<EMFILTER> lsParam = new List<EMFILTER>();
                foreach (var filter in defectFilter.FilterList)
                {
                    foreach (var select in filter.Filter)
                    {
                        foreach (var selParam in select.SelectParams)
                        {
                            if (!lsParam.Contains(selParam.Character))
                            {
                                lsParam.Add(selParam.Character);
                            }
                        }
                    }
                    foreach (var select in filter.SelectList)
                    {
                        foreach (var selParam in select.SelectParams)
                        {
                            if (!lsParam.Contains(selParam.Character))
                            {
                                lsParam.Add(selParam.Character);
                            }
                        }
                    }
                }
                foreach (var pa in lsParam)
                {
                    if (defectFilter.ResultList.FirstOrDefault(o => o.Feature == pa) == null)
                    {
                        defectFilter.ResultList.Add(new FilterResult(pa));
                    }
                }
                for (int i = defectFilter.ResultList.Count - 1; i >= 0; i--)
                {
                    if (!lsParam.Contains(defectFilter.ResultList[i].Feature))
                    {
                        defectFilter.ResultList.RemoveAt(i);
                    }
                }
            }
        }
    }
}
