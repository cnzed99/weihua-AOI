using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QualityGrade;
using SDFilter;
using SVGImage.SVG.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjProduceData
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 产量统计、缺陷统计控件ViewModel
    /// </summary>
    public partial class DefectsDataVM : ObservableObject,IRecipient<PropertyChangedMessage<string>>, IRecipient<PropertyChangedMessage<Quality>>,
        IRecipient<FilterConfig>, IRecipient<QualityConfig>
    {
        public DefectsDataVM() 
        {
            WeakReferenceMessenger.Default.Register<PropertyChangedMessage<string>, string>(this, "DefectName");
            WeakReferenceMessenger.Default.Register<PropertyChangedMessage<Quality>>(this);
            WeakReferenceMessenger.Default.Register<FilterConfig>(this);
            WeakReferenceMessenger.Default.Register<QualityConfig>(this);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计VM
        /// </summary>
        /// <param name="defectsProduce"></param>
        /// <param name="filterConfig"></param>
        /// <param name="qualityConfig"></param>
        public void SetDefectsProduce(DefectsProduce defectsProduce, FilterConfig filterConfig, QualityConfig qualityConfig) 
        {
            this.DefectsProduce = defectsProduce;
            Receive(filterConfig);
            Receive(qualityConfig);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计配置
        /// </summary>
        [ObservableProperty]
        private DefectsProduce defectsProduce;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷名字修改消息处理
        /// </summary>
        /// <param name="message">缺陷名字修改消息</param>
        public void Receive(PropertyChangedMessage<string> message)
        {
            var de = DefectsProduce.DefectNumbersList.FirstOrDefault(o => o.Name == message.OldValue);
            if (de != null)
            {
                de.Name = message.NewValue;
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级修改消息处理
        /// </summary>
        /// <param name="message">质量等级修改消息</param>
        public void Receive(PropertyChangedMessage<Quality> message)
        {
            var qua = DefectsProduce.QualityNumbersList.FirstOrDefault(o => o.Name == message.OldValue.Name);
            if (qua != null)
            {
                qua.Name = message.NewValue.Name;
                qua.ShowColor = message.NewValue.ShowColor;
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测设置配置修改消息处理
        /// </summary>
        /// <param name="filter">检测设置配置</param>
        public void Receive(FilterConfig filter)
        {
            List<string> strings = new List<string>();
            foreach (var sp in filter.SpeciesFilters)
            {
                foreach (var rp in sp.RecipeDefects)
                {
                    foreach (var de in rp.DefectFilters)
                    {
                        if (DefectsProduce.DefectNumbersList.FirstOrDefault(o => o.Name == de.Name) == null)
                        {
                            DefectsProduce.DefectNumbersList.Add(new(de.Name));
                        }
                        strings.Add(de.Name);
                    }
                }
            }
            for (int i = DefectsProduce.DefectNumbersList.Count - 1; i >= 0; i--)
            {
                if (!strings.Contains(DefectsProduce.DefectNumbersList[i].Name))
                {
                    DefectsProduce.DefectNumbersList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 质量等级配置修改消息处理
        /// </summary>
        /// <param name="message">质量等级配置</param>
        public void Receive(QualityConfig message)
        {
            List<string> strings = new List<string>();
            foreach (var qua in message.Qualities)
            {
                if (DefectsProduce.QualityNumbersList.FirstOrDefault(o => o.Name == qua.Name) == null)
                {
                    DefectsProduce.QualityNumbersList.Add(new QualityNumber(qua));
                }
                strings.Add(qua.Name);
            }
            for (int i = DefectsProduce.QualityNumbersList.Count - 1; i >= 0; i--)
            {
                if (!strings.Contains(DefectsProduce.QualityNumbersList[i].Name))
                {
                    DefectsProduce.QualityNumbersList.RemoveAt(i);
                }
            }
        }
    }
}
