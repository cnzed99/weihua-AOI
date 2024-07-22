using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 l李焕彬
    /// 通讯列表VM
    /// </summary>
    public partial class COpenCommunicationListVM : ObservableObject
    {
        public COpenCommunicationListVM()
        {
            foreach (var item in CCommunicationManagement.CommParamDic)
            {
                Settings.Add(item.Value);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 通讯列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CCommunicationSettingBase> settings =
            new ObservableCollection<CCommunicationSettingBase>();

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 添加通讯
        /// </summary>
        [RelayCommand]
        void Add()
        {
            OpenCommSelect openCommSelect = new OpenCommSelect();
            openCommSelect.selectVM.ActionAdd = (setting) =>
            {
                Settings.Add(setting);
            };
            openCommSelect.ShowDialog();
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 设置通讯
        /// </summary>
        [RelayCommand]
        void Set(CCommunicationSettingBase settingBase)
        {
            OpenCommunication openCommunication = new OpenCommunication(settingBase);
            openCommunication.ShowDialog();
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 删除通讯
        /// </summary>
        [RelayCommand]
        void Del(CCommunicationSettingBase settingBase)
        {
            Settings.Remove(settingBase);
            CCommunicationManagement.DelComm(settingBase.Guid);
        }
    }
}
