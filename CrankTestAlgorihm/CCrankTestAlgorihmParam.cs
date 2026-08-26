using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using AlgorithmDll;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using WH.VisionLearning;

namespace CrankTestAlgorihm
{
    /// <summary>
    /// 【曲轴方案3.4-注释】曲轴算法参数。六制程全挂 划痕/裂纹/磕碰，仅 YOLO（Area+Score）。无 Halcon。
    /// </summary>
    public class CCrankTestAlgorihmParam : CAlgorithmParamBase
    {
        public const string SpeciesAppearance = "外观";

        public static readonly string[] FrozenDefectNames = { "划痕", "裂纹", "磕碰" };

        public string User { get; set; }

        /// <summary>
        /// 【曲轴方案3.4-注释】未标定默认 0，不要抄盘齿 0.04。
        /// </summary>
        public double MmPerPixel { get; set; } = 0;

        IVisionModel WH_det;

        private string Model_Path;

        protected string[] class_names;

        public CCrankTestAlgorihmParam()
            : this("")
        {
        }

        public CCrankTestAlgorihmParam(string user)
            : base()
        {
            User = user ?? "";
            PrcessName = User;
            InitDefectFeatures();
            LoadYoloModel();
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
        /// 【曲轴方案3.4-注释】空守卫；无模型 M0 空跑 IsOK=true。不写 PLC / CrankImages，不跑 Halcon。
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

            if (WH_det is null || string.IsNullOrEmpty(Model_Path))
            {
                OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " 无模型，M0 空跑 IsOK=true");
                cell.IsOK = true;
                return;
            }

            DetectYolo(cell);
            cell.IsOK = true;
        }

        private void DetectYolo(Cell cell)
        {
            Mat matimg = null;
            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            try
            {
                var paramClass = AlgorParams?.FirstOrDefault(o => o.Name == ParamSelect) as CParam;
                if (paramClass is null)
                {
                    paramClass = AlgorParams?.FirstOrDefault() as CParam;
                }

                matimg = GetMatImage(cell, paramClass);
                if (matimg is null)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " GetMatImage 为空，跳过 Infer");
                    return;
                }

