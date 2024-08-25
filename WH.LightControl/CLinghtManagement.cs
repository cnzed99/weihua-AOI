using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;

namespace WH.LightControl
{
    /// <summary>
    /// 光源管理类
    /// 2024.8.23 鲍赞宝
    /// </summary>
    public class CLinghtManagement
    {
        /// <summary>
        /// 20240825 鲍赞宝
        /// 光源参数保存路径
        /// </summary>
        public static string s_LightConfigPath = "../SystemConfig/LightConfig.Json";

        /// <summary>
        /// 20240825 鲍赞宝
        /// 光源参数
        /// </summary>
        public static List<LightParamsBase> lightParams = new List<LightParamsBase>();

        /// <summary>
        /// 光源插件字典
        /// </summary>
        public static Dictionary<string, ILinght> LightHelpers = new Dictionary<string, ILinght>();

        public static Dictionary<string, LightControlBase> LightControlDic = new Dictionary<string, LightControlBase>();

        public static void LoadLightParams()
        {
            if (File.Exists(s_LightConfigPath))
            {
                lightParams = ConfigAPI.Load<List<LightParamsBase>>(s_LightConfigPath);
            }
            else
            {
                lightParams = new List<LightParamsBase>();               
            }
            LoadLightPlugs.LoadLight();
            for (int i = 0; i < LightHelpers.Count; i++)
            {
                
               LightHelpers[lightParams[i].LightBrandName].Init(s_LightConfigPath, i, out LightControlBase lightControl);
                if (lightControl != null)
                {
                    string lightkey = lightParams[i].LightBrandName + "-" + lightParams[i].LightStationName;
                    LightControlDic.Add(lightkey, lightControl);
                }
            }

        }

        public static void SaveConfigParams()
        {
            ConfigAPI.Save(lightParams, s_LightConfigPath);
        }

    }
}
