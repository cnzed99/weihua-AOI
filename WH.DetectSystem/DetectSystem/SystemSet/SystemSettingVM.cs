using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WH.Controls;
using WH.Entity;
using CommunityToolkit.Mvvm.Input;
using WH.DetectSystem.Models;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 20240704 TCG
    /// 系统设置 视图模型 继承自系统设置模型
    /// </summary>
    public partial class CSystemSettingsVM:CSystemSettingsModel
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
    /// <summary>
    /// 20240704 TCG
    /// 系统设置静态路径，提供保存扩展方法和读取方法
    /// </summary>
    public static class CSysSet
    {
        /// <summary>
        /// 系统参数保存的路径
        /// </summary>
        public static string ParameterPath = "..\\SystemConfig\\SystemSetting.Json";
        #region 保存参数

        public static void SaveParameter(this CSystemSettingsModel settingsModel)
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

        public static CSystemSettingsVM LoadParameter()
        {
            CSystemSettingsVM settingsModel = new CSystemSettingsVM();
            try
            {
                
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.Load<CSystemSettingsVM>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new CSystemSettingsVM();
                    }
                }
                else
                {
                    settingsModel = new CSystemSettingsVM();
                }
            }
            catch (Exception)
            {
                settingsModel = new CSystemSettingsVM();
            }
            return settingsModel;
        }

        #endregion
    }
}
