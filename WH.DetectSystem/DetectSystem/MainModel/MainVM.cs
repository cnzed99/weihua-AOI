using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AlarmSetCtrl;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FocusControl;
using HandyControl.Controls;
using HandyControl.Data;
using HistoryPlayback;
using HistoryPlayback.Model;
using Mapster;
using MarkControl;
using Motion;
using MySqlOperatesApi;
using Mysqlx.Crud;
using Newtonsoft.Json;
using ProjProduceData;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.DetectSystem._4_报警处理;
using WH.DetectSystem._5_存图操作;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;
using static Mysqlx.Crud.Order.Types;

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
                MaociAlarmSetConfig.SetCAlarm(MaociFilterConfig, MaociQualityConfig);
                MaociDefectsProduce.SetQuality(MaociQualityConfig);
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
            this.SDFilterVM.FilterConfig = MaociFilterConfig;
            this.SDFilterVM.DefectFeactures = MaociAlgorParamConfig.DefectFeatures;
            this.MaociAlgorVM.Config = MaociAlgorParamConfig;

            this.DefectsDataVM.DefectsProduce = MaociDefectsProduce;
            this.AlarmSetVM.CAlarmSet = MaociAlarmSetConfig;

            this.HistoryVM.HistoryModel = MaociHistoryModel;
            MaociHistoryModel.SetHistory(MaociFilterConfig);
            //MaociMysqlConfig.SetSQL(MaociFilterConfig);
            if (AppConfig.HasFocusConfig())
            {
                FocusCtrlVM = FocusConfig.CreateCtrlVM();
                FocusCtrlVM.SetCameraSerial(CameraSerial);
                this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();
                FocusCtrlVM.InitControl();
            }
            if (AppConfig.HasMarkConfig())
            {
                MarkCtrlVM = new CMarkCtrlVM();
                MarkCtrlVM.MarkConfig = MarkConfig;
                MarkCtrlVM.Connect();
            }
            if (AppConfig.HasMotionConfig())
            {
                MotionCtrlVM = MotionConfig.CreateCtrlVM();
                MotionCtrlVM.InitControl();
            }

            AlarmSetVM.Reset();
            HistoryVM.Reset();
            QualityVM.Reset();
            // 数据清零事件
            SystemSettings.ClearProduceEvent += () =>
            {
                if (SystemSettings.AutoClearEnable)
                {
                    this.DefectsDataVM.DefectsProduce.Clear();
                }
            };

            this.ProcessGroup = processGroup;
            if (
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CamParamDict[CameraSerial].ProjGuid = GUID;
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel =
                    this.m_WaitImgChannel;
                CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
                    MaociAlgorParamConfig.GetDistinctFunc();
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
            if (MotionConfig is not null)
                WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                    MotionConfig,
                    MotionConfig.token
                );
            InitTask();
            UpdateVMLoginPerson(CLoginViewModel.SloinPerson);
        }

        [ObservableProperty]
        BitmapSource modelImage; //= new BitmapImage(new Uri("D://铝极.png"));

        /// <summary>
        /// 2024.7.25 李焕彬
        /// 当前图像窗口操作对象
        /// </summary>
        [ObservableProperty]
        ImageView curView;

        [ObservableProperty]
        Brush modelBrush = Brushes.White;

        [ObservableProperty]
        BitmapSource lastImage; //= new BitmapImage(new Uri("D://铝极.png"));

        /// <summary>
        /// 2024.7.25 李焕彬
        /// 上一张图像窗口操作对象
        /// </summary>
        [ObservableProperty]
        ImageView lastView;

        [ObservableProperty]
        Brush lastBrush = Brushes.White;

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
            string motion,
            string cameraSerial,
            CProcessGroupModel processGroup
        )
        {
            GUID = Guid.NewGuid().ToString();
            this.token = new Token(GUID, this.GetType().Namespace);
            this.Name = name;
            this.Algorithm = algorithm;
            this.Focus = focus;
            this.Motion = motion;
            this.CameraSerial = cameraSerial;
            this.MaociAlgorParamConfig = CAlgorithmManagement
                .AlgorithmHeper[Algorithm]
                .CreateNewAlgorithm();
            this.MaociFilterConfig = new CFilterConfig(this.MaociAlgorParamConfig.DefectSpecies);
            if (AppConfig.HasFocusConfig())
                this.FocusConfig = CFocusManagement.FocusHeper[Focus].CreateNewfocus();
            if (AppConfig.HasMarkConfig())
            {
                MarkConfig = new CMarkConfig();
            }
            if (AppConfig.HasMotionConfig())
            {
                MotionConfig = CMotionManagement.MotionHeper[Motion].CreateNewMotion();
            }
            this.UpdateToken(); //更新Token要在Init前
            this.UpdateName();
            Init(processGroup);
        }

        #region 时间相关

        [ObservableProperty]
        double algorithmTime = 0;

        [ObservableProperty]
        double filterTime = 0;
        #endregion

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
        CMarkCtrlVM markCtrlVM;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 对焦控制VM
        /// </summary>
        [ObservableProperty]
        CFocusCtrlVMBase focusCtrlVM;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制VM
        /// </summary>
        [ObservableProperty]
        CMotionVMBase motionCtrlVM;

        #region 启停 状态

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 启动时用于保存当前账户信息 停止运行时用于恢复权限
        /// </summary>
        CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

        bool isStart = false;

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 是否启动
        /// </summary>
        public bool IsStart
        {
            get => isStart;
            set
            {
                if (this.IsManualTest)
                {
                    Growl.Warning("请退出设置或离线手动模式！");
                    return;
                }
                SetProperty(ref isStart, value);
                if (value)
                {
                    UpdateVMLoginPerson(loginPerson);
                }
                else
                {
                    UpdateVMLoginPerson(CLoginViewModel.SloinPerson);
                }
                FocusCtrlVM?.SetRunning(IsStart);
                MarkCtrlVM?.SetRunning(IsStart);
                MotionCtrlVM?.SetRunning(IsStart);
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
                MotionCtrlVM?.SetRunning(isManualTest);
            }
        }

        [ObservableProperty]
        bool deviceSeting = false;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 是否对焦中
        /// </summary>
        public bool IsFocusing
        {
            get { return FocusCtrlVM?.IsFocusing ?? false; }
        }
        #endregion

        #region 线程管理
        CancellationTokenSource m_cts = new CancellationTokenSource();

        public static readonly BoundedChannelOptions s_NormalChannelOptions =
            new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait };
        public static readonly BoundedChannelOptions s_SaveImgchannelOptions =
            new BoundedChannelOptions(5) { FullMode = BoundedChannelFullMode.Wait };
        public static readonly BoundedChannelOptions s_SinglechannelOptions =
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.Wait };

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
            s_SaveImgchannelOptions
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
            s_SinglechannelOptions
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
            s_SinglechannelOptions
        );

        public AutoResetEvent WaitSignal = new AutoResetEvent(false);

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
            #endregion

            #region 取图线程
            Task waitGetImageTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

                await foreach (Cell cell in m_WaitImgChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        cell.ProjName = Name;
                        cell.ProjGuid = GUID;
                        cell.EncoderPos = MarkCtrlVM?.GetEncoderCount() ?? 0;
                        cell.ID = (MaociDefectsProduce.Total + 1).ToString();
                        if (IsStart || IsManualTest)
                        {
                            if (!m_AlgorithmChannel.Writer.TryWrite(cell))
                            {
                                cell.Dispose();
                            }
                        }
                        else
                        {
                            BitmapSource bitmapSource = cell.Image.ToBitmapSource();
                            _ = CMainModelsModelVM.Dispatcher?.BeginInvoke(
                                new Action(() =>
                                {
                                    ModelImage = bitmapSource;
                                })
                            );
                            if (FocusCtrlVM?.IsFocusing ?? false)
                            {
                                if (!FocusCtrlVM.FocusWaitGetImageChannel.Writer.TryWrite(cell))
                                    cell.Dispose();
                            }
                            else
                            {
                                cell.Dispose();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        await m_InfoChannel.Writer.WriteAsync(new PrintMsg("取图出错！", LOG.LOG_ERROR));
                    }
                }
            });
            #endregion

            #region PC算法执行线程
            Task waitRecipeTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                await foreach (Cell cell in m_AlgorithmChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        //StringBuilder strbuilder = new StringBuilder("[");
                        //strbuilder.Append("算法");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   配方开始执行。");
                        //await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
                        cell.Stopwatch.Restart();
                        MaociAlgorParamConfig.MaociExcute(cell);
                        cell.RecipeTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                        cell.Stopwatch.Restart();
                        //strbuilder = new StringBuilder("[");
                        //strbuilder.Append("算法");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   配方执行完成,耗时:");
                        //strbuilder.Append(cell.RecipeTime.TotalMilliseconds.ToString("F2"));
                        AlgorithmTime = cell.RecipeTime.TotalMilliseconds;
                        //await m_InfoChannel.Writer.WriteAsync(
                        //    new PrintMsg(strbuilder.ToString(), LOG.LOG_INFO)
                        //);

                        //if (!m_FilterChannel.Writer.TryWrite(cell))
                        //{
                        //    cell.Dispose();

                        //}
                        try
                        {
                            cell.Quality = MaociQualityConfig.GetBest();
                            if (!cell.Skipthis)
                                MaociFilterConfig.FilterExute(cell);
                            else
                            {
                                SetBadCell(cell);
                            }
                            cell.FilterTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                            cell.Stopwatch.Stop();
                            cell.ProcessTime = DateTime.Now - cell.CreateTime;
                            StringBuilder strbuilder = new StringBuilder("[结束]     ");
                            strbuilder.Append(cell.ID);
                            strbuilder.Append("   检测结束,耗时:");
                            strbuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F2"));
                            if (cell.IsOK)
                            {
                                await m_InfoChannel.Writer.WriteAsync(
                                    new PrintMsg(strbuilder.ToString(), LOG.LOG_OK)
                                );
                            }
                            else if (MarkCtrlVM is not null)
                            {
                                int markPos = MarkCtrlVM.AddMark(cell.EncoderPos);
                                strbuilder.Append($",检测NG,增加打标位置{markPos}！");
                                await m_InfoChannel.Writer.WriteAsync(
                                    new PrintMsg(strbuilder.ToString(), LOG.LOG_NG)
                                );
                            }
                            FilterTime = cell.FilterTime.TotalMilliseconds;
                            if (!m_ShowImageChannel.Writer.TryWrite(cell))
                            {
                                cell.Dispose();
                                //strbuilder = new StringBuilder("[");
                                //strbuilder.Append("筛选线程");
                                //strbuilder.Append("]     ");
                                //strbuilder.Append(cell.ID);
                                //strbuilder.Append("   cell入显示队列失败。");
                                //await m_InfoChannel.Writer.WriteAsync(
                                //    new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                                //);
                            }
                            ModelBrush = cell.Quality.ShowColor.Brush;
                            if (!cell.IsOK)
                            {
                                LastBrush = ModelBrush;
                                LastImage = ModelImage;
                            }
                            MaociDefectsProduce.Excute(cell);
                            if (ProcessGroup.AddCellAndJudge(cell, out CCellPro cellOut))
                            {
                                object objAlarmLock = new object(); //报警监控用
                                ProcessGroup.MaociDefectsProduce.Excute(cellOut.Cell);
                                if (!m_dataBaseChannel.Writer.TryWrite(cellOut.Cell))
                                {
                                    //cell.Dispose();
                                }
                                lock (objAlarmLock)
                                {
                                    MaociAlarmSetConfig.Excute(cell);
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
            #endregion

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
            #endregion

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
                                BitmapSource bitmapSource = cell.Image.ToBitmapSource();
                                _ = CMainModelsModelVM.Dispatcher?.BeginInvoke(
                                    new Action(() =>
                                    {
                                        ModelImage = bitmapSource;
                                    })
                                );
                                for (int i = 0; i < 1; i++)
                                {
                                    ImageView drawView;
                                    if (i == 0)
                                    {
                                        drawView = CurView;
                                    }
                                    else
                                    {
                                        if (cell.IsOK)
                                            break;
                                        drawView = LastView;
                                    }
                                    await drawView.Dispatcher?.BeginInvoke(() =>
                                    {
                                        drawView.Clear(false);
                                        foreach (var edge in cell.DrawEdges)
                                        {
                                            drawView.SetPen(edge.BrushDraw);
                                            drawView.ImgDrawPoints(edge.Points, false);
                                        }
                                        if (!cell.IsOK)
                                        {
                                            DefectFilter dstFilter = cell.Detection.DefectFilter;
                                            StringBuilder textBuilder = new StringBuilder();
                                            textBuilder.AppendLine(dstFilter.Name);
                                            textBuilder.Append(cell.Quality.Name);
                                            drawView.SetFontBrush(cell.Quality.ShowColor.Brush);
                                            drawView.WinDrawText(
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
                                                    DefectFilter defectFilter =
                                                        detection.DefectFilter;
                                                    drawView.SetPen(defectFilter.ShowColor.Brush);
                                                    drawView.SetFontBrush(
                                                        defectFilter.ShowColor.Brush
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
                                                        drawView.ImgDrawText(
                                                            detection.DetectLog[i].ToString(),
                                                            detection.regionOut[i].GetCenter(),
                                                            false
                                                        );
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
                                                    drawView.SetPen(defectFilter.ShowColor.Brush);
                                                    drawView.SetFontBrush(
                                                        defectFilter.ShowColor.Brush
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
                                                        drawView.ImgDrawText(
                                                            cell.Detection.DetectLog[i].ToString(),
                                                            cell.Detection.regionOut[i].GetCenter(),
                                                            false
                                                        );
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
                                            drawView.SetFontBrush(cell.Quality.ShowColor.Brush);
                                            drawView.WinDrawText(
                                                "OK",
                                                AlignmentX.Right,
                                                AlignmentY.Top,
                                                false
                                            );
                                        }

                                        //2025.01.09 易群生
                                        //在识别到的字符附近区域显示识别到的字符
                                        if (cell.OcrResultString != "")
                                        {
                                            drawView.SetFontSize(20);
                                            drawView.SetFontBrush(Brushes.Red);
                                            drawView.ImgDrawText(cell.OcrResultString,
                                                10, Math.Max(cell.DrawEdges[0].Points[0].Y - 100,10), false);

                                        }

                                        drawView.Invalidate();
                                    });
                                }
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
                            #endregion
                        }
                        if (
                            (SystemSettings.OfflineSave || isStart)
                            && (
                                SaveImageVM.Param.SaveImageEnable
                                || SaveImageVM.Param.PiantScreenEnable
                            )
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
            #endregion

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
            #endregion

            #region 数据库线程
            //数据库写入容易出错，卡顿时间较长，容量最大10个
            Task dataBaseTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                //object objAlarmLock = new object(); //报警监控用
                await foreach (Cell cell in m_dataBaseChannel.Reader.ReadAllAsync())
                {
                    #region 写入Access数据库
                    //try
                    //{
                    //    var space = DiskSpace.GetHardDiskFreeSpace("D");
                    //    if (space > 1)
                    //    {
                    //        if ((!SystemStatic._isRuning && CSystemParamJson.SystemSetParam.OfflineSave) || SystemStatic._isRuning) //如果是离线检测状态 并且开启了离线存图和数据按钮  或者是正常运行状态
                    //        {
                    //            SQLClientAccess.AddData(cell, CSystemParamJson.SystemSetParam.NowShift);//数据库写入
                    //        }

                    //    }//空间不足1GB不存
                    //}
                    //catch (Exception ex)
                    //{
                    //    s_SysLog.Error("Access数据库写入错误:" + ex.Message + ex.StackTrace);
                    //}
                    #endregion

                    if (MySqlVM.MysqlExecute.SqlEnable)
                    {
                        try
                        {
                            if (SystemSettings.OfflineSave || IsStart)
                                ProcessGroup.MysqlBLL.AddData(cell, SystemSettings.NowShift);
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
                    }

                    #region 报警
                    try
                    {
                        //lock (objAlarmLock)
                        //{
                        //    MaociAlarmSetConfig.Excute(cell);
                        //}
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("监控报警出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                    }
                    finally
                    {
                        //取消单个制程数据库存储
                        //if (!m_dataBaseChannel.Writer.TryWrite(cell))
                        //    cell.Dispose();
                    }
                    #endregion

                    //cell.Dispose();
                }
            });
            #endregion

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
                                        if (HistoryVM.HistoryModel.NgImagePaths.Count > 1000)
                                            HistoryVM.HistoryModel.NgImagePaths.RemoveAt(1000);
                                        HistoryVM.HistoryModel.NgImagePaths.Insert(0, savePath);
                                    }
                                })
                            );
                        }
                        cell.Dispose(); //这个cell是复制的clone 存图后清理
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("存图线程出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                    }
                }
            });
            #endregion
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
        #endregion

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
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel = null;
            }
            this.CameraSerial = cameraSerial;
            FocusCtrlVM.SetCameraSerial(CameraSerial);
            if (
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CamParamDict[CameraSerial].ProjGuid = GUID;
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel =
                    this.m_WaitImgChannel;
                CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
                    MaociAlgorParamConfig.GetDistinctFunc();
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
                .CreateNewAlgorithm();
            this.MaociFilterConfig = new CFilterConfig(this.MaociAlgorParamConfig.DefectSpecies);
            this.MaociAlgorVM.Config = MaociAlgorParamConfig;
            this.SDFilterVM.FilterConfig = MaociFilterConfig;
            this.SDFilterVM.DefectFeactures = MaociAlgorParamConfig.DefectFeatures;
            this.MaociFilterConfig.SetSDFilterVM(MaociQualityConfig);
            this.MaociAlarmSetConfig.SetCAlarm(MaociFilterConfig, MaociQualityConfig);
            MaociDefectsProduce.SetFilter(new() { MaociFilterConfig });
            this.MaociHistoryModel.SetHistory(MaociFilterConfig);
            if (FocusCtrlVM is not null)
                this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();
            if (
                !string.IsNullOrEmpty(CameraSerial)
                && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
            )
            {
                CCameraManagement.CameraDict[CameraSerial].FuncDistinct =
                    MaociAlgorParamConfig.GetDistinctFunc();
            }

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
            this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();

            //修改之后注册自动对焦消息
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                FocusConfig,
                FocusConfig.token
            );
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 更新控制
        /// </summary>
        public void UpdateMotion(string motion)
        {
            if (!AppConfig.HasMotionConfig())
                return;
            //修改之前注销自动控制消息
            WeakReferenceMessenger.Default.UnregisterAll(MotionConfig);

            this.Motion = motion;
            this.MotionConfig = CMotionManagement.MotionHeper[Motion].CreateNewMotion();
            this.MotionCtrlVM = this.MotionConfig.CreateCtrlVM();

            //修改之后注册自动对焦消息
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MotionConfig,
                MotionConfig.token
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
            if (AppConfig.HasMotionConfig())
            {
                MotionConfig.token.ProGuid = GUID;
                ConfigModifyObservableBase.UpdateToken(MotionConfig, MotionConfig.token);
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
            if (MotionConfig is not null)
                MotionConfig.PrcessName = Name;
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
            if (MotionCtrlVM is not null)
                this.MotionCtrlVM.LoginPerson = loginPerson;
            this.HistoryVM.LoginPerson = loginPerson;
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
