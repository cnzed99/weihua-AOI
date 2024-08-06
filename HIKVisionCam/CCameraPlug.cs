using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using CameraModule;
using MvCamCtrl.NET;
using WH.Entity;
using static MvCamCtrl.NET.MyCamera;

namespace HIKVisionCam
{
    /// <summary>
    /// 20240802 TCG
    /// 相机插口类
    /// </summary>
    public class CCameraPlug : ICamera
    {
        /// <summary>
        /// 2024.8.2 李焕彬
        /// uint转string，获取IP
        /// </summary>
        /// <param name="uintIP">uint IP</param>
        /// <returns>string IP</returns>
        private string GetIP(uint uintIP)
        {
            try
            {
                byte[] ipBytes = BitConverter.GetBytes(uintIP);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(ipBytes);

                IPAddress ipAddress = new IPAddress(ipBytes);
                string ipString = ipAddress.ToString();

                return ipString;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.08.02 李焕彬
        /// 枚举相机
        /// </summary>
        /// <returns>相机集合</returns>
        public List<WHCameraInfo> EnumCamrea()
        {
            List<WHCameraInfo> CamListstr = new List<WHCameraInfo>();

            MV_CC_DEVICE_INFO_LIST deviceList = new MV_CC_DEVICE_INFO_LIST();
            try
            {
                int nRet = MV_CC_EnumDevices_NET(MV_GIGE_DEVICE | MV_USB_DEVICE, ref deviceList);

                for (int i = 0; i < deviceList.nDeviceNum; i++)
                {
                    WHCameraInfo info = new WHCameraInfo();
                    bool isHikcamera = false;
                    MV_CC_DEVICE_INFO device = (MV_CC_DEVICE_INFO)
                        Marshal.PtrToStructure(
                            deviceList.pDeviceInfo[i],
                            typeof(MV_CC_DEVICE_INFO)
                        );
                    if (device.nTLayerType == MV_GIGE_DEVICE) //网口相机
                    {
                        MV_GIGE_DEVICE_INFO gigeInfo = (MV_GIGE_DEVICE_INFO)ByteToStruct(
                            device.SpecialInfo.stGigEInfo,
                            typeof(MV_GIGE_DEVICE_INFO)
                        );
                        info.SerialNumber = gigeInfo.chSerialNumber;
                        info.CamIp = GetIP(gigeInfo.nCurrentIp);
                        info.CamType = "网口相机";
                        isHikcamera =
                            gigeInfo.chManufacturerName == "Hikrobot"
                            || gigeInfo.chManufacturerName == "GEV";
                    }
                    if (device.nTLayerType == MV_USB_DEVICE)
                    {
                        MV_USB3_DEVICE_INFO usbInfo = (MV_USB3_DEVICE_INFO)ByteToStruct(
                            device.SpecialInfo.stGigEInfo,
                            typeof(MV_USB3_DEVICE_INFO)
                        );
                        info.SerialNumber = usbInfo.chSerialNumber;
                        info.CamIp = GetIP(usbInfo.idProduct);
                        info.CamType = "USB相机";
                        isHikcamera =
                            usbInfo.chManufacturerName == "Hikrobot"
                            || usbInfo.chManufacturerName == "GEV";
                    }
                    if (isHikcamera)
                    {
                        info.Vender = Assembly.GetExecutingAssembly().GetName().Name;
                        CamListstr.Add(info);
                    }
                }

                return CamListstr;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error("获取所有连接的海康相机出现异常:" + ex.Message);
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
            CHIKCamera camera = new CHIKCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<CHIKParameterSetting>>(path)[
                index
            ];
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
            CHIKCamera camera = new CHIKCamera();
            camera.paramSetting = new CHIKParameterSetting(
                serialNumber,
                Assembly.GetExecutingAssembly().GetName().Name
            );
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
    }
}
