using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using AlarmSetCtrl;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FocusControl;
using HandyControl.Controls;
using HistoryPlayback;
using HistoryPlayback.Model;
using Mapster;
using MarkControl;
using Motion;
using MySqlOperatesApi;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.DetectSystem.ZipperLine;
using WH.DetectSystem.DetectSystem.GearLine;
using WH.DetectSystem.DetectSystem.CrankLine;
using WH.DetectSystem.DetectSystem.XinGearLine;
using WH.DetectSystem.Models;
using WH.DetectSystem._5_存图操作;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.IIService;
using WH.Entity.LogRecord;
using WH.LightControl;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace WH.DetectSystem.ViewModels
{
    public partial class CMainModelsModelVM : ObservableObject
    {
        /// <summary>
        /// 2024.9.4 李焕彬
        /// UI线程调度器，MainWindow
        /// </summary>
        public static Dispatcher Dispatcher { get; set; }

        [ObservableProperty]
        CLoginViewModel loginViewModel = new CLoginViewModel();

        public CSystemSettingsVM SystemSettings { get; set; } =
            CPublicServices.Container.Resolve<CSystemSettingsVM>();
        public Version version { get; set; } = Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>
        /// 运行日志和报警日志
        /// </summary>
        public CLogRec SysLog { get; } =
            CPublicServices.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);

        /// <summary>
        /// 操作日志
        /// </summary>
        public CLogRec OperateLog { get; } =
            CPublicServices.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);

        [ObservableProperty]
        string projPath;
        public static string projFilter = "工程文件|*.burrproj|工程文件|*.Json";

        [ObservableProperty]
        bool isLoading = false;

        /// <summary>
        /// 多制程本地持久化配置
        /// </summary>
        [ObservableProperty]
        CMainModelsModel cMainMModel = new CMainModelsModel();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 多制程视图模型
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CMainModel> cMainVMs = new ObservableCollection<CMainModel>();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 选中制程
        /// </summary>
        [ObservableProperty]
        CMainModel selectedProcess;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯列表
        /// </summary>
        [ObservableProperty]
        List<CCommunicationSettingBase> listCommSetParam ;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯管理
        /// </summary>
        public CCommunicationManagement CommManagement { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 相机列表
        /// </summary>
        public List<CCameraParameterBase> ListCamSetParam { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 相机管理
        /// </summary>
        public CCameraManagement CamManagement { get; set; }

        /// <summary>
        /// 20240828 TCG
        /// 光源名
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> lightNames = new ObservableCollection<string>();

        /// <summary>
        /// 20240828 TCG
        /// 光源管理
        /// </summary>
        public CLinghtManagement LightManagement { get; set; }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 存图设置
        /// </summary>
        public CSaveImageVM SaveImageVM { get; set; } =
            CPublicServices.Container.Resolve<CSaveImageVM>();

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 数据库
        /// </summary>
        public CMySqlVM MySqlVM { get; set; } = CPublicServices.Container.Resolve<CMySqlVM>();

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 算法插件管理
        ///</summary>
        public CAlgorithmManagement AlgorithmManagement { get; set; }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 对焦插件管理
        /// </summary>
        public CFocusManagement FocusManagement { get; set; }
        /// <summary>
        /// 拉链信息
        /// </summary>
      //  public CZipperInfo ZipperInfo { get; set; }

        #region 启停 状态
        bool isStart = false;

        /// <summary>
        /// 启动时用于保存当前账户信息 停止运行时用于恢复权限
        /// </summary>
        CLoginPerson loginPerson = new CLoginPerson();

        /// <summary>
        /// 是否启动 后台使用此变量判断用户是否启动软件
        /// </summary>
        public bool IsStart
        {
            get => isStart;
            set
            {
                SetProperty(ref isStart, value);
                //if (value)
                //{
                //    CLoginViewModel.SloinPerson.Adapt(loginPerson);
                //    CLoginViewModel.SloinPerson.IsNoPermission = true;
                //}
                //else
                //{
                //    loginPerson.Adapt(CLoginViewModel.SloinPerson);
                //}
                foreach (var mainVM in CMainVMs)
                {
                    mainVM.IsStart = isStart;
                }
                CMotionManagement.MotionCtrlVM?.SetRunning(IsStart);
            }
        }

        private bool isManualTest = false;

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 离线检测或手动调试
        /// </summary>
        public bool IsManualTest
        {
            get { return isManualTest; }
            set
            {
                isManualTest = value;
                foreach (var mainVM in CMainVMs)
                {
                    mainVM.IsManualTest = isManualTest;
                }
            }
        }

        //private bool isAutomaticTest = false;

        ///// <summary>
        ///// 2025.6.23 鲍赞宝
        ///// 拉链自动识别模式
        ///// </summary>
        //public bool IsAutomaticTest
        //{
        //    get { return isAutomaticTest; }
        //    set
        //    {
        //        isAutomaticTest = value;
        //        foreach (var mainVM in CMainVMs)
        //        {
        //            mainVM.IsAutomaticTest = isAutomaticTest;
        //        }
        //    }
        //}

        /// <summary>
        /// 界面绑定变量，勿用此变量判断用户是否启动软件
        /// </summary>
        [ObservableProperty]
        bool startStop = false;

        [ObservableProperty]
        bool deviceSeting = false;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 是否对焦中
        /// </summary>
        public bool IsFocusing
        {
            get
            {
                foreach (var item in CMainVMs)
                {
                    if (item.FocusCtrlVM?.IsFocusing ?? false)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion


        public CMainModelsModelVM()
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            LoginViewModel.UserChangeAction += (user, success) =>
            {
                SysLog.UserName = user.UserName;
                OperateLog.UserName = user.UserName;
            };
            TypeAdapterConfig<Brush, Brush>.NewConfig().MapWith(des => des);
            TypeAdapterConfig<Token, Token>.NewConfig().MapWith(des => des);
            TypeAdapterConfig<dynamic, dynamic>.NewConfig().MapWith(des => des);
        }

        #region 时间相关
        [ObservableProperty]
        static string systemTime;

        static DateTime StartTime = DateTime.Now;

        [ObservableProperty]
        static string runingTime = DateTime.Now.ToString("T");

        private void Timer_Tick(object sender, EventArgs e)
        {
            SystemTime = DateTime.Now.ToString("yyyy-MM-dd\r\nHH:mm:ss");
            var runTimeSpan = DateTime.Now - StartTime;
            RuningTime = runTimeSpan.ToString(@"hh\:mm\:ss");
            //if (CMainVMs.Count > 0)
            //{
            //    DateTime t = CMainVMs[0].SystemSettings.NextClearTime;
            //    //if (DateTime.Now >= e)
            //    //{
            //    //    if (CMainVMs[0].SystemSettings.AutoClearEnable)
            //    //    {
            //    //        CMainVMs[0].MaociDefectsProduce.Clear();
            //    //    }
            //    //}
            //}
        }
        #endregion

        #region 软件加载
        /// <summary>
        /// 软件加载
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task LoadAsync(IProgress<string> progress)
        {
            IsLoading = true;
           
            await Task.Run(async () =>
            {
                #region 读取主配置文件
                //在app.xaml.cs中读取
                //try
                //{
                //    SystemSettings = CSysSet.LoadParameter();
                //    if (SystemSettings != null)
                //    {
                //        s_SysLog.Info(SystemSettingResources.SystemSettingsReadSuccess);

                //        //CLoading.DispText("读取系统配置成功...", 10);
                //    }
                //    else
                //    {
                //        s_SysLog.Error(SystemSettingResources.SystemSettingsReadFailed);
                //        //CLoading.DispText("读取系统配置失败...", 10);
                //    }
                //    progress.Report(10);

                //await longtimefunc(progress);
                //}
                //catch (Exception) { }
                #endregion
                #region 读取所有光源dll
                try
                {
                    LightManagement = new CLinghtManagement(LightNames);
                  
                    Dispatcher.Invoke(() =>
                    {
                        CLinghtManagement.LoadLightParams();
                        foreach (var lightCtl in CLinghtManagement.LightControlDict.Values)
                        {
                          bool re=  lightCtl.Open(lightCtl.BaseConfig);
                            if (re)
                            {
                                Thread.Sleep(10);
                                for (int i = 0; i < lightCtl.BaseConfig.LightChannelList?.Count; i++)
                                {
                                    lightCtl.SetChannelValue(lightCtl.BaseConfig.LightChannelList[i]);
                                    Thread.Sleep(10);
                                }
                            }
                        }
                    });
                  
                }
                catch (Exception ex)
                {
                    Growl.Error(Properties.Resources.初始化光源失败 + "\r\n" + ex.Message);
                }
                #endregion

                #region 读取所有通讯参数文件并连接通讯
                progress.Report(Properties.Resources.正在加载通讯配置);
                try
                {
                    if (CCommunicationManagement.LoadCommParam())
                    {
                        ListCommSetParam = CCommunicationManagement.CommParamDic.Values.ToList();
                    }
                    else 
                    {
                        Growl.Error(Properties.Resources.通讯参数加载异常);
                    }
                    if (!CCommunicationManagement.OpenAllComm())
                    {
                        Growl.Error(Properties.Resources.通讯连接失败);
                    }
                 
                    //【盘齿方案11-注释】此处只 OpenAllComm；业务 com 等到 OpenProj 识别后再挂（拉链/盘齿互斥）
                }
                catch (Exception ex)
                {
                    Growl.Error(Properties.Resources.通讯连接失败 + "\r\n" + ex.Message);
                }
                #endregion

                #region 读取所有相机参数文件并连接相机
                progress.Report(Properties.Resources.正在加载相机配置);
                try
                {
                    if (CCameraManagement.LoadCamParams())
                    {
                        Growl.Info(Properties.Resources.加载相机参数完成);
                    }
                    else 
                    {
                        Growl.Error(Properties.Resources.加载相机参数失败);
                    }
                }
                catch (Exception ex)
                {
                    Growl.Error(Properties.Resources.相机连接失败 + "\r\n" + ex.Message);
                }
                #endregion

                #region 读取所有算法
                try
                {
                    AlgorithmManagement = new CAlgorithmManagement();
                }
                catch (Exception ex)
                {
                    Growl.Error(Properties.Resources.算法读取失败 + "\r\n" + ex.Message);
                }
                #endregion

                #region 读取所有对焦插件
                try
                {
                    FocusManagement = new CFocusManagement();
                }
                catch (Exception ex)
                {
                    Growl.Error(Properties.Resources.对焦插件读取失败 + "\r\n" + ex.Message);
                }
                #endregion

                #region 读取数据库
                try
                {
                    CMysqlBLL cMysql = SQLManagement.SqlLoad() as CMysqlBLL; //数据库采用统一配置
                    MySqlVM.MysqlExecute = cMysql;
                }
                catch (Exception ex)
                {
                    Growl.Error("数据库初始化出错:" + "\r\n" + ex.Message);
                }
                #endregion
                #region 读取拉链信息
                //【盘齿方案11-注释】LoadParameter 改到拉链 OpenProj，启动欢迎页不读拉链 JSON/形状模板
               // ZipperInfo = CZipperAutomaticAlgorithm.ZipperInfo;
                #endregion
            });
        }

        #endregion

        #region 打开工程文件
        /// <summary>
        /// 软件加载
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task OpenProj(IProgress<string> progress, string header)
        {
            IsLoading = true;
            //WeakReferenceMessenger.Default.Reset();
            #region 打开工程
            try
            {
                progress.Report(Properties.Resources.正在打开);
                //【盘齿方案11-注释】先加载再识别；插件混用则中止，不拆当前工程
                CMainModelsModel loaded = ConfigAPI.Load<CMainModelsModel>(header);
                if (loaded == null || loaded.CProcessGroups == null)
                {
                    Growl.Warning("工程文件无效或无法读取，已中止打开。");
                    SysLog.Error("OpenProj Load 失败: " + header);
                    return;
                }
                if (!COpenProjectLine.TryRecognize(loaded, out OpenProjectLineKind lineKind, out string conflictMsg))
                {
                    Growl.Warning(conflictMsg);
                    SysLog.Error(conflictMsg);
                    return;
                }
                DetachCurrentLine();
                ProjPath = header;
                RemoveAllProcessGroup();
                CMainMModel = loaded;
                COpenProjectLine.Kind = lineKind;
                AttachCurrentLine();
                SysLog.Info("OpenProjectLine=" + lineKind);
                Growl.Info("OpenProjectLine=" + lineKind);
                foreach (var group in CMainMModel.CProcessGroups)
                {
                    group.Init();
                    WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                        group.MaociQualityConfig,
                        group.MaociQualityConfig.token
                    );
                }
                UpdateMainVMs();
                //【盘齿方案11-注释】配方张数只下发给盘齿工程
                if (COpenProjectLine.IsGear)
                {
                    CGearLineHost.SendLoadedRecipePhotoAndFocus(CMainVMs, SysLog);
                }
                if (COpenProjectLine.IsXinGear)
                {
                    CXinGearLineHost.SendLoadedRecipePhotoCount(CMainVMs, SysLog);
                }
                SystemSettings.RecentProjs.Remove(header);
                SystemSettings.RecentProjs.Insert(0, header);
                progress.Report(Properties.Resources.正在更新项目列表);
                for (int i = SystemSettings.RecentProjs.Count - 1; i >= 0; i--)
                {
                    if (!File.Exists(SystemSettings.RecentProjs[i]))
                        SystemSettings.RecentProjs.Remove(SystemSettings.RecentProjs[i]);
                }
                while (SystemSettings.RecentProjs.Count > 10)
                {
                    SystemSettings.RecentProjs.RemoveAt(SystemSettings.RecentProjs.Count - 1);
                }
                await longtimefunc(progress);
            }
            catch (Exception ex)
            {
                SysLog.Error(ex.Message);
            }
            #endregion
        }

        //【盘齿方案2-注释】开工程成功后按制程 Name 收集 PhotoTotalCount，写一次配方张数
        /// <summary>
        /// 【盘齿方案11-注释】卸上一产线业务包，Kind 置 None。底层 Modbus 连接保持。
        /// </summary>
        void DetachCurrentLine()
        {
            CGearLineHost.Detach();
            CZipperLineHost.Detach(CMainMModel?.CProcessGroups);
            CCrankLineHost.Detach(); // 【曲轴方案2-注释】停心跳/ID 轮询，不清底层 Modbus
            CXinGearLineHost.Detach(); // 【新兴盘齿方案11-注释】空壳 Detach，不清底层 Modbus
            COpenProjectLine.Kind = OpenProjectLineKind.None;
            OnPropertyChanged(nameof(IsXinGear));
        }

        /// <summary>
        /// 【盘齿方案11-注释】按已提交的 Kind 挂业务 com。拉链工程在此 IniAutomaticAlgorithm。
        /// </summary>
        void AttachCurrentLine()
        {
            if (COpenProjectLine.IsGear)
            {
                CGearLineHost.Attach();
            }
            else if (COpenProjectLine.IsZipper)
            {
                CZipperLineHost.Attach(Dispatcher, SysLog);
            }
            else if (COpenProjectLine.IsCrank) // 【曲轴方案2-注释】挂 CCrankCommunicate；不下发 HD1200
            {
                CCrankLineHost.Attach();
            }
            else if (COpenProjectLine.IsXinGear) // 【新兴盘齿方案11-注释】挂 Host；开机写三路张数
            {
                CXinGearLineHost.Attach();
            }
        }

        async Task longtimefunc(IProgress<string> progress)
        {
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(10);
                progress.Report(i.ToString());
            }
        }
        #endregion

        #region 保存当前工程
        public void SaveCurrentProj()
        {
            if (string.IsNullOrEmpty(ProjPath))
                return;
            if (CMainMModel?.CProcessGroups?.Count == 0)
                return;
            SystemSettings.RecentProjs.Remove(ProjPath);
            SystemSettings.RecentProjs.Insert(0, ProjPath);
            ConfigAPI.Save(CMainMModel, ProjPath);
        }
        #endregion

        #region 帮助文档
        [RelayCommand]
        private void HelpLocalHost()
        {
            try
            {
                string path = @"D:\HelpFile\毛刺检测软件\dist";
                string name = "WH_MetalBurr";
                int port = 91;
                string sourceUri = "http://localhost:" + port;
                WH_IIService.OpenHelpFile(path, name, port);
                Process.Start(new ProcessStartInfo(sourceUri) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.帮助文档打开失败);
            }
        }
        #endregion
        /// <summary>
        /// 2024.9.2 李焕彬
        /// 移除制程
        /// </summary>
        [RelayCommand]
        public void RemoveProcess()
        {
            if (SelectedProcess != null)
            {
                Growl.AskGlobal(
                    Properties.Resources.DelecteAsk,
                    b =>
                    {
                        if (b)
                        {
                            SelectedProcess.ProcessGroup?.RemoveProcess(SelectedProcess);
                            UpdateMainVMs();
                            OperateLog.Info(Properties.Resources.删除制程);
                        }
                        return true;
                    }
                );
            }
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 复位报警
        /// </summary>
        [RelayCommand]
        public void ResetAlarm()
        {
            foreach (var item in CMainVMs)
            {
                item.FocusCtrlVM?.Reset();
            }
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 更新多制程视图模型
        /// </summary>
        //【盘齿方案0.5-注释】无分页固定布局判定：7 个盘齿制程名全部命中才用固定 4x3 模板，否则回通用 UniformGrid
        public bool UseGearFixedLayout =>
            CMainVMs.Count == COpenProjectLine.GearFixedProcessNames.Length
            && COpenProjectLine.GearFixedProcessNames.All(p => CMainVMs.Any(m => m.Name == p));

        // 【曲轴方案0.5-注释】六名全中且数量=6 才套曲轴格；不替代 IsCrank；不写入 GearFixedProcessNames
        public bool UseCrankFixedLayout =>
            CMainVMs.Count == COpenProjectLine.CrankFixedProcessNames.Length
            && COpenProjectLine.CrankFixedProcessNames.All(p => CMainVMs.Any(m => m.Name == p));

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】当前打开工程是新兴盘齿则主视图固定拍摄分页。不替代协议挂接以外的 IsXinGear 判断。
        /// </summary>
        public bool IsXinGear => COpenProjectLine.IsXinGear;

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】拍摄分页页1：齿底、齿顶。
        /// </summary>
        public IReadOnlyList<CMainModel> XinGearShotFaceProcesses
        {
            get
            {
                List<CMainModel> list = new List<CMainModel>();
                foreach (string name in COpenProjectLine.XinGearFixedProcessNames.Take(2))
                {
                    CMainModel item = CMainVMs.FirstOrDefault(m => m.Name == name);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】页2 绑定这一个侧面制程的 6 张分图，禁止拆成多个制程。
        /// </summary>
        public CMainModel XinGearSideProcess =>
            CMainVMs.FirstOrDefault(m => m.Name == COpenProjectLine.XinGearFixedProcessNames[2]);

        /// <summary>
        /// 【新兴盘齿方案0.7-注释】页2 当前格。不进 .burrproj。选中后 SelectedProcess 仍是侧面。
        /// </summary>
        [ObservableProperty]
        XinGearShotTileVM selectedXinGearShotTile;

        partial void OnSelectedXinGearShotTileChanged(XinGearShotTileVM value)
        {
            if (value == null)
            {
                return;
            }
            CMainModel side = XinGearSideProcess;
            if (side == null)
            {
                return;
            }
            SelectedProcess = side;
            side.ApplyXinGearShotTileFilterPreview(value.PhotoIndex);
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】点侧面页：SelectedProcess 仍是侧面。
        /// 【新兴盘齿方案0.7-注释】未选格时默认侧面 1。
        /// </summary>
        [RelayCommand]
        public void SelectXinGearSide()
        {
            CMainModel side = XinGearSideProcess;
            if (side == null)
            {
                return;
            }
            SelectedProcess = side;
            if (SelectedXinGearShotTile == null || !side.ShotTiles.Contains(SelectedXinGearShotTile))
            {
                SelectedXinGearShotTile = side.ShotTiles.FirstOrDefault();
            }
            else
            {
                side.ApplyXinGearShotTileFilterPreview(SelectedXinGearShotTile.PhotoIndex);
            }
        }

        public void UpdateMainVMs()
        {
            ObservableCollection<CMainModel> mainVMs = new ObservableCollection<CMainModel>();
            foreach (var group in CMainMModel.CProcessGroups)
            {
                foreach (var item in group.CMainModels)
                {
                    mainVMs.Add(item);
                }
            }
            foreach (var mainVM in CMainVMs)
            {
                if (mainVMs.FirstOrDefault(o => o == mainVM) == null)
                {
                    WeakReferenceMessenger.Default.UnregisterAll(mainVM.MaociAlgorParamConfig);
                    WeakReferenceMessenger.Default.UnregisterAll(mainVM.MaociFilterConfig);
                    WeakReferenceMessenger.Default.UnregisterAll(mainVM.MaociAlarmSetConfig);
                    if (mainVM.MarkConfig != null)
                    {
                        WeakReferenceMessenger.Default.UnregisterAll(mainVM.MarkConfig);
                    }
                    if (mainVM.FocusConfig != null)
                    {
                        WeakReferenceMessenger.Default.UnregisterAll(mainVM.FocusConfig);
                    }
                    mainVM.StopTask();
                }
            }
            SelectedProcess = mainVMs.FirstOrDefault();
            SelectedXinGearShotTile = null;
            CMainVMs = mainVMs;
            OnPropertyChanged(nameof(UseGearFixedLayout)); //【盘齿方案0.5-注释】制程集合变化后刷新固定布局判定
            OnPropertyChanged(nameof(UseCrankFixedLayout)); //【曲轴方案0.5-注释】
            OnPropertyChanged(nameof(IsXinGear)); //【新兴盘齿方案0.6-注释】
            OnPropertyChanged(nameof(XinGearShotFaceProcesses));
            OnPropertyChanged(nameof(XinGearSideProcess));
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 增加制程组，VM已存在
        /// </summary>
        /// <param name="groupVM">制程组视图模型</param>
        public void AddProcessGroup(List<CProcessGroupModel> groupVMs)
        {
            for (int i = CMainMModel.CProcessGroups.Count - 1; i >= 0; i--)
            {
                if (!groupVMs.Contains(CMainMModel.CProcessGroups[i]))
                {
                    WeakReferenceMessenger.Default.UnregisterAll(
                        CMainMModel.CProcessGroups[i].MaociQualityConfig
                    );
                    CMainMModel.CProcessGroups.RemoveAt(i);
                }
            }
            foreach (var groupVM in groupVMs)
            {
                if (CMainMModel.CProcessGroups.FirstOrDefault(o => o == groupVM) == null)
                {
                    CMainMModel.CProcessGroups.Add(groupVM);
                    WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                        groupVM.MaociQualityConfig,
                        groupVM.MaociQualityConfig.token
                    );
                }
            }
            UpdateMainVMs();
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 移除所有制程组
        /// </summary>
        public void RemoveAllProcessGroup()
        {
            if (CMainMModel?.CProcessGroups == null)
            {
                return;
            }
            foreach (var item in CMainMModel.CProcessGroups)
            {
                WeakReferenceMessenger.Default.UnregisterAll(item.MaociQualityConfig);
            }
            CMainMModel.CProcessGroups.Clear();
            UpdateMainVMs();
        }
    }
}
