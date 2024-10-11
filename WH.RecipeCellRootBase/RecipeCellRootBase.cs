using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.RecipeCellRootBase
{
    public class CellRootBase<C, T>
        where T : CellDetectionBase<T>, IwhClone<T>, new()
        where C : CellRootBase<C, T>, new()
    {
        // public PreVariable PreVal { get; set; } = new PreVariable();
        private CImage _image;
        public CImage Image
        {
            get => _image;
            set { _image = value; }
        }

        private T _detection;

        /// <summary>
        /// 定级缺陷
        /// </summary>
        public T Detection
        {
            get => _detection;
            set { _detection = value; }
        }
        public ConcurrentBag<T> Detections { get; set; } = new ConcurrentBag<T>();

        public virtual void Dispose()
        {
            if (this.Image != null)
            {
                this.Image.Dispose();
            }
            //if (this.ColorImage != null)
            //{
            //    ((IDisposable)this.ColorImage).Dispose();
            //}
            if (this.Detection != null)
            {
                this.Detection.Dispose();
            }
            foreach (T detection in this.Detections)
                detection.Dispose();
            this.Detections = new ConcurrentBag<T>();
        }

        public virtual C Clone()
        {
            C Cell = new C();
            Cell.Image = (CImage)this.Image.Clone();
            Cell.Detection = this.Detection?.Clone();
            foreach (T detection in this.Detections)
                Cell.Detections.Add(detection?.Clone());
            return Cell;
        }
    }

    public class CellDetectionBase<T> : IwhClone<T>
        where T : IwhClone<T>, new()
    {
        public CellDetectionBase()
        {
            //HOperatorSet.GenEmptyObj(out HObject _region);
            //Region = _region;
            //HOperatorSet.GenEmptyObj(out HObject _unionedRegion);
            //UnionedRegion = _unionedRegion;
        }

        public bool Result { get; set; } = true;

        // public HObject Region { get; set; } = new HObject();

        public float[] Value { get; set; }

        /// <summary>
        /// 检测日志
        /// </summary>
        public StringBuilder DetectLog { get; set; } = new StringBuilder();

        //  public HObject UnionedRegion { get; set; } = new HObject();



        public virtual void Dispose()
        {
            //((IDisposable)this.Region).Dispose();

            //UnionedRegion.Dispose();
        }

        public virtual T Clone()
        {
            T detection = new T();
            detection.Result = this.Result;

            //if (this.Region != null)
            //{
            //    detection.Region = this.Region.Clone();
            //}
            detection.DetectLog = new StringBuilder(this.DetectLog.ToString());
            //if (this.UnionedRegion != null)
            //{
            //    detection.UnionedRegion = this.UnionedRegion.Clone();
            //}
            if (this.Value != null)
            {
                detection.Value = (float[])this.Value.Clone();
            }
            return detection;
        }
    }

    public interface IwhClone<T>
    {
        bool Result { get; set; }

        // HObject Region { get; set; }

        float[] Value { get; set; }

        /// <summary>
        /// 检测日志
        /// </summary>
        StringBuilder DetectLog { get; set; }

        //  HObject UnionedRegion { get; set; }
        T Clone();
    }

    [Serializable]
    public enum workType
    {
        正面AOI,
        反面AOI,
    }

    /// <summary>
    /// 缺陷项范畴
    /// </summary>
    [Serializable]
    public enum Category
    {
        区域,
        值
    }

    /// <summary>
    /// 检测类型
    /// </summary>
    [Serializable]
    public enum DefectType
    {
        面积,
        数值
    }

    /// <summary>
    /// 2024.7.23 李焕彬
    /// 自定义图像类
    /// </summary>
    public class CImage : IDisposable, ICloneable
    {
        public CImage(BitmapSource bitmap)
        {
            ImageWidth = bitmap.PixelWidth;
            ImageHeight = bitmap.PixelHeight;
            PixelFormat = bitmap.Format;
            Palette = bitmap.Palette;
            StrideWidth = ImageWidth * ((PixelFormat.BitsPerPixel + 7) / 8);
            ImageSize = StrideWidth * ImageHeight;
            ImageData = Marshal.AllocHGlobal(ImageSize);
            bitmap.CopyPixels(
                new Int32Rect(0, 0, ImageWidth, ImageHeight),
                ImageData,
                ImageSize,
                StrideWidth
            );
        }

        public CImage(string path)
            : this(new BitmapImage(new Uri(path))) { }

        public CImage(int imageWidth, int imageHeight, nint imageData, PixelFormat pixelFormat)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            ImageData = imageData;
            PixelFormat = pixelFormat;
            int bitsPerPixel = pixelFormat == PixelFormats.Gray8 ? 8 : 24;
            StrideWidth = imageWidth * ((bitsPerPixel + 7) / 8);
            ImageSize = StrideWidth * ImageHeight;
        }

        public CImage(
            int imageWidth,
            int imageHeight,
            int strideWidth,
            nint imageData,
            PixelFormat pixelFormat,
            BitmapPalette palette = null
        )
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            StrideWidth = strideWidth;
            ImageData = imageData;
            PixelFormat = pixelFormat;
            Palette = palette;
            ImageSize = StrideWidth * ImageHeight;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像宽度
        /// </summary>
        public int ImageWidth { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像高度
        /// </summary>
        public int ImageHeight { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像数据
        /// </summary>
        public IntPtr ImageData { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像行宽
        /// </summary>
        public int StrideWidth { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像类型
        /// </summary>
        public PixelFormat PixelFormat { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像色表
        /// </summary>
        public BitmapPalette Palette { get; }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像大小
        /// </summary>
        public int ImageSize { get; }

        /// <summary>
        /// 2024.8.9 李焕彬
        /// bitmapSource
        /// </summary>
        private BitmapSource bitmapSource;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 转BitmapSource方法
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource ToBitmapSource()
        {
            if (this.bitmapSource == null)
            {
                this.bitmapSource = BitmapSource.Create(
                    ImageWidth,
                    ImageHeight,
                    96,
                    96,
                    PixelFormat,
                    Palette,
                    ImageData,
                    ImageSize,
                    StrideWidth
                );
                this.bitmapSource.Freeze();
            }

            return this.bitmapSource;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// Dispose方法
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(ImageData);
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// Clone方法
        /// </summary>
        public object Clone()
        {
            IntPtr ptrDst = Marshal.AllocHGlobal(ImageSize);
            byte[] data = new byte[ImageSize];
            Marshal.Copy(ImageData, data, 0, ImageSize);
            Marshal.Copy(data, 0, ptrDst, ImageSize);
            CImage image = new CImage(
                ImageWidth,
                ImageHeight,
                StrideWidth,
                ptrDst,
                PixelFormat,
                Palette
            );
            image.bitmapSource = bitmapSource;

            return image;
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 检测结果
    /// </summary>
    public enum EMDETECTRESULT
    {
        EMDR_OK = 0, //检测OK
        EMDR_NG_LIGHTEDGE = 1, //毛刺NG
        EMDR_NG_DARKEDGE = 2, //料区NG
        EMDR_NG_EMPTY = 3, //空白NG
        EMDR_TIMEOUT = 4, //检测超时
        EMDR_LOSEFOCUS = 5, //失焦异常
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域类型
    /// </summary>
    public enum EMREGIONTYPE
    {
        EMRT_DARKTOP = 0,
        EMRT_DARKBOT = 1,
        EMRT_LIGHTTOP = 2,
        EMRT_LIGHTBOT = 3,
        EMRT_MAOCIREGION = 4,
        EMRT_THICKREGION = 5,
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域信息
    /// </summary>
    public struct SRegionInfo
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形X
        /// </summary>
        public int X = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形Y
        /// </summary>
        public int Y = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形宽
        /// </summary>
        public int Width = 100;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形高
        /// </summary>
        public int Height = 100;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直宽度
        /// </summary>
        public double WidthBound = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直高度
        /// </summary>
        public double HeightBound = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um直角高度
        /// </summary>
        public double PeakHeight = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um长边长度
        /// </summary>
        public double LongLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um短边长度
        /// </summary>
        public double ShorLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 角度
        /// </summary>
        public double Phi = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um周长
        /// </summary>
        public double ContLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// um²面积
        /// </summary>
        public double Area = 0;

        public SRegionInfo() { }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 复制
        /// </summary>
        /// <param name="regionInfo">复制源</param>
        public void Copy(SRegionInfo regionInfo)
        {
            X = regionInfo.X;
            Y = regionInfo.Y;
            Width = regionInfo.Width;
            Height = regionInfo.Height;
            WidthBound = regionInfo.WidthBound;
            HeightBound = regionInfo.HeightBound;
            PeakHeight = regionInfo.PeakHeight;
            LongLen = regionInfo.LongLen;
            ShorLen = regionInfo.ShorLen;
            Phi = regionInfo.Phi;
            ContLen = regionInfo.ContLen;
            Area = regionInfo.Area;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域
    /// </summary>
    public struct SRegion
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域信息
        /// </summary>
        public SRegionInfo RegionInfo;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域点集
        /// </summary>
        public List<Point> points1;

        public SRegion()
        {
            RegionInfo = new SRegionInfo();
            points1 = new List<Point>();
        }

        public SRegion(SRegionInfo regionInfo, List<Point> points)
        {
            RegionInfo = regionInfo;
            points1 = points;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取矩形
        /// </summary>
        /// <returns></returns>
        public Rect GetRect()
        {
            return new Rect(RegionInfo.X, RegionInfo.Y, RegionInfo.Width, RegionInfo.Height);
        }

        public Point GetCenter()
        {
            return new Point(
                RegionInfo.X + RegionInfo.Width / 2,
                RegionInfo.Y + RegionInfo.Height / 2
            );
        }
    }
}
