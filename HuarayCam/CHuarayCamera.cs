using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Media;
using CameraModule;
using MVSDK_Net;
using WH.RecipeCellRootBase;

namespace HuarayCam;

public sealed class CHuarayCamera : CCameraBase
{
    private readonly MyCamera camera;
    private readonly BlockingCollection<FramePacket> frames = new(100);
    private readonly object conversionLock = new();
    private IMVDefine.IMV_FrameCallBack? frameCallback;
    private bool handleCreated;
    private bool cameraOpen;
    private bool grabbing;
    private volatile bool acceptingFrames;

    private CHuarayParameterSetting Parameters => (CHuarayParameterSetting)Setting;

    private readonly record struct FramePacket(IntPtr Data, int Width, int Height, PixelFormat Format);

    public CHuarayCamera()
    {
        HuarayRuntime.EnsureAvailable();
        camera = new MyCamera();
    }

    internal static bool IsSupportedDevice(IMVDefine.IMV_DeviceInfo device)
    {
        bool supportedInterface =
            device.nCameraType == IMVDefine.IMV_ECameraType.typeGigeCamera
            || device.nCameraType == IMVDefine.IMV_ECameraType.typeU3vCamera;
        return supportedInterface
            && (device.manufactureInfo == "Huaray Technology"
                || device.manufactureInfo == "Machine Vision");
    }

