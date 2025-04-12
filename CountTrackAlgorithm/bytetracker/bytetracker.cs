using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenVinoSharp.Extensions.result;
using YoloDeployPlatform.tracker;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Point = OpenCvSharp.Point;

namespace YoloDeployPlatform.Bytetrack
{
    // 1. 定义数据结构和常量
    public record BoundingBox(float X, float Y, float Width, float Height, float Score, int ClassId)
    {
        public float Left => X;
        public float Top => Y;
        public float Right => X + Width;
        public float Bottom => Y + Height;
        public float Vx, Vy;
        public static BoundingBox FromXYXY(
            float x1,
            float y1,
            float x2,
            float y2,
            float score,
            int classId
        ) => new(x1, y1, x2 - x1, y2 - y1, score, classId);

        public static List<BoundingBox> odd2BoundingBox(ObbResult obbs)
        {
            List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            foreach (var item in obbs.datas)
            {
                var rect = item.box.BoundingRect();
                boundingBoxes.Add(
                    new BoundingBox(rect.X, rect.Y, rect.Width, rect.Height, item.score, item.index)
                );
            }
            return boundingBoxes;
        }
        public static List<BoundingBox> odd2BoundingBox(List<ObbData> obbs)
        {
            List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            foreach (var item in obbs)
            {
                var rect = item.box.BoundingRect();
                boundingBoxes.Add(
                    new BoundingBox(rect.X, rect.Y, rect.Width, rect.Height, item.score, item.index)
                );
            }
            return boundingBoxes;
        }
    }

    public class TrackState
    {
        public int TrackId { get; set; }
        public KalmanFilter Filter { get; set; }
        public int Age { get; set; } = 1;
        public int TimeSinceUpdate { get; set; } = 0;
        public BoundingBox PredictedBox { get; set; }
        public float Score { get; set; }
        public int ClassId { get; set; }
        public float Iou { get; set; }
        public bool IsConfirmed => Hits >= MinHits;
        public List<Point> PathHistory { get; } = new List<Point>();

        public int Hits { get; set; }
        private  int MinHits = 3;
        private int MaxPathHistory = 20;

        public void update(BoundingBox det, float iou,int minHits)
        {
            Filter.Update(det);
            Score = det.Score;
            ClassId = det.ClassId;
            TimeSinceUpdate = 0;
            Iou = iou;
            MinHits = minHits;
            var center = new Point(
                PredictedBox.X + PredictedBox.Width / 2.0f,
                PredictedBox.Y + PredictedBox.Height / 2.0f
            );
            PathHistory.Add(center);

            // 限制历史记录长度以节省内存
            if (PathHistory.Count > MaxPathHistory)
                PathHistory.RemoveAt(0);
            Hits++;
            Age++;
        }
    }

    public class KalmanFilter
    {
        public Vector8 mean; // x, y, w, h, vx, vy, vw, vh
        private Matrix8x8 covariance;

        private readonly Matrix8x8 motionMat;
        private readonly Matrix4x8 updateMat;
        private readonly float stdWeightPosition = 1.0f / 20;
        private readonly float stdWeightVelocity = 1.0f / 160;

        public KalmanFilter(float vx)
        {
            motionMat = Matrix8x8.Identity;
            updateMat = Matrix4x8.Identity;

            for (int i = 0; i < 4; i++)
            {
                motionMat[i, i + 4] = 1.0f;
            }
            // mean.VX = -70f;
            mean.VX = vx;
            // 初始化协方差矩阵
            covariance = Matrix8x8.Identity;
        }

        public void Predict()
        {
            // Update velocity covariance
            for (int i = 0; i < 4; i++)
            {
                covariance[i + 4, i + 4] += (float)Math.Pow(stdWeightVelocity * mean[i], 2);
            }

            // Update position covariance
            for (int i = 0; i < 4; i++)
            {
                covariance[i, i] += (float)Math.Pow(stdWeightPosition * mean[i], 2);
            }
            // 过程噪声配置
            //float posNoise = 0.1f;    // 位置噪声
            //float velNoiseX = 140.0f; // X速度噪声（增大以适应反向运动）
            //float velNoiseY = 0.0f; // Y速度噪声

            // 仅对速度分量添加噪声
            //covariance[4, 4] += velNoiseX; // vx
            //covariance[5, 5] += velNoiseY; // vy
            // 预测状态
            mean = motionMat * mean;
            covariance = motionMat * covariance * motionMat.Transpose();
        }

