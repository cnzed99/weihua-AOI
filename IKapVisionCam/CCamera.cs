using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CameraModule;
using CommunityToolkit.Mvvm.DependencyInjection;
using IKapBoardClassLibrary;
using IKapC.NET;
using Newtonsoft.Json.Linq;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using CameraHandle = System.Int32;

namespace IKapVisionCam
{
    /// <summary>
    ///2025.1.14 李焕彬
    ///缓存图像数据
    /// </summary>
    class CBuffer
    {
        public CBuffer(IntPtr ptr, bool isUsing)
        {
            this.ptr = ptr;
            this.isUsing = isUsing;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像指针
        /// </summary>
        public IntPtr ptr;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 是否正在使用
        /// </summary>
        public bool isUsing = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 编码器位置
        /// </summary>
        public int encoderPos = 0;
    }

    /// <summary>
    /// 2025.1.14 李焕彬
    /// 相机操作派生类
    /// </summary>
    public class CCamera : CCameraBase
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机参数
        /// </summary>
        internal CParameterSetting paramSetting { get; set; }

        public CCamera()
            : base() { }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机设备句柄
        /// </summary>
        public IntPtr m_hCamera = new IntPtr(-1);

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 采集卡设备句柄
        /// </summary>
        public IntPtr m_hBoard = new IntPtr(-1);

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 回调委托
        /// </summary>
        delegate void IKapCallBackProc(IntPtr pParam);

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 传图回调
        /// </summary>
        private IKapCallBackProc OnFrameReadyProc;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 丢图回调
        /// </summary>
        private IKapCallBackProc OnFrameLostProc;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初筛算法日志
        /// </summary>
        protected CLogRec FilterLog = CLogRec.Create("Filter", "D:/Data");

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像缓存
        /// </summary>
        List<CBuffer> buffers = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 缓存个数
        /// </summary>
        int bufferSize = 100;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 帧数据大小
        /// </summary>
        int frameSize = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 当前写入帧序号
        /// </summary>
        int curFrame = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 线程锁
        /// </summary>
        object bufferLock = new object();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 保存图片序号
        /// </summary>
        int saveCount = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 断面初筛算法参数组
        /// </summary>
        List<SMaociAlgorParam> algParams = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 侧面初筛算法参数组
        /// </summary>
        List<SSideMaociAlgorParam> algParamSides = new();

