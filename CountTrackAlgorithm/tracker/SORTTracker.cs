using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenVinoSharp;
using OpenVinoSharp.Extensions.result;

namespace YoloDeployPlatform.tracker
{
    public class SORTTracker
    {
        public readonly List<Track> _tracks;
        private int _nextId;
        private readonly int _maxAge;
        private readonly float _iouThreshold;

        public readonly CountingLine _countingLine;
        //private int _crossCount = 0;
        private readonly HashSet<int> _countedTracks = new HashSet<int>();
        private readonly CrossDirection _crossDirection;

        public SORTTracker(
            CountingLine line,
            int maxAge = 1,
            float iouThreshold = 0.3f,
            CrossDirection direction = CrossDirection.RightToLeft
        )
        {
            _countingLine = line;
            _tracks = new List<Track>();
            _nextId = 1;
            _maxAge = maxAge;
            _iouThreshold = iouThreshold;
            _crossDirection = direction;
        }

        public List<(int Id, float[] Bbox)> Update(DetResult detections, out int corssCount, int MinHits = 3,int iteratorDis=5)
        {
            var result = new List<(int Id, float[] Bbox)>();
            if (detections.count == 0)
            {
                corssCount = 0;
                return result;
            }
            // 1. 预测现有跟踪器的位置
            foreach (var track in _tracks)
            {
                track.Predict();
            }

            // 2. 数据关联
            var (matches, unmatchedDetections, unmatchedTracks) = Associate(detections);

            // 3. 更新匹配的跟踪器
            foreach (var (detectionIdx, trackIdx) in matches)
            {
                _tracks[trackIdx].Update(detections[detectionIdx].box);
            }

            // 4. 处理未匹配的检测 - 创建新跟踪器
            foreach (var detectionIdx in unmatchedDetections)
            {
                _tracks.Add(new Track(_nextId++, detections[detectionIdx].box));
            }

            // 5. 处理未匹配的跟踪器 - 标记为丢失
            foreach (var trackIdx in unmatchedTracks)
            {
                _tracks[trackIdx].MarkMissed();
            }

            // 6. 移除丢失的跟踪器
            _tracks.RemoveAll(t =>
                t.Age - t.Hits > _maxAge || (t.Hits < MinHits && t.Age > MinHits)
            );

            // 7. 返回确认的跟踪结果

            foreach (var track in _tracks)
            {
                if (track.IsConfirmed)
                {
                    result.Add((track.Id, track.GetState()));
                }
            }
            // 检查过线情况
            CheckLineCrossing(iteratorDis, out corssCount);
            return result;
        }

        private (List<(int, int)>, List<int>, List<int>) Associate(DetResult detections)
        {
            if (_tracks.Count == 0)
            {
                return (
                    new List<(int, int)>(),
                    Enumerable.Range(0, detections.count).ToList(),
                    new List<int>()
                );
            }

            // 计算IoU矩阵
            var iouMatrix = new float[detections.count, _tracks.Count];
            for (int d = 0; d < detections.count; d++)
            {
                for (int t = 0; t < _tracks.Count; t++)
                {
                    var abox = KalmanFilter.ParseBbox(detections[d].box);
                    iouMatrix[d, t] = CalculateIoU(abox, _tracks[t].GetState());
                }
            }

            // 使用匈牙利算法进行匹配
            var matches = new List<(int, int)>();
            var unmatchedDetections = new List<int>();
            var unmatchedTracks = new List<int>();

            if (iouMatrix.GetLength(1) > 0)
            {
                // 使用Munkres/Hungarian算法
                var (rowIndices, colIndices) = HungarianAlgorithm.FindAssignments(iouMatrix, true);

                for (int i = 0; i < rowIndices.Length; i++)
                {
                    if (iouMatrix[rowIndices[i], colIndices[i]] < _iouThreshold)
                    {
                        unmatchedDetections.Add(rowIndices[i]);
                        unmatchedTracks.Add(colIndices[i]);
                    }
                    else
                    {
                        matches.Add((rowIndices[i], colIndices[i]));
                    }
                }

                // 找出未匹配的检测
                for (int d = 0; d < detections.count; d++)
                {
                    if (!rowIndices.Contains(d))
                    {
                        unmatchedDetections.Add(d);
                    }
                }

                // 找出未匹配的跟踪器
                for (int t = 0; t < _tracks.Count; t++)
                {
                    if (!colIndices.Contains(t))
                    {
                        unmatchedTracks.Add(t);
                    }
                }
            }
            else
            {
                unmatchedDetections = Enumerable.Range(0, detections.count).ToList();
            }

            return (matches, unmatchedDetections, unmatchedTracks);
        }
        public List<(int Id, float[] Bbox)> Update(ObbResult detections, out int corssCount, int MinHits = 3, int iteratorDis = 5)
        {
            var result = new List<(int Id, float[] Bbox)>();
            if (detections.count == 0)
            {
                corssCount = 0;
                return result;
            }
            // 1. 预测现有跟踪器的位置
            foreach (var track in _tracks)
            {
                track.Predict();
            }

            // 2. 数据关联
            var (matches, unmatchedDetections, unmatchedTracks) = Associate(detections);

            // 3. 更新匹配的跟踪器
            foreach (var (detectionIdx, trackIdx) in matches)
            {
                _tracks[trackIdx].Update(detections[detectionIdx].box.BoundingRect());
            }

            // 4. 处理未匹配的检测 - 创建新跟踪器
            foreach (var detectionIdx in unmatchedDetections)
            {
                _tracks.Add(new Track(_nextId++, detections[detectionIdx].box.BoundingRect()));
            }

            // 5. 处理未匹配的跟踪器 - 标记为丢失
            foreach (var trackIdx in unmatchedTracks)
            {
                _tracks[trackIdx].MarkMissed();
            }

            // 6. 移除丢失的跟踪器
            _tracks.RemoveAll(t =>
                t.Age - t.Hits > _maxAge || (t.Hits < MinHits && t.Age > MinHits)
            );

            // 7. 返回确认的跟踪结果

            foreach (var track in _tracks)
            {
                if (track.IsConfirmed)
                {
                    result.Add((track.Id, track.GetState()));
                }
            }
            // 检查过线情况
            CheckLineCrossing(iteratorDis, out corssCount);
            return result;
        }

