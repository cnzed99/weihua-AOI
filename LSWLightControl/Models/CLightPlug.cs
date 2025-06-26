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
    /// <summary>
    /// 20240825 鲍赞宝
    /// 立实为光源接口实现
    /// </summary>
    public class CLightPlug : ILight
    {

        /// <summary>
        /// 初始化光源
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="index">光源索引</param>
        /// <param name="light">光源对象</param>
        public CLightParamsBase Init(string path, string indexstr, out CLightControlBase lightobj)
        {
            LSWLightControlVM LSWlight = new LSWLightControlVM();
            if (File.Exists(path))
            {
                List<LSWLightConfig> lightparambase = ConfigAPI.LoadDeserialize<List<LSWLightConfig>>(path);
                if (lightparambase != null)
                {
                     LSWLightConfig lightparam = lightparambase.Find(t => t.LightStationName == indexstr) as LSWLightConfig;
                    if (lightparam != null)
                    {
                        LSWlight.Config = lightparam;
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
    }
}
