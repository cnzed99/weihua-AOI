using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.DetectSystem.Models;

namespace WH.DetectSystem.DetectSystem.MainModel
{
    public partial class CMainModelsModel
    {
        
        List<CMainModel> cMainModels = new List<CMainModel>();
        public List<CMainModel> CMainModels
        {
            get => cMainModels;
            set => cMainModels = value;
        }
    }
}
