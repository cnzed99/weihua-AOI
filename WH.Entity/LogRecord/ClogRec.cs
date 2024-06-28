using CommunityToolkit.Mvvm.ComponentModel;
using log4net;
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
using WH.Entity.LogRecord;

namespace WH.Entity.LogRecord
{
    public enum LOG
    {
        LOG_INFO,
        LOG_WARN,
        LOG_ERROR,
        LOG_OK,
        LOG_NG,
        LOG_TIP
    }

    /// <summary>
    /// Log文本操作 不显示至控件
    /// </summary>
    public partial class CLogRec:ObservableObject
    {
        static Hashtable s_logList = new Hashtable();
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
            get=> errorMessage;
            set
            {
                errorMessage = value; OnPropertyChanged();
            }
        }
        
        SlogMessage allMessage;
        public SlogMessage AllMessage
        {
            get => allMessage;
            set
            {
                allMessage = value; OnPropertyChanged();
            }
        }

        private  log4net.ILog _InfoLog ;
        private  log4net.ILog _ErrorLog ;
        /// <summary>
        /// 未指定errorLog时，共用同一个日志文件,指定后会将错误信息写入单独文件
        /// </summary>
        /// <param name="logName">日志名</param>
        /// <param name="pathDir">日志保存路径</param>
        /// <param name="errorLog">错误日志名</param>
        private CLogRec(string logName,string pathDir,string errorLog = null)
        {

            _InfoLog = new ClogSetting(logName,logName) { RootDir = pathDir }.Create();
            if (string.IsNullOrEmpty(errorLog)) _ErrorLog = _InfoLog;
            else _ErrorLog = new ClogSetting(errorLog, errorLog) { RootDir = pathDir }.Create();
        }
        public static CLogRec Create(string logName, string pathDir, string errorLog = null)
        {
            if(s_logList.ContainsKey(logName)) return (CLogRec)s_logList[logName];
            else
            {
                var log = new CLogRec(logName,pathDir,errorLog);
                s_logList.Add(logName,log);
                return log;
            }
        }
        void UpdateMessage(string msg,LOG logType)
        {
            var logInfo = new SlogMessage() { Message = msg, Log = logType };
            if (logType == LOG.LOG_ERROR)
            {
                ErrorMessage = logInfo;
            }
            else
            {
                InfoMessage = logInfo;
            }
           
            AllMessage = logInfo;
        }
        public  void Info(string message)
        {
            _InfoLog.Info(message);
            UpdateMessage(message,LOG.LOG_INFO);
        }
        public  void Warn(string message)
        {
            _InfoLog.Info(message);
            UpdateMessage(message,LOG.LOG_WARN);
        }
        public  void Error(string message)
        {
            _ErrorLog.Error(message);
            UpdateMessage(message, LOG.LOG_ERROR);
        }

        public void OK(string message)
        {
            _InfoLog.Info(message);
            UpdateMessage(message, LOG.LOG_OK);
        }
        public void NG(string message)
        {
            _InfoLog.Info(message);
            UpdateMessage(message, LOG.LOG_NG);
        }
        public void Tip(string message)
        {
            _InfoLog.Info(message);
            UpdateMessage(message, LOG.LOG_TIP);
        }


    }
    public struct SlogMessage
    {
        public LOG Log;
        public string Message;
    }
   
}