        public void Update(BoundingBox measurement)
        {
            // 测量向量 [x, y, w, h]
            Vector4 measurementVec =
                new(measurement.X, measurement.Y, measurement.Width, measurement.Height);

            // 创新向量 (测量残差)
            Vector4 innovation = measurementVec - new Vector4(mean.X, mean.Y, mean.Z, mean.W);

            // 观测矩阵 H (4x8)
            Matrix4x8 H = updateMat;

            // 创新协方差 S = H * P * H' + R
            Matrix4x4 S = H * covariance * H.Transpose() + Matrix4x4.Identity * 0.0f;

            // 卡尔曼增益 K = P * H' * S^-1
            Matrix8x4 K = covariance * H.Transpose() * S.Inverse();

            // 状态更新
            Vector8 correction = K * innovation;
            mean += correction;

            // 协方差更新
            Matrix8x8 I = Matrix8x8.Identity;
            covariance = (I - K * H) * covariance;
        }

        public BoundingBox GetPredictedBox() => new(mean.X, mean.Y, mean.Z, mean.W, 1.0f, 0);
    }

    // 2. ByteTrack 核心实现
    public class ByteTracker
    {
        private List<TrackState> trackStates = new();
        private int nextId = 1;

        // 参数配置
        public float TrackHighThreshold { get; set; } = 0.6f; //高分检测框阈值

        public float TrackLowThreshold { get; set; } = 0.1f; //低分检测框阈值

        //public float MatchThreshold { get; set; } = 0.7f;//
        public int MaxTimeLost { get; set; } = 10; //连续10帧找不到目标时丢弃该目标

        public float IoUThreshold { get; set; } = 0.3f; //预测框与检测框匹配阈值
        public readonly CountingLine _countingLine;
       // private int _crossCount = 0;
        private readonly CrossDirection _crossDirection;
        private readonly HashSet<int> _countedTracks = new HashSet<int>();

        public ByteTracker(
            CountingLine line,
            int maxAge = 1,
            float iouThreshold = 0.1f,
            CrossDirection direction = CrossDirection.UpToDown
        )
        {
            _countingLine = line;
            trackStates = new List<TrackState>();
            nextId = 1;
            MaxTimeLost = maxAge;
            IoUThreshold = iouThreshold;
            _crossDirection = direction;
        }


