using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlOperatesApi;
using SDFilter;
using WH.DetectSystem;
using WH.DetectSystem.ViewModels;

namespace DataQuery
{
    public partial class CDataQueryVM : ObservableObject
    {
        public CMySqlVM cMysqlBLL;
        CSystemSettingsVM SystemSettings = CPublicServices.Container.Resolve<CSystemSettingsVM>();

        public CDataQueryVM(CMySqlVM sqlViewModel)
        {
            cMysqlBLL = sqlViewModel;
        }

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询方式
        /// </summary>
        [ObservableProperty]
        int selectQueryMode = 0;

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
        DateTime startTime = DateTime.Parse("8:00");

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询结束时间
        /// </summary>
        [ObservableProperty]
        DateTime endTime = DateTime.Parse("20:00");

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
        async Task SearchAsync()
        {
            try
            {
                string SearchStatus = await Task.Run(() =>
                {
                    List<string> dates = new List<string>();
                    DateTime newStarTime = StartDate.Date + StartTime.TimeOfDay;
                    DateTime endStarTime = EndDate.Date + EndTime.TimeOfDay;
                    string strStartTime = newStarTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string strEndTime = endStarTime.ToString("yyyy-MM-dd HH:mm:ss");
                    switch (SelectQueryMode)
                    {
                        case 0:
                            dates.Add(SystemSettings.NowShift);
                            break;
                        case 1:
                            break;
                        default:
                            dates.Add(SystemSettings.NowShift);
                            break;
                    }
                    //dates.Add("2024年6月26日");
                    //dates.Add("2024年6月27日");
                    //dates.Add("2024年6月29日");
                    //string[]  = new string[] { "2024年6月26日", "2024年6月27日", "2024年6月29日" };
                    var dataTableCollection = cMysqlBLL.MysqlExecute.QueryData(
                        dates,
                        strStartTime,
                        strEndTime
                    );
                    if (dataTableCollection.Tables.Count > 0)
                    {
                        DataViews = dataTableCollection.Tables[0].DefaultView;
                        MessageText = "查询成功";
                    }
                    else
                    {
                        MessageText = "查询成功,该段时间没有生产。";
                    }
                    return "";
                });
            }
            catch (Exception ex)
            {
                MessageText = "查询失败:" + ex.Message;
            }
        }
    }
}
