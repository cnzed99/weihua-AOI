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
using WH.Entity;
using WH.Entity.Messages;

namespace WH.DetectSystem.ViewModels
{
    /// <summary>
    /// 2024.9.2 李焕彬
    /// 新增制程视图模型
    /// </summary>
    public partial class CNewProcessVM : ObservableValidator
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
        /// 制程视图模型
        /// </summary>
        [ObservableProperty]
        CMainModelsModelVM mainModelVM;

        /// <summary>
        /// 2024.9.4 李焕彬
        /// 相机列表，不包含已被使用的
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> cams = new();

        public CNewProcessVM(CMainModelsModelVM mainModelVM)
        {
            this.MainModelVM = mainModelVM;
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
                || string.IsNullOrEmpty(this.Focus)
                || GroupVM == null
            )
            {
                Growl.Warning(Properties.Resources.UnfinishedError);
                return;
            }
            if (GroupVM.CMainModels.FirstOrDefault(o => o.Name == this.Name) != null)
            {
                Growl.Warning(Properties.Resources.制程名称不能相同);
                return;
            }

            CMainModel model = new CMainModel(
                this.Name,
                this.Algorithm,
                this.Focus,
                this.CameraSerial.Split('-')[0],
                GroupVM
            );
            GroupVM?.AddProcess(model);
            MainModelVM.UpdateMainVMs();
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
