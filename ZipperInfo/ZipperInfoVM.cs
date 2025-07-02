using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Controls;

namespace ZipperInfo
{
    public  class ZipperInfoVM
    {
         public CZipperInfo ZipperInfo  { get; set; }

        //public ImageView UpMassView { get; set; }
        //public ImageView DownMassView { get; set; }
        //public ImageView PullMassView { get; set; }
        public ZipperInfoVM()
        {
            ZipperInfo= CZipperAutomaticAlgorithm.ZipperInfo;
            //UpMassView=new ImageView();
            //DownMassView=new ImageView();
            //PullMassView=new ImageView();   
        }
    }
}
