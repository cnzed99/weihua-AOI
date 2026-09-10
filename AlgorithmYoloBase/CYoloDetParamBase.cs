using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using AlgorithmDll;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using WH.RecipeCellRootBase;
using WH.RunCell;
using WH.VisionLearning;

namespace AlgorithmYoloBase
{
    /// YOLO Det 模板方法骨架
    public abstract class CYoloDetParamBase : CAlgorithmParamBase
    {
        public string User { get; set; }

        public double MmPerPixel { get; set; }

        protected IVisionModel WH_det;

        protected string Model_Path;

        protected string[] class_names;

        protected const int InputSize = 640;

        protected abstract string LogTag { get; }

        protected abstract string PluginFolderName { get; }

        protected abstract string AppearanceSpeciesName { get; }

        protected abstract bool ShouldSkipYoloIfNoModel { get; }

        protected abstract IReadOnlyList<string> GetModelSearchDirs(string processName);

        protected virtual bool IsModelMissing => WH_det is null;

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
            BindCellProcessName(cell, processName);
            OnBeforeYolo(cell, processName);
            if (ShouldSkipYoloIfNoModel && IsModelMissing)
            {
                OnNoModel(cell, processName);
                return;
            }

            DetectYolo(cell);
            OnAfterYolo(cell, processName);
            cell.IsOK = true;
        }

        protected virtual void BindCellProcessName(Cell cell, string processName) { }

        protected virtual void OnBeforeYolo(Cell cell, string processName) { }

        protected virtual void OnAfterYolo(Cell cell, string processName) { }

        protected virtual void OnNoModel(Cell cell, string processName)
        {
            OperateLog?.Warn(LogTag + processName + " 无模型，空跑 IsOK=true");
            cell.IsOK = true;
        }

        protected virtual void AfterModelLoaded(string processName) { }

        protected abstract CoordRestoreData CreateRestoreData(Cell cell, string labelname, DetData det);

        protected abstract void SetDefectRecipe(string processName);

        protected abstract CParamBase CreateParam(string name);

        protected virtual void InitDefectFeatures()
        {
            DefectFeatures = new List<CFeacture>();
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
        }

        /// 默认 RecWidth &gt; RecHeight（新兴/曲轴）;盘齿 override 保持 RecWidth &gt; RecWidth
        protected virtual SRegion GetDetectRegion(CoordRestoreData info)
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

