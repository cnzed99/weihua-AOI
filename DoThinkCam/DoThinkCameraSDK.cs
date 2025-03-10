using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DVPCameraType
{
    public enum dvpStatus
    {
        [Description("操作忽略掉了，不须要任何动作")]
        DVP_STATUS_IGNORED = 7,
        [Description("需要其他数据和操作")]
        DVP_STATUS_NEED_OTHER = 6,
        [Description("还须进行下一阶段，只完成了部分动作")]
        DVP_STATUS_NEXT_STAGE = 5,
        [Description("正忙，此次操作不能进行")]
        DVP_STATUS_BUSY = 4,
        [Description("需要等待，可以再次尝试")]
        DVP_STATUS_WAIT = 3,
        [Description("正在进行，已经被操作过")]
        DVP_STATUS_IN_PROCESS = 2,
        [Description("操作成功")]
        DVP_STATUS_OK = 1,
        [Description("操作失败")]
        DVP_STATUS_FAILED = 0,
        [Description("未知错误")]
        DVP_STATUS_UNKNOW = -1,
        [Description("不支持该功能")]
        DVP_STATUS_NOT_SUPPORTED = -2,
        [Description("初始化未完成")]
        DVP_STATUS_NOT_INITIALIZED = -3,
        [Description("参数无效")]
        DVP_STATUS_PARAMETER_INVALID = -4,
        [Description("参数越界")]
        DVP_STATUS_PARAMETER_OUT_OF_BOUND = -5,
        [Description("特性未打开")]
        DVP_STATUS_UNENABLE = -6,
        [Description("未连接到设备")]
        DVP_STATUS_UNCONNECTED = -7,
        [Description("功能无效")]
        DVP_STATUS_NOT_VALID = -8,
        [Description("设备没打开")]
        DVP_STATUS_UNPLAY = -9,
        [Description("未启动")]
        DVP_STATUS_NOT_STARTED = -10,
        [Description("未停止")]
        DVP_STATUS_NOT_STOPPED = -11,
        [Description("未准备好")]
        DVP_STATUS_NOT_READY = -12,
        [Description("无效句柄（空句柄或野句柄），通常是相机未打开所致")]
        DVP_STATUS_INVALID_HANDLE = -13,
        [Description("错误的描述")]
        DVP_STATUS_DESCR_FAULT = -20,
        [Description("错误的名称")]
        DVP_STATUS_NAME_FAULT = -21,
        [Description("错误的赋值")]
        DVP_STATUS_VALUE_FAULT = -22,
        [Description("被限制")]
        DVP_STATUS_LIMITED = -28,
        [Description("功能无效")]
        DVP_STATUS_FUNCTION_INVALID = -29,
        [Description("在自动进行中，手动方式无效")]
        DVP_STATUS_IN_AUTO = -30,
        [Description("操作被拒绝")]
        DVP_STATUS_DENIED = -31,
        [Description("偏移或地址未对齐")]
        DVP_STATUS_BAD_ALIGNMENT = -40,
        [Description("地址无效")]
        DVP_STATUS_ADDRESS_INVALID = -41,
        [Description("数据块大小无效")]
        DVP_STATUS_SIZE_INVALID = -42,
        [Description("数据量过载")]
        DVP_STATUS_OVER_LOAD = -43,
        [Description("数据量不够")]
        DVP_STATUS_UNDER_LOAD = -44,
        [Description("检查, 校验失败")]
        DVP_STATUS_CHECKED_FAILED = -50,
        [Description("不可用")]
        DVP_STATUS_UNUSABLE = -51,
        [Description("超时错误")]
        DVP_STATUS_TIME_OUT = -1000,
        [Description("硬件IO错误")]
        DVP_STATUS_IO_ERROR = -1001,
        [Description("通讯错误")]
        DVP_STATUS_COMM_ERROR = -1002,
        [Description("总线错误")]
        DVP_STATUS_BUS_ERROR = -1003,
        [Description("格式错误")]
        DVP_STATUS_FORMAT_INVALID = -1004,
        [Description("内容无效")]
        DVP_STATUS_CONTENT_INVALID = -1005,
        [Description("I2C总线错误")]
        DVP_STATUS_I2C_FAULT = -1010,
        [Description("I2C等待应答超时")]
        DVP_STATUS_I2C_ACK_TIMEOUT = -1011,
        [Description("I2C等待总线动作超时，例如SCL被外部器件拉为低电")]
        DVP_STATUS_I2C_BUS_TIMEOUT = -1012,
        [Description("SPI总线错误")]
        DVP_STATUS_SPI_FAULT = -1020,
        [Description("UART总线错误")]
        DVP_STATUS_UART_FAULT = -1030,
        [Description("GPIO总线错误")]
        DVP_STATUS_GPIO_FAULT = -1040,
        [Description("USB总线错误")]
        DVP_STATUS_USB_FAULT = -1050,
        [Description("PCI总线错误")]
        DVP_STATUS_PCI_FAULT = -1060,
        [Description("物理层错误")]
        DVP_STATUS_PHY_FAULT = -1070,
        [Description("链路层错误")]
        DVP_STATUS_LINK_FAULT = -1080,
        [Description("传输层错误")]
        DVP_STATUS_TRANS_FAULT = -1090,
        [Description("没有发现设备")]
        DVP_STATUS_NO_DEVICE_FOUND = -1100,
        [Description("未找到逻辑设备")]
        DVP_STATUS_NO_LOGIC_DEVICE_FOUND = -1101,
        [Description("设备已经打开")]
        DVP_STATUS_DEVICE_IS_OPENED = -1102,
        [Description("设备已经关闭")]
        DVP_STATUS_DEVICE_IS_CLOSED = -1103,
        [Description("设备已经断开连接")]
        DVP_STATUS_DEVICE_IS_DISCONNECTED = -1104,
        [Description("设备已经被其他主机打开")]
        DVP_STATUS_DEVICE_IS_OPENED_BY_ANOTHER = -1105,
        [Description("设备已经被启动")]
        DVP_STATUS_DEVICE_IS_STARTED = -1106,
        [Description("设备已经被停止设备已经被其他主机打开")]
        DVP_STATUS_DEVICE_IS_STOPPED = -1107,
        [Description("没有足够系统内存")]
        DVP_STATUS_INSUFFICIENT_MEMORY = -1200,
        [Description("存储器读写出现误码或无法正常读写")]
        DVP_STATUS_MEMORY_FAULT = -1201,
        [Description("写保护，不可写")]
        DVP_STATUS_WRITE_PROTECTED = -1202,
        [Description("创建文件失败")]
        DVP_STATUS_FILE_CREATE_FAILED = -1300,
        [Description("文件格式无效")]
        DVP_STATUS_FILE_INVALID = -1301,
        [Description("读取文件失败")]
        DVP_STATUS_FILE_READ_FAILED = -1302,
        [Description("写入文件失败")]
        DVP_STATUS_FILE_WRITE_FAILED = -1303,
        [Description("打开文件失败")]
        DVP_STATUS_FILE_OPEN_FAILED = -1304,
        [Description("读取数据较检失败")]
        DVP_STATUS_FILE_CHECKSUM_FAILED = -1305,
        [Description("数据采集失败，指定的时间内未获得数据")]
        DVP_STATUS_GRAB_FAILED = -1600,
        [Description("数据丢失，不完整")]
        DVP_STATUS_LOST_DATA = -1601,
        [Description("未接收到帧结束符")]
        DVP_STATUS_EOF_ERROR = -1602,
        [Description("数据采集功能已经打开")]
        DVP_STATUS_GRAB_IS_OPENED = -1603,
        [Description("数据采集功能已经关闭")]
        DVP_STATUS_GRAB_IS_CLOSED = -1604,
        [Description("数据采集已经启动")]
        DVP_STATUS_GRAB_IS_STARTED = -1605,
        [Description("数据采集已经停止")]
        DVP_STATUS_GRAB_IS_STOPPED = -1606,
        [Description("数据采集正在重启")]
        DVP_STATUS_GRAB_IS_RESTARTING = -1607
    }


    public struct dvpCameraInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("设计厂商")]
        public string Vendor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("生产厂商")]
        public string Manufacturer;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("型号")]
        public string Model;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("系列")]
        public string Family;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("连接名")]
        public string LinkName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("传感器描述")]
        public string SensorInfo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("硬件版本")]
        public string HardwareVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("固件版本")]
        public string FirmwareVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("内核驱动版本")]
        public string KernelVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("设备驱动版本")]
        public string DscamVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("友好设备名称")]
        public string FriendlyName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("接口描述")]
        public string PortInfo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("序列号")]
        public string SerialNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [Description("相机描述")]
        public string CameraInfo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [Description("用户命名")]
        public string UserID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("原始序列号")]
        public string OriginalSerialNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("保留字节")]
        public string reserved;
    }

    public enum dvpLineMode
    {
        [Description("输入信号引脚")]
        LINE_MODE_INPUT,
        [Description("输出信号引脚")]
        LINE_MODE_OUTPUT
    }

    public enum dvpLineSource
    {
        [Description("用户控制电平")]
        OUTPUT_SOURCE_NORMAL = 0,
        [Description("闪光灯信号")]
        OUTPUT_SOURCE_STROBE = 1,
        [Description("PWM信号")]
        OUTPUT_SOURCE_PWM = 2,
        [Description("PULSE信号")]
        OUTPUT_SOURCE_PULSE = 0x10
    }

    public enum dvpLine
    {
        [Description("引脚1")]
        LINE_1 = 65537,
        [Description("引脚2")]
        LINE_2,
        [Description("引脚3")]
        LINE_3,
        [Description("引脚4")]
        LINE_4,
        [Description("引脚5")]
        LINE_5,
        [Description("引脚6")]
        LINE_6,
        [Description("引脚7")]
        LINE_7,
        [Description("引脚8")]
        LINE_8
    }

    public enum dvpTriggerSource
    {
        [Description("dvpTriggerFire 软触发")]
        TRIGGER_SOURCE_SOFTWARE,
        [Description("引脚1触发")]
        TRIGGER_SOURCE_LINE1,
        [Description("引脚2触发")]
        TRIGGER_SOURCE_LINE2,
        [Description("引脚3触发")]
        TRIGGER_SOURCE_LINE3,
        [Description("引脚4触发")]
        TRIGGER_SOURCE_LINE4,
        [Description("引脚5触发")]
        TRIGGER_SOURCE_LINE5,
        [Description("引脚6触发")]
        TRIGGER_SOURCE_LINE6,
        [Description("引脚7触发")]
        TRIGGER_SOURCE_LINE7,
        [Description("引脚8触发")]
        TRIGGER_SOURCE_LINE8
    }

    public struct dvpDoubleDescr
    {
        [Description("步长")]
        public double fStep;

        [Description("最小值")]
        public double fMin;

        [Description("最大值")]
        public double fMax;

        [Description("默认值")]
        public double fDefault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    public enum dvpStreamFormat
    {
        [Description("8位图像")]
        S_RAW8 = 0,
        [Description("10位图像")]
        S_RAW10 = 1,
        [Description("12位图像")]
        S_RAW12 = 2,
        [Description("14位图像")]
        S_RAW14 = 3,
        [Description("16位图像")]
        S_RAW16 = 4,
        [Description("BGR三通道24比特图像")]
        S_BGR24 = 10,
        [Description("BGRA四通道32比特图像")]
        S_BGR32 = 11,
        [Description("BGRA四通道48比特图像")]
        S_BGR48 = 12,
        [Description("BGRA四通道64比特图像")]
        S_BGR64 = 13,
        [Description("RGB三通道24比特图像")]
        S_RGB24 = 14,
        [Description("RGBA四通道32比特图像")]
        S_RGB32 = 15,
        [Description("RGBA四通道48比特图像")]
        S_RGB48 = 16,
        [Description("RGBA四通道64比特图像")]
        S_RGB64 = 17,
        [Description("YUV411")]
        S_YCBCR_411 = 20,
        [Description("YUV422")]
        S_YCBCR_422 = 21,
        [Description("YUV444")]
        S_YCBCR_444 = 22,
        [Description("8位灰度图像")]
        S_MONO8 = 30,
        [Description("10位灰度图像")]
        S_MONO10 = 31,
        [Description("12位灰度图像")]
        S_MONO12 = 32,
        [Description("14位灰度图像")]
        S_MONO14 = 33,
        [Description("16位灰度图像")]
        S_MONO16 = 34,
        S_B8_G8_R8 = 40,
        S_B16_G16_R16 = 44
    }


    public enum dvpOpenMode
    {
        [Description("离线打开")]
        OPEN_OFFLINE = 0,
        [Description("打开实际设备")]
        OPEN_NORMAL = 1,
        [Description("以调试方式打开，千兆网相机可以避免打不调试")]
        OPEN_DEBUG = 8,
        [Description("图像采集和处理线程使用较高的优先级")]
        HIGH_PRIORITY = 0x10
    }


    public enum dvpStreamEvent
    {
        [Description("图像达到后")]
        STREAM_EVENT_ARRIVED,
        [Description("图像校正后")]
        STREAM_EVENT_CORRECTED,
        [Description("图像处理后")]
        STREAM_EVENT_PROCESSED,
        [Description("启动一个专门的线程以dvpGetFrame的方式获取图像")]
        STREAM_EVENT_FRAME_THREAD
    }

    public struct dvpFrame
    {
        [Description("格式")]
        public dvpImageFormat format;

        [Description("位宽")]
        public dvpBits bits;

        [Description("字节数")]
        public uint uBytes;

        [Description("宽度")]
        public int iWidth;

        [Description("高度")]
        public int iHeight;

        [Description("帧编号")]
        public ulong uFrameID;

        [Description("时间戳")]
        public ulong uTimestamp;

        [Description("曝光时间（微秒）")]
        public double fExposure;

        [Description("模拟增益")]
        public float fAGain;

        [Description("第一个像素点的位置")]
        public dvpFirstPosition position;

        [MarshalAs(UnmanagedType.I1)]
        [Description("是否水平翻转")]
        public bool bFlipHorizontalState;

        [MarshalAs(UnmanagedType.I1)]
        [Description("是否垂直翻转")]
        public bool bFlipVerticalState;

        [MarshalAs(UnmanagedType.I1)]
        [Description("是否旋转90度")]
        public bool bRotateState;

        [MarshalAs(UnmanagedType.I1)]
        [Description("是否逆时针旋转")]
        public bool bRotateOpposite;

        [Description("内部标志位")]
        public uint internalFlags;

        [Description("内部信息")]
        public uint internalValue;

        [Description("是否逆时针旋转")]
        public ulong uTriggerId;

        [Description("用户定制的数据")]
        public ulong userValue;

        [Description("附带信息数据指针")]
        public ulong pExtra;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        [Description("保留字节")]
        public uint[] reserved;
    }


    public enum dvpBits
    {
        [Description("8比特数据")]
        BITS_8,
        [Description("10比特数据")]
        BITS_10,
        [Description("12比特数据")]
        BITS_12,
        [Description("14比特数据")]
        BITS_14,
        [Description("16比特数据")]
        BITS_16
    }



    public enum dvpFirstPosition
    {
        [Description("左上角")]
        UP_LEFT,
        [Description("右上角")]
        UP_RIGHT,
        [Description("左下角")]
        BOTTOM_LEFT,
        [Description("右下角")]
        BOTTOM_RIGHT
    }


    public enum dvpStreamState
    {
        [Description("已停止")]
        STATE_STOPED = 0,
        [Description("已启动")]
        STATE_STARTED = 2
    }


    public enum dvpUserSet
    {
        [Description("默认只读设置")]
        USER_SET_DEFAULT,
        [Description("用户设置1")]
        USER_SET_1,
        [Description("用户设置2")]
        USER_SET_2
    }

    public enum dvpImageFormat
    {
        [Description("黑白图像")]
        FORMAT_MONO = 0,
        [Description("BGGR原始图像")]
        FORMAT_BAYER_BG = 1,
        [Description("GBRG原始图像")]
        FORMAT_BAYER_GB = 2,
        [Description("GBRG原始图像")]
        FORMAT_BAYER_GR = 3,
        [Description("RGGB原始图像")]
        FORMAT_BAYER_RG = 4,
        [Description("RGB三通道24比特图像")]
        FORMAT_BGR24 = 10,
        [Description("BGRA四通道32比特图像")]
        FORMAT_BGR32 = 11,
        [Description("BGR三通道48比特图像")]
        FORMAT_BGR48 = 12,
        [Description("BGRA四通道64比特图像")]
        FORMAT_BGR64 = 13,
        [Description("RGB三通道24比特图像")]
        FORMAT_RGB24 = 14,
        [Description("RGBA四通道32比特图像")]
        FORMAT_RGB32 = 15,
        FORMAT_RGB48 = 16,
        [Description("YUV411")]
        FORMAT_YUV_411 = 20,
        [Description("YUV422")]
        FORMAT_YUV_422 = 21,
        [Description("BGR三通道8比特拆分的图像")]
        FORMAT_B8_G8_R8 = 40,
        [Description("BGR三通道16比特拆分的图像")]
        FORMAT_B16_G16_R16 = 44
    }


    public struct dvpRegion
    {
        [Description("横向起始位置")]
        public int X;

        [Description("纵向起始位置")]
        public int Y;

        [Description("宽度（大于0）")]
        public int W;

        [Description("高度（大于0）")]
        public int H;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    public enum dvpEvent
    {
        EVENT_UNKNOW,
        EVENT_CONNECTED,
        EVENT_DISCONNECTED,
        EVENT_STREAM_STARTRD,
        EVENT_STREAM_STOPPED,
        EVENT_FRAME_LOST,
        EVENT_FRAME_TIMEOUT,
        EVENT_LOST_CONNECTION,
        EVENT_RECONNECTED,
        EVENT_FRAME_START,
        EVENT_FRAME_END,
        EVENT_TRIGGER_LAUNCH
    }


    public struct dvpVariant
    {
        [Description("数据首地址")]
        private IntPtr pData;

        [Description("数据字节数")]
        private uint uSize;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("数据类型名称")]
        public string name;
    }


    public struct dvpSensorInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("字符串描述")]
        public string descr;

        [Description("传感器类型")]
        public dvpSensorType sensor;

        [Description("原始像素类型")]
        public dvpSensorPixel pixel;

        [Description("区域")]
        public dvpRegionDescr region;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    public enum dvpSensorType
    {
        [Description("CMOS图像传感器")]
        SENSOR_TYPE_CMOS,
        [Description("CCD图像传感器")]
        SENSOR_TYPE_CCD
    }


    public struct dvpRegionDescr
    {
        [Description("最小宽度")]
        public int iMinW;

        [Description("最小高度")]
        public int iMinH;

        [Description("最大宽度")]
        public int iMaxW;

        [Description("最大高度")]
        public int iMaxH;

        [Description("宽度步长")]
        public int iStepW;

        [Description("高度步长")]
        public int iStepH;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }



    public enum dvpSensorPixel
    {
        [Description("黑白像素")]
        SENSOR_PIXEL_MONO,
        [Description("黑白像素")]
        SENSOR_PIXEL_BAYER_RG,
        [Description("GBRG彩色像素")]
        SENSOR_PIXEL_BAYER_GB,
        [Description("GRBG彩色像素")]
        SENSOR_PIXEL_BAYER_GR,
        [Description("BGGR彩色像素")]
        SENSOR_PIXEL_BAYER_BG
    }


    public struct dvpFrameCount
    {
        [Description("接收帧数，一般为传输到电脑的帧")]
        public uint uFrameCount;

        [Description("丢失帧数，来不及读取的")]
        public uint uFrameDrop;

        [Description("放弃的，采集后被丢掉的")]
        public uint uFrameIgnore;

        [Description("错误帧计数")]
        public uint uFrameError;

        [Description("被采集到的正确帧")]
        public uint uFrameOK;

        [Description("被提交 / 输出的有效帧")]
        public uint uFrameOut;

        [Description("重传帧")]
        public uint uFrameResend;

        [Description("图像处理帧总数")]
        public uint uFrameProc;

        [Description("采集帧率")]
        public float fFrameRate;

        [Description("图像处理帧率")]
        public float fProcRate;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    public struct dvpUserIoInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("可用的输入IO")]
        public char[] inputValid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("可用的输出IO")]
        public char[] outputValid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }



    public struct dvpLineInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("可用的IO")]
        public char[] valid;

        [Description("双向的IO")]
        public char[] bidir;

        [Description("是否支持反向器")]
        public char[] inverter;

        [Description("是否支持软件设置输出电平")]
        public char[] user;

        [Description("是否支持strobe信号")]
        public char[] strobe;

        [Description("是否支持pwm信号")]
        public char[] pwm;

        [Description("是否支持timer信号")]
        public char[] timer;

        [Description("是否支持uart信号")]
        public char[] uart;

        [Description("是否支持pulse信号")]
        public char[] pulse;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        [Description("保留字节")]
        public uint[] reserved;
    }

    public struct dvpFunctionInfo
    {
        [Description("是否支持触发功能")]
        public char bTrigger;

        [Description("是否支持硬件ISP功能")]
        public char bHardwareIsp;

        [Description("是否支持UserSet功能")]
        public char bUserSet;

        [Description("是否支持水平翻转功能")]
        public char bHflip;

        [Description("是否支持垂直翻转功能")]
        public char bVflip;

        [Description("是否支持负片功能")]
        public char bInverse;

        [Description("是否支持符合gige标准的 dvpLine 相机引脚功能")]
        public char bLine;

        [Description("是否支持制冷器")]
        public char bCooler;

        [Description("是否支持温度计功能")]
        public char bTemperature;

        [Description("保留")]
        public char bReserved3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        [Description("保留字节")]
        public int[] dvpReserved;
    }

    public enum dvpDeviceType
    {
        [Description("未知类型")]
        DEVICE_TYPE_UNKNOWN = 0,
        [Description("USB2.0接口的相机")]
        DEVICE_TYPE_USB2_CAMERA = 10,
        [Description("USB3.0接口的相机")]
        DEVICE_TYPE_USB3_CAMERA = 20,
        [Description("千兆网接口的相机")]
        DEVICE_TYPE_GLAN_CAMERA = 30,
        [Description("万兆网接口的相机")]
        DEVICE_TYPE_XGIGE_CAMERA = 40,
        [Description("万兆网采集卡")]
        DEVICE_TYPE_XGIGE_GRABBER = 100
    }

    public struct dvpTemperatureInfo
    {
        [Description("设备的温度")]
        public float fDevice;

        [Description("传感器的温度")]
        public float fSensor;

        [Description("芯片1的温度")]
        public float fChip1;

        [Description("芯片2的温度")]
        public float fChip2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] dvpReserved;
    }


    public struct dvpIntDescr
    {
        [Description("步长")]
        public int iStep;

        [Description("最小值")]
        public int iMin;

        [Description("最大值")]
        public int iMax;

        [Description("默认值")]
        public int iDefault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    public struct dvpFloatDescr
    {
        [Description("步长")]
        public float fStep;

        [Description("最小值")]
        public float fMin;

        [Description("最大值")]
        public float fMax;

        [Description("默认值")]
        public float fDefault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    public struct dvpUint64Descr
    {
        [Description("步长")]
        public uint uStep;

        [Description("最小值")]
        public uint uMin;

        [Description("最大值")]
        public uint uMax;

        [Description("默认值")]
        public uint uDefault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    public enum dvpCurveStyle
    {
        [Description("直线拟合")]
        CURVE_STYLE_LINE,
        [Description("平滑拟合")]
        CURVE_STYLE_WAVE
    }


    [Description("BGR曲线数组")]
    public struct dvpCurveArray
    {
        [Description("BGR曲线数组空间")]
        public dvpPoint3c point;

        [Description("BGR有效通道数，最多3个")]
        public uint rows;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [Description("BGR三通道的有效点数，每通道最多256个")]
        public uint[] cols;
    }


    public struct dvpPoint3c
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 768)]
        [Description("BGR三色256个锚点")]
        public dvpPoint2f[] matrix;
    }


    public struct dvpPoint2f
    {
        [Description("横坐标")]
        public float X;

        [Description("纵坐标")]
        public float Y;
    }


    public struct dvpCurveLut
    {
        [Description("使能状态")]
        public bool enable;

        [Description("曲线风格")]
        public dvpCurveStyle style;

        [Description("锚点数组，由少数几个锚点可以确定一个完整的LUT")]
        public dvpCurveArray array;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public uint[] reserved;
    }


    [Description("选择项描述")]
    public struct dvpEnumDescr
    {
        [Description("枚举对应值")]
        public int iEnumValue;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("枚举对应名")]
        public string szEnumName;
    }


    public struct dvpUintDescr
    {
        [Description("步长")]
        public uint uStep;

        [Description("最小值")]
        public uint uMin;

        [Description("最大值")]
        public uint uMax;

        [Description("默认值")]
        public uint uDefault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    [Description("选择项")]
    public struct dvpSelection
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [Description("字符串描述")]
        public string _string;

        [Description("索引")]
        public int iIndex;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    [Description("颜色校正信息")]
    public struct dvpColorCorrection
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [Description("BGR三色校正系数")]
        public float[] bgr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] dvpReserved;
    }


    public enum dvpAeMode
    {
        [Description("自动曝光，自动增益同时开启，曝光优先")]
        AE_MODE_AE_AG,
        [Description("自动曝光，自动增益同时开启，增益优先")]
        AE_MODE_AG_AE,
        [Description("自动曝光")]
        AE_MODE_AE_ONLY,
        [Description("自动增益")]
        AE_MODE_AG_ONLY
    }


    public enum dvpAntiFlick
    {
        [Description("禁止消除频闪")]
        ANTIFLICK_DISABLE,
        [Description("消除50hz频闪")]
        ANTIFLICK_50HZ,
        [Description("消除60hz频闪")]
        ANTIFLICK_60HZ
    }


    public enum dvpAeOperation
    {
        [Description("功能关闭")]
        AE_OP_OFF,
        [Description("进行一次")]
        AE_OP_ONCE,
        [Description("连续操作")]
        AE_OP_CONTINUOUS
    }

    public enum dvpAwbOperation
    {
        [Description("功能关闭")]
        AWB_OP_OFF,
        [Description("进行一次")]
        AWB_OP_ONCE,
        [Description("连续操作")]
        AWB_OP_CONTINUOUS
    }


    public enum dvpStrobeDriver
    {
        [Description("整个帧周期都有效")]
        FRAME_DURATION,
        [Description("由定时器驱动")]
        TIMER_LOGIC,
        [Description("跟随SENSOR的Strobe信号")]
        SENSOR_STROBE
    }


    public enum dvpStrobeOutputType
    {
        [Description("Strobe输出关闭")]
        STROBE_OUT_OFF,
        [Description("输出低电平")]
        STROBE_OUT_LOW,
        [Description("输出高电平")]
        STROBE_OUT_HIGH
    }

    public enum dvpTriggerInputType
    {
        [Description("触发输入关闭")]
        TRIGGER_IN_OFF,
        [Description("下降沿触发")]
        TRIGGER_NEG_EDGE,
        [Description("低电平触发")]
        TRIGGER_LOW_LEVEL,
        [Description("上升沿触发")]
        TRIGGER_POS_EDGE,
        [Description("高电平触发")]
        TRIGGER_HIGH_LEVEL
    }


    public enum dvpTriggerLineMode
    {
        [Description("普通")]
        TRIGGER_LINE_MODE_NORMAL,
        [Description("曝光时间由脉冲宽度决定")]
        TRIGGER_LINE_MODE_BULB
    }


    [Description("颜色矩阵")]
    public struct dvpColorMatrix
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        [Description("矩阵")]
        public float[] matrix;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("Reserved bytes")]
        public int[] reserved;
    }



    public struct dvpAeConfig
    {
        [Description("最小自动曝光时间（单位为微秒）")]
        public double fExposureMin;

        [Description("最大自动曝光时间（单位为微秒）")]
        public double fExposureMax;

        [Description("保留值1")]
        public double reserved1;

        [Description("最小自动增益值")]
        public float fGainMin;

        [Description("最大自动增益值")]
        public float fGainMax;

        [Description("保留值2")]
        public float reserved2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] dvpReserved;
    }

    public enum dvpBufferMode
    {
        BUFFER_MODE_NEWEST,
        BUFFER_MODE_FIFO
    }

    public struct dvpBufferConfig
    {
        [Description("缓存工作模式")]
        public dvpBufferMode mode;

        [Description("缓存队列大小")]
        public uint uQueueSize;

        [Description("缓存队列满时，是否丢弃新产生的帧")]
        public bool bDropNew;

        [Description("紧凑缓存的内存申请")]
        public bool bLite;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    public struct dvpLineTriggerConfig
    {
        [Description("预分频")]
        public uint uPreDiv;

        [Description("倍频")]
        public uint uMult;

        [Description("分频")]
        public uint uDiv;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public uint[] reserved;
    }


    public enum dvpOutputIo
    {
        [Description("输出1")]
        OUTPUT_IO_1 = 1,
        [Description("输出2")]
        OUTPUT_IO_2,
        [Description("输出3")]
        OUTPUT_IO_3,
        [Description("输出4")]
        OUTPUT_IO_4,
        [Description("输出5")]
        OUTPUT_IO_5,
        [Description("输出6")]
        OUTPUT_IO_6,
        [Description("输出7")]
        OUTPUT_IO_7,
        [Description("输出8")]
        OUTPUT_IO_8,
        [Description("输出9")]
        OUTPUT_IO_9,
        [Description("输出10")]
        OUTPUT_IO_10,
        [Description("输出11")]
        OUTPUT_IO_11,
        [Description("输出12")]
        OUTPUT_IO_12,
        [Description("输出13")]
        OUTPUT_IO_13,
        [Description("输出14")]
        OUTPUT_IO_14,
        [Description("输出15")]
        OUTPUT_IO_15,
        [Description("输出16")]
        OUTPUT_IO_16
    }

    public enum dvpOutputIoFunction
    {
        [Description("普通输出")]
        OUTPUT_FUNCTION_NORMAL,
        [Description("闪光灯输出")]
        OUTPUT_FUNCTION_STROBE
    }


    public enum dvpInputIo
    {
        [Description("输入1")]
        INPUT_IO_1 = 32769,
        [Description("输入2")]
        INPUT_IO_2,
        [Description("输入3")]
        INPUT_IO_3,
        [Description("输入4")]
        INPUT_IO_4,
        [Description("输入5")]
        INPUT_IO_5,
        [Description("输入6")]
        INPUT_IO_6,
        [Description("输入7")]
        INPUT_IO_7,
        [Description("输入8")]
        INPUT_IO_8
    }


    public enum dvpInputIoFunction
    {
        [Description("普通输入")]
        INPUT_FUNCTION_NORMAL = 32769,
        [Description("触发输入")]
        INPUT_FUNCTION_TRIGGER
    }

    [Description("图像格式")]
    public struct dvpFormatSelection
    {
        [Description("分辨率模式选择项")]
        public dvpSelection selection;

        [Description("对应的枚举类型")]
        public dvpStreamFormat format;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public uint[] reserved;
    }


    public struct dvpQuickRoi
    {
        [Description("选择项")]
        public dvpSelection selection;

        [Description("感兴趣的区域")]
        public dvpRegion roi;

        [Description("分辨率模式")]
        public dvpResolutionMode mode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    public struct dvpResolutionMode
    {
        [Description("分辨率模式选择项")]
        public dvpSelection selection;

        [Description("区域")]
        public dvpRegionDescr region;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }

    [Description("选择项描述")]
    public struct dvpSelectionDescr
    {
        [Description("默认索引")]
        public uint uDefault;

        [Description("索引个数")]
        public uint uCount;

        [Description("是否应停止视频流")]
        public char bNeedStop;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [Description("保留字节")]
        public int[] reserved;
    }


    public struct RECT
    {
        public int left;

        public int top;

        public int right;

        public int bottom;

        public int Width => right - left;

        public int Height => bottom - top;

        public bool IsEmpty
        {
            get
            {
                if (left < right)
                {
                    return top >= bottom;
                }
                return true;
            }
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public void Offset(int dx, int dy)
        {
            left += dx;
            top += dy;
            right += dx;
            bottom += dy;
        }
    }


    public struct dvpFrameBuffer
    {
        [Description("帧信息")]
        public dvpFrame frame;

        [Description("图像数据首地址")]
        public IntPtr pData;
    }


    public enum dvpReportPart
    {
        [Description("默认或未做功能分类")]
        PART_DEFAULT = 0,
        [Description("通讯相关")]
        PART_LINK = 16,
        [Description("控制相关")]
        PART_CONTROL = 32,
        [Description("数据流或采集相关")]
        PART_STREAM = 48,
        [Description("触发功能相关")]
        PART_TRIG = 64,
        [Description("GPIO功能相关")]
        PART_GPIO = 80,
        [Description("图像效果相关")]
        PART_IMAGE = 96
    }

    public enum dvpReportLevel
    {
        [Description("信息或提示")]
        LEVEL_INFO = 0x10,
        [Description("问题或故障")]
        LEVEL_PROBLEM = 0x40
    }


    public class DVPCamera
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int dvpStreamCallback(uint handle, dvpStreamEvent _event, IntPtr pContext, ref dvpFrame refFrame, IntPtr pBuffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int dvpEventCallback(uint handle, dvpEvent _event, IntPtr pContext, int param, ref dvpVariant refVariant);

        public const string str_dll_file = "DVPCamera64.dll";

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetCameraInfo(uint handle, ref dvpCameraInfo refCameraInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSensorInfo(uint handle, ref dvpSensorInfo refSensorInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFrameCount(uint handle, ref dvpFrameCount refFrameCount);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUserIoInfo(uint handle, ref dvpUserIoInfo refUserIoInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineInfo(uint handle, ref dvpLineInfo pLineInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFirstPosition(uint handle, ref dvpFirstPosition refFirstPosition);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFunctionInfo(uint handle, ref dvpFunctionInfo pFunctionInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetDeviceType(uint handle, ref dvpDeviceType pDeviceType);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTemperatureInfo(uint handle, ref dvpTemperatureInfo pTemperatureInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpTriggerFire(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpCreateDefectFixInfo(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpCreateFlatFieldInfo(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpResetDevice(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpRestart(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpHold(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetGamma(uint handle, ref int refGamma);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetGamma(uint handle, int Gamma);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetGammaDescr(uint handle, ref dvpIntDescr refGammaDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetGammaState(uint handle, ref bool refGammaState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetGammaState(uint handle, bool GammaState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetContrast(uint handle, ref int refContrast);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetContrast(uint handle, int Contrast);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetContrastDescr(uint handle, ref dvpIntDescr refContrastDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetContrastState(uint handle, ref bool refContrastState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetContrastState(uint handle, bool ContrastState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRoi(uint handle, ref dvpRegion refRoi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRoi(uint handle, dvpRegion Roi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRoiDescr(uint handle, ref dvpRegionDescr refRoiDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRoiState(uint handle, ref bool refRoiState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRoiState(uint handle, bool RoiState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorTemperature(uint handle, ref int refColorTemperature);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetColorTemperature(uint handle, int ColorTemperature);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorTemperatureDescr(uint handle, ref dvpIntDescr refColorTemperatureDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorTemperatureState(uint handle, ref bool refColorTemperatureState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetColorTemperatureState(uint handle, bool ColorTemperatureState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSharpness(uint handle, ref int refSharpness);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSharpness(uint handle, int Sharpness);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSharpnessDescr(uint handle, ref dvpIntDescr refSharpnessDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSharpnessState(uint handle, ref bool refSharpnessState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSharpnessState(uint handle, bool SharpnessState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSaturation(uint handle, ref int refSaturation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSaturation(uint handle, int Saturation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSaturationDescr(uint handle, ref dvpIntDescr refSaturationDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSaturationState(uint handle, ref bool refSaturationState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSaturationState(uint handle, bool SaturationState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct2d(uint handle, ref int refNoiseReduct2d);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetNoiseReduct2d(uint handle, int NoiseReduct2d);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct2dDescr(uint handle, ref dvpIntDescr refNoiseReduct2dDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct2dState(uint handle, ref bool refNoiseReduct2dState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetNoiseReduct2dState(uint handle, bool NoiseReduct2dState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct3d(uint handle, ref int refNoiseReduct3d);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetNoiseReduct3d(uint handle, int NoiseReduct3d);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct3dDescr(uint handle, ref dvpIntDescr refNoiseReduct3dDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetNoiseReduct3dState(uint handle, ref bool refNoiseReduct3dState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetNoiseReduct3dState(uint handle, bool NoiseReduct3dState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBlackLevel(uint handle, ref float refBlackLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBlackLevel(uint handle, float BlackLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBlackLevelDescr(uint handle, ref dvpFloatDescr refBlackLevelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBlackLevelState(uint handle, ref bool refBlackLevelState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBlackLevelState(uint handle, bool BlackLevelState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSoftTriggerLoop(uint handle, ref double refSoftTriggerLoop);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSoftTriggerLoop(uint handle, double SoftTriggerLoop);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSoftTriggerLoopDescr(uint handle, ref dvpDoubleDescr refSoftTriggerLoopDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSoftTriggerLoopState(uint handle, ref bool refSoftTriggerLoopState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSoftTriggerLoopState(uint handle, bool SoftTriggerLoopState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMultiFrames(uint handle, ref ulong refMultiFrames);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetMultiFrames(uint handle, ulong MultiFrames);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMultiFramesDescr(uint handle, ref dvpUint64Descr refMultiFramesDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMultiFramesState(uint handle, ref bool refMultiFramesState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetMultiFramesState(uint handle, char MultiFramesState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamPackInterval(uint handle, ref int refStreamPackInterval);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStreamPackInterval(uint handle, int StreamPackInterval);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamPackIntervalDescr(uint handle, ref dvpIntDescr refStreamPackIntervalDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamPackSize(uint handle, ref int refStreamPackSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStreamPackSize(uint handle, int StreamPackSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamPackSizeDescr(uint handle, ref dvpIntDescr refStreamPackSizeDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeTarget(uint handle, ref int refAeTarget);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeTarget(uint handle, int AeTarget);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeTargetDescr(uint handle, ref dvpIntDescr refAeTargetDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAnalogGain(uint handle, ref float refAnalogGain);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAnalogGain(uint handle, float AnalogGain);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAnalogGainDescr(uint handle, ref dvpFloatDescr refAnalogGainDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetExposure(uint handle, ref double refExposure);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetExposure(uint handle, double Exposure);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetExposureDescr(uint handle, ref dvpDoubleDescr refExposureDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerJitterFilter(uint handle, ref double refTriggerJitterFilter);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerJitterFilter(uint handle, double TriggerJitterFilter);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerJitterFilterDescr(uint handle, ref dvpDoubleDescr refTriggerJitterFilterDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerDelay(uint handle, ref double refTriggerDelay);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerDelay(uint handle, double TriggerDelay);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerDelayDescr(uint handle, ref dvpDoubleDescr refTriggerDelayDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeDelay(uint handle, ref double refStrobeDelay);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStrobeDelay(uint handle, double StrobeDelay);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeDelayDescr(uint handle, ref dvpDoubleDescr refStrobeDelayDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeDuration(uint handle, ref double refStrobeDuration);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStrobeDuration(uint handle, double StrobeDuration);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeDurationDescr(uint handle, ref dvpDoubleDescr refStrobeDurationDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFramesPerTrigger(uint handle, ref int refFramesPerTrigger);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetFramesPerTrigger(uint handle, int FramesPerTrigger);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFramesPerTriggerDescr(uint handle, ref dvpIntDescr refFramesPerTriggerDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTimerValue(uint handle, ref double refTimerValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTimerValue(uint handle, double TimerValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTimerValueDescr(uint handle, ref dvpDoubleDescr refTimerValueDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBufferQueueSize(uint handle, ref int refBufferQueueSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBufferQueueSize(uint handle, int BufferQueueSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBufferQueueSizeDescr(uint handle, ref dvpIntDescr refBufferQueueSizeDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineRate(uint handle, ref double pLineRate);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineRate(uint handle, double LineRate);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineRateDescr(uint handle, ref dvpDoubleDescr pLineRateDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerSource(uint handle, ref dvpTriggerSource pTriggerSource);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerSource(uint handle, dvpTriggerSource TriggerSource);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetCurveLut(uint handle, ref dvpCurveLut refCurveLut);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetCurveLut(uint handle, dvpCurveLut CurveLut);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorCorrection(uint handle, ref dvpColorCorrection refColorCorrection);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetColorCorrection(uint handle, dvpColorCorrection ColorCorrection);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamState(uint handle, ref dvpStreamState refStreamState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStreamState(uint handle, dvpStreamState StreamState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetHardwareIspState(uint handle, ref char refHardwareIspState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetHardwareIspState(uint handle, char HardwareIspState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerState(uint handle, ref bool refTriggerState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerState(uint handle, bool TriggerState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMonoState(uint handle, ref bool refMonoState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetMonoState(uint handle, bool MonoState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetInverseState(uint handle, ref bool refInverseState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetInverseState(uint handle, bool InverseState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFlipHorizontalState(uint handle, ref bool refFlipHorizontalState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetFlipHorizontalState(uint handle, bool FlipHorizontalState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFlipVerticalState(uint handle, ref bool refFlipVerticalState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetFlipVerticalState(uint handle, bool FlipVerticalState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFlatFieldState(uint handle, ref bool refFlatFieldState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetFlatFieldState(uint handle, bool FlatFieldState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetDefectFixState(uint handle, ref bool refDefectFixState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetDefectFixState(uint handle, bool DefectFixState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAutoDefectFixState(uint handle, ref bool refAutoDefectFixState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAutoDefectFixState(uint handle, bool AutoDefectFixState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRotateState(uint handle, ref bool refSimpleRotateState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRotateState(uint handle, bool SimpleRotateState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRotateOpposite(uint handle, ref bool refRotateOpposite);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRotateOpposite(uint handle, bool RotateOpposite);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetCoolerState(uint handle, ref bool pCoolerState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetCoolerState(uint handle, bool CoolerState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeRoi(uint handle, ref dvpRegion refAeRoi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeRoi(uint handle, dvpRegion AeRoi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAwbRoi(uint handle, ref dvpRegion refAwbRoi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAwbRoi(uint handle, dvpRegion AwbRoi);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeMode(uint handle, ref dvpAeMode refAeMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeMode(uint handle, dvpAeMode AeMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAntiFlick(uint handle, ref dvpAntiFlick refAntiFlick);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAntiFlick(uint handle, dvpAntiFlick AntiFlick);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeOperation(uint handle, ref dvpAeOperation refAeOperation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeOperation(uint handle, dvpAeOperation AeOperation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAwbOperation(uint handle, ref dvpAwbOperation refAwbOperation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAwbOperation(uint handle, dvpAwbOperation AwbOperation);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeDriver(uint handle, ref dvpStrobeDriver refStrobeDriver);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStrobeDriver(uint handle, dvpStrobeDriver StrobeDriver);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStrobeOutputType(uint handle, ref dvpStrobeOutputType refStrobeOutputType);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStrobeOutputType(uint handle, dvpStrobeOutputType StrobeOutputType);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerInputType(uint handle, ref dvpTriggerInputType refTriggerInputType);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerInputType(uint handle, dvpTriggerInputType TriggerInputType);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTriggerLineMode(uint handle, ref dvpTriggerLineMode pTriggerLineMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTriggerLineMode(uint handle, dvpTriggerLineMode TriggerLineMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSourceFormat(uint handle, ref dvpStreamFormat refSourceFormat);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSourceFormat(uint handle, dvpStreamFormat SourceFormat);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTargetFormat(uint handle, ref dvpStreamFormat refTargetFormat);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTargetFormat(uint handle, dvpStreamFormat TargetFormat);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUserColorMatrix(uint handle, ref dvpColorMatrix refUserColorMatrix);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetUserColorMatrix(uint handle, dvpColorMatrix UserColorMatrix);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLinkTimeout(uint handle, ref uint refLinkTimeout);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLinkTimeout(uint handle, ref uint LinkTimeout);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeConfig(uint handle, ref dvpAeConfig refAeConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeConfig(uint handle, dvpAeConfig AeConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBufferConfig(uint handle, ref dvpBufferConfig refBufferConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBufferConfig(uint handle, dvpBufferConfig BufferConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineTriggerConfig(uint handle, ref dvpLineTriggerConfig pLineTriggerConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineTriggerConfig(uint handle, ref dvpLineTriggerConfig LineTriggerConfig);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUserSet(uint handle, ref dvpUserSet pUserSet);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetUserSet(uint handle, dvpUserSet UserSet);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetInputIoLevel(uint handle, dvpInputIo inputIo, ref bool refInputIoLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineLevel(uint handle, dvpLine line, ref bool pLineLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineLevel(uint handle, dvpLine line, bool LineLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineInverter(uint handle, dvpLine line, ref bool pLineInverter);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineInverter(uint handle, dvpLine line, bool LineInverter);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineMode(uint handle, dvpLine line, ref dvpLineMode pLineMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineMode(uint handle, dvpLine line, dvpLineMode LineMode);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineSource(uint handle, dvpLine line, ref dvpLineSource pLineSource);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetLineSource(uint handle, dvpLine line, dvpLineSource LineSource);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetLineStatus(uint handle, dvpLine line, bool pLineStatus);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetOutputIoLevel(uint handle, dvpOutputIo outputIo, ref bool refOutputIoLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetOutputIoLevel(uint handle, dvpOutputIo outputIo, bool OutputIoLevel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetOutputIoFunction(uint handle, dvpOutputIo outputIo, ref dvpOutputIoFunction refpOutputIoFunction);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetOutputIoFunction(uint handle, dvpOutputIo outputIo, dvpOutputIoFunction OutputIoFunction);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetInputIoFunction(uint handle, dvpInputIo inputIo, ref dvpInputIoFunction refInputIoFunction);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetInputIoFunction(uint handle, dvpInputIo inputIo, dvpInputIoFunction InputIoFunction);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamFlowCtrlSel(uint handle, ref uint pStreamFlowCtrlSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStreamFlowCtrlSel(uint handle, uint StreamFlowCtrlSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamFlowCtrlSelDescr(uint handle, ref dvpSelectionDescr pStreamFlowCtrlSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStreamFlowCtrlSelDetail(uint handle, uint StreamFlowCtrlSel, ref dvpSelection pStreamFlowCtrlSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorSolutionSel(uint handle, ref uint refColorSolutionSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetColorSolutionSel(uint handle, uint ColorSolutionSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorSolutionSelDescr(uint handle, ref dvpSelectionDescr refColorSolutionSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetColorSolutionSelDetail(uint handle, uint ColorSolutionSel, ref dvpSelection refColorSolutionSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBayerDecodeSel(uint handle, ref uint refBayerDecodeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBayerDecodeSel(uint handle, uint BayerDecodeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBayerDecodeSelDescr(uint handle, ref dvpSelectionDescr refBayerDecodeSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBayerDecodeSelDetail(uint handle, uint BayerDecodeSel, ref dvpSelection refBayerDecodeSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSourceFormatSel(uint handle, ref uint refSourceFormatSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetSourceFormatSel(uint handle, uint SourceFormatSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSourceFormatSelDescr(uint handle, ref dvpSelectionDescr refSourceFormatSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSourceFormatSelDetail(uint handle, uint SourceFormatSel, ref dvpFormatSelection refSourceFormatSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTargetFormatSel(uint handle, ref uint refTargetFormatSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetTargetFormatSel(uint handle, uint TargetFormatSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTargetFormatSelDescr(uint handle, ref dvpSelectionDescr refTargetFormatSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetTargetFormatSelDetail(uint handle, uint TargetFormatSel, ref dvpFormatSelection refTargetFormatSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetResolutionModeSel(uint handle, ref uint refResolutionModeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetResolutionModeSel(uint handle, uint ResolutionModeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetResolutionModeSelDescr(uint handle, ref dvpSelectionDescr refResolutionModeSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetResolutionModeSelDetail(uint handle, uint ResolutionModeSel, ref dvpResolutionMode refResolutionModeSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeSchemeSel(uint handle, ref uint refAeSchemeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAeSchemeSel(uint handle, uint AeSchemeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeSchemeSelDescr(uint handle, ref dvpSelectionDescr refAeSchemeSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAeSchemeSelDetail(uint handle, ref int refSaturation, ref dvpSelection refAeSchemeSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetQuickRoiSel(uint handle, ref uint refQuickRoiSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetQuickRoiSel(uint handle, uint QuickRoiSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetQuickRoiSelDescr(uint handle, ref dvpSelectionDescr refQuickRoiSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetQuickRoiSelDetail(uint handle, uint QuickRoiSel, ref dvpQuickRoi refQuickRoiSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPixelRateSel(uint handle, ref uint refPixelRateSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetPixelRateSel(uint handle, uint PixelRateSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPixelRateSelDescr(uint handle, ref dvpSelectionDescr refPixelRateSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPixelRateSelDetail(uint handle, uint PixelRateSel, ref dvpSelection refPixelRateSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPacketSizeSel(uint handle, ref uint refPacketSizeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetPacketSizeSel(uint handle, uint PacketSizeSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPacketSizeSelDescr(uint handle, ref dvpSelectionDescr refPacketSizeSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetPacketSizeSelDetail(uint handle, uint PacketSizeSel, ref dvpSelection refPacketSizeSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAccelerateSel(uint handle, ref uint refAccelerateSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetAccelerateSel(uint handle, uint AccelerateSel);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAccelerateSelDescr(uint handle, ref dvpSelectionDescr refAccelerateSelDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAccelerateSelDetail(uint handle, uint AccelerateSel, ref dvpSelection refAccelerateSelDetail);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRgbGain(uint handle, float rGain, float gGain, float bGain);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRgbGain(uint handle, ref float refrGain, ref float refgGain, ref float refbGain);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetRgbGainState(uint handle, bool state);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetRgbGainState(uint handle, ref bool refState);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpRefresh(ref uint refCount);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpEnum(uint index, ref dvpCameraInfo refCameraInfo);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpOpenByName(string friendlyName, dvpOpenMode type, ref uint refHandle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpOpenByUserId(string UserId, dvpOpenMode type, ref uint refHandle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpOpenBySn(string Sn, dvpOpenMode type, ref uint refHandle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpOpen(uint index, dvpOpenMode mode, ref uint refHandlee);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpIsValid(uint handle, ref bool reIsValid);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpIsOnline(uint handle, ref bool reIsOnline);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpStart(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpStop(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpIsHold(uint handle, ref bool IsHold);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFrame(uint handle, ref dvpFrame refFrame, ref IntPtr pBuffer, uint timeout);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFrameBuffer(uint handle, ref dvpFrameBuffer refRaw, ref dvpFrameBuffer refOut, uint timeout);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpProcessImage(uint handle, ref dvpFrame refSourceFrame, IntPtr pSourceBuffer, ref dvpFrame pTargetFrame, IntPtr pTargetBuffer, uint targetBufferSize, dvpStreamFormat targetFormat);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDebugReport(uint handle, dvpReportPart part, dvpReportLevel level, bool bForce, string text, uint param);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpClose(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpRegisterStreamCallback(uint handle, dvpStreamCallback proc, dvpStreamEvent _event, IntPtr pContex);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpUnregisterStreamCallback(uint handle, dvpStreamCallback proc, dvpStreamEvent _event, IntPtr pContex);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpRegisterEventCallback(uint handle, dvpEventCallback proc, dvpEvent _event, IntPtr pContext);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpUnregisterEventCallback(uint handle, dvpEventCallback proc, dvpEvent _event, IntPtr pContext);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpLoadDefault(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpLoadConfig(uint handle, string path);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSaveConfig(uint handle, string path);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSaveUserSet(uint handle, dvpUserSet UserSet);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpLoadUserSet(uint handle, dvpUserSet UserSet);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSavePicture(ref dvpFrame refFrame, IntPtr pBuffer, string file, int quality);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpCaptureFile(uint handle, uint ResoulutionModeSel, ref dvpRegion roi, uint timeout, string FilePath, uint quality);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpCaptureFile(uint handle, uint ResoulutionModeSel, IntPtr roi, uint timeout, string FilePath, uint quality);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(ref dvpFrame refFrame, IntPtr pBuffer, IntPtr hWnd, ref RECT psRect, ref RECT pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(ref dvpFrame refFrame, IntPtr pBuffer, IntPtr hWnd, IntPtr psRect, IntPtr pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(ref dvpFrame refFrame, IntPtr pBuffer, IntPtr hWnd, ref RECT psRect, IntPtr pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(ref dvpFrame refFrame, IntPtr pBuffer, IntPtr hWnd, IntPtr psRect, ref RECT pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(IntPtr pFrame, IntPtr pBuffer, IntPtr hWnd, ref RECT psRect, ref RECT pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(IntPtr pFrame, IntPtr pBuffer, IntPtr hWnd, IntPtr psRect, IntPtr pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(IntPtr pFrame, IntPtr pBuffer, IntPtr hWnd, ref RECT psRect, IntPtr pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpDrawPicture(IntPtr pFrame, IntPtr pBuffer, IntPtr hWnd, IntPtr psRect, ref RECT pdRect);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpStartVideoRecord(string file, uint width, uint height, int quality, ref uint refHandle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpIsVideoRecorderValid(uint handle, ref bool refValid);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpVideoRecordWrite(uint handle, ref dvpFrame refFrame, IntPtr pBuffer);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpStopVideoRecord(uint handle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpShowPropertyModalDialog(uint handle, IntPtr hParent);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpReadUserData(uint handle, uint addr, IntPtr pBuffer, uint size);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpWriteUserData(uint handle, uint addr, IntPtr pBuffer, uint size);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetUserId(uint handle, string UserId, ref uint refLength);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGet(uint handle, uint command, IntPtr pParam, ref uint refSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSet(uint handle, uint command, IntPtr pParam, ref uint refSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpConfig(uint command, uint param, IntPtr pData);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpConfigEx(uint index, uint command, uint param, IntPtr pData);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetConfigString(uint handle, string key, ref IntPtr pStr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetConfigString(uint handle, string key, IntPtr pStr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSelectionDescr(uint handle, string key, ref dvpSelectionDescr refSelectionDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetSelectionDetail(uint handle, string key, int iIndex, ref dvpSelection refSelection);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpWriteGenICamReg(uint handle, uint addr, uint data);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpReadGenICamReg(uint handle, uint addr, ref uint data);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpWriteGenICamRegFloat(uint handle, uint addr, float data);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpReadGenICamRegFloat(uint handle, uint addr, ref float data);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpRotateImage(uint handle, int RotateAngle);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dvpGetInt32Value")]
        public static extern dvpStatus dvpGetIntValue(uint handle, string key, ref int pValue, ref dvpIntDescr pIntDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dvpGetInt32ValueSync")]
        public static extern dvpStatus dvpGetIntValueSync(uint handle, string key, ref int pValue, ref dvpIntDescr pIntDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetInt64Value(uint handle, string key, ref long pValue, ref dvpIntDescr pIntDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetInt64ValueSync(uint handle, string key, ref long pValue, ref dvpIntDescr pIntDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dvpSetInt32Value")]
        public static extern dvpStatus dvpSetIntValue(uint handle, string key, int iValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetInt64Value(uint handle, string key, long iValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUint64Value(uint handle, string key, ref ulong pValue, ref dvpUint64Descr pUint64Descr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUint64ValueSync(uint handle, string key, ref ulong pValue, ref dvpUint64Descr pUint64Descr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUint32Value(uint handle, string key, ref uint pValue, ref dvpUintDescr pUintU32Descr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetUint32ValueSync(uint handle, string key, ref uint pValue, ref dvpUintDescr pUintU32Descr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetUint64Value(uint handle, string key, ulong uValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetUint32Value(uint handle, string key, uint uValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumValue(uint handle, string key, ref int pValue, int[] SupportValue, ref uint pSupportNum);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumValueEx(uint handle, string key, ref uint pCurSel, ref int pValue, StringBuilder strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumValueSync(uint handle, string key, ref uint pCurSel, ref int pValue, StringBuilder strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetEnumValue(uint handle, string key, int iValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAllEnumValue(uint handle, string key, ref int pSupportValues, ref uint pSupportNum);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetAllEnumValueSync(uint handle, string key, ref int pSupportValues, ref uint pSupportNum);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumDescr(uint handle, string key, uint uIndex, ref dvpEnumDescr pEnumDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumDescrSync(uint handle, string key, uint uIndex, ref dvpEnumDescr pEnumDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetEnumValueByString(uint handle, string key, string strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumValueByString(uint handle, string key, StringBuilder strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetEnumValueByStringSync(uint handle, string key, StringBuilder strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFloatValue(uint handle, string key, ref float pValue, ref dvpFloatDescr pFloatDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetFloatValueSync(uint handle, string key, ref float pValue, ref dvpFloatDescr pFloatDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetFloatValue(uint handle, string key, float fValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetDoubleValue(uint handle, string key, ref double pValue, ref dvpDoubleDescr pDoubleDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetDoubleValueSync(uint handle, string key, ref double pValue, ref dvpDoubleDescr pDoubleDescr);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetDoubleValue(uint handle, string key, double dValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBoolValue(uint handle, string key, ref bool pValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetBoolValueSync(uint handle, string key, ref bool pValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetBoolValue(uint handle, string key, bool bValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStringValue(uint handle, string key, StringBuilder strValue, int iValueSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetStringValueSync(uint handle, string key, StringBuilder strValue, int iValueSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetStringValue(uint handle, string key, string strValue);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetCommandValue(uint handle, string key);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpSetMultiRoi(uint handle, dvpRegion[] regions, uint uSize);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMultiRoi(uint handle, int index, ref dvpRegion region);

        [DllImport("DVPCamera64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dvpStatus dvpGetMultiRoiNum(uint handle, ref int RoiNum);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

        public static int dvpWriteProfileString(string lpszSection, string lpszKeyName, string lpszString)
        {
            return WriteProfileString(lpszSection, lpszKeyName, lpszString);
        }

        [DllImport("kernel32")]
        public static extern long GetProfileString(string lpApplicationName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize);

        public static string dvpGetProfileString(string lpApplicationName, string lpKeyName, string lpDefault)
        {
            string result = "";
            StringBuilder stringBuilder = new StringBuilder(256);
            if (GetProfileString(lpApplicationName, lpKeyName, lpDefault, stringBuilder, 256) > 0)
            {
                result = stringBuilder.ToString();
            }

            return result;
        }
    }

}
