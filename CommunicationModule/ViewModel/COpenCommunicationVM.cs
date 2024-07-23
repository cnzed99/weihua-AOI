using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 l李焕彬
    /// 通讯设置VM
    /// </summary>
    public partial class COpenCommunicationVM : ObservableObject
    {
        public COpenCommunicationVM() { }

        public COpenCommunicationVM(CCommunicationSettingBase settingBase)
        {
            this.Setting = settingBase;
            Com = CCommunicationManagement.CommDic[Setting.Guid];
            s_Name = Setting.Guid;
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 通讯GUID
        /// </summary>
        public static string s_Name { get; set; }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 通讯参数对象
        /// </summary>
        [ObservableProperty]
        private CCommunicationSettingBase setting;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 通讯对象
        /// </summary>
        [ObservableProperty]
        private CCommunicationBase com;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 接收数据
        /// </summary>
        [ObservableProperty]
        private string txtRecv;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 转码后数据
        /// </summary>
        [ObservableProperty]
        private string txtDecode;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 信息提示
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> listTips;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 关闭通讯
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            Save();
            CCommunicationManagement.ComLogger.Info(Properties.Resources.CloseInfo);
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 保存通讯
        /// </summary>
        [RelayCommand]
        public void Save()
        {
            try
            {
                CCommunicationManagement.ComLogger.Info(Properties.Resources.SaveInfo);
                List<CCommunicationSettingBase> listparam = new List<CCommunicationSettingBase>();

                Dictionary<string, CCommunicationSettingBase>.ValueCollection Values =
                    CCommunicationManagement.CommParamDic.Values;

                foreach (var value in Values)
                {
                    listparam.Add(value);
                }

                ConfigAPI.Save(listparam, CCommunicationManagement.s_CommPath);
                CCommunicationManagement.ComLogger.Info(Properties.Resources.SaveSuccesInfo);
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.SaveError + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 连接通讯
        /// </summary>
        [RelayCommand]
        public void Connect()
        {
            try
            {
                CCommunicationManagement.ComLogger.Info(Properties.Resources.ConnectInfo);
                Com.Connect();
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    Properties.Resources.ConnectError + ex.Message
                );
                Growl.Error(Properties.Resources.ConnectError + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 断开通讯
        /// </summary>
        [RelayCommand]
        public void Disconnect()
        {
            CCommunicationManagement.ComLogger.Info(Properties.Resources.DisConectInfo);
            Com.Close();
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 自定义报警名
        /// </summary>
        private string alarmName = "自定义报警";

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 设置报警协议
        /// </summary>
        /// <param name="cAlarmProtocol">报警协议</param>
        [RelayCommand]
        public void SetAlarm(CAlarmAgreement cAlarmProtocol)
        {
            if (cAlarmProtocol != null)
            {
                CollectionEditor collectionEditor = new CollectionEditor();
                collectionEditor.Collection = cAlarmProtocol.Protocol;
                collectionEditor.Title = cAlarmProtocol.Name;
                collectionEditor.Lang = Com.GetLanguage();
                collectionEditor.ShowDialog();
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 添加报警协议
        /// </summary>
        [RelayCommand]
        public void AddAlarm()
        {
            int index = 0;
            for (int i = Setting.AlarmAgreements.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(Setting.AlarmAgreements[i].Name, alarmName + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(alarmName.Length)) + 1;
                    break;
                }
            }
            Setting.AlarmAgreements.Add(
                new CAlarmAgreement(alarmName + index, Com.CreateProtocol(), Setting)
            );
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 删除报警协议
        /// </summary>
        /// <param name="cAlarmProtocol">报警协议</param>
        [RelayCommand]
        public void DelAlarm(CAlarmAgreement cAlarmProtocol)
        {
            if (cAlarmProtocol != null)
            {
                Setting.AlarmAgreements.Remove(cAlarmProtocol);
            }
        }
    }
}
