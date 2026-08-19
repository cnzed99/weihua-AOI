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
using GearInfo;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;


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

            //【盘齿方案4-改动D】按制程名写入默认张数 N：内孔=6；轴顶侧面/轴底侧面=10；整轴侧面=14；其余（底部/齿顶/齿顶外圆等）=1
            this.PhotoTotalCount = name switch
            {
                "内孔" => 6,
                "轴顶侧面" => 10,
                "轴底侧面" => 10,
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

        /// <summary>
        /// 【盘齿方案4】改动A：本制程上次绑定的 ProductID（取图线程维护，禁止静态全局）
        /// 【盘齿方案4-注释】string；空/null 表示尚未绑定。占位 ID 与拉链一样来自产量+1，字符串比较。
        /// </summary>
        private string _lastBoundProductId;

        /// <summary>
        /// 【盘齿方案4】改动A：本制程当前 ID 已收张数（本地 PhotoIndex 1..N）
        /// </summary>
        private int _photoCounter;

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
        /// 【盘齿方案4】改动B：齿顶第 k 张分流。k==1 返回 false 入本制程算法；k==2 转发齿顶外圆；k>=3 丢弃。
        /// 返回 true 表示本张已处理完，取图线程应 continue，禁止写入本制程 m_AlgorithmChannel。
        /// CloneExecptImg 不拷贝 Image（方法内 WriteTo 已注释），此处移交 Image 所有权后再 Dispose 原 cell。
        /// </summary>
        private bool TryDispatchToothTopByIndex(Cell cell, int k)
        {
            if (k <= 1)
            {
                return false;
            }
            if (k >= 3)
            {
                SysLog.Warn($"{Name}-工位1多余张丢弃：产品ID:{cell.ID},PhotoIndex:{k}>=3");
                cell.Dispose();
                return true;
            }

            CMainModel outerVm = ProcessGroup?.CMainModels?.FirstOrDefault(m => m.Name == "齿顶外圆");
            if (outerVm == null)
            {
                SysLog.Error($"{Name}-工位1转发失败：同组未找到制程「齿顶外圆」，丢弃 ID:{cell.ID}");
                cell.Dispose();
                return true;
            }

            Cell fwd = cell.CloneExecptImg();
            fwd.Image = cell.Image;
            cell.Image = null;
            fwd.ID = cell.ID;
            fwd.PhotoIndex = 1;
            fwd.PhotoTatolCount = 1;
            fwd.ProjName = "齿顶外圆";
            fwd.ProjGuid = outerVm.GUID;
            fwd.IsPreBound = true;
            if (!outerVm.m_WaitImgChannel.Writer.TryWrite(fwd))
            {
                SysLog.Error($"{Name}-工位1转发失败：齿顶外圆通道写入失败，丢弃 ID:{fwd.ID}");
                fwd.Dispose();
            }
            else
            {
                SysLog.Info($"{Name}-工位1转发第2张到齿顶外圆：ID:{fwd.ID},PhotoIndex:{fwd.PhotoIndex},IsPreBound:{fwd.IsPreBound}");
            }
            cell.Dispose();
            return true;
        }

        /// <summary>
        /// 【盘齿方案2】P2-3 策略B：正式路径只读 GetProductID 缓存绑 ID。com==null 或 ID<=0 返回 false（调用方丢弃）。
        /// 禁止在取图线程 ReadHoldingRegister。成功则写 cell.ID/PhotoIndex/PhotoTatolCount 并维护换 ID 计数。
        /// </summary>
        private bool TryBindFormalProductId(Cell cell)
        {
            if (CGearCommunicate.com == null)
            {
                SysLog.Warn($"{Name}-无PLC，不绑ID，丢弃");
                return false;
            }
            int id = CGearCommunicate.GetProductID();
            if (id <= 0)
            {
                SysLog.Warn($"{Name}-产品ID无效:{id}，丢弃");
                return false;
            }
            string productID = id.ToString(CultureInfo.InvariantCulture);

            if (productID != _lastBoundProductId)
            {
                if (_photoCounter > 0 && _photoCounter < this.PhotoTotalCount)
                {
                    SysLog.Warn($"{Name}-残图告警：制程/{_lastBoundProductId}/已收{_photoCounter}/应收{this.PhotoTotalCount}");
                }
                _photoCounter = 0;
                _lastBoundProductId = productID;
            }

            _photoCounter++;
            cell.ID = productID;
            cell.PhotoIndex = _photoCounter;
            cell.PhotoTatolCount = this.PhotoTotalCount;
            return true;
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
                        //【盘齿方案4】改动B/G2：转发 Cell 已由齿顶绑好；必须在改动A 与离线 ImageFile=="" 覆盖之前拦截
                        if (cell.IsPreBound)
                        {
                            IDisRight = true;
                        }
                        else if (IsStart && !isAutomaticTest) //自动运行
                        {
                            IDisRight = true;
                            //【盘齿方案0-注释】原因：去掉取图线程按拉链制程名（正面/反面/上止/拉头拉片）绑PLC ID的三个分支，盘齿工位名语义不同，ID绑定由P2新协议重写
                            // 原： int productID = -1;
                            // 原： if (Name == "正面" || Name == "反面")
                            // 原： {
                                // 原： CZipperCommunicate.GetID(out productID);
                                // 原： m_WaitIDChannel.Reader.TryRead(out ZipperID zipperID);
                                // 原： if (zipperID.ProductID > 0)
                                // 原： {
                                    // 原： bool bnext = zipperID.ProductID < productID;
                                    // 原： while (bnext && zipperID.ProductID > 0)
                                    // 原： {
                                        // 原： m_WaitIDChannel.Reader.TryRead(out zipperID);
                                        // 原： bnext = zipperID.ProductID < productID;
                                        // 原： if (bnext)
                                        // 原： {
                                            // 原： SysLog.Info($"{Name}-变化的产品ID:{zipperID.ProductID}小于当前{productID}，抛弃{zipperID.ProductID}-{zipperID.PhotoID}");
                                            // 原： continue;
                                        // 原： }
                                    // 原： }
                                    // 原： cell.ID = zipperID.ProductID.ToString();
                                    // 原： cell.PhotoIndex = zipperID.PhotoID;
                                    // 原： SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},图片ID:{zipperID.PhotoID}");
                                // 原： }
                                // 原： else
                                // 原： {
                                    // 原： IDisRight = false;
                                    // 原： SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},抛弃");
                                    // 原： cell.Dispose();
                                // 原： }
                            // 原： }
                            // 原： else if (Name == "上止") //上止
                            // 原： {
                                // 原： IDisRight = true;
                                // 原： cell.ID = (ProcessGroup.MaociDefectsProduce.Total + 1).ToString();
                                // 原： cell.PhotoIndex = 1;
                                // 原： cell.PhotoTatolCount = 1;
                            // 原： }
                            // 原： else //拉头拉片
                            // 原： {
                                // 原： CZipperCommunicate.GetID2(out productID);
                                // 原： if (productID != -1)
                                // 原： {
                                    // 原： IDisRight = true;
                                    // 原： cell.ID = productID.ToString();
                                    // 原： cell.PhotoIndex = 1;
                                    // 原： cell.PhotoTatolCount = 1;
                                // 原： }

                            // 原： }

                            //【盘齿方案4】改动A：取图线程绑盘齿 ID / PhotoIndex 1..N（方案2 GetProductID 未接回，占位 Total+1）
                            // ⚠ G1：取图线程只更新 _lastBoundProductId/_photoCounter，不得直接操作 MergeCells
                            //【盘齿方案4-注释】占位 ID 与拉链一样来自产量+1；Total 仍是 double，只 ToString 不经 int。
                            //【盘齿方案4-注释】ToString("0", InvariantCulture) 避免 1 vs 1.0 导致换 ID 误判；字符串比较。
                            //【盘齿方案2-注释】 原： double nextId = ProcessGroup.MaociDefectsProduce.Total + 1;
                            //【盘齿方案2-注释】 原： if (nextId <= 0)
                            //【盘齿方案2-注释】 原： {
                                //【盘齿方案2-注释】 原： IDisRight = false;
                                //【盘齿方案2-注释】 原： SysLog.Warn($"{Name}-产品ID无效:{nextId}，丢弃");
                                //【盘齿方案2-注释】 原： cell.Dispose();
                                //【盘齿方案2-注释】 原： continue;
                            //【盘齿方案2-注释】 原： }
                            //【盘齿方案2-注释】 原： string productID = nextId.ToString("0", CultureInfo.InvariantCulture);

                            //【盘齿方案2-注释】 原： if (productID != _lastBoundProductId)
                            //【盘齿方案2-注释】 原： {
                                //【盘齿方案2-注释】 原： if (_photoCounter > 0 && _photoCounter < this.PhotoTotalCount)
                                //【盘齿方案2-注释】 原： {
                                    //【盘齿方案2-注释】 原： SysLog.Warn($"{Name}-残图告警：制程/{_lastBoundProductId}/已收{_photoCounter}/应收{this.PhotoTotalCount}");
                                //【盘齿方案2-注释】 原： }
                                //【盘齿方案2-注释】 原： _photoCounter = 0;
                                //【盘齿方案2-注释】 原： _lastBoundProductId = productID;
                            //【盘齿方案2-注释】 原： }

                            //【盘齿方案2-注释】 原： _photoCounter++;
                            //【盘齿方案2-注释】 原： cell.ID = productID;
                            //【盘齿方案2-注释】 原： cell.PhotoIndex = _photoCounter;
                            //【盘齿方案2-注释】 原： cell.PhotoTatolCount = this.PhotoTotalCount;
                            //【盘齿方案2】P2-3 策略B：正式路径只读 GetProductID 缓存；com==null 或 ID<=0 丢弃不绑
                            if (!TryBindFormalProductId(cell))
                            {
                                IDisRight = false;
                                cell.Dispose();
                                continue;
                            }

                            //【盘齿方案4】改动B：齿顶 N=1 的第2张转外圆、第3张起丢弃，禁止走本制程过张（否则第2张到不了转发）
                            if (_photoCounter > cell.PhotoTatolCount && Name != "齿顶")
                            {
                                IDisRight = false;
                                SysLog.Warn($"{Name}-过张丢弃：产品ID:{cell.ID},PhotoIndex:{_photoCounter}>PhotoTatolCount:{cell.PhotoTatolCount}");
                                cell.Dispose();
                                continue;
                            }
                            IDisRight = true;

                            //【盘齿方案4】改动B：齿顶分流（A 计数之后、入本制程 m_AlgorithmChannel 之前）
                            if (Name == "齿顶" && TryDispatchToothTopByIndex(cell, _photoCounter))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //【盘齿方案4】改动B：离线齿顶用文件名已写入的 PhotoIndex 当 k（无相机时 IsStart=false 走本分支）
                            if (Name == "齿顶")
                            {
                                if (TryDispatchToothTopByIndex(cell, cell.PhotoIndex))
                                {
                                    continue;
                                }
                                // k==1：本制程 N=1，避免同夹同时放 _1_/_2_ 时 PhotoTatolCount=2 误等第二张
                                cell.PhotoTatolCount = this.PhotoTotalCount;
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
                        if (IsStart && !isAutomaticTest) //自动运行
                        {
                            //【盘齿方案0-注释】原因：去掉拉链几何/模板/材质参数拷贝及拉链张数规则（PhotoTatolCount=ZipperImagesCount*2），盘齿参数由M8换料阶段重写
                            // 原： cell.ZipperPullerCX = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperPullerCX;
                            // 原： cell.ZipperPullerCY = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperPullerCY;
                            // 原： cell.PullOrgContours = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.OrgContours;
                            // 原： cell.PullHoldOrgContours = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.HoleOrgContours;
                            // 原： cell.PullsOrgHvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH; //拉片
                            // 原： cell.PullsOrgSvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS;
                            // 原： cell.PullsOrgVvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanV;
                            // 原： cell.PullerOrgHvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH; //拉头
                            // 原： cell.PullerOrgSvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS;
                            // 原： cell.PullerOrgVvalue = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanV;
                            // 原： cell.ModelID_Pull = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ModelID_Pull;
                            // 原： cell.ModelID_Logo = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ModelID_Logo;
                            // 原： cell.PullModelRow = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullModelRow;
                            // 原： cell.PullModelCol = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullModelCol;
                            // 原： cell.BackRectangle = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.BackRectangle;
                            // 原： cell.PullSegOrgArea = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullSegOrgArea;
                            // 原： cell.UpMass_1_MeanH = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanH;
                            // 原： cell.UpMass_1_MeanS = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanS;
                            // 原： cell.UpMass_1_MeanV = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_1_MeanV;
                            // 原： cell.UpMass_2_MeanH = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanH;
                            // 原： cell.UpMass_2_MeanS = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanS;
                            // 原： cell.UpMass_2_MeanV = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.UpMass_2_MeanV;
                            // 原： int photoTotalCount = 0;
                            // 原： if (Name != "正面" && Name != "反面")
                            // 原： {
                                // 原： photoTotalCount = 1;
                            // 原： }
                            // 原： else
                            // 原： {
                                // 原： photoTotalCount = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.ZipperImagesCount * 2;
                            // 原： }
                            // 原： cell.PhotoTatolCount = photoTotalCount;
                        }
                        //【盘齿方案0-注释】原因：去掉拉链几何/模板/材质参数拷贝及拉链张数规则（PhotoTatolCount=ZipperImagesCount*2），盘齿参数由M8换料阶段重写
                        // 原： cell.PullMaterlsType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType.ToString();
                        // 原： cell.DownStopMassType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperDownMassType.ToString();
                        // 原： cell.UpStopMassType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperUpMassType.ToString();
                        // 原： cell.BoltDiretion = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData?.BoltDiretion.ToString();
                        // 原： cell.ZipperLogoType = CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType;
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
                                        for (int i = 0; i < currentCells.Count; i++)
                                        {
                                            //【盘齿方案4-注释】原因：拉链专属字段/高低曝光合并，盘齿改用 GearImages+MergedPanorama；工位张号 1..N
                                            // 原：newCell.ZipperImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
                                            // 原：    currentCells[i].CreateTime, currentCells[i].RecipeTime));
                                            newCell.GearImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
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
                    newCell.Image = img[0];
                    newCell.MergedPanorama = img[0];
                    //【盘齿方案4-注释】原因：拉链专属字段/高低曝光合并，盘齿改用 GearImages+MergedPanorama；工位张号 1..N
                    // 原：if (img.Count == 1)
                    // 原：{
                    // 原：    newCell.Image = img[0];
                    // 原：}
                    // 原：else
                    // 原：{
                    // 原：    newCell.Image = img[0];
                    // 原：    newCell.ChangleImgae = img[1];
                    // 原：}
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
                List<Cell> ordered = cells.OrderBy(c => c.PhotoIndex).ToList();
                CImage merged = GetMergeImage(ordered);
                if (merged != null)
                {
                    cImages.Add(merged);
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
