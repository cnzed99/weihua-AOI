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
using AlgorithmDll;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace CameraModule
{
    /// <summary>
    /// 李焕彬 2024.7.24
    /// 相机操作基类
    /// </summary>
    public abstract class CCameraBase
    {
        /// <summary>
        /// 李焕彬 2024.7.24
        /// 旋转图像
        /// </summary>
        /// <param name="rotate">旋转角度，对应EMIMAGEROTATE</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="nLine">行宽</param>
        /// <param name="data">图像数据</param>
        /// <param name="widthOut">旋转后宽度</param>
        /// <param name="heightOut">旋转后高度</param>
        /// <param name="nLineOut">旋转后行宽</param>
        /// <param name="dataOut">旋转后图像数据</param>
        [DllImport("MaociAlg.dll")]
        public static extern void RotateImage(
            int rotate,
            int width,
            int height,
            int nLine,
            IntPtr data,
            int widthOut,
            int heightOut,
            int nLineOut,
            IntPtr dataOut
        );

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 对应制程名Guid
        /// </summary>
        public string ProjGuid { get; set; } = "";

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 相机回调日志
        /// </summary>
        protected CLogRec callbackLogger = CLogRec.Create("CamCallBack", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 相机生成图日志
        /// </summary>
        protected CLogRec getImageLogger = CLogRec.Create("CamGetImage", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 相机参数
        /// </summary>
        public CCameraParameterBase Setting { get; set; }

        public CCameraBase()
        {
            if (grabThread == null)
            {
                grabThread = new Thread(new ThreadStart(GrabThread));
                grabThread.IsBackground = true;
                grabThread.Start();
            }
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
        /// 通道数
        /// </summary>
        private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(
            100
        )
        {
            FullMode = BoundedChannelFullMode.DropWrite
        };

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 采集 信号
        /// </summary>
        public Channel<Cell> triggerImageChannel = Channel.CreateBounded<Cell>(channelOptions);

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 采集 图像队列
        /// </summary>
        public static Channel<Cell> waitGetImageChannel = Channel.CreateBounded<Cell>(
            channelOptions
        );

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 图像传递委托
        /// </summary>
        public Action<Cell> GrabFinishEvent { get; set; }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 图片队列
        /// </summary>
        protected Queue imageQueue = new Queue();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 取图线程
        /// </summary>
        protected Thread grabThread;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 线程锁
        /// </summary>
        protected Mutex mutex = new Mutex();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 超时计时
        /// </summary>
        protected Stopwatch timeOut = new Stopwatch();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 单帧回调时间
        /// </summary>
        protected Stopwatch callBacktime = new Stopwatch();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 合成转换时间 从拍照到传出图像
        /// </summary>
        public Stopwatch GetImagetime { get; set; } = new Stopwatch();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 是否丢帧
        /// </summary>
        protected bool IsLostFrame { get; set; } = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 开启采图
        /// </summary>
        protected bool startGrab = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 防止丢帧时 再次传出图片
        /// </summary>
        protected bool canGo = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 一个完整流程结束(无论正常还是丢帧) 限制循环进入
        /// </summary>
        protected bool noOver = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 判断是否正在采集图片
        /// </summary>
        protected bool isGrabing = false;

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
        /// 2024.7.23 李焕彬
        /// 取图
        /// </summary>
        /// <param name="grabbedRawData">图像数据</param>
        public virtual async Task<bool> GetImageFunc(IntPtr grabbedRawData)
        {
            int widthNew = Setting.ImageWidth;
            int heightNew = Setting.ImageHeight;
            int bitsPerPixel = Setting.CameraType == EMCAMERATYPE.EMCAMTYPEGRAY ? 8 : 24;
            int stride = Setting.ImageWidth * ((bitsPerPixel + 7) / 8);
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
            IntPtr ptrNew = Marshal.AllocHGlobal(strideNew * heightNew);
            RotateImage(
                (int)Setting.ImageRotate,
                Setting.ImageWidth,
                Setting.ImageHeight,
                stride,
                grabbedRawData,
                widthNew,
                heightNew,
                strideNew,
                ptrNew
            );
            CImage image = new CImage(
                widthNew,
                heightNew,
                strideNew,
                ptrNew,
                Setting.CameraType == EMCAMERATYPE.EMCAMTYPEGRAY
                    ? PixelFormats.Gray8
                    : PixelFormats.Rgb24
            );
            await ExportImage(image);

            return true;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 主动取流线程
        /// </summary>
        /// <param name="grabbedRawData">图像数据</param>
        public async virtual void GrabThread()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            StringBuilder textBuilder = new StringBuilder();
            while (true)
            {
                if (!startGrab)
                {
                    Thread.Sleep(10);
                    continue;
                }
                if (this.noOver)
                {
                    if ((int)timeOut.ElapsedMilliseconds >= Setting.TimeOut)
                    {
                        getImageLogger.Info(Properties.Resources.ErrorLostImage);
                        if (LostImage != null)
                        {
                            IsLostFrame = true;
                            await ExportImage(LostImage);
                        }
                        textBuilder.Clear();
                        textBuilder.Append(Properties.Resources.ErrorLostImage2);
                        textBuilder.Append(timeOut.ElapsedMilliseconds);
                        getImageLogger.Info(textBuilder.ToString());
                    }

                    if (this.imageQueue.Count > 0 && this.noOver)
                    {
                        IntPtr zero = IntPtr.Zero;
                        await GetImageFunc(zero);
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 传出图像
        /// </summary>
        /// <param name="outImage">图像</param>
        public async Task<bool> ExportImage(CImage outImage)
        {
            try
            {
                //需要增加判断是否是运行模式
                if (true)
                {
                    Cell cell = new Cell();
                    cell.ImageCam = outImage;
                    cell.FrameLoss = IsLostFrame;
                    GrabFinishEvent.Invoke(cell);
                    noOver = false;
                    isGrabing = false;
                    GetImagetime.Stop();
                }
                else
                {
                    Cell cell = await triggerImageChannel.Reader.ReadAsync();
                    cell.ImageCam = outImage;
                    cell.FrameLoss = IsLostFrame;
                    cell.CamSerial = Setting.SerialNumber;
                    await waitGetImageChannel.Writer.WriteAsync(cell);
                    GrabFinishEvent.Invoke(cell);
                    noOver = false;
                    isGrabing = false;
                    GetImagetime.Stop();
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
            startGrab = true;
            timeOut.Restart();
            imageQueue.Clear(); //拍照前清除
            callBacktime.Restart();
            GetImagetime.Restart();
            noOver = true;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化相机
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public bool InitializeCamera()
        {
            if (OpenCamera())
            {
                UserLoadParam();
                SetCameraParam();
                return true;
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
            UserSaveParam();
            CloseCamera();
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化相机
        /// </summary>
        /// <returns>2024.7.23 李焕彬</returns>
        public abstract bool OpenCamera();

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

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 关闭相机，关闭之前执行
        /// </summary>
        public abstract void CloseCamera();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 开始采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public abstract bool StartGrab();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 停止采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public abstract bool StopGrab();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetImageWidth(out int value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetImageHeight(out int value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetCameraType(out EMCAMERATYPE cameraType);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 修改相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        public abstract void SetTriggerMode(bool useTrigger);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetTriggerMode(out bool useTrigger);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetExposureTime(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public abstract void SetExposureTime(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetGain(out float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public abstract void SetGain(float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetGamma(out float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public abstract void SetGamma(float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public abstract void SetTriggerDelay(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetTriggerDelay(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public abstract void SetTriggerPulseWidth(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public abstract bool GetTriggerPulseWidth(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 保存用户参数
        /// </summary>
        public abstract void UserSaveParam();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 加载用户参数
        /// </summary>
        public abstract void UserLoadParam();
    }
}
