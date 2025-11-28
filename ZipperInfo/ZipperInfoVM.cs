using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            try
            {
                CZipperCommunicate.CamTriggerStop(); //停止拍照
                CZipperCommunicate.SendHelianStastPos(80);
                CZipperAutomaticAlgorithm.TestFinshEven(true);
                CZipperAutomaticAlgorithm.SaveParameter(ZipperInfo);
                CZipperCommunicate.AixtContinue(true);
                CZipperCommunicate.TestFinish();
               // CZipperCommunicate.SendWolkBack(ZipperInfo.AutoData.WalkBackLenght_qiuk);
                var window = win as Window;
                window.Close();
            }
            catch (Exception)
            {
                var window = win as Window;
                window.Close();
            }
  
        }
    }
}
