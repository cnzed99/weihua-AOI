using AlarmSetCtrl;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using FocusControl;
using HistoryPlayback.Model;
using MarkControl;
using Microsoft.Extensions.Configuration;
using Motion;
using Newtonsoft.Json;
using ProjProduceData;
using SDFilter;
using WH.Entity;
using WH.Entity.CommonLib;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 单个工程配置文件
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CMainModel : ObservableObject
    {
        /// <summary>
        /// 20240706 TCG
        /// 制程GUID
        /// </summary>
        [JsonProperty]
        public string GUID { get; set; }

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 相机序列号
        /// </summary>
        [JsonProperty]
        public string CameraSerial { get; set; }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程算法
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        string algorithm = "算法";

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 对焦方法
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        string focus = "对焦方法";

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制方法
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        string motion = "控制方法";

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程名
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        string name = "毛刺检测";

        [ObservableProperty]
        [JsonProperty]
        List<string> testImgFiles = new List<string>();

        /// <summary>
        /// 20240706 TCG
        /// 算法参数
        /// </summary>
        [JsonProperty(Order = 1)]
        //[JsonConverter(typeof(CAlgorithmParamConverter))]
        public CAlgorithmParamBase MaociAlgorParamConfig { get; set; }

        /// <summary>
        /// 20240706 TCG
        /// 检测设置
        /// </summary>
        [JsonProperty(Order = 3)]
        public CFilterConfig MaociFilterConfig { get; set; }

        /// <summary>
        /// 20240706 TCG
        /// 缺陷数据
        /// </summary>
        [JsonProperty(Order = 4)]
        public CDefectsProduce MaociDefectsProduce { get; set; } = new CDefectsProduce();

        /// <summary>
        /// 20240706 TCG
        /// 报警设置
        /// </summary>
        [JsonProperty(Order = 5)]
        public CAlarmSetConfig MaociAlarmSetConfig { get; set; } = new CAlarmSetConfig();

        ///// <summary>
        ///// 20240706 TCG
        ///// 数据库
        ///// </summary>
        //[JsonIgnore]
        //[AdaptIgnore]
        //public SQLBase MaociMysqlConfig { get; set; } //数据库

        /// <summary>
        /// 20240706 TCG
        /// 历史图查看
        /// </summary>
        [JsonProperty(Order = 7)]
        public CHistoryModel MaociHistoryModel { get; set; } = new CHistoryModel(); //历史回看

        /// <summary>
        /// 2024.9.3 李焕彬
        /// 打标
        /// </summary>
        [JsonProperty(Order = 8)]
        public CMarkConfig MarkConfig { get; set; }

        /// <summary>
        /// 2024.9.3 李焕彬
        /// 对焦
        /// </summary>
        [JsonProperty(Order = 9)]
        public CFocusConfigBase FocusConfig { get; set; }

        /// <summary>
        /// 2024.9.3 李焕彬
        /// 控制
        /// </summary>
        [JsonProperty(Order = 10)]
        public CMotionConfigBase MotionConfig { get; set; }

        /// <summary>
        /// 20240706 TCG
        /// 修改消息通道令牌
        /// </summary>
        [JsonProperty]
        Token token;

        public CMainModel() { }
    }
}
