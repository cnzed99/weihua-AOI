using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using WH.RecipeCellRootBase;
using System.Windows.Media.Imaging;

namespace ZipperInfo
{
    public partial class CZipperInfo: ObservableObject
    {
        /// <summary>
        /// 拉链长度
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("01.拉链长度(cm)")]
        [property: Description("拉链长度,单位cm")]
        [property: Browsable(true)]
        [ObservableProperty]
        float zipperLneght = 0.0f;
        /// <summary>
        /// 链牙型号(大小)
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("02.链牙型号")]
        [property: Description("链牙型号(大小)")]
        [property: Browsable(true)]
        [ObservableProperty]
        TOOTHTYPE zipperToothType = TOOTHTYPE._3号;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("03.穿拉头方式")]
        [property: Description("穿拉头方式")]
        [property: Browsable(true)]
        [ObservableProperty]
        PULLTYPE zipperSliderType=PULLTYPE.正穿;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("04.上止类型")]
        [property: Description("上止类型")]
        [property: Browsable(true)]
        [ObservableProperty]
        STOPMASS zipperUpMassType = STOPMASS.注塑;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("05.下止类型")]
        [property: Description("下止类型")]
        [property: Browsable(true)]
        [ObservableProperty]
        STOPMASS zipperDownMassType = STOPMASS.注塑;
        /// <summary>
        /// logo类型
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("06.logo类型")]
        [property: Description("logo类型")]
        [property: Browsable(true)]
        [ObservableProperty]
        LOGOTYPE zipperLogoType = LOGOTYPE.SBS;
        /// <summary>
        /// 拉链拍照触发的位置
        /// 2025.06.24 鲍赞宝
        /// </summary>
        public List<float> ZipperTriggerPos { get; set; }=new List<float>();
        /// <summary>
        /// 拉头ID改变的位置序号
        /// </summary>
        public int PullchangeIndex {  get; set; }
        /// <summary>
        /// 切断前共拍了几张图片
        /// </summary>
        public int CutoffIndex { get; set; }

        /// <summary>
        /// 拉链自动识别时的上下限位置
        /// 2025.06.30 鲍赞宝
        /// </summary>
        public List<float> HandAndTaliPos { get; set; } = new List<float>();

        /// <summary>
        /// 识别出来的上止的图像
        /// </summary>
        [ObservableProperty]
        public BitmapSource zipperUpmssImg;

        /// <summary>
        /// 识别出来的下止的图像
        /// </summary>
        [ObservableProperty]
        public BitmapSource zipperDownmssImg;

        /// <summary>
        /// 识别出来的正面拉片的图像
        /// </summary>
        [ObservableProperty]
        public BitmapSource zipperPullsImg;

        /// <summary>
        /// 识别出来的正面拉头的图像
        /// </summary>
        [ObservableProperty]
        public BitmapSource zipperPullerImg;


    }


    public enum TOOTHTYPE
    {
        _3号=0,
        _5号 = 1,
        _7号 = 2,
        _8号 = 3
    }
    public enum PULLTYPE
    {
        正穿=0,
        反穿= 1
    }

    public enum STOPMASS
    { 
        注塑=0,
        白铝=1,
        烤漆=2,
        透明=3,
        U型尼龙=4,
        融止=5,
        隐形=6,
        无=7
    }
    public enum LOGOTYPE
    {
        SBS=0,
        ANTA=1
    }

}
