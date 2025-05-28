using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace CameraModule
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机参数基类
    /// </summary>
    public partial class CCameraParameterBase : ConfigModifyObservableBase
    {
        public CCameraParameterBase()
        {
            this.token = new Token("", "CameraModule"); //固定token,使得插件参数更改可以通知到管理类的消息处理函数
        }

        public CCameraParameterBase(string serialnumber, string cameraSupplier)
        {
            this.token = new Token("", "CameraModule"); //固定token,使得插件参数更改可以通知到管理类的消息处理函数
            SerialNumber = serialnumber;
            CameraSupplier = cameraSupplier;
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 是否已连接
        /// </summary>
        [Category("通用参数")]
        [DisplayName("01.是否已连接")]
        [Description("01.是否已连接")]
        [JsonIgnore]
        public bool Connected
        {
            get
            {
                if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                {
                    return CCameraManagement.CameraDict[SerialNumber].Connected;
                }

                return false;
            }
            set
            {
                if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                {
                    var cam = CCameraManagement.CameraDict[SerialNumber];
                    if (value && !cam.Connected)
                    {
                        cam.InitializeCamera();
                    }
                    else if (!value && cam.Connected)
                    {
                        cam.EndCamera();
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机序列号
        /// </summary>



        private string serialNumber = "";

        [property: Category("通用参数")]
        [property: DisplayName("02.相机序列号")]
        [property: Description("02.相机序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        // [ObservableProperty]
        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                string old = serialNumber;
                SetProperty(ref serialNumber, value);
                if (old != serialNumber)
                {
                    try
                    {
                        if (CCameraManagement.CameraDict.ContainsKey(old))
                        {
                            if (
                                !CCameraManagement.CameraDict.Keys.Contains(serialNumber)
                                && !CCameraManagement.CamParamDict.Keys.Contains(serialNumber)
                            )
                            {
                                CCameraBase Camtemp = CCameraManagement.CameraDict[old];
                                //Camtemp.SerialNumber = serialNumber;
                                CCameraParameterBase paramtemp = CCameraManagement.CamParamDict[
                                    old
                                ];

                                CCameraManagement.CameraDict.Add(serialNumber, Camtemp);
                                CCameraManagement.CamParamDict.Add(serialNumber, paramtemp);

                                CCameraManagement.CameraDict[old].EndCamera(); //CloseCamera修改为EndCamera

                                CCameraManagement.CameraDict.Remove(old);
                                CCameraManagement.CamParamDict.Remove(old);
                                var cam = CCameraManagement.CameraDict[serialNumber];
                                if (!cam.Connected)
                                {
                                    cam.InitializeCamera();
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机品牌
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("03.相机品牌")]
        [property: Description("03.相机品牌")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private string cameraSupplier;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 启用本相机
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("04.启用本相机")]
        [property: Description("04.启用本相机")]
        [ObservableProperty]
        private bool enable = true;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机名
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("05.相机名")]
        [property: Description("05.相机名")]
        [ObservableProperty]
        private string name = "工位";

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 像素格式
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("06.像素格式")]
        [property: Description("06.像素格式")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private PixelFormat cameraType = PixelFormats.Rgb24;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图片旋转
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("07.图片旋转")]
        [property: Description("07.图片旋转")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        [ObservableProperty]
        private EMIMAGEROTATE imageRotate;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像宽度(像素)
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("08.图像宽度(像素)")]
        [property: Description("08.图像宽度(像素)")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private int imageWidth;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 图像高度(像素)
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("09.图像高度(像素)")]
        [property: Description("09.图像高度(像素)")]
        [property: ReadOnly(true)]
        [ObservableProperty]
        private int imageHeight;

        private uint exposureTime = 10;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 曝光时间(us)
        /// </summary>
        [Category("通用参数")]
        [DisplayName("10.曝光时间(us)")]
        [Description("10.曝光时间(us)")]
        public uint ExposureTime
        {
            get { return exposureTime; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetExposureTime(value);
                        if (
                            CCameraManagement
                                .CameraDict[SerialNumber]
                                .GetExposureTime(out var getValue)
                        )
                        {
                            SetProperty(ref exposureTime, getValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref exposureTime, value);
                }
            }
        }

        private float gain = 10.0f;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 增益
        /// </summary>
        [Category("通用参数")]
        [DisplayName("11.增益")]
        [Description("11.增益")]
        public float Gain
        {
            get { return gain; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetGain(value);
                        if (CCameraManagement.CameraDict[SerialNumber].GetGain(out var getValue))
                        {
                            SetProperty(ref gain, getValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref gain, value);
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发模式
        /// </summary>
        private EMTRIGGERMODE triggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发模式
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("12.触发模式")]
        [property: Description("12.触发模式")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        public EMTRIGGERMODE TriggerMode
        {
            get { return triggerMode; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetTriggerModePro(value);
                        if (
                            CCameraManagement
                                .CameraDict[SerialNumber]
                                .GetTriggerMode(out var getValue)
                        )
                            SetProperty(ref triggerMode, getValue);
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref triggerMode, value);
                }
            }
        }

        private uint triggerDelay;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发延时
        /// </summary>
        [Category("通用参数")]
        [DisplayName("13.触发延时")]
        [Description("13.触发延时")]
        public uint TriggerDelay
        {
            get { return triggerDelay; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetTriggerDelay(value);
                        if (
                            CCameraManagement
                                .CameraDict[SerialNumber]
                                .GetTriggerDelay(out var getValue)
                        )
                            SetProperty(ref triggerDelay, getValue);
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref triggerDelay, value);
                }
            }
        }

        private float gamma = 1f;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 伽马值
        /// </summary>
        [Category("通用参数")]
        [DisplayName("14.伽马值")]
        [Description("14.伽马值")]
        public float Gamma
        {
            get { return gamma; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetGamma(value);
                        if (CCameraManagement.CameraDict[SerialNumber].GetGamma(out var getValue))
                            SetProperty(ref gamma, getValue);
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref gamma, value);
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 像素当量(mm)
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("15.像素当量(mm)")]
        [property: Description("15.像素当量(mm)")]
        [ObservableProperty]
        private double mmPerPixel = 0.00225;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 单帧超时时间(ms)
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("16.单帧超时时间(ms)")]
        [property: Description("16.单帧超时时间(ms)")]
        [ObservableProperty]
        private int timeOut = 2000;

        private uint triggerPulseWidth = 10;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发脉冲宽度
        /// </summary>
        [Category("通用参数")]
        [DisplayName("17.触发脉冲宽度")]
        [Description("17.触发脉冲宽度")]
        public uint TriggerPulseWidth
        {
            get { return triggerPulseWidth; }
            set
            {
                if (Connected)
                {
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetTriggerPulseWidth(value);
                        if (
                            CCameraManagement
                                .CameraDict[SerialNumber]
                                .GetTriggerPulseWidth(out var getValue)
                        )
                            SetProperty(ref triggerPulseWidth, getValue);
                    }
                    catch (Exception ex)
                    {
                        Growl.Error(ex.Message);
                    }
                }
                else if (CCameraManagement.s_IsLoadParam)
                {
                    SetProperty(ref triggerPulseWidth, value);
                }
            }
        }

        /// <summary>
        /// 2025.05.26 鲍赞宝
        /// 像素当量(mm)
        /// </summary>
        [property: Category("通用参数")]
        [property: DisplayName("18.拼接图片数量")]
        [property: Description("18.拼接图片数量")]
        [ObservableProperty]
        private int camCount = 3;

        /// <summary>
        /// 所属制程
        /// </summary>
        public string ProjGuid { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// ToString
        /// </summary>
        /// <returns>ToString</returns>
        public override string ToString()
        {
            return $"{SerialNumber}-{Name}";
        }
    }

    ///// <summary>
    ///// 2024.7.17 李焕彬
    ///// 相机类型
    ///// </summary>
    //public enum EMCAMERATYPE
    //{
    //    /// <summary>
    //    /// 2024.7.17 李焕彬
    //    /// 黑白
    //    /// </summary>
    //    [EnumString("黑白", "GRAY")]
    //    EMCAMTYPEGRAY,

    //    /// <summary>
    //    /// 2024.7.17 李焕彬
    //    /// 彩色
    //    /// </summary>
    //    [EnumString("彩色", "COLOR")]
    //    EMCAMTYPECOLOR,
    //}

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 旋转角度
    /// </summary>
    public enum EMIMAGEROTATE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 0度
        /// </summary>
        [EnumString("0度", "0")]
        EMROTATE0 = 0,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 90度
        /// </summary>
        [EnumString("90度", "90")]
        EMROTATE90 = 1,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 180度
        /// </summary>
        [EnumString("180度", "180")]
        EMROTATE180 = 2,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 270度
        /// </summary>
        [EnumString("270度", "270")]
        EMROTATE270 = 3,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 触发模式
    /// </summary>
    public enum EMTRIGGERMODE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 无触发
        /// </summary>
        [EnumString("无触发", "NONE")]
        EMTRIGGERNONE,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 软触发
        /// </summary>
        [EnumString("软触发", "SOFTWARE")]
        EMTRIGGERSOFTWARE,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 硬触发
        /// </summary>
        [EnumString("硬触发", "HARDWARE")]
        EMTRIGGERHARDWARE,
    }

    public class CComboxEditorPro : PropertyEditorBase
    {
        PropertyItem _propertyItem;

        HandyControl.Controls.ComboBox comboBox;

        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            _propertyItem = propertyItem;
            var proptemp = propertyItem.Value as CCameraParameterBase;
            if (proptemp != null)
            {
                comboBox = new HandyControl.Controls.ComboBox();
                comboBox.ItemsSource = new List<string>() { proptemp?.SerialNumber };
                comboBox.Width = 293;
                comboBox.Margin = new Thickness(-120, 0, 0, 0);
                comboBox.DropDownOpened += ComboBox_DropDownOpened;
                comboBox.VerticalAlignment = VerticalAlignment.Bottom;
                comboBox.HorizontalAlignment = HorizontalAlignment.Left;

                comboBox.SetBinding(
                    HandyControl.Controls.ComboBox.SelectedItemProperty,
                    new Binding("SerialNumber") { Mode = BindingMode.TwoWay, Source = propertyItem }
                );
            }
            return comboBox;
        }

        public override DependencyProperty GetDependencyProperty() =>
            HandyControl.Controls.ComboBox.SelectedItemProperty;

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            List<string> camlist = new List<string>();
            if (_propertyItem != null)
            {
                CCameraParameterBase cam = _propertyItem.Value as CCameraParameterBase;
                if (cam != null)
                {
                    string suppli = cam.CameraSupplier;
                    var camsupplis = CCameraManagement.CameraHelpers.Where(c => c.Key == suppli);

                    foreach (var camFunc in camsupplis)
                    {
                        try
                        {
                            List<WHCameraInfo> whinfolist = camFunc.Value.EnumCamrea();
                            for (int i = 0; i < whinfolist.Count; i++)
                            {
                                camlist.Add(whinfolist[i].SerialNumber);
                            }
                        }
                        catch (Exception) { }
                    }
                }
                comboBox.ItemsSource = camlist;
            }
        }
    }
}
