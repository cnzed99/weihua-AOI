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


namespace ZipperTestAlgorihm
{
    public class CZipperTestAlgorihmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// 检测对象1
        /// </summary>
        IVisionModel yolo_all_det1;
        /// <summary>
        /// 检测对象2
        /// </summary>
        IVisionModel yolo_all_det2;
        /// <summary>
        /// 检测对象3
        /// </summary>
        IVisionModel yolo_all_det3;
        /// <summary>
        /// 检测对象4
        /// </summary>
        IVisionModel yolo_all_det4;

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
        /// Logo检测对象
        /// </summary>
        IVisionModel yolo_Logo_pull_det;

        /// <summary>
        /// 拉头查找对象
        /// </summary>
        IVisionModel yolo_pull_Serach_det;
        //定义4组矩形来裁切图片
        Rect[] cropRec = new Rect[4];


        /// <summary>
        /// 大缺陷检测对象
        /// </summary>
        IVisionModel yolo_BigDet_det;
        /// <summary>
        /// 拉片分割模型
        /// </summary>
        IVisionModel yolo_PullShape_Seg;

        public CZipperTestAlgorihmParam(string user) : base()
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
        private string Common_Model_Path;
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
        private string pull_Logo_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 大缺陷模型路径
        /// </summary>
        private string Big_Model_Path;

        /// <summary>
        /// 2025.11.17 鲍赞宝
        /// 拉片分割模型路径
        /// </summary>
        private string pullSharp_Model_Path;


        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        private string name_Path;
        //常规缺陷名称
        protected string[] Common_names;
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
        protected string[] pull_Logo_names;

        //拉头拉片缺陷名称
        protected string[] bigDet_names;

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


