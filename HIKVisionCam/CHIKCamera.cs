using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;
using System.Windows.Threading;
using CameraModule;
using MvCamCtrl.NET;
using Newtonsoft.Json.Linq;
using WH.Entity.CommonLib;
using static MvCamCtrl.NET.MyCamera;

namespace HIKVisionCam
{
    /// <summary>
    /// 2024.8.2 李焕彬
    /// 相机操作派生类
    /// </summary>
    public class CHIKCamera : CCameraBase
    {
        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机参数
        /// </summary>
        internal CHIKParameterSetting paramSetting { get; set; }

        public CHIKCamera()
            : base() { }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机对象
        /// </summary>
        private MyCamera m_MyCamera = new MyCamera();

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机回调
        /// </summary>
        private cbOutputExdelegate m_ImageCallback;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 初始化打开相机
        /// </summary>
        /// <returns>true 打开成功，false失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                MV_CC_DEVICE_INFO_LIST stDeviceList = new MV_CC_DEVICE_INFO_LIST();
                int nRet = MV_CC_EnumDevices_NET(MV_GIGE_DEVICE | MV_USB_DEVICE, ref stDeviceList);
                if (MV_OK != nRet)
                {
                    throw new Exception(Properties.Resources.ErrorEnumCam1);
                }
                //遍历所有相机
                for (int i = 0; i < stDeviceList.nDeviceNum; i++)
                {
                    MV_CC_DEVICE_INFO device = (MV_CC_DEVICE_INFO)
                        Marshal.PtrToStructure(
                            stDeviceList.pDeviceInfo[i],
                            typeof(MV_CC_DEVICE_INFO)
                        );
                    if (device.nTLayerType == MV_GIGE_DEVICE) //网口相机
                    {
                        MV_GIGE_DEVICE_INFO gigeInfo = (MV_GIGE_DEVICE_INFO)ByteToStruct(
                            device.SpecialInfo.stGigEInfo,
                            typeof(MV_GIGE_DEVICE_INFO)
                        );
                        if (gigeInfo.chSerialNumber == paramSetting.SerialNumber)
                            return Init(device);
                    }
                    else if (device.nTLayerType == MV_USB_DEVICE) //USB相机
                    {
                        MV_USB3_DEVICE_INFO usbInfo = (MV_USB3_DEVICE_INFO)ByteToStruct(
                            device.SpecialInfo.stUsb3VInfo,
                            typeof(MV_USB3_DEVICE_INFO)
                        );
                        if (paramSetting.SerialNumber != null)
                        {
                            if (usbInfo.chSerialNumber == paramSetting.SerialNumber)
                                return Init(device);
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
        /// 2024.8.2 李焕彬
        /// 初始化打开相机
        /// </summary>
        /// <param name="device">设备对象</param>
        /// <returns>true 打开成功，false失败</returns>
        private bool Init(MV_CC_DEVICE_INFO device)
        {
            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MV_OK != nRet)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                );
                return false;
            }

            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
            if (MV_OK != nRet)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                );
                m_MyCamera.MV_CC_DestroyDevice_NET();
                return false;
            }

