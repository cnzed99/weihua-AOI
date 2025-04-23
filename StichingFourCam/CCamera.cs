using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using CameraModule;
using CommunityToolkit.Mvvm.DependencyInjection;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;
using Timer = System.Timers.Timer;

namespace StichingFourCam
{
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

        HDevelopExportPro hDevelopExport { get; set; }

        public static readonly BoundedChannelOptions s_SaveImgchannelOptions =
            new BoundedChannelOptions(20) { FullMode = BoundedChannelFullMode.Wait };

        /// <summary>
        /// 取图队列
        /// </summary>
        public Channel<Cell> m_WaitImgChannel = Channel.CreateBounded<Cell>(
            s_SaveImgchannelOptions
        );

        /// <summary>
        /// 2025.4.20 李焕彬
        /// 拼图算法日志
        /// </summary>
        public static CLogRec StichingLog = CLogRec.Create("StichingAlg", "D:/Data");

        List<string> camSerials;

        public CCamera()
            : base() { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 打开相机
        /// </summary>
        /// <returns>true打开成功, false打开失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                if (
                    CCameraManagement.CameraDict[paramSetting.SerialNumber1].Connected
                    && CCameraManagement.CameraDict[paramSetting.SerialNumber2].Connected
                    && CCameraManagement.CameraDict[paramSetting.SerialNumber3].Connected
                    && CCameraManagement.CameraDict[paramSetting.SerialNumber4].Connected
                )
                {
                    camSerials = new List<string>()
                    {
                        paramSetting.SerialNumber1,
                        paramSetting.SerialNumber2,
                        paramSetting.SerialNumber3,
                        paramSetting.SerialNumber4
                    };
                    hDevelopExport = new(
                        paramSetting,
                        CCameraManagement.CameraDict[paramSetting.SerialNumber1].GetImageWidth(),
                        CCameraManagement.CameraDict[paramSetting.SerialNumber1].GetImageHeight()
                    );
                    m_WaitImgChannel = Channel.CreateBounded<Cell>(s_SaveImgchannelOptions);
                    CCameraManagement.CameraDict[paramSetting.SerialNumber1].OutputImageChannel =
                        m_WaitImgChannel;
                    CCameraManagement.CameraDict[paramSetting.SerialNumber2].OutputImageChannel =
                        m_WaitImgChannel;
                    CCameraManagement.CameraDict[paramSetting.SerialNumber3].OutputImageChannel =
                        m_WaitImgChannel;
                    CCameraManagement.CameraDict[paramSetting.SerialNumber4].OutputImageChannel =
                        m_WaitImgChannel;

                    this.Connected = true;
                    StartReceiveThread();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Connected = false;
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }

        /// <summary>
        /// 2025.4.19 李焕彬
        /// 汇总结果lock用
        /// </summary>
        Object objLock = new object();

        /// <summary>
        /// 2025.4.19 李焕彬
        /// 4个相机cell存储
        /// </summary>
        List<(DateTime createTime, Cell cell)> saveCells = new();

        Task taskReceive = null;

        public void StartReceiveThread()
        {
            taskReceive = Task.Factory.StartNew(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                await foreach (Cell cell in m_WaitImgChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        lock (objLock)
                        {
                            var timeNow = DateTime.Now;
                            saveCells.Add((timeNow, cell));
                            if (saveCells.Count > 10)
                            {
                                saveCells[0].cell.Dispose();
                                saveCells.RemoveAt(0);
                            }
                            bool isFindAll = true;
                            foreach (var item in camSerials)
                            {
                                if (
                                    saveCells.FindIndex(c =>
                                        c.cell.CamSerial == item
                                        && (timeNow - c.createTime)
                                            < TimeSpan.FromMilliseconds(paramSetting.TimeLimit)
                                    ) < 0
                                )
                                    isFindAll = false;
                            }
                            if (isFindAll)
                            {
                                List<Cell> cellFind = new List<Cell>();
                                foreach (var item in camSerials)
                                {
                                    var result = saveCells.Find(c =>
                                        c.cell.CamSerial == item
                                        && (timeNow - c.createTime)
                                            < TimeSpan.FromMilliseconds(paramSetting.TimeLimit)
                                    );
                                    cellFind.Add(result.cell);
                                }
                                Stopwatch sw = Stopwatch.StartNew();
                                var stichingImage = hDevelopExport.action(
                                    cellFind[0].Image,
                                    cellFind[1].Image,
                                    cellFind[2].Image,
                                    cellFind[3].Image
                                );
                                paramSetting.ProcessTime = sw.ElapsedMilliseconds;
                                StichingLog.Info($"执行拼图算法处理时间：{sw.ElapsedMilliseconds}ms!");
                                paramSetting.ImageWidth = stichingImage.ImageWidth;
                                paramSetting.ImageHeight = stichingImage.ImageHeight;
                                imageBufferStride = stichingImage.ImageWidth * 3;
                                OnFrameReadyFunc(stichingImage.ImageData);

                                foreach (var item in saveCells)
                                {
                                    item.cell.Dispose();
                                }
                                saveCells.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CCameraManagement.CamLogger.Error(
                            "拼接图像错误" + paramSetting.SerialNumber + ex.Message
                        );
                    }
                }
            });
        }

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

                ImageQueueChannel.Writer.TryWrite(ptr);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
            }
        }

        /// <summary>
        /// 更新拼图参数
        /// </summary>
        public void UpdateStichingParam()
        {
            try
            {
                lock (objLock)
                {
                    if (Connected)
                    {
                        hDevelopExport.terminal();
                        hDevelopExport = new(
                            paramSetting,
                            CCameraManagement
                                .CameraDict[paramSetting.SerialNumber1]
                                .GetImageWidth(),
                            CCameraManagement
                                .CameraDict[paramSetting.SerialNumber1]
                                .GetImageHeight()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    "拼接参数更新设置错误" + paramSetting.SerialNumber + ex.Message
                );
            }
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
            try
            {
                CCameraManagement.CameraDict[paramSetting.SerialNumber1].OutputImageChannel = null;
                CCameraManagement.CameraDict[paramSetting.SerialNumber2].OutputImageChannel = null;
                CCameraManagement.CameraDict[paramSetting.SerialNumber3].OutputImageChannel = null;
                CCameraManagement.CameraDict[paramSetting.SerialNumber4].OutputImageChannel = null;
                m_WaitImgChannel.Writer.Complete();
                taskReceive?.Wait();
                hDevelopExport?.terminal();
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorClose + paramSetting.SerialNumber + ex.Message
                );
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
            if (Connected)
            {
                try
                {
                    CCameraManagement
                        .CameraDict[paramSetting.SerialNumber1]
                        .ExecuteSoftwareTrigger();
                    CCameraManagement
                        .CameraDict[paramSetting.SerialNumber2]
                        .ExecuteSoftwareTrigger();
                    CCameraManagement
                        .CameraDict[paramSetting.SerialNumber3]
                        .ExecuteSoftwareTrigger();
                    CCameraManagement
                        .CameraDict[paramSetting.SerialNumber4]
                        .ExecuteSoftwareTrigger();
                }
                catch (Exception ex)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSoftWare2 + paramSetting.SerialNumber + ex.Message
                    );
                }

                base.ExecuteSoftwareTrigger();
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
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            cameraType = PixelFormats.Rgb24;
            return true;
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
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value) { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerPulseWidth(out uint value)
        {
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
