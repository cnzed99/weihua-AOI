using CameraModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using WH.Entity;
using DVPCameraType;

namespace DoThinkCamMulti
{
    public class CCameraPlug : ICamera 
    {
        /// <summary>
        /// 枚举已连接的相机
        /// </summary>
        /// <returns></returns>
        public List<WHCameraInfo> EnumCamrea()
        {
            List<WHCameraInfo> CamListstr = new List<WHCameraInfo>();
            uint count = 0;
            dvpStatus status = DVPCamera.dvpRefresh(ref count);

            try
            {

                for (uint i = 0; i < count; i++)
                {
                    dvpCameraInfo CamInfo = new dvpCameraInfo();

                    status = DVPCamera.dvpEnum(i, ref CamInfo);

                    WHCameraInfo info = new WHCameraInfo();

                    string prot = CamInfo.PortInfo;

                    bool isDScamera = false;
                    if ((prot == "Network Camera")||(prot == "GigE")) //网口相机
                    {
                        info.SerialNumber = CamInfo.SerialNumber;

                        if (CamInfo.Vendor == "")
                        {
                            isDScamera = true;
                           // info.Vender = "DoThinkCam";
                        }
                        int index = CamInfo.LinkName.IndexOf('-') + 1;
                        info.CamIp = CamInfo.LinkName.Substring(index);
                        // info.NetIp = CamInfo.IPAddress;
                        info.CamType = "网口相机";
                    }
                    if (prot == "USB") //USB相机
                    {
                        info.SerialNumber = CamInfo.SerialNumber;
                        if (CamInfo.Vendor == "")
                        {
                            isDScamera = true;
                            //info.Vender = "度申";
                        }
                        int index = CamInfo.LinkName.IndexOf('-') + 1;
                        info.CamIp = CamInfo.LinkName.Substring(index);
                        // info.NetIp = CamInfo.IPAddress;                    
                        info.CamType = "USB相机";
                    }

                    if (isDScamera)
                    {
                        CamListstr.Add(info);
                    }
                }

                return CamListstr;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error("枚举已连接的度申相机出现异常:" + ex.Message);
                throw;
            }

        }
        /// <summary>
        /// 初始化相机
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="index">相机索引</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns></returns>
        public CCameraParameterBase Init(string path, int index, out CCameraBase cam)
        {
            CDoThinkCamera camera = new CDoThinkCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<CDoThinkParameterSetting>>(path)[index];
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
        /// <summary>
        /// 创建新相机
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="cam">输出相机实例</param>
        /// <returns></returns>
        public CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam)
        {
            CDoThinkCamera camera = new CDoThinkCamera();
            camera.paramSetting = new CDoThinkParameterSetting(serialNumber, "DoThinkCamMulti");
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
    }
}
