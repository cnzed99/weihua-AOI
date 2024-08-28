using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    /// <summary>
    /// 光源接口
    /// 2024.08.23 鲍赞宝
    /// </summary>
    public interface ILight
    {
        /// <summary>
        /// 初始化光源
        /// 2024.08.23 鲍赞宝
        /// </summary>
        /// <param name="path">参数文件路径</param>
        /// <param name="index">索引</param>
        /// <param name="LightObj">光源实例</param>
        /// <returns>参数</returns>
        CLightParamsBase Init(string path, string index, out CLightControlBase LightObj);
    }

    public class LoadLightPlugs
    {
        /// <summary>
        /// 20240828 TCG
        /// 加载光源插件
        /// </summary>
        public static List<string> LoadLight()
        {
            List<string> lightFileName = new List<string>();
            foreach (var item in Directory.GetDirectories("LightPlug"))
            {
                string folderPath = item;
                string fileType = "*.dll";
                string[] files = Directory.GetFiles(folderPath, fileType);

                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        Assembly ass = DynamicAssembly.LoadAssembly(files[i]);
                        Type type = ass.GetTypes()
                            .ToList()
                            .Find(c => c.GetInterface("ILight") != null);
                        string fileName = Path.GetFileNameWithoutExtension(files[i]);
                        if (type != null)
                        {
                            ILight light = Activator.CreateInstance(type) as ILight;
                            CLinghtManagement.LightHelpers.Add((fileName, light));
                        }

                        lightFileName.Add(fileName);
                    }
                }
            }
            return lightFileName;
        }
    }
}
