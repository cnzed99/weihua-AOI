using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageWinCtrl
{
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
        public List<SDrawTextAlignment> TextAlignments { get; set; } = new List<SDrawTextAlignment>();
        public List<SDrawRegion> Regions { get; set; } = new List<SDrawRegion>();

        public CanvasPro()
        {
            this.Background = Brushes.Transparent;
            this.IsHitTestVisible = false;
            this.Loaded += CanvasPro_Loaded;
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(CanvasPro), new PropertyMetadata(null));

        private void CanvasPro_Loaded(object sender, RoutedEventArgs e)
        {
            CanvasOne = this;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var line in Lines)
            {
                line.pen.Brush = line.brushPen;
                line.pen.Thickness = line.thickness;
                dc.DrawLine(line.pen, new Point(line.pt1.X, line.pt1.Y), new Point(line.pt2.X, line.pt2.Y));
            }
            foreach (var ellipse in Ellipses)
            {
                ellipse.pen.Brush = ellipse.brushPen;
                ellipse.pen.Thickness = ellipse.thickness;
                dc.DrawEllipse(ellipse.brush, ellipse.pen, new Point(ellipse.center.X, ellipse.center.Y), ellipse.radiusX, ellipse.radiusY);
            }
            foreach (var rect in Rectangles)
            {
                rect.pen.Brush = rect.brushPen;
                rect.pen.Thickness = rect.thickness;
                dc.DrawRectangle(rect.brush, rect.pen, new Rect(rect.rectangle.X, rect.rectangle.Y, rect.rectangle.Width, rect.rectangle.Height));
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
            foreach (var region in Regions)
            {
                region.pen.Brush = region.brushPen;
                region.pen.Thickness = region.thickness;
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

                dc.DrawGeometry(region.brush, region.pen, geometry);
            }
        }

        public void DrawText(string text, Point origin, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, int fontSize, Brush fontBrush, bool isRender = true)
        {
            if (Source != null && Source is BitmapSource)
            {
                BitmapSource bitmapImage = (BitmapSource)Source;
                Texts.Add(new SDrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal), fontSize, fontBrush, bitmapImage.DpiX / 96f), fontSize, origin));
                if (isRender) this.InvalidateVisual();
            }

        }

        public void DrawText(string text, AlignmentX alignmentX, AlignmentY alignmentY, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, int fontSize, Brush fontBrush, bool isRender = true)
        {
            if (Source != null && Source is BitmapSource)
            {
                BitmapSource bitmapImage = (BitmapSource)Source;
                TextAlignments.Add(new SDrawTextAlignment(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal), fontSize, fontBrush, bitmapImage.DpiX / 96f), fontSize, alignmentX, alignmentY));
                if (isRender) this.InvalidateVisual();
            }
        }

        public void DrawRegion(List<Point> points, Brush brush, Brush brushPen, int thicknessPen, bool isRender = true)
        {
            if (points.Count == 0) return;
            Regions.Add(new SDrawRegion(brush, new Pen(), brushPen, thicknessPen, points));

            if (isRender) this.InvalidateVisual();
        }

        public void DrawLine(Brush brushPen, int thicknessPen, Point pt1, Point pt2, bool isRender = true)
        {
            Lines.Add(new SDrawLine(new Pen(), brushPen, thicknessPen, pt1, pt2));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawEllipse(Brush brush, Brush brushPen, int thicknessPen, Point center, double radiusX, double radiusY, bool isRender = true)
        {
            Ellipses.Add(new SDrawEllipse(brush, new Pen(), brushPen, thicknessPen, center, radiusX, radiusY));
            if (isRender) this.InvalidateVisual();
        }

        public void DrawRectangle(Brush brush, Brush brushPen, int thicknessPen, Rect rectangle, bool isRender = true)
        {
            Rectangles.Add(new SDrawRectangle(brush, new Pen(), brushPen, thicknessPen, rectangle));
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
            TextAlignments.Clear();
            Regions.Clear();

            this.InvalidateVisual();
        }
    }
}
