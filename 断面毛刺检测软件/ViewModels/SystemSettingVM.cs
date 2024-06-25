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
        public static SystemSettingsModel SystemSetParam = new SystemSettingsModel();
        #region 保存参数

        public static void SaveParameter()
        {
            try
            {
                ConfigAPI.Save(SystemSetParam, ParameterPath);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region 读取参数

        public static void LoadParameter()
        {

            try
            {
                if (File.Exists(ParameterPath))
                {
                    SystemSetParam = ConfigAPI.Load<SystemSettingsModel>(ParameterPath);
                    if (SystemSetParam == null)
                    {
                        SystemSetParam = new SystemSettingsModel();
                    }
                }
                else
                {
                    SystemSetParam = new SystemSettingsModel();
                }
            }
            catch (Exception)
            {
                SystemSetParam = new SystemSettingsModel();
            }

        }

        #endregion

        [RelayCommand]
        private void Close(System.ComponentModel.CancelEventArgs e)
        {
            if (HasErrors)
            {

                e.Cancel = true;
            }
        }
    }
}
