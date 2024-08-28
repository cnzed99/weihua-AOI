using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;
using WH.Entity.LogRecord;

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
        /// 2024.7.23 李焕彬
        /// 模块日志
        /// </summary>
        public static CLogRec CamLogger { get; set; } = CLogRec.Create("light", "D:/Data");

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 操作日志
        /// </summary>
        public static CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 20240825 鲍赞宝
        /// 光源名称集合
        /// </summary>
        public static ObservableCollection<string> lightName = new ObservableCollection<string>();

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
            try
            {
                lightName = LoadLightPlugs.LoadLight();
                for (int i = 0; i < LightHelpers.Count; i++)
                {
                    var param = LightHelpers[i].Item2.Init(s_LightConfigPath, LightHelpers[i].Item1, out CLightControlBase lightControl);
                    if (lightControl != null && param != null)
                    {
                        lightControl.BaseConfig.LightStationName = LightHelpers[i].Item1;
                        string lightkey = $"{lightControl.BaseConfig.LightBrandName}&{lightControl.BaseConfig.LightStationName}";
                        LightControlDict.Add(lightkey, lightControl);
                        //LightParamDict.Add(lightkey, param);
                    }
                }

                SaveLightParams();
            }
            catch (Exception ex)
            {
                CamLogger.Error("加载光源配置出错:" + ex.Message + "\r\n" + ex.StackTrace);
            }


        }
        /// <summary>
        /// 保存光源配置
        /// 2024.08.25 鲍赞宝
        /// </summary>
        public static void SaveLightParams()
        {
            try
            {
                List<CLightParamsBase> lightparams = new List<CLightParamsBase>();

                foreach (var item in LightControlDict)
                {
                    lightparams.Add(item.Value.BaseConfig);
                }
                if (lightparams.Count > 0)
                {
                    ConfigAPI.Save(lightparams, s_LightConfigPath);
                }
            }
            catch (Exception ex)
            {
                CamLogger.Error("保存光源配置出错:" + ex.Message + "\r\n" + ex.StackTrace);
            }
      
        }

    }
}
