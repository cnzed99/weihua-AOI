using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 窗口坐标绘图
    /// </summary>
    public class CanvasPro : Canvas
    {
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制直线集合
        /// </summary>
        public List<SDrawLine> Lines { get; set; } = new List<SDrawLine>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制椭圆集合
        /// </summary>
        public List<SDrawEllipse> Ellipses { get; set; } = new List<SDrawEllipse>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制矩形集合
        /// </summary>
        public List<SDrawRectangle> Rectangles { get; set; } = new List<SDrawRectangle>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制文字集合
        /// </summary>
        public List<SDrawText> Texts { get; set; } = new List<SDrawText>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制文字集合
        /// </summary>
        public List<SDrawTextAlignment> TextAlignments { get; set; } = new List<SDrawTextAlignment>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制区域集合
        /// </summary>
        public List<SDrawRegion> Regions { get; set; } = new List<SDrawRegion>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 当前画笔
        /// </summary>
        private Pen _Pen { get; set; } = new Pen(Brushes.Red, 1);
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 已用画笔集合
        /// </summary>
        private List<(Pen, double)> _Pens { get; set; } = new List<(Pen, double)>();
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体
        /// </summary>
        private FontFamily _FontFamily { get; set; } = new FontFamily("宋体");
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体风格
        /// </summary>
        private FontStyle _FontStyle { get; set; } = FontStyles.Normal;
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 加粗
        /// </summary>
        private FontWeight _FontWeight { get; set; } = FontWeights.Normal;
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体大小
        /// </summary>
        private int _FontSize { get; set; } = 15;
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体画刷
        /// </summary>
        private Brush _FontBrush { get; set; } = Brushes.Red;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 是否填充
        /// </summary>
        private bool fillEdge = false;
        public bool FillEdge
        {
            get { return fillEdge; }
            set
            {
                fillEdge = value;
                this.InvalidateVisual();
            }
        }

        public CanvasPro()
        {
            this.Background = Brushes.Transparent;
            this.IsHitTestVisible = false;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(CanvasPro), new PropertyMetadata(null));

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 刷新图像重载
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var line in Lines)
            {
                dc.DrawLine(line.pen, new Point(line.pt1.X, line.pt1.Y), new Point(line.pt2.X, line.pt2.Y));
            }
            foreach (var ellipse in Ellipses)
            {
                dc.DrawEllipse(FillEdge ? ellipse.pen.Brush : Brushes.Transparent, ellipse.pen, new Point(ellipse.center.X, ellipse.center.Y), ellipse.radiusX, ellipse.radiusY);
            }
            foreach (var rect in Rectangles)
            {
                dc.DrawRectangle(FillEdge ? rect.pen.Brush : Brushes.Transparent, rect.pen, new Rect(rect.rectangle.X, rect.rectangle.Y, rect.rectangle.Width, rect.rectangle.Height));
            }
            foreach (var region in Regions)
            {
                List<Point> tmp = new List<Point>();
                foreach (var item in region.points)
                {
                    tmp.Add(new Point(item.X, item.Y));
                }
                PathGeometry geometry = new PathGeometry();
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                polyLineSegment.Points = new PointCollection(tmp);
                PathFigure figure = new PathFigure(tmp[0], new[] { polyLineSegment }, false);
                geometry.Figures.Add(figure);

                dc.DrawGeometry(FillEdge ? region.pen.Brush : Brushes.Transparent, region.pen, geometry);
            }
            foreach (var text in Texts)
            {
                text.formattedText.SetFontSize(text.thickness);
                dc.DrawText(text.formattedText, new Point(text.origin.X, text.origin.Y));
            }
            foreach (var text in TextAlignments)
            {
                text.formattedText.SetFontSize(text.thickness);
                double x = 20, y = 20;
                switch (text.alignmentX)
                {
                    case AlignmentX.Center:
                        x = this.ActualWidth / 2 - text.formattedText.Width / 2;
                        break;
                    case AlignmentX.Right:
                        x = this.ActualWidth - text.formattedText.Width - 20;
                        break;
                }
                switch (text.alignmentY)
                {
                    case AlignmentY.Center:
                        y = this.ActualHeight / 2 - text.formattedText.Height / 2;
                        break;
                    case AlignmentY.Bottom:
                        y = this.ActualHeight - text.formattedText.Height - 20;
                        break;
                }
                dc.DrawText(text.formattedText, new Point(x, y));
            }
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 设置画笔
        /// </summary>
        /// <param name="brush">画刷</param>
        /// <param name="thickness">厚度</param>
        public void SetPen(Brush brush, double thickness)
        {
            _Pen = new Pen(brush, thickness);
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体类型
        /// </summary>
        /// <param name="fontFamily">字体类型</param>
        public void SetFontFamily(FontFamily fontFamily)
        {
            _FontFamily = fontFamily;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体风格
        /// </summary>
        /// <param name="fontStyle">字体风格</param>
        public void SetFontStyle(FontStyle fontStyle)
        {
            _FontStyle = fontStyle;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体加粗
        /// </summary>
        /// <param name="fontWeight">字体加粗</param>
        public void SetFontWeight(FontWeight fontWeight)
        {
            _FontWeight = fontWeight;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体大小
        /// </summary>
        /// <param name="fontSize">字体大小</param>
        public void SetFontSize(int fontSize)
        {
            _FontSize = fontSize;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体画刷
        /// </summary>
        /// <param name="fontBrush">字体画刷</param>
        public void SetFontBrush(Brush fontBrush)
        {
            _FontBrush = fontBrush;
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="origin">显示位置</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawText(string text, Point origin, bool isRender = true)
        {
            if (Source != null && Source is BitmapSource)
            {
                BitmapSource bitmapImage = (BitmapSource)Source;
                Texts.Add(new SDrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(_FontFamily, _FontStyle, _FontWeight, FontStretches.Normal), _FontSize, _FontBrush, bitmapImage.DpiX / 96f), _FontSize, origin));
                if (isRender) this.InvalidateVisual();
            }
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="alignmentX">Y对齐</param>
        /// <param name="alignmentY">X对齐</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawText(string text, AlignmentX alignmentX, AlignmentY alignmentY, bool isRender = true)
        {
            if (Source != null && Source is BitmapSource)
            {
                BitmapSource bitmapImage = (BitmapSource)Source;
                TextAlignments.Add(new SDrawTextAlignment(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(_FontFamily, _FontStyle, _FontWeight, FontStretches.Normal), _FontSize, _FontBrush, bitmapImage.DpiX / 96f), _FontSize, alignmentX, alignmentY));
                if (isRender) this.InvalidateVisual();
            }
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制连贯区域
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawRegion(List<Point> points, bool isRender = true)
        {
            if (points.Count == 0) return;

            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Regions.Add(new SDrawRegion(_Pen, points));

            if (isRender) this.InvalidateVisual();
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制不连贯区域
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawPoints(List<Point> points, bool isRender = true)
        {
            if (points.Count == 0) return;

            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            List<Point> region = new List<Point>();
            foreach (var item in points)
            {
                if (region.Count > 0 && Math.Sqrt((region.Last().X - item.X) * (region.Last().X - item.X) + (region.Last().Y - item.Y) * (region.Last().Y - item.Y)) > 2)
                {
                    Regions.Add(new SDrawRegion(_Pen, region));
                    region = new List<Point>();
                }
                region.Add(item);
            }
            if (region.Count > 0)
            {
                Regions.Add(new SDrawRegion(_Pen, region));
            }

            if (isRender) this.InvalidateVisual();
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制直线
        /// </summary>
        /// <param name="pt1">待绘制直线点1</param>
        /// <param name="pt2">待绘制直线点2</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Lines.Add(new SDrawLine(_Pen, pt1, pt2));
            if (isRender) this.InvalidateVisual();
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制椭圆形
        /// </summary>
        /// <param name="center">待绘制椭圆中心点</param>
        /// <param name="radiusX">待绘制椭圆X半径</param>
        /// <param name="radiusY">待绘制椭圆Y半径</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawEllipse(Point center, double radiusX, double radiusY, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Ellipses.Add(new SDrawEllipse(_Pen, center, radiusX, radiusY));
            if (isRender) this.InvalidateVisual();
        }
        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="rectangle">待绘制矩形</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawRectangle(Rect rectangle, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Rectangles.Add(new SDrawRectangle(_Pen, rectangle));
            if (isRender) this.InvalidateVisual();
        }
        /// <summary>
        /// 2024.7.8 李焕彬
        /// 清除显示
        /// </summary>
        public void Clear()
        {
            Lines.Clear();
            Ellipses.Clear();
            Rectangles.Clear();
            Texts.Clear();
            TextAlignments.Clear();
            Regions.Clear();
            _Pens.Clear();

            this.InvalidateVisual();
        }
    }
}
