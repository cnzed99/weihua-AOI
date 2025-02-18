using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using CameraModule;
using IKapBoardClassLibrary;
using IKapC.NET;
using WH.Entity;

namespace IKapVisionCam
{
    /// <summary>
    /// 2025.1.14 李焕彬
    /// 相机插口类
    /// </summary>
    public class CCameraPlug : ICamera
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// 枚举相机
        /// </summary>
        /// <returns>相机集合</returns>
        public List<WHCameraInfo> EnumCamrea()
        {
            try
            {
                List<WHCameraInfo> CamListstr = new List<WHCameraInfo>();

                uint res = IKapCLib.ItkManInitialize();
                if (!CheckIKapC(res))
                {
                    throw new Exception(res.ToString());
                }
                uint numCameras = 0;
                res = IKapCLib.ItkManGetDeviceCount(ref numCameras);
                CheckIKapC(res);
                if (numCameras == 0)
                    throw new Exception(res.ToString());
                for (uint i = 0; i < numCameras; i++)
                {
                    IKapCLib.ITKDEV_INFO di = new IKapCLib.ITKDEV_INFO();

                    // 获取相机设备信息。
                    //
                    // Get camera device information.
                    res = IKapCLib.ItkManGetDeviceInfo(i, ref di);
                    // 当设备为CoaXPress相机且序列号正确时。
                    // When the device is CoaXPress camera and the serial number is proper.
                    if (di.DeviceClass == "CoaXPress" && di.SerialNumber != "")
                    {
                        WHCameraInfo info = new WHCameraInfo();
                        info.SerialNumber = di.SerialNumber;
                        info.CamIp = "CoaXPress";
                        info.CamType = "CoaXPress";
                        CamListstr.Add(info);
                    }
                }
                return CamListstr;
            }
            catch (Exception)
            {
                IKapCLib.ItkManTerminate();
                return new List<WHCameraInfo>();
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初始化相机
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="index">相机索引</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns></returns>
        public CCameraParameterBase Init(string path, int index, out CCameraBase cam)
        {
            CCamera camera = new CCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<CParameterSetting>>(path)[index];
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 创建新相机
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns></returns>
        public CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam)
        {
            CCamera camera = new CCamera();
            camera.paramSetting = new CParameterSetting(
                serialNumber,
                Assembly.GetExecutingAssembly().GetName().Name
            );
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
    }
}
