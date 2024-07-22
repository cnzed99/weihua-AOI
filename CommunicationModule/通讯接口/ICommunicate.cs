using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯Dll接口
    /// </summary>
    public interface ICommunicate
    {
        CCommunicationSettingBase CreateNewCom(out CCommunicationBase com);
        CCommunicationSettingBase Init(string path, int index, out CCommunicationBase com);

    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯插件加载
    /// </summary>
    public class CLoadComPlugs
    {
        public static void LoadCom()
        {
            foreach (var item in Directory.GetDirectories("ComPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll");
                    Type type = ass.GetTypes().ToList().Find(c => c.GetInterface("ICommunicate") != null);
                    if (type != null && !CCommunicationManagement.ComHelper.ContainsKey(Path.GetFileNameWithoutExtension(item)))
                    {
                        ICommunicate com = Activator.CreateInstance(type) as ICommunicate;
                        CCommunicationManagement.ComHelper.Add(Path.GetFileNameWithoutExtension(item), com);
                    }
                }
            }
        }

        public static Type LoadType(string asemblyName, string typeName)
        {
            if (File.Exists($"ComPlug\\{asemblyName}\\{asemblyName}.dll"))
            {
                Assembly ass = Assembly.LoadFrom($"ComPlug\\{asemblyName}\\{asemblyName}.dll");
                Type type = ass.GetTypes().ToList().Find(c => c.FullName == typeName);
                return type;
            }
            return null;
        }
    }
}
