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

namespace OPTLightControl
{
    /// <summary>
    /// 20260129 龚伟东
    /// 奥普特光源接口实现
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
            OPTLightControlVM OPTlight = new OPTLightControlVM();

            if (File.Exists(path))
            {
                List<OPTLightConfig> templist = ConfigAPI.Load<List<OPTLightConfig>>(path);
                if (templist != null)
                {
                    IEnumerable<OPTLightConfig> selectName = templist.Where(t => t.LightStationName == indexstr);
                    int num=selectName.Count();
                    if (num>0)
                    {
                        OPTlight.Config = selectName.FirstOrDefault();
                    }
                    else
                    {
                        OPTLightConfig optlight = new OPTLightConfig();
                        optlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                        OPTlight.Config = optlight;
                    }
                }
                else
                {
                    OPTLightConfig optlight = new OPTLightConfig();
                    optlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                    OPTlight.Config = optlight;
                }
            }
            else
            {
                OPTLightConfig optlight = new OPTLightConfig();
                optlight.LightBrandName = Assembly.GetExecutingAssembly().GetName().Name;
                OPTlight.Config = optlight;

            }

            OPTlight.SetBaseParam(OPTlight.Config);
            lightobj = OPTlight;
            return OPTlight.Config;


        }
    }
}
