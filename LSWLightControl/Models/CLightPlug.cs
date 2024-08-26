using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using WH.Entity;
using WH.LightControl;

namespace LSWLightControl
{
    public class CLightPlug:ILight
    {
      
        /// <summary>
        /// 初始化光源
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="index">光源索引</param>
        /// <param name="light">光源对象</param>
        public CLightParamsBase Init(string path, int index, out CLightControlBase lightobj)
        {
            LSWLightControlVM LSWlight = new LSWLightControlVM();

            if (File.Exists(path))
            {
                List<LSWLightConfig> templist = ConfigAPI.LoadDeserialize<List<LSWLightConfig>>(path);

                
                if ((templist.Count>0&&(templist.Count-1)>= index))
                {
                    LSWlight.Config = templist[index];
                }
                else
                {
                    LSWLightConfig lswlight = new LSWLightConfig();
                    lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                    LSWlight.Config = lswlight;
                }
               
            }
            else
            {
                LSWLightConfig lswlight = new LSWLightConfig();
                lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                LSWlight.Config = lswlight;

            }

            LSWlight.SetBaseParam(LSWlight.Config);
            lightobj = LSWlight;
            return LSWlight.Config;

        }
        /// <summary>
        /// 创建一个新的光源实例
        /// 2024.08.23 鲍赞宝
        /// </summary>
        /// <param name="LightObj">光源实例</param>
        /// <returns></returns>
        //public CLightParamsBase CreatNewLight(out CLightControlBase lightObj)
        //{
        //    //LSWLightControlVM LSWlight = new LSWLightControlVM();
        //    //LSWLightConfig lswlight = new LSWLightConfig();
        //    //lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
        //    //LSWlight.Config = lswlight;
        //    //LSWlight.SetBaseParam(LSWlight.Config);
        //    //lightObj = LSWlight;
        //    //return LSWlight.Config;
        //}

    }
}
