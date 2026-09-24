using System;
using System.Windows;
using HandyControl.Controls;
using WH.Controls;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Debug 专用：离线调试快捷登录/注销。Release 下按钮保持隐藏，点击无操作。
    /// </summary>
    public partial class MainWindow
    {
        private void Btn_OfflineDebug_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            try
            {
                var loginVm = CMainList.LoginViewModel;
                bool wasLogged = loginVm.LoggedSuccess;
                bool logged = ToggleOfflineDebugLogin(loginVm);
                Btn_OfflineDebug.IsChecked = logged;
                if (logged)
                {
                    OperateLog.Info("离线调试：已登录工程师");
                    Growl.Info("离线调试：已登录工程师");
                }
                else if (wasLogged)
                {
                    OperateLog.Info("离线调试：已注销");
                    Growl.Info("离线调试：已注销");
                }
                else
                {
                    string err = string.IsNullOrEmpty(loginVm.ErrorMsg)
                        ? "离线调试登录失败"
                        : loginVm.ErrorMsg;
                    Growl.Error(err);
                }
            }
            catch (Exception ex)
            {
                Btn_OfflineDebug.IsChecked = false;
                Growl.Error("离线调试失败" + Environment.NewLine + ex.Message);
            }
#endif
        }

#if DEBUG
        partial void InitOfflineDebug()
        {
            Btn_OfflineDebug.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 账号工程师 / 密码 vision；已是该账号则注销。
        /// </summary>
        private static bool ToggleOfflineDebugLogin(CLoginViewModel loginVm)
        {
            const string fallbackUser = "工程师";
            const string debugPassword = "vision";
            string engineer = WH.Controls.Properties.Resources.Engineer;
            if (string.IsNullOrEmpty(engineer))
            {
                engineer = fallbackUser;
            }
            if (loginVm.LoggedSuccess && loginVm.LoginPerson != null
                && loginVm.LoginPerson.UserName == engineer)
            {
                loginVm.LogoutButtonCommand.Execute(null);
                return loginVm.LoggedSuccess;
            }
            if (!LoginLoad.useNamesDictionary.ContainsKey(engineer)
                && LoginLoad.useNamesDictionary.ContainsKey(fallbackUser))
            {
                engineer = fallbackUser;
            }
            loginVm.LoginPerson.UserName = engineer;
            loginVm.LoginPerson.PassWord = debugPassword;
            loginVm.LoginLeftTimeMinute = 480;
            loginVm.LoginButton();
            return loginVm.LoggedSuccess;
        }
#endif
    }
}