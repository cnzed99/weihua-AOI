using CameraModule;
using MVSDK_Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WH.Entity;



namespace DaHuaCam
{
   public class CCameraPlug : ICamera
   {
        /// <summary>
        /// 枚举已连接的相机
        /// </summary>
        /// <returns>相机集合</returns>
        public List<WHCameraInfo> EnumCamrea()                                                      
        {

            //储存相机信息
            List<WHCameraInfo> CamListstr = new List<WHCameraInfo>();

            //储存设备信息
            IMVDefine.IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();

            IMVDefine.IMV_EInterfaceType interfaceType = IMVDefine.IMV_EInterfaceType.interfaceTypeAll;

            try
            {
                //枚举设备
                int res = MyCamera.IMV_EnumDevices(ref deviceList, (uint)interfaceType);

                //枚举所有设备，并将结果存储在 deviceList 中
                for (int i = 0; i < deviceList.nDevNum; i++)
                {
                    
                    WHCameraInfo info = new WHCameraInfo();
                    bool isDaHuacamera = false;

                    // 枚举设备
                    // enum device
                    if (res != IMVDefine.IMV_OK)
                    {
                        // 枚举失败或无设备，直接返回空列表
                         return CamListstr;
                    }

                    IMVDefine.IMV_DeviceInfo deviceInfo =
                            (IMVDefine.IMV_DeviceInfo)
                                Marshal.PtrToStructure(
                                    deviceList.pDevInfo + Marshal.SizeOf(typeof(IMVDefine.IMV_DeviceInfo)) * i,
                                    typeof(IMVDefine.IMV_DeviceInfo));


                    if (deviceInfo.nCameraType== IMVDefine.IMV_ECameraType.typeGigeCamera)//网口相机
                    {
                        info.SerialNumber = deviceInfo.serialNumber;
                        info.CamType = "网口相机";
                        //info.CamIp = GetIP(deviceInfo.interfaceName);

                        IMVDefine.IMV_GigEDeviceInfo gigEDeviceInfo =
                            (IMVDefine.IMV_GigEDeviceInfo)
                            ByteToStruct(deviceInfo.deviceSpecificInfo.gigeDeviceInfo,
                               typeof(IMVDefine.IMV_GigEDeviceInfo));
                        info.CamIp = gigEDeviceInfo.ipAddress;

                        //info.CamIp = deviceInfo.cameraKey;
                        if (deviceInfo.manufactureInfo == "Huaray Technology"|| deviceInfo.manufactureInfo == "Machine Vision")
                        {
                            isDaHuacamera = true;

                        }
                    }

                    if (deviceInfo.nCameraType == IMVDefine.IMV_ECameraType.typeU3vCamera)//USB相机
                    {
                        info.SerialNumber = deviceInfo.serialNumber;
                        info.CamType = "USB相机";
                        info.CamIp = GetIP(deviceInfo.interfaceName);
                        if (deviceInfo.manufactureInfo == "Huaray Technology" || deviceInfo.manufactureInfo == "Machine Vision")
                        {
                            isDaHuacamera = true;
                        }
                    }

                    if (isDaHuacamera)
                    {
                        CamListstr.Add(info);
                    }

                }

                return CamListstr;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error("获取所有连接的大华相机出现异常:" + ex.Message);
                throw;

            }


        }

        public static object ByteToStruct(Byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }

            // 分配结构体内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);

            // 将byte数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);

            // 将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);

            // 释放内存空间
            Marshal.FreeHGlobal(structPtr);

            return obj;
        }


        /// <summary>
        /// 2025.7.9
        /// uint转string，获取IP
        /// </summary>
        /// <param name="uintIP">uint IP</param>
        /// <returns>string IP</returns>
        private string GetIP(string uintIP)
        {
            try
            {
                // 匹配方括号中的IP地址
                var match = Regex.Match(uintIP, @"\[(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\]");
                return match.Success ? match.Groups[1].Value : string.Empty;

            }
            catch (Exception)
            {
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
            CDaHuaCamera camera = new CDaHuaCamera();
            camera.paramSetting = ConfigAPI.LoadDeserialize<List<CDaHuaParameterSetting>>(path)[index];
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
            CDaHuaCamera camera = new CDaHuaCamera();
            camera.paramSetting = new CDaHuaParameterSetting(serialNumber, "DaHuaCam");
            camera.Init(camera.paramSetting);
            cam = camera;
            return camera.paramSetting;
        }

    }






}
