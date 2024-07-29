using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
}
