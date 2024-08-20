using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlarmSetCtrl;
using Autofac;
using CameraModule;
using CommunicationModule;
using CommunityToolkit.Mvvm.Messaging;
using DataQuery;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using HistoryPlayback.Model;
using Microsoft.Win32;
using MySqlOperatesApi;
using SaveImageManage;
using WH.Controls;
using WH.DetectSystem;
using WH.DetectSystem.ViewModels;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.Entity.Progress;
using WH.RecipeCellRootBase;
using 断面毛刺检测软件.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
        : HandyControl.Controls.Window,
            IRecipient<BitmapSource>,
            IRecipient<AlarmPopMessage>,
            IRecipient<AddOneNgImagePathMessage>
    {
        IObservable<Unit> StartStopSource;
        CMainModelsModelVM CMainList;
        CMainVM mainVM;
        CProgress<string> progress;
        CLogRec SysLog;
        CLogRec OperateLog;

        #region 初始化 加载
        public MainWindow()
        {
            InitializeComponent();
            CMainList = App.Container.Resolve<CMainModelsModelVM>();
            mainVM = CMainList.CMainVMs[0];
            SysLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);
            OperateLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);
            SysLog.Info(Properties.Resources.OpenSoftware);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            progress = new CProgress<string>(
                value => LoadProgressBar.Text = value,
                () =>
                {
                    this.IsEnabled = true;
                    CMainList.IsLoading = false;
                    this.Activate();
                },
                "Loaded!"
            );
            try
            {
                //半秒之内防止多次点击
                StartStopSource = Observable
                    .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                        h => Btn_StartStop.Click += h,
                        h => Btn_StartStop.Click -= h
                    )
                    .Select(x => Unit.Default)
                    .StartWith(Unit.Default);

                StartStopSource
                    .Throttle(TimeSpan.FromMilliseconds(500))
                    .Subscribe(_ =>
                    {
                        if (mainVM.IsManualTest) //离线手动下，不允许启动
                        {
                            mainVM.StartStop = false;
                            Growl.Warning("请退出设置或离线手动模式！");
                            return;
                        }
                        if (mainVM.MotionCtrlVM.IsFocusing)
                        {
                            mainVM.StartStop = false;
                            Growl.Warning("正在对焦中，不能启动！");
                            return;
                        }
                        if (mainVM.IsStart == mainVM.StartStop)
                            return;
                        mainVM.IsStart = mainVM.StartStop;
                        if (mainVM.IsStart)
                        {
                            OperateLog.Info(Properties.Resources.Start);
                        }
                        else
                        {
                            OperateLog.Info(Properties.Resources.Stop);
                        }
                    });

                this.IsEnabled = false;

                await CMainList.LoadAsync(progress);

                if (CMainList.SystemSettings.IsEnglish)
                {
                    var languageCode = "en-US";
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(
                        languageCode
                    );
                    ConfigHelper.Instance.SetLang("en");
                    LanguageManager.CLanguageManager.ChangeLanguage(new CultureInfo(languageCode));
                }
                ((IProgress<string>)progress).Report("Loaded!");
                WelComePage welComePage = new WelComePage(
                    CMainList.SystemSettings.RecentProjs.ToList(),
                    "断面毛刺检测软件"
                );
                welComePage.useraction = async (c) => await userActionFun(c);
                welComePage.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //throw;
            }
            finally
            {
                ((IProgress<string>)progress).Report("Loaded!");
            }
        }

        /// <summary>
        /// 欢迎页事件处理
        /// </summary>
        /// <param name="act"></param>
        private async Task userActionFun(string act)
        {
            switch (act)
            {
                case "mainform": //打开主界面
                    ((IProgress<string>)progress).Report("Loaded!");
                    //this.Visible = true;
                    //新建项目ToolStripMenuItem_Click(null, null);
                    break;
                case "openfile": //打开项目
                    //this.Visible = true;
                    OpenProj_Click(null, null);
                    break;
                default: //默认 打开最近项目

                    await OpenProjAsync(act);
                    break;
            }
            //this.WindowState = WindowState.Normal;
        }
        #endregion

        #region 用户登录
        private void btn_UserLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPage UserInfoFrm = new LoginPage(CMainList.LoginViewModel);
            UserInfoFrm.ShowDialog();
            mainVM.OperateLog.UserName = CMainList.LoginViewModel.LoginPerson.UserName;
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
                var result = MessageBox.Show(
                    "是否需要保存项目？\r\n Do you want to save it ?",
                    "提示|Tips",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.OK
                );
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;

                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    CMainList.SaveCurrentProj();
                }
                CMainList.SystemSettings.SaveParameter();
                CMainList.CMainVMs[0].MotionCtrlVM.SaveParameter();
                CMainList.CMainVMs[0].MarkCtrlVM.SaveParameter();
                CCommunicationManagement.SaveAllComConfig();
                CCameraManagement.SaveAllCamConfig();
                CCommunicationManagement.CloseAllComm();
                CCameraManagement.CloseAllCameras();
                OperateLog.Info(Properties.Resources.EnvironmentExit);
                Application.Current.Shutdown();
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

        private async void NewProj_Click(object sender, RoutedEventArgs e)
        {
            NewProjWindow newProj = App.Container.Resolve<Lazy<NewProjWindow>>().Value;

            OperateLog.Info(Properties.Resources.NewProj);
            if (newProj.ShowDialog() is true)
            {
                try
                {
                    await OpenProjAsync(CMainList.ProjPath);
                }
                catch (Exception exception)
                {
                    OperateLog.Error(Properties.Resources.NewFailed + "\r\n" + exception.Message);
                    Growl.Warning(Properties.Resources.NewFailed + "\r\n" + exception.Message);
                }
                finally
                {
                    progress.Report("Loaded!");
                }
            }
        }
        #endregion

        #region 打开
        private async void OpenProj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = CMainModelsModelVM.projFilter;
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
                progress.Report("Loaded!");
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
                if (string.IsNullOrEmpty(CMainList.ProjPath))
                    return;
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.Filter = CMainModelsModelVM.projFilter;
                //savefile.DefaultDirectory = "D:/";
                if (savefile.ShowDialog() is true)
                {
                    CMainList.ProjPath = savefile.FileName;
                    SaveProj();
                    await OpenProjAsync(CMainList.ProjPath);
                }
            }
            catch (Exception exception)
            {
                OperateLog.Error(Properties.Resources.SaveasFailed + "\r\n" + exception.Message);
                Growl.Warning(Properties.Resources.SaveasFailed + "\r\n" + exception.Message);
            }
            finally
            {
                progress.Report("Loaded!");
            }
        }
        #endregion

        #region 修改工程
        private void ModifyProj_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CMainList.ProjPath))
                return;
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
                    if (!string.IsNullOrEmpty(CMainList.ProjPath))
                    {
                        var result = MessageBox.Show(
                            "是否需要保存当前项目？\r\n Do you want to save it ?",
                            "提示|Tips",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.Yes
                        );
                        if (result == MessageBoxResult.Yes)
                        {
                            CMainList.SaveCurrentProj();
                            OperateLog.Info(
                                Properties.Resources.SaveProj + "\r\n" + CMainList.ProjPath
                            );
                            Growl.Success(
                                Properties.Resources.SaveProj + "\r\n" + CMainList.ProjPath
                            );
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
                    progress.Report("Loaded!");
                }
            }
        }
        #endregion

        private async Task OpenProjAsync(string header)
        {
            try
            {
                if (!File.Exists(header))
                    return;
                this.IsEnabled = false;
                progress.Reset();
                progress.Report("Initializing...");
                WeakReferenceMessenger.Default.UnregisterAll(this);
                await CMainList.OpenProj(progress, header);

                WeakReferenceMessenger.Default.Register<BitmapSource, Token>(
                    this,
                    CMainList.CMainVMs[0].TokeVM
                );
                WeakReferenceMessenger.Default.Register<AlarmPopMessage, Token>(
                    this,
                    CMainList.CMainVMs[0].MaociAlarmSetConfig.token
                );
                WeakReferenceMessenger.Default.Register<AddOneNgImagePathMessage, Token>(
                    this,
                    CMainList.CMainVMs[0].TokeVM
                );
                Growl.Success(Properties.Resources.OpenProj + "\r\n" + CMainList.ProjPath);
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
                if (string.IsNullOrEmpty(CMainList.ProjPath))
                    return;
                CMainList.SaveCurrentProj();
                Growl.Success(Properties.Resources.SaveProj + "\r\n" + CMainList.ProjPath);
                OperateLog.Info(Properties.Resources.SaveProj + "\r\n" + CMainList.ProjPath);
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
            //var languageCode = "zh-CN";
            //if (cbLang.IsChecked ?? true)
            //{
            //    languageCode = "en-US";
            //}
            //OperateLog.Info(Properties.Resources.LanguageChanged + languageCode);
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
            //LanguageManager.CLanguageManager.ChangeLanguage(new CultureInfo(languageCode));
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
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );
            renderTargetBitmap.Render(this);

            // 处理截图
            CroppedBitmap croppedBitmap = new CroppedBitmap(
                renderTargetBitmap,
                new Int32Rect(0, 0, (int)this.Width, (int)this.Height)
            );

            // 保存截图
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = ".bmp|*.bmp";
            if (savefile.ShowDialog() is true)
            {
                using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                OperateLog.Info(Properties.Resources.ScreenShot + savefile.FileName);
                MessageBox.Show(
                    Properties.Resources.ScreenShot + $":{savefile.FileName}",
                    "提示|Tips：",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        #endregion

        #region 系统设置

        private void SystemSetting_Click(object sender, RoutedEventArgs e)
        {
            SystemSettingWindow SysSetWindow = App
                .Container.Resolve<Lazy<SystemSettingWindow>>()
                .Value;
            SysSetWindow.DataContext = CMainList.SystemSettings;
            SysSetWindow.Show();
            SysSetWindow.Activate();
            OperateLog.Info(Properties.Resources.SystemSettings);
        }
        #endregion

        #region 存图设置
        private void SaveImageSetting_Click(object sender, RoutedEventArgs e)
        {
            CSaveImageSetFrm saveImageWindow = App
                .Container.Resolve<Lazy<CSaveImageSetFrm>>()
                .Value;
            saveImageWindow.DataContext = CMainList.CMainVMs[0].SaveImageVM;
            saveImageWindow.Show();
            saveImageWindow.Activate();
            OperateLog.Info(Properties.Resources.ImageSave);
        }
        #endregion

        #region 离线测试 手动调试
        List<bool> switches = new List<bool>();

        private void OffLineTest_Click(object sender, RoutedEventArgs e)
        {
            OffLineTestWindow offLine = App.Container.Resolve<Lazy<OffLineTestWindow>>().Value;
            offLine.Closed += ManualWindowClosed;
            offLine.Show();
            offLine.Activate();
            mainVM.IsManualTest = true;
            switches.Add(true);
            OperateLog.Info(Properties.Resources.Offline);
        }

        //手动调试
        private void ManualDebug_Click(object sender, RoutedEventArgs e)
        {
            TimeTriggerTestWindow timeTriggerWindow = App
                .Container.Resolve<Lazy<TimeTriggerTestWindow>>()
                .Value;
            timeTriggerWindow.Closed += ManualWindowClosed;
            timeTriggerWindow.Show();
            timeTriggerWindow.Activate();
            mainVM.IsManualTest = true;
            switches.Add(true);
            OperateLog.Info(Properties.Resources.手动调试);
        }

        /// <summary>
        /// 计算手动调试时的开关量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualWindowClosed(object sender, EventArgs e)
        {
            switches.RemoveAt(0);
            if (switches.Count <= 0)
            {
                mainVM.IsManualTest = false;
            }
        }

        #endregion

        #region 数据库设置
        private void Mysql_Click(object sender, RoutedEventArgs e)
        {
            SQLSetWindow sqlSetwindow = App.Container.Resolve<Lazy<SQLSetWindow>>().Value;
            sqlSetwindow.DataContext = CMainList.CMainVMs[0].MySqlVM;
            sqlSetwindow.Show();
            sqlSetwindow.Activate();
            OperateLog.Info(Properties.Resources.DataStatistics);
        }
        #endregion

        #region 数据查看
        private void DataQuery_Click(object sender, RoutedEventArgs e)
        {
            DataQueryWindow sqlSetwindow = App.Container.Resolve<Lazy<DataQueryWindow>>().Value;
            sqlSetwindow.DataContext = new CDataQueryVM(CMainList.CMainVMs[0].MySqlVM);
            sqlSetwindow.Show();
            sqlSetwindow.Activate();
            OperateLog.Info(Properties.Resources.DataStatistics);
        }
        #endregion

        #region 数据清空
        private void DataClear_Click(object sender, RoutedEventArgs e)
        {
            Growl.AskGlobal(Properties.Resources.CleraAsk, b =>
            {
                if (b)
                {
                    foreach (var item in CMainList.CMainVMs)
                    {
                        item.MaociDefectsProduce.Clear();
                    }
                    OperateLog.Info(Properties.Resources.DataClear);
                }
                return true;
            });
            
        }
        #endregion

        #region 相机通讯光控
        //相机设置
        private void CamSet_Click(object sender, RoutedEventArgs e)
        {
            CameraSetWindow cameraSetWindow = App.Container.Resolve<Lazy<CameraSetWindow>>().Value;
            cameraSetWindow.Show();
            cameraSetWindow.Activate();
        }

        /// <summary>
        /// 通讯设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommSet_Click(object sender, RoutedEventArgs e)
        {
            OpenCommunicationList openCommunicationList = App
                .Container.Resolve<Lazy<OpenCommunicationList>>()
                .Value;
            openCommunicationList.Show();
            openCommunicationList.Activate();
        }

        /// <summary>
        /// 光源控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LightControl_Click(object sender, RoutedEventArgs e)
        {
            var lightProcess = App.Container.ResolveKeyed<Process>("LightControl");
            //lightProcess.Start();
            lightProcess?.Start();
        }
        #endregion

        #region 关于
        //关于
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
        #endregion

        #region 清空Growl消息
        private void ClearGrowlMessage_Click(object sender, RoutedEventArgs e)
        {
            Growl.Clear();
        }
        #endregion

        #region 消息通道处理
        //取图显示
        public void Receive(BitmapSource image)
        {
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    mainVM.ModelImage = image;
                })
            );
        }

        //报警弹窗
        public void Receive(AlarmPopMessage message)
        {
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (message.alarm.IsPopWin)
                    {
                        //Growl.Warning(message.alarm.RegularShow);
                        Growl.Warning(
                            new GrowlInfo()
                            {
                                Message = message.alarm.RegularShow,
                                StaysOpen = false,
                                WaitTime = 3,
                            }
                        );
                        SysLog.Error(message.alarm.RegularShow);
                    }
                })
            );
        }

        //NG回放
        public void Receive(AddOneNgImagePathMessage message)
        {
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (!string.IsNullOrEmpty(message.Path))
                    {
                        if (mainVM.HistoryVM.HistoryModel.NgImagePaths.Count > 1000)
                            mainVM.HistoryVM.HistoryModel.NgImagePaths.RemoveAt(1000);
                        mainVM.HistoryVM.HistoryModel.NgImagePaths.Insert(0,message.Path);
                    }
                })
            );
        }
        #endregion
    }
}
