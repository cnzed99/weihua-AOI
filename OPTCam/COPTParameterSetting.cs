using CameraModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPTCam
{
    public class COPTParameterSetting : CCameraParameterBase
    {
        private TRIGGER_SOURCE triggerSource = TRIGGER_SOURCE.Software;
        //带有两个参数（序列号和相机供应商）的构造函数。
        public COPTParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }
        /// <summary>
        /// 2024.6.30 GWD
        /// 硬触发源
        /// </summary>
        [Category("专用参数")]
        [property: DisplayName("1.触发源")]
        [property: Description("触发源")]
        [Browsable(true)]

        //定义了一个公共属性 TriggerSource，用于获取和设置触发源。设置属性时，会更新相机的触发源。
        public TRIGGER_SOURCE TriggerSource
        {
            get { return triggerSource; }

            set
            {
                SetProperty(ref triggerSource, value);
                if (Connected)
                {
                    CCameraManagement.CameraDict[SerialNumber].SetCustomParam((uint)EMCUSTOMPARAMTYPE.EMPARAMTRIGGERSOURCE);
                }
            }
        }
        /// <summary>
        /// 2025.6.30 GWD
        /// 触发源
        /// </summary>
        /// //定义触发源类型，支持硬件触发（Line1~Line3）和软件触发。
        public enum TRIGGER_SOURCE
        {
            Line0,
            Line1,
            Line2,
            Line3,
            Line4,
            Software
        };
        public enum EMCUSTOMPARAMTYPE
        {
            EMPARAMTRIGGERSOURCE = 0,
        }
    }
}
