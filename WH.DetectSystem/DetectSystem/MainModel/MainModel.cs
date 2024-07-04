using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using ProjProduceData;
using QualityGrade;
using SDFilter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 工程配置文件
    /// </summary>
    public partial class MainModel:ObservableObject
    {
       
        [ObservableProperty]
        string name  = "毛刺检测";
        [ObservableProperty]
        List<string> testImgFiles = new List<string>();

        public MaociAlgorParamConfig MaociAlgorParamConfig { get; set; } = new MaociAlgorParamConfig();
        public FilterConfig MaociFilter { get; set; } = new FilterConfig();
        public QualityConfig MaociQuality { get; set; } = new QualityConfig();
        public DefectsProduce DefectsProduce { get; set; } = new DefectsProduce();
    }
}
