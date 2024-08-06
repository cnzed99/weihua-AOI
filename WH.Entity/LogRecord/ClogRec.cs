using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using log4net;

namespace WH.Entity.LogRecord
{
    public enum LOG
    {
        LOG_INFO = 1,
        LOG_WARN = 2,
        LOG_ERROR = 4,
        LOG_OK = 8,
        LOG_NG = 16,
        LOG_TIP = 32
    }

    /// <summary>
    /// 20240704 TCG
    /// Log文本操作 不显示至控件
    /// </summary>
    public partial class CLogRec : ObservableObject
    {
        /// <summary>
        /// 默认日志
        /// </summary>
        public static CLogRec Default = new CLogRec("Info", "./Log", "Error");
        static Hashtable logList = new Hashtable();

        static CLogRec()
        {
            logList.Add("Info", Default);
        }

        public string UserName { get; set; }
        SlogMessage infoMessage;
        public SlogMessage InfoMessage
        {
            get => infoMessage;
            set
            {
                infoMessage = value;
                OnPropertyChanged();
            }
        }

        SlogMessage errorMessage;
        public SlogMessage ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        SlogMessage allMessage;
        public SlogMessage AllMessage
        {
            get => allMessage;
            set
            {
                allMessage = value;
                OnPropertyChanged();
            }
        }

        private log4net.ILog infoLog;
        private log4net.ILog errorLog;

        /// <summary>
        /// 未指定errorLog时，共用同一个日志文件,指定后会将错误信息写入单独文件
        /// </summary>
        /// <param name="logName">日志名</param>
        /// <param name="pathDir">日志保存路径</param>
        /// <param name="errorLog">错误日志名</param>
        private CLogRec(string logName, string pathDir, string errorLog = null)
        {
            infoLog = new ClogSetting(logName, logName) { RootDir = pathDir }.Create();
            if (string.IsNullOrEmpty(errorLog))
                this.errorLog = infoLog;
            else
            {
                this.errorLog = new ClogSetting(errorLog, errorLog) { RootDir = pathDir }.Create();
            }
        }

        public static CLogRec Create(string logName, string pathDir, string errorLog = null)
        {
            if (logList.ContainsKey(logName))
                return (CLogRec)logList[logName];
            else
            {
                var log = new CLogRec(logName, pathDir, errorLog);
                logList.Add(logName, log);
                return log;
            }
        }

        void UpdateMessage(string msg, LOG logType)
        {
            var message = DateTime.Now.ToString("yyyy/M/d HH:mm:ss:fff") + " " + msg;
            var logInfo = new SlogMessage() { Message = message, Log = logType };
            if (logType == LOG.LOG_ERROR)
            {
                ErrorMessage = logInfo;
                Default.ErrorMessage = logInfo; //错误消息打印到默认日志
            }
            else
            {
                InfoMessage = logInfo;
            }

            AllMessage = logInfo;
        }

        public void Info(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();
            infoLog.Info(msg);
            UpdateMessage(msg, LOG.LOG_INFO);
        }

        public void Warn(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();
            infoLog.Info(msg);
            UpdateMessage(msg, LOG.LOG_WARN);
        }

        public void Error(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();

            errorLog.Error(msg);
            UpdateMessage(msg, LOG.LOG_ERROR);
        }

        public void OK(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();
            infoLog.Info(msg);
            UpdateMessage(msg, LOG.LOG_OK);
        }

        public void NG(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();
            infoLog.Info(msg);
            UpdateMessage(msg, LOG.LOG_NG);
        }

        public void Tip(string message)
        {
            StringBuilder sb = new StringBuilder(UserName);
            sb.Append(":");
            sb.Append(message);
            string msg = sb.ToString();
            infoLog.Info(msg);
            UpdateMessage(msg, LOG.LOG_TIP);
        }
    }

    /// <summary>
    /// 20240704 TCG
    /// 日志消息封装
    /// </summary>
    public struct SlogMessage : IEquatable<SlogMessage>
    {
        public LOG Log;
        public string Message;

        public bool Equals(SlogMessage other)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return DateTime.Now.Millisecond;
        }
    }
}
