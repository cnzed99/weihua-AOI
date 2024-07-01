using CommunityToolkit.Mvvm.ComponentModel;
using QualityGrade;
using SDFilter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件.Models
{
    /// <summary>
    /// 工程配置文件
    /// </summary>
    public partial class MainModel:ObservableObject
    {
       
        [ObservableProperty]
        string name  = "毛刺检测";

        public FilterConfig MaociFilter = new FilterConfig();
        public QualityConfig MaociQuality = new QualityConfig();
    }
}
