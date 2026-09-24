using System.IO;
using System.Runtime.Serialization;
using AlgorithmDll;
using AlgorithmYoloBase;
using OpenVinoSharp.Extensions.result;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace CrankTestAlgorihm
{
    /// <summary>
    /// 曲轴算法参数。六制程全挂 划痕/裂纹/磕碰，仅 YOLO（Area+Score）
    /// </summary>
    public class CCrankTestAlgorihmParam : CYoloDetParamBase
    {
        public const string SpeciesAppearance = "外观";

        public static readonly string[] FrozenDefectNames = { "划痕", "裂纹", "磕碰" };

        protected override string LogTag => "【曲轴方案】";

        protected override string PluginFolderName => "CrankTestAlgorihm";

        protected override string AppearanceSpeciesName => SpeciesAppearance;

        protected override bool ShouldSkipYoloIfNoModel => true;

        protected override bool IsModelMissing => WH_det is null || string.IsNullOrEmpty(Model_Path);

        public CCrankTestAlgorihmParam()
            : this("")
        {
        }

        public CCrankTestAlgorihmParam(string user)
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
            OperateLog?.Warn("【曲轴方案】" + processName + " 无模型，M0 空跑 IsOK=true");
            cell.IsOK = true;
        }

        /// <summary>
        /// DefectSpecies 带 JsonIgnore，反序列化后必须重建。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context) => OnDeserializedCore(context);

        protected override IReadOnlyList<string> GetModelSearchDirs(string processName)
        {
            string modelsRoot = ".\\AlgorithmPlug\\" + PluginFolderName + "\\Models\\";
            return new[]
            {
                modelsRoot + processName + "\\",
                modelsRoot + "_shared\\",
            };
        }

        protected override void WarnModelNotFound(string processName, IReadOnlyList<string> dirs)
        {
            OperateLog?.Warn(LogTag + processName + " 无模型文件，跳过加载");
        }

        protected override void OnEmptyClassNames(string processName, string dirPath)
        {
            OperateLog?.Warn(LogTag + processName + " classes.txt 为空，跳过加载");
            Model_Path = null;
        }

        protected override void OnLoadModelFailed()
        {
            WH_det = null;
            Model_Path = null;
        }

        protected override void AfterModelLoaded(string processName)
        {
            WarnExtraClassNames(processName, class_names);
        }

        private void WarnExtraClassNames(string processName, string[] names)
        {
            if (names is null)
            {
                return;
            }

            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                if (!FrozenDefectNames.Contains(name))
                {
                    OperateLog?.Warn("【曲轴方案】" + processName + " classes.txt 额外类名已忽略: " + name);
                }
            }
        }

        protected override (string, string[]) GetNames(string Dirpath)
        {
            if (string.IsNullOrEmpty(Dirpath) || !Directory.Exists(Dirpath))
            {
                return ("", null);
            }

            string[] searchPatterns = { "*.xml", "*.onnx", "*.engine", "*.pt", "*.model" };
            var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(Dirpath, pattern))
                .ToList();

            var classNames = Directory.GetFiles(Dirpath, "classes.txt");
            if (classNames.Length == 0)
            {
                classNames = Directory.GetFiles(Dirpath, "*.txt");
            }

            if (files.Count > 0 && classNames.Length > 0)
            {
                string model_Path = files[0];
                string name_Path = classNames[0];
                string[] de_names = File.ReadAllLines(name_Path);
                return (model_Path, de_names);
            }

            return ("", null);
        }

        protected override CoordRestoreData CreateRestoreData(Cell cell, string labelname, DetData det)
        {
            int imgIndex = cell.PhotoIndex - 1;
            return new CoordRestoreData(cell.Image.ImageWidth, imgIndex, 0, 0, labelname, det);
        }

        /// <summary>
        /// 配方永远 划痕/裂纹/磕碰，不依赖 classes.txt todo需要依赖classes.txt
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
