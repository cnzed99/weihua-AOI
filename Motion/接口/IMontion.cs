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

namespace Motion
{
    /// <summary>
    /// 控制接口
    /// 2025.3.6 李焕彬
    /// </summary>
    public interface IMotion
    {
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 创建新控制
        /// </summary>
        CMotionVMBase CreateNewMotion();
    }

    /// <summary>
    /// 2025.3.6 李焕彬
    /// 控制插件类
    /// </summary>
    public class CLoadMotionPlugs
    {
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 加载控制插件
        /// </summary>
        public static Dictionary<string, IMotion> LoadMotion()
        {
            Dictionary<string, IMotion> motionDict = new Dictionary<string, IMotion>();
            foreach (var item in Directory.GetDirectories("MotionPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom(
                        $"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"
                    );
                    Type type = ass.GetTypes()
                        .ToList()
                        .Find(c => c.GetInterface("IMotion") != null);
                    if (type != null)
                    {
                        IMotion motion = Activator.CreateInstance(type) as IMotion;
                        motionDict.Add(Path.GetFileNameWithoutExtension(item), motion);
                    }
                }
            }
            return motionDict;
        }
    }
}
