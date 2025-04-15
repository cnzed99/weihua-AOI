
using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenVinoSharp.Extensions.result;
using System.DirectoryServices;
using System.Windows.Controls.Primitives;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using YoloDeployPlatform.tracker;
using YoloobbAlgorithm;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.Serialization;
using YoloDeployPlatform.Bytetrack;
using System.Text;

namespace CountTrackAlgorithm
{
    public class CountTrackAlgorithm : CYoloAlgorithmParam
    {

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 算法参数派生类
        /// </summary>
        /// 

        private ByteTracker tracker;
        public CountTrackAlgorithm()
            : base()
        {
            string modelDirPath = ".\\AlgorithmPlug\\CountTrackAlgorithm\\Models";
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
                CDefectRecipe defectRecipe1 = new CDefectRecipe("数值", Category.值);
                cDefectRecipes.Add(defectRecipe1);

                //CDefectRecipe defectRecipe2 = new CDefectRecipe("ID", Category.值);
                //cDefectRecipes.Add(defectRecipe2);

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
        /// 2025.4.1 鲍赞宝
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CCountTrackParam(name, token));
        }


        [OnDeserialized]
        void IniTarck(StreamingContext context)
        {
            try
            {
                var paramClass = AlgorParams.FirstOrDefault(o => o.Name == ParamSelect) as CCountTrackParam;
                Point start = new Point(paramClass.StartX, paramClass.StartY);
                Point end = new Point(paramClass.EndX, paramClass.EndY);
                tracker = new ByteTracker(new CountingLine(start, end), paramClass.MaxTimeLost, paramClass.IouThreshold,paramClass.Direction);
            }
            catch (Exception)
            {
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
            var paramClass = AlgorParams.FirstOrDefault(o => o.Name == ParamSelect) as CCountTrackParam;
            if (paramClass != null)
            {
                Mat img = GetMatImage(cell, paramClass);
                List<ObbData> baseResultInfos = ImageInfer(img, paramClass.Score, paramClass.Nms);

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

                // var tracks = tracker.Update(sResultInfos, paramClass.MaxAge, paramClass.IouThreshold,paramClass.InflateW,paramClass.InflateH, out int corssCount, MinHits:paramClass.MinHits, iteratorDis: paramClass.IteratorDis);
                var tracks = tracker.Update(sResultInfos, paramClass.MaxTimeLost, paramClass.IouThreshold, paramClass.TrackHighThreshold,
                    paramClass.TrackLowThreshold,  paramClass.MinHits, paramClass.IteratorDis, paramClass.VelocityX,paramClass.InflateW,paramClass.InflateH, out int corssCount);
                foreach (var track in tracks)
                {
                    List<System.Windows.Point> pathPoints = new List<System.Windows.Point>();
                    //List<Point> path = track.PathHistory;

                    foreach (var p in track.PathHistory)
                    {
                        pathPoints.Add(new System.Windows.Point(p.X, p.Y));
                    }
                    cell.DrawEdges.Add(new CEdgeDraw(pathPoints, Brushes.Red));

                    //float w = (track.Bbox[2] * track.Bbox[3]) / 2f;
                    //float h = track.Bbox[3] / 2f;

                    //float topLeftX = track.Bbox[0] - w;
                    //float topLeftY = track.Bbox[1] - h;
                    //float bottomRightX = track.Bbox[0] + w;
                    //float bottomRightY = track.Bbox[1] + h;
                    // 追踪的正矩形框
                    List<System.Windows.Point> rec1MarkPoints = new List<System.Windows.Point>();
                    rec1MarkPoints.Add(new System.Windows.Point(track.PredictedBox.X, track.PredictedBox.Y));
                    rec1MarkPoints.Add(new System.Windows.Point(track.PredictedBox.Right, track.PredictedBox.Y));
                    rec1MarkPoints.Add(new System.Windows.Point(track.PredictedBox.Right, track.PredictedBox.Bottom));
                    rec1MarkPoints.Add(new System.Windows.Point(track.PredictedBox.X, track.PredictedBox.Bottom));
                    rec1MarkPoints.Add(new System.Windows.Point(track.PredictedBox.X, track.PredictedBox.Y));
                    cell.DrawEdges.Add(new CEdgeDraw(rec1MarkPoints, Brushes.Pink));

                    StringBuilder otherInfobuilder = new StringBuilder($"TrackId:{track.TrackId}\r");
                    otherInfobuilder.Append($"Iou:{track.Iou}\r");
                    //otherInfobuilder.Append($"Age:{track.Age} \r");
                    //otherInfobuilder.Append($"TimeSinceUpdate:{track.TimeSinceUpdate} \r");
                    //otherInfobuilder.Append($"Hits:{track.Hits}\r");
                    otherInfobuilder.Append($"VX:{track.Filter.mean.VX}\r");
                    otherInfobuilder.Append($"VY:{track.Filter.mean.VY}\r");
                    OtherInfo otherInfo=new OtherInfo(otherInfobuilder, new System.Windows.Point(track.PredictedBox.Right, track.PredictedBox.Bottom), Brushes.LightCyan);

                    cell.ShowOtherInfos.Add(otherInfo);
                }
                //计数线显示
                List<System.Windows.Point> countLinePoints = new List<System.Windows.Point>();
                countLinePoints.Add(new System.Windows.Point(paramClass.StartX, paramClass.StartY));
                countLinePoints.Add(new System.Windows.Point(paramClass.EndX, paramClass.EndY));
                cell.DrawEdges.Add(new CEdgeDraw(countLinePoints, Brushes.LightGreen));

                foreach (var ds in DefectSpecies)
                {
                    foreach (var de in ds.RecipeDefects)
                    {
                        CellDetection cellDetection1 = new CellDetection();
                        cellDetection1.Type = ds.Name;
                        cellDetection1.Category = de.Category;
                        cellDetection1.RecipeDefectName = de.Name;
                       
                        if (de.Category == Category.值)
                        {
                            cellDetection1.Value=new List<float>() { corssCount };
                        }
                        else 
                        { 
                            cellDetection1.Value = new List<float>();
                        }
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
        /// 计算点距离
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

        private SRegion GetDetectRegion2(System.Windows.Point p1, System.Windows.Point p2, double w, double h)
        {
            SRegionInfo sRegioninfo = new SRegionInfo();



            sRegioninfo.LongLen = w;
            sRegioninfo.ShorLen = h;
            sRegioninfo.Phi = 0;
            sRegioninfo.Area = sRegioninfo.LongLen * sRegioninfo.ShorLen;
            sRegioninfo.Score = 1;
            List<System.Windows.Point> rec1Points = new List<System.Windows.Point>();
            //info.box.Points().ForEach(p => rec1Points.Add(new System.Windows.Point(p.X, p.Y)));
            rec1Points.Add(p1);
            rec1Points.Add(p2);

            SRegion detectRegion = new SRegion(sRegioninfo, rec1Points);
            //var rect = info.box.BoundingRect();

            detectRegion.rect = new System.Windows.Rect(new System.Windows.Point(p1.X, p1.Y), new System.Windows.Size(w, h));
            return detectRegion;
        }
    }


    /// <summary>
    /// 2024.10.28 鲍赞宝
    /// AI参数类
    /// </summary>
    public partial class CCountTrackParam : CParam
    {
        public CCountTrackParam()
            : base() { }

        public CCountTrackParam(string name, Token token)
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
        /// 2025.4.1 鲍赞宝
        /// 连续10帧找不到目标时丢弃该目标
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("02.MaxTimeLost")]
        [property: Description("连续几帧找不到目标时丢弃该目标")]
        private int maxTimeLost = 10;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("03.iouThreshold")]
        [property: Description("iouThreshold")]
        private float iouThreshold = 0.3f;


        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 运动方向
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("04.运动方向")]
        [property: Description("运动方向")]
        private CrossDirection direction = CrossDirection.RightToLeft;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 代数距离
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("05.IteratorDis")]
        [property: Description("代数距离")]
        private int iteratorDis = 5;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 最小击中次数
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("06.MinHits")]
        [property: Description("MinHits")]
        private int minHits = 3;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 膨胀宽
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("07.宽度膨胀")]
        [property: Description("宽度膨胀")]
        private int inflateW = 100;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 膨胀宽
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("08.高度膨胀")]
        [property: Description("高度膨胀")]
        private int inflateH = 100;


        /// <summary>
        /// 2025.4.11 鲍赞宝
        /// 高分检测框阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("09.TrackHighThreshold")]
        [property: Description("高分检测框阈值")]
        private float trackHighThreshold = 0.6f;

        /// <summary>
        /// 2025.4.11 鲍赞宝
        /// 低分检测框阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("10.TrackLowThreshold")]
        [property: Description("低分检测框阈值")]
        private float trackLowThreshold  = 0.1f;

        /// <summary>
        /// 2025.4.11 鲍赞宝
        /// 速度
        /// </summary>
        [ObservableProperty]
        [property: Category("算法参数")]
        [property: DisplayName("11.VX")]
        [property: Description("速度")]
        private float velocityX = -70.0f;



        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 计数线起点X
        /// </summary>
        [ObservableProperty]
        [property: Category("计数线")]
        [property: DisplayName("01.起点X")]
        [property: Description("起点X")]
        private int startX = 0;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 计数线起点Y
        /// </summary>
        [ObservableProperty]
        [property: Category("计数线")]
        [property: DisplayName("02.起点Y")]
        [property: Description("起点Y")]
        private int startY = 0;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 计数线终点X
        /// </summary>
        [ObservableProperty]
        [property: Category("计数线")]
        [property: DisplayName("03.终点X")]
        [property: Description("终点X")]
        private int endX = 0;

        /// <summary>
        /// 2025.4.1 鲍赞宝
        /// 计数线终点Y
        /// </summary>
        [ObservableProperty]
        [property: Category("计数线")]
        [property: DisplayName("04.终点Y")]
        [property: Description("终点Y")]
        private int endY = 0;



    }
}
