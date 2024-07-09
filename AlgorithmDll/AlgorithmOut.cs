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
    /// 2024.7.4 李焕彬
    /// 算法输出管理类
    /// </summary>
    public partial class CAlgorithmOut : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 静态实例
        /// </summary>
        public static CAlgorithmOut s_Instance = new CAlgorithmOut();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层缺陷类
        /// </summary>
        public const string c_SpMaoci = "铝层缺陷类";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区缺陷类
        /// </summary>
        public const string c_SpThick = "料区缺陷类";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 毛刺
        /// </summary>
        public const string c_DeMaoci = "毛刺";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 掉料
        /// </summary>
        public const string c_DeThick = "掉料";

        public CAlgorithmOut()
        {
            SpeciesOut species1 = new SpeciesOut(c_SpMaoci);
            species1.Recipes.Add(new RecipeOut(c_DeMaoci));

            SpeciesOut species2 = new SpeciesOut(c_SpThick);
            species2.Recipes.Add(new RecipeOut(c_DeThick));

            specises.Add(species1);
            specises.Add(species2);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测类集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<SpeciesOut> specises = new ObservableCollection<SpeciesOut>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测类索引器
        /// </summary>
        /// <param name="name">检测类类名</param>
        /// <returns>检测类</returns>
        public SpeciesOut this[string name]
        {
            get
            {
                return Specises.FirstOrDefault(o => o.Name == name);
            }
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 检测类
    /// </summary>
    public partial class SpeciesOut : ObservableObject
    {
        public SpeciesOut(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 类名
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法缺陷集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<RecipeOut> recipes = new ObservableCollection<RecipeOut>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法缺陷索引器
        /// </summary>
        /// <param name="name">算法缺陷名</param>
        /// <returns>算法缺陷</returns>
        public RecipeOut this[string name]
        {
            get
            {
                return Recipes.FirstOrDefault(o => o.Name == name);
            }
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 算法
    /// </summary>
    public partial class RecipeOut : ObservableObject
    {
        public RecipeOut(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法名
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法输出区域
        /// </summary>
        public List<SRegion> Region { get; set; }
    }
}
