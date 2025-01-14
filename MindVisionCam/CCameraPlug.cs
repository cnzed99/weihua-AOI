using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using CameraModule;
using MVSDK;
using WH.Entity;

namespace MindVisionCam
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机插口类
    /// </summary>
    public class CCameraPlug : ICamera
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 枚举相机
        /// </summary>
        /// <returns>相机集合</returns>
        public List<WHCameraInfo> EnumCamrea()
        {
            List<WHCameraInfo> CamList = new List<WHCameraInfo>();

            try
            {
                CameraSdkStatus status = MvApi.CameraEnumerateDevice(
                    out tSdkCameraDevInfo[] tCameraDevInfoList
                );
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    foreach (var cameraInfo in tCameraDevInfoList)
                    {
                        WHCameraInfo info = new WHCameraInfo();
                        info.SerialNumber = Encoding
                            .Default.GetString(cameraInfo.acSn)
                            .Replace("\0", "");
                        info.CamType = Encoding
                            .Default.GetString(cameraInfo.acProductSeries)
                            .Replace("\0", "");
                        string[] portType = Encoding
                            .Default.GetString(cameraInfo.acPortType)
                            .Split('-');
                        info.CamType = portType[0];
                        info.CamIp = portType[2].Replace("\0", "");
                        CamList.Add(info);
                    }
                }
                else if (status != CameraSdkStatus.CAMERA_STATUS_NO_DEVICE_FOUND)
                {
                    CCameraManagement.CamLogger.Error(Properties.Resources.ErrorEnumCam1);
                }
                return CamList;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(Properties.Resources.ErrorEnumCam2 + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化相机
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="index">相机索引</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns>相机参数</returns>
        public CCameraParameterBase Init(string path, int index, out CCameraBase cam)
        {
            CMindVSCamera camera = new CMindVSCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<CMindVSParameterSetting>>(path)[
                index
            ];
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 创建新相机
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns>相机参数</returns>
        public CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam)
        {
            CMindVSCamera camera = new CMindVSCamera();
            camera.paramSetting = new CMindVSParameterSetting(
                serialNumber,
                Assembly.GetExecutingAssembly().GetName().Name
            );
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
    }
}
