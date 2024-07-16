using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlOperatesApiWPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataQuery
{
    public partial class CDataQueryVM:ObservableObject
    {
        MySqlViewModel cMysqlBLL;
        public CDataQueryVM()
        {
             cMysqlBLL = new MySqlViewModel();
        }

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询方式
        /// </summary>
        [ObservableProperty]
        int selectQueryMode=0;
        /// <summary>
        /// 024.7.7 鲍赞宝
        /// 当前班次
        /// </summary>
        [ObservableProperty]
        string nowShift;

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询起始日期
        /// </summary>
        [ObservableProperty]
        DateTime startDate = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询结束日期
        /// </summary>
        [ObservableProperty]
        DateTime endDate = DateTime.Now;

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询起始时间
        /// </summary>
        [ObservableProperty]
        DateTime startTime= DateTime.Parse("8:00");

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询结束时间
        /// </summary>
        [ObservableProperty]
        DateTime endTime= DateTime.Parse("20:00");

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询表
        /// </summary>
        [ObservableProperty]
        DataView dataViews;
        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 提示信息
        /// </summary>
        [ObservableProperty]
        string messageText;

        /// <summary>
        /// 查询
        /// </summary>
        [RelayCommand]
        void Search()
        {
            try
            {
                List<string> dates = new List<string>();

                DateTime newStarTime = StartDate.Date + StartTime.TimeOfDay;
                DateTime endStarTime = EndDate.Date + EndTime.TimeOfDay;
                string strStartTime = newStarTime.ToString("yyyy-MM-dd HH:mm:ss");
                string strEndTime = endStarTime.ToString("yyyy-MM-dd HH:mm:ss");
                switch (SelectQueryMode)
                {
                    case 0:
                        dates.Add(NowShift);
                        break;
                    case 1:
                        if (StartDate.Date == EndDate.Date) //如果是同一天
                        {
                            dates.Add(StartDate.Date.ToString("D"));
                        }
                        else
                        {
                            DateTime currentDate = StartDate;

                            while (currentDate.Date <= EndDate.Date)
                            {
                                dates.Add(currentDate.Date.ToString("D"));
                                currentDate = currentDate.AddDays(1);
                            }
                        }

                        break;
                    default:
                        dates.Add(NowShift);
                        break;
                }

                //dates.Add("2024年6月26日");
                //dates.Add("2024年6月27日");
                //dates.Add("2024年6月29日");
                //string[]  = new string[] { "2024年6月26日", "2024年6月27日", "2024年6月29日" };
                DataViews = cMysqlBLL.mysqlExecute.QueryData(dates, strStartTime, strEndTime).Tables[0].DefaultView;
                MessageText = "查询成功";
            }
            catch (Exception ex)
            {
                MessageText = "查询失败:"+ ex.Message;
            }
          

        }

    }
}
