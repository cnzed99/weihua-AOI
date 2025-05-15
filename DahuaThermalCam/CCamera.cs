using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using CameraModule;
using HandyControl.Tools.Extension;
using log4net.Core;
using NetSDKCS;
using Newtonsoft.Json.Linq;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace DahuaThermalCam
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

        private fRealDataCallBackEx2 realDataCallBackEx2;

        private fDisConnectCallBack disConnectCallback;

        private fHaveReConnectCallBack haveReConnectCallBack;

        public IntPtr lLoginID = IntPtr.Zero;

        public IntPtr playHandle = IntPtr.Zero;

        private NET_DEVICEINFO_Ex DeviceInfo;

        int nChannel = 0;

        public CCamera()
            : base()
        {
            disConnectCallback = new fDisConnectCallBack(DisConnectCallBack);
            haveReConnectCallBack = new fHaveReConnectCallBack(HaveReConnectCallBack);
            realDataCallBackEx2 = new fRealDataCallBackEx2(RealDataCallBackEx2);
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 初始化打开相机
        /// </summary>
        /// <returns>true 打开成功，false失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                NETClient.Init(disConnectCallback, IntPtr.Zero, null);
                NETClient.SetAutoReconnect(haveReConnectCallBack, IntPtr.Zero);

                lLoginID = NETClient.LoginWithHighLevelSecurity(
                    paramSetting.Ip,
                    paramSetting.Port,
                    paramSetting.User,
                    paramSetting.Password,
                    EM_LOGIN_SPAC_CAP_TYPE.TCP,
                    IntPtr.Zero,
                    ref DeviceInfo
                );
                if (lLoginID != IntPtr.Zero && DeviceInfo.nChanNum > 0)
                {
                    this.Connected = StartGrab();
                    return this.Connected;
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

        bool StartPlay()
        {
            playHandle = NETClient.RealPlay(lLoginID, nChannel, IntPtr.Zero);
            if (playHandle == IntPtr.Zero)
            {
                return false;
            }
            SetPalette();
            SetBrightAndContrast();
            SetSharpness();
            SetDetail();
            return true;
        }

        Stopwatch swShow = Stopwatch.StartNew(); //帧率计时器

        private void RealDataCallBackEx2(
            IntPtr lRealHandle,
            uint dwDataType,
            IntPtr pBuffer,
            uint dwBufSize,
            IntPtr param,
            IntPtr dwUser
        )
        {
            //try
            //{
            //    if (!(paramSetting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE || snapOne))
            //        return;
            //    snapOne = false;
            //    grabCount++;

            //    if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
            //    {
            //        //ImageQueueChannel.Writer.TryWrite(dataFrame.Bmp);
            //    }
            //    else
            //    {
            //        if (swShow.Elapsed.TotalMilliseconds > ((CParameterSetting)Setting).FrameTicks)
            //        {
            //            swShow.Restart();
            //            //ImageQueueChannel.Writer.TryWrite(dataFrame.Bmp);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
            //    );
            //}
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

        private void DisConnectCallBack(
            IntPtr lLoginID,
            IntPtr pchDVRIP,
            int nDVRPort,
            IntPtr dwUser
        ) { }

        private void HaveReConnectCallBack(
            IntPtr lLoginID,
            IntPtr pchDVRIP,
            int nDVRPort,
            IntPtr dwUser
        ) { }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            StopGrab();
            if (lLoginID != IntPtr.Zero)
            {
                NETClient.Logout(lLoginID);
                lLoginID = IntPtr.Zero;
            }
            //NETClient.Cleanup();
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 开始采集
        /// </summary>
        public override bool StartGrab()
        {
            try
            {
                return StartPlay();
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
                if (playHandle == IntPtr.Zero)
                    return false;

                NETClient.StopRealPlay(playHandle);
                playHandle = IntPtr.Zero;

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

        public static System.Windows.Media.PixelFormat ConvertPixelFormat(
            System.Drawing.Imaging.PixelFormat sourceFormat
        )
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return PixelFormats.Gray16;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return PixelFormats.Bgr555;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return PixelFormats.Bgr565;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Indexed8;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return PixelFormats.Indexed1;
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return PixelFormats.Rgb48;
                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    return PixelFormats.Prgba64;
                default:
                    return PixelFormats.Default;
            }
        }

        Bitmap bitmapSave;

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
                    grabCount++;
                    NET_SNAP_PARAMS stu_snap_param = new NET_SNAP_PARAMS()
                    {
                        Channel = (uint)nChannel,
                        Quality = 2,
                        mode = 0
                    };
                    NET_IN_SNAP_PIC_TO_FILE_PARAM inParam = new NET_IN_SNAP_PIC_TO_FILE_PARAM()
                    {
                        dwSize = (uint)Marshal.SizeOf(typeof(NET_IN_SNAP_PIC_TO_FILE_PARAM)),
                        stuParam = stu_snap_param,
                    };
                    NET_OUT_SNAP_PIC_TO_FILE_PARAM outParam = new NET_OUT_SNAP_PIC_TO_FILE_PARAM()
                    {
                        dwSize = (uint)Marshal.SizeOf(typeof(NET_OUT_SNAP_PIC_TO_FILE_PARAM)),
                        dwPicBufLen = 1024000,
                        szPicBuf = Marshal.AllocHGlobal(1024000),
                    };
                    if (NETClient.SnapPictureToFile(lLoginID, ref inParam, ref outParam, 1000))
                    {
                        byte[] bytes = new byte[outParam.dwPicBufRetLen];
                        Marshal.Copy(outParam.szPicBuf, bytes, 0, bytes.Length);
                        using (var ms = new MemoryStream(bytes))
                        {
                            using (var image = System.Drawing.Image.FromStream(ms))
                            {
                                paramSetting.ImageWidth = image.Width;
                                paramSetting.ImageHeight = image.Height;
                                bitmapSave = new Bitmap(image);
                                paramSetting.CameraType = ConvertPixelFormat(
                                    bitmapSave.PixelFormat
                                );
                                var bitmapData = bitmapSave.LockBits(
                                    new Rectangle(0, 0, bitmapSave.Width, bitmapSave.Height),
                                    ImageLockMode.ReadOnly,
                                    bitmapSave.PixelFormat
                                );
                                imageBufferStride = bitmapData.Stride;
                                ImageQueueChannel.Writer.TryWrite(bitmapData.Scan0);
                            }
                        }
                    }
                    Marshal.FreeHGlobal(outParam.szPicBuf);
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

        public void SetPalette()
        {
            object obj = new object();
            NETClient.GetNewDevConfig(
                lLoginID,
                nChannel,
                SDK_NEWDEVCONFIG_CMD.CFG_CMD_THERMO_GRAPHY,
                ref obj,
                typeof(NET_CFG_THERMOGRAPHY_INFO),
                1000
            );
            NET_CFG_THERMOGRAPHY_INFO info = (NET_CFG_THERMOGRAPHY_INFO)obj;
            info.stOptions[0].nColorization = (int)paramSetting.Colorization;
            info.stOptions[1].nColorization = (int)paramSetting.Colorization;
            info.stOptions[2].nColorization = (int)paramSetting.Colorization;
            NETClient.SetNewDevConfig(
                lLoginID,
                0,
                SDK_NEWDEVCONFIG_CMD.CFG_CMD_THERMO_GRAPHY,
                info,
                typeof(NET_CFG_THERMOGRAPHY_INFO),
                1000
            );
        }

        public void SetBrightAndContrast()
        {
            NET_VIDEOIN_COLOR_INFO info = new NET_VIDEOIN_COLOR_INFO();
            info.dwSize = (uint)Marshal.SizeOf(info);
            info.emCfgType = EM_A_NET_EM_CONFIG_TYPE.NET_EM_CONFIG_NORMAL;
            object obj = info;
            NETClient.GetOperateConfig(
                lLoginID,
                EM_CFG_OPERATE_TYPE.VIDEOIN_COLOR,
                nChannel,
                ref obj,
                typeof(NET_VIDEOIN_COLOR_INFO),
                1000
            );
            info = (NET_VIDEOIN_COLOR_INFO)obj;
            info.nBrightness = (int)paramSetting.Brightness;
            info.nContrast = (int)paramSetting.Contrast;
            NETClient.SetOperateConfig(
                lLoginID,
                EM_CFG_OPERATE_TYPE.VIDEOIN_COLOR,
                nChannel,
                info,
                typeof(NET_VIDEOIN_COLOR_INFO),
                1000
            );
        }

        public void SetSharpness()
        {
            NET_VIDEOIN_SHARPNESS_INFO info = new NET_VIDEOIN_SHARPNESS_INFO();
            info.dwSize = (uint)Marshal.SizeOf(info);
            info.emCfgType = EM_A_NET_EM_CONFIG_TYPE.NET_EM_CONFIG_NORMAL;
            object obj = info;
            NETClient.GetOperateConfig(
                lLoginID,
                EM_CFG_OPERATE_TYPE.VIDEOIN_SHARPNESS,
                nChannel,
                ref obj,
                typeof(NET_VIDEOIN_SHARPNESS_INFO),
                1000
            );
            info = (NET_VIDEOIN_SHARPNESS_INFO)obj;
            info.nSharpness = (int)paramSetting.Sharpness;
            info.emSharpnessMode = EM_A_NET_EM_SHARPNESS_MODE.NET_EM_SHARPNESS_MANAUL;
            NETClient.SetOperateConfig(
                lLoginID,
                EM_CFG_OPERATE_TYPE.VIDEOIN_SHARPNESS,
                nChannel,
                info,
                typeof(NET_VIDEOIN_SHARPNESS_INFO),
                1000
            );
        }

        public void SetDetail()
        {
            object obj = new object();
            NETClient.GetNewDevConfig(
                lLoginID,
                nChannel,
                SDK_NEWDEVCONFIG_CMD.CFG_CMD_LCE_STATE,
                ref obj,
                typeof(NET_CFG_LCE_STATE_INFO),
                1000
            );
            NET_CFG_LCE_STATE_INFO info = (NET_CFG_LCE_STATE_INFO)obj;
            info.unLCEValue = paramSetting.DetailEnhancer;
            NETClient.SetNewDevConfig(
                lLoginID,
                nChannel,
                SDK_NEWDEVCONFIG_CMD.CFG_CMD_LCE_STATE,
                info,
                typeof(NET_CFG_LCE_STATE_INFO),
                1000
            );
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            value = 1000;
            return false;
            //{
            //    value = _cameraBasicInfo.DataWidth;
            //    return value > 0;
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetWidth2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    value = 1000;
            //    return false;
            //}
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            value = 1000;
            return false;
            //try
            //{
            //    value = _cameraBasicInfo.DataHeight;
            //    return value > 0;
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetHeight2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    value = 1000;
            //    return false;
            //}
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            cameraType = PixelFormats.Bgra32;
            return false;
            //try
            //{
            //    cameraType = PixelFormats.Bgra32;
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetCamType2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    cameraType = PixelFormats.Bgra32;
            //    return false;
            //}
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
