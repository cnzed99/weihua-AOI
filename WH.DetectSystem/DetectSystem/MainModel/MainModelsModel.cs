using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using MySqlOperatesApi;
using Newtonsoft.Json;
using SaveImageManage;
using WH.DetectSystem.Models;

namespace WH.DetectSystem.DetectSystem.MainModel
{
    public partial class CMainModelsModel : ObservableObject
    {
        /// <summary>
        /// 2024.9.2 李焕彬
        /// 工程名
        /// </summary>
        [ObservableProperty]
        string name = "毛刺检测";

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 制程组
        /// </summary>
        [ObservableProperty]
        ObservableCollection<CProcessGroupModel> cProcessGroups =
            new ObservableCollection<CProcessGroupModel>();
    }
}
