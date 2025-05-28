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
        /// 拉链识别模型
        /// </summary>
        private YOLO yolo_zipperRecognize = new YOLO();

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

        private string zipperRecognize_Model_Path;

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
            zipperRecognize_Model_Path = Path.Combine(Model_Path, "threeDataModel.onnx");
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

            yolo_zipperRecognize.Dispose();
            yolo_labeldefect.Dispose();
            if (param != null)
            {
                string CurrentDevice = param.CurrentDevice;
                int text_Categ_num = text_Model_Names.Length;
                int label_Categ_num = LabelDetect_names.Length;
                float Score = param.Score;
                float Nms = param.Nms;
                InputImgSize Input_size = param.Input_size;
                ImgSize Output_size = param.Output_size;
                //string model_path =
                //    param.EngineType == EngineType.TensorRT
                //        ? Model_Path + ".engine"
                //        : Model_Path + ".onnx";
                yolo_zipperRecognize = YOLO.GetYolo(
                    model_type,
                    zipperRecognize_Model_Path,
                    engine_type,
                    CurrentDevice,
                    text_Categ_num,
                    Score,
                    Nms,
                    Input_size
                );
                //yolo_labeldefect = YOLO.GetYolo(
                //    model_type,
                //    label_Model_Path,
                //    engine_type,
                //    CurrentDevice,
                //    label_Categ_num,
                //    Score,
                //    Nms,
                //    Input_size
                //);
            }
        }

        public List<BaseResult> ImageInfer(Mat img, float score, float nms)
        {
            List<BaseResult> sResultInfos = new List<BaseResult>();
            BaseResult textresult,
                labelresult;
            textresult = yolo_zipperRecognize.predict(img, score, nms);
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
        private float nms = 0.5f;


        /// <summary>
        /// 20250331 TCG
        /// 模型尺寸
        /// </summary>
        [ObservableProperty]
        private InputImgSize input_size = InputImgSize.IN640;

        /// <summary>
        /// 20250331 TCG
        /// 模型尺寸
        /// </summary>
        [ObservableProperty]
        private ImgSize output_size = ImgSize.S640;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        private string currentDevice = "GPU.0";

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
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