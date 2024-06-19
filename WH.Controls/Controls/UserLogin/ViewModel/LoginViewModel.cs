using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace WH.Controls
{
    public partial class LoginViewModel : ObservableObject
    {
      
        /// <summary>
        /// 选择用户改变时传出委托
        /// </summary>
        public Action<LoginPerson,bool>? UserChangeAction { get; set; }
        /// <summary>
        /// 剩余登录时间传出委托
        /// </summary>
        public Action<int,int, bool>? TimeRemainingAction { get; set; }

        [ObservableProperty]
        private string errorMsg = "";
       


        private List<string> _itemList = new List<string>();
        /// <summary>
        /// 用户名列表
        /// </summary>
        public List<string> ItemList
        {
            get
            {
                _itemList.Clear();
                foreach (string key in LoginLoad.useNamesDictionary.Keys)
                {
                    _itemList.Add(key);
                }
                return _itemList;
            }
            set
            {
               
                SetProperty(ref _itemList, value);
            }
        }

        /// <summary>
        /// 已登录(true)
        /// </summary>
        bool LoggedSuccess = false;


        private int leftTimeMinute=30;
        /// <summary>
        /// 从界面获取的剩余时间设置
        /// </summary>
        public  int LoginLeftTimeMinute
        {
            get 
            {
                iLoginLeftTimeMinute = leftTimeMinute;
                iLoginLeftTimeSecond = 0;
                return leftTimeMinute; 
            }
            set
            {
                leftTimeMinute= value;
                iLoginLeftTimeMinute = value;
                iLoginLeftTimeSecond = 0;
                //DoNotify();
            }
        }


        /// <summary>
        /// 登录时间【分钟】
        /// </summary>
        private int iLoginLeftTimeMinute = 0;
        /// <summary>
        /// 登录剩余时间【秒钟】
        /// </summary>
        private  int iLoginLeftTimeSecond = 0;

        /// <summary>
        /// 登录计时
        /// </summary>
        private  DispatcherTimer tmrCheckAuthorizationLeftTime = new DispatcherTimer();

        /// <summary>
        /// 选择的用户
        /// </summary>
        public LoginPerson LoginPerson { get; set; } = new LoginPerson();
        public LoginViewModel()
        {
            bool ret = LoginLoad.LoadUsers();
            if (ret)
            {

                LoginPerson = new LoginPerson();

                tmrCheckAuthorizationLeftTime.Tick += TmrCheckAuthorizationLeftTime_Tick;
                tmrCheckAuthorizationLeftTime.Interval = TimeSpan.FromSeconds(1);
                tmrCheckAuthorizationLeftTime.Start();



            }
            else
            {
                ErrorMsg = "读取用户登录文件失败,请检查文件User.WH";
            }
            
        }

        private void TmrCheckAuthorizationLeftTime_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (LoggedSuccess)
                {
                    iLoginLeftTimeSecond += 1;
                    if (iLoginLeftTimeSecond == 60)
                    {
                        if (iLoginLeftTimeMinute > 0)
                        {
                            iLoginLeftTimeMinute -= 1;
                        }
                        iLoginLeftTimeSecond = 0;
                    }

                    if (0 >= iLoginLeftTimeMinute && 0 >= iLoginLeftTimeSecond)
                    {
                        LogoutButton();
                    }
                    TimeRemainingAction?.Invoke(iLoginLeftTimeMinute, iLoginLeftTimeSecond, LoggedSuccess);
                }


            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="obj"></param>
        [RelayCommand]
        public void LoginButton()
        {
            ErrorMsg = "";
            try
            {
                if (string.IsNullOrEmpty(LoginPerson.UserName))
                {
                    ErrorMsg = "请选择用户";
                    return;
                }

                if (string.IsNullOrEmpty(LoginPerson.PassWord))
                {
                    ErrorMsg = "请输入密码";
                    return;
                }

                if (!LoginLoad.useNamesDictionary.Keys.Contains(LoginPerson.UserName))
                {
                    ErrorMsg = "当前用户名不存在";
                    return;
                }

                LoginPerson person = LoginLoad.useNamesDictionary[LoginPerson.UserName];
                if (person.PassWord != LoginPerson.PassWord)
                {
                    ErrorMsg = "密码不正确";
                    return;
                }
                LoggedSuccess = true;
                LoginPerson.PrivileageLevel = person.PrivileageLevel;
               
                UserChangeAction?.Invoke(LoginPerson, LoggedSuccess);
                ErrorMsg = "登陆成功";
              
                iLoginLeftTimeMinute = LoginLeftTimeMinute;
                if (LoggedSuccess)
                {
                    WeakReferenceMessenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage() { Sender = new WeakReference(this)});
                    if (LoginPerson != null)
                    {
                        LoginPerson.PassWord = "";
                        ErrorMsg = "";
                    }
                }
                if (LoginPerson?.UserName=="管理员")
                {
                    LoginSetting setting = new LoginSetting();
                    setting.ShowDialog();

                    List<string> tempList = new List<string>();
                    foreach (string key in LoginLoad.useNamesDictionary.Keys)
                    {
                        tempList.Add(key);
                    }
                    ItemList = tempList;
                    LogoutButton();
                    LoginPerson.PassWord = "";
                }
                
             
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        [RelayCommand]
        private void LogoutButton()
        {
            //LoginPerson P = new LoginPerson();
            LoginPerson.UserName = "未登录";
            LoginPerson.PrivileageLevel = PRIVILEGE.无权限;
            LoggedSuccess = false;
            UserChangeAction?.Invoke(LoginPerson, LoggedSuccess);
            iLoginLeftTimeMinute = 0;
            iLoginLeftTimeSecond = 0;
      
            TimeRemainingAction?.Invoke(iLoginLeftTimeMinute, iLoginLeftTimeSecond, LoggedSuccess);
            ErrorMsg = "已注销";

        }

      

    }
}
