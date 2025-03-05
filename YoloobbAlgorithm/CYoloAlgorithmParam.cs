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
        string Model_Dirpath = ".\\AlgorithmPlug\\YoloobbAlgorithm\\Models";
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


        string[] Detect_names;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        public CYoloAlgorithmParam()
            : base()
        {
            //AlgorithmType = "FrontAlgorithm";
            string[] searchPatterns = { "*.onnx", "*.engine", "*.pt" }; 

            if (Directory.Exists(Model_Dirpath))
            {
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(Model_Dirpath, pattern))
                .ToList();

                if (files.Count>0)
                {
                    Model_Path = files[0];
                    name_Path= Model_Dirpath+ "\\classes.txt";
                    Detect_names = File.ReadAllLines(name_Path);
                }

                if (Detect_names?.Length > 0)
                {
                    List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
                    DefectSpecies = new List<CDefectSpecies>();

                    for (int i = 0; i < Detect_names.Length; i++)
                    {
                        CDefectRecipe defectRecipe = new CDefectRecipe(Detect_names[i], Category.区域);
                        cDefectRecipes.Add(defectRecipe);
                    }
                    CDefectRecipe defectRecipe1 = new CDefectRecipe("分数", Category.值);
                    cDefectRecipes.Add(defectRecipe1);

                    CDefectSpecies defectSpecies = new CDefectSpecies("缺陷类", cDefectRecipes);
                    DefectSpecies.Add(defectSpecies);
                }

                DefectFeatures = new();

                DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
                DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
                DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
                DefectFeatures.Add(new("Score", "分数", "Score", ""));
                DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));

                LoadModel();

            }

      
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

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 执行算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            var paramClass = (CParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect);
            List<ObbData> sResultInfos = ImageInfer(cell);
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
                    sResultInfos.ForEach(info => {
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
            

           

            //for (int i = 0; i < sResultInfos.Count; i++)
            //{
            //    CellDetection cellDetection2 = new CellDetection();
            //    List<float> detectRegion = new List<float>();
            //    detectRegion.Add((float)(sResultInfos[i].ResultScore*100));
            //    cellDetection2.Type = "表面缺陷类";
            //    cellDetection2.RecipeDefectName = "分数";
            //    cellDetection2.Category = Category.值;
            //    cellDetection2.Value = detectRegion;
            //    cell.AlgorithmOut.Add(cellDetection2);
            //}
        }

        void LoadModel()
        {
            CParam param = AlgorParams[0] as CParam;
            string model_type_str = "YOLOv8Obb";

            ModelType model_type = MyEnum.GetModelType<ModelType>(model_type_str);
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

        List<ObbData> ImageInfer(Cell cell)
        {
            List<ObbData> sResultInfos = new List<ObbData>();
            
            Mat img = new Mat(
                cell.Image.ImageHeight,
                cell.Image.ImageWidth,
                MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel+7)/8),
                cell.Image.ImageData
            );
            // img.SaveImage("C:\\Users\\Administrator.B\\Desktop\\新建文件夹\\1.jpg");
            //  Mat img = Cv2.ImRead("D:\\本地代码仓库\\yolo8 demo\\datasets\\Hamsausage0929\\images\\train\\0.jpg");
            //await Task.Run(() =>
            //{
            
            BaseResult result;
            result = yolo.predict(img);
            ObbResult obbResult = result as ObbResult;
            if (obbResult != null)
            {
                for (int i = 0; i < obbResult.count; i++)
                {
                    //SResultInfo reinfo = new SResultInfo();
                    //Point2f[] array = obbResult.datas[i].box.Points();
                    //reinfo.ResultPoints = array.ToList();
                    //reinfo.LabelStr = obbResult.datas[i].lable;
                    //reinfo.ResultScore = obbResult.datas[i].score;
                    
                    sResultInfos.Add(obbResult.datas[i]);
                    //for (int j = 0; j < 4; j++)
                    //{
                    //    Cv2.Line(image, (Point)array[j], (Point)array[(j + 1) % 4], new Scalar(255.0, 100.0, 200.0), 2);
                    //}

                    // Cv2.PutText(image, obbResult.datas[i].lable + "-" + obbResult.datas[i].score.ToString("0.00"), (Point)array[0], HersheyFonts.HersheySimplex, 0.8, new Scalar(0.0, 0.0, 0.0), 2);
                }
            }

            //});
            return sResultInfos;
        }

        private SRegion GetDetectRegion(ObbData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.box.Size.Width;
            sRegioninfo.ShorLen = info.box.Size.Height;
            sRegioninfo.Phi = info.box.Angle;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.score;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
            info.box.Points().ForEach(p=>rec1Points.Add(new System.Windows.Point(p.X, p.Y)));
            
            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            var rect = info.box.BoundingRect();
            detectRegion.rect = new System.Windows.Rect(new System.Windows.Point(rect.TopLeft.X, rect.TopLeft.Y), new System.Windows.Size(rect.Width, rect.Height));
            return detectRegion;
        }

        private void GetRecLen(List<Point2f> rec2Points, out double LongLen, out double ShorLen,out double phi)
        {
            double templen1 = 0;
            double templen2 = 0;
            phi = 0;
            templen1 = CalculateDistance(rec2Points[0], rec2Points[1]);
            templen2 = CalculateDistance(rec2Points[1], rec2Points[2]);

            if (templen1 > templen2)
            {
                LongLen = templen1;
                ShorLen = templen2;
                phi = Math.Atan((rec2Points[1].X - rec2Points[0].X) / (rec2Points[1].Y - rec2Points[0].Y));
            }
            else
            {
                LongLen = templen2;
                ShorLen = templen1;
            }
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 计算矩形长短边
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private double CalculateDistance(Point2f point1, Point2f point2)
        {
            double deltaX = point1.X - point2.X;
            double deltaY = point1.Y - point2.Y;
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
        /// 2025.3.1 鲍赞宝
        /// 最小分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("模型类型")]
        [property: Description("模型类型")]
        private EngineType engineType = EngineType.OpenVINO;


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
        private float nms = 0.4f;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 缺陷类型数量
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("缺陷类型数量")]
        [property: Description("已经标注的缺陷类型数量")]
        private int categ_num = 1;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 输入图像大小
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("输入图片大小")]
        [property: Description("检测的图片精度")]
        private InputImgSize input_size = InputImgSize.IN640;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("驱动设备")]
        [property: Description("驱动设备")]
        private string currentDevice = "GPU.0";
    }

    

}
