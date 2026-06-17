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


namespace MetalZipperAlgorihm
{
    public class CMetalZipperAlgorihmParam : CAlgorithmParamBase
    {
        /// <summary>
        /// 布带检测对象1（亮）
        /// </summary>
        IVisionModel WH_Cloth_det1;
        /// <summary>
        /// 布带检测对象2（亮）
        /// </summary>
        IVisionModel WH_Cloth_det2;
        /// <summary>
        /// 链牙检测对象1 (暗）
        /// </summary>
        IVisionModel WH_Tooth_det1;
        /// <summary>
        /// 链牙检测对象2 (暗）
        /// </summary>
        IVisionModel WH_Tooth_det2;

        /// <summary>
        /// 下止检测对象
        /// </summary>
        IVisionModel WH_DownStopMass_obb;


        /// <summary>
        /// 上止测量对象
        /// </summary>
        IVisionModel WH_UpStopMassMeas_obb;

        /// <summary>
        /// 拉头查找对象
        /// </summary>
        IVisionModel WH_pull_Serach_det;
        //定义4组矩形来裁切图片
        Rect[] cropRec = new Rect[2];


        /// <summary>
        /// 大缺陷检测对象
        /// </summary>
        IVisionModel WH_BigDet_det;
        /// <summary>
        /// 拉片分割模型
        /// </summary>
       // IVisionModel WH_PullShape_Seg;

        public CMetalZipperAlgorihmParam(string user) : base()
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
        /// 2025.3.3 鲍赞宝
        /// 通用模型路径
        /// </summary>
        private string Cloth_Model_Path;
        /// <summary>
        /// 下止模型路径
        /// </summary>
        private string downStopMass_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 大缺陷模型路径
        /// </summary>
        private string Big_Model_Path;

        /// <summary>
        /// 2025.11.17 鲍赞宝
        /// 拉片分割模型路径
        /// </summary>
        // private string pullSharp_Model_Path;

        /// <summary>
        /// 2026.6.6 鲍赞宝
        /// 金属链牙模型路径
        /// </summary>
        private string Tooth_Model_Path;


        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 布带缺陷名称路径
        /// </summary>
        //常规缺陷名称
        protected string[] Cloth_names;
        //下止缺陷名称
        protected string[] downStopMass_names;

        //大缺陷名称
        protected string[] bigDet_names;

        //拉片分割缺陷名称
        // protected string[] pullSharp_names;

        //金属链牙缺陷名称
        protected string[] Tooth_names;

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

            for (int i = 0; i < bigDet_names?.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(bigDet_names[i], Category.区域);
                bigRecipes.Add(defectRecipe);
            }

            CDefectSpecies bigSpecies = new CDefectSpecies("大缺陷", bigRecipes);
            #endregion
            #region 通用
            List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
            for (int i = 0; i < Cloth_names?.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(Cloth_names[i], Category.区域);
                cDefectRecipes.Add(defectRecipe);
            }
            for (int i = 0; i < Tooth_names?.Length; i++)
            {
                CDefectRecipe defectRecipe = new CDefectRecipe(Tooth_names[i], Category.区域);
                cDefectRecipes.Add(defectRecipe);
            }
            if (user == "正面")
            {
                //CDefectRecipe defectRecipe1_1 = new CDefectRecipe("上止距离1", Category.值);
                //CDefectRecipe defectRecipe1_2 = new CDefectRecipe("上止距离2", Category.值);
                //cDefectRecipes.Add(defectRecipe1_1);
                //cDefectRecipes.Add(defectRecipe1_2);
                CDefectRecipe defectRecipe3 = new CDefectRecipe("长插销距离", Category.值);
                cDefectRecipes.Add(defectRecipe3);
                CDefectRecipe defectRecipe6 = new CDefectRecipe("短插销距离", Category.值);
                cDefectRecipes.Add(defectRecipe6);
                CDefectRecipe defectRecipe4 = new CDefectRecipe("插销歪", Category.值);
                cDefectRecipes.Add(defectRecipe4);
                CDefectRecipe defectRecipe5 = new CDefectRecipe("插销偏", Category.值);
                cDefectRecipes.Add(defectRecipe5);
                CDefectRecipe defectRecipe7 = new CDefectRecipe("插销角度", Category.值);
                cDefectRecipes.Add(defectRecipe7);
            }
            //CDefectRecipe defectRecipe6 = new CDefectRecipe("上止高低", Category.值);
            //cDefectRecipes.Add(defectRecipe6);
            CDefectSpecies defectSpecies = new CDefectSpecies("拉链", cDefectRecipes);
            #endregion

