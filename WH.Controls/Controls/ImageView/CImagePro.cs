using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.8 李焕彬
    /// 直线绘制
    /// </summary>
    /// <param name="pen">画笔</param>
    /// <param name="pt1">点1</param>
    /// <param name="pt2">点2</param>
    public record SDrawLine(Pen pen, Point pt1, Point pt2);

    /// <summary>
    /// 2024.7.8 李焕彬
    /// 椭圆绘制
    /// </summary>
    /// <param name="pen">画笔</param>
    /// <param name="center">中心</param>
    /// <param name="radiusX">X半径</param>
    /// <param name="radiusY">Y半径</param>
    public record SDrawEllipse(Pen pen, Point center, double radiusX, double radiusY);

    /// <summary>
    /// 2024.7.8 李焕彬
    /// 矩形绘制
    /// </summary>
    /// <param name="pen">画笔</param>
    /// <param name="rectangle">矩形</param>
    public record SDrawRectangle(Pen pen, Rect rectangle);

    /// <summary>
    /// 2024.7.8 李焕彬
    /// 文字绘制
    /// </summary>
    /// <param name="formattedText">文字</param>
    /// <param name="thickness">厚度</param>
    /// <param name="origin">位置</param>
    public record SDrawText(FormattedText formattedText, int thickness, Point origin);

    /// <summary>
    /// 2024.7.8 李焕彬
    /// 文字绘制
    /// </summary>
    /// <param name="formattedText">文字</param>
    /// <param name="thickness">厚度</param>
    /// <param name="alignmentX">X对齐</param>
    /// <param name="alignmentY">Y对齐</param>
    public record SDrawTextAlignment(
        FormattedText formattedText,
        int thickness,
        AlignmentX alignmentX,
        AlignmentY alignmentY
    );

    /// <summary>
    /// 2024.7.8 李焕彬
    /// 区域绘制
    /// </summary>
    /// <param name="pen">画笔</param>
    /// <param name="points">点集</param>
    public record SDrawRegion(Pen pen, List<Point> points);

    /// <summary>
    /// 2024.7.4 李焕彬
    /// 图像坐标绘图
    /// </summary>
    public class CImagePro : Image
    {
        public CImagePro()
            : base()
        {
            Stretch = Stretch.Uniform;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children = new TransformCollection(
                new Transform[] { scaleTransform, translateTransform }
            );
            this.RenderTransform = transformGroup;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            this.MouseMove += ImagePro_MouseMove1;
            this.MouseWheel += ImagePro_MouseWheel;
            this.MouseLeftButtonUp += ImagePro_MouseLeftButtonUp;
            this.MouseLeftButtonDown += ImagePro_MouseLeftButtonDown;
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像
        /// </summary>
        private BitmapSource bitmapImage =>
            this.Source is BitmapSource ? (BitmapSource)this.Source : null;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像宽度
        /// </summary>
        public int ImageWidth => bitmapImage == null ? 0 : bitmapImage.PixelWidth;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像高度
        /// </summary>
        public int ImageHeight => bitmapImage == null ? 0 : bitmapImage.PixelHeight;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 缩放转换
        /// </summary>
        public ScaleTransform scaleTransform { get; set; } = new ScaleTransform();

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 坐标转换
        /// </summary>
        public TranslateTransform translateTransform { get; set; } = new TranslateTransform();

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标左键按下时间
        /// </summary>
        private DateTime LBtnDownTime { get; set; }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 上一个位置
        /// </summary>
        private Point LastPos { get; set; }

        /// <summary>
        /// 2024.7.26 李焕彬
        /// 绘图互斥锁,防止绘制时添加删除
        /// </summary>
        private Object lockDraw = new object();

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
        public List<SDrawTextAlignment> TextAlignments { get; set; } =
            new List<SDrawTextAlignment>();

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
        public bool FillEdge
        {
            get { return (bool)GetValue(FillEdgeProperty); }
            set { SetValue(FillEdgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillEdge.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillEdgeProperty = DependencyProperty.Register(
            "FillEdge",
            typeof(bool),
            typeof(CImagePro),
            new PropertyMetadata(false)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 最小缩放
        /// </summary>
        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale",
            typeof(double),
            typeof(CImagePro),
            new PropertyMetadata(0.5d)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 最大缩放
        /// </summary>
        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale",
            typeof(double),
            typeof(CImagePro),
            new PropertyMetadata(100.0d)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动红色分量
        /// </summary>
        public string R
        {
            get { return (string)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RProperty = DependencyProperty.Register(
            "R",
            typeof(string),
            typeof(CImagePro)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动绿色分量
        /// </summary>
        public string G
        {
            get { return (string)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GProperty = DependencyProperty.Register(
            "G",
            typeof(string),
            typeof(CImagePro)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动蓝色分量
        /// </summary>
        public string B
        {
            get { return (string)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty = DependencyProperty.Register(
            "B",
            typeof(string),
            typeof(CImagePro)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动x图像坐标
        /// </summary>
        public int X
        {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(int),
            typeof(CImagePro),
            new PropertyMetadata(0)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动Y图像坐标
        /// </summary>
        public int Y
        {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(int),
            typeof(CImagePro),
            new PropertyMetadata(0)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标左键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImagePro_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            LastPos = e.GetPosition(image);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标左键抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标滚轮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImagePro_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                return;
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

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 刷新图像重载
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Draw(dc, this.ActualWidth, scaleTransform.ScaleX);
        }

        /// <summary>
        /// 2024.7.29 李焕彬
        /// 在dc尚绘制图形
        /// </summary>
        /// <param name="drawingContext">目标dc</param>
        /// <param name="width">绘图窗口宽度</param>
        /// <param name="scale">窗口缩放比例</param>
        /// <param name="fontSize">字体大小，=0时用默认字体大小</param>
        public void Draw(DrawingContext dc, double width, double scale, int fontSize = 0)
        {
            lock (lockDraw)
            {
                foreach (var item in _Pens)
                {
                    item.Item1.Thickness = item.Item2 / scale;
                }

                double dRatio = width / ImageWidth;
                foreach (var line in Lines)
                {
                    dc.DrawLine(
                        line.pen,
                        new Point(line.pt1.X * dRatio, line.pt1.Y * dRatio),
                        new Point(line.pt2.X * dRatio, line.pt2.Y * dRatio)
                    );
                }
                foreach (var ellipse in Ellipses)
                {
                    dc.DrawEllipse(
                        FillEdge ? ellipse.pen.Brush : Brushes.Transparent,
                        ellipse.pen,
                        new Point(ellipse.center.X * dRatio, ellipse.center.Y * dRatio),
                        ellipse.radiusX * dRatio,
                        ellipse.radiusY * dRatio
                    );
                }
                foreach (var rect in Rectangles)
                {
                    dc.DrawRectangle(
                        FillEdge ? rect.pen.Brush : Brushes.Transparent,
                        rect.pen,
                        new Rect(
                            rect.rectangle.X * dRatio,
                            rect.rectangle.Y * dRatio,
                            rect.rectangle.Width * dRatio,
                            rect.rectangle.Height * dRatio
                        )
                    );
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

                    dc.DrawGeometry(
                        FillEdge ? region.pen.Brush : Brushes.Transparent,
                        region.pen,
                        geometry
                    );
                }
                foreach (var text in Texts)
                {
                    text.formattedText.SetFontSize(
                        fontSize == 0 ? text.thickness / scale : fontSize
                    );
                    dc.DrawText(
                        text.formattedText,
                        new Point(text.origin.X * dRatio, text.origin.Y * dRatio)
                    );
                }
                foreach (var text in TextAlignments)
                {
                    text.formattedText.SetFontSize(
                        fontSize == 0 ? text.thickness / scale : fontSize
                    );
                    double x = 20,
                        y = 20;
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
            lock (lockDraw)
            {
                if (bitmapImage != null)
                {
                    Texts.Add(
                        new SDrawText(
                            new FormattedText(
                                text,
                                CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(
                                    _FontFamily,
                                    _FontStyle,
                                    _FontWeight,
                                    FontStretches.Normal
                                ),
                                _FontSize,
                                _FontBrush,
                                bitmapImage.DpiX / 96f
                            ),
                            _FontSize,
                            origin
                        )
                    );
                }
            }
            if (isRender)
                this.InvalidateVisual();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="alignmentX">Y对齐</param>
        /// <param name="alignmentY">X对齐</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawText(
            string text,
            AlignmentX alignmentX,
            AlignmentY alignmentY,
            bool isRender = true
        )
        {
            lock (lockDraw)
            {
                if (bitmapImage != null)
                {
                    TextAlignments.Add(
                        new SDrawTextAlignment(
                            new FormattedText(
                                text,
                                CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(
                                    _FontFamily,
                                    _FontStyle,
                                    _FontWeight,
                                    FontStretches.Normal
                                ),
                                _FontSize,
                                _FontBrush,
                                bitmapImage.DpiX / 96f
                            ),
                            _FontSize,
                            alignmentX,
                            alignmentY
                        )
                    );
                }
            }
            if (isRender)
                this.InvalidateVisual();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制连贯区域
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawRegion(List<Point> points, bool isRender = true)
        {
            lock (lockDraw)
            {
                if (points.Count == 0)
                    return;

                if (!_Pens.Exists(e => e.Item1 == _Pen))
                    _Pens.Add((_Pen, _Pen.Thickness));

                Regions.Add(new SDrawRegion(_Pen, points));
            }
            if (isRender)
                this.InvalidateVisual();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制不连贯区域
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawPoints(List<Point> points, bool isRender = true)
        {
            lock (lockDraw)
            {
                if (points.Count == 0)
                    return;

                if (!_Pens.Exists(e => e.Item1 == _Pen))
                    _Pens.Add((_Pen, _Pen.Thickness));

                List<Point> region = new List<Point>();
                foreach (var item in points)
                {
                    if (
                        region.Count > 0
                        && Math.Sqrt(
                            (region.Last().X - item.X) * (region.Last().X - item.X)
                                + (region.Last().Y - item.Y) * (region.Last().Y - item.Y)
                        ) > 2
                    )
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
            }
            if (isRender)
                this.InvalidateVisual();
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
            lock (lockDraw)
            {
                if (!_Pens.Exists(e => e.Item1 == _Pen))
                    _Pens.Add((_Pen, _Pen.Thickness));

                Lines.Add(new SDrawLine(_Pen, pt1, pt2));
            }
            if (isRender)
                this.InvalidateVisual();
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
            lock (lockDraw)
            {
                if (!_Pens.Exists(e => e.Item1 == _Pen))
                    _Pens.Add((_Pen, _Pen.Thickness));

                Ellipses.Add(new SDrawEllipse(_Pen, center, radiusX, radiusY));
            }
            if (isRender)
                this.InvalidateVisual();
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="rectangle">待绘制矩形</param>
        /// <param name="isRender">是否刷新</param>
        public void DrawRectangle(Rect rectangle, bool isRender = true)
        {
            lock (lockDraw)
            {
                if (!_Pens.Exists(e => e.Item1 == _Pen))
                    _Pens.Add((_Pen, _Pen.Thickness));

                Rectangles.Add(new SDrawRectangle(_Pen, rectangle));
            }
            if (isRender)
                this.InvalidateVisual();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 缩放变化更新绘制
        /// </summary>
        public void OnScaleChanged()
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 清除显示
        /// </summary>
        public void Clear()
        {
            lock (lockDraw)
            {
                Lines.Clear();
                Ellipses.Clear();
                Rectangles.Clear();
                Texts.Clear();
                TextAlignments.Clear();
                Regions.Clear();
                _Pens.Clear();
            }
            this.InvalidateVisual();
        }
    }
}
