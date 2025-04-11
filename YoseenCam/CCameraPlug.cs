using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using WH.Entity;
using YoseenSDKCS;

namespace YoseenCam
{
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

                int error = YoseenSDKCS.YoseenSDK.Yoseen_InitSDK();
                if (error < 0)
                    throw new Exception(string.Format("Yoseen_InitSDK, ret {0}.", error));
                YoseenSDKCS.DiscoverCameraResp2[] dcrs =
                    YoseenSDKCS.YoseenSDK.Yoseen_DiscoverCameras2(0x01);

                foreach (var cam in dcrs)
                {
                    WHCameraInfo info = new WHCameraInfo();
                    info.SerialNumber = cam.BasicInfo.CameraId;
                    info.CamIp = YoseenSDKCS.YoseenUtil.uint2str(cam.CameraIp);
                    info.CamType = "internet";
                    CamListstr.Add(info);
                }

                return CamListstr;
            }
            catch (Exception)
            {
                YoseenSDKCS.YoseenSDK.Yoseen_FreeSDK();
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
