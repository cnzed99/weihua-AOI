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
using HalconDotNet;

namespace ZipperInfo
{
    public partial class CZipperInfo : ObservableObject
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
                if (TempData1?.AutoData != null)
                {
                    ZipperLneght = value * 10 + TempData1.AutoData.QuekouLenght * 10;
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
                if (TempData1 != null&& TempData1.AutoData!=null)
                {
                    TempData1.AutoData.ZipperLenght = value;
                    CGetZipperTriggerPoint.GetTriggerPoints(TempData1.AutoData, out List<float> points, out List<float> handandtalipoints, out int cutoffIndex, out int zipperCacheCount, out _);
                    TempData1.ZipperTriggerPos = points;
                    TempData1.HandAndTaliPos = handandtalipoints;
                    CGetZipperTriggerPoint.CheckPullPos(points);
                    CZipperCommunicate.SendZipperLenght(TempData1.AutoData.ZipperLenght);
                    //写入拍照的总图片数量
                    CZipperCommunicate.SendPhotoCount(points.Count);
                    //计算拉链触发点位 ID改变位置
                    CZipperCommunicate.SendPoints(points, handandtalipoints, cutoffIndex, zipperCacheCount);
                }

            }
        }
        TOOTHTYPE zipperToothType = TOOTHTYPE._3号;
        /// <summary>
        /// 链牙型号(大小)
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("02.链牙型号")]
        [property: Description("链牙型号(大小)")]
        [property: Browsable(true)]
        public TOOTHTYPE ZipperToothType
        {
            get { return zipperToothType; }
            set
            {
                zipperToothType = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }

            }
        }


        PULLTYPE zipperSliderType = PULLTYPE.正穿;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("03.穿拉头方式")]
        [property: Description("穿拉头方式")]
        [property: Browsable(true)]
        public PULLTYPE ZipperSliderType
        {
            get { return zipperSliderType; }
            set
            {
                zipperSliderType = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }
            }
        }

        STOPMASS zipperUpMassType = STOPMASS.注塑;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("04.上止类型")]
        [property: Description("上止类型")]
        [property: Browsable(true)]
        public STOPMASS ZipperUpMassType
        {
            get { return zipperUpMassType; }
            set
            {
                zipperUpMassType = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }
            }
        }


        STOPMASS zipperDownMassType = STOPMASS.注塑;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("05.下止类型")]
        [property: Description("下止类型")]
        [property: Browsable(true)]

        public STOPMASS ZipperDownMassType
        {
            get { return zipperDownMassType; }
            set
            {
                zipperDownMassType = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }
            }
        }

        string zipperLogoType = "SBS";
        /// <summary>
        /// logo类型
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("06.logo类型")]
        [property: Description("logo类型")]
        [property: Browsable(true)]
        public string ZipperLogoType
        {
            get { return zipperLogoType; }
            set
            {
                zipperLogoType = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }
            }
        }
        private bool whiteZippers;
        /// <summary>
        /// 是否是白色拉链，是则加严参数
        /// 2025.11.1 鲍赞宝
        /// </summary>
        public bool WhiteZippers
        {
            get { return whiteZippers; }
            set
            {
                whiteZippers = value;
                if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
                {
                    CZipperAutomaticAlgorithm.Instance.TestFinshEven?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// 拉头的材质类型
        /// </summary>
        public PULLMATERIALSTYPE PullMaterlsType { get; set; }
        /// <summary>
        /// 左右相机工位临时参数
        /// 2026.4.6 鲍赞宝
        /// </summary>
        public CZipperTempData TempData1 { get; set; } = new CZipperTempData();
        /// <summary>
        /// 上下相机工位临时参数
        /// 2026.4.6 鲍赞宝
        /// </summary>
        public CZipperTempData TempData2 { get; set; }=new CZipperTempData();


    }


    public partial class CZipperTempData : ObservableObject
    {
        /// <summary>
        /// 拉链拍照触发的位置
        /// 2025.06.24 鲍赞宝
        /// </summary>
        public List<float> ZipperTriggerPos { get; set; } = new List<float>();
        /// <summary>
        /// 拉链拍照的总张数
        /// </summary>
        public int ZipperImagesCount { get; set; }

        /// <summary>
        /// 拉头ID拍照位置的序号
        /// </summary>
        public int PullchangeIndex { get; set; }
        /// <summary>
        /// 切断前共拍了几张图片
        /// </summary>
        public int CutoffIndex { get; set; }

        /// <summary>
        /// 拉链触发的点位属于哪一类
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
        /// 识别出来的Logo图像
        /// 2025.06.30 鲍赞宝
        /// </summary>
        [JsonIgnore]
        public BitmapSource ZipperLogoImg { get; set; }
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
        public CAutomaticModel AutoData { get; set; }

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
        public int FindLogoSider { get; set; }
        /// <summary>
        /// 拉片分割出来的标准面积
        /// </summary>
        [ObservableProperty]
        float pullSegOrgArea;

        /// <summary>
        /// 拉片外形轮廓点集合
        /// </summary>
        public OpenCvSharp.Point[] OrgContours { get; set; }
        /// <summary>
        /// 拉片孔轮廓点集合
        /// </summary>
        public OpenCvSharp.Point[] HoleOrgContours { get; set; }
        /// <summary>
        /// 拉头的H值
        /// </summary>
        [ObservableProperty]
        float pullerMeanH;
        /// <summary>
        /// 拉头的S值
        /// </summary>

        [ObservableProperty]
        float pullerMeanS;
        /// <summary>
        /// 拉头的V值
        /// </summary>
        [ObservableProperty]
        float pullerMeanV;
        /// <summary>
        /// 拉片的H值
        /// </summary>
        [ObservableProperty] 
        float pullsMeanH;
        /// <summary>
        /// 拉片的S值
        /// </summary>
        [ObservableProperty] 
        float pullsMeanS;
        /// <summary>
        /// 拉片的V值
        /// </summary>
        [ObservableProperty]
        float pullsMeanV;
        /// <summary>
        /// Logo模版
        /// </summary>
        [JsonIgnore]
        public HTuple ModelID_Logo { get; set; }
        /// <summary>
        /// 拉片模版
        /// </summary>
        [JsonIgnore]
        public HTuple ModelID_Pull {  get; set; }

        /// <summary>
        /// 拉片模版原始坐标row
        /// </summary>
        [ObservableProperty]
        double pullModelRow;
        /// <summary>
        /// 拉片模版原始坐标col
        /// </summary>
        [ObservableProperty]
        double pullModelCol;
        /// <summary>
        /// 原始背景矩形
        /// </summary>
        public double[] BackRectangle;
        /// <summary>
        /// 拉头有无SAB
        /// </summary>
        public HAVESAB PullerHaveSAB { get; set; } = HAVESAB.有SAB;

        /// <summary>
        /// 上止上牙的H值
        /// </summary>
        [ObservableProperty]
        float upMass_1_MeanH;
        /// <summary>
        /// 上止上牙的S值
        /// </summary>

        [ObservableProperty]
        float upMass_1_MeanS;
        /// <summary>
        /// 上止上牙的V值
        /// </summary>
        [ObservableProperty]
        float upMass_1_MeanV;

        /// <summary>
        /// 上止下牙的H值
        /// </summary>
        [ObservableProperty]
        float upMass_2_MeanH;
        /// <summary>
        /// 上止下牙的S值
        /// </summary>

        [ObservableProperty]
        float upMass_2_MeanS;
        /// <summary>
        /// 上止下牙的V值
        /// </summary>
        [ObservableProperty]
        float upMass_2_MeanV;

    }


    public enum TOOTHTYPE
    {
        _3号 = 0,
        _5号 = 1,
        _7号 = 2,
        _8号 = 3
    }
    public enum PULLTYPE
    {
        正穿 = 0,
        反穿 = 1
    }

    public enum STOPMASS
    {
        注塑 = 0,
        白铝 = 1,
        烤漆 = 2,
        透明 = 3,
        U型尼龙 = 4,
        融止 = 5,
        隐形 = 6,
        金属=7,
        树脂=8,
        无 = 7
    }

    public enum PULLMATERIALSTYPE
    {
        烤漆 = 0,
        金属 = 1
    }

    public enum BOLTDIRECTION
    {
        左插 = 0,
        右插 = 1
    }
    /// <summary>
    /// 插销有无SAB
    /// </summary>
    public enum HAVESAB
    {
        有SAB = 0,
        无SAB = 1
    }
    /// <summary>
    /// 拉片是否有膜
    /// </summary>
    public enum  PULLSHAVEFILM
    {
        无膜 = 0,
        有膜 = 1
    }

}
