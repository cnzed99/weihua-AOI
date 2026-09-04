using System.IO;
using System.Runtime.Serialization;
using AlgorithmDll;
using AlgorithmYoloBase;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace PlaneGearTestAlgorihm
{
    /// <summary>
    /// 【新兴盘齿方案3.5-注释】平面齿算法参数。配方永远 6 名；有 Models\\制程 权重则 OpenVINO Det，无则空跑。
    /// </summary>
    public class CPlaneGearTestAlgorihmParam : CYoloDetParamBase
    {
        public const string SpeciesAppearance = "外观";

        public static readonly string[] FrozenDefectNames =
        {
            "毛刺", "飞边", "锈蚀", "碰伤", "压伤", "氧化",
        };

        protected override string LogTag => "【新兴盘齿方案3.5-注释】";

        protected override string PluginFolderName => "PlaneGearTestAlgorihm";

        protected override string AppearanceSpeciesName => SpeciesAppearance;

        protected override bool ShouldSkipYoloIfNoModel => true;

        public CPlaneGearTestAlgorihmParam()
            : this("")
        {
        }

        public CPlaneGearTestAlgorihmParam(string user)
            : base()
        {
            MmPerPixel = 0;
            InitYoloAfterConstruct(user);
        }

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

        protected override void BindCellProcessName(Cell cell, string processName)
        {
            if (!string.IsNullOrEmpty(processName))
            {
                cell.ProjName = processName;
            }
        }

        protected override void OnNoModel(Cell cell, string processName)
        {
            OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " 无模型，空跑 IsOK=true");
            cell.IsOK = true;
        }

        /// <summary>
        /// 【新兴盘齿方案3.4-注释】方案0 A5 空守卫。DefectSpecies 带 JsonIgnore，反序列化后必须重建。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context) => OnDeserializedCore(context);

        protected override IReadOnlyList<string> GetModelSearchDirs(string processName)
        {
            return new[] { ".\\AlgorithmPlug\\" + PluginFolderName + "\\Models\\" + processName + "\\" };
        }

        protected override void WarnModelNotFound(string processName, IReadOnlyList<string> dirs)
        {
            string dirPath = (dirs != null && dirs.Count > 0) ? dirs[0] : "";
            OperateLog?.Warn(LogTag + processName + " 无模型，空跑: " + dirPath);
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】只扫制程目录根下：优先 *.model，再 onnx/xml/engine/pt；classes.txt 不递归。
        /// </summary>
        protected override (string, string[]) GetNames(string Dirpath)
        {
            if (!Directory.Exists(Dirpath))
            {
                return ("", new string[1] { "" });
            }

            string[] searchPatterns = { "*.model", "*.onnx", "*.xml", "*.engine", "*.pt" };
            var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(Dirpath, pattern, SearchOption.TopDirectoryOnly))
                .ToList();

            var classNames = Directory.GetFiles(Dirpath, "*.txt", SearchOption.TopDirectoryOnly);
            if (files.Count > 0 && classNames.Length > 0)
            {
                string model_Path = files[0];
                string name_Path = classNames[0];
                string[] de_names = File.ReadAllLines(name_Path);
                return (model_Path, de_names);
            }

            return ("", new string[1] { "" });
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】Predict 必须用自有 Mat。禁止只包 ImageData：显示 ToBitmapSource 不拷贝，显示线程随后 FreeHGlobal，下一张 Infer 会 0xC0000005。
        /// </summary>
        public override Mat GetMatImage(Cell cell, CParamBase param)
        {
            if (cell?.Image == null || cell.Image.ImageData == IntPtr.Zero)
            {
                return null;
            }

            int channels = (cell.Image.PixelFormat.BitsPerPixel + 7) / 8;
            if (channels < 1)
            {
                channels = 1;
            }
            using (Mat wrap = new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC(channels),
                cell.Image.ImageData,
                cell.Image.StrideWidth))
            {
                return wrap.Clone();
            }
        }

        protected override CoordRestoreData CreateRestoreData(Cell cell, string labelname, DetData det)
        {
            // 【新兴盘齿方案0.6-注释】Infer 只写当前 cell.Image 图内坐标（imgIndex=0, mosaic2x2=false）。2x3 平移在 GetMergeCells。
            return new CoordRestoreData(
                cell.Image.ImageWidth, cell.Image.ImageHeight, 0, 0, 0, labelname, det, false);
        }

        protected override void WarnUnmatchedAppearanceLabels(List<CoordRestoreData> sResultInfos)
        {
            var recipeNames = new HashSet<string>();
            foreach (var ds in DefectSpecies)
            {
                if (ds.Name != SpeciesAppearance || ds.RecipeDefects is null)
                {
                    continue;
                }
                foreach (var de in ds.RecipeDefects)
                {
                    recipeNames.Add(de.Name);
                }
            }

            foreach (string unmatched in sResultInfos
                .Select(info => info.Labelstr)
                .Where(name => !string.IsNullOrEmpty(name) && !recipeNames.Contains(name))
                .Distinct())
            {
                OperateLog?.Warn("【新兴盘齿方案3.5-注释】未匹配配方的类名已丢弃: " + unmatched);
            }
        }

        /// <summary>
        /// 【新兴盘齿方案3.1-注释】配方永远 6 名，不依赖 classes.txt。Category=区域，Type=外观。
        /// </summary>
        protected override void SetDefectRecipe(string processName)
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

    public class CParam : CYoloInferParam
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
