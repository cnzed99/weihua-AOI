using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using CameraModule;
using DVPCameraType;

namespace DoThinkCam
{
    public class CDoThinkCamera : CCameraBase
    {
        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机参数
        /// </summary>
        internal CDoThinkParameterSetting paramSetting { get; set; }

        public CDoThinkCamera()
            : base() { }

        /// <summary>
        /// 相机对象
        /// </summary>
        protected uint CamHandle = 0; //相机句柄

        /// <summary>
        /// 相机设备列表
        /// </summary>
        dvpCameraInfo CameraInfo = new dvpCameraInfo(); //相机信息

        /// <summary>
        /// 回调
        /// </summary>
        private DVPCamera.dvpStreamCallback ImgCallback; //相机取图回调函数

        public IntPtr m_Ptr = new IntPtr();

        /// <summary>
        /// 初始化相机
        /// </summary>
        /// <returns></returns>
        public override bool OpenCamera()
        {
            try
            {
                bool result = false;

                uint num = 0;
                dvpStatus status = DVPCamera.dvpRefresh(ref num);

                if (num == 0)
                {
                    CCameraManagement.CamLogger.Error(Properties.Resources.CameraNotFound);
                    return result;
                }
                //遍历所有相机
                for (uint i = 0; i < num; i++)
                {
                    // 枚举相机信息
                    status = DVPCamera.dvpEnum(i, ref CameraInfo);
                    //string FriendlyName = "";
                    //if (status == dvpStatus.DVP_STATUS_OK)
                    //{
                    //    FriendlyName = CameraInfo.FriendlyName;
                    //}

                    if (paramSetting.SerialNumber != null)
                    {
                        if (paramSetting.SerialNumber == CameraInfo.SerialNumber)
                        {
                            if (paramSetting.Enable)
                            {
                                CCameraManagement.CamLogger.Info(
                                    Properties.Resources.InitialCamera + paramSetting.SerialNumber
                                );

                                if (IsValidHandle(CamHandle))
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.InitialCamera
                                            + paramSetting.SerialNumber
                                            + Properties.Resources.AppearError
                                    );
                                    return result;
                                }
                                else
                                {
                                    //status = DVPCamera.dvpOpenByName(FriendlyName, dvpOpenMode.OPEN_NORMAL, ref CamHandle);
                                    status = DVPCamera.dvpOpen(
                                        i,
                                        dvpOpenMode.OPEN_NORMAL,
                                        ref CamHandle
                                    );
                                    if (status != dvpStatus.DVP_STATUS_OK)
                                    {
                                        CCameraManagement.CamLogger.Error(
                                            Properties.Resources.InitialCamera
                                                + paramSetting.SerialNumber
                                                + Properties.Resources.OpenCameraFailReseason
                                        );
                                    }

                                    ImgCallback += ImageCallbackFunc;
                                    using (Process curProcess = Process.GetCurrentProcess())
                                    using (ProcessModule curModule = curProcess.MainModule)
                                    {
                                        status = DVPCamera.dvpRegisterStreamCallback(
                                            CamHandle,
                                            ImgCallback,
                                            dvpStreamEvent.STREAM_EVENT_PROCESSED,
                                            m_Ptr
                                        );
                                        if (status == dvpStatus.DVP_STATUS_OK)
                                        {
                                            CCameraManagement.CamLogger.Info(
                                                Properties.Resources.InitialCamera
                                                    + paramSetting.SerialNumber
                                                    + Properties
                                                        .Resources
                                                        .RegisterCallbackFunctionSucess
                                            );
                                        }
                                        else
                                        {
                                            CCameraManagement.CamLogger.Error(
                                                Properties.Resources.InitialCamera
                                                    + paramSetting.SerialNumber
                                                    + Properties
                                                        .Resources
                                                        .RegisterCallbackFunctionFail
                                            );
                                        }
                                    }
                                }

                                //if (this._grabThread == null)
                                //{
                                //    this._grabThread = new Thread(new ThreadStart(GrabThread));
                                //    this._grabThread.IsBackground = true;
                                //    this._grabThread.Start();
                                //}
                                this.Connected = true;
                                SetPixelFormat();
                                SetParameters(); //设置参数要在采集前
                                if (!StartGrab())
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.InitialCamera
                                            + paramSetting.SerialNumber
                                            + Properties.Resources.ExecuteStartGrabFail
                                    );
                                    return result;
                                }
                                else
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.InitialCamera
                        + paramSetting.SerialNumber
                        + Properties.Resources.AppearError
                        + ex.Message
                );
                throw;
            }
        }