        private (List<(int, int)>, List<int>, List<int>) Associate(ObbResult detections)
        {
            if (_tracks.Count == 0)
            {
                return (
                    new List<(int, int)>(),
                    Enumerable.Range(0, detections.count).ToList(),
                    new List<int>()
                );
            }

            // 计算IoU矩阵
            var iouMatrix = new float[detections.count, _tracks.Count];
            for (int d = 0; d < detections.count; d++)
            {
                for (int t = 0; t < _tracks.Count; t++)
                {
                    var abox = KalmanFilter.ParseBbox(detections[d].box.BoundingRect());
                    iouMatrix[d, t] = CalculateIoU(abox, _tracks[t].GetState());
                }
            }

            // 使用匈牙利算法进行匹配
            var matches = new List<(int, int)>();
            var unmatchedDetections = new List<int>();
            var unmatchedTracks = new List<int>();

            if (iouMatrix.GetLength(1) > 0)
            {
                // 使用Munkres/Hungarian算法
                var (rowIndices, colIndices) = HungarianAlgorithm.FindAssignments(iouMatrix, true);

                for (int i = 0; i < rowIndices.Length; i++)
                {
                    if (iouMatrix[rowIndices[i], colIndices[i]] < _iouThreshold)
                    {
                        unmatchedDetections.Add(rowIndices[i]);
                        unmatchedTracks.Add(colIndices[i]);
                    }
                    else
                    {
                        matches.Add((rowIndices[i], colIndices[i]));
                    }
                }

                // 找出未匹配的检测
                for (int d = 0; d < detections.count; d++)
                {
                    if (!rowIndices.Contains(d))
                    {
                        unmatchedDetections.Add(d);
                    }
                }

                // 找出未匹配的跟踪器
                for (int t = 0; t < _tracks.Count; t++)
                {
                    if (!colIndices.Contains(t))
                    {
                        unmatchedTracks.Add(t);
                    }
                }
            }
            else
            {
                unmatchedDetections = Enumerable.Range(0, detections.count).ToList();
            }

            return (matches, unmatchedDetections, unmatchedTracks);
        }