    private static string GetGigEIp(IMVDefine.IMV_DeviceInfo device)
    {
        byte[]? bytes = device.deviceSpecificInfo.gigeDeviceInfo;
        if (bytes == null || bytes.Length < Marshal.SizeOf<IMVDefine.IMV_GigEDeviceInfo>())
        {
            return "未知";
        }

        var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<IMVDefine.IMV_GigEDeviceInfo>(
                pinned.AddrOfPinnedObject()
            ).ipAddress;
        }
        finally
        {
            pinned.Free();
        }
    }

    public override bool OpenCamera()
    {
        try
        {
            HuarayRuntime.EnsureAvailable();
            var devices = new IMVDefine.IMV_DeviceList();
            int status = MyCamera.IMV_EnumDevices(
                ref devices,
                (uint)IMVDefine.IMV_EInterfaceType.interfaceTypeAll
            );
            EnsureSuccess(status, "枚举相机");

            int deviceSize = Marshal.SizeOf<IMVDefine.IMV_DeviceInfo>();
            for (int index = 0; index < devices.nDevNum; index++)
            {
                var device = Marshal.PtrToStructure<IMVDefine.IMV_DeviceInfo>(
                    IntPtr.Add(devices.pDevInfo, index * deviceSize)
                );
                if (!IsSupportedDevice(device) || device.serialNumber != Setting.SerialNumber)
                {
                    continue;
                }

                EnsureSuccess(
                    camera.IMV_CreateHandle(IMVDefine.IMV_ECreateHandleMode.modeByIndex, index),
                    "创建相机句柄"
                );
                handleCreated = true;
                int openStatus = camera.IMV_Open();
                if (openStatus == IMVDefine.IMV_INVALID_IP)
                {
                    throw new InvalidOperationException(
                        $"相机 IP {GetGigEIp(device)} 与连接网卡 {device.interfaceName} 不在同一网段（SDK -107）。请在 MV Viewer 调整相机 IP 或网卡地址"
                    );
                }
                EnsureSuccess(openStatus, "打开相机");
                cameraOpen = true;

                frameCallback = OnFrame;
                EnsureSuccess(camera.IMV_AttachGrabbing(frameCallback, IntPtr.Zero), "注册图像回调");
                ConfigureTrigger(Setting.TriggerMode);

                if (!StartGrab())
                {
                    throw new InvalidOperationException("启动相机采集失败");
                }

                Connected = true;
                return true;
            }

            OpenFailureReason = $"未找到华睿相机 {Setting.SerialNumber}";
            CCameraManagement.CamLogger.Error(OpenFailureReason);
        }
        catch (Exception ex)
        {
            OpenFailureReason = ex.Message;
            CCameraManagement.CamLogger.Error($"打开华睿相机 {Setting.SerialNumber} 失败: {ex}");
        }

        CloseCamera();
        return false;
    }

    public override void CloseCamera()
    {
        acceptingFrames = false;
        if (grabbing)
        {
            StopGrab();
        }
        if (cameraOpen)
        {
            LogSdkResult(camera.IMV_Close(), "关闭相机");
            cameraOpen = false;
        }
        if (handleCreated)
        {
            LogSdkResult(camera.IMV_DestroyHandle(), "销毁相机句柄");
            handleCreated = false;
        }
        frameCallback = null;
        Connected = false;
        while (frames.TryTake(out var frame))
        {
            Marshal.FreeHGlobal(frame.Data);
        }
    }

    public override bool StartGrab()
    {
        if (!cameraOpen)
        {
            return false;
        }
        if (grabbing)
        {
            return true;
        }

        acceptingFrames = true;
        int status = camera.IMV_StartGrabbing();
        if (status != IMVDefine.IMV_OK)
        {
            acceptingFrames = false;
            LogSdkResult(status, "启动采集");
            return false;
        }
        grabbing = true;
        return true;
    }

    public override bool StopGrab()
    {
        acceptingFrames = false;
        if (!grabbing)
        {
            return true;
        }
        int status = camera.IMV_StopGrabbing();
        grabbing = false;
        LogSdkResult(status, "停止采集");
        return status == IMVDefine.IMV_OK;
    }

    private unsafe void OnFrame(ref IMVDefine.IMV_Frame frame, IntPtr user)
    {
        if (!acceptingFrames || frame.pData == IntPtr.Zero)
        {
            return;
        }

        IntPtr ownedData = IntPtr.Zero;
        try
        {
            int width = checked((int)frame.frameInfo.width);
            int height = checked((int)frame.frameInfo.height);
            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("相机返回了空图像");
            }
            bool mono = IsMono(frame.frameInfo.pixelFormat);
            var format = mono ? PixelFormats.Gray8 : PixelFormats.Bgr24;
            int bytesPerPixel = mono ? 1 : 3;
            int byteCount = checked(width * height * bytesPerPixel);
            ownedData = Marshal.AllocHGlobal(byteCount);

            if (frame.frameInfo.pixelFormat == IMVDefine.IMV_EPixelType.gvspPixelMono8)
            {
                int sourceStride = checked(width + (int)frame.frameInfo.paddingX);
                ulong requiredBytes = (ulong)(height - 1) * (uint)sourceStride + (uint)width;
                if (frame.frameInfo.size < requiredBytes)
                {
                    throw new InvalidOperationException("Mono8 帧长度小于图像尺寸");
                }
                for (int row = 0; row < height; row++)
                {
                    Buffer.MemoryCopy(
                        (byte*)frame.pData + row * sourceStride,
                        (byte*)ownedData + row * width,
                        width,
                        width
                    );
                }
            }
            else
            {
                var conversion = new IMVDefine.IMV_PixelConvertParam
                {
                    nWidth = frame.frameInfo.width,
                    nHeight = frame.frameInfo.height,
                    ePixelFormat = frame.frameInfo.pixelFormat,
                    pSrcData = frame.pData,
                    nSrcDataLen = frame.frameInfo.size,
                    nPaddingX = frame.frameInfo.paddingX,
                    nPaddingY = frame.frameInfo.paddingY,
                    eBayerDemosaic = IMVDefine.IMV_EBayerDemosaic.demosaicBilinear,
                    eDstPixelFormat = mono
                        ? IMVDefine.IMV_EPixelType.gvspPixelMono8
                        : IMVDefine.IMV_EPixelType.gvspPixelBGR8,
                    pDstBuf = ownedData,
                    nDstBufSize = (uint)byteCount,
                };
                lock (conversionLock)
                {
                    EnsureSuccess(camera.IMV_PixelConvert(ref conversion), "转换图像格式");
                }
            }

            if (!frames.TryAdd(new FramePacket(ownedData, width, height, format)))
            {
                CCameraManagement.CamLogger.Error($"华睿相机 {Setting.SerialNumber} 图像队列已满，丢弃一帧");
                return;
            }
            ownedData = IntPtr.Zero;
            Interlocked.Increment(ref grabCount);
        }
        catch (Exception ex)
        {
            CCameraManagement.CamLogger.Error($"华睿相机 {Setting.SerialNumber} 处理图像失败: {ex}");
        }
        finally
        {
            if (ownedData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ownedData);
            }
        }
    }

    private static bool IsMono(IMVDefine.IMV_EPixelType pixelType)
    {
        return pixelType.ToString().StartsWith("gvspPixelMono", StringComparison.Ordinal);
    }

    public override void GrabThread()
    {
        while (isStartGrabThread)
        {
            if (frames.TryTake(out var frame, 100))
            {
                ProcessFrame(frame);
            }
        }
        while (frames.TryTake(out var frame))
        {
            Marshal.FreeHGlobal(frame.Data);
        }
    }

    private void ProcessFrame(FramePacket frame)
    {
        IntPtr rotated = IntPtr.Zero;
        try
        {
            int width = frame.Width;
            int height = frame.Height;
            int rotation = (int)Setting.ImageRotate;
            if (rotation == 1 || rotation == 3)
            {
                (width, height) = (height, width);
            }
            int channels = (frame.Format.BitsPerPixel + 7) / 8;
            int stride = checked(width * channels);
            rotated = Marshal.AllocHGlobal(checked(stride * height));
            RotateImage(
                rotation,
                channels,
                frame.Width,
                frame.Height,
                frame.Width * channels,
                frame.Data,
                width,
                height,
                stride,
                rotated
            );

            Setting.ImageWidth = frame.Width;
            Setting.ImageHeight = frame.Height;
            Setting.CameraType = frame.Format;
            var image = new CImage(width, height, stride, rotated, frame.Format);
            rotated = IntPtr.Zero;
            bool hasConsumer = IsSetWindowShowed
                ? GrabFinishEvent != null
                : OutputImageChannel != null;
            if (!hasConsumer)
            {
                image.Dispose();
            }
            else
            {
                ExportImage(image);
            }
        }
        catch (Exception ex)
        {
            CCameraManagement.CamLogger.Error($"华睿相机 {Setting.SerialNumber} 输出图像失败: {ex}");
        }
        finally
        {
            if (rotated != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(rotated);
            }
            Marshal.FreeHGlobal(frame.Data);
        }
    }

    protected override void SetTriggerMode(EMTRIGGERMODE mode)
    {
        bool resume = grabbing;
        if (resume)
        {
            EnsureSuccess(camera.IMV_StopGrabbing(), "暂停采集以设置触发模式");
            grabbing = false;
            acceptingFrames = false;
        }
        try
        {
            ConfigureTrigger(mode);
        }
        catch
        {
            try
            {
                ConfigureTrigger(Setting.TriggerMode);
            }
            catch (Exception restoreError)
            {
                CCameraManagement.CamLogger.Error(
                    $"华睿相机 {Setting.SerialNumber} 恢复原触发模式失败: {restoreError}"
                );
            }
            throw;
        }
        finally
        {
            if (resume && !StartGrab())
            {
                Connected = false;
                throw new InvalidOperationException("设置触发模式后恢复采集失败");
            }
        }
    }

    private void ConfigureTrigger(EMTRIGGERMODE mode)
    {
        EnsureSuccess(camera.IMV_SetEnumFeatureSymbol("TriggerMode", "Off"), "暂停触发配置");
        if (mode == EMTRIGGERMODE.EMTRIGGERNONE)
        {
            return;
        }

        string source = mode == EMTRIGGERMODE.EMTRIGGERSOFTWARE
            ? "Software"
            : Parameters.TriggerLine.ToString();
        EnsureSuccess(camera.IMV_SetEnumFeatureSymbol("TriggerSource", source), "设置触发源");
        EnsureSuccess(camera.IMV_SetEnumFeatureSymbol("TriggerSelector", "FrameStart"), "选择帧触发");
        EnsureSuccess(camera.IMV_SetEnumFeatureSymbol("TriggerMode", "On"), "启用触发");
        if (mode == EMTRIGGERMODE.EMTRIGGERHARDWARE)
        {
            string desiredActivation = Parameters.TriggerActivation.ToString();
            var currentActivation = new IMVDefine.IMV_String();
            int readStatus = camera.IMV_GetEnumFeatureSymbol(
                "TriggerActivation",
                ref currentActivation
            );
            if (readStatus != IMVDefine.IMV_OK || currentActivation.str != desiredActivation)
            {
                EnsureSuccess(
                    camera.IMV_SetEnumFeatureSymbol("TriggerActivation", desiredActivation),
                    "设置触发极性"
                );
            }
        }
    }

    public override void ExecuteSoftwareTrigger()
    {
        if (!Connected || Setting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE)
        {
            return;
        }
        LogSdkResult(camera.IMV_ExecuteCommandFeature("TriggerSoftware"), "执行软触发");
    }

    public override bool GetTriggerMode(out EMTRIGGERMODE mode)
    {
        mode = EMTRIGGERMODE.EMTRIGGERNONE;
        if (!TryGetEnum("TriggerMode", out var enabled))
        {
            return false;
        }
        if (enabled == "Off")
        {
            return true;
        }
        if (!TryGetEnum("TriggerSource", out var source))
        {
            return false;
        }
        mode = source == "Software"
            ? EMTRIGGERMODE.EMTRIGGERSOFTWARE
            : EMTRIGGERMODE.EMTRIGGERHARDWARE;
        return true;
    }

    public override void SetCameraParam()
    {
        if (GetImageWidth(out int width)) Setting.ImageWidth = width;
        if (GetImageHeight(out int height)) Setting.ImageHeight = height;
        if (GetCameraType(out PixelFormat format)) Setting.CameraType = format;
        SetExposureTime(Setting.ExposureTime);
        SetGain(Setting.Gain);
        SetTriggerDelay(Setting.TriggerDelay);
        SetGamma(Setting.Gamma);
    }

    public override bool GetImageWidth(out int value) => TryGetInt("Width", out value);
    public override bool GetImageHeight(out int value) => TryGetInt("Height", out value);

    public override bool GetCameraType(out PixelFormat cameraType)
    {
        cameraType = PixelFormats.Gray8;
        if (!TryGetEnum("PixelFormat", out string symbol)) return false;
        cameraType = symbol.StartsWith("Mono", StringComparison.Ordinal)
            ? PixelFormats.Gray8
            : PixelFormats.Bgr24;
        return true;
    }

    public override bool GetExposureTime(out uint value)
    {
        value = 0;
        if (!TryGetDouble("ExposureTime", out double result)) return false;
        value = (uint)Math.Round(result);
        return true;
    }

    public override void SetExposureTime(uint value) => TrySetDouble("ExposureTime", value);

    public override bool GetGain(out float value)
    {
        value = 0;
        if (!TryGetDouble("GainRaw", out double result)
            && !TryGetDouble("Gain", out result)) return false;
        value = (float)result;
        return true;
    }

    public override void SetGain(float value)
    {
        if (!TrySetDouble("GainRaw", value)) TrySetDouble("Gain", value);
    }

    public override bool GetGamma(out float value)
    {
        value = 0;
        if (!TryGetDouble("Gamma", out double result)) return false;
        value = (float)result;
        return true;
    }

    public override void SetGamma(float value) => TrySetDouble("Gamma", value);
    public override void SetTriggerDelay(uint value) => TrySetDouble("TriggerDelay", value);

    public override bool GetTriggerDelay(out uint value)
    {
        value = 0;
        if (!TryGetDouble("TriggerDelay", out double result)) return false;
        value = (uint)Math.Round(result);
        return true;
    }

    public override void SetTriggerPulseWidth(uint value)
    {
        if (value != 0)
        {
            CCameraManagement.CamLogger.Error("华睿相机不支持通过插件设置触发脉宽；由外部脉冲源控制");
        }
    }

    public override bool GetTriggerPulseWidth(out uint value)
    {
        value = 0;
        return true;
    }

    public override void UserSaveParam() { }
    public override void UserLoadParam() { }

    public override void SetCustomParam(uint value)
    {
        if (Connected && Setting.TriggerMode == EMTRIGGERMODE.EMTRIGGERHARDWARE)
        {
            SetTriggerMode(EMTRIGGERMODE.EMTRIGGERHARDWARE);
        }
    }

    public override void SetStrobeEnable(bool enable) => throw new NotSupportedException();
    public override void SetLineSelector(object line) => throw new NotSupportedException();
    public override void SetStrobeDuration(uint value) => throw new NotSupportedException();
    public override void SetLineSource(object source) => throw new NotSupportedException();
    public override void SetLineInverter(bool enable) => throw new NotSupportedException();
    public override void SetLineMode(object lineMode) => throw new NotSupportedException();
    public override void LineTriggerSoftware() => ExecuteSoftwareTrigger();
    public override void SetGammaEnable(bool enable) => TrySetEnum("GammaEnable", enable ? "On" : "Off");
    public override float GetFps() => 0;
    public override void SetFrameCount(int count) { }

    private bool TryGetInt(string feature, out int value)
    {
        long raw = 0;
        int status = camera.IMV_GetIntFeatureValue(feature, ref raw);
        value = status == IMVDefine.IMV_OK ? checked((int)raw) : 0;
        if (status != IMVDefine.IMV_OK) LogSdkResult(status, $"读取 {feature}");
        return status == IMVDefine.IMV_OK;
    }

    private bool TryGetDouble(string feature, out double value)
    {
        value = 0;
        int status = camera.IMV_GetDoubleFeatureValue(feature, ref value);
        if (status != IMVDefine.IMV_OK) LogSdkResult(status, $"读取 {feature}");
        return status == IMVDefine.IMV_OK;
    }

    private bool TrySetDouble(string feature, double value)
    {
        int status = camera.IMV_SetDoubleFeatureValue(feature, value);
        if (status != IMVDefine.IMV_OK) LogSdkResult(status, $"设置 {feature}");
        return status == IMVDefine.IMV_OK;
    }

    private bool TryGetEnum(string feature, out string value)
    {
        var symbol = new IMVDefine.IMV_String();
        int status = camera.IMV_GetEnumFeatureSymbol(feature, ref symbol);
        value = status == IMVDefine.IMV_OK ? symbol.str : string.Empty;
        if (status != IMVDefine.IMV_OK) LogSdkResult(status, $"读取 {feature}");
        return status == IMVDefine.IMV_OK;
    }

    private void TrySetEnum(string feature, string value)
    {
        LogSdkResult(camera.IMV_SetEnumFeatureSymbol(feature, value), $"设置 {feature}");
    }

    private static void EnsureSuccess(int status, string operation)
    {
        if (status != IMVDefine.IMV_OK)
        {
            throw new InvalidOperationException($"华睿 SDK {operation} 失败，错误码 {status}");
        }
    }

    private void LogSdkResult(int status, string operation)
    {
        if (status != IMVDefine.IMV_OK)
        {
            CCameraManagement.CamLogger.Error(
                $"华睿相机 {Setting.SerialNumber} {operation} 失败，SDK 错误码 {status}"
            );
        }
    }
}
