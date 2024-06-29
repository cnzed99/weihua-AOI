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

namespace QualityGrade
{
    public partial class QualityConfig : ObservableLog, IRecipient<OperateMessage>
    {
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        public QualityConfig()
        {
            WeakReferenceMessenger.Default.Register<OperateMessage>(this);
            Qualities.CollectionChanged += (s, e) => { base.CollectionChanged(e, nameof(Qualities)); };
        }

        [ObservableProperty]
        private ObservableCollection<Quality> qualities = new ObservableCollection<Quality>() { new Quality("G1") };

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
                }
            }
        }
    }
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 质量等级配置类
    /// </summary>
    public partial class Quality : ObservableLog
    {
        public Quality(string name)
        {
            this.Name = name;
        }
        /// <summary>
        /// 等级名 A\B\C\D
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 显示颜色
        /// </summary>
        [ObservableProperty]
        private Brush colorBrush = Brushes.Red;
        
        /// <summary>
        /// 说明
        /// </summary>
        [ObservableProperty]
        private string description;

        /// <summary>
        /// 等级水平 越高越差
        /// </summary>
        [ObservableProperty]
        private int priority = 0;

        /// <summary>
        /// 质量信号
        /// </summary>
        [ObservableProperty]
        private int signal = 0;

        public Quality Clone()
        {
            var quality = new Quality(this.Name);
            quality.Name = this.Name;
            quality.ColorBrush = this.ColorBrush;
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