        public List<(int Id, float[] Bbox)> Update(List<ObbData> detections,out int corssCount, int MinHits = 3, int iteratorDis = 5)
        {
            var result = new List<(int Id, float[] Bbox)>();
            if (detections.Count == 0)
            {
                corssCount = 0;
                return result;
            }
            // 1. 预测现有跟踪器的位置
            foreach (var track in _tracks)
            {
                track.Predict();
            }

            // 2. 数据关联
            var (matches, unmatchedDetections, unmatchedTracks) = Associate(detections);

            // 3. 更新匹配的跟踪器
            foreach (var (detectionIdx, trackIdx) in matches)
            {
                _tracks[trackIdx].Update(detections[detectionIdx].box.BoundingRect());
            }

            // 4. 处理未匹配的检测 - 创建新跟踪器
            foreach (var detectionIdx in unmatchedDetections)
            {
                _tracks.Add(new Track(_nextId++, detections[detectionIdx].box.BoundingRect()));
            }

            // 5. 处理未匹配的跟踪器 - 标记为丢失
            foreach (var trackIdx in unmatchedTracks)
            {
                _tracks[trackIdx].MarkMissed();
            }

            // 6. 移除丢失的跟踪器
            _tracks.RemoveAll(t =>
                t.Age - t.Hits > _maxAge || (t.Hits < MinHits && t.Age > MinHits)
            );

            // 7. 返回确认的跟踪结果

            foreach (var track in _tracks)
            {
                if (track.IsConfirmed)
                {
                    result.Add((track.Id, track.GetState()));
                }
            }
            // 检查过线情况
            CheckLineCrossing(iteratorDis ,out corssCount);
            return result;
        }

        private (List<(int, int)>, List<int>, List<int>) Associate(List<ObbData> detections)
        {
            if (_tracks.Count == 0)
            {
                return (
                    new List<(int, int)>(),
                    Enumerable.Range(0, detections.Count).ToList(),
                    new List<int>()
                );
            }

            // 计算IoU矩阵
            var iouMatrix = new float[detections.Count, _tracks.Count];
            for (int d = 0; d < detections.Count; d++)
            {
                for (int t = 0; t < _tracks.Count; t++)
                {
                    var abox = KalmanFilter.ParseBbox(detections[d].box.BoundingRect());
                    iouMatrix[d, t] = CalculateIoU(abox, _tracks[t].GetState());
                }
            }

            // 使用匈牙利算法进行匹配
            var matches = new List<(int, int)>();
            var unmatchedDetections = new List<int>();
            var unmatchedTracks = new List<int>();

            if (iouMatrix.GetLength(1) > 0)
            {
                // 使用Munkres/Hungarian算法
                var (rowIndices, colIndices) = HungarianAlgorithm.FindAssignments(iouMatrix, true);

                for (int i = 0; i < rowIndices.Length; i++)
                {
                    if (iouMatrix[rowIndices[i], colIndices[i]] < _iouThreshold)
                    {
                        unmatchedDetections.Add(rowIndices[i]);
                        unmatchedTracks.Add(colIndices[i]);
                    }
                    else
                    {
                        matches.Add((rowIndices[i], colIndices[i]));
                    }
                }

                // 找出未匹配的检测
                for (int d = 0; d < detections.Count; d++)
                {
                    if (!rowIndices.Contains(d))
                    {
                        unmatchedDetections.Add(d);
                    }
                }

                // 找出未匹配的跟踪器
                for (int t = 0; t < _tracks.Count; t++)
                {
                    if (!colIndices.Contains(t))
                    {
                        unmatchedTracks.Add(t);
                    }
                }
            }
            else
            {
                unmatchedDetections = Enumerable.Range(0, detections.Count).ToList();
            }

            return (matches, unmatchedDetections, unmatchedTracks);
        }

        private static float CalculateIoU(float[] boxA, float[] boxB)
        {
            // 计算两个边界框的交并比
            float xA = Math.Max(boxA[0], boxB[0]);
            float yA = Math.Max(boxA[1], boxB[1]);
            float xB = Math.Min(boxA[0] + boxA[2] * boxA[3], boxB[0] + boxB[2] * boxB[3]);
            float yB = Math.Min(boxA[1] + boxA[3], boxB[1] + boxB[3]);

            float interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
            float boxAArea = boxA[2] * boxA[3] * boxA[3];
            float boxBArea = boxB[2] * boxB[3] * boxB[3];

            return interArea / (boxAArea + boxBArea - interArea);
        }

        private void CheckLineCrossing(int iteratorDis, out int crossCount)
        {
            int _crossCount = 0;
            foreach (var track in _tracks.Where(t => t.IsConfirmed && !_countedTracks.Contains(t.Id)))
            {
                if (track.PathHistory.Count < 2)
                    continue;
                if (iteratorDis>=track.PathHistory.Count)
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
                        _countedTracks.Add(track.Id);
                       // Console.WriteLine($"物体 {track.Id} 过线，总计数: {_crossCount}");
                    }
                }
            }
            crossCount = _crossCount;
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

      //  public int GetCrossCount() => _crossCount;

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

    public enum CrossDirection
    {
        UpToDown,
        DownToUp,
        LeftToRight,
        RightToLeft
    }
}