using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusControl;
using HandyControl.Controls;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace LiquidLens
{
    /// <summary>
    /// 2024.9.28 李焕彬
    /// 控件VM
    /// </summary>
    public partial class CSetCtrlVM : CFocusCtrlVMBase
    {
        public CSetCtrlVM()
        {
            TestControl = new SetCtrl(this);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public override void SetRunning(bool isRuning)
        {
            base.SetRunning(isRuning);
            IsCorrecting = isRuning;
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 配置
        /// </summary>
        [ObservableProperty]
        private CConfig config;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 串口
        /// </summary>
        private SerialPort serialPort = new SerialPort();

        private bool connectedLens = false;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 连接信号
        /// </summary>
        public bool ConnectedLens
        {
            get { return connectedLens; }
            set
            {
                SetProperty(ref connectedLens, value);
                UpdateInfoAlarm();
            }
        }

        private CModbusRtu modbusRtu = new CModbusRtu();

        private bool connectedSensor = false;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 连接信号
        /// </summary>
        public bool ConnectedSensor
        {
            get { return connectedSensor; }
            set
            {
                SetProperty(ref connectedSensor, value);
                UpdateInfoAlarm();
            }
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 启动纠偏
        /// </summary>
        [ObservableProperty]
        bool isCorrecting = false;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// SensorCom线程
        /// </summary>
        private Task taskRead = null;

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 读取线程循环标志
        /// </summary>
        private bool isStart = false;

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 目标电压
        /// </summary>
        private double dstVol = 35;

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 设置电压标志
        /// </summary>
        private bool setVol = false;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 纠偏期望位
        /// </summary>
        [ObservableProperty]
        private double focusPosDst;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 纠偏期望电压
        /// </summary>
        [ObservableProperty]
        private double focusVolDst;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 纠偏感应值
        /// </summary>
        [ObservableProperty]
        private double sensorPos;

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 当前电压
        /// </summary>
        [NotifyPropertyChangedFor(nameof(CurPos))]
        [ObservableProperty]
        private double curVol;

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 当前位置
        /// </summary>
        public double CurPos
        {
            get
            {
                if (volDiss != null)
                {
                    ushort vol = (ushort)(CurVol * 100);
                    for (int i = 0; i < volDiss.Length; i++)
                    {
                        if (volDiss[i] == vol)
                        {
                            return Config.SoftLimitN + i * 0.001;
                        }
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 数据发送
        /// </summary>
        [ObservableProperty]
        private string sendData;

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 数据接收
        /// </summary>
        [ObservableProperty]
        private string recvData;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 对焦请求停止
        /// </summary>
        private CancellationTokenSource cancellFocus;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 粗对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CFocusData> focusDatas = new();

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 精对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CFocusData> fineFocusDatas = new();

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 电压物距数据，索引是物距（0.001mm递增），对应值是电压
        /// </summary>
        private ushort[] volDiss = null;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 初始化控制，包含连接、写入初始参数
        /// </summary>
        [RelayCommand]
        public override void InitControl()
        {
            base.InitControl();

            ConnectToLens();
            ConnectToSensor();
            if (taskRead == null)
            {
                isStart = true;
                taskRead = Task.Factory.StartNew(() =>
                {
                    //Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    while (isStart)
                    {
                        try
                        {
                            if (serialPort.IsOpen)
                            {
                                if (setVol) //设置电压
                                {
                                    setVol = false;
                                    string hex = Convert
                                        .ToString((int)Math.Round(dstVol * 100), 16)
                                        .PadLeft(4, '0');
                                    WriteData(
                                        0x40,
                                        Convert.ToByte(hex.Substring(0, 2), 16),
                                        Convert.ToByte(hex.Substring(2, 2), 16)
                                    );
                                    Thread.Sleep(30);
                                }
                                WriteData(0x82, 0, 0); //读取电压
                            }
                            else
                            {
                                ConnectedLens = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SysLog.Error(
                                Config.PrcessName
                                    + "-"
                                    + Properties.Resources.ReadErrorLens
                                    + "-"
                                    + ex.Message
                            );
                        }
                        try
                        {
                            if (modbusRtu.IsOpen) //读传感器值
                            {
                                SensorPos =
                                    modbusRtu.ReadInputRegister(Config.SlaveSensor, 100) / 100.0f;
                                if (IsCorrecting)
                                {
                                    FocusPosDst =
                                        Config.FocusPos + (SensorPos - Config.FocusSensorPos);
                                    FocusVolDst = GetVolFromDis(FocusPosDst);
                                    SetVoltage(FocusVolDst);
                                }
                            }
                            else
                            {
                                ConnectedSensor = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SysLog.Error(
                                Config.PrcessName
                                    + "-"
                                    + Properties.Resources.ReadErrorSensor
                                    + "-"
                                    + ex.Message
                            );
                        }
                        Thread.Sleep(20);
                    }
                });
            }
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 连接液态镜头
        /// </summary>
        [RelayCommand]
        public void ConnectToLens()
        {
            if (Config.Port != null)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        ConnectedLens = false;
                        serialPort.Close();
                    }
                    this.serialPort.PortName = Config.Port.Name;
                    this.serialPort.BaudRate = (int)Config.BaudRate;
                    this.serialPort.Parity = Config.Parity;
                    this.serialPort.DataBits = (int)Config.DataBits;
                    this.serialPort.StopBits = Config.StopBits;
                    this.serialPort.Handshake = Config.HandShake;
                    this.serialPort.ReadBufferSize = 1024;
                    this.serialPort.WriteBufferSize = 1024;
                    this.serialPort.WriteTimeout = 2000;
                    string[] ports = SerialPort.GetPortNames();
                    if (ports.Contains(this.serialPort.PortName))
                    {
                        this.serialPort.Open();
                        if (this.serialPort.IsOpen)
                        {
                            this.serialPort.DataReceived += new SerialDataReceivedEventHandler(
                                DataReceivedHandler
                            ); // 接收到数据时的事件
                            ConnectedLens = true;
                            InitWrite();
                            SysLog.Info(
                                Config.PrcessName + "-" + Properties.Resources.SuccessConnect
                            );
                        }
                    }
                    else
                    {
                        Growl.Error(
                            Config.PrcessName + "-" + Properties.Resources.ConnectErrorLens
                        );
                        SysLog.Error(
                            Config.PrcessName + "-" + Properties.Resources.ConnectErrorLens
                        );
                    }
                }
                catch (Exception)
                {
                    Growl.Error(Config.PrcessName + "-" + Properties.Resources.ConnectErrorLens);
                    SysLog.Error(Config.PrcessName + "-" + Properties.Resources.ConnectErrorLens);
                }
            }
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 连接传感器
        /// </summary>
        [RelayCommand]
        public void ConnectToSensor()
        {
            if (Config.PortSensor != null)
            {
                try
                {
                    if (ConnectedSensor)
                    {
                        ConnectedSensor = false;
                        modbusRtu.Close();
                    }
                    string[] ports = SerialPort.GetPortNames();
                    if (ports.Contains(Config.PortSensor.Name))
                    {
                        if (modbusRtu.Open(Config))
                        {
                            ConnectedSensor = true;
                            SysLog.Info(
                                Config.PrcessName + "-" + Properties.Resources.SuccessConnectSensor
                            );
                        }
                    }
                    else
                    {
                        Growl.Error(
                            Config.PrcessName + "-" + Properties.Resources.ConnectErrorSensor
                        );
                        SysLog.Error(
                            Config.PrcessName + "-" + Properties.Resources.ConnectErrorSensor
                        );
                    }
                }
                catch (Exception)
                {
                    Growl.Error(Config.PrcessName + "-" + Properties.Resources.ConnectErrorSensor);
                    SysLog.Error(Config.PrcessName + "-" + Properties.Resources.ConnectErrorSensor);
                }
            }
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 初始化写入
        /// </summary>
        public void InitWrite()
        {
            if (Config.SoftLimitP - Config.SoftLimitN > 0)
            {
                int count = Math.Min((int)((Config.SoftLimitP - Config.SoftLimitN) * 1000), 50000);
                volDiss = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    volDiss[i] = (ushort)
                        Math.Round(Config.GetVoltage(Config.SoftLimitN + i / 1000.0f) * 100);
                }
            }
        }

        /// <summary>
        /// 液态镜头串口接收数据后执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] data = new byte[100];
                int read = this.serialPort.Read(data, 0, data.Length);
                if (read >= 8)
                {
                    RecvData = CHexConvert.ByteToHexString(data);
                    //校验
                    if (data[0] == 0x55 && data[1] == 0x00)
                    {
                        switch (data[2])
                        {
                            case 0x82: //读取电压
                                CurVol =
                                    CHexConvert.HexToInt(
                                        CHexConvert.ByteToHexString(new[] { data[3], data[4] })
                                    ) / 100.0f;
                                break;
                            case 0x40: //设置电压
                                break;
                            case 0x32: //设置工作模式
                                break;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 更新报警信息
        /// </summary>
        private void UpdateInfoAlarm()
        {
            if (!ConnectedLens && !ConnectedSensor)
            {
                InfoAlarm = Properties.Resources.ConnectError;
            }
            else if (!ConnectedLens)
            {
                InfoAlarm = Properties.Resources.ConnectErrorLens;
            }
            else if (!ConnectedSensor)
            {
                InfoAlarm = Properties.Resources.ConnectErrorSensor;
            }
            else
            {
                InfoAlarm = "";
            }
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 液态串口写入数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="highData">高位</param>
        /// <param name="lowData">低位</param>
        public void WriteData(byte cmd, byte highData, byte lowData)
        {
            if (!ConnectedLens)
                return;
            byte[] data = new byte[8];
            data[0] = 0x55;
            data[1] = 0x00;
            data[2] = cmd;
            data[3] = highData;
            data[4] = lowData;
            int checkSum = data[0] + data[1] + data[2] + data[3] + data[4]; //校验
            string hex = Convert.ToString(checkSum, 16).PadLeft(4, '0');
            data[5] = Convert.ToByte(hex.Substring(0, 2), 16);
            data[6] = Convert.ToByte(hex.Substring(2, 2), 16);
            data[7] = 0xAA;
            SendData = CHexConvert.ByteToHexString(data);
            serialPort.Write(data, 0, 8);
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 液态串口设置电压
        /// </summary>
        /// <param name="vol">电压</param>
        /// <param name="popuMessage">超限位是否弹窗</param>
        public void SetVoltage(double vol, bool popuMessage = false)
        {
            if (volDiss == null || volDiss.Length == 0)
                return;
            if (
                (vol * 100 >= volDiss.First() && vol * 100 <= volDiss.Last())
                || (vol * 100 <= volDiss.First() && vol * 100 >= volDiss.Last())
            )
            {
                dstVol = vol;
                setVol = true;
            }
            else
            {
                if (popuMessage)
                {
                    Growl.Warning(Config.PrcessName + "-" + Properties.Resources.PosOverLimit);
                }
            }
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 液态串口设置物距
        /// </summary>
        /// <param name="distance">物距</param>
        /// <param name="popuMessage">超限位是否弹窗</param>
        public void SetDistance(double distance, bool popuMessage = false)
        {
            if (distance >= Config.SoftLimitN && distance <= Config.SoftLimitP)
            {
                SetVoltage(GetVolFromDis(distance), popuMessage);
            }
            else
            {
                if (popuMessage)
                {
                    Growl.Warning(Config.PrcessName + "-" + Properties.Resources.PosOverLimit);
                }
            }
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 液态镜头物距换算电压
        /// </summary>
        /// <param name="distance">物距</param>
        /// <returns>电压</returns>
        public double GetVolFromDis(double distance)
        {
            if (volDiss == null || volDiss.Length == 0)
                return 0;
            int index = (int)((distance - Config.SoftLimitN) * 1000);
            if (index < volDiss.Length)
            {
                return volDiss[index] / 100.0d;
            }
            return 0;
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 启动纠偏
        /// </summary>
        [RelayCommand]
        public void StartCorrect()
        {
            IsCorrecting = !IsCorrecting;
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 绝对值移动
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void AbsMove(double dis)
        {
            if (!ConnectedLens)
                return;
            SetDistance(dis, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 相对移动正向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void RelaMoveP(double dis)
        {
            if (!ConnectedLens)
                return;
            double dstDis = CurPos + dis;
            SetDistance(dstDis, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 相对移动反向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void RelaMoveN(double dis)
        {
            if (!ConnectedLens)
                return;
            double dstDis = CurPos - dis;
            SetDistance(dstDis, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 绝对值移动
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void VolAbsMove(double vol)
        {
            if (!ConnectedLens)
                return;
            SetVoltage(vol, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 相对移动正向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void VolRelaMoveP(double vol)
        {
            if (!ConnectedLens)
                return;
            double dstVol = CurVol + vol;
            SetVoltage(dstVol, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 相对移动反向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void VolRelaMoveN(double vol)
        {
            if (!ConnectedLens)
                return;
            double dstVol = CurVol - vol;
            SetVoltage(dstVol, true);
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 绝对值移动，等待到位
        /// </summary>
        /// <param name="pos">位置</param>
        public void WaitMoveTo(float pos)
        {
            double vol = GetVolFromDis(pos);
            SetVoltage(vol);
            Thread.Sleep(20);
            while (Math.Abs(vol - CurVol) > 0.01)
            {
                Thread.Sleep(10);
                cancellFocus.Token.ThrowIfCancellationRequested();
                //SetVoltage(vol);
            }
        }

        /// <summary>
        /// 2024.8.5 李焕彬
        /// 相机旧触发模式
        /// </summary>
        private EMTRIGGERMODE oldTriggerMode;

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 自动对焦
        /// </summary>
        [RelayCommand]
        public void AutoFocus()
        {
            if (!ConnectedLens)
                return;
            if (IsFocusing)
            {
                if (
                    HandyControl.Controls.MessageBox.Show(
                        Properties.Resources.AskStopFocus,
                        "Tips",
                        MessageBoxButton.YesNo
                    ) == MessageBoxResult.Yes
                )
                {
                    IsFocusing = false;

                    cancellFocus.Cancel();
                    return;
                }
            }
            if (!CCameraManagement.CameraDict.ContainsKey(CameraSerial))
            {
                Growl.Warning(Config.PrcessName + "-" + Properties.Resources.ErrorNoCam);
                return;
            }
            CCameraBase cam = CCameraManagement.CameraDict[CameraSerial];
            if (!cam.Connected)
            {
                Growl.Warning(Config.PrcessName + "-" + Properties.Resources.ErrorNoOpenCam);
                return;
            }
            cancellFocus = new CancellationTokenSource();

            if (!ConnectedLens)
            {
                Growl.Warning(Config.PrcessName + "-" + Properties.Resources.ErrorNoConnect);
                return;
            }
            if (IsRuning)
            {
                Growl.Warning(Config.PrcessName + "-" + Properties.Resources.ErrorNeedStop);
                return;
            }
            FocusDatas.Clear();
            FineFocusDatas.Clear();
            Task.Factory.StartNew(
                (Func<Task>)(
                    async () =>
                    {
                        try
                        {
                            IsCorrecting = false;
                            IsFocused = false;
                            IsFocusing = true;
                            oldTriggerMode = cam.Setting.TriggerMode;
                            cam.Setting.TriggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            WaitMoveTo(Config.SoftLimitN);
                            Thread.Sleep(500);
                            for (
                                float i = Config.SoftLimitN;
                                i <= Config.SoftLimitP;
                                i += Config.StepCoarse
                            )
                            {
                                cancellFocus.Token.ThrowIfCancellationRequested();
                                WaitMoveTo(i);
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                CImage image = cell.Image;

                                float distinct = FuncDistinct(image);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FocusDatas.Add(new(i, distinct));
                                });
                            }
                            float maxDistinct = FocusDatas.Max(o => o.distinct);
                            float focusPos = FocusDatas.First(o => o.distinct == maxDistinct).pos;
                            float focusPosN = Math.Max(
                                Config.SoftLimitN,
                                focusPos - Config.FineRange / 2
                            );
                            float focusPosP = Math.Min(
                                Config.SoftLimitP,
                                focusPos + Config.FineRange / 2
                            );
                            WaitMoveTo(focusPosN);
                            Thread.Sleep(500);
                            for (float i = focusPosN; i < focusPosP; i += Config.StepFine)
                            {
                                cancellFocus.Token.ThrowIfCancellationRequested();
                                WaitMoveTo(i);
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                CImage image = cell.Image;
                                float distinct = FuncDistinct(image);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FineFocusDatas.Add(new(i, distinct));
                                });
                            }
                            maxDistinct = FineFocusDatas.Max(o => o.distinct);
                            Config.FocusPos = FineFocusDatas
                                .First(o => o.distinct == maxDistinct)
                                .pos;
                            WaitMoveTo(Config.FocusPos);
                            cam.ExecuteSoftwareTrigger();
                            await FocusWaitGetImageChannel.Reader.ReadAsync();
                            Growl.Success(
                                Config.PrcessName + "-" + Properties.Resources.SuccessFocus
                            );
                            SysLog.Info(
                                Config.PrcessName + "-" + Properties.Resources.SuccessFocus
                            );
                            IsFocused = true;
                            Config.FocusSensorPos = SensorPos;
                            //modbusRtu.WriteSingleCoil(Config.SlaveSensor, 600, true);
                        }
                        catch (Exception ex)
                        {
                            Growl.Error(
                                Config.PrcessName
                                    + "-"
                                    + Properties.Resources.ErrorFocus
                                    + ex.Message
                            );
                            SysLog.Error(
                                Config.PrcessName
                                    + "-"
                                    + Properties.Resources.ErrorFocus
                                    + ex.Message
                            );
                        }
                        finally
                        {
                            IsFocusing = false;
                            cam.Setting.TriggerMode = oldTriggerMode;
                        }
                    }
                )
            );
        }
    }
}
