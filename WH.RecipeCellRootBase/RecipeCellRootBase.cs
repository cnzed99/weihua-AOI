using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows;
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
                Cell.Detections.Add(detection.Clone());
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
            //detection.DetectLog = new StringBuilder(this.DetectLog.ToString());
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
    /// 2024.7.23
    /// 自定义图像类
    /// </summary>
    public class CImage : IDisposable, ICloneable
    {
        public CImage(BitmapSource bitmap)
        {
            ImageWidth = bitmap.PixelWidth;
            ImageHeight = bitmap.PixelHeight;
            PixelFormat = bitmap.Format;
            StrideWidth = ImageWidth * ((PixelFormat.BitsPerPixel + 7) / 8);
            ImageSize = StrideWidth * ImageHeight;
            ImageData = Marshal.AllocHGlobal(ImageSize);
            bitmap.CopyPixels(new Int32Rect(0, 0, ImageWidth, ImageHeight), ImageData, ImageSize, StrideWidth);
        }

        public CImage(string path) : this(new BitmapImage(new Uri(path)))
        {

        }

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

        public CImage(int imageWidth, int imageHeight, int strideWidth, nint imageData, PixelFormat pixelFormat)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            StrideWidth = strideWidth;
            ImageData = imageData;
            PixelFormat = pixelFormat;
            ImageSize = StrideWidth * ImageHeight;
        }

        /// <summary>
        /// 2024.7.23
        /// 转BitmapSource方法
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource ToBitmapSource()
        {
            return BitmapSource.Create(
                            ImageWidth,
                            ImageHeight,
                            96,
                            96,
                            PixelFormat,
                            null,
                            ImageData,
                            ImageSize,
                            StrideWidth
                        );
        }

        /// <summary>
        /// 2024.7.23
        /// 图像宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 2024.7.23
        /// 图像高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 2024.7.23
        /// 图像数据
        /// </summary>
        public IntPtr ImageData { get; set; }

        /// <summary>
        /// 2024.7.23
        /// 图像行宽
        /// </summary>
        public int StrideWidth { get; set; }

        /// <summary>
        /// 2024.7.23
        /// 图像类型
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// 2024.7.23
        /// 图像大小
        /// </summary>
        public int ImageSize { get; set; }

        /// <summary>
        /// 2024.7.23
        /// Dispose方法
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(ImageData);
        }

        /// <summary>
        /// 2024.7.23
        /// Clone方法
        /// </summary>
        public object Clone()
        {
            IntPtr ptrDst = Marshal.AllocHGlobal(ImageSize);
            byte[] data = new byte[ImageSize];
            Marshal.Copy(ImageData, data, 0, ImageSize);
            Marshal.Copy(data, 0, ptrDst, ImageSize);

            return new CImage(ImageWidth, ImageHeight, StrideWidth, ptrDst, PixelFormat);
        }
    }
}