            if (Common_names?.Length > 0)
            {
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
                for (int i = 0; i < Common_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(Common_names[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
                if (user == "正面")
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
                for (int i = 0; i < pullSharp_names.Length; i++)
                {
                    CDefectRecipe defectRecipe2 = new CDefectRecipe(pullSharp_names[i], Category.值);
                    pullRecipes.Add(defectRecipe2);
                }
                //颜色
                CDefectRecipe defectRecipe1_H = new CDefectRecipe("H", Category.值);
                CDefectRecipe defectRecipe1_S = new CDefectRecipe("S", Category.值);
                pullRecipes.Add(defectRecipe1_H);
                pullRecipes.Add(defectRecipe1_S);

                CDefectSpecies pullSpecies = new CDefectSpecies("拉头拉片", pullRecipes);

                List<CDefectRecipe> logoRecipes = new List<CDefectRecipe>();
                for (int i = 0; i < pull_Logo_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(pull_Logo_names[i], Category.区域);
                    logoRecipes.Add(defectRecipe);
                }
                CDefectSpecies logoSpecies = new CDefectSpecies("LOGO", logoRecipes);



                #endregion

                DefectSpecies.Add(defectSpecies);
                DefectSpecies.Add(bigSpecies);
                DefectSpecies.Add(pullSpecies);
                DefectSpecies.Add(logoSpecies);
            }
        }

        protected void ReadNames(string user)
        {

            string modelDirpath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\";

            string commonModelPath = modelDirpath + "CommonModel\\";
            string bigModelPath = modelDirpath + "BigDetModel\\";

            if (user == "正面")
            {
                commonModelPath = commonModelPath + "Front\\";
                bigModelPath = bigModelPath + "Front\\";
            }
            else
            {
                commonModelPath = commonModelPath + "Back\\";
                bigModelPath = bigModelPath + "Back\\";
            }
            //if (user == "反面")
            //{
            //    commonModelPath = commonModelPath + "Back\\";
            //    bigModelPath = bigModelPath + "Back\\";
            //}
            var commons = GetNames(commonModelPath);
            if (commons.Item1 != "")
            {
                Common_Model_Path = commons.Item1;
                Common_names = commons.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }
            var bigstrs = GetNames(bigModelPath);
            if (bigstrs.Item1 != "")
            {
                Big_Model_Path = bigstrs.Item1;
                bigDet_names = bigstrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }
            if (user == "正面")
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

            string pullLogoModel = modelDirpath + "Pull\\PullLogoModel\\";
            var logopulltrs = GetNames(pullLogoModel);
            if (logopulltrs.Item1 != "")
            {
                pull_Logo_Model_Path = logopulltrs.Item1;
                pull_Logo_names = logopulltrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }

            string pullSegModelpath = modelDirpath + "Pull\\PullSegModel\\";
            var pullSegtrs = GetNames(pullSegModelpath);
            if (pullSegtrs.Item1 != "")
            {
                pullSharp_Model_Path = pullSegtrs.Item1;
                pullSharp_names = pullSegtrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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
                //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\测试存图\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "转后.png", img);
               // bool runtype = false; //判断是只处理1张图像还是多张图像，true为1张
                //if (cell.PhotoTatolCount == 2)
                //{
                //    runtype = true;
                //}
                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (cell.PhotoIndex != 100) //除了拉头图片，其他先检大缺陷
                {
                    upmassCount = 0;
                    DetResult bigResult = ImageInferDet(yolo_BigDet_det, img);

                    if (bigResult != null && bigResult.datas.Count > 0) //如果有大缺陷直接退出
                    {
                        List<Point> massPoints = new List<Point>(); //上止的位置
                        for (int j = 0; j < bigResult.datas.Count; j++)
                        {
                            int labelindex = int.Parse(bigResult.datas[j].lable);
                            string labelname = bigDet_names[labelindex];
                            CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, 0, 0, labelname, bigResult.datas[j]);
                            if ((labelname.Contains("正面上止") || labelname.Contains("反面上止"))&& cell.PhotoIndex != cell.PhotoTatolCount - 1)
                                continue;
                            //if ((labelname.Contains("正面下止") || labelname.Contains("反面下止")) && cell.PhotoIndex != 1)
                            //    continue;
                            dets.Add(restoreData);
                            if (cell.PhotoIndex == 1 && labelname.Contains("正面下止")) //检测下止
                            {
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
                                cell.DownMassMatImg = cropDownMat;

                                // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面下止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropDownMat);
                                ObbResult downResult = ImageInferObb(yolo_DownStopMass_obb, cropDownMat);
                                if (downResult != null)
                                {
                                    if (downResult.datas.Count > 0)
                                    {
                                        List<int> luyaIndex = new List<int>();
                                        string downmassIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("正面下止")).ToString();
                                        List<ObbData> downmass = downResult.datas.FindAll(c => c.lable == downmassIndexstr).ToList(); //下止

                                        string lianciIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("链齿")).ToString();
                                        List<ObbData> lianciorg = downResult.datas.FindAll(c => c.lable == lianciIndexstr).ToList(); //链齿

                                        string lianyaIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("链牙")).ToString();
                                        List<ObbData> lianyaorg = downResult.datas.FindAll(c => c.lable == lianyaIndexstr).ToList(); //链牙

                                        List<ObbData> otherobb = downResult.datas.Where(s => s.lable != downmassIndexstr && s.lable != lianciIndexstr && s.lable != lianyaIndexstr).ToList();

                                        List<ObbData> lianci = lianciorg.Where(s => s.score >= paramClass.DownLianciScore).ToList();
                                        List<ObbData> lianya = lianyaorg.Where(s => s.score >= paramClass.DownLianciScore).ToList();
                                        //计算下止到链齿的最短距离
                                        List<(float, int)> Diss = new List<(float, int)>();
                                        for (int a = 0; a < downmass.Count; a++)
                                        {
                                            for (int b = 0; b < lianci.Count; b++)
                                            {
                                                float dis = CalculateDistance(downmass[a], lianci[b]);
                                                Diss.Add((dis, b));
                                                if (lianci[b].box.Center.X < downmass[a].box.Center.X) //链牙在下止左边 露牙
                                                {
                                                    luyaIndex.Add(b);
                                                }
                                            }
                                        }
                                        if (Diss.Count > 0)
                                        {
                                            var min = Diss.Min(t => t.Item1);
                                            var dis = Diss.First(t => t.Item1 == min);
                                            CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "下止距离", lianci[dis.Item2]);
                                            disData.Value = dis.Item1;
                                            dets.Add(disData);
                                            Diss.Clear();
                                        }
                                        else //没找到下止和链牙
                                        {
                                            CoordRestoreData disData = new CoordRestoreData("下止距离", 1000);
                                            dets.Add(disData);
                                        }

                                        if (luyaIndex.Count > 0)
                                        {
                                            for (int b = 0; b < luyaIndex.Count; b++)
                                            {
                                                CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "下止露牙", lianci[luyaIndex[b]]);
                                                dets.Add(disData);
                                            }
                                        }
                                        List<(float, int)> Angs = new List<(float, int)>();
                                        for (int a = 0; a < downmass.Count; a++)
                                        {
                                            for (int b = 0; b < lianya.Count; b++)
                                            {
                                                List<Point2f> downmassListsort = downmass[a].box.Points().ToList();  //先按Y从小到大排序
                                                downmassListsort.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                                                List<Point2f> lianyaListsort = lianya[b].box.Points().ToList();
                                                lianyaListsort.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                                                if (downmassListsort.Count >= 2 && lianyaListsort.Count >= 2)
                                                {
                                                    List<Point2f> downmass01 = new List<Point2f>() { downmassListsort[0], downmassListsort[1] }; //再按X从小到大排序
                                                    downmass01.Sort((p1, p2) => p1.X.CompareTo(p2.X));
                                                    List<Point2f> lianya01 = new List<Point2f>() { lianyaListsort[0], lianyaListsort[1] };
                                                    lianya01.Sort((p1, p2) => p1.X.CompareTo(p2.X));

                                                    float A1 = CalculateLineAngle(downmass01[0], downmass01[1]);
                                                    float A2 = CalculateLineAngle(lianya01[0], lianya01[1]);

                                                    float an = A2 - A1;
                                                    Angs.Add((Math.Abs(an), b));

                                                    float downmassCenterPos = Math.Abs(downmass[a].box.Center.Y - lianya[b].box.Center.Y);
                                                    CoordRestoreData disData = new CoordRestoreData("下止偏", downmassCenterPos);
                                                    dets.Add(disData);
                                                }
                                            }
                                        }
                                        if (Angs.Count > 0)
                                        {
                                            var max = Angs.Max(t => t.Item1);
                                            var ang = Angs.First(t => t.Item1 == max);
                                            CoordRestoreData angData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "下止歪", lianya[ang.Item2]);
                                            angData.Value = ang.Item1;
                                            dets.Add(angData);
                                            Angs.Clear();
                                        }
                                        else //没找到下止和链牙
                                        {
                                            CoordRestoreData disData = new CoordRestoreData("下止歪", 360);
                                            dets.Add(disData);
                                        }

                                        for (int a = 0; a < otherobb.Count; a++)
                                        {
                                            int obblabelindex = int.Parse(otherobb[a].lable);
                                            string obblabelname = downStopMass_names[obblabelindex];
                                            CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, obblabelname, otherobb[a]);
                                            dets.Add(disData);
                                        }
                                    }
                                }

                            }
                            if (cell.PhotoIndex == cell.PhotoTatolCount - 1 && labelname.Contains("正面上止"))// && !runtype) //最后一张图片有上止图片
                            {
                                RunUpMassDet(cell, img, bigResult.datas[j], out Point upmassPos, out List<CoordRestoreData> updets);
                                massPoints.Add(upmassPos);
                                if (updets?.Count > 0)
                                {
                                    dets.AddRange(updets);
                                }
                            }

                        }
                        if (massPoints.Count >= 2)
                        {
                            if (massPoints.Count == 2)
                            {
                                float massdis = Math.Abs(massPoints[0].X - massPoints[1].X); //临时这样写
                                CoordRestoreData disData = new CoordRestoreData("上止高低", massdis);
                                dets.Add(disData);
                                massPoints.Clear();
                            }
                            else
                            {
                                float massdis = Math.Abs(massPoints[massPoints.Count - 1].X - massPoints[massPoints.Count - 2].X); //临时这样写
                                CoordRestoreData disData = new CoordRestoreData("上止高低", massdis);
                                dets.Add(disData);
                                massPoints.Clear();
                            }

                        }
                    }
                }
                List<Mat> mats = new List<Mat>();
                int smallimgWidth = cell.Image.ImageWidth / 4;
                // int smallimgWidth = 640;
                // int widthstep = 480;
                int smallimgHeight = cell.Image.ImageHeight;
                if (cell.PhotoIndex != 100)  //拉头的图片不拆图
                {
                    for (int i = 0; i < 4; i++)
                    {

                        cropRec[i].X = i * smallimgWidth;
                        cropRec[i].Y = 0;
                        cropRec[i].Width = smallimgWidth;
                        cropRec[i].Height = smallimgHeight;
                        Mat cropimg = img[cropRec[i]];
                        mats.Add(cropimg);
                        cell.FourCutMatImg.Add(cropimg);

                        //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\截图\" +DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropimg);
                    }
                }

                if (cell.PhotoIndex == 1) //第一张有下止的图
                {
                    if (mats.Count == 0)
                    {
                        return;
                    }
                   // upmassCount = 0;
                    List<Point> massPoints = new List<Point>();
                    List<(DetResult, int)> detrets = ImageInferall(mats).Result;
                    if (detrets != null)
                    {
                        for (int i = 0; i < detrets.Count; i++)
                        {
                            for (int j = 0; j < detrets[i].Item1.datas.Count; j++)
                            {
                                int labelindex = int.Parse(detrets[i].Item1.datas[j].lable);
                                string labelname = Common_names[labelindex];


                                //if (labelname.Contains("正面上止") || (labelname.Contains("反面上止") && !runtype)) //第一张图片不该有上止
                                //    continue;
                                if (labelname.Contains("毛丝") && (detrets[i].Item2 == 3 || detrets[i].Item2 == 4))
                                    continue;
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelname, detrets[i].Item1.datas[j]);
                                dets.Add(restoreData);

                            }

                        }

                    }
                }
                else if (cell.PhotoIndex == cell.PhotoTatolCount - 1 )//&& !runtype) //最后一张图片有上止图片
                {
                    if (mats.Count == 0)
                    {
                        return;
                    }
                   // upmassCount = 0;
                   // List<Point> massPoints = new List<Point>(); //上止的位置
                    List<(DetResult, int)> detrets = ImageInferall(mats).Result;
                    if (detrets != null)
                    {
                        for (int i = 0; i < detrets.Count; i++)
                        {
                            for (int j = 0; j < detrets[i].Item1.datas.Count; j++)
                            {
                                int labelindex = int.Parse(detrets[i].Item1.datas[j].lable);
                                string labelname = Common_names[labelindex];

                                //if (labelname.Contains("正面下止") || labelname.Contains("反面下止")) //最后一张图片不该有下止
                                //    continue;
                                if (labelname.Contains("毛丝") && (detrets[i].Item2 == 1 || detrets[i].Item2 == 2))
                                    continue;
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelname, detrets[i].Item1.datas[j]);
                                dets.Add(restoreData);

                            }

                        }
                    }
                }
                else if (cell.PhotoIndex == 100) //有拉头的图片
                {
                    DetResult pullserachResult = ImageInferDet(yolo_pull_Serach_det, img);
                    if (pullserachResult != null)
                    {
                        for (int j = 0; j < pullserachResult.datas.Count; j++)
                        {
                            int labelindex = int.Parse(pullserachResult.datas[j].lable);
                            string labelname = pull_Search_names[labelindex];
                            if (labelname.Contains("拉头") || labelname.Contains("拉片"))
                            {
                                int lx;
                                if (labelname.Contains("拉头拉片"))
                                {
                                    lx = pullserachResult.datas[j].box.X - 120;
                                }
                                else
                                {
                                    lx = pullserachResult.datas[j].box.X + pullserachResult.datas[j].box.Width / 2 - 400;
                                }
                                int ly = pullserachResult.datas[j].box.Y + pullserachResult.datas[j].box.Height / 2 - 320;
                                int recw = 800;
                                int rech = 640;

                                if ((lx + recw) > img.Width)
                                {
                                    lx = img.Width - recw;
                                }
                                if (lx < 0)
                                {
                                    lx = 0;
                                }

                                if ((ly + rech) > img.Height)
                                {
                                    ly = img.Height - rech;
                                }
                                if (ly < 0)
                                {
                                    ly = 0;
                                }

                                Mat croppullMat = img[new Rect(lx, ly, recw, rech)];
                                cell.ZipperPullPartImg = croppullMat;
                                #region Logo识别
                                DetResult logoResult = ImageInferDet(yolo_Logo_pull_det, croppullMat);
                                if (logoResult != null)
                                {
                                    for (int i = 0; i < logoResult.count; i++)
                                    {
                                        int pulllabelindex = int.Parse(logoResult[i].lable);
                                        string pullabelname = pull_Logo_names[pulllabelindex];
                                        CoordRestoreData restoreData = new CoordRestoreData(0, 0, 0, 0, pullabelname, logoResult.datas[i], 1);
                                        dets.Add(restoreData);
                                    }
                                }
                                #endregion
                                #region 金属 烤漆拉头
                                if (cell.PullMaterlsType == "烤漆")
                                {
                                    //Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (2)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", colorMat111);                        
                                    DetResult paintpullResult = ImageInferDet(yolo_Paint_pull_det, croppullMat);
                                    if (paintpullResult != null)
                                    {
                                        for (int i = 0; i < paintpullResult.count; i++)
                                        {
                                            int pulllabelindex = int.Parse(paintpullResult[i].lable);
                                            string pullabelname = pull_Paint_names[pulllabelindex];
                                            CoordRestoreData restoreData = new CoordRestoreData(0, 0, 0, 0, pullabelname, paintpullResult.datas[i], 1);
                                            dets.Add(restoreData);
                                        }
                                    }
                                }
                                else
                                {
                                    DetResult metapullResult = ImageInferDet(yolo_Meta_pull_det, croppullMat);
                                    if (metapullResult != null)
                                    {
                                        for (int i = 0; i < metapullResult.count; i++)
                                        {
                                            int pulllabelindex = int.Parse(metapullResult[i].lable);
                                            string pullabelname = pull_Meta_names[pulllabelindex];
                                            CoordRestoreData restoreData = new CoordRestoreData(0, 0, 0, 0, pullabelname, metapullResult.datas[i], 1);
                                            dets.Add(restoreData);
                                        }
                                    }
                                }

                                #endregion
                                #region 拉片外形
                                List<double> dsimilaritys = new List<double>();
                                List<List<Point>> allcontourpoints = new List<List<Point>>();
                                if (labelname.Contains("拉片"))
                                {
                                    SegResult pullsegResult = yolo_PullShape_Seg.Predict(croppullMat) as SegResult;

                                    if (pullsegResult == null) return;

                                    List<Point[]> contoursList = new List<Point[]>();
                                    //double allperimeter = 0; //周长总长
                                    //double allarea = 0; //总面积
                                    foreach (var seg in pullsegResult.datas)
                                    // if (pullsegResult.count > 0)
                                    {
                                        //  Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (2)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", seg.mask);
                                        Mat maskgray = new Mat();
                                        Cv2.CvtColor(seg.mask, maskgray, ColorConversionCodes.BGR2GRAY);
                                        Mat binary = new Mat();
                                        Cv2.Threshold(maskgray, binary, 10, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                                        Point[][] contours;
                                        HierarchyIndex[] hierarchy;
                                        Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                                        maskgray.Dispose();
                                        binary.Dispose();
                                        if (contours != null && contours.Length == 1)
                                        {
                                            contoursList.Add(contours[0]);
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
                                if (labelname.Contains("拉头"))
                                {
                                    int px= pullserachResult.datas[j].box.X+20;
                                    int py = pullserachResult.datas[j].box.Y + 30;

                                    int rew = 120;
                                    int reh = 70;
                                    Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                                   // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (21)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
                                    Mat hsvImage=new Mat();
                                    Cv2.CvtColor(cropullColorMat, hsvImage, ColorConversionCodes.BGR2HSV);
                                    Scalar hsvMean = Cv2.Mean(hsvImage);

                                    // HSV通道说明：
                                    // H: 0-179 (色调)
                                    // S: 0-255 (饱和度)
                                    // V: 0-255 (明度)
                                    double hMean = hsvMean.Val0;
                                    double sMean = hsvMean.Val1;
                                    // double vMean = hsvMean.Val2;
                                    CoordRestoreData disDataH = new CoordRestoreData("H", (float)hMean);
                                    CoordRestoreData disDataS = new CoordRestoreData("S", (float)sMean);
                                    dets.Add(disDataH);
                                    dets.Add(disDataS);
                                    hsvImage.Dispose();

                                }
                                if (labelname.Contains("拉片"))
                                {
                                    //int px = pullserachResult.datas[j].box.X + 30;
                                    //int py = pullserachResult.datas[j].box.Y + 80;

                                    //int rew = 140;
                                    //int reh = 35;

                                    int cx = (pullserachResult.datas[j].box.X + pullserachResult.datas[j].box.Right) / 2;
                                    int cy = (pullserachResult.datas[j].box.Y + pullserachResult.datas[j].box.Bottom) / 2;

                                    int rew = 80;
                                    int reh = 30;
                                    int px = cx + 30;
                                    int py= cy - reh/2;
                                    Mat cropullColorMat = img[new Rect(px, py, rew, reh)];
                                   // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\新建文件夹 (22)\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + ".png", cropullColorMat);
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
                                    CoordRestoreData disDataH = new CoordRestoreData("H", (float)hMean);
                                    CoordRestoreData disDataS = new CoordRestoreData("S", (float)sMean);
                                    dets.Add(disDataH);
                                    dets.Add(disDataS);
                                    hsvImage.Dispose();
                                }
                                #endregion
                            }
                        }
                    }
                }
                else //中间布带，链牙缺陷
                {
                    if (mats.Count == 0)
                    {
                        return;
                    }
                    List<(DetResult, int)> detrets = ImageInferall(mats).Result;
                    if (detrets != null)
                    {
                        for (int i = 0; i < detrets.Count; i++)
                        {
                            for (int j = 0; j < detrets[i].Item1.datas.Count; j++)
                            {
                                int labelindex = int.Parse(detrets[i].Item1.datas[j].lable);
                                string labelname = Common_names[labelindex];

                                //if (labelname.Contains("正面上止") || labelname.Contains("反面上止") || (labelname.Contains("正面下止") || labelname.Contains("反面下止")))
                                //    continue;
                                if (labelname.Contains("毛丝"))
                                    continue;
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelname, detrets[i].Item1.datas[j]);
                                dets.Add(restoreData);
                            }
                        }
                    }

                }
                ParseResult(dets, cell);
                img.Dispose();
                mats.Clear();
            }
        }

        int upmassCount;
        //private void RunUpMassDet(Cell cell, Mat img, DetData detData, int i, int smallimgWidth, CParam param, out Point upmassPos, out List<CoordRestoreData> updets)
        private void RunUpMassDet(Cell cell, Mat img, DetData detData, out Point upmassPos, out List<CoordRestoreData> updets)
        {
            updets = new List<CoordRestoreData>();
            //坐标还原
            //int nameindex = int.Parse(detData.lable);
            //string labelstr = bigDet_names[nameindex];
            //CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, 0, 0, labelstr, detData);
            upmassPos = new Point(detData.box.X, detData.box.Y);
            int recw = 192;
            int rech = 96;
            int rex = Convert.ToInt32((detData.box.X+ detData.box.Width/2) - recw / 2);
            int rey = Convert.ToInt32((detData.box.Y+detData.box.Height/2) - rech / 2);
            if ((rex + recw) > cell.Image.ImageWidth)
            {
                rex = cell.Image.ImageWidth - recw;
            }
            if (rex < 0)
            {
                rex = 0;
            }
           // updets.Add(restoreData);
            Mat cropUpMat = img[new Rect(rex, rey, recw, rech)];
            cell.UpMassMatImg.Add(cropUpMat);

            DetResult otherdet = ImageInferDet(yolo_UpStopMassDefe_det, cropUpMat);
            if (otherdet != null)
            {
                if (otherdet.datas.Count > 0)
                {
                    for (int k = 0; k < otherdet.datas.Count; k++)
                    {
                        int otherindex = int.Parse(otherdet[k].lable);
                        string otherstr = upStopMassDefe_names[otherindex];
                        CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, rex, rey, otherstr, otherdet[k]);
                        updets.Add(disData);
                    }


                    //List<ObbData> otherdet = upResult.datas.Where(s => s.lable != upmassIndexstr && s.lable != lianciIndexstr).ToList();

                    // List<ObbData> upmass=upmassorg.Where(s=>s.score>= param.UpLianciScore).ToList();
                    //  List<ObbData> lianci = lianciorg.Where(s => s.score >= param.UpLianciScore).ToList();
                    //计算上止到链齿的最短距离
                }

            }

            ObbResult upmeasobbResult = ImageInferObb(yolo_UpStopMassMeas_obb, cropUpMat);
            if (upmeasobbResult != null && upmeasobbResult.datas.Count > 0)
            {
                List<int> luyaIndex = new List<int>();
                string upmassIndexstr = Array.FindIndex(upStopMassMeas_names, s => s.Contains("正面上止")).ToString();
                List<ObbData> upmass = upmeasobbResult.datas.FindAll(c => c.lable == upmassIndexstr).ToList(); //上止

                string lianciIndexstr = Array.FindIndex(upStopMassMeas_names, s => s.Contains("链齿")).ToString();
                List<ObbData> lianciorg = upmeasobbResult.datas.FindAll(c => c.lable == lianciIndexstr).ToList(); //链齿

                // Cv2.ImWrite(@"D:\测试存图\" +DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + "上止.png", cropUpMat);
                List<(float, int)> Diss = new List<(float, int)>();
                for (int a = 0; a < upmass.Count; a++)
                {
                    for (int b = 0; b < lianciorg.Count; b++)
                    {
                        float dis = CalculateDistance(upmass[a], lianciorg[b]);
                        Diss.Add((dis, b));
                        if (lianciorg[b].box.Center.X > upmass[a].box.Center.X) //链牙在下止左边 露牙
                        {
                            luyaIndex.Add(b);
                        }
                    }
                }

                if (Diss.Count > 0) //有找到链牙和上止
                {
                    upmassCount++;
                    var min = Diss.Min(t => t.Item1);
                    var dis = Diss.First(t => t.Item1 == min);
                    CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, rex, rey, $"上止距离{upmassCount}", lianciorg[dis.Item2]);
                    disData.Value = dis.Item1;
                    updets.Add(disData);
                    Diss.Clear();
                }
                else //没找到链牙和上止
                {
                    upmassCount++;
                    CoordRestoreData disData = new CoordRestoreData($"上止距离{upmassCount}", 1000);
                    updets.Add(disData);
                }
                if (luyaIndex.Count > 0)
                {
                    for (int b = 0; b < luyaIndex.Count; b++)
                    {
                        CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, rex, rey, "上止露牙", lianciorg[b]);
                    }
                }
            }
        }

        private async Task<List<(DetResult, int)>> ImageInferall(List<Mat> mats)
        {
            List<(DetResult, int)> alldetResult = new List<(DetResult, int)>();
            Task<DetResult> task1 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det1, mats[0]);
                return sResultInfos;
            });

            Task<DetResult> task2 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det2, mats[1]);
                return sResultInfos;
            });
            Task<DetResult> task3 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det3, mats[2]);
                return sResultInfos;
            });
            Task<DetResult> task4 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det4, mats[3]);
                return sResultInfos;
            });
            await Task.WhenAll(task1, task2, task3, task4);
            alldetResult.Add((task1.Result, 1));
            alldetResult.Add((task2.Result, 2));
            alldetResult.Add((task3.Result, 3));
            alldetResult.Add((task4.Result, 4));
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
                    int common_Categ_num = Common_names.Length;

                    int downmass_num = 0;
                    if (downStopMass_names?.Length > 0)
                    {
                        downmass_num = downStopMass_names.Length;
                    }
                    int upmass_num = 0;
                    if (upStopMassDefe_names?.Length > 0)
                    {
                        upmass_num = upStopMassDefe_names.Length;
                    }

                    int upmassmeas_num = 0;
                    if (upStopMassMeas_names?.Length > 0)
                    {
                        upmassmeas_num = upStopMassMeas_names.Length;
                    }

                    int pull_search_num = pull_Search_names.Length;
                    int metapull_num = pull_Meta_names.Length;
                    int paintpull_num = pull_Paint_names.Length;
                    int logopull_num = pull_Logo_names.Length;
                    int big_num = bigDet_names.Length;
                    int pullsharp_num = pullSharp_names.Length;
                    float Score = param.CommonScore;
                    float Nms = param.Nms;
                    int Input_size = 640;

                    //Task task1 = Task.Run(() =>
                    //{
                    yolo_all_det1 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Common_Model_Path, EngineType.TensorRT,
