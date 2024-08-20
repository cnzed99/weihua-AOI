using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace QualityGrade
{
    /// <summary>
    /// 2024.6.26 李焕彬
    /// 质量等级控件VM
    /// </summary>
    public partial class CQualityCtrlVM : ObservableObject
    {
        public CQualityCtrlVM() { }

        private CQualityConfig qualityConfig; //不要在这里赋值

        /// <summary>
        /// 20240716 TCG
        /// 质量等级配置
        /// </summary>
        public CQualityConfig QualityConfig
        {
            get => qualityConfig;
            set { SetProperty(ref qualityConfig, value); }
        }

        public void Reset()
        {
            QualitySelect = null;
            QualitySet = new Quality();
        }

        /// <summary>
        /// 2024.7.5 TCG
        /// 当前设置质量 不要赋值
        /// </summary>
        [ObservableProperty]
        private Quality qualitySet; //不要在这里赋值

        private Quality qualitySelect; //不要在这里赋值

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 被选中质量
        /// </summary>
        public Quality QualitySelect
        {
            get { return qualitySelect; }
            set
            {
                SetProperty(ref qualitySelect, value);
                if (value != null)
                {
                    QualitySet = value.Clone();
                }
                RemoveCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除、编辑判断
        /// </summary>
        /// <returns></returns>
        private bool CanRemoveAndEdit() => QualitySelect != null;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加质量等级
        /// </summary>
        [RelayCommand]
        public void Add()
        {
            if (QualitySet != null&& !string.IsNullOrWhiteSpace(QualitySet.Name))
            {
                if (QualitySet.ShowColor==null)
                {
                    Growl.Info(Properties.Resource1.ShowColorStr);
                    return;
                }
                if (QualityConfig.Qualities.ToList().Exists(o => o.Name == QualitySet.Name))
                {
                    Growl.Error(Properties.Resource1.NameErrorInfo);
                }
                else
                {
                    var qua = QualitySet.Clone();
                    qua.Priority = 0;
                    if (QualityConfig.Qualities.Count > 0)
                        qua.Priority =
                            QualityConfig.Qualities[QualityConfig.Qualities.Count - 1].Priority + 1;
                    QualityConfig.Qualities.Add(qua);
                    qua.token.ProGuid = QualityConfig.token.ProGuid;
                    //WeakReferenceMessenger.Default.Send<CQualityConfig>(CQualityConfig);
                }
            }
            else
            {
                Growl.Info(Properties.Resource1.NullRemind);
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 删除质量等级
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveAndEdit))]
        public void Remove()
        {
            if (QualitySelect != null)
            {
                Growl.Ask(Properties.Resource1.DelecteAsk, b =>
                {
                    if (b)
                    {
                        QualityConfig.Qualities.Remove(QualitySelect);
                        WeakReferenceMessenger.Default.Send<CQualityConfig>(QualityConfig);
                    }
                    return true;
                });
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 编辑质量等级
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveAndEdit))]
        public void Edit()
        {
            if (QualitySelect != null)
            {
                if (
                    QualitySelect.Name != QualitySet.Name
                    && QualityConfig.Qualities.ToList().Exists(o => o.Name == QualitySet.Name)
                )
                {
                    Growl.Error(Properties.Resource1.NameErrorInfo);
                }
                else
                {
                    var oldValue = QualitySelect.Clone();
                    QualitySelect.Name = QualitySet.Name;
                    QualitySelect.Priority = QualitySet.Priority;
                    QualitySelect.ShowColor = QualitySet.ShowColor;
                    QualitySelect.Signal = QualitySet.Signal;
                    QualitySelect.Description = QualitySet.Description;

                    //WeakReferenceMessenger.Default.Send<PropertyChangedMessage<Quality>>(new PropertyChangedMessage<Quality>(this, null, oldValue, QualitySelect));
                }
            }
        }
    }
}
