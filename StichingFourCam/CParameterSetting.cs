using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.Controls;
using WH.Entity.Attribute;

namespace StichingFourCam
{
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

        [ObservableProperty]
        [property: Category("结果显示")]
        [property: DisplayName("拼图耗时ms")]
        long processTime = 0;

        [ObservableProperty]
        [property: Category("相机参数")]
        [property: DisplayName("拍照间隔ms")]
        int timeLimit = 200;

        private string serialNumber1;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机1序列号
        /// </summary>
        [property: Category("相机参数")]
        [property: DisplayName("相机1序列号")]
        [property: Description("相机1序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        public string SerialNumber1
        {
            get { return serialNumber1; }
            set { SetProperty(ref serialNumber1, value); }
        }

        private string serialNumber2;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机2序列号
        /// </summary>
        [property: Category("相机参数")]
        [property: DisplayName("相机2序列号")]
        [property: Description("相机2序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        public string SerialNumber2
        {
            get { return serialNumber2; }
            set { SetProperty(ref serialNumber2, value); }
        }

        private string serialNumber3;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机3序列号
        /// </summary>
        [property: Category("相机参数")]
        [property: DisplayName("相机3序列号")]
        [property: Description("相机3序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        public string SerialNumber3
        {
            get { return serialNumber3; }
            set { SetProperty(ref serialNumber3, value); }
        }

        private string serialNumber4;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机4序列号
        /// </summary>
        [property: Category("相机参数")]
        [property: DisplayName("相机4序列号")]
        [property: Description("相机4序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        public string SerialNumber4
        {
            get { return serialNumber4; }
            set { SetProperty(ref serialNumber4, value); }
        }

        private double pixelsizeInMM = 0.232;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("PixelsizeInMM")]
        [property: Description("PixelsizeInMM")]
        public double PixelsizeInMM
        {
            get { return pixelsizeInMM; }
            set
            {
                if (SetProperty(ref pixelsizeInMM, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double fineAdjustmentMatchingWidth = 50;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("FineAdjustmentMatchingWidth ")]
        [property: Description("FineAdjustmentMatchingWidth ")]
        public double FineAdjustmentMatchingWidth
        {
            get { return fineAdjustmentMatchingWidth; }
            set
            {
                if (SetProperty(ref fineAdjustmentMatchingWidth, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double fineAdjustmentMaxShift = 15;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("FineAdjustmentMaxShift ")]
        [property: Description("FineAdjustmentMaxShift ")]
        public double FineAdjustmentMaxShift
        {
            get { return fineAdjustmentMaxShift; }
            set
            {
                if (SetProperty(ref fineAdjustmentMaxShift, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double blendingSeam = 1;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("BlendingSeam")]
        [property: Description("BlendingSeam")]
        public double BlendingSeam
        {
            get { return blendingSeam; }
            set
            {
                if (SetProperty(ref blendingSeam, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double silhouetteMeasureDistance = 100;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("SilhouetteMeasureDistance")]
        [property: Description("SilhouetteMeasureDistance")]
        public double SilhouetteMeasureDistance
        {
            get { return silhouetteMeasureDistance; }
            set
            {
                if (SetProperty(ref silhouetteMeasureDistance, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double silhouetteMeasureLength2 = 200;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("SilhouetteMeasureLength2")]
        [property: Description("SilhouetteMeasureLength2")]
        public double SilhouetteMeasureLength2
        {
            get { return silhouetteMeasureLength2; }
            set
            {
                if (SetProperty(ref silhouetteMeasureLength2, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double silhouetteMeasuresigma = 1;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("SilhouetteMeasuresigma")]
        [property: Description("SilhouetteMeasuresigma")]
        public double SilhouetteMeasuresigma
        {
            get { return silhouetteMeasuresigma; }
            set
            {
                if (SetProperty(ref silhouetteMeasuresigma, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double silhouetteMeasureThreshold = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("SilhouetteMeasureThreshold")]
        [property: Description("SilhouetteMeasureThreshold")]
        public double SilhouetteMeasureThreshold
        {
            get { return silhouetteMeasureThreshold; }
            set
            {
                if (SetProperty(ref silhouetteMeasureThreshold, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double silhouetteMaxTilt = 10;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("SilhouetteMaxTilt ")]
        [property: Description("SilhouetteMaxTilt ")]
        public double SilhouetteMaxTilt
        {
            get { return silhouetteMaxTilt; }
            set
            {
                if (SetProperty(ref silhouetteMaxTilt, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double cylinderRadiusInMM = 15;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("CylinderRadiusInMM ")]
        [property: Description("CylinderRadiusInMM ")]
        public double CylinderRadiusInMM
        {
            get { return cylinderRadiusInMM; }
            set
            {
                if (SetProperty(ref cylinderRadiusInMM, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double labelMinRow = 600;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("LabelMinRow ")]
        [property: Description("LabelMinRow ")]
        public double LabelMinRow
        {
            get { return labelMinRow; }
            set
            {
                if (SetProperty(ref labelMinRow, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private double labelMaxRow = 1700;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("LabelMaxRow")]
        [property: Description("LabelMaxRow")]
        public double LabelMaxRow
        {
            get { return labelMaxRow; }
            set
            {
                if (SetProperty(ref labelMaxRow, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private bool highImageQuality = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("HighImageQuality ")]
        [property: Description("HighImageQuality ")]
        public bool HighImageQuality
        {
            get { return highImageQuality; }
            set
            {
                if (SetProperty(ref highImageQuality, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private bool performFineAdjustment = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("PerformFineAdjustment")]
        [property: Description("PerformFineAdjustment")]
        public bool PerformFineAdjustment
        {
            get { return performFineAdjustment; }
            set
            {
                if (SetProperty(ref performFineAdjustment, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }

        private bool tiledImage = false;

        /// <summary>
        /// 2025.1.14 李焕彬
        ///
        /// </summary>
        [property: Category("拼图算法参数")]
        [property: DisplayName("TiledImage")]
        [property: Description("TiledImage")]
        public bool TiledImage
        {
            get { return tiledImage; }
            set
            {
                if (SetProperty(ref tiledImage, value) && Connected)
                {
                    ((CCamera)CCameraManagement.CameraDict[SerialNumber]).UpdateStichingParam();
                }
            }
        }
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
                List<string> camlist = new List<string>();
                foreach (var item in CCameraManagement.CamParamDict)
                {
                    if (item.Value.SerialNumber != proptemp?.SerialNumber)
                    {
                        camlist.Add(item.Value.SerialNumber);
                    }
                }

                comboBox.ItemsSource = camlist;
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
                    foreach (var item in CCameraManagement.CamParamDict)
                    {
                        if (item.Value.SerialNumber != cam.SerialNumber)
                        {
                            camlist.Add(item.Value.SerialNumber);
                        }
                    }
                }
                comboBox.ItemsSource = camlist;
            }
        }
    }
}
