using System.Collections.ObjectModel;
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
            CZipperAutomaticAlgorithm.Instance.TestFinshEven += TestFinshTodo;
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

        private void TestFinshTodo(bool finsh)
        {

            if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
            {
                foreach (var cell in MergeCells)
                {
                    SaveOtherOldImage(cell);
                }
                MergeCells.Clear();
                UpdatDetSet();
                ////UpdatWhiteZipperParam(); //白色拉链加严处理 20260424 鲍赞宝 弃用
                //if (this.Name == "正面")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "左相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("左相机");
                //            Updatepull("左相机");
                //            UpdatepullSegArea("左相机");
                //        }


                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "右相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("右相机");
                //            Updatepull("右相机");
                //            UpdatepullSegArea("右相机");
                //        }

                //    }
                //}
                //if (this.Name == "反面")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "右相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("右相机");
                //            Updatepull("右相机");
                //            UpdatepullSegArea("右相机");
                //        }

                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "左相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("左相机");
                //            Updatepull("左相机");
                //            UpdatepullSegArea("左相机");
                //        }

                //    }
                //}
                //if (this.Name == "正面内")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "正面外")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "反面内")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "反面外")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
            }
            if (!finsh)
            {
                IsAutomaticTest = false;
            }
        }

        //private void InfoChangeFunc()
        //{
        //    TestFinshTodo(true);
        //}

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
                        if (IsStart && !isAutomaticTest) //自动运行
                        {
                            IDisRight = true;
                            int productID = -1;
                            if (Name == "正面" || Name == "反面")
                            {
                                CZipperCommunicate.GetID(out productID);
                                m_WaitIDChannel.Reader.TryRead(out ZipperID zipperID);
                                if (zipperID.ProductID > 0)
                                {
                                    bool bnext = zipperID.ProductID < productID;
                                    while (bnext && zipperID.ProductID > 0)
                                    {
                                        m_WaitIDChannel.Reader.TryRead(out zipperID);
                                        bnext = zipperID.ProductID < productID;
                                        if (bnext)
                                        {
                                            SysLog.Info($"{Name}-变化的产品ID:{zipperID.ProductID}小于当前{productID}，抛弃{zipperID.ProductID}-{zipperID.PhotoID}");
                                            continue;
                                        }
                                    }
                                    cell.ID = zipperID.ProductID.ToString();
                                    cell.PhotoIndex = zipperID.PhotoID;
                                    SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},图片ID:{zipperID.PhotoID}");
                                }
                                else
                                {
                                    IDisRight = false;
                                    SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},抛弃");
                                    cell.Dispose();
                                }
                            }
                            else if (Name == "上止") //上止
                            {
                                IDisRight = true;
                                cell.ID = (ProcessGroup.MaociDefectsProduce.Total + 1).ToString();
                                cell.PhotoIndex = 1;
                                cell.PhotoTatolCount = 1;
                            }
                            else //拉头拉片
                            {
                                CZipperCommunicate.GetID2(out productID);
                                if (productID != -1)
                                {
                                    IDisRight = true;
                                    cell.ID = productID.ToString();
                                    cell.PhotoIndex = 1;
                                    cell.PhotoTatolCount = 1;
                                }

                            }

                        }
                        else
                        {
                            IDisRight = true;
                            if (cell.ImageFile == "") //手动调试
                            {
                                cell.ID = (ProcessGroup.MaociDefectsProduce.Total + 1).ToString();
                                cell.PhotoIndex = 1;
                                cell.PhotoTatolCount = 1;
                            }
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
                        if (IsStart && !isAutomaticTest) //自动运行
                        {
                            cell.ZipperPullerCX = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperPullerCX;
                            cell.ZipperPullerCY = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperPullerCY;
                            cell.PullOrgContours = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.OrgContours;
                            cell.PullHoldOrgContours = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.HoleOrgContours;
                            cell.PullsOrgHvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH; //拉片
                            cell.PullsOrgSvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS;
                            cell.PullsOrgVvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanV;
                            cell.PullerOrgHvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH; //拉头
                            cell.PullerOrgSvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS;
                            cell.PullerOrgVvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanV;
                            cell.ModelID_Pull = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ModelID_Pull;
                            cell.ModelID_Logo = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ModelID_Logo;
                            cell.PullModelRow = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullModelRow;
                            cell.PullModelCol = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullModelCol;
                            cell.BackRectangle = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.BackRectangle;
                            cell.PullSegOrgArea = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullSegOrgArea;
                            cell.UpMass_1_MeanH = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanH;
                            cell.UpMass_1_MeanS = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanS;
                            cell.UpMass_1_MeanV = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanV;
                            cell.UpMass_2_MeanH = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanH;
                            cell.UpMass_2_MeanS = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanS;
                            cell.UpMass_2_MeanV = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanV;
                            int photoTotalCount = 0;
                            if (Name != "正面" && Name != "反面")
                            {
                                photoTotalCount = 1;
                            }
                            else
                            {
                                photoTotalCount = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperImagesCount * 2;
                            }
                            cell.PhotoTatolCount = photoTotalCount;
                        }
                        cell.PullMaterlsType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType.ToString();
                        cell.DownStopMassType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperDownMassType.ToString();
                        cell.UpStopMassType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperUpMassType.ToString();
                        cell.BoltDiretion = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData?.BoltDiretion.ToString();
                        cell.ZipperLogoType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType;
                        cell.ProjName = Name;
                        cell.ProjGuid = GUID;
                        try
                        {
                            if (!isAutomaticTest)
                            {
                                //if (!CZipperAutomaticAlgorithm.AutoSettingPosFinsh && IsStart) //自动调整拉链位置
                                //{
                                //    // ZipperAutomaticAlgorithm.AutoSettingTriggerPos(cell);
                                //    CZipperAutomaticAlgorithm.Instance.AutoSettingTriggerPos(cell);
                                //}
                                MaociAlgorParamConfig.MaociExcute(cell);
                            }
                            else
                            {
                                // ZipperAutomaticAlgorithm.ZipperAutomaticAlgorithmRun(cell);
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
                                        for (int i = 0; i < currentCells.Count; i++)
                                        {
                                            newCell.ZipperImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
                                                currentCells[i].CreateTime, currentCells[i].RecipeTime));
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
                                        if (CellOut.Cell.IsOK && CellOut.Cell.ID != "0")
                                        {
                                            if (Name == "正面" || Name == "反面")
                                            {
                                                CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.OK);
                                            }
                                            else if (Name == "上止")
                                            {
                                                CZipperCommunicate.SendResult3(CellOut.Cell.ID, ZIPPERESULT.OK);
                                            }
                                            else
                                            {
                                                CZipperCommunicate.SendResult2(CellOut.Cell.ID, ZIPPERESULT.OK);
                                            }


                                        }
                                        else
                                        {
                                            if (Name == "正面" || Name == "反面")
                                            {
                                                if (CellOut.Cell.Detection.DefectFilter.Name.Contains("大接头") || CellOut.Cell.Detection.DefectFilter.Name.Contains("大破损")
                                                 || CellOut.Cell.Detection.DefectFilter.Name.Contains("大起毛"))
                                                {
                                                    CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.NG); 
                                                }
                                                else
                                                {
                                                    CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.NG2);
                                                }
                                            }
                                            else if (Name == "上止")
                                            {
                                                CZipperCommunicate.SendResult3(CellOut.Cell.ID, ZIPPERESULT.NG);

                                            }
                                            else
                                            {
                                                CZipperCommunicate.SendResult2(CellOut.Cell.ID, ZIPPERESULT.NG);
                                            }

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
                    if (cells[i].ZipperPullPartImg != null)
                    {
                        newCell.ZipperPullPartImg = cells[i].ZipperPullPartImg;
                    }
                    if (cells[i].UpMassMatImg != null && cells[i].UpMassMatImg.Count > 0)
                    {
                        for (int j = 0; j < cells[i].UpMassMatImg.Count; j++)
                        {
                            newCell.UpMassMatImg.Add(cells[i].UpMassMatImg[j]);
                        }
                    }

                    if (cells[i].FourCutMatImg != null && cells[i].FourCutMatImg.Count > 0)
                    {
                        for (int j = 0; j < cells[i].FourCutMatImg.Count; j++)
                        {
                            newCell.FourCutMatImg.Add(cells[i].FourCutMatImg[j]);
                        }
                    }
                    if (cells[i].DownMassMatImg != null)
                    {
                        newCell.DownMassMatImg = cells[i].DownMassMatImg;
                    }
                    newCell.SaveBigImagesIndex.AddRange(cells[i].SaveBigImagesIndex);
                    newCell.SaveCutImagesIndex.AddRange(cells[i].SaveCutImagesIndex);
                }

                newCell.SaveBigImagesIndex = newCell.SaveBigImagesIndex.Distinct().ToList(); //去掉重复项
                newCell.SaveCutImagesIndex = newCell.SaveCutImagesIndex.Distinct().ToList();
                List<CImage> img = GetCImage(cells);
                if (img?.Count > 0)
                {
                    if (img.Count == 1)
                    {
                        newCell.Image = img[0];
                    }
                    else
                    {
                        newCell.Image = img[0];
                        newCell.ChangleImgae = img[1];
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
                // 将图片按 PhotoIndex 分为两组：PhotoIndex < 100 为一组，PhotoIndex >= 100 为一组（主体图片）
                List<Cell> lowIndexGroup = cells.Where(c => c.PhotoIndex >= 100).ToList();
                List<Cell> highIndexGroup = cells.Where(c => c.PhotoIndex < 100).ToList();
                lowIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
                highIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
                if (lowIndexGroup.Count > 0)
                {
                    CImage lowimage = GetMergeImage(lowIndexGroup);
                    cImages.Add(lowimage);
                }
                if (highIndexGroup.Count > 0)
                {
                    CImage heightimage = GetMergeImage(highIndexGroup);
                    cImages.Add(heightimage);
                }
                return cImages;


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


        private void UpdatWhiteZipperParam()
        {
            #region 布带脏污
            RecipeDefect dirtyDetNames = this.MaociFilterConfig["拉链"]["布带脏污"];
            if (dirtyDetNames != null)
            {
                foreach (var df in dirtyDetNames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "分数" || pa.Character.EnName == "Score")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 30;
                                    }
                                    else
                                    {
                                        pa.Min = 35;
                                    }

                                }
                                if (pa.Character.ZhName == "面积" || pa.Character.EnName == "Area")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 180;
                                    }
                                    else
                                    {
                                        pa.Min = 280;
                                    }

                                }
                            }
                        }
                    }

                }
            }

            #endregion
            #region 点脏污
            RecipeDefect pointDetNames = this.MaociFilterConfig["拉链"]["点脏污"];
            if (pointDetNames != null)
            {
                foreach (var df in pointDetNames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "数量" || pa.Character.EnName == "Count")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 2;
                                    }
                                    else
                                    {
                                        pa.Min = 3;
                                    }
                                }
                                if (pa.Character.ZhName == "分数" || pa.Character.EnName == "Score")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 30;
                                    }
                                    else
                                    {
                                        pa.Min = 35;
                                    }

                                }
                                if (pa.Character.ZhName == "面积" || pa.Character.EnName == "Area")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 130;
                                    }
                                    else
                                    {
                                        pa.Min = 280;
                                    }

                                }
                            }
                        }
                    }

                }
            }

            #endregion
        }
        /// <summary>
        /// 更新缺陷配置
        /// </summary>
        /// <param name="leftorright"></param>
        private void UpdatDetSet()
        {
            if (this.Name == "正面" || this.Name == "反面")
            {
                RecipeDefect zipperDetNames = this.MaociFilterConfig["方块插销"]["SAB"];
                if (zipperDetNames != null)
                {
                    foreach (var df in zipperDetNames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.LockHaveSAB == HAVESAB.有SAB)
                            {
                                fl.FilterSelectEnable = true;
                            }
                            else
                            {
                                fl.FilterSelectEnable = false;
                            }

                        }
                    }
                }
            }

            if (this.Name == "拉头")
            {
                RecipeDefect zipperDetNames = this.MaociFilterConfig["拉头拉片"]["SAB"];
                if (zipperDetNames != null)
                {
                    foreach (var df in zipperDetNames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerHaveSAB == HAVESAB.有SAB)
                            {
                                // fl.FilterSelectEnable = true;
                                fl.IsReversal = true;
                            }
                            else
                            {
                                // fl.FilterSelectEnable = false;
                                fl.IsReversal = true;
                            }

                        }
                    }
                }
            }

            if (this.Name == "拉片")
            {
                SpeciesFilter pullsdetName = this.MaociFilterConfig["拉头拉片"];
                foreach (var detname in pullsdetName.RecipeDefects)
                {
                    if (detname.Name != "拉片外形")
                    {
                        foreach (var df in detname.DefectFilters)
                        {
                            foreach (var fl in df.FilterList)
                            {

                                if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.PullsHaveFilm == PULLSHAVEFILM.有膜)
                                {
                                    fl.FilterSelectEnable = false;
                                }
                                else
                                {
                                    fl.FilterSelectEnable = true;
                                }

                                // fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                            }
                        }
                    }

                }

            }

            //foreach (var detname in zipperDetNames.RecipeDefects)
            //{
            //    if (detname.Name.Contains("正面上止") || detname.Name.Contains("反面上止"))
            //    {
            //        foreach (var df in detname.DefectFilters)
            //        {
            //            foreach (var fl in df.FilterList)
            //            {
            //                if ("无" == CZipperAutomaticAlgorithm.ZipperInfo.ZipperUpMassType.ToString())
            //                {
            //                    fl.FilterSelectEnable = false;
            //                }
            //                else
            //                {
            //                    fl.FilterSelectEnable = true;
            //                }

            //            }
            //        }
            //    }

            //    if (detname.Name.Contains("正面下止") || detname.Name.Contains("反面下止"))
            //    {
            //        foreach (var df in detname.DefectFilters)
            //        {
            //            foreach (var fl in df.FilterList)
            //            {
            //                if ("无" == CZipperAutomaticAlgorithm.ZipperInfo.ZipperDownMassType.ToString())
            //                {
            //                    fl.FilterSelectEnable = false;
            //                }
            //                else
            //                {
            //                    fl.FilterSelectEnable = true;
            //                }

            //            }
            //        }
            //    }

            //}
        }
        /// <summary>
        /// 更新拉头配置
        /// </summary>
        /// <param name="leftorright"></param>
        private void Updatepull(string leftorright)
        {
            if (leftorright == "左相机")
            {
                RecipeDefect pullnames = this.MaociFilterConfig["LOGO"]["拉头"];
                if (pullnames != null)
                {
                    foreach (var df in pullnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
                RecipeDefect pullernames = this.MaociFilterConfig["LOGO"]["拉片"];
                if (pullernames != null)
                {
                    foreach (var df in pullernames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true;
                        }
                    }
                }
            }
            else
            {
                RecipeDefect pullnames = this.MaociFilterConfig["LOGO"]["拉头"];
                if (pullnames != null)
                {
                    foreach (var df in pullnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true;
                        }
                    }
                }
                RecipeDefect pullernames = this.MaociFilterConfig["LOGO"]["拉片"];
                if (pullernames != null)
                {
                    foreach (var df in pullernames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
            }
        }
        private void UpdatepullSegArea(string leftorright)
        {
            if (leftorright == "左相机")
            {
                RecipeDefect pullsegnames = this.MaociFilterConfig["拉头拉片"]["拉片外形"];
                if (pullsegnames != null)
                {
                    foreach (var df in pullsegnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true; //
                        }
                    }
                }
                RecipeDefect pullHnames = this.MaociFilterConfig["拉头拉片"]["拉头色差1"];
                if (pullHnames != null)
                {
                    foreach (var df in pullHnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            foreach (var se in fl.SelectList)
                            {
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                    {
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                        {

                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH - 20; //烤漆拉片H
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH + 22;
                                        }
                                        else
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH - 22; //包胶拉片H
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH + 25;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                RecipeDefect pullSnames = this.MaociFilterConfig["拉头拉片"]["拉头色差2"];
                if (pullSnames != null)
                {
                    foreach (var df in pullSnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            foreach (var se in fl.SelectList)
                            {
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                    {
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS - 15; //烤漆拉片S
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS + 22;
                                        }
                                        else
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS - 23; //包胶拉片S
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS + 28;
                                        }

                                    }
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                RecipeDefect pullsegnames = this.MaociFilterConfig["拉头拉片"]["拉片外形"]; ;
                if (pullsegnames != null)
                {
                    foreach (var df in pullsegnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
                RecipeDefect pullHnames = this.MaociFilterConfig["拉头拉片"]["拉头色差1"];
                foreach (var df in pullHnames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH - 15; //烤漆拉头H
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH + 18;
                                    }
                                    else
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH - 20; //金属拉头H
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH + 25;
                                    }
                                }
                            }
                        }
                    }
                }
                RecipeDefect pullSnames = this.MaociFilterConfig["拉头拉片"]["拉头色差2"];
                foreach (var df in pullSnames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS - 12; //烤漆拉头S
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS + 15;
                                    }
                                    else
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS - 15; //金属拉头S
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS + 25;
                                    }
                                }
                            }
                        }
                    }

                }

            }


        }

        /// <summary>
        /// 更新Logo配置
        /// </summary>
        private void UpdateLogo(string leftorright)
        {
            if (leftorright == "左相机") //拍拉片
            {
                SpeciesFilter logonames = this.MaociFilterConfig["LOGO"];
                if ("无LOGO" == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            foreach (var df in logoname.DefectFilters)
                            {
                                foreach (var fl in df.FilterList)
                                {
                                    fl.FilterSelectEnable = true;
                                    fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            if (logoname.Name == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                            {
                                foreach (var df in logoname.DefectFilters)
                                {

                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.FindLogoSider == 3)
                                        {
                                            fl.IsReversal = false;//当有LOGO时，如果检测到LOGO 和正确的LOGO一致时，需要取反为OK
                                        }
                                        else
                                        {
                                            fl.IsReversal = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var df in logoname.DefectFilters)
                                {

                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        fl.IsReversal = false; //
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else  //拍拉头
            {
                SpeciesFilter logonames = this.MaociFilterConfig["LOGO"];
                if ("无LOGO" == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            foreach (var df in logoname.DefectFilters)
                            {
                                foreach (var fl in df.FilterList)
                                {
                                    fl.FilterSelectEnable = true;
                                    fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            if (logoname.Name == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                            {
                                foreach (var df in logoname.DefectFilters)
                                {
                                    foreach (var fl in df.FilterList)
                                    {

                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.FindLogoSider == 3)
                                        {
                                            fl.FilterSelectEnable = true;
                                            fl.IsReversal = true;//
                                        }
                                        else
                                        {
                                            fl.FilterSelectEnable = false;
                                            fl.IsReversal = false;//
                                        }

                                    }
                                }
                            }
                            else
                            {
                                foreach (var df in logoname.DefectFilters)
                                {
                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        fl.IsReversal = false; //当有LOGO时，如果检测到别的LOGO ，不能取反，需要检出
                                    }
                                }
                            }
                        }
                    }
                }
            }
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