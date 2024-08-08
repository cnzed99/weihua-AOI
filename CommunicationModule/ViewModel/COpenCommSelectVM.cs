using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 添加通讯VM
    /// </summary>
    public partial class COpenCommSelectVM : ObservableObject
    {
        public COpenCommSelectVM()
        {
            Keys = CCommunicationManagement.ComHelper.Keys.ToList(); //获取列表
        }

        public Action<CCommunicationSettingBase> ActionAdd { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 通讯类型集合
        /// </summary>
        [ObservableProperty]
        private List<string> keys;

        /// <summary>
        /// 添加通讯
        /// </summary>
        /// <param name="key"></param>
        [RelayCommand]
        public void Add(object obj)
        {
            var objArr = obj as object[];

            if (objArr != null && objArr.Length == 2)
            {
                string key = objArr[0] as string;
                string name = objArr[1] as string;
                if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(name))
                {
                    CCommunicationManagement.ComLogger.Info($"添加通讯界面=>添加{key}通讯协议,通讯名称为:" + name);
                    CCommunicationSettingBase cCommunicationSettingBase = CCommunicationManagement
                        .ComHelper[key]
                        .CreateNewCom(out CCommunicationBase com);
                    cCommunicationSettingBase.Name = name;
                    cCommunicationSettingBase.CommType = key;
                    CCommunicationManagement.AddComm(cCommunicationSettingBase, com);
                    ActionAdd?.Invoke(cCommunicationSettingBase);
                }
                else
                {
                    Growl.Error("名称不能为空，添加通讯失败！");
                }
            }
        }
    }
}
