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
    }
}
