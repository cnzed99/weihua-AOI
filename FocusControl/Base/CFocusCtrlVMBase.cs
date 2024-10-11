using System.Threading.Channels;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Controls;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace FocusControl
{
    /// <summary>
    /// 2024.7.9 李焕彬
    /// 对焦数据
    /// </summary>
    public class CFocusData : IComparable<CFocusData>
    {
        /// <summary>
        /// 2024.7.9 李焕彬
        /// 位置
        /// </summary>
        public float pos { get; set; }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 清晰度
        /// </summary>
        public float distinct { get; set; }

        public CFocusData(float pos, float distinct)
        {
            this.pos = pos;
            this.distinct = distinct;
        }

        public int CompareTo(CFocusData other)
        {
            return (int)Math.Abs((this.distinct - other.distinct) * 100000);
        }
    }

    /// <summary>
    /// 2024.9.30 李焕彬
    /// 对焦控件VM
    /// </summary>
    public partial class CFocusCtrlVMBase : ObservableObject
    {
        /// <summary>
        /// 显示控件
        /// </summary>
        [ObservableProperty]
        UserControl testControl;

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 设置相机序列号
        /// </summary>
        /// <param name="cameraSerial">序列号</param>
        public virtual void SetCameraSerial(string cameraSerial)
        {
            this.CameraSerial = cameraSerial;
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public virtual void SetRunning(bool isRuning)
        {
            this.IsRuning = isRuning;
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相机序列号
        /// </summary>
        protected string CameraSerial { get; set; } = "";

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 清晰度算法
        /// </summary>
        public Func<CImage, float> FuncDistinct { get; set; }

        /// <summary>
        /// 20240725 TCG
        /// 当前制程是否启动
        /// </summary>
        [ObservableProperty]
        private bool isRuning = false;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CFocusConfigBase config;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 报警信息
        /// </summary>
        [ObservableProperty]
        private string infoAlarm;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 正在对焦状态
        /// </summary>
        [ObservableProperty]
        private bool isFocusing = false;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 是否已经对焦状态
        /// </summary>
        [ObservableProperty]
        private bool isFocused = false;

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 通道数
        /// </summary>
        private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 对焦采集 图像队列
        /// </summary>
        public Channel<Cell> FocusWaitGetImageChannel = Channel.CreateBounded<Cell>(channelOptions);

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        public static CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 初始化控制
        /// </summary>
        public virtual void InitControl() { }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 复位
        /// </summary>
        public virtual void Reset() { }
    }
}
