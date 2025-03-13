using System.Threading.Channels;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Controls;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace Motion
{
    /// <summary>
    /// 2025.3.6 李焕彬
    /// 控制控件VM
    /// </summary>
    public partial class CMotionVMBase : ObservableObject
    {
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 显示控件
        /// </summary>
        [ObservableProperty]
        UserControl testControl;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public virtual void SetRunning(bool isRuning)
        {
            this.IsRuning = isRuning;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 当前制程是否启动
        /// </summary>
        [ObservableProperty]
        private bool isRuning = false;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CMotionConfigBase config;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 报警信息
        /// </summary>
        [ObservableProperty]
        private string infoAlarm;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 运行日志
        /// </summary>
        public static CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 初始化控制
        /// </summary>
        public virtual void InitControl() { }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 复位
        /// </summary>
        public virtual void Reset() { }
    }
}
