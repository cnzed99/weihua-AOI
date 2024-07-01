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
using WH.Entity.Progress;
using System;
using System.Reflection.PortableExecutable;
using Autofac;
using WH.Entity.LogRecord;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        IObservable<Unit> StartStopSource;
        MainVM mainVM;
        CProgress<double> progress;
        CLogRec SysLog;
        CLogRec OperateLog;
        #region 初始化 加载
        public MainWindow()
        {
            InitializeComponent();
            mainVM = App.Container.Resolve<MainVM>();
            SysLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);
            OperateLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);
            SysLog.Info(Properties.Resources.OpenSoftware);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            progress = new CProgress<double>(value => LoadProgressBar.Value = value,
                () =>
                {
                    this.IsEnabled = true;
                    mainVM.IsLoading = false;
                    this.Activate();
                }, 100);
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
                        if(mainVM.isStart) OperateLog.Info(Properties.Resources.Start);
                        else OperateLog.Info(Properties.Resources.Stop);
                    }
                    );
                this.IsEnabled = false;
                
                await mainVM.LoadAsync(progress);
                if (mainVM.SystemSettings.IsEnglish)
                {
                    var languageCode = "en-US";
                  
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
                    LanguageManager.LanguageManager.ChangeLanguage(new CultureInfo(languageCode));
                }


            }
            catch (Exception ex)
            {

                //throw;
            }
            finally
            {
                ((IProgress<double>)progress).Report(100);
            }
        }

        #endregion

        #region 用户登录
        private void btn_UserLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPage UserInfoFrm = new LoginPage(mainVM.LoginViewModel);

            UserInfoFrm.ShowDialog();
            OperateLog.Info(Properties.Resources.OpenedUserLogin);
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
                    mainVM.SaveCurrentProj();
                   
                }
                mainVM.SystemSettings.SaveParameter();
                OperateLog.Info(Properties.Resources.EnvironmentExit);
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

        #region 新建 打开 最近打开 另存 保存

        #region 新建工程

        private void NewProj_Click(object sender, RoutedEventArgs e)
        {
            NewProjWindow newProj = App.Container.Resolve<Lazy<NewProjWindow>>().Value;
           
            OperateLog.Info(Properties.Resources.NewProj);
            newProj.ShowDialog();
            
        }
        #endregion

        #region 打开
        private async void OpenProj_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = MainVM.projFilter;
                //openFileDialog.DefaultDirectory = "D:/";
                if (openFileDialog.ShowDialog() is true)
                {
                    await OpenProjAsync(openFileDialog.FileName);

                }
            }
            catch (Exception exception)
            {
                OperateLog.Error(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
                Growl.Warning(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
            }
            finally
            {
                progress.Report(100);
            }
        }
        #endregion



        #region 保存
        private void SaveCurrentProj_Click(object sender, RoutedEventArgs e)
        {
            SaveProj();
        }
        #endregion

        #region 另存为
        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(mainVM.ProjPath)) return;
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.Filter = MainVM.projFilter;
                //savefile.DefaultDirectory = "D:/";
                if (savefile.ShowDialog() is true)
                {
                    mainVM.ProjPath = savefile.FileName;
                    SaveProj();
                    await OpenProjAsync(mainVM.ProjPath);

                }
            }
            catch (Exception exception)
            {
                OperateLog.Error(Properties.Resources.SaveasFailed + "\r\n" + exception.Message);
                Growl.Warning(Properties.Resources.SaveasFailed + "\r\n" + exception.Message);
            }
            finally
            {
                progress.Report(100);
            }
        }
        #endregion

        #region 修改工程
        private void ModifyProj_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mainVM.ProjPath)) return;
            ModifyProjWindow modifyProj = App.Container.Resolve<Lazy<ModifyProjWindow>>().Value;
            OperateLog.Info(Properties.Resources.ModifyProj);
            modifyProj.ShowDialog();
        }
        #endregion

        #region 最近打开
        private async void Recent_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem { Header: string header })
            {
                try
                {
                    if (!string.IsNullOrEmpty(mainVM.ProjPath))
                    {
                        var result = MessageBox.Show("是否需要保存当前项目？\r\n Do you want to save it ?", "提示|Tips", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (result == MessageBoxResult.Yes)
                        {
                            mainVM.SaveCurrentProj();
                            OperateLog.Info(Properties.Resources.SaveProj + "\r\n" + mainVM.ProjPath);
                            Growl.Success(Properties.Resources.SaveProj + "\r\n" + mainVM.ProjPath);
                        }
                    }
                    await OpenProjAsync(header);

                }
                catch (Exception exception)
                {
                    OperateLog.Error(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
                    Growl.Warning(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
                }
                finally
                {
                    progress.Report(100);
                }

            }
        }
        #endregion

        private async Task OpenProjAsync(string header)
        {
            try
            {
                this.IsEnabled = false;
                progress.Reset();
                progress.Report(0);
                await mainVM.OpenProj(progress, header);
                Growl.Success(Properties.Resources.OpenProj + "\r\n" + mainVM.ProjPath);
                OperateLog.Info(Properties.Resources.OpenProj + "\r\n" + header);
            }
            catch (Exception exception)
            {
                OperateLog.Error(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
                Growl.Warning(Properties.Resources.OpenFailed + "\r\n" + exception.Message);
            }
        }
        private void SaveProj()
        {
            try
            {
                if (string.IsNullOrEmpty(mainVM.ProjPath)) return;
                mainVM.SaveCurrentProj();
                Growl.Success(Properties.Resources.SaveProj + "\r\n" + mainVM.ProjPath);
                OperateLog.Info(Properties.Resources.SaveProj + "\r\n" + mainVM.ProjPath);
            }
            catch (Exception exception)
            {
                OperateLog.Error(Properties.Resources.SaveFailed + "\r\n" + exception.Message);
                Growl.Warning(Properties.Resources.SaveFailed + "\r\n" + exception.Message);
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
            OperateLog.Info(Properties.Resources.LanguageChanged + languageCode);
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
                OperateLog.Info(Properties.Resources.ScreenShot+savefile.FileName);
                MessageBox.Show(Properties.Resources.ScreenShot+$":{savefile.FileName}","提示|Tips：",MessageBoxButton.OK,MessageBoxImage.Information);

            }
            
        }
        #endregion

        #region 系统设置

        private void SystemSetting_Click(object sender, RoutedEventArgs e)
        {
            SystemSettingWindow SysSetWindow = App.Container.Resolve<Lazy< SystemSettingWindow>>().Value;
            SysSetWindow.DataContext = mainVM.SystemSettings;
            SysSetWindow.Show();
            OperateLog.Info(Properties.Resources.SystemSettings);
        }




        #endregion
       
    }
}