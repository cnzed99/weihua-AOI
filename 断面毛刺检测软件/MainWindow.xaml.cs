using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;
using System.Reactive;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 断面毛刺检测软件.Models;
using 断面毛刺检测软件.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;
using System.Globalization;
using WH.Controls;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        IObservable<Unit>? StartStopSource;
        MainVM mainVM = new MainVM();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainVM;

            LoginPage.UserChangeEvent += () =>
            {
                btn_UserLoginImg.ImageSource = LoginPage.UserImg;

                //btn_UserLogin.Background = Brushes.Chartreuse;
                btn_UserLogin.ToolTip = LoginPage.UserName + ":" + LoginPage.LoginCode.ToString();

            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //半秒之内防止多次点击
            StartStopSource = Observable
                .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => Btn_StartStop.Click += h, h => Btn_StartStop.Click -= h)
                .Select(x => Unit.Default)
                .StartWith(Unit.Default);

            StartStopSource
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => 
                {
                    if (mainVM.isStart == mainVM.StartStop) return;
                    mainVM.isStart = mainVM.StartStop;
                }
                );
        }

        #region 用户登录
        private void btn_UserLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPage UserInfoFrm = new LoginPage();

            UserInfoFrm.ShowDialog();
            //PreDllConfig.PreConfigLog.Info(Properties.Resources.CurrentUser + LoginPage.UserName);
        }

        #endregion

        #region 主题色系

        private void cbSkin_Checked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ToggleButton toggle)
            {

                if (toggle.IsChecked ?? true)
                {
                    GlobalData.Config.Skin = SkinType.Dark;

                    ((App)Application.Current).UpdateSkin(SkinType.Dark);

                }
                else
                {
                    GlobalData.Config.Skin = SkinType.Default;
                    ((App)Application.Current).UpdateSkin(SkinType.Default);
                }
                GlobalData.Save();
                Application.Current.MainWindow.ApplyTemplate();
            }
        }
        #endregion

        #region 窗体关闭

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("是否需要保存项目？\r\n Do you want to save it ?", "提示|Tips", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    Growl.Success("取消操作！");
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    mainVM.ApplyChanges();
                    string json = JsonConvert.SerializeObject(mainVM.Model);
                    using (FileStream fs = new FileStream(mainVM.ProjPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(json);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                    }
                }
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //CLogRec.Error(exception.Message);
            }
            finally
            {
                //CLoading.Close();
            }
            
        }

        #endregion

        #region 最近打开
        private void Recent_Click(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is MenuItem { Header:string header })
            {
                try
                {
                    var result = MessageBox.Show("是否需要保存当前项目？\r\n Do you want to save it ?", "提示|Tips", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        mainVM.ApplyChanges();
                        string json = JsonConvert.SerializeObject(mainVM.Model);
                        using (FileStream fs = new FileStream(mainVM.ProjPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(json);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Flush();
                        }
                        Growl.Success("已保存："+mainVM.Name);
                    }
                    mainVM.Model = JsonConvert.DeserializeObject<MainModel>(File.ReadAllText(header))??new MainModel();
                    mainVM.SystemSettings.RecentProjs.Remove(header);
                    mainVM.SystemSettings.RecentProjs.Insert(0, header);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    //CLogRec.Error(exception.Message);
                }
                finally
                {
                    //CLoading.Close();
                }
                
            }
        }
        #endregion

        #region 语言切换
        private void Lang_Checked(object sender, RoutedEventArgs e)
        {
            var languageCode = "zh-CN";
            if (cbLang.IsChecked??true)
            {
                languageCode = "en-US";
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
            LanguageManager.LanguageManager.ChangeLanguage(new CultureInfo(languageCode));
        }
        #endregion

       
    }
}