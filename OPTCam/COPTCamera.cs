using CameraModule;
using Newtonsoft.Json.Linq;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using SciCamera.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using static OPTCam.COPTParameterSetting;
using GdiPlus = System.Drawing.Imaging;

namespace OPTCam
{
    public class COPTCamera : CCameraBase
    {
        /// <summary>
        /// 2026.04.13 WZY
        /// 相机参数
        /// </summary>
        internal COPTParameterSetting paramSetting { get; set; }

        

        public COPTCamera()
           : base() { }

        /// <summary>
        /// 相机对象
        /// </summary>
        SciCam m_currentDev = new SciCam();
        private IntPtr m_pDstData;
        private nint m_nDataLenth = 0;
        /// <summary>
        /// 相机回调
        /// </summary>

        private  SciCam.fnOnPayloadDelegate imageCallback;        // 静态回调变量


        public override bool OpenCamera()
        {
            try
            {
                //ch:设备列表 | en:Device List
                SciCam.SCI_DEVICE_INFO_LIST m_stDevList = new SciCam.SCI_DEVICE_INFO_LIST();    
                #region 枚举相机
                uint nReVal = SciCam.DiscoveryDevices(ref m_stDevList, (uint)(SciCam.SciCamTLType.SciCam_TLType_Gige) | (uint)(SciCam.SciCamTLType.SciCam_TLType_Usb3));
                if (nReVal != SciCam.SCI_CAMERA_OK|| m_stDevList.count == 0)
                {
                    throw new Exception(Properties.Resources.ErrorEnumCam1);
                    //    ShowMsg("Discovery devices failed!", nReVal, -1, true);

                    //    return;
                }
                //if (m_stDevList.count == 0)
                //{
                //    throw new Exception(Properties.Resources.ErrorEnumCam1);
                //    //ShowMsg("Discovery devices Success, but found 0 device.", 0, -1, true);
                //    //return;
                //}
                //string chDeviceName;

                //相机命名
                //for (int i = 0; i < m_stDevList.count; i++)
                //{
                //    //CheckBox checkBox = Controls.Find("checkBox" + (i + 1), true).FirstOrDefault() as CheckBox;

                //    SciCam.SCI_DEVICE_INFO device = m_stDevList.pDevInfo[i];
                //    SciCam.SciCamTLType devTlType = device.tlType;
                //    SciCam.SciCamDeviceType devType = device.devType;
                //    if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Gige)
                //    {
                //        SciCam.SCI_DEVICE_GIGE_INFO gigeDevInfo = (SciCam.SCI_DEVICE_GIGE_INFO)SciCam.ByteToStruct(device.info.gigeInfo, typeof(SciCam.SCI_DEVICE_GIGE_INFO));
                //        string devModelName = gigeDevInfo.modelName;
                //        string devSerialNumber = gigeDevInfo.serialNumber;
                //        string devIP = i4tos(gigeDevInfo.ip);

                //        string itemName = string.Format("[{0}] GigE: {1}({2})----[{3}]", i, devModelName, devSerialNumber, devIP);
                //        //if (!string.IsNullOrEmpty(gigeDevInfo.userDefineName))
                //        //{

                //        //chDeviceName = string.Format("{0} [{1}]", gigeDevInfo.modelName, gigeDevInfo.userDefineName);

                //        //}
                //        //else
                //        //{
                //        //    chDeviceName = string.Format("{0} [{1}]", gigeDevInfo.modelName, gigeDevInfo.serialNumber);

                //        //}
                //        //checkBox.Enabled = true;
                //        //checkBox.Text = chDeviceName;
                //        //m_nValidCamNum++;
                //    }
                //    else if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Usb3)
                //    {
                //        SciCam.SCI_DEVICE_USB3_INFO usb3Info = (SciCam.SCI_DEVICE_USB3_INFO)SciCam.ByteToStruct(device.info.usb3Info, typeof(SciCam.SCI_DEVICE_USB3_INFO));
                //        string devModelName = usb3Info.modelName;
                //        string devSerialNumber = usb3Info.serialNumber;

                //        string itemName = string.Format("[{0}] U3V: {1}({2})", i, devModelName, devSerialNumber);
                       
                //        //if (!string.IsNullOrEmpty(usb3Info.userDefineName))
                //        //{
                //        //    chDeviceName = string.Format("{0} [{1}]", usb3Info.modelName, usb3Info.userDefineName);

                //        //}
                //        //else
                //        //{
                //        //    chDeviceName = string.Format("{0} [{1}]", usb3Info.modelName, usb3Info.serialNumber);

                //        //}
                //        //checkBox.Enabled = true;
                //        //checkBox.Text = chDeviceName;
                //        //m_nValidCamNum++;
                //    }
                //    //else
                //    //{
                //    //    chDeviceName = string.Format("Cam {0}", i + 1);
                //    //    //checkBox.Text = chDeviceName;
                //    //    //checkBox.Enabled = false;
                //    //}

                //    //m_bCamCheck[i] = false;
                //}
                #endregion
                #region 打开相机
                // 遍历设备寻找匹配序列号的相机
                for (int i = 0; i < m_stDevList.count; i++)
                {
                    //获取相机信息
                    string devSerialNumber = "";
                    SciCam.SCI_DEVICE_INFO device = m_stDevList.pDevInfo[i];
                    SciCam.SciCamTLType devTlType = device.tlType;
                    SciCam.SciCamDeviceType devType = device.devType;
                    if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Gige)
                    {
                        SciCam.SCI_DEVICE_GIGE_INFO gigeDevInfo = (SciCam.SCI_DEVICE_GIGE_INFO)SciCam.ByteToStruct(device.info.gigeInfo, typeof(SciCam.SCI_DEVICE_GIGE_INFO));
                        //string devModelName = gigeDevInfo.modelName;
                        devSerialNumber = gigeDevInfo.serialNumber;
                        //string devIP = i4tos(gigeDevInfo.ip);
                        //string itemName = string.Format("[{0}] GigE: {1}({2})----[{3}]", i, devModelName, devSerialNumber, devIP);
                       
                    }
                    else if (devTlType == SciCam.SciCamTLType.SciCam_TLType_Usb3)
                    {
                        SciCam.SCI_DEVICE_USB3_INFO usb3Info = (SciCam.SCI_DEVICE_USB3_INFO)SciCam.ByteToStruct(device.info.usb3Info, typeof(SciCam.SCI_DEVICE_USB3_INFO));
                        //string devModelName = usb3Info.modelName;
                        devSerialNumber = usb3Info.serialNumber;

                        //string itemName = string.Format("[{0}] U3V: {1}({2})", i, devModelName, devSerialNumber);

                    }

                    // 检查厂商和序列号
                    if (devSerialNumber != paramSetting.SerialNumber)
                    {
                        continue;
                    }


                    nReVal = m_currentDev.CreateDevice(ref m_stDevList.pDevInfo[i]);
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                        );
                        return false;
                        //ShowMsg("Create device failed", nReVal);
                        //return;
                    }

