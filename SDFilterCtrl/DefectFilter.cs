using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mapster;
using Newtonsoft.Json;
using QualityGrade;
using SVGImage.SVG.Filters;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.27 李焕彬
    /// 缺陷筛选管理类
    /// </summary>
    public partial class FilterConfig : ObservableLog, IRecipient<OperateMessage>
    {
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public FilterConfig()
        {
            foreach (var specie in AlgorithmOut.instance.Specises)
            {
                SpeciesFilter speciesFilter = new SpeciesFilter(specie.Name);
                foreach (var recipe in specie.Recipes)
                {
                    speciesFilter.RecipeDefects.Add(new RecipeDefect(recipe.Name));
                }
                SpeciesFilters.Add(speciesFilter);
            }

            SpeciesFilters.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(SpeciesFilters)); };

            WeakReferenceMessenger.Default.Register(this);
        }

        [ObservableProperty]
        private ObservableCollection<SpeciesFilter> speciesFilters = new ObservableCollection<SpeciesFilter>();

        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(FilterConfig))
            {
                OperateLog.Info($"过滤分选-{message.message}");
                return;
            }
            foreach (var sp in SpeciesFilters)
            {
                if(message.obj.GetType() == typeof(SpeciesFilter))
                {
                    if (sp == message.obj)
                    {
                        OperateLog.Info($"过滤分选-{sp.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
                foreach (var re in sp.RecipeDefects)
                {
                    if (message.obj.GetType() == typeof(RecipeDefect))
                    {
                        if(re == message.obj)
                        {
                            OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{message.message}");
                            return;
                        }
                        continue;
                    }
                    foreach (var de in re.DefectFilters)
                    {
                        if (message.obj.GetType() == typeof(DefectFilter))
                        {
                            if (de == message.obj)
                            {
                                OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-{message.message}");
                                return;
                            }
                            continue;
                        }
                        foreach (var fis in de.FilterList)
                        {
                            if (message.obj.GetType() == typeof(FilterAndSelect))
                            {
                                if (fis == message.obj)
                                {
                                    OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-{message.message}");
                                    return;
                                }
                                continue;
                            }
                            foreach (var se in fis.SelectList)
                            {
                                if (message.obj.GetType() == typeof(SelectConfig))
                                {
                                    if (se == message.obj)
                                    {
                                        OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-{message.message}");
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-条件{se.SelectParams.IndexOf(pa)}-{message.message}");
                                        return;
                                    }
                                }
                            }
                            foreach (var fi in fis.Filter)
                            {
                                if (message.obj.GetType() == typeof(SelectConfig))
                                {
                                    if (fi == message.obj)
                                    {
                                        OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-{message.message}");
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in fi.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info($"过滤分选-{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-条件{fi.SelectParams.IndexOf(pa)}-{message.message}");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 2024.6.27 李焕彬
    /// 检测类
    /// </summary>
    public partial class SpeciesFilter : ObservableLog
    {
        public SpeciesFilter(string name) 
        {
            this.Name = name;
            recipeDefects.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(recipeDefects)); };
        }

        [ObservableProperty]
        private string name;

        [JsonIgnore]
        [ObservableProperty]
        private bool result = true;

        [ObservableProperty]
        private ObservableCollection<RecipeDefect> recipeDefects = new ObservableCollection<RecipeDefect>();

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 2024.6.28 李焕彬
    /// 算法缺陷
    /// </summary>
    public partial class RecipeDefect : ObservableLog
    {
        public RecipeDefect(string name)
        {
            this.Name = name;
            DefectFilters.Add(new DefectFilter(Name + "0"));
            defectFilters.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(defectFilters)); };
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private ObservableCollection<DefectFilter> defectFilters = new ObservableCollection<DefectFilter>();

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 2024.6.23 李焕彬
    /// 自定义缺陷
    /// </summary>
    public partial class DefectFilter : ObservableLog
    {
        public DefectFilter(string name)
        {
            this.Name = name;
            foreach (DetectFeature item in Enum.GetValues(typeof(DetectFeature)))
            {
                ResultList.Add(new FilterResult(item));
            }
            ShowColor = BrushPro.instance.KnownColors[new Random().Next(BrushPro.instance.KnownColors.Count - 1)].brush;

            filterList.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(filterList)); };
            resultList.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(resultList)); };
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int priority = 0;

        [ObservableProperty]
        private Brush showColor = null;

        [ObservableProperty]
        private int qualityLevel = 0;

        [ObservableProperty]
        private ObservableCollection<FilterAndSelect> filterList = new ObservableCollection<FilterAndSelect>() { new FilterAndSelect() };

        [JsonIgnore]
        [ObservableProperty]
        private ObservableCollection<FilterResult> resultList = new ObservableCollection<FilterResult>() { };

        public override string ToString()
        {
            return Name;
        }

    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 过滤、分选参数集
    /// </summary>
    public partial class FilterAndSelect : ObservableLog
    {
        public FilterAndSelect()
        {
            filter.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(filter)); };
            selectList.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(selectList)); };
        }

        [JsonIgnore]
        [ObservableProperty]
        private bool result = true;

        [ObservableProperty]
        private string unionOrConnect = "不打散不合并";

        [ObservableProperty]
        private ObservableCollection<SelectConfig> filter = new ObservableCollection<SelectConfig>() { new SelectConfig()};

        [ObservableProperty]
        private ObservableCollection<SelectConfig> selectList= new ObservableCollection<SelectConfig>() { new SelectConfig()};

        public override string ToString()
        {
            return $"过滤分选器->过滤：{string.Join("||",Filter)}，分选：{string.Join("||", SelectList)}";
        }

    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 选择参数集
    /// </summary>
    public partial class SelectConfig : ObservableLog
    {
        public SelectConfig()
        {
            selectParams.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(selectParams)); };
        }

        [ObservableProperty]
        private ObservableCollection<OneSelectParams> selectParams = new ObservableCollection<OneSelectParams>() { new OneSelectParams()};

        public override string ToString()
        {
            return "条件集->" + string.Join("&&", SelectParams);
        }
    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 选择参数
    /// </summary>
    public partial class OneSelectParams : ObservableLog
    {
        [ObservableProperty]
        private DetectFeature character = DetectFeature.顶点高度;

        [ObservableProperty]
        private double min = 1.0;

        [ObservableProperty]
        private double max = double.PositiveInfinity;

        [ObservableProperty]
        private bool maxLimit = true;

        [ObservableProperty]
        private bool minLimit = true;

        public override string ToString()
        {
            if (MaxLimit && MinLimit)
            {
                return $"{Min}≤{Character}≤{Max}";
            }
            else if (MinLimit)//限制最小
            {
                return $"{Min}≤{Character}≤{double.PositiveInfinity}";
            }
            else if (MaxLimit)//限制最大
            {
                return $"{double.NegativeInfinity}≤{Character}≤{Max}";
            }
            else//都不限制
            {
                return $"{double.NegativeInfinity}≤{Character}≤{double.PositiveInfinity}";
            }
        }

        /// <summary>
        /// 2024.6.27 李焕彬
        /// 判断特征值是否在最大最小值限定范围内
        /// </summary>
        /// <param name="value">特征值</param>
        /// <returns>在范围内为true，否则为false</returns>
        private bool Excute(double value)
        {
            if (MinLimit && MaxLimit)
            {
                if (value >= Min && value <= Max)
                {
                    return true;
                }

            }
            else if (MinLimit)//限制最小
            {
                if (value >= Min)
                {
                    return true;
                }
            }
            else if (MaxLimit)//限制最大
            {
                if (value <= Max)
                {
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// 2024.6.27 李焕彬
        /// 筛选所有特征值在最大最小值限定范围内的区域
        /// </summary>
        /// <param name="sRegionIn">待筛选区域</param>
        /// <param name="sRegionOut">在限定范围内的区域</param>
        /// <returns>在限定范围内的区域数量为0时为true，否则为false</returns>
        public bool Excute(List<SRegion> sRegionIn, out List<SRegion> sRegionOut)
        {
            sRegionOut = new List<SRegion>();
            switch (Character)
            {
                case DetectFeature.顶点高度:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.dPeakHeight));
                    break;
                case DetectFeature.面积:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.nArea));
                    break;
                case DetectFeature.长边:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.dLongLen));
                    break;
                case DetectFeature.短边:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.dShorLen));
                    break;
                case DetectFeature.角度:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.dPhi));
                    break;
                case DetectFeature.周长:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.dContLen));
                    break;
                case DetectFeature.宽度:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.nWidth));
                    break;
                case DetectFeature.高度:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.regionInfo.nHeight));
                    break;
                case DetectFeature.数量:
                    if (Excute(sRegionIn.Count)) sRegionOut = sRegionIn;
                    break;
            }
            return sRegionOut.Count == 0;
        }

    }

    public partial class FilterResult : ObservableObject
    {
        public FilterResult(DetectFeature detectFeature) 
        {
            feature = detectFeature;
        }

        [ObservableProperty]
        private DetectFeature feature;

        [ObservableProperty]
        private double value;
    }

    public enum DetectFeature
    {
        顶点高度,
        面积,
        长边,
        短边,
        角度,
        周长,
        宽度,
        高度,
        数量
    }
}
