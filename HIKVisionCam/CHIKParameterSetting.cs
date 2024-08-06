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
using static MvCamCtrl.NET.MyCamera;

namespace HIKVisionCam
{
    /// <summary>
    /// 20240802 TCG
    /// 相机参数派生类
    /// </summary>
    public partial class CHIKParameterSetting : CCameraParameterBase
    {
        public CHIKParameterSetting()
            : base() { }

        public CHIKParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        private EMCAMTRIGGERSOURCE triggerSource = EMCAMTRIGGERSOURCE.EMTRIGGERSOURCELINE0;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 硬触发源
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("1.硬触发源")]
        [property: Description("1.硬触发源")]
        [property: Editor(typeof(CEnumPropertyEditorPro), typeof(CEnumPropertyEditorPro))]
        public EMCAMTRIGGERSOURCE TriggerSource
        {
            get { return triggerSource; }
            set
            {
                SetProperty(ref triggerSource, value);
                if (Connected)
                {
                    CCameraManagement
                        .CameraDict[SerialNumber]
                        .SetCustomParam((uint)EMCUSTOMPARAMTYPE.EMPARAMTRIGGERSOURCE);
                }
            }
        }
    }

    /// <summary>
    /// 2024.8.6 李焕彬
    /// 硬触发源
    /// </summary>
    public enum EMCAMTRIGGERSOURCE
    {
        /// <summary>
        /// 2024.8.6 李焕彬
        /// 线路0
        /// </summary>
        [EnumString("线路0", "Line0")]
        EMTRIGGERSOURCELINE0 = 0,

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 线路1
        /// </summary>
        [EnumString("线路1", "Line1")]
        EMTRIGGERSOURCELINE1 = 1,

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 线路2
        /// </summary>
        [EnumString("线路2", "Line2")]
        EMTRIGGERSOURCELINE2 = 2,

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 线路3
        /// </summary>
        [EnumString("线路3", "Line3")]
        EMTRIGGERSOURCELINE3 = 3,

        /// <summary>
        /// 2024.8.6 李焕彬
        /// 线路0
        /// </summary>
        [EnumString("计数器0", "Counter0")]
        EMTRIGGERSOURCECOUNTER0 = 4,
    };

    /// <summary>
    /// 2024.8.6 李焕彬
    /// 自定义参数类别
    /// </summary>
    public enum EMCUSTOMPARAMTYPE
    {
        EMPARAMTRIGGERSOURCE = 0,
    }
}
