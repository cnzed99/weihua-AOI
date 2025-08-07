
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public  class ProgressBarViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 识别完成事件
        /// </summary>
        public static  Action  ProgressFinshEven;

        // 单例实例
        private static ProgressBarViewModel _instance;
        public static ProgressBarViewModel Instance => _instance ??= new ProgressBarViewModel();

        private ProgressBarViewModel() { }

        private static string autoMessage="";
        /// <summary>
        /// 自动识别时的信息提示
        /// </summary>
        public static string AutoMessage
        {
            get { return autoMessage; }
            set 
            { 
                autoMessage = value;
                Instance.OnPropertyChanged(nameof(AutoMessageText));
            }
        }

        private static double progressBarValue=20;
        /// <summary>
        /// 进度条值
        /// </summary>
        public static double ProgressBarValue
        {
            get { return progressBarValue; }
            set { 
                progressBarValue = value;
                Instance.OnPropertyChanged(nameof(ProgressBarValueText));
            }
        }

        // 实例属性用于绑定
        public string AutoMessageText => AutoMessage;

        public double ProgressBarValueText => ProgressBarValue;

        // INotifyPropertyChanged实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


       
    }



}
