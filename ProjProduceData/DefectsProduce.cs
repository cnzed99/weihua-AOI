using CommunityToolkit.Mvvm.ComponentModel;
using QualityGrade;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.CommonLib;
using WH.RunCell;

namespace ProjProduceData
{
    /// <summary>
    /// 2024.7.3 李焕彬
    /// 缺陷统计数据
    /// </summary>
    public partial class DefectsProduce : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<DefectNumber> defectNumbersList = new ObservableCollection<DefectNumber>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量统计
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<QualityNumber> qualityNumbersList = new ObservableCollection<QualityNumber>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷总数
        /// </summary>
        [ObservableProperty]
        private int ng = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 产品总数
        /// </summary>
        [ObservableProperty]
        private int total = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷总数/产品总数
        /// </summary>
        [ObservableProperty]
        private double percent;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 索引器
        /// </summary>
        /// <param name="name">缺陷名</param>
        /// <returns>对应缺陷</returns>
        public DefectNumber this[string name]
        {
            get
            {
                if (!DefectNumbersList.ToList().Exists(o => o.Name == name))
                {
                    DefectNumber defect = new DefectNumber(name);
                    DefectNumbersList.Add(defect);
                    return defect;
                }

                return DefectNumbersList.FirstOrDefault(o => o.Name == name);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计函数
        /// </summary>
        /// <param name="cell">要统计的cell</param>
        public void AddDefectProduce(Cell cell)
        {
            Total += 1;
            if (cell.Detection != null)
            {
                Ng += 1;
                var currentDefect = this[cell.Detection.Name];
                currentDefect.Number += 1;
                foreach (var defect in DefectNumbersList)
                {
                    defect.Percent = defect.Number / Ng;
                }
            }
            foreach (var defect in DefectNumbersList)
            {
                defect.PercentofAll = defect.Number / Total;
            }
            //排序
            //List<DefectNumber> defectNumbers = DefectNumbersList.ToList();
            //defectNumbers.Sort((a, b) => (int)(-a.Number + b.Number));
            //for (int i = 0; i < defectNumbers.Count; i++)
            //{
            //    var dex = DefectNumbersList.IndexOf(defectNumbers[i]);
            //    if (dex == i) continue;
            //    DefectNumbersList.Move(dex, i);
            //}
            
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            foreach (var defect in DefectNumbersList)
            {
                defect.Number = 0;
                defect.Percent = 0;
                defect.PercentofAll = 0;
            }

            foreach(var qua in QualityNumbersList)
            {
                qua.Number = 0;
            }

            Total = 0;
            Ng = 0;
            Percent = 0;
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 缺陷数据
    /// </summary>
    public partial class DefectNumber : ObservableObject
    {
        public DefectNumber() 
        {

        }

        public DefectNumber(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名字
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数值
        /// </summary>
        [ObservableProperty]
        private int number;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 占所有缺陷比
        /// </summary>
        [ObservableProperty]
        private double percent;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 占所有检测数比
        /// </summary>
        [ObservableProperty]
        private double percentofAll;
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 质量数据
    /// </summary>
    public partial class QualityNumber : ObservableObject
    {
        public QualityNumber() { }
        public QualityNumber(Quality qua)
        {
            this.Name = qua.Name;
            this.ShowColor = qua.ShowColor;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数值
        /// </summary>
        [ObservableProperty]
        private int number;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名字
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 显示颜色
        /// </summary>
        [ObservableProperty]
        private KnownColor showColor;
    }
}