            // 注册回调函数
            m_ImageCallback = new cbOutputExdelegate(ImageCallbackFunc);
            nRet = m_MyCamera.MV_CC_RegisterImageCallBackEx_NET(m_ImageCallback, IntPtr.Zero);
            if (MV_OK != nRet)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                );
            }
            this.Connected = true;
            return StartGrab();
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 回调函数
        /// </summary>
        /// <param name="pData">图像数据</param>
        /// <param name="pFrameInfo">图像信息</param>
        /// <param name="pUser">上下文信息</param>
        private void ImageCallbackFunc(
            IntPtr pData,
            ref MV_FRAME_OUT_INFO_EX pFrameInfo,
            IntPtr pUser
        )
        {
            try
            {
                startGrabSoft = true;
                grabCount++;
                StringBuilder textBuilder = new StringBuilder("hik");
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());

                //mutex.WaitOne();
                //imageQueue.Enqueue(pData);
                //mutex.ReleaseMutex();
                ImageQueueChannel.Writer.TryWrite(pData);
                paramSetting.ImageWidth = pFrameInfo.nWidth;
                paramSetting.ImageHeight = pFrameInfo.nHeight;
                paramSetting.CameraType =
                    pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_Mono8
                        ? EMCAMERATYPE.EMCAMTYPEGRAY
                        : EMCAMERATYPE.EMCAMTYPECOLOR;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
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
        /// 2024.8.2 李焕彬
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            m_MyCamera.MV_CC_StopGrabbing_NET();
            m_MyCamera.MV_CC_ClearImageBuffer_NET();
            m_MyCamera.MV_CC_CloseDevice_NET();
            m_MyCamera.MV_CC_DestroyDevice_NET();
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 开始采集
        /// </summary>
        public override bool StartGrab()
        {
            try
            {
                bool result = false;

                if (this.Connected)
                {
                    int nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
                    if (MV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStart + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 停止采集
        /// </summary>
        public override bool StopGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
                    if (MV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStop + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            try
            {
                if (this.Connected)
                {
                    base.ExecuteSoftwareTrigger();
                    int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                    if (MV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorSoftWare + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
            paramSetting.TriggerSource = paramSetting.TriggerSource;
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            try
            {
                MVCC_INTVALUE intValue = new MVCC_INTVALUE();
                int nRet = m_MyCamera.MV_CC_GetIntValue_NET("Width", ref intValue);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetWidth + nRet.ToString()
                    );
                    value = 1000;
                    return false;
                }
                else
                {
                    value = (int)intValue.nCurValue;
                    return true;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetWidth2 + paramSetting.SerialNumber + ex.Message
                );
                value = 1000;
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
            try
            {
                MVCC_INTVALUE intValue = new MVCC_INTVALUE();
                int nRet = m_MyCamera.MV_CC_GetIntValue_NET("Height", ref intValue);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetHeight + nRet.ToString()
                    );
                    value = 1000;
                    return false;
                }
                else
                {
                    value = (int)intValue.nCurValue;
                    return true;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetHeight2 + paramSetting.SerialNumber + ex.Message
                );
                value = 1000;
                return false;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out EMCAMERATYPE cameraType)
        {
            try
            {
                MVCC_ENUMVALUE enumValue = new();
                int nRet = m_MyCamera.MV_CC_GetEnumValue_NET("PixelFormat", ref enumValue);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetCamType + nRet.ToString()
                    );
                    cameraType = EMCAMERATYPE.EMCAMTYPEGRAY;
                    return false;
                }
                else
                {
                    cameraType =
                        enumValue.nCurValue == 0x01080001
                            ? EMCAMERATYPE.EMCAMTYPEGRAY
                            : EMCAMERATYPE.EMCAMTYPECOLOR;
                    return true;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetCamType2 + paramSetting.SerialNumber + ex.Message
                );
                cameraType = EMCAMERATYPE.EMCAMTYPEGRAY;
                return false;
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 修改相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            try
            {
                int nRet = MV_OK;
                switch (mode)
                {
                    case EMTRIGGERMODE.EMTRIGGERNONE:
                        nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                            "TriggerMode",
                            (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF
                        );
                        break;
                    case EMTRIGGERMODE.EMTRIGGERSOFTWARE:
                        nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                            "TriggerMode",
                            (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON
                        );
                        if (nRet == MV_OK)
                        {
                            nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                                "TriggerSource",
                                (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE
                            );
                        }
                        break;
                    case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                        nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                            "TriggerMode",
                            (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON
                        );
                        if (nRet == MV_OK)
                        {
                            nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                                "TriggerSource",
                                (uint)paramSetting.TriggerSource
                            );
                        }
                        break;
                }
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerMode + nRet.ToString()
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

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            try
            {
                MVCC_ENUMVALUE enumValue = new();
                int nRet = m_MyCamera.MV_CC_GetEnumValue_NET("TriggerMode", ref enumValue);
                if (nRet == MV_OK)
                {
                    if (enumValue.nCurValue == (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF)
                    {
                        mode = EMTRIGGERMODE.EMTRIGGERNONE;
                        return true;
                    }
                    else
                    {
                        nRet = m_MyCamera.MV_CC_GetEnumValue_NET("TriggerSource", ref enumValue);
                        if (nRet == MV_OK)
                        {
                            if (
                                enumValue.nCurValue
                                == (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE
                            )
                            {
                                mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            }
                            else
                            {
                                mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            }
                            return true;
                        }
                    }
                }
                mode = EMTRIGGERMODE.EMTRIGGERNONE;
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerMode + nRet.ToString()
                );
                return false;
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
        /// 2024.8.2 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            try
            {
                MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
                int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref pcFloatValue);
                if (MV_OK == nRet)
                {
                    value = (uint)pcFloatValue.fCurValue;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetExposureTime + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            try
            {
                int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", (float)value);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetExposureTime + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            try
            {
                MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
                int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("Gain", ref pcFloatValue);
                if (MV_OK == nRet)
                {
                    value = pcFloatValue.fCurValue;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value)
        {
            try
            {
                int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", value);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGain + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            try
            {
                MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
                int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("Gamma", ref pcFloatValue);
                if (MV_OK == nRet)
                {
                    value = pcFloatValue.fCurValue;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGamma + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            try
            {
                int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gamma", value);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGamma + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value)
        {
            try
            {
                int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("TriggerDelay", value);
                if (MV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerDelay + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            try
            {
                MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
                int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("TriggerDelay", ref pcFloatValue);
                if (MV_OK == nRet)
                {
                    value = (uint)pcFloatValue.fCurValue;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerDelay + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value) { }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerPulseWidth(out uint value)
        {
            value = 0;
            return true;
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam()
        {
            try
            {
                if (this.Connected)
                {
                    int nRet = m_MyCamera.MV_CC_SetEnumValue_NET("UserSetDefault", 1);
                    nRet = m_MyCamera.MV_CC_SetEnumValue_NET("UserSetSelector", 1);
                    Thread.Sleep(50);
                    nRet = m_MyCamera.MV_CC_SetCommandValue_NET("UserSetSave");
                    if (MV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorUserSaveParam + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam() { }

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 设置自定义参数
        /// </summary>
        /// <param name="value">自定义参数类别 看EMCUSTOMPARAMTYPE</param>
        public override void SetCustomParam(uint value)
        {
            switch ((EMCUSTOMPARAMTYPE)value)
            {
                case EMCUSTOMPARAMTYPE.EMPARAMTRIGGERSOURCE:
                    {
                        if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERHARDWARE)
                        {
                            try
                            {
                                int nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                                    "TriggerSource",
                                    (uint)paramSetting.TriggerSource
                                );
                                if (MV_OK != nRet)
                                {
                                    CCameraManagement.CamLogger.Error(
                                        Properties.Resources.ErrorSetTriggerSource + nRet.ToString()
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                CCameraManagement.CamLogger.Error(
                                    Properties.Resources.ErrorSetTriggerSource2
                                        + paramSetting.SerialNumber
                                        + ex.Message
                                );
                            }
                        }
                    }
                    break;
            }
        }

        public override void SetStrobeEnable(bool enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSelector(object line)
        {
            throw new NotImplementedException();
        }

        public override void SetStrobeDuration(uint value)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSource(object source)
        {
            throw new NotImplementedException();
        }

        public override void SetLineInverter(bool enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineMode(object lineMode)
        {
            throw new NotImplementedException();
        }

        public override void LineTriggerSoftware()
        {
            throw new NotImplementedException();
        }

        public override void SetGammaEnable(bool enable)
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