            return new SRegion(sRegioninfo, rec1Points);
        }

        protected void DetectYolo(Cell cell)
        {
            Mat matimg = null;
            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            try
            {
                var paramClass = AlgorParams?.FirstOrDefault(o => o.Name == ParamSelect) as CYoloInferParam;
                if (paramClass is null)
                {
                    paramClass = AlgorParams?.FirstOrDefault() as CYoloInferParam;
                }

                // 推理前把算法栏 Score/Nms 刷进已加载模型，改完立刻生效。拉链插件不走本基类。
                if (paramClass != null)
                {
                    WH_det?.UpdateNMS_Score(paramClass.Nms, paramClass.Score);
                }

                matimg = GetMatImage(cell, paramClass);
                if (matimg is null)
                {
                    OperateLog?.Warn(LogTag + processName + " GetMatImage 为空，跳过 Infer");
                    return;
                }

                DetResult detResult = null;
                try
                {
                    detResult = ImageInferDet(WH_det, matimg);
                }
                catch (Exception ex)
                {
                    OperateLog?.Warn(LogTag + processName + " Predict 失败: " + ex.Message);
                    return;
                }

                if (detResult is null)
                {
                    OperateLog?.Warn(LogTag + processName + " Infer 无结果（模型未加载或 Predict 返回空）");
                    return;
                }

                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (detResult.datas != null && class_names != null && cell.Image != null)
                {
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
                        dets.Add(CreateRestoreData(cell, labelname, detResult.datas[j]));
                    }
                }

                ParseAppearance(dets, cell);
            }
            finally
            {
                matimg?.Dispose();
            }
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

            return new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                cell.Image.ImageData);
        }

        protected virtual void WarnUnmatchedAppearanceLabels(List<CoordRestoreData> sResultInfos) { }

        protected void ParseAppearance(List<CoordRestoreData> sResultInfos, Cell cell)
        {
            if (sResultInfos is null)
            {
                return;
            }

            if (cell?.AlgorithmOut is null || DefectSpecies is null)
            {
                return;
            }

            WarnUnmatchedAppearanceLabels(sResultInfos);

            foreach (var ds in DefectSpecies)
            {
                if (ds.Name != AppearanceSpeciesName)
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
                    cellDetection1.Type = AppearanceSpeciesName;
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

        protected void LoadYoloModel()
        {
            string processName = string.IsNullOrEmpty(User) ? PrcessName : User;
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            try
            {
                CYoloInferParam param = AlgorParams?.FirstOrDefault() as CYoloInferParam;
                if (param is null)
                {
                    OperateLog?.Warn(LogTag + processName + " 无算法参数，跳过加载模型");
                    return;
                }

                IReadOnlyList<string> dirs = GetModelSearchDirs(processName);
                (string, string[]) names = ("", null);
                string usedDir = null;
                if (dirs != null)
                {
                    foreach (string dir in dirs)
                    {
                        names = GetNames(dir);
                        if (!string.IsNullOrEmpty(names.Item1) && names.Item2 != null)
                        {
                            usedDir = dir;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(names.Item1) || names.Item2 is null)
                {
                    WarnModelNotFound(processName, dirs);
                    return;
                }

                Model_Path = names.Item1;
                class_names = names.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (class_names.Length == 0)
                {
                    OnEmptyClassNames(processName, usedDir);
                    return;
                }

                AfterModelLoaded(processName);

                string currentDevice = string.IsNullOrWhiteSpace(param.CurrentDevice) ? "CPU" : param.CurrentDevice;
                float score = param.Score;
                float nms = param.Nms;
                WH_det = VisionModelExtensions.GetVisionModel(
                    ModelType.VisionModelDet,
                    Model_Path,
                    EngineType.OpenVINO,
                    currentDevice,
                    class_names.Length,
                    score,
                    nms,
                    InputSize);
                OperateLog?.Info(
                    LogTag + processName + " 模型已加载: " + Model_Path
                    + ", device=" + currentDevice
                    + ", classes=" + class_names.Length);
            }
            catch (Exception ex)
            {
                OperateLog?.Warn(LogTag + processName + " 加载模型失败: " + ex.Message);
                OnLoadModelFailed();
            }
        }

        protected virtual void WarnModelNotFound(string processName, IReadOnlyList<string> dirs)
        {
            string dirPath = (dirs != null && dirs.Count > 0) ? dirs[0] : "";
            OperateLog?.Warn(LogTag + processName + " 无模型文件，跳过加载: " + dirPath);
        }

        protected virtual void OnEmptyClassNames(string processName, string dirPath)
        {
            OperateLog?.Warn(LogTag + processName + " classes.txt 为空，跳过加载: " + dirPath);
        }

        protected virtual void OnLoadModelFailed() { }

        protected virtual (string, string[]) GetNames(string Dirpath)
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

                return ("", new string[1] { "" });
            }

            return ("", new string[1] { "" });
        }

        protected void OnDeserializedCore(StreamingContext context)
        {
            if (string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(PrcessName))
            {
                User = PrcessName;
            }

            if (AlgorParams is null)
            {
                AlgorParams = new ObservableCollection<CParamBase>();
            }
            if (AlgorParams.Count == 0)
            {
                AddParam("分组1");
            }

            InitDefectFeatures();
            LoadYoloModel();
            SetDefectRecipe(User);
        }

        protected void InitYoloAfterConstruct(string user)
        {
            User = user ?? "";
            PrcessName = User;
            InitDefectFeatures();
            LoadYoloModel();
            SetDefectRecipe(User);
        }
    }
}
