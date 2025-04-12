using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace YoloDeployPlatform.tracker
{
    public class Track
    {
        public int Id { get; }
        public KalmanFilter Filter { get; }
        public int Age { get; set; }
        public int Hits { get; set; }
        public int HitStreak { get; set; }
        public float[] LastObservation { get; set; }
        public bool IsConfirmed => Hits >= MinHits;
        public List<Point> PathHistory { get; } = new List<Point>();
        private  int MinHits = 5;
        private int MaxPathHistory = 10;

        public Track(int id, Rect bbox,int minHits)
        {
            var box = KalmanFilter.ParseBbox(bbox);
            Id = id;
            Filter = new KalmanFilter(box);
            Age = 1;
            Hits = 1;
            HitStreak = 1;
            LastObservation = box;
            MinHits = minHits;
            // 记录中心点轨迹
            var center = new Point(box[0], box[1]);
            PathHistory.Add(center);

            // 限制历史记录长度以节省内存
            if (PathHistory.Count > MaxPathHistory)
                PathHistory.RemoveAt(0);
        }

        public void Predict()
        {
            Filter.Predict();
            Age++;
        }

        public void Update(Rect bbox)
        {
            var box = KalmanFilter.ParseBbox(bbox);
            Filter.Update(box);
            // 使用滤波后的状态获取中心点（更平滑）
            var state = Filter.GetBbox();
            var center = new Point(state[0], state[1]); // 直接取x,y中心坐标

            PathHistory.Add(center);

            Hits++;
            HitStreak++;
            LastObservation = box;
            // 限制历史记录长度
            if (PathHistory.Count > MaxPathHistory)
                PathHistory.RemoveAt(0);
        }

        public void MarkMissed()
        {
            HitStreak = 0;
        }

        public float[] GetState()
        {
            return Filter.GetBbox();
        }
    }
}