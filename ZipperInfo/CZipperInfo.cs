using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using WH.RecipeCellRootBase;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace ZipperInfo
{
    public partial class CZipperInfo: ObservableObject
    {

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
                if(AutoData!= null)
                    {
                    ZipperLneght = value * 10 + AutoData.QuekouLenght * 10;
                }
                
                OnPropertyChanged();
            }
        }


        private float zipperLneght;
        /// <summary>
        /// 拉链长度 单位mm
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("01.拉链长度(mm)")]
        [property: Description("拉链长度,单位mm")]
        [property: Browsable(true)]
        public float ZipperLneght
        {
            get { return zipperLneght; }
            set
            { 
                zipperLneght = value; 
              //  OnPropertyChanged();
                if (AutoData != null)
                {
                    AutoData.ZipperLenght = value;
                    CGetZipperTriggerPoint.GetTriggerPoints(AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount,out _);
                    ZipperTriggerPos = points;
                    HandAndTaliPos = handandtalipoints;
                    CZipperCommunicate.SendZipperLenght(AutoData.ZipperLenght);
                    //写入拍照的总图片数量
                    CZipperCommunicate.SendPhotoCount(points.Count);
                    //计算拉链触发点位 ID改变位置
                    CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);
                }
              
            }
        }

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
        string zipperLogoType = "SBS";
        /// <summary>
        /// 拉链拍照触发的位置
        /// 2025.06.24 鲍赞宝
        /// </summary>
        public List<float> ZipperTriggerPos { get; set; }=new List<float>();
        /// <summary>
        /// 拉头ID拍照位置的序号
        /// </summary>
        public int PullchangeIndex {  get; set; }
        /// <summary>
        /// 切断前共拍了几张图片
        /// </summary>
        public int CutoffIndex { get; set; }

        /// <summary>
        /// 拉链触发的点累属于哪一类
        /// </summary>
        public int TriggerType { get; set; }

        /// <summary>
        /// 拉链自动识别时的上下限位置
        /// 2025.06.30 鲍赞宝
        /// </summary>
        public List<float> HandAndTaliPos { get; set; } = new List<float>();

        /// <summary>
        /// 识别出来的上止的图像
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [JsonIgnore]
        public BitmapSource ZipperUpmssImg { get; set; }

        /// <summary>
        /// 识别出来的下止的图像
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [JsonIgnore]
        public BitmapSource ZipperDownmssImg { get; set; }

        /// <summary>
        /// 识别出来的正面拉片的图像
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [JsonIgnore]
        public BitmapSource ZipperPullsImg { get; set; }

        /// <summary>
        /// 识别出来的正面拉头的图像
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [JsonIgnore]
        public BitmapSource ZipperPullerImg { get; set; }
        /// <summary>
        /// 拉头识别框中心X
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [ObservableProperty]
        public int zipperPullerCX;
        /// <summary>
        /// 拉头识别框中心Y
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [ObservableProperty]
        public int zipperPullerCY;
        /// <summary>
        /// 拉链外部参数
        /// 2025.06.30 鲍赞宝
        /// </summary>
        public CAutomaticModel AutoData {  get; set; }
        /// <summary>
        /// 是否是白色拉链，是则加严参数
        /// 2025.11.1 鲍赞宝
        /// </summary>
        [ObservableProperty]
        public bool whiteZippers;
        /// <summary>
        /// Logo文字集合
        /// 2025.11.04 鲍赞宝
        /// </summary>
        [ObservableProperty]
        public string[] logoTypeStrs;
        /// <summary>
        /// 在那一面找到Logo 0:两面都没找到， 1:在拉片面找到 ，2：在两面都找到， 3：在拉头面找到
        /// 2025.11.13 鲍赞宝
        /// </summary>
        public  int FindLogoSider {  get; set; }

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
    //public enum LOGOTYPE
    //{
    //    SBS=0,
    //    ANTA=1,
    //    单包=2,
    //    无Logo=3,
    //    Kith,
    //    Oneills,
    //    JAKO,
    //    ONLY
    //}

}