        /// <summary>
        /// 2025.1.14 李焕彬
        /// CXP信息复制
        /// </summary>
        /// <param name="srcInfo"></param>
        /// <param name="dstInfo"></param>
        private void CopyCXPInfo(
            IKapCLib.ITK_CXP_DEV_INFO srcInfo,
            ref IKapBoard.IKAP_CXP_BOARD_INFO dstInfo
        )
        {
            dstInfo.BoardIndex = srcInfo.BoardIndex;
            dstInfo.CameraIndex = srcInfo.CameraId;
            dstInfo.MasterPort = srcInfo.MasterPort;
            dstInfo.SlaveCount = srcInfo.SlaveCount;

            dstInfo.Reserved = new byte[252];
            for (int j = 0; j < srcInfo.Reserved.Length; j++)
            {
                dstInfo.Reserved[j] = (byte)srcInfo.Reserved.ElementAt(j);
            }

            dstInfo.SlavePort = new uint[7];
            for (int j = 0; j < srcInfo.SlavePort.Length; j++)
            {
                dstInfo.SlavePort[j] = srcInfo.SlavePort[j];
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 打开相机
        /// </summary>
        /// <returns>true打开成功, false打开失败</returns>
        public override bool OpenCamera()
        {
            try
            {
                uint res = IKapCLib.ItkManInitialize(); //使用IKapCLib之前需要先执行ItkManInitialize，静态函数，影响所有Ikap库相机
                if (!CheckIKapC(res))
                {
                    throw new Exception(res.ToString());
                }
                uint numCameras = 0;
                res = IKapCLib.ItkManGetDeviceCount(ref numCameras);
                //遍历所有相机
                for (uint i = 0; i < numCameras; i++)
                {
                    IKapCLib.ITKDEV_INFO di = new IKapCLib.ITKDEV_INFO();
                    res = IKapCLib.ItkManGetDeviceInfo(i, ref di);
                    if (di.SerialNumber == paramSetting.SerialNumber)
                    {
                        res = IKapCLib.ItkDevOpen(
                            i,
                            (int)ItkDeviceAccessMode.ITKDEV_VAL_ACCESS_MODE_EXCLUSIVE,
                            ref m_hCamera
                        );
                        CheckIKapC(res);

                        IKapCLib.ITK_CXP_DEV_INFO cxpDevInfo = new IKapCLib.ITK_CXP_DEV_INFO();
                        IKapBoard.IKAP_CXP_BOARD_INFO cxpBoardInfo =
                            new IKapBoard.IKAP_CXP_BOARD_INFO();
                        // 获取CoaXPress相机设备信息。
                        //
                        // Get CoaXPress camera device information.
                        res = IKapCLib.ItkManGetCXPDeviceInfo(i, ref cxpDevInfo);
                        CheckIKapC(res);
                        // 打开采集卡。
                        //
                        // Open frame grabber.
                        CopyCXPInfo(cxpDevInfo, ref cxpBoardInfo);
                        m_hBoard = IKapBoard.IKapOpenCXP(
                            (uint)BoardType.IKBoardPCIE,
                            cxpBoardInfo.BoardIndex,
                            cxpBoardInfo
                        );
                        if (m_hBoard.Equals(-1))
                        {
                            throw new Exception();
                        }
                        int ret = IKapBoard.IKapLoadConfigurationFromFile(
                            m_hBoard,
                            AppDomain.CurrentDomain.BaseDirectory + $"\\IKap1.vlcf"
                        );
                        if (!CheckIKapBoard(ret))
                            throw new Exception();

                        SetBufferFrameCount(paramSetting.TotalFrameCount);
                        SetInternalTrigFreq(paramSetting.InterTriggerFrequence);

                        //采集卡超时时间不能设置，或者直接设置为0，不然时间到会报超时停止采集
                        //ret = IKapBoard.IKapSetInfo(
                        //    m_hBoard,
                        //    (uint)INFO_ID.IKP_TIME_OUT,
                        //    paramSetting.TimeOut
                        //);
                        //if (!CheckIKapBoard(ret))
                        //    throw new Exception();

                        int grab_mode = (int)GrabMode.IKP_GRAB_NON_BLOCK;
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_GRAB_MODE,
                            grab_mode
                        );
                        if (!CheckIKapBoard(ret))
                            throw new Exception();

                        int transfer_mode = (int)
                            FrameTransferMode.IKP_FRAME_TRANSFER_SYNCHRONOUS_NEXT_EMPTY_WITH_PROTECT;
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_FRAME_TRANSFER_MODE,
                            transfer_mode
                        );
                        if (!CheckIKapBoard(ret))
                            throw new Exception();
                        IKapBoard.IKapGetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_FRAME_SIZE,
                            ref frameSize
                        );
                        OnFrameReadyProc = new IKapCallBackProc(OnFrameReadyFunc);
                        ret = IKapBoard.IKapRegisterCallback(
                            m_hBoard,
                            (uint)CallBackEvents.IKEvent_FrameReady,
                            Marshal.GetFunctionPointerForDelegate(OnFrameReadyProc),
                            m_hBoard
                        );
                        if (!CheckIKapBoard(ret))
                            throw new Exception();

                        OnFrameLostProc = new IKapCallBackProc(OnFrameLostFunc);
                        ret = IKapBoard.IKapRegisterCallback(
                            m_hBoard,
                            (uint)CallBackEvents.IKEvent_FrameLost,
                            Marshal.GetFunctionPointerForDelegate(OnFrameLostProc),
                            m_hBoard
                        );
                        if (!CheckIKapBoard(ret))
                            throw new Exception();

                        //ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_AUTO_CLEAR, 0);//关闭自动清空机制
                        //if (!CheckIKapBoard(ret)) throw new Exception();

                        this.Connected = true;
                        Task.Factory.StartNew(new Action(PreFilter)); //初筛
                        StartGrab();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorInitCam2 + paramSetting.SerialNumber + ex.Message
                );
                return false;
            }
        }

