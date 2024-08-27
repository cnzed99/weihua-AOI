using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;

namespace WH.LightControl
{
    /// <summary>
    /// 光源接口
    /// 2024.08.23 鲍赞宝
    /// </summary>
    public  interface ILight
    {
        /// <summary>
        /// 初始化光源
        /// 2024.08.23 鲍赞宝
        /// </summary>
        /// <param name="path">参数文件路径</param>
        /// <param name="index">索引</param>
        /// <param name="LightObj">光源实例</param>
        /// <returns>参数</returns>
        CLightParamsBase Init(string path, int index, out CLightControlBase LightObj);
        /// <summary>
        /// 创建一个新的光源实例
        /// 2024.08.23 鲍赞宝
        /// </summary>
        /// <param name="LightObj">光源实例</param>
        /// <returns></returns>
       // CLightParamsBase CreatNewLight(out CLightControlBase LightObj);
    }


    public class LoadLightPlugs
    {
        /// <summary>
        /// 2024.8.23 李焕彬
        /// 加载相机插件
        /// </summary>
        public static ObservableCollection<string> LoadLight()
        {
            ObservableCollection<string> lightFileName = new ObservableCollection<string>();
            foreach (var item in Directory.GetDirectories("LightPlug"))
            {
                string folderPath = item;
                string fileType = "*.dll";
                string[] files = Directory.GetFiles(folderPath, fileType);

                if (files.Length>0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        Assembly ass = Assembly.LoadFrom(files[i]);
                        Type type = ass.GetTypes().ToList().Find(c => c.GetInterface("ILight") != null);
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
