using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageWinCtrl
{
    public record SDrawLine(Pen pen, Point pt1, Point pt2);
    public record SDrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY);
    public record SDrawRectangle(Brush brush, Pen pen, Rect rectangle);
    public record SDrawText(FormattedText formattedText, Point origin);
    public record SDrawRegion(Brush brush, Pen pen, Geometry geometry);
    public class CanvasPro : Canvas
    {
        public CanvasPro CanvasOne
        {
            get { return (CanvasPro)GetValue(CanvasOneProperty); }
            set { SetValue(CanvasOneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanvasOne.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasOneProperty =
            DependencyProperty.Register("CanvasOne", typeof(CanvasPro), typeof(CanvasPro), new PropertyMetadata(null));


        public List<SDrawLine> Lines { get; set; } = new List<SDrawLine>();
        public List<SDrawEllipse> Ellipses { get; set; } = new List<SDrawEllipse>();
        public List<SDrawRectangle> Rectangles { get; set; } = new List<SDrawRectangle>();
        public List<SDrawText> Texts { get; set; } = new List<SDrawText>();
        public List<SDrawRegion> Regions { get; set; } = new List<SDrawRegion>();

        public CanvasPro()
        {
            this.Background = Brushes.Transparent;
            this.IsHitTestVisible = false;
            this.Loaded += CanvasPro_Loaded;
        }

        private void CanvasPro_Loaded(object sender, RoutedEventArgs e)
        {
            CanvasOne = this;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            foreach (var line in Lines)
            {
                dc.DrawLine(line.pen, line.pt1, line.pt2);
            }
            foreach (var ellipse in Ellipses)
            {
                dc.DrawEllipse(ellipse.brush, ellipse.pen, ellipse.center, ellipse.radiusX, ellipse.radiusY);
            }
            foreach (var rect in Rectangles)
            {
                dc.DrawRectangle(rect.brush, rect.pen, rect.rectangle);
            }
            foreach (var text in Texts)
            {
                dc.DrawText(text.formattedText, text.origin);
            }
            foreach (var region in Regions)
            {
                dc.DrawGeometry(region.brush, region.pen, region.geometry);
            }
        }

        public void DrawText(string text, Point origin, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, int fontSize, Brush fontBrush, bool isRender = true)
        {
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            Texts.Add(new SDrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal), fontSize, fontBrush, graphics.DpiX / 96f), origin));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawRegion(List<Point> points, Brush brush, Pen pen, bool isRender = true)
        {
            PathGeometry geometry = new PathGeometry();
            PolyLineSegment polyLineSegment = new PolyLineSegment();
            polyLineSegment.Points = new PointCollection(points);
            PathFigure figure = new PathFigure(points[0], new[] { polyLineSegment }, false);
            geometry.Figures.Add(figure);
            Regions.Add(new SDrawRegion(brush, pen, geometry));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2, bool isRender = true)
        {
            Lines.Add(new SDrawLine(pen, pt1, pt2));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY, bool isRender = true)
        {
            Ellipses.Add(new SDrawEllipse(brush, pen, center, radiusX, radiusY));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawRectangle(Brush brush, Pen pen, Rect rectangle, bool isRender = true)
        {
            Rectangles.Add(new SDrawRectangle(brush, pen, rectangle));
            if (isRender) this.InvalidateVisual();
        }

        public void Invalidate()
        {
            this.InvalidateVisual();
        }

        public void Clear()
        {
            Lines.Clear();
            Ellipses.Clear();
            Rectangles.Clear();
            Texts.Clear();
            Regions.Clear();

            this.InvalidateVisual();
        }
    }
}