        public List<TrackState> Update(List<ObbData> detects,int maxTimeLost, float iouThreshold, float trackHighThreshold,
                    float trackLowThreshold, int minHits, int iteratorDis,float vx, out int corssCount)
        {
            if (trackHighThreshold < trackLowThreshold)
            {
                trackHighThreshold = trackLowThreshold + 0.5f;
            }
            if (trackHighThreshold>1.0f)
            {
                trackHighThreshold = 1.0f;
            }
            if (trackHighThreshold <0.0f)
            {
                trackHighThreshold = 0.6f;
            }

            if (trackLowThreshold > trackHighThreshold)
            {
                trackLowThreshold = trackHighThreshold - 0.5f;
            }
            if (trackLowThreshold > 1.0f)
            {
                trackLowThreshold = 0.1f;
            }
            if (trackLowThreshold < 0.0f)
            {
                trackLowThreshold = 0.1f;
            }
            if(iouThreshold > 1.0f|| iouThreshold<0.0f)
            {
                iouThreshold = 0.3f;
            }
            if (maxTimeLost <= 0)
            {
                maxTimeLost = 1;
            }
            TrackHighThreshold =trackHighThreshold;
            TrackLowThreshold=trackLowThreshold;
            MaxTimeLost = maxTimeLost;
            IoUThreshold=iouThreshold;

            var detections = BoundingBox.odd2BoundingBox(detects);
            // 步骤1: 预测所有现有轨迹
            foreach (var track in trackStates)
            {
                track.Filter.Predict();
                track.PredictedBox = track.Filter.GetPredictedBox();
                track.TimeSinceUpdate++;
            }

            // 步骤2: 将检测分为高分数和低分数
            var highScoreDets = detections.Where(d => d.Score >= TrackHighThreshold).ToList();
            var lowScoreDets = detections
                .Where(d => d.Score < TrackHighThreshold && d.Score >= TrackLowThreshold)
                .ToList();

            // 步骤3: 第一次匹配 - 高分数检测与现有轨迹
            var (matchedPairs, unmatchedTracks, unmatchedDets) = MatchDetectionToTracks(
                highScoreDets,
                trackStates
            );

            // 步骤4: 第二次匹配 - 剩余高分数检测和低分数检测与未匹配轨迹
            var remainingDets = unmatchedDets.Concat(lowScoreDets).ToList();
            var (secondMatchedPairs, secondUnmatchedTracks, _) = MatchDetectionToTracks(
                remainingDets,
                unmatchedTracks
            );

            // 合并匹配结果
            matchedPairs.AddRange(secondMatchedPairs);

            // 步骤5: 更新匹配的轨迹
            foreach (var (trackIdx, detIdx, iou) in matchedPairs)
            {
                var track = trackStates[trackIdx];
                var det =
                    detIdx < highScoreDets.Count
                        ? highScoreDets[detIdx]
                        : remainingDets[detIdx - highScoreDets.Count];
                track.update(det, iou, minHits);
                //track.Filter.Update(det);
                //track.Score = det.Score;
                //track.ClassId = det.ClassId;
                //track.TimeSinceUpdate = 0;
                //track.Iou = iou;
                //var center = new Point(track.PredictedBox.X+track.PredictedBox.Width/2.0f, track.PredictedBox.Y+track.PredictedBox.Height/2.0f);
                //track.PathHistory.Add(center);

                //// 限制历史记录长度以节省内存
                //if (PathHistory.Count > MaxPathHistory)
                //    PathHistory.RemoveAt(0);

                //track.Age++;
            }

            // 步骤6: 初始化新轨迹 (仅高分数检测)
            foreach (var det in unmatchedDets.Where(d => d.Score >= TrackHighThreshold))
            {
                var newTrack = new TrackState
                {
                    TrackId = nextId++,
                    Filter = new KalmanFilter(vx),
                    PredictedBox = det,
                    Score = det.Score,
                    ClassId = det.ClassId,
                    TimeSinceUpdate = 0
                };
                newTrack.Filter.Update(det);
                trackStates.Add(newTrack);
            }

            // 步骤7: 移除丢失的轨迹
            trackStates = trackStates.Where(t => t.TimeSinceUpdate <= MaxTimeLost).ToList();
            // 检查过线情况
            CheckLineCrossing(iteratorDis, out corssCount);
            return trackStates.Where(t => t.TimeSinceUpdate == 0).ToList();
        }
        public List<TrackState> Update(ObbResult detects, int maxTimeLost, float iouThreshold, float trackHighThreshold,
                    float trackLowThreshold, int minHits, int iteratorDis,float vx, out int corssCount)
        {
            if (trackHighThreshold < trackLowThreshold)
            {
                trackHighThreshold = trackLowThreshold + 0.5f;
            }
            if (trackHighThreshold > 1.0f)
            {
                trackHighThreshold = 1.0f;
            }
            if (trackHighThreshold < 0.0f)
            {
                trackHighThreshold = 0.6f;
            }

            if (trackLowThreshold > trackHighThreshold)
            {
                trackLowThreshold = trackHighThreshold - 0.5f;
            }
            if (trackLowThreshold > 1.0f)
            {
                trackLowThreshold = 0.1f;
            }
            if (trackLowThreshold < 0.0f)
            {
                trackLowThreshold = 0.1f;
            }
            if (iouThreshold > 1.0f || iouThreshold < 0.0f)
            {
                iouThreshold = 0.3f;
            }
            if (maxTimeLost <= 0)
            {
                maxTimeLost = 1;
            }
            TrackHighThreshold = trackHighThreshold;
            TrackLowThreshold = trackLowThreshold;
            MaxTimeLost = maxTimeLost;
            IoUThreshold = iouThreshold;

            var detections = BoundingBox.odd2BoundingBox(detects);
            // 步骤1: 预测所有现有轨迹
            foreach (var track in trackStates)
            {
                track.Filter.Predict();
                track.PredictedBox = track.Filter.GetPredictedBox();
                track.TimeSinceUpdate++;
            }

            // 步骤2: 将检测分为高分数和低分数
            var highScoreDets = detections.Where(d => d.Score >= TrackHighThreshold).ToList();
            var lowScoreDets = detections
                .Where(d => d.Score < TrackHighThreshold && d.Score >= TrackLowThreshold)
                .ToList();

            // 步骤3: 第一次匹配 - 高分数检测与现有轨迹
            var (matchedPairs, unmatchedTracks, unmatchedDets) = MatchDetectionToTracks(
                highScoreDets,
                trackStates
            );

            // 步骤4: 第二次匹配 - 剩余高分数检测和低分数检测与未匹配轨迹
            var remainingDets = unmatchedDets.Concat(lowScoreDets).ToList();
            var (secondMatchedPairs, secondUnmatchedTracks, _) = MatchDetectionToTracks(
                remainingDets,
                unmatchedTracks
            );

            // 合并匹配结果
            matchedPairs.AddRange(secondMatchedPairs);

            // 步骤5: 更新匹配的轨迹
            foreach (var (trackIdx, detIdx, iou) in matchedPairs)
            {
                var track = trackStates[trackIdx];
                var det =
                    detIdx < highScoreDets.Count
                        ? highScoreDets[detIdx]
                        : remainingDets[detIdx - highScoreDets.Count];
                track.update(det, iou, minHits);
                //track.Filter.Update(det);
                //track.Score = det.Score;
                //track.ClassId = det.ClassId;
                //track.TimeSinceUpdate = 0;
                //track.Iou = iou;
                //var center = new Point(track.PredictedBox.X+track.PredictedBox.Width/2.0f, track.PredictedBox.Y+track.PredictedBox.Height/2.0f);
                //track.PathHistory.Add(center);

                //// 限制历史记录长度以节省内存
                //if (PathHistory.Count > MaxPathHistory)
                //    PathHistory.RemoveAt(0);

                //track.Age++;
            }

            // 步骤6: 初始化新轨迹 (仅高分数检测)
            foreach (var det in unmatchedDets.Where(d => d.Score >= TrackHighThreshold))
            {
                var newTrack = new TrackState
                {
                    TrackId = nextId++,
                    Filter = new KalmanFilter(vx),
                    PredictedBox = det,
                    Score = det.Score,
                    ClassId = det.ClassId,
                    TimeSinceUpdate = 0
                };
                newTrack.Filter.Update(det);
                trackStates.Add(newTrack);
            }

            // 步骤7: 移除丢失的轨迹
            trackStates = trackStates.Where(t => t.TimeSinceUpdate <= MaxTimeLost).ToList();
            // 检查过线情况
            CheckLineCrossing(iteratorDis, out corssCount);
            return trackStates.Where(t => t.TimeSinceUpdate == 0).ToList();
        }