            #region 下止
            if (downStopMass_names?.Length > 0)
            {
                string[] Downstrs = downStopMass_names.Where(s =>  s != "链齿" && s != "链牙").ToArray();
                for (int i = 0; i < Downstrs.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(Downstrs[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
                //CDefectRecipe defectRecipe5 = new CDefectRecipe("下止露牙", Category.区域);
                //cDefectRecipes.Add(defectRecipe5);
            }

            #endregion

            #region 拉头 拉片 LOGO 拉片外形

            //int sbsindex = pull_names.ToList().IndexOf("SBS");
            //string[] pullstrs = pull_names.Take(sbsindex).ToArray();
            //string[] logostrs = pull_names.Skip(sbsindex).ToArray();

            // string[] pullstrs = pull_Meta_names.Where(s => s.Contains("拉")).ToArray();


            //List<CDefectRecipe> pullRecipes = new List<CDefectRecipe>();
            //for (int i = 0; i < pull_Meta_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe = new CDefectRecipe(pull_Meta_names[i], Category.区域);
            //    pullRecipes.Add(defectRecipe);
            //}
            //for (int i = 0; i < pull_Paint_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe = new CDefectRecipe(pull_Paint_names[i], Category.区域);
            //    pullRecipes.Add(defectRecipe);
            //}
            //for (int i = 0; i < pullSharp_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe2 = new CDefectRecipe(pullSharp_names[i], Category.值);
            //    pullRecipes.Add(defectRecipe2);
            //}
            ////颜色
            //CDefectRecipe defectRecipe1_H = new CDefectRecipe("拉头色差1", Category.值);
            //CDefectRecipe defectRecipe1_S = new CDefectRecipe("拉头色差2", Category.值);
            //pullRecipes.Add(defectRecipe1_H);
            //pullRecipes.Add(defectRecipe1_S);

            //CDefectSpecies pullSpecies = new CDefectSpecies("拉头拉片", pullRecipes);

            //List<CDefectRecipe> logoRecipes = new List<CDefectRecipe>();
            //for (int i = 0; i < pull_Logo_names.Length; i++)
            //{
            //    CDefectRecipe defectRecipe = new CDefectRecipe(pull_Logo_names[i], Category.区域);
            //    logoRecipes.Add(defectRecipe);
            //}
            //CDefectSpecies logoSpecies = new CDefectSpecies("LOGO", logoRecipes);



            #endregion

            DefectSpecies.Add(defectSpecies);
            DefectSpecies.Add(bigSpecies);
            //DefectSpecies.Add(pullSpecies);
            //DefectSpecies.Add(logoSpecies);

        }

        protected void ReadNames(string user)
        {

            string modelDirpath = ".\\AlgorithmPlug\\MetalZipperAlgorihm\\Models\\";

            string commonModelPath = modelDirpath + "CommonModel\\";
            string bigModelPath = modelDirpath + "BigDetModel\\";

            var commons = GetNames(commonModelPath);
            if (commons.Item1 != "")
            {
                Cloth_Model_Path = commons.Item1;
                Cloth_names = commons.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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

            }

            string ToothModel = modelDirpath + "ToothModel\\";
            var toothchtrs = GetNames(ToothModel);
            if (toothchtrs.Item1 != "")
            {
                Tooth_Model_Path = toothchtrs.Item1;
                Tooth_names = toothchtrs.Item2.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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
                // Cv2.ImWrite(@"D:\MealImages\" + cell.CamName +"_"+cell.ID+"_"+cell.PhotoIndex+".jpg", img); 
                // bool runtype = false; //判断是只处理1张图像还是多张图像，true为1张
                //if (cell.PhotoTatolCount == 2)
                //{
                //    runtype = true;
                //}

                // int oddoreven = cell.PhotoIndex % 2;
                List<CoordRestoreData> dets = new List<CoordRestoreData>();
                if (cell.PhotoIndex >= 100) // 大缺陷只检测偶数图（第二张图）
                {
                    DetResult bigResult = ImageInferDet(WH_BigDet_det, img);

                    if (bigResult != null && bigResult.datas.Count > 0) //如果有大缺陷直接退出
                    {
                        for (int j = 0; j < bigResult.datas.Count; j++)
                        {
                            int labelindex = int.Parse(bigResult.datas[j].lable);
                            string labelname = bigDet_names[labelindex];
                            CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, (cell.PhotoIndex/100) - 1, 0, 0, labelname, bigResult.datas[j]);
                            if (labelname.Contains("方块插销") && cell.PhotoIndex != 100)
                                continue;
                            dets.Add(restoreData);
                            if (labelname.Contains("方块插销") && cell.PhotoIndex == 100) //检测下止
                            {
                                int recw = 384;
                                int rech = 384;
                                int rex = Convert.ToInt32(restoreData.OrgCenterX - 115);
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

                                // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面下止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_")  + ".png", cropDownMat);
                                ObbResult downResult = ImageInferObb(WH_DownStopMass_obb, cropDownMat);
                                if (downResult != null)
                                {
                                    if (downResult.datas.Count > 0)
                                    {
                                        List<int> luyaIndex = new List<int>();
                                        string fangkuaiIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("方块插销")).ToString();
                                        List<ObbData> fangkuaimass = downResult.datas.FindAll(c => c.lable == fangkuaiIndexstr).ToList(); //方块

                                        string downmassIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("长插销")).ToString();
                                        List<ObbData> downmass = downResult.datas.FindAll(c => c.lable == downmassIndexstr).ToList(); //长插销

                                        string downmassIndexstr2 = Array.FindIndex(downStopMass_names, s => s.Contains("短插销")).ToString();
                                        List<ObbData> downmass2 = downResult.datas.FindAll(c => c.lable == downmassIndexstr2).ToList(); //短插销

                                        string lianciIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("链齿")).ToString();
                                        List<ObbData> lianciorg = downResult.datas.FindAll(c => c.lable == lianciIndexstr).ToList(); //链齿

                                        string lianyaIndexstr = Array.FindIndex(downStopMass_names, s => s.Contains("链牙")).ToString();
                                        List<ObbData> lianyaorg = downResult.datas.FindAll(c => c.lable == lianyaIndexstr).ToList(); //链牙

                                        List<ObbData> otherobb = downResult.datas.Where(s => s.lable != fangkuaiIndexstr && s.lable != downmassIndexstr && s.lable != lianciIndexstr && s.lable != lianyaIndexstr && s.lable != downmassIndexstr2).ToList();

                                        List<ObbData> lianci = lianciorg.Where(s => s.score >= paramClass.DownLianciScore).ToList();
                                        List<ObbData> lianya = lianyaorg.Where(s => s.score >= paramClass.DownLianciScore).ToList();

                                        if (downmass.Count > 0)
                                        {
                                            float dcpointx = downmass[0].box.Center.X;
                                            float dcpointy = downmass[0].box.Center.Y;
                                            List<ObbData> lianci2 = lianci.FindAll(s => Math.Abs(s.box.Center.Y - dcpointy) <= 15).ToList(); //链齿根据Y坐标距离来筛选，排除和下止不在同一水平线的链齿
                                            lianci2.Sort((a, b) => Math.Abs(a.box.Center.X - dcpointx).CompareTo(Math.Abs(b.box.Center.X - dcpointx))); //根据X坐标距离来排序，找出最靠近下止的链齿

                                            float dis = lianci2[0].box.Center.X - dcpointx;
                                            CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "长插销距离", lianci2[0]);
                                            disData.Value = dis;
                                            dets.Add(disData);

                                            CoordRestoreData disData1 = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "长插销", downmass[0]);
                                            dets.Add(disData1);
                                        }

                                        if (downmass2.Count > 0)
                                        {
                                            float dcpointx = downmass2[0].box.Center.X;
                                            float dcpointy = downmass2[0].box.Center.Y;
                                            List<ObbData> lianci2 = lianci.FindAll(s => Math.Abs(s.box.Center.Y - dcpointy) <= 15).ToList(); //链齿根据Y坐标距离来筛选，排除和下止不在同一水平线的链齿
                                            lianci2.Sort((a, b) => Math.Abs(a.box.Center.X - dcpointx).CompareTo(Math.Abs(b.box.Center.X - dcpointx))); //根据X坐标距离来排序，找出最靠近下止的链齿
                                            float dis = lianci2[0].box.Center.X - dcpointx;
                                            CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "短插销距离", lianci2[0]);
                                            disData.Value = dis;
                                            dets.Add(disData);
                                            CoordRestoreData disData1 = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "短插销", downmass2[0]);
                                            dets.Add(disData1);
                                        }

                                        ////计算下止到链齿的最短距离
                                        //List<(float, int)> Diss = new List<(float, int)>();
                                        //for (int a = 0; a < downmass.Count; a++)
                                        //{
                                        //    for (int b = 0; b < lianci.Count; b++)
                                        //    {
                                        //        float dis = CalculateDistance(downmass[a], lianci[b]);
                                        //        Diss.Add((dis, b));
                                        //        if (lianci[b].box.Center.X < downmass[a].box.Center.X) //链牙在下止左边 露牙
                                        //        {
                                        //            luyaIndex.Add(b);
                                        //        }
                                        //    }
                                        //}
                                        //if (Diss.Count > 0)
                                        //{
                                        //    var min = Diss.Min(t => t.Item1);
                                        //    var dis = Diss.First(t => t.Item1 == min);
                                        //    CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "下止距离", lianci[dis.Item2]);
                                        //    disData.Value = dis.Item1;
                                        //    dets.Add(disData);
                                        //    Diss.Clear();
                                        //}
                                        //else //没找到下止和链牙
                                        //{
                                        //    CoordRestoreData disData = new CoordRestoreData("下止距离", 1000);
                                        //    dets.Add(disData);
                                        //}

                                        //if (luyaIndex.Count > 0)
                                        //{
                                        //    for (int b = 0; b < luyaIndex.Count; b++)
                                        //    {
                                        //        CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "下止露牙", lianci[luyaIndex[b]]);
                                        //        dets.Add(disData);
                                        //    }
                                        //}
                                        List<(float, int)> Angs = new List<(float, int)>();
                                        for (int a = 0; a < downmass.Count; a++)
                                        {
                                            for (int b = 0; b < downmass2.Count; b++)
                                            {
                                                List<Point2f> downmassListsort = downmass[a].box.Points().ToList();  //先按Y从小到大排序
                                                downmassListsort.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                                                List<Point2f> downmass2Listsort = downmass2[b].box.Points().ToList();
                                                downmass2Listsort.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                                                if (downmassListsort.Count >= 2 && downmass2Listsort.Count >= 2)
                                                {
                                                    List<Point2f> downmass01 = new List<Point2f>() { downmassListsort[0], downmassListsort[1] }; //再按X从小到大排序
                                                    downmass01.Sort((p1, p2) => p1.X.CompareTo(p2.X));
                                                    List<Point2f> downmass02 = new List<Point2f>() { downmass2Listsort[0], downmass2Listsort[1] };
                                                    downmass02.Sort((p1, p2) => p1.X.CompareTo(p2.X));

                                                    float A1 = CalculateLineAngle(downmass01[0], downmass01[1]);
                                                    float A2 = CalculateLineAngle(downmass02[0], downmass02[1]);

                                                    float an = A2 - A1;
                                                    Angs.Add((Math.Abs(an), b));

                                                    //float downmassCenterPos = Math.Abs(downmass[a].box.Center.Y - lianya[b].box.Center.Y);
                                                    //CoordRestoreData disData = new CoordRestoreData("下止偏", downmassCenterPos);
                                                    //dets.Add(disData);
                                                }
                                            }
                                        }
                                        if (Angs.Count > 0)
                                        {
                                            var max = Angs.Max(t => t.Item1);
                                            var ang = Angs.First(t => t.Item1 == max);
                                            CoordRestoreData angData = new CoordRestoreData(cell.Image.ImageWidth, 0, rex, rey, "插销角度", downmass2[ang.Item2]);
                                            angData.Value = ang.Item1;
                                            dets.Add(angData);
                                            Angs.Clear();
                                        }
                                        else //没找到插销
                                        {
                                            CoordRestoreData disData = new CoordRestoreData("插销角度", 360);
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
                        }
                    }
                }
                List<Mat> mats = new List<Mat>();
                int smallimgWidth = cell.Image.ImageWidth / cropRec.Length;
                int smallimgHeight = cell.Image.ImageHeight;
                for (int i = 0; i < cropRec.Length; i++)
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

