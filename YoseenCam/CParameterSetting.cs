using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using YoseenSDKCS;

namespace YoseenCam
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

        private xxxdatatype dataType = xxxdatatype.video;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 预览流类型
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("1.预览流类型")]
        [property: Description("1.预览流类型")]
        public xxxdatatype DataType
        {
            get { return dataType; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        if (
                            ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetPriviewType(
                                value
                            )
                        )
                        {
                            SetProperty(ref dataType, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref dataType, value);
                }
            }
        }

        private xxxpalette palette = xxxpalette.IronBow;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 调色板
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("2.调色板")]
        [property: Description("2.调色板")]
        public xxxpalette Palette
        {
            get { return palette; }
            set
            {
                SetProperty(ref palette, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref palette, value);
                }
            }
        }

        private strech_type strech_type = strech_type.STRECH_TYPE_PHE;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 图像算法
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("3.图像算法")]
        [property: Description("3.图像算法")]
        public strech_type Strech_type
        {
            get { return strech_type; }
            set
            {
                SetProperty(ref strech_type, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref strech_type, value);
                }
            }
        }

        private byte dde_level = 0;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// DDE等级
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("4.DDE等级")]
        [property: Description("4.DDE等级")]
        public byte Dde_level
        {
            get { return dde_level; }
            set
            {
                SetProperty(ref dde_level, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref dde_level, value);
                }
            }
        }

        private float gainSpecial = 2;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 增益
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("5.增益")]
        [property: Description("5.增益")]
        public float GainSpecial
        {
            get { return gainSpecial; }
            set
            {
                SetProperty(ref gainSpecial, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref gainSpecial, value);
                }
            }
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 内触发帧率
        /// </summary>
        [ObservableProperty]
        [property: Category("专用参数")]
        [property: DisplayName("内触发帧率")]
        [property: Description("内触发帧率,几毫秒显示一张")]
        private int frameTicks = 1;

        private bool enableTempLimit = false;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 启用温宽调节
        /// </summary>
        [Category("温宽调节")]
        [property: DisplayName("1.启用温宽调节")]
        [property: Description("1.启用温宽调节")]
        public bool EnableTempLimit
        {
            get { return enableTempLimit; }
            set
            {
                SetProperty(ref enableTempLimit, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref enableTempLimit, value);
                }
            }
        }

        private float tempLower = 20;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 低温
        /// </summary>
        [Category("温宽调节")]
        [property: DisplayName("2.低温")]
        [property: Description("2.低温，单位为C")]
        public float TempLower
        {
            get { return tempLower; }
            set
            {
                SetProperty(ref tempLower, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref tempLower, value);
                }
            }
        }

        private float tempHigher = 40;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 高温
        /// </summary>
        [Category("温宽调节")]
        [property: DisplayName("3.高温")]
        [property: Description("3.高温，单位为C")]
        public float TempHigher
        {
            get { return tempHigher; }
            set
            {
                SetProperty(ref tempHigher, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref tempHigher, value);
                }
            }
        }

        private ColorTempType ct_type = ColorTempType.None;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 类型
        /// </summary>
        [Category("温度标记")]
        [property: DisplayName("1.类型")]
        [property: Description("1.类型")]
        public ColorTempType Ct_type
        {
            get { return ct_type; }
            set
            {
                SetProperty(ref ct_type, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref ct_type, value);
                }
            }
        }

        private float ct_temp0 = 20;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 标记温度0
        /// </summary>
        [Category("温度标记")]
        [property: DisplayName("2.标记温度0")]
        [property: Description("2.标记温度0，单位为C")]
        public float Ct_temp0
        {
            get { return ct_temp0; }
            set
            {
                SetProperty(ref ct_temp0, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref ct_temp0, value);
                }
            }
        }

        private float ct_temp1 = 40;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 标记温度1
        /// </summary>
        [Category("温度标记")]
        [property: DisplayName("3.标记温度1")]
        [property: Description("3.标记温度1，单位为C")]
        public float Ct_temp1
        {
            get { return ct_temp1; }
            set
            {
                SetProperty(ref ct_temp1, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref ct_temp1, value);
                }
            }
        }

        private CKnownColor ct_color0 = CBrushPro.s_Instance.KnownColors[0];

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 标记颜色0
        /// </summary>
        [Category("温度标记")]
        [property: DisplayName("4.标记颜色0")]
        [property: Description("4.标记颜色0")]
        public CKnownColor Ct_color0
        {
            get { return ct_color0; }
            set
            {
                SetProperty(ref ct_color0, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref ct_color0, value);
                }
            }
        }

        private CKnownColor ct_color1 = CBrushPro.s_Instance.KnownColors[1];

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 标记颜色0
        /// </summary>
        [Category("温度标记")]
        [property: DisplayName("5.标记颜色1")]
        [property: Description("5.标记颜色1")]
        public CKnownColor Ct_color1
        {
            get { return ct_color1; }
            set
            {
                SetProperty(ref ct_color1, value);
                if (Connected)
                {
                    try
                    {
                        ((CCamera)CCameraManagement.CameraDict[SerialNumber]).SetImage();
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref ct_color1, value);
                }
            }
        }
    }

    /// <summary>
    /// 2024.8.6 李焕彬
    /// 预览流类型
    /// </summary>
    public enum EMPREVIEWTYPE
    {
        /// <summary>
        /// 2024.8.6 李焕彬
        /// 视频流
        /// </summary>

        EMPREVIEWTYPEVIDEO = 0,

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 温度流
        /// </summary>

        EMPREVIEWTYPETEMP = 1,
    };
}
