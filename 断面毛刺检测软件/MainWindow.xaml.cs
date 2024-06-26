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
using Microsoft.Win32;
using 断面毛刺检测软件.Views;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        IObservable<Unit> StartStopSource;
        MainVM mainVM = new MainVM();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainVM;
            mainVM.SysLog.Info(Properties.Resources.OpenSoftware);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
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
                        if(mainVM.isStart) mainVM.OperateLog.Info(Properties.Resources.Start);
                        else mainVM.OperateLog.Info(Properties.Resources.Stop);
                    }
                    );
                #region 读取主配置文件
                SystemSettingsVM.LoadParameter();
                if (SystemSettingsVM.SystemSetParam != null)
                {
                    mainVM.SysLog.Info(Properties.Resources.SystemSettingsReadSuccess);
                    
                    //CLoading.DispText("读取系统配置成功...", 10);
                }
                else
                {
                    mainVM.SysLog.Error(Properties.Resources.SystemSettingsReadFailed);
                    //CLoading.DispText("读取系统配置失败...", 10);
                }

                if (SystemSettingsVM.SystemSetParam.IsEnglish)
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                }
                #endregion
            }
            catch (Exception ex)
            {

                //throw;
            }
        }

        #region 用户登录
        private void btn_UserLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPage UserInfoFrm = new LoginPage(mainVM.LoginViewModel);

            UserInfoFrm.ShowDialog();
            mainVM.OperateLog.Info(Properties.Resources.OpenedUserLogin);
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
                SystemSettingsVM.SaveParameter();
                mainVM.OperateLog.Info(Properties.Resources.EnvironmentExit);
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
                    mainVM.SystemSettings.RecentProjs.Add(header);
                }
                catch (Exception exception)
                {
                    mainVM.SysLog.Error(Properties.Resources.OpenFailed+"\r\n"+exception.Message);
                    Growl.Warning(Properties.Resources.OpenFailed+"\r\n"+exception.Message);
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
            mainVM.OperateLog.Info(Properties.Resources.LanguageChanged + languageCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
            LanguageManager.LanguageManager.ChangeLanguage(new CultureInfo(languageCode));
        }

        #endregion

        #region 截屏保存

        private void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            TakeScreenshotButton_Click(sender, e);
        }
       
        private void TakeScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取屏幕图像
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this);

            // 处理截图
            CroppedBitmap croppedBitmap = new CroppedBitmap(renderTargetBitmap, new Int32Rect(0, 0, (int)this.Width, (int)this.Height));

            // 保存截图
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = ".bmp|*.bmp";
            if(savefile.ShowDialog() is true)
            {
                using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                mainVM.OperateLog.Info(Properties.Resources.ScreenShot+savefile.FileName);
                MessageBox.Show(Properties.Resources.ScreenShot+$":{savefile.FileName}","提示|Tips：",MessageBoxButton.OK,MessageBoxImage.Information);

            }
            
        }
        #endregion

        #region 系统设置

        private void SystemSetting_Click(object sender, RoutedEventArgs e)
        {
            SystemSettingWindow SysSetWindow = new SystemSettingWindow();
            SysSetWindow.DataContext = mainVM.SystemSettings;
            SysSetWindow.Show();
            mainVM.OperateLog.Info(Properties.Resources.SystemSettings);
        }
        #endregion
    }
}