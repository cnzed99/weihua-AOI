using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using WH.Controls;
using WH.Entity;
using WH.Entity.Attribute;

namespace 断面毛刺检测软件.Models
{
    public partial class SystemSettingsModel:ObservableValidator
    {
        /// <summary>
        /// 系统参数保存的路径
        /// </summary>
        public static string ParameterPath =  "..\\SystemConfig\\SystemSetting.Json";
        public static SystemSettingsModel SystemSetParam = new SystemSettingsModel();
        public ObservableCollection<string> RecentProjs { get; set; } = new ObservableCollection<string>() { "C:\\Users\\Mainvm.Json", "C:\\Users\\Mainvm233.Json" };

        [ObservableProperty]
        bool isEnglish = false;

        [ObservableProperty]
        bool offlineSave = false;
        [ObservableProperty]
        bool isToMysql = false;

        #region 数据清零参数
        /// <summary>
        /// 自动清零使能
        /// </summary>
        [ObservableProperty]
        private bool autoClearEnable = true;

        /// <summary>
        /// 选择清零时间间隔
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NowShift))]
        [NotifyPropertyChangedFor(nameof(NextClearTime))]
        private int clearEveryDay = 1;
       
        private DateTime _dayShift = DateTime.Parse("8:00");
        /// <summary>
        /// 白班时间
        /// </summary>
        [LessThan(nameof(NightShift))]
        public DateTime DayShift
        {
            get => _dayShift;
            set
            {
                SetProperty(ref _dayShift, value,true);
                OnPropertyChanged(nameof(NowShift));
                OnPropertyChanged(nameof(NextClearTime));
            }
        }


        private DateTime _nightShift = DateTime.Parse("20:00");
        /// <summary>
        /// 晚班时间
        /// </summary>
        [GreaterThan(nameof(DayShift))]
        public DateTime NightShift
        {
            get => _nightShift;
            set
            { 
                SetProperty(ref _nightShift, value, true);
                OnPropertyChanged(nameof(NowShift));
                OnPropertyChanged(nameof(NextClearTime));
            }
        }


        private string nowShift = "白班";
        /// <summary>
        /// 当前班次
        /// </summary>
        public string NowShift
        {
            get
            {

                DateTime now = DateTime.Now;
                int nowSecond = now.Hour * 3600 + now.Minute * 60 + now.Second;
                int dayshiftSecond = _dayShift.Hour * 3600 + _dayShift.Minute * 60 + _dayShift.Second;
                int nightshiftSecond = _nightShift.Hour * 3600 + _nightShift.Minute * 60 + _nightShift.Second;

                switch (ClearEveryDay)
                {
                    case 1: //每班次
                        if (nowSecond >= dayshiftSecond && nowSecond < nightshiftSecond) //判断当前是白班
                        {

                            // nowShift = now.ToLongDateString() + " 白班";

                            nowShift = string.Format("{0} 白班", now.ToLongDateString());
                        }
                        else
                        {
                            if (nowSecond >= 0 && nowSecond < nightshiftSecond)//如果是0点以后到DayShift这段时间  则班次是前一天的晚班
                            {
                                // nowShift = now.AddDays(-1).ToLongDateString() + " 晚班";

                                nowShift = string.Format("{0} 晚班", now.AddDays(-1).ToLongDateString());
                            }
                            else //否则是当天的晚班
                            {
                                //nowShift = now.ToLongDateString() + " 晚班";
                                nowShift = string.Format("{0} 晚班", now.ToLongDateString());
                            }
                        }
                        break;
                    case 2: //每天 
                        if (nowSecond >= dayshiftSecond && nowSecond < nightshiftSecond) //白班晚班数据放在同一天的数据库表
                        {

                            // nowShift = now.ToLongDateString() + " 白班+晚班";
                            nowShift = string.Format("{0} 白班+晚班", now.ToLongDateString());
                        }
                        else //晚班
                        {
                            if (nowSecond > 0 && nowSecond < nightshiftSecond)//如果是0点以后到DayShift这段时间  则班次是前一天的晚班
                            {
                                // nowShift = now.AddDays(-1).ToLongDateString() + " 白班+晚班";

                                nowShift = string.Format("{0} 白班+晚班", now.AddDays(-1).ToLongDateString());
                            }
                            else //否则是当天的晚班
                            {
                                //nowShift = now.ToLongDateString() + " 白班+晚班";
                                nowShift = string.Format("{0} 白班+晚班", now.ToLongDateString());
                            }

                        }
                        break;
                    case 3:
                        int day = (int)now.DayOfWeek;
                        if (day == 1 && nowSecond < nightshiftSecond) //如果是周一 并且是夜班时间段  则现在是上一周的时间段
                        {
                            int week = WeekOfYear(now, new CultureInfo("zh-CN"));
                            nowShift = now.Year + "年 第" + (week - 1).ToString() + "周";
                        }
                        else
                        {
                            int week = WeekOfYear(now, new CultureInfo("zh-CN"));
                            nowShift = now.Year + "年 第" + (week).ToString() + "周";
                        }

                        break;
                }

                return nowShift;
            }
            set => nowShift = value;
        }



        private int WeekOfYear(DateTime dt, CultureInfo ci)
        {
            return ci.Calendar.GetWeekOfYear(dt, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
        }

        /// <summary>
        /// 下次清零时间
        /// </summary>
        private DateTime nextClearTime;
      
        #endregion

        #region 获取下一次的清零时间
        public DateTime NextClearTime
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime next = new DateTime();
                switch (ClearEveryDay)
                {
                    case 1: //每班次清零
                        if (NowShift.Contains("白班")) //判断当前是白班
                        {
                            next = new DateTime(now.Year, now.Month, now.Day, NightShift.Hour, NightShift.Minute, NightShift.Second);//如果现在是白班,下一次清零时间就是当天的晚班开始时间
                            
                        }
                        else //判断当前是晚班
                        {

                            next = new DateTime(now.Year, now.Month, now.Day, DayShift.Hour, DayShift.Minute, DayShift.Second);//如果现在是晚班,下一次清零时间就是第二天的白班开始时间

                            int nowTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

                            int Nighttime = NightShift.Hour * 60 + NightShift.Minute;

                            if (nowTime < Nighttime)
                            {
                               //10:20   10:31
                            }
                            else
                            {
                                next = next.AddDays(1);
                            }
                        }
                        break;
                    case 2:
                        next = new DateTime(now.Year, now.Month, now.Day, DayShift.Hour, DayShift.Minute, DayShift.Second);//下一次清零时间就是第二天的白班开始时间
                        next = next.AddDays(1);
                        break;

                    case 3: //每周一清零
                        next = new DateTime(now.Year, now.Month, now.Day, DayShift.Hour, DayShift.Minute, DayShift.Second);
                        int weekDay = (int)next.DayOfWeek;
                        DateTime minDay = next.AddDays(0 - weekDay);// 第0天 周日
                        next = minDay.AddDays(8); //下个周一
                        break;
                }

                return next;
            }
           set=>SetProperty(ref nextClearTime, value);
        }
        #endregion


        #region 保存参数

        public static void SaveParameter()
        {
            try
            {
                ConfigAPI.Save(SystemSetParam, ParameterPath);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region 读取参数

        public static void LoadParameter()
        {

            try
            {
                if (File.Exists(ParameterPath))
                {
                    SystemSetParam = ConfigAPI.Load<SystemSettingsModel>(ParameterPath);
                    if (SystemSetParam == null)
                    {
                        SystemSetParam = new SystemSettingsModel();
                    }
                }
                else
                {
                    SystemSetParam = new SystemSettingsModel();
                }
            }
            catch (Exception)
            {
                SystemSetParam = new SystemSettingsModel();
            }
            
        }

        #endregion

        [RelayCommand]
        private void Close(System.ComponentModel.CancelEventArgs e)
        {
            if (HasErrors)
            {

                e.Cancel = true;
            }
        }
    }
   
}
