using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FocusControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FocusControl
{
    /// <summary>
    /// 对焦接口
    /// 2024.09.04 李焕彬
    /// </summary>
    public interface IFocus
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 创建新对焦
        /// </summary>
        CFocusConfigBase CreateNewfocus();

        /// <summary>
        /// 初始化对焦
        /// 2024.09.04 李焕彬
        /// </summary>
        /// <param name="focusConfig">对焦配置基类对象</param>
        /// <returns>对焦配置派生类对象</returns>
        CFocusConfigBase Init(CFocusConfigBase focusConfig);
    }

    /// <summary>
    /// 2024.09.04 李焕彬
    /// 对焦插件类
    /// </summary>
    public class CLoadFocusPlugs
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 加载对焦插件
        /// </summary>
        public static Dictionary<string, IFocus> LoadFocus()
        {
            Dictionary<string, IFocus> focusDict = new Dictionary<string, IFocus>();
            foreach (var item in Directory.GetDirectories("FocusPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom(
                        $"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"
                    );
                    Type type = ass.GetTypes().ToList().Find(c => c.GetInterface("IFocus") != null);
                    if (type != null)
                    {
                        IFocus focus = Activator.CreateInstance(type) as IFocus;
                        focusDict.Add(Path.GetFileNameWithoutExtension(item), focus);
                    }
                }
            }
            return focusDict;
        }
    }

    /// <summary>
    /// 2024.09.04 李焕彬
    /// 对焦参数Json转换器
    /// </summary>
    public class CFocusConfigConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CFocusConfigBase);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var paramBase = serializer.Deserialize<CFocusConfigBase>(reader);
            if (
                paramBase != null
                && !string.IsNullOrEmpty(paramBase.FocusType)
                && CFocusManagement.FocusHeper.ContainsKey(paramBase.FocusType)
            )
            {
                return CFocusManagement.FocusHeper[paramBase.FocusType].Init(paramBase);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