                DetResult detResult = null;
                try
                {
                    detResult = ImageInferDet(WH_det, matimg);
                }
                catch (Exception ex)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " Predict 失败: " + ex.Message);
                    return;
                }

                if (detResult is null)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " Infer 无结果（模型未加载或 Predict 返回空）");
                    return;
                }

                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (detResult.datas != null && class_names != null && cell.Image != null)
                {
                    int imgIndex = cell.PhotoIndex - 1;
                    for (int j = 0; j < detResult.datas.Count; j++)
                    {
                        if (!int.TryParse(detResult.datas[j].lable, out int labelindex))
                        {
                            continue;
                        }
                        if (labelindex < 0 || labelindex >= class_names.Length)
                        {
                            continue;
                        }
                        string labelname = class_names[labelindex];
                        CoordRestoreData restoreData = new CoordRestoreData(
                            cell.Image.ImageWidth, imgIndex, 0, 0, labelname, detResult.datas[j]);
                        dets.Add(restoreData);
                    }
                }

                ParseResult(dets, cell);
            }
            finally
            {
                matimg?.Dispose();
            }
        }

        /// <summary>
        /// 【曲轴方案3.4-注释】方案0 A5 空守卫。DefectSpecies 带 JsonIgnore，反序列化后必须重建。
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
            LoadYoloModel();
            SetDefectRecipe(User);
        }

        /// <summary>
        /// 【曲轴方案3.4-注释】制程目录优先，否则 Models\_shared。无路径 / names 空 / 缺目录 → return，禁止 NRE。
        /// </summary>
        private void LoadYoloModel()
        {
            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            try
            {
                CParam param = AlgorParams?.FirstOrDefault() as CParam;
                if (param is null)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " 无算法参数，跳过加载模型");
                    return;
                }

                string modelsRoot = ".\\AlgorithmPlug\\CrankTestAlgorihm\\Models\\";
                string processDir = modelsRoot + processName + "\\";
                string sharedDir = modelsRoot + "_shared\\";

                var names = GetNames(processDir);
                if (string.IsNullOrEmpty(names.Item1) || names.Item2 is null)
                {
                    names = GetNames(sharedDir);
                }

                if (string.IsNullOrEmpty(names.Item1) || names.Item2 is null)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " 无模型文件，跳过加载");
                    return;
                }

                Model_Path = names.Item1;
                class_names = names.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (class_names.Length == 0)
                {
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " classes.txt 为空，跳过加载");
                    Model_Path = null;
                    return;
                }

                WarnExtraClassNames(processName, class_names);

                string currentDevice = string.IsNullOrWhiteSpace(param.CurrentDevice) ? "CPU" : param.CurrentDevice;
                float score = param.Score;
                float nms = param.Nms;
                const int inputSize = 640;
                WH_det = VisionModelExtensions.GetVisionModel(
                    ModelType.VisionModelDet,
                    Model_Path,
                    EngineType.OpenVINO,
                    currentDevice,
                    class_names.Length,
                    score,
                    nms,
                    inputSize);
                OperateLog?.Info(
                    "【曲轴方案3.4-注释】" + processName + " 模型已加载: " + Model_Path
                    + ", device=" + currentDevice
                    + ", classes=" + class_names.Length);
            }
            catch (Exception ex)
            {
                OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " 加载模型失败: " + ex.Message);
                WH_det = null;
                Model_Path = null;
            }
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
                    OperateLog?.Warn("【曲轴方案3.4-注释】" + processName + " classes.txt 额外类名已忽略（不进配方树）: " + name);
                }
            }
        }

        private (string, string[]) GetNames(string Dirpath)
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

        public DetResult ImageInferDet(IVisionModel WH, Mat img)
        {
            if (WH != null)
            {
                return WH.Predict(img) as DetResult;
            }

            return null;
        }

        public virtual Mat GetMatImage(Cell cell, CParamBase param)
        {
            if (cell?.Image == null)
            {
                return null;
            }

            Mat img = new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                cell.Image.ImageData);
            return img;
        }

        /// <summary>
        /// 【曲轴方案3.4-注释】Type 用种名「外观」（过滤树按 Type 索引 SpeciesFilter）；引擎为 YOLO。
        /// </summary>
        protected void ParseResult(List<CoordRestoreData> sResultInfos, Cell cell)
        {
            if (sResultInfos is null || cell.AlgorithmOut is null || DefectSpecies is null)
            {
                return;
            }

            foreach (var ds in DefectSpecies)
            {
                if (ds.Name != SpeciesAppearance)
                {
                    continue;
                }

                if (ds.RecipeDefects is null)
                {
                    continue;
                }

                foreach (var de in ds.RecipeDefects)
                {
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = SpeciesAppearance;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();

                    var finds = sResultInfos.FindAll(info =>
                    {
                        return info.Labelstr == de.Name;
                    });
                    if (finds.Count > 0)
                    {
                        foreach (var item in finds)
                        {
                            SRegion sRegion = GetDetectRegion(item);
                            cellDetection1.regionOut.Add(sRegion);
                            cellDetection1.Value.Add(item.Value);
                            cellDetection1.ShowInView = item.ShowInView;
                        }
                    }
                    cell.AlgorithmOut.Add(cellDetection1);
                }
            }
            sResultInfos.Clear();
        }

        protected SRegion GetDetectRegion(CoordRestoreData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            sRegioninfo.WidthBound = info.RecWidth;
            sRegioninfo.HeightBound = info.RecHeight;
            if (info.RecWidth > info.RecHeight)
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
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                info.ShowLeftUp,
                info.ShowRightUp,
                info.ShowRightDown,
                info.ShowLeftDown,
            };

            return new SRegion(sRegioninfo, rec1Points);
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
        /// 【曲轴方案3.4-注释】配方永远 划痕/裂纹/磕碰，不依赖 classes.txt。第四名 Warn 忽略。无几何种。
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

        /// <summary>
        /// 【曲轴方案3.4-注释】推理设备，默认 CPU。
        /// </summary>
        [Category("基础参数")]
        [DisplayName("01驱动器")]
        [Description("驱动器")]
        public string CurrentDevice { get; set; } = "CPU";

        /// <summary>
        /// 【曲轴方案3.4-注释】检测分数阈值，默认 0.3。
        /// </summary>
        [Category("分数设置")]
        [DisplayName("01 分数阈值")]
        [Description("分数阈值")]
        public float Score { get; set; } = 0.3f;

        /// <summary>
        /// 【曲轴方案3.4-注释】NMS，默认 0.5。
        /// </summary>
        public float Nms { get; set; } = 0.5f;
    }

    /// <summary>
    /// 【曲轴方案3.4-注释】区域特征，仅 Area/Score/Width/Height；不加 Dist/MaxDev/Diameter。
    /// </summary>
    public struct SRegionInfo : IRegionInfo
    {
        public double WidthBound = 0;
        public double HeightBound = 0;
        public double LongLen = 0;
        public double ShorLen = 0;
        public double Phi = 0;
        public double Score = 0;
        public double Area = 0;
        public float PositionX = 0;
        public float PositionY = 0;

        public SRegionInfo() { }

        public double GetValue(CFeacture feacture, SRegion region)
        {
            switch (feacture.Id)
            {
                case "Width":
                    return WidthBound;
                case "Height":
                    return HeightBound;
                case "Area":
                    return Area;
                case "Score":
                    return Score;
                default:
                    return 0;
            }
        }

        public SRegion Union(List<SRegion> regions)
        {
            SRegionInfo regionInfo = new SRegionInfo();
            if (regions != null && regions.Count > 0)
            {
                regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
                regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
                regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
                regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
            }
            List<System.Windows.Point> pts = new List<System.Windows.Point>();
            if (regions != null)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    if (regions[i].points != null)
                    {
                        pts.AddRange(regions[i].points);
                    }
                }
            }
            return new SRegion(regionInfo, pts);
        }
    }

    /// <summary>
    /// 【曲轴方案3.4-注释】Det 轴对齐框坐标还原。
    /// </summary>
    public struct CoordRestoreData
    {
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, DetData det, int showinview = 0)
        {
            ShowLeftUp.X = det.box.Left + orgx + imgWidth * imgIndex;
            ShowLeftUp.Y = det.box.Top + orgy;
            ShowRightUp.X = det.box.Right + orgx + imgWidth * imgIndex;
            ShowRightUp.Y = det.box.Top + orgy;

            ShowRightDown.X = det.box.Right + orgx + imgWidth * imgIndex;
            ShowRightDown.Y = det.box.Bottom + orgy;

            ShowLeftDown.X = det.box.Left + orgx + imgWidth * imgIndex;
            ShowLeftDown.Y = det.box.Bottom + orgy;

            RecWidth = det.box.Width;
            RecHeight = det.box.Height;
            OrgCenterX = (float)(det.box.Left + det.box.Width / 2.0) + orgx;
            OrgCenterY = (float)(det.box.Top + det.box.Height / 2.0) + orgy;
            Score = det.score * 100;
            Labelstr = labelstr;
            Angle = 0.0f;
            Value = 0.0f;
            ShowInView = showinview;
        }

        public System.Windows.Point ShowLeftUp = new System.Windows.Point();
        public System.Windows.Point ShowRightUp = new System.Windows.Point();
        public System.Windows.Point ShowRightDown = new System.Windows.Point();
        public System.Windows.Point ShowLeftDown = new System.Windows.Point();
        public float OrgCenterX { get; set; }
        public float RecWidth { get; set; }
        public float RecHeight { get; set; }
        public float Score { get; set; }
        public string Labelstr { get; set; }
        public float Angle { get; set; }
        public float Value { get; set; }
        public int ShowInView { get; set; }
        public float OrgCenterY { get; set; }
    }
}