        private (
            List<(int trackIdx, int detIdx, float iou)>,
            List<TrackState>,
            List<BoundingBox>
        ) MatchDetectionToTracks(List<BoundingBox> detections, List<TrackState> tracks)
        {
            if (detections.Count == 0 || tracks.Count == 0)
            {
                return (new List<(int, int, float)>(), tracks.ToList(), detections.ToList());
            }

            // 计算IoU矩阵
            float[,] iouMatrix = new float[tracks.Count, detections.Count];
            for (int i = 0; i < tracks.Count; i++)
            {
                for (int j = 0; j < detections.Count; j++)
                {
                    iouMatrix[i, j] = CalculateIoU(tracks[i].PredictedBox, detections[j]);
                }
            }

            // 简单匹配 - 选择IoU最大的对
            var matchedPairs = new List<(int, int, float)>();
            var unmatchedTracks = Enumerable.Range(0, tracks.Count).ToList();
            var unmatchedDets = Enumerable.Range(0, detections.Count).ToList();

            while (true)
            {
                float maxIoU = 0;
                int bestTrackIdx = -1;
                int bestDetIdx = -1;

                for (int i = 0; i < tracks.Count; i++)
                {
                    if (!unmatchedTracks.Contains(i))
                        continue;

                    for (int j = 0; j < detections.Count; j++)
                    {
                        if (!unmatchedDets.Contains(j))
                            continue;

                        if (iouMatrix[i, j] > maxIoU && iouMatrix[i, j] > IoUThreshold)
                        {
                            maxIoU = iouMatrix[i, j];
                            bestTrackIdx = i;
                            bestDetIdx = j;
                        }
                    }
                }

                if (bestTrackIdx == -1 || bestDetIdx == -1)
                    break;

                matchedPairs.Add((bestTrackIdx, bestDetIdx, maxIoU));
                unmatchedTracks.Remove(bestTrackIdx);
                unmatchedDets.Remove(bestDetIdx);
            }

            return (
                matchedPairs,
                unmatchedTracks.Select(i => tracks[i]).ToList(),
                unmatchedDets.Select(i => detections[i]).ToList()
            );
        }

