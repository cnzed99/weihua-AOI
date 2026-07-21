using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using HalconDotNet;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using SharpCompress;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using WH.VisionLearning;


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

        OpenCvSharp.Point[] offlineContours;
        /// <summary>
        /// 离线测试拉片圆孔模板
        /// </summary>

        OpenCvSharp.Point[] offlineHoleContours;

        float offlineHvalue, offlineSvalue, offlineVvalue;

        HTuple offlineModel_Pull;

        HTuple offlineModel_Logo;

        float offlineRow = 0;
        float offlineCol = 0;

        HObject BackRectangle;

        float offinePullArea = 0;

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
            CDefectRecipe defectRecipe1_V = new CDefectRecipe("拉头色差3", Category.值);
            pullRecipes.Add(defectRecipe1_H);
            pullRecipes.Add(defectRecipe1_S);
            pullRecipes.Add(defectRecipe1_V);

            if (user == "拉片")
            {
                CDefectRecipe defectRecipe2 = new CDefectRecipe("拉片外形", Category.值);
                pullRecipes.Add(defectRecipe2);
                CDefectRecipe defectRecipe3 = new CDefectRecipe("孔洞外形", Category.值);
                pullRecipes.Add(defectRecipe3);

                CDefectRecipe defectRecipe4 = new CDefectRecipe("拉片面积", Category.值);
                pullRecipes.Add(defectRecipe4);
                CDefectRecipe defectRecipe5 = new CDefectRecipe("拉片周长", Category.值);
                pullRecipes.Add(defectRecipe5);

                CDefectRecipe defectRecipe6 = new CDefectRecipe("孔洞面积", Category.值);
                pullRecipes.Add(defectRecipe6);
                CDefectRecipe defectRecipe7 = new CDefectRecipe("孔洞周长", Category.值);
                pullRecipes.Add(defectRecipe7);

                List<CDefectRecipe> logoRecipes = new List<CDefectRecipe>();
                CDefectRecipe defectRecipelogo = new CDefectRecipe("Logo", Category.值);
                logoRecipes.Add(defectRecipelogo);
                CDefectSpecies logoSpecies = new CDefectSpecies("LOGO", logoRecipes);
                DefectSpecies.Add(logoSpecies);
            }

            CDefectSpecies pullSpecies = new CDefectSpecies("拉头拉片", pullRecipes);
            #endregion
            DefectSpecies.Add(pullSpecies);



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

                    HObject ho_Image = new HObject();
                    if (cell.ImageFile == "")
                    {
                        HOperatorSet.GenImageInterleaved(out ho_Image,
                            cell.Image.ImageData,
                            "rgb",
                            cell.Image.ImageWidth,
                            cell.Image.ImageHeight,
                            -1,
                            "byte",
                            0,
                            0,
                            0,
                            0,
                            -1,
                            0
                            );
                    }
                    else
                    {
                        HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                    }

                    Point[] PullPoints = null; Point[] HolesPoints = null;
                    float Hvalue = 0; float Svalue = 0; float Vvalue = 0; float logoScore = 0;
                    HTuple ModelID_Logo = null, ModelID_Pull = null;

                    if (paramClass.OffLinePullerTemplateEnabel)
                    {
                        GetContoursAndHSVAndModel(ho_Image, out HObject ho_Rectangle, out PullPoints, out HolesPoints,
                            out Hvalue, out Svalue, out Vvalue, out ModelID_Logo,
                            out _, out _, out _, out _, out ModelID_Pull, out HTuple hv_RowRef, out HTuple hv_ColumnRef, out HTuple pullArea);

                        paramClass.OffLinePullerTemplateEnabel = false;
                        offlineContours = PullPoints;
                        offlineHoleContours = HolesPoints;
                        offlineHvalue = Hvalue;
                        offlineSvalue = Svalue;
                        offlineVvalue = Vvalue;
                        offlineModel_Logo = ModelID_Logo;
                        offlineModel_Pull = ModelID_Pull;
                        offlineRow = (float)hv_RowRef.D;
                        offlineCol = (float)hv_ColumnRef.D;
                        BackRectangle = ho_Rectangle;
                        offinePullArea = (float)pullArea;
                    }


                    float orgconarea = 0; float orgconArclength = 0; //基准拉片面积和周长
                    float orgholeconarea = 0; float orgholeconArclength = 0; //基准孔洞面积和周长

                    float pullsDiffH = 0;
                    float pullsDiffS = 0;
                    float pullsDiffV = 0;

                    if (cell.ImageFile == "")//在线
                    {

                        GetContoursAndHSV(cell, paramClass, ho_Image, cell.BackRectangle, cell.PullModelRow, cell.PullModelCol, cell.ModelID_Logo, cell.ModelID_Pull,
                            out PullPoints, out HolesPoints, out Hvalue, out Svalue, out Vvalue, out logoScore,out float pullarea);
                        CoordRestoreData logorestoreData = new CoordRestoreData("Logo", logoScore);
                        dets.Add(logorestoreData);

                        GetAreaAndArcLength(cell.PullOrgContours, out orgconarea, out orgconArclength);
                        GetAreaAndArcLength(cell.PullHoldOrgContours, out orgholeconarea, out orgholeconArclength);
                        pullsDiffH = Math.Abs(cell.PullsOrgHvalue - Hvalue);
                        pullsDiffS = Math.Abs(cell.PullsOrgSvalue - Svalue);
                        pullsDiffV = Math.Abs(cell.PullsOrgVvalue - Vvalue);

                        //double simiValue = MatchShapesWithCv2(cell.PullOrgContours, PullPoints);
                        float bigpullareaDiff = Math.Abs(cell.PullSegOrgArea - pullarea);
                        CoordRestoreData restoreData = new CoordRestoreData("拉片外形", bigpullareaDiff, PullPoints);
                        dets.Add(restoreData);
                        double holdsimiValue = MatchShapesWithCv2(cell.PullHoldOrgContours, HolesPoints);
                        CoordRestoreData restoreData1 = new CoordRestoreData("孔洞外形", (float)holdsimiValue, HolesPoints);
                        dets.Add(restoreData1);
                    }
                    else
                    {
                        GetContoursAndHSV(cell, paramClass, ho_Image, BackRectangle, offlineRow, offlineCol, offlineModel_Logo, offlineModel_Pull,
                           out PullPoints, out HolesPoints, out Hvalue, out Svalue, out Vvalue, out logoScore, out float pullarea);
                        CoordRestoreData logorestoreData = new CoordRestoreData("Logo", logoScore);
                        dets.Add(logorestoreData);
                        GetAreaAndArcLength(offlineContours, out orgconarea, out orgconArclength);
                        GetAreaAndArcLength(offlineHoleContours, out orgholeconarea, out orgholeconArclength);
                        pullsDiffH = Math.Abs(offlineHvalue - Hvalue);
                        pullsDiffS = Math.Abs(offlineSvalue - Svalue);
                        pullsDiffV = Math.Abs(offlineVvalue - Vvalue);

                        //double simiValue = MatchShapesWithCv2(cell.PullOrgContours, PullPoints);
                        float bigpullareaDiff = Math.Abs(offinePullArea - pullarea);
                        CoordRestoreData restoreData = new CoordRestoreData("拉片外形", bigpullareaDiff, PullPoints);
                        dets.Add(restoreData);
                        double holdsimiValue = MatchShapesWithCv2(offlineHoleContours, HolesPoints);
                        CoordRestoreData restoreData1 = new CoordRestoreData("孔洞外形", (float)holdsimiValue, HolesPoints);
                        dets.Add(restoreData1);
                    }

                    GetAreaAndArcLength(PullPoints, out float pullconarea, out float pullconArclength);
                    GetAreaAndArcLength(HolesPoints, out float holeconarea, out float holeconArclength);

                    float pullareaDiff = Math.Abs(orgconarea - pullconarea);
                    float pullarcLengthDiff = Math.Abs(orgconArclength - pullconArclength);

                    float holeAreaDiff = Math.Abs(orgholeconarea - holeconarea);
                    float holeArcLengthDiff = Math.Abs(orgholeconArclength - holeconArclength);

                    CoordRestoreData pullareaData = new CoordRestoreData("拉片面积", pullareaDiff);
                    dets.Add(pullareaData);
                    CoordRestoreData pullacrLenghtData = new CoordRestoreData("拉片周长", pullarcLengthDiff);
                    dets.Add(pullacrLenghtData);

                    CoordRestoreData holeareaData = new CoordRestoreData("孔洞面积", holeAreaDiff);
                    dets.Add(holeareaData);
                    CoordRestoreData holeacrLenghtData = new CoordRestoreData("孔洞周长", holeArcLengthDiff);
                    dets.Add(holeacrLenghtData);

                    CoordRestoreData disDataH = new CoordRestoreData("拉头色差1", pullsDiffH);
                    CoordRestoreData disDataS = new CoordRestoreData("拉头色差2", pullsDiffS);
                    CoordRestoreData disDataV = new CoordRestoreData("拉头色差3", pullsDiffV);
                    dets.Add(disDataH);
                    dets.Add(disDataS);
                    dets.Add(disDataV);
                }
                #endregion
                #region 拉头颜色
                if (User == "拉头")
                {
                    //int px = 190, py = 280;
                    //int rew = 100, reh = 50;

                    //Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                    //// Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                    //Mat hsvImage = new Mat();
                    //Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                    //Scalar hsvMean = Cv2.Mean(hsvImage);
                    //// HSV通道说明：
                    //// H: 0-179 (色调)
                    //// S: 0-255 (饱和度)
                    //// V: 0-255 (明度)
                    //float hMean = (float)hsvMean.Val0;
                    //float sMean = (float)hsvMean.Val1;
                    //float vMean = (float)hsvMean.Val2;
                    GetPullerHSV(cell, out float hMean, out float sMean, out float vMean);
                    float pullsDiffH = 0;
                    float pullsDiffS = 0;
                    float pullsDiffV = 0;
                    if (paramClass.OffLinePullerTemplateEnabel)
                    {
                        paramClass.OffLinePullerTemplateEnabel = false;
                        offlineHvalue = hMean;
                        offlineSvalue = sMean;
                        offlineVvalue = vMean;
                    }
                    if (cell.ImageFile == "")//在线
                    {
                        pullsDiffH = Math.Abs(cell.PullerOrgHvalue - hMean);
                        pullsDiffS = Math.Abs(cell.PullerOrgSvalue - sMean);
                        pullsDiffV = Math.Abs(cell.PullerOrgVvalue - vMean);
                    }
                    else
                    {

                        pullsDiffH = Math.Abs(offlineHvalue - hMean);
                        pullsDiffS = Math.Abs(offlineSvalue - sMean);
                        pullsDiffV = Math.Abs(offlineVvalue - vMean);

                    }
                    CoordRestoreData disDataH = new CoordRestoreData("拉头色差1", pullsDiffH);
                    CoordRestoreData disDataS = new CoordRestoreData("拉头色差2", pullsDiffS);
                    CoordRestoreData disDataV = new CoordRestoreData("拉头色差3", pullsDiffV);
                    dets.Add(disDataH);
                    dets.Add(disDataS);
                    dets.Add(disDataV);
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
                        int modelsize = 0;
                        if (User == "拉片")
                        {
                            modelsize = 1024;
                        }
                        else
                        {
                            modelsize = 512;
                        }
                        WH_Meta_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Meta_Model_Path, engineType,
    CurrentDevice, pull_num, param.MetaPullScore, Nms, modelsize);
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
            // 如果任何轮廓为空，返回一个很大的不相似值
            if (contours1 == null || contours2 == null)
                return 1001;
            if (contours1.Length != 0 && contours2.Length == 0)
            {
                return 1002;
            }
            if (contours1.Length == 0 && contours2.Length != 0)
            {
                return 1003;
            }
            if (contours1.Length == 0 || contours2.Length == 0)
            {
                return 0;
            }

            // 1) 保留原始 Hu 矩度量作为参考
            //double huValue;
            //using (var contour1Mat = new Mat(contours1.Length, 1, MatType.CV_32SC2))
            //using (var contour2Mat = new Mat(contours2.Length, 1, MatType.CV_32SC2))
            //{
            //    for (int i = 0; i < contours1.Length; i++)
            //        contour1Mat.Set(i, 0, new Point(contours1[i].X, contours1[i].Y));
            //    for (int i = 0; i < contours2.Length; i++)
            //        contour2Mat.Set(i, 0, new Point(contours2[i].X, contours2[i].Y));

            //    huValue = Cv2.MatchShapes(contour1Mat, contour2Mat, mode);
            //}

            // 2) 计算归一化的 Hausdorff 距离（0..1），对局部偏差更敏感
            double haus = ComputeHausdorffDistance(contours1, contours2);

            // 3) 融合两个度量（权重可根据需要调整），返回越小表示越相似
            double fused = haus;
            return fused;
            //return huValue;
        }

        // 计算两组轮廓点的对称 Hausdorff 距离，并按图像对角线归一化（使结果大致在 0..1 范围）
        private double ComputeHausdorffDistance(Point[] a, Point[] b)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0)
                return double.MaxValue;

            double MaxMinDistAB = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < b.Length; j++)
                {
                    double dx = a[i].X - b[j].X;
                    double dy = a[i].Y - b[j].Y;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    if (d < minDist) minDist = d;
                }
                if (minDist > MaxMinDistAB) MaxMinDistAB = minDist;
            }

            double MaxMinDistBA = 0.0;
            for (int i = 0; i < b.Length; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < a.Length; j++)
                {
                    double dx = b[i].X - a[j].X;
                    double dy = b[i].Y - a[j].Y;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    if (d < minDist) minDist = d;
                }
                if (minDist > MaxMinDistBA) MaxMinDistBA = minDist;
            }

            double hausdorff = Math.Max(MaxMinDistAB, MaxMinDistBA);

            // 归一化：使用两个轮廓合并包围盒的对角线长度
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in a.Concat(b))
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            double diag = Math.Sqrt((maxX - minX) * (maxX - minX) + (maxY - minY) * (maxY - minY));
            if (diag <= 0.0) diag = 1.0;

            return hausdorff / diag; // 归一化后通常在 0..1 范围
        }

        private void GetAreaAndArcLength(Point[] contours, out float conArea, out float conArcLength)
        {
            if (contours == null || contours.Length == 0)
            {
                conArea = 0;
                conArcLength = 0;
                return;
            }
            // 计算轮廓面积
            conArea = (float)Cv2.ContourArea(contours);
            // 计算轮廓周长（第二个参数true表示轮廓是否闭合）
            conArcLength = (float)Cv2.ArcLength(contours, true);
        }


        private void GetContoursAndHSV(Cell cell,CParam param, HObject ho_Image, HObject ho_Rectangle, double hv_RowRef, double hv_ColumnRef,
                             HTuple ModelID_Logo, HTuple ModelID_pull,
                             out Point[] pullPoints, out Point[] holdPoints,
                             out float Hvalue, out float Svalue, out float Vvalue, out float findScore,out float pullArea)
        {



            // Local iconic variables 
            findScore = 0; pullArea = 0;
            pullPoints = null; holdPoints = null;
            Hvalue = 0; Svalue = 0; Vvalue = 0;
            HObject ho_GrayImage;
            HObject ho_Contours_pull, ho_HoleRegion, ho_pullRegion;
            HObject ho_Contours_hole, ho_RegionAffineTrans = null;
            HObject ho_ImageReduced = null;
            HObject ho_pullRegionaff = null;

            // Local control variables 

            HTuple hv_Rows_pull = new HTuple(), hv_Cols_pull = new HTuple();
            HTuple hv_Area2 = new HTuple(), hv_Row6 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_PointOrder1 = new HTuple();
            HTuple hv_Length1 = new HTuple(), hv_MeanH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_MeanV = new HTuple();
            HTuple hv_Rows_hole = new HTuple(), hv_Cols_hole = new HTuple();
            HTuple hv_row1_re = new HTuple(), hv_col1_re = new HTuple();
            HTuple hv_row2_re = new HTuple(), hv_col2_re = new HTuple();

            HTuple hv_Score = new HTuple(), hv_ModelID_logo = new HTuple();
            HTuple hv_IsHandle = new HTuple(), hv_Score1 = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Angle = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_pullRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_pull);
            HOperatorSet.GenEmptyObj(out ho_HoleRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_hole);
            HOperatorSet.GenEmptyObj(out ho_RegionAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_pullRegionaff);

            try
            {
               // HOperatorSet.ReadRegion(out ho_Rectangle, "D://三合一软件//SystemConfig//Region_Back.hobj");
               HOperatorSet.WriteRegion(ho_Rectangle, "D://三合一软件//SystemConfig//Region_Back11111.hobj");
                GetPullsRegion(ho_Image, out _, out _, out  ho_pullRegion);
                HOperatorSet.AreaCenter(ho_pullRegion, out HTuple hv_pullArea, out _, out _);
                pullArea = (float)hv_pullArea.D;
                if (ModelID_pull != null)
                {
                    //*转正图片
                    ho_RegionAffineTrans.Dispose(); hv_Score.Dispose();
                    GetAffImage(ho_Image, ho_Rectangle, out ho_RegionAffineTrans, ModelID_pull, hv_RowRef,
                        hv_ColumnRef, out hv_Score);
                    if ((int)(new HTuple((new HTuple(hv_Score.TupleLength())).TupleGreater(0))) != 0)
                    {

                        //获取拉片区域
                        ho_GrayImage.Dispose(); ho_pullRegionaff.Dispose(); //ho_pullRegion.Dispose();
                        // GetPullsRegion(ho_ImageAffineTrans, out ho_GrayImage, out ho_Rectangle, out ho_pullRegion);
                        GetAffinePullRegion(ho_Image, ho_RegionAffineTrans, out ho_GrayImage, out ho_pullRegionaff);

                        //获取拉片区域轮廓
                        ho_Contours_pull.Dispose();
                        HOperatorSet.GenContourRegionXld(ho_pullRegionaff, out ho_Contours_pull, "border");
                        hv_Rows_pull.Dispose(); hv_Cols_pull.Dispose();
                        HOperatorSet.GetContourXld(ho_Contours_pull, out hv_Rows_pull, out hv_Cols_pull);
                        hv_Area2.Dispose(); hv_Row6.Dispose(); hv_Column4.Dispose(); hv_PointOrder1.Dispose();
                        HOperatorSet.AreaCenterXld(ho_Contours_pull, out hv_Area2, out hv_Row6, out hv_Column4,
                            out hv_PointOrder1);
                        hv_Length1.Dispose();
                        HOperatorSet.LengthXld(ho_Contours_pull, out hv_Length1);
                        //获取拉片HSV值


                        //获取拉片中的孔洞
                        ho_HoleRegion.Dispose(); hv_MeanH.Dispose(); hv_MeanS.Dispose(); hv_MeanV.Dispose();
                        GetHoleRegion(ho_Image, ho_GrayImage, ho_pullRegionaff, out ho_HoleRegion, out hv_MeanH,
                            out hv_MeanS, out hv_MeanV);
                        ho_Contours_hole.Dispose();
                        HOperatorSet.GenContourRegionXld(ho_HoleRegion, out ho_Contours_hole, "border");
                        hv_Rows_hole.Dispose(); hv_Cols_hole.Dispose();
                        HOperatorSet.GetContourXld(ho_Contours_hole, out hv_Rows_hole, out hv_Cols_hole);

                        //hv_IsHandle.Dispose();
                        //HOperatorSet.TupleIsHandle(hv_ModelID_logo, out hv_IsHandle);
                        hv_Score1.Dispose();
                        hv_Score1 = 0;
                        if (cell.ZipperLogoType== "有LOGO" && ModelID_Logo != null && ModelID_Logo.Length > 0) //如果有LOGO模板 则查找LOGO 没有模版 则看当前的是否有LOGO，有则为混入了logo
                        {
                            ho_ImageReduced.Dispose();
                            HOperatorSet.ReduceDomain(ho_GrayImage, ho_pullRegionaff, out ho_ImageReduced
                                );
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Row.Dispose(); hv_Column.Dispose(); hv_Angle.Dispose(); hv_Score1.Dispose();
                                HOperatorSet.FindShapeModel(ho_ImageReduced, ModelID_Logo, (new HTuple(-10)).TupleDeg()
                                    , (new HTuple(20)).TupleDeg(), param.LogoPullScore, 1, 0.5, "least_squares", 0, 0.9,
                                    out hv_Row, out hv_Column, out hv_Angle, out hv_Score1);
                            }
                        }
                        else
                        {
                            hv_row1_re.Dispose(); hv_col1_re.Dispose(); hv_row2_re.Dispose(); hv_col2_re.Dispose(); hv_ModelID_logo.Dispose();
                            GetLogoModel(ho_pullRegionaff, ho_HoleRegion, ho_GrayImage, false, out hv_row1_re,
                                out hv_col1_re, out hv_row2_re, out hv_col2_re, out hv_ModelID_logo);

                            if ((int)((new HTuple((new HTuple((new HTuple(hv_row1_re.TupleGreater(0))).TupleAnd(
                                new HTuple(hv_row2_re.TupleGreater(0))))).TupleAnd(new HTuple(hv_col1_re.TupleGreater(
                                0))))).TupleAnd(new HTuple(hv_col2_re.TupleGreater(0)))) != 0)
                            {
                                //找到了LOGO 判断基准是否有Logo 再做处理
                                hv_Score1 = 0; //如果基准图没有logo,但当前找到了LOGO 则为混入了LOGO 赋值0
                            }
                            else
                            {
                                hv_Score1 = 1; //如果基准图没有logo,当前也没找到了LOGO 则为混入了LOGO 赋值1
                            }
                        }
                        if ((int)(new HTuple((new HTuple(hv_Score1.TupleLength())).TupleGreater(0))) != 0)
                        {
                            findScore = (float)hv_Score1.D;
                        }


                        //得到颜色值
                        Hvalue = (float)hv_MeanH.D;
                        Svalue = (float)hv_MeanS.D;
                        Vvalue = (float)hv_MeanV.D;

                        //得到拉片轮廓点和孔轮廓点
                        pullPoints = new Point[hv_Rows_pull.Length];
                        for (int i = 0; i < hv_Rows_pull.Length; i++)
                        {
                            pullPoints[i] = new Point((int)hv_Cols_pull[i].D, (int)hv_Rows_pull[i].D);
                        }
                        holdPoints = new Point[hv_Rows_hole.Length];
                        for (int i = 0; i < hv_Rows_hole.Length; i++)
                        {
                            holdPoints[i] = new Point((int)hv_Cols_hole[i].D, (int)hv_Rows_hole[i].D);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ho_Image.Dispose();
                ho_GrayImage.Dispose();
                ho_pullRegionaff.Dispose();
                ho_Contours_pull.Dispose();
                ho_HoleRegion.Dispose();
                ho_Contours_hole.Dispose();
                ho_ImageReduced.Dispose();
                ho_pullRegion.Dispose();
                hv_Rows_pull.Dispose();
                hv_Cols_pull.Dispose();
                hv_Area2.Dispose();
                hv_Row6.Dispose();
                hv_Column4.Dispose();
                hv_PointOrder1.Dispose();
                hv_Length1.Dispose();
                hv_MeanH.Dispose();
                hv_MeanS.Dispose();
                hv_MeanV.Dispose();
                hv_Rows_hole.Dispose();
                hv_Cols_hole.Dispose();
                hv_row1_re.Dispose();
                hv_col1_re.Dispose();
                hv_row2_re.Dispose();
                hv_col2_re.Dispose();
                hv_ModelID_logo.Dispose();
                hv_Score.Dispose();
                hv_IsHandle.Dispose();
                hv_Score1.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Angle.Dispose();
            }
        }

        private void GetContoursAndHSVAndModel(HObject ho_Image, out HObject ho_Rectangle, out Point[] pullPoints, out Point[] holdPoints,
                                     out float Hvalue, out float Svalue, out float Vvalue,
                                     out HTuple ModelID_Logo, out HTuple recRow1_logo, out HTuple recCol1_logo,
                                     out HTuple recRow2_logo, out HTuple recCol2_logo,
                                     out HTuple ModelID_pull, out HTuple hv_RowRef, out HTuple hv_ColumnRef, out HTuple pullArea)
        {

            // Local iconic variables 

            // HObject ho_Image,
            HObject ho_GrayImage;
            HObject ho_pullRegion, ho_Contours_pull, ho_HoleRegion;
            HObject ho_Contours_hole, ho_Image1 = null, ho_ImageAffineTrans = null;
            HObject ho_ImageReduced = null;

            // Local control variables 

            HTuple hv_Rows_pull = new HTuple(), hv_Cols_pull = new HTuple();
            HTuple hv_Area2 = new HTuple(), hv_Row6 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_PointOrder1 = new HTuple();
            HTuple hv_Length1 = new HTuple(), hv_MeanH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_MeanV = new HTuple();
            HTuple hv_Rows_hole = new HTuple(), hv_Cols_hole = new HTuple();
            HTuple hv_row1_re = new HTuple(), hv_col1_re = new HTuple();
            HTuple hv_row2_re = new HTuple(), hv_col2_re = new HTuple();
            HTuple hv_ModelID_logo = new HTuple();
            hv_RowRef = new HTuple(); hv_ColumnRef = new HTuple();
            HTuple hv_ModelID_pull = new HTuple();
            HTuple hv_Score = new HTuple(), hv_BcanCreate = new HTuple();
            HTuple hv_IsHandle = new HTuple(), hv_Score1 = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Angle = new HTuple();
            // Initialize local and output iconic variables 
            // HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_pullRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_pull);
            HOperatorSet.GenEmptyObj(out ho_HoleRegion);
            HOperatorSet.GenEmptyObj(out ho_Contours_hole);
            HOperatorSet.GenEmptyObj(out ho_Image1);
            HOperatorSet.GenEmptyObj(out ho_ImageAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);

            ModelID_Logo = new HTuple();
            recRow1_logo = new HTuple();
            recCol1_logo = new HTuple();
            recRow2_logo = new HTuple();
            recCol2_logo = new HTuple();
            ModelID_pull = new HTuple();
            pullArea = new HTuple();
            try
            {
                //获取拉片区域
                ho_GrayImage.Dispose(); ho_Rectangle.Dispose(); ho_pullRegion.Dispose(); pullArea.Dispose();
                GetPullsRegion(ho_Image, out ho_GrayImage, out ho_Rectangle, out ho_pullRegion);
                HOperatorSet.AreaCenter(ho_pullRegion, out pullArea, out _, out _);
                //获取拉片区域轮廓
                ho_Contours_pull.Dispose();
                HOperatorSet.GenContourRegionXld(ho_pullRegion, out ho_Contours_pull, "border");
                hv_Rows_pull.Dispose(); hv_Cols_pull.Dispose();
                HOperatorSet.GetContourXld(ho_Contours_pull, out hv_Rows_pull, out hv_Cols_pull);
                hv_Area2.Dispose(); hv_Row6.Dispose(); hv_Column4.Dispose(); hv_PointOrder1.Dispose();
                HOperatorSet.AreaCenterXld(ho_Contours_pull, out hv_Area2, out hv_Row6, out hv_Column4,
                    out hv_PointOrder1);
                hv_Length1.Dispose();
                HOperatorSet.LengthXld(ho_Contours_pull, out hv_Length1);
                //获取拉片HSV值


                //获取拉片中的孔洞
                ho_HoleRegion.Dispose(); hv_MeanH.Dispose(); hv_MeanS.Dispose(); hv_MeanV.Dispose();
                GetHoleRegion(ho_Image, ho_GrayImage, ho_pullRegion, out ho_HoleRegion, out hv_MeanH,
                    out hv_MeanS, out hv_MeanV);
                ho_Contours_hole.Dispose();
                HOperatorSet.GenContourRegionXld(ho_HoleRegion, out ho_Contours_hole, "border");
                hv_Rows_hole.Dispose(); hv_Cols_hole.Dispose();
                HOperatorSet.GetContourXld(ho_Contours_hole, out hv_Rows_hole, out hv_Cols_hole);

                //获取LOGO模版
                hv_row1_re.Dispose(); hv_col1_re.Dispose(); hv_row2_re.Dispose(); hv_col2_re.Dispose(); hv_ModelID_logo.Dispose();
                GetLogoModel(ho_pullRegion, ho_HoleRegion, ho_GrayImage, true, out hv_row1_re, out hv_col1_re,
                    out hv_row2_re, out hv_col2_re, out hv_ModelID_logo);

                //*获取拉片模版
                hv_RowRef.Dispose(); hv_ColumnRef.Dispose(); hv_ModelID_pull.Dispose();
                GetPullSharpModel(ho_pullRegion, ho_GrayImage, out hv_RowRef, out hv_ColumnRef,
                    out hv_ModelID_pull);

                //传出Logo 模版和参数
                ModelID_Logo.Dispose();
                recRow1_logo.Dispose();
                recCol1_logo.Dispose();
                recRow2_logo.Dispose();
                recCol2_logo.Dispose();
                hv_IsHandle.Dispose();
                HOperatorSet.TupleIsHandle(hv_ModelID_logo, out hv_IsHandle);
                if ((int)(hv_IsHandle) != 0)
                {
                    ModelID_Logo = hv_ModelID_logo;
                    recRow1_logo = hv_row1_re;
                    recCol1_logo = hv_col1_re;
                    recRow2_logo = hv_row2_re;
                    recCol2_logo = hv_col2_re;
                }

                //传出拉片模版
                //得到拉片模版
                ModelID_pull.Dispose();
                ModelID_pull = hv_ModelID_pull;
                //得到颜色值
                Hvalue = (float)hv_MeanH.D;
                Svalue = (float)hv_MeanS.D;
                Vvalue = (float)hv_MeanV.D;

                //得到拉片轮廓点和孔轮廓点
                pullPoints = new Point[hv_Rows_pull.Length];
                for (int i = 0; i < hv_Rows_pull.Length; i++)
                {
                    pullPoints[i] = new Point((int)hv_Cols_pull[i].D, (int)hv_Rows_pull[i].D);
                }
                holdPoints = new Point[hv_Rows_hole.Length];
                for (int i = 0; i < hv_Rows_hole.Length; i++)
                {
                    holdPoints[i] = new Point((int)hv_Cols_hole[i].D, (int)hv_Rows_hole[i].D);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // ho_Image.Dispose();
                ho_GrayImage.Dispose();
                // ho_Rectangle.Dispose();
                ho_pullRegion.Dispose();
                ho_Contours_pull.Dispose();
                ho_HoleRegion.Dispose();
                ho_Contours_hole.Dispose();
                ho_Image1.Dispose();
                ho_ImageAffineTrans.Dispose();
                ho_ImageReduced.Dispose();

                hv_Rows_pull.Dispose();
                hv_Cols_pull.Dispose();
                hv_Area2.Dispose();
                hv_Row6.Dispose();
                hv_Column4.Dispose();
                hv_PointOrder1.Dispose();
                hv_Length1.Dispose();
                hv_MeanH.Dispose();
                hv_MeanS.Dispose();
                hv_MeanV.Dispose();
                hv_Rows_hole.Dispose();
                hv_Cols_hole.Dispose();
                hv_row1_re.Dispose();
                hv_col1_re.Dispose();
                hv_row2_re.Dispose();
                hv_col2_re.Dispose();
                // hv_ModelID_logo.Dispose();
                //hv_RowRef.Dispose();
                //hv_ColumnRef.Dispose();
                // hv_ModelID_pull.Dispose();
                hv_Score.Dispose();
                hv_BcanCreate.Dispose();
                hv_IsHandle.Dispose();
                hv_Score1.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Angle.Dispose();
            }
        }


        private void GetPullerHSV(Cell cell, out float Hvalue, out float Svalue, out float Vvalue)
        {
            Hvalue = 0; Svalue = 0; Vvalue = 0;

            HObject ho_Image = null, ho_GrayImage = null, ho_Region = null;
            HObject ho_ConnectedRegions = null, ho_RegionOpening = null;
            HObject ho_ConnectedRegions1 = null, ho_SelectedRegions = null;
            HObject ho_RegionFillUp = null, ho_RegionDifference = null;
            HObject ho_ConnectedRegions2 = null, ho_SelectedRegions1 = null;
            HObject ho_RegionErosion = null, ho_ImageR = null, ho_ImageG = null;
            HObject ho_ImageB = null, ho_ImageResultH = null, ho_ImageResultS = null;
            HObject ho_ImageResultV = null;

            // Local control variables 

            HTuple hv_MeanH = new HTuple(), hv_DevH = new HTuple();
            HTuple hv_MeanS = new HTuple(), hv_DevS = new HTuple();
            HTuple hv_MeanV = new HTuple(), hv_DevV = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageResultH);
            HOperatorSet.GenEmptyObj(out ho_ImageResultS);
            HOperatorSet.GenEmptyObj(out ho_ImageResultV);
            try
            {
                if (cell.ImageFile == "")
                {
                    HOperatorSet.GenImageInterleaved(out ho_Image,
                        cell.Image.ImageData,
                        "rgb",
                        cell.Image.ImageWidth,
                        cell.Image.ImageHeight,
                        -1,
                        "byte",
                        0,
                        0,
                        0,
                        0,
                        -1,
                        0
                        );
                }
                else
                {
                    HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                }

                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_GrayImage, out ho_Region, 0, 15);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_ConnectedRegions, out ho_RegionOpening, 1.5);
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions1);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions, "max_area",
                    70);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions, out ho_RegionFillUp);
                ho_RegionDifference.Dispose();
                HOperatorSet.Difference(ho_RegionFillUp, ho_SelectedRegions, out ho_RegionDifference
                    );
                ho_ConnectedRegions2.Dispose();
                HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions2);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions2, out ho_SelectedRegions1,
                    "max_area", 70);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_SelectedRegions1, out ho_RegionErosion, 5.5);

                ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_ImageR, out ho_ImageG, out ho_ImageB
                    );
                ho_ImageResultH.Dispose(); ho_ImageResultS.Dispose(); ho_ImageResultV.Dispose();
                HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageResultH,
                    out ho_ImageResultS, out ho_ImageResultV, "hsv");
                hv_MeanH.Dispose(); hv_DevH.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultH, out hv_MeanH, out hv_DevH);
                hv_MeanS.Dispose(); hv_DevS.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultS, out hv_MeanS, out hv_DevS);
                hv_MeanV.Dispose(); hv_DevV.Dispose();
                HOperatorSet.Intensity(ho_RegionErosion, ho_ImageResultV, out hv_MeanV, out hv_DevV);

                Hvalue = (float)hv_MeanH.D;
                Svalue = (float)hv_MeanS.D;
                Vvalue = (float)hv_MeanV.D;
            }
            catch (Exception)
            {
                Hvalue = 255;
                Svalue = 255;
                Vvalue = 255;
                throw;
            }
            finally
            {
                ho_Image.Dispose();
                ho_GrayImage.Dispose();
                ho_Region.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionFillUp.Dispose();
                ho_RegionDifference.Dispose();
                ho_ConnectedRegions2.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();
                ho_ImageResultH.Dispose();
                ho_ImageResultS.Dispose();
                ho_ImageResultV.Dispose();
                hv_MeanH.Dispose();
                hv_DevH.Dispose();
                hv_MeanS.Dispose();
                hv_DevS.Dispose();
                hv_MeanV.Dispose();
                hv_DevV.Dispose();
            }

        }

        public void GetAffImage(HObject ho_Image1, HObject ho_Rectangle, out HObject ho_RegionAffineTrans,
            HTuple hv_ModelID, HTuple hv_RowRef, HTuple hv_ColumnRef, out HTuple hv_Score)
        {




            // Local iconic variables 

            HObject ho_GrayImage, ho_ImageAffineTrans;

            // Local control variables 

            HTuple hv_Row3 = new HTuple(), hv_Column3 = new HTuple();
            HTuple hv_Angle = new HTuple(), hv_HomMat2D1 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RegionAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_ImageAffineTrans);
            hv_Score = new HTuple();
            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_Image1, out ho_GrayImage);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Row3.Dispose(); hv_Column3.Dispose(); hv_Angle.Dispose(); hv_Score.Dispose();
                HOperatorSet.FindShapeModel(ho_GrayImage, hv_ModelID, (new HTuple(-10)).TupleDeg()
                    , (new HTuple(20)).TupleDeg(), 0.65, 1, 0.5, "least_squares", 0, 0.9, out hv_Row3,
                    out hv_Column3, out hv_Angle, out hv_Score);
            }
            ho_ImageAffineTrans.Dispose();
            HOperatorSet.GenEmptyObj(out ho_ImageAffineTrans);
            if ((int)(new HTuple((new HTuple(hv_Score.TupleLength())).TupleGreater(0))) != 0)
            {
                //vector_angle_to_rigid (Row3, Column3, Angle, RowRef, ColumnRef, 0, HomMat2D)
                hv_HomMat2D1.Dispose();
                HOperatorSet.VectorAngleToRigid(hv_RowRef, hv_ColumnRef, 0, hv_Row3, hv_Column3,
                    hv_Angle, out hv_HomMat2D1);
                //affine_trans_image (Image1, ImageAffineTrans, HomMat2D, 'constant', 'false')
                ho_RegionAffineTrans.Dispose();
                HOperatorSet.AffineTransRegion(ho_Rectangle, out ho_RegionAffineTrans, hv_HomMat2D1,
                    "nearest_neighbor");
            }
            ho_GrayImage.Dispose();
            ho_ImageAffineTrans.Dispose();

            hv_Row3.Dispose();
            hv_Column3.Dispose();
            hv_Angle.Dispose();
            hv_HomMat2D1.Dispose();

            return;
        }
        public void GetAffinePullRegion(HObject ho_Image, HObject ho_RegionAffineTrans,
    out HObject ho_GrayImage, out HObject ho_AffpullRegion)
        {



            // Local iconic variables 

            HObject ho_ImageReduced, ho_Region1, ho_RegionFillUp;
            HObject ho_ConnectedRegions1, ho_SelectedRegions1, ho_RegionDifference;
            HObject ho_ConnectedRegions2;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_AffpullRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);

            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);

            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_RegionAffineTrans, out ho_ImageReduced
                );

            //获取拉片
            ho_Region1.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, 220, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region1, out ho_RegionFillUp);
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions1);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions1, "max_area",
                70);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionAffineTrans, ho_SelectedRegions1, out ho_RegionDifference
                );
            ho_ConnectedRegions2.Dispose();
            HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions2);
            ho_AffpullRegion.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions2, out ho_AffpullRegion, "max_area",
                70);
            ho_ImageReduced.Dispose();
            ho_Region1.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDifference.Dispose();
            ho_ConnectedRegions2.Dispose();


            return;
        }

        private void GetHoleRegion(HObject ho_Image, HObject ho_GrayImage, HObject ho_pullRegion,
            out HObject ho_HoleRegion, out HTuple hv_MeanH, out HTuple hv_MeanS, out HTuple hv_MeanV)
        {



            // Local iconic variables 

            HObject ho_ImageReduced, ho_Region, ho_RegionFillUp;
            HObject ho_RegionClosing, ho_ConnectedRegions, ho_SelectedRegions2;
            HObject ho_RegionFillUp3, ho_RegionDifference, ho_RegionOpening;
            HObject ho_ImageR, ho_ImageG, ho_ImageB, ho_ImageH, ho_ImageS;
            HObject ho_ImageV;

            // Local control variables 

            HTuple hv_DevH = new HTuple(), hv_DevS = new HTuple();
            HTuple hv_DevV = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_HoleRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp3);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageH);
            HOperatorSet.GenEmptyObj(out ho_ImageS);
            HOperatorSet.GenEmptyObj(out ho_ImageV);
            hv_MeanH = new HTuple();
            hv_MeanS = new HTuple();
            hv_MeanV = new HTuple();
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_pullRegion, out ho_ImageReduced);
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, 230, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region, out ho_RegionFillUp);
            ho_RegionClosing.Dispose();
            HOperatorSet.ClosingCircle(ho_RegionFillUp, out ho_RegionClosing, 5.5);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
            ho_SelectedRegions2.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions2, (new HTuple("area")).TupleConcat(
                "convexity"), "and", (new HTuple(6500)).TupleConcat(0.9), (new HTuple(9999999999)).TupleConcat(
                1));
            ho_RegionFillUp3.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions2, out ho_RegionFillUp3);
            ho_HoleRegion.Dispose();
            HOperatorSet.SelectShapeStd(ho_RegionFillUp3, out ho_HoleRegion, "max_area",
                70);
            //*获取拉片HSV
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_pullRegion, ho_RegionClosing, out ho_RegionDifference
                );
            ho_RegionOpening.Dispose();
            HOperatorSet.OpeningCircle(ho_RegionDifference, out ho_RegionOpening, 3.5);
            ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
            HOperatorSet.Decompose3(ho_Image, out ho_ImageR, out ho_ImageG, out ho_ImageB
                );
            ho_ImageH.Dispose(); ho_ImageS.Dispose(); ho_ImageV.Dispose();
            HOperatorSet.TransFromRgb(ho_ImageR, ho_ImageG, ho_ImageB, out ho_ImageH, out ho_ImageS,
                out ho_ImageV, "hsv");
            hv_MeanH.Dispose(); hv_DevH.Dispose();
            HOperatorSet.Intensity(ho_RegionOpening, ho_ImageH, out hv_MeanH, out hv_DevH);
            hv_MeanS.Dispose(); hv_DevS.Dispose();
            HOperatorSet.Intensity(ho_RegionOpening, ho_ImageS, out hv_MeanS, out hv_DevS);
            hv_MeanV.Dispose(); hv_DevV.Dispose();
            HOperatorSet.Intensity(ho_RegionOpening, ho_ImageV, out hv_MeanV, out hv_DevV);
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionClosing.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions2.Dispose();
            ho_RegionFillUp3.Dispose();
            ho_RegionDifference.Dispose();
            ho_RegionOpening.Dispose();
            ho_ImageR.Dispose();
            ho_ImageG.Dispose();
            ho_ImageB.Dispose();
            ho_ImageH.Dispose();
            ho_ImageS.Dispose();
            ho_ImageV.Dispose();

            hv_DevH.Dispose();
            hv_DevS.Dispose();
            hv_DevV.Dispose();

            return;
        }

        private void GetLogoModel(HObject ho_pullRegion, HObject ho_HoleRegion, HObject ho_GrayImage,
          bool hv_BcanCreate, out HTuple hv_row1_re, out HTuple hv_col1_re, out HTuple hv_row2_re,
          out HTuple hv_col2_re, out HTuple hv_ModelID_logo)
        {




            // Local iconic variables 

            HObject ho_RegionErosion, ho_ImageReduced;
            HObject ho_ImageEmphasize, ho_Region, ho_RegionFillUp, ho_RegionDifference;
            HObject ho_RegionFillUp1, ho_ConnectedRegions, ho_RegionOpening;
            HObject ho_ConnectedRegions1, ho_SelectedRegions, ho_SelectedRegions1;
            HObject ho_RegionDifference2, ho_RegionUnion, ho_Rectangle1;
            HObject ho_ImageReduced1 = null;

            // Local control variables 

            HTuple hv_Area = new HTuple(), hv_Row_hole = new HTuple();
            HTuple hv_Column_hole = new HTuple(), hv_Area3 = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row11 = new HTuple(), hv_Column11 = new HTuple();
            HTuple hv_Row21 = new HTuple(), hv_Column21 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            hv_row1_re = new HTuple();
            hv_col1_re = new HTuple();
            hv_row2_re = new HTuple();
            hv_col2_re = new HTuple();
            hv_ModelID_logo = new HTuple();
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_pullRegion, out ho_RegionErosion, 7.5);
            hv_Area.Dispose(); hv_Row_hole.Dispose(); hv_Column_hole.Dispose();
            HOperatorSet.AreaCenter(ho_HoleRegion, out hv_Area, out hv_Row_hole, out hv_Column_hole);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_RegionErosion, out ho_ImageReduced
                );
            ho_ImageEmphasize.Dispose();
            HOperatorSet.Emphasize(ho_ImageReduced, out ho_ImageEmphasize, 17, 17, 1);
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageEmphasize, out ho_Region, 35, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region, out ho_RegionFillUp);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionFillUp, ho_Region, out ho_RegionDifference);
            ho_RegionFillUp1.Dispose();
            HOperatorSet.FillUp(ho_RegionDifference, out ho_RegionFillUp1);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp1, out ho_ConnectedRegions);
            ho_RegionOpening.Dispose();
            HOperatorSet.OpeningCircle(ho_ConnectedRegions, out ho_RegionOpening, 1.5);
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions1);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions, "area",
                "and", 600, 999999999999);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            if ((int)(new HTuple((new HTuple(hv_Row_hole.TupleLength())).TupleGreater(0))) != 0)
            {
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_SelectedRegions, out ho_SelectedRegions1, (new HTuple("row")).TupleConcat(
                        "column"), "and", ((hv_Row_hole - 20)).TupleConcat(hv_Column_hole - 20), ((hv_Row_hole + 20)).TupleConcat(
                        hv_Column_hole + 20));
                }
            }
            ho_RegionDifference2.Dispose();
            HOperatorSet.Difference(ho_SelectedRegions, ho_SelectedRegions1, out ho_RegionDifference2
                );
            hv_Area3.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
            HOperatorSet.AreaCenter(ho_RegionDifference2, out hv_Area3, out hv_Row1, out hv_Column1);
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionDifference2, out ho_RegionUnion);
            hv_Row11.Dispose(); hv_Column11.Dispose(); hv_Row21.Dispose(); hv_Column21.Dispose();
            HOperatorSet.SmallestRectangle1(ho_RegionUnion, out hv_Row11, out hv_Column11,
                out hv_Row21, out hv_Column21);

            ho_Rectangle1.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            hv_row1_re.Dispose();
            hv_row1_re = 0;
            hv_row2_re.Dispose();
            hv_row2_re = 0;
            hv_col1_re.Dispose();
            hv_col1_re = 0;
            hv_col2_re.Dispose();
            hv_col2_re = 0;
            hv_ModelID_logo.Dispose();
            hv_ModelID_logo = 0;
            if ((int)((new HTuple((new HTuple((new HTuple(hv_Row11.TupleGreater(0))).TupleAnd(
                new HTuple(hv_Column11.TupleGreater(0))))).TupleAnd(new HTuple(hv_Row21.TupleGreater(
                0))))).TupleAnd(new HTuple(hv_Column21.TupleGreater(0)))) != 0)
            {
                hv_row1_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_row1_re = hv_Row11;
                }
                hv_row2_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_row2_re = hv_Row21;
                }
                hv_col1_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_col1_re = hv_Column11;
                }
                hv_col2_re.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_col2_re = hv_Column21;
                }
                ho_Rectangle1.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle1, hv_row1_re, hv_col1_re, hv_row2_re,
                    hv_col2_re);
                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle1, out ho_ImageReduced1
                    );

                if (hv_BcanCreate)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ModelID_logo.Dispose();
                        HOperatorSet.CreateShapeModel(ho_ImageReduced1, "auto", (new HTuple(-10)).TupleDeg()
                            , (new HTuple(20)).TupleDeg(), "auto", "auto", "use_polarity", "auto",
                            "auto", out hv_ModelID_logo);
                    }
                }

            }
            ho_RegionErosion.Dispose();
            ho_ImageReduced.Dispose();
            ho_ImageEmphasize.Dispose();
            ho_Region.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionDifference.Dispose();
            ho_RegionFillUp1.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_RegionOpening.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDifference2.Dispose();
            ho_RegionUnion.Dispose();
            ho_Rectangle1.Dispose();
            ho_ImageReduced1.Dispose();

            hv_Area.Dispose();
            hv_Row_hole.Dispose();
            hv_Column_hole.Dispose();
            hv_Area3.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row11.Dispose();
            hv_Column11.Dispose();
            hv_Row21.Dispose();
            hv_Column21.Dispose();

            return;
        }

        private void GetPullSharpModel(HObject ho_pullRegion, HObject ho_GrayImage, out HTuple hv_RowRef,
            out HTuple hv_ColumnRef, out HTuple hv_ModelID)
        {



            // Local iconic variables 

            HObject ho_Rectangle1, ho_ImageReduced1;

            // Local control variables 

            HTuple hv_Row11 = new HTuple(), hv_Column11 = new HTuple();
            HTuple hv_Row21 = new HTuple(), hv_Column21 = new HTuple();
            HTuple hv_Area = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            hv_RowRef = new HTuple();
            hv_ColumnRef = new HTuple();
            hv_ModelID = new HTuple();
            hv_Row11.Dispose(); hv_Column11.Dispose(); hv_Row21.Dispose(); hv_Column21.Dispose();
            HOperatorSet.SmallestRectangle1(ho_pullRegion, out hv_Row11, out hv_Column11,
                out hv_Row21, out hv_Column21);
            ho_Rectangle1.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle1, hv_Row11, hv_Column11, hv_Row21,
                hv_Column21);
            hv_Area.Dispose(); hv_RowRef.Dispose(); hv_ColumnRef.Dispose();
            HOperatorSet.AreaCenter(ho_Rectangle1, out hv_Area, out hv_RowRef, out hv_ColumnRef);
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle1, out ho_ImageReduced1);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_ModelID.Dispose();
                HOperatorSet.CreateShapeModel(ho_ImageReduced1, "auto", (new HTuple(-10)).TupleDeg()
                    , (new HTuple(20)).TupleDeg(), "auto", "auto", "use_polarity", "auto", "auto",
                    out hv_ModelID);
            }
            ho_Rectangle1.Dispose();
            ho_ImageReduced1.Dispose();

            hv_Row11.Dispose();
            hv_Column11.Dispose();
            hv_Row21.Dispose();
            hv_Column21.Dispose();
            hv_Area.Dispose();

            return;
        }

        private void GetPullsRegion(HObject ho_Image, out HObject ho_GrayImage, out HObject ho_Rectangle,
              out HObject ho_pullRegion)
        {



            // Local iconic variables 

            HObject ho_Region, ho_ConnectedRegions, ho_SelectedRegions;
            HObject ho_ImageReduced, ho_Region1, ho_RegionFillUp, ho_ConnectedRegions1;
            HObject ho_SelectedRegions1, ho_RegionDifference, ho_ConnectedRegions2;

            // Local control variables 

            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_pullRegion);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_GrayImage, out ho_Region, 180, 255);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                70);
            hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
            HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out hv_Row1, out hv_Column1,
                out hv_Row2, out hv_Column2);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1 + 100, hv_Column1 + 100, hv_Row2 - 100,
                    hv_Column2 - 50);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle, out ho_ImageReduced);

            //获取拉片
            ho_Region1.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, 220, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Region1, out ho_RegionFillUp);
            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions1);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions1, out ho_SelectedRegions1, "max_area",
                70);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_Rectangle, ho_SelectedRegions1, out ho_RegionDifference
                );
            ho_ConnectedRegions2.Dispose();
            HOperatorSet.Connection(ho_RegionDifference, out ho_ConnectedRegions2);
            ho_pullRegion.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions2, out ho_pullRegion, "max_area",
                70);


            ho_Region.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region1.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDifference.Dispose();
            ho_ConnectedRegions2.Dispose();

            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();

            return;
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

        public CoordRestoreData(string labelstr, float value, Point[] contours, int showinview = 0)
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


            List<System.Windows.Point> Points = new List<System.Windows.Point>();
            for (int j = 0; j < contours?.Length; j++)
            {
                System.Windows.Point point = new System.Windows.Point() { X = contours[j].X, Y = contours[j].Y };
                Points.Add(point);
            }
            if (Points.Count > 0)
            {
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
