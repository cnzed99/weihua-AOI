
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace WH.RecipeCellRootBase
{
    public class CellRootBase<C, T> where T : CellDetectionBase<T>, IwhClone<T>, new() where C : CellRootBase<C, T>, new()
    {
       // public PreVariable PreVal { get; set; } = new PreVariable();
        private MemoryStream _image;
        public MemoryStream Image
        {
            get => _image;
            set
            {
                _image = value;
            }
        }

        private T _detection;
        /// <summary>
        /// 定级缺陷
        /// </summary>
        public T Detection
        {
            get => _detection;
            set
            {
                _detection = value;
            }
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
            if (this.Image != null)
            {
                this.Image.WriteTo(Cell.Image);
            }
            Cell.Detection = this.Detection?.Clone();
            foreach (T detection in this.Detections)
                Cell.Detections.Add(detection.Clone());
            return Cell;
        }
    }

    public class CellDetectionBase<T> : IwhClone<T> where T : IwhClone<T>, new()
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


}
