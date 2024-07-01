using AlgorithmDll;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapster;
using QualityGrade;
using SDFilter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WH.Controls;
using WH.Entity.LogRecord;
using 断面毛刺检测软件.Models;

namespace 断面毛刺检测软件.ViewModels
{
    /// <summary>
    /// 主界面视图模型
    /// </summary>
    public partial class MainVM : MainModel
    {
        [ObservableProperty]
        LoginViewModel loginViewModel = new LoginViewModel();
        [ObservableProperty]
        SystemSettingsVM systemSettings = new SystemSettingsVM();
        /// <summary>
        /// 运行日志和报警日志
        /// </summary>
        public CLogRec SysLog { get;} = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_SYS);
        /// <summary>
        /// 操作日志
        /// </summary>
        public CLogRec OperateLog { get;} = App.Container.ResolveKeyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE);
        /// <summary>
        /// 当前工程
        /// </summary>
        private MainModel _model = new MainModel();
        /// <summary>
        /// 当前工程 禁止直接修改其属性
        /// </summary>
        public MainModel Model { get => _model; set{
                SetProperty(ref _model, value);
                _model.Adapt(this);
                SDFilterCtrlVM.FilterConfig = MaociFilter;
                QualityCtrlVM.QualityConfig = MaociQuality;
                //MaociAlgorParamCtrlVm
            } }
        [ObservableProperty]
        string projPath ;
        public static string projFilter = "工程文件|*.burrproj|工程文件|*.Json";
        public MainVM()
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
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

        #region 启停 状态
        /// <summary>
        /// 是否启动 后台使用此变量判断用户是否启动软件
        /// </summary>
        public bool isStart = false;
        /// <summary>
        /// 界面绑定变量，勿用此变量判断用户是否启动软件
        /// </summary>
        [ObservableProperty]
        bool startStop = false;

        [ObservableProperty]
        bool isLoading = false;
        #endregion

        #region 应用或丢弃当前工程变更
        /// <summary>
        /// 保存当前工程的修改
        /// </summary>
        public void ApplyChanges()=>this.Adapt(this._model);
        /// <summary>
        /// 丢弃当前工程的修改
        /// </summary>
        public void DiscardChanges()=>_model.Adapt(this);
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
                    SystemSettings = SysSet.LoadParameter();
                    if (SystemSettings != null)
                    {
                        SysLog.Info(Properties.Resources.SystemSettingsReadSuccess);
                        
                        //CLoading.DispText("读取系统配置成功...", 10);
                    }
                    else
                    {
                        SysLog.Error(Properties.Resources.SystemSettingsReadFailed);
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
        public async Task OpenProj(IProgress<double> progress,string header)
        {
            IsLoading = true;
            #region 打开工程
            try
            {
                ProjPath = header;
                Model = JsonConvert.DeserializeObject<MainModel>(File.ReadAllText(header)) ?? new MainModel();
                SystemSettings.RecentProjs.Remove(header);
                SystemSettings.RecentProjs.Insert(0, header);
                progress.Report(50);
                while(SystemSettings.RecentProjs.Count>10)
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


        
        public void SaveCurrentProj()
        {
            if (string.IsNullOrEmpty(ProjPath)) return;
            ApplyChanges();
            string json = JsonConvert.SerializeObject(Model);
            using (FileStream fs = new FileStream(ProjPath, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }
        }

        [ObservableProperty]
        private MaociAlgorParamCtrlVm maociAlgorParamCtrlVm = new MaociAlgorParamCtrlVm();

        [ObservableProperty]
        private SDFilterCtrlVM sDFilterCtrlVM = new SDFilterCtrlVM();

        [ObservableProperty]
        private QualityCtrlVM qualityCtrlVM = new QualityCtrlVM();
    }
}
