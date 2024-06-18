using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Drawing;


namespace WH.Controls
{
    /// <summary>
    /// WelComePage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Window
    {
        LoginViewModel viewModel = new LoginViewModel();
        /// <summary>
        /// 用户名
        /// </summary>
        public static string UserName="未登录";
        /// <summary>
        /// 权限等级
        /// </summary>
        public static PRIVILEGE LoginCode = PRIVILEGE.无权限;

        /// <summary>
        /// logo图片
        /// </summary>
        public static BitmapImage UserImg = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));
        /// <summary>
        /// 是否登录成功
        /// </summary>
        public static bool LoggedSuccess = false;


        private static int loginLeftTimeMinute;
        /// <summary>
        /// 登录剩余有效时间【分钟】
        /// </summary>
        public static int LoginLeftTimeMinute
        {
            get
            {
                if (loginLeftTimeMinute <= 0)
                {
                    return 0;
                }
                return loginLeftTimeMinute - 1;
            }
        }


        public static int loginLeftTimeSecond;
        /// <summary>
        /// 登录剩余有效时间【秒钟】
        /// </summary>
        public static int LoginLeftTimeSecond
        {
            get
            {
                if (60 - loginLeftTimeSecond >= 60)
                {
                    return 0;
                }
                return 60 - loginLeftTimeSecond;
            }
        }
        /// <summary>
        /// 用户变动触发事件
        /// </summary>
        public static Action? UserChangeEvent;


        public LoginPage()
        {
            InitializeComponent();
            viewModel = new LoginViewModel();
            viewModel.UserChangeAction = new Action<LoginPerson,bool>(getPerson);
            this.DataContext = viewModel;

            viewModel.TimeRemainingAction = new Action<int, int,bool>((m, s,b) =>
            {
                loginLeftTimeMinute = m;
                loginLeftTimeSecond = s;
                LoggedSuccess = b;
            });

        }


        private void WinMove_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void getPerson(LoginPerson person, bool loggedSuccess)
        {
            UserName = person.UserName;
            LoginCode = person.PrivileageLevel;
            UserImg = person.LogoImage;
            LoggedSuccess=loggedSuccess;
            UserChangeEvent?.Invoke();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.LoginPerson.PassWord = "";
            }
            this.Hide();

        }
    }
}
