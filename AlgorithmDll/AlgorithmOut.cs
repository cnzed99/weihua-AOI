using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.28 李焕彬
    /// 算法输出管理类
    /// </summary>
    public partial class AlgorithmOut : ObservableObject
    {
        public static AlgorithmOut instance = new AlgorithmOut();

        public AlgorithmOut()
        {
            SpeciesOut species1 = new SpeciesOut("铝层缺陷类");
            species1.Recipes.Add(new RecipeOut("毛刺"));

            SpeciesOut species2 = new SpeciesOut("料区缺陷类");
            species2.Recipes.Add(new RecipeOut("掉料"));

            specises.Add(species1);
            specises.Add(species2);
        }

        [ObservableProperty]
        private ObservableCollection<SpeciesOut> specises = new ObservableCollection<SpeciesOut>();
    }

    /// <summary>
    /// 2024.6.28 李焕彬
    /// 检测类
    /// </summary>
    public partial class SpeciesOut : ObservableObject
    {
        public SpeciesOut(string name)
        {
            this.Name = name;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private ObservableCollection<RecipeOut> recipes = new ObservableCollection<RecipeOut>();
    }

    /// <summary>
    /// 2024.6.28 李焕彬
    /// 算法
    /// </summary>
    public partial class RecipeOut : ObservableObject
    {
        public RecipeOut(string name)
        {
            this.Name = name;
        }

        [ObservableProperty]
        private string name;

        public List<SRegion> region;
    }
}
