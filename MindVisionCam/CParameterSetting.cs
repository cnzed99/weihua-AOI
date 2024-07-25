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

namespace MindVisionCam
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CParameterSetting : CCameraParameterBase
    {
        public CParameterSetting()
            : base() { }

        public CParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发源
        /// </summary>
        private EMTRIGGERMODE triggerSource = EMTRIGGERMODE.EMTRIGGERSOFTWARE;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 触发源
        /// </summary>
        [property: Category("专用参数")]
        [property: DisplayName("1.触发源")]
        [property: Description("1.触发源")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        public EMTRIGGERMODE TriggerSource
        {
            get { return triggerSource; }
            set
            {
                SetProperty(ref triggerSource, value);
                if (Connected)
                {
                    CCameraManagement.CameraDict[SerialNumber].SetTriggerMode(this.TriggerMode);
                }
            }
        }
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 触发模式
    /// </summary>
    public enum EMTRIGGERMODE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 软触发
        /// </summary>
        [EnumString("软触发", "SOFTWARE")]
        EMTRIGGERSOFTWARE = 1,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 硬触发
        /// </summary>
        [EnumString("硬触发", "HARDWARE")]
        EMTRIGGERHARDWARE = 2,
    }
}
