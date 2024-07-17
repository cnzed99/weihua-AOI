//using System.Text;
//using WH.Entity.CommonLib;

//namespace CommunicationModule
//{
//    public static class ConvertBytes
//    {
//        /// <summary>
//        /// 转换信号为byte型
//        /// </summary>
//        /// <param name="str">信号值</param>
//        /// <returns></returns>
//        public static byte[] GetBytes(string str, SYSCONVERT sysconvert, ENCODING encoding)
//        {
//            try
//            {
//                byte[] bytes = null;
//                if (sysconvert == SYSCONVERT.十六进制)
//                {
//                    bytes = CHexConvert.HexStringToByte(str);
//                }
//                else
//                {
//                    switch (encoding)
//                    {
//                        case ENCODING.NONE:
//                            string[] arr = str.Split(' ');
//                            bytes = new byte[arr.Length];
//                            for (int i = 0; i < arr.Length; i++)
//                            {
//                                bytes[i] = Convert.ToByte(arr[i]);
//                            }

//                            break;
//                        case ENCODING.ASCII:
//                            bytes = Encoding.ASCII.GetBytes(str);
//                            break;
//                        case ENCODING.Default:
//                            bytes = Encoding.Default.GetBytes(str);
//                            break;
//                        case ENCODING.UTF8:
//                            bytes = Encoding.UTF8.GetBytes(str);
//                            break;
//                        case ENCODING.Unicode:
//                            bytes = Encoding.Unicode.GetBytes(str);
//                            break;
//                    }
//                }
//                return bytes;
//            }
//            catch (Exception ex)
//            {
//                CCommunicationManagement.ComLogger.Error(
//                    "ConvertBytes.GetBytes()转换出错" + ex.Message
//                );
//                throw;
//            }
//        }

//        public static string ShowBytes(byte[] bytes, SYSCONVERT sysconvert)
//        {
//            string str = string.Empty;

//            if (sysconvert == SYSCONVERT.十六进制)
//            {
//                str = CHexConvert.ByteToHexString(bytes);
//            }
//            else
//            {
//                for (int i = 0; i < bytes.Length; i++)
//                {
//                    if (i == 0)
//                        str = bytes[i].ToString();
//                    else
//                        str += " " + bytes[i].ToString();
//                }
//            }
//            return str;
//        }

//        /// <summary>
//        /// 通过字符编码把byte[]转成String
//        /// </summary>
//        /// <param name="bytes">byte数组</param>
//        /// <returns></returns>
//        public static string GetString(byte[] bytes, SYSCONVERT sysconvert, ENCODING encoding)
//        {
//            try
//            {
//                string str = string.Empty;

//                switch (encoding)
//                {
//                    case ENCODING.NONE:
//                        str = ShowBytes(bytes, sysconvert);
//                        break;
//                    case ENCODING.ASCII:
//                        str = Encoding.ASCII.GetString(bytes);
//                        break;
//                    case ENCODING.Default:
//                        str = Encoding.Default.GetString(bytes);
//                        break;
//                    case ENCODING.UTF8:
//                        str = Encoding.UTF8.GetString(bytes);
//                        break;
//                    case ENCODING.Unicode:
//                        str = Encoding.Unicode.GetString(bytes);
//                        break;
//                }

//                return str;
//            }
//            catch (Exception ex)
//            {
//                CCommunicationManagement.ComLogger.Error(
//                    "ConvertBytes.GetString()转换出错" + ex.Message
//                );
//                throw;
//            }
//        }
//    }
//}
