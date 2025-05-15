using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using NetSDKCS;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace DahuaThermalCam
{
    /// <summary>
    /// 20240802 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CParameterSetting : CCameraParameterBase
    {
        public CParameterSetting()
            : base() { }

        public CParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// ip
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("IP")]
        [property: Description("IP")]
        private string ip = "192.168.1.108";

        /// <summary>
        /// 2025.5.13 李焕彬
        /// port
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("Port")]
        [property: Description("Port")]
        private ushort port = 37777;

        /// <summary>
        /// 2025.5.13 李焕彬
        /// User
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("User")]
        [property: Description("User")]
        private string user = "admin";

        /// <summary>
        /// 2025.5.13 李焕彬
        /// Password
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("Password")]
        [property: Description("Password")]
        private string password = "q123456789";

        private EM_THERMO_COLORIZATION colorization = EM_THERMO_COLORIZATION.RAINBOW;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 伪色彩
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("1.伪色彩")]
        [property: Description("1.伪色彩")]
        public EM_THERMO_COLORIZATION Colorization
        {
            get { return colorization; }
            set
            {
                SetProperty(ref colorization, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetPalette();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref colorization, value);
                }
            }
        }

        private uint brightness = 50;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 亮度
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("2.亮度")]
        [property: Description("2.亮度")]
        public uint Brightness
        {
            get { return brightness; }
            set
            {
                SetProperty(ref brightness, value);
                if (Connected)
                {
                    try
                    {
                        (
                            (CCamera)CCameraManagement.CameraDict[SerialNumber]
                        ).SetBrightAndContrast();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref brightness, value);
                }
            }
        }

        private uint contrast = 50;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 对比度
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("3.对比度")]
        [property: Description("3.对比度")]
        public uint Contrast
        {
            get { return contrast; }
            set
            {
                SetProperty(ref contrast, value);
                if (Connected)
                {
                    try
                    {
                        (
                            (CCamera)CCameraManagement.CameraDict[SerialNumber]
                        ).SetBrightAndContrast();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref contrast, value);
                }
            }
        }

        private uint sharpness = 60;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 锐度
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("4.锐度")]
        [property: Description("4.锐度")]
        public uint Sharpness
        {
            get { return sharpness; }
            set
            {
                SetProperty(ref sharpness, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetSharpness();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref sharpness, value);
                }
            }
        }

        private uint detailEnhancer = 60;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 细节增强
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("5.细节增强")]
        [property: Description("5.细节增强")]
        public uint DetailEnhancer
        {
            get { return detailEnhancer; }
            set
            {
                SetProperty(ref detailEnhancer, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetDetail();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref detailEnhancer, value);
                }
            }
        }
    }
}
