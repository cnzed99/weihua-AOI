using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace YoloDeployPlatform.tracker
{
    public class CountingLine
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        // 线的方向向量
        public Point Direction;

        public CountingLine(Point start, Point end)
        {
            Start = start;
            End = end;
            Direction = new Point(End.X - Start.X, End.Y - Start.Y);
        }

        // 判断点p在线的哪一侧（用于交叉检测）
        public float WhichSide(Point p)
        {
            return (End.X - Start.X) * (p.Y - Start.Y) - (End.Y - Start.Y) * (p.X - Start.X);
        }
    }
}