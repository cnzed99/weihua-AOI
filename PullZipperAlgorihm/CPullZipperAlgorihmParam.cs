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
using WH.VisionLearning;
using System.Management;


namespace PullZipperAlgorihm
{
    public class CPullZipperAlgorihmParam : CAlgorithmParamBase
    {


        /// <summary>
        /// 金属拉头检测对象
        /// </summary>
        IVisionModel WH_Meta_pull_det;

        /// <summary>
        /// 拉片分割模型
        /// </summary>
        IVisionModel WH_PullShape_Seg;

        /// <summary>
        /// Logo检测对象
        /// </summary>
        IVisionModel WH_Logo_pull_det;


        public CPullZipperAlgorihmParam(string user) : base()
        {
            User = user;
            SetDefectRecipe(User);

            DefectFeatures = new();
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("ColorDiffValue", "色差", "ColorDiffValue", "")); //20260424 鲍赞宝 针对缺陷与它周边的
                                                                                   //色差差异来判断它的明显程度
        }

        /// <summary>
        /// 金属拉头模型路径
        /// </summary>
        private string pull_Meta_Model_Path;

        /// <summary>
        /// Logo模型路径
        /// </summary>
        private string pull_Logo_Model_Path;

        /// <summary>
        /// 2025.11.17 鲍赞宝
        /// 拉片分割模型路径
        /// </summary>
        private string pullSharp_Model_Path;

        //金属拉头匹配对对象名称
        protected string[] pull_Meta_names;

        //Logo匹配对对象名称
        protected string[] pull_Logo_names;

        //拉片分割缺陷名称
        protected string[] pullSharp_names;

        /// <summary>
        /// 离线测试的拉片外形模板
        /// </summary>

        protected OpenCvSharp.Point[] offlineContours;

        public string User { get; set; }
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

        protected void SetDefectRecipe(string user)
        {
            ReadNames(user);

            #region 拉头缺陷
            List<CDefectRecipe> pullRecipes = new List<CDefectRecipe>();
            DefectSpecies = new List<CDefectSpecies>();

            for (int i = 0; i < pull_Meta_names?.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(pull_Meta_names[i], Category.区域);
                pullRecipes.Add(defectRecipe);
            }
            #endregion
            #region  LOGO 拉片外形

            for (int i = 0; i < pullSharp_names?.Length; i++)
            {
                CDefectRecipe defectRecipe2 = new CDefectRecipe(pullSharp_names[i], Category.值);
                pullRecipes.Add(defectRecipe2);
            }
            //颜色
            CDefectRecipe defectRecipe1_H = new CDefectRecipe("拉头色差1", Category.值);
            CDefectRecipe defectRecipe1_S = new CDefectRecipe("拉头色差2", Category.值);
            pullRecipes.Add(defectRecipe1_H);
            pullRecipes.Add(defectRecipe1_S);

            CDefectSpecies pullSpecies = new CDefectSpecies("拉头拉片", pullRecipes);

            List<CDefectRecipe> logoRecipes = new List<CDefectRecipe>();
            for (int i = 0; i < pull_Logo_names?.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(pull_Logo_names[i], Category.区域);
                logoRecipes.Add(defectRecipe);
            }
            CDefectSpecies logoSpecies = new CDefectSpecies("LOGO", logoRecipes);
            #endregion
            DefectSpecies.Add(pullSpecies);
            DefectSpecies.Add(logoSpecies);


        }

