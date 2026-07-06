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

namespace XRDigitalLightControl
{
    /// <summary>
    /// 20260412 鲍赞宝
    /// 祥瑞光源接口实现
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
            XRDigitalLightControlVM XRlight = new XRDigitalLightControlVM();
            if (File.Exists(path))
            {
                List<XRDigitalLightConfig> lightparambase = ConfigAPI.LoadDeserialize<List<XRDigitalLightConfig>>(path);
                if (lightparambase != null)
                {
                     XRDigitalLightConfig lightparam = lightparambase.Find(t => t.LightStationName == indexstr) as XRDigitalLightConfig;
                    if (lightparam != null)
                    {
                        XRlight.Config = lightparam;
                    }
                    else
                    {
                        XRDigitalLightConfig lswlight = new XRDigitalLightConfig();
                        lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                        XRlight.Config = lswlight;
                    }
                }
                else
                {
                    XRDigitalLightConfig lswlight = new XRDigitalLightConfig();
                    lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                    XRlight.Config = lswlight;
                }
            }
            else
            {
                XRDigitalLightConfig lswlight = new XRDigitalLightConfig();
                lswlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                XRlight.Config = lswlight;

            }

            XRlight.SetBaseParam(XRlight.Config);
            lightobj = XRlight;
            return XRlight.Config;

        }
    }
}
