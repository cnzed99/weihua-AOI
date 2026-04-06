using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public partial class CAutomaticModel: ObservableObject
    {
        /// <summary>
        /// 工位名称
        /// </summary>
        [ObservableProperty]
        string stationName;

        /// <summary>
        /// 刀口长度（cm)
        /// </summary>
        float quekoulenght=3.5f;
        public float QuekouLenght
        {
            get { return quekoulenght; }
            set
            {
                quekoulenght = value;
                ZipperLenght = ShowZipperLenght * 10 + quekoulenght * 10;
                OnPropertyChanged();
            }
        }

        private float showZipperLenght;
        /// <summary>
        /// 显示界面用的拉链长度，单位（cm）
        /// </summary>
        public float ShowZipperLenght
        {
            get { return showZipperLenght; }
            set 
            { 
                showZipperLenght = value;
                ZipperLenght = showZipperLenght * 10+ QuekouLenght * 10;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 拉链长度(单位：mm)
        /// </summary>
        [ObservableProperty]
        float zipperLenght = 0;

        /// <summary>
        /// 相机中心到切刀的中心距
        /// </summary>
        [ObservableProperty]
        float daoDitance = 782;  
        /// <summary>
        /// 相机视野
        /// </summary>
        [ObservableProperty]
        float ccdWidth = 114;


        [ObservableProperty]
        int zipperMinBgMean = 120;
        [ObservableProperty]
        int zipperMaxBgMean = 220;
        [ObservableProperty]
        int zipperMinMean = 90;
        [ObservableProperty]
        int zipperMaxMean = 180;

        [ObservableProperty]
        int pullMinBgMean = 120;
        [ObservableProperty]
        int pullMaxBgMean = 220;
        [ObservableProperty]
        int pullMinMean = 90;
        [ObservableProperty]
        int pullMaxMean = 180;
        //[ObservableProperty]
        //int walkBackLenght_slow = 400;
        //[ObservableProperty]
        //int walkBackLenght_qiuk = 1000;




    }
}
