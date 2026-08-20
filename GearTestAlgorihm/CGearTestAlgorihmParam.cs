using System.Runtime.Serialization;
using AlgorithmDll;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace GearTestAlgorihm
{
    /// <summary>
    /// 盘齿算法参数（方案3.4 空实现）。
    /// 不加载模型、不推理；缺陷树按方案3.1 按制程挂名，便于后续过滤树。
    /// </summary>
    public class CGearTestAlgorihmParam : CAlgorithmParamBase
    {
        public string User { get; set; }

        /// <summary>
        /// 面积换算默认值（方案3.4）；标定后以相机 cell.MmPerPixel 为准。
        /// </summary>
        public double MmPerPixel { get; set; } = 0.04;

        public CGearTestAlgorihmParam()
            : this("")
        {
        }

        public CGearTestAlgorihmParam(string user)
            : base()
        {
            User = user ?? "";
            PrcessName = User;
            SetDefectRecipe(User);
            InitDefectFeatures();
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
        /// 空实现：不推理、不写 AlgorithmOut。过滤阶段会把无缺陷判为 OK。
        /// 上端面（倒角偏、孔钻错）、上轴侧面（小孔未钻）走 Halcon 空壳，仍返回空列表。
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
            if (processName == "上轴侧面" || processName == "上端面")
            {
                RunHalconEmpty(cell);
            }

            cell.IsOK = true;
        }

        /// <summary>
        /// Halcon 空壳（方案3.1）：上端面=倒角偏+孔钻错；上轴侧面=小孔未钻。
        /// 后续接入见 HDev/05。本阶段不引用 Halcon 运行时。
        /// </summary>
        protected List<CellDetection> RunHalconEmpty(Cell cell)
        {
            return new List<CellDetection>();
        }

        /// <summary>
        /// 方案0 A5：反序列化时空守卫。空工程无模型路径时不得 NRE。
        /// DefectSpecies 带 [JsonIgnore]，必须在此重建。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context)
        {
            if (string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(PrcessName))
            {
                User = PrcessName;
            }

            SetDefectRecipe(User);
            InitDefectFeatures();
        }

        protected void InitDefectFeatures()
        {
            //【盘齿方案6-注释】几何过滤用「数值」；区域下拉只用面积类特征
            DefectFeatures = new List<CFeacture>
            {
                new("Area", "面积", "Area", "um2"),
                new("Width", "宽度", "Width", "um"),
                new("Height", "高度", "Height", "um"),
                new("Score", "分数", "Score", ""),
            };
        }

        /// <summary>
        /// 按方案3.1 矩阵挂本制程缺陷名。始终 new List，禁止 null。
        /// 几何、倒角偏、孔钻错、小孔未钻 均为 Halcon。过滤 Category.值。
        /// </summary>
        protected void SetDefectRecipe(string processName)
        {
            DefectSpecies = new List<CDefectSpecies>();
            var yolo = new List<CDefectRecipe>();
            var geo = new List<CDefectRecipe>();

            void Area(string name) => yolo.Add(new CDefectRecipe(name, Category.区域));
            void Val(string name) => geo.Add(new CDefectRecipe(name, Category.值));

            switch (processName)
            {
                case "下端面":
                    Area("锈蚀");
                    Area("有划痕");
                    Area("端面碰伤");
                    Area("端面缠花");
                    break;
                case "上齿面":
                    Area("锈蚀");
                    Area("有划痕");
                    Area("齿顶缠花");
                    Area("表面压伤");
                    Area("齿顶碰伤");
                    break;
                case "上端面":
                    Area("锈蚀");
                    Area("有划痕");
                    Area("端面碰伤");
                    Area("端面缠花");
                    Val("倒角偏"); // Halcon
                    Val("孔钻错"); // Halcon
                    break;
                case "内孔":
                    Area("内壁锈蚀");
                    Area("内孔划伤");
                    Area("内孔缠花");
                    break;
                case "上轴侧面":
                    Area("锈蚀");
                    Area("有划痕");
                    Val("小孔未钻"); // Halcon
                    break;
                case "下轴侧面":
                    Area("锈蚀");
                    Area("有划痕");
                    break;
                case "整轴侧面":
                    Area("锈蚀");
                    Area("有划痕");
                    Area("侧面齿轮磕碰");
                    break;
            }

            if (yolo.Count > 0)
            {
                DefectSpecies.Add(new CDefectSpecies("外观", yolo));
            }
            if (geo.Count > 0)
            {
                DefectSpecies.Add(new CDefectSpecies("几何", geo));
            }
        }
    }

    public partial class CParam : CParamBase
    {
        public CParam()
            : base()
        {
        }

        public CParam(string name, Token token)
            : base(name, token)
        {
        }
    }
}