CurrentDevice, common_Categ_num, Score, Nms, Input_size);
                    // });

                    //Task task2 = Task.Run(() =>
                    //{
                    yolo_all_det2 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Common_Model_Path, EngineType.TensorRT,
CurrentDevice, common_Categ_num, Score, Nms, Input_size);
                    // });

                    //Task task3 = Task.Run(() =>
                    //{
                    yolo_all_det3 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Common_Model_Path, EngineType.TensorRT,
CurrentDevice, common_Categ_num, Score, Nms, Input_size);
                    // });

                    //Task task4 = Task.Run(() =>
                    //{
                    yolo_all_det4 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Common_Model_Path, EngineType.TensorRT,
CurrentDevice, common_Categ_num, Score, Nms, Input_size);
                    //});


                    //Task task5 = Task.Run(() =>
                    //{
                    if (downmass_num > 0)
                    {
                        yolo_DownStopMass_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, downStopMass_Model_Path, EngineType.TensorRT,
CurrentDevice, downmass_num, param.DownScore, Nms, 256);
                    }
                    //  });

                    //Task task6 = Task.Run(() =>
                    //{
                    if (upmass_num > 0)
                    {
                        yolo_UpStopMassDefe_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, upStopMassDefe_Model_Path, EngineType.TensorRT,
    CurrentDevice, upmass_num, param.UpScore, Nms, 192);
                    }
                    //  });

                    if (upmassmeas_num > 0)
                    {
                        yolo_UpStopMassMeas_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, upStopMassMeas_Model_Path, EngineType.TensorRT,
    CurrentDevice, upmassmeas_num, param.UpLianciScore, Nms, 192);
                    }

                    //Task task7 = Task.Run(() =>
                    //{
                    yolo_pull_Serach_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Search_Model_Path, EngineType.TensorRT,
