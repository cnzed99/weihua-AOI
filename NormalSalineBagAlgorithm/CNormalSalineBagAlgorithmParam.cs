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
using YoloobbAlgorithm;
using System.Windows.Media;

namespace NormalSalineBagAlgorithm
{
    public class CNormalSalineBagAlgorithmParam : CYoloAlgorithmParam
    {

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        public CNormalSalineBagAlgorithmParam()
            : base()
        {
            string modelDirPath = ".\\AlgorithmPlug\\NormalSalineBagAlgorithm\\Models";
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
                //CDefectRecipe defectRecipe1 = new CDefectRecipe("分数", Category.值);
                //cDefectRecipes.Add(defectRecipe1);

                CDefectSpecies defectSpecies = new CDefectSpecies("盐水袋", cDefectRecipes);
                DefectSpecies.Add(defectSpecies);
            }

            DefectFeatures = new();

            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
        }


        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CNormalSalineBagParam(name, token));
        }

        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 执行算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            var paramClass = AlgorParams.FirstOrDefault(o => o.Name == ParamSelect) as CNormalSalineBagParam;
            if (paramClass != null)
            {              
                if (paramClass.RecMarkEnable)
                {
                    List<System.Windows.Point> rec1MarkPoints = new List<System.Windows.Point>();
                    rec1MarkPoints.Add(new System.Windows.Point(paramClass.TopLeftX, paramClass.TopLeftY));
                    rec1MarkPoints.Add(new System.Windows.Point(paramClass.BottomRightX, paramClass.TopLeftY));
                    rec1MarkPoints.Add(new System.Windows.Point(paramClass.BottomRightX, paramClass.BottomRightY));
                    rec1MarkPoints.Add(new System.Windows.Point(paramClass.TopLeftX, paramClass.BottomRightY));
                    rec1MarkPoints.Add(new System.Windows.Point(paramClass.TopLeftX, paramClass.TopLeftY));
                    cell.DrawEdges.Add(new CEdgeDraw(rec1MarkPoints, Brushes.Pink));
                }
                Mat img = GetMatImage(cell, paramClass);
                List<ObbData> baseResultInfos = ImageInfer(img, paramClass.Score, paramClass.Nms);
                if (baseResultInfos.Count == 0) { return; }

                //去掉重叠的框

                List<ObbData> sResultInfos = new List<ObbData>();
                if (baseResultInfos != null)
                {
                    if (baseResultInfos.Count > 0)
                    {
                        sResultInfos.Add(baseResultInfos[0]);

                        for (int i = 0; i < baseResultInfos.Count; i++)
                        {
                            bool isOverlapping = false;

                            foreach (var coord in sResultInfos)
                            {
                                double distance = CalculateDistance(baseResultInfos[i], coord);
                                if (distance < paramClass.OverlapDis)
                                {
                                    isOverlapping = true;
                                    break; // 找到重叠则不再检查其他坐标  
                                }
                            }
                            // 如果未重叠，则加入到 uniqueCoordinates  
                            if (!isOverlapping)
                            {
                                sResultInfos.Add(baseResultInfos[i]);
                            }

                        }
                    }
                }

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

        public override Mat GetMatImage(Cell cell, CParamBase param)
        {
            Mat mat = base.GetMatImage(cell, param);
            CNormalSalineBagParam bagparam = param as CNormalSalineBagParam;
            if (bagparam != null)
            {
                if (bagparam.RecMarkEnable)
                {
                    int width = bagparam.BottomRightX - bagparam.TopLeftX;
                    int height = bagparam.BottomRightY - bagparam.TopLeftY;
                    Rect roi = new Rect(bagparam.TopLeftX, bagparam.TopLeftY, width, height);
                    Mat reduceimg = ReduceDomain(mat, roi);
                    return reduceimg;
                }
                else
                {
                    return mat;
                }
            }
            else { return mat; }
        }

        private Mat ReduceDomain(Mat src, Rect roi)
        {
            // 创建与原图尺寸相同的全黑背景图像
            Mat dst = new Mat(src.Size(), src.Type(), Scalar.Black);

            // 检查 ROI 是否在图像范围内
            Rect safeRoi = new Rect(
                X: Math.Max(roi.X, 0),
                Y: Math.Max(roi.Y, 0),
                Width: Math.Min(roi.Width, src.Width - roi.X),
                Height: Math.Min(roi.Height, src.Height - roi.Y)
            );

            if (safeRoi.Width > 0 && safeRoi.Height > 0)
            {
                // 提取原图 ROI 区域
                Mat srcRoi = new Mat(src, safeRoi);

                // 将 ROI 复制到目标图像的对应位置
                Mat dstRoi = new Mat(dst, safeRoi);
                srcRoi.CopyTo(dstRoi);
                srcRoi.Dispose();
                dstRoi.Dispose();
            }
            return dst;
        }

    }

    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// AI参数类
    /// </summary>
    public partial class CNormalSalineBagParam : CParam
    {
        public CNormalSalineBagParam()
            : base() { }

        public CNormalSalineBagParam(string name, Token token)
         : base(name, token) { }

        /// <summary>
        /// 2025.3.25 鲍赞宝
        /// 驱动设备
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("01.重叠距离")]
        [property: Description("重叠距离")]
        private float overlapDis = 100.0f;


        /// <summary>
        /// 2024.3.28 鲍赞宝
        ///是否开启
        /// </summary>
        [ObservableProperty]
        [property: Category("搜索区域")]
        [property: DisplayName("01.开启搜索框")]
        [property: Description("开启搜索框")]
        private bool recMarkEnable = false;

        /// <summary>
        /// 2024.3.28 鲍赞宝
        /// 搜索框左上角坐标X
        /// </summary>
        [ObservableProperty]
        [property: Category("搜索区域")]
        [property: DisplayName("02.左上角坐标X")]
        [property: Description("左上角坐标X")]
        private int topLeftX = 0;

        /// <summary>
        /// 2024.3.28 鲍赞宝
        /// 搜索框左上角坐标Y
        /// </summary>
        [ObservableProperty]
        [property: Category("搜索区域")]
        [property: DisplayName("03.左上角坐标Y")]
        [property: Description("左上角坐标Y")]
        private int topLeftY = 0;

        /// <summary>
        /// 2024.3.28 鲍赞宝
        /// 搜索框右下角坐标X
        /// </summary>
        [ObservableProperty]
        [property: Category("搜索区域")]
        [property: DisplayName("04.右下角坐标X")]
        [property: Description("右下角坐标X")]
        private int bottomRightX = 0;

        /// <summary>
        /// 2024.3.28 鲍赞宝
        /// 搜索框右下角坐标Y
        /// </summary>
        [ObservableProperty]
        [property: Category("搜索区域")]
        [property: DisplayName("05.右下角坐标Y")]
        [property: Description("右下角坐标Y")]
        private int bottomRightY = 0;

    }



}
