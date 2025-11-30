using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using WH.Entity.Attribute;
using Newtonsoft.Json;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 系统设置 模型
    /// </summary>
    public partial class CSystemSettingsModel : ObservableValidator
    {
        public CSystemSettingsModel()
        {
            //oldtime = NextClearTime;
            //nextClearTime = oldtime;
            clearTimer = new DispatcherTimer(DispatcherPriority.Normal);
            clearTimer.Interval = TimeSpan.FromSeconds(1);
            clearTimer.Tick += Timer_Tick;
            clearTimer.Start();
        }

        /// <summary>
        /// 清零计时器
        /// </summary>
        DispatcherTimer clearTimer;

        /// <summary>
        /// 20240801 TCG
        /// 最近打开的工程
        /// </summary>
        public ObservableCollection<string> RecentProjs { get; set; } =
            new ObservableCollection<string>()
            {
                "C:\\Users\\Mainvm.Json",
                "C:\\Users\\Mainvm233.Json"
            };

        /// <summary>
        /// 20240801 TCG
        /// 中英文切换
        /// </summary>
        [ObservableProperty]
        bool isEnglish = false;

        partial void OnIsEnglishChanged(bool value)
        {
            var languageCode = "zh-CN";
            if (value)
            {
                languageCode = "en-US";
            }
            ConfigHelper.Instance.SetLang(languageCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
            LanguageManager.CLanguageManager.ChangeLanguage(new CultureInfo(languageCode));
            //OperateLog.Info(Properties.Resources.LanguageChanged + languageCode);
        }

        /// <summary>
        /// 20240801 TCG
        /// 离线调试下是否存图和数据库
        /// </summary>
        [ObservableProperty]
        bool offlineSave = false;

        //[ObservableProperty]
        //bool isToMysql = false;

        /// <summary>
        /// 2024.7.26 李焕彬
        /// 是否显示所有缺陷
        /// </summary>
        [ObservableProperty]
        bool showAllDefect = false;

        /// <summary>
        /// 2024.7.26 李焕彬
        /// 是否显示额外的区域
        /// </summary>
        [ObservableProperty]
        bool showDrawEdges = false;
        

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 显示帧率
        /// </summary>
        [ObservableProperty]
        int displayFrameRate = 10;

        #region 数据清零参数
        /// <summary>
        /// 显示错误信息
        /// </summary>
        [ObservableProperty]
        private string errorMsg = "";

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
        [DateTimeLessThan(nameof(NightShift))]
        public DateTime DayShift
        {
            get => _dayShift;
            set
            {
                var validationContext = new ValidationContext(this)
                {
                    MemberName = nameof(DayShift)
                };
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateProperty(
                    value,
                    validationContext,
                    validationResults
                );

                if (isValid)
                {
                    ErrorMsg = "";
                    SetProperty(ref _dayShift, value, true);
                    OnPropertyChanged(nameof(NowShift));
                    GetNextClearTime();
                    OnPropertyChanged(nameof(NextClearTime));
                }
                else
                {
                    ErrorMsg = validationResults[0].ErrorMessage;
                    //Validator.ValidateProperty(value, validationContext);
                    // OnPropertyChanged(nameof(DayShift),true);
                    ////SetProperty(ref _dayShift, value, false);
                }
            }
        }

        private DateTime _nightShift = DateTime.Parse("20:00");

        /// <summary>
        /// 晚班时间
        /// </summary>
        [DateTimeGreaterThan(nameof(DayShift))]
        public DateTime NightShift
        {
            get => _nightShift;
            set
            {
                var validationContext = new ValidationContext(this)
                {
                    MemberName = nameof(NightShift)
                };
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateProperty(
                    value,
                    validationContext,
                    validationResults
                );

                if (isValid)
                {
                    ErrorMsg = "";
                    SetProperty(ref _nightShift, value, true);
                    OnPropertyChanged(nameof(NowShift));
                    GetNextClearTime();
                    OnPropertyChanged(nameof(NextClearTime));
                }
                else
                {
                    ErrorMsg = validationResults[0].ErrorMessage;
                }
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
                int dayshiftSecond =
                    _dayShift.Hour * 3600 + _dayShift.Minute * 60 + _dayShift.Second;
                int nightshiftSecond =
                    _nightShift.Hour * 3600 + _nightShift.Minute * 60 + _nightShift.Second;

                switch (ClearEveryDay)
                {
                    case 1: //每班次
                        if (nowSecond >= dayshiftSecond && nowSecond < nightshiftSecond) //判断当前是白班
                        {
                            // nowShift = now.ToLongDateString() + " 白班";

                            nowShift = string.Format("{0}-白班", now.ToLongDateString());
                        }
                        else
                        {
                            if (nowSecond >= 0 && nowSecond < nightshiftSecond) //如果是0点以后到DayShift这段时间  则班次是前一天的晚班
                            {
                                // nowShift = now.AddDays(-1).ToLongDateString() + " 晚班";

                                nowShift = string.Format(
                                    "{0}-晚班",
                                    now.AddDays(-1).ToLongDateString()
                                );
                            }
                            else //否则是当天的晚班
                            {
                                //nowShift = now.ToLongDateString() + " 晚班";
                                nowShift = string.Format("{0}-晚班", now.ToLongDateString());
                            }
                        }
                        break;
                    case 2: //每天
                        if (nowSecond >= dayshiftSecond && nowSecond < nightshiftSecond) //白班晚班数据放在同一天的数据库表
                        {
                            // nowShift = now.ToLongDateString() + " 白班+晚班";
                            nowShift = string.Format("{0}-白班-晚班", now.ToLongDateString());
                        }
                        else //晚班
                        {
                            if (nowSecond > 0 && nowSecond < nightshiftSecond) //如果是0点以后到DayShift这段时间  则班次是前一天的晚班
                            {
                                // nowShift = now.AddDays(-1).ToLongDateString() + " 白班+晚班";

                                nowShift = string.Format(
                                    "{0}-白班-晚班",
                                    now.AddDays(-1).ToLongDateString()
                                );
                            }
                            else //否则是当天的晚班
                            {
                                //nowShift = now.ToLongDateString() + " 白班+晚班";
                                nowShift = string.Format("{0}-白班-晚班", now.ToLongDateString());
                            }
                        }
                        break;
                    case 3:
                        int day = (int)now.DayOfWeek;
                        if (day == 1 && nowSecond < nightshiftSecond) //如果是周一 并且是夜班时间段  则现在是上一周的时间段
                        {
                            int week = WeekOfYear(now, new CultureInfo("zh-CN"));
                            nowShift = now.Year + "年-第" + (week - 1).ToString() + "周";
                        }
                        else
                        {
                            int week = WeekOfYear(now, new CultureInfo("zh-CN"));
                            nowShift = now.Year + "年-第" + (week).ToString() + "周";
                        }

                        break;
                }

                return nowShift;
            }
            set => nowShift = value;
        }

        private int WeekOfYear(DateTime dt, CultureInfo ci)
        {
            return ci.Calendar.GetWeekOfYear(
                dt,
                ci.DateTimeFormat.CalendarWeekRule,
                ci.DateTimeFormat.FirstDayOfWeek
            );
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now > NextClearTime&& Loaded)
            {
                //oldtime = NextClearTime;
                ClearProduceEvent?.Invoke(); 
                GetNextClearTime();
            }
        }

        #endregion

        #region 获取下一次的清零时间

        /// <summary>
        /// 数据清零事件
        /// </summary>
        public event Action ClearProduceEvent;
        /// <summary>
        /// 软件初始化完成
        /// </summary>
        [JsonIgnore]
        public bool Loaded=false;

        //static DateTime oldtime;

        /// <summary>
        /// 下次清零时间
        /// </summary>
        private DateTime nextClearTime;

        public DateTime NextClearTime
        {
            get { return nextClearTime; }
            set { SetProperty(ref nextClearTime, value); }
        }

        /// <summary>
        /// 获取下次清零事件
        /// </summary>
        private void GetNextClearTime()
        {
            DateTime now = DateTime.Now;
            DateTime next = new DateTime();
            switch (ClearEveryDay)
            {
                case 1: //每班次清零
                    if (NowShift.Contains("白班")) //判断当前是白班
                    {
                        next = new DateTime(
                            now.Year,
                            now.Month,
                            now.Day,
                            NightShift.Hour,
                            NightShift.Minute,
                            NightShift.Second
                        ); //如果现在是白班,下一次清零时间就是当天的晚班开始时间
                    }
                    else //判断当前是晚班
                    {
                        next = new DateTime(
                            now.Year,
                            now.Month,
                            now.Day,
                            DayShift.Hour,
                            DayShift.Minute,
                            DayShift.Second
                        ); //如果现在是晚班,下一次清零时间就是第二天的白班开始时间

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
                    next = new DateTime(
                        now.Year,
                        now.Month,
                        now.Day,
                        DayShift.Hour,
                        DayShift.Minute,
                        DayShift.Second
                    ); //下一次清零时间就是第二天的白班开始时间
                    next = next.AddDays(1);
                    break;

                case 3: //每周一清零
                    next = new DateTime(
                        now.Year,
                        now.Month,
                        now.Day,
                        DayShift.Hour,
                        DayShift.Minute,
                        DayShift.Second
                    );
                    int weekDay = (int)next.DayOfWeek;
                    DateTime minDay = next.AddDays(0 - weekDay); // 第0天 周日
                    next = minDay.AddDays(8); //下个周一
                    break;
            }
            NextClearTime = next;
        }

        #endregion
    }
}
