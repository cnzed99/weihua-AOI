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
using HandyControl.Controls;
using Motion;
using Newtonsoft.Json.Linq;
using WH.Controls;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace PlcControl
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 运动控件VM
    /// </summary>
    public partial class CMotionCtrlVM : CMotionVMBase
    {
        public CMotionCtrlVM()
            : base()
        {
            TestControl = new MotionCtrl(this);
            UpdateInfoAlarm();
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public override void SetRunning(bool isRuning)
        {
            base.SetRunning(isRuning);
            //modbusTcp?.WriteSingleCoil(MotionConfig.AddrStartMotion, isRuning);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CMotionConfig motionConfig;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// plc实例
        /// </summary>
        private CModbusTcp modbusTcp;

        private bool connected = false;

        /// <summary>
        /// 2025.3.6 李焕彬
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

        private bool isAlarmAxis = false;

        /// <summary>
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
        /// 初始化控制，包含连接、写入初始参数
        /// </summary>
        public override void InitControl()
        {
            base.InitControl();
            if (modbusTcp != null)
            {
                modbusTcp.Close();
                modbusTcp.actionConnect -= ConnectAction;
                modbusTcp.ReadElemData -= ModbusTcp_ReadElemData;
            }
            modbusTcp = CModbusTcp.Create(MotionConfig.IP, MotionConfig.Port);
            modbusTcp.actionConnect += ConnectAction;
            modbusTcp.ReadElemData += ModbusTcp_ReadElemData;
        }

        /// <summary>
        /// 连上后执行
        /// 2025.3.6 李焕彬
        /// </summary>
        /// <param name="isConnect"></param>
        void ConnectAction(bool isConnect)
        {
            if (this.Connected != isConnect)
            {
                this.Connected = isConnect;
                if (this.Connected)
                {
                    InitWrite();
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
        /// 2025.3.6 李焕彬
        /// 初始化写入
        /// </summary>
        public void InitWrite()
        {
            if (modbusTcp == null)
                return;
            WriteRegister();
            WriteSignal();
        }

        /// <summary>
        /// 2025.3.6 李焕彬
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

        /// <summary>
        /// 更新报警信息
        /// 2025.3.6 李焕彬
        /// </summary>
        private void UpdateInfoAlarm()
        {
            if (!Connected)
            {
                InfoAlarm = Properties.Resources.ConnectError;
            }
            else
            {
                InfoAlarm = "";
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 增加输入点
        /// </summary>
        [RelayCommand]
        public void AddInput()
        {
            MotionConfig.SignalIns.Add(new CSignalIn(MotionConfig.token, "自定义", 100));
        }

        /// <summary>
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
        /// 增加输出点
        /// </summary>
        [RelayCommand]
        public void AddOutput()
        {
            MotionConfig.SignalOuts.Add(new CSignalOut(MotionConfig.token, "自定义", 100));
        }

        /// <summary>
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
        /// 增加读写寄存器
        /// </summary>
        [RelayCommand]
        public void AddRegister()
        {
            MotionConfig.RegisterSets.Add(new CElement(MotionConfig.token));
        }

        /// <summary>
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
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
        /// 2025.3.6 李焕彬
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

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入输出点信号
        /// </summary>
        [RelayCommand]
        public void WriteSignal()
        {
            if (modbusTcp == null)
                return;
            foreach (var signalOut in MotionConfig.SignalOuts)
            {
                modbusTcp.WriteSingleCoil(signalOut.Addr, signalOut.Set);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
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

                List<CSignalIn> signalIn = MotionConfig.SignalIns.ToList();
                foreach (var e in signalIn)
                {
                    e.HasSignal = modbusTcp.ReadCoil(e.Addr);
                }

                List<CSignalOut> signalOut = MotionConfig.SignalOuts.ToList();
                foreach (var e in signalOut)
                {
                    e.HasSignal = modbusTcp.ReadCoil(e.Addr);
                }
            }
            catch (Exception err)
            {
                SysLog.Error(MotionConfig.PrcessName + "-" + err.Message);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 设置输出点信号
        /// </summary>
        /// <param name="signalOut">输出信号</param>
        [RelayCommand]
        public void SetOutput(CSignalOut signalOut)
        {
            if (modbusTcp == null)
                return;
            modbusTcp.WriteSingleCoil(signalOut.Addr, signalOut.Set);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 复位
        /// </summary>
        public override void Reset() { }
    }
}
