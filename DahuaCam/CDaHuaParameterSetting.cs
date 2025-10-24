using CameraModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Controls;
using WH.Entity.Attribute;
using MVSDK_Net;
using System.ComponentModel;


namespace DaHuaCam
{
    /// <summary>
    /// 20250626 GWD
    /// 相机参数派生类
    /// </summary>

    public partial class CDaHuaParameterSetting:CCameraParameterBase
    {

        //一个是无参数构造函数
        public CDaHuaParameterSetting()
            : base() { }

        //带有两个参数（序列号和相机供应商）的构造函数。
        public CDaHuaParameterSetting(string serialnumber, string cameraSupplier) 
            : base(serialnumber, cameraSupplier){ }





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
            Software
        };

        //引脚信号源（如曝光激活、定时器输出等）。
        public enum LINE_SOURCE
        {
            ExposureActive = 0,
            Timer0Active = 1,
            AcquisitionTriggerWait = 3,
            UserOutput0 = 4,
            LightTrigger = 5,
            UserExpTime = 8

        }

        //引脚模式
        public enum LINE_MODE
        {
            /// <summary>
            /// 输入
            /// </summary>
            Input = 0,
            /// <summary>
            /// 频闪
            /// </summary>
            Output = 1

        }
        //选择具体引脚（Line0~Line2）。
        public enum LINE_SELECT
        {

            /// <summary>
            /// 引脚1
            /// </summary>
            Line0,
            /// <summary>
            /// 引脚2
            /// </summary>
            Line1,
            /// <summary>
            /// 引脚3
            /// </summary>
            Line2
        }

        private TRIGGER_SOURCE triggerSource = TRIGGER_SOURCE.Software;
        /// <summary>
        /// 2024.6.30 GWD
        /// 硬触发源
        /// </summary>
        [Category("大华相机专用参数")]
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
                    CCameraManagement
                        .CameraDict[SerialNumber]
                        .SetCustomParam((uint)EMCUSTOMPARAMTYPE.EMPARAMTRIGGERSOURCE);
                }
            }     
        }

        private LINE_SELECT _linesselect;
        [Category("大华相机专用参数")]
        [DisplayName("2.输入输出引脚")]
        [Description("设置输入输出引脚")]
        [Browsable(true)]
        public LINE_SELECT LineSelect
        {
            get { return _linesselect; }
            set
            {
                _linesselect = value;

            }
        }

        private LINE_MODE _linemode;
        [Category("大华相机专用参数")]
        [DisplayName("3.选择输入输出")]
        [Description("设置选择输入输出模式")]
        [Browsable(true)]
        public LINE_MODE LineMode
        {
            get { return _linemode; }
            set
            {
                _linemode = value;

            }
        }
        private LINE_SOURCE _linesource = LINE_SOURCE.ExposureActive;
        [Category("大华相机专用参数")]
        [DisplayName("4.引脚触发模式")]
        [Description("引脚触发模式")]
        [Browsable(true)]
        public LINE_SOURCE LineSource
        {
            get { return _linesource; }
            set
            {
                _linesource = value;

            }
        }

    }



    /// <summary>
    /// 2024.8.6 李焕彬
    /// 硬触发源
    /// </summary>
    //public enum EMCAMTRIGGERSOURCE
    //{
    //    /// <summary>
    //    /// 2024.8.6 李焕彬
    //    /// 线路0
    //    /// </summary>
    //    [EnumString("线路0", "Line0")]
    //    EMTRIGGERSOURCELINE0 = 0,

    //    /// <summary>
    //    /// 2024.8.6 李焕彬
    //    /// 线路1
    //    /// </summary>
    //    [EnumString("线路1", "Line1")]
    //    EMTRIGGERSOURCELINE1 = 1,

    //    /// <summary>
    //    /// 2024.8.6 李焕彬
    //    /// 线路2
    //    /// </summary>
    //    [EnumString("线路2", "Line2")]
    //    EMTRIGGERSOURCELINE2 = 2,

    //};



    public enum EMCUSTOMPARAMTYPE
    {
        EMPARAMTRIGGERSOURCE = 0,
    }

}
