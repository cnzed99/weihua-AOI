using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 断面毛刺检测软件.Models
{
    public partial class SystemSettingsModel:ObservableObject
    {

        public ObservableCollection<string> RecentProjs { get; set; } = new ObservableCollection<string>() { "C:\\Users\\Mainvm.Json", "C:\\Users\\Mainvm233.Json" };
    }
}
