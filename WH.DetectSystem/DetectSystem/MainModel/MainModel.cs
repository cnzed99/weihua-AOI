using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AlarmSetCtrlWPF;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HistoryPlayback;
using MySqlOperatesApiWPF;
using Newtonsoft.Json;
using ProjProduceData;
using QualityGrade;
using SaveImageManage;
using SDFilter;
using WH.Entity.CommonLib;
using MotionControl;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 20240704 TCG
    /// 单个工程配置文件
    /// </summary>
    public partial class CMainModel : ObservableObject
    {
        /// <summary>
        /// 20240706 TCG
        /// 制程GUID
        /// </summary>
        public string GUID { get; set; }

        [ObservableProperty]
        string name = "毛刺检测";

        [ObservableProperty]
        List<string> testImgFiles = new List<string>();

        /// <summary>
        /// 20240706 TCG
        /// 算法参数
        /// </summary>
        [JsonProperty(Order = 1)]
        public CMaociAlgorParamConfig MaociAlgorParamConfig { get; set; } =
            new CMaociAlgorParamConfig();

        /// <summary>
        /// 20240706 TCG
        /// 质量等级
        /// </summary>
        [JsonProperty(Order = 2)]
        public CQualityConfig MaociQualityConfig { get; set; } = new CQualityConfig();

        /// <summary>
        /// 20240706 TCG
        /// 检测设置
        /// </summary>
        [JsonProperty(Order = 3)]
        public CFilterConfig MaociFilterConfig { get; set; } = new CFilterConfig();

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

        /// <summary>
        /// 20240706 TCG
        /// 数据库
        /// </summary>
        [JsonProperty(Order = 5)]
        public MySqlViewModel MySqlVM { get; set; } = new MySqlViewModel(); //数据库

        /// <summary>
        /// 20240706 TCG
        /// 存图设置
        /// </summary>
        [JsonProperty(Order = 6)]
        public CSaveImageConfig MaociSaveImageConfig { get; set; } = new CSaveImageConfig(); //存图

        /// <summary>
        /// 20240706 TCG
        /// 历史图查看
        /// </summary>
        [JsonProperty(Order = 7)]
        public CHistoryVM MaociHistoryVM { get; set; } = new CHistoryVM(); //历史回看

        /// <summary>
        /// 20240706 TCG
        /// 修改消息通道令牌
        /// </summary>
        Token token;

        public CMainModel()
        {
            GUID = Guid.NewGuid().ToString();
            this.token = new Token(GUID, String.Empty);
        }

        public void UpdateToken()
        {
            MaociAlgorParamConfig.token.ProGuid = GUID;
            MaociQualityConfig.token.ProGuid = GUID;
            MaociFilterConfig.token.ProGuid = GUID;
            MaociAlarmSetConfig.token.ProGuid = GUID;
            MaociSaveImageConfig.token.ProGuid = GUID;

            ConfigModifyObservableBase.UpdateToken(MaociAlarmSetConfig, MaociAlarmSetConfig.token);
            ConfigModifyObservableBase.UpdateToken(MaociFilterConfig, MaociFilterConfig.token);
            ConfigModifyObservableBase.UpdateToken(
                MaociAlgorParamConfig,
                MaociAlgorParamConfig.token
            );
            ConfigModifyObservableBase.UpdateToken(MaociQualityConfig, MaociQualityConfig.token);

            //ConfigModifyObservableBase.UpdateToken(MySqlVM, MySqlVM.token);
            //ConfigModifyObservableBase.UpdateToken(SaveImageVM, SaveImageVM.token);
        }
    }
}
