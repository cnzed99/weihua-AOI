using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace WH.LightControl
{
    /// <summary>
    /// 光源接口
    /// 2024.08.23 鲍赞宝
    /// </summary>
    public  interface ILinght
    {

        void Init(string path, int index, out LightControlBase cam);
    }


    public class LoadLightPlugs
    {
        /// <summary>
        /// 2024.8.23 李焕彬
        /// 加载相机插件
        /// </summary>
        public static void LoadLight()
        {
            foreach (var item in Directory.GetDirectories("LightPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll");
                    Type type = ass.GetTypes().ToList().Find(c => c.GetInterface("ILight") != null);
                    if (type != null&& !CLinghtManagement.LightHelpers.ContainsKey(Path.GetFileNameWithoutExtension(item)))
                    {
                        ILinght light = Activator.CreateInstance(type) as ILinght;
                        CLinghtManagement.LightHelpers.Add(Path.GetFileNameWithoutExtension(item), light);
                    }
                }
            }
        }
    }
}
