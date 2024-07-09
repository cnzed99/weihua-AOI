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
using WH.Entity.CommonLib;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 单个工程配置文件
    /// </summary>
    public partial class CMainModel:ObservableObject
    {
        /// <summary>
        /// 20240706 TCG
        /// 制程GUID
        /// </summary>
        public string GUID {  get; set; }

        [ObservableProperty]
        string name  = "毛刺检测";
        [ObservableProperty]
        List<string> testImgFiles = new List<string>();

        /// <summary>
        /// 算法参数
        /// </summary>
        [JsonProperty(Order = 1)]
        public CMaociAlgorParamConfig MaociAlgorParamConfig { get; set; } = new CMaociAlgorParamConfig();
        /// <summary>
        /// 质量等级
        /// </summary>
        [JsonProperty(Order = 2)]
        public CQualityConfig MaociQuality { get; set; } = new CQualityConfig();
        /// <summary>
        /// 检测设置
        /// </summary>
        [JsonProperty(Order = 3)]
        public CFilterConfig MaociFilter { get; set; } = new CFilterConfig();
        /// <summary>
        /// 缺陷数据
        /// </summary>
        [JsonProperty(Order = 4)]
        public CDefectsProduce DefectsProduce { get; set; } = new CDefectsProduce();

        Token token;
        public CMainModel()
        {
            GUID = Guid.NewGuid().ToString();
        
        }
        public void UpdateToken()
        {
            MaociAlgorParamConfig.token.ProGuid = GUID;
            MaociQuality.token.ProGuid = GUID;
            MaociFilter.token.ProGuid = GUID;
            
            ConfigModifyObservableBase.UpdateToken(MaociFilter, MaociFilter.token);
            ConfigModifyObservableBase.UpdateToken(MaociAlgorParamConfig,MaociAlgorParamConfig.token);
            ConfigModifyObservableBase.UpdateToken(MaociQuality, MaociQuality.token);
            
           
        }
    }
}
