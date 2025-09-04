using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Effects;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using QualityGrade;
using SDFilter;
using WH.Entity.CommonLib;
using WH.RunCell;

namespace ProjProduceData
{
    /// <summary>
    /// 2024.7.3 李焕彬
    /// 缺陷统计数据
    /// 20240706 TCG 启用保存，除了NG TOTAL PERCENT 质量等级和缺陷统计不保存，在反序列化时从DefectFilter/Quality中获取引用
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CDefectsProduce : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计
        /// </summary>
        [property: JsonProperty]
        [ObservableProperty]
        private ObservableCollection<DefectNumber> defectNumbersList = new();

        /// <summary>
        /// 2025.9.1 鲍赞宝
        /// 缺陷统计(排序和排除数量0的项，用于显示）
        /// </summary>
        [property: JsonProperty]
        [ObservableProperty]
        private List<DefectNumber> defectNumbersSortList = new();

        //private ICollectionView sortedView;
        //public ICollectionView SortedView
        //{
        //    get
        //    {
        //        var view = CollectionViewSource.GetDefaultView(DefectNumbersList);
        //        view.Filter = item =>
        //        {
        //            dynamic dataItem = item;
        //            return dataItem.Number != 0;
        //        };
        //        view.SortDescriptions.Clear();
        //        view.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Descending));
        //        return view;
        //        //return sortedView;
        //    }
        //    set
        //    {
        //        sortedView = value;
        //        OnPropertyChanged();
        //    }
        //}

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量统计
        /// </summary>
        [property: JsonProperty]
        [ObservableProperty]
        private ObservableCollection<QualityNumber> qualityNumbersList = new();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷总数
        /// </summary>
        [property: JsonProperty]
        [ObservableProperty]
        private double ng = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 产品总数
        /// </summary>
        [property: JsonProperty]
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
                if (Total <= 0)
                    return 0;
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
                if (Total <= 0)
                    return 0;
                return Ng / Total;
            }
        }

        /// <summary>
        /// 20200706 TCG
        /// 利用属性通知，只通知一次，多绑定会通知多次
        /// </summary>
        [property: JsonProperty]
        [ObservableProperty]
        public double oK;

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
                if (DefectNumbersList.FirstOrDefault(o => o.Name == name) == null)
                {
                    DefectNumbersList.Add(new(name));
                }
                return DefectNumbersList.FirstOrDefault(o => o.Name == name);
            }
        }

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 缺陷配置
        /// </summary>
        List<CFilterConfig> filterConfigs;

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 质量等级
        /// </summary>
        CQualityConfig qualityConfig;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计
        /// </summary>
        /// <param name="filterConfigsNew"></param>
        public void SetFilter(List<CFilterConfig> filterConfigsNew)
        {
            if (this.filterConfigs != null)
            {
                foreach (var item in this.filterConfigs)
                {
                    item.DefectList.CollectionChanged -= DefectList_CollectionChanged;
                    foreach (var defect in item.DefectList)
                    {
                        defect.PropertyChanged -= Defect_PropertyChanged;
                    }
                }
            }
            this.filterConfigs = filterConfigsNew;
            UpdateDefects(true);
            foreach (var item in filterConfigs)
            {
                item.DefectList.CollectionChanged += DefectList_CollectionChanged;
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计
        /// </summary>
        /// <param name="qualityConfigNew"></param>
        public void SetQuality(CQualityConfig qualityConfigNew)
        {
            if (this.qualityConfig != null)
            {
                this.qualityConfig.Qualities.CollectionChanged -= Qualities_CollectionChanged;
            }
            this.qualityConfig = qualityConfigNew;
            UpdateQualitys(true);
            qualityConfig.Qualities.CollectionChanged += Qualities_CollectionChanged;
        }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 更新缺陷，如果是初始化，则需要PropertyChanged
        /// </summary>
        /// <param name="isInit">是否初始化</param>
        public void UpdateDefects(bool isInit = false)
        {
            List<string> defectExists = new List<string>();
            foreach (var config in filterConfigs)
            {
                foreach (var defect in config.DefectList)
                {
                    defectExists.Add(defect.Name);
                    if (DefectNumbersList.FirstOrDefault(o => o.Name == defect.Name) == null)
                    {
                        DefectNumber number = new(defect.Name);
                        DefectNumbersList.Add(number);
                    }
                    if (isInit)
                        defect.PropertyChanged += Defect_PropertyChanged;
                }
            }
            for (int i = DefectNumbersList.Count - 1; i >= 0; i--)
            {
                if (!defectExists.Contains(DefectNumbersList[i].Name))
                {
                    DefectNumbersList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 更新质量等级，如果是初始化，则需要PropertyChanged
        /// </summary>
        /// <param name="isInit">是否初始化</param>
        public void UpdateQualitys(bool isInit = false)
        {
            List<int> qualityExists = new List<int>();
            foreach (var qua in qualityConfig.Qualities)
            {
                qualityExists.Add(qua.Priority);
                QualityNumber qualityNumber = QualityNumbersList.FirstOrDefault(o =>
                    o.Priority == qua.Priority
                );
                if (qualityNumber == null)
                {
                    QualityNumber number = new(qua);
                    QualityNumbersList.Add(number);
                }
                else
                {
                    qualityNumber.Name = qua.Name;
                    qualityNumber.ShowColor = qua.ShowColor;
                }
                if (isInit)
                    qua.PropertyChanged += Qua_PropertyChanged;
            }
            for (int i = QualityNumbersList.Count - 1; i >= 0; i--)
            {
                if (!qualityExists.Contains(QualityNumbersList[i].Priority))
                {
                    QualityNumbersList.RemoveAt(i);
                }
            }
        }

        private void Defect_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateDefects();
            }
        }

        private void Qua_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "ShowColor")
            {
                UpdateQualitys();
            }
        }

        private void DefectList_CollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            UpdateDefects();
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    DefectFilter defect = item as DefectFilter;
                    if (defect != null)
                    {
                        defect.PropertyChanged += Defect_PropertyChanged;
                    }
                }
            }
        }

        private void Qualities_CollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            UpdateQualitys();
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    Quality qua = item as Quality;
                    if (qua != null)
                    {
                        qua.PropertyChanged += Qua_PropertyChanged;
                    }
                }
            }
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

            foreach (var qua in QualityNumbersList)
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
    /// </summary>
    public partial class DefectNumber : ObservableObject
    {
        public DefectNumber() { }

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
            this.Priority = qua.Priority;
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
        private int priority;

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
