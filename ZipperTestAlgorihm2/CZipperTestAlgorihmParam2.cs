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


namespace ZipperTestAlgorihm2
{
    public class CZipperTestAlgorihmParam2 : CAlgorithmParamBase
    {


        /// <summary>
        /// 下止检测对象
        /// </summary>
        IVisionModel yolo_DownStopMass_obb;

        /// <summary>
        /// 上止检测对象
        /// </summary>
        IVisionModel yolo_UpStopMassDefe_det;

        /// <summary>
        /// 上止测量对象
        /// </summary>
        IVisionModel yolo_UpStopMassMeas_obb;

        /// <summary>
        /// 金属拉头检测对象
        /// </summary>
        IVisionModel yolo_Meta_pull_det;

        /// <summary>
        /// 烤漆拉头检测对象
        /// </summary>
        IVisionModel yolo_Paint_pull_det;


        /// <summary>
        /// 拉头查找对象
        /// </summary>
        IVisionModel yolo_pull_Serach_det;



        /// <summary>
        /// 大缺陷检测对象
        /// </summary>
        IVisionModel yolo_BigDet_det;


        public CZipperTestAlgorihmParam2(string user) : base()
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


        }

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 通用模型路径
        /// </summary>
      //  private string Common_Model_Path;
        /// <summary>
        /// 下止模型路径
        /// </summary>
        private string downStopMass_Model_Path;
        /// <summary>
        /// 上止模型路径
        /// </summary>
        private string upStopMassDefe_Model_Path;

        /// <summary>
        /// 上止模型路径
        /// </summary>
        private string upStopMassMeas_Model_Path;
        /// <summary>
        /// 拉头匹配模型路径
        /// </summary>
        private string pull_Search_Model_Path;

        /// <summary>
        /// 金属拉头模型路径
        /// </summary>
        private string pull_Meta_Model_Path;

        /// <summary>
        /// 烤漆拉头模型路径
        /// </summary>
        private string pull_Paint_Model_Path;

        /// <summary>
        /// Logo模型路径
        /// </summary>
       // private string pull_Logo_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 大缺陷模型路径
        /// </summary>
        private string Big_Model_Path;

        /// <summary>
        /// 2025.11.17 鲍赞宝
        /// 拉片分割模型路径
        /// </summary>
      //  private string pullSharp_Model_Path;


        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        private string name_Path;
        //常规缺陷名称
       // protected string[] Common_names;
        //上止缺陷名称
        protected string[] upStopMassDefe_names;

        //上止测量名称
        protected string[] upStopMassMeas_names;
        //下止缺陷名称
        protected string[] downStopMass_names;

        //拉头匹配对对象名称
        protected string[] pull_Search_names;

        //金属拉头匹配对对象名称
        protected string[] pull_Meta_names;

        //烤漆拉头匹配对对象名称
        protected string[] pull_Paint_names;
        //Logo匹配对对象名称
       // protected string[] pull_Logo_names;

        //拉头拉片缺陷名称
        protected string[] bigDet_names;

        //拉片分割缺陷名称
      //  protected string[] pullSharp_names;

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

            #region 大缺陷
            List<CDefectRecipe> bigRecipes = new List<CDefectRecipe>();
            DefectSpecies = new List<CDefectSpecies>();

            for (int i = 0; i < bigDet_names.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(bigDet_names[i], Category.区域);
                bigRecipes.Add(defectRecipe);
            }

