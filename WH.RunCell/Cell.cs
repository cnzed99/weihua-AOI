using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlgorithmDll;
using WH.RecipeCellRootBase;

namespace WH.RunCell
{
    public class Cell : CellRootBase<Cell, CellDetection>
    {
        public static int MaxParallelism = 5;

        /// <summary>
        /// 是否是单张图片，最后一张图片
        /// 提示 使能按钮操作
        /// </summary>
        public bool isOnce { get; set; } = false;

        /// <summary>
        /// 图像文件
        /// </summary>
        public string ImageFile = string.Empty;

        /// <summary>
        /// 缺陷位置图像
        /// </summary>
        public BitmapSource DefectPartImg { get; set; }

        /// <summary>
        /// 义乌爱旭的丝网特殊用途 从预处理库中拿图显示
        /// </summary>
        public BitmapSource ChangleImgae { get; set; }

        public Cell()
        {
            this.CreateTime = DateTime.Now;
            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();
        }

        /// <summary>
        /// 2024.7.2 李焕彬
        /// 算法输出管理，含输出区域
        /// </summary>
        public CMaociTest MaociTestOut { get; set; } = new CMaociTest();

        /// <summary>
        /// 是否是OK产品
        /// </summary>
        public bool IsOK { get; set; } = true;

        /// <summary>
        /// 用来存储是质量OK还是颜色OK [0]=质量 [1]=颜色;
        /// </summary>
        // public bool[] DetectionOrColorOK { get; set; } = new bool[2];

        /// <summary>
        /// 流水号
        /// </summary>
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// 产品ID
        /// </summary>
        public string WaferID { get; set; } = string.Empty;

        /// <summary>
        /// 当前班次的产品序号
        /// </summary>
        public int ProductIndex { get; set; }

        /// <summary>
        /// 接收信息字典
        /// </summary>
        public Dictionary<string, string> OtherInfoRecv { get; set; }

        /// <summary>
        /// 发送信息字典
        /// </summary>
        public Dictionary<string, string> OtherInfoSend { get; set; } =
            new Dictionary<string, string>();

        ///// <summary>
        ///// 原图像== Image
        ///// </summary>
        //public BitmapSource RealImage => this.Image;

        public BitmapSource DownImage { get; set; }

        public BitmapSource SmallImage { get; set; }

        public double defectSize { get; set; }

        /// <summary>
        /// 检测开始时间 从收到触发信号开始计时
        /// </summary>
        public DateTime BeginVisionTime { get; set; }

        /// <summary>
        /// 创建Cell的时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// 跳过
        /// </summary>
        public bool Skipthis { get; set; } = false;

        private EMDETECTRESULT algoriDetectResult = EMDETECTRESULT.EMDR_OK;

