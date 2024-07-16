using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using log4net;
using WH.Entity.LogRecord;

namespace WH.Controls
{
    /// <summary>
    /// LogCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class LogCtrl : UserControl
    {
        public SlogMessage LogMessage
        {
            get { return (SlogMessage)GetValue(LogMessageProperty); }
            set { SetValue(LogMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogMessageProperty = DependencyProperty.Register(
            "LogMessage",
            typeof(SlogMessage),
            typeof(LogCtrl),
            new PropertyMetadata(LogMessageChanged)
        );

        private static void LogMessageChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            LogCtrl ctrl = (LogCtrl)d;
            SlogMessage logMessage = (SlogMessage)e.NewValue;
            ctrl.AddLogToListBox(logMessage.Message, logMessage.Log);
        }

        public LogCtrl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 将文本添加到目标ListBox控件和日志文件
        /// </summary>
        /// <param name="TargetListBox">目标ListBox控件</param>
        /// <param name="TargetText">需要添加的文本内容</param>
        /// <returns>执行结果</returns>
        public bool AddLogToListBox(string targetText, LOG logType = LOG.LOG_INFO)
        {
            if (string.IsNullOrEmpty(targetText))
            {
                return false;
            }

            targetText = targetText.Replace("HALCON", "CCD").Replace("Halcon", "ccd");

            try
            {
                Brush textColor = Brushes.Black;
                switch (logType)
                {
                    case LOG.LOG_ERROR:

                        textColor = Brushes.Red;
                        break;
                    case LOG.LOG_WARN:

                        textColor = Brushes.CadetBlue;
                        break;
                    case LOG.LOG_OK:

                        textColor = Brushes.LimeGreen;
                        break;
                    case LOG.LOG_NG:

                        textColor = Brushes.IndianRed;
                        break;
                    case LOG.LOG_INFO:

                        textColor = Brushes.DarkGray;
                        break;
                    case LOG.LOG_TIP:

                        textColor = Brushes.DeepPink;
                        break;
                    default:

                        textColor = Brushes.DarkGray;
                        break;
                }
                this.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        int count = LogBox.Document.Blocks.Count;
                        if (count > 150)
                        {
                            LogBox.Document.Blocks.Clear();
                        }

                        Run item = new Run(targetText);
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(item);
                        paragraph.Margin = new Thickness(0, 5, 0, 0);
                        paragraph.LineHeight = 1;
                        paragraph.Foreground = textColor;
                        LogBox.Document.Blocks.Add(paragraph);
                        LogBox.ScrollToEnd();
                    })
                );
                //TargetListBox.Focus();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            //}
        }
    }
}
