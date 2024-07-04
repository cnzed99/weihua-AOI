using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WH.Entity.LogRecord;


public class ClogSetting
{
    private const int c_SIZE_CONTENT = 10485760;
    private const int c_COUNT_BACK = 20;

    public string Name { get; set; }

    //public string FileName { get; set; }

    public string RootDir { get; set; }

    public string BranchDir { get; set; }

    public int MaxFileSize { get; set; }

    public int MaxBackFiles { get; set; }

    public ClogSetting()
        : this("Default", "DefaultLogs")
    {
    }

    public ClogSetting(string name, string branchDir)
    {
        this.RootDir = "D:\\Data\\Logs";
        this.MaxFileSize = 10485760;//10MB
        this.MaxBackFiles = 100;
        this.Name = name;
        this.BranchDir = branchDir;


    }
}

public static class ClogHepler
{
    private static ConcurrentDictionary<Guid, ClogHepler.PairLogItem> logPairDictionary;

    static ClogHepler()
    {
        BasicConfigurator.Configure();
        ClogHepler.logPairDictionary = new ConcurrentDictionary<Guid, ClogHepler.PairLogItem>();
    }
    /// <summary>
    /// 创建或获取LOG记录实例
    /// </summary>
    /// <param name="logSetting">配置文件</param>
    /// <returns>返回ILog</returns>
    public static ILog Create(this ClogSetting logSetting)
    {

        ILog logger1 = LogManager.GetLogger(logSetting.Name);
        Logger logger2 = logger1.Logger as Logger;
        if (logger2.Appenders.Count == 0)
        {
            logger2.Level = logger2.Hierarchy.LevelMap["ALL"];
            RollingFileAppender rollingFileAppender = new RollingFileAppender();
            rollingFileAppender.Name = logSetting.Name;
            rollingFileAppender.File = logSetting.RootDir + "/" + logSetting.BranchDir + "/";
            rollingFileAppender.AppendToFile = true;
            rollingFileAppender.StaticLogFileName = false;
            rollingFileAppender.MaxFileSize = (long)logSetting.MaxFileSize;
            rollingFileAppender.MaxSizeRollBackups = logSetting.MaxBackFiles;
            RollingFileAppender newAppender = rollingFileAppender;
            if (logSetting.MaxBackFiles > 0)
            {
                newAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
                newAppender.DatePattern = "yyyy-MM-dd\".txt\"";
            }
            else
            {
                newAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
                newAppender.DatePattern = "yyyy-MM-dd\".txt\"";
            }
            PatternLayout patternLayout = new PatternLayout()
            {
                ConversionPattern = "%d{yyyy-MM-dd HH:mm:ss.fff}\t[%thread]\t%level\t%logger\t-\t%message%newline"
            };
            patternLayout.ActivateOptions();
            newAppender.Layout = (ILayout)patternLayout;
            newAppender.Encoding = Encoding.UTF8;
            newAppender.ActivateOptions();
            logger2.AddAppender((IAppender)newAppender);
        }
        logSetting.DeleteLog();
        return logger1;
    }
    // <summary>
    /// 删除过期日志
    /// </summary>
    /// <param name="logSetting"></param>
    public static void DeleteLog(this ClogSetting logSetting)
    {

        DirectoryInfo dyInfo = new DirectoryInfo(Path.Combine(logSetting.RootDir, logSetting.BranchDir));
        //获取文件夹下所有的文件
        foreach (FileInfo feInfo in dyInfo.GetFiles())
        {
            //判断文件日期是否小于指定日期，是则删除
            if (feInfo.CreationTime < DateTime.Now.AddDays(-logSetting.MaxBackFiles))
                feInfo.Delete();
        }
    }
    /// <summary>
    /// 启用记录 
    /// </summary>
    /// <param name="logger">日志实例名称</param>
    /// <param name="message">描述</param>
    /// <param name="isSpanWithMillisecond">是否记录毫秒数</param>
    /// <param name="isDelayWrite">延迟写入日志</param>
    /// <returns></returns>
    public static Guid BeginInfo(this ILog logger, string message, bool isSpanWithMillisecond = true, bool isDelayWrite = false)
    {
        Guid key = Guid.NewGuid();
        ClogHepler.logPairDictionary.TryAdd(key, new ClogHepler.PairLogItem(message, isDelayWrite, isSpanWithMillisecond));
        if (!isDelayWrite)
            logger.Info((object)(message + "开始"));
        return key;
    }
    /// <summary>
    /// 结束日志
    /// </summary>
    /// <param name="logger">日志实例名称</param>
    /// <param name="guidKey">全局唯一标识</param>
    /// <param name="isSucceed">true:添加到info，false:添加到error</param>
    public static void EndInfo(this ILog logger, Guid guidKey, bool isSucceed)
    {
        if (!ClogHepler.logPairDictionary.ContainsKey(guidKey))
            return;
        double timeSpan = ClogHepler.logPairDictionary[guidKey].TimeSpan;
        ClogHepler.PairLogItem pairLogItem;
        ClogHepler.logPairDictionary.TryRemove(guidKey, out pairLogItem);
        string message;
        if (pairLogItem.IsDelayWrite)
            message = string.Format("{0}结束（Result:{1})，耗时 {2}{3}（开始时间：{4:yyyy-MM-dd HH:mm:ss.fff}）", (object)pairLogItem.Content, (object)isSucceed, (object)timeSpan, pairLogItem.IsSanWithMillisecond ? (object)"ms" : (object)"s", (object)pairLogItem.Start);
        else
            message = string.Format("{0}结束（Result:{1})，耗时 {2}{3}", (object)pairLogItem.Content, (object)isSucceed, (object)timeSpan, pairLogItem.IsSanWithMillisecond ? (object)"ms" : (object)"s");
        if (isSucceed)
            logger.Info((object)message);
        else
            logger.Error((object)message);
    }
    /// <summary>
    /// 结束 日志会写入开始到结束的时间
    /// </summary>
    /// <param name="logger">日志实例名称</param>
    /// <param name="guidKey">全局唯一标识</param>
    /// <param name="addMessage">附加信息</param>
    public static void EndInfo(this ILog logger, Guid guidKey, string addMessage)
    {
        if (!ClogHepler.logPairDictionary.ContainsKey(guidKey))
            return;
        double timeSpan = ClogHepler.logPairDictionary[guidKey].TimeSpan;
        ClogHepler.PairLogItem pairLogItem;
        ClogHepler.logPairDictionary.TryRemove(guidKey, out pairLogItem);
        string message;
        if (pairLogItem.IsDelayWrite)
            message = string.Format("{0}结束（{1})，耗时 {2}{3}（开始时间：{4:yyyy-MM-dd HH:mm:ss.fff}）", (object)pairLogItem.Content, (object)addMessage, (object)timeSpan, pairLogItem.IsSanWithMillisecond ? (object)"ms" : (object)"s", (object)pairLogItem.Start);
        else
            message = string.Format("{0}结束（{1})，耗时 {2}{3}", (object)pairLogItem.Content, (object)addMessage, (object)timeSpan, pairLogItem.IsSanWithMillisecond ? (object)"ms" : (object)"s");
        logger.Info((object)message);
    }
    private class PairLogItem
    {
        /// <summary>
        /// 启用时间
        /// </summary>
        public DateTime Start { get; private set; }

        public string Content { get; private set; }

        public bool IsDelayWrite { get; private set; }

        public bool IsSanWithMillisecond { get; private set; }
        /// <summary>
        /// 启用后时长
        /// </summary>
        public double TimeSpan
        {
            get
            {
                TimeSpan timeSpan = DateTime.Now - this.Start;
                return this.IsSanWithMillisecond ? timeSpan.TotalMilliseconds : timeSpan.TotalSeconds;
            }
        }

        public PairLogItem(string content, bool isDelayWrite, bool isSpanWithMillisecond)
        {
            this.Start = DateTime.Now;
            this.Content = content;
            this.IsDelayWrite = isDelayWrite;
            this.IsSanWithMillisecond = isSpanWithMillisecond;
        }
    }
}

