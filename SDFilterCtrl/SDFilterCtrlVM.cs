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
using HandyControl.Controls;
using QualityGrade;
using WH.Controls;
using WH.Controls.SingleInstance;
using WH.RecipeCellRootBase;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 检测设置窗口VM
    /// </summary>
    public partial class CSDFilterCtrlVM : ObservableObject
    {
        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤分选配置
        /// </summary>
        [ObservableProperty]
        private CFilterConfig filterConfig;

        /// <summary>
        /// 质量等级配置
        /// </summary>
        public CQualityConfig QualityConfig { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 特征项，过滤分选Combox用
        /// </summary>
        public List<CFeacture> DefectFeactures { get; set; }
        /// <summary>
        /// 上一次选中的项
        /// </summary>
        public DefectFilter LastSelectedDefectFilter { get; set; }


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
            int index = 2;
            for (int i = recipeDefect.DefectFilters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(
                    recipeDefect.DefectFilters[i].Name,
                    recipeDefect.Name + "[2-9]+"
                );
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(recipeDefect.Name.Length)) + 1;
                    break;
                }
            }
            var de = new DefectFilter(recipeDefect.Name + index, FilterConfig.token);
            recipeDefect.DefectFilters.Add(de);
            de.QualityLevel = QualityConfig.GetWorst();
            //WeakReferenceMessenger.Default.Send<CFilterConfig>(FilterConfig);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除过滤器
        /// </summary>
        /// <param name="obj">删除目标、所属算法缺陷</param>
        [RelayCommand]
        void RemoveFilter(object obj)
        {
            Growl.AskGlobal(
                FilterConfig.PrcessName + "-" + Properties.Resource1.DelecteAsk,
                b =>
                {
                    if (b)
                    {
                        var objArr = obj as object[];
                        if (objArr != null && objArr.Length == 2)
                        {
                            DefectFilter defectFilter = (DefectFilter)objArr[0];
                            RecipeDefect recipeDefect = (RecipeDefect)objArr[1];
                            recipeDefect.DefectFilters.Remove(defectFilter);
                            //WeakReferenceMessenger.Default.Send<CFilterConfig>(FilterConfig);
                        }
                    }
                    return true;
                }
            );
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
                    new DefectFilterSetWin(
                        defectFilter,
                        speciesFilter,
                        QualityConfig,
                        DefectFeactures
                    ),
                    defectFilter.Name
                );
                if (defectFilter != null)
                {
                    // 如果上一次选中项存在，取消其选中状态
                    if (LastSelectedDefectFilter != null)
                    {
                        LastSelectedDefectFilter.IsSelected = false;
                    }

                    // 设置当前项为选中状态
                    defectFilter.IsSelected = true;

                    // 更新上一次选中项为当前项
                    LastSelectedDefectFilter = defectFilter;
                }
                defectFilterSetWin.Title = defectFilter.Name;
                defectFilterSetWin.Closed += (s, e) =>
                {
                    //每次关闭打开刷新ResultList
                    List<CFeacture> lsParam = new List<CFeacture>();
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
                };
                defectFilterSetWin.Show();
                defectFilterSetWin.Activate();
            }
        }
    }
}
