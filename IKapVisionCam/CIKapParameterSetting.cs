using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Controls;
using WH.Entity.Attribute;

namespace IKapVisionCam
{
    /// <summary>
    /// 2025.1.14 李焕彬
    /// 初筛算法
    /// </summary>
    public enum EMPREALGORITHM
    {
        [EnumString("断面算法", "Transect")]
        EMPREALGORITHMMAOCI = 0,

        [EnumString("侧面算法", "Side")]
        EMPREALGORITHMSIDEMAOCI = 1,
    }

    /// <summary>
    /// 2025.1.14 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CParameterSetting : CCameraParameterBase
    {
        public CParameterSetting()
            : base() { }

        public CParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        private int totalFrameCount = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机缓冲区帧数
        /// </summary>
        [property: Category("专用参数")]
        [property: DisplayName("相机缓冲区帧数")]
        [property: Description("相机缓冲区帧数")]
        public int TotalFrameCount
        {
            get { return totalFrameCount; }
            set
            {
                if (value > 0)
                {
                    if (Connected)
                    {
                        if (
                            (
                                (CCamera)CCameraManagement.CameraDict[SerialNumber]
                            ).SetBufferFrameCount(value)
                        )
                        {
                            SetProperty(ref totalFrameCount, value);
                        }
                    }
                    else
                    {
                        SetProperty(ref totalFrameCount, value);
                    }
                }
            }
        }

        private int interTriggerFrequence = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 内触发频率
        /// </summary>
        [property: Category("专用参数")]
        [property: DisplayName("内触发频率")]
        [property: Description("内触发频率")]
        public int InterTriggerFrequence
        {
            get { return interTriggerFrequence; }
            set
            {
                if (value > 0)
                {
                    if (Connected)
                    {
                        if (
                            (
                                (CCamera)CCameraManagement.CameraDict[SerialNumber]
                            ).SetInternalTrigFreq(value)
                        )
                        {
                            SetProperty(ref interTriggerFrequence, value);
                        }
                    }
                    else
                    {
                        SetProperty(ref interTriggerFrequence, value);
                    }
                }
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 更新初筛参数
        /// </summary>
        [property: Category("专用参数")]
        [property: DisplayName("更新初筛参数")]
        [property: Description("更新初筛参数")]
        public bool UpdateFilterParam
        {
            get { return false; }
            set
            {
                if (Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateFilterParam();
                }
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// OK显示帧率
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("OK显示帧率")]
        [property: Description("OK显示帧率,几秒显示一张")]
        private int oKFrameTicks = 1;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 启用初筛算法
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("启用初筛")]
        [property: Description("启用初筛")]
        private bool useFilter = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 初筛算法
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("初筛算法")]
        [property: Description("初筛算法")]
        private EMPREALGORITHM preAlgorithm = EMPREALGORITHM.EMPREALGORITHMMAOCI;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 超时时间ms
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("初筛超时时间(ms)")]
        [property: Description("超时时间说明")]
        private uint preTimeOut = 3000;

        private bool saveImage = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 保存图片
        /// </summary>
        [property: Category("专用参数")]
        [property: DisplayName("保存图片")]
        [property: Description("保存图片")]
        public bool SaveImage
        {
            get { return saveImage; }
            set
            {
                SetProperty(ref saveImage, value);
                if (Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).StartSaveImage();
                }
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层自适应阈值邻域大小")]
        [property: Description("铝层自适应阈值说明")]
        private uint adaptiveSize = 14;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层自适应阈值增加值")]
        [property: Description("铝层自适应阈值说明")]
        private int adaptiveAddGray = 20;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层过滤矩阵邻域大小")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbSize = 5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层过滤矩阵邻域点数量限制")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbLightPoint = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值说明")]
        private uint darkThresh = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值说明")]
        private uint lightThresh = 80;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区厚度限制，掉料检测
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("料区厚度限制(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThickLimit = 67.5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("料区厚度NG连续长度限制(um)")]
        [property: Description("料区NG连续长度说明")]
        private double darkThickContinueLen = 11.25;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("料区厚度(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThick = 189;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层厚度限制，毛刺检测
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层厚度限制(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThickLimit = 15.75;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层厚度NG连续长度限制(um)")]
        [property: Description("铝层NG连续长度说明")]
        private double lightThickContinueLen = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层厚度(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThick = 13.5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层在料区中心位置限制上
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层在料区中心位置限制上(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitT = 45;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层在料区中心位置限制下
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层在料区中心位置限制下(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitB = 45;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 铝层位置偏移值
        /// </summary>
        [ObservableProperty]
        [property: Category("断面初筛算法")]
        [property: DisplayName("铝层位置偏移值(um)")]
        [property: Description("料区中心位置限制说明")]
        private double lightPosOffest = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("侧面初筛算法")]
        [property: DisplayName("过滤矩阵邻域大小")]
        [property: Description("过滤矩阵邻域大小")]
        private uint neighbSizeSide = 5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("侧面初筛算法")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值")]
        private uint darkThreshSide = 140;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 毛刺斜率限制
        /// </summary>
        [ObservableProperty]
        [property: Category("侧面初筛算法")]
        [property: DisplayName("毛刺斜率限制(um)")]
        [property: Description("毛刺斜率限制(um)")]
        double maociLimit = 14;
    }
}
