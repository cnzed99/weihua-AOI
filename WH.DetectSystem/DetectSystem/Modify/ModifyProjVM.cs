using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Mapster;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.Entity.Messages;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 20240704 TCG
    /// 工程修改 视图模型
    /// </summary>
    public partial class CModifyProjVM : ObservableValidator
    {
        #region 需要配置的属性 必需项
        string name;

        [Required]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, true);
        }

        string projPath;
        public string ProjPath
        {
            get => projPath;
            set => SetProperty(ref projPath, value, true);
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程组
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CProcessGroupModel> cProcessGroups =
            new ObservableCollection<CProcessGroupModel>();
        #endregion

        CMainModelsModelVM mainModelVM;

        public CModifyProjVM(CMainModelsModelVM mainModelVM)
        {
            this.mainModelVM = mainModelVM;
            this.Name = mainModelVM.CMainMModel.Name;
            this.ProjPath = mainModelVM.ProjPath;
            foreach (var item in mainModelVM.CMainMModel.CProcessGroups)
            {
                CProcessGroups.Add(
                    new CProcessGroupModel(item.Name)
                    {
                        Description = item.Description,
                        GUID = item.GUID
                    }
                );
            }
        }

        [RelayCommand]
        void Sure()
        {
            if (HasErrors || string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.ProjPath))
            {
                Growl.Warning(Properties.Resources.UnfinishedError);
                return;
            }
            for (int i = 0; i < CProcessGroups.Count - 1; i++)
            {
                for (int j = i + 1; j < CProcessGroups.Count; j++)
                {
                    if (CProcessGroups[i].Name == CProcessGroups[j].Name)
                    {
                        Growl.Warning(Properties.Resources.制程组名称不能相同);
                        return;
                    }
                }
            }
            mainModelVM.CMainMModel.Name = this.Name;
            mainModelVM.ProjPath = this.ProjPath;
            for (int i = 0; i < CProcessGroups.Count; i++)
            {
                var groupFind = mainModelVM.CMainMModel.CProcessGroups.FirstOrDefault(o =>
                    o.GUID == CProcessGroups[i].GUID
                );
                if (groupFind != null)
                {
                    groupFind.Name = CProcessGroups[i].Name;
                    groupFind.Description = CProcessGroups[i].Description;
                    groupFind.NameUpdata();
                    CProcessGroups[i] = groupFind;
                }
            }
            mainModelVM.AddProcessGroup(CProcessGroups.ToList());
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this), DialogResult = true }
            );
        }

        [RelayCommand]
        void Cancel()
        {
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this) }
            );
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 增加制程组
        /// </summary>
        [RelayCommand]
        public void Add()
        {
            int index = 1;
            while (true)
            {
                if (!CProcessGroups.ToList().Exists(o => o.Name == $"制程组{index}"))
                {
                    break;
                }
                index++;
            }
            CProcessGroups.Add(new($"制程组{index}"));
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 删除制程组
        /// </summary>
        /// <param name="groupVM">制程组</param>
        [RelayCommand]
        public void Del(CProcessGroupModel group)
        {
            if (group != null)
            {
                Growl.AskGlobal(
                    Properties.Resources.DelecteAsk,
                    b =>
                    {
                        if (b)
                        {
                            CProcessGroups.Remove(group);
                        }
                        return true;
                    }
                );
            }
        }
    }
}
