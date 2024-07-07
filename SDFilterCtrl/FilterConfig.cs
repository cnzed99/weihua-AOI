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
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.27 李焕彬
    /// 缺陷筛选管理类
    /// </summary>
    public partial class FilterConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.2 李焕彬
        /// 操作日志
        /// </summary>
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public FilterConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
            var SpFilters = new ObservableCollection<SpeciesFilter>();
            foreach (var specie in AlgorithmOut.s_Instance.Specises)
            {
                SpeciesFilter speciesFilter = new SpeciesFilter(specie.Name);
                foreach (var recipe in specie.Recipes)
                {
                    speciesFilter.RecipeDefects.Add(new RecipeDefect(recipe.Name));
                }
                SpFilters.Add(speciesFilter);
            }
            SpeciesFilters = SpFilters;
           
            //WeakReferenceMessenger.Default.Register<OperateMessage, Token>(this, token);
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 类别列表
        /// </summary>
        [property: DisplayName("类别列表")]
        [ObservableProperty]
        private ObservableCollection<SpeciesFilter> speciesFilters;

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 索引器
        /// </summary>
        /// <param name="name">类名</param>
        /// <returns></returns>
        public SpeciesFilter this[string name]
        {
            get
            {
                return SpeciesFilters.FirstOrDefault(s => s.Name == name);
            }
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(FilterConfig))
            {
                OperateLog.Info($"检测设置：{message.message}");
                return;
            }
            foreach (var sp in SpeciesFilters)
            {
                if(message.obj.GetType() == typeof(SpeciesFilter))
                {
                    if (sp == message.obj)
                    {
                        OperateLog.Info($"类别：{sp.Name}-{message.message}");
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
                            OperateLog.Info($"类别：{sp.Name}-{re.Name}-{message.message}");
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
                                OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-{message.message}");
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
                                    OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-{message.message}");
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
                                        OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-{message.message}");
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-条件{se.SelectParams.IndexOf(pa)}-{message.message}");
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
                                        OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-{message.message}");
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in fi.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info($"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-条件{fi.SelectParams.IndexOf(pa)}-{message.message}");
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
            var AlgorithmOut = cell.MaociTestOut.AlgorithmOut;
            //int deIndex = 0;
            foreach (var sp in AlgorithmOut.Specises)
            {
                foreach (var rp in sp.Recipes)
                {
                    foreach (var de in this[sp.Name][rp.Name].DefectFilters)//缺陷
                    {
                        //deIndex++;
                        CellDetection detection = new CellDetection();
                        //detection.Name = de.Name;
                        detection.Type = sp.Name;
                        detection.RecipeDefectName = rp.Name;
                        //detection.Priority = de.Priority;
                        //detection.Quality = de.QualityLevel;//质量等级
                        detection.DefectFilter = de;//缺陷过滤器
                        //detection.Index = deIndex;
                        //detection.ShowColor = de.ShowColor;
                        if (cell.CancelSource.IsCancellationRequested) return;//任务取消时退出
                        foreach (var filter in de.FilterList)//过滤分选器
                        {
                            List<SRegion> detectRegion = AlgorithmOut[sp.Name][rp.Name].Region;
                            switch (filter.UnionOrConnect)
                            {
                                case "区域合并":
                                    SRegionInfo regionInfo = new SRegionInfo();
                                    regionInfo.Width = detectRegion.Select(o => o.RegionInfo.Width).Sum();
                                    regionInfo.Height = detectRegion.Select(o => o.RegionInfo.Height).Sum();
                                    regionInfo.PeakHeight = detectRegion.Select(o => o.RegionInfo.PeakHeight).Sum();
                                    regionInfo.LongLen = detectRegion.Select(o => o.RegionInfo.LongLen).Sum();
                                    regionInfo.ShorLen = detectRegion.Select(o => o.RegionInfo.ShorLen).Sum();
                                    regionInfo.Phi = detectRegion.Select(o => o.RegionInfo.Phi).Max();
                                    regionInfo.ContLen = detectRegion.Select(o => o.RegionInfo.ContLen).Sum();
                                    regionInfo.Area = detectRegion.Select(o => o.RegionInfo.Area).Sum();
                                    foreach (SRegion region in detectRegion)
                                    {
                                        region.RegionInfo.Copy(regionInfo);
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
                                    if (selParam.Character == EMFILTER.EMFILTER_NUM)
                                        oneSelectParams = selParam;
                                    else
                                        bResult = selParam.Excute(selRegion, out selRegion);//&&
                                }
                                if (oneSelectParams != null) bResult = oneSelectParams.Excute(selRegion, out selRegion);//数量判断
                                if (!bResult)
                                {
                                    detection.regionOut = selRegion;
                                    detection.DetectLog.AppendLine(detection.DefectFilter.Name);
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
                        List<EMFILTER> lsParam = new List<EMFILTER>();
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
                                    case EMFILTER.EMFILTER_PEAKHEI:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.PeakHeight).Max();
                                        break;
                                    case EMFILTER.EMFILTER_AREA:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.Area).Max();
                                        break;
                                    case EMFILTER.EMFILTER_LONGLEN:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.LongLen).Max();
                                        break;
                                    case EMFILTER.EMFILTER_SHORTLEN:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.ShorLen).Max();
                                        break;
                                    case EMFILTER.EMFILTER_PHI:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.Phi).Max();
                                        break;
                                    case EMFILTER.EMFILTER_CONTLEN:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.ContLen).Max();
                                        break;
                                    case EMFILTER.EMFILTER_WIDTH:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.Width).Max();
                                        break;
                                    case EMFILTER.EMFILTER_HEIGHT:
                                        de.ResultList[i].Value = detection.regionOut.Select(o => o.RegionInfo.Height).Max();
                                        break;
                                    case EMFILTER.EMFILTER_NUM:
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
                        if (!detection.Result)//NG
                        {
                            var qualityLevel = detection.DefectFilter.QualityLevel;
                            if (cell.Detection == null)
                            {
                                cell.Detection = detection;
                                cell.Quality = detection.DefectFilter.Quality;
                            }
                            else
                            {
                                if (cell.Detection.DefectFilter.QualityLevel < qualityLevel)//质量等级 还需判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.Quality = detection.DefectFilter.Quality;
                                }
                                else if (cell.Detection.DefectFilter.QualityLevel == qualityLevel && cell.Detection?.DefectFilter.Priority < detection.DefectFilter.Priority)//质量等级相等时 判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.Quality = detection.DefectFilter.Quality;
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
    public partial class SpeciesFilter : ConfigModifyObservableBase
    {
        public SpeciesFilter()
        {
            this.token = new Token("", this.GetType().Namespace);
            RecipeDefects = new ObservableCollection<RecipeDefect>();
        }
        public SpeciesFilter(string name) :this()
        {
            this.Name = name;
           
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名称
        /// </summary>
        [property: DisplayName("名称")]
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测结果
        /// </summary>
        [JsonIgnore]
        [ObservableProperty]
        private bool result = true;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷列表
        /// </summary>
        [property: DisplayName("缺陷列表")]
        [ObservableProperty]
        private ObservableCollection<RecipeDefect> recipeDefects;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 索引器
        /// </summary>
        /// <param name="name">算法名</param>
        /// <returns></returns>
        public RecipeDefect this[string name]
        {
            get
            {
                return RecipeDefects.FirstOrDefault(x => x.Name == name);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 2024.6.28 李焕彬
    /// 算法缺陷
    /// </summary>
    public partial class RecipeDefect : ConfigModifyObservableBase
    {
        public RecipeDefect()
        {
            this.token = new Token("", this.GetType().Namespace);
            DefectFilters = new ObservableCollection<DefectFilter>();
        }
        public RecipeDefect(string name):this()
        {
            this.Name = name;
            DefectFilters.Add(new DefectFilter(Name + "0"));
          
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名称
        /// </summary>
        [property: DisplayName("名称")]
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷列表
        /// </summary>
        [property: DisplayName("缺陷列表")]
        [ObservableProperty]
        private ObservableCollection<DefectFilter> defectFilters;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 索引器
        /// </summary>
        /// <param name="name">缺陷名</param>
        /// <returns></returns>
        public DefectFilter this[string name]
        {
            get
            {
                return DefectFilters.FirstOrDefault(x => x.Name == name);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 2024.6.23 李焕彬
    /// 自定义缺陷
    /// </summary>
    public partial class DefectFilter : ConfigModifyObservableBase
    {
        public DefectFilter()
        {
            this.token = new Token("", this.GetType().Namespace);
            foreach (EMFILTER item in Enum.GetValues(typeof(EMFILTER)))
            {
                ResultList.Add(new FilterResult(item));
            }
            FilterList = new ObservableCollection<FilterAndSelect>() { new FilterAndSelect() };
        }
        public DefectFilter(string name):this()
        {
            this.Name = name;
           
            ShowColor = BrushPro.s_Instance.KnownColors[new Random().Next(BrushPro.s_Instance.KnownColors.Count - 1)];
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名称
        /// </summary>
        [property: DisplayName("名称")]
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 优先级
        /// </summary>
        [property: DisplayName("优先级")]
        [ObservableProperty]
        private int priority = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 颜色
        /// </summary>
        [property: DisplayName("颜色")]
        [ObservableProperty]
        private KnownColor showColor = null;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级
        /// </summary>
        [property: DisplayName("质量等级")]
        [ObservableProperty]
        private Quality qualityLevel;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤列表
        /// </summary>
        [property: DisplayName("过滤列表")]
        [ObservableProperty]
        private ObservableCollection<FilterAndSelect> filterList;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 结果列表
        /// </summary>
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private ObservableCollection<FilterResult> resultList = new ObservableCollection<FilterResult>() { };

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// 20240705 TCG
        /// 当前缺陷 产出
        /// </summary>
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private int number;

        /// <summary>
        /// 20240705 TCG
        /// 占所有缺陷比
        /// </summary>
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private double percent;

        /// <summary>
        /// 20240705 TCG
        /// 占所有检测数比
        /// </summary>
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private double percentofAll;
    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 过滤、分选参数集
    /// </summary>
    public partial class FilterAndSelect : ConfigModifyObservableBase
    {
        public FilterAndSelect()
        {
            this.token = new Token("", this.GetType().Namespace);
            Filter = new ObservableCollection<SelectConfig>() { new SelectConfig() };
            SelectList = new ObservableCollection<SelectConfig>() { new SelectConfig() };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测结果
        /// </summary>
        [JsonIgnore]
        [ObservableProperty]
        private bool result = true;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 输入操作
        /// </summary>
        [property: DisplayName("输入操作")]
        [ObservableProperty]
        private string unionOrConnect = "不打散不合并";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤器
        /// </summary>
        [property: DisplayName("过滤器")]
        [ObservableProperty]
        private ObservableCollection<SelectConfig> filter;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 分选器
        /// </summary>
        [property: DisplayName("分选器")]
        [ObservableProperty]
        private ObservableCollection<SelectConfig> selectList;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"过滤分选器->过滤：{string.Join("||",Filter)}，分选：{string.Join("||", SelectList)}";
        }

    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 选择参数集
    /// </summary>
    public partial class SelectConfig : ConfigModifyObservableBase
    {
        public SelectConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
            SelectParams = new ObservableCollection<OneSelectParams>() { new OneSelectParams() };
           
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 筛选条目
        /// </summary>
        [property: DisplayName("筛选条目")]
        [ObservableProperty]
        private ObservableCollection<OneSelectParams> selectParams;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "条件集->" + string.Join("&&", SelectParams);
        }
    }
    /// <summary>
    /// 2024.6.23 李焕彬
    /// 选择参数
    /// </summary>
    public partial class OneSelectParams : ConfigModifyObservableBase
    {
        public OneSelectParams()
        {
            this.token = new Token("", this.GetType().Namespace);
        }
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 特征
        /// </summary>
        [property: DisplayName("特征")]
        [ObservableProperty]
        private EMFILTER character = EMFILTER.EMFILTER_PEAKHEI;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最小值
        /// </summary>
        [property: DisplayName("最小值")]
        [ObservableProperty]
        private double min = 1.0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 最大值
        /// </summary>
        [property: DisplayName("最大值")]
        [ObservableProperty]
        private double max = double.PositiveInfinity;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 上限
        /// </summary>
        [property: DisplayName("上限")]
        [ObservableProperty]
        private bool maxLimit = true;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 下限
        /// </summary>
        [property:DisplayName("下限")]
        [ObservableProperty]
        private bool minLimit = true;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string characterName = EnumStringAttribute.GetEnumName(Character);

            if (MaxLimit && MinLimit)
            {
                return $"{Min}≤{characterName}≤{Max}";
            }
            else if (MinLimit)//限制最小
            {
                return $"{Min}≤{characterName}≤{double.PositiveInfinity}";
            }
            else if (MaxLimit)//限制最大
            {
                return $"{double.NegativeInfinity}≤{characterName}≤{Max}";
            }
            else//都不限制
            {
                return $"{double.NegativeInfinity}≤{characterName}≤{double.PositiveInfinity}";
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
                case EMFILTER.EMFILTER_PEAKHEI:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.PeakHeight));
                    break;
                case EMFILTER.EMFILTER_AREA:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.Area));
                    break;
                case EMFILTER.EMFILTER_LONGLEN:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.LongLen));
                    break;
                case EMFILTER.EMFILTER_SHORTLEN:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.ShorLen));
                    break;
                case EMFILTER.EMFILTER_PHI:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.Phi));
                    break;
                case EMFILTER.EMFILTER_CONTLEN:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.ContLen));
                    break;
                case EMFILTER.EMFILTER_WIDTH:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.Width));
                    break;
                case EMFILTER.EMFILTER_HEIGHT:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.Height));
                    break;
                case EMFILTER.EMFILTER_NUM:
                    if (Excute(sRegionIn.Count)) sRegionOut = sRegionIn;
                    break;
            }
            return sRegionOut.Count == 0;
        }

    }

    /// <summary>
    /// 2024.6.27 李焕彬
    /// 检测结果
    /// </summary>
    public partial class FilterResult : ObservableObject
    {
        public FilterResult()
        {
            
        }
        public FilterResult(EMFILTER detectFeature) 
        {
            feature = detectFeature;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 特征
        /// </summary>
        [ObservableProperty]
        private EMFILTER feature;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测结果值
        /// </summary>
        [ObservableProperty]
        private double value;
    }

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 特征类型
    /// </summary>
    public enum EMFILTER
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 顶点高度
        /// </summary>
        [EnumString("顶点高度","PeakHeight")]
        EMFILTER_PEAKHEI,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 面积
        /// </summary>
        [EnumString("面积", "Area")]
        EMFILTER_AREA,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 长边
        /// </summary>
        [EnumString("长边", "LongLength")]
        EMFILTER_LONGLEN,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 短边
        /// </summary>
        [EnumString("短边", "ShortLength")]
        EMFILTER_SHORTLEN,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 角度
        /// </summary>
        [EnumString("角度", "Angle")]
        EMFILTER_PHI,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 周长
        /// </summary>
        [EnumString("周长", "ContLength")]
        EMFILTER_CONTLEN,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 宽度
        /// </summary>
        [EnumString("宽度", "Width")]
        EMFILTER_WIDTH,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 高度
        /// </summary>
        [EnumString("高度", "Height")]
        EMFILTER_HEIGHT,
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 数量
        /// </summary>
        [EnumString("数量", "Num")]
        EMFILTER_NUM
    }
}
