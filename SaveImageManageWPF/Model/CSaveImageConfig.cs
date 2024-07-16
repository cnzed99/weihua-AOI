using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace SaveImageManage
{
    /// <summary>
    /// 20240715 TCG
    /// 存图配置
    /// </summary>
    public partial class CSaveImageConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 20240711 TCG
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public CSaveImageConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        #region 存截图参数
        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 开启存缺陷截图
        /// </summary>
        [property: DisplayName("开启存缺陷截图")]
        [ObservableProperty]
        private bool piantScreenEnable = true;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// OK存截图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("存OK截图")]
        private bool oKScreenShot;

        /// <summary>
        /// 2024.7.6 鲍赞宝
        /// NG截图保存图片的路径集合
        /// </summary>
        [IgnoreModifyLog]
        [ObservableProperty]
        ObservableCollection<string> ngImagePaths = new ObservableCollection<string>();

        #endregion 存截图参数

        #region 存原图参数
        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 开启存图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("开启存图")]
        private bool _saveImageEnable = false;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 选择存图内容
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("选择存图内容")]
        private string saveSelect = "1";

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 存图主路径
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("存图主路径")]
        private string saveImagePath = "E:\\WH-Image";

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 存图片的格式
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("存图片的格式")]
        private string saveImageFormat = ".tiff";

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 按工程名称保存
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("按工程名称保存")]
        private bool savebyProjName = true;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 按制程(相机)名称保存
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("按制程(相机)名称保存")]
        private bool savebyCamName = false;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 按小时保存
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("按小时保存")]
        private bool savebyHour = false;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 按缺陷类型保存
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("按缺陷类型保存")]
        private bool savebyDefectName = false;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 按流水号保存
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("按流水号保存")]
        private bool savebyID;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// OK保留天数
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("OK保留天数")]
        private int okDays = 1;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// NG保留天数
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("NG保留天数")]
        private int ngDays = 30;

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// OK间隔存图片数
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("OK间隔存图片数")]
        private int okIntervalCount;

        public void Receive(OperateMessage message)
        {
            //记录修改信息
            if (message.obj.GetType() == typeof(CSaveImageConfig))
            {
                OperateLog.Info($"存图设置-{message.message}");
            }
        }

        #endregion 存原图参数
    }
}
