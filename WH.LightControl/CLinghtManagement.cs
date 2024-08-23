using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.LightControl
{
    /// <summary>
    /// 光源管理类
    /// 2024.8.23 鲍赞宝
    /// </summary>
    public class CLinghtManagement
    {
        /// <summary>
        /// 20240723 TCG
        /// 主通讯参数保存路径
        /// </summary>
        public static string s_LightConfigPath = "../SystemConfig/LightConfig.Json";

        /// <summary>
        /// 光源插件字典
        /// </summary>
        public static Dictionary<string, ILinght> LightHelpers = new Dictionary<string, ILinght>();

        public CLinghtManagement()
        {
            LoadLightPlugs.LoadLight();

            for (int i = 0; i < LightHelpers.Count; i++)
            {

            }
        }
    }
}