        private static float CalculateIoU(BoundingBox a, BoundingBox b)
        {
            float x1 = Math.Max(a.Left, b.Left);
            float y1 = Math.Max(a.Top, b.Top);
            float x2 = Math.Min(a.Right, b.Right);
            float y2 = Math.Min(a.Bottom, b.Bottom);

            float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float areaA = a.Width * a.Height;
            float areaB = b.Width * b.Height;

            return intersection / (areaA + areaB - intersection);
        }

        private void CheckLineCrossing(int iteratorDis,out int crossCount)
        {
            int _crossCount = 0;
            foreach (
                var track in trackStates.Where(t =>
                    t.IsConfirmed && !_countedTracks.Contains(t.TrackId)
                )
            )
            {
                if (track.PathHistory.Count < 2)
                    continue;

                if (iteratorDis >= track.PathHistory.Count)
                {
                    iteratorDis = track.PathHistory.Count - 1;
                }
                var lastPos = track.PathHistory[track.PathHistory.Count - 1];
                var prevPos = track.PathHistory[track.PathHistory.Count - iteratorDis];

                if (IsCrossingLine(prevPos, lastPos, _countingLine))
                {
                    CrossDirection direction = GetCrossDirection(prevPos, lastPos, _countingLine);
                    if (direction == _crossDirection)
                    {
                        _crossCount++;
                        _countedTracks.Add(track.TrackId);
                        //Console.WriteLine($"物体 {track.TrackId} 过线，总计数: {_crossCount}");
                    }
                }
            }
           crossCount=_crossCount;
        }

        private bool IsCrossingLine(OpenCvSharp.Point p1, OpenCvSharp.Point p2, CountingLine line)
        {
            float d1 = line.WhichSide(p1);
            float d2 = line.WhichSide(p2);

            // 两点在线的不同侧
            if (d1 * d2 < 0)
            {
                // 计算交点参数
                float ua = (
                    line.Direction.X * (p1.Y - line.Start.Y)
                    - line.Direction.Y * (p1.X - line.Start.X)
                );
                float ub =
                    (p2.X - p1.X) * (p1.Y - line.Start.Y) - (p2.Y - p1.Y) * (p1.X - line.Start.X);
                float denominator =
                    line.Direction.Y * (p2.X - p1.X) - line.Direction.X * (p2.Y - p1.Y);

                if (denominator != 0)
                {
                    ua /= denominator;
                    ub /= denominator;

                    // 检查交点是否在线段上
                    if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

       // public int GetCrossCount() => _crossCount;

        private CrossDirection GetCrossDirection(
            OpenCvSharp.Point prevPos,
            OpenCvSharp.Point lastPos,
            CountingLine line
        )
        {
            if (Math.Abs(line.Start.X - line.End.X) > Math.Abs(line.Start.Y - line.End.Y))
            {
                // 主要是水平线
                return lastPos.Y < prevPos.Y ? CrossDirection.DownToUp : CrossDirection.UpToDown;
            }
            else
            {
                // 主要是垂直线
                return lastPos.X > prevPos.X
                    ? CrossDirection.LeftToRight
                    : CrossDirection.RightToLeft;
            }
        }
    }
}