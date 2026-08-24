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

namespace GearTestAlgorihm
{
    /// <summary>
    /// 盘齿算法参数（方案3.4）。
    /// 仅「整轴侧面」加载 OpenVINO Det 并 Infer；其它制程仍空壳。
    /// </summary>
    public class CGearTestAlgorihmParam : CAlgorithmParamBase
    {
        public string User { get; set; }

        /// <summary>
        /// 面积换算默认值（方案3.4）；标定后以相机 cell.MmPerPixel 为准。
        /// </summary>
        public double MmPerPixel { get; set; } = 0.04;

        /// <summary>
        /// 【盘齿方案3.4-注释】整轴侧面 Det 模型。
        /// </summary>
        IVisionModel WH_Side_det;

        /// <summary>
        /// 【盘齿方案3.4-注释】整轴侧面模型路径。
        /// </summary>
        private string Side_Model_Path;

        /// <summary>
        /// 【盘齿方案3.4-注释】整轴侧面 classes.txt 类别名。
        /// </summary>
        protected string[] Side_names;

        public CGearTestAlgorihmParam()
            : this("")
        {
        }

        public CGearTestAlgorihmParam(string user)
            : base()
        {
            User = user ?? "";
            PrcessName = User;
            //【盘齿方案3.4-注释】分组1 已由 CAlgorithmParamBase 构造 AddParam；override AddParam 会在 base() 里写入 CParam
            SetDefectRecipe(User);
            InitDefectFeatures();
            LoadWholeShaftSideModel();
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
        /// 非整轴侧面：不推理、不写 AlgorithmOut。过滤阶段会把无缺陷判为 OK。
        /// 上端面（倒角偏、孔钻错）、上轴侧面（小孔未钻）走 Halcon 空壳，仍返回空列表。
        /// 整轴侧面：OpenVINO Det Infer，ParseResult Type=外观。
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
            if (processName == "整轴侧面")
            {
                DetectWholeShaftSide(cell);
            }
            else if (processName == "上轴侧面" || processName == "上端面")
            {
                RunHalconEmpty(cell);
            }

            cell.IsOK = true;
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】仅整轴侧面：GetMatImage → ImageInferDet → 坐标还原 → ParseResult。
        /// </summary>
        private void DetectWholeShaftSide(Cell cell)
        {
            Mat matimg = null;
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
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面 GetMatImage 为空，跳过 Infer");
                    return;
                }

                DetResult detResult = null;
                try
                {
                    detResult = ImageInferDet(WH_Side_det, matimg);
                }
                catch (Exception ex)
                {
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面 Predict 失败: " + ex.Message);
                    return;
                }

                if (detResult is null)
                {
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面 Infer 无结果（模型未加载或 Predict 返回空）");
                    return;
                }

                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (detResult.datas != null && Side_names != null && cell.Image != null)
                {
                    int imgIndex = cell.PhotoIndex - 1;
                    for (int j = 0; j < detResult.datas.Count; j++)
                    {
                        if (!int.TryParse(detResult.datas[j].lable, out int labelindex))
                        {
                            continue;
                        }
                        if (labelindex < 0 || labelindex >= Side_names.Length)
                        {
                            continue;
                        }
                        string labelname = Side_names[labelindex];
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
        /// Halcon 空壳（方案3.1）：上端面=倒角偏+孔钻错。
        /// 后续接入见 HDev/05。本阶段不引用 Halcon 运行时。
        /// </summary>
        protected List<CellDetection> RunHalconEmpty(Cell cell)
        {
            return new List<CellDetection>();
        }

        /// <summary>
        /// 方案0 A5：反序列化时空守卫。空工程无模型路径时不得 NRE。
        /// DefectSpecies 带 [JsonIgnore]，必须在此重建。
        /// 【盘齿方案3.4-注释】整轴侧面在此加载 OpenVINO Det。
        /// </summary>
        [OnDeserialized]
        private void LoadModel(StreamingContext context)
        {
            if (string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(PrcessName))
            {
                User = PrcessName;
            }

            //【盘齿方案3.4-注释】反序列化可能不走完整构造或 JSON 把 AlgorParams 冲成空，此处才需要补分组1
            if (AlgorParams is null)
            {
                AlgorParams = new System.Collections.ObjectModel.ObservableCollection<CParamBase>();
            }
            if (AlgorParams.Count == 0)
            {
                AddParam("分组1");
            }

            SetDefectRecipe(User);
            InitDefectFeatures();
            LoadWholeShaftSideModel();
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】仅 User/PrcessName==整轴侧面 时加载模型。无文件或 names 空则打日志返回，禁止 NRE。
        /// </summary>
        private void LoadWholeShaftSideModel()
        {
            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            if (processName != "整轴侧面")
            {
                return;
            }

            try
            {
                CParam param = AlgorParams?.FirstOrDefault() as CParam;
                if (param is null)
                {
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面无算法参数，跳过加载模型");
                    return;
                }

                string dirPath = ".\\AlgorithmPlug\\GearTestAlgorihm\\Models\\" + processName + "\\";
                var names = GetNames(dirPath);
                if (string.IsNullOrEmpty(names.Item1) || names.Item2 is null)
                {
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面无模型文件，跳过加载: " + dirPath);
                    return;
                }

                Side_Model_Path = names.Item1;
                Side_names = names.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (Side_names.Length == 0)
                {
                    OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面 classes.txt 为空，跳过加载: " + dirPath);
                    return;
                }

                string currentDevice = string.IsNullOrWhiteSpace(param.CurrentDevice) ? "CPU" : param.CurrentDevice;
                float score = param.Score;
                float nms = param.Nms;
                const int inputSize = 640;
                WH_Side_det = VisionModelExtensions.GetVisionModel(
                    ModelType.VisionModelDet,
                    Side_Model_Path,
                    EngineType.OpenVINO,
                    currentDevice,
                    Side_names.Length,
                    score,
                    nms,
                    inputSize);
                OperateLog?.Info(
                    "【盘齿方案3.4-注释】整轴侧面模型已加载: " + Side_Model_Path
                    + ", device=" + currentDevice
                    + ", classes=" + Side_names.Length);
            }
            catch (Exception ex)
            {
                OperateLog?.Warn("【盘齿方案3.4-注释】整轴侧面加载模型失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】目录内取 files[0] 作为模型、首个 txt 作为类别。每制程保持 1 个 .model + 1 个 classes.txt。
        /// </summary>
        private (string, string[]) GetNames(string Dirpath)
        {
            if (Directory.Exists(Dirpath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model" };
                var files = searchPatterns
                    .SelectMany(pattern => Directory.GetFiles(Dirpath, pattern))
                    .ToList();

                var classNames = Directory.GetFiles(Dirpath, "*.txt", SearchOption.AllDirectories);

                if (files.Count > 0 && classNames.Length > 0)
                {
                    string model_Path = files[0];
                    string name_Path = classNames[0];
                    string[] de_names = File.ReadAllLines(name_Path);
                    return (model_Path, de_names);
                }
                else
                {
                    return ("", new string[1] { "" });
                }
            }
            else
            {
                return ("", new string[1] { "" });
            }
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】Det Infer。模型为空返回 null。
        /// </summary>
        public DetResult ImageInferDet(IVisionModel WH, Mat img)
        {
            if (WH != null)
            {
                DetResult resultDet;
                resultDet = WH.Predict(img) as DetResult;
                return resultDet;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】Mat 包 cell.Image.ImageData，不拷像素。
        /// </summary>
        public virtual Mat GetMatImage(Cell cell, CParamBase param)
        {
            if (cell.Image == null) return null;

            Mat img = new Mat(
           cell.Image.ImageHeight,
           cell.Image.ImageWidth,
           MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
           cell.Image.ImageData
             );
            return img;
        }

        /// <summary>
        /// 【盘齿方案3.4-注释】只扫种别外观；CellDetection.Type 固定「外观」。几何项不写入本次 YOLO 输出。
        /// </summary>
        protected void ParseResult(List<CoordRestoreData> sResultInfos, Cell cell)
        {
            if (sResultInfos is null)
            {
                return;
            }

            foreach (var ds in DefectSpecies)
            {
                if (ds.Name != "外观")
                {
                    continue;
                }

                foreach (var de in ds.RecipeDefects)
                {
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = "外观";
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
        /// 【盘齿方案3.4-注释】精简自拉链 GetDetectRegion(CoordRestoreData)，本插件内复制，不引用金属拉链工程。
        /// </summary>
        protected SRegion GetDetectRegion(CoordRestoreData info)
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

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);

            return detectRegion;
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
                    Area("小孔未钻"); // YOLO 区域，3.8 前不走 Halcon
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

        /// <summary>
        /// 【盘齿方案3.4-注释】推理设备，默认 CPU。
        /// </summary>
        [Category("基础参数")]
        [DisplayName("01驱动器")]
        [Description("驱动器")]
        public string CurrentDevice { get; set; } = "CPU";

        /// <summary>
        /// 【盘齿方案3.4-注释】检测分数阈值。
        /// </summary>
        [Category("分数设置")]
        [DisplayName("01 分数阈值")]
        [Description("分数阈值")]
        public float Score { get; set; } = 0.3f;

        /// <summary>
        /// 【盘齿方案3.4-注释】NMS，与拉链默认一致。
        /// </summary>
        public float Nms { get; set; } = 0.5f;
    }

    /// <summary>
    /// 【盘齿方案3.4-注释】精简自拉链 SRegionInfo，供 GetDetectRegion 写区域特征。
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
                case "LongLength":
                    return LongLen;
                case "ShortLength":
                    return ShorLen;
                case "Angle":
                    return Phi;
                case "Score":
                    return Score;
                case "ColorDiffValue":
                    return ColorDiffValue;
                case "PositionX":
                    return PositionX;
                case "PositionY":
                    return PositionY;
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
    /// 【盘齿方案3.4-注释】精简自拉链 CoordRestoreData，仅保留 DetData 轴对齐框构造。
    /// </summary>
    public struct CoordRestoreData
    {
        /// <summary>
        /// 【盘齿方案3.4-注释】坐标还原。imgIndex=PhotoIndex-1，orgx/orgy=0。
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
