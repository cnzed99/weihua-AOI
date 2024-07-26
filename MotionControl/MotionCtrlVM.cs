using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace MotionControl
{
    /// <summary>
    /// 2024.7.9 李焕彬
    /// 对焦数据
    /// </summary>
    public record FocusData(float pos, float distinct);

    /// <summary>
    /// 2024.7.9 李焕彬
    /// 运动控件VM
    /// </summary>
    public partial class CMotionCtrlVM : ObservableObject
    {
        /// <summary>
        /// 2024.7.25 李焕彬
        /// 计算对焦清晰度
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="nLine"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        public static extern float CalcDistinct(int width, int height, int nLine, IntPtr data);

        public CMotionCtrlVM()
        {
            MotionConfig = LoadParameter();
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MotionConfig,
                MotionConfig.token
            );
            InitControl();
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 设置相机序列号
        /// </summary>
        /// <param name="cameraSerial">序列号</param>
        public void SetMotion(string cameraSerial)
        {
            this.CameraSerial = cameraSerial;
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public void SetRunning(bool isRuning)
        {
            this.IsRuning = isRuning;
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 相机序列号
        /// </summary>
        private string CameraSerial { get; set; }

        /// <summary>
        /// 20240725 TCG
        /// 当前制程是否启动
        /// </summary>
        private bool IsRuning { get; set; } = false;

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CMotionConfig motionConfig;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// plc实例
        /// </summary>
        private CModbusTcp modbusTcp = new CModbusTcp();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 连接信号
        /// </summary>
        [ObservableProperty]
        private bool connected = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴使能信号
        /// </summary>
        [ObservableProperty]
        private bool isEnable = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴报警信号
        /// </summary>
        [ObservableProperty]
        private bool isAlarmAxis = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴驱动报警信号
        /// </summary>
        [ObservableProperty]
        private bool isDriveAlarm = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 当前位置
        /// </summary>
        [ObservableProperty]
        private double curPos;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 当前速度
        /// </summary>
        [ObservableProperty]
        private double curSpeed;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 当前扭矩
        /// </summary>
        [ObservableProperty]
        private double curTarque;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦基准位
        /// </summary>
        [ObservableProperty]
        private double focusPos;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 纠偏期望位
        /// </summary>
        [ObservableProperty]
        private double focusPosDst;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 纠偏感应值
        /// </summary>
        [ObservableProperty]
        private double sensorPos;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 自动对焦状态
        /// </summary>
        [ObservableProperty]
        private bool isFocusing;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦请求停止
        /// </summary>
        private CancellationTokenSource cancellFocus;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 粗对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<FocusData> focusDatas = new();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 精对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<FocusData> fineFocusDatas = new();

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 通道数
        /// </summary>
        private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.DropWrite
        };

        /// <summary>
        /// 李焕彬 2024.7.24
        /// 对焦采集 图像队列
        /// </summary>
        public Channel<Cell> FocusWaitGetImageChannel = Channel.CreateBounded<Cell>(channelOptions);

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 初始化控制，包含连接、写入初始参数
        /// </summary>
        public void InitControl()
        {
            modbusTcp.Close();
            modbusTcp = new CModbusTcp();
            modbusTcp.ConnectToPLC(MotionConfig.IP, MotionConfig.Port);
            modbusTcp.actionConnect = (isConnect) =>
            {
                if (this.Connected != isConnect)
                {
                    this.Connected = isConnect;
                    if (this.Connected)
                    {
                        InitWrite();
                        modbusTcp.SendXYData = ModbusTcp_SendXYData;
                        modbusTcp.SendMxData = ModbusTcp_SendMxData;
                        modbusTcp.SendDxData = ModbusTcp_SendDxData;
                    }
                    else
                    {
                        Growl.Error(Properties.Resources.ConnectError);
                    }
                }
            };
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 初始化写入
        /// </summary>
        public void InitWrite()
        {
            modbusTcp.WriteCoilM(MotionConfig.AddrEnable, true);
            WriteRegister();
            WriteRegisterFix();
            WriteSignal();
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 连接命令
        /// </summary>
        [RelayCommand]
        public void Connect()
        {
            if (!Connected)
            {
                InitControl();
            }
            else
            {
                Growl.Warning(Properties.Resources.Connected);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 增加输入点
        /// </summary>
        [RelayCommand]
        public void AddInput()
        {
            MotionConfig.SignalIns.Add(new CSignalIn(MotionConfig.token, "X1", 1));
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 删除输入点
        /// </summary>
        /// <param name="obj">输入点集合</param>
        [RelayCommand]
        public void DelInput(object obj)
        {
            if (obj is IList signalIns)
            {
                for (int i = signalIns.Count - 1; i >= 0; i--)
                {
                    MotionConfig.SignalIns.Remove((CSignalIn)signalIns[i]);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 增加输出点
        /// </summary>
        [RelayCommand]
        public void AddOutput()
        {
            MotionConfig.SignalOuts.Add(new CSignalOut(MotionConfig.token, "Y1", 1));
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 删除输出点
        /// </summary>
        /// <param name="obj">输出点集合</param>
        [RelayCommand]
        public void DelOutput(object obj)
        {
            if (obj is IList signalOuts)
            {
                for (int i = signalOuts.Count - 1; i >= 0; i--)
                {
                    MotionConfig.SignalOuts.Remove((CSignalOut)signalOuts[i]);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 增加读写寄存器
        /// </summary>
        [RelayCommand]
        public void AddRegister()
        {
            MotionConfig.RegisterSets.Add(new CRegisterSet(MotionConfig.token));
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 删除读写寄存器
        /// </summary>
        /// <param name="obj">寄存器集合</param>
        [RelayCommand]
        public void DelRegister(object obj)
        {
            if (obj is IList registers)
            {
                for (int i = registers.Count - 1; i >= 0; i--)
                {
                    MotionConfig.RegisterSets.Remove((CRegisterSet)registers[i]);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入寄存器值
        /// </summary>
        [RelayCommand]
        public void WriteRegister()
        {
            foreach (var reg in MotionConfig.RegisterSets)
            {
                if (reg.Addr != null)
                    modbusTcp.WriteRegisterD(reg.Addr, reg.ValueWrite);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入固定参数寄存器值
        /// </summary>
        [RelayCommand]
        public void WriteRegisterFix()
        {
            modbusTcp.WriteRegisterD(MotionConfig.AddrFocusPos, MotionConfig.FocusPos);
            modbusTcp.WriteRegisterD(MotionConfig.AddrSoftLimitP, MotionConfig.SoftLimitP);
            modbusTcp.WriteRegisterD(MotionConfig.AddrSoftLimitN, MotionConfig.SoftLimitN);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入输出点信号
        /// </summary>
        [RelayCommand]
        public void WriteSignal()
        {
            foreach (var signalOut in MotionConfig.SignalOuts)
            {
                if (signalOut.AddrM != null)
                    modbusTcp.WriteCoilM(signalOut.AddrM, signalOut.Set);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 接收输入点、输出点状态并处理
        /// </summary>
        /// <param name="xState">输入点状态集合</param>
        /// <param name="yState">输出点状态集合</param>
        private void ModbusTcp_SendXYData(bool[] xState, bool[] yState)
        {
            foreach (var signal in MotionConfig.SignalIns.ToList())
            {
                try
                {
                    if (signal.Addr < xState.Length)
                    {
                        signal.HasSignal = xState[signal.Addr];
                    }
                }
                catch (Exception ex)
                {
                    Growl.Warning(signal + ":" + ex.Message);
                }
            }

            foreach (var signal in MotionConfig.SignalOuts.ToList())
            {
                try
                {
                    if (signal.Addr < yState.Length)
                    {
                        signal.HasSignal = yState[signal.Addr];
                    }
                }
                catch (Exception ex)
                {
                    Growl.Warning(signal + ":" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 接收线圈状态并处理
        /// </summary>
        /// <param name="mData">线圈状态集合</param>
        private void ModbusTcp_SendMxData(bool[] mData)
        {
            try
            {
                IsEnable = modbusTcp.ReadCoilM(mData, MotionConfig.AddrEnable);
                IsAlarmAxis = modbusTcp.ReadCoilM(mData, MotionConfig.AddrIsAlarm);
                IsDriveAlarm = modbusTcp.ReadCoilM(mData, MotionConfig.AddrDriveAlarm);
            }
            catch (Exception err)
            {
                Growl.Warning(err.Message);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 接收批量寄存器数据并处理
        /// </summary>
        /// <param name="dValue">寄存器数据集合</param>
        private void ModbusTcp_SendDxData(ushort[] dValue)
        {
            try
            {
                CurPos = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrPosCur);
                CurSpeed = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrSpdCur);
                CurTarque = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrTorqueCur);
                FocusPos = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrFocusPos);
                FocusPosDst = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrFocusDst);
                SensorPos = modbusTcp.ReadRegisterD(dValue, MotionConfig.AddrSensorPos);
                foreach (var reg in MotionConfig.RegisterSets)
                {
                    if (reg.Addr != null)
                    {
                        reg.ValueRead = modbusTcp.ReadRegisterD(dValue, reg.Addr);
                    }
                }
            }
            catch (Exception err)
            {
                Growl.Warning(err.Message);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 绝对值移动
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void AbsMove(double dis)
        {
            if (dis >= MotionConfig.SoftLimitN && dis <= MotionConfig.SoftLimitP)
            {
                modbusTcp.WriteRegisterD(MotionConfig.AddrPosAbs, (float)dis);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveAbs, true);
                Thread.Sleep(50);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveAbs, false);
            }
            else
            {
                Growl.Warning(Properties.Resources.PosOverLimit);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 相对移动正向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void RelaMoveP(double dis)
        {
            if (dis + CurPos <= MotionConfig.SoftLimitP)
            {
                modbusTcp.WriteRegisterD(MotionConfig.AddrPosRela, (float)dis);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveRela, true);
                Thread.Sleep(50);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveRela, false);
            }
            else
            {
                Growl.Warning(Properties.Resources.PosOverLimit);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 相对移动反向
        /// </summary>
        /// <param name="dis">移动距离</param>
        [RelayCommand]
        public void RelaMoveN(double dis)
        {
            if (CurPos - dis >= MotionConfig.SoftLimitN)
            {
                modbusTcp.WriteRegisterD(MotionConfig.AddrPosRela, (float)-dis);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveRela, true);
                Thread.Sleep(50);
                modbusTcp.WriteCoilM(MotionConfig.AddrMoveRela, false);
            }
            else
            {
                Growl.Warning(Properties.Resources.PosOverLimit);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 鼠标按下命令
        /// </summary>
        /// <param name="obj">地址</param>
        [RelayCommand]
        public void MouseDown(object obj)
        {
            if (obj is string para)
            {
                try
                {
                    modbusTcp.WriteCoilM(para, true);
                }
                catch (Exception err)
                {
                    Growl.Warning(err.Message);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 鼠标抬起命令
        /// </summary>
        /// <param name="obj">地址</param>
        [RelayCommand]
        public void MouseUp(object obj)
        {
            if (obj is string para)
            {
                try
                {
                    modbusTcp.WriteCoilM(para, false);
                }
                catch (Exception err)
                {
                    Growl.Warning(err.Message);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 使能
        /// </summary>
        [RelayCommand]
        public void Enable()
        {
            if (!IsEnable)
            {
                InitWrite();
            }
            modbusTcp.WriteCoilM(MotionConfig.AddrEnable, !IsEnable);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 设置输出点信号
        /// </summary>
        /// <param name="signalOut">输出信号</param>
        [RelayCommand]
        public void SetOutput(CSignalOut signalOut)
        {
            if (String.IsNullOrEmpty(signalOut.AddrM))
            {
                Growl.Warning(Properties.Resources.SetOutputError);
                return;
            }
            modbusTcp.WriteCoilM(signalOut.AddrM, signalOut.Set);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 自动对焦
        /// </summary>
        [RelayCommand]
        public void AutoFocus()
        {
            if (!CCameraManagement.CameraDict.ContainsKey(CameraSerial))
            {
                Growl.Warning("没有相机！");
                return;
            }
            CCameraBase cam = CCameraManagement.CameraDict[CameraSerial];
            if (!cam.Connected)
            {
                Growl.Warning("未打开相机！");
                return;
            }
            if (IsFocusing)
            {
                if (
                    HandyControl.Controls.MessageBox.Show(
                        "正在对焦中，是否停止对焦？",
                        "Tips",
                        MessageBoxButton.YesNo
                    ) == MessageBoxResult.Yes
                )
                {
                    IsFocusing = false;
                    cam.IsSetWindowShowed = false;
                    cancellFocus.Cancel();
                    return;
                }
            }
            cancellFocus = new CancellationTokenSource();

            if (!Connected)
            {
                Growl.Warning("运动控制未连接！");
                return;
            }
            if (IsRuning)
            {
                Growl.Warning("软件需要先暂停！");
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
                            cam.IsSetWindowShowed = true;
                            IsFocusing = true;
                            for (
                                float i = MotionConfig.SoftLimitN;
                                i < MotionConfig.SoftLimitP;
                                i += MotionConfig.StepCoarse
                            )
                            {
                                AbsMove(i);
                                while (Math.Abs(i - CurPos) > 0.01)
                                {
                                    Thread.Sleep(10);
                                    cancellFocus.Token.ThrowIfCancellationRequested();
                                    AbsMove(i);
                                }
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                CImage image = cell.Image;
                                float distinct = CalcDistinct(
                                    cell.Image.ImageWidth,
                                    cell.Image.ImageHeight,
                                    cell.Image.StrideWidth,
                                    cell.Image.ImageData
                                );
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FocusDatas.Add(new(i, distinct));
                                });
                            }
                            float focusPos = FocusDatas
                                .MaxBy((Func<FocusData, float>)(o => o.distinct))
                                .pos;
                            float focusPosN = Math.Max(
                                MotionConfig.SoftLimitN,
                                focusPos - MotionConfig.FineRange / 2
                            );
                            float focusPosP = Math.Min(
                                MotionConfig.SoftLimitP,
                                focusPos + MotionConfig.FineRange / 2
                            );
                            for (float i = focusPosN; i < focusPosP; i += MotionConfig.StepFine)
                            {
                                AbsMove(i);
                                while (Math.Abs(i - CurPos) > 0.01)
                                {
                                    Thread.Sleep(10);
                                    cancellFocus.Token.ThrowIfCancellationRequested();
                                    AbsMove(i);
                                }
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                CImage image = cell.Image;

                                float distinct = CalcDistinct(
                                    image.ImageWidth,
                                    image.ImageHeight,
                                    image.StrideWidth,
                                    image.ImageData
                                );
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FineFocusDatas.Add(new(i, distinct));
                                });
                            }
                            MotionConfig.FocusPos = FineFocusDatas
                                .MaxBy((Func<FocusData, float>)(o => o.distinct))
                                .pos;
                            AbsMove(MotionConfig.FocusPos);
                            while (Math.Abs(MotionConfig.FocusPos - CurPos) > 0.01)
                            {
                                Thread.Sleep(50);
                                cancellFocus.Token.ThrowIfCancellationRequested();
                                AbsMove(MotionConfig.FocusPos);
                            }
                            cam.ExecuteSoftwareTrigger();
                            await FocusWaitGetImageChannel.Reader.ReadAsync();
                            modbusTcp.WriteRegisterD(
                                MotionConfig.AddrFocusPos,
                                MotionConfig.FocusPos
                            );
                            Growl.Success("对焦完成！");
                        }
                        catch (Exception ex)
                        {
                            Growl.Error("对焦异常！" + ex.Message);
                        }
                        finally
                        {
                            cam.IsSetWindowShowed = false;
                            IsFocusing = false;
                        }
                    }
                )
            );
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 配置保存路径
        /// </summary>
        private const string c_ParameterPath = "..\\SystemConfig\\MotionConfig.Json";

        #region 保存参数
        /// <summary>
        /// 2024.7.12 李焕彬
        /// 控制配置保存方法
        /// </summary>
        public void SaveParameter()
        {
            try
            {
                ConfigAPI.Save(MotionConfig, c_ParameterPath);
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
        public CMotionConfig LoadParameter()
        {
            CMotionConfig config = new CMotionConfig();
            try
            {
                if (File.Exists(c_ParameterPath))
                {
                    config = ConfigAPI.Load<CMotionConfig>(c_ParameterPath);
                    if (config == null)
                    {
                        config = new CMotionConfig();
                    }
                }
                else
                {
                    config = new CMotionConfig();
                }
            }
            catch (Exception)
            {
                config = new CMotionConfig();
            }
            return config;
        }

        #endregion
    }
}
