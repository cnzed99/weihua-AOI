using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using CameraModule;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json.Linq;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using Timer = System.Timers.Timer;

namespace OfflineTestCam
{
    /// <summary>
    ///2025.1.14 李焕彬
    ///缓存图像数据
    /// </summary>
    class CBuffer
    {
        public CBuffer(IntPtr ptr, bool isUsing)
        {
            this.ptr = ptr;
            this.isUsing = isUsing;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像指针
        /// </summary>
        public IntPtr ptr;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 是否正在使用
        /// </summary>
        public bool isUsing = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 编码器位置
        /// </summary>
        public int encoderPos = 0;
    }

    /// <summary>
    /// 2025.1.14 李焕彬
    /// 相机操作派生类
    /// </summary>
    public class CCamera : CCameraBase
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机参数
        /// </summary>
        internal CParameterSetting paramSetting { get; set; }

        public CCamera()
            : base() { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初筛算法日志
        /// </summary>
        protected CLogRec FilterLog = CLogRec.Create("Filter", "D:/Data");

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像缓存
        /// </summary>
        List<CBuffer> buffers = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 缓存个数
        /// </summary>
        int bufferSize = 100;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 帧数据大小
        /// </summary>
        int frameSize = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 当前写入帧序号
        /// </summary>
        int curFrame = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 线程锁
        /// </summary>
        object bufferLock = new object();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 断面初筛算法参数组
        /// </summary>
        List<SMaociAlgorParam> algParams = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 侧面初筛算法参数组
        /// </summary>
        List<SSideMaociAlgorParam> algParamSides = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 定时器线程
        /// </summary>
        Task taskTimerRecv;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 开始定时器
        /// </summary>
        bool startTimerRecv = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像索引
        /// </summary>
        int indexRecv = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 已加载图像集
        /// </summary>
        List<CImage> images = new List<CImage>();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 打开相机
        /// </summary>
        /// <returns>true打开成功, false打开失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                this.Connected = true;
                if (Directory.Exists(paramSetting.ImagePath))
                {
                    foreach (var item in Directory.GetFiles(paramSetting.ImagePath))
                    {
                        images.Add(new(item));
                    }
                }

                if (images.Count > 0)
                {
                    frameSize = images[0].ImageSize;
                    Task.Factory.StartNew(new Action(PreFilter)); //初筛

                    taskTimerRecv = Task.Factory.StartNew(() =>
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Highest;
                        startTimerRecv = true;
                        Stopwatch sw = Stopwatch.StartNew();
                        double msPerTick = 1000.0 / Stopwatch.Frequency;
                        while (startTimerRecv)
                        {
                            if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERNONE)
                            {
                                if (
                                    sw.ElapsedTicks * msPerTick
                                    > 1000.0 / ((double)paramSetting.InterTriggerFrequence)
                                )
                                {
                                    sw.Restart();
                                    lock (images)
                                    {
                                        if (images.Count > 0)
                                        {
                                            OnFrameReadyFunc(
                                                images[indexRecv % images.Count].ImageData
                                            );
                                            indexRecv++;
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                );
            }

            return true;
        }

        bool received = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 回调函数，当一帧图像采集完成时，函数被调用
        /// </summary>
        /// <param name="ptr">图像指针</param>
        public void OnFrameReadyFunc(IntPtr ptr)
        {
            try
            {
                startGrabSoft = true;
                grabCount++;
                StringBuilder textBuilder = new StringBuilder(Properties.Resources.InfoReceImage);
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());
                if (
                    paramSetting.UseFilter
                    && paramSetting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE
                )
                {
                    lock (bufferLock)
                    {
                        if (!buffers[curFrame % bufferSize].isUsing)
                        {
                            CopyMemory1(ptr, buffers[curFrame % bufferSize].ptr, frameSize);
                            curFrame++;
                        }
                        else
                        {
                            FilterLog.Error($"Lost Frame:{grabCount}!");
                        }
                    }
                }
                else
                {
                    ImageQueueChannel.Writer.TryWrite(ptr);
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初筛处理线程
        /// </summary>
        public void PreFilter()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            for (int g = 0; g < bufferSize; g++)
            {
                buffers.Add(new(Marshal.AllocHGlobal(frameSize), false));
                algParams.Add(new(paramSetting, paramSetting.MmPerPixel * 1000));
                algParamSides.Add(new(paramSetting, paramSetting.MmPerPixel * 1000));
            }
            IntPtr ptrResult = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 50);
            int[] result = new int[50];
            int[] indexs = new int[50];
            IntPtr[] intPtrs = new IntPtr[50];
            int useBuffers = 0;
            curFrame = 0;
            int lastFrame = 0;
            int stride = Setting.ImageWidth * ((8 + 7) / 8);
            Stopwatch sw = new Stopwatch();
            Stopwatch swOkShow = Stopwatch.StartNew(); //OK显示计时器
            Stopwatch swFilterCount = Stopwatch.StartNew();
            int filterCount = 0;
            while (Connected)
            {
                if (swFilterCount.ElapsedMilliseconds > 1000 && filterCount > 0)
                {
                    swFilterCount.Restart();
                    FilterLog.Info($"执行初筛算法图像数:{filterCount}!");
                    filterCount = 0;
                }
                if (paramSetting.UseFilter)
                {
                    if (curFrame > lastFrame)
                    {
                        lock (bufferLock)
                        {
                            if (curFrame - lastFrame > 50)
                            {
                                FilterLog.Error($"处理帧数超过50，丢失帧数:{curFrame - lastFrame - 50}!");
                                lastFrame = curFrame - 50;
                            }
                            useBuffers = 0;
                            for (int g = lastFrame; g < curFrame; g++)
                            {
                                intPtrs[useBuffers] = buffers[g % bufferSize].ptr;
                                indexs[useBuffers] = g % bufferSize;
                                buffers[indexs[useBuffers]].isUsing = true;
                                useBuffers++;
                            }
                            lastFrame = curFrame;
                        }
                        sw.Restart();
                        switch (paramSetting.PreAlgorithm)
                        {
                            case EMPREALGORITHM.EMPREALGORITHMMAOCI:
                                PreTest(
                                    intPtrs,
                                    useBuffers,
                                    Setting.ImageWidth,
                                    Setting.ImageHeight,
                                    Setting.ImageWidth,
                                    algParams.ToArray(),
                                    ptrResult
                                );
                                break;
                            case EMPREALGORITHM.EMPREALGORITHMSIDEMAOCI:
                                PreTestSide(
                                    intPtrs,
                                    useBuffers,
                                    Setting.ImageWidth,
                                    Setting.ImageHeight,
                                    Setting.ImageWidth,
                                    algParamSides.ToArray(),
                                    ptrResult
                                );
                                break;
                        }

                        sw.Stop();
                        Marshal.Copy(ptrResult, result, 0, useBuffers);
                        int ngCount = 0;
                        lock (bufferLock)
                        {
                            for (int g = 0; g < useBuffers; g++)
                            {
                                if (result[g] != 0)
                                {
                                    ImageQueueChannel.Writer.TryWrite(intPtrs[g]);
                                    ngCount++;
                                }
                                buffers[indexs[g]].isUsing = false;
                            }
                            if (ngCount > 0)
                            {
                                swOkShow.Restart();
                            }
                            else
                            {
                                if (
                                    swOkShow.Elapsed.TotalSeconds
                                    > ((CParameterSetting)Setting).OKFrameTicks
                                )
                                {
                                    swOkShow.Restart();
                                    ImageQueueChannel.Writer.TryWrite(intPtrs[0]);
                                }
                            }
                        }
                        FilterLog.Info(
                            $"执行初筛算法图像数:{useBuffers},处理时间：{sw.ElapsedMilliseconds}ms,NG数{ngCount}!"
                        );
                        filterCount += useBuffers;
                    }
                }
            }
            for (int g = 0; g < bufferSize; g++)
            {
                Marshal.FreeHGlobal(buffers[g].ptr);
            }
            buffers.Clear();
            algParams.Clear();
            algParamSides.Clear();
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 未用
        /// </summary>
        public bool SetInternalTrigFreq(int frameCount)
        {
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 更新初筛参数
        /// </summary>
        public void UpdateFilterParam()
        {
            for (int g = 0; g < bufferSize; g++)
            {
                algParams[g] = new(paramSetting, paramSetting.MmPerPixel * 1000);
                algParamSides[g] = new(paramSetting, paramSetting.MmPerPixel * 1000);
            }
            MessageBox.Show("更新初筛参数完成！");
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像数据
        /// </summary>
        /// <param name="zoo">图像数据</param>
        /// <returns></returns>
        public override bool GetImageFunc(IntPtr zoo)
        {
            try
            {
                return base.GetImageFunc(zoo);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorImageFunc + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            startTimerRecv = false;
            taskTimerRecv?.Wait();
            lock (images)
            {
                foreach (var item in images)
                {
                    item.Dispose();
                }
                images.Clear();
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 开始采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StartGrab()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorStart2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 停止采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StopGrab()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorStop2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
            {
                if (images.Count > 0)
                {
                    OnFrameReadyFunc(images[indexRecv % images.Count].ImageData);
                    indexRecv++;
                }
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            //CameraHandle cameraHandle = new CameraHandle();
            //IKapBoard.IKapGetInfo(m_hBoard, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref cameraHandle);
            //value = cameraHandle;
            //return true;
            if (images.Count > 0)
            {
                value = images[0].ImageWidth;
                return true;
            }
            else
            {
                value = 10;
                return false;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            if (images.Count > 0)
            {
                value = images[0].ImageHeight;
                return true;
            }
            else
            {
                value = 10;
                return false;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            if (images.Count > 0)
            {
                cameraType = images[0].PixelFormat;
                return true;
            }
            else
            {
                cameraType = PixelFormats.Gray8;
                return false;
            }
        }

        EMTRIGGERMODE modeSet;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置触发源
        /// </summary>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            modeSet = mode;
        }

        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            mode = modeSet;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            value = 100;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            value = 100;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            //try
            //{
            //    int gamma = 0;
            //    CameraSdkStatus status = MvApi.CameraGetGamma(m_hCamera, ref gamma);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = gamma / 100;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetGamma + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetGamma2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetGamma(m_hCamera, (int)value * 100);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetGamma + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetGamma2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetStrobeDelayTime(m_hCamera, value);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetTriggerDelay + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetTriggerDelay2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            //try
            //{
            //    uint delayTime = 0;
            //    CameraSdkStatus status = MvApi.CameraGetStrobeDelayTime(m_hCamera, ref delayTime);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = delayTime;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetTriggerDelay + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerDelay2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetStrobePulseWidth(m_hCamera, value);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetTriggerPulseWidth + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetTriggerPulseWidth2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerPulseWidth(out uint value)
        {
            //try
            //{
            //    uint delayTime = 0;
            //    CameraSdkStatus status = MvApi.CameraGetStrobePulseWidth(m_hCamera, ref delayTime);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = delayTime;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetTriggerPulseWidth + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerPulseWidth2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam() { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam() { }

        public override void SetCustomParam(uint value)
        {
            throw new NotImplementedException();
        }

        public override void SetStrobeEnable(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSelector(object Line)
        {
            throw new NotImplementedException();
        }

        public override void SetStrobeDuration(uint Value)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSource(object source)
        {
            throw new NotImplementedException();
        }

        public override void SetLineInverter(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineMode(object LineMode)
        {
            throw new NotImplementedException();
        }

        public override void LineTriggerSoftware()
        {
            throw new NotImplementedException();
        }

        public override void SetGammaEnable(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override float GetFps()
        {
            throw new NotImplementedException();
        }

        public override void SetFrameCount(int count)
        {
            throw new NotImplementedException();
        }
    }
}
