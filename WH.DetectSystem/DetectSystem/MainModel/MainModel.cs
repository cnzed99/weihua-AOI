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
using System.Collections.ObjectModel;
using WH.DetectSystem.DetectSystem.MainModel;
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
        private string algorithm = "算法";

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 对焦方法
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        private string focus = "对焦方法";

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程名
        /// </summary>
        [ObservableProperty]
        [JsonProperty]
        private string name = "检测制程";

        /// <summary>
        /// 2026.8.14【盘齿方案4-改动D】该制程每件应收图张数 N（合并门槛来源，随 .burrproj 序列化）
        /// 盘齿默认：下端面/上齿面/上端面=1；内孔=6；上轴侧面/下轴侧面=10；整轴侧面=14（建制程时按名写入，见 CMainModel 构造）
        /// 0 值防线（方案审核 G3）：合并门槛 MainVM.cs:938 为 Count>=PhotoTatolCount，0 恒真会导致每张图立即合并；
        /// 旧工程无此键时走字段缺省 1，getter 再兜底 &lt;=0 返回 1
        /// </summary>
        [JsonProperty]
        private int photoTotalCount = 1;

        [JsonIgnore]
        public int PhotoTotalCount
        {
            get => photoTotalCount <= 0 ? 1 : photoTotalCount;
            set
            {
                if (SetProperty(ref photoTotalCount, value))
                {
                    SyncXinGearShotTilesAfterPhotoCountChanged();
                }
            }
        }

        //【盘齿方案0.1-注释】制程级副窗列表，随工程保存；不进 SystemSetting.Json。旧工程缺字段保持空集合=不含副窗
        [ObservableProperty]
        [JsonProperty]
        ObservableCollection<CProcessSubWindowItem> processSubWindows = new();

        //【盘齿方案0.5-注释】无分页固定布局元数据：按制程名映射行列/跨度（非盘齿/未命中给默认，不进 .burrproj）
                [JsonIgnore]
        public int GridRow
        {
            get
            {
                return Name switch
                {
                    //【盘齿方案0.5-注释】上端面/下端面仅布局对调：上端面行0、下端面行1
                    "下端面" => 1,
                    "上齿面" => 0,
                    "内孔" => 0,
                    "上端面" => 0,
                    "整轴侧面" => 1,
                    "上轴侧面" => 2,
                    "下轴侧面" => 2,
                    //【曲轴方案0.5-注释】六格 Row（0-based）；盘齿 case 原样保留
                    "端面" => 0,
                    "杆面" => 0,
                    "底部光滑面" => 1,
                    "底盘侧面" => 1,
                    "顶面" => 2,
                    "底面" => 2,
                    _ => 0,
                };
            }
        }

        [JsonIgnore]
        public int GridCol
        {
            get
            {
                return Name switch
                {
                    "下端面" => 0,
                    "上齿面" => 1,
                    "内孔" => 2,
                    "上端面" => 0,
                    "整轴侧面" => 1,
                    "上轴侧面" => 0,
                    "下轴侧面" => 2,
                    //【曲轴方案0.5-注释】六格 Col（0-based）；盘齿 case 原样保留
                    "端面" => 0,
                    "杆面" => 1,
                    "底部光滑面" => 0,
                    "底盘侧面" => 1,
                    "顶面" => 0,
                    "底面" => 2,
                    _ => 0,
                };
            }
        }

        [JsonIgnore]
        public int GridColSpan
        {
            get
            {
                return Name switch
                {
                    "内孔" => 2,
                    "整轴侧面" => 3,
                    "上轴侧面" => 2,
                    "下轴侧面" => 2,
                    //【曲轴方案0.5-注释】六格 ColSpan；端面/底部光滑面走 _ => 1
                    "杆面" => 3,
                    "底盘侧面" => 3,
                    "顶面" => 2,
                    "底面" => 2,
                    _ => 1,
                };
            }
        }

        [ObservableProperty]
        [JsonProperty]
        private List<string> testImgFiles = new List<string>();

        /// <summary>
        /// 20240706 TCG
        /// 算法参数
        /// </summary>
        [JsonProperty(Order = 1)]
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
        /// 20240706 TCG
        /// 修改消息通道令牌
        /// </summary>
        [JsonProperty]
        private Token token;

        public CMainModel() { }
    }
}
