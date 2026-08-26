using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
using Motion;
using MySqlOperatesApi;
using SaveImageManage;
using WH.Controls;
using WH.DetectSystem;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.Entity.Progress;
using WH.LightControl;
using WH.RecipeCellRootBase;
using ZipperInfo;
using GearInfo;
using 断面毛刺检测软件.Views;
using 断面毛刺检测软件.Views.GearProduct;
using MessageBox = HandyControl.Controls.MessageBox;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window, IRecipient<AlarmPopMessage>
    {
        private IObservable<Unit> StartStopSource;
        private CMainModelsModelVM CMainList;
        private CProgress<string> progress;
        private CLogRec SysLog;
        private CLogRec OperateLog;
        private System.Timers.Timer Hearttimer; //心跳发送
        private bool _zipperTestFinshHooked;

        #region 初始化 加载

        partial void InitOfflineDebug();

        public MainWindow()
        {
            InitializeComponent();
            InitOfflineDebug();
            AddAssemblyPath();
            CMainList = App.Container.Resolve<CMainModelsModelVM>();
            CMainModelsModelVM.Dispatcher = this.Dispatcher;
            SysLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);
            OperateLog = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);
            SysLog.Info(Properties.Resources.OpenSoftware);
        }

        public void AddAssemblyPath()
        {
            string[] pathes = new string[]
            {
                "AlgorithmPlug",
                "CamPlug",
                "ComPlug",
                "FocusPlug",
                "LightPlug",
                "MotionPlug"
            };
            List<string> allDirectories = new List<string>();
            foreach (string s in pathes)
            {
                string pluginFolder = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    s
                );

                // 递归获取所有子目录
                allDirectories.AddRange(
                    Directory.GetDirectories(pluginFolder, "*", SearchOption.AllDirectories)
                );
            }
            // 将子目录添加到 PATH 环境变量
            string path = Environment.GetEnvironmentVariable("PATH");
            foreach (var directory in allDirectories)
            {
                path += ";" + directory;
            }
            Environment.SetEnvironmentVariable("PATH", path);
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
                        if (CMainList.IsManualTest) //离线手动下，不允许启动
                        {
                            CMainList.StartStop = false;
                            foreach (var MainVM in CMainList.CMainVMs)
                            {
                                MainVM.IsStart = false;
                            }
                            Growl.Warning("请退出设置或离线手动模式！");
                            return;
                        }
                        if (CMainList.IsFocusing)
                        {
                            CMainList.StartStop = false;
                            Growl.Warning("正在对焦中，不能启动！");
                            return;
                        }
                        //【盘齿方案2-注释】原因：即将启动时校验所有制程组名都能在 GroupResults 中找到；缺映射不启动。无 PLC 只要 JSON 正常仍允许启动。
                        //【曲轴方案11-注释】点位 JSON 仍仅 IsGear；IsCrank 本期允许空转（停点 C 再接 Crank JSON）
                        if (CMainList.StartStop && COpenProjectLine.IsGear && !TryValidateProcessGroupResultMapping())
                        {
                            CMainList.StartStop = false;
                            return;
                        }
                        if (CMainList.IsStart == CMainList.StartStop)
                            return;
                        CMainList.IsStart = CMainList.StartStop;
                        if (CMainList.IsStart)
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
                //【盘齿方案11-注释】两套面板都创建，Visibility 由 ApplyOpenProjectLineUi 按工程切换
                zipperInfoShow.DataContext = new ZipperInfoVM();
                gearProductShow.DataContext = new GearProductVM();
                ApplyOpenProjectLineUi();
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
                //【盘齿方案0-注释】原因：欢迎页标题由拉链改为盘齿（原拉链标题保留在下方注释块）
                // 原： WelComePage welComePage = new WelComePage(
                // 原：     CMainList.SystemSettings.RecentProjs.ToList(),
                // 原：     "拉链智能视觉检测软件"
                // 原： );
                //【盘齿方案0】新标题（原拉链标题见上方注释块）
                //【曲轴方案0-注释】欢迎页用中性名，不写死盘齿/拉链/曲轴；HandyControl 左上角 Title 仍保持空
                WelComePage welComePage = new WelComePage(
                    CMainList.SystemSettings.RecentProjs.ToList(),
                    "视觉检测软件"
                );
                welComePage.useraction = async (c) => await userActionFun(c);
                //【盘齿方案11-注释】拉链心跳仅 IsZipper 且已启动时写 42638；盘齿走 CGearCommunicate 定时器
                Hearttimer = new System.Timers.Timer(1000);
                Hearttimer.Elapsed += (sender, e) =>
                {
                    if (COpenProjectLine.IsZipper && CMainList.IsStart)
                    {
                        CZipperCommunicate.SendHeartBeat();
                    }
                };
                Hearttimer.Start();
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
        /// 【盘齿方案11-注释】按当前打开工程切换右侧面板、换料按钮、拉链完成事件。不写 Window.Title（HandyControl 左上角标题栏保持空）。
        /// </summary>
        void ApplyOpenProjectLineUi()
        {
            //【曲轴方案11-注释】曲轴不显示拉链信息栏/盘齿换料；Visibility 仍只跟 IsZipper/IsGear
            if (zipperInfoBorder != null)
            {
                zipperInfoBorder.Visibility = COpenProjectLine.IsZipper ? Visibility.Visible : Visibility.Collapsed;
            }
            if (gearProductBorder != null)
            {
                gearProductBorder.Visibility = COpenProjectLine.IsGear ? Visibility.Visible : Visibility.Collapsed;
            }
            if (Btn_TestStart != null)
            {
                Btn_TestStart.Visibility = COpenProjectLine.IsZipper ? Visibility.Visible : Visibility.Collapsed;
            }

            if (COpenProjectLine.IsZipper)
            {
                if (!_zipperTestFinshHooked)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven += ClearProduceData;
                    _zipperTestFinshHooked = true;
                }
            }
            else if (_zipperTestFinshHooked)
            {
                CZipperAutomaticAlgorithm.Instance.TestFinshEven -= ClearProduceData;
                _zipperTestFinshHooked = false;
            }
        }
        /// <summary>
        /// 启动前校验工程所有制程组名都能在已加载的 GroupResults 中找到。点位未加载或缺映射返回 false。
        /// JSON 正常时无 PLC 仍允许启动。
        /// </summary>
        private bool TryValidateProcessGroupResultMapping()
        {
            //【曲轴方案11-注释】仅盘齿读 GearProtocolPoints.json；IsCrank 不走此校验
            if (!COpenProjectLine.IsGear)
            {
                return true;
            }
            try
            {
                bool pointsReady = CGearCommunicate.Points != null
                    && CGearCommunicate.Points.GroupResults != null
                    && CGearCommunicate.Points.GroupResults.Count > 0;
                if (!pointsReady)
                {
                    CGearCommunicate.TryLoadProtocolPoints();
                    pointsReady = CGearCommunicate.Points != null
                        && CGearCommunicate.Points.GroupResults != null
                        && CGearCommunicate.Points.GroupResults.Count > 0;
                }
                if (!pointsReady)
                {
                    Growl.Warning("组结果点位未加载，无法启动。请检查运行目录 SystemConfig\\GearProtocolPoints.json。");
                    return false;
                }

                if (CMainList == null || CMainList.CMainMModel == null || CMainList.CMainMModel.CProcessGroups == null)
                {
                    Growl.Warning("工程制程组未加载，无法启动。");
                    return false;
                }

                foreach (var group in CMainList.CMainMModel.CProcessGroups)
                {
                    string groupName = group == null ? null : group.Name;
                    if (string.IsNullOrEmpty(groupName) || !CGearCommunicate.Points.GroupResults.ContainsKey(groupName))
                    {
                        string showName = string.IsNullOrEmpty(groupName) ? "(空)" : groupName;
                        Growl.Warning("制程组「" + showName + "」在点位表 GroupResults 中没有映射，无法启动。请核对组名是否为「制程组1」/「制程组2」。");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Growl.Warning("组结果点位校验失败，无法启动。\r\n" + ex.Message);
                return false;
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

        #endregion 初始化 加载

        #region 用户登录

        private void btn_UserLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPage UserInfoFrm = new LoginPage(CMainList.LoginViewModel);
            UserInfoFrm.ShowDialog();
            //mainVM.OperateLog.UserName = CMainList.LoginViewModel.LoginPerson.UserName;
            OperateLog.Info(Properties.Resources.OpenedUserLogin);
        }

        #endregion 用户登录

        #region 窗体关闭

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //var result = MessageBox.Show(
                //    "是否需要保存项目？\r\n Do you want to save it ?",
                //    "提示|Tips",
                //    MessageBoxButton.YesNoCancel,
                //    MessageBoxImage.Question,
                //    MessageBoxResult.OK
                //);
                //if (result == MessageBoxResult.Cancel)
                //{
                //    e.Cancel = true;

                //    return;
                //}
                //else if (result == MessageBoxResult.Yes)
                //{
                //    CMainList.SaveCurrentProj();
                //}
                CMainList.SaveCurrentProj();
                CMainList.SystemSettings.SaveParameter();
                CCommunicationManagement.SaveAllComConfig();
                CCameraManagement.SaveAllCamConfig();
                CCommunicationManagement.CloseAllComm();
                CCameraManagement.CloseAllCameras();
                CLinghtManagement.CloseAllLinghtConnect();
                // CMotionManagement.SaveMotionConfig();
                OperateLog.Info(Properties.Resources.EnvironmentExit);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                // Console.WriteLine(exception);
                //CLogRec.Error(exception.Message);
            }
            finally
            {
                //CLoading.Close();
            }
        }

        #endregion 窗体关闭

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

        #endregion 新建工程

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

        #endregion 打开

        #region 保存

        private void SaveCurrentProj_Click(object sender, RoutedEventArgs e)
        {
            SaveProj();
        }

        #endregion 保存

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

        #endregion 另存为

        #region 修改工程

        private void ModifyProj_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CMainList.ProjPath))
                return;
            ModifyProjWindow modifyProj = App.Container.Resolve<Lazy<ModifyProjWindow>>().Value;
            OperateLog.Info(Properties.Resources.ModifyProj);
            modifyProj.ShowDialog();
        }

        #endregion 修改工程

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

        #endregion 最近打开

        #region 新增制程

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            NewProcessWindow newProcess = App.Container.Resolve<Lazy<NewProcessWindow>>().Value;

            OperateLog.Info(Properties.Resources.新增制程);
            if (newProcess.ShowDialog() is true)
            {
                try
                {
                    Growl.Success(Properties.Resources.新增制程成功);
                }
                catch (Exception exception)
                {
                    Growl.Success(Properties.Resources.新增制程失败);
                }
                finally { }
            }
        }

        #endregion 新增制程

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
                WeakReferenceMessenger.Default.Register<AlarmPopMessage>(this);
                await CMainList.OpenProj(progress, header);
                if (!string.Equals(CMainList.ProjPath, header, StringComparison.OrdinalIgnoreCase))
                {
                    progress.Report("Loaded!");
                    return;
                }
                ApplyOpenProjectLineUi();
                if (CMainList.CMainMModel?.CProcessGroups != null
                    && CMainList.CMainMModel.CProcessGroups.Count > 0
                    && CMainList.CMainMModel.CProcessGroups[0]?.CMainModels != null
                    && CMainList.CMainMModel.CProcessGroups[0].CMainModels.Count > 0
                    && CMainList.CMainMModel.CProcessGroups[0].CMainModels[0]?.SystemSettings != null)
                {
                    CMainList.CMainMModel.CProcessGroups[0].CMainModels[0].SystemSettings.ClearProduceEvent += ClearProduceData;
                    CMainList.CMainMModel.CProcessGroups[0].CMainModels[0].SystemSettings.Loaded = true;
                }
                CMainList.IsStart = true;
                CMainList.StartStop = CMainList.IsStart;
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

        #endregion 新建 打开 最近打开 另存 保存

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

        #endregion 语言切换

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

        #endregion 截屏保存

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

        #endregion 系统设置

        #region 存图设置

        private void SaveImageSetting_Click(object sender, RoutedEventArgs e)
        {
            CSaveImageSetFrm saveImageWindow = App
                .Container.Resolve<Lazy<CSaveImageSetFrm>>()
                .Value;
            CSaveImageVM SaveImageVM = CPublicServices.Container.Resolve<CSaveImageVM>();
            saveImageWindow.DataContext = SaveImageVM;
            saveImageWindow.Show();
            saveImageWindow.Activate();
            OperateLog.Info(Properties.Resources.ImageSave);
        }

        #endregion 存图设置

        #region 离线测试 手动调试

        private List<bool> switches = new List<bool>();

        private void OffLineTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OffLineTestWindow offLine = App.Container.Resolve<Lazy<OffLineTestWindow>>().Value;
                offLine.Closed += ManualWindowClosed;
                offLine.Show();
                offLine.Activate();
                CMainList.IsStart = false;
                CMainList.IsManualTest = true;
                CMainList.StartStop = false;

                switches.Add(true);
                OperateLog.Info(Properties.Resources.Offline);
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.Offline + "\r\n" + ex.Message);
            }

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
            CMainList.IsManualTest = true;
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
                CMainList.IsManualTest = false;
            }
        }

        #endregion 离线测试 手动调试

        #region 数据库设置

        private void Mysql_Click(object sender, RoutedEventArgs e)
        {
            SQLSetWindow sqlSetwindow = App.Container.Resolve<Lazy<SQLSetWindow>>().Value;
            CMySqlVM MySqlVM = CPublicServices.Container.Resolve<CMySqlVM>();
            sqlSetwindow.DataContext = MySqlVM;
            sqlSetwindow.Show();
            sqlSetwindow.Activate();
            OperateLog.Info(Properties.Resources.DataStatistics);
        }

        #endregion 数据库设置

        #region 数据查看

        private void DataQuery_Click(object sender, RoutedEventArgs e)
        {
            DataQueryWindow sqlSetwindow = App.Container.Resolve<Lazy<DataQueryWindow>>().Value;
            Dictionary<string, (CMysqlBLL sql, List<string> defects)> sqls =
                new Dictionary<string, (CMysqlBLL sql, List<string> defects)>();
            foreach (var item in CMainList.CMainMModel.CProcessGroups)
            {
                List<string> defects = new();
                foreach (var mainVM in item.CMainModels)
                {
                    foreach (var defect in mainVM.MaociFilterConfig.DefectList)
                    {
                        defects.Add(defect.Name);
                    }
                }
                sqls.Add(item.Name, (item.MysqlBLL, defects));
            }
            sqlSetwindow.DataContext = new CDataQueryVM(sqls);
            sqlSetwindow.Show();
            sqlSetwindow.Activate();
            OperateLog.Info(Properties.Resources.DataStatistics);
        }

        #endregion 数据查看

        #region 数据清空

        private void DataClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Growl.AskGlobal(
                    Properties.Resources.CleraAsk,
                    b =>
                    {
                        if (b)
                        {
                            ClearProduceData(b);
                            //foreach (var item in CMainList.CMainVMs)
                            //{
                            //    item.MaociDefectsProduce?.Clear();
                            //}
                            //foreach (var item in CMainList.CMainMModel.CProcessGroups)
                            //{
                            //    item.MaociDefectsProduce?.Clear();
                            //    item.Cells.Clear();
                            //}
                            OperateLog.Info(Properties.Resources.DataClear);
                        }
                        return true;
                    }
                );
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.DataClear + "\r\n" + ex.Message);
            }
        }

        private void ClearProduceData(bool finsh)
        {
            //if (CMainList.CMainMModel.CProcessGroups[0].CMainModels[0].SystemSettings.AutoClearEnable)
            //{
            foreach (var item in CMainList.CMainVMs)
            {
                item.MaociDefectsProduce?.Clear();
            }
            foreach (var item in CMainList.CMainMModel.CProcessGroups)
            {
                // item.MaociDefectsProduce?.Clear();
                item.MaociDefectsOneFlowProduce?.Clear();
                item.Cells.Clear();
            }
        }
        private void ClearProduceData()
        {

            //foreach (var item in CMainList.CMainVMs)
            //{
            //    item.MaociDefectsProduce?.Clear();
            //}
            foreach (var item in CMainList.CMainMModel.CProcessGroups)
            {
                item.MaociDefectsProduce?.Clear();
                item.Cells.Clear();
            }
        }

        #endregion 数据清空

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
        private void CommSet_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenCommunicationList openCommunicationList = App
                .Container.Resolve<Lazy<OpenCommunicationList>>()
                .Value;
            openCommunicationList.Show();
            openCommunicationList.Activate();
        }

        /// <summary>
        /// 通讯设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommInstance_Click(object sender, RoutedEventArgs e)
        {
            if (
                e.OriginalSource is MenuItem menuitem
                && menuitem.DataContext is CCommunicationSettingBase settingBase
            )
            {
                OpenCommunication openCommunication = new OpenCommunication(settingBase);
                openCommunication.Show();
            }
        }

        /// <summary>
        /// 光源控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LightItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is MenuItem { Header: string header })
                {
                    if (CLinghtManagement.LightControlDict.Count > 0)
                    {
                        var ienumkeyNames = CLinghtManagement
                            .LightControlDict.Keys.ToArray()
                            .ToList();

                        foreach (var keyname in ienumkeyNames)
                        {
                            string keysub = keyname.Split('&')[1];
                            if (keysub == header)
                            {
                                if (CLinghtManagement.LightControlDict.Keys.Contains(keyname))
                                {
                                    LightSetWindow lihtsetWin = App
                                        .Container.Resolve<Lazy<LightSetWindow>>()
                                        .Value;
                                    lihtsetWin.DataContext = CLinghtManagement.LightControlDict[
                                        keyname
                                    ];
                                    lihtsetWin.Show();
                                    lihtsetWin.Activate();
                                }
                            }
                        }
                    }
                    else
                    {
                        Growl.Warning(Properties.Resources.光源字典为空);
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
        }

        private void lightControl_Cilck(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddLightWindow addlight = App.Container.Resolve<Lazy<AddLightWindow>>().Value;
            addlight.ShowDialog();
            addlight.Activate();
        }

        #endregion 相机通讯光控

        #region 关于

        //关于
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        #endregion 关于

        #region 清空Growl消息

        private void ClearGrowlMessage_Click(object sender, RoutedEventArgs e)
        {
            Growl.Clear();
        }

        #endregion 清空Growl消息

        #region 消息通道处理

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
                    }
                    SysLog.Info(message.alarm.RegularShow);
                })
            );
        }

        #endregion 消息通道处理

        #region 修改制程
        /// <summary>
        /// 2024.9.5 李焕彬
        /// 修改制程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem.Tag != null)
            {
                CMainModel mainVM = (CMainModel)menuItem.Tag;
                NewProcessWindow newProcessWindow = new NewProcessWindow(mainVM);
                newProcessWindow.ShowDialog();
                OperateLog.Info(Properties.Resources.ProcessEdit);
            }
        }

        private void MotionPlug_Click(object sender, RoutedEventArgs e)
        {
            CMotionManagement.OpenMotionWindow();
        }
        #endregion

        #region 自动换料
        private void AutoMatic_Click(object sender, RoutedEventArgs e)
        {
            if (!COpenProjectLine.IsZipper)
            {
                return;
            }
            try
            {
                CZipperAutomaticAlgorithm.Instance.Dispatcher = this.Dispatcher;
                AutoFinshWindow autoFinshWindow;
                ProgressBarWindow progressBarWindow = new ProgressBarWindow();
                ZipperAutomaticWindow AutomaticWindow = App
                .Container.Resolve<Lazy<ZipperAutomaticWindow>>()
                .Value;
                CZipperAutomaticVM automaticVM = new CZipperAutomaticVM();
                AutomaticWindow.DataContext = automaticVM;
                automaticVM.StartAutoTestEven = (b) =>
                {
                    foreach (var mainVM in CMainList.CMainVMs)
                    {
                        CMainList.StartStop = true;
                        mainVM.IsStart = b;
                        mainVM.IsAutomaticTest = b;
                    }

                    ProgressBarViewModel.ProgressFinshEven = null;
                    ProgressBarViewModel.ProgressFinshEven = () =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            progressBarWindow?.Close();
                        });
                    };
                    if (b)
                    {
                        progressBarWindow.ShowDialog();
                        if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                        {
                            ZipperInfoVM zipperInfoVM = new ZipperInfoVM();
                            autoFinshWindow = new AutoFinshWindow();
                            autoFinshWindow.DataContext = zipperInfoVM;
                            autoFinshWindow.Closed += AutoFinshWindow_Closed;
                            autoFinshWindow.Show();
                            autoFinshWindow.Activate();
                            zipperInfoShow.DataContext = zipperInfoVM;
                        }
                    }
                };
                AutomaticWindow.Show();
                AutomaticWindow.Activate();
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void AutoFinshWindow_Closed(object sender, EventArgs e)
        {
            this.Dispatcher?.Invoke(() =>
            {
                foreach (var mainVM in CMainList.CMainVMs)
                {
                    mainVM.IsAutomaticTest = false;
                }
                CZipperAutomaticAlgorithm.Instance.onWichStage = 0;
            });
        }
        #endregion
    }

    /// <summary>
    /// 2024.9.2 李焕彬
    /// 图像窗口列数转换器
    /// </summary>
    public class ColumnCountConverter : IValueConverter
    {
        /// <summary>
        /// 2024.9.2 李焕彬
        /// </summary>
        /// <param name="value">制程数</param>
        /// <param name="targetType">未用</param>
        /// <param name="parameter">未用</param>
        /// <param name="culture">未用</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)value;
            double solve = (-1 + Math.Sqrt(1 + 4 * count)) / 2;
            if (Math.Abs(solve - Math.Round(solve)) < double.Epsilon)
            {
                return (int)solve;
            }
            else
            {
                return (int)Math.Round(solve + 0.5);
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
