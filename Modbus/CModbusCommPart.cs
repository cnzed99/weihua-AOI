using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunicationModule;
using HandyControl.Controls;
using LanguageManager;
using NModbus;
using NModbus.Extensions.Enron;

namespace Modbus
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// Modbus通讯
    /// </summary>
    public class CModbusCommPart : CCommunicationBase
    {
        /// <summary>
        /// 2024.7.21 李焕彬
        /// tcp
        /// </summary>
        private TcpClient tcpClient = null;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// ModbusFactory
        /// </summary>
        private ModbusFactory factory;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// IModbusMaster接口
        /// </summary>
        private IModbusMaster master;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 参数
        /// </summary>
        public CModbusSetting setting; //参数

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 连接后触发事件
        /// </summary>
        public override event Action<bool, string> ConnectedEventArgs;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 是否读取测试元件
        /// </summary>
        public bool isReadTestElem = false;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读取线程
        /// </summary>
        private Task taskRead = null;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 站地址
        /// </summary>
        private byte slaveAddress = 0x01;

        public CModbusCommPart(CModbusSetting prama)
        {
            setting = prama;
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    base.TestControl = new TestControl(this);
                })
            );
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 连接
        /// </summary>
        public override void Connect()
        {
            try
            {
                if (setting.Enable) //是否启用
                {
                    CCommunicationManagement.ComLogger.Info(
                        "TCP连接线程开启,准备连接服务器:" + this.setting.RemoteIP
                    );
                    if (this.tcpClient != null)
                    {
                        Close();
                    }
                    tcpClient = new TcpClient();
                    tcpClient.Connect(setting.RemoteIP, (int)setting.RemotePort);
                    if (tcpClient.Connected)
                    {
                        factory = new ModbusFactory();
                        master = factory.CreateMaster(tcpClient);
                        master.Transport.ReadTimeout = 1000;
                        master.Transport.WriteTimeout = 1000;
                        master.Transport.Retries = 10;
                        IsConnected = true;
                        CCommunicationManagement.ComLogger.Info(
                            "连接服务器:" + this.setting.RemoteIP + "成功!"
                        );
                        CCommunicationManagement.SysLog.Info(
                            "连接服务器:" + this.setting.RemoteIP + "成功!"
                        );
                        ConnectedEventArgs?.Invoke(IsConnected, "连接状态:已连接");
                        taskRead = Task.Factory.StartNew(OnRefresh);
                    }
                    else
                    {
                        CCommunicationManagement.ComLogger.Info("连接服务器失败：" + this.setting.RemoteIP);
                        ConnectedEventArgs?.Invoke(IsConnected, "连接服务器失败：" + this.setting.RemoteIP);
                    }
                }
                else
                {
                    ConnectedEventArgs?.Invoke(false, "连接状态:本通讯未启用");
                }
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    "连接服务器:" + this.setting.RemoteIP + "出现异常:" + ex.Message
                );
                Close();
                throw;
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 关闭
        /// </summary>
        /// <returns>true关闭成功，false关闭失败</returns>
        public override bool Close()
        {
            try
            {
                if (tcpClient != null)
                {
                    IsConnected = false;
                    while (taskRead != null && !taskRead.IsCompleted)
                        Thread.Sleep(50);
                    master?.Dispose();
                    tcpClient?.Close();
                    tcpClient?.Dispose();
                    tcpClient = null;
                    ConnectedEventArgs?.Invoke(IsConnected, "连接状态:已断开连接");
                    CCommunicationManagement.ComLogger.Info("关闭连接:" + this.setting.RemoteIP);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 实时刷新数据
        /// </summary>
        private void OnRefresh()
        {
            while (IsConnected)
            {
                try
                {
                    if (tcpClient != null && tcpClient.Connected)
                    {
                        if (isReadTestElem)
                        {
                            List<CElement> cElements = setting.TestElems.ToList();
                            foreach (CElement e in cElements)
                            {
                                switch (e.Type)
                                {
                                    case EMELEMTYPE.EMELEMM:
                                        e.ReadValue = ReadCoil(e.Addr) ? 1 : 0;
                                        break;

                                    case EMELEMTYPE.EMELEMD_REAL:
                                        e.ReadValue = ReadHoldingRegisterReal(e.Addr);
                                        break;

                                    case EMELEMTYPE.EMELEMD_INT:
                                        e.ReadValue = ReadHoldingRegisterInt16(e.Addr);
                                        break;

                                    case EMELEMTYPE.EMELEMD_DINT:
                                        e.ReadValue = ReadHoldingRegisterInt32(e.Addr);
                                        break;
                                }
                            }
                        }
                        Thread.Sleep(100);
                    }
                    else
                    {
                        IsConnected = false;
                        this.ConnectedEventArgs?.Invoke(false, "与服务器断开连接");
                        CCommunicationManagement.ComLogger.Error("与服务器断开连接");
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 创建协议
        /// </summary>
        /// <returns>协议指令集</returns>
        public override IList CreateProtocol()
        {
            return new ObservableCollection<CElement>();
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 获取语言包
        /// </summary>
        /// <returns>测试控件对象</returns>
        public override CLanguageManager GetLanguage()
        {
            return CLang.s_Instance;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 发送数据
        /// </summary>
        /// <param name="list">协议指令集</param>
        public override void Send(IList list)
        {
            ObservableCollection<CElement> elements = list as ObservableCollection<CElement>;
            foreach (CElement element in elements)
            {
                switch (element.Type)
                {
                    case EMELEMTYPE.EMELEMM:
                        WriteSingleCoil(element.Addr, element.WriteValue >= 1);
                        break;

                    case EMELEMTYPE.EMELEMD_REAL:
                        WriteSingleRegisterReal(element.Addr, element.WriteValue);
                        break;

                    case EMELEMTYPE.EMELEMD_INT:
                        WriteSingleRegisterInt16(element.Addr, (Int16)element.WriteValue);
                        break;

                    case EMELEMTYPE.EMELEMD_DINT:
                        WriteSingleRegisterInt32(element.Addr, (Int32)element.WriteValue);
                        break;
                }
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取线圈状态
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>状态</returns>
        public bool ReadCoil(ushort startAddress)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    bool[] bools = master.ReadCoils(slaveAddress, startAddress, 1);
                    if (bools.Length == 1)
                    {
                        return bools[0];
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public float ReadHoldingRegisterReal(ushort startAddress)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    var value = master.ReadHoldingRegisters(slaveAddress, startAddress, 2);
                    if (value.Length == 2)
                    {
                        byte[] data = BitConverter.GetBytes(value[0] + (value[1] << 16));
                        return BitConverter.ToSingle(data, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }

            return -1;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public Int16 ReadHoldingRegisterInt16(ushort startAddress)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    var value = master.ReadHoldingRegisters(slaveAddress, startAddress, 1);
                    if (value.Length == 1)
                    {
                        byte[] data = BitConverter.GetBytes(value[0]);
                        return BitConverter.ToInt16(data, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }

            return -1;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public Int32 ReadHoldingRegisterInt32(ushort startAddress)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    var value = master.ReadHoldingRegisters(slaveAddress, startAddress, 2);
                    if (value.Length == 2)
                    {
                        byte[] data = BitConverter.GetBytes(value[0] + (value[1] << 16));
                        return BitConverter.ToInt32(data, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }

            return -1;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个线圈的数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleCoil(ushort startAddress, bool value)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    bool[] data = new bool[1] { value };
                    master.WriteMultipleCoils(slaveAddress, startAddress, data);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个寄存器REAL
        /// </summary>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleRegisterReal(ushort registerAddress, float value)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    byte[] fData = BitConverter.GetBytes(value);
                    ushort[] Data = new ushort[2];
                    Data[0] = (ushort)((fData[1] << 8) + fData[0]);
                    Data[1] = (ushort)((fData[3] << 8) + fData[2]);

                    master.WriteMultipleRegisters(slaveAddress, registerAddress, Data);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个寄存器INT
        /// </summary>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleRegisterInt16(ushort registerAddress, Int16 value)
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    byte[] fData = BitConverter.GetBytes(value);
                    ushort[] Data = new ushort[1];
                    Data[0] = (ushort)((fData[1] << 8) + fData[0]);

                    master.WriteMultipleRegisters(slaveAddress, registerAddress, Data);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个寄存器DINT
        /// </summary>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleRegisterInt32(ushort registerAddress, Int32 value)
        {
            try
            {
                if (tcpClient!=null&&tcpClient.Connected)
                {
                    byte[] fData = BitConverter.GetBytes(value);
                    ushort[] Data = new ushort[2];
                    Data[0] = (ushort)((fData[1] << 8) + fData[0]);
                    Data[1] = (ushort)((fData[3] << 8) + fData[2]);

                    master.WriteMultipleRegisters(slaveAddress, registerAddress, Data);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
            }
        }
    }
}