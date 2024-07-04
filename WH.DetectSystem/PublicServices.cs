using Autofac;
using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.DetectSystem.ViewModels;
using WH.Entity.LogRecord;

namespace WH.DetectSystem
{
    public class PublicServices
    {
        public static IContainer Container { get; set; }
        public static ContainerBuilder ConfigureServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(CLogRec.Create("Info", "./Log", "Error")).Keyed<CLogRec>(LOGTYPE.LOGTYPE_SYS).SingleInstance();
            builder.RegisterInstance(CLogRec.Create("Operate", "D:/Data")).Keyed<CLogRec>(LOGTYPE.LOGTYPE_OPERATE).SingleInstance();
            builder.RegisterType<MainVM>().SingleInstance();
            return builder;
        }
    }
    public enum LOGTYPE
    {
        LOGTYPE_SYS,
        LOGTYPE_OPERATE
    }
}
