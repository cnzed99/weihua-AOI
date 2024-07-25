using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
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
using MySqlOperatesApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjProduceData;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.DetectSystem.SystemSet;
using WH.DetectSystem.Models;
using WH.DetectSystem._4_报警处理;
using WH.DetectSystem._5_存图操作;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.Entity.Messages;
using WH.RecipeCellRootBase;
using WH.RunCell;

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
        }

        [ObservableProperty]
        BitmapSource modelImage = new BitmapImage(new Uri("D://铝极.png"));

        [ObservableProperty]
        Brush modelBrush = Brushes.White;

        [ObservableProperty]
        BitmapSource lastImage = new BitmapImage(new Uri("D://铝极.png"));

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
                    CCameraManagement.StartImaging(this.GUID, this.CameraSerial);
                }
                else
                {
                    CCameraManagement.StopImaging(this.GUID, this.CameraSerial);
                }
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
        /// 数据库
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        CMySqlVM mySqlVM = new CMySqlVM(); //数据库
        #region 线程管理
        CancellationTokenSource m_cts = new CancellationTokenSource();

        public static readonly BoundedChannelOptions s_NormalChannelOptions =
            new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropWrite };
        public static readonly BoundedChannelOptions s_SaveImgchannelOptions =
            new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropWrite };

        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly Channel<string> m_InfoChannel = Channel.CreateBounded<string>(
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
            s_SaveImgchannelOptions
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
                        string msg = await m_InfoChannel.Reader.ReadAsync();
                        //await Task.Delay(10);
                        SysLog.Info(msg);
                    }
                    catch (Exception e)
                    {
                        SysLog.Error("信息记录线程出错:" + e.Message + e.StackTrace);
                    }
                }
            });
            #endregion

            #region 触发相机线程
            //Task TriggerCameraTask = Task.Run(async () =>
            //{
            //    Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //    DateTime triggerStartData = DateTime.Now;
            //    IEnumerator<string> imgitor = new List<string>()
            //    {
            //        "D://铝极.png",
            //        "D://原图-1.bmp",
            //        "D://原图-2.bmp",
            //        "D://原图-3.bmp",
            //        "D://设备-1.PNG",
            //        "D://设备-2.PNG",
            //        "D://设备-3.PNG"
            //    }.GetEnumerator();
            //    CBrushPro color = new CBrushPro();
            //    IEnumerator<CKnownColor> brushes = color.KnownColors.GetEnumerator();
            //    Random random = new Random(50);
            //    while (true)
            //    {
            //        #region test

            //        await Task.Delay(20);
            //        if (!IsStart)
            //            continue;
            //        if (!imgitor.MoveNext())
            //        {
            //            imgitor.Reset();
            //            imgitor.MoveNext();
            //        }
            //        if (!brushes.MoveNext())
            //        {
            //            brushes.Reset();
            //            brushes.MoveNext();
            //        }

            //        Cell cell = new Cell()
            //        {
            //            Image = new(imgitor.Current),
            //            ID = "00001",
            //            OtherInfoRecv = new Dictionary<string, string>(),
            //            isOnce = false,
            //            CancelSource = this.m_cts,
            //            ProjGuid = GUID,
            //            ComGuid = "001",
            //            CamSerial = CameraSerial,
            //            Quality = MaociQualityConfig.Qualities[0]
            //        };
            //        if (random.Next(10) > 5)
            //            cell.IsOK = true;
            //        StringBuilder strbuilder = new StringBuilder("[");

            //        strbuilder.Append("触发");
            //        strbuilder.Append("]     ");
            //        strbuilder.Append(cell.ID);
            //        strbuilder.Append("   收到触发信号");

            //        await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
            //        await m_WaitImgChannel.Writer.WriteAsync(cell);

            //        #endregion

            //        //((string Cam, string split), string Com, byte[] bytes) Data = await CCommunicationBase.DataChannel.Reader.ReadAsync();
            //        //try
            //        //{
            //        //    if (CCameraManagement.CameraDict.ContainsKey(Data.Item1.Cam) && !CCameraManagement.CameraDict[Data.Item1.Cam].ProjGuid[Data.Item1.split].IsNullOrEmpty()
            //        //    && CCommunicationManagement.CheckTriggerSignal(Data.Com, Data.Item1.Cam, Data.Item1.split, Data.bytes, out string IDstr, out Dictionary<string, string> otherRecvInfo))
            //        //    {
            //        //        if (SystemStatic._isRuning && !CCameraManagement.CameraDict[Data.Item1.Cam]._isGrabing && CCameraManagement.CameraDict.ContainsKey(Data.Item1.Cam))
            //        //        {
            //        //            CCameraManagement.CameraDict[Data.Item1.Cam]._isGrabing = true;

            //        //            Cell cell = new Cell()
            //        //            {
            //        //                //ID = (DetectSysConfig.Config.LevelProduce.TotalNum + 1).ToString(), //这里的总数要改成PLC发上来的
            //        //                ID = IDstr,
            //        //                OtherInfoRecv = otherRecvInfo,
            //        //                isOnce = false,
            //        //                CancelSource = this.CancelToken,
            //        //                ProjGuid = CCameraManagement.CameraDict[Data.Item1.Cam].ProjGuid[Data.Item1.split],
            //        //                ComGuid = Data.Com,
            //        //                CamSerial = Data.Item1.Cam
            //        //            };
            //        //            // 准备 清空
            //        //            CCommunicationManagement.SendReadySignal(Data.Item1.Cam, IDstr, cell.OtherInfoSend);

            //        //            StringBuilder strbuilder = new StringBuilder("[");

            //        //            strbuilder.Append("触发");
            //        //            strbuilder.Append("]     ");
            //        //            strbuilder.Append(cell.ID);
            //        //            strbuilder.Append("   收到触发信号");

            //        //            _infoLog.Enqueue(strbuilder.ToString());

            //        //            TimeSpan triggerSpan = DateTime.Now - triggerStartData;
            //        //            triggerStartData = DateTime.Now;

            //        //            this.BeginInvoke(new Action(() =>
            //        //            {
            //        //                if (triggerSpan.TotalMilliseconds > 99999)
            //        //                {
            //        //                    triggerSpan = TimeSpan.FromMilliseconds(99999);
            //        //                }

            //        //                DetectProgress.Value = 0;
            //        //                lb_triggerTime.Text = triggerSpan.TotalMilliseconds.ToString("F");
            //        //            }));


            //        //            if (!CCameraManagement.CameraDict[Data.Item1.Cam].Connected)
            //        //            {
            //        //                this.NotifyError("相机未打开，请检查相机是否连接", 5000);
            //        //                throw new Exception("相机未打开，请检查相机连接");
            //        //            }
            //        //            if (!CCommunicationManagement.CommDic[Data.Com].IsConnected && !_bTestOffLine)
            //        //            {
            //        //                this.NotifyError("通讯未打开，请检查通讯是否连接", 5000);
            //        //                throw new Exception("通讯未打开，请检查通讯是否连接");
            //        //            }

            //        //            cell.Stopwatch.Restart();
            //        //            strbuilder.Clear();
            //        //            strbuilder.Append("[");
            //        //            strbuilder.Append("触发");
            //        //            strbuilder.Append("]     ");
            //        //            strbuilder.Append(cell.ID);
            //        //            strbuilder.Append("   开始触发拍照");
            //        //            _infoLog.Enqueue(strbuilder.ToString());

            //        //            cell.BeginVisionTime = DateTime.Now;
            //        //            await CCameraManagement.CameraDict[Data.Item1.Cam]._TriggerImageChannel.Writer.WriteAsync(cell);
            //        //            CCameraManagement.CameraDict[Data.Item1.Cam].TriggerCount++;
            //        //            strbuilder.Clear();
            //        //            strbuilder.Append("收到触发信号:");
            //        //            strbuilder.Append(CCameraManagement.CameraDict[Data.Item1.Cam].TriggerCount);
            //        //            strbuilder.Append("次, 开始触发");
            //        //            CCameraManagement.CamLogger.Info(strbuilder);
            //        //            CCameraManagement.CameraDict[Data.Item1.Cam].ExecuteSoftwareTrigger();
            //        //            // 正在拍照中
            //        //            CCommunicationManagement.SendGrabbingSignal(Data.Item1.Cam, IDstr, cell.OtherInfoSend);

            //        //        }

            //        //    }
            //        //}
            //        //catch (Exception ex)
            //        //{
            //        //    CCameraManagement.CameraDict[Data.Item1.Cam]._isGrabing = false;
            //        //    this.Invoke(new Action(() =>
            //        //    {
            //        //        CUpdateRecords.AddLogToListBox("触发相机线程出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR);
            //        //        this.NotifyWarning("触发相机线程出错:" + ex.Message, 1000);
            //        //    }));
            //        //}
            //    }
            //});
            #endregion

            #region 取图线程
            Task waitGetImageTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                await foreach (Cell cell in m_WaitImgChannel.Reader.ReadAllAsync())
                {
                    WeakReferenceMessenger.Default.Send(cell.Image, TokeVM);

                    await m_AlgorithmChannel.Writer.WriteAsync(cell);
                    //await Task.Delay(50);
                    //try
                    //{
                    //    Cell cell = await CCameraBase.WaitGetImageChannel.Reader.ReadAsync();
                    //    CCommunicationManagement.SendGrabFinishSignal(cell.CamSerial, cell.ID, cell.OtherInfoSend);
                    //    FlowEditSetFrm.InputImg = cell.ColorImage;
                    //    cell.GetImageTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                    //    cell.Stopwatch.Restart();
                    //    StringBuilder strbuilder = new StringBuilder("[");

                    //    strbuilder.Append("采图");
                    //    strbuilder.Append("]     ");
                    //    strbuilder.Append(cell.ID);
                    //    strbuilder.Append("   图片采集完成,耗时:");
                    //    strbuilder.Append(cell.GetImageTime.TotalMilliseconds.ToString("F2"));
                    //    _infoLog.Enqueue(strbuilder.ToString());

                    //    // _infoLog.Enqueue($"{$"[{CCameraBase.WaitGetImageQueue.s_Name}]",-10}{cell.ID,-8}{"图片采集完成",-20}耗时 {cell.GetImageTime.TotalMilliseconds:0.00}");

                    //    // this._waitPreImageQueue.Enqueue(cell);
                    //    //显示图片到窗口
                    //    await PreImageChannel.Writer.WriteAsync(cell);
                    //    GetImageSuccess(cell);
                    //    if (cell != null && cell.FrameLoss)
                    //    {
                    //        CUpdateRecords.AddLogToListBox("取图线程:相机取图丢帧", LOG.LOG_ERROR);
                    //        List<string[]> Exception = new List<string[]>();
                    //        Exception.Add(new string[3] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "1", "取图线程:相机取图丢帧" });
                    //        CSVUtil.WriteCSV(Directory.GetCurrentDirectory() + "\\Log\\相机丢帧记录.CSV", true, Exception);
                    //    }

                    //}
                    //catch (Exception ex)
                    //{
                    //    CUpdateRecords.AddLogToListBox("取图线程出错:" + ex.Message + ex.StackTrace, LOG.LOG_ERROR);
                    //}
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
                        StringBuilder strbuilder = new StringBuilder("[");

                        //strbuilder.Append("算法");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   开始执行配方");
                        //await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
                        MaociAlgorParamConfig.MaociExcute(cell);
                        //SystemStatic.RecipeList[cell.ProjGuid].RecipeExcute(cell);

                        //if (cell._skipthis)
                        //{
                        //    SetBadCell(cell); //默认是一个最差的片
                        //}
                        //await Task.Delay(30);

                        cell.RecipeTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                        cell.Stopwatch.Restart();
                        strbuilder.Clear();
                        strbuilder.Append("[");
                        strbuilder.Append("算法");
                        strbuilder.Append("]     ");
                        strbuilder.Append(cell.ID);
                        strbuilder.Append("   配方执行完成,耗时:");
                        strbuilder.Append(cell.RecipeTime.TotalMilliseconds.ToString("F2"));
                        AlgorithmTime = cell.RecipeTime.TotalMilliseconds;
                        await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
                        //if (CSystemParamJson.SystemSetParam.ShowChangeImage)
                        //{
                        //    if (cell.PreVal.ImageHDict.Keys.Contains("硅片区R"))
                        //    {
                        //        cell.ChangleImgae = cell.PreVal.ImageHDict["硅片区R"].GetDictObject("ImgR_ImageTrans");
                        //        GetImageSuccess(cell);
                        //    }

                        //}
                        await m_FilterChannel.Writer.WriteAsync(cell);
                    }
                    catch (Exception ex)
                    {
                        SysLog.Error("配方执行线程出错：" + ex.Message);
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
                        StringBuilder strbuilder = new StringBuilder("[");

                        //strbuilder.Append("筛选");
                        //strbuilder.Append("]     ");
                        //strbuilder.Append(cell.ID);
                        //strbuilder.Append("   开始筛选");
                        //await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
                        //await Task.Delay(30);
                        MaociFilterConfig.FilterExute(cell);
                        //SystemStatic.SysConfigList[cell.ProjGuid].Config.CFilterConfig.FilterExcute(cell);
                        //ColorGradeGroupConfig colorConfig = null;
                        //if (SystemStatic.SysConfigList[cell.ProjGuid].Config.ColorConfig.SelectedParam != null)
                        //{
                        //    colorConfig = SystemStatic.SysConfigList[cell.ProjGuid].Config.ColorConfig.SelectedParam;

                        //}
                        //SetCellQuality(cell);
                        //定级在这里完成
                        cell.FilterTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);
                        cell.Stopwatch.Stop();
                        //CCommunicationManagement.SendDetectionFinishSignal(cell.CamSerial, cell.ID, cell.QualitySignal, cell.ColorSignel, cell.OtherInfoSend);

                        cell.ProcessTime = DateTime.Now - cell.CreateTime;

                        strbuilder.Clear();
                        strbuilder.Append("[结束]     ");
                        strbuilder.Append(cell.ID);
                        strbuilder.Append("   检测结束,耗时:");
                        strbuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F2"));
                        FilterTime = cell.FilterTime.TotalMilliseconds;
                        await m_InfoChannel.Writer.WriteAsync(strbuilder.ToString());
                        //cell.Stopwatch.Stop();
                        //if ((!SystemStatic._isRuning && CSystemParamJson.SystemSetParam.OfflineSave) || SystemStatic._isRuning)//如果是离线检测状态 并且开启了离线存图和数据按钮  或者是正常运行状态
                        //{
                        //    this.Invoke(new Action(() =>
                        //    {
                        //        SystemStatic.SysConfigList[cell.ProjGuid].Config.LevelProduce.Update(cell);//设置当前缺陷，实现缺陷生产数据情况自行处理
                        //        dataGridView1.DataSource = SystemStatic.SysConfigList[cell.ProjGuid].Config.LevelProduce.DefectProduce.DefectNumbersList.Where(o => o.Number > 0).ToList();// DetectSysConfig.Config.LevelProduce.DefectProduce.DefectNumbersList;//刷新表格数据
                        //        dataGridView1.Invalidate(dataGridView1.DisplayRectangle);
                        //    }));

                        if (MaociSaveImageConfig.SaveImageEnable || !cell.IsOK) //Clone 比较耗时 只有在开启存图 或NG时才复制Cell
                        {
                            await m_SaveImageChannel.Writer.WriteAsync(cell.Clone());
                        }

                        await m_ShowImageChannel.Writer.WriteAsync(cell);
                        //_waitShowImageQueue.Enqueue(cell); //先加入存图 再加入显示  因为先显示可能会先把cell dispose掉,Clone时会报错
                        //GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        SysLog.Error("筛选线程执行出错:" + ex.Message + ex.StackTrace);
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
                        //showText.Clear();//list只保存一张图片的文本提示
                        //HImage dumpimage = HWin_DispProduct.hWindow.DumpWindowImage();
                        //HOperatorSet.ZoomImageSize(dumpimage, out HObject img, 64, 64, "constant");
                        //Bitmap bmp = WHImageConvert.HImage2Bitmap(img);
                        //Brush showcolor = cell.Quality.ShowColor;
                        ModelBrush = cell.Quality.ShowColor.Brush;
                        if (!cell.IsOK)
                        {
                            LastBrush = ModelBrush;
                            LastImage = ModelImage;
                        }
                        //cell.Detection = new CellDetection()
                        //{
                        //    s_Name = "掉料0",
                        //    DefectFilter = MaociFilter.SpeciesFilters[0].RecipeDefects[0].DefectFilters[0],

                        //};
                        MaociDefectsProduce.Excute(cell);
                        WeakReferenceMessenger.Default.Send(cell, TokeVM);
                        Console.WriteLine(DateTime.Now.Millisecond);
                        //if (!cell.IsOK) //如果质量OK 颜色不OK
                        //{
                        //    showcolor = cell.q.ShowColor;
                        //}
                        //else
                        //{
                        //    showcolor =
                        //}
                        //this.Invoke(new Action(() =>
                        //{
                        //    //DetectSysConfig.Config.LevelProduce.Update(cell);//设置当前缺陷，实现缺陷生产数据情况自行处理
                        //    HWin_DispProduct.hWindow.DispText(cell.ProcessTime.TotalMilliseconds.ToString("0"),
                        //        "window", "bottom", "right", "white", new string[] { "box", "shadow", "box_color" },
                        //        new string[] { "true", "false", "#00CD66" });

                        //    //this.SaveShowText(cell.ProcessTime.TotalMilliseconds.ToString("0"), "window", "bottom", "right", "white",
                        //    //                                    new string[] { "box", "shadow", "box_color" }, new string[] { "true", "false", "#00CD66" });//add by zhuhm 20230801将文本信息保存到list，便于特征窗口还原文本信息
                        //    //StringBuilder textBuilder = new StringBuilder();
                        //    //textBuilder.Append(cell.QualityName);

                        //}));
                        await m_AlarmChannel.Writer.WriteAsync(cell);
                        _ = Task.Run(() => {
                            #region 窗口显示
                            //this.Invoke(new Action(() =>
                            //{
                            //    try
                            //    {
                            //        if (HWin_DispProduct.DispObjects != null) HWin_DispProduct.DispObjects.Clear();//todo:ZhuHM20230801   清除前张图片的regions，，只保存当前图片的Regions
                            //        lb_GetImgTime.Text = cell.GetImageTime.TotalMilliseconds.ToString("F2");
                            //        lb_PreprocessTime.Text = cell.PreTime.TotalMilliseconds.ToString("F2");
                            //        lb_RecipeTime.Text = cell.RecipeTime.TotalMilliseconds.ToString("F2");
                            //        lb_FilterTime.Text = cell.FilterTime.TotalMilliseconds.ToString("F2");
                            //        lb_ColorValue.Text = cell.ColorValue.ToString();
                            //        StringBuilder textBuilder = new StringBuilder();

                            //        string color = "green";

                            //        if (cell.Detection != null)
                            //        {
                            //            HWin_DispProduct.Background = WpfWFTransfer.WFColor2Wpf(cell.QualityColor);

                            //            textBuilder.AppendLine(cell.Detection.s_Name);

                            //            color = cell.QualityColorStr;
                            //            //窗口左上角显示定级缺陷名称，信号值

                            //            // dataGridView1.Invalidate(dataGridView1.DisplayRectangle);
                            //            //显示所有Region缺陷
                            //            if (_projConfig.ProcessSet.ShowAllDefects)
                            //            {
                            //                List<(double, double, double, HObject, string, string)> lsArea = new List<(double, double, double, HObject, string, string)>();
                            //                foreach (CellDetection detection in cell.Detections)
                            //                {
                            //                    if (detection.Result || detection.Category != Category.区域) continue;
                            //                    HOperatorSet.SetColor(HWin_DispProduct.hWindow, detection.ShowColor);
                            //                    HObject dispRegion = detection.UnionedRegion.Clone();
                            //                    HWin_DispProduct.DispObj(dispRegion);
                            //                    var split = detection.DetectLog.ToString().Split('\n');
                            //                    HOperatorSet.AreaCenter(detection.UnionedRegion, out HTuple area, out HTuple row, out HTuple col);
                            //                    if (split.Length >= 2 && area.Length > 0)
                            //                    {
                            //                        this.SaveShowText((HTuple)$"{split[0]}\n{split[1]}", "image", row, col, detection.ShowColor,
                            //                                    new[] { "box_color", "shadow" }, new[] { "#00000040", "false" });
                            //                        this.SaveShowText(detection.DetectLog.ToString(), "image", 0, 0, detection.ShowColor,
                            //                                    new[] { "box_color", "shadow" }, new[] { "#00000040", "false" }, dispRegion);
                            //                    }
                            //                    for (int i = 0; i < area.Length; i++)
                            //                    {
                            //                        lsArea.Add((area[i].D, row[i].D, col[i].D, detection.UnionedRegion.SelectObj(i + 1), detection.DetectLog.ToString(), detection.ShowColor));
                            //                    }
                            //                }
                            //                lsArea.Sort((left, right) => left.Item1.CompareTo(right.Item1));
                            //                int countShow = Math.Min(3, lsArea.Count);//只显示面积最大的3个标签
                            //                for (int i = lsArea.Count - 1; i >= lsArea.Count - countShow; i--)
                            //                {
                            //                    TextToFeatures(lsArea[i].Item4, lsArea[i].Item5, out string result);
                            //                    HWin_DispProduct.hWindow.DispText(result, "image", lsArea[i].Item2,
                            //                    lsArea[i].Item3, lsArea[i].Item6, new[] { "box_color", "shadow" }, new[] { "#00000040", "false" });
                            //                }
                            //            }
                            //            else //只显示定级缺陷
                            //            {
                            //                switch (cell.Detection.Category)
                            //                {
                            //                    case Category.区域:
                            //                        HOperatorSet.SetColor(HWin_DispProduct.hWindow,
                            //                            cell.Detection.ShowColor);
                            //                        HObject dispRegion = cell.Detection.UnionedRegion.Clone();
                            //                        HWin_DispProduct.DispObj(dispRegion);
                            //                        var split = cell.Detection.DetectLog.ToString().Split('\n');
                            //                        HOperatorSet.AreaCenter(cell.Detection.UnionedRegion,
                            //                                out HTuple area, out HTuple row, out HTuple col);
                            //                        if (split.Length >= 2 && area.Length > 0)
                            //                        {
                            //                            HWin_DispProduct.hWindow.DispText(
                            //                            (HTuple)$"{split[0]}\n{split[1]}", "image", row[0],
                            //                                    col[0], (HTuple)cell.Detection.ShowColor,
                            //                                    new[] { "box_color", "shadow" },
                            //                                    new[] { "#00000040", "false" });
                            //                            this.SaveShowText((HTuple)$"{split[0]}\n{split[1]}", "image", row, col, cell.Detection.ShowColor,
                            //                                new[] { "box_color", "shadow" }, new[] { "#00000040", "false" });
                            //                            this.SaveShowText(cell.Detection.DetectLog.ToString(), "image", row.D, col.D, cell.Detection.ShowColor,
                            //                                new[] { "box_color", "shadow" }, new[] { "#00000040", "false" }, dispRegion);
                            //                        }
                            //                        break;
                            //                    case Category.值:
                            //                        if (cell.Detection.DetectLog.Length == 0) break;
                            //                        HWin_DispProduct.hWindow.DispText(
                            //                            cell.Detection.DetectLog.ToString(), "window", "top", "left", "green",
                            //                            new[] { "box", "shadow" }, new[] { "false", "false" });
                            //                        this.SaveShowText(cell.Detection.DetectLog.ToString(), "window", "top", "left", "green",
                            //                            new[] { "box", "shadow" }, new[] { "false", "false" });//add by zhuhm 20230801将文本信息保存到list，便于特征窗口还原文本信息
                            //                        break;
                            //                }
                            //            }

                            //        }

                            //        textBuilder.Append("信号:");
                            //        textBuilder.Append(cell.QualitySignal);
                            //        if (cell.ColorGrade != null)
                            //        {
                            //            textBuilder.Append("|");
                            //            textBuilder.Append(cell.ColorSignel);
                            //        }
                            //        HWin_DispProduct.hWindow.DispText(textBuilder.ToString(), "window", "top", "right", color,
                            //            new[] { "box", "shadow" }, new[] { "false", "false" });

                            //        this.SaveShowText(textBuilder.ToString(), "window", "top", "right", color,
                            //                            new[] { "box", "shadow" }, new[] { "false", "false" });//add by zhuhm 20230801将文本信息保存到list，便于特征窗口还原文本信息
                            //        if (cell.Detection != null)
                            //        {
                            //            SaveDumpImage(cell); //存NG截图
                            //        }
                            //        // cell.Dispose(); //结束 清理

                            //        cell.ShowTime = new TimeSpan(cell.Stopwatch.ElapsedTicks);

                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        SysLog.Error("显示线程出错: " + ex.Message + ex.StackTrace);
                            //    }
                            //    finally
                            //    {
                            //        cell.Dispose(); //结束 清理
                            //        GC.Collect();
                            //    }

                            //}));
                            #endregion
                        });

                        if (cell.isOnce)
                        {
                            //this.Invoke(new Action(EnableButtons));
                        }

                        //cell.Dispose();
                    }
                    catch (Exception ex)
                    {
                        SysLog.Error("显示线程出错: " + ex.Message + ex.StackTrace);
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
                        SysLog.Error("监控报警出错:" + ex.Message + ex.StackTrace);
                    }
                    finally
                    {
                        await m_dataBaseChannel.Writer.WriteAsync(cell);
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
                    //    SysLog.Error("Access数据库写入错误:" + ex.Message + ex.StackTrace);
                    //}
                    #endregion

                    if (SystemSettings.IsToMysql)
                    {
                        try
                        {
                            if (SystemSettings.OfflineSave || IsStart)
                                MySqlVM.MysqlExecute.AddData(cell, SystemSettings.NowShift);
                        }
                        catch (Exception ex)
                        {
                            SysLog.Error("Mysql数据库写入出错:" + ex.Message);
                            Growl.Warning(
                                new GrowlInfo()
                                {
                                    Message = "Mysql数据库写入出错:",
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
                await foreach (Cell cell in m_SaveImageChannel.Reader.ReadAllAsync())
                {
                    try
                    {
                        //// int queCount = _waitSaveImageQueue.Count;
                        string savePath = MaociSaveImageConfig.Excute(cell);
                        WeakReferenceMessenger.Default.Send(
                            new AddOneNgImagePathMessage() { Path = savePath },
                            TokeVM
                        );
                        cell.Dispose(); //这个cell是复制的clone 存图后清理
                    }
                    catch (Exception ex)
                    {
                        SysLog.Error("存图线程出错:" + ex.Message + ex.StackTrace);
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

        /// <summary>
        /// 获取图像成功 显示至窗口
        /// </summary>
        /// <param name="cell"></param>
        private void GetImageSuccess(Cell cell)
        {
            try
            {
                //if (CSystemParamJson.SystemSetParam.ScaleEnable)
                //{
                //    HOperatorSet.Decompose3(cell.ColorImage, out HObject SelectR, out HObject SelectG, out HObject SelectB);
                //    HOperatorSet.ScaleImage(SelectR, out HObject imageSaledR, CSystemParamJson.SystemSetParam.ScaleMult[0], 0);
                //    HOperatorSet.ScaleImage(SelectG, out HObject imageSaledG, CSystemParamJson.SystemSetParam.ScaleMult[1], 0);
                //    HOperatorSet.ScaleImage(SelectB, out HObject imageSaledB, CSystemParamJson.SystemSetParam.ScaleMult[2], 0);

                //    HOperatorSet.Compose3(imageSaledR, imageSaledG, imageSaledB, out HObject mulitiChannelImage);
                //    cell.ColorImage.Dispose();
                //    cell.ColorImage = mulitiChannelImage;
                //    SelectR.Dispose();
                //    SelectG.Dispose();
                //    SelectB.Dispose();
                //    imageSaledR.Dispose();
                //    imageSaledG.Dispose();
                //    imageSaledB.Dispose();

                //}
                //this.BeginInvoke(new Action(() =>
                //{

                //    //if (CSystemParamJson.SystemSetParam.ShowChangeImage)
                //    //{
                //    //    if (cell.ChangleImgae != null)
                //    //    {
                //    //        if (cell.FrameLoss)
                //    //        {
                //    //            HWin_DispProduct.ModelImage = cell.ChangleImgae.Clone();
                //    //            _isFirstImage = true;
                //    //        }
                //    //        else
                //    //        {
                //    //            if (_isFirstImage)
                //    //            {
                //    //                HWin_DispProduct.ModelImage = cell.ChangleImgae.Clone();
                //    //                _isFirstImage = false;
                //    //            }
                //    //            else
                //    //            {
                //    //                HWin_DispProduct.Image = cell.ChangleImgae.Clone();
                //    //            }
                //    //        }
                //    //    }

                //    //}
                //    //else
                //    //{
                //    //    if (cell.FrameLoss)
                //    //    {
                //    //        HWin_DispProduct.ModelImage = cell.ColorImage.Clone();
                //    //        _isFirstImage = true;
                //    //    }
                //    //    else
                //    //    {
                //    //        if (_isFirstImage)
                //    //        {
                //    //            HWin_DispProduct.ModelImage = cell.ColorImage.Clone();
                //    //            _isFirstImage = false;
                //    //        }
                //    //        else
                //    //        {
                //    //            HWin_DispProduct.Image = cell.ColorImage.Clone();
                //    //        }
                //    //    }
                //    //}

                //    //HWin_DispProduct.Background = System.Windows.Media.Brushes.Transparent;

                //}));
            }
            catch (Exception ex)
            {
                SysLog.Error($"显示窗口出错:{ex.Message}");
            }
        }
        #endregion
    }
}