                    nReVal = m_currentDev.OpenDevice();
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                        );
                        
                        return false;
                        //ShowMsg("Open device failed", nReVal);
                        //return;
                    }


                    // ch:注册事件回调 | en:Register event callback
                    imageCallback = new SciCam.fnOnPayloadDelegate(OnPayloadReceived);
                    nReVal = m_currentDev.RegisterPayloadCallBack(imageCallback, IntPtr.Zero, true);
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                          Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                          );

                      
                        //Console.WriteLine("Register Event CallBack fail! nRet {0}", nReVal);
                        //break;
                    }
                    this.Connected = true;
                    return StartGrab();
                    ////m_bDeviceOpened = true;
                    ////RefreshWindow();
                    //RefreshTriggerModeStatus();
                    //GetPixelFormat();
                    //GetParameter();

                }
                // 没有找到匹配的相机
                CCameraManagement.CamLogger.Error(
                    $"未找到匹配序列号{paramSetting.SerialNumber}的奥普特相机");
                return false;

                #endregion
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                    );
                return false;
            }
            
        }
        // ch:采集回调接口 | en:Grab Callback interface     
        public void ImageCallBack(IntPtr payload, IntPtr tag)
        {
            try 
            {
                startGrabSoft = true;
                grabCount++;  // 帧计数
                StringBuilder textBuilder = new StringBuilder("OPT");
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());

                //imageCallback = new SciCam.fnOnPayloadDelegate(OnPayloadReceived);
                //if (payload == IntPtr.Zero) return;
                //uint nReVal = SciCam.SCI_CAMERA_OK;
                //SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE attribute = new SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE();
                //nReVal = SciCam.PayloadGetAttribute(payload, ref attribute);
                //if (nReVal != SciCam.SCI_CAMERA_OK || !attribute.isComplete)
                //{
                //    Console.WriteLine("Failed to get image attributes or frame is incomplete! nRet:{0}", nReVal);
                //}
                //else
                //{
                //    Console.WriteLine("Get One Frame: Width[{0}], Height[{1}], nFrameNum[{2}]",
                //    attribute.imgAttr.width, attribute.imgAttr.height, attribute.frameID);
                //}

                //paramSetting.ImageWidth = (int)attribute.imgAttr.width;
                //paramSetting.ImageHeight = (int)attribute.imgAttr.height;
                //SciCam.SciCamPixelType imgPixelType = attribute.imgAttr.pixelType;
                //switch (imgPixelType)
                //{

                //    case SciCam.SciCamPixelType.Mono8:
                //    case SciCam.SciCamPixelType.Mono8s:
                //        //case IMVDefine.IMV_EPixelType.gvspPixelMono10:
                //        paramSetting.CameraType = PixelFormats.Gray8;
                //        break;
                //    case SciCam.SciCamPixelType.BayerGR8:
                //    case SciCam.SciCamPixelType.BayerRG8:
                //    case SciCam.SciCamPixelType.BayerGB8:
                //    case SciCam.SciCamPixelType.BayerBG8:
                //    case SciCam.SciCamPixelType.RGB8:
                //    case SciCam.SciCamPixelType.BGR8:
                //    case SciCam.SciCamPixelType.RGBa8:
                //    case SciCam.SciCamPixelType.BGRa8:
                //        paramSetting.CameraType = PixelFormats.Rgb24;
                //        break;
                //    default:
                //        paramSetting.CameraType = PixelFormats.Rgb24;
                //        break;
                //}

                //string fileName = string.Format("Device_{0}Image_W{1}_H{2}_fID{3}.bmp", 0, paramSetting.ImageWidth, paramSetting.ImageHeight, attribute.frameID);
                //nReVal = SciCam.PayloadSaveImage(fileName, SciCam.SciCamPixelType.Mono8, payload, (long)paramSetting.ImageWidth, (long)paramSetting.ImageHeight);
                //if (nReVal != SciCam.SCI_CAMERA_OK)
                //{
                //    //ShowMsg("Save bmp image failed", nReVal, index + 1, true);
                //}



                //ImageQueueChannel.Writer.TryWrite(payload);
                //GetConvertedInfo(payload);
                ////像素格式转化
                //ConvertToBGR24(payload);
                ////m_pDstData = payload;
                ////获得指向图像数据的指针
                //if (m_pDstData != IntPtr.Zero)
                //{
                //    ImageQueueChannel.Writer.TryWrite(m_pDstData);
                //}
            }
            catch(Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                       Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                   );
            }
            
        }
        private void OnPayloadReceived(IntPtr payload, IntPtr tag)
        {
            int reVal = GetConvertedInfo(payload);
        }

        private int GetConvertedInfo(IntPtr payload)
        {
            if (payload == IntPtr.Zero)
            {
                return -1;
            }

            startGrabSoft = true;
            grabCount++;
            StringBuilder textBuilder = new StringBuilder("OPT");
            textBuilder.Append(grabCount);
            getImageLogger.Info(textBuilder.ToString());

            SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE payloadAttribute = new SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE();
            uint nReVal = SciCam.PayloadGetAttribute(payload, ref payloadAttribute);
            if (nReVal != SciCam.SCI_CAMERA_OK)
            {
                return -1;
            }

            bool imgIsComplete = payloadAttribute.isComplete;
            SciCam.SciCamPayloadMode payloadMode = payloadAttribute.payloadMode;
            SciCam.SciCamPixelType imgPixelType = payloadAttribute.imgAttr.pixelType;
            ulong imgWidth = payloadAttribute.imgAttr.width;
            ulong imgHeight = payloadAttribute.imgAttr.height;
            ulong framID = payloadAttribute.frameID;

            paramSetting.ImageWidth = (int)payloadAttribute.imgAttr.width;
            paramSetting.ImageHeight = (int)payloadAttribute.imgAttr.height;
            paramSetting.CameraType =
                   imgPixelType == SciCam.SciCamPixelType.Mono8
                       ? PixelFormats.Gray8
                       : PixelFormats.Rgb24;

            if (!imgIsComplete || payloadMode != SciCam.SciCamPayloadMode.SciCam_PayloadMode_2D)
            {
                return -1;
            }

            IntPtr imgData = IntPtr.Zero;
            nReVal = SciCam.PayloadGetImage(payload, ref imgData);
            if (nReVal != SciCam.SCI_CAMERA_OK)
            {
                return -1;
            }

            long destImgSize = 0;
            IntPtr destImg = IntPtr.Zero;  // 使用局部变量，不要用成员变量

            try
            {
                if (imgPixelType == SciCam.SciCamPixelType.Mono1p ||
                    imgPixelType == SciCam.SciCamPixelType.Mono2p ||
                    imgPixelType == SciCam.SciCamPixelType.Mono4p ||
                    imgPixelType == SciCam.SciCamPixelType.Mono8s ||
                    imgPixelType == SciCam.SciCamPixelType.Mono8 ||
                    imgPixelType == SciCam.SciCamPixelType.Mono10 ||
                    imgPixelType == SciCam.SciCamPixelType.Mono10p ||
                    imgPixelType == SciCam.SciCamPixelType.Mono12 ||
                    imgPixelType == SciCam.SciCamPixelType.Mono12p ||
                    imgPixelType == SciCam.SciCamPixelType.Mono14 ||
                    imgPixelType == SciCam.SciCamPixelType.Mono16 ||
                    imgPixelType == SciCam.SciCamPixelType.Mono10Packed ||
                    imgPixelType == SciCam.SciCamPixelType.Mono12Packed ||
                    imgPixelType == SciCam.SciCamPixelType.Mono14p)
                {
                    // 第一次调用获取所需大小
                    nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData,
                        SciCam.SciCamPixelType.Mono8, IntPtr.Zero, ref destImgSize, true, 0);

                    if (nReVal == SciCam.SCI_CAMERA_OK && destImgSize > 0)
                    {
                        destImg = Marshal.AllocHGlobal((int)destImgSize);

                        // 第二次调用实际转换
                        nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData,
                            SciCam.SciCamPixelType.Mono8, destImg, ref destImgSize, true, 0);

                        if (nReVal == SciCam.SCI_CAMERA_OK)
                        {
                            // 创建Bitmap并发送到队列
                            using (Bitmap bitMap = new Bitmap((int)imgWidth, (int)imgHeight,
                                GdiPlus.PixelFormat.Format8bppIndexed))
                            {
                                GdiPlus.BitmapData bitmapData = bitMap.LockBits(
                                    new Rectangle(0, 0, (int)imgWidth, (int)imgHeight),
                                    GdiPlus.ImageLockMode.WriteOnly,
                                    GdiPlus.PixelFormat.Format8bppIndexed);

                                try
                                {
                                    // 需要在项目属性中启用"允许不安全代码"
                                    unsafe
                                    {
                                        byte* srcPtr = (byte*)destImg.ToPointer();
                                        byte* dstPtr = (byte*)bitmapData.Scan0.ToPointer();
                                        Buffer.MemoryCopy(srcPtr, dstPtr, destImgSize, destImgSize);
                                    }
                                   // Marshal.Copy(destImg, bitmapData.Scan0, 0, (int)destImgSize);

                                    // 设置灰度调色板
                                    GdiPlus.ColorPalette palette = bitMap.Palette;
                                    for (int i = 0; i < 256; i++)
                                    {
                                        palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                                    }
                                    bitMap.Palette = palette;
                                }
                                finally
                                {
                                    bitMap.UnlockBits(bitmapData);
                                }

                                // 将Bitmap转换为字节数组或直接发送
                                // 这里建议使用using块确保资源释放
                               // ImageQueueChannel.Writer.TryWrite(destImg);
                                if (!ImageQueueChannel.Writer.TryWrite(destImg))
                                {
                                    CCameraManagement.CamLogger.Error(
                                Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + "图像指针写入队列通道失败！");
                                }
                                _semaphoreSlim.Release(1);
                            }
                        }
                        else
                        {
                            // 转换失败，释放内存
                            Marshal.FreeHGlobal(destImg);
                            destImg = IntPtr.Zero;
                        }
                    }
                }
                else
                {
                    // 彩色图像处理（类似逻辑）
                    nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData,
                        SciCam.SciCamPixelType.RGB8, IntPtr.Zero, ref destImgSize, true, 0);

                    if (nReVal == SciCam.SCI_CAMERA_OK && destImgSize > 0)
                    {
                        destImg = Marshal.AllocHGlobal((int)destImgSize);

                        nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData,
                            SciCam.SciCamPixelType.RGB8, destImg, ref destImgSize, true, 0);

                        if (nReVal == SciCam.SCI_CAMERA_OK)
                        {
                            using (Bitmap bitMap = new Bitmap((int)imgWidth, (int)imgHeight,
                                GdiPlus.PixelFormat.Format24bppRgb))
                            {
                                GdiPlus.BitmapData bitmapData = bitMap.LockBits(
                                    new Rectangle(0, 0, (int)imgWidth, (int)imgHeight),
                                    GdiPlus.ImageLockMode.WriteOnly,
                                    GdiPlus.PixelFormat.Format24bppRgb);

                                try
                                {
                                    // 需要在项目属性中启用"允许不安全代码"
                                    unsafe
                                    {
                                        byte* srcPtr = (byte*)destImg.ToPointer();
                                        byte* dstPtr = (byte*)bitmapData.Scan0.ToPointer();
                                        Buffer.MemoryCopy(srcPtr, dstPtr, destImgSize, destImgSize);
                                    }
                                    //Marshal.Copy(destImg, bitmapData.Scan0, 0, (int)destImgSize);
                                }
                                finally
                                {
                                    bitMap.UnlockBits(bitmapData);
                                }

                               // ImageQueueChannel.Writer.TryWrite(destImg);
                                if (!ImageQueueChannel.Writer.TryWrite(destImg))
                                {
                                    CCameraManagement.CamLogger.Error(
                                Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + "图像指针写入队列通道失败！");
                                }
                                _semaphoreSlim.Release(1);
                            }
                        }
                        else
                        {
                            Marshal.FreeHGlobal(destImg);
                            destImg = IntPtr.Zero;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 发生异常时释放内存
                if (destImg != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(destImg);
                    destImg = IntPtr.Zero;
                }

                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message);
            }
            return 0;
        }


        //private int GetConvertedInfo(IntPtr payload)
        //{
        //    if (payload == IntPtr.Zero)
        //    {
        //        return -1;
        //    }
        //    startGrabSoft = true;
        //    grabCount++;  // 帧计数
        //    StringBuilder textBuilder = new StringBuilder("OPT");
        //    textBuilder.Append(grabCount);
        //    getImageLogger.Info(textBuilder.ToString());

        //    SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE payloadAttribute = new SciCam.SCI_CAM_PAYLOAD_ATTRIBUTE();
        //    uint nReVal = SciCam.PayloadGetAttribute(payload, ref payloadAttribute);
        //    if (nReVal != SciCam.SCI_CAMERA_OK)
        //    {
        //        return -1;
        //    }

        //    bool imgIsComplete = payloadAttribute.isComplete;
        //    SciCam.SciCamPayloadMode payloadMode = payloadAttribute.payloadMode;
        //    SciCam.SciCamPixelType imgPixelType = payloadAttribute.imgAttr.pixelType;
        //    ulong imgWidth = payloadAttribute.imgAttr.width;
        //    ulong imgHeight = payloadAttribute.imgAttr.height;
        //    ulong framID = payloadAttribute.frameID;

        //    paramSetting.ImageWidth = (int)payloadAttribute.imgAttr.width;
        //    paramSetting.ImageHeight = (int)payloadAttribute.imgAttr.height;
        //    paramSetting.CameraType =
        //           imgPixelType == SciCam.SciCamPixelType.Mono8
        //               ? PixelFormats.Gray8
        //               : PixelFormats.Rgb24;

        //    if (!imgIsComplete || payloadMode != SciCam.SciCamPayloadMode.SciCam_PayloadMode_2D)
        //    {
        //        return -1;
        //    }

        //    IntPtr imgData = IntPtr.Zero;
        //    nReVal = SciCam.PayloadGetImage(payload, ref imgData);
        //    if (nReVal != SciCam.SCI_CAMERA_OK)
        //    {
        //        return -1;
        //    }
        //    //ImageQueueChannel.Writer.TryWrite(imgData);
        //    long destImgSize = 0;

        //    if (imgPixelType == SciCam.SciCamPixelType.Mono1p ||

        //        imgPixelType == SciCam.SciCamPixelType.Mono2p ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono4p ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono8s ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono8 ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono10 ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono10p ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono12 ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono12p ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono14 ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono16 ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono10Packed ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono12Packed ||
        //        imgPixelType == SciCam.SciCamPixelType.Mono14p)
        //    {
        //        nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.Mono8, IntPtr.Zero, ref destImgSize, true, 0);
        //        if (nReVal == SciCam.SCI_CAMERA_OK)
        //        {
        //            //IntPtr destImg = Marshal.AllocHGlobal((int)destImgSize);
        //            m_pDstData = Marshal.AllocHGlobal((int)destImgSize);
        //            try
        //            {
        //                nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.Mono8, m_pDstData, ref destImgSize, true, 0);
        //                //nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.Mono8, destImg, ref destImgSize, true, 0);
        //                if (nReVal == SciCam.SCI_CAMERA_OK)
        //                {
        //                    //这里使用 Marshal.Copy 方法将从 destImg 指向的内存位置复制 destImgSize 字节的数据到 bBitmap 字节数组中。
        //                    byte[] bBitmap = new byte[destImgSize];
        //                    //Marshal.Copy(destImg, bBitmap, 0, (int)destImgSize);
        //                    Marshal.Copy(m_pDstData, bBitmap, 0, (int)destImgSize);
        //                    //使用 Bitmap 类的构造函数创建一个新的位图对象，指定宽度 imgWidth 和高度 imgHeight，并指定像素格式为 Format8bppIndexed，即8位灰度图像。
        //                    Bitmap bitMap = new Bitmap((int)imgWidth, (int)imgHeight, GdiPlus.PixelFormat.Format8bppIndexed);
        //                    //使用 LockBits 方法锁定位图的指定区域（整个位图），以便直接访问位图的像素数据。指定了写入模式 (WriteOnly) 和像素格式 (Format8bppIndexed)。
        //                    GdiPlus.BitmapData bitmapData = bitMap.LockBits(new Rectangle(0, 0, (int)imgWidth, (int)imgHeight), GdiPlus.ImageLockMode.WriteOnly, GdiPlus.PixelFormat.Format8bppIndexed);
        //                    //用 Marshal.Copy 方法将 bBitmap 字节数组中的数据复制到位图的像素数据 (bitmapData.Scan0) 中。bitmapData.Scan0 是位图数据的起始地址。
        //                    Marshal.Copy(bBitmap, 0, bitmapData.Scan0, (int)destImgSize);
        //                    //使用 UnlockBits 方法解锁位图数据，释放对位图数据的访问。
        //                    bitMap.UnlockBits(bitmapData);

        //                    //设置调色板
        //                    GdiPlus.ColorPalette palette = bitMap.Palette;
        //                    for (int i = 0; i < 256; i++)
        //                    {
        //                        palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
        //                    }
        //                    bitMap.Palette = palette;

        //                    //获得指向图像数据的指针
        //                    if (m_pDstData != IntPtr.Zero)
        //                    {
        //                        ImageQueueChannel.Writer.TryWrite(m_pDstData);
        //                        _semaphoreSlim.Release(1);
        //                    }                  
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                CCameraManagement.CamLogger.Error(
        //              Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
        //          );
        //            }
        //        }
        //    }
        //    else
        //    {
        //        nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.RGB8, IntPtr.Zero, ref destImgSize, true, 0);
        //        if (nReVal == SciCam.SCI_CAMERA_OK)
        //        {
        //            //IntPtr destImg = Marshal.AllocHGlobal((int)destImgSize);
        //            m_pDstData = Marshal.AllocHGlobal((int)destImgSize);
        //            try
        //            {
        //                nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.RGB8, m_pDstData, ref destImgSize, true, 0);
        //                //nReVal = SciCam.PayloadConvertImageEx(ref payloadAttribute.imgAttr, imgData, SciCam.SciCamPixelType.RGB8, destImg, ref destImgSize, true, 0);
        //                if (nReVal == SciCam.SCI_CAMERA_OK)
        //                {
        //                    byte[] bBitmap = new byte[destImgSize];
        //                    //Marshal.Copy(destImg, bBitmap, 0, (int)destImgSize);
        //                    Marshal.Copy(m_pDstData, bBitmap, 0, (int)destImgSize);
        //                    Bitmap bitMap = new Bitmap((int)imgWidth, (int)imgHeight, GdiPlus.PixelFormat.Format24bppRgb);
        //                    GdiPlus.BitmapData bitmapData = bitMap.LockBits(new Rectangle(0, 0, (int)imgWidth, (int)imgHeight), GdiPlus.ImageLockMode.WriteOnly, GdiPlus.PixelFormat.Format24bppRgb);
        //                    Marshal.Copy(bBitmap, 0, bitmapData.Scan0, (int)destImgSize);
        //                    bitMap.UnlockBits(bitmapData);

        //                    //获得指向图像数据的指针
        //                    if (m_pDstData != IntPtr.Zero)
        //                    {
        //                        ImageQueueChannel.Writer.TryWrite(m_pDstData);
        //                        _semaphoreSlim.Release(1);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                CCameraManagement.CamLogger.Error(
        //              Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message);
        //            }
        //        }
        //    }
        //    return 0;
        //}

        public override bool GetImageFunc(IntPtr zoo)
        {
            try
            {
               
                return base.GetImageFunc(zoo);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorImageFunc + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }


        public override void CloseCamera()
        {
            try
            {
                if (this.Connected && m_currentDev != null)
                {
                    // ch:停止取流 | en:Stop grab image
                    m_currentDev.StopGrabbing();
                    //if (nReVal != SciCam.SCI_CAMERA_OK)
                    //{
                    //    Console.WriteLine("Stop Grabbing fail! nRet:{0}", nReVal);
                    //    break;
                    //}
                    imageCallback -= OnPayloadReceived;
                  
                    // ch:关闭设备 | en:Close device
                    m_currentDev.CloseDevice();
                    //if (nReVal != SciCam.SCI_CAMERA_OK)
                    //{
                    //    Console.WriteLine("Close Device fail! nRet:{0}", nReVal);
                    //    break;
                    //}

                    // ch:销毁句柄 | en:Destroy handle
                  m_currentDev.DeleteDevice();
                    //if (nReVal != SciCam.SCI_CAMERA_OK)
                    //{
                    //    Console.WriteLine("Delete Device fail! nRet:{0}", nReVal);
                    //    break;
                    //}


                    this.Connected = false;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
            $"关闭奥普特相机序列号:{paramSetting.SerialNumber}出现异常:" + ex.Message);
                throw;
            }
        }
        public override bool StartGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    uint nReVal = m_currentDev.StartGrabbing();
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                           Properties.Resources.ErrorStart + nReVal.ToString()
                       );
                        //m_bThreadState = false;
                        //ShowMsg("Start grabbing failed", nReVal);
                        //return;
                    }
                    else
                    {
                        result = true;
                    }





                }
                return result;

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorStart2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        public override bool StopGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    uint nReVal = m_currentDev.StopGrabbing();
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStop + nReVal.ToString()
                        );
                    }
                    else
                    {
                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorStop2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            try
            {
                int nReVal = (int)SciCam.SCI_CAMERA_OK;
                switch (mode)
                {
                    case EMTRIGGERMODE.EMTRIGGERNONE:

                        nReVal = (int)m_currentDev.SetEnumValueByStringEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerMode", "Off");
                        //if (nReVal != SciCam.SCI_CAMERA_OK)
                        //{
                        //    ShowMsg("Set trigger mode off failed", nReVal);
                        //}
                        //nRet = cam.IMV_SetEnumFeatureSymbol("TriggerMode", "Off");
                        break;
                    case EMTRIGGERMODE.EMTRIGGERSOFTWARE:

                        nReVal = (int)m_currentDev.SetEnumValueByStringEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerMode", "On");
                        if (nReVal == (int)SciCam.SCI_CAMERA_OK)
                        {

                           
                            nReVal = (int)m_currentDev.SetEnumValueByStringEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerSource", "Software");


                        }

                        //if (nReVal != SciCam.SCI_CAMERA_OK)
                        //{
                        //    ShowMsg("Set TriggerSource: Software failed", nReVal);
                        //    checkBox_softwareTrigger.Checked = false;
                        //}

                        break;
                    case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                        nReVal = (int)m_currentDev.SetEnumValueByStringEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerMode", "On");
                        string triggerSourceparam = paramSetting.TriggerSource.ToString();
                        if (nReVal == (int)SciCam.SCI_CAMERA_OK)
                        {
                            
                            nReVal = (int)m_currentDev.SetEnumValueByStringEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerSource", triggerSourceparam );//"Line1"


                        }


                        break;
                }
                if ((int)SciCam.SCI_CAMERA_OK != nReVal)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerMode + nReVal.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetTriggerMode2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            try
            {
                if (this.Connected)
                {


                    base.ExecuteSoftwareTrigger();
                    uint nReVal = m_currentDev.SetCommandValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "TriggerSoftware");
                    //if (SciCam.SCI_CAMERA_OK != nReVal)
                    //{
                    //    ShowMsg("TriggerSoftware  fail! ", nReVal, i + 1, true);
                    //}

                    if (SciCam.SCI_CAMERA_OK != nReVal)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorSoftWare + nReVal.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSoftWare2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        public override bool GetExposureTime(out uint value)
        {
            try
            {

                double exposureTime = 0;
                uint nReVal = GetOPTExposureTime(ref exposureTime);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetExposureTime + nReVal.ToString()
                    );
                    return false;
                   
                    //ShowMsg("Get ExposureTime failed", nReVal);
                }
                else
                {
                    value = (uint)exposureTime;
                    return true;
                }

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetExposureTime2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }
        private uint GetOPTExposureTime(ref double exposureTime)
        {
            //double exposureTime = 0;
            string[] nodeName = new string[]
            {
                "ExposureTime",
                "ExposureTimeAbs",
                "ExposureTimeRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        exposureTime = fNodeVal.dVal;
                        //textBox_exposure.Text = fNodeVal.dVal.ToString();
                        break;
                    }
                }
                else
                {
                    exposureTime = iNodeVal.nVal;
                    //textBox_exposure.Text = iNodeVal.nVal.ToString();
                    break;
                }
            }

            return nReVal;
        }


        public override void SetExposureTime(uint value)
        {
            try
            {

                uint nReVal = SetOPTExposureTime(value);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetExposureTime + nReVal.ToString()
                    );
                    //ShowMsg("Set ExposureTime failed", nReVal);
                }



            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetExposureTime2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }
        private uint SetOPTExposureTime(uint value)
        {
            string[] nodeName = new string[]
            {
                "ExposureTime",
                "ExposureTimeAbs",
                "ExposureTimeRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            int iExposure = (int)value;
            //int iExposure = 0;
            //bool success = int.TryParse(textBox_exposure.Text, out iExposure);
            //if (success)
            //{
            for (int i = 0; i < nodeName.Count(); i++)
            {
                nReVal = m_currentDev.SetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], iExposure);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    double dExposure = (double)value;
                    //double dExposure = 0;
                    //success = double.TryParse(textBox_exposure.Text, out dExposure);
                    //if (success)
                    //{
                    nReVal = m_currentDev.SetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], dExposure);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        break;
                    }
                    //}
                }
                else
                {
                    break;
                }
            }
            //}

            return nReVal;
        }

        public override bool GetGain(out float value)
        {
            try
            {
                double gain = 0;
                uint nReVal = GetOPTGain(ref gain);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain + nReVal.ToString()
                    );
                    return false;
                    //ShowMsg("Get Gain failed", nReVal);
                }
                else
                {
                    value = (float)gain;
                    return true;
                }

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGain2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }
        private uint GetOPTGain(ref double gain)
        {
            string[] nodeName = new string[]
            {
                "Gain",
                "GainRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        gain = fNodeVal.dVal;
                        break;
                    }
                }
                else
                {
                    gain = iNodeVal.nVal;
                    break;
                }
            }

            return nReVal;
        }

        public override void SetGain(float value)
        {
            try
            {
                double gain = 0;
                uint nReVal = SetOPTGain(value);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGain + nReVal.ToString()
                    );
                    //ShowMsg("Set Gain failed", nReVal);
                }
                //int res = cam.IMV_SetDoubleFeatureValue("GainRaw", Value);
                //if (res != IMVDefine.IMV_OK)
                //{
                //    CCameraManagement.CamLogger.Error(
                //        Properties.Resources.ErrorSetGain + res.ToString()
                //    );
                //}
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGain2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }
        private uint SetOPTGain(float value)
        {
            string[] nodeName = new string[]
            {
                "Gain",
                "GainRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;



            int iGain = 0;
            iGain = (int)value;
            //bool success = int.TryParse(textBox_gain.Text, out iGain);
            //if (success)
            //{
            for (int i = 0; i < nodeName.Count(); i++)
            {
                nReVal = m_currentDev.SetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], iGain);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    double dGain = 0;
                    dGain = (double)value;
                    //success = double.TryParse(textBox_gain.Text, out dGain);
                    //if (success)
                    //{
                    nReVal = m_currentDev.SetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], dGain);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        break;
                    }
                    //}
                }
            }
            //}

            return nReVal;
        }

      

        /// <summary>
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
            paramSetting.TriggerSource = paramSetting.TriggerSource;
        }

        /// <summary>
        /// 2026.4.13 GWD
        /// 设置自定义参数
        /// </summary>
        /// <param name="value">自定义参数类别 看EMCUSTOMPARAMTYPE</param>
        public override void SetCustomParam(uint value)
        {
            switch ((EMCUSTOMPARAMTYPE)value)
            {
                case EMCUSTOMPARAMTYPE.EMPARAMTRIGGERSOURCE:
                    {
                        if (paramSetting.TriggerMode == EMTRIGGERMODE.EMTRIGGERHARDWARE)
                        {
                            try
                            {
                                //int res = cam.IMV_GetIntFeatureValue("TriggerSource", ref (long)paramSetting.TriggerSource);
                                //int nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                                //    "TriggerSource",
                                //    (uint)paramSetting.TriggerSource

                                //if (IMVDefine.IMV_OK != res)
                                //{
                                //    CCameraManagement.CamLogger.Error(
                                //        Properties.Resources.ErrorSetTriggerSource + res.ToString()
                                //    );
                                //}

                                //double triggerDelayTime = 0;
                                //uint nReVal = GetOPTTriggerDelay(ref triggerDelayTime);
                                //if (nReVal != SciCam.SCI_CAMERA_OK)
                                //{
                                //    value = 0;
                                //    CCameraManagement.CamLogger.Error(
                                //        Properties.Resources.ErrorGetTriggerDelay + nReVal.ToString()
                                //    );
                                //    return false;
                                //    ShowMsg("Get ExposureTime failed", nReVal);
                                //}
                                //else
                                //{
                                //    value = (uint)triggerDelayTime;
                                //    return true;

                                //}
                            }
                            catch (Exception ex)
                            {
                                CCameraManagement.CamLogger.Error(Properties.Resources.ErrorSetTriggerSource2+ paramSetting.SerialNumber+ ex.Message);
                            }
                        }
                    }
                    break;
            }

        }
        private uint SetOPTTriggerSource(uint value)
        {
            string[] nodeName = new string[]
            {
                "TriggerSource",
                "TriggerSourceAbs",
                "TriggerSourceRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            int iTriggerDelayTime = (int)value;
            //int iExposure = 0;
            //bool success = int.TryParse(textBox_exposure.Text, out iExposure);
            //if (success)
            //{
            for (int i = 0; i < nodeName.Count(); i++)
            {
                nReVal = m_currentDev.SetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], iTriggerDelayTime);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    double dTriggerDelayTime = (double)value;
                    //double dExposure = 0;
                    //success = double.TryParse(textBox_exposure.Text, out dExposure);
                    //if (success)
                    //{
                    nReVal = m_currentDev.SetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], dTriggerDelayTime);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        break;
                    }
                    //}
                }
                else
                {
                    break;
                }
            }
            //}

            return nReVal;
        }

        public override bool GetTriggerDelay(out uint value)
        {
            try
            {

                double triggerDelayTime = 0;
                uint nReVal = GetOPTTriggerDelay(ref triggerDelayTime);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerDelay + nReVal.ToString()
                    );
                    return false;
                    //ShowMsg("Get ExposureTime failed", nReVal);
                }
                else
                {
                    value = (uint)triggerDelayTime;
                    return true;
                    
                }





              
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerDelay2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }
        private uint GetOPTTriggerDelay(ref double triggerDelayTime)
        {
            string[] nodeName = new string[]
            {
                "TriggerDelay",
                "TriggerDelayRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        triggerDelayTime = fNodeVal.dVal;
                        break;
                    }
                }
                else
                {
                    triggerDelayTime = iNodeVal.nVal;
                    break;
                }
            }

            return nReVal;
        }

        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            try
            {
                

                SciCam.SCI_NODE_VAL_ENUM eNodeVal = new SciCam.SCI_NODE_VAL_ENUM();

                uint nReVal = m_currentDev.GetEnumValue("TriggerMode", ref eNodeVal);
                string triggerMode = "Off";
                string triggerSource = "Software";
                if (nReVal == SciCam.SCI_CAMERA_OK)
                {

                    for (int i = 0; i < eNodeVal.itemCount; i++)
                    {

                        if (eNodeVal.nVal == eNodeVal.items[i].val)
                        {
                            triggerMode = eNodeVal.items[i].desc;
                            break;

                        }

                    }

                    if (triggerMode == "Off")
                    {

                        mode = EMTRIGGERMODE.EMTRIGGERNONE;
                        return true;

                    }
                    else
                    {

                        nReVal = m_currentDev.GetEnumValue("TriggerSource", ref eNodeVal);
                        if (nReVal == SciCam.SCI_CAMERA_OK)
                        {
                            for (int i = 0; i < eNodeVal.itemCount; i++)
                            {

                                if (eNodeVal.nVal == eNodeVal.items[i].val)
                                {
                                    triggerSource = eNodeVal.items[i].desc;
                                    break;

                                }

                            }
                            if (triggerSource == "Software")
                            {

                                mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                                

                            }
                            else
                            {
                                mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            }
                            return true;
                        }
                    }



                }
                mode = EMTRIGGERMODE.EMTRIGGERNONE;
                return true;

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetTriggerMode2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        public override void SetStrobeEnable(bool enable)
        {
            throw new NotImplementedException();
        }
        public override void SetLineSelector(object line)
        {
            throw new NotImplementedException();
        }
        public override void SetStrobeDuration(uint value)
        {
            throw new NotImplementedException();
        }
        public override void SetLineSource(object source)
        {
            throw new NotImplementedException();
        }
        public override void SetLineInverter(bool enable)
        {
            throw new NotImplementedException();
        }
        public override void SetLineMode(object lineMode)
        {
            throw new NotImplementedException();
        }
        public override void LineTriggerSoftware()
        {
            throw new NotImplementedException();
        }
        public override void SetGammaEnable(bool enable)
        {
            throw new NotImplementedException();
        }
        public override float GetFps()
        {
            throw new NotImplementedException();
        }


        public override void UserLoadParam()
        {


        }
        public override void UserSaveParam()
        {
            try
            {
                if (this.Connected)
                {
                    uint nReVal = m_currentDev.FeatureSaveEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "FeatureFile.xml");
                    Thread.Sleep(50);
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                           Properties.Resources.ErrorUserSaveParam + nReVal.ToString()
                       );
                        //Console.WriteLine("Save Feature fail! nRet {0}]", nReVal);
                        //break;
                    }


                    ////1、选择当前配置为UserSet1
                    ////1、Select the UserSet1 configuration as the current configuration
                    //int res = cam.IMV_SetEnumFeatureSymbol("UserSetSelector", "UserSet1");


                    ////2、保存配置到UserSet1
                    ////2、Save configuration to UserSet1
                    //res = cam.IMV_ExecuteCommandFeature("UserSetSave");

                    //Thread.Sleep(50);

                    //if (IMVDefine.IMV_OK != res)
                    //{
                    //    CCameraManagement.CamLogger.Error(
                    //        Properties.Resources.ErrorUserSaveParam + res.ToString()
                    //    );
                    //}
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorUserSaveParam2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }

        public override bool GetImageWidth(out int value)
        {
              try
                {
                    int imageWidth = 0;
                    uint nReVal = GetOPTImageWidth(ref imageWidth);
                    if (nReVal != SciCam.SCI_CAMERA_OK)
                    {
                        value = 1000;
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorGetWidth + nReVal.ToString()
                        );
                        return false;
                        //ShowMsg("Get Gain failed", nReVal);
                    }
                    else
                    {
                        value = imageWidth;
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetWidth2 + paramSetting.SerialNumber + ex.Message
                );
                    value = 1000;
                    return false;
                }
           
        }
        private uint GetOPTImageWidth(ref int imageWidth)
        {
            string[] nodeName = new string[]
            {
                "Width",
                "WidthRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        imageWidth = (int)fNodeVal.dVal;
                        break;
                    }
                }
                else
                {
                    imageWidth = (int)iNodeVal.nVal;
                    break;
                }
            }

            return nReVal;
        }

        public override bool GetImageHeight(out int value)
        {
            try
            {
                int imageHeight = 0;
                uint nReVal = GetOPTImageHeigth(ref imageHeight);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    value = 1000;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetHeight + nReVal.ToString()
                    );
                    return false;
                    //ShowMsg("Get Gain failed", nReVal);
                }
                else
                {
                    value = imageHeight;
                    return true;
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetHeight2 + paramSetting.SerialNumber + ex.Message
                );
                value = 1000;
                return false;
            }
        }
        private uint GetOPTImageHeigth(ref int imageHeight)
        {
            string[] nodeName = new string[]
            {
                "Height",
                "HeightRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        imageHeight = (int)fNodeVal.dVal;
                        break;
                    }
                }
                else
                {
                    imageHeight = (int)iNodeVal.nVal;
                    break;
                }
            }

            return nReVal;
        }

        public override bool GetCameraType(out PixelFormat cameraType)
        {
            try
            {
                SciCam.SCI_NODE_VAL_ENUM eNodeVal = new SciCam.SCI_NODE_VAL_ENUM();
                
                uint nReVal = m_currentDev.GetEnumValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, "PixelFormat",ref eNodeVal);
                //uint nReVal = m_currentDev.GetEnumValue("PixelFormat", ref eNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    CCameraManagement.CamLogger.Error(
                      Properties.Resources.ErrorGetCamType + nReVal.ToString()
                  );


                    cameraType = PixelFormats.Gray8;
                    return false;
                }
                else
                {
                    //int currentIndex = 0;

                    //for (int i = 0; i < eNodeVal.itemCount; i++)
                    //{

                    //    string itemStr = eNodeVal.items[i].desc;

                    //    //comboBox_pixelFormat.Items.Add(itemStr);
                    //    //cameraType = nEntryNum == 0x01080001 ? PixelFormats.Gray8 : PixelFormats.Rgb24;
                    //    if (itemStr == "Mono8")
                    //    {
                    //        cameraType = PixelFormats.Gray8;
                    //    }
                    //    else
                    //    {
                    //        cameraType = PixelFormats.Rgb24;
                           
                    //    }
                       

                    //}
                    cameraType =
                       eNodeVal.nVal == 17301505 ? PixelFormats.Gray8 : PixelFormats.Rgb24;
                    return true;
                   
                }
            }
            catch (Exception ex)

            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetCamType2 + paramSetting.SerialNumber + ex.Message
                );
                cameraType = PixelFormats.Gray8;
                return false;
            }
        }


        public override void SetFrameCount(int count)
        {

        }

        public override void SetGamma(float value)
        {
            try
            {
                double gamma = 0;
                uint nReVal = SetOPTGamma(value);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGamma + nReVal.ToString()
                    );
                    //ShowMsg("Set Gain failed", nReVal);
                }
                //int res = cam.IMV_SetDoubleFeatureValue("GainRaw", Value);
                //if (res != IMVDefine.IMV_OK)
                //{
                //    CCameraManagement.CamLogger.Error(
                //        Properties.Resources.ErrorSetGain + res.ToString()
                //    );
                //}
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGamma2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }
        private uint SetOPTGamma(float value)
        {
            string[] nodeName = new string[]
            {
                "Gamma",
                "GammaRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;



            int iGamma = 0;
            iGamma = (int)value;
            //bool success = int.TryParse(textBox_gain.Text, out iGain);
            //if (success)
            //{
            for (int i = 0; i < nodeName.Count(); i++)
            {
                nReVal = m_currentDev.SetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], iGamma);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    double dGamma = 0;
                    dGamma = (double)value;
                    //success = double.TryParse(textBox_gain.Text, out dGain);
                    //if (success)
                    //{
                    nReVal = m_currentDev.SetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], dGamma);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        break;
                    }
                    //}
                }
            }
            //}

            return nReVal;
        }
        public override bool GetGamma(out float value)
        {
            try
            {
                double gamma = 0;
                uint nReVal = GetOPTGamma(ref gamma);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGamma + nReVal.ToString()
                    );
                    return false;
                    //ShowMsg("Get Gain failed", nReVal);
                }
                else
                {
                    value = (float)gamma;
                    return true;
                }

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorGetGamma2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }
        private uint GetOPTGamma(ref double gamma)
        {
            string[] nodeName = new string[]
            {
                "Gamma",
                "GammaRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            for (int i = 0; i < nodeName.Count(); i++)
            {
                SciCam.SCI_NODE_VAL_INT iNodeVal = new SciCam.SCI_NODE_VAL_INT();
                nReVal = m_currentDev.GetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref iNodeVal);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    SciCam.SCI_NODE_VAL_FLOAT fNodeVal = new SciCam.SCI_NODE_VAL_FLOAT();
                    nReVal = m_currentDev.GetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], ref fNodeVal);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        gamma = fNodeVal.dVal;
                        break;
                    }
                }
                else
                {
                    gamma = iNodeVal.nVal;
                    break;
                }
            }

            return nReVal;
        }



        

        public override void SetTriggerDelay(uint value)
        {
            try
            {
                //uint nReVal = m_currentDev.SetGrabTimeout(value);
                uint nReVal = SetOPTTriggerDelayTime(value);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    CCameraManagement.CamLogger.Error(
                       Properties.Resources.ErrorSetTriggerDelay + nReVal.ToString()
                   );
                    //Console.WriteLine("SetGrabTimeout failed, return error code: {0}", nReVal);

                    //return;

                }
                //else
                //{

                //    Console.WriteLine("SetGrabTimeout success.");

                //}


            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetTriggerDelay2
                        + paramSetting.SerialNumber
                        + ex.Message
                );
                throw;
            }
        }
        private uint SetOPTTriggerDelayTime(uint value)
        {
            string[] nodeName = new string[]
            {
                "TriggerDelay",
                "TriggerDelayAbs",
                "TriggerDelayRaw"
            };

            uint nReVal = SciCam.SCI_CAMERA_OK;
            int iTriggerDelayTime = (int)value;
            //int iExposure = 0;
            //bool success = int.TryParse(textBox_exposure.Text, out iExposure);
            //if (success)
            //{
            for (int i = 0; i < nodeName.Count(); i++)
            {
                nReVal = m_currentDev.SetIntValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], iTriggerDelayTime);
                if (nReVal != SciCam.SCI_CAMERA_OK)
                {
                    double dTriggerDelayTime = (double)value;
                    //double dExposure = 0;
                    //success = double.TryParse(textBox_exposure.Text, out dExposure);
                    //if (success)
                    //{
                    nReVal = m_currentDev.SetFloatValueEx(SciCam.SciCamDeviceXmlType.SciCam_DeviceXml_Camera, nodeName[i], dTriggerDelayTime);
                    if (nReVal == SciCam.SCI_CAMERA_OK)
                    {
                        break;
                    }
                    //}
                }
                else
                {
                    break;
                }
            }
            //}

            return nReVal;
        }
        public override void SetTriggerPulseWidth(uint value)
        {
            
        }

        public override bool GetTriggerPulseWidth(out uint value)
        {
            value = 0;
            return true;
        }



    }
}
