using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using QualityGrade;
using SDFilter;
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
    /// 20240706 TCG 启用保存，除了NG TOTAL PERCENT 质量等级和缺陷统计不保存，在反序列化时从DefectFilter/Quality中获取引用
    /// </summary>
    public partial class CDefectsProduce : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计
        /// </summary>
        [property: JsonIgnore]
        [ObservableProperty]
        private ObservableCollection<DefectFilter> defectNumbersList = new();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量统计
        /// </summary>
        [property: JsonIgnore]
        [ObservableProperty]
        private ObservableCollection<Quality> qualityNumbersList = new();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷总数
        /// </summary>
        [ObservableProperty]
        private double ng = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 产品总数
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OKPercent))]
        [NotifyPropertyChangedFor(nameof(NGPercent))]
        private double total = 0;

        /// <summary>
        /// 20240706 TCG
        /// 良率 只读属性binding 需写上Mode = OneWay
        /// </summary>
        [JsonIgnore]
        public double OKPercent
        {
            get
            {
                if (Total <= 0) return 0;
                return OK / Total;
            }
        }
        /// <summary>
        /// 20240706 TCG
        /// 不良率 只读属性binding 需写上Mode = OneWay
        /// </summary>
        [JsonIgnore]
        public double NGPercent
        {
            get
            {
                if (Total <= 0) return 0;
                return Ng / Total;
            }
        }
        /// <summary>
        /// 20200706 TCG
        /// 利用属性通知，只通知一次，多绑定会通知多次
        /// </summary>
        [ObservableProperty]
        public double oK;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 索引器
        /// </summary>
        /// <param name="name">缺陷名</param>
        /// <returns>对应缺陷</returns>
        public DefectFilter this[string name]
        {
            get
            {
                //if (!DefectNumbersList.ToList().Exists(o => o.Name == name))
                //{
                //    DefectNumber defect = new DefectNumber(name);
                //    DefectNumbersList.Add(defect);
                //    return defect;
                //}

                return DefectNumbersList.FirstOrDefault(o => o.Name == name);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计函数
        /// 2024.7.5 TCG 增加质量统计逻辑 先加NG 再加Total，只让Total通知一次
        /// </summary>
        /// <param name="cell">要统计的cell</param>
        public void AddDefectProduce(Cell cell)
        {
            
            if (cell.Detection != null)
            {
                Ng += 1;
                //var currentDefect = this[cell.Detection.Name];
                //currentDefect.Number += 1;
                cell.Detection.DefectFilter.Number += 1;
                foreach (var defect in DefectNumbersList)
                {
                    defect.Percent = (double)defect.Number / Ng;
                }
            }
            else
            {
                OK += 1;
            }
            cell.Quality.Number += 1;
            Total += 1;
            foreach (var defect in DefectNumbersList)
            {
                defect.PercentofAll = (double)defect.Number / Total;
            }

            //QualityNumbersList.FirstOrDefault(o=>o.Name == cell.Quality.Name).Number += 1;//检索过多 界面卡顿

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
            OK = 0;
            Ng = 0;
            Total = 0;
        }
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 缺陷数据
    /// 20240706 TCG 弃用，改用DefectFilter 统计
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
    /// 20240706 TCG 弃用，Quality 统计
    /// 质量数据
    /// </summary>
    public partial class QualityNumber : ObservableObject
    {
        public QualityNumber() { }
        public QualityNumber(Quality qua)
        {
            this.Name = qua.Name;
            this.ShowColor = qua.ShowColor;
            this.QualityLevel = qua.Priority;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数值
        /// </summary>
        [ObservableProperty]
        private int number;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数值
        /// </summary>
        [ObservableProperty]
        private int qualityLevel;

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
        private CKnownColor showColor;
    }
}
