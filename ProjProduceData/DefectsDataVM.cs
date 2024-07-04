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

        public void SetDefectsProduce(DefectsProduce defectsProduce, FilterConfig filterConfig, QualityConfig qualityConfig) 
        {
            this.DefectsProduce = defectsProduce;
            Receive(filterConfig);
            Receive(qualityConfig);
        }

        [ObservableProperty]
        private DefectsProduce defectsProduce;

        public void Receive(PropertyChangedMessage<string> message)
        {
            var de = DefectsProduce.DefectNumbersList.FirstOrDefault(o => o.Name == message.OldValue);
            if (de != null)
            {
                de.Name = message.NewValue;
            }
        }

        public void Receive(PropertyChangedMessage<Quality> message)
        {
            var qua = DefectsProduce.QualityNumbersList.FirstOrDefault(o => o.Name == message.OldValue.Name);
            if (qua != null)
            {
                qua.Name = message.NewValue.Name;
                qua.ShowColor = message.NewValue.ShowColor;
            }
        }

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
