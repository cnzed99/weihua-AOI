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

        public const string spMaoci = "铝层缺陷类";

        public const string spThick = "料区缺陷类";

        public const string deMaoci = "毛刺";

        public const string deThick = "掉料";

        public AlgorithmOut()
        {
            SpeciesOut species1 = new SpeciesOut(spMaoci);
            species1.Recipes.Add(new RecipeOut(deMaoci));

            SpeciesOut species2 = new SpeciesOut(spThick);
            species2.Recipes.Add(new RecipeOut(deThick));

            specises.Add(species1);
            specises.Add(species2);
        }

        [ObservableProperty]
        private ObservableCollection<SpeciesOut> specises = new ObservableCollection<SpeciesOut>();

        public SpeciesOut this[string name]
        {
            get
            {
                return Specises.FirstOrDefault(o => o.Name == name);
            }
        }
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

        public RecipeOut this[string name]
        {
            get
            {
                return Recipes.FirstOrDefault(o => o.Name == name);
            }
        }
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

        public List<SRegion> Region { get; set; }
    }
}
