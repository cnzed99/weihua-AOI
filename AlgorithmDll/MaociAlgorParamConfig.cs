using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 算法参数配置管理类
    /// </summary>
    public partial class CMaociAlgorParamConfig
        : ConfigModifyObservableBase,
            IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public CMaociAlgorParamConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
            PcParams = new ObservableCollection<CMaociAlgorParam>()
            {
                new CMaociAlgorParam(c_ParamName, token)
            };
            FpgaParams = new ObservableCollection<CMaociAlgorParamFpga>()
            {
                new CMaociAlgorParamFpga(c_ParamName, token)
            };
            UpdataMaociAlgorParamUse();
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CMaociAlgorParamConfig))
            {
                OperateLog.Info($"算法参数-{message.message}");
                UpdataMaociAlgorParamUse();
                return;
            }
            foreach (var qua in PcParams)
            {
                if (message.obj.GetType() == typeof(CMaociAlgorParam))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"PC参数-{qua.Name}-{message.message}");
                        UpdataMaociAlgorParamUse();
                        return;
                    }
                    continue;
                }
            }
            foreach (var qua in FpgaParams)
            {
                if (message.obj.GetType() == typeof(CMaociAlgorParamFpga))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"FPGA参数-{qua.Name}-{message.message}");
                        UpdataMaociAlgorParamUse();
                        return;
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 默认分组名
        /// </summary>
        private const string c_ParamName = "分组1";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 参数列表
        /// </summary>
        [property: DisplayName("参数列表")]
        [ObservableProperty]
        private ObservableCollection<CMaociAlgorParam> pcParams;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 当前算法参数组
        /// </summary>
        [property: DisplayName("当前算法参数组")]
        [ObservableProperty]
        private string pcSelect = c_ParamName;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 预处理参数列表
        /// </summary>
        [property: DisplayName("预处理参数列表")]
        [ObservableProperty]
        private ObservableCollection<CMaociAlgorParamFpga> fpgaParams;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 当前预处理参数组
        /// </summary>
        [property: DisplayName("当前预处理参数组")]
        [ObservableProperty]
        private string fpgaSelect = c_ParamName;

        /// <summary>
        /// 2024.7.5 李焕彬
        /// 正在使用的PC参数结构体
        /// </summary>
        public SMaociAlgorParam MaociAlgorParamUse { get; set; }

        /// <summary>
        /// 2024.7.5 李焕彬
        /// 正在使用的FPGA参数结构体
        /// </summary>
        public SMaociAlgorParamFpga MaociAlgorParamFpgaUse { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        private void UpdataMaociAlgorParamUse()
        {
            if (PcParams.FirstOrDefault(o => o.Name == PcSelect) != null)
            {
                MaociAlgorParamUse = new(PcParams.FirstOrDefault(o => o.Name == PcSelect));
            }
            if (FpgaParams.FirstOrDefault(o => o.Name == FpgaSelect) != null)
            {
                MaociAlgorParamFpgaUse = new(FpgaParams.FirstOrDefault(o => o.Name == FpgaSelect));
            }
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public partial class CMaociAlgorParam : ConfigModifyObservableBase
    {
        public CMaociAlgorParam()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public CMaociAlgorParam(string name, Token token)
        {
            this.token = token;
            Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 分组名
        /// </summary>
        [ObservableProperty]
        [property: Category("1.GroupName")]
        [property: DisplayName("分组名")]
        [property: Description("分组名")]
        private string name = "";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("自适应阈值邻域大小")]
        [property: Description("自适应阈值邻域大小")]
        private uint adaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("自适应阈值增加值")]
        [property: Description("自适应阈值增加值")]
        private int adaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("过滤矩阵邻域大小")]
        [property: Description("过滤矩阵邻域大小")]
        private uint neighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("过滤矩阵邻域点数量限制")]
        [property: Description("过滤矩阵邻域点数量限制")]
        private uint neighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值")]
        private uint darkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值")]
        private uint lightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层厚度")]
        [property: Description("铝层厚度")]
        private uint lightThick = 6;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    };

    /// <summary>
    /// FPGA算法参数
    /// </summary>
    public partial class CMaociAlgorParamFpga : ConfigModifyObservableBase
    {
        public CMaociAlgorParamFpga()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public CMaociAlgorParamFpga(string name, Token token)
        {
            this.token = token;
            Name = name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 分组名
        /// </summary>
        [ObservableProperty]
        [property: Category("1.GroupName")]
        [property: DisplayName("分组名")]
        [property: Description("分组名")]
        private string name = "";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: EditorAttribute()]
        [property: Category("2.Algorithm")]
        [property: DisplayName("自适应阈值邻域大小")]
        [property: Description("自适应阈值邻域大小")]
        private uint adaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("自适应阈值增加值")]
        [property: Description("自适应阈值增加值")]
        private int adaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("过滤矩阵邻域大小")]
        [property: Description("过滤矩阵邻域大小")]
        private uint neighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("过滤矩阵邻域点数量限制")]
        [property: Description("过滤矩阵邻域点数量限制")]
        private uint neighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值")]
        private uint darkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("2.Algorithm")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值")]
        private uint lightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度限制，掉料检测
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度限制")]
        [property: Description("料区厚度限制")]
        private uint darkThickLimit = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度NG连续长度限制")]
        [property: Description("料区厚度NG连续长度限制")]
        private uint darkThickContinueLen = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("料区厚度")]
        [property: Description("料区厚度")]
        private uint darkThick = 84;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度限制，毛刺检测
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度限制")]
        [property: Description("铝层厚度限制")]
        private uint lightThickLimit = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度NG连续长度限制")]
        [property: Description("铝层厚度NG连续长度限制")]
        private uint lightThickContinueLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层厚度")]
        [property: Description("铝层厚度")]
        private uint lightThick = 6;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制上
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层在料区中心位置限制上")]
        [property: Description("铝层在料区中心位置限制上")]
        private uint posLimitT = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制下
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层在料区中心位置限制下")]
        [property: Description("铝层在料区中心位置限制下")]
        private uint posLimitB = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层位置偏移值
        /// </summary>
        [ObservableProperty]
        [property: Category("3.Judge")]
        [property: DisplayName("铝层位置偏移值")]
        [property: Description("铝层位置偏移值")]
        private int lightPosOffest = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
