using System.ComponentModel;
using System.Runtime.Serialization;
using AlgorithmDll;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace PlaneGearTestAlgorihm
{
    /// <summary>
    /// 【新兴盘齿方案3.4-注释】平面齿算法参数。三制程全挂 毛刺/飞边/锈蚀/碰伤/压伤/氧化。波次1 空跑，无 Halcon、无圆心尺寸。
    /// </summary>
    public class CPlaneGearTestAlgorihmParam : CAlgorithmParamBase
    {
        public const string SpeciesAppearance = "外观";

        public static readonly string[] FrozenDefectNames =
        {
            "毛刺", "飞边", "锈蚀", "碰伤", "压伤", "氧化",
        };

        public string User { get; set; }

        /// <summary>
        /// 【新兴盘齿方案3.4-注释】未标定默认 0，不要抄盘齿 0.04。
        /// </summary>
        public double MmPerPixel { get; set; } = 0;

        public CPlaneGearTestAlgorihmParam()
            : this("")
        {
        }

        public CPlaneGearTestAlgorihmParam(string user)
            : base()
        {
            User = user ?? "";
            PrcessName = User;
            InitDefectFeatures();
            SetDefectRecipe(User);
        }

        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParam(name, token));
        }

        public override Func<CImage, float> GetDistinctFunc()
        {
            return (CImage image) => 0;
        }

        /// <summary>
        /// 【新兴盘齿方案3.4-注释】空守卫；无模型空跑 IsOK=true。不 Clear AlgorithmOut。不写 PLC。
        /// </summary>
        public override void DetectImage(Cell cell)
        {
            if (cell is null)
            {
                return;
            }

            if (cell.AlgorithmOut is null)
            {
                cell.AlgorithmOut = new List<CellDetection>();
            }

            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            if (!string.IsNullOrEmpty(processName))
            {
                cell.ProjName = processName;
            }

            OperateLog?.Warn("【新兴盘齿方案3.4-注释】" + processName + " 无模型，空跑 IsOK=true");
            cell.IsOK = true;
        }

        /// <summary>
        /// 【新兴盘齿方案3.4-注释】方案0 A5 空守卫。DefectSpecies 带 JsonIgnore，反序列化后必须重建。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context)
        {
            if (string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(PrcessName))
            {
                User = PrcessName;
            }

            if (AlgorParams is null)
            {
                AlgorParams = new System.Collections.ObjectModel.ObservableCollection<CParamBase>();
            }
            if (AlgorParams.Count == 0)
            {
                AddParam("分组1");
            }

            InitDefectFeatures();
            SetDefectRecipe(User);
        }

        protected void InitDefectFeatures()
        {
            DefectFeatures = new List<CFeacture>();
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
        }

        /// <summary>
        /// 【新兴盘齿方案3.1-注释】配方永远 6 名，不依赖 classes.txt。Category=区域，Type=外观。
        /// </summary>
        protected void SetDefectRecipe(string processName)
        {
            DefectSpecies = new List<CDefectSpecies>();
            var yolo = new List<CDefectRecipe>();
            foreach (string name in FrozenDefectNames)
            {
                yolo.Add(new CDefectRecipe(name, Category.区域));
            }

            DefectSpecies.Add(new CDefectSpecies(SpeciesAppearance, yolo));
        }
    }

    public class CParam : CParamBase
    {
        public CParam()
            : base()
        {
        }

        public CParam(string name, Token token)
            : base(name, token)
        {
        }

        [Category("基础参数")]
        [DisplayName("01驱动器")]
        [Description("驱动器")]
        public string CurrentDevice { get; set; } = "CPU";

        [Category("分数设置")]
        [DisplayName("01 分数阈值")]
        [Description("分数阈值")]
        public float Score { get; set; } = 0.3f;

        public float Nms { get; set; } = 0.5f;
    }
}
