using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void Init(string path, int index, out LightControlBase light)
        {
            LSWLightControlVM LSWlight = new LSWLightControlVM();
            LSWlight.Config = ConfigAPI.Load<List<LSWLightConfig>>(path)[index];
            light = LSWlight;
        }
    }
}
