using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HandyControl.Controls;
using NModbus;
using static System.Net.Mime.MediaTypeNames;

namespace MotionControl
{
    /// <summary>
    /// 2024.7.12 李焕彬
    /// 与PLC通讯,采用modbusTCP协议
    /// </summary>
    public class CModbusTcp
    {
        /// <summary>
        /// 2024.7.12 李焕彬
        /// tcp
        /// </summary>
        private TcpClient tcpClient = new TcpClient();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// ModbusFactory
        /// </summary>
        private ModbusFactory factory;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// IModbusMaster接口
        /// </summary>
        private IModbusMaster master;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 传送XY的状态
        /// </summary>
        public Action<bool[], bool[]> SendXYData;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 传送M的状态
        /// </summary>
        public Action<bool[]> SendMxData;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 传送D区的数据
        /// </summary>
        public Action<ushort[]> SendDxData;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 循环标志
        /// </summary>
        private bool isStart = false;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// TCP线程
        /// </summary>
        private Task taskTcp = null;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 连接事件
        /// </summary>
        public Action<bool> actionConnect;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 比例转换,暂时用1
        /// </summary>
        public static float s_Convert = 1.0f;

        public CModbusTcp() { }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 连接到PLC
        /// </summary>
        public void ConnectToPLC(string ip, int port)
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
        /// 2024.7.12 李焕彬
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
                        bool[] xState = master.ReadInputs(0x01, 0xF800, 0x10);
                        //读取Y0-Y17状态   //0xFC00‑0xFFFF
                        bool[] yState = master.ReadCoils(0x01, 0xFC00, 0x10);
                        //读取M0-M100状态   //0x0000‑0x1F3F
                        bool[] mState = master.ReadCoils(0x01, 0x0000, 0x65);
                        //读取D100 - D119的数值 //0x0000‑0x1F3F
                        ushort[] dValue = master.ReadHoldingRegisters(0x01, 0x0064, 0x1E);
                        List<bool> xyState = new List<bool>();
                        //数组合并
                        xyState.AddRange(xState);
                        xyState.AddRange(yState);
                        //触发事件，传递XY的状态
                        SendXYData?.Invoke(xState, yState);
                        //触发事件，传递M的状态
                        SendMxData?.Invoke(mState);
                        //触发事件，传递D的数据
                        SendDxData?.Invoke(dValue);
                        //plc连接事件
                        actionConnect.Invoke(true);
                        Thread.Sleep(10);
                    }
                    else
                    {
                        //plc断开事件
                        actionConnect.Invoke(false);
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 关闭线程
        /// </summary>
        public void Close()
        {
            isStart = false;
        }

        /// <summary>
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
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
        /// 2024.7.12 李焕彬
        /// 写寄存器值
        /// </summary>
        /// <param name="register">寄存器地址</param>
        /// <param name="value">写入值</param>
        public async void WriteRegisterD(string register, float value)
        {
            try
            {
                var regex = Regex.Match(register, "D[0-9]+");
                if (!regex.Success)
                    throw new($"输入地址{register}有误！");
                string addr = regex.Value.Substring(1);
                ushort coilAddress = Convert.ToUInt16(addr);

                byte[] fData = BitConverter.GetBytes(value * s_Convert);
                ushort[] Data = new ushort[2];
                Data[0] = (ushort)((fData[1] << 8) + fData[0]);
                Data[1] = (ushort)((fData[3] << 8) + fData[2]);

                await WriteMultipleRegistersAsync(0x01, coilAddress, Data);
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                CMotionCtrlVM.SysLog.Error(ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读取寄存器值
        /// </summary>
        /// <param name="dValue">寄存器数据集合</param>
        /// <param name="register">寄存器地址</param>
        /// <returns></returns>
        public float ReadRegisterD(ushort[] dValue, string register)
        {
            try
            {
                //D100-D119共20个寄存器都是Float型，占2个寄存器,需要转换
                var regex = Regex.Match(register, "D[0-9]+");
                if (!regex.Success)
                    throw new($"{register}:" + Properties.Resources.AddrError);
                string addr = regex.Value.Substring(1);
                int coilAddress = Convert.ToUInt16(addr) - 100;
                byte[] data = BitConverter.GetBytes(
                    dValue[coilAddress] + (dValue[coilAddress + 1] << 16)
                );
                return BitConverter.ToSingle(data, 0) / s_Convert;
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                CMotionCtrlVM.SysLog.Error(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写线圈
        /// </summary>
        /// <param name="coil">线圈地址</param>
        /// <param name="bTrue"></param>
        public async void WriteCoilM(string coil, bool bTrue)
        {
            try
            {
                var regex = Regex.Match(coil, "M[0-9]+");
                if (!regex.Success)
                    throw new($"{coil}:" + Properties.Resources.AddrError);
                string addr = regex.Value.Substring(1);
                ushort coilAddress = Convert.ToUInt16(addr);

                await WriteSingleCoilAsync(0x01, (ushort)(coilAddress + 0x0000), bTrue);
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                CMotionCtrlVM.SysLog.Error(ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读线圈
        /// </summary>
        /// <param name="mData">线圈数据集合</param>
        /// <param name="coil">线圈地址</param>
        /// <returns></returns>
        public bool ReadCoilM(bool[] mData, string coil)
        {
            try
            {
                var regex = Regex.Match(coil, "M[0-9]+");
                if (!regex.Success)
                    throw new($"{coil}:" + Properties.Resources.AddrError);
                string addr = regex.Value.Substring(1);
                ushort coilAddress = Convert.ToUInt16(addr);

                return mData[coilAddress];
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                CMotionCtrlVM.SysLog.Error(ex.Message);
                return false;
            }
        }
    }
}
