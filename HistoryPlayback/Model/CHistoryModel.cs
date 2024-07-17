using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SDFilter;
using WH.Entity.Attribute;

namespace HistoryPlayback.Model
{
    /// <summary>
    /// 记录历史图片路径 等
    /// </summary>
    public partial class CHistoryModel : ObservableObject
    {
        /// <summary>
        /// 20240717 TCG
        /// NG截图保存图片的路径集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> ngImagePaths = new ObservableCollection<string>();

        /// <summary>
        /// 20240717 TCG
        /// 缺陷列表
        /// </summary>
        [property: JsonIgnore]
        [ObservableProperty]
        [property: IgnoreModifyLog]
        private ObservableCollection<DefectFilter> defectList = new();
    }
}
