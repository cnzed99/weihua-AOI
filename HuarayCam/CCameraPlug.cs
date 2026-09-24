using System.Runtime.InteropServices;
using CameraModule;
using MVSDK_Net;
using WH.Entity;

namespace HuarayCam;

public class CCameraPlug : ICamera
{
    public List<WHCameraInfo> EnumCamrea()
    {
        HuarayRuntime.EnsureAvailable();
        var result = new List<WHCameraInfo>();
        var devices = new IMVDefine.IMV_DeviceList();
        int status = MyCamera.IMV_EnumDevices(
            ref devices,
            (uint)IMVDefine.IMV_EInterfaceType.interfaceTypeAll
        );
        if (status != IMVDefine.IMV_OK)
        {
            CCameraManagement.CamLogger.Error($"华睿相机枚举失败: {status}");
            return result;
        }

        int deviceSize = Marshal.SizeOf<IMVDefine.IMV_DeviceInfo>();
        for (int index = 0; index < devices.nDevNum; index++)
        {
            var device = Marshal.PtrToStructure<IMVDefine.IMV_DeviceInfo>(
                IntPtr.Add(devices.pDevInfo, index * deviceSize)
            );
            if (!CHuarayCamera.IsSupportedDevice(device))
            {
                continue;
            }

            // A saved DaHuaCam instance remains owned by the legacy plug-in.
            if (CCameraManagement.CamParamDict.TryGetValue(device.serialNumber, out var saved)
                && saved.CameraSupplier == "DaHuaCam")
            {
                continue;
            }

            string ip = string.Empty;
            string type = "USB相机";
            if (device.nCameraType == IMVDefine.IMV_ECameraType.typeGigeCamera)
            {
                type = "网口相机";
                var gigEBytes = device.deviceSpecificInfo.gigeDeviceInfo;
                if (gigEBytes != null
                    && gigEBytes.Length >= Marshal.SizeOf<IMVDefine.IMV_GigEDeviceInfo>())
                {
                    var handle = GCHandle.Alloc(gigEBytes, GCHandleType.Pinned);
                    try
                    {
                        var gigE = Marshal.PtrToStructure<IMVDefine.IMV_GigEDeviceInfo>(
                            handle.AddrOfPinnedObject()
                        );
                        ip = gigE.ipAddress;
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }

            result.Add(new WHCameraInfo
            {
                SerialNumber = device.serialNumber,
                CamType = type,
                CamIp = ip,
            });
        }

        return result;
    }

    public CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam)
    {
        var camera = new CHuarayCamera();
        var setting = new CHuarayParameterSetting(serialNumber);
        camera.Init(setting);
        cam = camera;
        return setting;
    }

    public CCameraParameterBase Init(string path, int index, out CCameraBase cam)
    {
        var camera = new CHuarayCamera();
        var setting = ConfigAPI.LoadDeserialize<List<CHuarayParameterSetting>>(path)[index];
        camera.Init(setting);
        cam = camera;
        return setting;
    }
}
