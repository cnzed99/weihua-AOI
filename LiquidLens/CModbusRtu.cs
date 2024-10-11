using System.IO.Ports;
using NModbus;
using NModbus.Serial;

namespace LiquidLens
{
    /// <summary>
    /// 2024.7.12 李焕彬
    /// modbusRTU协议
    /// </summary>
    public class CModbusRtu
    {
        /// <summary>
        /// 静态CModbusTcp集合
        /// </summary>
        public static List<CModbusRtu> s_modbusTcps = new List<CModbusRtu>();

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
        /// 串口对象
        /// </summary>
        private SerialPort serialPort = null;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 比例转换,暂时用1
        /// </summary>
        public static float s_Convert = 1.0f;

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 是否打开
        /// </summary>
        public bool IsOpen => serialPort != null && serialPort.IsOpen;

        public CModbusRtu() { }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 打开
        /// </summary>
        /// <param name="config">配置</param>
        /// <returns>打开成功为true，否则为false</returns>
        public bool Open(CConfig config)
        {
            try
            {
                //建立连接
                this.serialPort = new SerialPort();
                this.serialPort.PortName = config.PortSensor.Name;
                this.serialPort.BaudRate = (int)config.BaudRateSensor;
                this.serialPort.Parity = config.ParitySensor;
                this.serialPort.DataBits = (int)config.DataBitsSensor;
                this.serialPort.StopBits = config.StopBitsSensor;
                this.serialPort.Handshake = config.HandShakeSensor;
                this.serialPort.Open();
                factory = new ModbusFactory();
                master = factory.CreateRtuMaster(serialPort);
                master.Transport.ReadTimeout = 1000;
                master.Transport.WriteTimeout = 1000;
                master.Transport.Retries = 10;

                return serialPort.IsOpen;
            }
            catch (Exception err)
            {
                CSetCtrlVM.SysLog.Error(err.Message);
            }
            return false;
        }

        /// <summary>
        /// 2024.9.30 李焕彬
        /// 关闭
        /// </summary>
        public void Close()
        {
            serialPort?.Close();
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
        /// 读取线圈状态
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>状态</returns>
        public bool ReadCoil(byte slaveAddress, ushort startAddress)
        {
            if (serialPort.IsOpen)
            {
                bool[] bools = master.ReadCoils(slaveAddress, startAddress, 1);
                if (bools.Length == 1)
                {
                    return bools[0];
                }
            }

            return false;
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读取单个保持寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public float ReadHoldingRegister(byte slaveAddress, ushort startAddress)
        {
            if (serialPort.IsOpen)
            {
                var value = master.ReadHoldingRegisters(slaveAddress, startAddress, 2);
                if (value.Length == 2)
                {
                    byte[] data = BitConverter.GetBytes(value[0] + (value[1] << 16));
                    return BitConverter.ToInt16(data, 0);
                }
            }
            return 0;
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 读取单个输入寄存器数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <returns>数据</returns>
        public float ReadInputRegister(byte slaveAddress, ushort startAddress)
        {
            if (serialPort.IsOpen)
            {
                var value = master.ReadInputRegisters(slaveAddress, startAddress, 2);
                if (value.Length == 2)
                {
                    byte[] data = BitConverter.GetBytes(value[0] + (value[1] << 16));
                    return BitConverter.ToInt16(data, 0);
                }
            }
            return 0;
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入单个线圈的数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleCoil(byte slaveAddress, ushort startAddress, bool value)
        {
            if (serialPort.IsOpen)
            {
                master.WriteSingleCoil(slaveAddress, startAddress, value);
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入单个寄存器
        /// </summary>
        /// <param name="registerAddress">开始地址</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public void WriteSingleRegister(byte slaveAddress, ushort registerAddress, float value)
        {
            if (serialPort.IsOpen)
            {
                byte[] fData = BitConverter.GetBytes(value);
                ushort[] Data = new ushort[2];
                Data[0] = (ushort)((fData[1] << 8) + fData[0]);
                Data[1] = (ushort)((fData[3] << 8) + fData[2]);

                master.WriteMultipleRegisters(slaveAddress, registerAddress, Data);
            }
        }
    }
}
