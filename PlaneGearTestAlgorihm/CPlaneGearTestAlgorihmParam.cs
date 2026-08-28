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

namespace PlaneGearTestAlgorihm
{
    /// <summary>
    /// 【新兴盘齿方案3.5-注释】平面齿算法参数。配方永远 6 名；有 Models\\制程 权重则 OpenVINO Det，无则空跑。
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

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】本制程 Det（每实例只加载 Models\\该制程）。
        /// </summary>
        IVisionModel WH_det;

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】本制程权重路径。
        /// </summary>
        private string Model_Path;

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】本制程 classes.txt（现网多为锈蚀一行）。
        /// </summary>
        protected string[] class_names;

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
        /// 【新兴盘齿方案3.5-注释】有 WH_det 则 Infer；无模型空跑 IsOK=true。不 Clear AlgorithmOut。不写 PLC。
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

            if (WH_det is null)
            {
                OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " 无模型，空跑 IsOK=true");
                cell.IsOK = true;
                return;
            }

            DetectYolo(cell);
            cell.IsOK = true;
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】GetMatImage → ImageInferDet → 坐标还原 → ParseResult。
        /// </summary>
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
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " GetMatImage 为空，跳过 Infer");
                    return;
                }

                DetResult detResult = null;
                try
                {
                    detResult = ImageInferDet(WH_det, matimg);
                }
                catch (Exception ex)
                {
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " Predict 失败: " + ex.Message);
                    return;
                }

                if (detResult is null)
                {
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " Infer 无结果（模型未加载或 Predict 返回空）");
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
            LoadYoloModel();
            SetDefectRecipe(User);
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】按 User/PrcessName 加载 AlgorithmPlug\\PlaneGearTestAlgorihm\\Models\\该制程。无文件则日志返回，禁止 NRE。
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
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " 无算法参数，跳过加载模型");
                    return;
                }

                string dirPath = ".\\AlgorithmPlug\\PlaneGearTestAlgorihm\\Models\\" + processName + "\\";
                var names = GetNames(dirPath);
                if (string.IsNullOrEmpty(names.Item1) || names.Item2 is null)
                {
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " 无模型，空跑: " + dirPath);
                    return;
                }

                Model_Path = names.Item1;
                class_names = names.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (class_names.Length == 0)
                {
                    OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " classes.txt 为空，跳过加载: " + dirPath);
                    return;
                }

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
                    "【新兴盘齿方案3.5-注释】" + processName + " 模型已加载: " + Model_Path
                    + ", device=" + currentDevice
                    + ", classes=" + class_names.Length);
            }
            catch (Exception ex)
            {
                OperateLog?.Warn("【新兴盘齿方案3.5-注释】" + processName + " 加载模型失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】只扫制程目录根下：优先 *.model，再 onnx/xml/engine/pt；classes.txt 不递归。
        /// </summary>
        private (string, string[]) GetNames(string Dirpath)
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
        /// 【新兴盘齿方案3.5-注释】Det Infer。模型为空返回 null。
        /// </summary>
        public DetResult ImageInferDet(IVisionModel WH, Mat img)
        {
            if (WH != null)
            {
                DetResult resultDet;
                resultDet = WH.Predict(img) as DetResult;
                return resultDet;
            }

            return null;
        }

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】Mat 包 cell.Image.ImageData，不拷像素。
        /// </summary>
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
        /// 【新兴盘齿方案3.5-注释】只扫种别外观；Type=外观。配方 6 名未检出仍 Add 空框；对不上的类名丢弃并日志。
        /// </summary>
        protected void ParseResult(List<CoordRestoreData> sResultInfos, Cell cell)
        {
            if (sResultInfos is null || cell?.AlgorithmOut is null || DefectSpecies is null)
            {
                return;
            }

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

            foreach (var ds in DefectSpecies)
            {
                if (ds.Name != SpeciesAppearance)
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

        /// <summary>
        /// 【新兴盘齿方案3.5-注释】本插件内复制 CoordRestoreData→SRegion，不引用 GearTestAlgorihm。
        /// </summary>
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
            sRegioninfo.ColorDiffValue = info.Value;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                info.ShowLeftUp,
                info.ShowRightUp,
                info.ShowRightDown,
                info.ShowLeftDown,
            };

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            return detectRegion;
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

    /// <summary>
    /// 【新兴盘齿方案3.5-注释】精简自盘齿 SRegionInfo，供 GetDetectRegion 写区域特征。
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
        public double ContLen = 0;
        public double ColorDiffValue = 0;
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
            regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
            regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
            regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
            regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
            List<System.Windows.Point> pts = new List<System.Windows.Point>();
            for (int i = 0; i < regions.Count; i++)
            {
                pts.AddRange(regions[i].points);
            }
            return new SRegion(regionInfo, pts);
        }
    }

    /// <summary>
    /// 【新兴盘齿方案3.5-注释】精简自盘齿 CoordRestoreData，仅 DetData 轴对齐框。Value=分数供 CellDetection.Value。
    /// </summary>
    public struct CoordRestoreData
    {
        /// <summary>
        /// 【新兴盘齿方案3.5-注释】坐标还原。imgIndex=PhotoIndex-1，orgx/orgy=0。
        /// </summary>
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
            Value = Score;
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
