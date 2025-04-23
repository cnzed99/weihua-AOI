using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;

namespace Motion
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 控制插件静态管理类
    /// </summary>
    public class CMotionManagement
    {
        public CMotionManagement()
        {
            if (AppConfig.HasMotionConfig())
            {
                MotionHeper = CLoadMotionPlugs.LoadMotion();
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制插件字典
        /// </summary>
        public static Dictionary<string, IMotion> MotionHeper { get; set; }
    }
}
