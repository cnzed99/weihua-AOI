using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using log4net.Util;
using WH.Entity.LogRecord;

namespace WH.Controls.Controls.Attach
{
    public static class ClogRecElement
    {
        public static SlogMessage GetLogMessage(DependencyObject obj)
        {
            return (SlogMessage)obj.GetValue(LogMessageProperty);
        }

        public static void SetLogMessage(DependencyObject obj, SlogMessage value)
        {
            obj.SetValue(LogMessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for LogMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogMessageProperty =
            DependencyProperty.RegisterAttached(
                "LogMessage",
                typeof(SlogMessage),
                typeof(ClogRecElement),
                new PropertyMetadata(LogMessageChanged)
            );

        private static void LogMessageChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is RichTextBox ctrl)
            {
                SlogMessage logMessage = (SlogMessage)e.NewValue;
                ctrl.AddLogToListBox(logMessage.Message, logMessage.Log);
            }
        }

        /// <summary>
        /// 将文本添加到目标ListBox控件和日志文件
        /// </summary>
        /// <param name="TargetListBox">目标ListBox控件</param>
        /// <param name="TargetText">需要添加的文本内容</param>
        /// <returns>执行结果</returns>
        public static bool AddLogToListBox(
            this RichTextBox ctrl,
            string targetText,
            LOG logType = LOG.LOG_INFO
        )
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
                ctrl.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        int count = ctrl.Document.Blocks.Count;
                        if (count > 150)
                        {
                            ctrl.Document.Blocks.Clear();
                        }

                        Run item = new Run(targetText);
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(item);
                        paragraph.Margin = new Thickness(0, 5, 0, 0);
                        paragraph.LineHeight = 1;
                        paragraph.Foreground = textColor;
                        ctrl.Document.Blocks.Add(paragraph);
                        ctrl.ScrollToEnd();
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
