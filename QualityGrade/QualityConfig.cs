using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Newtonsoft.Json;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace QualityGrade
{
    /// <summary>
    /// 2024.6.28 李焕彬
    /// 质量等级配置类
    /// </summary>
    public partial class CQualityConfig : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.2 李焕彬
        /// 操作日志
        /// </summary>
        [IgnoreModifyLog]
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 所属制程组名
        /// </summary>
        [property: IgnoreModifyLog]
        public string PrcessName { get; set; }

        public CQualityConfig()
        {
            this.token = new Token("", this.GetType().Namespace);
            Qualities = new ObservableCollection<Quality>()
            {
                new Quality("G1") { Priority = 0 },
                new Quality("G2") { Priority = 1 }
            };
            //参数修改
            //WeakReferenceMessenger.Default.Register<OperateMessage, Token>(this, token);
        }

        [property: DisplayName("等级列表")]
        [ObservableProperty]
        private ObservableCollection<Quality> qualities;

        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CQualityConfig))
            {
                OperateLog.Info($"{PrcessName}-质量等级-{message.message}");
                return;
            }
            foreach (var qua in Qualities)
            {
                if (message.obj is Quality quality)
                {
                    if (qua == quality)
                    {
                        OperateLog.Info($"{PrcessName}-质量等级-{qua.Name}-{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }

        public Quality GetBest()
        {
            if (Qualities.Count > 0)
                return Qualities[0];
            else
                return new Quality("G1") { Priority = 0 };
        }

        public Quality GetWorst()
        {
            if (Qualities.Count > 0)
                return Qualities.Last();
            else
                return new Quality("G1") { Priority = 0 };
        }
    }

    /// <summary>
    /// 2024.6.26 李焕彬
    /// 质量
    /// </summary>
    [DisplayName("等级")]
    public partial class Quality : ConfigModifyObservableBase, IEquatable<Quality>
    {
        public Quality()
        {
            this.token = new Token("", this.GetType().Namespace);
        }

        public Quality(string name)
            : this()
        {
            this.Name = name;
            ShowColor = CBrushPro.s_Instance.KnownColors[
                new Random().Next(CBrushPro.s_Instance.KnownColors.Count - 1)
            ];
        }

        /// <summary>
        /// 等级名 A\B\C\D 放置基类中
        /// </summary>
        //[ObservableProperty]
        //[property: DisplayName("名称")]
        //private string name;

        /// <summary>
        /// 显示颜色
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("颜色")]
        private CKnownColor showColor = null;

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
        private int priority = -1;

        /// <summary>
        /// 质量信号
        /// </summary>
        [ObservableProperty]
        [property: DisplayName("信号")]
        private int signal = 0;

        public Quality Clone()
        {
            var quality = new Quality(this.Name);
            //quality.token = this.token;
            quality.Name = this.Name;
            quality.ShowColor = this.ShowColor;
            quality.Description = this.Description;
            quality.Priority = this.Priority;
            quality.Signal = this.Signal;

            return quality;
        }

        /// <summary>
        /// 当前质量等级 产出
        /// </summary>
        [property: IgnoreModifyLog]
        [ObservableProperty]
        private int number;

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 20240705 TCG
        /// 增加比较运算符 比较优先级大小
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator >(Quality obj1, Quality obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;
            return obj1?.Priority > obj2?.Priority;
        }

        /// <summary>
        /// 20240705 TCG
        /// 增加比较运算符 比较优先级大小
        /// </summary>
        public static bool operator <(Quality obj1, Quality obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;
            return obj1?.Priority < obj2?.Priority;
        }

        /// <summary>
        /// 20240705 TCG
        /// 增加比较运算符 比较优先级大小
        /// </summary>
        public static bool operator ==(Quality obj1, Quality obj2)
        {
            if (obj1 is null && obj2 is null)
                return true;
            return obj1?.Priority == obj2?.Priority;
        }

        /// <summary>
        /// 20240705 TCG
        /// 增加比较运算符 比较优先级大小
        /// </summary>
        public static bool operator !=(Quality obj1, Quality obj2)
        {
            if (obj1 == obj2)
                return false;
            return obj1?.Priority != obj2?.Priority;
        }

        public override bool Equals(object obj)
        {
            if (obj is Quality quality)
            {
                return this == quality;
            }
            return false;
        }

        public bool Equals(Quality other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return Priority.GetHashCode();
        }
    }
}
