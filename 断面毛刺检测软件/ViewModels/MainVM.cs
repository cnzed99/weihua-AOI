using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// 系统配置
        /// </summary>
        public SystemSettingsModel SystemSettings { get; set; } = new SystemSettingsModel();
        /// <summary>
        /// 运行日志和报警日志
        /// </summary>
        public CLogRec SysLog { get; set; } = new CLogRec("Info","./Log","Error");
        public CLogRec OperateLog { get; set; } = new CLogRec("Operate", "D:/Data");
        private MainModel _model = new MainModel();
        /// <summary>
        /// 当前工程 禁止直接修改其属性
        /// </summary>
        public MainModel Model { get => _model; set{
                SetProperty(ref _model, value);
                _model.Adapt(this);
            } }
        public MainVM()
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        [ObservableProperty]
        static string systemTime;

        static DateTime StartTime = DateTime.Now;
        [ObservableProperty]
        static string runingTime = DateTime.Now.ToString("T");
      
        /// <summary>
        /// 是否启动 后台使用此变量判断用户是否启动软件
        /// </summary>
        public bool isStart = false;
        /// <summary>
        /// 界面绑定变量，勿用此变量判断用户是否启动软件
        /// </summary>
        [ObservableProperty]
        bool startStop = false;
        private void Timer_Tick(object sender, EventArgs e)
        {
            SystemTime = DateTime.Now.ToString("yyyy-MM-dd\r\nHH:mm:ss");
            var runTimeSpan = DateTime.Now - StartTime;
            RuningTime = runTimeSpan.ToString(@"hh\:mm\:ss");
           
        }
        
        /// <summary>
        /// 保存当前工程的修改
        /// </summary>
        public void ApplyChanges()=>this.Adapt(this._model);
        /// <summary>
        /// 丢弃当前工程的修改
        /// </summary>
        public void DiscardChanges()=>_model.Adapt(this);
       
    }
}
