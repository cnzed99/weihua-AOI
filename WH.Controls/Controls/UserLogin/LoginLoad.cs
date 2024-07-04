using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace WH.Controls
{
    /// <summary>
    /// 静态用户信息容器，提供加载保存方法
    /// </summary>
    public class LoginLoad
    {

       static string  savePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Users.WH";

        /// <summary>
        /// 用户名 密码 字典
        /// </summary>
        public static Dictionary<string, CLoginPerson> useNamesDictionary = new Dictionary<string, CLoginPerson>();


        public static bool LoadUsers()
        {
            useNamesDictionary.Clear();
            try
            {
                bool Exists = File.Exists(savePath);

                //文件不存在
                if (!Exists)
                {
                    GetNewUsers();

                }
                else
                {
                    try
                    {
                        useNamesDictionary = CEncryption.Load<Dictionary<string, CLoginPerson>>(savePath);
                        if (useNamesDictionary == null)
                        {
                            GetNewUsers();
                        }
                    }
                    catch (Exception)
                    {
                        GetNewUsers();
                    }

                }
                return true;

            }
            catch (Exception)
            {
                return false;
            }

        }


        public static void SaveUsers()
        {
            try
            {
                CEncryption.Save(useNamesDictionary, savePath);
            }
            catch (Exception)
            {
                throw;
            }
           
        }


        private static void GetNewUsers()
        {

            CLoginPerson P1 = new CLoginPerson();
            CLoginPerson P2 = new CLoginPerson();
            CLoginPerson P3 = new CLoginPerson();
            CLoginPerson P4 = new CLoginPerson();

            P1.UserName = Properties.Resources.Operator;
            P1.PassWord = "1";
            P1.PrivileageLevel = PRIVILEGE.OPERATOR;

            P2.UserName = Properties.Resources.Craftman;
            P2.PassWord = "123";
            P2.PrivileageLevel = PRIVILEGE.TECHNOLOGIST;

            P3.UserName = Properties.Resources.Engineer;
            P3.PassWord = "vision";
            P3.PrivileageLevel = PRIVILEGE.ENGINEER;

            P4.UserName = Properties.Resources.Administrator;
            P4.PassWord = "WH-VISION";
            P4.PrivileageLevel = PRIVILEGE.ADMINISTRATOR;

            useNamesDictionary.Add(P1.UserName, P1);
            useNamesDictionary.Add(P2.UserName, P2);
            useNamesDictionary.Add(P3.UserName, P3);
            useNamesDictionary.Add(P4.UserName, P4);

            CEncryption.Save(useNamesDictionary, savePath);
        }
      
    }
    /// <summary>
    /// 20240704 TCG
    /// 加密解密
    /// </summary>
    public static class CEncryption
    {
        //默认密钥向量
        private static byte[] Keys = { 0x20, 0x17, 0x12, 0x25, 0x19, 0x83, 0x04, 0x15 };

        //DES加密字符串
        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="EncryptString">待加密的字符串</param>
        /// <param name="EncryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串 </returns>
        private static string Encrypt(string EncryptString, string EncryptKey)//EncryptDES
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(EncryptKey.Substring(0, 8));//转换为字节
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(EncryptString);

                var dCSP = DES.Create();//实例化数据加密标准
                MemoryStream mStream = new MemoryStream();//实例化内存流

                //将数据流链接到加密转换的流
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        //DES解密字符串
        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="DecryptString">待解密的字符串</param>
        /// <param name="DecryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        private static string Decrypt(string DecryptString, string DecryptKey)//DecryptDES
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(DecryptKey);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(DecryptString);
                var DCSP = DES.Create();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2023.2.28 鲍赞宝
        /// 保存配方配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="directory"></param>
        public static void Save<T>(T obj, string fileName)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
                string strEncrypt = Encrypt(json, "12345999");
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(strEncrypt);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 2023.2.28 鲍赞宝
        /// 加载配方配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static T Load<T>(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (StreamReader reader = File.OpenText(fileName))
                    {
                        // JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        string json = File.ReadAllText(fileName);
                        string strDecrypt = Decrypt(json, "12345999");
                        T config = JsonConvert.DeserializeObject<T>(strDecrypt);
                        return config;
                    }
                }
                else
                {
                    return default;
                }
            }
            catch (Exception)
            {

                throw;
            }


        }


    }


}
