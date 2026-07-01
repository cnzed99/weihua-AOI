using CameraModule;
using MVSDK_Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using WH.Entity;


namespace DaHuaCam
{
    public class CDaHuaCamera : CCameraBase
    {
        /// <summary>
        /// 2025.06.26 GWD
        /// 相机参数
        /// </summary>
        internal CDaHuaParameterSetting paramSetting { get; set; }

        public CDaHuaCamera()
            : base() { }

        /// <summary>
        /// 相机对象
        /// </summary>
        MyCamera cam = new MyCamera();
        List<IMVDefine.IMV_Frame> m_frameList = new List<IMVDefine.IMV_Frame>(); // 图像缓存列
        private IntPtr m_pDstData;
        private int m_nDataLenth = 0;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 相机回调
        /// </summary>
        private IMVDefine.IMV_FrameCallBack frameCallBack;


        public override bool OpenCamera()
        {
            try
            {

                //创建对象deviceList，用于存储设备列表。
                IMVDefine.IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();
                //interfaceTypeAll表示搜索所有类型的接口设备。
                IMVDefine.IMV_EInterfaceType interfaceTp = IMVDefine.IMV_EInterfaceType.interfaceTypeAll;
                int res = MyCamera.IMV_EnumDevices(ref deviceList, (uint)interfaceTp);

                //判断枚举设备是否为0
                if (res != IMVDefine.IMV_OK || deviceList.nDevNum == 0)
                {
                    throw new Exception(Properties.Resources.ErrorEnumCam1);
                }

                // 遍历设备寻找匹配序列号的相机
                for (int i = 0; i < deviceList.nDevNum; i++)
                {
                    IMVDefine.IMV_DeviceInfo deviceInfo =
                        (IMVDefine.IMV_DeviceInfo)
                            Marshal.PtrToStructure(
                                deviceList.pDevInfo + Marshal.SizeOf(typeof(IMVDefine.IMV_DeviceInfo)) * i,
                                typeof(IMVDefine.IMV_DeviceInfo));


                    // 检查厂商和序列号
                    if ( deviceInfo.serialNumber != paramSetting.SerialNumber)
                    {
                        continue;
                    }

                    // 创建设备句柄
                    res = cam.IMV_CreateHandle(IMVDefine.IMV_ECreateHandleMode.modeByIndex, i);
                    if (res != IMVDefine.IMV_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                        );
                        return false;
                    }


                    // 打开设备 
                    // open device 
                    res = cam.IMV_Open();
                    if (res != IMVDefine.IMV_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                        );
                        cam.IMV_DestroyHandle();
                        return false;
                    }


                    // 注册数据帧回调函数
                    // Register data frame callback function
                    frameCallBack = new IMVDefine.IMV_FrameCallBack(ImageCallbackFunc);
                    res = cam.IMV_AttachGrabbing(frameCallBack, IntPtr.Zero);
                    if (res != IMVDefine.IMV_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                              Properties.Resources.ErrorInitCam1 + paramSetting.SerialNumber + ""
                        );
                    }
                    this.Connected = true;
                    return StartGrab();
                }

