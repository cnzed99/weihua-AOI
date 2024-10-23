using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgorithmDll
{
    /// <summary>
    /// 算法接口
    /// 2024.09.04 李焕彬
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 创建新算法
        /// </summary>
        /// <returns>算法参数</returns>
        CAlgorithmParamBase CreateNewAlgorithm();

        ///// <summary>
        ///// 初始化算法
        ///// 2024.09.04 李焕彬
        ///// </summary>
        ///// <param name="path">参数文件路径</param>
        ///// <param name="index">索引</param>
        ///// <param name="LightObj">算法实例</param>
        ///// <returns>参数</returns>
        //CAlgorithmParamBase Init(CAlgorithmParamBase algorithmParamBase);
    }

    /// <summary>
    /// 2024.09.04 李焕彬
    /// 算法插件类
    /// </summary>
    public class CLoadAlgorithmPlugs
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 加载算法插件
        /// </summary>
        public static Dictionary<string, IAlgorithm> LoadAlgorithm()
        {
            Dictionary<string, IAlgorithm> algotithmDict = new Dictionary<string, IAlgorithm>();
            foreach (var item in Directory.GetDirectories("AlgorithmPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom(
                        $"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"
                    );
                    Type type = ass.GetTypes()
                        .ToList()
                        .Find(c => c.GetInterface("IAlgorithm") != null);
                    if (type != null)
                    {
                        IAlgorithm algorithm = Activator.CreateInstance(type) as IAlgorithm;
                        algotithmDict.Add(Path.GetFileNameWithoutExtension(item), algorithm);
                    }
                }
            }
            return algotithmDict;
        }
    }

    ///// <summary>
    ///// 2024.09.04 李焕彬
    ///// 算法参数Json转换器
    ///// </summary>
    //public class CAlgorithmParamConverter : JsonConverter
    //{
    //    public override bool CanWrite => false;

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(CAlgorithmParamBase);
    //    }

    //    public override object ReadJson(
    //        JsonReader reader,
    //        Type objectType,
    //        object existingValue,
    //        JsonSerializer serializer
    //    )
    //    {
    //        var paramBase = serializer.Deserialize<CAlgorithmParamBase>(reader);
    //        if (
    //            paramBase != null
    //            && !string.IsNullOrEmpty(paramBase.AlgorithmType)
    //            && CAlgorithmManagement.AlgorithmHeper.ContainsKey(paramBase.AlgorithmType)
    //        )
    //        {
    //            return CAlgorithmManagement.AlgorithmHeper[paramBase.AlgorithmType].Init(paramBase);
    //        }
    //        return null;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
