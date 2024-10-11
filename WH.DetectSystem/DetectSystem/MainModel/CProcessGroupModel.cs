using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using ProjProduceData;
using QualityGrade;
using WH.Entity.CommonLib;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 2024.9.2 李焕彬
    /// 制程组
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CProcessGroupModel : ObservableObject
    {
        public CProcessGroupModel() { }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程GUID
        /// </summary>
        [JsonProperty]
        public string GUID { get; set; }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 修改消息通道令牌
        /// </summary>
        [JsonProperty]
        Token token;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 组名
        /// </summary>
        [JsonProperty]
        [ObservableProperty]
        string name = "制程组1";

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 描述
        /// </summary>
        [JsonProperty]
        [ObservableProperty]
        string description;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 质量等级
        /// </summary>
        [JsonProperty]
        public CQualityConfig MaociQualityConfig { get; set; } = new CQualityConfig();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 缺陷数据汇总
        /// </summary>
        [JsonProperty]
        public CDefectsProduce MaociDefectsProduce { get; set; } = new CDefectsProduce();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 多制程模型
        /// </summary>
        [JsonProperty]
        [ObservableProperty]
        ObservableCollection<CMainModel> cMainModels = new ObservableCollection<CMainModel>();
    }
}