CurrentDevice, pull_search_num, param.AutoScore, Nms, 480);
                    // });

                    // Task task8 = Task.Run(() =>
                    // {
                    yolo_Meta_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Meta_Model_Path, EngineType.TensorRT,
CurrentDevice, metapull_num, param.MetaPullScore, Nms, 640);
                    //});

                    yolo_Paint_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Paint_Model_Path, EngineType.TensorRT,
CurrentDevice, paintpull_num, param.PaintPullScore, Nms, 640);

                    yolo_Logo_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Logo_Model_Path, EngineType.TensorRT,
CurrentDevice, logopull_num, param.LogoPullScore, 0.8f, 640);

                    //Task task9 = Task.Run(() =>
                    //{
                    yolo_BigDet_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Big_Model_Path, EngineType.TensorRT,
CurrentDevice, big_num, param.BigScore, Nms, 480);
                    //});
                    // Task task10 = Task.Run(() =>
                    // {
                    yolo_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSharp_Model_Path, EngineType.TensorRT,
CurrentDevice, pullsharp_num, param.PullSharpScore, Nms, 640);
                    // });

                    // await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9);

                }

            }
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

        public  double MatchShapesUsingHuMoments(Point[] contours1, Point[] contours2)
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
        public  double MatchShapesWithCv2(Point[] contours1, Point[] contours2,
                                                ShapeMatchModes mode = ShapeMatchModes.I2)
        {
            using (var contour1Mat = new Mat(contours1.Length, 1, MatType.CV_32SC2))
            using (var contour2Mat = new Mat(contours2.Length, 1, MatType.CV_32SC2))
            {
                
                    // 填充点数据
                    for (int i = 0; i < contours1.Length; i++)
                    {
                        contour1Mat.Set(i, 0, new Point( contours1[i].X, contours1[i].Y ));
                    }
                    for (int i = 0; i < contours2.Length; i++)
                    {
                        contour2Mat.Set(i, 0, new Point ( contours2[i].X, contours2[i].Y ));
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
            yolo_all_det1.UpdateNMS_Score(param.Nms, param.CommonScore);
            yolo_all_det2.UpdateNMS_Score(param.Nms, param.CommonScore);
            yolo_all_det3.UpdateNMS_Score(param.Nms, param.CommonScore);
            yolo_all_det4.UpdateNMS_Score(param.Nms, param.CommonScore);
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
            yolo_PullShape_Seg.UpdateNMS_Score(param.Nms, param.PullSharpScore);
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
