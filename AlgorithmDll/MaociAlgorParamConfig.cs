using AlgorithmDll.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WH.Entity.CommonLib;
using Newtonsoft.Json;
using WH.Entity.LogRecord;

namespace AlgorithmDll
{
    public partial class MaociAlgorParamConfig : ObservableLog, IRecipient<OperateMessage>
    {
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public MaociAlgorParamConfig() 
        {
            PcParams.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(PcParams)); };
            FpgaParams.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(FpgaParams)); };
            WeakReferenceMessenger.Default.Register<OperateMessage, string>(this, this.GetType().Namespace);
        }

        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(MaociAlgorParamConfig))
            {
                OperateLog.Info($"算法参数-{message.message}");
                return;
            }
            foreach (var qua in PcParams)
            {
                if (message.obj.GetType() == typeof(MaociAlgorParam))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"PC参数-{qua.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
            }
            foreach (var qua in FpgaParams)
            {
                if (message.obj.GetType() == typeof(MaociAlgorParamFpga))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"FPGA参数-{qua.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }

        private const string paramName1 = "分组1";

        [ObservableProperty]
        private ObservableCollection<MaociAlgorParam> pcParams = new ObservableCollection<MaociAlgorParam>() { new MaociAlgorParam(paramName1) };

        [ObservableProperty]
        private string pcSelect = paramName1;

        [ObservableProperty]
        private ObservableCollection<MaociAlgorParamFpga> fpgaParams = new ObservableCollection<MaociAlgorParamFpga>() { new MaociAlgorParamFpga(paramName1) };

        [ObservableProperty]
        private string fpgaSelect = paramName1;
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public partial class MaociAlgorParam : ObservableLog
    {
        public MaociAlgorParam(string name)
        {
            Name = name;
        }

        [ObservableProperty]
        [property: Category("1.GroupName")]
        [property: DisplayName("GroupName")]
        [property: Description("GroupName")]
        private string name = "";

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("AdaptiveSize")]
        [property: Description("AdaptiveSize")]
        private uint adaptiveSize = 14;//自适应阈值邻域大小

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("AdaptiveAddGray")]
        [property: Description("AdaptiveAddGray")]
        private int adaptiveAddGray = 20;//自适应阈值增加值

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("NeighbSize")]
        [property: Description("NeighbSize")]
        private uint neighbSize = 5;//过滤矩阵邻域大小

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("NeighbLightPoint")]
        [property: Description("NeighbLightPoint")]
        private uint neighbLightPoint = 30;//过滤矩阵邻域点数量限制

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("DarkThresh")]
        [property: Description("DarkThresh")]
        private uint darkThresh = 30;//料区阈值

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("LightThresh")]
        [property: Description("LightThresh")]
        private uint lightThresh = 80;//铝层阈值

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("LightThick")]
        [property: Description("LightThick")]
        private uint lightThick = 6;//铝层厚度

        public override string ToString()
        {
            return Name;
        }
    };

    /// <summary>
    /// FPGA算法参数
    /// </summary>
    public partial class MaociAlgorParamFpga : ObservableLog
    {
        public MaociAlgorParamFpga(string name)
        {
            Name = name;
        }

        [ObservableProperty]
        [property: Category("1.GroupName")]
        [property: DisplayName("GroupName")]
        [property: Description("GroupName")]
        private string name = "";

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("AdaptiveSize")]
        [property: Description("AdaptiveSize")]
        private uint adaptiveSize = 14;//自适应阈值邻域大小

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("AdaptiveAddGray")]
        [property: Description("AdaptiveAddGray")]
        private int adaptiveAddGray = 20;//自适应阈值增加值

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("NeighbSize")]
        [property: Description("NeighbSize")]
        private uint neighbSize = 5;//过滤矩阵邻域大小

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("NeighbLightPoint")]
        [property: Description("NeighbLightPoint")]
        private uint neighbLightPoint = 30;//过滤矩阵邻域点数量限制

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("DarkThresh")]
        [property: Description("DarkThresh")]
        private uint darkThresh = 30;//料区阈值

        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("LightThresh")]
        [property: Description("LightThresh")]
        private uint lightThresh = 80;//铝层阈值

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("DarkThickLimit")]
        [property: Description("DarkThickLimit")]
        private uint darkThickLimit = 30;//料区厚度限制，掉料检测

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("DarkThickContinueLen")]
        [property: Description("DarkThickContinueLen")]
        private uint darkThickContinueLen = 5;//料区厚度NG连续长度限制

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("DarkThick")]
        [property: Description("DarkThick")]
        private uint darkThick = 84;//料区厚度

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("LightThickLimit")]
        [property: Description("LightThickLimit")]
        private uint lightThickLimit = 7;//铝层厚度限制，毛刺检测

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("LightThickContinueLen")]
        [property: Description("LightThickContinueLen")]
        private uint lightThickContinueLen = 0;//铝层厚度NG连续长度限制

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("LightThick")]
        [property: Description("LightThick")]
        private uint lightThick = 6;//铝层厚度

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("PosLimitT")]
        [property: Description("PosLimitT")]
        private uint posLimitT = 20;//铝层在料区中心位置限制上

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("PosLimitB")]
        [property: Description("PosLimitB")]
        private uint posLimitB = 20;//铝层在料区中心位置限制下

        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("LightPosOffest")]
        [property: Description("LightPosOffest")]
        private int lightPosOffest = 0;//铝层位置偏移值

        public override string ToString()
        {
            return Name;
        }
    }
}
