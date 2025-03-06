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
using WH.Entity.Messages;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 2024.9.2 李焕彬
    /// 修改制程VM
    /// </summary>
    public partial class CModifyProcessVM : ObservableValidator
    {
        #region 需要配置的属性 必需项
        string name;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 名称
        /// </summary>
        [Required]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, true);
        }

        string cameraSerial;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 相机序列号
        /// </summary>
        [Required]
        public string CameraSerial
        {
            get => cameraSerial;
            set => SetProperty(ref cameraSerial, value, true);
        }

        string algorithm;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 算法名
        /// </summary>
        [Required]
        public string Algorithm
        {
            get => algorithm;
            set => SetProperty(ref algorithm, value, true);
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 对焦方法
        /// </summary>
        [ObservableProperty]
        string focus;

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 控制方法
        /// </summary>
        [ObservableProperty]
        string motion;

        CProcessGroupModel groupVM;

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程组
        /// </summary>
        [AdaptIgnore]
        public CProcessGroupModel GroupVM
        {
            get => groupVM;
            set => SetProperty(ref groupVM, value);
        }

        #endregion
        /// <summary>
        /// 2024.9.2 李焕彬
        /// 工程视图
        /// </summary>
        [ObservableProperty]
        CMainModelsModelVM mainModelVM;

        /// <summary>
        /// 2024.9.4 李焕彬
        /// 相机列表，不包含已被使用的
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> cams = new();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程模型
        /// </summary>
        CMainModel mainVM;

        public CModifyProcessVM(CMainModelsModelVM mainModelVM, CMainModel mainVM)
        {
            this.MainModelVM = mainModelVM;
            this.mainVM = mainVM;
            this.Name = mainVM.Name;
            this.algorithm = mainVM.Algorithm;
            this.Focus = mainVM.Focus;
            this.Motion = mainVM.Motion;
            this.GroupVM = mainVM.ProcessGroup;
            foreach (var cam in CCameraManagement.CamParamDict)
            {
                if (
                    mainModelVM.CMainVMs.FirstOrDefault(o =>
                        o.CameraSerial == cam.Value.SerialNumber
                    ) == null
                )
                {
                    Cams.Add($"{cam.Value.SerialNumber}-{cam.Value.Name}");
                }
            }
            if (mainVM.CameraSerial == "无相机")
            {
                this.CameraSerial = "无相机-无相机";
            }
            else
            {
                this.CameraSerial = CCameraManagement.CamParamDict.ContainsKey(mainVM.CameraSerial)
                    ? $"{mainVM.CameraSerial}-{CCameraManagement.CamParamDict[mainVM.CameraSerial].Name}"
                    : $"{mainVM.CameraSerial}-相机未找到";
                Cams.Add(this.CameraSerial);
            }
            Cams.Add("无相机-无相机");
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 确定
        /// </summary>
        [RelayCommand]
        void Sure()
        {
            if (
                HasErrors
                || string.IsNullOrEmpty(this.Name)
                || string.IsNullOrEmpty(this.CameraSerial)
                || string.IsNullOrEmpty(this.Algorithm)
                || GroupVM == null
            )
            {
                Growl.Warning(Properties.Resources.UnfinishedError);
                return;
            }
            if (GroupVM.CMainModels.FirstOrDefault(o => o.Name == this.Name && o != mainVM) != null)
            {
                Growl.Warning(Properties.Resources.制程名称不能相同);
                return;
            }
            if (mainVM.Name != this.Name)
            {
                mainVM.Name = this.Name;
                mainVM.UpdateName();
            }
            if (mainVM.CameraSerial != this.CameraSerial.Split('-')[0])
            {
                mainVM.UpdateCam(this.CameraSerial.Split('-')[0]);
            }
            if (mainVM.Algorithm != this.Algorithm)
            {
                mainVM.UpdateAlgorithm(this.Algorithm);
            }
            if (mainVM.Focus != this.Focus)
            {
                mainVM.UpdateFocus(this.Focus);
            }
            if (mainVM.Motion != this.Motion)
            {
                mainVM.UpdateFocus(this.Focus);
            }
            if (this.GroupVM != this.mainVM.ProcessGroup)
            {
                this.mainVM.ProcessGroup?.RemoveProcess(mainVM);
                this.GroupVM?.AddProcess(mainVM);
                MainModelVM.UpdateMainVMs();
            }
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this), DialogResult = true }
            );
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 取消
        /// </summary>
        [RelayCommand]
        void Cancel()
        {
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(
                new CloseWindowMessage() { Sender = new WeakReference(this), DialogResult = false }
            );
        }
    }
}
