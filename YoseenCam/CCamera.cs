using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CameraModule;
using HandyControl.Tools.Extension;
using Newtonsoft.Json.Linq;
using YoseenSDKCS;

namespace YoseenCam
{
    /// <summary>
    /// 2024.8.2 李焕彬
    /// 相机操作派生类
    /// </summary>
    public class CCamera : CCameraBase
    {
        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机参数
        /// </summary>
        internal CParameterSetting paramSetting { get; set; }

        public CCamera()
            : base()
        {
            _previewCallback = new YoseenSDKCS.YoseenPreviewCallback(funcFrameRecved);
        }

        int _userHandle = -1;
        int _previewHandle = -1;
        YoseenSDKCS.YoseenLoginInfo _loginInfo;
        YoseenSDKCS.CameraBasicInfo _cameraBasicInfo;
        strech_control ctrl = new strech_control();

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 初始化打开相机
        /// </summary>
        /// <returns>true 打开成功，false失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                int error = YoseenSDKCS.YoseenSDK.Yoseen_InitSDK();
                if (error < 0)
                    throw new Exception(string.Format("Yoseen_InitSDK, ret {0}.", error));

                YoseenSDKCS.DiscoverCameraResp2[] dcrs =
                    YoseenSDKCS.YoseenSDK.Yoseen_DiscoverCameras2(0x01);

