using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using CameraModule;
using WH.Entity;

namespace StichingFourCam
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

                WHCameraInfo info = new WHCameraInfo();
                info.SerialNumber = $"四相机拼图";
                info.CamIp = $"四相机拼图";
                info.CamType = "四相机拼图";
                CamListstr.Add(info);

                return CamListstr;
            }
            catch (Exception)
            {
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
