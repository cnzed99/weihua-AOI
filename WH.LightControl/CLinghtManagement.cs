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
        public static List<CLightParamsBase> lightParams = new List<CLightParamsBase>();

        /// <summary>
        /// 光源插件字典
        /// </summary>
        public static List<(string, ILight)> LightHelpers = new List<(string, ILight)>();

        public static Dictionary<string, CLightControlBase> LightControlDict = new Dictionary<string, CLightControlBase>();

       // public static Dictionary<string, CLightParamsBase> LightParamDict =new Dictionary<string, CLightParamsBase>();
       /// <summary>
       /// 加载光源配置并初始化
       /// 2024.08.25 鲍赞宝
       /// </summary>
        public static void LoadLightParams()
        {
            //if (File.Exists(s_LightConfigPath))
            //{
            //    lightParams = ConfigAPI.LoadDeserialize<List<CLightParamsBase>>(s_LightConfigPath);
            //}
            //else
            //{
            //    lightParams = new List<CLightParamsBase>();

            //    CLightParamsBase Lsw = new CLightParamsBase(); //如果没有配置文件,默认生成一个
            //    Lsw.LightBrandName = "LSWLightControl";
            //    lightParams.Add(Lsw);
            //}
            //if (lightParams != null)
            //{

            LoadLightPlugs.LoadLight();
            for (int i = 0; i < LightHelpers.Count; i++)
            {
                var param = LightHelpers[i].Item2.Init(s_LightConfigPath, i, out CLightControlBase lightControl);
                if (lightControl != null)
                {
                    lightControl.BaseConfig.LightStationName = LightHelpers[i].Item1;
                    string lightkey = $"{lightControl.BaseConfig.LightBrandName}-{lightControl.BaseConfig.LightStationName}";
                    LightControlDict.Add(lightkey, lightControl);
                    //LightParamDict.Add(lightkey, param);
                }
            }

            SaveConfigParams();

        }
        /// <summary>
        /// 保存光源配置
        /// 2024.08.25 鲍赞宝
        /// </summary>
        public static void SaveConfigParams()
        {
            List<CLightParamsBase> lightparams = new List<CLightParamsBase>();

            foreach (var item in LightControlDict)
            {
                lightparams.Add(item.Value.BaseConfig);
            }
            ConfigAPI.Save(lightparams, s_LightConfigPath);
     
        }

    }
}