        /// <summary>
        /// 2024.7.31 李焕彬
        /// 算法检查结果.NG时跳过
        /// </summary>
        public EMDETECTRESULT AlgoriDetectResult
        {
            get { return algoriDetectResult; }
            set
            {
                algoriDetectResult = value;
                if (algoriDetectResult != EMDETECTRESULT.EMDR_OK)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _timeOut = false;

        /// <summary>
        /// 超时
        /// </summary>
        public bool TimeOut
        {
            get => _timeOut;
            set
            {
                _timeOut = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _preError;

        /// <summary>
        /// 预处理报错
        /// </summary>
        public bool PreError
        {
            get { return _preError; }
            set
            {
                _preError = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _recipeError;

        /// <summary>
        /// 配方报错
        /// </summary>
        public bool RecipeError
        {
            get { return _recipeError; }
            set
            {
                _recipeError = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _isempty = false;

        /// <summary>
        /// 空料
        /// </summary>
        public bool IsEmpty
        {
            get => _isempty;
            set
            {
                _isempty = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _ismix = false;

        /// <summary>
        /// 混料
        /// </summary>
        public bool IsMix
        {
            get => _ismix;
            set
            {
                _ismix = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        private bool _isBurst = false;

        /// <summary>
        /// 爆板
        /// </summary>
        public bool IsBurstBoard
        {
            get => _isBurst;
            set
            {
                _isBurst = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        public string DefectType { get; set; } = string.Empty;

        public List<string> DetectedDef { get; set; }

        /// <summary>
        /// 质量等级
        /// </summary>
        public dynamic Quality { get; set; }

        /// <summary>
        /// 计时
        /// </summary>
        public Stopwatch Stopwatch { get; set; }

        private bool _frameLoss = false;

        /// <summary>
        /// 是否丢帧
        /// </summary>
        public bool FrameLoss
        {
            get { return _frameLoss; }
            set
            {
                _frameLoss = value;
                if (value)
                {
                    Skipthis = true;
                }
            }
        }

        /// <summary>
        /// 2024.7.29 李焕彬
        /// 截图
        /// </summary>
        public BitmapSource DumpImage { get; set; }

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 编码器位置
        /// </summary>
        public int EncoderPos { get; set; } = 0;

        public override void Dispose()
        {
            base.Dispose();

            if (this.DefectPartImg != null)
            {
                if (this.DefectPartImg.CanFreeze)
                {
                    this.DefectPartImg.Freeze();
                }
            }
            this.Stopwatch.Stop();

            if (DownImage != null && DownImage.CanFreeze)
            {
                DownImage.Freeze();
            }
            if (SmallImage != null && SmallImage.CanFreeze)
            {
                SmallImage.Freeze();
            }
        }

        public override Cell Clone()
        {
            Cell cell = base.Clone();
            cell.DefectType = this.DefectType;
            cell.ID = this.ID;
            cell.IsOK = this.IsOK;
            cell.IsEmpty = this.IsEmpty;
            //this.Image.WriteTo(cell.Image);
            cell.DownImage = this.DownImage?.Clone();
            // cell.QualityColorStr = this.QualityColorStr;
            cell.SmallImage = this.SmallImage?.Clone();
            cell.Quality = this.Quality;
            //cell.QualityName = this.QualityName;
            //cell.QualitySignal = this.QualitySignal;
            //cell.QualityColor = this.QualityColor;
            //cell.ColorSignel = this.ColorSignel;
            //cell.ColorGrade = this.ColorGrade?.Clone();
            //cell.ColorValue = this.ColorValue;
            cell.GetImageTime = this.GetImageTime;
            cell.CreateTime = this.CreateTime;
            cell.PreTime = this.PreTime;
            cell.FlowTimeSpan = this.FlowTimeSpan;
            cell.RecipeTime = this.RecipeTime;
            cell.ProcessTime = this.ProcessTime;
            cell.FilterTime = this.FilterTime;
            cell.ShowTime = this.ShowTime;
            cell.Stopwatch = this.Stopwatch;
            cell.SaveImgTime = this.SaveImgTime;
            cell.TwoTrgTimeSpan = this.TwoTrgTimeSpan;
            cell.Skipthis = this.Skipthis;
            cell.IsBurstBoard = this.IsBurstBoard;
            cell.IsMix = this.IsMix;
            cell.ImageFile = this.ImageFile;
            cell.LineName = this.LineName;
            cell.ProjName = this.ProjName;
            cell.CamSerial = this.CamSerial;
            cell.CamName = this.CamName;
            cell.ProjGuid = this.ProjGuid;
            cell.ComGuid = this.ComGuid;
            // cell.DetectionOrColorOK = this.DetectionOrColorOK;
            cell.OtherInfoRecv = this.OtherInfoRecv;
            cell.OtherInfoSend = this.OtherInfoSend;
            cell.DataBytes = this.DataBytes;
            cell.WaferID = this.WaferID;
            cell.ProductIndex = this.ProductIndex;
            cell.AlgoriDetectResult = this.AlgoriDetectResult;
            cell.EncoderPos = this.EncoderPos;
            return cell;
        }

        /// <summary>
        /// 取图时间
        /// </summary>
        public TimeSpan GetImageTime { get; set; }

        /// <summary>
        /// 执行预处理时间
        /// </summary>
        public TimeSpan PreTime { get; set; }

        /// <summary>
        /// 执行配方时间
        /// </summary>
        public TimeSpan RecipeTime { get; set; }

        /// <summary>
        /// 执行筛选时间
        /// </summary>
        public TimeSpan FilterTime { get; set; }

        /// <summary>
        /// 显示时间
        /// </summary>
        public TimeSpan ShowTime { get; set; }

        /// <summary>
        /// 存图时间
        /// </summary>
        public TimeSpan SaveImgTime { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan ProcessTime { get; set; }

        public Dictionary<string, TimeSpan> FlowTimeSpan { get; set; }

        public TimeSpan TwoTrgTimeSpan { get; set; }
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationTokenSource CancelSource
        {
            get => _cancellationTokenSource;
            set
            {
                _cancellationTokenSource = value;
                _parallelOptions = new ParallelOptions()
                {
                    CancellationToken = value.Token,
                    TaskScheduler = TaskScheduler.Default,
                    MaxDegreeOfParallelism = MaxParallelism
                };
            }
        }

        private ParallelOptions _parallelOptions;

        public ParallelOptions ParallelOption
        {
            get => _parallelOptions;
        }

        /// <summary>
        /// 制程绑定的相机序列号
        /// </summary>
        public string CamSerial { get; set; } = string.Empty;
        public string CamName { get; set; } = string.Empty;

        /// <summary>
        /// 制程绑定的通讯
        /// </summary>
        public string ComGuid { get; set; } = string.Empty;

        /// <summary>
        /// 制程绑定的唯一ID
        /// </summary>
        public string ProjGuid { get; set; } = string.Empty;

        public string ProjName { get; set; } = string.Empty;

        /// <summary>
        /// 线名称（属于哪条产线）
        /// </summary>
        public string LineName { get; set; } = string.Empty;

        /// <summary>
        /// 接收到的信号数据
        /// </summary>
        public byte[] DataBytes { get; set; }
    }
}
