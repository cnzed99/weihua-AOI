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
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 创建新通讯
        /// </summary>
        /// <param name="com">通讯对象</param>
        /// <returns>通讯参数</returns>
        CCommunicationSettingBase CreateNewCom(out CCommunicationBase com);
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 初始化通讯
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="index">索引</param>
        /// <param name="com">通讯对象</param>
        /// <returns>通讯参数</returns>
        CCommunicationSettingBase Init(string path, int index, out CCommunicationBase com);

    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯插件加载
    /// </summary>
    public class CLoadComPlugs
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 加载通讯插件
        /// </summary>
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
    }
}
