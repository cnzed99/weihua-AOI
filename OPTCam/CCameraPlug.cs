
using CameraModule;
using SciCamera.Net;
using System.Net;
using System.Runtime.InteropServices;
using WH.Entity;

namespace OPTCam
{
    public class CCameraPlug : ICamera
    {
        public CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam)
        {
            COPTCamera camera = new COPTCamera();
            camera.paramSetting = new COPTParameterSetting(serialNumber, "OPTCam");
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }

        public List<WHCameraInfo> EnumCamrea()
        {
            //储存相机信息
            List<WHCameraInfo> CamListstr = new List<WHCameraInfo>();

            //储存设备信息
            //IMVDefine.IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();
            SciCam.SCI_DEVICE_INFO_LIST m_stDevList = new SciCam.SCI_DEVICE_INFO_LIST();
            
            
            
           

            //IMVDefine.IMV_EInterfaceType interfaceType = IMVDefine.IMV_EInterfaceType.interfaceTypeAll;

            try
            {
                //枚举设备
                //int res = MyCamera.IMV_EnumDevices(ref deviceList, (uint)interfaceType);
                uint nReVal = SciCam.DiscoveryDevices(ref m_stDevList, (uint)(SciCam.SciCamTLType.SciCam_TLType_Gige) | (uint)(SciCam.SciCamTLType.SciCam_TLType_Usb3));
                //枚举所有设备，并将结果存储在 deviceList 中
                for (int i = 0; i < m_stDevList.count; i++)
                {
                    SciCam.SCI_DEVICE_INFO device = m_stDevList.pDevInfo[i];
                    SciCam.SciCamTLType devTlType = device.tlType;
                    SciCam.SciCamDeviceType devType = device.devType;

                    WHCameraInfo info = new WHCameraInfo();
                    bool isDaHuacamera = false;

                    // 枚举设备
                    // enum device
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        // 枚举失败或无设备，直接返回空列表
                        return CamListstr;
                    }
                    if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Gige)
                    {
                        SciCam.SCI_DEVICE_GIGE_INFO gigeDevInfo = (SciCam.SCI_DEVICE_GIGE_INFO)SciCam.ByteToStruct(device.info.gigeInfo, typeof(SciCam.SCI_DEVICE_GIGE_INFO));
                        //string devModelName = gigeDevInfo.modelName;
                        info.SerialNumber = gigeDevInfo.serialNumber;
                        info.CamType = "网口相机";

                        string devIP = i4tos(gigeDevInfo.ip);
                        info.CamIp = devIP;
                        //string itemName = string.Format("[{0}] GigE: {1}({2})----[{3}]", i, devModelName, devSerialNumber, devIP);

                    }
                    else if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Usb3)
                    {
                        SciCam.SCI_DEVICE_USB3_INFO usb3Info = (SciCam.SCI_DEVICE_USB3_INFO)SciCam.ByteToStruct(device.info.usb3Info, typeof(SciCam.SCI_DEVICE_USB3_INFO));
                        //string devModelName = usb3Info.modelName;
                        info.SerialNumber = usb3Info.serialNumber;
                        info.CamType = "USB相机";
                        info.CamIp = usb3Info.guid;
                        //string itemName = string.Format("[{0}] U3V: {1}({2})", i, devModelName, devSerialNumber);

                    }
                    CamListstr.Add(info);


                   

                }

                return CamListstr;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error("获取所有连接的大华相机出现异常:" + ex.Message);
                throw;

            }
        }
        private string i4tos(uint ip)
        {
            IPAddress iPAddress = new IPAddress(ip);
            return iPAddress.ToString();
        }

        public CCameraParameterBase Init(string path, int index, out CCameraBase cam)
        {
            COPTCamera camera = new COPTCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<COPTParameterSetting>>(path)[index];
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }
    }

}