                // 没有找到匹配的相机
                CCameraManagement.CamLogger.Error(
                    $"未找到匹配序列号{paramSetting.SerialNumber}的大华相机");
                return false;
                //return false;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                );
                return false;

            }
        }


        // 相机打开回调 
        // camera open event callback 
        public void OnCameraOpen(object sender, EventArgs e)
        {
        }


        // 相机关闭回调 
        // camera close event callback 
        public void OnCameraClose(object sender, EventArgs e)
        {
        }




        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="pData">图像数据</param>
        /// <param name="pFrameInfo">图像信息</param>
        /// <param name="pUser">上下文信息</param>

        float _grabCount = 0;
        float _onceCount = 0;
        int inqueue = 0; //回调里入队列计数


        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="pData">图像数据</param>
        /// <param name="pFrameInfo">图像信息</param>
        /// <param name="pUser">上下文信息</param>
        public void ImageCallbackFunc(ref IMVDefine.IMV_Frame frame, IntPtr pUser)
        {

            try
            {

                startGrabSoft = true;
                grabCount++;  // 帧计数
                StringBuilder textBuilder = new StringBuilder("DaHua");
                textBuilder.Append(grabCount);
                getImageLogger.Info(textBuilder.ToString());

                //mutex.WaitOne();// 等待互斥锁
                //imageQueue.Enqueue(pData);// 将图像数据加入队列
                //mutex.ReleaseMutex();// 释放互斥锁
                paramSetting.ImageWidth = (int)frame.frameInfo.width;
                paramSetting.ImageHeight = (int)frame.frameInfo.height;

                //2025.10.11修改
                switch (frame.frameInfo.pixelFormat)
                {

                    case IMVDefine.IMV_EPixelType.gvspPixelMono8:
                    case IMVDefine.IMV_EPixelType.gvspPixelMono8S:
                        //case IMVDefine.IMV_EPixelType.gvspPixelMono10:
                        paramSetting.CameraType = PixelFormats.Gray8;
                        break;
                    case IMVDefine.IMV_EPixelType.gvspPixelBayGR8:
                    case IMVDefine.IMV_EPixelType.gvspPixelBayRG8:
                    case IMVDefine.IMV_EPixelType.gvspPixelBayGB8:
                    case IMVDefine.IMV_EPixelType.gvspPixelBayBG8:
                    case IMVDefine.IMV_EPixelType.gvspPixelRGB8:
                    case IMVDefine.IMV_EPixelType.gvspPixelBGR8:
                    case IMVDefine.IMV_EPixelType.gvspPixelRGBA8:
                    case IMVDefine.IMV_EPixelType.gvspPixelBGRA8:
                        paramSetting.CameraType = PixelFormats.Rgb24;
                        break;
                    default:
                        paramSetting.CameraType = PixelFormats.Rgb24;
                        break;
                }

                //图片转化


                //像素格式转化
                ConvertToBGR24(frame);

                //获得指向图像数据的指针
                if (m_pDstData != IntPtr.Zero)
                {
                    if(!ImageQueueChannel.Writer.TryWrite(m_pDstData))
                    {
                        CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + "图像指针写入队列通道失败！");
                    }
                    _semaphoreSlim.Release(1);
                }

            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
            }
        }


        private bool ConvertToBGR24(IMVDefine.IMV_Frame frame)
        {
            //用于存储像素格式转换的参数
            IMVDefine.IMV_PixelConvertParam stPixelConvertParam = new IMVDefine.IMV_PixelConvertParam();

            //当内存申请失败，返回false
            try
            {
                if (m_pDstData == IntPtr.Zero || (int)(frame.frameInfo.width * frame.frameInfo.height * 3) > m_nDataLenth)
                {
                    if (m_pDstData != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_pDstData);
                    }
                    m_pDstData = Marshal.AllocHGlobal((int)(frame.frameInfo.width * frame.frameInfo.height * 3));
                    m_nDataLenth = (int)(frame.frameInfo.width * frame.frameInfo.height * 3);
                }
            }
            catch
            {
                return false;
            }

            // 图像转换成BGR8
            // convert image to BGR8
            //设置 stPixelConvertParam 的各个字段，指定源图像的宽度、高度、像素格式、数据指针、数据长度、填充以及目标图像的格式等信息
            stPixelConvertParam.nWidth = frame.frameInfo.width;
            stPixelConvertParam.nHeight = frame.frameInfo.height;
            stPixelConvertParam.ePixelFormat = frame.frameInfo.pixelFormat;
            stPixelConvertParam.pSrcData = frame.pData;
            stPixelConvertParam.nSrcDataLen = frame.frameInfo.size;
            stPixelConvertParam.nPaddingX = frame.frameInfo.paddingX;
            stPixelConvertParam.nPaddingY = frame.frameInfo.paddingY;
            stPixelConvertParam.eBayerDemosaic = IMVDefine.IMV_EBayerDemosaic.demosaicBilinear;
            //stPixelConvertParam.eDstPixelFormat = IMVDefine.IMV_EPixelType.gvspPixelBGR8;
            stPixelConvertParam.eDstPixelFormat = IMVDefine.IMV_EPixelType.gvspPixelRGB8;

            stPixelConvertParam.pDstBuf = m_pDstData;
            stPixelConvertParam.nDstBufSize = (uint)m_nDataLenth;

            int res = cam.IMV_PixelConvert(ref stPixelConvertParam);
            if (res != IMVDefine.IMV_OK)
            {
                // 转码出错,返回false           
                return false;
            }

            return true;
        }


        //图像转化
        private void imageConvert(IMVDefine.IMV_Frame frame, int format)
        {
            IMVDefine.IMV_PixelConvertParam stPixelConvertParam = new IMVDefine.IMV_PixelConvertParam();
            uint nDstBufSize = 0;
            string pConvertFormatStr = "";
            string pFileName = "";
            IMVDefine.IMV_EPixelType convertFormat = IMVDefine.IMV_EPixelType.gvspPixelMono8;
            switch (format)
            {
                case 0:
                    nDstBufSize = frame.frameInfo.width * frame.frameInfo.height;
                    pConvertFormatStr = "Mono8";
                    pFileName = "convertMono8.bin";
                    break;
                case 1:
                    nDstBufSize = frame.frameInfo.width * frame.frameInfo.height * 3;
                    pConvertFormatStr = "RGB8";
                    pFileName = "convertRGB8.bin";
                    convertFormat = IMVDefine.IMV_EPixelType.gvspPixelRGB8;
                    break;
                case 2:
                    nDstBufSize = frame.frameInfo.width * frame.frameInfo.height * 3;
                    pConvertFormatStr = "BGR8";
                    pFileName = "convertBGR8.bin";
                    convertFormat = IMVDefine.IMV_EPixelType.gvspPixelBGR8;
                    break;
                case 3:
                    nDstBufSize = frame.frameInfo.width * frame.frameInfo.height * 4;
                    pConvertFormatStr = "BGRA8";
                    pFileName = "convertBGRA8.bin";
                    convertFormat = IMVDefine.IMV_EPixelType.gvspPixelBGRA8;
                    break;
            }
            IntPtr pDstBuf = Marshal.AllocHGlobal((int)nDstBufSize);

            stPixelConvertParam.nWidth = frame.frameInfo.width;
            stPixelConvertParam.nHeight = frame.frameInfo.height;
            stPixelConvertParam.ePixelFormat = frame.frameInfo.pixelFormat;
            stPixelConvertParam.pSrcData = frame.pData;
            stPixelConvertParam.nSrcDataLen = frame.frameInfo.size;
            stPixelConvertParam.nPaddingX = frame.frameInfo.paddingX;
            stPixelConvertParam.nPaddingY = frame.frameInfo.paddingY;
            stPixelConvertParam.eBayerDemosaic = IMVDefine.IMV_EBayerDemosaic.demosaicNearestNeighbor;
            stPixelConvertParam.eDstPixelFormat = convertFormat;
            stPixelConvertParam.pDstBuf = pDstBuf;
            stPixelConvertParam.nDstBufSize = nDstBufSize;

            int res = cam.IMV_PixelConvert(ref stPixelConvertParam);
            if (res == IMVDefine.IMV_OK)
            {
                Console.WriteLine("image convert to {0} successfully! nDstDataLen {1}", pConvertFormatStr,
                    stPixelConvertParam.nDstBufSize);

                SaveToBin(pDstBuf, pFileName, (int)nDstBufSize);
            }
            else
            {
                Console.WriteLine("image convert to {0} failed! ErrorCode[{1}]", pConvertFormatStr, res);
            }

            if (pDstBuf != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pDstBuf);
                pDstBuf = IntPtr.Zero;
            }
        }

        private static bool SaveToBin(IntPtr pSource, string path, int size)
        {
            byte[] pBuffer = new byte[size];
            Marshal.Copy(pSource, pBuffer, 0, size);
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    using (BinaryWriter sw = new BinaryWriter(stream)) //建立二进制文件流
                    {
                        sw.Write(pBuffer);
                    }
                }
                Console.WriteLine("Save bin Successfully!Save path is [{0}]", path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return false;
            }
            return true;
        }






        /// <summary>
        /// 获取图像数据
        /// </summary>
        /// <param name="zoo">图像数据</param>
        /// <returns></returns>
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
                if (this.Connected && cam != null)
                {
                    cam.IMV_StopGrabbing();
                    frameCallBack -= ImageCallbackFunc;
                    cam.IMV_ClearFrameBuffer();
                    cam.IMV_Close();
                    cam.IMV_DestroyHandle();
                    //cam = null;
                    this.Connected = false;
                }
            }
            catch(Exception ex)
            {
                CCameraManagement.CamLogger.Error(
            $"关闭大华相机序列号:{paramSetting.SerialNumber}出现异常:" + ex.Message);
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
                    int nRet = cam.IMV_StartGrabbing();
                    if (nRet != IMVDefine.IMV_OK)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStart + nRet.ToString()
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
                    int nRet = cam.IMV_StopGrabbing();
                    if (IMVDefine.IMV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorStop + nRet.ToString()
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



        /// <summary>
        /// 修改相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            try
            {
                int nRet = IMVDefine.IMV_OK;
                switch (mode)
                {
                    case EMTRIGGERMODE.EMTRIGGERNONE:
                        nRet = cam.IMV_SetEnumFeatureSymbol("TriggerMode", "Off");
                        break;
                    case EMTRIGGERMODE.EMTRIGGERSOFTWARE:

                        nRet = cam.IMV_SetEnumFeatureSymbol("TriggerMode", "On");
                        if (nRet == IMVDefine.IMV_OK)
                        {
                            nRet = cam.IMV_SetEnumFeatureSymbol("TriggerSource", "Software");

                        }
                        break;
                    case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                        nRet = cam.IMV_SetEnumFeatureSymbol("TriggerMode", "On");
                        if (nRet == IMVDefine.IMV_OK)
                        {
                            nRet = cam.IMV_SetEnumFeatureSymbol(
                                "TriggerSource",
                                "Line1");

                        }


                        break;
                }
                if (IMVDefine.IMV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerMode + nRet.ToString()
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
                    //int nRet = cam.IMV_SetEnumFeatureSymbol("TriggerSource", "Software");
                    int nRet = cam.IMV_ExecuteCommandFeature("TriggerSoftware");
                    if (IMVDefine.IMV_OK != nRet)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorSoftWare + nRet.ToString()
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


        public int GetExposureTimeMin()
        {
            int min = 0;
            return min;
        }

        public int GetExposureTimeMax()
        {
            int max = 0;
            return max;
        }


        /// <summary>
        /// 
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        /// 
        public override bool GetExposureTime(out uint value)
        {
            try
            {

                double exposureTime = 0;
                int res = cam.IMV_GetDoubleFeatureValue("ExposureTime", ref exposureTime);
                if (IMVDefine.IMV_OK == res)
                {
                    value = (uint)exposureTime;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetExposureTime + res.ToString()
                    );
                    return false;
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



        /// <summary>
        /// 
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            try
            {
                int res = cam.IMV_SetDoubleFeatureValue("ExposureTime", (float)value);
                if (IMVDefine.IMV_OK != res)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetExposureTime + res.ToString()
                    );
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


        /// <summary>
        /// 
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            try
            {
                double gain = 0;
                int res = cam.IMV_GetDoubleFeatureValue("GainRaw", ref gain);
                if (IMVDefine.IMV_OK == res)
                {
                    value = (float)gain;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain + res.ToString()
                    );
                    return false;
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

        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="Value"></param>
        public override void SetGain(float Value)
        {
            try
            {

                int res = cam.IMV_SetDoubleFeatureValue("GainRaw", Value);
                if (res != IMVDefine.IMV_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGain + res.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetGain2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }


        /// <summary>
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value"></param>
        public override void SetTriggerDelay(uint value)
        {
            try
            {
                int res = cam.IMV_SetDoubleFeatureValue("TriggerDelay", value);

                if (IMVDefine.IMV_OK != res)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerDelay + res.ToString()
                    );
                }
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






        /// <summary>
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
            paramSetting.TriggerSource = paramSetting.TriggerSource;
        }


        /// <summary>
        /// 2024.6.30 GWD
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
                                //int res =cam.IMV_GetIntFeatureValue("TriggerSource", ref (long)paramSetting.TriggerSource);
                                //int nRet = m_MyCamera.MV_CC_SetEnumValue_NET(
                                //    "TriggerSource",
                                //    (uint)paramSetting.TriggerSource

                                //if (IMVDefine.IMV_OK != res)
                                //{
                                //    CCameraManagement.CamLogger.Error(
                                //        Properties.Resources.ErrorSetTriggerSource + res.ToString()
                                //    );
                                //}
                            }
                            catch (Exception ex)
                            {
                                CCameraManagement.CamLogger.Error(
                                    Properties.Resources.ErrorSetTriggerSource2
                                        + paramSetting.SerialNumber
                                        + ex.Message
                                );
                            }
                        }
                    }
                    break;
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


        public override void UserLoadParam() { }



        public override void UserSaveParam()
        {
            try
            {
                if (this.Connected)
                {
                    //1、选择当前配置为UserSet1
                    //1、Select the UserSet1 configuration as the current configuration
                    int res = cam.IMV_SetEnumFeatureSymbol("UserSetSelector", "UserSet1");


                    //2、保存配置到UserSet1
                    //2、Save configuration to UserSet1
                    res = cam.IMV_ExecuteCommandFeature("UserSetSave");

                    Thread.Sleep(50);

                    if (IMVDefine.IMV_OK != res)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorUserSaveParam + res.ToString()
                        );
                    }
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

        /// <summary>
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>

        public override bool GetImageWidth(out int value)
        {
            try
            {
                Int64 intValue = 0;
                int res = cam.IMV_GetIntFeatureValue("Width", ref intValue);
                if (IMVDefine.IMV_OK != res)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetWidth + res.ToString()
                    );
                    value = 1000;
                    return false;
                }
                else
                {
                    value = (int)intValue;
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

        /// <summary>
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            try
            {
                Int64 intValue = 0;
                int res = cam.IMV_GetIntFeatureValue("Height", ref intValue);
                if (IMVDefine.IMV_OK != res)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetWidth + res.ToString()
                    );
                    value = 1000;
                    return false;
                }
                else
                {
                    value = (int)intValue;
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


        public void GetPixelFormat()
        {
         
        }



        public override void SetFrameCount(int count)
        {

        }


        protected bool IsValidHandle(uint handle)
        {
            return false;

        }

        protected void OpenLight()
        {

        }
        private void CloseLight()
        {

        }

        /// <summary>
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>


        public override bool GetCameraType(out PixelFormat cameraType)
        {
            try
            {
                // 获取所有可以设置的像素格式 
                // get all Image Pixel Format
                uint nEntryNum = 0;
                IMVDefine.IMV_EnumEntryList pixelTypeList = new IMVDefine.IMV_EnumEntryList();
                int res = cam.IMV_GetEnumFeatureEntryNum("PixelFormat", ref nEntryNum);
                if (res != IMVDefine.IMV_OK)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetCamType + res.ToString()
                    );
                    cameraType = PixelFormats.Gray8;
                    return false;
                }
                else
                {
                    cameraType = nEntryNum == 0x01080001 ? PixelFormats.Gray8 : PixelFormats.Rgb24;
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


        /// <summary>
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            try
            {
                IMVDefine.IMV_String triggerMode = new IMVDefine.IMV_String();
                int res = cam.IMV_GetEnumFeatureSymbol("TriggerMode", ref triggerMode);
                if (res == IMVDefine.IMV_OK)
                {
                    if (triggerMode.str == "Off")
                    {
                        mode = EMTRIGGERMODE.EMTRIGGERNONE;
                        return true;
                    }
                    else
                    {
                        res = cam.IMV_GetEnumFeatureSymbol("TriggerSource", ref triggerMode);
                        if (res == IMVDefine.IMV_OK)
                        {
                            if (triggerMode.str == "Software")
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

        /// <summary>
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            try
            {

                int nRet = cam.IMV_SetDoubleFeatureValue("Gamma", (float)value);
                if (IMVDefine.IMV_OK != nRet)
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGamma + nRet.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorSetGamma2 + paramSetting.SerialNumber + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {

            try
            {
                double gamma = 0;
                int res = cam.IMV_GetDoubleFeatureValue("Gamma", ref gamma);
                if (IMVDefine.IMV_OK == res)
                {
                    value = (float)gamma;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain + res.ToString()
                    );
                    return false;
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

        /// <summary>
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        /// 
        public override bool GetTriggerDelay(out uint value)
        {
            try
            {
                double pcFloatValue = 0;
                int nRet = cam.IMV_GetDoubleFeatureValue("TriggerDelay", ref pcFloatValue);
                if (IMVDefine.IMV_OK == nRet)
                {
                    value = (uint)pcFloatValue;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerDelay + nRet.ToString()
                    );
                    return false;
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

        public override void SetTriggerPulseWidth(uint value) { }

        public override bool GetTriggerPulseWidth(out uint value)
        {
            value = 0;
            return true;
        }


    }
}