        float _grabCount = 0;
        float _onceCount = 0;

        int inqueue = 0; //回调里入队列计数

        private int ImageCallbackFunc( /*dvpHandle*/
            uint handle,
            dvpStreamEvent _event,
            IntPtr pContext,
            ref dvpFrame refFrame,
            IntPtr pBuffer
        )
        {
            try
            {
                startGrabSoft = true;
                grabCount++;
                StringBuilder textBuilder = new StringBuilder("Do3Think");
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());

                //mutex.WaitOne();
                //imageQueue.Enqueue(pBuffer);
                //mutex.ReleaseMutex();
                ImageQueueChannel.Writer.TryWrite(pBuffer);
                paramSetting.ImageWidth = refFrame.iWidth;
                paramSetting.ImageHeight = refFrame.iHeight;

                paramSetting.CameraType =
                    refFrame.format == dvpImageFormat.FORMAT_MONO
                        ? PixelFormats.Gray8
                        : PixelFormats.Rgb24;

                return 0;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ImageCallbackFuncError
                        + paramSetting.SerialNumber
                        + ex.Message
                );

                return 1;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像数据
        /// </summary>
        /// <param name="zoo">图像数据</param>
        /// <returns></returns>
        public override bool GetImageFunc(IntPtr zoo)
        {
            try
            {
                //IntPtr grabbedRawData = (IntPtr)this.imageQueue.Dequeue();
                //return base.GetImageFunc(grabbedRawData);
                return base.GetImageFunc(zoo);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.GetImageFuncError + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }

        //dvpImageFormat pixelFmt = dvpImageFormat.FORMAT_MONO;
        //public override void ColorimgTransfer(out HObject image, IntPtr grabbedRawData)
        //{
        //    image=new HObject();
        //    HOperatorSet.GenEmptyObj(out image);
        //    switch (pixelFmt)
        //    {
        //        case dvpImageFormat.FORMAT_MONO:
        //            _DSparameters.CameraType = CAMERATYPE.黑白;
        //            HOperatorSet.GenImage1(out image, "byte", Width, Height, grabbedRawData);
        //            break;

        //        case dvpImageFormat.FORMAT_BAYER_RG:
        //            _DSparameters.CameraType = CAMERATYPE.彩色;
        //            HOperatorSet.GenImage1(out HObject imageTmp, "byte", Width, Height, grabbedRawData);
        //            HOperatorSet.CfaToRgb(imageTmp, out image, "bayer_rg", "bilinear_dir");
        //            imageTmp.Dispose();
        //            CloseLight(); //注意:度申彩色相机特殊触发光源方式,用在黑白相机会产生卡顿
        //            break;
        //        case dvpImageFormat.FORMAT_RGB24:
        //            _DSparameters.CameraType = CAMERATYPE.彩色;
        //            HOperatorSet.GenImageInterleaved(out image, grabbedRawData, "rgb", Width, Height, -1, "byte", 0, 0, 0, 0, -1, 0);
        //            break;
        //    }

        //}

        /// <summary>
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            try
            {
                if (this.Connected)
                {
                    if (IsValidHandle(CamHandle))
                    {
                        CCameraManagement.CamLogger.Info(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteCloseCamera
                        );
                        if (ImgCallback != null)
                        {
                            ImgCallback -= ImageCallbackFunc;
                        }
                        DVPCamera.dvpStop(CamHandle);
                        DVPCamera.dvpClose(CamHandle);
                        Connected = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteCloseCameraError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public override bool StartGrab()
        {
            try
            {
                bool result = false;
                dvpStreamState state = new dvpStreamState();
                if (state != dvpStreamState.STATE_STARTED)
                {
                    DVPCamera.dvpStart(CamHandle);
                    result = true;
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteStartGrab
                    );
                }
                return result;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteStartGrabError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public override bool StopGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    dvpStatus state = DVPCamera.dvpStop(CamHandle);
                    if (state != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteStopGrab
                        );
                    }
                    else
                    {
                        CCameraManagement.CamLogger.Info(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteStopGrabFail
                        );
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteStopGrabError
                        + ex.Message
                );
                throw;
            }
        }

        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            switch (mode)
            {
                case EMTRIGGERMODE.EMTRIGGERNONE:
                    SetTriggerMode(false);
                    break;
                case EMTRIGGERMODE.EMTRIGGERSOFTWARE:
                    SetTriggerMode(true);
                    break;
                case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                    SetTriggerMode(true);
                    break;
            }
        }

        /// <summary>
        /// 修改相机触发模式和触发源
        /// </summary>
        public void SetTriggerMode(bool modle)
        {
            try
            {
                if (this.Connected)
                {
                    if (IsValidHandle(CamHandle))
                    {
                        CCameraManagement.CamLogger.Info(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetTriggerModeIs
                                + modle.ToString()
                        );
                        dvpStatus status = DVPCamera.dvpSetTriggerState(CamHandle, modle);
                        if (status != dvpStatus.DVP_STATUS_OK)
                        {
                            CCameraManagement.CamLogger.Info(
                                Properties.Resources.CameraSerialNumber
                                    + paramSetting.SerialNumber
                                    + Properties.Resources.ExecuteSetTriggerModeFail
                                    + modle.ToString()
                            );
                        }
                        bool isOn = false;
                        status = DVPCamera.dvpGetTriggerState(CamHandle, ref isOn);
                        if (isOn) //判断是触发模式还是连续模式
                        {
                            dvpTriggerSource sorue = dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
                            status = DVPCamera.dvpGetTriggerSource(CamHandle, ref sorue);
                            //if (sorue == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE)
                            //{
                            //    paramSetting.TriggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            //}
                            //else
                            //{
                            //    paramSetting.TriggerMode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            //}
                        }
                        else
                        {
                            //paramSetting.TriggerMode = EMTRIGGERMODE.EMTRIGGERNONE;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetTriggerModeError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 修改相机外触发源
        /// </summary>
        public void SetTriggerSource(object triggerSource)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetTriggerSourceIs
                            + Convert.ToString(triggerSource)
                    );
                    dvpStatus status = DVPCamera.dvpSetTriggerSource(
                        CamHandle,
                        (dvpTriggerSource)
                            Enum.Parse(typeof(dvpTriggerSource), Convert.ToString(triggerSource))
                    );
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetTriggerSourceFail
                        );
                    }
                    dvpTriggerSource sorue = dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
                    status = DVPCamera.dvpGetTriggerSource(CamHandle, ref sorue);
                    if (sorue == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE)
                    {
                        //paramSetting.TriggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                    }
                    else
                    {
                        //paramSetting.TriggerMode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetTriggerSourceError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            try
            {
                if (this.Connected)
                {
                    if (IsValidHandle(CamHandle))
                    {
                        if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
                        {
                            base.ExecuteSoftwareTrigger();
                            if (paramSetting.CameraType == PixelFormats.Rgb24)
                            {
                                OpenLight();
                            }
                            getImageLogger.Info(Properties.Resources.ExecuteSoftwareTriggerOnce);
                            _onceCount = 0;
                            base.ExecuteSoftwareTrigger();
                            //一旦执行这个函数就相当于生成一个外部触发器
                            //注意:如果曝光时间过长，点击“发送软触发信号”太快可能会导致触发失败
                            //因为前一帧可能处于连续曝光或输出不完全的状态
                            dvpStatus status = DVPCamera.dvpTriggerFire(CamHandle);
                            if (status != dvpStatus.DVP_STATUS_OK)
                            {
                                CCameraManagement.CamLogger.Error(
                                    Properties.Resources.CameraSerialNumber
                                        + paramSetting.SerialNumber
                                        + Properties.Resources.ExecuteSoftwareTriggerFail
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSoftwareTriggerError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 获取最小曝光时间
        /// </summary>
        public int GetExposureTimeMin()
        {
            try
            {
                int min = 0;
                if (this.Connected)
                {
                    dvpDoubleDescr ExposureInfo = new dvpDoubleDescr();
                    DVPCamera.dvpGetExposureDescr(CamHandle, ref ExposureInfo);
                    min = (int)ExposureInfo.fMin;
                }
                return min;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteGetExposureTimeMinError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 获取最大曝光时间
        /// </summary>
        public int GetExposureTimeMax()
        {
            try
            {
                int max = 0;
                if (this.Connected)
                {
                    dvpDoubleDescr ExposureInfo = new dvpDoubleDescr();
                    DVPCamera.dvpGetExposureDescr(CamHandle, ref ExposureInfo);
                    max = (int)ExposureInfo.fMax;
                }
                return max;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteGetExposureTimeMaxError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            value = 0;
            try
            {
                if (this.Connected)
                {
                    double ExposureTime = 0;
                    DVPCamera.dvpGetExposure(CamHandle, ref ExposureTime);
                    value = (uint)ExposureTime;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteGetExposureTimeError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            SetExposureTime(value, 4);
        }

        /// <summary>
        /// 设置曝光值
        /// </summary>
        /// <param name="Value">曝光值</param>
        /// <returns></returns>
        public void SetExposureTime(uint value, uint Channel)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetExposureTime
                            + value
                    );
                    // dvpStatus status = DVPCamera.dvpSetExposure(CamHandle, (double)value);
                    dvpStatus status;
                    if (paramSetting.CameraType == PixelFormats.Gray8)
                    {
                        switch (Channel)
                        {
                            case 1:
                                status = DVPCamera.dvpWriteGenICamReg(CamHandle, 0x1201000, value);
                                if (status != dvpStatus.DVP_STATUS_OK)
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.CameraSerialNumber
                                            + paramSetting.SerialNumber
                                            + Properties
                                                .Resources
                                                .ExecuteSetChannel1ExposureTimeFail
                                    );
                                }
                                break;
                            case 2:
                                status = DVPCamera.dvpWriteGenICamReg(CamHandle, 0x1201004, value);
                                if (status != dvpStatus.DVP_STATUS_OK)
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.CameraSerialNumber
                                            + paramSetting.SerialNumber
                                            + Properties
                                                .Resources
                                                .ExecuteSetChannel2ExposureTimeFail
                                    );
                                }
                                break;
                            case 3:
                                status = DVPCamera.dvpWriteGenICamReg(CamHandle, 0x1201008, value);
                                if (status != dvpStatus.DVP_STATUS_OK)
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.CameraSerialNumber
                                            + paramSetting.SerialNumber
                                            + Properties
                                                .Resources
                                                .ExecuteSetChannel3ExposureTimeFail
                                    );
                                }
                                break;
                            case 4:
                                status = DVPCamera.dvpWriteGenICamReg(CamHandle, 0x120100c, value);
                                if (status != dvpStatus.DVP_STATUS_OK)
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.CameraSerialNumber
                                            + paramSetting.SerialNumber
                                            + Properties
                                                .Resources
                                                .ExecuteSetChannel4ExposureTimeFail
                                    );
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (Channel == 4)
                        {
                            status = DVPCamera.dvpSetExposure(CamHandle, (double)value);
                            if (status != dvpStatus.DVP_STATUS_OK)
                            {
                                CCameraManagement.CamLogger.Error(
                                    Properties.Resources.CameraSerialNumber
                                        + paramSetting.SerialNumber
                                        + Properties.Resources.ExecuteSetExposureTimeFail
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetExposureTimeError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            try
            {
                value = 0;
                if (this.Connected)
                {
                    DVPCamera.dvpGetAnalogGain(CamHandle, ref value);
                    return true;
                }
                else
                {
                    //CCameraManagement.CamLogger.Error(
                    //Properties.Resources.ErrorGetGain + nRet.ToString()


                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteGetGainError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="Value"></param>
        public override void SetGain(float Value)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetGainIs
                            + Value
                    );

                    dvpStatus status = DVPCamera.dvpSetAnalogGain(CamHandle, (float)Value);
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetGainFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetGainError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value"></param>
        public override void SetTriggerDelay(uint value)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetTriggerDelayIs
                            + value
                    );
                    dvpStatus status = DVPCamera.dvpSetTriggerDelay(CamHandle, (double)value);
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetTriggerDelayFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetTriggerDelayError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 启用IO输出
        /// </summary>
        /// <param name="Enble"></param>
        public override void SetStrobeEnable(bool Enable)
        {
            try
            {
                if (this.Connected)
                {
                    //无
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///选择输出线路
        /// </summary>
        /// <param name="Line"></param>
        public override void SetLineSelector(object Line)
        {
            try
            {
                if (this.Connected)
                {
                    //无
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteLineSelectorError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        ///设置输出持续时间
        /// </summary>
        /// <param name="Value"></param>
        public override void SetStrobeDuration(uint value)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetStrobeDurationValueIs
                            + value
                            + "us"
                    );
                    dvpStatus status = DVPCamera.dvpSetStrobeDuration(CamHandle, (double)value);
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetStrobeDurationValueFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetStrobeDurationValueError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
            paramSetting.TriggerSource = paramSetting.TriggerSource;
        }

        /// <summary>
        ///选择输出触发类型
        /// </summary>
        /// <param name="type"></param>
        public override void SetLineSource(object source)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSelectOutputTriggerTypeIs
                            + Convert.ToString(source)
                    );
                    dvpStatus status = DVPCamera.dvpSetLineSource(
                        CamHandle,
                        (dvpLine)paramSetting.LineSelect,
                        (dvpLineSource)Enum.Parse(typeof(dvpLineSource), Convert.ToString(source))
                    );
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSelectOutputTriggerTypeFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSelectOutputTriggerTypeError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        ///信号反转
        /// </summary>
        public override void SetLineInverter(bool Enable)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteLineInverter
                    );
                    dvpStatus status = DVPCamera.dvpSetLineInverter(
                        CamHandle,
                        (dvpLine)paramSetting.LineSelect,
                        Enable
                    );
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteLineInverterFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteLineInverterError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        ///设置输入输出模式
        /// </summary>
        public override void SetLineMode(object LineMode)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetInputOutputMode
                            + paramSetting.LineSelect.ToString()
                            + Properties.Resources.StatueIs
                            + LineMode.ToString()
                    );
                    dvpStatus status = DVPCamera.dvpSetLineMode(
                        CamHandle,
                        (dvpLine)paramSetting.LineSelect,
                        (dvpLineMode)Enum.Parse(typeof(dvpLineMode), Convert.ToString(LineMode))
                    );
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExecuteSetInputOutputModeFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetInputOutputModeError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 输出触发
        /// </summary>
        public override void LineTriggerSoftware()
        {
            try
            {
                if (this.Connected)
                {
                    //dvpStatus status = DVPCamera.dvpsetline(CamHandle);
                    //if (status != dvpStatus.DVP_STATUS_OK)
                    //{

                    //}
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 设置Gamma值
        /// </summary>
        /// <param name="Value"></param>
        public override void SetGamma(float value)
        {
            try
            {
                if (this.Connected)
                {
                    int gammavalue = (int)(value * 100);
                    if (gammavalue >= 200 || gammavalue <= 0)
                    {
                        return;
                    }
                    CCameraManagement.CamLogger.Info(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExcuteSetGammaValueIs
                            + value
                    );
                    dvpStatus status = DVPCamera.dvpSetGamma(CamHandle, gammavalue);

                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExcuteSetGammaValueFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExcuteSetGammaValueError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 设置Gamma使能
        /// </summary>
        /// <param name="Enable"></param>
        public override void SetGammaEnable(bool Enable)
        {
            try
            {
                if (this.Connected)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExcuteSetGammaEnableIs
                            + Enable.ToString()
                    );
                    dvpStatus status = DVPCamera.dvpSetGammaState(CamHandle, Enable);
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.CameraSerialNumber
                                + paramSetting.SerialNumber
                                + Properties.Resources.ExcuteSetGammaEnableFail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExcuteSetGammaEnableError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 获取实时帧率
        /// </summary>
        /// <returns></returns>
        public override float GetFps()
        {
            try
            {
                float fps = 0;
                if (this.Connected) { }
                return fps;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam() { }

        /// <summary>
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam()
        {
            if (this.Connected)
            {
                dvpStatus status = DVPCamera.dvpSetUserSet(CamHandle, dvpUserSet.USER_SET_1);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExcuteSelectUserSetingPowerOnFail
                    );
                }
                status = DVPCamera.dvpSaveUserSet(CamHandle, dvpUserSet.USER_SET_1);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExcuteSaveUserSetingFail
                    );
                }
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExcuteSelectUserSetingUserSet1PowerOnSucess
                );
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExcuteSaveUserSetingSucess
                );
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            dvpRegion tempRegion;
            tempRegion.X = 0;
            tempRegion.Y = 0;
            tempRegion.W = 0;
            tempRegion.H = 0;
            tempRegion.reserved = new int[32];

            try
            {
                dvpStatus status = DVPCamera.dvpGetRoi(CamHandle, ref tempRegion);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    value = tempRegion.W;
                    return true;
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            dvpRegion tempRegion;
            tempRegion.X = 0;
            tempRegion.Y = 0;
            tempRegion.W = 0;
            tempRegion.H = 0;
            tempRegion.reserved = new int[32];

            try
            {
                dvpStatus status = DVPCamera.dvpGetRoi(CamHandle, ref tempRegion);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    value = tempRegion.H;
                    return true;
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                value = 1000;
                return false;
            }
        }

        /// <summary>
        /// 获取图片类型
        /// </summary>
        public void GetPixelFormat()
        {
            try
            {
                dvpStreamFormat refSourceFormat = dvpStreamFormat.S_RAW8;
                dvpStatus status = DVPCamera.dvpGetTargetFormat(CamHandle, ref refSourceFormat);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    switch (refSourceFormat)
                    {
                        case dvpStreamFormat.S_MONO8:
                            paramSetting.CameraType = PixelFormats.Gray8;
                            break;
                        case dvpStreamFormat.S_RAW8:
                        case dvpStreamFormat.S_RGB24:
                        case dvpStreamFormat.S_BGR24:
                            paramSetting.CameraType = PixelFormats.Rgb24;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.GetPixelFormatError
                        + ex.Message
                );
                throw;
            }
        }

        public bool SetPixelFormat()
        {
            try
            {
                dvpStatus status1 = DVPCamera.dvpSetEnumValueByString(
                    CamHandle,
                    "TargetFormat",
                    "RGB24"
                );
                //dvpStreamFormat refSourceFormat = dvpStreamFormat.S_RAW8;
                //dvpStatus status1 = DVPCamera.dvpSetSourceFormat(CamHandle, refSourceFormat);

                //dvpStreamFormat refTargetFormat = dvpStreamFormat.S_BGR24;
                //dvpStatus status2 = DVPCamera.dvpSetTargetFormat(CamHandle, refTargetFormat);

                //if ((status1 == dvpStatus.DVP_STATUS_OK)&&(status2 == dvpStatus.DVP_STATUS_OK))
                if (status1 == dvpStatus.DVP_STATUS_OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.GetPixelFormatError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 设置一次触发相机拍照次数
        /// </summary>
        public override void SetFrameCount(int count)
        {
            try
            {
                CCameraManagement.CamLogger.Info(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetFrameCount
                        + count.ToString()
                );
                dvpStatus status = DVPCamera.dvpSetFramesPerTrigger(CamHandle, count);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.CameraSerialNumber
                            + paramSetting.SerialNumber
                            + Properties.Resources.ExecuteSetFrameCountFail
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteSetFrameCountError
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 启动时设置参数
        /// </summary>
        private void SetParameters()
        {
            SetTriggerSource(this.paramSetting.TriggerSource);
            //SetTriggerMode(this.paramSetting.TriggerMode);
            SetExposureTime(this.paramSetting.ExposureTime, 4);
            SetGain(paramSetting.Gain);
            //TriggerDelay(this.paramSetting.TriggerDelay);
            SetGamma(paramSetting.Gamma);
            GetPixelFormat();
        }

        /// <summary>
        /// 判读句柄是否有效
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        protected bool IsValidHandle(uint handle)
        {
            bool bValidHandle = false;
            dvpStatus status = DVPCamera.dvpIsValid(handle, ref bValidHandle);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                return bValidHandle;
            }

            return false;
        }

        /// <summary>
        /// 开灯
        /// </summary>
        protected void OpenLight()
        {
            if (paramSetting.CameraType == PixelFormats.Gray8)
                return;
            int[] buffer = new int[2];
            buffer[0] = 0x1100A00;
            buffer[1] = 0xF1;
            IntPtr pParam = Marshal.AllocHGlobal(buffer.Length * 4);
            uint size = (uint)buffer.Length * 4;
            Marshal.Copy(buffer, 0, pParam, buffer.Length);
            var status = DVPCamera.dvpSet(CamHandle, 0x1000, pParam, ref size);
            if (status != dvpStatus.DVP_STATUS_OK)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteOpenLightFail
                );
            }
        }

        /// <summary>
        /// 关灯
        /// </summary>
        private void CloseLight()
        {
            if (paramSetting.CameraType == PixelFormats.Gray8)
                return;
            int[] buffer = new int[2];
            buffer[0] = 0x1100A00;
            buffer[1] = 0xF0;
            IntPtr pParam = Marshal.AllocHGlobal(buffer.Length * 4);
            uint size = (uint)buffer.Length * 4;
            Marshal.Copy(buffer, 0, pParam, buffer.Length);
            var status = DVPCamera.dvpSet(CamHandle, 0x1000, pParam, ref size);
            if (status != dvpStatus.DVP_STATUS_OK)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.CameraSerialNumber
                        + paramSetting.SerialNumber
                        + Properties.Resources.ExecuteCloseLightFail
                );
            }
        }

        public override bool GetCameraType(out PixelFormat cameraType)
        {
            cameraType = PixelFormats.Rgb24;
            return true;
        }

        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            bool refTriggerState = false;
            var status = DVPCamera.dvpGetTriggerState(CamHandle, ref refTriggerState);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                if (!refTriggerState)
                {
                    mode = EMTRIGGERMODE.EMTRIGGERNONE;
                    return true;
                }
                else
                {
                    dvpTriggerSource pTriggerSource = dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
                    status = DVPCamera.dvpGetTriggerSource(CamHandle, ref pTriggerSource);

                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        if (pTriggerSource == dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE)
                        {
                            mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            return true;
                        }
                        else
                        {
                            mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            return true;
                        }
                    }
                    else
                    {
                        mode = EMTRIGGERMODE.EMTRIGGERNONE;
                        return false;
                    }
                }
            }
            else
            {
                mode = EMTRIGGERMODE.EMTRIGGERNONE;
                return false;
            }
        }

        public override bool GetGamma(out float value)
        {
            value = 0f;
            int refGamma = 0;
            var status = DVPCamera.dvpGetGamma(CamHandle, ref refGamma);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                value = refGamma;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool GetTriggerDelay(out uint value)
        {
            value = 0;
            double refTriggerDelay = 0;
            var status = DVPCamera.dvpGetTriggerDelay(CamHandle, ref refTriggerDelay);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                value = (uint)refTriggerDelay;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void SetTriggerPulseWidth(uint value) { }

        public override bool GetTriggerPulseWidth(out uint value)
        {
            value = 0;
            return true;
        }

        public override void SetCustomParam(uint value) { }
    }
}
