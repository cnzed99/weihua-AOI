using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlarmSetCtrl;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;
using FocusControl;
using HandyControl.Controls;
using HistoryPlayback;
using Mapster;
using MarkControl;
using MySqlOperatesApi;
using ProjProduceData;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.ViewModels;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem._4_报警处理;
using WH.DetectSystem._5_存图操作;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;
using ZipperInfo;
using System.Runtime.InteropServices;
using System.IO;


namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 主界面视图模型
    /// </summary>
    public partial class CMainModel
    {
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

        /// <summary>
        /// 系统设置
        /// </summary>
        public CSystemSettingsVM SystemSettings =
            CPublicServices.Container.Resolve<CSystemSettingsVM>();

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

        private CProcessGroupModel processGroup;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程组
        /// </summary>
        public CProcessGroupModel ProcessGroup
        {
            get { return processGroup; }
            set
            {
                if (SetProperty(ref processGroup, value))
                {
                    MaociQualityConfig = processGroup.MaociQualityConfig;
                }
            }
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 质量等级
        /// </summary>
        public CQualityConfig MaociQualityConfig
        {
            set
            {
                this.SDFilterVM.QualityConfig = MaociQualityConfig;
                this.QualityVM.QualityConfig = MaociQualityConfig;
                MaociFilterConfig.SetSDFilterVM(MaociQualityConfig);
                AlarmSetVM.SetQuality(MaociQualityConfig);
                MaociDefectsProduce.SetQuality(MaociQualityConfig);
                this.AlarmSetVM.SetFilter(new() { MaociFilterConfig });
                MaociDefectsProduce.SetFilter(new() { MaociFilterConfig });
            }
            get => ProcessGroup?.MaociQualityConfig;
        }

        //【盘齿方案0.1-注释】 VM: HasProcessSubWindow / ProcessSubWindowTitle + collection hook; not in .burrproj
        [JsonIgnore]
        public bool HasProcessSubWindow =>
            ProcessSubWindows != null && ProcessSubWindows.Any(o => o.Enabled);

        [JsonIgnore]
        public string ProcessSubWindowTitle
        {
            get
            {
                var item = ProcessSubWindows?.FirstOrDefault(o => o.Enabled);
                if (item == null || string.IsNullOrWhiteSpace(item.DisplayName))
                    return ProcessSubWindowList.DefaultTitle;
                return item.DisplayName;
            }
        }

        partial void OnProcessSubWindowsChanged(
            ObservableCollection<CProcessSubWindowItem> oldValue,
            ObservableCollection<CProcessSubWindowItem> newValue
        )
        {
            UnhookProcessSubWindowCollection(oldValue);
            HookProcessSubWindowCollection(newValue);
            NotifyProcessSubWindowDisplay();
        }

        public void AttachProcessSubWindowNotifications()
        {
            UnhookProcessSubWindowCollection(ProcessSubWindows);
            HookProcessSubWindowCollection(ProcessSubWindows);
            NotifyProcessSubWindowDisplay();
        }

        void NotifyProcessSubWindowDisplay()
        {
            OnPropertyChanged(nameof(HasProcessSubWindow));
            OnPropertyChanged(nameof(ProcessSubWindowTitle));
        }

        void HookProcessSubWindowCollection(ObservableCollection<CProcessSubWindowItem> col)
        {
            if (col == null)
                return;
            col.CollectionChanged += ProcessSubWindows_CollectionChanged;
            foreach (var item in col)
            {
                item.PropertyChanged += ProcessSubWindowItem_PropertyChanged;
            }
        }

        void UnhookProcessSubWindowCollection(ObservableCollection<CProcessSubWindowItem> col)
        {
            if (col == null)
                return;
            col.CollectionChanged -= ProcessSubWindows_CollectionChanged;
            foreach (var item in col)
            {
                item.PropertyChanged -= ProcessSubWindowItem_PropertyChanged;
            }
        }

        void ProcessSubWindows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (CProcessSubWindowItem item in e.OldItems)
                    item.PropertyChanged -= ProcessSubWindowItem_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (CProcessSubWindowItem item in e.NewItems)
                    item.PropertyChanged += ProcessSubWindowItem_PropertyChanged;
            }
            NotifyProcessSubWindowDisplay();
        }

        void ProcessSubWindowItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                string.IsNullOrEmpty(e.PropertyName)
                || e.PropertyName == nameof(CProcessSubWindowItem.Enabled)
                || e.PropertyName == nameof(CProcessSubWindowItem.DisplayName)
            )
            {
                NotifyProcessSubWindowDisplay();
            }
        }

        /// <summary>
        /// 20240707 TCG
        /// 初始化当前制程，分配过滤、等级、算法配置对象，注册参数修改消息
        /// </summary>
        public void Init(CProcessGroupModel processGroup)
        {
            if (MaociAlgorParamConfig.DefectSpecies != null)
            {
                foreach (var sp in MaociAlgorParamConfig.DefectSpecies)
                {
                    foreach (var re in sp.RecipeDefects)
                    {
                        var newsp = MaociFilterConfig[sp.Name][re.Name];
                        if (newsp == null)
                        {
                            RecipeDefect rd = new RecipeDefect(re.Name, re.Category, MaociFilterConfig[sp.Name].RecipeDefects[0].token);
                            MaociFilterConfig[sp.Name].RecipeDefects.Add(rd);
                        }
                    }
                }
            }
            this.MaociAlgorVM.Config = MaociAlgorParamConfig;
            this.SDFilterVM.FilterConfig = MaociFilterConfig;
            this.SDFilterVM.DefectFeactures = MaociAlgorParamConfig.DefectFeatures;

            this.DefectsDataVM.DefectsProduce = MaociDefectsProduce;
            this.AlarmSetVM.CAlarmSet = MaociAlarmSetConfig;

            this.HistoryVM.HistoryModel = MaociHistoryModel;
            MaociHistoryModel.SetHistory(MaociFilterConfig);
            //MaociMysqlConfig.SetSQL(MaociFilterConfig);
            if (AppConfig.HasFocusConfig())
            {
                FocusCtrlVM = FocusConfig.CreateCtrlVM();
                FocusCtrlVM.SetCameraSerial(CameraSerial);
                // this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();
                FocusCtrlVM.InitControl();
            }
            if (AppConfig.HasMarkConfig())
            {
                MarkCtrlVM = new CMarkCtrlVM();
                MarkCtrlVM.MarkConfig = MarkConfig;
                MarkCtrlVM.Connect();
            }

            AlarmSetVM.Reset();
            HistoryVM.Reset();
            QualityVM.Reset();
            // 数据清零事件
            //SystemSettings.ClearProduceEvent += () =>
            //{
            //    if (SystemSettings.AutoClearEnable)
            //    {
            //        this.DefectsDataVM.DefectsProduce.Clear();
            //    }
            //};

            this.ProcessGroup = processGroup;
            AttachProcessSubWindowNotifications();
            if (
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CamParamDict[CameraSerial].ProjGuid = GUID;
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel =
                    this.m_WaitImgChannel;
                //CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
                //    MaociAlgorParamConfig.GetDistinctFunc();
            }
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociFilterConfig,
                MaociFilterConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociAlgorParamConfig,
                MaociAlgorParamConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociAlarmSetConfig,
                MaociAlarmSetConfig.token
            );
            if (MarkConfig is not null)
                WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                    MarkConfig,
                    MarkConfig.token
                );
            if (FocusConfig is not null)
                WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                    FocusConfig,
                    FocusConfig.token
                );
            //【盘齿方案0-注释】原因：取消制程Init内拉链自动识别完成事件订阅，TestFinshTodo及HSV回写方法群留作复盘不删
            // 原： CZipperAutomaticAlgorithm.Instance.TestFinshEven += TestFinshTodo;
            // CZipperAutomaticAlgorithm.ZipperInfoChangeEven += InfoChangeFunc;
            InitTask();
            UpdateVMLoginPerson(CLoginViewModel.SloinPerson);
            //using (var ms = new MemoryStream(Properties.Resources.黑背景))
            //{
            //    var bitmap = new BitmapImage();
            //    bitmap.BeginInit();
            //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmap.StreamSource = ms;
            //    bitmap.EndInit();
            //    bitmap.Freeze();
            //    ClearImage = bitmap;
            //}
            ClearImage = new BitmapImage(new Uri("pack://application:,,,/WH.DetectSystem;component/Resources/黑背景.png"));

            //  BitmapSource bitmap = new BitmapImage(new Uri("C://Users//Administrator.B//Desktop//黑背景.png"));
            var defectNumbers = this.DefectsDataVM.DefectsProduce.DefectNumbersList.Where(d => d.Number != 0);
            this.DefectsDataVM.DefectsProduce.DefectNumbersSortList = defectNumbers.OrderByDescending(d => d.Number).ToList();
            HistoryVM.HisTital = this.Name;
            HistoryVM.ClearImage = ClearImage.Clone();
        }

        [ObservableProperty]
        private BitmapSource modelImage; //= new BitmapImage(new Uri("D://铝极.png"));

        [ObservableProperty]
        private BitmapSource zipperPullImage;

        /// <summary>
        /// 2024.7.25 李焕彬
        /// 主窗口操作对象
        /// </summary>
        [ObservableProperty]
        private ImageView curView;

        [ObservableProperty]
        private Brush modelBrush = Brushes.White;

        [ObservableProperty]
        private BitmapSource lastImage; //= new BitmapImage(new Uri("D://铝极.png"));

        /// <summary>
        /// 2024.7.25 李焕彬
        /// 拉头窗口操作对象
        /// </summary>
        [ObservableProperty]
        private ImageView lastView;

        [ObservableProperty]
        private Brush lastBrush = Brushes.White;

        private BitmapSource ClearImage;


        /// <summary>
        /// 新建制程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="algorithm"></param>
        /// <param name="cameraSerial"></param>
        /// <param name="processGroup"></param>
        public CMainModel(
            string name,
            string algorithm,
            string focus,
            string cameraSerial,
            CProcessGroupModel processGroup
        )
        {
            GUID = Guid.NewGuid().ToString();
            this.token = new Token(GUID, this.GetType().Namespace);
            this.Name = name;
            this.Algorithm = algorithm;
            this.Focus = focus;
            this.CameraSerial = cameraSerial;

            //【盘齿方案4-改动D】按制程名写入默认张数 N：内孔=6；上轴侧面/下轴侧面=10；整轴侧面=14；其余（下端面/上齿面/上端面等）=1
            this.PhotoTotalCount = name switch
            {
                "内孔" => 6,
                "上轴侧面" => 10,
                "下轴侧面" => 10,
                "整轴侧面" => 14,
                _ => 1,
            };
            this.MaociAlgorParamConfig = CAlgorithmManagement
                .AlgorithmHeper[Algorithm]
                .CreateNewAlgorithm(name);
            this.MaociFilterConfig = new CFilterConfig(this.MaociAlgorParamConfig.DefectSpecies);
            if (AppConfig.HasFocusConfig())
                this.FocusConfig = CFocusManagement.FocusHeper[Focus].CreateNewfocus();
            if (AppConfig.HasMarkConfig())
            {
                MarkConfig = new CMarkConfig();
            }
            this.UpdateToken(); //更新Token要在Init前
            this.UpdateName();
            Init(processGroup);
        }

        #region 时间相关

        [ObservableProperty]
        private double algorithmTime = 0;

        [ObservableProperty]
        private double filterTime = 0;

        #endregion 时间相关

        /// <summary>
        /// 20240716 TCG
        /// 当前制程的token，用于向窗口传递界面更新数据，窗口需实现IRecipient<T> 借口
        /// </summary>
        public Token TokeVM { get; set; }

        /// <summary>
        /// 算法参数控件VM
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CMaociAlgorParamCtrlVm maociAlgorVM = new CMaociAlgorParamCtrlVm();

        /// <summary>
        /// 检测设置控件VM
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CSDFilterCtrlVM sDFilterVM = new CSDFilterCtrlVM();

        /// <summary>
        /// 质量等级控件VM
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CQualityCtrlVM qualityVM = new CQualityCtrlVM();

        /// <summary>
        /// 缺陷数据VM
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CDefectsDataVM defectsDataVM = new CDefectsDataVM();

        /// <summary>
        /// 报警设置
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CAlarmSetConfigVM alarmSetVM = new CAlarmSetConfigVM(); //报警

        /// <summary>
        /// 历史图回看
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        private CHistoryVM historyVM = new CHistoryVM(); //历史回看

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 打标控制VM,初始化需要放在运动控制前面
        /// </summary>
        [ObservableProperty]
        private CMarkCtrlVM markCtrlVM;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦控制VM
        /// </summary>
        [ObservableProperty]
        private CFocusCtrlVMBase focusCtrlVM;

        /// <summary>
        /// 多个cells合并
        /// </summary>
        private List<Cell> MergeCells = new List<Cell>();

        #region 启停 状态

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 启动时用于保存当前账户信息 停止运行时用于恢复权限
        /// </summary>
        private CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

        private bool isStart = false;

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 是否启动
        /// </summary>
        public bool IsStart
        {
            get => isStart;
            set
            {
                //if (this.IsManualTest)
                //{
                //    Growl.Warning("请退出设置或离线手动模式！");
                //    return;
                //}
                SetProperty(ref isStart, value);
                //if (value)
                //{
                //    UpdateVMLoginPerson(loginPerson);
                //}
                //else
                //{
                //    UpdateVMLoginPerson(CLoginViewModel.SloinPerson);
                //}
                FocusCtrlVM?.SetRunning(IsStart);
                MarkCtrlVM?.SetRunning(IsStart);
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
                FocusCtrlVM?.SetRunning(isManualTest);
                MarkCtrlVM?.SetRunning(isManualTest);
            }
        }

        private bool isAutomaticTest = false;

        /// <summary>
        /// 2025.6.23 鲍赞宝
        /// 拉链自动识别模式
        /// </summary>
        public bool IsAutomaticTest
        {
            get { return isAutomaticTest; }
            set
            {
                isAutomaticTest = value;
            }
        }

        [ObservableProperty]
        private bool deviceSeting = false;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 是否对焦中
        /// </summary>
        public bool IsFocusing
        {
            get { return FocusCtrlVM?.IsFocusing ?? false; }
        }

        #endregion 启停 状态

        #region 线程管理

        private CancellationTokenSource m_cts = new CancellationTokenSource();

        public static readonly BoundedChannelOptions s_NormalChannelOptions =
            new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };

        public static readonly BoundedChannelOptions s_SaveImgchannelOptions =
            new BoundedChannelOptions(5) { FullMode = BoundedChannelFullMode.Wait };

        public static readonly BoundedChannelOptions s_SinglechannelOptions =
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.Wait };

        public static readonly BoundedChannelOptions s_WaitIDchannelOptions =
        new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };

        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly Channel<PrintMsg> m_InfoChannel = Channel.CreateBounded<PrintMsg>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 取图队列
        /// </summary>
        public readonly Channel<Cell> m_WaitImgChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 算法 图像队列
        /// </summary>
        private readonly Channel<Cell> m_AlgorithmChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 过滤 图像队列
        /// </summary>
        private readonly Channel<Cell> m_FilterChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 显示 图像队列
        /// </summary>
        private readonly Channel<Cell> m_ShowImageChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 报警队列
        /// </summary>
        private readonly Channel<Cell> m_AlarmChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 数据库队列
        /// </summary>
        private readonly Channel<Cell> m_dataBaseChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 存储 图像队列
        /// </summary>
        private readonly Channel<Cell> m_SaveImageChannel = Channel.CreateBounded<Cell>(
            s_NormalChannelOptions
        );

        /// <summary>
        /// 等待ID队列
        /// </summary>
        public readonly Channel<ZipperID> m_WaitIDChannel = Channel.CreateBounded<ZipperID>(s_WaitIDchannelOptions);

        public AutoResetEvent WaitSignal = new AutoResetEvent(false);

        private bool abc(FilterAndSelect filter)
        {
            return filter.IsReversal;
        }

        /// <summary>
        /// 拉链自动识别算法
        /// </summary>
       // public CZipperAutomaticAlgorithm ZipperAutomaticAlgorithm = new CZipperAutomaticAlgorithm();
        private void InitTask()
        {
            #region 信息记录线程

            Task infoTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                while (true)
                {
                    await foreach (PrintMsg msg in m_InfoChannel.Reader.ReadAllAsync())
                    {
                        try
                        {
                            await Task.Delay(10);
                            Action<string> act = msg.logType switch
                            {
                                LOG.LOG_INFO => SysLog.Info,
                                LOG.LOG_ERROR => SysLog.Error,
                                LOG.LOG_OK => SysLog.OK,
                                LOG.LOG_NG => SysLog.NG,
                                LOG.LOG_TIP => SysLog.Tip,
                                LOG.LOG_WARN => SysLog.Warn,
                                _ => SysLog.Info
                            };
                            act(Name + "-" + msg.message);
                        }
                        catch (Exception e)
                        {
                            SysLog.Error("信息记录线程出错:" + e.Message + e.StackTrace);
                        }
                    }
                }
            });

            #endregion 信息记录线程

            #region 取图线程
            int tempphotoID = 0;
            int tempid = 0;
            bool IDisRight = false;
            Task waitGetImageTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                await foreach (Cell cell in m_WaitImgChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        //【盘齿方案4】改动B/G2：转发 Cell 已由上齿面绑好；必须在改动A 与离线 ImageFile=="" 覆盖之前拦截
                        if (cell.IsPreBound)
                        {
                            IDisRight = true;
                        }
                        else if (IsStart && !isAutomaticTest) //自动运行
                        {
                            //【盘齿方案11-注释】拉链三分支绑 ID；盘齿 GetProductID + 上齿面转发
                            if (COpenProjectLine.IsZipper)
                            {
                                if (TryConsumeZipperCaptureReject(cell, ref IDisRight))
                                {
                                    continue;
                                }
                            }
                            else if (COpenProjectLine.IsGear)
                            {
                                if (TryConsumeGearFormalCapture(cell, ref IDisRight))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                IDisRight = true;
                            }
                        }
                        else
                        {
                            //【盘齿方案4】改动B：离线上齿面用文件名已写入的 PhotoIndex 当 k（无相机时 IsStart=false 走本分支）
                            if (COpenProjectLine.IsGear && TryConsumeGearOfflineToothTop(cell))
                            {
                                continue;
                            }
                            IDisRight = true;
                            if (cell.ImageFile == "") //手动调试
                            {
                                cell.ID = (ProcessGroup.MaociDefectsProduce.Total + 1).ToString();
                                cell.PhotoIndex = 1;
                                //【盘齿方案4-注释】原因：离线/手动张数改读制程配置 PhotoTotalCount（方案4 改动D；方案审核 G3——写死 1 则旋转工位离线合并验收不通）
                                // 原： cell.PhotoTatolCount = 1;
                                cell.PhotoTatolCount = this.PhotoTotalCount;
                            }
                            //【盘齿方案4】改动A：OffLineTestCtrl 已写 ID/PhotoIndex/PhotoTatolCount 且 ImageFile 非空时透传，不覆盖
                        }

                        // cell.EncoderPos = MarkCtrlVM?.GetEncoderCount() ?? 0;
                        if ((IsStart || IsManualTest || isAutomaticTest) && IDisRight)
                        {
                            if (!m_AlgorithmChannel.Writer.TryWrite(cell))
                            {
                                cell.Dispose();
                            }
                        }
                        else
                        {
                            BitmapSource bitmapSource = cell.Image?.ToBitmapSource();
                            _ = CMainModelsModelVM.Dispatcher?.BeginInvoke(
                                new Action(() =>
                                {
                                    ModelImage = bitmapSource;
                                })
                            );
                            //if (FocusCtrlVM?.IsFocusing ?? false)
                            //{
                            //    if (!FocusCtrlVM.FocusWaitGetImageChannel.Writer.TryWrite(cell))
                            //        cell.Dispose();
                            //}
                            //else
                            //{
                            //    cell.Dispose();
                            //}
                        }
                    }
                    catch (Exception)
                    {
                        await m_InfoChannel.Writer.WriteAsync(new PrintMsg("取图出错！", LOG.LOG_ERROR));
                    }
                }
            });

            #endregion 取图线程

            #region PC算法执行线程

            Task waitRecipeTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                await foreach (Cell cell in m_AlgorithmChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        cell.Stopwatch.Restart();
                        if (COpenProjectLine.IsZipper)
                        {
                            CopyZipperInfoOntoCell(cell, IsStart && !isAutomaticTest);
                        }
                        cell.ProjName = Name;
                        cell.ProjGuid = GUID;
                        try
                        {
                            if (!isAutomaticTest)
                            {
                                MaociAlgorParamConfig.MaociExcute(cell);
                            }
                            else if (COpenProjectLine.IsZipper)
                            {
                                CZipperAutomaticAlgorithm.Instance.ZipperAutomaticAlgorithmRun(cell);
                            }

                        }
                        catch (Exception ex)
                        {
                            await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("算法执行出错：" + ex.Message, LOG.LOG_ERROR));
                        }

                        cell.RecipeTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                        AlgorithmTime = cell.RecipeTime.TotalMilliseconds;
                        cell.Stopwatch.Restart();
                        try
                        {
                            if (!isAutomaticTest) //运行
                            {
                                //【盘齿方案4】G1：算法线程清旧 ID 残图（取图线程不得操作 MergeCells）
                                List<Cell> staleIdCells = MergeCells.FindAll(c => c.ID != cell.ID);
                                if (staleIdCells.Count > 0)
                                {
                                    SysLog.Warn($"{Name}-残图清台：新ID:{cell.ID}，清除旧ID残图{staleIdCells.Count}张");
                                    for (int i = 0; i < staleIdCells.Count; i++)
                                    {
                                        SaveOtherOldImage(staleIdCells[i]);
                                        MergeCells.Remove(staleIdCells[i]);
                                    }
                                }
                                MergeCells.Add(cell);
                                SysLog.Info($"{Name}-添加cell到MergeCells->产品ID:{cell.ID},图片编号:{cell.PhotoIndex},当前MergeCells数量:{MergeCells.Count}");
                                List<Cell> currentCells = MergeCells.FindAll(c => c.ID == cell.ID);
                                SysLog.Info($"{Name}-当前MergeCells里{cell.ID}的数量:{currentCells.Count}");
                                if (currentCells.Count >= cell.PhotoTatolCount)
                                {
                                    // sss = 0;
                                    SysLog.Info($"{Name}-满足{currentCells.Count}>={cell.PhotoTatolCount}条件,准备合并");
                                    List<Cell> orderCell = currentCells.OrderBy(c => c.CreateTime).ToList();
                                    Cell newCell = GetMergeCells(orderCell);
                                    if (currentCells.Count > 1)
                                    {
                                        if (COpenProjectLine.IsZipper)
                                        {
                                            AppendZipperMergedStationImages(newCell, currentCells);
                                        }
                                        else if (COpenProjectLine.IsGear)
                                        {
                                            AppendGearMergedStationImages(newCell, currentCells);
                                        }
                                    }
                                    SysLog.Info($"{Name}-准备移除所有{newCell.ID},当前MergeCells里共有{MergeCells.Count}");
                                    MergeCells.RemoveAll(c => c.ID == newCell.ID);
                                    SysLog.Info($"{Name}-已移除所有{newCell.ID},当前MergeCells里共有{MergeCells.Count}");
                                    newCell.Quality = MaociQualityConfig.GetBest();
                                    if (!newCell.Skipthis)
                                        MaociFilterConfig.FilterExute(newCell);
                                    else
                                    {
                                        SetBadCell(newCell);
                                    }
                                    newCell.FilterTime = new TimeSpan(newCell.Stopwatch.ElapsedTicks);
                                    newCell.Stopwatch.Stop();
                                    newCell.ProcessTime = DateTime.Now - newCell.CreateTime;
                                    StringBuilder strbuilder = new StringBuilder("[结束]     ");
                                    strbuilder.Append(newCell.ID);
                                    strbuilder.Append("   检测结束,耗时:");
                                    strbuilder.Append(newCell.ProcessTime.TotalMilliseconds.ToString("F2"));
                                    if (newCell.IsOK)
                                    {
                                        await m_InfoChannel.Writer.WriteAsync(
                                            new PrintMsg(strbuilder.ToString(), LOG.LOG_OK)
                                        );
                                    }
                                    else if (MarkCtrlVM is not null)
                                    {
                                        int markPos = MarkCtrlVM.AddMark(newCell.EncoderPos);
                                        strbuilder.Append($",检测NG,增加打标位置{markPos}！");
                                        await m_InfoChannel.Writer.WriteAsync(
                                            new PrintMsg(strbuilder.ToString(), LOG.LOG_NG)
                                        );
                                    }
                                    else
                                    {
                                        await m_InfoChannel.Writer.WriteAsync(
                                           new PrintMsg(strbuilder.ToString(), LOG.LOG_NG)
                                       );
                                    }
                                    FilterTime = newCell.FilterTime.TotalMilliseconds;


                                    ModelBrush = newCell.Quality.ShowColor.Brush;
                                    if (!newCell.IsOK)
                                    {
                                        LastBrush = ModelBrush;
                                        LastImage = ModelImage;
                                    }
                                    MaociDefectsProduce.Excute(newCell);
                                    MaociAlarmSetConfig.Excute(newCell);

                                    if (ProcessGroup.AddCellAndJudge(newCell, out CCellPro CellOut))
                                    {
                                        ProcessGroup.MaociDefectsProduce.Excute(CellOut.Cell);
                                        ProcessGroup.MaociDefectsOneFlowProduce.Excute(CellOut.Cell);
                                        ProcessGroup.AlarmSetConfig.Excute(CellOut.Cell);
                                       // ProcessGroup.MaociQualityConfig.Excute(CellOut.Cell);
                                        if (COpenProjectLine.IsZipper)
                                        {
                                            SendZipperProcessResult(CellOut);
                                        }
                                        else if (COpenProjectLine.IsGear)
                                        {
                                            SendGearGroupResult(CellOut);
                                        }


                                        if (!m_dataBaseChannel.Writer.TryWrite(CellOut.Cell))
                                        {
                                            //CellOut.Cell.Dispose();
                                        }
                                    }
                                    if (!m_ShowImageChannel.Writer.TryWrite(newCell))
                                    {
                                        newCell.Dispose();
                                        //strbuilder = new StringBuilder("[");
                                        //strbuilder.Append("筛选线程");
                                        //strbuilder.Append("]     ");
                                        //strbuilder.Append(newCell.ID);
                                        //strbuilder.Append("   newCell入显示队列失败。");
                                        //await m_InfoChannel.Writer.WriteAsync(
                                        //    new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                                        //);
                                    }
                                    List<Cell> otheroldcell = MergeCells.Where(c => (DateTime.Now - c.CreateTime).TotalSeconds > 180).ToList(); //把超过5分钟没有进行组合的图片保存起来
                                    if (otheroldcell?.Count > 0)
                                    {
                                        for (int i = 0; i < otheroldcell.Count; i++)
                                        {
                                            SaveOtherOldImage(otheroldcell[i]);
                                            MergeCells.Remove(otheroldcell[i]);
                                        }

                                    }

                                }

                            }
                            else  //自动识别
                            {
                                if (!m_ShowImageChannel.Writer.TryWrite(cell))
                                {
                                    cell.Dispose();
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            await m_InfoChannel.Writer.WriteAsync(
                                new PrintMsg(
                                    "筛选线程执行出错:" + ex.Message + ex.StackTrace,
                                    LOG.LOG_ERROR
                                )
                            );
                            GC.Collect();
                        }
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("配方执行线程出错：" + ex.Message, LOG.LOG_ERROR)
                        );
                        GC.Collect();
                    }
                }
            });

            #endregion PC算法执行线程

            #region 筛选线程 放置在算法线程

            //Task waitFilterTask = Task.Run(async () =>
            //{
            //    Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //    await foreach (Cell cell in m_FilterChannel.Reader.ReadAllAsync())
            //    {
            //        try
            //        {
            //            cell.Quality = MaociQualityConfig.GetBest();
            //            if (!cell.Skipthis)
            //                MaociFilterConfig.FilterExute(cell);
            //            else
            //            {
            //                SetBadCell(cell);
            //            }
            //            cell.FilterTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
            //            cell.Stopwatch.Stop();
            //            cell.ProcessTime = DateTime.Now - cell.CreateTime;
            //            StringBuilder strbuilder = new StringBuilder("[结束]     ");
            //            strbuilder.Append(cell.ID);
            //            strbuilder.Append("   检测结束,耗时:");
            //            strbuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F2"));
            //            if (cell.IsOK)
            //            {
            //                await m_InfoChannel.Writer.WriteAsync(
            //                    new PrintMsg(strbuilder.ToString(), LOG.LOG_OK)
            //                );
            //            }
            //            else
            //            {
            //                int markPos = MarkCtrlVM.AddMark(cell.EncoderPos);
            //                strbuilder.Append($",检测NG,增加打标位置{markPos}！");
            //                await m_InfoChannel.Writer.WriteAsync(
            //                    new PrintMsg(strbuilder.ToString(), LOG.LOG_NG)
            //                );
            //            }
            //            FilterTime = cell.FilterTime.TotalMilliseconds;
            //            if (!m_ShowImageChannel.Writer.TryWrite(cell))
            //            {
            //                cell.Dispose();
            //                //strbuilder = new StringBuilder("[");
            //                //strbuilder.Append("筛选线程");
            //                //strbuilder.Append("]     ");
            //                //strbuilder.Append(cell.ID);
            //                //strbuilder.Append("   cell入显示队列失败。");
            //                //await m_InfoChannel.Writer.WriteAsync(
            //                //    new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
            //                //);
            //            }
            //            ModelBrush = cell.Quality.ShowColor.Brush;
            //            if (!cell.IsOK)
            //            {
            //                LastBrush = ModelBrush;
            //                LastImage = ModelImage;
            //            }
            //            MaociDefectsProduce.Excute(cell);
            //            if (ProcessGroup.AddCellAndJudge(cell, out CCellPro cellOut))
            //            {
            //                ProcessGroup.MaociDefectsProduce.Excute(cellOut.Cell);
            //                if (!m_dataBaseChannel.Writer.TryWrite(cellOut.Cell))
            //                    cell.Dispose();
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            await m_InfoChannel.Writer.WriteAsync(
            //                new PrintMsg("筛选线程执行出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
            //            );
            //            GC.Collect();
            //        }
            //    }
            //});

            #endregion 筛选线程 放置在算法线程

            #region 显示线程

            Task waitShowTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                Stopwatch stopwatch = Stopwatch.StartNew();
                await foreach (Cell cell in m_ShowImageChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        if (stopwatch.ElapsedMilliseconds > 1000 / SystemSettings.DisplayFrameRate)
                        {
                            #region 窗口显示

                            try
                            {
                                BitmapSource bitmapSource = cell.Image?.ToBitmapSource();
                                BitmapSource zipperPullimg = cell.ChangleImgae?.ToBitmapSource();//MatConverter.Mat2BitmapSource(cell.ZipperPullPartImg);
                                ImageView drawView;

                                await CMainModelsModelVM.Dispatcher.BeginInvoke(() =>
                                {
                                    if (CurView != null && LastView != null)
                                    {
                                        CurView.Clear(false);
                                        LastView.Clear(false);
                                        ModelImage = bitmapSource;
                                        if (zipperPullimg != null)
                                        {
                                            ZipperPullImage = zipperPullimg;
                                        }
                                        else
                                        {
                                            ZipperPullImage = ClearImage;
                                        }
                                        foreach (var edge in cell.DrawEdges)
                                        {
                                            if (SystemSettings.ShowDrawEdges)
                                            {
                                                if (edge.ShowInView == 0)
                                                {
                                                    drawView = CurView;
                                                }
                                                else
                                                {
                                                    drawView = LastView;
                                                }

                                                switch (edge.DrawType)
                                                {
                                                    case EMDRAWTYPE.EMDRAWTYPE_POINTS:
                                                        drawView.SetPen(edge.BrushDraw);
                                                        drawView.ImgDrawPoints(edge.Points, false);
                                                        break;

                                                    case EMDRAWTYPE.EMDRAWTYPE_REGION:
                                                        drawView.SetPen(edge.BrushDraw);
                                                        drawView.ImgDrawRegion(edge.Points, false);
                                                        break;

                                                    case EMDRAWTYPE.EMDRAWTYPE_Text:
                                                        drawView.SetFontBrush(edge.BrushDraw);
                                                        drawView.SetFontSize(edge.FontSize);
                                                        drawView.ImgDrawText(
                                                            edge.Text,
                                                            edge.TextPos,
                                                            false
                                                        );
                                                        break;
                                                }
                                            }
                                        }

                                        #region 画取反 OK的结果区域

                                        ////var value = cell.Detections.TakeWhile(de =>
                                        ////    (((DefectFilter)(de.DefectFilter)).FilterList)
                                        ////        .TakeWhile(filter => filter.IsReversal)
                                        ////        .Count() > 0
                                        ////);
                                        //var value = cell
                                        //    .Detections.ToList()
                                        //    .FindAll(de =>
                                        //        (((DefectFilter)(de.DefectFilter)).FilterList)
                                        //            .ToList()
                                        //            .Find(filter => filter.IsReversal)
                                        //            is not null
                                        //    );
                                        //foreach (var detection in value)
                                        //{
                                        //    if (detection.Category != Category.区域
                                        //        || detection.regionOut.Count == 0)
                                        //        continue;
                                        //    if (detection.ShowInView == 0)
                                        //    {
                                        //        drawView = CurView;
                                        //    }
                                        //    else
                                        //    {
                                        //        drawView = LastView;
                                        //    }
                                        //    drawView.SetFontSize(15);
                                        //    DefectFilter defectFilter = detection.DefectFilter;
                                        //    drawView.SetPen(Brushes.Red);
                                        //    drawView.SetFontBrush(Brushes.Red);
                                        //    for (int i = 0; i < detection.regionOut.Count; i++)
                                        //    {
                                        //        drawView.ImgDrawRegion(
                                        //            detection.regionOut[i].points,
                                        //            false
                                        //        );
                                        //        //drawView.ImgDrawText(
                                        //        //    detection.DetectLog[i].ToString(),
                                        //        //    detection.regionOut[i].GetCenter(),
                                        //        //    false
                                        //        //);
                                        //        System.Windows.Point p1 = detection.regionOut[i].GetCenter();
                                        //        System.Windows.Point p2 = new System.Windows.Point(p1.X, cell.Image.ImageHeight);
                                        //        string txtlog = detection.DetectLog[i].ToString().Split(':')[0];
                                        //        drawView.ImgDrawText(
                                        //          txtlog,
                                        //          p2,
                                        //          false
                                        //          );
                                        //        //if (i == detection.regionOut.Count - 1)
                                        //        //{
                                        //        //    drawView.ImgDrawText(
                                        //        //        detection.DetectLog.ToString(),
                                        //        //        detection.regionOut[i].GetCenter(),
                                        //        //        false
                                        //        //    );
                                        //        //}
                                        //    }
                                        //}

                                        #endregion 画取反 OK的结果区域
                                        if (!isAutomaticTest)
                                        {
                                            if (!cell.IsOK && cell.Quality != null)
                                            {
                                                CurView.SetFontSize(25);
                                                CurView.SetFontWeight(System.Windows.FontWeights.Bold);
                                                DefectFilter dstFilter = cell.Detection?.DefectFilter;
                                                StringBuilder textBuilder = new StringBuilder();
                                                textBuilder.Append(cell.Quality.Name);
                                                textBuilder.Append(":");
                                                if (cell.Detection?.Category != Category.区域)
                                                {
                                                    if (cell.Detection?.Value.Count > 0)
                                                    {
                                                        textBuilder.Append($"{dstFilter.Name}-{cell.Detection?.Value.Max().ToString("f2")}");
                                                    }
                                                    else
                                                    {
                                                        textBuilder.Append(dstFilter.Name);
                                                    }
                                                }
                                                else
                                                {
                                                    textBuilder.Append(dstFilter.Name);
                                                }

                                                CurView.SetFontBrush(cell.Quality?.ShowColor.Brush);
                                                CurView.WinDrawText(
                                                    textBuilder.ToString(),
                                                    AlignmentX.Right,
                                                    AlignmentY.Top,
                                                    false
                                                );
                                                //显示所有Region缺陷
                                                if (SystemSettings.ShowAllDefect)
                                                {
                                                    foreach (var detection in cell.Detections)
                                                    {
                                                        if (
                                                            detection.Result
                                                            || detection.Category != Category.区域
                                                            || detection.regionOut.Count == 0
                                                        )
                                                            continue;
                                                        if (detection.ShowInView == 0)
                                                        {
                                                            drawView = CurView;
                                                        }
                                                        else
                                                        {
                                                            drawView = LastView;
                                                        }
                                                        //DefectFilter defectFilter =
                                                        //    detection.DefectFilter;
                                                        // drawView.SetPen(defectFilter.ShowColor.Brush);
                                                        //drawView.SetFontBrush(
                                                        //    defectFilter.ShowColor.Brush
                                                        //);
                                                        drawView.SetFontSize(15);
                                                        CurView.SetFontWeight(System.Windows.FontWeights.Normal);
                                                        DefectFilter defectFilter =
                                                     detection.DefectFilter;
                                                        drawView.SetPen(Brushes.Red);
                                                        drawView.SetFontBrush(
                                                            Brushes.Red
                                                        );
                                                        for (
                                                            int i = 0;
                                                            i < detection.regionOut.Count;
                                                            i++
                                                        )
                                                        {
                                                            drawView.ImgDrawRegion(
                                                                detection.regionOut[i].points,
                                                                false
                                                            );
                                                            //System.Windows.Point p1 = detection.regionOut[i].GetCenter();
                                                            // System.Windows.Point p2 = new System.Windows.Point(p1.X, cell.Image.ImageHeight - 60);
                                                            System.Windows.Point p2 = detection.regionOut[i].GetBottomRight();
                                                            string txtlog = detection.DetectLog[i].ToString().Split(':')[0];
                                                            drawView.ImgDrawText(
                                                              txtlog,
                                                              p2,
                                                              false
                                                              );
                                                            //drawView.ImgDrawText(
                                                            //    detection.DetectLog[i].ToString(),
                                                            //    detection.regionOut[i].GetCenter(),
                                                            //    false
                                                            //);
                                                            //if (i == detection.regionOut.Count - 1)
                                                            //{
                                                            //    drawView.ImgDrawText(
                                                            //        detection.DetectLog.ToString(),
                                                            //        detection.regionOut[i].GetCenter(),
                                                            //        false
                                                            //    );
                                                            //}
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    DefectFilter defectFilter =
                                                        cell.Detection.DefectFilter;
                                                    if (
                                                        !(
                                                            cell.Detection.Result
                                                            || cell.Detection.Category != Category.区域
                                                            || cell.Detection.regionOut.Count == 0
                                                        )
                                                    )
                                                    {
                                                        if (cell.Detection.ShowInView == 0)
                                                        {
                                                            drawView = CurView;
                                                        }
                                                        else
                                                        {
                                                            drawView = LastView;
                                                        }
                                                        drawView.SetFontSize(15);
                                                        CurView.SetFontWeight(System.Windows.FontWeights.Normal);
                                                        //drawView.SetPen(defectFilter.ShowColor.Brush);
                                                        //drawView.SetFontBrush(
                                                        //    defectFilter.ShowColor.Brush
                                                        //);
                                                        drawView.SetPen(Brushes.Red);
                                                        drawView.SetFontBrush(
                                                            Brushes.Red
                                                        );
                                                        for (
                                                            int i = 0;
                                                            i < cell.Detection.regionOut.Count;
                                                            i++
                                                        )
                                                        {
                                                            drawView.ImgDrawRegion(
                                                                cell.Detection.regionOut[i].points,
                                                                false
                                                            );
                                                            // System.Windows.Point p1 = cell.Detection.regionOut[i].GetCenter();
                                                            //  System.Windows.Point p2 = new System.Windows.Point(p1.X, cell.Image.ImageHeight - 60);
                                                            System.Windows.Point p2 = cell.Detection.regionOut[i].GetBottomRight();
                                                            string txtlog = cell.Detection.DetectLog.ToString().Split(':')[0];
                                                            drawView.ImgDrawText(
                                                              txtlog,
                                                              p2,
                                                              false
                                                              );
                                                            //drawView.ImgDrawText(
                                                            //    cell.Detection.DetectLog[i].ToString(),
                                                            //    cell.Detection.regionOut[i].GetCenter(),
                                                            //    false
                                                            //);
                                                            //if (i == cell.Detection.regionOut.Count - 1)
                                                            //{
                                                            //    drawView.ImgDrawText(
                                                            //        cell.Detection.DetectLog.ToString(),
                                                            //        cell.Detection.regionOut[i].GetCenter(),
                                                            //        false
                                                            //    );
                                                            //}
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                CurView.SetFontSize(25);
                                                CurView.SetFontWeight(System.Windows.FontWeights.Bold);
                                                CurView.SetFontBrush(cell.Quality?.ShowColor.Brush);
                                                CurView.WinDrawText(
                                                    "OK",
                                                    AlignmentX.Right,
                                                    AlignmentY.Top,
                                                    false
                                                );
                                            }
                                        }
                                        // drawView.Invalidate();
                                        CurView.Invalidate();
                                        LastView.Invalidate();
                                    }
                                });


                            }
                            catch (Exception ex)
                            {
                                await m_InfoChannel.Writer.WriteAsync(
                                    new PrintMsg(
                                        "显示线程出错: " + ex.Message + ex.StackTrace,
                                        LOG.LOG_ERROR
                                    )
                                );
                                Growl.Error(Name + "-" + "显示线程出错: " + ex.Message + ex.StackTrace);
                            }
                            stopwatch.Restart();

                            #endregion 窗口显示
                        }
                        if (
                            (SystemSettings.OfflineSave || isStart)
                            && (
                                SaveImageVM.Param.SaveImageEnable
                                || SaveImageVM.Param.PiantScreenEnable
                            ) && !isAutomaticTest
                        ) //Clone 比较耗时 只有在开启存图时才复制Cell
                        {
                            Cell copy = cell.Clone();
                            if (!m_SaveImageChannel.Writer.TryWrite(copy))
                            {
                                copy.Dispose();
                            }
                        }

                        //if (!m_AlarmChannel.Writer.TryWrite(cell))
                        //{
                        //    cell.Dispose();
                        //    StringBuilder strbuilder = new StringBuilder("[");
                        //    strbuilder.Append("显示线程");
                        //    strbuilder.Append("]     ");
                        //    strbuilder.Append(cell.ID);
                        //    strbuilder.Append("   cell入报警队列失败。");
                        //    await m_InfoChannel.Writer.WriteAsync(
                        //        new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                        //    );
                        //}

                        if (cell.isOnce) { }
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("显示线程出错: " + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                    }
                    finally
                    {
                        cell.Dispose(); //cell释放后对数据汇总、数据库不影响，故放在显示线程里释放，防止未显示先释放
                        WaitSignal.Set();
                    }
                }
            });

            #endregion 显示线程

            #region 报警线程 放置于数据库线程

            //Task alarmTask = Task.Run(async () =>
            //{
            //    object objAlarmLock = new object(); //报警监控用
            //    Thread.CurrentThread.Priority = ThreadPriority.Normal;
            //    await foreach (Cell cell in m_AlarmChannel.Reader.ReadAllAsync())
            //    {
            //        try
            //        {
            //            lock (objAlarmLock)
            //            {
            //                MaociAlarmSetConfig.Excute(cell);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            await m_InfoChannel.Writer.WriteAsync(
            //                new PrintMsg("监控报警出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
            //            );
            //        }
            //        finally
            //        {
            //            //取消单个制程数据库存储
            //            //if (!m_dataBaseChannel.Writer.TryWrite(cell))
            //            //    cell.Dispose();
            //        }
            //    }
            //});

            #endregion 报警线程 放置于数据库线程

            #region 数据库线程

            //数据库写入容易出错，卡顿时间较长，容量最大10个
            Task dataBaseTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                // object objAlarmLock = new object(); //报警监控用
                await foreach (Cell cell in m_dataBaseChannel.Reader.ReadAllAsync())
                {


                    try
                    {
                        if (MySqlVM.MysqlExecute.SqlEnable)
                        {
                            if (SystemSettings.OfflineSave || IsStart)
                                ProcessGroup.MysqlBLL.AddData(cell, SystemSettings.NowShift);
                        }

                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("Mysql数据库写入出错:" + ex.Message, LOG.LOG_ERROR)
                        );
                        Growl.Warning(
                            new HandyControl.Data.GrowlInfo()
                            {
                                Message = Name + "-" + "Mysql数据库写入出错!",
                                StaysOpen = false,
                                WaitTime = 2,
                            }
                        );
                    }
                    //finally
                    //{
                    //    cell.Dispose();
                    //}


                    //#region 报警
                    //try
                    //{
                    //    lock (objAlarmLock)
                    //    {
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    await m_InfoChannel.Writer.WriteAsync(
                    //        new PrintMsg("监控报警出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                    //    );
                    //}
                    //finally
                    //{
                    //    //取消单个制程数据库存储
                    //    //if (!m_dataBaseChannel.Writer.TryWrite(cell))
                    //    //    cell.Dispose();
                    //}
                    //#endregion


                }
            });

            #endregion 数据库线程

            #region 存图线程

            Task waitSaveImgTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                int saveCount = 0; //存图间隔计数用
                await foreach (Cell cell in m_SaveImageChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        string savePath = SaveImageVM.Param.Excute(
                            SystemSettings,
                            cell,
                            ref saveCount
                        );
                        if (savePath != null)
                        {
                            _ = CMainModelsModelVM.Dispatcher?.BeginInvoke(
                                new Action(() =>
                                {
                                    if (!string.IsNullOrEmpty(savePath))
                                    {
                                        if (savePath.Contains("NG"))
                                        {
                                            if (HistoryVM.HistoryModel.NgImagePaths.Count > 2000)
                                                HistoryVM.HistoryModel.NgImagePaths.RemoveAt(2000);
                                            HistoryVM.HistoryModel.NgImagePaths.Insert(0, savePath);
                                        }
                                        else
                                        {
                                            if (HistoryVM.HistoryModel.OkImagePaths.Count > 2000)
                                                HistoryVM.HistoryModel.OkImagePaths.RemoveAt(2000);
                                            HistoryVM.HistoryModel.OkImagePaths.Insert(0, savePath);
                                        }

                                    }
                                })
                            );
                        }

                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("存图线程出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                    }
                    finally
                    {
                        cell.Dispose(); //这个cell是复制的clone 存图后清理
                    }
                }
            });

            #endregion 存图线程
        }

        /// <summary>
        /// 将多个Cells合并成一个新的Cell
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        Cell GetMergeCells(List<Cell> cells)
        {
            if (cells.Count == 1) //单个的直接返回 20240429 鲍赞宝
            {
                return cells[0];
            }
            else
            {
                Cell newCell = cells[cells.Count - 1].CloneExecptImg();
                // 合并IsOK逻辑：只要有一个为false则整体为false
                newCell.IsOK = !cells.Any(c => !c.IsOK);

                // 按RecipeDefectName合并AlgorithmOut
                newCell.AlgorithmOut = cells
                       .SelectMany(c => c.AlgorithmOut)  // 展平所有AlgorithmOut
                       .GroupBy(cd => cd.RecipeDefectName) // 按名称分组
                       .Select(g => new CellDetection
                       {
                           RecipeDefectName = g.Key,
                           regionOut = g.SelectMany(cd => cd.regionOut).ToList(),
                           Category = g.FirstOrDefault()?.Category ?? Category.区域,
                           Value = g.SelectMany(cd => cd.Value).ToList(),
                           Type = g.FirstOrDefault()?.Type ?? "",
                           Index = g.Max(cd => cd.Index), // 取最大Index
                           ShowInView = g.Max(cd => cd.ShowInView)
                       }).ToList();
                for (int i = 0; i < cells.Count; i++)
                {
                    newCell.DrawEdges.AddRange(cells[i].DrawEdges);
                    if (COpenProjectLine.IsZipper)
                    {
                        CopyZipperMergeSideFields(newCell, cells[i]);
                    }
                    //【盘齿方案4-注释】原因：拉链专属字段/高低曝光合并，盘齿改用 GearImages+MergedPanorama；工位张号 1..N
                    // 原：if (cells[i].ZipperPullPartImg != null)
                    // 原：{
                    // 原：    newCell.ZipperPullPartImg = cells[i].ZipperPullPartImg;
                    // 原：}
                    // 原：if (cells[i].UpMassMatImg != null && cells[i].UpMassMatImg.Count > 0)
                    // 原：{
                    // 原：    for (int j = 0; j < cells[i].UpMassMatImg.Count; j++)
                    // 原：    {
                    // 原：        newCell.UpMassMatImg.Add(cells[i].UpMassMatImg[j]);
                    // 原：    }
                    // 原：}
                    // 原：if (cells[i].FourCutMatImg != null && cells[i].FourCutMatImg.Count > 0)
                    // 原：{
                    // 原：    for (int j = 0; j < cells[i].FourCutMatImg.Count; j++)
                    // 原：    {
                    // 原：        newCell.FourCutMatImg.Add(cells[i].FourCutMatImg[j]);
                    // 原：    }
                    // 原：}
                    // 原：if (cells[i].DownMassMatImg != null)
                    // 原：{
                    // 原：    newCell.DownMassMatImg = cells[i].DownMassMatImg;
                    // 原：}
                    newCell.SaveBigImagesIndex.AddRange(cells[i].SaveBigImagesIndex);
                    newCell.SaveCutImagesIndex.AddRange(cells[i].SaveCutImagesIndex);
                }

                newCell.SaveBigImagesIndex = newCell.SaveBigImagesIndex.Distinct().ToList(); //去掉重复项
                newCell.SaveCutImagesIndex = newCell.SaveCutImagesIndex.Distinct().ToList();
                List<CImage> img = GetCImage(cells);
                if (img?.Count > 0)
                {
                    if (COpenProjectLine.IsZipper)
                    {
                        ApplyZipperMergedDisplay(newCell, img);
                    }
                    else
                    {
                        ApplyGearMergedDisplay(newCell, img);
                    }
                }
                return newCell;
            }
        }
        //  Mat matresult;
        private List<CImage> GetCImage(List<Cell> cells)
        {
            List<CImage> cImages = new List<CImage>();
            if (cells.Count <= 1)
            {
                if (cells.Count == 1)
                {
                    cImages.Add((CImage)cells[0].Image.Clone());
                    return cImages;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                //【盘齿方案4-注释】原因：拉链专属字段/高低曝光合并，盘齿改用 GearImages+MergedPanorama；工位张号 1..N
                // 原：// 将图片按 PhotoIndex 分为两组：PhotoIndex < 100 为一组，PhotoIndex >= 100 为一组（主体图片）
                // 原：List<Cell> lowIndexGroup = cells.Where(c => c.PhotoIndex >= 100).ToList();
                // 原：List<Cell> highIndexGroup = cells.Where(c => c.PhotoIndex < 100).ToList();
                // 原：lowIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
                // 原：highIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
                // 原：if (lowIndexGroup.Count > 0)
                // 原：{
                // 原：    CImage lowimage = GetMergeImage(lowIndexGroup);
                // 原：    cImages.Add(lowimage);
                // 原：}
                // 原：if (highIndexGroup.Count > 0)
                // 原：{
                // 原：    CImage heightimage = GetMergeImage(highIndexGroup);
                // 原：    cImages.Add(heightimage);
                // 原：}
                // 原：return cImages;
                if (COpenProjectLine.IsZipper)
                {
                    return CollectZipperCImages(cells);
                }
                return CollectGearCImages(cells);


            }
        }

        private CImage GetMergeImage(List<Cell> cells)
        {
            if (cells[0].Image == null) return null;
            if (cells.Count == 1)
            {
                return (CImage)cells[0].Image.Clone();
            }
            int height = cells[0].Image.ImageHeight;
            int width = cells[0].Image.ImageWidth;
            int[] widths = cells.Select(c => c.Image.ImageWidth).ToArray();
            IntPtr[] imageDatas = cells.Select(c => c.Image.ImageData).ToArray();
            unsafe
            {

                // 计算目标参数
                int totalWidth = cells.Sum(c => c.Image.ImageWidth);
                int bytesPerPixel = cells[0].Image.PixelFormat.BitsPerPixel / 8;
                int srcStride = GetImageStride(width, bytesPerPixel); // 假设所有图像行跨距相同

                // 分配目标内存（总字节数 = 总宽度 * 高度 * 每像素字节数）
                int dstSize = totalWidth * height * bytesPerPixel;
                IntPtr dstPtr = Marshal.AllocHGlobal(dstSize);
                try
                {

                    // 预计算目标行起始地址数组
                    IntPtr[] dstRowOffsets = new IntPtr[height];
                    for (int y = 0; y < height; y++)
                    {
                        dstRowOffsets[y] = dstPtr + y * totalWidth * bytesPerPixel;
                    }

                    // 逐行复制数据
                    for (int srcIdx = 0; srcIdx < widths.Length; srcIdx++)
                    {
                        int srcX = widths.Take(srcIdx).Sum(); // 当前源图像在目标中的起始X坐标
                        byte* srcBuffer = (byte*)imageDatas[srcIdx].ToPointer();
                        byte* dstBuffer = (byte*)dstPtr.ToPointer() + srcX * bytesPerPixel;

                        // 复制所有行
                        for (int y = 0; y < height; y++)
                        {
                            Buffer.MemoryCopy(
                                srcBuffer + y * srcStride,
                                dstBuffer + y * totalWidth * bytesPerPixel,
                                widths[srcIdx] * bytesPerPixel,
                                widths[srcIdx] * bytesPerPixel);
                        }
                    }

                    CImage image = new CImage(totalWidth, height, dstPtr, cells[0].Image.PixelFormat);
                    return image;
                }
                catch
                {
                    Marshal.FreeHGlobal(dstPtr);
                    throw;
                }
            }
        }

        /// <summary>
        /// 计算单行数据的实际跨度（含对齐填充）
        /// </summary>
        private int GetImageStride(int width, int format)
        {
            int rawStride = width * format;
            int alignment = 4; // 假设系统按4字节对齐
            return ((rawStride + alignment - 1) / alignment) * alignment;
        }


        public void StopTask()
        {
            m_WaitImgChannel.Writer.Complete();
            m_InfoChannel.Writer.Complete();
            m_AlgorithmChannel.Writer.Complete();
            m_FilterChannel.Writer.Complete();
            m_ShowImageChannel.Writer.Complete();
            m_AlarmChannel.Writer.Complete();
            m_dataBaseChannel.Writer.Complete();
            m_SaveImageChannel.Writer.Complete();
        }

        #endregion 线程管理

        /// <summary>
        /// 2024.7.30 李焕彬
        /// 一开始设置为最差的产品
        /// </summary>
        /// <param name="cell"></param>
        private void SetBadCell(Cell cell)
        {
            cell.Quality = MaociQualityConfig.GetWorst();
            cell.IsOK = false;
            cell.Detection = new CellDetection() { Category = Category.值, };
            if (cell.FrameLoss)
            {
                cell.Detection.DefectFilter = MaociFilterConfig.GetDefectFilter(
                    "异常类",
                    "拍照异常",
                    "丢帧"
                );
            }
            else { }
        }


        /// <summary>
        /// 2024.9.2 李焕彬
        /// 更新相机序列号
        /// </summary>
        public void UpdateCam(string cameraSerial)
        {
            if (
                !string.IsNullOrEmpty(cameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(cameraSerial)
            )
            {
                CCameraManagement.CameraDict[cameraSerial].OutputImageChannel = null;
            }
            this.CameraSerial = cameraSerial;
            if (AppConfig.HasFocusConfig())
            {
                FocusCtrlVM.SetCameraSerial(CameraSerial);
            }
            if (
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CamParamDict[CameraSerial].ProjGuid = GUID;
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel =
                    this.m_WaitImgChannel;
                //CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
                //    MaociAlgorParamConfig.GetDistinctFunc();
            }
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 更新算法
        /// </summary>
        public void UpdateAlgorithm(string algorithm)
        {
            //修改之前注销算法、过滤分选消息
            WeakReferenceMessenger.Default.UnregisterAll(MaociFilterConfig);
            WeakReferenceMessenger.Default.UnregisterAll(MaociAlgorParamConfig);

            this.Algorithm = algorithm;
            this.MaociAlgorParamConfig = CAlgorithmManagement
                .AlgorithmHeper[Algorithm]
                .CreateNewAlgorithm(this.Name);
            this.MaociFilterConfig = new CFilterConfig(this.MaociAlgorParamConfig.DefectSpecies);
            this.MaociAlgorVM.Config = MaociAlgorParamConfig;
            this.SDFilterVM.FilterConfig = MaociFilterConfig;
            this.SDFilterVM.DefectFeactures = MaociAlgorParamConfig.DefectFeatures;
            this.MaociFilterConfig.SetSDFilterVM(MaociQualityConfig);
            this.AlarmSetVM.SetFilter(new() { MaociFilterConfig });
            MaociDefectsProduce.SetFilter(new() { MaociFilterConfig });
            this.MaociHistoryModel.SetHistory(MaociFilterConfig);
            //if (FocusCtrlVM is not null)
            //    this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();
            //if (
            //    !string.IsNullOrEmpty(CameraSerial)
            //    && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            //)
            //{
            //    CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
            //        MaociAlgorParamConfig.GetDistinctFunc();
            //}

            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociFilterConfig,
                MaociFilterConfig.token
            );
            //修改之后注册算法、过滤分选消息
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociAlgorParamConfig,
                MaociAlgorParamConfig.token
            );
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 更新对焦
        /// </summary>
        public void UpdateFocus(string focus)
        {
            if (!AppConfig.HasFocusConfig())
                return;
            //修改之前注销自动对焦消息
            WeakReferenceMessenger.Default.UnregisterAll(FocusConfig);

            this.Focus = focus;
            this.FocusConfig = CFocusManagement.FocusHeper[Focus].CreateNewfocus();
            this.FocusCtrlVM = this.FocusConfig.CreateCtrlVM();
            this.FocusCtrlVM.SetCameraSerial(CameraSerial);
            //this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();

            //修改之后注册自动对焦消息
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                FocusConfig,
                FocusConfig.token
            );
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 新建制程时更新Token
        /// </summary>
        public void UpdateToken()
        {
            MaociAlgorParamConfig.token.ProGuid = GUID;
            MaociFilterConfig.token.ProGuid = GUID;
            MaociAlarmSetConfig.token.ProGuid = GUID;

            ConfigModifyObservableBase.UpdateToken(MaociAlarmSetConfig, MaociAlarmSetConfig.token);
            ConfigModifyObservableBase.UpdateToken(MaociFilterConfig, MaociFilterConfig.token);
            ConfigModifyObservableBase.UpdateToken(
                MaociAlgorParamConfig,
                MaociAlgorParamConfig.token
            );
            if (AppConfig.HasMarkConfig())
            {
                MarkConfig.token.ProGuid = GUID;
                ConfigModifyObservableBase.UpdateToken(MarkConfig, MarkConfig.token);
            }
            if (AppConfig.HasFocusConfig())
            {
                FocusConfig.token.ProGuid = GUID;
                ConfigModifyObservableBase.UpdateToken(FocusConfig, FocusConfig.token);
            }
        }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 新建制程、修改名称时更新name
        ///</summary>
        public void UpdateName()
        {
            MaociAlgorParamConfig.PrcessName = Name;
            MaociFilterConfig.PrcessName = Name;
            MaociAlarmSetConfig.PrcessName = Name;
            if (MarkConfig is not null)
                MarkConfig.PrcessName = Name;
            if (FocusConfig is not null)
                FocusConfig.PrcessName = Name;
        }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 更新VM权限
        ///</summary>
        public void UpdateVMLoginPerson(CLoginPerson loginPerson)
        {
            this.QualityVM.LoginPerson = loginPerson;
            this.MaociAlgorVM.LoginPerson = loginPerson;
            this.SDFilterVM.LoginPerson = loginPerson;
            this.AlarmSetVM.LoginPerson = loginPerson;
            if (MarkConfig is not null)
                this.MarkCtrlVM.LoginPerson = loginPerson;
            if (FocusCtrlVM is not null)
                this.FocusCtrlVM.LoginPerson = loginPerson;
            this.HistoryVM.LoginPerson = loginPerson;
        }


        private void SaveOtherOldImage(Cell cell)
        {
            if (cell == null) return;
            try
            {
                string na = DateTime.Now.ToString("D") + $"\\{this.Name}";
                string savepathDir = "D:\\DiscardedImages\\" + na;
                if (!Directory.Exists(savepathDir))
                {
                    Directory.CreateDirectory(savepathDir);
                }
                string okng = cell.IsOK ? "OK" : "NG";
                string imagename = $"{cell.ID}_{cell.PhotoIndex}_{okng}_{cell.PhotoTatolCount}_{cell.CreateTime.ToString("HHmmssfff")}";
                string filepath = $"{savepathDir}\\{imagename}.png";
                WriteImage(cell?.Image.ToBitmapSource(), filepath, "png");
                cell?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 2024.7.29 李焕彬
        /// 保存图片
        /// </summary>
        /// <param name="bitImage">图片</param>
        /// <param name="filepath">存图路径</param>
        /// <param name="format">图片格式</param>
        private void WriteImage(BitmapSource bitmap, string filepath, string format)
        {
            if (bitmap != null)
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
        }
    }

    public struct PrintMsg
    {
        public string message;
        public LOG logType;

        public PrintMsg(string msg, LOG type)
        {
            message = msg;
            logType = type;
        }
    }
}
