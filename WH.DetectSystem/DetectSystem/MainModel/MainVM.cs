using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AlarmSetCtrl;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using HandyControl.Data;
using HistoryPlayback;
using HistoryPlayback.Model;
using Mapster;
using MapsterMapper;
using MarkControl;
using MotionControl;
using MySqlOperatesApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjProduceData;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.Models;
using WH.DetectSystem._4_报警处理;
using WH.DetectSystem._5_存图操作;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.DiskSpace;
using WH.Entity.LogRecord;
using WH.Entity.Messages;
using WH.RecipeCellRootBase;
using WH.RunCell;
using static Mysqlx.Crud.Order.Types;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 20240704 TCG
    /// 主界面视图模型
    /// </summary>
    public partial class CMainVM : CMainModel
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

        private CSystemSettingsVM SystemSettings =
            CPublicServices.Container.Resolve<CSystemSettingsVM>();

        /// <summary>
        /// 当前工程
        /// </summary>
        private CMainModel model;

        /// <summary>
        /// 当前工程 禁止直接修改其属性
        /// </summary>

        public CMainModel Model
        {
            get => model;
            set
            {
                SetProperty(ref model, value);
                model.Adapt(this);
                InitNewModel();

                TokeVM.ProGuid = value.GUID;
            }
        }

        /// <summary>
        /// 20240707 TCG
        /// 初始化当前制程，分配过滤、等级、算法配置对象，注册参数修改消息
        /// </summary>
        public void InitNewModel()
        {
            this.UpdateToken(); //先更新token 再同步引用
            MaociFilterConfig.SetSDFilterVM(MaociQualityConfig);
            MaociAlarmSetConfig.SetCAlarm(MaociFilterConfig, MaociQualityConfig);
            MaociDefectsProduce.SetDefectsProduce(MaociFilterConfig, MaociQualityConfig);
            MaociHistoryModel.SetHistory(MaociFilterConfig);

            MaociMysqlConfig.SetSQL(MaociFilterConfig);

            MotionCtrlVM.SetMotion(CameraSerial);

            AlarmSetVM.Reset();
            HistoryVM.Reset();
            QualityVM.Reset();
            #region 注册参数修改通道令牌
            WeakReferenceMessenger.Default.UnregisterAll(MaociFilterConfig);
            WeakReferenceMessenger.Default.UnregisterAll(MaociQualityConfig);
            WeakReferenceMessenger.Default.UnregisterAll(MaociAlgorParamConfig);
            WeakReferenceMessenger.Default.UnregisterAll(MaociAlarmSetConfig);
            WeakReferenceMessenger.Default.UnregisterAll(MaociSaveImageConfig);
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociFilterConfig,
                MaociFilterConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociQualityConfig,
                MaociQualityConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociAlgorParamConfig,
                MaociAlgorParamConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociAlarmSetConfig,
                MaociAlarmSetConfig.token
            );
            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                MaociSaveImageConfig,
                MaociSaveImageConfig.token
            );

            #endregion
            if (CameraSerial != null && CCameraManagement.CameraDict.ContainsKey(CameraSerial))
            {
                CCameraManagement.CameraDict[CameraSerial].OutputImageChannel = m_WaitImgChannel;
            }
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

        public CMainVM()
        {
            this.SDFilterVM.FilterConfig = MaociFilterConfig;
            this.SDFilterVM.QualityConfig = MaociQualityConfig;

            this.QualityVM.QualityConfig = MaociQualityConfig;
            this.MaociAlgorVM.Config = MaociAlgorParamConfig;
            this.SaveImageVM.Param = MaociSaveImageConfig;

            this.DefectsDataVM.DefectsProduce = MaociDefectsProduce;
            this.AlarmSetVM.CAlarmSet = MaociAlarmSetConfig;

            this.HistoryVM.HistoryModel = MaociHistoryModel;
            var cMysql = SQLManagement.SqlLoad() as CMysqlBLL; //数据库采用统一配置
            MaociMysqlConfig = cMysql;
            this.MySqlVM.MysqlExecute = cMysql;
            InitTask();
            TokeVM = new Token("", this.GetType().Namespace);
        }

        #region 时间相关

        [ObservableProperty]
        double algorithmTime = 0;

        [ObservableProperty]
        double filterTime = 0;
        #endregion

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
                if (value)
                {
                    CLoginViewModel.SloinPerson.Adapt(loginPerson);
                    CLoginViewModel.SloinPerson.IsNoPermission = true;
                }
                else
                {
                    loginPerson.Adapt(CLoginViewModel.SloinPerson);
                }
                MotionCtrlVM.SetRunning(IsStart);
                MarkCtrlVM.SetRunning(IsStart);
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
                MotionCtrlVM.SetRunning(isManualTest);
                MarkCtrlVM.SetRunning(isManualTest);
            }
        }

        /// <summary>
        /// 界面绑定变量，勿用此变量判断用户是否启动软件
        /// </summary>
        [ObservableProperty]
        bool startStop = false;

        [ObservableProperty]
        bool deviceSeting = false;

        #endregion

        #region 应用或丢弃当前工程变更

        /// <summary>
        /// 保存当前工程的修改
        /// </summary>
        public void ApplyChanges() => this.Adapt(this.model);

        /// <summary>
        /// 丢弃当前工程的修改
        /// </summary>
        public void DiscardChanges() => model.Adapt(this);
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
        /// 存图设置
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        CSaveImageVM saveImageVM = new CSaveImageVM(); //存图

        /// <summary>
        /// 2024.7.15 李焕彬
        /// 打标控制VM,初始化需要放在运动控制前面
        /// </summary>
        [ObservableProperty]
        CMarkCtrlVM markCtrlVM = new CMarkCtrlVM();

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 运动控制VM
        /// </summary>
        [ObservableProperty]
        CMotionCtrlVM motionCtrlVM = new CMotionCtrlVM();

        /// <summary>
        /// 数据库
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        CMySqlVM mySqlVM = new CMySqlVM(); //数据库
        #region 线程管理
        CancellationTokenSource m_cts = new CancellationTokenSource();

        public static readonly BoundedChannelOptions s_NormalChannelOptions =
            new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait };
        public static readonly BoundedChannelOptions s_SaveImgchannelOptions =
            new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait };
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
            CMainVM.s_SaveImgchannelOptions
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
                    try
                    {
                        PrintMsg msg = await m_InfoChannel.Reader.ReadAsync();
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
                        act(msg.message);
                    }
                    catch (Exception e)
                    {
                        SysLog.Error("信息记录线程出错:" + e.Message + e.StackTrace);
                    }
                }
            });
            #endregion

            #region 取图线程
            Task waitGetImageTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                await foreach (Cell cell in m_WaitImgChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        cell.ProjName = Name;
                        cell.EncoderPos = MarkCtrlVM.GetEncoderCount();
                        //从本地读图 没有相机时无需赋值
                        if (
                            !string.IsNullOrEmpty(CameraSerial)
                            && CCameraManagement.CamParamDict.ContainsKey(CameraSerial)
                        )
                        {
                            cell.CamName = CCameraManagement.CamParamDict[CameraSerial].Name;
                        }
                        WeakReferenceMessenger.Default.Send(cell.Image.ToBitmapSource(), TokeVM);
                        if (MotionCtrlVM.IsFocusing)
                        {
                            if (!MotionCtrlVM.FocusWaitGetImageChannel.Writer.TryWrite(cell))
                                cell.Dispose();
                        }
                        else if (IsStart || IsManualTest)
                        {
                            if (!m_AlgorithmChannel.Writer.TryWrite(cell))
                            {
                                //StringBuilder strbuilder = new StringBuilder("[");
                                //strbuilder.Append("取图线程");
                                //strbuilder.Append("]     ");
                                //strbuilder.Append(cell.ID);
                                //strbuilder.Append("   cell入算法队列失败。");
                                //await m_InfoChannel.Writer.WriteAsync(
                                //    new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                                //);
                                cell.Dispose();
                            }
                        }
                        else
                        {
                            cell.Dispose();
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
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
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

                        if (!m_FilterChannel.Writer.TryWrite(cell))
                        {
                            cell.Dispose();
                            //StringBuilder strbuilder = new StringBuilder("[");
                            //strbuilder.Append("算法线程");
                            //strbuilder.Append("]     ");
                            //strbuilder.Append(cell.ID);
                            //strbuilder.Append("   cell入筛选队列失败。");
                            //await m_InfoChannel.Writer.WriteAsync(
                            //    new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                            //);
                        }
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("配方执行线程出错：" + ex.Message, LOG.LOG_ERROR)
                        );
                        //SysLog.Error("配方执行线程出错：" + ex.Message);
                        GC.Collect();
                    }
                }
            });
            #endregion

            #region 筛选线程
            Task waitFilterTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                await foreach (Cell cell in m_FilterChannel.Reader.ReadAllAsync())
                {
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
                        else
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
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("筛选线程执行出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                        //SysLog.Error("筛选线程执行出错:" + ex.Message + ex.StackTrace);
                        GC.Collect();
                    }
                }
            });
            #endregion

            #region 显示线程
            Task waitShowTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                await foreach (Cell cell in m_ShowImageChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        //Console.WriteLine(DateTime.Now.Millisecond);

                        #region 窗口显示
                        try
                        {
                            for (int i = 0; i < 2; i++)
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
                                await drawView.Dispatcher.BeginInvoke(() =>
                                {
                                    drawView.Clear(false);
                                    drawView.SetPen(Brushes.Blue);
                                    drawView.ImgDrawPoints(cell.MaociTestOut.DarkTopRegion, false);
                                    drawView.ImgDrawPoints(cell.MaociTestOut.DarkBotRegion, false);
                                    drawView.SetPen(Brushes.Green);
                                    drawView.ImgDrawPoints(cell.MaociTestOut.LightBotRegion, false);
                                    drawView.ImgDrawPoints(cell.MaociTestOut.LightTopRegion, false);
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
                                                DefectFilter defectFilter = detection.DefectFilter;
                                                drawView.SetPen(defectFilter.ShowColor.Brush);
                                                drawView.SetFontBrush(defectFilter.ShowColor.Brush);
                                                for (int i = 0; i < detection.regionOut.Count; i++)
                                                {
                                                    drawView.ImgDrawPoints(
                                                        detection.regionOut[i].points1,
                                                        false
                                                    );
                                                    if (i == detection.regionOut.Count - 1)
                                                    {
                                                        drawView.ImgDrawText(
                                                            detection.DetectLog.ToString(),
                                                            detection.regionOut[i].GetCenter(),
                                                            false
                                                        );
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            DefectFilter defectFilter = cell.Detection.DefectFilter;
                                            if (
                                                !(
                                                    cell.Detection.Result
                                                    || cell.Detection.Category != Category.区域
                                                    || cell.Detection.regionOut.Count == 0
                                                )
                                            )
                                            {
                                                drawView.SetPen(defectFilter.ShowColor.Brush);
                                                drawView.SetFontBrush(defectFilter.ShowColor.Brush);
                                                for (
                                                    int i = 0;
                                                    i < cell.Detection.regionOut.Count;
                                                    i++
                                                )
                                                {
                                                    drawView.ImgDrawPoints(
                                                        cell.Detection.regionOut[i].points1,
                                                        false
                                                    );
                                                    if (i == cell.Detection.regionOut.Count - 1)
                                                    {
                                                        drawView.ImgDrawText(
                                                            cell.Detection.DetectLog.ToString(),
                                                            cell.Detection.regionOut[i].GetCenter(),
                                                            false
                                                        );
                                                    }
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
                                    drawView.Invalidate();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await m_InfoChannel.Writer.WriteAsync(
                                new PrintMsg("显示线程出错: " + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                            );
                            Growl.Error("显示线程出错: " + ex.Message + ex.StackTrace);
                        }
                        #endregion
                        if (
                            (SystemSettings.OfflineSave || isStart)
                            && (
                                MaociSaveImageConfig.SaveImageEnable
                                || MaociSaveImageConfig.PiantScreenEnable
                            )
                        ) //Clone 比较耗时 只有在开启存图时才复制Cell
                        {
                            Cell copy = cell.Clone();
                            if (!m_SaveImageChannel.Writer.TryWrite(copy))
                            {
                                copy.Dispose();
                            }
                        }

                        if (!m_AlarmChannel.Writer.TryWrite(cell))
                        {
                            cell.Dispose();
                            StringBuilder strbuilder = new StringBuilder("[");
                            strbuilder.Append("显示线程");
                            strbuilder.Append("]     ");
                            strbuilder.Append(cell.ID);
                            strbuilder.Append("   cell入报警队列失败。");
                            await m_InfoChannel.Writer.WriteAsync(
                                new PrintMsg(strbuilder.ToString(), LOG.LOG_ERROR)
                            );
                        }

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
                        WaitSignal.Set();
                    }
                }
            });
            #endregion

            #region 报警线程
            Task alarmTask = Task.Run(async () =>
            {
                object objAlarmLock = new object(); //报警监控用
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                await foreach (Cell cell in m_AlarmChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        lock (objAlarmLock)
                        {
                            MaociAlarmSetConfig.Excute(cell);
                        }
                    }
                    catch (Exception ex)
                    {
                        await m_InfoChannel.Writer.WriteAsync(
                            new PrintMsg("监控报警出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR)
                        );
                    }
                    finally
                    {
                        if (!m_dataBaseChannel.Writer.TryWrite(cell))
                            cell.Dispose();
                    }
                }
            });
            #endregion

            #region 数据库线程
            //数据库写入容易出错，卡顿时间较长，容量最大10个
            Task dataBaseTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
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

                    if (MaociMysqlConfig.SqlEnable)
                    {
                        try
                        {
                            if (SystemSettings.OfflineSave || IsStart)
                                MySqlVM.MysqlExecute.AddData(cell, SystemSettings.NowShift);
                        }
                        catch (Exception ex)
                        {
                            await m_InfoChannel.Writer.WriteAsync(
                                new PrintMsg("Mysql数据库写入出错:" + ex.Message, LOG.LOG_ERROR)
                            );
                            Growl.Warning(
                                new GrowlInfo()
                                {
                                    Message = "Mysql数据库写入出错!",
                                    StaysOpen = false,
                                    WaitTime = 2,
                                }
                            );
                        }
                    }
                    cell.Dispose();
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
                        string savePath = MaociSaveImageConfig.Excute(
                            SystemSettings,
                            cell,
                            ref saveCount
                        );
                        if (savePath != null)
                        {
                            WeakReferenceMessenger.Default.Send(
                                new AddOneNgImagePathMessage() { Path = savePath },
                                TokeVM
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
            switch (cell.AlgoriDetectResult)
            {
                case EMDETECTRESULT.EMDR_OK:
                    break;
                case EMDETECTRESULT.EMDR_NG_LIGHTEDGE:
                    break;
                case EMDETECTRESULT.EMDR_NG_DARKEDGE:
                    cell.Detection.DefectFilter = MaociFilterConfig.GetDefectFilter(
                        "异常类",
                        "算法异常",
                        "料区边缘Ng"
                    );
                    break;
                case EMDETECTRESULT.EMDR_NG_EMPTY:
                    break;
                case EMDETECTRESULT.EMDR_TIMEOUT:
                    cell.Detection.DefectFilter = MaociFilterConfig.GetDefectFilter(
                        "异常类",
                        "算法异常",
                        "超时"
                    );
                    break;
                default:
                    break;
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
