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
using System.DirectoryServices;
using OpenCvSharp.Dnn;
using System.Windows.Input;

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

        /// <summary>
        /// 正面下止检测对象
        /// </summary>
        private YOLO yolo_DownStopMass_obb = new YOLO();

        /// <summary>
        /// 正面上止检测对象
        /// </summary>
        private YOLO yolo_UpStopMass_obb = new YOLO();

        /// <summary>
        /// 正面上止检测对象
        /// </summary>
        private YOLO yolo_pull_det = new YOLO();

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
            string modelDirPath = ".\\AlgorithmPlug\\ZipperTestAlgorihm\\Models\\";

            ReadNames(modelDirPath);

            if (Common_names?.Length > 0)
            {
                List<CDefectRecipe> cDefectRecipes = new List<CDefectRecipe>();
                DefectSpecies = new List<CDefectSpecies>();

                for (int i = 0; i < Common_names.Length; i++)
                {
                    CDefectRecipe defectRecipe = new CDefectRecipe(Common_names[i], Category.区域);
                    cDefectRecipes.Add(defectRecipe);
                }
                CDefectRecipe defectRecipe0 = new CDefectRecipe("上止压伤", Category.区域);
                cDefectRecipes.Add(defectRecipe0);
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

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 通用模型
        /// </summary>
        private string Common_Model_Path;
        /// <summary>
        /// 下止模型路径
        /// </summary>
        private string downStopMass_Model_Path;
        /// <summary>
        /// 上止模型路径
        /// </summary>
        private string upStopMass_Model_Path;
        /// <summary>
        /// 拉头
        /// </summary>
        private string pull_Model_Path;

        /// <summary>
        /// 2025.3.3 鲍赞宝
        /// 缺陷名称路径
        /// </summary>
        private string name_Path;
        //常规缺陷名称
        protected string[] Common_names;
        //上止缺陷名称
        protected string[] upStopMass_names;
        //下止缺陷名称
        protected string[] downStopMass_names;

        //拉头拉片缺陷名称
        protected string[] pull_names;


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


            string commonModelPath = modelDirpath + "CommonModel\\";
            var commons = GetNames(commonModelPath);
            if (commons.Item1 != "")
            {
                Common_Model_Path = commons.Item1;
                Common_names = commons.Item2;
            }
            string downStopMassPath = modelDirpath + "DownStopMassModel\\";
            var downstopstrs = GetNames(downStopMassPath);
            if (downstopstrs.Item1 != "")
            {
                downStopMass_Model_Path = downstopstrs.Item1;
                downStopMass_names = downstopstrs.Item2;
            }
            string upStopMassModelPath = modelDirpath + "UpStopMassModel\\";
            var upstopstrs = GetNames(upStopMassModelPath);
            if (upstopstrs.Item1 != "")
            {
                upStopMass_Model_Path = upstopstrs.Item1;
                upStopMass_names = upstopstrs.Item2;
            }

            string pullModelPath = modelDirpath + "PullModel\\";
            var pulltrs = GetNames(pullModelPath);
            if (pulltrs.Item1 != "")
            {
                pull_Model_Path = pulltrs.Item1;
                pull_names = pulltrs.Item2;
            }

        }


        private (string, string[]) GetNames(string Dirpath)
        {
            if (Directory.Exists(Dirpath))
            {
                string[] searchPatterns = { "*.onnx", "*.engine", "*.pt", "*.xml" };
                var files = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(Dirpath, pattern))
                .ToList();

                if (files.Count > 0)
                {
                    string model_Path = files[0];
                    string name_Path = Dirpath + "\\classes.txt";
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
                            string labelname = Common_names[labelindex];
                            if (labelname.Contains("正面下止"))
                            {
                                //坐标还原
                                int nameindex = int.Parse(detrets[i].datas[j].lable);
                                string labelstr = Common_names[nameindex];
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, 0, i * smallimgWidth, 0, labelstr, detrets[i].datas[j]);
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
                               // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面下止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropDownMat);
                                ObbResult downResult = ImageInferObb(yolo_DownStopMass_obb, cropDownMat, paramClass.Score, paramClass.Nms);
                                if (downResult.datas.Count > 0)
                                {
                                    List<ObbData> downmass = downResult.datas.FindAll(c => c.lable == "0").ToList();
                                    List<ObbData> lianci = downResult.datas.FindAll(c => c.lable == "1").ToList();
                                    List<ObbData> lianya = downResult.datas.FindAll(c => c.lable == "2").ToList();
                                    //计算下止到链齿的最短距离
                                    List<(float, int)> Diss = new List<(float, int)>();
                                    for (int a = 0; a < downmass.Count; a++)
                                    {
                                        for (int b = 0; b < lianci.Count; b++)
                                        {
                                            float dis = CalculateDistance(downmass[a], lianci[b]);
                                            Diss.Add((dis, b));
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
                                    List<(float, int)> Angs = new List<(float, int)>();
                                    for (int a = 0; a < downmass.Count; a++)
                                    {
                                        for (int b = 0; b < lianya.Count; b++)
                                        {
                                            float an = downmass[a].box.Angle - lianya[b].box.Angle;
                                            Angs.Add((an, b));
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
                                }
                            }
                            else
                            {
                                int nameindex = int.Parse(detrets[i].datas[j].lable);
                                string labelstr = Common_names[nameindex];
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex-1, i * smallimgWidth, 0, labelstr, detrets[i].datas[j]);
                                dets.Add(restoreData);
                            }
                        }
                    }


                }
                else if (cell.PhotoIndex == cell.PhotoTatolCount) //最后一张图片有上止图片
                {
                    int instr = 0;
                    List<Point> massPoints = new List<Point>(); //上止的位置
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].datas[j].lable);
                            string labelname = Common_names[labelindex];
                            if (labelname.Contains("正面上止"))
                            {
                                //坐标还原
                                int nameindex = int.Parse(detrets[i].datas[j].lable);
                                string labelstr = Common_names[nameindex];
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelstr, detrets[i].datas[j]);
                                massPoints.Add(new Point(restoreData.OrgCenterX, restoreData.OrgCenterY));
                                int recw = 192;
                                int rech = 96;
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
                                dets.Add(restoreData);
                                Mat cropUpMat = img[new Rect(rex, rey, recw, rech)];
                                ObbResult upResult = ImageInferObb(yolo_UpStopMass_obb, cropUpMat, paramClass.Score, paramClass.Nms);
                                if (upResult.datas.Count > 0)
                                {
                                  
                                    List<ObbData> upmass = upResult.datas.FindAll(c => c.lable == "0").ToList();
                                    List<ObbData> lianci = upResult.datas.FindAll(c => c.lable == "2").ToList();
                                    List<ObbData> yashang = upResult.datas.FindAll(c => c.lable == "1").ToList();
                                    //计算上止到链齿的最短距离
                                    List<(float, int)> Diss = new List<(float, int)>();
                                    for (int a = 0; a < upmass.Count; a++)
                                    {
                                        for (int b = 0; b < lianci.Count; b++)
                                        {
                                            float dis = CalculateDistance(upmass[a], lianci[b]);
                                            Diss.Add((dis, b));
                                        }
                                    }
                                    if (Diss.Count > 0)
                                    {
                                        instr++;
                                        var min = Diss.Min(t => t.Item1);
                                        var dis = Diss.First(t => t.Item1 == min);
                                        CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, rex, rey, $"上止距离{instr}", lianci[dis.Item2]);
                                        disData.Value = dis.Item1;
                                        dets.Add(disData);
                                        Diss.Clear();
                                    }

                                    for (int k = 0;k < yashang.Count; k++)
                                    {
                                        CoordRestoreData disData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, rex, rey, "上止压伤", yashang[k]);
                                        dets.Add(disData);
                                    }
                                }
                                if (massPoints.Count==2)
                                {
                                    float massdis =Math.Abs( massPoints[0].X - massPoints[1].X); //临时这样写
                                    CoordRestoreData disData = new CoordRestoreData("上止高低", massdis);
                                    dets.Add(disData);
                                    massPoints.Clear();
                                }
                                    
                                // Cv2.ImWrite(@"C:\Users\Administrator.B\Desktop\正面上止\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff_") + i + ".png", cropUpMat);

                            }
                            else
                            {
                                int nameindex = int.Parse(detrets[i].datas[j].lable);
                                string labelstr = Common_names[nameindex];
                                CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelstr, detrets[i].datas[j]);
                                dets.Add(restoreData);
                            }
                        }
                    }
                }
                else if (cell.PhotoIndex == 100) //有拉头的图片
                {
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int labelindex = int.Parse(detrets[i].datas[j].lable);
                            string labelname = pull_names[labelindex];
                            if (labelname.Contains("拉头"))
                            {
                                int lx = detrets[i].datas[j].box.X + detrets[i].datas[j].box.Width / 2 - 240;
                                int ly = detrets[i].datas[j].box.Y + detrets[i].datas[j].box.Height / 2-240;
                               int recw = 480;
                                int rech = 480;

                                if ((lx + recw) > mats[i].Width)
                                {
                                    lx = mats[i].Width - recw;
                                }
                                if (lx < 0)
                                {
                                    lx = 0;
                                }

                                if ((ly + rech) > mats[i].Height)
                                {
                                    ly = mats[i].Height - rech;
                                }
                                if (ly < 0)
                                {
                                    ly = 0;
                                }

                                Mat croppullMat = img[new Rect(lx, ly, recw, rech)];
                                cell.ZipperPullPartImg = Mat2BitmapSource(croppullMat);

                            }


                        }
                    }
                }
                else //中间布带，链牙缺陷
                {
                    List<DetResult> detrets = ImageInferall(mats, paramClass.Score, paramClass.Nms).Result;
                    for (int i = 0; i < detrets.Count; i++)
                    {
                        for (int j = 0; j < detrets[i].datas.Count; j++)
                        {
                            int nameindex = int.Parse(detrets[i].datas[j].lable);
                            string labelstr = Common_names[nameindex];
                            CoordRestoreData restoreData = new CoordRestoreData(cell.Image.ImageWidth, cell.PhotoIndex - 1, i * smallimgWidth, 0, labelstr, detrets[i].datas[j]);
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
                DetResult sResultInfos = ImageInferDet(yolo_all_det1, mats[0], score, nms);
                return sResultInfos;
            });

            Task<DetResult> task2 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det2, mats[1], score, nms);
                return sResultInfos;
            });
            Task<DetResult> task3 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det3, mats[2], score, nms);
                return sResultInfos;
            });
            Task<DetResult> task4 = Task.Run(() =>
            {
                DetResult sResultInfos = ImageInferDet(yolo_all_det4, mats[3], score, nms);
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
                            if (de.Category == Category.值)
                            {
                                List<System.Windows.Point> rec1MarkPoints = new List<System.Windows.Point>();
                                rec1MarkPoints.Add(item.ShowLeftUp);
                                rec1MarkPoints.Add(item.ShowRightUp);
                                rec1MarkPoints.Add(item.ShowRightDown);
                                rec1MarkPoints.Add(item.ShowLeftDown);
                                rec1MarkPoints.Add(item.ShowLeftUp);
                                cell.DrawEdges.Add(new CEdgeDraw(rec1MarkPoints, Brushes.Pink));
                            }
                         
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

            ModelType model_type_det = ModelType.YOLOv8Det;
            ModelType model_type_obb = ModelType.YOLOv8Obb;
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
                int common_Categ_num = Common_names.Length;
                int downmass_num = downStopMass_names.Length;
                int upmass_num = upStopMass_names.Length;
                int pull_num = pull_names.Length;
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
                    model_type_det,
                    Common_Model_Path,
                    engine_type,
                    CurrentDevice,
                    common_Categ_num,
                    Score,
                    Nms,
                    Input_size
                );
                yolo_all_det2 = YOLO.GetYolo(
                model_type_det,
                Common_Model_Path,
                engine_type,
                CurrentDevice,
                common_Categ_num,
                Score,
                Nms,
                Input_size
            );
                yolo_all_det3 = YOLO.GetYolo(
                model_type_det,
                Common_Model_Path,
                engine_type,
                CurrentDevice,
                common_Categ_num,
                Score,
                Nms,
                Input_size
            );
                yolo_all_det4 = YOLO.GetYolo(
                model_type_det,
                Common_Model_Path,
                engine_type,
                CurrentDevice,
                common_Categ_num,
                Score,
                Nms,
                Input_size
            );
                yolo_DownStopMass_obb = YOLO.GetYolo(
                    model_type_obb,
                    downStopMass_Model_Path,
                    engine_type,
                    CurrentDevice,
                    downmass_num,
                    Score,
                    Nms,
                    InputImgSize.IN256
                    );
                yolo_UpStopMass_obb = YOLO.GetYolo(
                    model_type_obb,
                    upStopMass_Model_Path,
                    engine_type,
                    CurrentDevice,
                    upmass_num,
                    Score,
                    Nms,
                    InputImgSize.IN192
                    );
                yolo_pull_det = YOLO.GetYolo(
                  model_type_det,
                  pull_Model_Path,
                  engine_type,
                  CurrentDevice,
                  pull_num,
                  Score,
                  Nms,
                  Input_size
              );
            }
        }

        public DetResult ImageInferDet(YOLO yolo, Mat img, float score, float nms)
        {
            DetResult resultDet;
            resultDet = yolo.predict(img, score, nms) as DetResult;
            return resultDet;
        }
        public ObbResult ImageInferObb(YOLO yolo, Mat img, float score, float nms)
        {
            ObbResult resultDet;
            resultDet = yolo.predict(img, score, nms) as ObbResult;
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
            float deltaY = point1.box.Center.Y - point2.box.Center.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, DetData det)
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
            Score = det.score;
            Labelstr = labelstr;
            Angle = 0.0f;
            Value = 0.0f;

        }
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy, string labelstr, ObbData obb)
        {
            //坐标还原 

            ShowLeftUp.X = obb.box.Points()[0].X + orgx+imgWidth * imgIndex;
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
            Score = obb.score;
            Labelstr = labelstr;
            Angle = obb.box.Angle;
            Value = 0.0f;
        }
        public CoordRestoreData(string labelstr,float value)
        {
            //坐标还原 

            ShowLeftUp.X = 0;
            ShowLeftUp.Y = 0;

            ShowRightUp.X =0;
            ShowRightUp.Y = 0;

            ShowRightDown.X = 0;
            ShowRightDown.Y = 0;

            ShowLeftDown.X = 0;
            ShowLeftDown.Y = 0;

            RecWidth = 0;
            RecHeight = 0;
            OrgCenterX = 0;
            OrgCenterY =0;
            Score =0;
            Labelstr = labelstr;
            Angle = 0;
            Value = value;
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
    }

}
