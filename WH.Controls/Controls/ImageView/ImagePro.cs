using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    public record SDrawLine(Pen pen, Point pt1, Point pt2);
    public record SDrawEllipse(Pen pen, Point center, double radiusX, double radiusY);
    public record SDrawRectangle(Pen pen, Rect rectangle);
    public record SDrawText(FormattedText formattedText, int thickness, Point origin);
    public record SDrawTextAlignment(FormattedText formattedText, int thickness, AlignmentX alignmentX, AlignmentY alignmentY);
    public record SDrawRegion(Pen pen, List<Point> points);
    public class ImagePro : Image
    {
        public ImagePro() : base()
        {
            Stretch = Stretch.Uniform;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children = new TransformCollection(new Transform[] { scaleTransform, translateTransform });
            this.RenderTransform = transformGroup;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            this.MouseMove += ImagePro_MouseMove1;
            this.MouseWheel += ImagePro_MouseWheel;
            this.MouseLeftButtonUp += ImagePro_MouseLeftButtonUp;
            this.MouseLeftButtonDown += ImagePro_MouseLeftButtonDown;
        }

        private BitmapSource bitmapImage => this.Source is BitmapSource ? (BitmapSource)this.Source : null;
        public int ImageWidth => bitmapImage == null ? 0 : bitmapImage.PixelWidth;
        public int ImageHeight => bitmapImage == null ? 0 : bitmapImage.PixelHeight;
        public ScaleTransform scaleTransform { get; set; } = new ScaleTransform();
        public TranslateTransform translateTransform { get; set; } = new TranslateTransform();
        public DateTime LBtnDownTime { get; set; }
        public Point LastPos { get; set; }
        public List<SDrawLine> Lines { get; set; } = new List<SDrawLine>();
        public List<SDrawEllipse> Ellipses { get; set; } = new List<SDrawEllipse>();
        public List<SDrawRectangle> Rectangles { get; set; } = new List<SDrawRectangle>();
        public List<SDrawText> Texts { get; set; } = new List<SDrawText>();
        public List<SDrawTextAlignment> TextAlignments { get; set; } = new List<SDrawTextAlignment>();
        public List<SDrawRegion> Regions { get; set; } = new List<SDrawRegion>();
        private Pen _Pen { get; set; } = new Pen(Brushes.Red, 1);
        private List<(Pen, double)> _Pens { get; set; } = new List<(Pen, double)>();
        private FontFamily _FontFamily { get; set; } = new FontFamily("宋体");
        private FontStyle _FontStyle { get; set; } = FontStyles.Normal;
        private FontWeight _FontWeight { get; set; } = FontWeights.Normal;
        private int _FontSize { get; set; } = 15;
        private Brush _FontBrush { get; set; } = Brushes.Red;

        public bool FillEdge
        {
            get { return (bool)GetValue(FillEdgeProperty); }
            set { SetValue(FillEdgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillEdge.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillEdgeProperty =
            DependencyProperty.Register("FillEdge", typeof(bool), typeof(ImagePro), new PropertyMetadata(false));

        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register("MinScale", typeof(double), typeof(ImagePro), new PropertyMetadata(0.5d));

        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register("MaxScale", typeof(double), typeof(ImagePro), new PropertyMetadata(100.0d));

        public string R
        {
            get { return (string)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(string), typeof(ImagePro));

        public string G
        {
            get { return (string)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(string), typeof(ImagePro));

        public string B
        {
            get { return (string)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(string), typeof(ImagePro));

        public int X
        {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(int), typeof(ImagePro), new PropertyMetadata(0));

        public int Y
        {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(int), typeof(ImagePro), new PropertyMetadata(0));


        private void ImagePro_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            LastPos = e.GetPosition(image);
        }

        private void ImagePro_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - LBtnDownTime).TotalMilliseconds < 300)
            {
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
                translateTransform.X = 0;
                translateTransform.Y = 0;
                OnScaleChanged();
            }
            LBtnDownTime = DateTime.Now;
        }

        private void ImagePro_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) return;
            var image = (Image)sender;
            Point pos = e.GetPosition(image);
            //当RenderTransformOrigin为0.5，0.5时坐标中心为图像中心，需减去视窗宽度/2
            double oldScaleCenterX = scaleTransform.CenterX;
            double oldScaleCenterY = scaleTransform.CenterY;
            double oldScale = scaleTransform.ScaleX;
            scaleTransform.CenterX = pos.X;
            scaleTransform.CenterY = pos.Y;
            double scale = scaleTransform.ScaleX * (e.Delta > 0 ? 1.5 : 0.8);
            scale = Math.Max(MinScale, scale);
            scale = Math.Min(MaxScale, scale);
            scaleTransform.ScaleX = scaleTransform.ScaleY = scale;
            double dOffestX = (scaleTransform.CenterX - oldScaleCenterX) * (oldScale - 1);
            double dOffestY = (scaleTransform.CenterY - oldScaleCenterY) * (oldScale - 1);
            translateTransform.X += dOffestX;
            translateTransform.Y += dOffestY;

            OnScaleChanged();
        }

        private void ImagePro_MouseMove1(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            Point pos = e.GetPosition(image);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                translateTransform.X += (pos.X - LastPos.X) * scaleTransform.ScaleX;
                translateTransform.Y += (pos.Y - LastPos.Y) * scaleTransform.ScaleX;
                LastPos = e.GetPosition(image);
            }

            int x = (int)(pos.X * ImageWidth / image.DesiredSize.Width);
            int y = (int)(pos.Y * ImageHeight / image.DesiredSize.Height);
            if (bitmapImage != null && x >= 0 && x < ImageWidth && y >= 0 && y < ImageHeight)
            {
                X = x;
                Y = y;

                int stride = ImageWidth * ((bitmapImage.Format.BitsPerPixel + 7) / 8);
                var bits = bitmapImage.Format.BitsPerPixel / 8;
                byte[] data = new byte[bits];
                bitmapImage.CopyPixels(new Int32Rect(x, y, 1, 1), data, bits, 0);
                if (bits >= 3)
                {
                    B = data[0].ToString("B: 0");
                    G = data[1].ToString("G: 0");
                    R = data[2].ToString("R: 0");
                }
                else
                {
                    R = data[0].ToString();
                    G = string.Empty;
                    B = string.Empty;
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var item in _Pens)
            {
                item.Item1.Thickness = item.Item2 / scaleTransform.ScaleX;
            }

            double dRatio = this.ActualWidth / ImageWidth;
            foreach (var line in Lines)
            {
                dc.DrawLine(line.pen, new Point(line.pt1.X * dRatio, line.pt1.Y * dRatio), new Point(line.pt2.X * dRatio, line.pt2.Y * dRatio));
            }
            foreach (var ellipse in Ellipses)
            {
                dc.DrawEllipse(FillEdge ? ellipse.pen.Brush : Brushes.Transparent, ellipse.pen, new Point(ellipse.center.X * dRatio, ellipse.center.Y * dRatio), ellipse.radiusX * dRatio, ellipse.radiusY * dRatio);
            }
            foreach (var rect in Rectangles)
            {
                dc.DrawRectangle(FillEdge ? rect.pen.Brush : Brushes.Transparent, rect.pen, new Rect(rect.rectangle.X * dRatio, rect.rectangle.Y * dRatio, rect.rectangle.Width * dRatio, rect.rectangle.Height * dRatio));
            }
            foreach (var region in Regions)
            {
                List<Point> tmp = new List<Point>();
                foreach (var item in region.points)
                {
                    tmp.Add(new Point(item.X * dRatio, item.Y * dRatio));
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
                text.formattedText.SetFontSize(text.thickness / scaleTransform.ScaleX);
                dc.DrawText(text.formattedText, new Point(text.origin.X * dRatio, text.origin.Y * dRatio));
            }
            foreach (var text in TextAlignments)
            {
                text.formattedText.SetFontSize(text.thickness / scaleTransform.ScaleX);
                double x = 20, y = 20;
                switch (text.alignmentX)
                {
                    case AlignmentX.Center:
                        x = ImageWidth / 2 - text.formattedText.Width / dRatio / 2;
                        break;
                    case AlignmentX.Right:
                        x = ImageWidth - text.formattedText.Width / dRatio - 20;
                        break;
                }
                switch (text.alignmentY)
                {
                    case AlignmentY.Center:
                        y = ImageHeight / 2 - text.formattedText.Height / dRatio / 2;
                        break;
                    case AlignmentY.Bottom:
                        y = ImageHeight - text.formattedText.Height / dRatio - 20;
                        break;
                }
                dc.DrawText(text.formattedText, new Point(x * dRatio, y * dRatio));
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
            if (bitmapImage != null)
            {
                Texts.Add(new SDrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(_FontFamily, _FontStyle, _FontWeight, FontStretches.Normal), _FontSize, _FontBrush, bitmapImage.DpiX / 96f), _FontSize, origin));
                if (isRender) this.InvalidateVisual();
            }

        }

        public void DrawText(string text, AlignmentX alignmentX, AlignmentY alignmentY, bool isRender = true)
        {
            if (bitmapImage != null)
            {
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

        public void OnScaleChanged()
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
            _Pens.Clear();

            this.InvalidateVisual();
        }
    }
}
