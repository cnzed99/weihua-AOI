using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FocusControl;
using Newtonsoft.Json;

namespace LiquidLens
{
    public class PlugIn : IFocus
    {
        /// <summary>
        /// 2024.9.28 李焕彬
        /// 创建新对焦
        /// </summary>
        public CFocusConfigBase CreateNewfocus()
        {
            CConfig motionConfig = new CConfig();
            return motionConfig;
        }

        /// <summary>
        /// 2024.9.28 李焕彬
        /// 初始化对焦
        /// </summary>
        /// <param name="focusConfig">对焦配置基类对象</param>
        /// <returns>对焦配置派生类对象</returns>
        public CFocusConfigBase Init(CFocusConfigBase focusConfig)
        {
            CConfig config = new CConfig();
            if (focusConfig != null)
            {
                config = JsonConvert.DeserializeObject<CConfig>(
                    JsonConvert.SerializeObject(focusConfig)
                );
            }
            return config;
        }
    }
}
