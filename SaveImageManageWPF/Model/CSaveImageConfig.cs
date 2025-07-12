using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
            this.token = new Token("CSaveImageConfig", this.GetType().Namespace);
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

        private string saveImagePath = "E:\\WH-Image";

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 存图主路径
        /// </summary>
        // [ObservableProperty]
        [property: DisplayName("存图主路径")]
        public string SaveImagePath
        {
            get { return saveImagePath; }
            set
            {
                DriveInfo info = new DriveInfo("E://");
                if (!info.IsReady)
                {
                    string valuetemp = value.Replace("E:", "D:");
                    SetProperty(ref saveImagePath, valuetemp);
                }
                else
                {
                    SetProperty(ref saveImagePath, value);
                }
            }
        }

        /// <summary>
        /// 2024.6.28 鲍赞宝
        /// 存图片的格式
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("存图片的格式")]
        private string saveImageFormat = ".bmp";

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
        private int okIntervalCount = 0;

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 存图磁盘可用空间限制(GB)
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("存图磁盘可用空间限制(GB)")]
        private int freeSpaceLimit = 5;
        /// <summary>
        /// 2025.7.11 鲍赞宝
        /// 保存四拆分截图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("保存四拆分截图")]
        private bool saveFourCutEnable = false;

        /// <summary>
        /// 2025.7.11 鲍赞宝
        /// 保存上止截图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("保存上止")]
        private bool saveUpMassEnable = false;

        /// <summary>
        /// 2025.7.11 鲍赞宝
        /// 保存下止截图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("保存下止")]
        private bool saveDownMassEnable = false;

        /// <summary>
        /// 2025.7.11 鲍赞宝
        /// 保存拉头截图
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("保存拉头")]
        private bool savePullEnable = false;

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
