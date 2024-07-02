using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : UserControl
    {
        public ImageView()
        {
            InitializeComponent();
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageView), new PropertyMetadata(null, (d, e) =>
            {
                if (e.NewValue != null)
                {
                    
                    ImageView view = (ImageView)d;
                    //view.Image.Source = (BitmapSource)e.NewValue;
                    //view.Canvas.Source = (BitmapSource)e.NewValue;
                    view.updateImg((BitmapSource)e.NewValue);
                }
            }));

        public int ImageWidth => Source == null ? 0 : Source.PixelWidth;
        public int ImageHeight => Source == null ? 0 : Source.PixelHeight;

        public void updateImg(BitmapSource img)
        {
            Image.Source = img;
            Canvas.Source = img;
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        public void SetPen(Brush brush, double thickness)
        {
            this.Image.SetPen(brush, thickness);
            this.Canvas.SetPen(brush, thickness);
        }

        public void SetFontFamily(FontFamily fontFamily)
        {
            this.Image.SetFontFamily(fontFamily);
            this.Canvas.SetFontFamily(fontFamily);
        }

        public void SetFontStyle(FontStyle fontStyle)
        {
            this.Image.SetFontStyle(fontStyle);
            this.Canvas.SetFontStyle(fontStyle);
        }

        public void SetFontWeight(FontWeight fontWeight)
        {
            this.Image.SetFontWeight(fontWeight);
            this.Canvas.SetFontWeight(fontWeight);
        }

        public void SetFontSize(int fontSize)
        {
            this.Image.SetFontSize(fontSize);
            this.Canvas.SetFontSize(fontSize);
        }

        public void SetFontBrush(Brush fontBrush)
        {
            this.Image.SetFontBrush(fontBrush);
            this.Canvas.SetFontBrush(fontBrush);
        }

        public void ImgDrawRegion(List<Point> points, bool isRender = true)
        {
            this.Image.DrawRegion(points, isRender);
        }

        public void ImgDrawPoints(List<Point> points, bool isRender = true)
        {
            this.Image.DrawPoints(points, isRender);
        }

        public void ImgDrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            this.Image.DrawLine(pt1, pt2, isRender);
        }

        public void ImgDrawEllipse(Point center, double radiusX, double radiusY, bool isRender = true)
        {
            this.Image.DrawEllipse(center, radiusX, radiusY, isRender);
        }

        public void ImgDrawRectangle(Rect rectangle, bool isRender = true)
        {
            this.Image.DrawRectangle(rectangle, isRender);
        }

        public void ImgDrawText(string text, Point origin, bool isRender = true)
        {
            this.Image.DrawText(text, origin, isRender);
        }

        public void ImgDrawText(string text, AlignmentX alignmentX, AlignmentY alignmentY, bool isRender = true)
        {
            this.Image.DrawText(text, alignmentX, alignmentY, isRender);
        }

        public void WinDrawRegion(List<Point> points, bool isRender = true)
        {
            this.Canvas.DrawRegion(points, isRender);
        }

        public void WinDrawPoints(List<Point> points, bool isRender = true)
        {
            this.Canvas.DrawPoints(points, isRender);
        }

        public void WinDrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            this.Canvas.DrawLine(pt1, pt2, isRender);
        }

        public void WinDrawEllipse(Point center, double radiusX, double radiusY, bool isRender = true)
        {
            this.Canvas.DrawEllipse(center, radiusX, radiusY, isRender);
        }

        public void WinDrawRectangle(Rect rectangle, bool isRender = true)
        {
            this.Canvas.DrawRectangle(rectangle, isRender);
        }

        public void WinDrawText(string text, Point origin, bool isRender = true)
        {
            this.Canvas.DrawText(text, origin, isRender);
        }

        public void WinDrawText(string text, AlignmentX alignmentX, AlignmentY alignmentY, bool isRender = true)
        {
            this.Canvas.DrawText(text, alignmentX, alignmentY, isRender);
        }

        public void Clear()
        {
            this.Image.Clear();
            this.Canvas.Clear();
        }

        public void Invalidate()
        {
            this.Image.InvalidateVisual();
            this.Canvas.InvalidateVisual();
        }
    }
}
