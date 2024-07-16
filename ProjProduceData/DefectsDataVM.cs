using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QualityGrade;
using SDFilter;
using SVGImage.SVG.Filters;

namespace ProjProduceData
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 产量统计、缺陷统计控件ViewModel
    /// </summary>
    public partial class CDefectsDataVM : ObservableObject
    {
        public CDefectsDataVM()
        {
            //WeakReferenceMessenger.Default.Register<PropertyChangedMessage<string>, string>(this, "DefectName");
            //WeakReferenceMessenger.Default.Register<PropertyChangedMessage<Quality>>(this);
            //WeakReferenceMessenger.Default.Register<CFilterConfig>(this);
            //WeakReferenceMessenger.Default.Register<CQualityConfig>(this);
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化缺陷统计VM
        /// </summary>
        /// <param name="defectsProduce"></param>
        /// <param name="filterConfig"></param>
        /// <param name="qualityConfig"></param>
        public void SetDefectsProduce(
            CDefectsProduce defectsProduce,
            CFilterConfig filterConfig,
            CQualityConfig qualityConfig
        )
        {
            this.DefectsProduce = defectsProduce;
            Receive(filterConfig);
            filterConfig.SpeciesFilters.CollectionChanged += (s, e) =>
            {
                Receive(filterConfig);
            };
            foreach (var sp in filterConfig.SpeciesFilters)
            {
                sp.RecipeDefects.CollectionChanged += (s, e) =>
                {
                    Receive(filterConfig);
                };
                foreach (var rd in sp.RecipeDefects)
                {
                    rd.DefectFilters.CollectionChanged += (s, e) =>
                    {
                        Receive(filterConfig);
                    };
                }
            }

            this.DefectsProduce.QualityNumbersList = qualityConfig.Qualities;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 缺陷统计配置
        /// </summary>
        [ObservableProperty]
        private CDefectsProduce defectsProduce;

        ///// <summary>
        ///// 2024.7.4 李焕彬
        ///// 缺陷名字修改消息处理
        ///// </summary>
        ///// <param name="message">缺陷名字修改消息</param>
        //public void Receive(PropertyChangedMessage<string> message)
        //{
        //    var de = CDefectsProduce.DefectNumbersList.FirstOrDefault(o => o.Name == message.OldValue);
        //    if (de != null)
        //    {
        //        de.Name = message.NewValue;
        //    }
        //}

        ///// <summary>
        ///// 2024.7.4 李焕彬
        ///// 质量等级修改消息处理
        ///// </summary>
        ///// <param name="message">质量等级修改消息</param>
        //public void Receive(PropertyChangedMessage<Quality> message)
        //{
        //    var qua = CDefectsProduce.QualityNumbersList.FirstOrDefault(o => o.Name == message.OldValue.Name);
        //    if (qua != null)
        //    {
        //        qua.Name = message.NewValue.Name;
        //        qua.ShowColor = message.NewValue.ShowColor;
        //    }
        //}

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 检测设置配置修改消息处理
        /// </summary>
        /// <param name="filter">检测设置配置</param>
        public void Receive(CFilterConfig filter)
        {
            List<string> strings = new List<string>();
            foreach (var sp in filter.SpeciesFilters)
            {
                foreach (var rp in sp.RecipeDefects)
                {
                    foreach (var de in rp.DefectFilters)
                    {
                        if (!DefectsProduce.DefectNumbersList.Contains(de))
                        {
                            DefectsProduce.DefectNumbersList.Add(de);
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
    }
}
