using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapster;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.Entity;
using WH.Entity.Messages;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 20240704 TCG
    /// 新建工程 视图模型
    /// </summary>
    public partial class CNewProjVM : ObservableValidator
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

        [Required]
        public string ProjPath
        {
            get => projPath;
            set => SetProperty(ref projPath, value, true);
        }
        string cameraSerial;

        [Required]
        public string CameraSerial
        {
            get => cameraSerial;
            set => SetProperty(ref cameraSerial, value, true);
        }
        #endregion
        CMainModelsModelVM mainModelVM;
        CMainVM mainVM;

        public CNewProjVM(CMainModelsModelVM mainVM)
        {
            mainModelVM = mainVM;
            this.mainVM = mainVM.CMainVMs[0];

            this.mainVM.Adapt(this);
            this.ProjPath = mainModelVM.ProjPath;
        }

        #region 应用或丢弃当前工程变更
        /// <summary>
        /// 保存当前工程的修改
        /// </summary>
        public void ApplyChanges()
        {
            mainModelVM.ProjPath = this.ProjPath;
            if (mainVM.Model is null)
                mainVM.Model = new CMainModel();
            this.Adapt(mainVM);
        }

        /// <summary>
        /// 丢弃当前工程的修改
        /// </summary>
        public void DiscardChanges() => mainVM.Adapt(this);
        #endregion

        [RelayCommand]
        void Sure()
        {
            if (HasErrors)
                return;
            this.mainVM.GUID = Guid.NewGuid().ToString(); //GUID
            this.ApplyChanges();
            CCameraManagement.CamParamDict[cameraSerial].ProjGuid = this.mainVM.GUID;
            mainModelVM.SaveCurrentProj();
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this), DialogResult = true }
            );
        }

        [RelayCommand]
        void Cancel()
        {
            this.DiscardChanges();
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this), DialogResult = false }
            );
        }
    }
}
