using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using NModbus;
using static System.Net.Mime.MediaTypeNames;

namespace PlcControl
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 与PLC通讯,采用modbusTCP协议
    /// </summary>
    public class CModbusTcp
    {
        /// <summary>
        /// 静态CModbusTcp集合
        /// </summary>
        public static List<CModbusTcp> s_modbusTcps = new List<CModbusTcp>();

        /// <summary>
        /// 创建对象或返回IP和端口号一致的对象
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        public static CModbusTcp Create(string ip, int port)
        {
            CModbusTcp modbusTcp = s_modbusTcps.FirstOrDefault(o => o.ip == ip && o.port == port);
            if (modbusTcp != null)
            {
                modbusTcp.userCount++;
                return modbusTcp;
            }
            CModbusTcp modbus = new CModbusTcp(ip, port);
            s_modbusTcps.Add(modbus);
            return modbus;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// tcp
        /// </summary>
        private TcpClient tcpClient = new TcpClient();

        /// <summary>
        /// 2025.3.6 李焕彬
        /// ModbusFactory
        /// </summary>
        private ModbusFactory factory;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// IModbusMaster接口
        /// </summary>
        private IModbusMaster master;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 传送XY的状态
        /// </summary>
        public Action<bool[], bool[]> SendXYData;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取元件事件
        /// </summary>
        public Action ReadElemData;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 循环标志
        /// </summary>
        private bool isStart = false;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// TCP线程
        /// </summary>
        private Task taskTcp = null;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 连接事件
        /// </summary>
        public Action<bool> actionConnect;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 比例转换,暂时用1
        /// </summary>
        public static float s_Convert = 1.0f;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 站地址
        /// </summary>
        private byte slaveAddress = 0x01;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// ip地址
        /// </summary>
        private string ip;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 端口号
        /// </summary>
        private int port;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 引用数目
        /// </summary>
        private int userCount = 1;

        public CModbusTcp(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            ConnectToPLC();
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 连接到PLC
        /// </summary>
        public void ConnectToPLC()
        {
            taskTcp = Task.Factory.StartNew(() =>
            {
                try
                {
                    //建立连接
                    tcpClient.Connect(ip, port);
                    factory = new ModbusFactory();
                    master = factory.CreateMaster(tcpClient);
                    master.Transport.ReadTimeout = 1000;
                    master.Transport.WriteTimeout = 1000;
                    master.Transport.Retries = 10;
                    isStart = true;
                    OnRefresh();
                }
                catch (Exception err)
                {
                    Growl.Error(err.Message);
                    CMotionCtrlVM.SysLog.Error(err.Message);
                }
            });
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 实时刷新数据
        /// </summary>
        private void OnRefresh()
        {
            while (isStart)
            {
                try
                {
                    if (tcpClient.Connected)
                    {
                        //读取X0-X17状态   //0xF800‑0xFBFF
                        //bool[] xState = master.ReadInputs(0x01, 0x0, 1610);
                        //bool[] xState = master.ReadCoils(0x01, 0x0, 2047);
                        //读取Y0-Y17状态   //0xFC00‑0xFFFF
                        //bool[] yState = master.ReadCoils(0x01, 0x0, 14999);
                        //List<bool> xyState = new List<bool>();
                        //数组合并
                        //xyState.AddRange(xState);
                        //xyState.AddRange(yState);
                        //触发事件，传递XY的状态
                        //SendXYData?.Invoke(xState, yState);
                        //触发事件，读取元件
                        ReadElemData?.Invoke();
                        //plc连接事件
                        actionConnect?.Invoke(true);
                        Thread.Sleep(10);
                    }
                    else
                    {
                        //plc断开事件
                        actionConnect?.Invoke(false);
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 关闭线程
        /// </summary>
        public void Close()
        {
            userCount--;
            if (userCount == 0)
            {
                isStart = false;
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取输入点状态
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">点数量</param>
        /// <returns></returns>
        private bool[] ReadInputs(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            if (master == null)
            {
                return null;
            }
            return master.ReadInputs(slaveAddress, startAddress, numberOfPoints);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取线圈状态
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">数量</param>
        /// <returns></returns>
        private bool[] ReadCoils(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            if (master == null)
            {
                return null;
            }
            return master.ReadCoils(slaveAddress, startAddress, numberOfPoints);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个线圈的数据
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public async Task WriteSingleCoilAsync(byte slaveAddress, ushort startAddress, bool value)
        {
            if (master == null)
            {
                return;
            }
            await master.WriteSingleCoilAsync(slaveAddress, startAddress, value);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取寄存器数据
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">数量</param>
        /// <returns></returns>
        private ushort[] ReadHoldingRegisters(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints
        )
        {
            if (master == null)
            {
                return null;
            }
            return master.ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public async Task WriteSingleRegisterAsync(
            byte slaveAddress,
            ushort registerAddress,
            ushort value
        )
        {
            if (master == null)
            {
                return;
            }
            await master.WriteSingleRegisterAsync(slaveAddress, registerAddress, value);
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入多个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="data">写入数组</param>
        /// <returns></returns>
        public async Task WriteMultipleRegistersAsync(
            byte slaveAddress,
            ushort startAddress,
            ushort[] data
        )
        {
            if (master == null)
            {
                return;
            }
            await master.WriteMultipleRegistersAsync(slaveAddress, startAddress, data);
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
                if (tcpClient.Connected)
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
                CMotionCtrlVM.SysLog.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 读取寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public float ReadHoldingRegister(ushort startAddress)
        {
            try
            {
                if (tcpClient.Connected)
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
                CMotionCtrlVM.SysLog.Error(ex.Message + ex.StackTrace);
            }

            return 0;
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
                if (tcpClient.Connected)
                {
                    master.WriteSingleCoil(slaveAddress, startAddress, value);
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + ex.StackTrace);
                CMotionCtrlVM.SysLog.Error(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 写入单个寄存器
        /// </summary>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleRegister(ushort registerAddress, float value)
        {
            try
            {
                if (tcpClient.Connected)
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
                CMotionCtrlVM.SysLog.Error(ex.Message + ex.StackTrace);
            }
        }
    }
}
