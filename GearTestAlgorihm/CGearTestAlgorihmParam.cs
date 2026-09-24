using System.ComponentModel;
using System.Runtime.Serialization;
using AlgorithmDll;
using AlgorithmYoloBase;
using GearTestAlgorihm.Halcon;
using OpenVinoSharp.Extensions.result;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace GearTestAlgorihm
{
    /// <summary>
    /// 继承算法参数基类。七制程 YOLO Infer，ParseResult Type=外观。
    /// </summary>
    public class CGearTestAlgorihmParam : CYoloDetParamBase
    {
        protected override string LogTag => "【盘齿方案】";

        protected override string PluginFolderName => "GearTestAlgorihm";

        protected override string AppearanceSpeciesName => "外观";

        protected override bool ShouldSkipYoloIfNoModel => false;

        public CGearTestAlgorihmParam()
            : this("")
        {
        }

        public CGearTestAlgorihmParam(string user)
            : base()
        {
            MmPerPixel = 0.04;
            InitYoloAfterConstruct(user);
        }

        /// <summary>
        /// 3.6 ChamferOffset Halcon params (frozen fast defaults).
        /// </summary>
        public ChamferOffsetParams ChamferOffset { get; set; } = new ChamferOffsetParams();

        /// <summary>
        /// 上齿面齿轮数 Halcon 参数
        /// </summary>
        public GearToothCountParams GearToothCount { get; set; } = new GearToothCountParams();

        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParam(name, token));
        }

        protected override CParamBase CreateParam(string name)
        {
            return new CParam(name, token);
        }

        public override Func<CImage, float> GetDistinctFunc()
        {
            return (CImage image) => 0;
        }

        protected override void OnBeforeYolo(Cell cell, string processName)
        {
            if (processName == "上齿面")
            {
                RunHalconGearToothCount(cell);
            }
            if (processName == "上端面")
            {
                RunHalconChamferOffset(cell);
            }
        }

        protected override void OnAfterYolo(Cell cell, string processName)
        {
            if (processName == "上齿面")
            {
                RunMarkedToothCountFlags(cell);
            }
            if (processName == "上轴侧面")
            {
                RunHalconEmpty(cell);
            }
        }

        /// <summary>
        /// 上齿面：数 YOLO「标记齿」框。0→漏倒=1；＞1→重复=1
        /// </summary>
        protected void RunMarkedToothCountFlags(Cell cell)
        {
            int n = 0;
            if (cell?.AlgorithmOut != null)
            {
                for (int i = 0; i < cell.AlgorithmOut.Count; i++)
                {
                    CellDetection item = cell.AlgorithmOut[i];
                    if (item != null && item.RecipeDefectName == "标记齿")
                    {
                        n = item.regionOut == null ? 0 : item.regionOut.Count;
                        break;
                    }
                }
            }

            AddMarkedToothFlag(cell, "标记齿漏倒", n == 0 ? 1f : 0f);
            AddMarkedToothFlag(cell, "标记齿重复", n > 1 ? 1f : 0f);
        }

        static void AddMarkedToothFlag(Cell cell, string name, float value)
        {
            if (cell?.AlgorithmOut == null)
            {
                return;
            }

            CellDetection detection = new CellDetection();
            detection.Type = "几何";
            detection.Category = Category.值;
            detection.RecipeDefectName = name;
            detection.ShowInView = 0;
            detection.Value = new List<float> { value };
            cell.AlgorithmOut.Add(detection);
        }

        /// <summary>
        /// 上齿面齿轮数：Halcon 包装写入一条几何齿数。
        /// </summary>
        protected void RunHalconGearToothCount(Cell cell)
        {
            CellDetection detection = new CellDetection();
            detection.Type = "几何";
            detection.Category = Category.值;
            detection.RecipeDefectName = "齿轮数";
            detection.ShowInView = 0;
            detection.Value = new List<float> { -1f };
            try
            {
                GearToothCountResult r = GearToothCountAlgorithm.Run(cell, GearToothCount);
                detection.Value = new List<float> { r.Count };
                if (!string.IsNullOrEmpty(r.Warn))
                {
                    OperateLog?.Warn("【盘齿方案】" + r.Warn);
                }
            }
            catch (Exception ex)
            {
                OperateLog?.Warn("【盘齿方案】" + ex.Message);
            }
            cell.AlgorithmOut.Add(detection);
        }

        /// <summary>
        /// 上端面倒角偏：Halcon 包装写入一条几何 Dist1/2/3。
        /// </summary>
        protected void RunHalconChamferOffset(Cell cell)
        {
            CellDetection detection = new CellDetection();
            detection.Type = "几何";
            detection.Category = Category.值;
            detection.RecipeDefectName = "倒角偏";
            detection.ShowInView = 0;
            detection.Value = new List<float> { -1f, -1f, -1f };
            try
            {
                ChamferOffsetResult r = ChamferOffsetAlgorithm.Run(cell, ChamferOffset);
                detection.Value = new List<float> { r.Dist1, r.Dist2, r.Dist3 };
                if (!string.IsNullOrEmpty(r.Warn))
                {
                    OperateLog?.Warn("【盘齿方案】" + r.Warn);
                }
            }
            catch (Exception ex)
            {
                OperateLog?.Warn("【盘齿方案】" + ex.Message);
            }
            cell.AlgorithmOut.Add(detection);
        }

        /// <summary>
        /// Halcon 空壳。上轴侧面小孔未钻仍不引用算子。
        /// </summary>
        protected List<CellDetection> RunHalconEmpty(Cell cell)
        {
            return new List<CellDetection>();
        }

        /// <summary>
        /// 按当前制程名加载 OpenVINO Det。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context) => OnDeserializedCore(context);

        protected override IReadOnlyList<string> GetModelSearchDirs(string processName)
        {
            return new[] { ".\\AlgorithmPlug\\" + PluginFolderName + "\\Models\\" + processName + "\\" };
        }

        protected override CoordRestoreData CreateRestoreData(Cell cell, string labelname, DetData det)
        {
            int imgIndex = cell.PhotoIndex - 1;
            return new CoordRestoreData(cell.Image.ImageWidth, imgIndex, 0, 0, labelname, det);
        }

        /// <summary>
        /// 精简自拉链 GetDetectRegion。长短边比较保持现状 RecWidth &gt; RecWidth。
        /// </summary>
        protected override SRegion GetDetectRegion(CoordRestoreData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            sRegioninfo.WidthBound = info.RecWidth;
            sRegioninfo.HeightBound = info.RecHeight;
            if (info.RecWidth > info.RecWidth)
            {
                sRegioninfo.LongLen = info.RecWidth;
                sRegioninfo.ShorLen = info.RecHeight;
            }
            else
            {
                sRegioninfo.LongLen = info.RecHeight;
                sRegioninfo.ShorLen = info.RecWidth;
            }

            sRegioninfo.Phi = info.Angle;
            sRegioninfo.Area = info.RecWidth * info.RecHeight;
            sRegioninfo.Score = info.Score;
            sRegioninfo.PositionX = info.OrgCenterX;
            sRegioninfo.PositionY = info.OrgCenterY;
            sRegioninfo.ColorDiffValue = info.Value;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                info.ShowLeftUp,
                info.ShowRightUp,
                info.ShowRightDown,
                info.ShowLeftDown,
            };

            return new SRegion(sRegioninfo, rec1Points);
        }

        protected override void InitDefectFeatures()
        {
            //几何过滤用「数值」；区域下拉只用面积类特征
            DefectFeatures = new List<CFeacture>();
            DefectFeatures = new();
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("ColorDiffValue", "色差", "ColorDiffValue", "")); 
            DefectFeatures.Add(new("PositionX", "位置X", "PositionX", "um"));
            DefectFeatures.Add(new("PositionY", "位置Y", "PositionY", "um"));
        }

        /// <summary>
        /// 几何：上齿面齿轮数；上端面倒角偏
        /// </summary>
        protected override void SetDefectRecipe(string processName)
        {
            DefectSpecies = new List<CDefectSpecies>();
            var yolo = new List<CDefectRecipe>();
            var geo = new List<CDefectRecipe>();

            if (class_names != null)
            {
                for (int i = 0; i < class_names.Length; i++)
                {
                    string name = class_names[i];
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    yolo.Add(new CDefectRecipe(name, Category.区域));
                }
            }

            if (processName == "上齿面")
            {
                geo.Add(new CDefectRecipe("齿轮数", Category.值));
                geo.Add(new CDefectRecipe("标记齿漏倒", Category.值));
                geo.Add(new CDefectRecipe("标记齿重复", Category.值));
            }

            if (processName == "上端面")
            {
                geo.Add(new CDefectRecipe("倒角偏", Category.值, 3));
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

    public partial class CParam : CYoloInferParam
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
