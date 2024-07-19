using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mapster;
using Newtonsoft.Json;
using QualityGrade;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace SDFilter
{
    /// <summary>
    /// 2024.6.27 李焕彬
    /// 缺陷筛选管理类
    /// </summary>
    public partial class CFilterConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.2 李焕彬
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 20240719 TCG
        /// 缺陷列表，外部引用较多
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public ObservableCollection<DefectFilter> DefectList { get; set; } =
            new ObservableCollection<DefectFilter>();

        public CFilterConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
            var SpFilters = new ObservableCollection<SpeciesFilter>();
            foreach (var specie in CAlgorithmOut.s_Instance.Specises)
            {
                SpeciesFilter speciesFilter = new SpeciesFilter(specie.Name);
                foreach (var recipe in specie.Recipes)
                {
                    speciesFilter.RecipeDefects.Add(new RecipeDefect(recipe.Name, token));
                }
                SpFilters.Add(speciesFilter);
            }
            SpeciesFilters = SpFilters;
        }

        public void SetSDFilterVM(CQualityConfig qualityConfig)
        {
            Synchronization(qualityConfig);
            UpdateDefectList();

            SpeciesFilters.CollectionChanged += (s, e) =>
            {
                UpdateDefectList();
            };
            foreach (var sp in SpeciesFilters)
            {
                sp.RecipeDefects.CollectionChanged += (s, e) =>
                {
                    UpdateDefectList();
                };
                foreach (var rd in sp.RecipeDefects)
                {
                    rd.DefectFilters.CollectionChanged += (s, e) =>
                    {
                        UpdateDefectList();
                    };
                }
            }
        }

        /// <summary>
        /// 20240715 TCG
        /// 同步毛刺等级实例
        /// </summary>
        /// <param name="MaociQuality"></param>
        protected void Synchronization(CQualityConfig MaociQuality)
        {
            #region 同步毛刺过滤配置
            foreach (var spFilter in SpeciesFilters)
            {
                foreach (var reFilger in spFilter.RecipeDefects)
                {
                    foreach (var deFilter in reFilger.DefectFilters)
                    {
                        //新建配方 质量等级没有赋值时赋值最差
                        if (deFilter.QualityLevel is null)
                        {
                            deFilter.QualityLevel = MaociQuality.Qualities.Last();
                        }
                        else
                        {
                            var findquality = MaociQuality.Qualities.FirstOrDefault(o =>
                                o.Priority == deFilter.QualityLevel.Priority
                            );
                            deFilter.QualityLevel = null;
                            deFilter.QualityLevel = findquality;
                        }
                    }
                }
            }
            #endregion
        }

        protected void UpdateDefectList()
        {
            List<string> strings = new List<string>();
            foreach (var sp in SpeciesFilters)
            {
                foreach (var rp in sp.RecipeDefects)
                {
                    foreach (var de in rp.DefectFilters)
                    {
                        if (!DefectList.Contains(de))
                        {
                            DefectList.Add(de);
                        }
                        strings.Add(de.Name);
                    }
                }
            }
            for (int i = DefectList.Count - 1; i >= 0; i--)
            {
                if (!strings.Contains(DefectList[i].Name))
                {
                    DefectList.RemoveAt(i);
                }
            }
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
            get { return SpeciesFilters.FirstOrDefault(s => s.Name == name); }
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CFilterConfig))
            {
                OperateLog.Info($"检测设置：{message.message}");
                return;
            }
            foreach (var sp in SpeciesFilters)
            {
                if (message.obj.GetType() == typeof(SpeciesFilter))
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
                        if (re == message.obj)
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
                                OperateLog.Info(
                                    $"类别：{sp.Name}-{re.Name}-{de.Name}-{message.message}"
                                );
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
                                    OperateLog.Info(
                                        $"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-{message.message}"
                                    );
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
                                        OperateLog.Info(
                                            $"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-{message.message}"
                                        );
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info(
                                            $"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-分选{fis.SelectList.IndexOf(se)}-条件{se.SelectParams.IndexOf(pa)}-{message.message}"
                                        );
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
                                        OperateLog.Info(
                                            $"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-{message.message}"
                                        );
                                        return;
                                    }
                                    continue;
                                }
                                foreach (var pa in fi.SelectParams)
                                {
                                    if (pa == message.obj)
                                    {
                                        OperateLog.Info(
                                            $"类别：{sp.Name}-{re.Name}-{de.Name}-过滤分选器{de.FilterList.IndexOf(fis)}-过滤{fis.Filter.IndexOf(fi)}-条件{fi.SelectParams.IndexOf(pa)}-{message.message}"
                                        );
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
    public partial class SpeciesFilter : ConfigModifyObservableBase
    {
        public SpeciesFilter()
        {
            this.token = new Token("", this.GetType().Namespace);
            RecipeDefects = new ObservableCollection<RecipeDefect>();
        }

        public SpeciesFilter(string name)
            : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名称 放置基类中
        /// </summary>
        //[property: DisplayName("名称")]
        //[ObservableProperty]
        //private string name;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测结果
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
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
            get { return RecipeDefects.FirstOrDefault(x => x.Name == name); }
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
        }

        public RecipeDefect(string name, Token token)
        {
            this.token = token;
            this.Name = name;
            DefectFilters = new ObservableCollection<DefectFilter>()
            {
                new DefectFilter(Name + "0", token)
            };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 名称 放置基类中
        /// </summary>
        //[property: DisplayName("名称")]
        //[ObservableProperty]
        //private string name;

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
            get { return DefectFilters.FirstOrDefault(x => x.Name == name); }
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
        }

        public DefectFilter(string name, Token token)
        {
            this.token = token;
            this.Name = name;
            FilterList = new ObservableCollection<FilterAndSelect>() { new FilterAndSelect(token) };
            ResultList.Add(new FilterResult(EMFILTER.EMFILTER_PEAKHEI));
            ShowColor = CBrushPro.s_Instance.KnownColors[
                new Random().Next(CBrushPro.s_Instance.KnownColors.Count - 1)
            ];
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
        private CKnownColor showColor = null;

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
        private ObservableCollection<FilterResult> resultList =
            new ObservableCollection<FilterResult>() { };

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
        }

        public FilterAndSelect(Token token)
        {
            this.token = token;
            Filter = new ObservableCollection<SelectConfig>() { new SelectConfig(token) };
            SelectList = new ObservableCollection<SelectConfig>() { new SelectConfig(token) };
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测结果
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private bool result = true;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 输入操作
        /// </summary>
        [property: DisplayName("输入操作")]
        [ObservableProperty]
        private EMUNIONMETHOD unionMethod = EMUNIONMETHOD.EMUNIONMETHOD_NONE;

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
            return $"过滤分选器->过滤：{string.Join("||", Filter)}，分选：{string.Join("||", SelectList)}";
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
        }

        public SelectConfig(Token token)
        {
            this.token = token;
            SelectParams = new ObservableCollection<OneSelectParams>()
            {
                new OneSelectParams(token)
            };
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

        public OneSelectParams(Token token)
        {
            this.token = token;
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
        [property: DisplayName("下限")]
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
            else if (MinLimit) //限制最小
            {
                return $"{Min}≤{characterName}≤{double.PositiveInfinity}";
            }
            else if (MaxLimit) //限制最大
            {
                return $"{double.NegativeInfinity}≤{characterName}≤{Max}";
            }
            else //都不限制
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
            else if (MinLimit) //限制最小
            {
                if (value >= Min)
                {
                    return true;
                }
            }
            else if (MaxLimit) //限制最大
            {
                if (value <= Max)
                {
                    return true;
                }
            }
            return false;
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
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.WidthBound));
                    break;
                case EMFILTER.EMFILTER_HEIGHT:
                    sRegionOut = sRegionIn.FindAll(o => Excute(o.RegionInfo.HeightBound));
                    break;
                case EMFILTER.EMFILTER_NUM:
                    if (Excute(sRegionIn.Count))
                        sRegionOut = sRegionIn;
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
        public FilterResult() { }

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
        [property: JsonIgnore]
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
        [EnumString("顶点高度", "PeakHeight")]
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

    /// <summary>
    /// 2024.7.9 李焕彬
    /// 合并方法
    /// </summary>
    public enum EMUNIONMETHOD
    {
        /// <summary>
        /// 2024.7.9 李焕彬
        /// 不打散不合并
        /// </summary>
        [EnumString("不打散不合并", "Not Break And Union")]
        EMUNIONMETHOD_NONE,

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 打散
        /// </summary>
        [EnumString("打散", "Break")]
        EMUNIONMETHOD_BREAK,

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 合并
        /// </summary>
        [EnumString("合并", "Union")]
        EMUNIONMETHOD_UNION,
    }
}
