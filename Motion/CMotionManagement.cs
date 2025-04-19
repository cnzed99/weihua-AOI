using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Entity;

namespace Motion
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 控制插件静态管理类
    /// </summary>
    public static class CMotionManagement
    {
        static CMotionManagement()
        {
            //初始化运动控制插件
            if (AppConfig.HasMotion())
            {
                MotionHeper = CLoadMotionPlugs.LoadMotion();
                if (MotionHeper.ContainsKey(AppConfig.MotionPulgName()))
                {
                    MotionCtrlVM = MotionHeper[AppConfig.MotionPulgName()].CreateNewMotion();
                }
            }
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制插件字典
        /// </summary>
        public static Dictionary<string, IMotion> MotionHeper { get; set; }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制VM
        /// </summary>
        public static CMotionVMBase MotionCtrlVM { get; set; }

        public static void OpenMotionWindow()
        {
            if (MotionCtrlVM != null)
            {
                MotionWindow motionWindow = new MotionWindow();
                motionWindow.DataContext = MotionCtrlVM;
                motionWindow.Show();
            }
            else
            {
                Growl.Error("控制插件为空！");
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void SaveMotionConfig()
        {
            MotionCtrlVM?.SaveConfig();
        }
    }
}
