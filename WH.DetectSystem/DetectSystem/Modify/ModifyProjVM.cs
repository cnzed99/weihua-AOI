using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapster;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            set=>SetProperty(ref name, value,true);
        }

        string projPath;
        [Required]
        public string ProjPath
        {
            get => projPath;
            set => SetProperty(ref projPath, value,true);
        }
        #endregion
       
        CMainListVM mainvm;
        public CModifyProjVM(CMainListVM mainVM)
        {
            mainvm = mainVM;
            mainVM.Adapt(this);
        }

        #region 应用或丢弃当前工程变更
        /// <summary>
        /// 保存当前工程的修改
        /// </summary>
        public void ApplyChanges()
        {
            mainvm.ProjPath = this.ProjPath;
            this.Adapt(mainvm);

        }
        /// <summary>
        /// 丢弃当前工程的修改
        /// </summary>
        public void DiscardChanges() => mainvm.Adapt(this);
        #endregion

        [RelayCommand]
        void Sure()
        {
            if (HasErrors) return;
            this.ApplyChanges();
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage() { Sender = new WeakReference(this) });
        }
        [RelayCommand]
        void Cancel()
        {
            this.DiscardChanges();
            WeakReferenceMessenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage() { Sender = new WeakReference(this) });
        }
    }
}
