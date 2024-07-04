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
        /// 缺陷统计
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<DefectNumber> defectNumbersList = new ObservableCollection<DefectNumber>();

        /// <summary>
        /// 质量统计
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<QualityNumber> qualityNumbersList = new ObservableCollection<QualityNumber>();

        /// <summary>
        /// 缺陷总数
        /// </summary>
        [ObservableProperty]
        private int ng = 0;

        /// <summary>
        /// 产品总数
        /// </summary>
        [ObservableProperty]
        private int total = 0;

        /// <summary>
        /// 缺陷总数/产品总数
        /// </summary>
        [ObservableProperty]
        private double percent;

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
            List<DefectNumber> defectNumbers = DefectNumbersList.ToList();
            defectNumbers.Sort((a, b) => (int)(-a.Number + b.Number));
            for (int i = 0; i < defectNumbers.Count; i++)
            {
                var dex = DefectNumbersList.IndexOf(defectNumbers[i]);
                if (dex == i) continue;
                DefectNumbersList.Move(dex, i);
            }
            
        }

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

    public partial class DefectNumber : ObservableObject
    {
        public DefectNumber() 
        {

        }

        public DefectNumber(string name)
        {
            this.Name = name;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private double percent;

        [ObservableProperty]
        private double percentofAll;
    }

    public partial class QualityNumber : ObservableObject
    {
        public QualityNumber() { }
        public QualityNumber(Quality qua)
        {
            this.Name = qua.Name;
            this.ShowColor = qua.ShowColor;
        }

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private KnownColor showColor;
    }
}
