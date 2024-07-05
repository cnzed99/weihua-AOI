using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using ProjProduceData;
using QualityGrade;
using SDFilter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 单个工程配置文件
    /// </summary>
    public partial class CMainModel:ObservableObject
    {
        
        [ObservableProperty]
        string name  = "毛刺检测";
        [ObservableProperty]
        List<string> testImgFiles = new List<string>();

        /// <summary>
        /// 算法参数
        /// </summary>
        [JsonProperty(Order = 1)]
        public MaociAlgorParamConfig MaociAlgorParamConfig { get; set; }
        /// <summary>
        /// 质量等级
        /// </summary>
        [JsonProperty(Order = 2)]
        public QualityConfig MaociQuality { get; set; }
        /// <summary>
        /// 检测设置
        /// </summary>
        [JsonProperty(Order = 3)]
        public FilterConfig MaociFilter { get; set; }
        /// <summary>
        /// 缺陷数据
        /// </summary>
        [JsonProperty(Order = 4)]
        public DefectsProduce DefectsProduce { get; set; }
    }
}
