using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CameraModule;
using MVSDK;
using Newtonsoft.Json.Linq;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;

namespace MindVisionCam
{
    /// <summary>
    /// 李焕彬 2024.7.24
    /// 相机操作派生类
    /// </summary>
    public class CMindVSCamera : CCameraBase
    {
        /// <summary>
        /// 李焕彬 2024.7.24
        /// 相机参数
        /// </summary>
        internal CMindVSParameterSetting paramSetting { get; set; }

        public CMindVSCamera()
            : base() { }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 迈德威视相机对象
        /// </summary>
        private CameraHandle m_hCamera = 0;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 相机特性描述
        /// </summary>
        private tSdkCameraCapbility tCameraCapability;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 相机回调
        /// </summary>
        private CAMERA_SNAP_PROC m_CaptureCallback;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 图像回调函数的上下文参数
        /// </summary>
        private IntPtr m_iCaptureCallbackCtx;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 打开相机
        /// </summary>
        /// <returns>true打开成功, false打开失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraEnumerateDevice(
                    out tSdkCameraDevInfo[] tCameraDevInfoList
                );
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    throw new Exception(Properties.Resources.ErrorEnumCam1);
                }
                //遍历所有相机
                for (int i = 0; i < tCameraDevInfoList.Length; i++)
                {
                    if (
                        Encoding.ASCII.GetString(tCameraDevInfoList[i].acSn).Replace("\0", "")
                        == paramSetting.SerialNumber
                    )
                    {
                        status = MvApi.CameraInit(ref tCameraDevInfoList[i], -1, -1, ref m_hCamera);
                        if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                        {
                            MvApi.CameraGetCapability(m_hCamera, out tCameraCapability);
                            uint trigCapabilityMask = 0;
                            MvApi.CameraGetExtTrigCapability(m_hCamera, ref trigCapabilityMask);
                            CAMERA_SNAP_PROC pCaptureCallOld = null;
                            m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
                            MvApi.CameraSetCallbackFunction(
                                m_hCamera,
                                m_CaptureCallback,
                                m_iCaptureCallbackCtx,
                                ref pCaptureCallOld
                            );
                            this.Connected = true;
                            StartGrab();
                            return true;
                        }
                        else
                        {
                            CCameraManagement.CamLogger.Error(
                                Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                            );
                            return false;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 回调函数
        /// </summary>
        /// <param name="hCamera">相机对象</param>
        /// <param name="pFrameBuffer">图像数据</param>
        /// <param name="pFrameHead">图像信息</param>
        /// <param name="pContext">上下文信息</param>
        public void ImageCaptureCallback(
            CameraHandle hCamera,
            IntPtr pFrameBuffer,
            ref tSdkFrameHead pFrameHead,
            IntPtr pContext
        )
        {
            try
            {
                startGrabSoft = true;
                grabCount++;
                StringBuilder textBuilder = new StringBuilder(Properties.Resources.InfoReceImage);
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());

                ImageQueueChannel.Writer.TryWrite(pFrameBuffer);

                paramSetting.ImageWidth = pFrameHead.iWidth;
                paramSetting.ImageHeight = pFrameHead.iHeight;
                paramSetting.CameraType =
                    pFrameHead.uiMediaType == (uint)emImageFormat.CAMERA_MEDIA_TYPE_MONO8
                        ? PixelFormats.Gray8
                        : PixelFormats.Rgb24;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
            }
        }

        /// <summary>
        /// 李焕彬 2024.7.24
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
        /// 李焕彬 2024.7.24
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            MvApi.CameraUnInit(m_hCamera);
        }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 开始采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StartGrab()
        {
            try
            {
                bool result = false;

                if (this.Connected)
                {
                    CameraSdkStatus status = MvApi.CameraPlay(m_hCamera);
                    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStart + status.ToString()
                        );
                    }
                    else
                    {
                        result = true;
                    }
                }
                return result;
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
        /// 李焕彬 2024.7.24
        /// 停止采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StopGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    startGrabSoft = false;
                    CameraSdkStatus status = MvApi.CameraPause(m_hCamera);
                    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStop + status.ToString()
                        );
                    }
                    else
                    {
                        result = true;
                    }
                }

                return result;
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
        /// 李焕彬 2024.7.24
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            try
            {
                if (this.Connected)
                {
                    base.ExecuteSoftwareTrigger();
                    CameraSdkStatus status = MvApi.CameraSoftTriggerEx(m_hCamera, 1);
                    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorSoftWare + status.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSoftWare2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            value = tCameraCapability.sResolutionRange.iWidthMax;
            return true;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            value = tCameraCapability.sResolutionRange.iHeightMax;
            return true;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            cameraType =
                Marshal.ReadInt32(tCameraCapability.pMediaTypeDesc) == 0
                    ? PixelFormats.Gray8
                    : PixelFormats.Rgb24;
            return true;
        }

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 设置触发源
        /// </summary>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            try
            {
                CameraSdkStatus status;
                switch (mode)
                {
                    case EMTRIGGERMODE.EMTRIGGERNONE:
                        status = MvApi.CameraSetTriggerMode(m_hCamera, 0);
                        break;
                    case EMTRIGGERMODE.EMTRIGGERSOFTWARE:
                        status = MvApi.CameraSetTriggerMode(m_hCamera, 1);
                        break;
                    case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                        status = MvApi.CameraSetTriggerMode(m_hCamera, 2);
                        break;
                    default:
                        status = MvApi.CameraSetTriggerMode(m_hCamera, 0);
                        break;
                }
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerMode + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetTriggerMode2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            try
            {
                int getValue = 0;
                CameraSdkStatus status = MvApi.CameraGetTriggerMode(m_hCamera, ref getValue);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    switch (getValue)
                    {
                        case 0:
                            mode = EMTRIGGERMODE.EMTRIGGERNONE;
                            break;
                        case 1:
                            mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            break;
                        case 2:
                            mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            break;
                        default:
                            mode = EMTRIGGERMODE.EMTRIGGERNONE;
                            break;
                    }
                    return true;
                }
                else
                {
                    mode = EMTRIGGERMODE.EMTRIGGERNONE;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerMode + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerMode2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            try
            {
                double dCameraExpTime = 0;
                CameraSdkStatus status = MvApi.CameraGetExposureTime(m_hCamera, ref dCameraExpTime);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    value = (uint)dCameraExpTime;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetExposureTime + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetExposureTime2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraSetExposureTime(m_hCamera, value);
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetExposureTime + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetExposureTime2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            try
            {
                int gain = 0;
                CameraSdkStatus status = MvApi.CameraGetAnalogGain(m_hCamera, ref gain);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    value = gain / 8;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGain2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value)
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraSetAnalogGain(m_hCamera, (int)value * 8);
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGain + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetGain2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            try
            {
                int gamma = 0;
                CameraSdkStatus status = MvApi.CameraGetGamma(m_hCamera, ref gamma);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    value = gamma / 100;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGamma + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGamma2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraSetGamma(m_hCamera, (int)value * 100);
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGamma + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetGamma2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value)
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraSetStrobeDelayTime(m_hCamera, value);
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerDelay + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetTriggerDelay2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            try
            {
                uint delayTime = 0;
                CameraSdkStatus status = MvApi.CameraGetStrobeDelayTime(m_hCamera, ref delayTime);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    value = delayTime;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerDelay + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerDelay2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value)
        {
            try
            {
                CameraSdkStatus status = MvApi.CameraSetStrobePulseWidth(m_hCamera, value);
                if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerPulseWidth + status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetTriggerPulseWidth2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerPulseWidth(out uint value)
        {
            try
            {
                uint delayTime = 0;
                CameraSdkStatus status = MvApi.CameraGetStrobePulseWidth(m_hCamera, ref delayTime);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    value = delayTime;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerPulseWidth + status.ToString()
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerPulseWidth2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam()
        {
            try
            {
                if (this.Connected)
                {
                    CameraSdkStatus status = MvApi.CameraSaveParameter(
                        m_hCamera,
                        (int)emSdkParameterTeam.PARAMETER_TEAM_A
                    );
                    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorUserSaveParam + status.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorUserSaveParam2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam()
        {
            try
            {
                if (this.Connected)
                {
                    CameraSdkStatus status = MvApi.CameraLoadParameter(
                        m_hCamera,
                        (int)emSdkParameterTeam.PARAMETER_TEAM_A
                    );
                    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorUserLoadParam + status.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorUserLoadParam2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

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