                if (mats.Count == 0)
                {
                    return;
                }
                List<(DetResult, int)> detrets = null;
                if (cell.PhotoIndex >= 100)
                {
                    detrets = ToothImageInferall(mats).Result; //链牙工位的缺陷检测结果 
                }
                else
                {
                    detrets = ClothImageInferall(mats).Result; //布带工位的缺陷检测结果
                }

                if (detrets != null)
                {
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].Item1?.datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].Item1.datas[j].lable);
                            string labelname="";
                            int photoindex = 0;
                            if (cell.PhotoIndex >= 100)
                            {
                                labelname = Tooth_names[labelindex];
                                photoindex = cell.PhotoIndex / 100;
                            }
                            else
                            {
                                labelname = Cloth_names[labelindex];
                                photoindex = cell.PhotoIndex;
                            }
                            if (labelname == "布带脏污" || labelname == "黑点脏污")
                            {
                                float colordiffvalue = DirtyDefetOperration(mats[detrets[i].Item2 - 1], detrets[i].Item1.datas[j]);
                                CoordRestoreData dirtyData = new CoordRestoreData(cell.Image.ImageWidth, photoindex - 1, i * smallimgWidth, 0, labelname, detrets[i].Item1.datas[j], colordiffvalue);
                                dets.Add(dirtyData);
                                continue;
                            }
                            CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, photoindex - 1, i * smallimgWidth, 0, labelname, detrets[i].Item1.datas[j]);
                            dets.Add(restoreData);

                        }

                    }

                }

                ParseResult(dets, cell);
                img.Dispose();
                mats.Clear();
            }
        }


        /// <summary>
        /// 布带工位的算法推理，2026.6.9 鲍赞宝
        /// </summary>
        /// <param name="mats"></param>
        /// <returns></returns>
        private async Task<List<(DetResult, int)>> ClothImageInferall(List<Mat> mats)
        {
            List<(DetResult, int)> alldetResult = new List<(DetResult, int)>();
            Task<DetResult> task1 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(WH_Cloth_det1, mats[0]);
                return sResultInfos;
            });

            Task<DetResult> task2 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(WH_Cloth_det2, mats[1]);
                return sResultInfos;
            });

            await Task.WhenAll(task1, task2);
            alldetResult.Add((task1.Result, 1));
            alldetResult.Add((task2.Result, 2));
            return alldetResult;
        }
        /// <summary>
        /// 链牙工位的算法推理，2026.6.9 鲍赞宝
        /// </summary>
        /// <param name="mats"></param>
        /// <returns></returns>
        private async Task<List<(DetResult, int)>> ToothImageInferall(List<Mat> mats)
        {
            List<(DetResult, int)> alldetResult = new List<(DetResult, int)>();
            Task<DetResult> task1 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(WH_Tooth_det1, mats[0]);
                return sResultInfos;
            });

            Task<DetResult> task2 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(WH_Tooth_det2, mats[1]);
                return sResultInfos;
            });

            await Task.WhenAll(task1, task2);
            alldetResult.Add((task1.Result, 1));
            alldetResult.Add((task2.Result, 2));
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
                int Input_size = 1024;
                float Score = param.CommonScore;
                if (Cloth_names != null && Cloth_names.Length > 0)
                {
                    int common_Categ_num = Cloth_names.Length;
                    WH_Cloth_det1 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Cloth_Model_Path, engineType,
                        CurrentDevice, common_Categ_num, Score, Nms, Input_size);

                    WH_Cloth_det2 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Cloth_Model_Path, engineType,
                        CurrentDevice, common_Categ_num, Score, Nms, Input_size);
                }

                if (Tooth_names != null && Tooth_names.Length > 0)
                {
                    int tooth_Categ_num = Tooth_names.Length;
                    WH_Tooth_det1 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Tooth_Model_Path, engineType,
                        CurrentDevice, tooth_Categ_num, Score, Nms, Input_size);

                    WH_Tooth_det2 = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Tooth_Model_Path, engineType,
                        CurrentDevice, tooth_Categ_num, Score, Nms, Input_size);
                }

                if (downStopMass_names != null && downStopMass_names?.Length > 0)
                {
                    int downmass_num = downStopMass_names.Length;
                    WH_DownStopMass_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, downStopMass_Model_Path, engineType,
                        CurrentDevice, downmass_num, param.DownScore, Nms, 384);
                }
                if (bigDet_names != null && bigDet_names.Length > 0)
                {
                    int big_num = bigDet_names.Length;
                    WH_BigDet_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, Big_Model_Path, engineType,
                        CurrentDevice, big_num, param.BigScore, Nms, 1024);
                }


                //                    if (upmass_num > 0)
                //                    {
                //                        WH_UpStopMassDefe_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, upStopMassDefe_Model_Path, engineType,
                //    CurrentDevice, upmass_num, param.UpScore, Nms, 192);
                //                    }


                //                    if (upmassmeas_num > 0)
                //                    {
                //                        WH_UpStopMassMeas_obb = VisionModelExtensions.GetVisionModel(ModelType.VisionModelObb, upStopMassMeas_Model_Path, engineType,
                //    CurrentDevice, upmassmeas_num, param.UpLianciScore, Nms, 192);
                //                    }

                //                    WH_pull_Serach_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Search_Model_Path, engineType,
                //CurrentDevice, pull_search_num, param.AutoScore, Nms, 480);

                //                    WH_Meta_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Meta_Model_Path, engineType,
                //CurrentDevice, metapull_num, param.MetaPullScore, Nms, 640);


                //                    WH_Paint_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Paint_Model_Path, engineType,
                //CurrentDevice, paintpull_num, param.PaintPullScore, Nms, 640);

                //                    WH_Logo_pull_det = VisionModelExtensions.GetVisionModel(ModelType.VisionModelDet, pull_Logo_Model_Path, engineType,
                //CurrentDevice, logopull_num, param.LogoPullScore, 0.8f, 640);




                //                    WH_PullShape_Seg = VisionModelExtensions.GetVisionModel(ModelType.VisionModelSeg, pullSharp_Model_Path, engineType,
                //CurrentDevice, pullsharp_num, param.PullSharpScore, Nms, 640);


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
            sRegioninfo.ColorDiffValue = info.Value;//20260424 鲍赞宝
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

        #region 计算色差
        /// <summary>
        /// 计算两个颜色点的 ΔE76
        /// </summary>
        public double CalculateDeltaE76(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            double dl = l2 - l1;
            double da = a2 - a1;
            double db = b2 - b1;
            return Math.Sqrt(dl * dl + da * da + db * db);
        }


        /// <summary>
        /// 计算 ΔE94 色差
        /// </summary>
        public double CalculateDeltaE94(
            double l1, double a1, double b1,
            double l2, double a2, double b2,
            string applicationType = "graphicarts")
        {
            // 计算 C1, C2
            double c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            double c2 = Math.Sqrt(a2 * a2 + b2 * b2);

            // 计算 ΔL, ΔC, ΔH
            double deltaL = l2 - l1;
            double deltaC = c2 - c1;
            double deltaH = CalculateDeltaH(a1, b1, a2, b2, c1, c2);

            // 计算 C̄
            double cBar = (c1 + c2) / 2.0;

            // 权重因子
            double sl = 1.0;
            double sc = 1.0 + 0.045 * cBar;
            double sh = 1.0 + 0.015 * cBar;

            // 应用类型调整
            if (applicationType.ToLower() == "textiles")
            {
                sl = 1.0;
                sc = 1.0 + 0.045 * cBar;
                sh = 1.0 + 0.015 * cBar;
            }

            // 计算 ΔE94
            double term1 = deltaL / sl;
            double term2 = deltaC / sc;
            double term3 = deltaH / sh;

            return Math.Sqrt(term1 * term1 + term2 * term2 + term3 * term3);
        }

        /// <summary>
        /// 计算 ΔE00 色差（CIEDE2000）
        /// </summary>
        public double CalculateDeltaE00(
            double l1, double a1, double b1,
            double l2, double a2, double b2)
        {
            // 1. 计算 C1, C2
            double c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            double c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            double cBar = (c1 + c2) / 2.0;

            // 2. 计算 G
            double g = 0.5 * (1 - Math.Sqrt(
                Math.Pow(cBar, 7) / (Math.Pow(cBar, 7) + Math.Pow(25, 7))));

            // 3. 计算 a'
            double a1p = a1 * (1 + g);
            double a2p = a2 * (1 + g);

            // 4. 计算 C'
            double c1p = Math.Sqrt(a1p * a1p + b1 * b1);
            double c2p = Math.Sqrt(a2p * a2p + b2 * b2);

            // 5. 计算 h'
            double h1p = CalculateHue(a1p, b1);
            double h2p = CalculateHue(a2p, b2);

            // 6. 计算 ΔL', ΔC', ΔH'
            double deltaLp = l2 - l1;
            double deltaCp = c2p - c1p;
            double deltahp = CalculateDeltaHue(h1p, h2p, c1p, c2p);
            double deltaHp = 2 * Math.Sqrt(c1p * c2p) * Math.Sin(ToRadians(deltahp) / 2.0);

            // 7. 计算平均值
            double lpBar = (l1 + l2) / 2.0;
            double cpBar = (c1p + c2p) / 2.0;
            double hpBar = CalculateHueBar(h1p, h2p, c1p, c2p);

            // 8. 计算 T
            double t = 1 - 0.17 * Math.Cos(ToRadians(hpBar - 30)) +
                       0.24 * Math.Cos(ToRadians(2 * hpBar)) +
                       0.32 * Math.Cos(ToRadians(3 * hpBar + 6)) -
                       0.20 * Math.Cos(ToRadians(4 * hpBar - 63));

            // 9. 计算 Δθ 和 Rc
            double deltaTheta = 30 * Math.Exp(-Math.Pow((hpBar - 275) / 25, 2));
            double rc = 2 * Math.Sqrt(
                Math.Pow(cpBar, 7) / (Math.Pow(cpBar, 7) + Math.Pow(25, 7)));

            // 10. 计算权重因子
            double sl = 1 + (0.015 * Math.Pow(lpBar - 50, 2)) / Math.Sqrt(20 + Math.Pow(lpBar - 50, 2));
            double sc = 1 + 0.045 * cpBar;
            double sh = 1 + 0.015 * cpBar * t;
            double rt = -Math.Sin(ToRadians(2 * deltaTheta)) * rc;

            // 11. 最终计算
            double term1 = deltaLp / sl;
            double term2 = deltaCp / sc;
            double term3 = deltaHp / sh;

            return Math.Sqrt(term1 * term1 + term2 * term2 + term3 * term3 + rt * term2 * term3);
        }

        private double CalculateHue(double a, double b)
        {
            if (a == 0 && b == 0) return 0;
            double h = Math.Atan2(b, a) * 180.0 / Math.PI;
            return h >= 0 ? h : h + 360;
        }

        private double CalculateDeltaHue(double h1, double h2, double c1, double c2)
        {
            if (c1 * c2 == 0) return 0;
            double diff = h2 - h1;
            if (Math.Abs(diff) <= 180) return diff;
            return diff > 180 ? diff - 360 : diff + 360;
        }

        private double CalculateHueBar(double h1, double h2, double c1, double c2)
        {
            if (c1 * c2 == 0) return h1 + h2;
            double sum = h1 + h2;
            if (Math.Abs(h1 - h2) > 180) sum += 360;
            return sum / 2.0;
        }
        private double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        /// <summary>
        /// 单点 BGR 转 Lab
        /// </summary>
        public (double L, double a, double b) BgrToLab(byte b, byte g, byte r)
        {
            // 先转 XYZ
            var xyz = RgbToXyz(r, g, b);
            // 再转 Lab
            return XyzToLab(xyz.x, xyz.y, xyz.z);
        }

        /// <summary>
        /// RGB 转 XYZ（sRGB 色彩空间）
        /// </summary>
        private (double x, double y, double z) RgbToXyz(byte r, byte g, byte b)
        {
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            // 反伽马校正
            red = (red > 0.04045) ? Math.Pow((red + 0.055) / 1.055, 2.4) : red / 12.92;
            green = (green > 0.04045) ? Math.Pow((green + 0.055) / 1.055, 2.4) : green / 12.92;
            blue = (blue > 0.04045) ? Math.Pow((blue + 0.055) / 1.055, 2.4) : blue / 12.92;

            // 转换矩阵
            double x = red * 0.4124564 + green * 0.3575761 + blue * 0.1804375;
            double y = red * 0.2126729 + green * 0.7151522 + blue * 0.0721750;
            double z = red * 0.0193339 + green * 0.1191920 + blue * 0.9503041;

            return (x * 100, y * 100, z * 100);
        }

        // D65 标准光源白点
        private const double Xn = 95.047;
        private const double Yn = 100.0;
        private const double Zn = 108.883;

        /// <summary>
        /// XYZ 转 Lab
        /// </summary>
        private (double L, double a, double b) XyzToLab(double x, double y, double z)
        {
            x /= Xn;
            y /= Yn;
            z /= Zn;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            double l = (116.0 * y) - 16.0;
            double aVal = 500.0 * (x - y);
            double bVal = 200.0 * (y - z);

            return (l, aVal, bVal);
        }

        private double CalculateDeltaH(double a1, double b1, double a2, double b2, double c1, double c2)
        {
            double deltaA = a2 - a1;
            double deltaB = b2 - b1;
            double deltaC = c2 - c1;

            double deltaHSquared = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
            return deltaHSquared > 0 ? Math.Sqrt(deltaHSquared) : 0;
        }
        /// <summary>
        /// 处理脏污缺陷色差
        /// 20260424 鲍赞宝
        /// </summary>
        /// <param name="detData"></param>
        /// <param name="detName"></param>
        public float DirtyDefetOperration(Mat img, DetData detData)
        {
            int derArea = detData.box.Width * detData.box.Height;
            if (derArea <= 1000)
            {
                int recx = detData.box.X;
                int recy = detData.box.Y;

                int recw2 = detData.box.Width * 2;
                int rech2 = detData.box.Height * 2;
                //先平移范围

                int newrecx = detData.box.X + detData.box.Width + 10;

                if ((newrecx + recw2) >= img.Width)
                    newrecx = img.Width - recw2;
                if (newrecx < 0) newrecx = 1;

                if ((recy + rech2) > img.Height)
                    recy = img.Height - rech2;
                if (recy < 0) recy = 1;

                //截取附近的区域
                Mat cropMat = img[new Rect(newrecx, recy, recw2, rech2)];
                //Scalar meanValues = Cv2.Mean(cropMat);
                //double Bvalue = meanValues.Val0;
                //double Gvalue = meanValues.Val1;
                //double Rvalue = meanValues.Val2;

                Mat hsvImage = new Mat();
                Cv2.CvtColor(cropMat, hsvImage, ColorConversionCodes.BGR2HSV);
                Scalar hsvMean = Cv2.Mean(hsvImage);
                double Hvalue = hsvMean.Val0;
                double Svalue = hsvMean.Val1;
                double Vvalue = hsvMean.Val2;

                //原来缺陷的区域缩小一半
                int recx2 = recx + (int)(detData.box.Width * 0.25);// + recw/2;
                int recy2 = recy + (int)(detData.box.Height * 0.25);// + rech/2;
                int recw = (int)(detData.box.Width * 0.75);
                int rech = (int)(detData.box.Height * 0.75);


                if ((recx2 + recw) > img.Width) recx2 = img.Width - recw;
                if (recx2 < 0) recx2 = 1;

                if ((recy2 + rech) > img.Height) recy2 = img.Height - rech;
                if (rech2 < 0) rech2 = 1;


                Mat orgcropMat = img[new Rect(recx2, recy2, recw, rech)];
                //Scalar orgmeanValues = Cv2.Mean(orgcropMat);
                //double orgBvalue = orgmeanValues.Val0;
                //double orgGvalue = orgmeanValues.Val1;
                //double orgRvalue = orgmeanValues.Val2;

                Mat orghsvImage = new Mat();
                Cv2.CvtColor(orgcropMat, orghsvImage, ColorConversionCodes.BGR2HSV);
                Scalar orghsvMean = Cv2.Mean(orghsvImage);
                double orgHvalue = orghsvMean.Val0;
                double orgSvalue = orghsvMean.Val1;
                double orgVvalue = orghsvMean.Val2;

                var (l1, a1, b1Lab) = BgrToLab((byte)orgHvalue, (byte)orgSvalue, (byte)orgVvalue);
                var (l2, a2, b2Lab) = BgrToLab((byte)Hvalue, (byte)Svalue, (byte)Vvalue);
                double de00 = CalculateDeltaE76(l1, a1, b1Lab, l2, a2, b2Lab);
                return (float)de00;
            }
            else
            {
                return 1;
            }



        }

        #endregion

        private void UpdateScore(CParam param)
        {
            WH_Cloth_det1?.UpdateNMS_Score(param.Nms, param.CommonScore);
            WH_Cloth_det2?.UpdateNMS_Score(param.Nms, param.CommonScore);
            WH_Tooth_det1?.UpdateNMS_Score(param.Nms, param.CommonScore);
            WH_Tooth_det2?.UpdateNMS_Score(param.Nms, param.CommonScore);
            //if (WH_UpStopMassDefe_det != null)
            //{
            //    WH_UpStopMassDefe_det.UpdateNMS_Score(param.Nms, param.UpScore);
            //}
            //if (WH_UpStopMassMeas_obb != null)
            //{
            //    WH_UpStopMassMeas_obb.UpdateNMS_Score(param.Nms, param.UpLianciScore);
            //}
            if (WH_DownStopMass_obb != null)
            {
                WH_DownStopMass_obb.UpdateNMS_Score(param.Nms, param.DownScore);
            }
            ////WH_pull_Serach_det.UpdateNMS_Score(param.PullScore, param.Nms);
            //WH_Meta_pull_det?.UpdateNMS_Score(param.Nms, param.MetaPullScore);
            //WH_Paint_pull_det?.UpdateNMS_Score(param.Nms, param.PaintPullScore);
            //// WH_Logo_pull_det.UpdateNMS_Score(0.8f, param.LogoPullScore);

            WH_BigDet_det?.UpdateNMS_Score(param.Nms, param.BigScore);
            //WH_pull_Serach_det?.UpdateNMS_Score(param.Nms, param.AutoScore);
            //WH_PullShape_Seg?.UpdateNMS_Score(param.Nms, param.PullSharpScore);
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
        /// <param name="imgWidth">图像宽</param>
        /// <param name="imgIndex">图像序号</param>
        /// <param name="orgx">缺陷坐标x</param>
        /// <param name="orgy">缺陷坐标y</param>
        /// <param name="labelstr">缺陷名称</param>
        /// <param name="det">缺陷对象</param>
        /// <param name="showinview">在那个窗口显示</param>
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
        /// <summary>
        /// 坐标还原 
        /// 20260424 鲍赞宝
        /// </summary>
        /// <param name="imgWidth">图像宽</param>
        /// <param name="imgIndex">图像序号</param>
        /// <param name="orgx">缺陷坐标x</param>
        /// <param name="orgy">缺陷坐标y</param>
        /// <param name="labelstr">缺陷名称</param>
        /// <param name="det">缺陷对象</param>
        /// <param name="colordiffValue">色差值</param>
        /// <param name="showinview">在那个窗口显示</param>
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, DetData det, float colordiffValue, int showinview = 0)
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
            Value = colordiffValue;
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
        /// <summary>
        /// 坐标还原
        /// </summary>
        /// <param name="labelstr">缺陷名称</param>
        /// <param name="value">值大小</param>
        /// <param name="showinview">在那个窗口显示</param>
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
