using System.Text;
using WH.Entity.CommonLib;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯数据转换
    /// </summary>
    public static class CConvertBytes
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 转换string为byte型
        /// </summary>
        /// <param name="str">需要转换字符串</param>
        /// <param name="sysconvert">进制转换</param>
        /// <param name="encoding">编码类型</param>
        /// <returns></returns>
        public static byte[] GetBytes(string str, EMSYSCONVERT sysconvert, EMENCODING encoding)
        {
            try
            {
                byte[] bytes = null;
                if (sysconvert == EMSYSCONVERT.EMCONVERTHEXDECIMAL)
                {
                    bytes = CHexConvert.HexStringToByte(str);
                }
                else
                {
                    switch (encoding)
                    {
                        case EMENCODING.EMENCODENONE:
                            string[] arr = str.Split(' ');
                            bytes = new byte[arr.Length];
                            for (int i = 0; i < arr.Length; i++)
                            {
                                bytes[i] = Convert.ToByte(arr[i]);
                            }

                            break;
                        case EMENCODING.EMENCODEASCII:
                            bytes = Encoding.ASCII.GetBytes(str);
                            break;
                        case EMENCODING.EMENCODEDEFAULT:
                            bytes = Encoding.Default.GetBytes(str);
                            break;
                        case EMENCODING.EMENCODEUTF8:
                            bytes = Encoding.UTF8.GetBytes(str);
                            break;
                        case EMENCODING.EMENCODEUNICODE:
                            bytes = Encoding.Unicode.GetBytes(str);
                            break;
                    }
                }
                return bytes;
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    "ConvertBytes.GetBytes()转换出错" + ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通过进制类型把byte[]转成String
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="sysconvert">进制</param>
        /// <returns></returns>
        public static string ShowBytes(byte[] bytes, EMSYSCONVERT sysconvert)
        {
            string str = string.Empty;

            if (sysconvert == EMSYSCONVERT.EMCONVERTHEXDECIMAL)
            {
                str = CHexConvert.ByteToHexString(bytes);
            }
            else
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                        str = bytes[i].ToString();
                    else
                        str += " " + bytes[i].ToString();
                }
            }
            return str;
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通过字符编码把byte[]转成String
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="sysconvert">进制转换</param>
        /// <param name="encoding">编码类型</param>
        /// <returns></returns>
        public static string GetString(byte[] bytes, EMSYSCONVERT sysconvert, EMENCODING encoding)
        {
            try
            {
                string str = string.Empty;

                switch (encoding)
                {
                    case EMENCODING.EMENCODENONE:
                        str = ShowBytes(bytes, sysconvert);
                        break;
                    case EMENCODING.EMENCODEASCII:
                        str = Encoding.ASCII.GetString(bytes);
                        break;
                    case EMENCODING.EMENCODEDEFAULT:
                        str = Encoding.Default.GetString(bytes);
                        break;
                    case EMENCODING.EMENCODEUTF8:
                        str = Encoding.UTF8.GetString(bytes);
                        break;
                    case EMENCODING.EMENCODEUNICODE:
                        str = Encoding.Unicode.GetString(bytes);
                        break;
                }

                return str;
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    "ConvertBytes.GetString()转换出错" + ex.Message
                );
                throw;
            }
        }
    }
}
