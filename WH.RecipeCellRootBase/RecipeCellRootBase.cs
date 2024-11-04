using System.Collections.Concurrent;
using System.Globalization;
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

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 输出区域
        /// </summary>
        public List<SRegion> regionOut { get; set; } = new List<SRegion>();

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 输出数值
        /// </summary>
        public List<float> Value { get; set; }

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
            detection.regionOut = this.regionOut?.ToList();
            if (this.Value != null)
            {
                detection.Value = this.Value.ToList();
            }
            return detection;
        }
    }

    public interface IwhClone<T>
    {
        bool Result { get; set; }

        public List<SRegion> regionOut { get; set; }

        List<float> Value { get; set; }

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
        区域 = 0,
        值 = 1,
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

    ///// <summary>
    ///// 2024.6.25 李焕彬
    ///// 区域类型
    ///// </summary>
    //public enum EMREGIONTYPE
    //{
    //    EMRT_DARKTOP = 0,
    //    EMRT_DARKBOT = 1,
    //    EMRT_LIGHTTOP = 2,
    //    EMRT_LIGHTBOT = 3,
    //    EMRT_MAOCIREGION = 4,
    //    EMRT_THICKREGION = 5,
    //};

    /// <summary>
    /// 2024.10.22 李焕彬
    /// 算法
    /// </summary>
    public class CDefectRecipe
    {
        /// <summary>
        /// 2024.10.22 李焕彬
        /// 算法名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 区域/值
        /// </summary>
        public Category Category { get; set; }

        public CDefectRecipe(string name, Category category)
        {
            Name = name;
            Category = category;
        }
    }

    /// <summary>
    /// 2024.10.22 李焕彬
    /// 检测类
    /// </summary>
    public class CDefectSpecies
    {
        /// <summary>
        /// 2024.10.22 李焕彬
        /// 检测类名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 算法缺陷
        /// </summary>
        public List<CDefectRecipe> RecipeDefects { get; set; }

        public CDefectSpecies(string speciesName, List<CDefectRecipe> recipeDefect)
        {
            Name = speciesName;
            RecipeDefects = recipeDefect;
        }
    }

    /// <summary>
    /// 2024.10.21 李焕彬
    /// 缺陷特征
    /// </summary>
    public class CFeacture
    {
        public CFeacture()
        {
            this.Id = "null";
        }

        public CFeacture(string id, string zhName, string enName, string unit)
        {
            this.Id = id;
            this.ZhName = zhName;
            this.EnName = enName;
            this.Unit = unit;
        }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 唯一标识符，不能改
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 缺陷名中文
        /// </summary>
        public string ZhName { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 缺陷名英文
        /// </summary>
        public string EnName { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 缺陷单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 获取缺陷名
        /// </summary>
        /// <returns>缺陷名</returns>
        public string GetName()
        {
            switch (CultureInfo.CurrentCulture.Name)
            {
                case "zh-CN":
                    return ZhName;
                default:
                    return EnName;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is CFeacture feacture && Id == feacture.Id;
        }

        public override string ToString()
        {
            return GetName();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(CFeacture left, CFeacture right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CFeacture left, CFeacture right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 数量特征
        /// </summary>
        public static CFeacture FeactureCount = new("Count", "数量", "Count", "pcs");

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 数值特征
        /// </summary>
        public static CFeacture FeactureValue = new("Value", "数值", "Value", "");
    }

    /// <summary>
    /// 2024.10.21 李焕彬
    /// 区域信息
    /// </summary>
    public interface IRegionInfo
    {
        /// <summary>
        /// 2024.10.21 李焕彬
        /// 获取对应缺陷特征值
        /// </summary>
        /// <param name="character">缺陷特征</param>
        /// <returns>缺陷特征值</returns>
        double GetValue(CFeacture character, SRegion region);

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 合并区域
        /// </summary>
        /// <param name="regions">区域集</param>
        /// <returns>合并后区域</returns>
        SRegion Union(List<SRegion> regions);
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域
    /// </summary>
    public struct SRegion
    {
        public Rect rect;
        
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域信息
        /// </summary>
        public IRegionInfo regionInfo;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域点集
        /// </summary>
        public List<Point> points;

        public SRegion(IRegionInfo _regionInfo, List<Point> _points)
        {
            regionInfo = _regionInfo;
            points = _points;
            rect = new Rect(
                new Point(points.Select(o => o.X).Min(), points.Select(o => o.Y).Min()),
                new Point(points.Select(o => o.X).Max(), points.Select(o => o.Y).Max())
            );
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域矩形
        /// </summary>
        /// <returns>区域矩形</returns>
        public Rect GetRect()
        {
            return rect;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域中心
        /// </summary>
        /// <returns>区域中心</returns>
        public Point GetCenter()
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}
