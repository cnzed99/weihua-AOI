using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using HandyControl.Interactivity;
using WH.DetectSystem.ViewModels;
using WH.Entity.LogRecord;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// IOC容器 管理各种实例 类型
    /// </summary>
    public class CPublicServices
    {
        public static IContainer Container { get; set; }

        public static ContainerBuilder ConfigureServices()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterInstance(CLogRec.Default)
                .Keyed<CLogRec>(LOGTYPE.LOGTYPE_SYS)
                .SingleInstance();
            builder
                .RegisterInstance(CLogRec.Create("Operate", "D:/Data"))
                .Keyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE)
                .SingleInstance();

            return builder;
        }
    }

    /// <summary>
    /// 20240704 TCG
    /// 日志类型，目前两类 系统日志和操作日志
    /// </summary>
    public enum LOGTYPE
    {
        LOGTYPE_SYS = 1,
        LOGTYPE_OPERATE = 2
    }
}
