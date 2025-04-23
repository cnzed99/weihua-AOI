using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WH.Entity.CommonLib;

namespace WH.Controls.Controls
{
    public class ButtonProperty : HandyControl.Controls.bu
    {
        public ButtonProperty()
        {
            this.ItemsSource = new COMSPro().COMDevices;
        }

        public COMDevice ComDevice
        {
            get { return (COMDevice)GetValue(ComDeviceProperty); }
            set { SetValue(ComDeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComDeviceProperty = DependencyProperty.Register(
            "ComDevice",
            typeof(COMDevice),
            typeof(COMSComboBox),
            new FrameworkPropertyMetadata(
                default(COMDevice),
                (s, e) =>
                {
                    if (e.NewValue != null)
                    {
                        COMSComboBox cmb = (COMSComboBox)s;
                        cmb.SelectedItem = e.NewValue;
                    }
                }
            )
        );

        /// <summary>
        /// 选中改变事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (e.AddedItems.Count > 0)
            {
                COMDevice comDevice = e.AddedItems[0] as COMDevice;
                if (comDevice != null)
                {
                    ComDevice = comDevice;
                }
            }
        }
    }
}