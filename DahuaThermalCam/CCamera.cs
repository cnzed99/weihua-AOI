using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using CameraModule;
using NetSDKCS;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace DahuaThermalCam
{
    /// <summary>
    /// 2025.5.13 李焕彬
    /// 相机操作派生类
    /// </summary>
    public class CCamera : CCameraBase
    {
        /// <summary>
        /// 2025.5.13 李焕彬
        /// 相机参数
        /// </summary>
        internal CParameterSetting paramSetting { get; set; }

        private fDisConnectCallBack disConnectCallback;

        private fHaveReConnectCallBack haveReConnectCallBack;

        public IntPtr lLoginID = IntPtr.Zero;

        public IntPtr playHandle = IntPtr.Zero;

        private NET_DEVICEINFO_Ex DeviceInfo;

        const uint c_bufSize = 5024000; //未解压温度数据存放内存大小

        IntPtr DataBuff = IntPtr.Zero; //未解压温度数据

        IntPtr TempData = IntPtr.Zero; //解压后温度数据

        int nChannel = 0;

        public CCamera()
            : base()
        {
            disConnectCallback = new fDisConnectCallBack(DisConnectCallBack);
            haveReConnectCallBack = new fHaveReConnectCallBack(HaveReConnectCallBack);
        }

        /// <summary>
        /// 2025.5.13 李焕彬
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
            return true;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
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
        /// 2025.5.13 李焕彬
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
            if (DataBuff != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(DataBuff);
                DataBuff = IntPtr.Zero;
            }
            if (TempData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(TempData);
                TempData = IntPtr.Zero;
            }
            //NETClient.Cleanup();
        }

        /// <summary>
        /// 2025.5.13 李焕彬
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
        /// 2025.5.13 李焕彬
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
        /// 2025.5.13 李焕彬
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

                    NET_IN_GET_HEATMAPS_INFO pInParam = new NET_IN_GET_HEATMAPS_INFO();
                    pInParam.nChannel = 0;
                    pInParam.dwSize = (uint)Marshal.SizeOf(typeof(NET_IN_GET_HEATMAPS_INFO));
                    NET_OUT_GET_HEATMAPS_INFO pOutParam = new NET_OUT_GET_HEATMAPS_INFO();
                    pOutParam.dwSize = (uint)Marshal.SizeOf(typeof(NET_OUT_GET_HEATMAPS_INFO));
                    pOutParam.dwMaxDataBufLen = c_bufSize;
                    if (DataBuff == IntPtr.Zero)
                    {
                        DataBuff = Marshal.AllocHGlobal((int)c_bufSize);
                    }
                    pOutParam.pbDataBuf = DataBuff;
                    if (NETClient.GetHeatMapsDirectly(lLoginID, ref pInParam, ref pOutParam, 1000))
                    {
                        int size = pOutParam.stMetaData.nHeight * pOutParam.stMetaData.nWidth;
                        if (TempData == IntPtr.Zero)
                        {
                            TempData = Marshal.AllocHGlobal(size * sizeof(float));
                        }
                        NET_RADIOMETRY_DATA data = new NET_RADIOMETRY_DATA()
                        {
                            stMetaData = pOutParam.stMetaData,
                            pbDataBuf = pOutParam.pbDataBuf,
                            dwBufSize = pOutParam.dwRetDataBufLen
                        };
                        NETClient.RadiometryDataParse(ref data, IntPtr.Zero, TempData);
                        float[] temps = new float[size];
                        Marshal.Copy(TempData, temps, 0, size);
                        bitmapSave = TemperatureColorMap.CreateTemperatureImageOptimized(
                            temps,
                            pOutParam.stMetaData.nWidth,
                            pOutParam.stMetaData.nHeight,
                            paramSetting.EnableTempLimit ? paramSetting.TempLower : -20,
                            paramSetting.EnableTempLimit ? paramSetting.TempHigher : 150,
                            paramSetting.Palette
                        );
                        paramSetting.ImageWidth = bitmapSave.Width;
                        paramSetting.ImageHeight = bitmapSave.Height;
                        paramSetting.CameraType = PixelFormats.Bgra32;
                        // 锁定位图数据
                        BitmapData bitmapData = bitmapSave.LockBits(
                            new Rectangle(0, 0, bitmapSave.Width, bitmapSave.Height),
                            ImageLockMode.WriteOnly,
                            bitmapSave.PixelFormat
                        );
                        imageBufferStride = bitmapData.Stride;
                        ImageQueueChannel.Writer.TryWrite(bitmapData.Scan0);
                    }
                    else
                    {
                        throw new Exception("GetHeatMapsDirectly fail!");
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
        /// 2025.5.13 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            value = 1000;
            return false;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            value = 1000;
            return false;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            cameraType = PixelFormats.Bgra32;
            return false;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 修改相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        protected override void SetTriggerMode(EMTRIGGERMODE mode) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
            return true;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            value = 0;
            return true;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value) { }

        /// <summary>
        /// 2025.5.13 李焕彬
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
        /// 2025.5.13 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            value = 0;
            return true;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            value = 0;
            return true;
        }

        /// <summary>
        /// 2025.5.13 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value) { }

        /// <summary>
        /// 2025.5.13 李焕彬
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
        /// 2025.5.13 李焕彬
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam() { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam() { }

        /// <summary>
        /// 2025.5.13 李焕彬
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
