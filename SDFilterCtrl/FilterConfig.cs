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
using WH.RunCell;

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

            WeakReferenceMessenger.Default.Register<OperateMessage, string>(this, this.GetType().Namespace);
        }

        [ObservableProperty]
        private ObservableCollection<SpeciesFilter> speciesFilters = new ObservableCollection<SpeciesFilter>();

        public SpeciesFilter this[string name]
        {
            get
            {
                return SpeciesFilters.FirstOrDefault(s => s.Name == name);
            }
        }

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

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 过滤分选，结果输出在cell里面
        ///</summary>
        /// <param name="cell"></param>
        public void Excute(Cell cell)
        {
            if (cell._skipthis)
            {
                return;
            }
            var AlgorithmOut = cell.AlgorithmOut.AlgorithmOut;

            foreach (var sp in AlgorithmOut.Specises)
            {
                foreach (var rp in sp.Recipes)
                {
                    foreach (var de in this[sp.Name][rp.Name].DefectFilters)//缺陷
                    {
                        CellDetection detection = new CellDetection();
                        detection.Name = de.Name;
                        detection.Type = sp.Name;
                        detection.RecipeDefectName = rp.Name;
                        detection.Priority = de.Priority;
                        detection.QualityLevel = de.QualityLevel;
                        detection.ShowColor = de.ShowColor;
                        if (cell.CancelSource.IsCancellationRequested) return;//任务取消时退出
                        foreach (var filter in de.FilterList)//过滤分选器
                        {
                            List<SRegion> detectRegion = AlgorithmOut[sp.Name][rp.Name].Region;
                            switch (filter.UnionOrConnect)
                            {
                                case "区域合并":
                                    SRegionInfo regionInfo = new SRegionInfo();
                                    regionInfo.nWidth = detectRegion.Select(o => o.regionInfo.nWidth).Sum();
                                    regionInfo.nHeight = detectRegion.Select(o => o.regionInfo.nHeight).Sum();
                                    regionInfo.dPeakHeight = detectRegion.Select(o => o.regionInfo.dPeakHeight).Sum();
                                    regionInfo.dLongLen = detectRegion.Select(o => o.regionInfo.dLongLen).Sum();
                                    regionInfo.dShorLen = detectRegion.Select(o => o.regionInfo.dShorLen).Sum();
                                    regionInfo.dPhi = detectRegion.Select(o => o.regionInfo.dPhi).Max();
                                    regionInfo.dContLen = detectRegion.Select(o => o.regionInfo.dContLen).Sum();
                                    regionInfo.nArea = detectRegion.Select(o => o.regionInfo.nArea).Sum();
                                    foreach (SRegion region in detectRegion)
                                    {
                                        region.regionInfo.Copy(regionInfo);
                                    }
                                    break;
                            }

                            List<SRegion> filterOuts = new List<SRegion>();//过滤后的区域
                            foreach (var select in filter.Filter)//过滤
                            {
                                List<SRegion> selRegion = detectRegion;
                                foreach (var selParam in select.SelectParams)
                                {
                                    selParam.Excute(selRegion, out selRegion);//&&
                                }
                                filterOuts.AddRange(selRegion);//||
                            }
                            foreach (var select in filter.SelectList)//分选
                            {
                                List<SRegion> selRegion = filterOuts;
                                bool bResult = true;
                                OneSelectParams oneSelectParams = null;//若有数量判断，则留到分选完后
                                foreach (var selParam in select.SelectParams)
                                {
                                    if (selParam.Character == DetectFeature.数量)
                                        oneSelectParams = selParam;
                                    else
                                        bResult = selParam.Excute(selRegion, out selRegion);//&&
                                }
                                if (oneSelectParams != null) bResult = oneSelectParams.Excute(selRegion, out selRegion);//数量判断
                                if (!bResult)
                                {
                                    detection.regionOut = selRegion;
                                    detection.DetectLog.AppendLine(detection.Name);
                                    detection.DetectLog.AppendLine($"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}");
                                    detection.Result = false;
                                    break;//有一个分选不合格就跳出，不执行剩下的分选（||）
                                }
                            }
                            //有一个过滤分选器不合格就跳出，不执行剩下的过滤分选器（||）
                            if (!detection.Result)
                            {
                                filter.Result = false;
                                this[sp.Name].Result = false;
                                break;
                            }
                        }

                        //DetectLog增加分选的类型
                        List<DetectFeature> lsParam = new List<DetectFeature>();
                        foreach (var filter in de.FilterList)
                        {
                            foreach (var select in filter.SelectList)
                            {
                                foreach (var selParam in select.SelectParams)
                                {
                                    if (!lsParam.Contains(selParam.Character))
                                    {
                                        detection.DetectLog.AppendLine($"{selParam.Character}:");
                                        lsParam.Add(selParam.Character);
                                    }
                                }
                            }
                        }

                        //检测区显示
                        for (int i = de.ResultList.Count - 1; i >= 0; i--)
                        {
                            if (lsParam.Contains(de.ResultList[i].Feature))
                            {
                                switch (de.ResultList[i].Feature)
                                {
                                    case DetectFeature.顶点高度:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.dPeakHeight).Max();
                                        break;
                                    case DetectFeature.面积:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.nArea).Max();
                                        break;
                                    case DetectFeature.长边:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.dLongLen).Max();
                                        break;
                                    case DetectFeature.短边:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.dShorLen).Max();
                                        break;
                                    case DetectFeature.角度:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.dPhi).Max();
                                        break;
                                    case DetectFeature.周长:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.dContLen).Max();
                                        break;
                                    case DetectFeature.宽度:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.nWidth).Max();
                                        break;
                                    case DetectFeature.高度:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.regionInfo.nHeight).Max();
                                        break;
                                    case DetectFeature.数量:
                                        de.ResultList[i].Value = detection.regionOut.Count;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                de.ResultList.RemoveAt(i);
                            }
                        }
                        if (!detection.Result)
                        {
                            var qualityLevel = detection.QualityLevel;
                            if (cell.Detection == null)
                            {
                                cell.Detection = detection;
                                cell.QualityLevel = detection.QualityLevel;
                            }
                            else
                            {
                                if (cell.QualityLevel < detection.QualityLevel)//质量等级 还需判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.QualityLevel = detection.QualityLevel;
                                }
                                else if (cell.QualityLevel == detection.QualityLevel && cell.Detection?.Priority < detection.Priority)//质量等级相等时 判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.QualityLevel = detection.QualityLevel;
                                }
                            }
                        }
                        cell.Detections.Add(detection);
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
        public SpeciesFilter()
        {
            
        }
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

        public RecipeDefect this[string name]
        {
            get
            {
                return RecipeDefects.FirstOrDefault(x => x.Name == name);
            }
        }

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
        public RecipeDefect()
        {
            
        }
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

        public DefectFilter this[string name]
        {
            get
            {
                return DefectFilters.FirstOrDefault(x => x.Name == name);
            }
        }

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
        public DefectFilter()
        {
            
        }
        public DefectFilter(string name)
        {
            this.Name = name;
            foreach (DetectFeature item in Enum.GetValues(typeof(DetectFeature)))
            {
                ResultList.Add(new FilterResult(item));
            }
            ShowColor = BrushPro.instance.KnownColors[new Random().Next(BrushPro.instance.KnownColors.Count - 1)];

            filterList.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(filterList)); };
            resultList.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(resultList)); };
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int priority = 0;

        [ObservableProperty]
        private KnownColor showColor = null;

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
        public FilterResult()
        {
            
        }
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
