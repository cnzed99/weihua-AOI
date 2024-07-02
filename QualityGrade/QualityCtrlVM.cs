using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace QualityGrade
{
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 质量等级控件VM
    /// </summary>
    public partial class QualityCtrlVM : ObservableLog, IRecipient<RequestMessage<ObservableCollection<Quality>>>
    {
        public QualityCtrlVM() 
        {
            WeakReferenceMessenger.Default.Register<RequestMessage<ObservableCollection<Quality>>>(this);
        }

        public void Receive(RequestMessage<ObservableCollection<Quality>> message)
        {
            message.Reply(QualityConfig.Qualities);
        }

        [ObservableProperty]
        private QualityConfig qualityConfig = new QualityConfig();

        [ObservableProperty]
        private Quality qualitySet = new Quality("G1");

        private Quality qualitySelect;

        public Quality QualitySelect
        {
            get { return qualitySelect; }
            set { 
                SetProperty(ref qualitySelect, value);
                if (value != null)
                {
                    QualitySet = value.Clone();
                }
                RemoveCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanRemoveAndEdit() => QualitySelect != null;

        [RelayCommand]
        public void Add()
        {
            if (QualitySet != null)
            {
                if (QualityConfig.Qualities.ToList().Exists(o => o.Name == QualitySet.Name))
                {
                    Growl.Error(Properties.Resource1.NameErrorInfo);
                }
                else
                {
                    var qua = QualitySet.Clone();
                    if (QualityConfig.Qualities.Count > 0) qua.Priority = QualityConfig.Qualities[QualityConfig.Qualities.Count - 1].Priority + 1;
                    QualityConfig.Qualities.Add(qua);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanRemoveAndEdit))]
        public void Remove()
        {
            if (QualitySelect != null)
            {
                QualityConfig.Qualities.Remove(QualitySelect);
            }
        }

        [RelayCommand(CanExecute = nameof(CanRemoveAndEdit))]
        public void Edit()
        {
            if (QualitySelect != null)
            {
                if (QualitySelect.Name != QualitySet.Name && QualityConfig.Qualities.ToList().Exists(o => o.Name == QualitySet.Name))
                {
                    Growl.Error(Properties.Resource1.NameErrorInfo);
                }
                else
                {
                    QualitySelect.Name = QualitySet.Name;
                    QualitySelect.Priority = QualitySet.Priority;
                    QualitySelect.ShowColor = QualitySet.ShowColor;
                    QualitySelect.Signal = QualitySet.Signal;
                    QualitySelect.Description = QualitySet.Description;
                }
            }
        }
    }
}
