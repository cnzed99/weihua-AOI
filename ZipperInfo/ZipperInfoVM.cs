using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WH.Controls;

namespace ZipperInfo
{
    public partial class ZipperInfoVM: ObservableObject
    {
        //  public CZipperInfo ZipperInfo  { get; set; }

        [ObservableProperty]
        CZipperInfo zipperInfo;
        public ZipperInfoVM()
        {
            ZipperInfo= CZipperAutomaticAlgorithm.ZipperInfo;
        }

        [RelayCommand]
        void save(object win)
        {
            CZipperAutomaticAlgorithm.SaveParameter(ZipperInfo);
            var window = win as Window;
            window.Close(); 
        }
    }
}
