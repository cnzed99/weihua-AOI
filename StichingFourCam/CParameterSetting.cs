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
        [property: Category("拼图参数")]
        [property: DisplayName("时间戳间隔ms")]
        int timeLimit;

        private string serialNumber1;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 相机1序列号
        /// </summary>
        [property: Category("拼图参数")]
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
        [property: Category("拼图参数")]
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
        [property: Category("拼图参数")]
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
        [property: Category("拼图参数")]
        [property: DisplayName("相机4序列号")]
        [property: Description("相机4序列号")]
        [property: Editor(typeof(CComboxEditorPro), typeof(CComboxEditorPro))]
        public string SerialNumber4
        {
            get { return serialNumber4; }
            set { SetProperty(ref serialNumber4, value); }
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
