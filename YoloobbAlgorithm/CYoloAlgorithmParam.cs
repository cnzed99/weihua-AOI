using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Flann;
using OpenVinoSharp.Extensions.process;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace YoloobbAlgorithm
{
    public class CYoloAlgorithmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// yolo对象
        /// </summary>
        private YOLO yolo = new YOLO();

        private string infer_type = "det";


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
        string Model_Path;
        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        string name_Path;


        protected string[] Detect_names;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        public CYoloAlgorithmParam()
            : base()
        {
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
            //AlgorithmType = "FrontAlgorithm";
            string[] searchPatterns = { "*.onnx", "*.engine", "*.pt" };

            if (Directory.Exists(modelDirpath))
            {
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(modelDirpath, pattern))
                .ToList();

                if (files.Count > 0)
                {
                    Model_Path = files[0];
                    name_Path = modelDirpath + "\\classes.txt";
                    Detect_names = File.ReadAllLines(name_Path);
                }

            }

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
                List<ObbData> sResultInfos = ImageInfer(img, paramClass.Score, paramClass.Nms);
                if (sResultInfos.Count == 0) { return; }

                foreach (var ds in DefectSpecies)
                {
                    foreach (var de in ds.RecipeDefects)
                    {
                        CellDetection cellDetection1 = new CellDetection();
                        cellDetection1.Type = ds.Name;
                        cellDetection1.Category = de.Category;
                        cellDetection1.RecipeDefectName = de.Name;
                        cellDetection1.Value = new List<float>();
                        List<ObbData> infos = new List<ObbData>();
                        sResultInfos.ForEach(info =>
                        {
                            int index = int.Parse(info.lable);
                            if (Detect_names[index] == de.Name)
                            {

                                SRegion sRegion = GetDetectRegion(info);

                                cellDetection1.regionOut.Add(sRegion);
                                infos.Add(info);
                            }
                        });
                        cell.AlgorithmOut.Add(cellDetection1);
                        infos.ForEach(info => sResultInfos.Remove(info));
                    }
                }
            }

        }
        [OnDeserialized]
        void LoadModel(StreamingContext context)
        {
            CParam param = AlgorParams[0] as CParam;
             string model_type_str = "YOLOv8Obb";

             ModelType model_type = MyEnum.GetModelType<ModelType>(model_type_str);
           // ModelType model_type = param.ModelType;
            // EngineType engine_type = MyEnum.GetEngineType<EngineType>(engine_type_str);
            EngineType engine_type = param.EngineType;

            if ((model_type == ModelType.YOLOv8Det) || (model_type == ModelType.YOLOWorld))
            {
                infer_type = "det";
            }
            else if (
                (model_type == ModelType.YOLOv9Seg)
                || (model_type == ModelType.YOLOv8Seg)
                || (model_type == ModelType.YOLOv5Seg)
            )
            {
                infer_type = "seg";
            }
            else if ((model_type == ModelType.YOLOv8Pose))
            {
                infer_type = "pose";
            }
            else if ((model_type == ModelType.YOLOv8Obb))
            {
                infer_type = "obb";
            }
            else if ((model_type == ModelType.YOLOv8Cls))
            {
                infer_type = "cls";
            }

            string extension = Path.GetExtension(Model_Path);
            if (EngineType.TensorRT == engine_type)
            {
                //if ((extension != ".engine") && (extension == ".onnx"))
                //{
                //    OnnxToEngine from = new OnnxToEngine(Model_Path);
                //    from.Show();
                //    string directory = Path.GetDirectoryName(Model_Path);
                //    string file = Path.GetFileNameWithoutExtension(Model_Path);
                //    Model_Path = Path.Combine(directory, file) + ".engine";

                //    return;
                //}
                //else if (extension == ".engine") { }
                //else
                //{
                //   // show_worn_msg_box("Please select the correct model format.");
                //    return;
                //}
            }
            else
            {
                if (
                    (
                        extension == ".onnx"
                        && (
                            EngineType.ONNX == engine_type
                            || EngineType.OpenVINO == engine_type
                            || EngineType.OpenCV == engine_type
                        )
                    ) || (extension == ".xml" && EngineType.OpenVINO == engine_type)
                ) { }
                else
                {
                    // show_worn_msg_box("Please select the correct model format.");
                    return;
                }
            }

            yolo.Dispose();
            if (param != null)
            {
                string CurrentDevice = param.CurrentDevice;
                int Categ_num = param.Categ_num;
                float Score = param.Score;
                float Nms = param.Nms;
                InputImgSize Input_size = param.Input_size;

                yolo = YOLO.GetYolo(
                    model_type,
                    Model_Path,
                    engine_type,
                    CurrentDevice,
                    Categ_num,
                    Score,
                    Nms,
                    Input_size
                );
            }
        }

        public List<ObbData> ImageInfer(Mat img, float score, float nms)
        {
            List<ObbData> sResultInfos = new List<ObbData>();
            BaseResult result;
            result = yolo.predict(img, score, nms);
            ObbResult? obbResult = result as ObbResult;

            if (obbResult != null)
            {

                for (int i = 0; i < obbResult.count; i++)
                {
                    sResultInfos.Add(obbResult.datas[i]);
                }
            }
            return sResultInfos;
        }

        public SRegion GetDetectRegion(ObbData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.box.Size.Width;
            sRegioninfo.ShorLen = info.box.Size.Height;
            sRegioninfo.Phi = info.box.Angle;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.score*100.0f;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
            info.box.Points().ForEach(p => rec1Points.Add(new System.Windows.Point(p.X, p.Y)));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            var rect = info.box.BoundingRect();
            detectRegion.rect = new System.Windows.Rect(new System.Windows.Point(rect.TopLeft.X, rect.TopLeft.Y), new System.Windows.Size(rect.Width, rect.Height));
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
          cell.Image.ImageData);
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
        [property: DisplayName("01.最小分数阈值")]
        [property: Description("最小分数阈值")]
        private float score = 0.6f;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// NMScore
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("02.NMScore")]
        [property: Description("NMScore")]
        private float nms = 0.5f;


        /// <summary>
        /// 2025.3.1 鲍赞宝
        /// 工程类型
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("03.工程类型")]
        [property: Description("工程类型")]
        private EngineType engineType = EngineType.OpenVINO;



        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 缺陷类型数量
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("04.缺陷类型数量")]
        [property: Description("已经标注的缺陷类型数量")]
        private int categ_num = 1;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 输入图像大小
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("05.输入图片大小")]
        [property: Description("检测的图片精度")]
        private InputImgSize input_size = InputImgSize.IN640;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("06.驱动设备")]
        [property: Description("驱动设备")]
        private string currentDevice = "GPU.0";
    }



}
