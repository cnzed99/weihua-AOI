using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using Newtonsoft.Json;
using System.ComponentModel;

namespace QualityGrade
{
    /// <summary>
    /// 2024.6.28 李焕彬
    /// 质量等级配置类
    /// </summary>
    public partial class QualityConfig : ObservableLog, IRecipient<OperateMessage>
    {
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public QualityConfig()
        {
            Qualities = new ObservableCollection<Quality>() { new Quality("G1") };
            WeakReferenceMessenger.Default.Register<OperateMessage, string>(this, this.GetType().Namespace);
        }
        [property: DisplayName("等级列表")]
        [ObservableProperty]
        private ObservableCollection<Quality> qualities;
       
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(QualityConfig))
            {
                OperateLog.Info($"质量等级-{message.message}");
                return;
            }
            foreach (var qua in Qualities)
            {
                if (message.obj.GetType() == typeof(Quality))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"质量等级-{qua.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }
    }
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 质量
    /// </summary>
    [DisplayName("等级")]
    public partial class Quality : ObservableLog
    {
        public Quality()
        {

        }
        public Quality(string name)
        {
            this.Name = name;
            ShowColor = BrushPro.instance.KnownColors[new Random().Next(BrushPro.instance.KnownColors.Count - 1)];
        }
        /// <summary>
        /// 等级名 A\B\C\D
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("名称")]
        private string name;

        /// <summary>
        /// 显示颜色
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("颜色")]
        private KnownColor showColor = null;
        
        /// <summary>
        /// 说明
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("说明")]
        private string description;

        /// <summary>
        /// 等级水平 越高越差
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("优先级")]
        private int priority = 0;

        /// <summary>
        /// 质量信号
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("信号")]
        private int signal = 0;

        public Quality Clone()
        {
            var quality = new Quality(this.Name);
            quality.Name = this.Name;
            quality.ShowColor = this.ShowColor;
            quality.Description = this.Description;
            quality.Priority = this.Priority;
            quality.Signal = this.Signal;
            return quality;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
