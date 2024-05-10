using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace ImageWinCtrl
{
    public class ImagePro : Image
    {
        public ImagePro() : base()
        {
            Stretch = System.Windows.Media.Stretch.Uniform;

            System.Windows.Media.TransformGroup transformGroup = new System.Windows.Media.TransformGroup();
            transformGroup.Children = new System.Windows.Media.TransformCollection(new System.Windows.Media.Transform[] { scaleTransform, translateTransform });
            this.RenderTransform = transformGroup;
            System.Windows.Media.RenderOptions.SetBitmapScalingMode(this, System.Windows.Media.BitmapScalingMode.NearestNeighbor);

            this.MouseMove += ImagePro_MouseMove1;
            this.MouseWheel += ImagePro_MouseWheel;
            this.MouseLeftButtonUp += ImagePro_MouseLeftButtonUp;
            this.MouseLeftButtonDown += ImagePro_MouseLeftButtonDown;
            this.Loaded += ImagePro_Loaded;
        }

        private void ImagePro_Loaded(object sender, RoutedEventArgs e)
        {
            ImageOne = this;
        }

        private WriteableBitmap? WriteableBmp;

        private Bitmap? GraphicsBmp;

        private Graphics? GraphicsOne;

        public System.Windows.Media.ScaleTransform scaleTransform { get; set; } = new System.Windows.Media.ScaleTransform();
        public System.Windows.Media.TranslateTransform translateTransform { get; set; } = new System.Windows.Media.TranslateTransform();
        public DateTime LBtnDownTime { get; set; }
        public System.Windows.Point LastPos { get; set; }

        public int ImageWidth
        {
            get
            {
                return BitmapSource != null ? BitmapSource.Width : 0;
            }
        }

        public int ImageHeight
        {
            get
            {
                return BitmapSource != null ? BitmapSource.Height : 0;
            }
        }

        [TypeConverter(typeof(BitmapConverter))]
        public Bitmap BitmapSource
        {
            get { return (Bitmap)GetValue(BitmapSourceProperty); }
            set { SetValue(BitmapSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitmapSourceProperty =
            DependencyProperty.Register("BitmapSource", typeof(Bitmap), typeof(ImagePro), new PropertyMetadata(null, (d, e) => {
                if (e.NewValue != null)
                {
                    ImagePro imagePro = (ImagePro)d;
                    if (e.OldValue != null && imagePro.NeedDispose) ((Bitmap)e.OldValue).Dispose();
                    Bitmap BitmapSource = (Bitmap)e.NewValue;
                    if (imagePro.WriteableBmp == null || imagePro.GraphicsBmp == null || imagePro.GraphicsOne == null || 
                    imagePro.WriteableBmp.PixelWidth != BitmapSource.Width || imagePro.WriteableBmp.PixelHeight != BitmapSource.Height)
                    {
                        imagePro.WriteableBmp = new WriteableBitmap(BitmapSource.Width, BitmapSource.Height, BitmapSource.HorizontalResolution,
                        BitmapSource.VerticalResolution, System.Windows.Media.PixelFormats.Bgr24, null);
                        imagePro.Source = imagePro.WriteableBmp;
                        if (imagePro.GraphicsBmp != null) imagePro.GraphicsBmp.Dispose();
                        imagePro.GraphicsBmp = new Bitmap(BitmapSource.Width, BitmapSource.Height, imagePro.WriteableBmp.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, imagePro.WriteableBmp.BackBuffer);
                        if(imagePro.GraphicsOne != null) imagePro.GraphicsOne.Dispose();
                        imagePro.GraphicsOne = Graphics.FromImage(imagePro.GraphicsBmp);
                    }
                    imagePro.WriteableBmp.Lock();
                    imagePro.GraphicsOne.DrawImage(BitmapSource, new PointF(0, 0));
                    imagePro.GraphicsOne.Flush();
                    imagePro.WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, imagePro.WriteableBmp.PixelWidth, imagePro.WriteableBmp.PixelHeight));
                    imagePro.WriteableBmp.Unlock();
                }
            }));

        public string StringSource
        {
            get { return (string)GetValue(StringSourceProperty); }
            set { SetValue(StringSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringSourceProperty =
            DependencyProperty.Register("StringSource", typeof(string), typeof(ImagePro), new PropertyMetadata("", (d, e) => {
                ImagePro imagePro = (ImagePro)d;
                imagePro.BitmapSource = new Bitmap((string)e.NewValue);
            }));



        public ImagePro ImageOne
        {
            get { return (ImagePro)GetValue(ImageOneProperty); }
            set { SetValue(ImageOneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageOne.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageOneProperty =
            DependencyProperty.Register("ImageOne", typeof(ImagePro), typeof(ImagePro), new PropertyMetadata(null));



        public bool NeedDispose
        {
            get { return (bool)GetValue(NeedDisposeProperty); }
            set { SetValue(NeedDisposeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NeedDispose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NeedDisposeProperty =
            DependencyProperty.Register("NeedDispose", typeof(bool), typeof(ImagePro), new PropertyMetadata(true));

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

        public int R
        {
            get { return (int)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(int), typeof(ImagePro), new PropertyMetadata(0));

        public int G
        {
            get { return (int)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        // Using a DependencyProperty as the backing store for G.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(int), typeof(ImagePro), new PropertyMetadata(0));

        public int B
        {
            get { return (int)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(int), typeof(ImagePro), new PropertyMetadata(0));

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
            if (BitmapSource != null)
            {
                var image = (Image)sender;
                LastPos = e.GetPosition(image);
            }
        }

        private void ImagePro_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - LBtnDownTime).TotalMilliseconds < 300)
            {
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
                translateTransform.X = 0;
                translateTransform.Y = 0;
            }
            LBtnDownTime = DateTime.Now;
        }

        private void ImagePro_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) return;
            var image = (Image)sender;
            System.Windows.Point pos = e.GetPosition(image);
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
        }

        private void ImagePro_MouseMove1(object sender, MouseEventArgs e)
        {
            if (BitmapSource != null)
            {
                var image = (Image)sender;
                System.Windows.Point pos = e.GetPosition(image);
                double x = pos.X * BitmapSource.Width / image.DesiredSize.Width;
                double y = pos.Y * BitmapSource.Height / image.DesiredSize.Height;
                if (BitmapSource != null && x >= 0 && x < BitmapSource.Width && y >= 0 && y < BitmapSource.Height)
                {
                    X = (int)x;
                    Y = (int)y;
                    System.Drawing.Color color = BitmapSource.GetPixel(X, Y);
                    R = color.R;
                    G = color.G;
                    B = color.B;
                }
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    translateTransform.X += (pos.X - LastPos.X) * scaleTransform.ScaleX;
                    translateTransform.Y += (pos.Y - LastPos.Y) * scaleTransform.ScaleX;
                    LastPos = e.GetPosition(image);
                }
            }
        }

        public void DrawText(string text, Font font, Brush brush, float x, float y, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                GraphicsOne.DrawString(text, font, brush, x, y);
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void DrawText(string text, Font font, Brush brush, ContentAlignment alignment, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                var size = GraphicsOne.MeasureString(text, font);
                switch (alignment)
                {
                    case ContentAlignment.TopLeft:
                        GraphicsOne.DrawString(text, font, brush, 20, 20);
                        break;
                    case ContentAlignment.TopCenter:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth / 2 - size.Width / 2, 20);
                        break;
                    case ContentAlignment.TopRight:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth - 20 - size.Width, 20);
                        break;
                    case ContentAlignment.MiddleLeft:
                        GraphicsOne.DrawString(text, font, brush, 20, ImageHeight / 2 - size.Height / 2);
                        break;
                    case ContentAlignment.MiddleCenter:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth / 2 - size.Width / 2, ImageHeight / 2 - size.Height / 2);
                        break;
                    case ContentAlignment.MiddleRight:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth - 20 - size.Width, ImageHeight / 2 - size.Height / 2);
                        break;
                    case ContentAlignment.BottomLeft:
                        GraphicsOne.DrawString(text, font, brush, 20, ImageHeight - 20 - size.Height);
                        break;
                    case ContentAlignment.BottomCenter:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth / 2 - size.Width / 2, ImageHeight - 20 - size.Height);
                        break;
                    case ContentAlignment.BottomRight:
                        GraphicsOne.DrawString(text, font, brush, ImageWidth - 20 - size.Width, ImageHeight - 20 - size.Height);
                        break;
                    default:
                        break;
                }
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void DrawEdge(List<System.Drawing.Point> points, Color color, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsBmp != null)
            {
                WriteableBmp.Lock();
                foreach (var item in points)
                {
                    GraphicsBmp.SetPixel(item.X, item.Y, color);
                }
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void DrawLine(Pen pen, PointF pt1, PointF pt2, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                GraphicsOne.DrawLine(pen, pt1, pt2);
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void DrawEllipse(Pen pen, float x, float y, float width, float height, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                GraphicsOne.DrawEllipse(pen, x, y, width, height);
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height, bool isRender = true)
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                GraphicsOne.DrawRectangle(pen, x, y, width, height);
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

        public void Clear()
        {
            Invalidate();
        }

        public void Invalidate()
        {
            if (WriteableBmp != null && GraphicsOne != null)
            {
                WriteableBmp.Lock();
                GraphicsOne.DrawImage(BitmapSource, new PointF(0, 0));
                GraphicsOne.Flush();
                WriteableBmp.AddDirtyRect(new Int32Rect(0, 0, WriteableBmp.PixelWidth, WriteableBmp.PixelHeight));
                WriteableBmp.Unlock();
            }
        }

    }

    public class BitmapConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value != null)
            {
                string str = (string)value;
                return new Bitmap(str);
            }
            return null;
        }
    }
}
