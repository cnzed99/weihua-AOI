using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace WH.Entity
{
    /// <summary>
    /// 20240704 TCG
    /// 配置文件 保存加载
    /// </summary>
    public static class ConfigAPI
    {
        private static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// 2023.1.30 汤传刚
        /// 保存配方配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="directory"></param>
        public static void Save<T>(T obj, string fileName)
        {
            Directory.GetParent(fileName)?.Create();
            string json = JsonConvert.SerializeObject(
                obj,
                Formatting.Indented,
                JsonSerializerSettings
            );
            //string bt64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }
        }

        /// <summary>
        /// 2023.1.30 汤传刚
        /// 加载配方配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"></param>
        /// <returns>文件不存在时返回new 对象</returns>
        public static T Load<T>(string fileName)
            where T : new()
        {
            if (
                File.Exists(fileName)
                || File.Exists(fileName = fileName.Replace(".whrecipe", ".Json"))
            )
            {
                using (StreamReader reader = File.OpenText(fileName))
                {
                    string bt64 = reader.ReadToEnd();
                    try
                    {
                        //byte[] bytes = Convert.FromBase64String(bt64);
                        //bt64 = Encoding.UTF8.GetString(bytes);
                        //JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
                        //{
                        //    ObjectCreationHandling = ObjectCreationHandling.Replace,
                        //};
                        T config = new T();
                        JsonConvert.PopulateObject(bt64, config, JsonSerializerSettings);
                        //T config = JsonConvert.DeserializeObject<T>(bt64, serializerSettings);
                        return config;
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
            }
            else
            {
                //ZzMessageBox.Show(fileName+"文件不存在！");
                return default(T);
            }
        }
    }
}