            CDefectSpecies bigSpecies = new CDefectSpecies("大缺陷", bigRecipes);
            #endregion
            #region 通用
            List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
            //for (int i = 0; i < Common_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe = new CDefectRecipe(Common_names[i], Category.区域);
            //    cDefectRecipes.Add(defectRecipe);
            //}
            if (user.Contains( "正面"))
            {
                CDefectRecipe defectRecipe1_1 = new CDefectRecipe("上止距离1", Category.值);
                CDefectRecipe defectRecipe1_2 = new CDefectRecipe("上止距离2", Category.值);
                cDefectRecipes.Add(defectRecipe1_1);
                cDefectRecipes.Add(defectRecipe1_2);
                CDefectRecipe defectRecipe2 = new CDefectRecipe("上止高低", Category.值);
                cDefectRecipes.Add(defectRecipe2);
                CDefectRecipe defectRecipe3 = new CDefectRecipe("下止距离", Category.值);
                cDefectRecipes.Add(defectRecipe3);
                CDefectRecipe defectRecipe4 = new CDefectRecipe("下止歪", Category.值);
                cDefectRecipes.Add(defectRecipe4);
                CDefectRecipe defectRecipe5 = new CDefectRecipe("下止偏", Category.值);
                cDefectRecipes.Add(defectRecipe5);
            }
            CDefectSpecies defectSpecies = new CDefectSpecies("拉链", cDefectRecipes);
            #endregion
            #region 上止
            if (upStopMassDefe_names?.Length > 0)
            {
                //string[] upstrs = upStopMassDefe_names.Where(s => s != "注塑正面上止" && s != "链齿").ToArray();
                for (int i = 0; i < upStopMassDefe_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(upStopMassDefe_names[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
            }

            #endregion
            #region 下止
            if (downStopMass_names?.Length > 0)
            {
                string[] Downstrs = downStopMass_names.Where(s => s != "注塑正面下止" && s != "链齿" && s != "链牙").ToArray();
                for (int i = 0; i < Downstrs.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(Downstrs[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
                CDefectRecipe defectRecipe5 = new CDefectRecipe("下止露牙", Category.区域);
                cDefectRecipes.Add(defectRecipe5);
            }

            #endregion

            #region 拉头 拉片 LOGO 拉片外形

            //int sbsindex = pull_names.ToList().IndexOf("SBS");
            //string[] pullstrs = pull_names.Take(sbsindex).ToArray();
            //string[] logostrs = pull_names.Skip(sbsindex).ToArray();

            // string[] pullstrs = pull_Meta_names.Where(s => s.Contains("拉")).ToArray();


            List<CDefectRecipe> pullRecipes = new List<CDefectRecipe>();
            for (int i = 0; i < pull_Meta_names.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(pull_Meta_names[i], Category.区域);
                pullRecipes.Add(defectRecipe);
            }
            for (int i = 0; i < pull_Paint_names.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(pull_Paint_names[i], Category.区域);
                pullRecipes.Add(defectRecipe);
            }
            //for (int i = 0; i < pullSharp_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe2 = new CDefectRecipe(pullSharp_names[i], Category.值);
            //    pullRecipes.Add(defectRecipe2);
            //}
            //颜色
            CDefectRecipe defectRecipe1_H = new CDefectRecipe("H", Category.值);
            CDefectRecipe defectRecipe1_S = new CDefectRecipe("S", Category.值);
            pullRecipes.Add(defectRecipe1_H);
            pullRecipes.Add(defectRecipe1_S);

            CDefectSpecies pullSpecies = new CDefectSpecies("拉头拉片", pullRecipes);

            List<CDefectRecipe> logoRecipes = new List<CDefectRecipe>();
            //for (int i = 0; i < pull_Logo_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe = new CDefectRecipe(pull_Logo_names[i], Category.区域);
            //    logoRecipes.Add(defectRecipe);
            //}
            CDefectSpecies logoSpecies = new CDefectSpecies("LOGO", logoRecipes);



            #endregion

          // DefectSpecies.Add(defectSpecies);
            DefectSpecies.Add(bigSpecies);
            //DefectSpecies.Add(pullSpecies);
            //DefectSpecies.Add(logoSpecies);

        }

        protected void ReadNames(string user)
        {

            string modelDirpath = ".\\AlgorithmPlug\\ZipperTestAlgorihm2\\Models\\";

            string commonModelPath = modelDirpath + "CommonModel\\";
            string bigModelPath = modelDirpath + "BigDetModel\\";

            if (user.Contains("正面"))
            {
               // commonModelPath = commonModelPath + "Front\\";
                bigModelPath = bigModelPath + "Front\\";
            }
            else
            {
               // commonModelPath = commonModelPath + "Back\\";
                bigModelPath = bigModelPath + "Back\\";
            }
            //if (user == "反面")
            //{
            //    commonModelPath = commonModelPath + "Back\\";
            //    bigModelPath = bigModelPath + "Back\\";
            //}
            //var commons = GetNames(commonModelPath);
            //if (commons.Item1 != "")
            //{
            //    Common_Model_Path = commons.Item1;
            //    Common_names = commons.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            //}
            var bigstrs = GetNames(bigModelPath);
            if (bigstrs.Item1 != "")
            {
                Big_Model_Path = bigstrs.Item1;
                bigDet_names = bigstrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }
            if (user.Contains("正面"))
            {
                string downStopMassPath = modelDirpath + "DownStopMassModel\\";
                var downstopstrs = GetNames(downStopMassPath);
                if (downstopstrs.Item1 != "")
                {
                    downStopMass_Model_Path = downstopstrs.Item1;
                    downStopMass_names = downstopstrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                }
                string upStopMassDefeModelPath = modelDirpath + "UpStopMassModel\\UpStopMassDefe";
                var upstopsdefetrs = GetNames(upStopMassDefeModelPath);
                if (upstopsdefetrs.Item1 != "")
                {
                    upStopMassDefe_Model_Path = upstopsdefetrs.Item1;
                    upStopMassDefe_names = upstopsdefetrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                }

                string upStopMassMeasModelPath = modelDirpath + "UpStopMassModel\\UpStopMassMeas";
                var upstopsmeastrs = GetNames(upStopMassMeasModelPath);
                if (upstopsmeastrs.Item1 != "")
                {
                    upStopMassMeas_Model_Path = upstopsmeastrs.Item1;
                    upStopMassMeas_names = upstopsmeastrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                }
            }


            string pullModelScearch = modelDirpath + "Pull\\PullSearch\\";
            var pullsearchtrs = GetNames(pullModelScearch);
            if (pullsearchtrs.Item1 != "")
            {
                pull_Search_Model_Path = pullsearchtrs.Item1;
                pull_Search_names = pullsearchtrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            string pullMetaModel = modelDirpath + "Pull\\PullMetaModel\\";
            var metapulltrs = GetNames(pullMetaModel);
            if (metapulltrs.Item1 != "")
            {
                pull_Meta_Model_Path = metapulltrs.Item1;
                pull_Meta_names = metapulltrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            string pullPaintModel = modelDirpath + "Pull\\PullPaintModel\\";
            var paintpulltrs = GetNames(pullPaintModel);
            if (paintpulltrs.Item1 != "")
            {
                pull_Paint_Model_Path = paintpulltrs.Item1;
                pull_Paint_names = paintpulltrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            //string pullLogoModel = modelDirpath + "Pull\\PullLogoModel\\";
            //var logopulltrs = GetNames(pullLogoModel);
            //if (logopulltrs.Item1 != "")
            //{
            //    pull_Logo_Model_Path = logopulltrs.Item1;
            //    pull_Logo_names = logopulltrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            //}

            //string pullSegModelpath = modelDirpath + "Pull\\PullSegModel\\";
            //var pullSegtrs = GetNames(pullSegModelpath);
            //if (pullSegtrs.Item1 != "")
            //{
            //    pullSharp_Model_Path = pullSegtrs.Item1;
            //    pullSharp_names = pullSegtrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            //}

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
              //  UpdateScore(paramClass);
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
//            CParam param = AlgorParams.FirstOrDefault() as CParam;
//            if (param != null)
//            {
//                if (param != null)
//                {
//                    string CurrentDevice = param.CurrentDevice;
//                    int common_Categ_num = Common_names.Length;

//                    int downmass_num = 0;
//                    if (downStopMass_names?.Length > 0)
//                    {
//                        downmass_num = downStopMass_names.Length;
//                    }
//                    int upmass_num = 0;
//                    if (upStopMassDefe_names?.Length > 0)
//                    {
//                        upmass_num = upStopMassDefe_names.Length;
//                    }

//                    int upmassmeas_num = 0;
//                    if (upStopMassMeas_names?.Length > 0)
//                    {
//                        upmassmeas_num = upStopMassMeas_names.Length;
//                    }

//                    int pull_search_num = pull_Search_names.Length;
//                    int metapull_num = pull_Meta_names.Length;
//                    int paintpull_num = pull_Paint_names.Length;
//                    int logopull_num = pull_Logo_names.Length;
//                    int big_num = bigDet_names.Length;
//                    int pullsharp_num = pullSharp_names.Length;
//                    float Score = param.CommonScore;
//                    float Nms = param.Nms;
//                    int Input_size = 640;

//                    if (downmass_num > 0)
//                    {
//                        yolo_DownStopMass_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, downStopMass_Model_Path, EngineType.TensorRT,
//CurrentDevice, downmass_num, param.DownScore, Nms, 256);
//                    }
//                    //  });

//                    //Task task6 = Task.Run(() =>
//                    //{
//                    if (upmass_num > 0)
//                    {
//                        yolo_UpStopMassDefe_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, upStopMassDefe_Model_Path, EngineType.TensorRT,
//    CurrentDevice, upmass_num, param.UpScore, Nms, 192);
//                    }
//                    //  });

//                    if (upmassmeas_num > 0)
//                    {
//                        yolo_UpStopMassMeas_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, upStopMassMeas_Model_Path, EngineType.TensorRT,
//    CurrentDevice, upmassmeas_num, param.UpLianciScore, Nms, 192);
//                    }

//                    //Task task7 = Task.Run(() =>
//                    //{
//                    yolo_pull_Serach_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Search_Model_Path, EngineType.TensorRT,
//CurrentDevice, pull_search_num, param.AutoScore, Nms, 480);
//                    // });

//                    // Task task8 = Task.Run(() =>
//                    // {
//                    yolo_Meta_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Meta_Model_Path, EngineType.TensorRT,
//CurrentDevice, metapull_num, param.MetaPullScore, Nms, 640);
//                    //});

//                    yolo_Paint_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Paint_Model_Path, EngineType.TensorRT,
//CurrentDevice, paintpull_num, param.PaintPullScore, Nms, 640);

//                }

//            }
        }

        public DetResult ImageInferDet(IVisionModel yolo, Mat img)
        {
            if (yolo != null)
            {
                DetResult resultDet;
                resultDet = yolo.Predict(img) as DetResult;
                return resultDet;
            }
            else
            {
                return null;
            }


        }
        public ObbResult ImageInferObb(IVisionModel yolo, Mat img)
        {
            if (yolo != null)
            {
                ObbResult resultDet;
                resultDet = yolo.Predict(img) as ObbResult;
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
        /// 计算两轮廓相识度，越接近0越相似
        /// </summary>
        /// <returns></returns>
        //double MatchShapesUsingHuMoments(Point[] contours1, Point[] contours2)
        //{
        //    Moments moments1 = Cv2.Moments(contours1);
        //    Moments moments2 = Cv2.Moments(contours2);

        //    double[] hu1 = moments1.HuMoments();
        //    double[] hu2 = moments2.HuMoments();

        //    //计算相似度（值越小越相似）
        //    double similarity = 0;
        //    for (int i = 0; i < 7; i++)
        //    {
        //        double a = Math.Abs(hu1[i]);
        //        double b = Math.Abs(hu2[i]);

        //        if (a + b > 0)
        //        {
        //            similarity += Math.Abs(a - b) / Math.Abs(a + b);
        //        }
        //    }

        //    return similarity;
        //}
        // 容差值，用于判断Hu矩是否接近零
        private const double Epsilon = 1e-10;

        // 各阶Hu矩的权重（可根据需求调整）
        private static readonly double[] Weights = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

        public double MatchShapesUsingHuMoments(Point[] contours1, Point[] contours2)
        {
            // ========== 输入验证 ==========
            if (contours1 == null || contours2 == null)
            {
                throw new ArgumentNullException("轮廓点集不能为null");
            }

            if (contours1.Length < 3 || contours2.Length < 3)
            {
                throw new ArgumentException("轮廓至少需要3个点才能构成有效形状");
            }

            // ========== 计算几何矩 ==========
            Moments moments1 = Cv2.Moments(contours1);
            Moments moments2 = Cv2.Moments(contours2);

            // ========== 计算Hu不变矩 ==========
            double[] hu1 = moments1.HuMoments();
            double[] hu2 = moments2.HuMoments();

            // ========== 计算加权相似度 ==========
            double totalSimilarity = 0.0;

            for (int i = 0; i < 7; i++)
            {
                double h1 = Math.Abs(hu1[i]);
                double h2 = Math.Abs(hu2[i]);

                // 处理两个值都接近零的情况
                if (h1 < Epsilon && h2 < Epsilon)
                {
                    // 两者都接近零，认为此维度完全相似
                    continue;
                }
                else if (h1 < Epsilon || h2 < Epsilon)
                {
                    // 只有一个接近零，完全不相似
                    totalSimilarity += Weights[i] * 1.0;
                }
                else
                {
                    // 使用对数距离（更稳定的度量方式）
                    // 将相对差异转换为对数域计算
                    double logDiff = Math.Abs(Math.Log(h1) - Math.Log(h2));

                    // 使用tanh函数将结果压缩到[0,1]范围
                    // tanh(x) 在x较小时近似为x，x较大时趋近于1
                    totalSimilarity += Weights[i] * Math.Tanh(logDiff);
                }
            }

            return totalSimilarity;
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


        //private BitmapSource Mat2BitmapSource(Mat img)
        //{
        //    // 方法1：编码为 PNG 字节流
        //    Cv2.ImEncode(".png", InputArray.Create(img), out byte[] imageBytes);

        //    // 方法2：通过 MemoryStream 转换
        //    using (MemoryStream ms = new MemoryStream(imageBytes))
        //    {
        //        //// 方式A：直接创建 BitmapSource（需指定像素格式）
        //        //BitmapSource bitmapSource = BitmapSource.Create(
        //        //    img.Width,
        //        //    img.Height,
        //        //    96, 96, // DPI
        //        //    PixelFormats.Pbgra32, // OpenCV 默认 BGR 格式
        //        //null,
        //        //imageBytes,
        //        //    img.Width * (img.Channels() == 1 ? 1 : 4) // 每行字节数
        //        //);

        //        // 方式B：通过 PngBitmapEncoder（更通用）
        //        //BmpBitmapEncoder encoder = new BmpBitmapEncoder();
        //        //encoder.Frames.Add(BitmapFrame.Create(ms));
        //        // BitmapSource enbitmapSource = encoder.Frames[0];
        //        BitmapSource enbitmapSource = BitmapFrame.Create(ms);
        //        BitmapSource bitmapSource = new CachedBitmap(enbitmapSource, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        //        //if (bitmapSource is BitmapFrameDecode)
        //        //{
        //        //    // 方案1：转换为缓存位图


        //        //    // 方案2：克隆像素数据
        //        //    var writable = new WriteableBitmap(source);
        //        //    writable.Freeze();
        //        //    return writable;
        //        //}
        //        bitmapSource.Freeze();

        //        return bitmapSource;



        //    }
        //}

        //private BitmapSource Mat2BitmapSource(Mat img)
        //{
        //    using (System.Drawing.Bitmap bitmap = img.ToBitmap())
        //    {
        //        BitmapSource bitimg = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //           bitmap.GetHbitmap(),
        //           IntPtr.Zero,
        //           System.Windows.Int32Rect.Empty,
        //           BitmapSizeOptions.FromEmptyOptions());
        //        bitimg.Freeze();
        //        return bitimg;
        //    }
        //}

        /// <summary>
        /// 裁剪BitmapSource的核心方法
        /// </summary>
        private BitmapSource CropBitmapSource(BitmapSource source, System.Windows.Int32Rect cropRect)
        {
            // 计算像素缓冲区大小
            int stride = source.Format.BitsPerPixel * cropRect.Width / 8;
            byte[] buffer = new byte[cropRect.Height * stride];

            // 复制目标区域的像素数据（高效内存操作）
            source.CopyPixels(cropRect, buffer, stride, 0);

            // 创建新BitmapSource（保留原始DPI和色彩格式）
            return BitmapSource.Create(
                cropRect.Width,
                cropRect.Height,
                source.DpiX,
                source.DpiY,
                source.Format,
                source.Palette,
                buffer,
                stride
            );
        }

        private void UpdateScore(CParam param)
        {

            if (yolo_UpStopMassDefe_det != null)
            {
                yolo_UpStopMassDefe_det.UpdateNMS_Score(param.Nms, param.UpScore);
            }
            if (yolo_UpStopMassMeas_obb != null)
            {
                yolo_UpStopMassMeas_obb.UpdateNMS_Score(param.Nms, param.UpLianciScore);
            }
            if (yolo_DownStopMass_obb != null)
            {
                yolo_DownStopMass_obb.UpdateNMS_Score(param.Nms, param.DownScore);
            }
            //yolo_pull_Serach_det.UpdateNMS_Score(param.PullScore, param.Nms);
            yolo_Meta_pull_det.UpdateNMS_Score(param.Nms, param.MetaPullScore);
            yolo_Paint_pull_det.UpdateNMS_Score(param.Nms, param.PaintPullScore);
            // yolo_Logo_pull_det.UpdateNMS_Score(0.8f, param.LogoPullScore);

            yolo_BigDet_det.UpdateNMS_Score(param.Nms, param.BigScore);
            yolo_pull_Serach_det.UpdateNMS_Score(param.Nms, param.AutoScore);

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
        /// 通用模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("01 通用模型分数阈值")]
        [property: Description("通用模型分数阈值")]
        private float commonScore = 0.3f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 下止模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("02 下止缺陷分数阈值")]
        [property: Description("下止缺陷分数阈值")]
        private float downScore = 0.4f;

        /// <summary>
        /// 2025.11.07 鲍赞宝
        /// 下止模型的链齿分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("03 下止链齿分数阈值")]
        [property: Description("下止链齿分数阈值")]
        private float downLianciScore = 0.6f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 上止模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("04 上止缺陷分数阈值")]
        [property: Description("上止缺陷分数阈值")]
        private float upScore = 0.4f;

        /// <summary>
        /// 2025.11.07 鲍赞宝
        /// 上止模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("05 上止链齿分数阈值")]
        [property: Description("上止链齿分数阈值")]
        private float upLianciScore = 0.7f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("06 金属拉头分数阈值")]
        [property: Description("金属拉头分数阈值")]
        private float metaPullScore = 0.4f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("07 烤漆拉头分数阈值")]
        [property: Description("烤漆拉头分数阈值")]
        private float paintPullScore = 0.4f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("08 Logo分数阈值")]
        [property: Description("Logo分数阈值")]
        private float logoPullScore = 0.4f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("09 大缺陷分数阈值")]
        [property: Description("大缺陷分数阈值")]
        private float bigScore = 0.4f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("10 自动识别分数阈值")]
        [property: Description("自动识别分数阈值")]
        private float autoScore = 0.45f;

        /// <summary>
        /// 2024.7.21 鲍赞宝
        /// 拉头模型分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("分数设置")]
        [property: DisplayName("11 拉片分割分数阈值")]
        [property: Description("拉片分割分数阈值")]
        private float pullSharpScore = 0.5f;

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



        ///// <summary>
        ///// 20250331 TCG
        ///// 模型尺寸
        ///// </summary>
        //[ObservableProperty]
        //private InputImgSize input_size = InputImgSize.IN640;

        ///// <summary>
        ///// 20250331 TCG
        ///// 模型尺寸
        ///// </summary>
        //[ObservableProperty]
        //private ImgSize output_size = ImgSize.S640;

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 驱动设备
        /// </summary>
        //[ObservableProperty]
        //private string currentDevice = "GPU.0";

        ///// <summary>
        ///// 2024.10.28 鲍赞宝
        ///// 驱动设备
        ///// </summary>
        //[ObservableProperty]
        //private EngineType engineType = EngineType.OpenVINO;

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
