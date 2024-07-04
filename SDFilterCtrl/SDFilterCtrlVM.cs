using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WH.Entity.CommonLib;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 检测设置窗口VM
    /// </summary>
    public partial class SDFilterCtrlVM : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤分选配置
        /// </summary>
        [ObservableProperty]
        private FilterConfig filterConfig;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加过滤器
        /// </summary>
        /// <param name="speciesFilter">目标类别</param>
        [RelayCommand]
        public void AddFilter(SpeciesFilter speciesFilter)
        {
            if (speciesFilter.RecipeDefects.Count == 0) return;
            RecipeDefect recipeDefect = speciesFilter.RecipeDefects.First();
            int index = 0;
            for (int i = recipeDefect.DefectFilters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(recipeDefect.DefectFilters[i].Name, recipeDefect.Name + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(recipeDefect.Name.Length)) + 1;
                    break;
                }
            }
            recipeDefect.DefectFilters.Add(new DefectFilter(recipeDefect.Name + index));
            WeakReferenceMessenger.Default.Send<FilterConfig>(FilterConfig);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除过滤器
        /// </summary>
        /// <param name="obj">删除目标、所属算法缺陷</param>
        [RelayCommand]
        public void RemoveFilter(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                DefectFilter defectFilter = (DefectFilter)objArr[0];
                RecipeDefect recipeDefect = (RecipeDefect)objArr[1];
                recipeDefect.DefectFilters.Remove(defectFilter);
                WeakReferenceMessenger.Default.Send<FilterConfig>(FilterConfig);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 编辑过滤器
        /// </summary>
        /// <param name="obj">编辑目标、所属检测类</param>
        [RelayCommand]
        public void EditFilter(object obj) 
        {
            var objArr = obj as object[];
            if (objArr != null && objArr.Length == 2)
            {
                DefectFilter defectFilter = (DefectFilter)objArr[0];
                SpeciesFilter speciesFilter = (SpeciesFilter)objArr[1];
                DefectFilterSetWin defectFilterSetWin = new DefectFilterSetWin(defectFilter, speciesFilter);
                defectFilterSetWin.ShowDialog();
            }
        }
    }
}
