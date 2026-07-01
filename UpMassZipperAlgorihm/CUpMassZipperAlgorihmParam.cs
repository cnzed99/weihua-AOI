using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Expression.Shapes;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using WH.VisionLearning;


namespace UpMassZipperAlgorihm
{
    public class CUpMassZipperAlgorihmParam : CAlgorithmParamBase
    {

        public CUpMassZipperAlgorihmParam(string user) : base()
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
        /// 上止检测对象
        /// </summary>
        IVisionModel WH_UpStopMassDefe_det;

        /// <summary>
        /// 上止模型路径
        /// </summary>
        private string upStopMassDefe_Model_Path;


        //上止缺陷名称
        protected string[] upStopMassDefe_names;

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

            #region 上止
            List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
            DefectSpecies = new List<CDefectSpecies>();

            if (upStopMassDefe_names?.Length > 0)
            {
                // string[] upstrs = upStopMassDefe_names.Where(s => s != "链齿").ToArray();
                for (int i = 0; i < upStopMassDefe_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(upStopMassDefe_names[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
            }
            CDefectRecipe defectRecipe1_1 = new CDefectRecipe("高牙距离", Category.值);
            CDefectRecipe defectRecipe1_2 = new CDefectRecipe("低牙距离", Category.值);
            cDefectRecipes.Add(defectRecipe1_1);
            cDefectRecipes.Add(defectRecipe1_2);

            CDefectRecipe defectRecipe1_3 = new CDefectRecipe("高牙色差H", Category.值);
            CDefectRecipe defectRecipe1_4 = new CDefectRecipe("高牙色差S", Category.值);
            cDefectRecipes.Add(defectRecipe1_3);
            cDefectRecipes.Add(defectRecipe1_4);

            CDefectRecipe defectRecipe1_5 = new CDefectRecipe("低牙色差H", Category.值);
            CDefectRecipe defectRecipe1_6 = new CDefectRecipe("低牙色差S", Category.值);
            cDefectRecipes.Add(defectRecipe1_5);
            cDefectRecipes.Add(defectRecipe1_6);

            CDefectRecipe defectRecipe1_7 = new CDefectRecipe("高牙平齐", Category.值);
            CDefectRecipe defectRecipe1_8 = new CDefectRecipe("低牙平齐", Category.值);
            cDefectRecipes.Add(defectRecipe1_7);
            cDefectRecipes.Add(defectRecipe1_8);

            CDefectSpecies UpmassSpecies = new CDefectSpecies("上止", cDefectRecipes);
            #endregion
            DefectSpecies.Add(UpmassSpecies);


        }

        protected void ReadNames(string user)
        {

            string modelDirpath = ".\\AlgorithmPlug\\UpMassZipperAlgorihm\\Models\\";

            string upStopMassDefeModelPath = modelDirpath + "UpStopMassModel\\";
            var upstopsdefetrs = GetNames(upStopMassDefeModelPath);
            if (upstopsdefetrs.Item1 != "")
            {
                upStopMassDefe_Model_Path = upstopsdefetrs.Item1;
                upStopMassDefe_names = upstopsdefetrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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
                //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "转前.png", matimg);
                Mat img;
                if (cell.ImageFile == "")  //相机图 在线检测
                {
                    // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "离线读图转前.png", matimg);
                    Mat colorMat = new Mat();
                    Cv2.CvtColor(matimg, colorMat, ColorConversionCodes.BGR2RGB);
                    img = colorMat;
                    //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "离线兔兔转后.png", img);
                    matimg.Dispose();

                }
                else //离线图 离线检测
                {
                    img = matimg;
                    // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "相机原图.png", img);
                }
                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                ObbResult upResult = ImageInferObb(WH_UpStopMassDefe_det, img);

                if (upResult != null && upResult.datas.Count > 0) //如果有大缺陷直接退出
                {

                    string lianciIndexstr = Array.FindIndex(upStopMassDefe_names, s => s.Contains("链齿")).ToString();
                    List<ObbData> lianciorg = upResult.datas.FindAll(c => c.lable == lianciIndexstr).ToList(); //链齿
                    // 获取lianciorg中坐标最右的点
                    if (lianciorg?.Count > 0)
                    {
                        float maxx = lianciorg.Max(c => c.box.Center.X);
                        ObbData maxxObb = lianciorg.Find(c => c.box.Center.X == maxx); //高牙链牙的最后一个牙
                        if (maxxObb != null)
                        {
                            float rang = 35;
                            List<ObbData> obbDatas1 = upResult.datas.FindAll(s => Math.Abs(s.box.Center.Y - maxxObb.box.Center.Y) <= rang).ToList(); //分组 与最右边的链齿在一水平线的为一组
                            List<ObbData> obbDatas2 = upResult.datas.FindAll(s => Math.Abs(s.box.Center.Y - maxxObb.box.Center.Y) > rang).ToList(); //另一边为一组
                            #region 高牙
                            // 高牙距离
                            string upmassIndexstr = Array.FindIndex(upStopMassDefe_names, s => s.Contains("上止")).ToString();
                            List<ObbData> upmass1 = obbDatas1.FindAll(c => c.lable == upmassIndexstr).ToList(); //上止

                            if (upmass1 != null && upmass1.Count > 0)
                            {
                                Mat uppatch = GetRoatImage(upmass1[0], img); //高牙上止截图1
                                                                             // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "高牙上止1.png", uppatch);
                                Mat liancipatch = GetRoatImage(maxxObb, img); //上止最近的一颗牙
                                // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "高牙链牙1.png", liancipatch);
                                var sub1 = GetHsSub(uppatch, liancipatch);

                                CoordRestoreData colordisDataH = new CoordRestoreData("高牙色差H", sub1.Item1);
                                dets.Add(colordisDataH);
                                CoordRestoreData colordisDataS = new CoordRestoreData("高牙色差S", sub1.Item2);
                                dets.Add(colordisDataS);
                                uppatch.Dispose();
                                liancipatch.Dispose();
                                // 最右与上止的X方向距离
                                float dismin = Math.Abs(upmass1[0].box.Center.X - maxx);
                                CoordRestoreData disData = new CoordRestoreData("高牙距离", dismin);
                                dets.Add(disData);
                            }
                            else
                            {
                                CoordRestoreData disData = new CoordRestoreData("高牙距离", 1000);
                                dets.Add(disData);
                            }
                            #endregion

                            #region 低牙
                            //低牙距离
                            List<ObbData> upmass2 = obbDatas2.FindAll(c => c.lable == upmassIndexstr).ToList(); //上止
                            if (upmass2 != null && upmass2.Count > 0)
                            {
                                List<ObbData> lianci2 = obbDatas2.FindAll(c => c.lable == lianciIndexstr).ToList(); //链齿
                                float lastx = lianci2.Max(x => x.box.Center.X);
                                ObbData lastobb = lianci2.Find(c => c.box.Center.X == lastx);
                                if (lastobb != null)
                                {
                                    Mat uppatch = GetRoatImage(upmass2[0], img); //高牙上止截图1
                                                                                 // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "低牙上止1.png", uppatch);
                                    Mat liancipatch = GetRoatImage(lastobb, img); //上止最近的一颗牙
                                                                                  // Cv2.ImWrite(@"D:\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "低牙链牙1.png", liancipatch);
                                    var sub1 = GetHsSub(uppatch, liancipatch);

                                    CoordRestoreData colordisDataH = new CoordRestoreData("低牙色差H", sub1.Item1);
                                    dets.Add(colordisDataH);
                                    CoordRestoreData colordisDataS = new CoordRestoreData("低牙色差S", sub1.Item2);
                                    dets.Add(colordisDataS);
                                    uppatch.Dispose();
                                    liancipatch.Dispose();
                                    float dismin = Math.Abs(upmass2[0].box.Center.X - lastobb.box.Center.X);
                                    CoordRestoreData disData = new CoordRestoreData("低牙距离", dismin);
                                    dets.Add(disData);
                                }
                                else
                                {
                                    CoordRestoreData disData = new CoordRestoreData("低牙距离", 1000);
                                    dets.Add(disData);
                                }
                                #region 链牙平齐
                                if (upmass1.Count > 0 && upmass2.Count > 0)
                                {
                                    float firstx = upmass1[0].box.Center.Y - upmass2[0].box.Center.Y;
                                    if (firstx > 0) // 高牙在下
                                    {
                                        //高牙在下
                                        Point2f[] upPoints1 = upmass1[0].box.Points();
                                        float upmaxy = upPoints1.Max(p => p.Y); // 上止
                                        Point2f[] yaPoints1 = maxxObb.box.Points(); //牙
                                        float yamaxy = yaPoints1.Max(p => p.Y); // 牙高点
                                        float xiaSub = Math.Abs(upmaxy - yamaxy);

                                        //低牙在上
                                        Point2f[] upPoints2 = upmass2[0].box.Points();
                                        float upminy2 = upPoints2.Min(p => p.Y); // 上止
                                        Point2f[] yaPoints2 = lastobb.box.Points(); //牙
                                        float yaminy = yaPoints2.Min(p => p.Y); // 牙高点
                                        float shangSub = Math.Abs(upminy2 - yaminy);

                                        CoordRestoreData xiadisData = new CoordRestoreData("高牙平齐", xiaSub);
                                        dets.Add(xiadisData);
                                        CoordRestoreData shangdisData = new CoordRestoreData("低牙平齐", shangSub);
                                        dets.Add(shangdisData);

                                    }
                                    else //低牙在下
                                    {
                                        //高牙在上
                                        Point2f[] upPoints1 = upmass1[0].box.Points();
                                        float upmaxy = upPoints1.Min(p => p.Y); // 上止
                                        Point2f[] yaPoints1 = maxxObb.box.Points(); //牙
                                        float yamaxy = yaPoints1.Min(p => p.Y); // 牙高点
                                        float xiaSub = Math.Abs(upmaxy - yamaxy);

                                        //低牙在下
                                        Point2f[] upPoints2 = upmass2[0].box.Points();
                                        float upminy2 = upPoints2.Max(p => p.Y); // 上止
                                        Point2f[] yaPoints2 = lastobb.box.Points(); //牙
                                        float yaminy = yaPoints2.Max(p => p.Y); // 牙高点
                                        float shangSub = Math.Abs(upminy2 - yaminy);

                                        CoordRestoreData xiadisData = new CoordRestoreData("高牙平齐", xiaSub);
                                        dets.Add(xiadisData);
                                        CoordRestoreData shangdisData = new CoordRestoreData("低牙平齐", shangSub);
                                        dets.Add(shangdisData);
                                    }
                                }
                             
                            }
                                #endregion
                        }
                        else
                        {
                            CoordRestoreData disData = new CoordRestoreData("低牙距离", 1000);
                            dets.Add(disData);
                        }
                            #endregion


                    }
                    for (int j = 0; j < upResult.datas.Count; j++)
                    {
                        int labelindex = int.Parse(upResult.datas[j].lable);
                        string labelname = upStopMassDefe_names[labelindex];
                        CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth,
                            cell.PhotoIndex - 1, 0, 0, labelname, upResult.datas[j]);
                        dets.Add(restoreData);
                    }
                }
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

                    int up_num = upStopMassDefe_names.Length;
                    float Nms = param.Nms;

                    WH_UpStopMassDefe_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, upStopMassDefe_Model_Path, engineType,
CurrentDevice, up_num, param.UpMassScore, Nms, 512);


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
        private Mat GetRoatImage(ObbData obb, Mat img)
        {
            if (obb == null) return null;
            Mat img2 = new Mat();
            if (img.Channels() == 4)
            {
                Cv2.CvtColor(img, img2, ColorConversionCodes.BGRA2BGR);
            }
            else
            {
                img2 = img;
            }
            // 定义斜矩形
            // 定义斜矩形：中心(200,200)，宽80高40，旋转 -30°
            RotatedRect rrect = new RotatedRect(
                obb.box.Center,
                obb.box.Size,
                obb.box.Angle);

            //  坑：OpenCV 的 RotatedRect.Angle 范围是 [-90,0)，
            // 当 |angle|>45 时，width/height 会被自动互换，angle 也偏移
            // 所以取 Size 时建议这样保稳：
            float w = rrect.Size.Width - 10;
            float h = rrect.Size.Height - 5;
            if (Math.Abs(rrect.Angle) > 45)
            {
                (w, h) = (h, w);   // 互换
            }

            Mat patch = new Mat();
            Cv2.GetRectSubPix(img2, new Size(w, h), rrect.Center, patch);

            return patch;

        }
        //private Mat GetRoatImage(ObbData obb, Mat img)
        //{
        //    if (obb == null) return null;
        //    //Mat img2 = new Mat();
        //    //if (img.Channels() == 4)
        //    //{
        //    //    Cv2.CvtColor(img, img2, ColorConversionCodes.BGRA2BGR);
        //    //}
        //    //else
        //    //{
        //    //    img2 = img; 
        //    //}
        //    // 定义斜矩形
        //    float width = obb.box.Size.Width - 25;
        //    if (width < 0)
        //    {
        //        width = obb.box.Size.Width;
        //    }
        //    float height = obb.box.Size.Height - 10;
        //    if (height < 0)
        //    {
        //        height = obb.box.Size.Height;
        //    }
        //    Size2f smallsize = new Size2f(width, height);
        //    RotatedRect rrect = new RotatedRect(
        //    obb.box.Center,
        //    smallsize,
        //    obb.box.Angle);

        //    // 1. 算旋转矩阵（绕 rrect.Center 转 -angle，即把矩形掰正）
        //    Mat rotMat = Cv2.GetRotationMatrix2D(rrect.Center, rrect.Angle, 1.0);

        //    // 2. 整图仿射变换
        //    Mat rotated = new Mat();
        //    Cv2.WarpAffine(img, rotated, rotMat, img.Size(),
        //        InterpolationFlags.Linear, BorderTypes.Replicate);

        //    Point2f[] pts = rrect.Points();

        //    //// 直接手动旋转（更快、更清晰）
        //    //Point2f center = rrect.Center;
        //    //double a = rrect.Angle * Math.PI / 180.0;
        //    //double cosA = Math.Cos(a);
        //    //double sinA = Math.Sin(a);

        //    //Point[] intPts = pts.Select(p =>
        //    //{
        //    //    float dx = p.X - center.X;
        //    //    float dy = p.Y - center.Y;
        //    //    return new Point(
        //    //        (int)(center.X + dx * cosA - dy * sinA),
        //    //        (int)(center.Y + dx * sinA + dy * cosA)
        //    //    );
        //    //}).ToArray();

        //    //Rect roi = Cv2.BoundingRect(intPts);
        //    //roi = roi.Intersect(new Rect(0, 0, rotated.Width, rotated.Height));
        //    //Mat patch = new Mat(rotated, roi);

        //    // 构造 Mat
        //    Mat mapPts = new Mat(pts.Length, 1, MatType.CV_32FC2);
        //    mapPts.SetArray(pts);

        //    // 仿射变换
        //    Cv2.Transform(mapPts, mapPts, rotMat);

        //    // ✅ 正确取出 Point2f[]
        //    Point2f[] transformedPts = new Point2f[pts.Length];
        //    mapPts.GetArray(out transformedPts);

        //    // 转成 Point[]
        //    Point[] intPts = Array.ConvertAll(transformedPts, p => new Point((int)p.X, (int)p.Y));

        //    // 计算 ROI
        //    Rect roi = Cv2.BoundingRect(intPts);
        //   // roi = roi.Intersect(new Rect(0, 0, rotated.Width, rotated.Height));

        //    Mat patch = new Mat(rotated, roi);
        //    mapPts.Dispose();
        //    rotated.Dispose();
        //    rotMat.Dispose();
        //    return patch;

        //}
        /// <summary>
        /// 获取两张图片H S值的差值
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        private (float, float) GetHsSub(Mat src, Mat dst)
        {
            Mat hsvImage = new Mat();
            Cv2.CvtColor(src, hsvImage, ColorConversionCodes.BGR2HSV);
            Scalar hsvMean = Cv2.Mean(hsvImage);
            double hMean = hsvMean.Val0;
            double sMean = hsvMean.Val1;

            Mat hsvImage2 = new Mat();
            Cv2.CvtColor(dst, hsvImage2, ColorConversionCodes.BGR2HSV);
            Scalar hsvMean2 = Cv2.Mean(hsvImage2);
            double hMean2 = hsvMean2.Val0;
            double sMean2 = hsvMean2.Val1;

            hsvImage.Dispose();
            hsvImage2.Dispose();
            float subH = (float)Math.Abs(hMean - hMean2);
            float subS = (float)Math.Abs(sMean - sMean2);

            return (subH, subS);
        }

        private void UpdateScore(CParam param)
        {
            WH_UpStopMassDefe_det?.UpdateNMS_Score(param.Nms, param.UpMassScore);
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
    [property: DisplayName("09 大缺陷分数阈值")]
    [property: Description("大缺陷分数阈值")]
    private float upMassScore = 0.4f;

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
