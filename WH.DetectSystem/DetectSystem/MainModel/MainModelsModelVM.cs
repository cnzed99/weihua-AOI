using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using AlarmSetCtrl;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using HistoryPlayback;
using HistoryPlayback.Model;
using Mapster;
using MarkControl;
using MotionControl;
using MySqlOperatesApi;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.DetectSystem.SystemSet;
using WH.DetectSystem.Models;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace WH.DetectSystem.ViewModels
{
    public partial class CMainModelsModelVM : ObservableObject
    {
        [ObservableProperty]
        CLoginViewModel loginViewModel = new CLoginViewModel();

        public CSystemSettingsVM SystemSettings { get; set; } =
            CPublicServices.Container.Resolve<CSystemSettingsVM>();
        public Version version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version;

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
        /// 多制程视图模型
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CMainVM> cMainVMs = new ObservableCollection<CMainVM>();

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯列表
        /// </summary>
        public List<CCommunicationSettingBase> ListCommSetParam { get; set; }

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
        }
        #endregion

        #region 软件加载
        /// <summary>
        /// 软件加载
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task LoadAsync(IProgress<double> progress)
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
                //        SysLog.Info(SystemSettingResources.SystemSettingsReadSuccess);

                //        //CLoading.DispText("读取系统配置成功...", 10);
                //    }
                //    else
                //    {
                //        SysLog.Error(SystemSettingResources.SystemSettingsReadFailed);
                //        //CLoading.DispText("读取系统配置失败...", 10);
                //    }
                //    progress.Report(10);

                await longtimefunc(progress);
                //}
                //catch (Exception) { }
                #endregion

                #region 读取所有通讯参数文件并连接通讯
                try
                {
                    if (File.Exists(CCommunicationManagement.s_CommPath))
                    {
                        ListCommSetParam = ConfigAPI.LoadDeserialize<
                            List<CCommunicationSettingBase>
                        >(CCommunicationManagement.s_CommPath);
                    }
                    else
                    {
                        ListCommSetParam = new List<CCommunicationSettingBase>();
                    }
                    CommManagement = new CCommunicationManagement(
                        ListCommSetParam,
                        CCommunicationManagement.s_CommPath
                    );
                    if (!CommManagement.OpenAllComm())
                    {
                        Growl.Error("连接通讯失败，请检查参数表！");
                    }
                }
                catch (Exception ex)
                {
                    Growl.Error("读取通讯参数发生异常,请检查参数表是否损坏:\r\n" + ex.Message);
                }
                #endregion

                #region 读取所有相机参数文件并连接相机
                try
                {
                    if (File.Exists(CCameraManagement.s_CamPath))
                    {
                        ListCamSetParam = ConfigAPI.LoadDeserialize<List<CCameraParameterBase>>(
                            CCameraManagement.s_CamPath
                        );
                    }
                    else
                    {
                        ListCamSetParam = new List<CCameraParameterBase>();
                    }
                    CamManagement = new CCameraManagement(
                        ListCamSetParam,
                        CCameraManagement.s_CamPath
                    );
                }
                catch (Exception ex)
                {
                    Growl.Error("读取相机参数发生异常,请检查参数表是否损坏:\r\n" + ex.Message);
                }
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
        public async Task OpenProj(IProgress<double> progress, string header)
        {
            IsLoading = true;
            //WeakReferenceMessenger.Default.Reset();
            #region 打开工程
            try
            {
                ProjPath = header;
                WeakReferenceMessenger.Default.UnregisterAll(CMainVMs[0].MaociAlgorParamConfig);
                WeakReferenceMessenger.Default.UnregisterAll(CMainVMs[0].MaociQualityConfig);
                WeakReferenceMessenger.Default.UnregisterAll(CMainVMs[0].MaociFilterConfig);
                WeakReferenceMessenger.Default.UnregisterAll(CMainVMs[0].MaociAlarmSetConfig);
                WeakReferenceMessenger.Default.UnregisterAll(CMainVMs[0].MaociSaveImageConfig);

                CMainMModel = ConfigAPI.Load<CMainModelsModel>(header);
                //foreach (var item in CMainVMs)
                //{
                //    item.StopTask();
                //}
                //CMainVMs.Clear();
                foreach (var item in CMainMModel.CMainModels)
                {
                    //CMainVMs.Add(new CMainVM() { Model = item });
                    CMainVMs[0].Model = item;
                }
                SystemSettings.RecentProjs.Remove(header);
                SystemSettings.RecentProjs.Insert(0, header);
                progress.Report(50);
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

        async Task longtimefunc(IProgress<double> progress)
        {
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(10);
                progress.Report(i);
            }
        }
        #endregion

        #region 保存当前工程
        public void SaveCurrentProj()
        {
            if (string.IsNullOrEmpty(ProjPath))
                return;
            foreach (var proj in CMainVMs)
            {
                proj.ApplyChanges();
            }
            SystemSettings.RecentProjs.Remove(ProjPath);
            SystemSettings.RecentProjs.Insert(0, ProjPath);
            ConfigAPI.Save(CMainMModel, ProjPath);
        }
        #endregion
    }
}
