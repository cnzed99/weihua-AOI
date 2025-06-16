using System.ComponentModel;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using System.IO;
using System.Runtime.Serialization;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using OpenCvSharp.ML;

namespace ZipperTestAlgorihm
{
    public class CZipperTestAlgorihmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// 检测对象1
        /// </summary>
        private YOLO yolo_all_det1 = new YOLO();
        /// <summary>
        /// 检测对象2
        /// </summary>
        private YOLO yolo_all_det2 = new YOLO();
        /// <summary>
        /// 检测对象3
        /// </summary>
        private YOLO yolo_all_det3 = new YOLO();
        /// <summary>
        /// 检测对象4
        /// </summary>
        private YOLO yolo_all_det4 = new YOLO();

        // private YOLO yolo_labeldefect = new YOLO();

        //定义4组矩形来裁切图片
        Rect[] cropRec = new Rect[4];

        public CZipperTestAlgorihmParam() : base()
        {
            //DefectSpecies = new()
            //{
            //    new("上止类", new() { new("上止有无", Category.区域) }),
            //    new("下止类",new() {  new("下止有无", Category.区域) }),
            //    new("拉头类",new() {  new("拉头有无", Category.区域) }),
            //};
            string modelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models";
            ReadNames(modelDirPath);

            if (Detect_names?.Length > 0)
            {
                List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
                DefectSpecies = new List<CDefectSpecies>();

                for (int i = 0; i < Detect_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(Detect_names[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
                //CDefectRecipe defectRecipe1 = new CDefectRecipe("数值", Category.值);
                //cDefectRecipes.Add(defectRecipe1);

                //CDefectRecipe defectRecipe2 = new CDefectRecipe("ID", Category.值);
                //cDefectRecipes.Add(defectRecipe2);

                CDefectSpecies defectSpecies = new CDefectSpecies("拉链", cDefectRecipes);
                DefectSpecies.Add(defectSpecies);
            }

            DefectFeatures = new();

            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));

        }

        protected ModelType _ModelType = ModelType.YOLOv8Det;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 模型路径
        /// </summary>
        private string Model_Path;

        private string text_Model_Path;

        private string label_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        private string name_Path;

        protected string[] text_Model_Names;
        protected string[] LabelDetect_names;

        protected string[] Detect_names;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        //public GeneralMLOBBAlgorithmParam()
        //    : base()
        //{
        //    text_Model_Path = Path.Combine(Model_Path, "threeDataModel.onnx");
        //    string textModelNamesPath = Path.Combine(Model_Path, "threeDataModelClasses.txt");
        //    text_Model_Names = File.ReadAllLines(textModelNamesPath);
        //    label_Model_Path = Path.Combine(Model_Path, "LabelDefectModel.onnx");
        //    string LabelDetectModelNamesPath = Path.Combine(
        //        Model_Path,
        //        "LabelDefectModelClasses.txt"
        //    );
        //    LabelDetect_names = File.ReadAllLines(LabelDetectModelNamesPath);
        //    //string[] searchPatterns = { "*.onnx", "*.engine", "*.pt" };

        //    //if (Directory.Exists(Model_Dirpath))
        //    //{
        //    //    var files = searchPatterns
        //    //    .SelectMany(pattern => Directory.GetFiles(Model_Dirpath, pattern))
        //    //    .ToList();

        //    //    if (files.Count>0)
        //    //    {
        //    //        Model_Path = files[0];
        //    //        name_Path= Model_Dirpath+ "\\classes.txt";
        //    //        Detect_names = File.ReadAllLines(name_Path);
        //    //    }

        //    //if (Detect_names?.Length > 0)
        //    //{
        //    //    List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
        //    //    DefectSpecies = new List<CDefectSpecies>();

        //    //    for (int i = 0; i < Detect_names.Length; i++)
        //    //    {
        //    //        CDefectRecipe defectRecipe = new CDefectRecipe(Detect_names[i], Category.区域);
        //    //        cDefectRecipes.Add(defectRecipe);
        //    //    }
        //    //    //CDefectRecipe defectRecipe1 = new CDefectRecipe("分数", Category.值);
        //    //    //cDefectRecipes.Add(defectRecipe1);

        //    //    CDefectSpecies defectSpecies = new CDefectSpecies("盐水袋", cDefectRecipes);
        //    //    DefectSpecies.Add(defectSpecies);
        //    //}

        //    //DefectFeatures = new();

        //    //DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
        //    //DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
        //    //DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
        //    //DefectFeatures.Add(new("Score", "分数", "Score", ""));
        //    //DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));

        //    // LoadModel();

        //    //}
        //}

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
            string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml" };

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
                int smallimgWidth = cell.Image.ImageWidth / 4;
                int smallimgHeight = cell.Image.ImageHeight;
                List<Mat> mats = new List<Mat>();
                for (int i = 0; i < 4; i++)
                {
                    cropRec[i].X = i * smallimgWidth;
                    cropRec[i].Y = 0;
                    cropRec[i].Width = smallimgWidth;
                    cropRec[i].Height = smallimgHeight;
                    Mat cropimg = img[cropRec[i]];
                    mats.Add(cropimg);

                    //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\截图\" +DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropimg);
                }

                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (cell.PhotoIndex == 1) //第一张有下止的图
                {
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].datas[j].lable);
                            string labelname = Detect_names[labelindex];
                            if (labelname.Contains("正面下止"))
                            {
                                //坐标还原           
                                CoordRestoreData restoreData = new CoordRestoreData(smallimgWidth, smallimgHeight, i, detrets[i].datas[j]);
                                dets.Add(restoreData);
                                int recw = 256;
                                int rech = 256;
                                int rex = Convert.ToInt32(restoreData.OrgCenterX - recw / 2);
                                int rey = Convert.ToInt32(restoreData.OrgCenterY - rech / 2);
                                if ((rex + recw) > cell.Image.ImageWidth)
                                {
                                    rex = cell.Image.ImageWidth - recw;
                                }
                                if (rex < 0)
                                {
                                    rex = 0;
                                }
                                Mat cropDownMat = img[new Rect(rex, rey, recw, rech)];
                                 Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面下止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropDownMat);
                            }
                            else
                            {
                                CoordRestoreData restoreData = new CoordRestoreData(smallimgWidth, smallimgHeight, i, detrets[i].datas[j]);
                                restoreData.OrgX = restoreData.OrgX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                restoreData.OrgCenterX = restoreData.OrgCenterX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                dets.Add(restoreData);
                            }
                        }
                    }


                }
                else if (cell.PhotoIndex == cell.PhotoTatolCount) //最后一张图片有上止图片
                {
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].datas[j].lable);
                            string labelname = Detect_names[labelindex];
                            if (labelname.Contains("正面上止"))
                            {
                                //坐标还原           
                                CoordRestoreData restoreData = new CoordRestoreData(smallimgWidth, smallimgHeight, i, detrets[i].datas[j]);

                                int recw = 192;
                                int rech = 96;
                                int rex = Convert.ToInt32(restoreData.OrgCenterX - recw/2);
                                int rey = Convert.ToInt32(restoreData.OrgCenterY - rech/2);
                                if ((rex+ recw)> cell.Image.ImageWidth)
                                {
                                    rex = cell.Image.ImageWidth - recw;
                                }
                                if (rex <0 )
                                {
                                    rex = 0;
                                }
                                Mat cropUpMat = img[new Rect(rex, rey, recw, rech)];
                                restoreData.OrgX = restoreData.OrgX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                restoreData.OrgCenterX = restoreData.OrgCenterX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                dets.Add(restoreData);
                                Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面上止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropUpMat);
                            }
                            else
                            {
                                CoordRestoreData restoreData = new CoordRestoreData(smallimgWidth, smallimgHeight, i, detrets[i].datas[j]);
                                restoreData.OrgX = restoreData.OrgX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                restoreData.OrgCenterX = restoreData.OrgCenterX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                                dets.Add(restoreData);
                            }
                        }
                    }
                }
                else if (cell.PhotoIndex == 100) //有拉链的图片
                {

                }
                else //中间布带，链牙缺陷
                {
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].datas[j].lable);
                            string labelname = Detect_names[labelindex];
                            CoordRestoreData restoreData = new CoordRestoreData(smallimgWidth, smallimgHeight, i * cell.PhotoIndex, detrets[i].datas[j]);
                            restoreData.OrgX = restoreData.OrgX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                            restoreData.OrgCenterX = restoreData.OrgCenterX + (cell.PhotoIndex - 1) * cell.Image.ImageWidth;
                            dets.Add(restoreData);
                        }
                    }
                }
                ParseResult(dets, cell);
            }
        }

        private async Task<List<DetResult>> ImageInferall(List<Mat> mats, float score, float nms)
        {


            List<DetResult> alldetResult = new List<DetResult>();
            Task<DetResult> task1 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInfer(yolo_all_det1, mats[0], score, nms);
                return sResultInfos;
            });

            Task<DetResult> task2 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInfer(yolo_all_det2, mats[1], score, nms);
                return sResultInfos;
            });
            Task<DetResult> task3 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInfer(yolo_all_det3, mats[2], score, nms);
                return sResultInfos;
            });
            Task<DetResult> task4 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInfer(yolo_all_det4, mats[3], score, nms);
                return sResultInfos;
            });
            await Task.WhenAll(task1, task2, task3, task4);
            alldetResult.Add(task1.Result);
            alldetResult.Add(task2.Result);
            alldetResult.Add(task3.Result);
            alldetResult.Add(task4.Result);
            return alldetResult;
        }
        protected void ParseResult(List<CoordRestoreData> sResultInfos, Cell cell)
        {
            if (sResultInfos is null)
            {
                return;
            }

            foreach (var ds in DefectSpecies)
            {
                foreach (var de in ds.RecipeDefects)
                {
                    if (!Detect_names.Contains(de.Name))
                        continue;
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = ds.Name;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();

                    //DetResult detrets = sResultInfos as DetResult;
                    var finds = sResultInfos.FindAll(info =>
                    {
                        int index = int.Parse(info.Labelstr);
                        return Detect_names[index] == de.Name;
                    });
                    if (finds.Count > 0)
                    {
                        foreach (var item in finds)
                        {
                            SRegion sRegion = GetDetectRegion(item);
                            //if (classNames[item.index].Contains("拉头"))
                            //{
                            //    if (!img.Empty())
                            //    {
                            //        Mat submat = img.SubMat(item.box);
                            //        cell.ZipperPullPartImg = Mat2BitmapSource(submat);
                            //    }
                            //}
                            cellDetection1.regionOut.Add(sRegion);
                        }
                    }
                    cell.AlgorithmOut.Add(cellDetection1);

                }
            }
            sResultInfos.Clear();
        }
        [OnDeserialized]
        private void LoadModel(StreamingContext context)
        {
            CParam param = AlgorParams[0] as CParam;

            ModelType model_type = ModelType.YOLOv8Det;
            // ModelType model_type = param.ModelType;
            // EngineType engine_type = MyEnum.GetEngineType<EngineType>(engine_type_str);
            EngineType engine_type = param.EngineType;

            yolo_all_det1.Dispose();
            yolo_all_det2.Dispose();
            yolo_all_det3.Dispose();
            yolo_all_det4.Dispose();
            //yolo_labeldefect.Dispose();
            if (param != null)
            {
                string CurrentDevice = param.CurrentDevice;
                int text_Categ_num = Detect_names.Length;
                // int label_Categ_num = LabelDetect_names.Length;
                float Score = param.Score;
                float Nms = param.Nms;
                InputImgSize Input_size = param.Input_size;
                ImgSize Output_size = param.Output_size;
                //string model_path =
                //    param.EngineType == EngineType.TensorRT
                //        ? Model_Path + ".engine"
                //        : Model_Path + ".onnx";
                yolo_all_det1 = YOLO.GetYolo(
                    model_type,
                    Model_Path,
                    engine_type,
                    CurrentDevice,
                    text_Categ_num,
                    Score,
                    Nms,
                    Input_size
                );
                yolo_all_det2 = YOLO.GetYolo(
                model_type,
                Model_Path,
                engine_type,
                CurrentDevice,
                text_Categ_num,
                Score,
                Nms,
                Input_size
            );
                yolo_all_det3 = YOLO.GetYolo(
                model_type,
                Model_Path,
                engine_type,
                CurrentDevice,
                text_Categ_num,
                Score,
                Nms,
                Input_size
            );
                yolo_all_det4 = YOLO.GetYolo(
                model_type,
                Model_Path,
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

        public DetResult ImageInfer(YOLO yolo, Mat img, float score, float nms)
        {
            DetResult resultDet;
            resultDet = yolo.predict(img, score, nms) as DetResult;
            return resultDet;
        }

        protected SRegion GetDetectRegion(CoordRestoreData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
            sRegioninfo.LongLen = info.RecWidth;
            sRegioninfo.ShorLen = info.RecHeight;
            sRegioninfo.Phi = info.Angle;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = info.Score;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                new System.Windows.Point(info.OrgX, info.OrgY),
                new System.Windows.Point(info.OrgX + info.RecWidth, info.OrgY),
                new System.Windows.Point(info.OrgX + info.RecWidth, info.OrgY + info.RecHeight),
                new System.Windows.Point(info.OrgX, info.OrgY + info.RecHeight),
            };
           // rec1Points.Add(new System.Windows.Point(info.box.X, info.box.Y));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
           // var rect = info.DetDate.box;
            //detectRegion.rect = new System.Windows.Rect(
            //    new System.Windows.Point(info.OrgX, info.OrgY),
            //    new System.Windows.Size(info.RecWidth, info.RecHeight)
            //);
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



        private BitmapSource Mat2BitmapSource(Mat img)
        {
            // 方法1：编码为 PNG 字节流
            Cv2.ImEncode(".png", InputArray.Create(img), out byte[] imageBytes);

            // 方法2：通过 MemoryStream 转换
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                //// 方式A：直接创建 BitmapSource（需指定像素格式）
                //BitmapSource bitmapSource = BitmapSource.Create(
                //    img.Width,
                //    img.Height,
                //    96, 96, // DPI
                //    PixelFormats.Pbgra32, // OpenCV 默认 BGR 格式
                //null,
                //imageBytes,
                //    img.Width * (img.Channels() == 1 ? 1 : 4) // 每行字节数
                //);

                // 方式B：通过 PngBitmapEncoder（更通用）
                //BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(ms));
                // BitmapSource enbitmapSource = encoder.Frames[0];
                BitmapSource enbitmapSource = BitmapFrame.Create(ms);
                BitmapSource bitmapSource = new CachedBitmap(enbitmapSource, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                //if (bitmapSource is BitmapFrameDecode)
                //{
                //    // 方案1：转换为缓存位图


                //    // 方案2：克隆像素数据
                //    var writable = new WriteableBitmap(source);
                //    writable.Freeze();
                //    return writable;
                //}
                bitmapSource.Freeze();

                return bitmapSource;



            }
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

    }


    public struct CoordRestoreData
    {
        /// <summary>
        /// 坐标还原
        /// </summary>
        /// <param name="imgwidth">当前图宽</param>
        /// <param name="imgheight">当前图高</param>
        /// <param name="imgIndex">图片编号</param>
        public CoordRestoreData(int imgwidth, int imgheight, int imgIndex, DetData det)
        {
            //坐标还原 
            OrgX = det.box.X + imgwidth * imgIndex;
            OrgY = det.box.Y;
            RecWidth = det.box.Width;
            RecHeight = det.box.Height;
            OrgCenterX = OrgX + det.box.Width / 2;
            OrgCenterY = OrgY + det.box.Height / 2;
            Score = det.score;
            Labelstr = det.lable;
            Angle = 0.0f;

        }
        public CoordRestoreData(int imgwidth, int imgheight, int imgIndex, ObbData obb)
        {
            //坐标还原 
            OrgX = obb.box.Points()[0].X + imgwidth * imgIndex;
            OrgY = obb.box.Points()[0].Y;
            RecWidth = obb.box.Size.Width;
            RecHeight = obb.box.Size.Height;
            OrgCenterX = obb.box.Center.X + imgwidth * imgIndex;
            OrgCenterY = obb.box.Center.Y;
            Score = obb.score;
            Labelstr = obb.lable;
            Angle = obb.box.Angle;
        }
        /// <summary>
        /// 原图的左上角X
        /// </summary>
        public float OrgX { get; set; }
        /// <summary>
        /// 原图的右上角Y
        /// </summary>
        public float OrgY { get; set; }
        /// <summary>
        /// 原图的中心X
        /// </summary>
        public float OrgCenterX { get; set; }
        /// <summary>
        /// 原图的中心Y
        /// </summary>
        public float OrgCenterY { get; set; }
        /// <summary>
        /// 缺陷框宽
        /// </summary>
        public float RecWidth { get; set; }
        /// <summary>
        /// 缺陷框高
        /// </summary>
        public float RecHeight { get; set; }
        // public DetData DetDate { get; set; }
        /// <summary>
        /// 分数
        /// </summary>
        public float Score { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Labelstr { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        public float Angle { get; set; }
    }

}
