using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.DetectSystem.SystemSet;
using WH.DetectSystem.Models;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace WH.DetectSystem.ViewModels
{
    public partial class CMainModelsModelVM: ObservableObject
    {
        [ObservableProperty]
        CLoginViewModel loginViewModel = new CLoginViewModel();
        [ObservableProperty]
        CSystemSettingsVM systemSettings = new CSystemSettingsVM();
        /// <summary>
        /// 运行日志和报警日志
        /// </summary>
        public CLogRec SysLog { get; } = CPublicServices.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);
        /// <summary>
        /// 操作日志
        /// </summary>
        public CLogRec OperateLog { get; } = CPublicServices.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);
        [ObservableProperty]
        string projPath;
        public static string projFilter = "工程文件|*.burrproj|工程文件|*.Json";

        [ObservableProperty]
        bool isLoading = false;

        [ObservableProperty]
        CMainModelsModel cMainMModel = new CMainModelsModel();
        
        [ObservableProperty]
        ObservableCollection<CMainVM> cMainVMs = new ObservableCollection<CMainVM>();

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
                try
                {
                    SystemSettings = CSysSet.LoadParameter();
                    if (SystemSettings != null)
                    {
                        SysLog.Info(SystemSettingResources.SystemSettingsReadSuccess);

                        //CLoading.DispText("读取系统配置成功...", 10);
                    }
                    else
                    {
                        SysLog.Error(SystemSettingResources.SystemSettingsReadFailed);
                        //CLoading.DispText("读取系统配置失败...", 10);
                    }
                    progress.Report(10);

                    await longtimefunc(progress);
                }
                catch (Exception)
                {

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
            #region 打开工程
            try
            {
                ProjPath = header;
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
                while (SystemSettings.RecentProjs.Count > 10)
                {
                    SystemSettings.RecentProjs.RemoveAt(SystemSettings.RecentProjs.Count - 1);
                }
                await longtimefunc(progress);
            }
            catch (Exception)
            {

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
            if (string.IsNullOrEmpty(ProjPath)) return;
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
