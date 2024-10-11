using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WH.Entity.Attribute;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.1 李焕彬
    /// Known颜色类，含颜色和颜色名
    /// </summary>
    public class COMDevice : IEquatable<COMDevice>
    {
        public COMDevice() { }

        /// <summary>
        /// 20240729 TCG
        /// COM 口
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 20240729 TCG
        /// 重载等于
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Equals(COMDevice other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            var kcolor = (COMDevice)other;
            if (kcolor.Name == Name)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 20240729 TCG
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    /// <summary>
    /// 20240729 TCG
    /// COM 集合
    /// </summary>
    public class COMSPro
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// Known颜色集
        /// </summary>
        public List<COMDevice> COMDevices { get; set; } = new List<COMDevice>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化Known颜色集
        /// </summary>
        public COMSPro()
        {
            COMDevices.AddRange(
                SerialPort.GetPortNames().ToList().ConvertAll(com => new COMDevice() { Name = com })
            );
        }
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供四个默认COM枚举值
    /// </summary>
    public enum ECOM
    {
        [EnumString("COM1", "COM1")]
        ECOM_COM1 = 1,

        [EnumString("COM2", "COM2")]
        ECOM_COM2 = 2,

        [EnumString("COM3", "COM3")]
        ECOM_COM3 = 3,

        [EnumString("COM4", "COM4")]
        ECOM_COM4 = 4,
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供常用波特率枚举
    /// </summary>
    public enum BAUDRATE
    {
        [EnumString("2400", "2400")]
        BAUDRATE_2400 = 2400,

        [EnumString("4800", "4800")]
        BAUDRATE_4800 = 4800,

        [EnumString("9600", "9600")]
        BAUDRATE_9600 = 9600,

        [EnumString("19200", "19200")]
        BAUDRATE_19200 = 19200,

        [EnumString("38400", "38400")]
        BAUDRATE_38400 = 38400,

        [EnumString("57600", "57600")]
        BAUDRATE_57600 = 57600,

        [EnumString("115200", "115200")]
        BAUDRATE_115200 = 115200,
    }

    /// <summary>
    /// 20240724 TCG
    /// 提供数据位枚举
    /// </summary>
    public enum DATABITS
    {
        [EnumString("8", "8")]
        DATABITS_8 = 8,

        [EnumString("16", "16")]
        DATABITS_16 = 16,
    }
}