        bool received = false;

        /// <summary>
        /// 李焕彬 2024.12.25
        /// 回调函数，当一帧图像采集完成时，函数被调用
        /// </summary>
        /// <param name="pParam">板卡对象</param>
        public void OnFrameReadyFunc(IntPtr pParam)
        {
            try
            {
                IntPtr hDev = (IntPtr)pParam;
                IntPtr pUserBuffer = IntPtr.Zero;
                int nFrameIndex = 0;
                IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();

                IKapBoard.IKapGetInfo(
                    hDev,
                    (uint)INFO_ID.IKP_CURRENT_BUFFER_INDEX,
                    ref nFrameIndex
                );
                IKapBoard.IKapGetBufferStatus(hDev, nFrameIndex, ref status);

                // 当图像缓冲区满时。
                if (status.uFull == 1)
                {
                    received = true;
                    // 获取缓冲区地址。
                    IKapBoard.IKapGetBufferAddress(hDev, nFrameIndex, ref pUserBuffer);

                    startGrabSoft = true;
                    grabCount++;
                    StringBuilder textBuilder = new StringBuilder(
                        Properties.Resources.InfoReceImage
                    );
                    textBuilder.Append(grabCount + $"缓冲区索引：{nFrameIndex}");
                    getImageLogger.Info(textBuilder.ToString());

                    //IKapBoard.IKapReleaseBuffer(hDev, nFrameIndex);
                    if (
                        paramSetting.UseFilter
                        && paramSetting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE
                    )
                    {
                        lock (bufferLock)
                        {
                            if (!buffers[curFrame % bufferSize].isUsing)
                            {
                                buffers[curFrame % bufferSize].encoderPos = 0;
                                CopyMemory1(
                                    pUserBuffer,
                                    buffers[curFrame % bufferSize].ptr,
                                    frameSize
                                );
                                curFrame++;
                            }
                            else
                            {
                                FilterLog.Error($"Lost Frame:{grabCount}!");
                            }
                        }
                    }
                    else
                    {
                        ImageQueueChannel.Writer.TryWrite(pUserBuffer);
                    }
                    if (paramSetting.SaveImage)
                    {
                        if (saveCount < 1000)
                        {
                            CImage image = new CImage(
                                paramSetting.ImageWidth,
                                paramSetting.ImageHeight,
                                pUserBuffer,
                                paramSetting.CameraType == PixelFormats.Gray8
                                    ? PixelFormats.Gray8
                                    : PixelFormats.Rgb24
                            );
                            image.SaveImage($"Image\\{saveCount++}.jpg");
                        }
                        else
                        {
                            paramSetting.SaveImage = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(
                    Properties.Resources.ErrorCallBack + paramSetting.SerialNumber + ex.Message
                );
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初筛处理线程
        /// </summary>
        public void PreFilter()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            for (int g = 0; g < bufferSize; g++)
            {
                buffers.Add(new(Marshal.AllocHGlobal(frameSize), false));
                algParams.Add(new(paramSetting, paramSetting.MmPerPixel * 1000));
                algParamSides.Add(new(paramSetting, paramSetting.MmPerPixel * 1000));
            }
            IntPtr ptrResult = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 50);
            int[] result = new int[50];
            int[] indexs = new int[50];
            IntPtr[] intPtrs = new IntPtr[50];
            int useBuffers = 0;
            curFrame = 0;
            int lastFrame = 0;
            int stride = Setting.ImageWidth * ((8 + 7) / 8);
            Stopwatch sw = new Stopwatch();
            Stopwatch swOkShow = Stopwatch.StartNew(); //OK显示计时器
            Stopwatch swFilterCount = Stopwatch.StartNew();
            int filterCount = 0;
            while (Connected)
            {
                if (swFilterCount.ElapsedMilliseconds > 1000 && filterCount > 0)
                {
                    swFilterCount.Restart();
                    FilterLog.Info($"执行初筛算法图像数:{filterCount}!");
                    filterCount = 0;
                }
                if (paramSetting.UseFilter)
                {
                    if (curFrame > lastFrame)
                    {
                        lock (bufferLock)
                        {
                            if (curFrame - lastFrame > 50)
                            {
                                FilterLog.Error($"处理帧数超过50，丢失帧数:{curFrame - lastFrame - 50}!");
                                lastFrame = curFrame - 50;
                            }
                            useBuffers = 0;
                            for (int g = lastFrame; g < curFrame; g++)
                            {
                                intPtrs[useBuffers] = buffers[g % bufferSize].ptr;
                                indexs[useBuffers] = g % bufferSize;
                                buffers[indexs[useBuffers]].isUsing = true;
                                useBuffers++;
                            }
                            lastFrame = curFrame;
                        }
                        sw.Restart();
                        switch (paramSetting.PreAlgorithm)
                        {
                            case EMPREALGORITHM.EMPREALGORITHMMAOCI:
                                PreTest(
                                    intPtrs,
                                    useBuffers,
                                    Setting.ImageWidth,
                                    Setting.ImageHeight,
                                    Setting.ImageWidth,
                                    algParams.ToArray(),
                                    ptrResult
                                );
                                break;
                            case EMPREALGORITHM.EMPREALGORITHMSIDEMAOCI:
                                PreTestSide(
                                    intPtrs,
                                    useBuffers,
                                    Setting.ImageWidth,
                                    Setting.ImageHeight,
                                    Setting.ImageWidth,
                                    algParamSides.ToArray(),
                                    ptrResult
                                );
                                break;
                        }

                        sw.Stop();
                        Marshal.Copy(ptrResult, result, 0, useBuffers);
                        int ngCount = 0;
                        lock (bufferLock)
                        {
                            for (int g = 0; g < useBuffers; g++)
                            {
                                if (result[g] != 0)
                                {
                                    ImageQueueChannel.Writer.TryWrite(intPtrs[g]);
                                    ngCount++;
                                }
                                buffers[indexs[g]].isUsing = false;
                            }
                            if (ngCount > 0)
                            {
                                swOkShow.Restart();
                            }
                            else
                            {
                                if (
                                    swOkShow.Elapsed.TotalSeconds
                                    > ((CParameterSetting)Setting).OKFrameTicks
                                )
                                {
                                    swOkShow.Restart();
                                    ImageQueueChannel.Writer.TryWrite(intPtrs[0]);
                                }
                            }
                        }
                        FilterLog.Info(
                            $"执行初筛算法图像数:{useBuffers},处理时间：{sw.ElapsedMilliseconds}ms,NG数{ngCount}!"
                        );
                        filterCount += useBuffers;
                    }
                }
            }
            for (int g = 0; g < bufferSize; g++)
            {
                Marshal.FreeHGlobal(buffers[g].ptr);
            }
            buffers.Clear();
            algParams.Clear();
            algParamSides.Clear();
        }

        /// <summary>
        /// 李焕彬 2024.12.25
        /// 回调函数，当采集丢帧时，函数被调用
        /// </summary>
        /// <param name="pParam">板卡对象</param>
        public void OnFrameLostFunc(IntPtr pParam)
        {
            CCameraManagement.CamLogger.Info("Lost Frame!");
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 缓冲区帧数设置
        /// </summary>
        /// <param name="frameCount">缓冲区帧数</param>
        /// <returns></returns>
        public bool SetBufferFrameCount(int frameCount)
        {
            int ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_COUNT, frameCount); //缓冲区帧数要大于0，不然会报错
            return CheckIKapBoard(ret);
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 内触发帧率设置
        /// </summary>
        /// <param name="frameCount">内触发帧率</param>
        /// <returns></returns>
        public bool SetInternalTrigFreq(int frameCount)
        {
            int ret = IKapBoard.IKapSetInfo(
                m_hBoard,
                (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_FREQUENCY,
                frameCount
            );
            return CheckIKapBoard(ret);
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 更新初筛参数
        /// </summary>
        public void UpdateFilterParam()
        {
            for (int g = 0; g < bufferSize; g++)
            {
                algParams[g] = new(paramSetting, paramSetting.MmPerPixel * 1000);
                algParamSides[g] = new(paramSetting, paramSetting.MmPerPixel * 1000);
            }
            MessageBox.Show("更新初筛参数完成！");
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 开始保存图像
        /// </summary>
        public void StartSaveImage()
        {
            if (paramSetting.SaveImage)
            {
                Directory.CreateDirectory("Image\\");
                saveCount = 0;
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
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

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 关闭相机
        /// </summary>
        public override void CloseCamera()
        {
            int ret = (int)ErrorCode.IK_RTN_OK;
            ret = IKapBoard.IKapUnRegisterCallback(
                m_hBoard,
                (uint)CallBackEvents.IKEvent_FrameReady
            );

            ret = IKapBoard.IKapUnRegisterCallback(
                m_hBoard,
                (uint)CallBackEvents.IKEvent_FrameLost
            );

            // 关闭采集卡设备。
            //
            // Close frame grabber device.
            if (!m_hBoard.Equals(-1))
            {
                IKapBoard.IKapClose(m_hBoard);
                m_hBoard = (IntPtr)(-1);
            }

            // 关闭相机设备。
            //
            // Close camera device.
            if (!m_hCamera.Equals(-1))
            {
                IKapCLib.ItkDevClose(m_hCamera);
                m_hCamera = (IntPtr)(-1);
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 开始采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StartGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                    int ret = (int)ErrorCode.IK_RTN_OK;

                    res = IKapCLib.ItkDevExecuteCommand(m_hCamera, "AcquisitionStop");
                    if (!CheckIKapC(res))
                        throw new Exception();

                    ret = IKapBoard.IKapStartGrab(m_hBoard, 0);
                    if (!CheckIKapBoard(ret))
                        throw new Exception();

                    res = IKapCLib.ItkDevExecuteCommand(m_hCamera, "AcquisitionStart");
                    if (!CheckIKapC(res))
                        throw new Exception();

                    result = true;
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

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 停止采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public override bool StopGrab()
        {
            try
            {
                bool result = false;
                if (this.Connected)
                {
                    startGrabSoft = false;
                    int ret = IKapBoard.IKapStopGrab(m_hBoard);
                    if (!CheckIKapBoard(ret))
                        throw new Exception();

                    result = true;
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
        /// 2025.1.14 李焕彬
        /// 软触发触发拍照执行
        /// </summary>
        public override void ExecuteSoftwareTrigger()
        {
            try
            {
                if (this.Connected)
                {
                    base.ExecuteSoftwareTrigger();
                    received = false;
                    int ret = IKapBoard.IKapSetInfo(
                        m_hBoard,
                        (int)INFO_ID.IKP_SOFTWARE_TRIGGER_START,
                        1
                    );
                    CheckIKapBoard(ret);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (true)
                    {
                        Thread.Sleep(10);
                        if (!received)
                        {
                            ret = IKapBoard.IKapSetInfo(
                                m_hBoard,
                                (int)INFO_ID.IKP_SOFTWARE_TRIGGER_START,
                                1
                            );
                        }
                        if (received || stopwatch.ElapsedMilliseconds > 1000)
                        {
                            break;
                        }
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

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置参数
        /// </summary>
        public override void SetCameraParam()
        {
            base.SetCameraParam();
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageWidth(out int value)
        {
            CameraHandle cameraHandle = new CameraHandle();
            IKapBoard.IKapGetInfo(m_hBoard, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref cameraHandle);
            value = cameraHandle;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetImageHeight(out int value)
        {
            CameraHandle cameraHandle = new CameraHandle();
            IKapBoard.IKapGetInfo(m_hBoard, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref cameraHandle);
            value = cameraHandle;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetCameraType(out PixelFormat cameraType)
        {
            CameraHandle cameraHandle = new CameraHandle();
            IKapBoard.IKapGetInfo(m_hBoard, (uint)INFO_ID.IKP_IMAGE_TYPE, ref cameraHandle);
            cameraType = (int)cameraHandle == 0 ? PixelFormats.Gray8 : PixelFormats.Rgb24;
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置触发源
        /// </summary>
        protected override void SetTriggerMode(EMTRIGGERMODE mode)
        {
            try
            {
                int ret = (int)ErrorCode.IK_RTN_OK;
                switch (mode)
                {
                    case EMTRIGGERMODE.EMTRIGGERNONE:
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE,
                            0
                        );
                        break;
                    case EMTRIGGERMODE.EMTRIGGERSOFTWARE:
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE,
                            9
                        );
                        break;
                    case EMTRIGGERMODE.EMTRIGGERHARDWARE:
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE,
                            5
                        );
                        break;
                    default:
                        ret = IKapBoard.IKapSetInfo(
                            m_hBoard,
                            (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE,
                            0
                        );
                        break;
                }
                if (!CheckIKapBoard(ret))
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetTriggerMode + ret.ToString()
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
        /// 2025.1.14 李焕彬
        /// 获取触发模式
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool GetTriggerMode(out EMTRIGGERMODE mode)
        {
            try
            {
                int getValue = 0;
                int ret = IKapBoard.IKapGetInfo(
                    m_hBoard,
                    (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE,
                    ref getValue
                );
                if (CheckIKapBoard(ret))
                {
                    switch (getValue)
                    {
                        case 0:
                            mode = EMTRIGGERMODE.EMTRIGGERNONE;
                            break;
                        case 9:
                            mode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            break;
                        case 5:
                            mode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                            break;
                        default:
                            mode = EMTRIGGERMODE.EMTRIGGERNONE;
                            break;
                    }
                    return true;
                }
                else
                {
                    mode = EMTRIGGERMODE.EMTRIGGERNONE;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetTriggerMode
                            + paramSetting.SerialNumber
                            + ret.ToString()
                    );
                    return false;
                }
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
        /// 2025.1.14 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetExposureTime(out uint value)
        {
            try
            {
                uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                IntPtr hFeature = new IntPtr(-1);
                res = IKapCLib.ItkDevAllocFeature(m_hCamera, "ExposureTime", ref hFeature);
                CheckIKapC(res);
                double dCameraExpTime = 0;
                res = IKapCLib.ItkFeatureGetDouble(hFeature, ref dCameraExpTime);
                if (CheckIKapC(res))
                {
                    value = (uint)dCameraExpTime;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetExposureTime
                            + paramSetting.SerialNumber
                            + res.ToString()
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
        /// 2025.1.14 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public override void SetExposureTime(uint value)
        {
            try
            {
                uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                IntPtr hFeature = new IntPtr(-1);
                res = IKapCLib.ItkDevAllocFeature(m_hCamera, "ExposureTime", ref hFeature);
                CheckIKapC(res);
                res = IKapCLib.ItkFeatureSetDouble(hFeature, value);
                if (!CheckIKapC(res))
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetExposureTime
                            + paramSetting.SerialNumber
                            + res.ToString()
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
        /// 2025.1.14 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGain(out float value)
        {
            try
            {
                uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                IntPtr hFeature = new IntPtr(-1);
                res = IKapCLib.ItkDevAllocFeature(m_hCamera, "DigitalGain", ref hFeature);
                CheckIKapC(res);
                double gain = 0;
                res = IKapCLib.ItkFeatureGetDouble(hFeature, ref gain);
                if (CheckIKapC(res))
                {
                    value = (float)gain;
                    return true;
                }
                else
                {
                    value = 0;
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorGetGain2
                            + paramSetting.SerialNumber
                            + res.ToString()
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
        /// 2025.1.14 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public override void SetGain(float value)
        {
            try
            {
                uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                IntPtr hFeature = new IntPtr(-1);
                res = IKapCLib.ItkDevAllocFeature(m_hCamera, "DigitalGain", ref hFeature);
                CheckIKapC(res);
                res = IKapCLib.ItkFeatureSetDouble(hFeature, value);
                if (!CheckIKapC(res))
                {
                    CCameraManagement.CamLogger.Error(
                        Properties.Resources.ErrorSetGain
                            + paramSetting.SerialNumber
                            + res.ToString()
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
        /// 2025.1.14 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetGamma(out float value)
        {
            //try
            //{
            //    int gamma = 0;
            //    CameraSdkStatus status = MvApi.CameraGetGamma(m_hCamera, ref gamma);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = gamma / 100;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetGamma + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetGamma2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public override void SetGamma(float value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetGamma(m_hCamera, (int)value * 100);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetGamma + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetGamma2 + paramSetting.SerialNumber + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public override void SetTriggerDelay(uint value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetStrobeDelayTime(m_hCamera, value);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetTriggerDelay + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetTriggerDelay2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerDelay(out uint value)
        {
            //try
            //{
            //    uint delayTime = 0;
            //    CameraSdkStatus status = MvApi.CameraGetStrobeDelayTime(m_hCamera, ref delayTime);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = delayTime;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetTriggerDelay + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerDelay2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public override void SetTriggerPulseWidth(uint value)
        {
            //try
            //{
            //    CameraSdkStatus status = MvApi.CameraSetStrobePulseWidth(m_hCamera, value);
            //    if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorSetTriggerPulseWidth + status.ToString()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorSetTriggerPulseWidth2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public override bool GetTriggerPulseWidth(out uint value)
        {
            //try
            //{
            //    uint delayTime = 0;
            //    CameraSdkStatus status = MvApi.CameraGetStrobePulseWidth(m_hCamera, ref delayTime);
            //    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            //    {
            //        value = delayTime;
            //        return true;
            //    }
            //    else
            //    {
            //        value = 0;
            //        CCameraManagement.CamLogger.Error(
            //            Properties.Resources.ErrorGetTriggerPulseWidth + status.ToString()
            //        );
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorGetTriggerPulseWidth2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
            value = 0;
            return false;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 保存用户参数
        /// </summary>
        public override void UserSaveParam()
        {
            try
            {
                if (this.Connected)
                {
                    uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                    IntPtr hFeature = new IntPtr(-1);
                    res = IKapCLib.ItkDevAllocFeature(m_hCamera, "UserSetSave", ref hFeature);
                    CheckIKapC(res);
                    res = IKapCLib.ItkFeatureExecuteCommand(hFeature);
                    if (!CheckIKapC(res))
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorUserSaveParam
                                + paramSetting.SerialNumber
                                + res.ToString()
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
        /// 2025.1.14 李焕彬
        /// 加载用户参数
        /// </summary>
        public override void UserLoadParam()
        {
            //try
            //{
            //    if (this.Connected)
            //    {
            //        uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
            //        IntPtr hFeature = new IntPtr(-1);
            //        res = IKapCLib.ItkDevAllocFeature(m_hCamera, "UserSetLoad", ref hFeature);
            //        CheckIKapC(res);
            //        res = IKapCLib.ItkFeatureExecuteCommand(hFeature);
            //        if (!CheckIKapC(res))
            //        {
            //            CCameraManagement.CamLogger.Error(
            //                Properties.Resources.ErrorUserLoadParam
            //                    + paramSetting.SerialNumber
            //                    + res.ToString()
            //            );
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CCameraManagement.CamLogger.Error(
            //        Properties.Resources.ErrorUserLoadParam2
            //            + paramSetting.SerialNumber
            //            + ex.Message
            //    );
            //    throw;
            //}
        }

        public override void SetCustomParam(uint value)
        {
            throw new NotImplementedException();
        }

        public override void SetStrobeEnable(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSelector(object Line)
        {
            throw new NotImplementedException();
        }

        public override void SetStrobeDuration(uint Value)
        {
            throw new NotImplementedException();
        }

        public override void SetLineSource(object source)
        {
            throw new NotImplementedException();
        }

        public override void SetLineInverter(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override void SetLineMode(object LineMode)
        {
            throw new NotImplementedException();
        }

        public override void LineTriggerSoftware()
        {
            throw new NotImplementedException();
        }

        public override void SetGammaEnable(bool Enable)
        {
            throw new NotImplementedException();
        }

        public override float GetFps()
        {
            throw new NotImplementedException();
        }

        public override void SetFrameCount(int count)
        {
            throw new NotImplementedException();
        }
    }
}
