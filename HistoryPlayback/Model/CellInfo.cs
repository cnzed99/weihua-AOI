using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic;

namespace HistoryPlayback
{
    public partial class CellInfo : ObservableObject
    {
        /// <summary>
        /// 2024.7.4鲍赞宝
        /// 流水号
        /// </summary>
        [ObservableProperty]
        string iD;

        /// <summary>
        /// 2024.7.4鲍赞宝
        /// 创建时间
        /// </summary>
        [ObservableProperty]
        string createTime;

        /// <summary>
        /// 2024.7.4鲍赞宝
        /// 检测耗时
        /// </summary>
        [ObservableProperty]
        string takeTime;

        /// <summary>
        /// 2024.7.4鲍赞宝
        /// 质量等级
        /// </summary>
        [ObservableProperty]
        string level;

        /// <summary>
        /// 2024.7.4鲍赞宝
        /// 缺陷名称
        /// </summary>
        [ObservableProperty]
        string defectName;
    }
}
