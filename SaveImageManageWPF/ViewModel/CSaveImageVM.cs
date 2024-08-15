using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using WH.Entity;
using WH.RunCell;
#if NET8_0_OR_GREATER
using Microsoft.Win32;
#else
using System.Windows.Forms;
#endif

namespace SaveImageManage
{
    public partial class CSaveImageVM : ObservableValidator
    {
        /// <summary>
        /// 存图参数
        /// </summary>
        public CSaveImageConfig Param { get; set; }

        public CSaveImageVM()
        {
            Param = SaveImageManagement.LoadParameter();
        }

        [RelayCommand]
        private void Close(System.ComponentModel.CancelEventArgs e)
        {
            if (!HasErrors)
            {
                SaveImageManagement.SaveParameter(Param);
                // e.Cancel = true;
                //    WeakReferenceMessenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage() { Sender = new WeakReference(this) });
            }
        }

        [RelayCommand]
        void SelectPath()
        {
            try
            {
#if NET8_0_OR_GREATER
                var dialog = new OpenFolderDialog();

#else
                var dialog = new OpenFileDialog();

#endif

                if (dialog.ShowDialog() is true)
                {
                    Param.SaveImagePath = dialog.FolderName;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 保存存图参数类
    /// </summary>
    public static class SaveImageManagement
    {
        /// <summary>
        /// 存图参数保存的路径
        /// </summary>
        public static string ParameterPath = "..\\SystemConfig\\SaveImageParam.Json";

        #region 保存参数

        public static void SaveParameter(CSaveImageConfig param)
        {
            try
            {
                ConfigAPI.Save(param, ParameterPath);
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数

        public static CSaveImageConfig LoadParameter()
        {
            CSaveImageConfig settingsModel = new CSaveImageConfig();
            try
            {
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.Load<CSaveImageConfig>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new CSaveImageConfig();
                    }
                }
                else
                {
                    settingsModel = new CSaveImageConfig();
                }
            }
            catch (Exception)
            {
                settingsModel = new CSaveImageConfig();
            }
            return settingsModel;
        }

        #endregion
    }
}
