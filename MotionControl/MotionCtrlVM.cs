using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using FocusControl;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using WH.Controls;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace MotionControl
{
    /// <summary>
    /// 2024.7.9 李焕彬
    /// 运动控件VM
    /// </summary>
    public partial class CMotionCtrlVM : CFocusCtrlVMBase
    {
        public CMotionCtrlVM()
            : base()
        {
            TestControl = new MotionCtrl(this);
            UpdateInfoAlarm();
        }

        /// <summary>
        /// 2024.7.9 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public override void SetRunning(bool isRuning)
        {
            base.SetRunning(isRuning);
            modbusTcp?.WriteSingleCoil(MotionConfig.AddrStartFocus, isRuning);
        }

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
        private CModbusTcp modbusTcp;

        private bool connected = false;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 连接信号
        /// </summary>
        public bool Connected
        {
            get { return connected; }
            set
            {
                SetProperty(ref connected, value);
                UpdateInfoAlarm();
            }
        }

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴使能信号
        /// </summary>
        [ObservableProperty]
        private bool isEnable = false;

        private bool isAlarmAxis = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴报警信号
        /// </summary>
        public bool IsAlarmAxis
        {
            get { return isAlarmAxis; }
            set
            {
                SetProperty(ref isAlarmAxis, value);
                UpdateInfoAlarm();
            }
        }

        private bool isDriveAlarm = false;

        /// <summary>
        /// 2024.7.11 李焕彬
        /// 轴驱动报警信号
        /// </summary>
        public bool IsDriveAlarm
        {
            get { return isDriveAlarm; }
            set
            {
                SetProperty(ref isDriveAlarm, value);
                UpdateInfoAlarm();
            }
        }

        /// <summary>
        /// 2024.12.18 李焕彬
        /// 原点信号
        /// </summary>
        [ObservableProperty]
        private bool homeSignal = false;

        /// <summary>
        /// 2024.12.18 李焕彬
        /// 正向限位信号
        /// </summary>
        [ObservableProperty]
        private bool limitPSignal = false;

        /// <summary>
        /// 2024.12.18 李焕彬
        /// 负向限位信号
        /// </summary>
        [ObservableProperty]
        private bool limitNSignal = false;

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
        /// 对焦请求停止
        /// </summary>
        private CancellationTokenSource cancellFocus;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 粗对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CFocusData> focusDatas = new();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 精对焦数据
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CFocusData> fineFocusDatas = new();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 初始化控制，包含连接、写入初始参数
        /// </summary>
        public override void InitControl()
        {
            base.InitControl();
            if (modbusTcp != null)
            {
                modbusTcp.Close();
                modbusTcp.actionConnect -= ConnectAction;
                modbusTcp.SendXYData -= ModbusTcp_SendXYData;
                modbusTcp.ReadElemData -= ModbusTcp_ReadElemData;
            }
            modbusTcp = CModbusTcp.Create(MotionConfig.IP, MotionConfig.Port);
            modbusTcp.actionConnect += ConnectAction;
            modbusTcp.SendXYData += ModbusTcp_SendXYData;
            modbusTcp.ReadElemData += ModbusTcp_ReadElemData;
        }

        void ConnectAction(bool isConnect)
        {
            if (this.Connected != isConnect)
            {
                this.Connected = isConnect;
                if (this.Connected)
                {
                    InitWrite();
                    Growl.AskGlobal(
                        MotionConfig.PrcessName + "-" + Properties.Resources.AskGoHome,
                        b =>
                        {
                            if (b)
                            {
                                Thread.Sleep(20);
                                //回原点状态置true;
                                modbusTcp.WriteSingleCoil(MotionConfig.AddrGoHome, true);
                                Thread.Sleep(50);
                                //回原点状态置false
                                modbusTcp.WriteSingleCoil(MotionConfig.AddrGoHome, false);
                            }
                            return true;
                        }
                    );
                    Growl.Success(
                        MotionConfig.PrcessName + "-" + Properties.Resources.SuccessConnect
                    );
                    SysLog.Info(
                        MotionConfig.PrcessName + "-" + Properties.Resources.SuccessConnect
                    );
                }
                else
                {
                    Growl.Error(MotionConfig.PrcessName + "-" + Properties.Resources.ConnectError);
                    SysLog.Error(MotionConfig.PrcessName + "-" + Properties.Resources.ConnectError);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 初始化写入
        /// </summary>
        public void InitWrite()
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleCoil(MotionConfig.AddrEnable, true);
            //WriteRegister();
            WriteRegisterFix();
            //WriteSignal();
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
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.Connected);
            }
        }

        private void UpdateInfoAlarm()
        {
            if (!Connected)
            {
                InfoAlarm = Properties.Resources.ConnectError;
            }
            else if (IsAlarmAxis && IsDriveAlarm)
            {
                InfoAlarm = Properties.Resources.AxisAndMotionAlarm;
            }
            else if (IsAlarmAxis)
            {
                InfoAlarm = Properties.Resources.AxisAlarm;
            }
            else if (IsDriveAlarm)
            {
                InfoAlarm = Properties.Resources.MotionAlarm;
            }
            else
            {
                InfoAlarm = "";
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
                if (signalIns.Count > 0)
                {
                    Growl.AskGlobal(
                        MotionConfig.PrcessName + "-" + Properties.Resources.DelecteAsk,
                        b =>
                        {
                            if (b)
                            {
                                for (int i = signalIns.Count - 1; i >= 0; i--)
                                {
                                    MotionConfig.SignalIns.Remove((CSignalIn)signalIns[i]);
                                }
                            }
                            return true;
                        }
                    );
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
                if (signalOuts.Count > 0)
                {
                    Growl.AskGlobal(
                        MotionConfig.PrcessName + "-" + Properties.Resources.DelecteAsk,
                        b =>
                        {
                            if (b)
                            {
                                for (int i = signalOuts.Count - 1; i >= 0; i--)
                                {
                                    MotionConfig.SignalOuts.Remove((CSignalOut)signalOuts[i]);
                                }
                            }
                            return true;
                        }
                    );
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
            MotionConfig.RegisterSets.Add(new CElement(MotionConfig.token));
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 删除读写寄存器
        /// </summary>
        /// <param name="registerSet">寄存器</param>
        [RelayCommand]
        public void DelRegister(CElement registerSet)
        {
            Growl.AskGlobal(
                MotionConfig.PrcessName + "-" + Properties.Resources.DelecteAsk,
                b =>
                {
                    if (b)
                    {
                        MotionConfig.RegisterSets.Remove(registerSet);
                    }
                    return true;
                }
            );
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入寄存器值
        /// </summary>
        [RelayCommand]
        public void WriteRegister()
        {
            if (modbusTcp == null)
                return;
            foreach (var reg in MotionConfig.RegisterSets)
            {
                switch (reg.Type)
                {
                    case EMELEMTYPE.EMELEMM:
                        modbusTcp.WriteSingleCoil(reg.Addr, reg.WriteValue == 1);
                        break;
                    case EMELEMTYPE.EMELEMD:
                        modbusTcp.WriteSingleRegister(reg.Addr, reg.WriteValue);
                        break;
                }
            }
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 写入单个寄存器
        /// </summary>
        /// <param name="registerSet">寄存器</param>
        [RelayCommand]
        public void WriteSingleRegister(CElement registerSet)
        {
            if (modbusTcp == null)
                return;
            switch (registerSet.Type)
            {
                case EMELEMTYPE.EMELEMM:
                    modbusTcp.WriteSingleCoil(registerSet.Addr, registerSet.WriteValue == 1);
                    break;
                case EMELEMTYPE.EMELEMD:
                    modbusTcp.WriteSingleRegister(registerSet.Addr, registerSet.WriteValue);
                    break;
            }
        }

        ///// <summary>
        ///// 2024.7.12 李焕彬
        ///// 鼠标按下命令
        ///// </summary>
        ///// <param name="obj">地址</param>
        //[RelayCommand]
        //public void MouseDown2(CElement registerSet)
        //{
        //    if (modbusTcp == null)
        //        return;
        //    WriteSingleRegister(registerSet);
        //}

        ///// <summary>
        ///// 2024.7.12 李焕彬
        ///// 鼠标抬起命令
        ///// </summary>
        ///// <param name="obj">地址</param>
        //[RelayCommand]
        //public void MouseUp2(CElement registerSet)
        //{
        //    if (modbusTcp == null)
        //        return;
        //    if (registerSet.Type == EMELEMTYPE.EMELEMM)
        //    {
        //        try
        //        {
        //            modbusTcp.WriteSingleCoil(registerSet.Addr, false);
        //        }
        //        catch (Exception err)
        //        {
        //            Growl.Error(MotionConfig.PrcessName + "-" + err.Message);
        //            SysLog.Error(MotionConfig.PrcessName + "-" + err.Message);
        //        }
        //    }
        //}

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入固定参数寄存器值
        /// </summary>
        [RelayCommand]
        public void WriteRegisterFix()
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleRegister(MotionConfig.AddrFocusPos, MotionConfig.FocusPos);
            modbusTcp.WriteSingleRegister(MotionConfig.AddrAcc, MotionConfig.Acc);
            modbusTcp.WriteSingleRegister(MotionConfig.AddrSpeed, MotionConfig.Speed);
            modbusTcp.WriteSingleRegister(MotionConfig.AddrLimitPPos, MotionConfig.SoftLimitP);
            modbusTcp.WriteSingleRegister(MotionConfig.AddrLimitNPos, MotionConfig.SoftLimitN);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入输出点信号
        /// </summary>
        [RelayCommand]
        public void WriteSignal()
        {
            if (modbusTcp == null)
                return;
            foreach (var signalOut in MotionConfig.SignalOuts)
            {
                modbusTcp.WriteSingleCoil(signalOut.AddrM, signalOut.Set);
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
                    SysLog.Error(MotionConfig.PrcessName + "-" + signal + ":" + ex.Message);
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
                    SysLog.Error(MotionConfig.PrcessName + "-" + signal + ":" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读取元件数据
        /// </summary>
        private void ModbusTcp_ReadElemData()
        {
            if (modbusTcp == null)
                return;
            try
            {
                List<CElement> cElements = MotionConfig.RegisterSets.ToList();
                foreach (CElement e in cElements)
                {
                    switch (e.Type)
                    {
                        case EMELEMTYPE.EMELEMM:
                            e.ReadValue = modbusTcp.ReadCoil(e.Addr) ? 1 : 0;
                            break;
                        case EMELEMTYPE.EMELEMD:
                            e.ReadValue = modbusTcp.ReadHoldingRegister(e.Addr);
                            break;
                    }
                }

                IsEnable = modbusTcp.ReadCoil(MotionConfig.AddrEnable);
                IsAlarmAxis = modbusTcp.ReadCoil(MotionConfig.AddrIsAlarm);
                IsDriveAlarm = modbusTcp.ReadCoil(MotionConfig.AddrDriveAlarm);
                HomeSignal = modbusTcp.ReadCoil(MotionConfig.AddrHomeSignal);
                LimitPSignal = modbusTcp.ReadCoil(MotionConfig.AddrLimitPSignal);
                LimitNSignal = modbusTcp.ReadCoil(MotionConfig.AddrLimitNSignal);

                CurPos = modbusTcp.ReadHoldingRegister(MotionConfig.AddrPosCur);
                CurSpeed = modbusTcp.ReadHoldingRegister(MotionConfig.AddrSpdCur);
                CurTarque = modbusTcp.ReadHoldingRegister(MotionConfig.AddrTorqueCur);
                FocusPosDst = modbusTcp.ReadHoldingRegister(MotionConfig.AddrFocusDst);
                SensorPos = modbusTcp.ReadHoldingRegister(MotionConfig.AddrSensorPos);
            }
            catch (Exception err)
            {
                SysLog.Error(MotionConfig.PrcessName + "-" + err.Message);
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
            if (modbusTcp == null)
                return;
            if (dis >= MotionConfig.SoftLimitN && dis <= MotionConfig.SoftLimitP)
            {
                modbusTcp.WriteSingleRegister(MotionConfig.AddrPosAbs, (float)dis);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveAbs, true);
                Thread.Sleep(50);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveAbs, false);
            }
            else
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.PosOverLimit);
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
            if (modbusTcp == null)
                return;
            if (dis + CurPos <= MotionConfig.SoftLimitP)
            {
                modbusTcp.WriteSingleRegister(MotionConfig.AddrPosRela, (float)dis);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveRela, true);
                Thread.Sleep(50);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveRela, false);
            }
            else
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.PosOverLimit);
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
            if (modbusTcp == null)
                return;
            if (CurPos - dis >= MotionConfig.SoftLimitN)
            {
                modbusTcp.WriteSingleRegister(MotionConfig.AddrPosRela, (float)-dis);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveRela, true);
                Thread.Sleep(50);
                modbusTcp.WriteSingleCoil(MotionConfig.AddrMoveRela, false);
            }
            else
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.PosOverLimit);
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
            if (modbusTcp == null)
                return;
            if (obj is ushort para)
            {
                try
                {
                    modbusTcp.WriteSingleCoil(para, true);
                }
                catch (Exception err)
                {
                    Growl.Error(MotionConfig.PrcessName + "-" + err.Message);
                    SysLog.Error(MotionConfig.PrcessName + "-" + err.Message);
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
            if (modbusTcp == null)
                return;
            if (obj is ushort para)
            {
                try
                {
                    modbusTcp.WriteSingleCoil(para, false);
                }
                catch (Exception err)
                {
                    Growl.Error(MotionConfig.PrcessName + "-" + err.Message);
                    SysLog.Error(MotionConfig.PrcessName + "-" + err.Message);
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
            if (modbusTcp == null)
                return;
            //if (!IsEnable)
            //{
            //    InitWrite();
            //}
            modbusTcp.WriteSingleCoil(MotionConfig.AddrEnable, !IsEnable);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 设置输出点信号
        /// </summary>
        /// <param name="signalOut">输出信号</param>
        [RelayCommand]
        public void SetOutput(CSignalOut signalOut)
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleCoil(signalOut.AddrM, signalOut.Set);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 设置速度
        /// </summary>
        /// <param name="dis">移动速度/param>
        public void SetSpeed(float speed)
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleRegister(MotionConfig.AddrSpeed, speed);
        }

        /// <summary>
        /// 2024.8.12 李焕彬
        /// 纠偏归零
        /// </summary>
        public void SetZero()
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleCoil(MotionConfig.AddrSetZero, true);
            Thread.Sleep(100);
            modbusTcp.WriteSingleCoil(MotionConfig.AddrSetZero, false);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 绝对值移动，等待到位
        /// </summary>
        /// <param name="dis">位置</param>
        public void WaitMoveTo(float pos)
        {
            AbsMove(pos);
            while (Math.Abs(pos - CurPos) > 0.01)
            {
                Thread.Sleep(10);
                cancellFocus.Token.ThrowIfCancellationRequested();
                AbsMove(pos);
            }
        }

        /// <summary>
        /// 2024.8.5 李焕彬
        /// 相机旧触发模式
        /// </summary>
        private EMTRIGGERMODE oldTriggerMode;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 自动对焦
        /// </summary>
        [RelayCommand]
        public void AutoFocus()
        {
            if (modbusTcp == null)
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
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.ErrorNoCam);
                return;
            }
            CCameraBase cam = CCameraManagement.CameraDict[CameraSerial];
            if (!cam.Connected)
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.ErrorNoOpenCam);
                return;
            }
            cancellFocus = new CancellationTokenSource();

            if (!Connected)
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.ErrorNoConnect);
                return;
            }
            if (IsRuning)
            {
                Growl.Warning(MotionConfig.PrcessName + "-" + Properties.Resources.ErrorNeedStop);
                return;
            }
            FocusDatas.Clear();
            FineFocusDatas.Clear();
            while (FocusWaitGetImageChannel.Reader.TryRead(out Cell cell))
            {
                cell.Dispose();
            }
            ;
            Task.Factory.StartNew(
                (Func<Task>)(
                    async () =>
                    {
                        try
                        {
                            IsFocused = false;
                            IsFocusing = true;
                            oldTriggerMode = cam.Setting.TriggerMode;
                            cam.Setting.TriggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                            SetSpeed(MotionConfig.SpeedFocus);
                            WaitMoveTo(MotionConfig.SoftLimitN);
                            Thread.Sleep(300);
                            AbsMove(MotionConfig.SoftLimitP);
                            while (Math.Abs(MotionConfig.SoftLimitP - CurPos) > 0.01)
                            {
                                cancellFocus.Token.ThrowIfCancellationRequested();
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                if (!cell.FrameLoss)
                                {
                                    CImage image = cell.Image;
                                    float distinct =
                                        FuncDistinct != null ? FuncDistinct.Invoke(cell.Image) : 0;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        FocusDatas.Add(new((float)CurPos, distinct));
                                    });
                                }
                                cell.Dispose();
                            }

                            float maxDistinct = FocusDatas.Max(o => o.distinct);
                            float focusPos = FocusDatas.First(o => o.distinct == maxDistinct).pos;
                            float focusPosN = Math.Max(
                                MotionConfig.SoftLimitN,
                                focusPos - MotionConfig.FineRange / 2
                            );
                            float focusPosP = Math.Min(
                                MotionConfig.SoftLimitP,
                                focusPos + MotionConfig.FineRange / 2
                            );
                            WaitMoveTo(focusPosN);
                            for (float i = focusPosN; i < focusPosP; i += MotionConfig.StepFine)
                            {
                                cancellFocus.Token.ThrowIfCancellationRequested();
                                WaitMoveTo(i);
                                cam.ExecuteSoftwareTrigger();
                                Cell cell = await FocusWaitGetImageChannel.Reader.ReadAsync();
                                if (!cell.FrameLoss)
                                {
                                    CImage image = cell.Image;
                                    float distinct =
                                        FuncDistinct != null ? FuncDistinct.Invoke(cell.Image) : 0;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        FineFocusDatas.Add(new(i, distinct));
                                    });
                                }
                                cell.Dispose();
                            }
                            maxDistinct = FineFocusDatas.Max(o => o.distinct);
                            MotionConfig.FocusPos = FineFocusDatas
                                .First(o => o.distinct == maxDistinct)
                                .pos;
                            WaitMoveTo(MotionConfig.FocusPos);
                            cam.ExecuteSoftwareTrigger();
                            await FocusWaitGetImageChannel.Reader.ReadAsync();
                            modbusTcp.WriteSingleRegister(
                                MotionConfig.AddrFocusPos,
                                MotionConfig.FocusPos
                            );
                            WriteRegisterFix();
                            Growl.Success(
                                MotionConfig.PrcessName + "-" + Properties.Resources.SuccessFocus
                            );
                            SysLog.Info(
                                MotionConfig.PrcessName + "-" + Properties.Resources.SuccessFocus
                            );
                            IsFocused = true;
                            SetZero();
                        }
                        catch (Exception ex)
                        {
                            Growl.Error(
                                MotionConfig.PrcessName
                                    + "-"
                                    + Properties.Resources.ErrorFocus
                                    + ex.Message
                            );
                            SysLog.Error(
                                MotionConfig.PrcessName
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

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 复位
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleCoil(MotionConfig.AddrReset, true);
            Thread.Sleep(50);
            modbusTcp.WriteSingleCoil(MotionConfig.AddrReset, false);
        }
    }
}
