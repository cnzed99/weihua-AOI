using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HandyControl.Controls;
using Microsoft.Win32;

namespace WH.Controls
{
    /// <summary>
    /// 2024.7.8 李焕彬
    /// 图像窗口
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : UserControl
    {
        public ImageView()
        {
            InitializeComponent();
            this.Loaded += ImageView_Loaded;
        }

        private void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            Operator = this;
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像操作
        /// </summary>
        public ImageView Operator
        {
            get { return (ImageView)GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Operator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register(
            "Operator",
            typeof(ImageView),
            typeof(ImageView)
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示图像,需指定绑定模式为OneWayToSource
        /// </summary>
        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(BitmapSource),
            typeof(ImageView),
            new PropertyMetadata(
                null,
                (d, e) =>
                {
                    if (e.NewValue != null)
                    {
                        ImageView view = (ImageView)d;
                        //view.Image.Source = (BitmapSource)e.NewValue;
                        //view.Canvas.Source = (BitmapSource)e.NewValue;
                        view.UpdateImg((BitmapSource)e.NewValue);
                    }
                }
            )
        );

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像宽度
        /// </summary>
        public int ImageWidth => Source == null ? 0 : Source.PixelWidth;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 图像高度
        /// </summary>
        public int ImageHeight => Source == null ? 0 : Source.PixelHeight;

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 更新图像
        /// </summary>
        /// <param name="img">待显示图像</param>
        public void UpdateImg(BitmapSource img)
        {
            Image.Source = img;
            Canvas.Source = img;
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 清除绘图显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 设置画笔
        /// </summary>
        /// <param name="brush">画刷</param>
        /// <param name="thickness">厚度</param>
        public void SetPen(Brush brush, double thickness = 1)
        {
            this.Image.SetPen(brush, thickness);
            this.Canvas.SetPen(brush, thickness);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体类型
        /// </summary>
        /// <param name="fontFamily">字体类型</param>
        public void SetFontFamily(FontFamily fontFamily)
        {
            this.Image.SetFontFamily(fontFamily);
            this.Canvas.SetFontFamily(fontFamily);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体风格
        /// </summary>
        /// <param name="fontStyle">字体风格</param>
        public void SetFontStyle(FontStyle fontStyle)
        {
            this.Image.SetFontStyle(fontStyle);
            this.Canvas.SetFontStyle(fontStyle);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体加粗
        /// </summary>
        /// <param name="fontWeight">字体加粗</param>
        public void SetFontWeight(FontWeight fontWeight)
        {
            this.Image.SetFontWeight(fontWeight);
            this.Canvas.SetFontWeight(fontWeight);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体大小
        /// </summary>
        /// <param name="fontSize">字体大小</param>
        public void SetFontSize(int fontSize)
        {
            this.Image.SetFontSize(fontSize);
            this.Canvas.SetFontSize(fontSize);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 字体画刷
        /// </summary>
        /// <param name="fontBrush">字体画刷</param>
        public void SetFontBrush(Brush fontBrush)
        {
            this.Image.SetFontBrush(fontBrush);
            this.Canvas.SetFontBrush(fontBrush);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制连贯区域，图像坐标
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawRegion(List<Point> points, bool isRender = true)
        {
            this.Image.DrawRegion(points, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制不连贯区域，图像坐标
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawPoints(List<Point> points, bool isRender = true)
        {
            this.Image.DrawPoints(points, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制直线，图像坐标
        /// </summary>
        /// <param name="pt1">待绘制直线点1</param>
        /// <param name="pt2">待绘制直线点2</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            this.Image.DrawLine(pt1, pt2, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制椭圆形，图像坐标
        /// </summary>
        /// <param name="center">待绘制椭圆中心点</param>
        /// <param name="radiusX">待绘制椭圆X半径</param>
        /// <param name="radiusY">待绘制椭圆Y半径</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawEllipse(
            Point center,
            double radiusX,
            double radiusY,
            bool isRender = true
        )
        {
            this.Image.DrawEllipse(center, radiusX, radiusY, isRender);
        }

        /// <summary>
        /// 绘制矩形，图像坐标
        /// </summary>
        /// <param name="rectangle">待绘制矩形</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawRectangle(Rect rectangle, bool isRender = true)
        {
            this.Image.DrawRectangle(rectangle, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字，图像坐标
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="origin">显示位置</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawText(string text, Point origin, bool isRender = true)
        {
            this.Image.DrawText(text, origin, isRender);
        }

        /// <summary>
        /// 2025.01.10 易群生
        /// 显示文字，图像坐标
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isRender"></param>
        public void ImgDrawText(string text, double x,double y, bool isRender = true)
        {
            Point origin=new Point(x,y);
            this.Image.DrawText(text, origin, isRender);
        }


        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字，图像坐标
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="alignmentX">Y对齐</param>
        /// <param name="alignmentY">X对齐</param>
        /// <param name="isRender">是否刷新</param>
        public void ImgDrawText(
            string text,
            AlignmentX alignmentX,
            AlignmentY alignmentY,
            bool isRender = true
        )
        {
            this.Image.DrawText(text, alignmentX, alignmentY, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制区域吗，窗口坐标
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawRegion(List<Point> points, bool isRender = true)
        {
            this.Canvas.DrawRegion(points, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制不连贯点集，窗口坐标
        /// </summary>
        /// <param name="points">待绘制区域点集</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawPoints(List<Point> points, bool isRender = true)
        {
            this.Canvas.DrawPoints(points, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制直线，窗口坐标
        /// </summary>
        /// <param name="pt1">待绘制直线点1</param>
        /// <param name="pt2">待绘制直线点2</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawLine(Point pt1, Point pt2, bool isRender = true)
        {
            this.Canvas.DrawLine(pt1, pt2, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制椭圆，窗口坐标
        /// </summary>
        /// <param name="center">待绘制椭圆中心</param>
        /// <param name="radiusX">待绘制椭圆X半径</param>
        /// <param name="radiusY">待绘制椭圆Y半径</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawEllipse(
            Point center,
            double radiusX,
            double radiusY,
            bool isRender = true
        )
        {
            this.Canvas.DrawEllipse(center, radiusX, radiusY, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 绘制矩形，窗口坐标
        /// </summary>
        /// <param name="rectangle">待绘制矩形</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawRectangle(Rect rectangle, bool isRender = true)
        {
            this.Canvas.DrawRectangle(rectangle, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字，窗口坐标
        /// </summary>
        /// <param name="text">待绘制文字</param>
        /// <param name="origin">显示位置</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawText(string text, Point origin, bool isRender = true)
        {
            this.Canvas.DrawText(text, origin, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 显示文字，窗口坐标
        /// </summary>
        /// <param name="text">文字</param>
        /// <param name="alignmentX">X对齐</param>
        /// <param name="alignmentY">Y对齐</param>
        /// <param name="isRender">是否刷新</param>
        public void WinDrawText(
            string text,
            AlignmentX alignmentX,
            AlignmentY alignmentY,
            bool isRender = true
        )
        {
            this.Canvas.DrawText(text, alignmentX, alignmentY, isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 清除显示
        /// </summary>
        /// <param name="isRender">是否刷新</param>
        public void Clear(bool isRender = true)
        {
            this.Image.Clear(isRender);
            this.Canvas.Clear(isRender);
        }

        /// <summary>
        /// 2024.7.8 李焕彬
        /// 更新显示
        /// </summary>
        public void Invalidate()
        {
            this.Image.InvalidateVisual();
            this.Canvas.InvalidateVisual();
        }

        /// <summary>
        /// 2024.9.13 李焕彬
        /// 保存图片
        /// </summary>
        /// <param name="path">路径</param>
        public void SaveImage(string path)
        {
            if (Source == null)
                return;
            try
            {
                BitmapEncoder encoder;
                switch (Path.GetExtension(path))
                {
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        encoder = new BmpBitmapEncoder();
                        break;
                }
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(Source));
                    encoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
        }

        /// <summary>
        /// 2024.9.13 李焕彬
        /// 保存图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ".bmp|*.bmp|.jpg|*.jpg|.png|*.png|.tiff|*.tiff";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveImage(saveFileDialog.FileName);
            }
        }

        ///// <summary>
        ///// 2024.7.5 李焕彬
        ///// 获取窗口截图
        ///// </summary>
        ///// <param name="width">截图宽</param>
        ///// <param name="height">截图高</param>
        ///// <returns>输出图像</returns>
        //public BitmapSource GetImage()
        //{
        //    DrawingVisual drawingVisual = new DrawingVisual();
        //    DrawingContext drawingContext = drawingVisual.RenderOpen();
        //    drawingContext.DrawImage(Source, new Rect(0, 0, ImageWidth, ImageHeight));
        //    this.Image.Draw(drawingContext, ImageWidth, 1, 50);
        //    this.Canvas.Draw(drawingContext, ImageWidth, ImageHeight, 50);
        //    drawingContext.Close();
        //    RenderTargetBitmap renderTargetBitmap =
        //        new(ImageWidth, ImageHeight, 96, 96, PixelFormats.Default);
        //    renderTargetBitmap.Render(drawingVisual);
        //    renderTargetBitmap.Freeze();
        //    return renderTargetBitmap;
        //}
    }
}
