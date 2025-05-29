using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CameraModule.Model;
using HandyControl.Controls;
using OpenCvSharp;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace CameraModule
{
    /// <summary>
    /// 李焕彬 2024.7.24
    /// 相机操作基类
    /// </summary>
    public abstract class CCameraBase : IVisionFuns
    {
        /// <summary>
        /// 李焕彬 2024.7.24
        /// 旋转图像
        /// </summary>
        /// <param name="rotate">旋转角度，对应EMIMAGEROTATE</param>
        /// <param name="nChannel">通道数</param>
        /// <param name="widthSrc">宽度</param>
        /// <param name="heightSrc">高度</param>
        /// <param name="nLineSrc">行宽</param>
        /// <param name="dataSrc">图像数据</param>
        /// <param name="widthDst">旋转后宽度</param>
        /// <param name="heightDst">旋转后高度</param>
        /// <param name="nLineDst">旋转后行宽</param>
        /// <param name="dataDst">旋转后图像数据</param>
        [DllImport("GeneralAlg.dll")]
        public static extern void RotateImage(
            int rotate,
            int nChannel,
            int widthSrc,
            int heightSrc,
            int nLineSrc,
            IntPtr dataSrc,
            int widthDst,
            int heightDst,
            int nLineDst,
            IntPtr dataDst
        );

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 对应制程名Guid
        /// </summary>
        public string ProjGuid { get; set; } = "";

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 相机生成图日志
        /// </summary>
        protected CLogRec getImageLogger = CLogRec.Create("CamGetImage", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        protected CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 相机参数
        /// </summary>
        public CCameraParameterBase Setting { get; set; }

        public CCameraBase()
        {
        }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 初始化参数
        /// </summary>
        /// <param name="parameters">参数</param>
        public void Init(CCameraParameterBase parameters)
        {
            Setting = parameters;
        }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 采集 信号
        /// </summary>
        //public Channel<Cell> triggerImageChannel = Channel.CreateBounded<Cell>(channelOptions);

        /// <summary>
        /// 20240726 TCG
        /// 输出图像队列
        /// </summary>
        public Channel<Cell> OutputImageChannel;

        ///// <summary>
        ///// 李焕彬 2024.7.24
        ///// 对焦采集 图像队列
        ///// </summary>
        //public static Channel<Cell> FocusWaitGetImageChannel = Channel.CreateBounded<Cell>(
        //    channelOptions
        //);

        /// <summary>
        /// 20240725 TCG
        /// 当前制程是否启动
        /// </summary>
        public bool IsSetWindowShowed { get; set; } = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 图像传递委托
        /// </summary>
        public Action<Cell> GrabFinishEvent { get; set; }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 通道数
        /// </summary>
        private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 图像队列，相机回调函数传入
        /// </summary>
        public Channel<IntPtr> ImageQueueChannel = Channel.CreateBounded<IntPtr>(channelOptions);

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 取图线程
        /// </summary>
        protected Thread grabThread;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 取图线程循环标志
        /// </summary>
        protected bool isStartGrabThread = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 线程锁
        /// </summary>
        protected Mutex mutex = new Mutex();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 超时计时,软触发拍照时间
        /// </summary>
        protected Stopwatch timeOut = new Stopwatch();

        ///// <summary>
        ///// 李焕彬 2024.7.24
        ///// 合成转换时间 从拍照到传出图像
        ///// </summary>
        //public Stopwatch GetImagetime { get; set; } = new Stopwatch();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 开启采图，执行软触发前会设为true
        /// </summary>
        protected bool startGrabSoft = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 防止丢帧时 再次传出图片
        /// </summary>
        protected bool canGo = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 用于接收到的触发数计数 与GrabCount进行比对
        /// </summary>
        protected int triggerCount;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 用于相机实际拍照计数 与TriggerCount进行比对
        /// </summary>
        protected int grabCount;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 相机连接状态
        /// </summary>
        public bool Connected { get; set; } = false;

        /// <summary>
        /// 李焕彬 2025.4.11
        /// 原始图像数据行宽，-1:自动生成行宽,原始图像数据为rgba时需要设置
        /// </summary>
        protected int imageBufferStride = -1;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 丢帧图像
        /// </summary>
        public CImage LostImage
        {
            get
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage(
                        new Uri(AppDomain.CurrentDomain.BaseDirectory + "LostImage\\LostImage.png")
                    );
                    int stride =
                        bitmapImage.PixelWidth * ((bitmapImage.Format.BitsPerPixel + 7) / 8);
                    IntPtr ptr = Marshal.AllocHGlobal(bitmapImage.PixelHeight * stride);
                    bitmapImage.CopyPixels(
                        new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight),
                        ptr,
                        bitmapImage.PixelHeight * stride,
                        stride
                    );
                    return new CImage(
                        bitmapImage.PixelWidth,
                        bitmapImage.PixelHeight,
                        stride,
                        ptr,
                        bitmapImage.Format
                    );
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 清晰度算法
        /// </summary>
        public Func<CImage, float> FuncDistinct { get; set; }

        List<Mat> mats = new List<Mat>();
        Mat result;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 取图
        /// </summary>
        /// <param name="grabbedRawData">图像数据</param>
        public virtual bool GetImageFunc(IntPtr grabbedRawData)
        {
            try
            {
                int widthNew = Setting.ImageWidth;
                int heightNew = Setting.ImageHeight;
                int bitsPerPixel = Setting.CameraType.BitsPerPixel;
                int stride =
                    imageBufferStride > 0
                        ? imageBufferStride
                        : Setting.ImageWidth * ((bitsPerPixel + 7) / 8);
                switch (Setting.ImageRotate)
                {
                    case EMIMAGEROTATE.EMROTATE0:
                        break;

                    case EMIMAGEROTATE.EMROTATE90:
                        widthNew = Setting.ImageHeight;
                        heightNew = Setting.ImageWidth;
                        break;

                    case EMIMAGEROTATE.EMROTATE180:
                        break;

                    case EMIMAGEROTATE.EMROTATE270:
                        widthNew = Setting.ImageHeight;
                        heightNew = Setting.ImageWidth;
                        break;
                }
                int strideNew = widthNew * ((bitsPerPixel + 7) / 8);
                // int strideNew = widthNew * bitsPerPixel;
                IntPtr ptrNew = Marshal.AllocHGlobal(strideNew * heightNew);
                RotateImage(
                    (int)Setting.ImageRotate,
                    bitsPerPixel / 8,
                    Setting.ImageWidth,
                    Setting.ImageHeight,
                    stride,
                    grabbedRawData,
                    widthNew,
                    heightNew,
                    strideNew,
                    ptrNew
                );

                CImage image = new CImage(widthNew, heightNew, strideNew, ptrNew, Setting.CameraType);
                ExportImage(image);

                //Mat img = new Mat(heightNew, widthNew, MatType.CV_8UC((bitsPerPixel + 7) / 8), ptrNew);
                //mats.Add(img);
                //if (mats.Count >= Setting.CamCount)
                //{
                //    // 拼接图像
                //    result = new Mat();

                //    Cv2.HConcat(mats.ToArray(), result);
                //    int strideMat = result.Width * ((bitsPerPixel + 7) / 8);

                //    CImage image = new CImage(result.Width, result.Height, strideMat, result.Data, Setting.CameraType);
                //    ExportImage(image);
                //    foreach (Mat mat in mats)
                //    {
                //          mat.Dispose(); 
                //    }
                //    mats.Clear();
                //}

                return true;
            }
            catch (Exception)
            {
                mats.Clear ();
                return false;
            }
          
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 主动取流线程
        /// </summary>
        /// <param name="grabbedRawData">图像数据</param>
        public virtual void GrabThread()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (isStartGrabThread)
            {
                try
                {
                    if (Setting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
                    {
                        if (startGrabSoft)
                        {
                            if (this.ImageQueueChannel.Reader.TryRead(out IntPtr ptr))
                            {
                                startGrabSoft = false;
                                timeOut.Stop();
                                GetImageFunc(ptr);
                            }
                            else if ((int)timeOut.ElapsedMilliseconds >= Setting.TimeOut)
                            {
                                startGrabSoft = false;
                                timeOut.Stop();
                                if (LostImage != null)
                                {
                                    ExportImage(LostImage, true);
                                }
                                StringBuilder textBuilder = new StringBuilder();
                                textBuilder.Append(Properties.Resources.ErrorLostImage2);
                                textBuilder.Append(timeOut.ElapsedMilliseconds);
                                CCameraManagement.CamLogger.Error(textBuilder.ToString());
                            }
                        }
                    }
                    else
                    {
                        if (this.ImageQueueChannel.Reader.TryRead(out IntPtr ptr))
                        {
                            GetImageFunc(ptr);
                        }
                    }
                    Thread.Sleep(5);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 传出图像
        /// </summary>
        /// <param name="outImage">图像</param>
        public bool ExportImage(CImage outImage, bool isLostFrame = false)
        {
            try
            {
                Cell cell = new Cell()
                {
                    Image = outImage,
                    FrameLoss = isLostFrame,
                    CamSerial = Setting.SerialNumber,
                    ProjGuid = Setting.ProjGuid,
                    CamName = Setting.Name,
                    MmPerPixel = Setting.MmPerPixel,
                };
                //需要增加判断是否是运行模式
                if (IsSetWindowShowed)
                {
                    GrabFinishEvent?.Invoke(cell);
                }
                //Cell cell = await triggerImageChannel.Reader.ReadAsync();
                else if (OutputImageChannel is not null)
                {
                    if (!OutputImageChannel.Writer.TryWrite(cell))
                    {
                        //StringBuilder strbuilder = new StringBuilder("[");
                        //strbuilder.Append("相机");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   cell入列失败，丢弃。");
                        //CCameraManagement.CamLogger.Error(strbuilder.ToString());
                        cell.Dispose();
                    }
                    else
                    {
                        //StringBuilder strbuilder = new StringBuilder("[");
                        //strbuilder.Append("相机");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   cell入列完成。");
                        //SysLog.Info(strbuilder.ToString());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.InfoSerial
                        + Setting.SerialNumber
                        + Properties.Resources.ErrorExportImage
                        + ex.Message
                );
                return true;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 软触发触发拍照执行
        /// </summary>
        public virtual void ExecuteSoftwareTrigger()
        {
            CCameraManagement.CamLogger.Info(Properties.Resources.SoftWareOnce);
            timeOut.Restart();
            //imageQueue.Clear(); //拍照前清除
            while (ImageQueueChannel.Reader.TryRead(out _)) { }
            startGrabSoft = true;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化相机
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public bool InitializeCamera()
        {
            try
            {
                if (OpenCamera())
                {
                    UserLoadParam();
                    SetCameraParam();
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.InfoInit + Setting.SerialNumber
                    );
                    SysLog.Info(Properties.Resources.InfoInit + Setting.SerialNumber);

                    while (ImageQueueChannel.Reader.TryRead(out _)) { } //lhb2025.1.6 清理缓存，防止读取旧的已经被删除的内存导致报错
                    isStartGrabThread = true;
                    grabThread = new Thread(new ThreadStart(GrabThread));
                    grabThread.IsBackground = true;
                    grabThread.Start();

                    return true;
                }
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInit3 + Setting.SerialNumber
                );
                Growl.Error(Properties.Resources.ErrorInit3 + Setting.SerialNumber);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(Properties.Resources.ErrorInit2 + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 关闭相机
        /// </summary>
        /// <returns></returns>
        public void EndCamera()
        {
            try
            {
                if (Connected)
                {
                    StopGrab();
                    isStartGrabThread = false;
                    while (grabThread.IsAlive && grabThread.Join(500)) { }
                    //imageQueue.Clear();
                    UserSaveParam();
                    CloseCamera();
                    Connected = false;
                    while (ImageQueueChannel.Reader.TryRead(out _)) { } //lhb2025.1.6 清理缓存
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorClose + Setting.SerialNumber + ex.Message
                );
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置参数
        /// </summary>
        public virtual void SetCameraParam()
        {
            if (GetImageWidth(out var imageWidth))
            {
                Setting.ImageWidth = imageWidth;
            }
            if (GetImageHeight(out var imageHeight))
            {
                Setting.ImageHeight = imageHeight;
            }
            if (GetCameraType(out var cameraType))
            {
                Setting.CameraType = cameraType;
            }
            Setting.ExposureTime = Setting.ExposureTime;
            Setting.Gain = Setting.Gain;
            Setting.TriggerMode = Setting.TriggerMode;
            Setting.TriggerDelay = Setting.TriggerDelay;
            Setting.Gamma = Setting.Gamma;
            Setting.TriggerPulseWidth = Setting.TriggerPulseWidth;
        }

        public virtual int GetImageWidth()
        {
            switch (Setting.ImageRotate)
            {
                case EMIMAGEROTATE.EMROTATE90:
                    return Setting.ImageHeight;

                case EMIMAGEROTATE.EMROTATE270:
                    return Setting.ImageHeight;

                default:
                    return Setting.ImageWidth;
            }
        }

        public virtual int GetImageHeight()
        {
            switch (Setting.ImageRotate)
            {
                case EMIMAGEROTATE.EMROTATE90:
                    return Setting.ImageWidth;

                case EMIMAGEROTATE.EMROTATE270:
                    return Setting.ImageWidth;

                default:
                    return Setting.ImageHeight;
            }
        }

        public void SetTriggerModePro(EMTRIGGERMODE mode)
        {
            startGrabSoft = false;
            SetTriggerMode(mode);
        }

        public abstract bool OpenCamera();

        public abstract void CloseCamera();

        public abstract bool StartGrab();

        public abstract bool StopGrab();

        public abstract bool GetImageWidth(out int value);

        public abstract bool GetImageHeight(out int value);

        public abstract bool GetCameraType(out PixelFormat cameraType);

        protected abstract void SetTriggerMode(EMTRIGGERMODE mode);

        public abstract bool GetTriggerMode(out EMTRIGGERMODE mode);

        public abstract bool GetExposureTime(out uint value);

        public abstract void SetExposureTime(uint value);

        public abstract bool GetGain(out float value);

        public abstract void SetGain(float value);

        public abstract bool GetGamma(out float value);

        public abstract void SetGamma(float value);

        public abstract void SetTriggerDelay(uint value);

        public abstract bool GetTriggerDelay(out uint value);

        public abstract void SetTriggerPulseWidth(uint value);

        public abstract bool GetTriggerPulseWidth(out uint value);

        public abstract void UserSaveParam();

        public abstract void UserLoadParam();

        public abstract void SetStrobeEnable(bool enable);

        public abstract void SetLineSelector(object line);

        public abstract void SetStrobeDuration(uint value);

        public abstract void SetLineSource(object source);

        public abstract void SetLineInverter(bool enable);

        public abstract void SetLineMode(object lineMode);

        public abstract void LineTriggerSoftware();

        public abstract void SetGammaEnable(bool enable);

        public abstract float GetFps();

        public abstract void SetFrameCount(int count);

        public abstract void SetCustomParam(uint value);
    }
}