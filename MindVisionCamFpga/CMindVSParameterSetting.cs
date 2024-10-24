using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using MVSDK;
using WH.Controls;
using WH.Entity.Attribute;

namespace MindVisionCamFpga
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CMindVSParameterSetting : CCameraParameterBase
    {
        public CMindVSParameterSetting()
            : base() { }

        public CMindVSParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层自适应阈值邻域大小")]
        [property: Description("铝层自适应阈值说明")]
        private uint adaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层自适应阈值增加值")]
        [property: Description("铝层自适应阈值说明")]
        private int adaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层过滤矩阵邻域大小")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层过滤矩阵邻域点数量限制")]
        [property: Description("铝层过滤矩阵说明")]
        private uint neighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("料区阈值")]
        [property: Description("料区阈值说明")]
        private uint darkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层阈值")]
        [property: Description("铝层阈值说明")]
        private uint lightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度限制，掉料检测
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("料区厚度限制(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThickLimit = 67.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("料区厚度NG连续长度限制(um)")]
        [property: Description("料区NG连续长度说明")]
        private double darkThickContinueLen = 11.25;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("料区厚度(um)")]
        [property: Description("料区厚度限制说明")]
        private double darkThick = 189;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度限制，毛刺检测
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层厚度限制(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThickLimit = 15.75;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度NG连续长度限制
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层厚度NG连续长度限制(um)")]
        [property: Description("铝层NG连续长度说明")]
        private double lightThickContinueLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层厚度(um)")]
        [property: Description("铝层厚度限制说明")]
        private double lightThick = 13.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制上
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层在料区中心位置限制上(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitT = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层在料区中心位置限制下
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层在料区中心位置限制下(um)")]
        [property: Description("料区中心位置限制说明")]
        private double posLimitB = 45;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层位置偏移值
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("铝层位置偏移值(um)")]
        [property: Description("料区中心位置限制说明")]
        private double lightPosOffest = 0;
    }
}