                foreach (var cam in dcrs)
                {
                    if (cam.BasicInfo.CameraId == paramSetting.SerialNumber)
                    {
                        _loginInfo.CameraAddr = YoseenSDKCS.YoseenUtil.uint2str(cam.CameraIp);
                        int userHandle = YoseenSDKCS.YoseenSDK.Yoseen_Login(
                            ref _loginInfo,
                            ref _cameraBasicInfo
                        );
                        if (userHandle >= 0)
                        {
                            _userHandle = userHandle;

                            this.Connected = StartGrab();
                            return this.Connected;
                        }
                        break;
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

        YoseenSDKCS.YoseenPreviewInfo _previewInfo;

        YoseenSDKCS.YoseenPreviewCallback _previewCallback;

        bool StartPlay(xxxdatatype dataType)
        {
            if (_previewHandle >= 0)
                return false;

            _previewInfo.DataType = (int)dataType;
            _previewInfo.CustomData = IntPtr.Zero;
            _previewInfo.CustomCallback = _previewCallback;
            _previewInfo.Hwnd = IntPtr.Zero;

            int previewHandle = YoseenSDKCS.YoseenSDK.Yoseen_StartPreview(
                _userHandle,
                ref _previewInfo
            );

            if (previewHandle < 0)
            {
                throw new Exception(string.Format("Yoseen_StartPreview, ret {0}.", previewHandle));
            }
            _previewHandle = previewHandle;
            if (dataType == YoseenSDKCS.xxxdatatype.temp)
            {
                paramSetting.ImageWidth = _cameraBasicInfo.DataWidth;
                paramSetting.ImageHeight = _cameraBasicInfo.DataHeight;
                SetImage();
            }
            else
            {
                paramSetting.ImageWidth = _cameraBasicInfo.VideoWidth;
                paramSetting.ImageHeight = _cameraBasicInfo.VideoHeight;
            }

            paramSetting.CameraType = PixelFormats.Bgra32;
            imageBufferStride = paramSetting.ImageWidth * 4;
            return true;
        }

        Stopwatch swShow = Stopwatch.StartNew(); //帧率计时器

        void funcFrameRecved(int errorCode, ref YoseenSDKCS.DataFrame dataFrame, IntPtr customData)
        {
            try
            {
                if (!(paramSetting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE || snapOne))
                    return;
                snapOne = false;
                grabCount++;

                if (0 == errorCode)
                {
                    if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
                    {
                        ImageQueueChannel.Writer.TryWrite(dataFrame.Bmp);
                    }
                    else
                    {
                        if (
                            swShow.Elapsed.TotalMilliseconds
                            > ((CParameterSetting)Setting).FrameTicks
                        )
                        {
                            swShow.Restart();
                            ImageQueueChannel.Writer.TryWrite(dataFrame.Bmp);
                        }
                    }
                }
                else
                {
                    throw new Exception(string.Format("funcFrameRecved, ret {0}.", errorCode));
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
            StopGrab();
            int ret = YoseenSDKCS.YoseenSDK.Yoseen_Logout(_userHandle);
            _userHandle = -1;
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 开始采集
        /// </summary>
        public override bool StartGrab()
        {
            try
            {
                return StartPlay(paramSetting.DataType);
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
                if (_previewHandle < 0)
                    return false;

                int ret = YoseenSDKCS.YoseenSDK.Yoseen_StopPreview(_previewHandle);
                _previewHandle = -1;
                if (ret < 0)
                {
                    throw new Exception(string.Format("Yoseen_StopPreview, ret {0}", ret));
                }

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

        bool snapOne = false;

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
                    snapOne = true;
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
            paramSetting.DataType = paramSetting.DataType;
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
                value = _cameraBasicInfo.DataWidth;
                return value > 0;
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
                value = _cameraBasicInfo.DataHeight;
                return value > 0;
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
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            try
            {
                cameraType = PixelFormats.Bgra32;
                return true;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetCamType2 + paramSetting.SerialNumber + ex.Message
                );
                cameraType = PixelFormats.Bgra32;
                return false;
            }
        }

        EMTRIGGERMODE modeSet;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 修改相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            modeSet = mode;
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            mode = modeSet;
            return true;
            //try
            //{
            //    MVCC_ENUMVALUE enumValue = new();
            //    int nRet = m_MyCamera.MV_CC_GetEnumValue_NET("TriggerMode", ref enumValue);
            //    if (nRet == MV_OK)
            //    {
            //        if (enumValue.nCurValue == (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF)
            //        {
            //            mode = EMTRIGGERMODE.EMTRIGGERNONE;
            //            return true;
            //        }
            //        else
            //        {
            //            nRet = m_MyCamera.MV_CC_GetEnumValue_NET("TriggerSource", ref enumValue);
            //            if (nRet == MV_OK)
            //            {
            //                if (
            //                    enumValue.nCurValue
            //                    == (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE
            //                )
            //                {
            //                    mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
            //                }
            //                else
            //                {
            //                    mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
            //                }
            //                return true;
            //            }
            //        }
            //    }
            //    mode = EMTRIGGERMODE.EMTRIGGERNONE;
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerMode + nRet.ToString()
            //    );
            //    return false;
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerMode2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
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
            return true;
            //try
            //{
            //    MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
            //    int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref pcFloatValue);
            //    if (MV_OK == nRet)
            //    {
            //        value = (uint)pcFloatValue.fCurValue;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetExposureTime + nRet.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetExposureTime2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            //try
            //{
            //    int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", (float)value);
            //    if (MV_OK != nRet)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetExposureTime + nRet.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetExposureTime2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            value = 0;
            return true;
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value)
        {
            //try
            //{
            //    int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", value);
            //    if (MV_OK != nRet)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetGain + nRet.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetGain2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            value = 0;
            return true;
            //try
            //{
            //    MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
            //    int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("Gamma", ref pcFloatValue);
            //    if (MV_OK == nRet)
            //    {
            //        value = pcFloatValue.fCurValue;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetGamma + nRet.ToString()
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
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            //try
            //{
            //    int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gamma", value);
            //    if (MV_OK != nRet)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetGamma + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value)
        {
            //try
            //{
            //    int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("TriggerDelay", value);
            //    if (MV_OK != nRet)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetTriggerDelay + nRet.ToString()
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
        /// 2024.8.2 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            value = 0;
            return true;
            //try
            //{
            //    MVCC_FLOATVALUE pcFloatValue = new MVCC_FLOATVALUE();
            //    int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("TriggerDelay", ref pcFloatValue);
            //    if (MV_OK == nRet)
            //    {
            //        value = (uint)pcFloatValue.fCurValue;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetTriggerDelay + nRet.ToString()
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
        public override void UserSaveParam() { }

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
        public override void SetCustomParam(uint value) { }

        public bool SetPriviewType(xxxdatatype dataType)
        {
            if (dataType != paramSetting.DataType)
            {
                StopGrab();
                Thread.Sleep(500);
                StartPlay(dataType);
            }
            return true;
        }

        public bool SetImage()
        {
            ctrl.change_type = StrechControlFlags.SCF_All;
            ctrl.strech_type = paramSetting.Strech_type;
            ctrl.dde_level = paramSetting.Dde_level;
            ctrl.man_enable = (byte)(paramSetting.EnableTempLimit == true ? 1 : 0);
            ctrl.man_temp0 = (short)(paramSetting.TempLower * 10);
            ctrl.man_temp1 = (short)(paramSetting.TempHigher * 10);
            ctrl.ct_type = paramSetting.Ct_type;
            ctrl.ct_temp0 = (short)(paramSetting.Ct_temp0 * 10);
            ctrl.ct_temp1 = (short)(paramSetting.Ct_temp1 * 10);
            ctrl.ct_color0 = paramSetting.Ct_color0.Color.ToInt32();
            ctrl.ct_color1 = paramSetting.Ct_color1.Color.ToInt32();
            ctrl.gain = paramSetting.GainSpecial;
            int res = YoseenSDK.Yoseen_PreviewSetImage(
                _previewHandle,
                ref ctrl,
                (int)paramSetting.Palette
            );
            if (res != 0)
            {
                throw new Exception(string.Format("Yoseen_PreviewSetImage, ret {0}.", res));
            }
            return true;
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