        protected void ReadNames(string user)
        {

            string modelDirpath = ".\\AlgorithmPlug\\PullZipperAlgorihm\\Models\\";

            string pullModelPath = modelDirpath + "PullMetaModel\\";

            if (user == "拉片")
            {
                pullModelPath = pullModelPath + "Front\\";
            }
            else
            {
                pullModelPath = pullModelPath + "Back\\";
            }

            var bigstrs = GetNames(pullModelPath);
            if (bigstrs.Item1 != "")
            {
                pull_Meta_Model_Path = bigstrs.Item1;
                pull_Meta_names = bigstrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            string pullsegModelPath = modelDirpath + "PullSegModel\\";

            var segstrs = GetNames(pullsegModelPath);
            if (segstrs.Item1 != "")
            {
                pullSharp_Model_Path = segstrs.Item1;
                pullSharp_names = segstrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            string logoModelPath = modelDirpath + "PullLogoModel\\";
            if (user == "拉片")
            {
                logoModelPath = logoModelPath + "Front\\";
            }
            else
            {
                logoModelPath = logoModelPath + "Back\\";
            }
            var logotrs = GetNames(logoModelPath);
            if (logotrs.Item1 != "")
            {
                pull_Logo_Model_Path = logotrs.Item1;
                pull_Logo_names = logotrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }
        }

        private (string, string[]) GetNames(string Dirpath)
        {
            if (Directory.Exists(Dirpath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml", "*.model", "*.Gmodel" };
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


        int upmassCount;
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
                UpdateScore(paramClass);
                Mat matimg = GetMatImage(cell, paramClass);
                if (matimg == null)
                {
                    return;
                }
                Mat img;
                if (cell.ImageFile == "")  //相机图 在线检测
                {
                    Mat colorMat = new Mat();
                    Cv2.CvtColor(matimg, colorMat, ColorConversionCodes.BGR2RGB);
                    img = colorMat;
                    matimg.Dispose();

                }
                else //离线图 离线检测
                {
                    img = matimg;
                    // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "相机原图.png", img);
                }
                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                DetResult pullResult = ImageInferDet(WH_Meta_pull_det, img);

                if (pullResult != null && pullResult.datas.Count > 0) //如果有大缺陷直接退出
                {
                    // List<Point> massPoints = new List<Point>(); //上止的位置
                    for (int j = 0; j < pullResult.datas.Count; j++)
                    {
                        int labelindex = int.Parse(pullResult.datas[j].lable);
                        string labelname = pull_Meta_names[labelindex];
                        CoordRestoreData restoreData = new CoordRestoreData(0, 0, 0, 0, labelname, pullResult.datas[j]);
                        dets.Add(restoreData);
                    }
                }

                #region 拉片外形
                if (User == "拉片")
                {
                    List<double> dsimilaritys = new List<double>();
                    List<List<Point>> allcontourpoints = new List<List<Point>>();

                    SegResult pullsegResult = WH_PullShape_Seg.Predict(img) as SegResult;

                    if (pullsegResult == null) return;

                    List<Point[]> contoursList = new List<Point[]>();
                    foreach (var seg in pullsegResult.datas)
                    {

                        Mat maskgray = new Mat();
                        Cv2.CvtColor(seg.mask, maskgray, ColorConversionCodes.BGR2GRAY);
                        Mat binary = new Mat();
                        Cv2.Threshold(maskgray, binary, 10, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                        Point[][] contours;
                        HierarchyIndex[] hierarchy;
                        Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                        maskgray.Dispose();
                        binary.Dispose();
                        Point[] maxps = contours?.MaxBy(p => p.Length);
                        if (maxps != null)
                        {
                            contoursList.Add(maxps);
                        }

                    }
                    if (paramClass.OffLinePullerTemplateEnabel)
                    {
                        paramClass.OffLinePullerTemplateEnabel = false;
                        offlineContours = contoursList.OrderByDescending(contour => contour.Length).First();
                    }

                    if (cell.ImageFile == "")//在线
                    {
                        if (cell.OrgContours != null && cell.OrgContours.Length > 0 && contoursList.Count > 0)
                        {
                            for (int i = 0; i < contoursList.Count; i++)
                            {
                                //double simiValue = MatchShapesUsingHuMoments(cell.OrgContours, contoursList[i]);
                                double simiValue = MatchShapesWithCv2(cell.OrgContours, contoursList[i]);

                                dsimilaritys.Add(simiValue);
                            }
                            double minvalue = dsimilaritys.Min();
                            int minindex = dsimilaritys.IndexOf(minvalue);
                            List<Point> p = contoursList[minindex].ToList();
                            p.Add(p[0]);
                            allcontourpoints.Add(p);
                            CoordRestoreData restoreData = new CoordRestoreData(pullSharp_names[0], (float)minvalue, allcontourpoints, 1);
                            dets.Add(restoreData);
                        }
                        else
                        {
                            CoordRestoreData restoreData = new CoordRestoreData(pullSharp_names[0], 1000, allcontourpoints, 1);
                            dets.Add(restoreData);
                        }
                    }
                    else //离线
                    {
                        if (offlineContours != null && contoursList.Count > 0)
                        {
                            for (int i = 0; i < contoursList.Count; i++)
                            {
                                // double simiValue = MatchShapesUsingHuMoments(offlineContours, contoursList[i]);
                                double simiValue = MatchShapesWithCv2(offlineContours, contoursList[i]);
                                dsimilaritys.Add(simiValue);
                            }
                            double minvalue = dsimilaritys.Min();
                            int minindex = dsimilaritys.IndexOf(minvalue);
                            List<Point> p = contoursList[minindex].ToList();
                            p.Add(p[0]);
                            allcontourpoints.Add(p);

                            CoordRestoreData restoreData = new CoordRestoreData(pullSharp_names[0], (float)minvalue, allcontourpoints, 1);
                            dets.Add(restoreData);
                        }
                        else
                        {
                            CoordRestoreData restoreData = new CoordRestoreData(pullSharp_names[0], 1000, allcontourpoints, 1);
                            dets.Add(restoreData);
                        }
                    }

                }
                #endregion
                #region 拉头拉片颜色
                int px = 0, py = 0;
                int rew = 0, reh = 0;
                if (User == "拉头")
                {
                    px = 190;
                    py = 280;
                    rew = 100;
                    reh = 50;
                }
                else
                {
                    px = img.Width / 2;
                    py = img.Height / 2;
                    rew = 50;
                    reh = 50;
                }

                Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                Mat hsvImage = new Mat();
                Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                Scalar hsvMean = Cv2.Mean(hsvImage);

                // HSV通道说明：
                // H: 0-179 (色调)
                // S: 0-255 (饱和度)
                // V: 0-255 (明度)
                double hMean = hsvMean.Val0;
                double sMean = hsvMean.Val1;
                // double vMean = hsvMean.Val2;
                CoordRestoreData disDataH = new CoordRestoreData("拉头色差1", (float)hMean);
                CoordRestoreData disDataS = new CoordRestoreData("拉头色差2", (float)sMean);
                dets.Add(disDataH);
                dets.Add(disDataS);
                hsvImage.Dispose();

                #endregion
                #region Logo识别
                DetResult logoResult = ImageInferDet(WH_Logo_pull_det, img);
                if (logoResult != null)
                {
                    for (int i = 0; i < logoResult.count; i++)
                    {
                        int pulllabelindex = int.Parse(logoResult[i].lable);
                        string pullabelname = pull_Logo_names[pulllabelindex];
                        CoordRestoreData restoreData = new CoordRestoreData(0, 0, 0, 0, pullabelname, logoResult.datas[i]);
                        dets.Add(restoreData);
                    }
                }
                #endregion

                ParseResult(dets, cell);
                img.Dispose();
            }

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
                    //if (!Common_names.Contains(de.Name))
                    //    continue;
                    CellDetection cellDetection1 = new CellDetection();
                    cellDetection1.Type = ds.Name;
                    cellDetection1.Category = de.Category;
                    cellDetection1.RecipeDefectName = de.Name;
                    cellDetection1.Value = new List<float>();

                    //DetResult detrets = sResultInfos as DetResult;
                    var finds = sResultInfos.FindAll(info =>
                    {
                        //int index = int.Parse(info.Labelstr);
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
                            if (de.Category == Category.值)
                            {
                                if (item.Contours.Count > 0)
                                {
                                    for (int i = 0; i < item.Contours.Count; i++)
                                    {
                                        cell.DrawEdges.Add(new CEdgeDraw(item.Contours[i], Brushes.Pink, showinview: item.ShowInView));
                                    }
                                }
                                else
                                {
                                    List<System.Windows.Point> rec1MarkPoints = new List<System.Windows.Point>();
                                    rec1MarkPoints.Add(item.ShowLeftUp);
                                    rec1MarkPoints.Add(item.ShowRightUp);
                                    rec1MarkPoints.Add(item.ShowRightDown);
                                    rec1MarkPoints.Add(item.ShowLeftDown);
                                    rec1MarkPoints.Add(item.ShowLeftUp);
                                    cell.DrawEdges.Add(new CEdgeDraw(rec1MarkPoints, Brushes.Pink, showinview: item.ShowInView));
                                }


                            }

                        }

                    }
                    cell.AlgorithmOut.Add(cellDetection1);

                }
            }
            sResultInfos.Clear();
        }
        [OnDeserialized]
        private async void LoadModel(StreamingContext context)
        {
            CParam param = AlgorParams.FirstOrDefault() as CParam;
            if (param != null)
            {
                if (param != null)
                {
                    string CurrentDevice = param.CurrentDevice;

                    EngineType engineType;
                    if (HasDedicatedGraphicsCard()) //有显卡
                    {
                        engineType = EngineType.TensorRT;
                    }
                    else
                    {
                        engineType = EngineType.OpenVINO;
                        CurrentDevice = "CPU";
                    }
                    float Nms = param.Nms;
                    if (pull_Meta_names != null)
                    {
                        int pull_num = pull_Meta_names.Length;
                        WH_Meta_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Meta_Model_Path, engineType,
    CurrentDevice, pull_num, param.MetaPullScore, Nms, 640);
                    }

                    if (pullSharp_names != null)
                    {
                        int pullsharp_num = pullSharp_names.Length;
                        WH_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSharp_Model_Path, engineType,
CurrentDevice, pullsharp_num, param.PullSharpScore, Nms, 640);
                    }
                    if (pull_Logo_names != null)
                    {
                        int logopull_num = pull_Logo_names.Length;
                        WH_Logo_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Logo_Model_Path, engineType,
    CurrentDevice, logopull_num, param.LogoPullScore, 0.5f, 512);
                    }
                }

            }
        }

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
        public ObbResult ImageInferObb(IVisionModel WH, Mat img)
        {
            if (WH != null)
            {
                ObbResult resultDet;
                resultDet = WH.Predict(img) as ObbResult;
                return resultDet;
            }
            else
            {
                return null;
            }

        }

        protected SRegion GetDetectRegion(CoordRestoreData info)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();
            //GetRecLen(rec2Points, out double LongLen, out double ShorLen,out double phi);
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
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>()
            {
                info.ShowLeftUp,
                info.ShowRightUp,
                info.ShowRightDown,
                info.ShowLeftDown,
            };
            // rec1Points.Add(new System.Windows.Point(info.box.X, info.box.Y));

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);

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
        /// 计算良两点距离
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private float CalculateDistance(ObbData point1, ObbData point2)
        {
            float deltaX = point1.box.Center.X - point2.box.Center.X;
            // return Math.Abs(deltaX);
            float deltaY = point1.box.Center.Y - point2.box.Center.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// 2025.9.24 鲍赞宝
        /// 计算两点连线与X轴的夹角（度数）
        /// </summary>
        /// <param name="p1">第一个点</param>
        /// <param name="p2">第二个点</param>
        /// <returns>角度</returns>
        private float CalculateLineAngle(Point2f p1, Point2f p2)
        {
            // 计算坐标差值
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            // 使用Atan2计算弧度（注意参数顺序：dy, dx）
            // Atan2返回值范围：[-π, π]
            double radians = Math.Atan2(dy, dx);
            // 转换为度数
            float degrees = (float)(radians * (180.0 / Math.PI));
            return degrees;
        }

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
        /// 使用OpenCV内置方法计算相似度（作为对比参考）
        /// </summary>
        public double MatchShapesWithCv2(Point[] contours1, Point[] contours2,
                                                ShapeMatchModes mode = ShapeMatchModes.I2)
        {
            using (var contour1Mat = new Mat(contours1.Length, 1, MatType.CV_32SC2))
            using (var contour2Mat = new Mat(contours2.Length, 1, MatType.CV_32SC2))
            {

                // 填充点数据
                for (int i = 0; i < contours1.Length; i++)
                {
                    contour1Mat.Set(i, 0, new Point(contours1[i].X, contours1[i].Y));
                }
                for (int i = 0; i < contours2.Length; i++)
                {
                    contour2Mat.Set(i, 0, new Point(contours2[i].X, contours2[i].Y));
                }

                return Cv2.MatchShapes(contour1Mat, contour2Mat, mode);

            }
        }

        private void UpdateScore(CParam param)
        {
            WH_Meta_pull_det?.UpdateNMS_Score(param.Nms, param.MetaPullScore);
            WH_PullShape_Seg?.UpdateNMS_Score(param.Nms, param.PullSharpScore);
        }
        public static bool HasDedicatedGraphicsCard()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    // 常见独立显卡关键词
                    if (name.Contains("NVIDIA") ||
                        name.Contains("AMD") ||
                        name.Contains("Radeon"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false; // 如果查询失败，返回false
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
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("01驱动器")]
        [property: Description("驱动器")]
        private string currentDevice = "GPU.0";

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("02 金属拉头分数阈值")]
        [property: Description("金属拉头分数阈值")]
        private float metaPullScore = 0.4f;


        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("03 拉片分割分数阈值")]
        [property: Description("拉片分割分数阈值")]
        private float pullSharpScore = 0.5f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("04 Logo分数阈值")]
        [property: Description("Logo分数阈值")]
        private float logoPullScore = 0.4f;

        [ObservableProperty]
        [property: Category("离线设置模板")]
        [property: DisplayName("01 离线设置拉片外形模版开关")]
        [property: Description("离线设置拉片模版开关")]
        bool offLinePullerTemplateEnabel;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// NMScore
        /// </summary>
        [ObservableProperty]
        private float nms = 0.5f;

    }

    public struct CoordRestoreData
    {
        /// <summary>
        /// 坐标还原
        /// </summary>
        /// <param name="imgwidth">当前图宽</param>
        /// <param name="imgheight">当前图高</param>
        /// <param name="imgIndex">图片编号</param>
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, DetData det, int showinview = 0)
        {
            //坐标还原 

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
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, ObbData obb, int showinview = 0)
        {
            //坐标还原 

            ShowLeftUp.X = obb.box.Points()[0].X + orgx + imgWidth * imgIndex;
            ShowLeftUp.Y = obb.box.Points()[0].Y + orgy;

            ShowRightUp.X = obb.box.Points()[1].X + orgx + imgWidth * imgIndex;
            ShowRightUp.Y = obb.box.Points()[1].Y + orgy;

            ShowRightDown.X = obb.box.Points()[2].X + orgx + imgWidth * imgIndex;
            ShowRightDown.Y = obb.box.Points()[2].Y + orgy;

            ShowLeftDown.X = obb.box.Points()[3].X + orgx + imgWidth * imgIndex;
            ShowLeftDown.Y = obb.box.Points()[3].Y + orgy;

            RecWidth = obb.box.Size.Width;
            RecHeight = obb.box.Size.Height;
            OrgCenterX = obb.box.Center.X + orgx;
            OrgCenterY = obb.box.Center.Y + orgy;
            Score = obb.score * 100;
            Labelstr = labelstr;
            Angle = obb.box.Angle;
            Value = 0.0f;
            ShowInView = showinview;
        }
        public CoordRestoreData(string labelstr, float value, int showinview = 0)
        {
            //坐标还原 

            ShowLeftUp.X = 0;
            ShowLeftUp.Y = 0;

            ShowRightUp.X = 0;
            ShowRightUp.Y = 0;

            ShowRightDown.X = 0;
            ShowRightDown.Y = 0;

            ShowLeftDown.X = 0;
            ShowLeftDown.Y = 0;

            RecWidth = 0;
            RecHeight = 0;
            OrgCenterX = 0;
            OrgCenterY = 0;
            Score = 0;
            Labelstr = labelstr;
            Angle = 0;
            Value = value;
            ShowInView = showinview;
        }

        public CoordRestoreData(string labelstr, float value, List<List<Point>> contours, int showinview = 0)
        {
            //坐标还原 

            ShowLeftUp.X = 0;
            ShowLeftUp.Y = 0;

            ShowRightUp.X = 0;
            ShowRightUp.Y = 0;

            ShowRightDown.X = 0;
            ShowRightDown.Y = 0;

            ShowLeftDown.X = 0;
            ShowLeftDown.Y = 0;

            RecWidth = 0;
            RecHeight = 0;
            OrgCenterX = 0;
            OrgCenterY = 0;
            Score = 0;
            Labelstr = labelstr;
            Angle = 0;
            Value = value;
            ShowInView = showinview;

            for (int i = 0; i < contours.Count; i++)
            {
                List<System.Windows.Point> Points = new List<System.Windows.Point>();
                for (int j = 0; j < contours[i].Count; j++)
                {
                    System.Windows.Point point = new System.Windows.Point() { X = contours[i][j].X, Y = contours[i][j].Y };
                    Points.Add(point);
                }

                Contours.Add(Points);
            }
        }
        /// <summary>
        /// 用于显示左上角点
        /// </summary>
        public System.Windows.Point ShowLeftUp = new System.Windows.Point();
        /// <summary>
        /// 用于显示右上角点
        /// </summary>
        public System.Windows.Point ShowRightUp = new System.Windows.Point();
        /// <summary>
        /// 用于显示左上角点
        /// </summary>
        public System.Windows.Point ShowRightDown = new System.Windows.Point();
        /// <summary>
        ///用于显示左上角点
        /// </summary>
        public System.Windows.Point ShowLeftDown = new System.Windows.Point();
        /// <summary>
        /// 原图上中心X
        /// </summary>
        public float OrgCenterX { get; set; }
        /// <summary>
        /// 原图上中心Y
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
        /// <summary>
        /// 值
        /// </summary>
        public float Value { get; set; }
        /// <summary>
        /// 在哪个窗口显示区域
        /// </summary>
        public int ShowInView { get; set; }
        /// <summary>
        /// 分割区域轮廓点集
        /// </summary>
        public List<List<System.Windows.Point>> Contours = new List<List<System.Windows.Point>>();
    }

}
