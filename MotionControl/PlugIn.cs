using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FocusControl;
using Newtonsoft.Json;

namespace MotionControl
{
    public class PlugIn : IFocus
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 创建新对焦
        /// </summary>
        public CFocusConfigBase CreateNewfocus()
        {
            CMotionConfig motionConfig = new CMotionConfig();
            return motionConfig;
        }

        ///// <summary>
        ///// 初始化对焦
        ///// 2024.09.04 李焕彬
        ///// </summary>
        ///// <param name="focusConfig">对焦配置基类对象</param>
        ///// <returns>对焦配置派生类对象</returns>
        //public CFocusConfigBase Init(CFocusConfigBase focusConfig)
        //{
        //    CMotionConfig motionConfig = new CMotionConfig();
        //    if (focusConfig != null)
        //    {
        //        motionConfig = JsonConvert.DeserializeObject<CMotionConfig>(
        //            JsonConvert.SerializeObject(focusConfig)
        //        );
        //    }
        //    return motionConfig;
        //}
    }
}
