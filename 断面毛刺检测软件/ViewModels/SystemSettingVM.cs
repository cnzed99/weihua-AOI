using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WH.Controls;
using WH.Entity;
using CommunityToolkit.Mvvm.Input;
using 断面毛刺检测软件.Models;

namespace 断面毛刺检测软件.ViewModels
{
    public partial class SystemSettingsVM:SystemSettingsModel
    {

        [RelayCommand]
        private void Close(System.ComponentModel.CancelEventArgs e)
        {
            if (HasErrors)
            {

                e.Cancel = true;
            }
        }
    }

    public static class SysSet
    {
        /// <summary>
        /// 系统参数保存的路径
        /// </summary>
        public static string ParameterPath = "..\\SystemConfig\\SystemSetting.Json";
        #region 保存参数

        public static void SaveParameter(this SystemSettingsModel settingsModel)
        {
            try
            {
                ConfigAPI.Save(settingsModel, ParameterPath);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region 读取参数

        public static SystemSettingsVM LoadParameter()
        {
            SystemSettingsVM settingsModel = new SystemSettingsVM();
            try
            {
                
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.Load<SystemSettingsVM>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new SystemSettingsVM();
                    }
                }
                else
                {
                    settingsModel = new SystemSettingsVM();
                }
            }
            catch (Exception)
            {
                settingsModel = new SystemSettingsVM();
            }
            return settingsModel;
        }

        #endregion
    }
}
