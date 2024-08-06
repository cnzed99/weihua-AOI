using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.15 李焕彬
    /// 16进制转换
    /// </summary>
    public class CHexConvert
    {
        /// <summary>
        /// 16进制字符串转成int32
        /// </summary>
        /// <param name="hexstring">16位表示的字符串</param>
        /// <param name="reverse">是否反转</param>
        /// <returns>int32数字</returns>
        public static int HexToInt(string hexstring, bool reverse)
        {
            byte[] array = CHexConvert.HexStringToByte(hexstring);
            if (reverse)
            {
                Array.Reverse(array);
            }
            int num = (int)array[0];
            for (int i = 1; i < array.Length - 1; i++)
            {
                if (array[i] != 0)
                {
                    num += Convert.ToInt32(Math.Pow(16.0, (double)array[i]));
                }
            }
            return num;
        }

        /// <summary>
        /// 16进制字符串转成int32
        /// </summary>
        /// <param name="hexstring">16位表示的字符串</param>
        /// <returns>int32数字</returns>
        public static int HexToInt(string hexstring)
        {
            return CHexConvert.HexToInt(hexstring, false);
        }

        /// <summary>
        /// 16进制字符串转成byte数组
        /// </summary>
        /// <param name="hexstring">16位表示的字符串</param>
        /// <returns>byte数组</returns>
        public static byte[] HexStringToByte(string hexstring)
        {
            string[] array = hexstring.Trim().Split(new char[] { ' ' });
            byte[] array2;
            if (array.Length != 0)
            {
                array2 = new byte[array.Length];
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i] = Convert.ToByte(array[i], 16);
                }
            }
            else
            {
                array2 = null;
            }
            return array2;
        }

        /// <summary>
        /// byte转换成16进制字符串
        /// </summary>
        /// <param name="buffer">byte</param>
        /// <returns>16进制表示的字符串</returns>
        public static string ByteToHexString(byte buffer)
        {
            return Convert.ToString(buffer, 16).ToUpper().PadLeft(2, '0');
        }

        /// <summary>
        /// byte数组转成16进制字符串
        /// </summary>
        /// <param name="buffer">byte数组</param>
        /// <returns>16位表示的字符串</returns>
        public static string ByteToHexString(byte[] buffer)
        {
            string text = string.Empty;
            string result;
            try
            {
                foreach (byte value in buffer)
                {
                    text = text + " " + Convert.ToString(value, 16).PadLeft(2, '0');
                }
                result = text.Trim().ToUpper();
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }
    }
}
