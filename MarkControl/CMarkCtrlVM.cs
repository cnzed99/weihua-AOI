using System.Collections.ObjectModel;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using Timer = System.Timers.Timer;

namespace MarkControl
{
    /// <summary>
    /// 2024.7.15 李焕彬
    /// 打标控制VM
    /// </summary>
    public partial class CMarkCtrlVM : ObservableObject
    {
        public CMarkCtrlVM()
        {
            //MarkConfig = LoadParameter();
            //WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
            //    MarkConfig,
            //    MarkConfig.token
            //);
            //Connect();
        }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson;

        /// <summary>
        /// 20240801 李焕彬
        /// 当前制程是否启动
        /// </summary>
        [ObservableProperty]
        private bool isRuning = false;

        /// <summary>
        /// 20240801 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public void SetRunning(bool isRuning)
        {
            this.IsRuning = isRuning;
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 定时获取打标模块信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Connected)
            {
                int triggerCnt = 0;
                CMiniEcatLib.Mb_E4O4TrigOut_GetCounter(
                    MarkConfig.SlaveId,
                    MarkConfig.TriggerId,
                    ref triggerCnt
                );
                TrigCount = triggerCnt;
                int encoderCnt = 0;
                CMiniEcatLib.Mb_E4O4Encoder_GetEncoderData(
                    MarkConfig.SlaveId,
                    MarkConfig.EncoderId,
                    ref encoderCnt
                );
                EncoderCount = encoderCnt;
                int waitTriggerCnt = 0;
                CMiniEcatLib.Mb_E4O4DynamicCmp_GetFifoCnt(
                    MarkConfig.SlaveId,
                    MarkConfig.CmpNO,
                    ref waitTriggerCnt
                );
                WaitTrigCount = waitTriggerCnt;
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CMarkConfig markConfig;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 连接信号
        /// </summary>
        [ObservableProperty]
        private bool connected = false;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器数
        /// </summary>
        [ObservableProperty]
        private int encoderCount = 0;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 已触发计数
        /// </summary>
        [ObservableProperty]
        private double trigCount = 0;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 未触发计数
        /// </summary>
        [ObservableProperty]
        private double waitTrigCount = 0;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 触发点集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CTrigPos> trigPoses = new ObservableCollection<CTrigPos>();

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 测试触发点集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CTrigPos> testPoses = new ObservableCollection<CTrigPos>();

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 设置编码器数值
        /// </summary>
        [ObservableProperty]
        private int encoderSetValue = 0;

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        protected CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2024.8.9 李焕彬
        /// 线性比较器是否开启
        /// </summary>
        [ObservableProperty]
        private bool lineComparing = false;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 初始化控制，包含连接、写入初始参数
        /// </summary>
        [RelayCommand]
        public void InitControl()
        {
            if (!Connected)
                return;
            //初始化编码器，使能
            CMiniEcatLib.Mb_E4O4Encoder_Initial(
                MarkConfig.SlaveId,
                MarkConfig.EncoderId,
                (int)MarkConfig.EncoderMode,
                (int)MarkConfig.EncoderDir,
                1
            );
            //编码器数值清零,打开软件时跟相机同步清零
            CMiniEcatLib.Mb_E4O4Encoder_SetCurrentData(MarkConfig.SlaveId, MarkConfig.EncoderId, 0);
            //设置触发输出模式
            CMiniEcatLib.Mb_E4O4TrigOut_SetOutMode(MarkConfig.SlaveId, (int)MarkConfig.OutMode);
            //设置触发模式
            CMiniEcatLib.Mb_E4O4TrigOut_SetTrigMode(
                MarkConfig.SlaveId,
                MarkConfig.TriggerId,
                (int)MarkConfig.TrigMode
            );
            //设置脉宽
            CMiniEcatLib.Mb_E4O4TrigOut_SetPulseWidth(
                MarkConfig.SlaveId,
                MarkConfig.TriggerId,
                MarkConfig.PulseWidth
            );
            //触发计数值清零
            CMiniEcatLib.Mb_E4O4TrigOut_ResetCounter(MarkConfig.SlaveId, MarkConfig.TriggerId);
            //从站号、动态比较器号(0-3)、编码器号(0-3)，绑定动态比较器0和编码器0
            CMiniEcatLib.Mb_E4O4DynamicCmp_BindingEncoder(
                MarkConfig.SlaveId,
                MarkConfig.CmpNO,
                MarkConfig.EncoderId
            );
            //触发通道绑定动态触发表(0001-比较器0，0010-比较器1，0100-比较器2，1000-比较器3)，第二参数是通道号
            CMiniEcatLib.Mb_E4O4TrigOut_BandingDynamic(
                MarkConfig.SlaveId,
                MarkConfig.TriggerId,
                1u << MarkConfig.CmpNO
            );
            //清空动态比较器点表
            CMiniEcatLib.Mb_E4O4DynamicCmp_ClrFifoData(MarkConfig.SlaveId, MarkConfig.CmpNO);
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 连接打标模块
        /// </summary>
        [RelayCommand]
        public void Connect()
        {
            try
            {
                int slaveNum = 0;
                //不开启别名
                CMiniEcatLib.Mb_InitEcat(ref slaveNum, 0);
                if (slaveNum > 0)
                {
                    Connected = true;
                    InitControl();
                    Timer timer = new Timer(300);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Enabled = true;

                    Growl.Success(
                        MarkConfig.PrcessName + "-" + Properties.Resources.SuccessConnect
                    );
                    SysLog.Info(MarkConfig.PrcessName + "-" + Properties.Resources.SuccessConnect);
                }
                else
                {
                    Connected = false;
                    Growl.Error(MarkConfig.PrcessName + "-" + Properties.Resources.ConnectError);
                    SysLog.Error(MarkConfig.PrcessName + "-" + Properties.Resources.ConnectError);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(MarkConfig.PrcessName + "-" + ex.Message);
                SysLog.Error(MarkConfig.PrcessName + "-" + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 打标测试
        /// </summary>
        [RelayCommand]
        public void MarkTest()
        {
            if (!Connected)
                return;
            CMiniEcatLib.Mb_E4O4TrigOut_SetManualPulseOutput(
                MarkConfig.SlaveId,
                MarkConfig.TriggerId
            );
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器清零
        /// </summary>
        [RelayCommand]
        public void SetZeroEncoder()
        {
            if (!Connected)
                return;
            CMiniEcatLib.Mb_E4O4Encoder_SetCurrentData(MarkConfig.SlaveId, MarkConfig.EncoderId, 0);
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 触发计数清零
        /// </summary>
        [RelayCommand]
        public void SetZeroTrigs()
        {
            if (!Connected)
                return;
            CMiniEcatLib.Mb_E4O4TrigOut_ResetCounter(MarkConfig.SlaveId, MarkConfig.TriggerId);
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 清空待触发点表
        /// </summary>
        [RelayCommand]
        public void ClearTrigs()
        {
            if (!Connected)
                return;
            CMiniEcatLib.Mb_E4O4DynamicCmp_ClrFifoData(MarkConfig.SlaveId, MarkConfig.CmpNO);
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 批量点测试
        /// </summary>
        [RelayCommand]
        public void MarksTest()
        {
            if (!Connected)
                return;
            if (TestPoses.Count > 0)
            {
                bool needOffest = false;
                if (
                    HandyControl.Controls.MessageBox.Show(
                        Properties.Resources.AskMarkNeedOffest,
                        "Tips",
                        MessageBoxButton.YesNo
                    ) == MessageBoxResult.Yes
                )
                {
                    needOffest = true;
                }
                List<int> posList = TestPoses.Select(o => o.Pos).ToList();
                posList.Sort();
                foreach (var item in posList)
                {
                    AddMark(item, needOffest);
                }
            }
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 增加打标位置
        /// </summary>
        /// <param name="pos">打标位置</param>
        /// <param name="needAddoffest">true需要加补偿值，false不需要</param>
        public int AddMark(int pos, bool needAddoffest = true)
        {
            if (needAddoffest)
            {
                pos += (int)MarkConfig.GetPulseOffest();
            }
            if (!Connected)
                return pos;
            int encoderCnt = 0;
            CMiniEcatLib.Mb_E4O4Encoder_GetEncoderData(
                MarkConfig.SlaveId,
                MarkConfig.EncoderId,
                ref encoderCnt
            );
            int[] posArray = new int[] { (int)pos };
            CMiniEcatLib.Mb_E4O4DynamicCmp_SetFifoData(
                MarkConfig.SlaveId,
                MarkConfig.CmpNO,
                posArray.Length,
                ref posArray[0]
            );
            System.Windows.Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    if (TrigPoses.Count > 50)
                    {
                        TrigPoses.RemoveAt(0);
                    }
                    TrigPoses.Add(new CTrigPos(pos));
                })
            );

            return pos;
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 清空
        /// </summary>
        [RelayCommand]
        public void Clear()
        {
            if (!Connected)
                return;
            TrigPoses.Clear();
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 设置编码器数值
        /// </summary>
        [RelayCommand]
        public void SetEncoderValue()
        {
            if (!Connected)
                return;
            CMiniEcatLib.Mb_E4O4Encoder_SetCurrentData(
                MarkConfig.SlaveId,
                MarkConfig.EncoderId,
                EncoderSetValue
            );
        }

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 获取当前编码器值
        /// </summary>
        /// <returns>编码器值</returns>
        public int GetEncoderCount()
        {
            if (!Connected)
                return 0;
            int encoderCnt = 0;
            CMiniEcatLib.Mb_E4O4Encoder_GetEncoderData(
                MarkConfig.SlaveId,
                MarkConfig.EncoderId,
                ref encoderCnt
            );
            EncoderCount = encoderCnt;
            return EncoderCount;
        }

        /// <summary>
        /// 线性比较器号
        /// </summary>
        int lineCompNo = 0;

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 开启线性比较器测试
        /// </summary>
        [RelayCommand]
        public void StartLineCompTest()
        {
            if (!Connected)
                return;
            if (LineComparing)
            {
                //关闭
                CMiniEcatLib.Mb_E4O4LineCmp_SetEnable(MarkConfig.SlaveId, lineCompNo, 0);
                CMiniEcatLib.Mb_E4O4LineCmp_SetTriggerData(
                    MarkConfig.SlaveId,
                    lineCompNo,
                    0,
                    999999,
                    500
                );
                LineComparing = false;
                return;
            }
            //开启
            int triggerId = 0;
            CMiniEcatLib.Mb_E4O4TrigOut_BandingCompare(
                MarkConfig.SlaveId,
                triggerId,
                1u << lineCompNo,
                0,
                0
            );
            CMiniEcatLib.Mb_E4O4LineCmp_BingdingEncoder(
                MarkConfig.SlaveId,
                MarkConfig.EncoderId,
                lineCompNo
            );
            CMiniEcatLib.Mb_E4O4LineCmp_SetTriggerData(
                MarkConfig.SlaveId,
                lineCompNo,
                1000,
                int.MaxValue,
                MarkConfig.LineCompInterval
            );
            CMiniEcatLib.Mb_E4O4TrigOut_SetPulseWidth(MarkConfig.SlaveId, triggerId, 100000);
            SetZeroEncoder();
            CMiniEcatLib.Mb_E4O4LineCmp_SetEnable(MarkConfig.SlaveId, lineCompNo, 0);
            CMiniEcatLib.Mb_E4O4LineCmp_SetEnable(MarkConfig.SlaveId, lineCompNo, 1);
            LineComparing = true;
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 配置保存路径
        /// </summary>
        private const string c_ParameterPath = "..\\SystemConfig\\MarkConfig.Json";

        #region 保存参数
        /// <summary>
        /// 2024.7.12 李焕彬
        /// 控制配置保存方法
        /// </summary>
        public void SaveParameter()
        {
            try
            {
                ConfigAPI.Save(MarkConfig, c_ParameterPath);
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数
        /// <summary>
        /// 2024.7.12 李焕彬
        /// 控制配置加载方法
        /// </summary>
        /// <returns></returns>
        public CMarkConfig LoadParameter()
        {
            CMarkConfig config = new CMarkConfig();
            try
            {
                if (File.Exists(c_ParameterPath))
                {
                    config = ConfigAPI.Load<CMarkConfig>(c_ParameterPath);
                    if (config == null)
                    {
                        config = new CMarkConfig();
                    }
                }
                else
                {
                    config = new CMarkConfig();
                }
            }
            catch (Exception)
            {
                config = new CMarkConfig();
            }
            return config;
        }

        #endregion
    }

    /// <summary>
    /// 2024.7.15 李焕彬
    /// 触发位置
    /// </summary>
    public partial class CTrigPos : ObservableObject
    {
        public CTrigPos() { }

        public CTrigPos(int pos)
        {
            this.Pos = pos;
            this.DateTime = DateTime.Now;
        }

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 添加时间
        /// </summary>
        [ObservableProperty]
        private DateTime dateTime;

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 编码器值
        /// </summary>
        [ObservableProperty]
        private int pos;
    }
}
