using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Flann;
using OpenVinoSharp.Extensions.process;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace GeneralMLOBBAlgorithm
{
    public class GeneralMLOBBAlgorithmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// yolo对象
        /// </summary>
        private YOLO yolo_text = new YOLO();

        private YOLO yolo_labeldefect = new YOLO();
        protected ModelType _ModelType = ModelType.YOLOv8Det;

        // internal string engine_type_str { get; set; }= "OpenVINO";

        // private string engine_type_str = "OpenVINO";
        //  private string engine_type_str = "TensorRT";

        //  string Model_Path = ".\\AlgorithmPlug\\YoloobbAlgorithm\\1024OBB-p99.engine";
        // string Model_Path = ".\\AlgorithmPlug\\YoloobbAlgorithm\\1024OBB-p99.onnx";
        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 模型文件夹
        /// </summary>
        //string Model_Dirpath = ".\\Models";
        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 模型路径
        /// </summary>
        private string Model_Path = ".\\AlgorithmPlug\\BottleAlgorithm\\Models\\";

        private string text_Model_Path;

        private string label_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        private string name_Path;

        protected string[] text_Model_Names;
        protected string[] LabelDetect_names;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        public GeneralMLOBBAlgorithmParam()
            : base()
        {
            text_Model_Path = Path.Combine(Model_Path, "threeDataModel.onnx");
            string textModelNamesPath = Path.Combine(Model_Path, "threeDataModelClasses.txt");
            text_Model_Names = File.ReadAllLines(textModelNamesPath);
            label_Model_Path = Path.Combine(Model_Path, "LabelDefectModel.onnx");
            string LabelDetectModelNamesPath = Path.Combine(
                Model_Path,
                "LabelDefectModelClasses.txt"
            );
            LabelDetect_names = File.ReadAllLines(LabelDetectModelNamesPath);
            //string[] searchPatterns = { "*.onnx", "*.engine", "*.pt" };

            //if (Directory.Exists(Model_Dirpath))
            //{
            //    var files = searchPatterns
            //    .SelectMany(pattern => Directory.GetFiles(Model_Dirpath, pattern))
            //    .ToList();

            //    if (files.Count>0)
            //    {
            //        Model_Path = files[0];
            //        name_Path= Model_Dirpath+ "\\classes.txt";
            //        Detect_names = File.ReadAllLines(name_Path);
            //    }

            //if (Detect_names?.Length > 0)
            //{
            //    List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
            //    DefectSpecies = new List<CDefectSpecies>();

            //    for (int i = 0; i < Detect_names.Length; i++)
            //    {
            //        CDefectRecipe defectRecipe = new CDefectRecipe(Detect_names[i], Category.区域);
            //        cDefectRecipes.Add(defectRecipe);
            //    }
            //    //CDefectRecipe defectRecipe1 = new CDefectRecipe("分数", Category.值);
            //    //cDefectRecipes.Add(defectRecipe1);

            //    CDefectSpecies defectSpecies = new CDefectSpecies("盐水袋", cDefectRecipes);
            //    DefectSpecies.Add(defectSpecies);
            //}

            //DefectFeatures = new();

            //DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            //DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            //DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            //DefectFeatures.Add(new("Score", "分数", "Score", ""));
            //DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));

            // LoadModel();

            //}
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParam(name, token));
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        ///
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Func<CImage, float> GetDistinctFunc()
        {
            return (CImage image) =>
            {
                return 0;
            };
        }

        protected void ReadNames(string modelDirpath)
        {
            text_Model_Names = File.ReadAllLines(name_Path);
            //AlgorithmType = "FrontAlgorithm";
            //string[] searchPatterns = { "*.onnx", "*.engine", "*.pt" };

            //if (Directory.Exists(modelDirpath))
            //{
            //    var files = searchPatterns
            //        .SelectMany(pattern => Directory.GetFiles(modelDirpath, pattern))
            //        .ToList();

            //    if (files.Count > 0)
            //    {
            //        string directory = Path.GetDirectoryName(files[0]);
            //        if (Directory.Exists(directory))
            //        {
            //            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(
            //                files[0]
            //            );

            //            string result = Path.Combine(directory, fileNameWithoutExtension);
            //            //Model_Path = result;
            //            name_Path = modelDirpath + "\\classes.txt";
            //            Detect_names = File.ReadAllLines(name_Path);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 执行算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            var paramClass = AlgorParams.FirstOrDefault(o => o.Name == ParamSelect) as CParam;
            if (paramClass != null)
            {
                Mat img = GetMatImage(cell, paramClass);
                List<BaseResult> sResultInfos = ImageInfer(img, paramClass.Score, paramClass.Nms);
                ParseResult(sResultInfos[0], cell, ModelType.YOLOv8Det, text_Model_Names);
                ParseResult(sResultInfos[1], cell, ModelType.YOLOv8Det, LabelDetect_names);
            }
        }

        protected void ParseResult(
            BaseResult sResultInfos,
            Cell cell,
            ModelType modelType,
            string[] classNames
        )
        {
            if (sResultInfos is null)
            {
                return;
            }

            foreach (var ds in DefectSpecies)
            {
                foreach (var de in ds.RecipeDefects)
                {
                    if (!classNames.Contains(de.Name))
                        continue;
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = ds.Name;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();
                    switch (modelType)
                    {
                        case ModelType.YOLOv8Det:
                            DetResult detrets = sResultInfos as DetResult;
                            var finds = detrets.find_all(info =>
                            {
                                int index = int.Parse(info.lable);
                                return classNames[index] == de.Name;
                            });
                            if (finds.Count > 0)
                            {
                                foreach (var item in finds)
                                {
                                    SRegion sRegion = GetDetectRegion(item);

                                    cellDetection1.regionOut.Add(sRegion);
                                }
                            }
                            //cell.AlgorithmOut.Add(cellDetection1);

                            break;

                        case ModelType.YOLOv8Obb:
                            ObbResult obbrets = sResultInfos as ObbResult;
                            var findobbs = obbrets.find_all(info =>
                            {
                                int index = int.Parse(info.lable);
                                return classNames[index] == de.Name;
                            });
                            if (findobbs.Count > 0)
                            {
                                foreach (var item in findobbs)
                                {
                                    SRegion sRegion = GetDetectRegion(item);

                                    cellDetection1.regionOut.Add(sRegion);
                                }
                            }
                            //infos.ForEach(info => sResultInfos.Remove(info));
                            break;

                        default:
                            break;
                    }
                    cell.AlgorithmOut.Add(cellDetection1);
                }
            }
        }

        [OnDeserialized]
        private void LoadModel(StreamingContext context)
        {
            CParam param = AlgorParams[0] as CParam;

            ModelType model_type = ModelType.YOLOv8Det;
            // ModelType model_type = param.ModelType;
            // EngineType engine_type = MyEnum.GetEngineType<EngineType>(engine_type_str);
            EngineType engine_type = param.EngineType;

            //if ((model_type == ModelType.YOLOv8Det) || (model_type == ModelType.YOLOWorld))
            //{
            //    infer_type = "det";
            //}
            //else if (
            //    (model_type == ModelType.YOLOv9Seg)
            //    || (model_type == ModelType.YOLOv8Seg)
            //    || (model_type == ModelType.YOLOv5Seg)
            //)
            //{
            //    infer_type = "seg";
            //}
            //else if ((model_type == ModelType.YOLOv8Pose))
            //{
            //    infer_type = "pose";
            //}
            //else if ((model_type == ModelType.YOLOv8Obb))
            //{
            //    infer_type = "obb";
            //}
            //else if ((model_type == ModelType.YOLOv8Cls))
            //{
            //    infer_type = "cls";
            //}

            //string extension = Path.GetExtension(Model_Path);
            //if (EngineType.TensorRT == engine_type)
            //{
            //    //if ((extension != ".engine") && (extension == ".onnx"))
            //    //{
            //    //    OnnxToEngine from = new OnnxToEngine(Model_Path);
            //    //    from.Show();
            //    //    string directory = Path.GetDirectoryName(Model_Path);
            //    //    string file = Path.GetFileNameWithoutExtension(Model_Path);
            //    //    Model_Path = Path.Combine(directory, file) + ".engine";

            //    //    return;
            //    //}
            //    //else if (extension == ".engine") { }
            //    //else
            //    //{
            //    //   // show_worn_msg_box("Please select the correct model format.");
            //    //    return;
            //    //}
            //}
            //else
            //{
            //    if (
            //        (
            //            extension == ".onnx"
            //            && (
            //                EngineType.ONNX == engine_type
            //                || EngineType.OpenVINO == engine_type
            //                || EngineType.OpenCV == engine_type
            //            )
            //        ) || (extension == ".xml" && EngineType.OpenVINO == engine_type)
            //    ) { }
            //    else
            //    {
            //        // show_worn_msg_box("Please select the correct model format.");
            //        return;
            //    }
            //}

            yolo_text.Dispose();
            yolo_labeldefect.Dispose();
            if (param != null)
            {
                string CurrentDevice = param.CurrentDevice;
                int Categ_num = text_Model_Names.Length;
                float Score = param.Score;
                float Nms = param.Nms;
                int Input_size = param.Input_size;
                ImgSize Output_size = param.Output_size;
                //string model_path =
                //    param.EngineType == EngineType.TensorRT
                //        ? Model_Path + ".engine"
                //        : Model_Path + ".onnx";
                yolo_text = YOLO.GetYolo(
                    model_type,
                    text_Model_Path,
                    engine_type,
                    CurrentDevice,
                    Categ_num,
                    Score,
                    Nms,
                    Input_size,
                    Output_size
                );
                yolo_labeldefect = YOLO.GetYolo(
                    model_type,
                    label_Model_Path,
                    engine_type,
                    CurrentDevice,
                    Categ_num,
                    Score,
                    Nms,
                    Input_size,
                    Output_size
                );
            }
        }

        public List<BaseResult> ImageInfer(Mat img, float score, float nms)
        {
            List<BaseResult> sResultInfos = new List<BaseResult>();
            BaseResult textresult,
                labelresult;
            textresult = yolo_text.predict(img, score, nms);
            labelresult = yolo_labeldefect.predict(img, score, nms);
            sResultInfos.Add(textresult);
            sResultInfos.Add(labelresult);
            //if (result != null)
            //{
            //    for (int i = 0; i < result.count; i++)
            //    {
            //        sResultInfos.Add(obbResult.datas[i]);
            //    }
            //}
            return sResultInfos;
        }

        protected SRegion GetDetectRegion(DetData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.box.Size.Width;
            sRegioninfo.ShorLen = info.box.Size.Height;
            sRegioninfo.Phi = 0f;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.score;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                new System.Windows.Point(info.box.X, info.box.Y),
                new System.Windows.Point(info.box.X + info.box.Width, info.box.Y),
                new System.Windows.Point(info.box.X + info.box.Width, info.box.Y + info.box.Height),
                new System.Windows.Point(info.box.X, info.box.Y + info.box.Height),
            };
            //rec1Points.Add(new System.Windows.Point(info.box.X, info.box.Y));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            var rect = info.box;
            detectRegion.rect = new System.Windows.Rect(
                new System.Windows.Point(rect.TopLeft.X, rect.TopLeft.Y),
                new System.Windows.Size(rect.Width, rect.Height)
            );
            return detectRegion;
        }

        protected SRegion GetDetectRegion(ObbData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.box.Size.Width;
            sRegioninfo.ShorLen = info.box.Size.Height;
            sRegioninfo.Phi = info.box.Angle;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.score * 100.0f;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
            info.box.Points().ForEach(p => rec1Points.Add(new System.Windows.Point(p.X, p.Y)));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            var rect = info.box.BoundingRect();
            detectRegion.rect = new System.Windows.Rect(
                new System.Windows.Point(rect.TopLeft.X, rect.TopLeft.Y),
                new System.Windows.Size(rect.Width, rect.Height)
            );
            return detectRegion;
        }

        //private void GetRecLen(List<Point2f> rec2Points, out double LongLen, out double ShorLen,out double phi)
        //{
        //    double templen1 = 0;
        //    double templen2 = 0;
        //    phi = 0;
        //    templen1 = CalculateDistance(rec2Points[0], rec2Points[1]);
        //    templen2 = CalculateDistance(rec2Points[1], rec2Points[2]);

        //    if (templen1 > templen2)
        //    {
        //        LongLen = templen1;
        //        ShorLen = templen2;
        //        phi = Math.Atan((rec2Points[1].X - rec2Points[0].X) / (rec2Points[1].Y - rec2Points[0].Y));
        //    }
        //    else
        //    {
        //        LongLen = templen2;
        //        ShorLen = templen1;
        //    }
        //}

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 计算矩形长短边
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private double CalculateDistance(ObbData point1, ObbData point2)
        {
            double deltaX = point1.box.Center.X - point2.box.Center.X;
            double deltaY = point1.box.Center.Y - point2.box.Center.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 计算矩形面积
        /// </summary>
        /// <param name="len1"></param>
        /// <param name="len2"></param>
        /// <returns></returns>
        private double GetRecArea(double len1, double len2)
        {
            return len1 * len2;
        }

        public virtual Mat GetMatImage(Cell cell, CParamBase param)
        {
            Mat img = new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                cell.Image.ImageData
            );
            return img;
        }
    }

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// AI参数类
    /// </summary>
    public partial class CParam : CParamBase
    {
        public CParam()
            : base() { }

        public CParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 最小分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("最小分数阈值")]
        [property: Description("最小分数阈值")]
        private float score = 0.6f;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// NMScore
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("NMScore")]
        [property: Description("NMScore")]
        private float nms = 0.5f;

        ///// <summary>
        ///// 2024.10.28 鲍赞宝
        ///// 缺陷类型数量
        ///// </summary>
        //[ObservableProperty]
        //[property: Category("基础参数")]
        //[property: DisplayName("缺陷类型数量")]
        //[property: Description("已经标注的缺陷类型数量")]
        //private int categ_num = 3;

        /// <summary>
        /// 20250331 TCG
        /// 模型尺寸
        /// </summary>
        [ObservableProperty]
        [property: Category("尺寸参数")]
        [property: DisplayName("模型尺寸1")]
        [property: Description("模型尺寸1")]
        private int input_size = 640;

        /// <summary>
        /// 20250331 TCG
        /// 模型尺寸
        /// </summary>
        [ObservableProperty]
        [property: Category("尺寸参数")]
        [property: DisplayName("模型尺寸2")]
        [property: Description("模型尺寸2")]
        private ImgSize output_size = ImgSize.S640;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("加载参数")]
        [property: DisplayName("驱动设备")]
        [property: Description("驱动设备")]
        private string currentDevice = "GPU.0";

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("加载参数")]
        [property: DisplayName("平台")]
        [property: Description("平台")]
        private EngineType engineType = EngineType.OpenVINO;

        //[ObservableProperty]
        //[property: Category("设置界面")]
        //[property: DisplayName("打开设置界面")]
        //[property: Description("打开设置界面")]
        //[property: Editor(
        //    typeof(COpenSetWindowPropertyEditor),
        //    typeof(COpenSetWindowPropertyEditor)
        //)]
        //private OpenSetWindowProperty isOpened = new OpenSetWindowProperty("123", false);
    }
}
