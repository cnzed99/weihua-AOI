using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    public class CanvasPro : Canvas
    {
        public List<SDrawLine> Lines { get; set; } = new List<SDrawLine>();
        public List<SDrawEllipse> Ellipses { get; set; } = new List<SDrawEllipse>();
        public List<SDrawRectangle> Rectangles { get; set; } = new List<SDrawRectangle>();
        public List<SDrawText> Texts { get; set; } = new List<SDrawText>();
        public List<SDrawTextAlignment> TextAlignments { get; set; } = new List<SDrawTextAlignment>();
        public List<SDrawRegion> Regions { get; set; } = new List<SDrawRegion>();
        private Pen _Pen { get; set; } = new Pen(Brushes.Red, 1);
        private FontFamily _FontFamily { get; set; } = new FontFamily("宋体");
        private FontStyle _FontStyle { get; set; } = FontStyles.Normal;
        private FontWeight _FontWeight { get; set; } = FontWeights.Normal;
        private int _FontSize { get; set; } = 15;
        private Brush _FontBrush { get; set; } = Brushes.Red;
        private List<(Pen, double)> _Pens { get; set; } = new List<(Pen, double)>();

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

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(CanvasPro), new PropertyMetadata(null));

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

        public void SetPen(Brush brush, double thickness)
        {
            _Pen = new Pen(brush, thickness);
        }

        public void SetFontFamily(FontFamily fontFamily)
        {
            _FontFamily = fontFamily;
        }

        public void SetFontStyle(FontStyle fontStyle)
        {
            _FontStyle = fontStyle;
        }

        public void SetFontWeight(FontWeight fontWeight)
        {
            _FontWeight = fontWeight;
        }

        public void SetFontSize(int fontSize)
        {
            _FontSize = fontSize;
        }

        public void SetFontBrush(Brush fontBrush)
        {
            _FontBrush = fontBrush;
        }

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

        public void DrawRegion(List<Point> points, bool isRender = true)
        {
            if (points.Count == 0) return;

            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Regions.Add(new SDrawRegion(_Pen, points));

            if (isRender) this.InvalidateVisual();
        }

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

        public void DrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Lines.Add(new SDrawLine(_Pen, pt1, pt2));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawEllipse(Point center, double radiusX, double radiusY, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Ellipses.Add(new SDrawEllipse(_Pen, center, radiusX, radiusY));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawRectangle(Rect rectangle, bool isRender = true)
        {
            if (!_Pens.Exists(e => e.Item1 == _Pen)) _Pens.Add((_Pen, _Pen.Thickness));

            Rectangles.Add(new SDrawRectangle(_Pen, rectangle));
            if (isRender) this.InvalidateVisual();
        }

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
